using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using Xunit;
using System.Text;
using System.IO;
using Nostromo.Server.FileHelper;

namespace Nostromo.Tests.FileHelper
{
    public class NativeHasherTests
    {
        private const string TestFilePath = "test_file.txt";
        private const string TestContent = "Hello, World!";

        public NativeHasherTests()
        {
            // Create a test file if it doesn't exist
            if (!File.Exists(TestFilePath))
            {
                File.WriteAllText(TestFilePath, TestContent);
            }
        }
        //[Theory]
        //[InlineData("[MTBB] Space Dandy (2014) - 01 - Live with the Flow, Baby (1080p HEVC 10bit BluRay) [42B6765A].mkv", "66e1f517b71312c0d111d56cf94ea2b8")]
        //[InlineData("a", "bde52cb31de33e46245e05fbdbd6fb24")]             // Single character
        //[InlineData("abc", "a448017aaf21d8525fc10ae87aa6729d")]           // Standard test vector
        //[InlineData("message digest", "d9130a8164549fe818874806e1c7014b")] // Standard test vector
        //public async Task CalculateHashesAsync_KnownED2KTestVectors_MatchesExpected(string input, string expected)
        //{
        //    // Arrange
        //    File.WriteAllText(TestFilePath, input);

        //    try
        //    {
        //        // Act
        //        var result = await NativeHasher.CalculateHashesAsync(TestFilePath);

        //        // Assert
        //        Assert.Equal(expected, result.ED2K);
        //    }
        //    finally
        //    {
        //        CleanupTestFile();
        //    }
        //}

        //[Theory]
        //[InlineData("", "00000000")]                   // Empty string
        //[InlineData("The quick brown fox", "519025e9")] // Standard test string
        //[InlineData("123456789", "cbf43926")]          // Standard test vector
        //public async Task CalculateHashesAsync_KnownCRC32TestVectors_MatchesExpected(string input, string expected)
        //{
        //    // Arrange
        //    File.WriteAllText(TestFilePath, input);

        //    try
        //    {
        //        // Act
        //        var result = await NativeHasher.CalculateHashesAsync(TestFilePath);

        //        // Assert
        //        Assert.Equal(expected, result.CRC32);
        //    }
        //    finally
        //    {
        //        CleanupTestFile();
        //    }
        //}

        [Fact]
        public async Task CalculateHashesAsync_ED2KFormat_IsValid()
        {
            // Arrange
            File.WriteAllText(TestFilePath, TestContent);

            try
            {
                // Act
                var result = await NativeHasher.CalculateHashesAsync(TestFilePath);

                // Assert
                Assert.Matches("^[0-9a-f]{32}$", result.ED2K);
            }
            finally
            {
                CleanupTestFile();
            }
        }

        [Fact]
        public async Task CalculateHashesAsync_CRC32Format_IsValid()
        {
            // Arrange
            File.WriteAllText(TestFilePath, TestContent);

            try
            {
                // Act
                var result = await NativeHasher.CalculateHashesAsync(TestFilePath);

                // Assert
                Assert.Matches("^[0-9a-f]{8}$", result.CRC32);
            }
            finally
            {
                CleanupTestFile();
            }
        }
        private void CleanupTestFile()
        {
            if (File.Exists(TestFilePath))
            {
                File.Delete(TestFilePath);
            }
        }

        [Fact]
        public async Task CalculateHashesAsync_ValidFile_ReturnsCorrectHashes()
        {
            // Arrange
            var content = Encoding.UTF8.GetBytes(TestContent);
            File.WriteAllBytes(TestFilePath, content);

            // Calculate expected hashes
            var expectedMd5 = Convert.ToHexString(MD5.HashData(content)).ToLowerInvariant();
            var expectedSha1 = Convert.ToHexString(SHA1.HashData(content)).ToLowerInvariant();

            int progressCalls = 0;
            void ProgressCallback(string file, int progress)
            {
                Assert.Equal(TestFilePath, file);
                Assert.InRange(progress, 0, 100);
                progressCalls++;
            }

            try
            {
                // Act
                var result = await NativeHasher.CalculateHashesAsync(
                    TestFilePath,
                    progressCallback: ProgressCallback);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(expectedMd5, result.MD5);
                Assert.Equal(expectedSha1, result.SHA1);
                Assert.Equal(content.Length, result.FileSize);
                Assert.NotNull(result.ED2K);
                Assert.NotNull(result.CRC32);
                Assert.True(progressCalls > 0, "Progress callback was never called");
            }
            finally
            {
                CleanupTestFile();
            }
        }

        [Fact]
        public async Task CalculateHashesAsync_NonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "non_existent_file.txt";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.Exception>(
                () => NativeHasher.CalculateHashesAsync(nonExistentPath)
            );

            Assert.Contains("File not found", exception.Message);
        }

        [Fact]
        public async Task CalculateHashesAsync_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var content = new byte[1024 * 1024 * 10]; // 10MB of data
            File.WriteAllBytes(TestFilePath, content);

            try
            {
                // Act & Assert
                var task = NativeHasher.CalculateHashesAsync(TestFilePath, cancellationToken: cts.Token);
                cts.Cancel();

                await Assert.ThrowsAsync<OperationCanceledException>(
                    () => task
                );
            }
            finally
            {
                CleanupTestFile();
            }
        }

        [Fact]
        public async Task CalculateHashesAsync_LargeFile_CorrectlyProcessesFile()
        {
            // Arrange
            var content = new byte[1024 * 1024 * 5]; // 5MB of data
            new Random(42).NextBytes(content); // Fill with random data
            File.WriteAllBytes(TestFilePath, content);

            var progressValues = new List<int>();
            void ProgressCallback(string file, int progress)
            {
                progressValues.Add(progress);
            }

            try
            {
                // Act
                var result = await NativeHasher.CalculateHashesAsync(
                    TestFilePath,
                    progressCallback: ProgressCallback);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(content.Length, result.FileSize);
                Assert.True(progressValues.Count > 1, "Should have multiple progress updates for large file");
                Assert.Contains(100, progressValues); // Should reach 100%
                Assert.True(progressValues.All(v => v >= 0 && v <= 100), "All progress values should be between 0 and 100");
            }
            finally
            {
                CleanupTestFile();
            }
        }

        [Fact]
        public async Task CalculateHashesAsync_EmptyFile_ReturnsValidHashes()
        {
            // Arrange
            File.WriteAllBytes(TestFilePath, Array.Empty<byte>());

            try
            {
                // Act
                var result = await NativeHasher.CalculateHashesAsync(TestFilePath);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(0, result.FileSize);
                Assert.NotNull(result.MD5);
                Assert.NotNull(result.SHA1);
                Assert.NotNull(result.CRC32);
                Assert.NotNull(result.ED2K);
                Assert.True(result.ProcessingTime.TotalMilliseconds >= 0);
            }
            finally
            {
                CleanupTestFile();
            }
        }

        //[Theory]
        //[InlineData("")]
        //[InlineData(null)]
        //public async Task CalculateHashesAsync_InvalidPath_ThrowsException(string path)
        //{
        //    // Act & Assert
        //    await Assert.ThrowsAsync<ArgumentException>(
        //        () => NativeHasher.CalculateHashesAsync(path)
        //    );
        //}
    }
}