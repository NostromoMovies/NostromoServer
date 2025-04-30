using System.Security.Cryptography;
using System.IO.Hashing;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Nostromo.Server.FileHelper
{
    public class NativeHasher
    {
        private const int BUFFER_SIZE = 1024 * 1024; // 1MB buffer
        private const int ED2K_BLOCK_SIZE = 9728000;

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

        public static async Task<HashResult> CalculateHashesAsync(
            string filePath,
            OnHashProgress? progressCallback = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException("File not found", filePath);

            long fileLength = fileInfo.Length;

            using var fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                BUFFER_SIZE,
                FileOptions.Asynchronous);

            using var md5 = MD5.Create();
            using var sha1 = SHA1.Create();
            var crc32 = new CRC32Wrapper();

            var blockHashes = new List<byte>();
            byte[] ed2kBlockBuffer = new byte[ED2K_BLOCK_SIZE];
            int ed2kBlockPosition = 0;

            byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            try
            {
                long totalBytesRead = 0;
                int bytesRead;
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                    sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
                    crc32.Append(buffer.AsSpan(0, bytesRead));

                    int offset = 0;
                    while (offset < bytesRead)
                    {
                        int needed = ED2K_BLOCK_SIZE - ed2kBlockPosition;
                        int copyCount = Math.Min(needed, bytesRead - offset);

                        Buffer.BlockCopy(
                            src: buffer,
                            srcOffset: offset,
                            dst: ed2kBlockBuffer,
                            dstOffset: ed2kBlockPosition,
                            count: copyCount
                        );

                        ed2kBlockPosition += copyCount;
                        offset += copyCount;

                        if (ed2kBlockPosition == ED2K_BLOCK_SIZE)
                        {
                            var blockHash = ComputeMd4(ed2kBlockBuffer, ed2kBlockPosition);
                            blockHashes.AddRange(blockHash);
                            ed2kBlockPosition = 0;
                        }
                    }

                    totalBytesRead += bytesRead;
                    int percent = (int)((totalBytesRead * 100L) / fileLength);

                    progressCallback?.Invoke(filePath, percent);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (ed2kBlockPosition > 0)
                {
                    var lastBlockHash = ComputeMd4(ed2kBlockBuffer, ed2kBlockPosition);
                    blockHashes.AddRange(lastBlockHash);
                }

                md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

                string ed2kHex = ComputeEd2kFromBlockHashes(blockHashes);

                stopwatch.Stop();

                return new HashResult
                {
                    MD5 = BitConverter.ToString(md5.Hash!).Replace("-", "").ToLowerInvariant(),
                    SHA1 = BitConverter.ToString(sha1.Hash!).Replace("-", "").ToLowerInvariant(),
                    CRC32 = crc32.GetFinalHex(),
                    ED2K = ed2kHex,
                    FileSize = fileLength,
                    ProcessingTime = stopwatch.Elapsed
                };
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static string ComputeEd2kFromBlockHashes(List<byte> blockHashes)
        {
            var combinedHash = new MD4(CollectionsMarshal.AsSpan(blockHashes)).Digest;
            return Convert.ToHexString(combinedHash).ToLowerInvariant();
        }

        private static byte[] ComputeMd4(byte[] data, int length)
        {
            var tmp = new MD4(new ReadOnlySpan<byte>(data, 0, length));
            return tmp.Digest.ToArray();
        }

        private class MD4
        {
            private static readonly byte[] InitialState =
            {
                0x01, 0x23, 0x45, 0x67,
                0x89, 0xab, 0xcd, 0xef,
                0xfe, 0xdc, 0xba, 0x98,
                0x76, 0x54, 0x32, 0x10
            };

            private const int BlockSize = 64; // 512 bits

            private uint A, B, C, D;

            public MD4(ReadOnlySpan<byte> input)
            {
                A = BitConverter.ToUInt32(InitialState.AsSpan(0, 4));
                B = BitConverter.ToUInt32(InitialState.AsSpan(4, 4));
                C = BitConverter.ToUInt32(InitialState.AsSpan(8, 4));
                D = BitConverter.ToUInt32(InitialState.AsSpan(12, 4));

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

            private static Span<byte> PadInput(ReadOnlySpan<byte> input)
            {
                long originalLengthBits = (long)input.Length * 8;
                int paddedLength = ((input.Length + 1 + 8 + 63) & ~63);

                byte[] padded = new byte[paddedLength];
                input.CopyTo(padded);
                padded[input.Length] = 0x80;
                BitConverter.GetBytes(originalLengthBits).CopyTo(padded, paddedLength - 8);

                return padded;
            }

            private void ProcessBlock(ReadOnlySpan<byte> block)
            {
                uint[] X = new uint[16];
                for (int i = 0; i < 16; i++)
                {
                    X[i] = MemoryMarshal.Read<uint>(block.Slice(i * 4, 4));
                }

                uint aa = A, bb = B, cc = C, dd = D;

                // Round 1
                Round1(ref A, B, C, D, 0, 3, X);
                Round1(ref D, A, B, C, 1, 7, X);
                Round1(ref C, D, A, B, 2, 11, X);
                Round1(ref B, C, D, A, 3, 19, X);

                Round1(ref A, B, C, D, 4, 3, X);
                Round1(ref D, A, B, C, 5, 7, X);
                Round1(ref C, D, A, B, 6, 11, X);
                Round1(ref B, C, D, A, 7, 19, X);

                Round1(ref A, B, C, D, 8, 3, X);
                Round1(ref D, A, B, C, 9, 7, X);
                Round1(ref C, D, A, B, 10, 11, X);
                Round1(ref B, C, D, A, 11, 19, X);

                Round1(ref A, B, C, D, 12, 3, X);
                Round1(ref D, A, B, C, 13, 7, X);
                Round1(ref C, D, A, B, 14, 11, X);
                Round1(ref B, C, D, A, 15, 19, X);

                // Round 2
                int[] r2Order = { 0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15 };
                for (int i = 0; i < 16; i += 4)
                {
                    Round2(ref A, B, C, D, r2Order[i + 0], 3, X);
                    Round2(ref D, A, B, C, r2Order[i + 1], 5, X);
                    Round2(ref C, D, A, B, r2Order[i + 2], 9, X);
                    Round2(ref B, C, D, A, r2Order[i + 3], 13, X);
                }

                // Round 3
                int[] r3Order = { 0, 8, 4, 12, 2, 10, 6, 14, 1, 9, 5, 13, 3, 11, 7, 15 };
                for (int i = 0; i < 16; i += 4)
                {
                    Round3(ref A, B, C, D, r3Order[i + 0], 3, X);
                    Round3(ref D, A, B, C, r3Order[i + 1], 9, X);
                    Round3(ref C, D, A, B, r3Order[i + 2], 11, X);
                    Round3(ref B, C, D, A, r3Order[i + 3], 15, X);
                }

                A += aa;
                B += bb;
                C += cc;
                D += dd;
            }

            private static uint F(uint x, uint y, uint z) => (x & y) | (~x & z);
            private static uint G(uint x, uint y, uint z) => (x & y) | (x & z) | (y & z);
            private static uint H(uint x, uint y, uint z) => x ^ y ^ z;

            private static void Round1(ref uint a, uint b, uint c, uint d, int k, int s, uint[] X)
            {
                a = BitOperations.RotateLeft(a + F(b, c, d) + X[k], s);
            }

            private static void Round2(ref uint a, uint b, uint c, uint d, int k, int s, uint[] X)
            {
                const uint Round2Constant = 0x5A827999;
                a = BitOperations.RotateLeft(a + G(b, c, d) + X[k] + Round2Constant, s);
            }

            private static void Round3(ref uint a, uint b, uint c, uint d, int k, int s, uint[] X)
            {
                const uint Round3Constant = 0x6ED9EBA1;
                a = BitOperations.RotateLeft(a + H(b, c, d) + X[k] + Round3Constant, s);
            }
        }

        private class CRC32Wrapper
        {
            private readonly System.IO.Hashing.Crc32 _crc32 = new();

            public void Append(ReadOnlySpan<byte> data)
            {
                _crc32.Append(data);
            }

            public string GetFinalHex()
            {
                Span<byte> hash = stackalloc byte[4];
                _crc32.GetCurrentHash(hash);

                hash.Reverse();
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
        }

    }
}