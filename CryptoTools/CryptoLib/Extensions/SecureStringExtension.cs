using System.Runtime.InteropServices;
using System.Security;
using CryptoLib.Services;

namespace CryptoLib.Extensions;

public static class SecureStringExtension
{
    private static byte[] Hash(this SecureString ss, Func<byte[], byte[]> hashFunc)
    {
        var bStr = Marshal.SecureStringToBSTR(ss);
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

    public static string Hash(this SecureString password, string algorithm)
    {
        using var hashAlgorithm = HashingService.GetHashAlgorithm(algorithm);
        var pwHash = password.Hash(hashAlgorithm.ComputeHash);
        return Convert.ToHexString(pwHash).ToLower();
    }

    public static bool EqualsHash(this SecureString ss1, SecureString ss2)
    {
        if (ss1 == null)
            throw new ArgumentNullException(nameof(ss1));
        if (ss2 == null)
            throw new ArgumentNullException(nameof(ss2));
        if (ss1.Length != ss2.Length)
            return false;
        var ssBStr1Ptr = nint.Zero;
        var ssBStr2Ptr = nint.Zero;
        try
        {
            ssBStr1Ptr = Marshal.SecureStringToBSTR(ss1);
            ssBStr2Ptr = Marshal.SecureStringToBSTR(ss2);
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
    
    public static bool IsEqualTo(this SecureString ss1, SecureString ss2)
    {
        var bStr1 = IntPtr.Zero;
        var bStr2 = IntPtr.Zero;
        try
        {
            bStr1 = Marshal.SecureStringToBSTR(ss1);
            bStr2 = Marshal.SecureStringToBSTR(ss2);
            var length1 = Marshal.ReadInt32(bStr1, -4);
            var length2 = Marshal.ReadInt32(bStr2, -4);
            if (length1 == length2)
            {
                for (var x = 0; x < length1; ++x)
                {
                    var b1 = Marshal.ReadByte(bStr1, x);
                    var b2 = Marshal.ReadByte(bStr2, x);
                    if (b1 != b2) return false;
                }
            }
            else return false;
            return true;
        }
        finally
        {
            if (bStr2 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bStr2);
            if (bStr1 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bStr1);
        }
    }
}