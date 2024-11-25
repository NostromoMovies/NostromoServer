using System.Security.Cryptography;
using System.IO.Hashing;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Nostromo.Server.FileHelper;

public class NativeHasher
{
    private const int BUFFER_SIZE = 1024 * 1024; // 1MB buffer

    public class HashResult
    {
        public string ED2K { get; set; }
        public string CRC32 { get; set; }
        public string MD5 { get; set; }
        public string SHA1 { get; set; }
        public long FileSize { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }

    public delegate void OnHashProgress(string fileName, int progressPercent);

    // MD4 implementation based on RFC 1320
    private class MD4
    {
        private static readonly byte[] InitialState = {
            0x01, 0x23, 0x45, 0x67,
            0x89, 0xab, 0xcd, 0xef,
            0xfe, 0xdc, 0xba, 0x98,
            0x76, 0x54, 0x32, 0x10
        };

        private const int BlockSize = 64;  // 512 bits
        private const uint Round2Constant = 0x5A827999;
        private const uint Round3Constant = 0x6ED9EBA1;

        private uint A, B, C, D;

        public MD4(ReadOnlySpan<byte> input)
        {
            A = BitConverter.ToUInt32(InitialState.AsSpan()[0..4]);
            B = BitConverter.ToUInt32(InitialState.AsSpan()[4..8]);
            C = BitConverter.ToUInt32(InitialState.AsSpan()[8..12]);
            D = BitConverter.ToUInt32(InitialState.AsSpan()[12..16]);

            var paddedInput = PadInput(input);
            for (int i = 0; i < paddedInput.Length; i += BlockSize)
            {
                ProcessBlock(paddedInput.Slice(i, BlockSize));
            }
        }

        public ReadOnlySpan<byte> Digest
        {
            get
            {
                byte[] result = new byte[16];
                BitConverter.TryWriteBytes(result.AsSpan(0, 4), A);
                BitConverter.TryWriteBytes(result.AsSpan(4, 4), B);
                BitConverter.TryWriteBytes(result.AsSpan(8, 4), C);
                BitConverter.TryWriteBytes(result.AsSpan(12, 4), D);
                return result;
            }
        }

        public string DigestString => Convert.ToHexString(Digest).ToLowerInvariant();

        private static Span<byte> PadInput(ReadOnlySpan<byte> input)
        {
            long originalLength = input.Length * 8L;
            int paddedLength = ((input.Length + 8 + 1) + 63) & ~63;

            byte[] padded = new byte[paddedLength];
            input.CopyTo(padded);
            padded[input.Length] = 0x80;

            BitConverter.GetBytes(originalLength).CopyTo(padded.AsSpan(paddedLength - 8));

            return padded;
        }

        private void ProcessBlock(ReadOnlySpan<byte> block)
        {
            uint aa = A, bb = B, cc = C, dd = D;

            // Helper functions moved outside of local functions
            static uint F(uint x, uint y, uint z) => (x & y) | (~x & z);
            static uint G(uint x, uint y, uint z) => (x & y) | (x & z) | (y & z);
            static uint H(uint x, uint y, uint z) => x ^ y ^ z;

            static void ProcessRound1(ref uint a, uint b, uint c, uint d, ReadOnlySpan<byte> blockData, int k, int s)
            {
                a = BitOperations.RotateLeft(
                    a + F(b, c, d) + MemoryMarshal.Read<uint>(blockData[(k * 4)..]),
                    s);
            }

            static void ProcessRound2(ref uint a, uint b, uint c, uint d, ReadOnlySpan<byte> blockData, int k, int s)
            {
                a = BitOperations.RotateLeft(
                    a + G(b, c, d) + MemoryMarshal.Read<uint>(blockData[(k * 4)..]) + Round2Constant,
                    s);
            }

            static void ProcessRound3(ref uint a, uint b, uint c, uint d, ReadOnlySpan<byte> blockData, int k, int s)
            {
                a = BitOperations.RotateLeft(
                    a + H(b, c, d) + MemoryMarshal.Read<uint>(blockData[(k * 4)..]) + Round3Constant,
                    s);
            }

            // Round 1
            for (int i = 0; i < 4; i++)
            {
                ProcessRound1(ref A, B, C, D, block, i * 4 + 0, 3);
                ProcessRound1(ref D, A, B, C, block, i * 4 + 1, 7);
                ProcessRound1(ref C, D, A, B, block, i * 4 + 2, 11);
                ProcessRound1(ref B, C, D, A, block, i * 4 + 3, 19);
            }

            // Round 2
            int[] round2Order = { 0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15 };
            for (int i = 0; i < 4; i++)
            {
                ProcessRound2(ref A, B, C, D, block, round2Order[i * 4 + 0], 3);
                ProcessRound2(ref D, A, B, C, block, round2Order[i * 4 + 1], 5);
                ProcessRound2(ref C, D, A, B, block, round2Order[i * 4 + 2], 9);
                ProcessRound2(ref B, C, D, A, block, round2Order[i * 4 + 3], 13);
            }

            // Round 3
            int[] round3Order = { 0, 8, 4, 12, 2, 10, 6, 14, 1, 9, 5, 13, 3, 11, 7, 15 };
            for (int i = 0; i < 4; i++)
            {
                ProcessRound3(ref A, B, C, D, block, round3Order[i * 4 + 0], 3);
                ProcessRound3(ref D, A, B, C, block, round3Order[i * 4 + 1], 9);
                ProcessRound3(ref C, D, A, B, block, round3Order[i * 4 + 2], 11);
                ProcessRound3(ref B, C, D, A, block, round3Order[i * 4 + 3], 15);
            }

            A += aa;
            B += bb;
            C += cc;
            D += dd;
        }
    }

    private static class Ed2k
    {
        private const int BlockSize = 9728000;

        public static string ComputeHash(string filePath, IProgress<int>? progress = null)
        {
            using var fileStream = new BufferedStream(File.OpenRead(filePath));
            var fileInfo = new FileInfo(filePath);
            var blockCount = (int)(fileInfo.Length / BlockSize) + 1;
            var blockHashes = new List<byte>();
            var buffer = ArrayPool<byte>.Shared.Rent(BlockSize);

            try
            {
                for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
                {
                    progress?.Report(blockIndex * 100 / blockCount);

                    int bytesRead = fileStream.Read(buffer, 0, BlockSize);
                    if (bytesRead == 0) break;

                    var blockHash = new MD4(new ReadOnlySpan<byte>(buffer, 0, bytesRead)).Digest.ToArray();
                    blockHashes.AddRange(blockHash);
                }

                progress?.Report(100);
                return new MD4(CollectionsMarshal.AsSpan(blockHashes)).DigestString;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer, clearArray: false);
            }
        }
    }

    public static async Task<HashResult> CalculateHashesAsync(
        string filePath,
        OnHashProgress progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            using var fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                BUFFER_SIZE,
                FileOptions.Asynchronous);

            using var md5 = MD5.Create();
            using var sha1 = SHA1.Create();
            var crc32 = new CRC32();

            var buffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            try
            {
                long totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                    sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
                    crc32.TransformBlock(buffer, 0, bytesRead);

                    totalBytesRead += bytesRead;
                    var progress = (int)((totalBytesRead * 100) / fileInfo.Length);
                    progressCallback?.Invoke(filePath, progress);

                    cancellationToken.ThrowIfCancellationRequested();
                }

                md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                crc32.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

                var ed2kProgress = new Progress<int>(percent =>
                    progressCallback?.Invoke(filePath, percent));
                string ed2kHash = Ed2k.ComputeHash(filePath, ed2kProgress);

                return new HashResult
                {
                    MD5 = BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant(),
                    SHA1 = BitConverter.ToString(sha1.Hash).Replace("-", "").ToLowerInvariant(),
                    CRC32 = crc32.Hash,
                    ED2K = ed2kHash,
                    FileSize = fileInfo.Length,
                    ProcessingTime = stopwatch.Elapsed
                };
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new Exception($"Failed to calculate file hashes: {ex.Message}", ex);
        }
    }

    private class CRC32
    {
        private readonly System.IO.Hashing.Crc32 _crc32 = new();

        public void TransformBlock(byte[] buffer, int offset, int count)
        {
            _crc32.Append(buffer.AsSpan(offset, count));
        }

        public void TransformFinalBlock(byte[] buffer, int offset, int count)
        {
            if (count > 0)
            {
                _crc32.Append(buffer.AsSpan(offset, count));
            }
        }

        public string Hash
        {
            get
            {
                Span<byte> hash = stackalloc byte[4];
                _crc32.GetCurrentHash(hash);

                hash.Reverse();
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
        }
    }
}