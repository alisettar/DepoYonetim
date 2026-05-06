using System.Security.Cryptography;

namespace WMS.Shared.Common.Cryptography;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;
    private const string Delimiter = "$";

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);

        var saltHex = Convert.ToHexStringLower(salt);
        var hashHex = Convert.ToHexStringLower(hash);

        return $"pbkdf2$sha256${Iterations}${saltHex}${hashHex}";
    }

    public static bool Verify(string password, string hash)
    {
        var parts = hash.Split(Delimiter);
        if (parts.Length != 5) return false;

        if (parts[0] != "pbkdf2" || parts[1] != "sha256") return false;

        if (!int.TryParse(parts[2], out var iterations) || iterations <= 0) return false;

        var salt = Convert.FromHexString(parts[3]);
        var storedHash = Convert.FromHexString(parts[4]);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var computedHash = pbkdf2.GetBytes(HashSize);

        return SpanEquals(computedHash, storedHash);
    }

    private static bool SpanEquals(Span<byte> a, Span<byte> b)
    {
        if (a.Length != b.Length) return false;
        var result = 0;
        for (var i = 0; i < a.Length; i++)
            result |= a[i] ^ b[i];
        return result == 0;
    }
}
