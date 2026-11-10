using System.Security.Cryptography;

namespace FrotaGO.Helpers;

/// <summary>Responsabilidade única: gerar e verificar hash de senha (PBKDF2).</summary>
public static class PasswordHasher
{
    private const int SaltSize = 16, KeySize = 32, Iterations = 100_000;

    public static string Hash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string senha, string hash)
    {
        var partes = hash.Split('.');
        if (partes.Length != 2) return false;
        var salt = Convert.FromBase64String(partes[0]);
        var esperado = Convert.FromBase64String(partes[1]);
        var atual = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return CryptographicOperations.FixedTimeEquals(esperado, atual);
    }
}
