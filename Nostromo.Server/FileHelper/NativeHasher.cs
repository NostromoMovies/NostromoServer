using System.Security.Cryptography;
using System.IO.Hashing;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Nostromo.Server.FileHelper;

public class NativeHasher
{
    private const int BUFFER_SIZE = 1024 * 1024; // 1MB buffer
    private const int ED2K_CHUNK_SIZE = 9728000; // ED2K uses 9.5MB chunks

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

            // Initialize all hashers
            using var md5 = MD5.Create();
            using var sha1 = SHA1.Create();
            var crc32 = new CRC32();  // Simplified CRC32
            var ed2k = new ED2KHash();

            var buffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            try
            {
                long totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    // Update all hashes in one pass
                    md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                    sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
                    crc32.TransformBlock(buffer, 0, bytesRead);  // Simplified call
                    ed2k.TransformBlock(buffer, 0, bytesRead, null, 0);

                    totalBytesRead += bytesRead;
                    var progress = (int)((totalBytesRead * 100) / fileInfo.Length);
                    progressCallback?.Invoke(filePath, progress);

                    cancellationToken.ThrowIfCancellationRequested();
                }

                // Final block for all hashes
                md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                crc32.TransformFinalBlock(Array.Empty<byte>(), 0, 0);  // Keep for consistency
                ed2k.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

                return new HashResult
                {
                    MD5 = BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant(),
                    SHA1 = BitConverter.ToString(sha1.Hash).Replace("-", "").ToLowerInvariant(),
                    CRC32 = crc32.Hash,  // Now returns proper hex string
                    ED2K = BitConverter.ToString(ed2k.Hash).Replace("-", "").ToLowerInvariant(),
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
    private class ED2KHash : HashAlgorithm
    {
        private const int CHUNK_SIZE = 9728000; // 9.5MB
        private readonly List<byte> _hashBytes;
        private readonly MD4 _md4;
        private readonly byte[] _currentChunk;
        private int _currentChunkSize;
        private byte[] _finalHash;
        private int _chunkCount;

        public ED2KHash()
        {
            _hashBytes = new List<byte>();
            _md4 = MD4.Create();
            _currentChunk = new byte[CHUNK_SIZE];
            _currentChunkSize = 0;
            _chunkCount = 0;
        }

        public override void Initialize()
        {
            _hashBytes.Clear();
            _currentChunkSize = 0;
            _finalHash = null;
            _chunkCount = 0;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            int position = ibStart;
            int remaining = cbSize;

            while (remaining > 0)
            {
                int bytesToCopy = Math.Min(CHUNK_SIZE - _currentChunkSize, remaining);
                Buffer.BlockCopy(array, position, _currentChunk, _currentChunkSize, bytesToCopy);
                _currentChunkSize += bytesToCopy;

                if (_currentChunkSize == CHUNK_SIZE)
                {
                    ProcessChunk();
                }

                position += bytesToCopy;
                remaining -= bytesToCopy;
            }
        }

        private void ProcessChunk()
        {
            byte[] chunkHash = _md4.ComputeHash(_currentChunk, 0, _currentChunkSize);
            _hashBytes.AddRange(chunkHash);
            _currentChunkSize = 0;
            _chunkCount++;
        }

        protected override byte[] HashFinal()
        {
            if (_finalHash != null)
                return _finalHash;

            // Process any remaining data
            if (_currentChunkSize > 0)
            {
                ProcessChunk();
            }

            // If no data was processed, return hash of empty array
            if (_hashBytes.Count == 0)
            {
                _finalHash = _md4.ComputeHash(Array.Empty<byte>());
                return _finalHash;
            }

            // Key difference: If only one chunk, return the hash bytes directly
            if (_chunkCount == 1)
            {
                _finalHash = _hashBytes.ToArray();
                return _finalHash;
            }

            // Otherwise hash the combined hashes
            _finalHash = _md4.ComputeHash(_hashBytes.ToArray());
            return _finalHash;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _md4?.Dispose();
                _hashBytes.Clear();
            }
            base.Dispose(disposing);
        }
    }

    // MD4 managed implementation
    private class MD4Managed : MD4
    {
        private readonly uint[] state = new uint[4];
        private readonly uint[] buffer = new uint[16];
        private readonly byte[] input = new byte[64];
        private int inputLength;
        private long totalLength;

        public override void Initialize()
        {
            state[0] = 0x67452301;
            state[1] = 0xefcdab89;
            state[2] = 0x98badcfe;
            state[3] = 0x10325476;
            inputLength = 0;
            totalLength = 0;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            totalLength += cbSize;
            var index = 0;
            while (index < cbSize)
            {
                var bytesToCopy = Math.Min(64 - inputLength, cbSize - index);
                Buffer.BlockCopy(array, ibStart + index, input, inputLength, bytesToCopy);
                inputLength += bytesToCopy;
                index += bytesToCopy;

                if (inputLength == 64)
                {
                    ProcessBlock();
                    inputLength = 0;
                }
            }
        }

        protected override byte[] HashFinal()
        {
            var padLength = (120 - (int)(totalLength % 64)) % 64;
            if (padLength == 0) padLength = 64;

            var padding = new byte[padLength + 8];
            padding[0] = 0x80;

            var bits = totalLength * 8;
            padding[padLength] = (byte)bits;
            padding[padLength + 1] = (byte)(bits >> 8);
            padding[padLength + 2] = (byte)(bits >> 16);
            padding[padLength + 3] = (byte)(bits >> 24);
            padding[padLength + 4] = (byte)(bits >> 32);
            padding[padLength + 5] = (byte)(bits >> 40);
            padding[padLength + 6] = (byte)(bits >> 48);
            padding[padLength + 7] = (byte)(bits >> 56);

            HashCore(padding, 0, padding.Length);

            var output = new byte[16];
            Buffer.BlockCopy(state, 0, output, 0, 16);
            return output;
        }

        private void ProcessBlock()
        {
            for (var i = 0; i < 16; i++)
            {
                buffer[i] = BitConverter.ToUInt32(input, i * 4);
            }

            var a = state[0];
            var b = state[1];
            var c = state[2];
            var d = state[3];

            // Round 1
            a = FF(a, b, c, d, buffer[0], 3);
            d = FF(d, a, b, c, buffer[1], 7);
            c = FF(c, d, a, b, buffer[2], 11);
            b = FF(b, c, d, a, buffer[3], 19);
            a = FF(a, b, c, d, buffer[4], 3);
            d = FF(d, a, b, c, buffer[5], 7);
            c = FF(c, d, a, b, buffer[6], 11);
            b = FF(b, c, d, a, buffer[7], 19);
            a = FF(a, b, c, d, buffer[8], 3);
            d = FF(d, a, b, c, buffer[9], 7);
            c = FF(c, d, a, b, buffer[10], 11);
            b = FF(b, c, d, a, buffer[11], 19);
            a = FF(a, b, c, d, buffer[12], 3);
            d = FF(d, a, b, c, buffer[13], 7);
            c = FF(c, d, a, b, buffer[14], 11);
            b = FF(b, c, d, a, buffer[15], 19);

            // Round 2
            a = GG(a, b, c, d, buffer[0], 3);
            d = GG(d, a, b, c, buffer[4], 5);
            c = GG(c, d, a, b, buffer[8], 9);
            b = GG(b, c, d, a, buffer[12], 13);
            a = GG(a, b, c, d, buffer[1], 3);
            d = GG(d, a, b, c, buffer[5], 5);
            c = GG(c, d, a, b, buffer[9], 9);
            b = GG(b, c, d, a, buffer[13], 13);
            a = GG(a, b, c, d, buffer[2], 3);
            d = GG(d, a, b, c, buffer[6], 5);
            c = GG(c, d, a, b, buffer[10], 9);
            b = GG(b, c, d, a, buffer[14], 13);
            a = GG(a, b, c, d, buffer[3], 3);
            d = GG(d, a, b, c, buffer[7], 5);
            c = GG(c, d, a, b, buffer[11], 9);
            b = GG(b, c, d, a, buffer[15], 13);

            // Round 3
            a = HH(a, b, c, d, buffer[0], 3);
            d = HH(d, a, b, c, buffer[8], 9);
            c = HH(c, d, a, b, buffer[4], 11);
            b = HH(b, c, d, a, buffer[12], 15);
            a = HH(a, b, c, d, buffer[2], 3);
            d = HH(d, a, b, c, buffer[10], 9);
            c = HH(c, d, a, b, buffer[6], 11);
            b = HH(b, c, d, a, buffer[14], 15);
            a = HH(a, b, c, d, buffer[1], 3);
            d = HH(d, a, b, c, buffer[9], 9);
            c = HH(c, d, a, b, buffer[5], 11);
            b = HH(b, c, d, a, buffer[13], 15);
            a = HH(a, b, c, d, buffer[3], 3);
            d = HH(d, a, b, c, buffer[11], 9);
            c = HH(c, d, a, b, buffer[7], 11);
            b = HH(b, c, d, a, buffer[15], 15);

            state[0] += a;
            state[1] += b;
            state[2] += c;
            state[3] += d;
        }

        private static uint FF(uint a, uint b, uint c, uint d, uint x, int s)
        {
            var f = (b & c) | (~b & d);
            return RotateLeft(a + f + x, s);
        }

        private static uint GG(uint a, uint b, uint c, uint d, uint x, int s)
        {
            var g = (b & c) | (b & d) | (c & d);
            return RotateLeft(a + g + x + 0x5A827999, s);
        }

        private static uint HH(uint a, uint b, uint c, uint d, uint x, int s)
        {
            var h = b ^ c ^ d;
            return RotateLeft(a + h + x + 0x6ED9EBA1, s);
        }

        private static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }
    }

    private abstract class MD4 : HashAlgorithm
    {
        protected MD4()
        {
            HashSizeValue = 128;
        }

        public static new MD4 Create()
        {
            return new MD4Managed();
        }
    }
}