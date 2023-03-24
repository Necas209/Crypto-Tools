using System;
using System.Runtime.InteropServices;
using System.Security;

namespace CryptoTools.Services;

public static class SecureStringExtension
{
    private static byte[] Hash(this SecureString input, Func<byte[], byte[]> hashFunc)
    {
        var bStr = Marshal.SecureStringToBSTR(input);
        var length = Marshal.ReadInt32(bStr, -4);
        var bytes = new byte[length];

        var bytesPin = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            Marshal.Copy(bStr, bytes, 0, length);
            Marshal.ZeroFreeBSTR(bStr);
            return hashFunc(bytes);
        }
        finally
        {
            for (var i = 0; i < bytes.Length; i++) bytes[i] = 0;
            bytesPin.Free();
        }
    }

    public static string Hash(SecureString password, string algorithm)
    {
        using var hashAlgorithm = HashingService.GetHashAlgorithm(algorithm);
        var pwHash = password.Hash(hashAlgorithm.ComputeHash);
        return Convert.ToHexString(pwHash).ToLower();
    }

    public static bool EqualsHash(this SecureString secureString1, SecureString secureString2)
    {
        if (secureString1 == null)
            throw new ArgumentNullException(nameof(secureString1));
        if (secureString2 == null)
            throw new ArgumentNullException(nameof(secureString2));
        if (secureString1.Length != secureString2.Length)
            return false;
        var ssBStr1Ptr = nint.Zero;
        var ssBStr2Ptr = nint.Zero;
        try
        {
            ssBStr1Ptr = Marshal.SecureStringToBSTR(secureString1);
            ssBStr2Ptr = Marshal.SecureStringToBSTR(secureString2);
            var str1 = Marshal.PtrToStringBSTR(ssBStr1Ptr);
            var str2 = Marshal.PtrToStringBSTR(ssBStr2Ptr);
            return str1.Equals(str2);
        }
        finally
        {
            if (ssBStr1Ptr != nint.Zero) Marshal.ZeroFreeBSTR(ssBStr1Ptr);
            if (ssBStr2Ptr != nint.Zero) Marshal.ZeroFreeBSTR(ssBStr2Ptr);
        }
    }
}