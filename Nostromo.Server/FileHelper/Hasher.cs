using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Nostromo.Server.FileHelper;

public class Hasher
{
    private static readonly Destructor Finalise = new();

    [DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, uint dwFlags);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeLibrary(IntPtr hModule);

    // Callback delegate for hash progress
    public delegate int OnHashProgress([MarshalAs(UnmanagedType.LPWStr)] string fileName, int progressPercent);

    //[DllImport("hasher.dll", EntryPoint = "CalculateHashes_AsyncIO", CallingConvention = CallingConvention.Cdecl,
    //    CharSet = CharSet.Unicode)]
    private static extern int CalculateHashes_callback_dll(
        [MarshalAs(UnmanagedType.LPWStr)] string fileName,
        [MarshalAs(UnmanagedType.LPArray)] byte[] hash,
        [MarshalAs(UnmanagedType.FunctionPtr)] OnHashProgress progressCallback,
        [MarshalAs(UnmanagedType.Bool)] bool getCRC32,
        [MarshalAs(UnmanagedType.Bool)] bool getMD5,
        [MarshalAs(UnmanagedType.Bool)] bool getSHA1
    );

    internal sealed class Destructor : IDisposable
    {
        public IntPtr ModuleHandle;

        ~Destructor()
        {
            if (ModuleHandle != IntPtr.Zero)
            {
                FreeLibrary(ModuleHandle);
                ModuleHandle = IntPtr.Zero;
            }
        }

        public void Dispose() { }
    }

    static Hasher()
    {
        var fullexepath = Assembly.GetEntryAssembly()?.Location;
        try
        {
            if (fullexepath != null)
            {
                var fi = new FileInfo(fullexepath);
                fullexepath = Path.Combine(fi.Directory.FullName, Environment.Is64BitProcess ? "x64" : "x86",
                    "librhash.dll");
                Finalise.ModuleHandle = LoadLibraryEx(fullexepath, IntPtr.Zero, 0);
            }
        }
        catch (Exception)
        {
            Finalise.ModuleHandle = IntPtr.Zero;
        }
    }

    public class HashResult
    {
        public string CRC32 { get; set; }
        public string MD5 { get; set; }
        public string SHA1 { get; set; }
    }

    private static string HashToString(byte[] hash, int start, int length)
    {
        if (hash == null || hash.Length == 0) return string.Empty;

        var hex = new StringBuilder(length * 2);
        for (var x = start; x < start + length; x++)
        {
            hex.AppendFormat("{0:x2}", hash[x]);
        }
        return hex.ToString().ToUpper();
    }

    public static HashResult CalculateHashes(string filePath, OnHashProgress progressCallback = null)
    {
        var hash = new byte[56]; // Buffer for all hash types
        var result = new HashResult();

        try
        {
            // Handle UNC paths
            var filename = filePath.StartsWith(@"\\") ? filePath : @"\\?\" + filePath;

            var status = CalculateHashes_callback_dll(
                filename,
                hash,
                progressCallback,
                true,  // CRC32
                true,  // MD5
                true   // SHA1
            );

            if (status == 0) // Success
            {
                result.CRC32 = HashToString(hash, 0, 4);
                result.MD5 = HashToString(hash, 4, 16);
                result.SHA1 = HashToString(hash, 20, 20);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to calculate file hashes: {ex.Message}", ex);
        }

        return result;
    }
}