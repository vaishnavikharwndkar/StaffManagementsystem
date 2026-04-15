using System.Security.Cryptography;
using System.Text;

namespace StaffTaskManagement.Utilities;

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    public static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password).Equals(hash, StringComparison.OrdinalIgnoreCase);
    }
}

