using System;
using System.Runtime.InteropServices;
using System.Security;

namespace CryptoTools.Extensions;

public static class SecureStringExtension
{
    public static string ToPlainText(this SecureString secureString)
    {
        var unmanagedString = IntPtr.Zero;
        try
        {
            // Convert the SecureString to an unmanaged string
            unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            // Convert the unmanaged string to a managed string
            return Marshal.PtrToStringUni(unmanagedString) ?? string.Empty;
        }
        finally
        {
            // Free the unmanaged string
            Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
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