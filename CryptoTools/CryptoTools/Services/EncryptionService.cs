using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTools.Services;

public static class EncryptionService
{
    private static SymmetricAlgorithm GetAlgorithm(string name)
    {
        return name switch
        {
            "AES" => Aes.Create(),
            "DES" => DES.Create(),
            "RC2" => RC2.Create(),
            "TripleDES" => TripleDES.Create(),
            _ => throw new CryptographicException("Invalid encryption algorithm")
        };
    }

    public static string EncryptImage(string inputFilePath, string algorithmName)
    {
        using var algorithm = GetAlgorithm(algorithmName);
        // Set the encryption key and generate an Initialization Vector
        algorithm.GenerateKey();
        algorithm.GenerateIV();
        ImageFormat imageFormat;
        using (var image = Image.FromFile(inputFilePath))
        {
            imageFormat = image.RawFormat;
        }

        using var inputFileStream = new FileStream(inputFilePath, FileMode.Open);
        // Create the output file path
        var imageExtension = Path.GetExtension(inputFilePath);
        var outputFilePath = Path.Combine(Path.GetDirectoryName(inputFilePath) ?? string.Empty,
            Path.GetFileNameWithoutExtension(inputFilePath) + ".encrypted" + imageExtension);
        using var outputFileStream = new FileStream(outputFilePath, FileMode.Create);
        using var encryptor = algorithm.CreateEncryptor();
        // Load the image and extract the pixel data
        var bitmap = new Bitmap(inputFileStream);
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var numBytes = bmpData.Stride * bitmap.Height;
        var pixelData = new byte[numBytes];
        Marshal.Copy(bmpData.Scan0, pixelData, 0, numBytes);
        bitmap.UnlockBits(bmpData);
        // Encrypt the pixel data
        var encryptedPixelData = encryptor.TransformFinalBlock(pixelData, 0, numBytes);
        // Save the encrypted pixel data as a new image
        var encryptedBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
        var encryptedBmpData = encryptedBitmap.LockBits(rect, ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);
        Marshal.Copy(encryptedPixelData[..numBytes], 0, encryptedBmpData.Scan0, numBytes);
        encryptedBitmap.UnlockBits(encryptedBmpData);
        encryptedBitmap.Save(outputFileStream, imageFormat);
        return outputFilePath;
    }

    public static void EncryptFile(string fileName, string algorithmName, RSAParameters rsaParameters)
    {
        // Create instance of RSA for asymmetric encryption of the AES key.
        var rsa = RSA.Create();
        rsa.ImportParameters(rsaParameters);
        // Create instance of the specified algorithm for symmetric encryption of the file.
        var symAlg = GetAlgorithm(algorithmName);
        // Generate a random key and IV.
        symAlg.GenerateKey();
        symAlg.GenerateIV();
        // Use RSACryptoServiceProvider to encrypt the AES key.
        var keyEncrypted = rsa.Encrypt(symAlg.Key, RSAEncryptionPadding.Pkcs1);
        // Create byte arrays to contain the length values of the key, IV and file extension.
        var lKey = keyEncrypted.Length;
        var lIv = symAlg.IV.Length;
        var extension = Encoding.UTF8.GetBytes(Path.GetExtension(fileName));
        var lExt = extension.Length;
        // Change the extension of the encrypted file to the original file extension.
        var outFile = Path.ChangeExtension(fileName, ".enc");
        using var outFs = new FileStream(outFile, FileMode.Create);
        using var bw = new BinaryWriter(outFs);
        // Write the key, IV and file extension length to the (outFs) FileStream.
        bw.Write(lKey);
        bw.Write(lIv);
        bw.Write(lExt);
        // Write the key, IV and file extension to the (outFs) FileStream.
        bw.Write(keyEncrypted);
        bw.Write(symAlg.IV);
        bw.Write(extension);
        using var cs = new CryptoStream(outFs, symAlg.CreateEncryptor(), CryptoStreamMode.Write);
        // By encrypting a chunk at a time, you can save memory and accommodate large files.
        // blockSizeBytes can be any arbitrary size.
        var blockSizeBytes = symAlg.BlockSize / 8;
        var data = new byte[blockSizeBytes];
        using (var inFs = new FileStream(fileName, FileMode.Open))
        {
            int count;
            do
            {
                count = inFs.Read(data, 0, blockSizeBytes);
                cs.Write(data, 0, count);
            } while (count > 0);
        }

        cs.FlushFinalBlock();
    }

    public static bool DecryptFile(string fileName, string algorithmName, RSAParameters rsaParameters)
    {
        // Create instance of RSA for asymmetric decryption of the AES key.
        var rsa = RSA.Create();
        rsa.ImportParameters(rsaParameters);
        // Create instance of the specified algorithm for symmetric decryption of the file.
        var symAlg = GetAlgorithm(algorithmName);
        using var inFs = new FileStream(fileName, FileMode.Open);
        using var br = new BinaryReader(inFs);
        // Read the key, IV and file extension length from the (inFs) FileStream.
        var lKey = br.ReadInt32();
        var lIv = br.ReadInt32();
        var lExt = br.ReadInt32();
        // Read the key, IV and file extension from the (inFs) FileStream.
        var keyEncrypted = br.ReadBytes(lKey);
        var iv = br.ReadBytes(lIv);
        var extension = br.ReadBytes(lExt);
        try
        {
            // Use RSACryptoServiceProvider to decrypt the AES key.
            var keyDecrypted = rsa.Decrypt(keyEncrypted, RSAEncryptionPadding.Pkcs1);
            // Change the file's extension to the original extension.
            var outFile = Path.ChangeExtension(fileName, Encoding.UTF8.GetString(extension));
            using var outFs = new FileStream(outFile, FileMode.Create);
            inFs.Position = lKey + lIv + lExt + 12;
            using var cs = new CryptoStream(outFs, symAlg.CreateDecryptor(keyDecrypted, iv), CryptoStreamMode.Write);
            // By decrypting a chunk a time, you can save memory and accommodate large files.
            // blockSizeBytes can be any arbitrary size.
            var blockSizeBytes = symAlg.BlockSize / 8;
            var data = new byte[blockSizeBytes];
            int count;
            do
            {
                count = inFs.Read(data, 0, blockSizeBytes);
                cs.Write(data, 0, count);
            } while (count > 0);

            cs.FlushFinalBlock();
            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}