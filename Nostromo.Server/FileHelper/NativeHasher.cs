using System.Security.Cryptography;
using System.IO.Hashing;
using System.Buffers;
using Nostromo.Server.FileHelper.ED2k_algo;

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

            // Initialize regular hashers
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
                    // Update regular hashes
                    md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                    sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
                    crc32.TransformBlock(buffer, 0, bytesRead);

                    totalBytesRead += bytesRead;
                    var progress = (int)((totalBytesRead * 100) / fileInfo.Length);
                    progressCallback?.Invoke(filePath, progress);

                    cancellationToken.ThrowIfCancellationRequested();
                }

                // Finalize regular hashes
                md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                crc32.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

                // Calculate ED2K separately using the existing implementation
                var ed2kProgress = new Progress<int>(percent =>
                    progressCallback?.Invoke(filePath, percent));
                string ed2kHash = Ed2k.Compute(filePath, ed2kProgress);

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