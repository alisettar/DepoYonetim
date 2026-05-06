using System.Security.Cryptography;

namespace WMS.Shared.Common.Cryptography;

public class AesGcmService
{
    private readonly byte[] _masterKey;
    private const int KeySize = 256;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public AesGcmService(byte[] masterKey)
    {
        if (masterKey.Length != KeySize / 8)
            throw new ArgumentException($"Master key must be {KeySize} bits ({KeySize / 8} bytes).", nameof(masterKey));

        _masterKey = masterKey;
    }

    public string Encrypt(string plaintext)
    {
        using var aes = new AesGcm(_masterKey, TagSize);
        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var nonce = new byte[NonceSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(nonce);
        var tag = new byte[TagSize];
        var ciphertext = new byte[plaintextBytes.Length];

        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Format: nonce(24) + tag(32) + ciphertext(hex)
        var nonceHex = BitConverter.ToString(nonce).Replace("-", "").ToLower();
        var tagHex = BitConverter.ToString(tag).Replace("-", "").ToLower();
        var cipherHex = Convert.ToHexStringLower(ciphertext);

        return $"{nonceHex}${tagHex}${cipherHex}";
    }

    public string Decrypt(string encrypted)
    {
        var parts = encrypted.Split('$');
        if (parts.Length != 3)
            throw new ArgumentException("Invalid encrypted format.", nameof(encrypted));

        var nonce = HexToBytes(parts[0]);
        var tag = HexToBytes(parts[1]);
        var ciphertext = Convert.FromHexString(parts[2]);

        using var aes = new AesGcm(_masterKey, TagSize);
        var plaintext = new byte[ciphertext.Length];
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return System.Text.Encoding.UTF8.GetString(plaintext);
    }

    private static byte[] HexToBytes(string hex)
    {
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }
}
