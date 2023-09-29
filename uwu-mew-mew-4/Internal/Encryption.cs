using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace uwu_mew_mew_4.Internal;

internal static class Encryption
{
    private const int IvLength = 16;

    private const string Salt = "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";
    private static readonly byte[] Key;

    static Encryption()
    {
        var key = Environment.GetEnvironmentVariable("ENCRYPTION_KEY")!;
        var argon = new Argon2d(Encoding.UTF8.GetBytes(key));
        argon.Salt = Encoding.UTF8.GetBytes(Salt);
        argon.DegreeOfParallelism = 16;
        argon.MemorySize = 16000;
        argon.Iterations = 2;

        Key = argon.GetBytes(32);

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
    }


    public static byte[] Encrypt(byte[] data)
    {
        using var aes = Aes.Create();
        if (aes == null)
            throw new ApplicationException("Failed to create AES cipher.");

        aes.Key = Key;

        using var buffer = new MemoryStream();
        buffer.Write(aes.IV, 0, IvLength);

        using (var cryptoStream = new CryptoStream(buffer, aes.CreateEncryptor(), CryptoStreamMode.Write))
        using (var writer = new BinaryWriter(cryptoStream))
        {
            writer.Write(data);
        }

        var encryptedBytes = buffer.ToArray();

        return encryptedBytes;
    }

    public static byte[] Decrypt(byte[] encryptedData)
    {
        using var aes = Aes.Create();
        if (aes == null)
            throw new ApplicationException("Failed to create AES cipher.");

        aes.Key = Key;

        using var buffer = new MemoryStream(encryptedData);
        var iv = new byte[IvLength];
        // ReSharper disable once MustUseReturnValue
        // Memory stream will always give expected amount of bytes
        buffer.Read(iv, 0, IvLength);
        aes.IV = iv;

        using var cryptoStream = new CryptoStream(buffer, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var reader = new BinaryReader(cryptoStream);
        var decryptedBytes = reader.ReadBytes(encryptedData.Length - IvLength);

        return decryptedBytes;
    }
}