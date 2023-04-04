using System.Security.Cryptography;
using System.Text;

#pragma warning disable SYSLIB0022

namespace CryptoLib.Services;

public static class EncryptionService
{
    private static SymmetricAlgorithm GetAlgorithm(string name)
    {
        return name switch
        {
            "AES" => Aes.Create(),
            "DES" => DES.Create(),
            "RC2" => RC2.Create(),
            "Rijndael" => Rijndael.Create(),
            "TripleDES" => TripleDES.Create(),
            _ => throw new CryptographicException("Invalid encryption algorithm")
        };
    }

    public static byte[] EncryptImage(byte[] bytes, string algorithmName, CipherMode cipherMode = CipherMode.CBC)
    {
        using var algorithm = GetAlgorithm(algorithmName);
        // Set the encryption key and generate an Initialization Vector
        algorithm.GenerateKey();
        algorithm.GenerateIV();
        algorithm.Mode = cipherMode;
        using var ms = new MemoryStream();
        // Create cryptographic stream
        var encryptor = algorithm.CreateEncryptor();
        using var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        // Encrypt the bytes
        cryptoStream.Write(bytes, 0, bytes.Length);
        return ms.ToArray();
    }

    public static void EncryptFile(string fileName, string algorithmName, RSAParameters rsaParameters)
    {
        // Create instance of RSA for asymmetric encryption of the AES key.
        var rsa = RSA.Create();
        rsa.ImportParameters(rsaParameters);
        // Create instance of the specified algorithm for symmetric encryption of the file.
        var symAlg = GetAlgorithm(algorithmName);
        var transform = symAlg.CreateEncryptor();
        // Use RSACryptoServiceProvider to encrypt the AES key.
        var keyEncrypted = rsa.Encrypt(symAlg.Key, RSAEncryptionPadding.Pkcs1);
        // Create byte arrays to contain the length values of the key, IV and file extension.
        var lKey = keyEncrypted.Length;
        var lIv = symAlg.IV.Length;
        var extension = Encoding.UTF8.GetBytes(Path.GetExtension(fileName));
        var lExt = extension.Length;
        // Change the file's extension to ".enc"
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
        using var cs = new CryptoStream(outFs, transform, CryptoStreamMode.Write);
        // By encrypting a chunk at a time, you can save memory and accommodate large files. blockSizeBytes can be any arbitrary size.
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

    public static void DecryptFile(string fileName, string algorithmName, RSAParameters rsaParameters)
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
        // Use RSACryptoServiceProvider to decrypt the AES key.
        var keyDecrypted = rsa.Decrypt(keyEncrypted, RSAEncryptionPadding.Pkcs1);
        // Change the file's extension to the original extension.
        var outFile = Path.ChangeExtension(fileName, Encoding.UTF8.GetString(extension));
        using var outFs = new FileStream(outFile, FileMode.Create);
        inFs.Position = lKey + lIv + lExt + 12;
        using var cs = new CryptoStream(outFs, symAlg.CreateDecryptor(keyDecrypted, iv), CryptoStreamMode.Write);
        // By decrypting a chunk a time, you can save memory and accommodate large files. blockSizeBytes can be any arbitrary size.
        var blockSizeBytes = symAlg.BlockSize / 8;
        var data = new byte[blockSizeBytes];
        int count;
        do
        {
            count = inFs.Read(data, 0, blockSizeBytes);
            cs.Write(data, 0, count);
        } while (count > 0);

        cs.FlushFinalBlock();
    }
}