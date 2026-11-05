using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using System.Threading.Tasks;

namespace Passwortspeicher.Services;

public class CryptoService
{
    // Hash-Wert für den Account-Benutzernamen (SHA-256)
    public static string HashUsername(string username)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(username.ToLower().Trim());
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    // Master-Key aus Master-Passwort und Salt ableiten (Argon2id)
    public static byte[] DeriveKey(string password, string saltBase64)
    {
        byte[] salt = Convert.FromBase64String(saltBase64);
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 8,
            MemorySize = 65536,
            Iterations = 4
        };
        return argon2.GetBytes(32);
    }

    public static string GenerateSalt()
    {
        byte[] salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        return Convert.ToBase64String(salt);
    }

    public static string HashPassword(string password, string saltBase64)
    {
        byte[] key = DeriveKey(password, saltBase64);
        return Convert.ToBase64String(key);
    }

    public static bool VerifyPassword(string password, string storedHash, string saltBase64)
    {
        string newHash = HashPassword(password, saltBase64);
        return newHash == storedHash;
    }

    public static string GenerateRandomPassword(int length = 16)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=";
        var data = new byte[length];
        RandomNumberGenerator.Fill(data);
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[data[i] % chars.Length];
        }
        return new string(result);
    }

    // AES-256-GCM Verschlüsselung
    public static string Encrypt(string plainText, byte[] key)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        byte[] nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipherBytes = new byte[plainBytes.Length];
        byte[] tag = new byte[16];

        using (var aes = new AesGcm(key, 16))
        {
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        byte[] result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, nonce.Length + tag.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    // AES-256-GCM Entschlüsselung
    public static string Decrypt(string cipherTextBase64, byte[] key)
    {
        if (string.IsNullOrEmpty(cipherTextBase64)) return string.Empty;

        byte[] encryptedData = Convert.FromBase64String(cipherTextBase64);
        if (encryptedData.Length < 28) return string.Empty;

        byte[] nonce = new byte[12];
        byte[] tag = new byte[16];
        byte[] cipherBytes = new byte[encryptedData.Length - 28];

        Buffer.BlockCopy(encryptedData, 0, nonce, 0, 12);
        Buffer.BlockCopy(encryptedData, 12, tag, 0, 16);
        Buffer.BlockCopy(encryptedData, 28, cipherBytes, 0, cipherBytes.Length);

        byte[] plainBytes = new byte[cipherBytes.Length];

        using (var aes = new AesGcm(key, 16))
        {
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
        }

        return Encoding.UTF8.GetString(plainBytes);
    }
}