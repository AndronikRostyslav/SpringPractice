using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Provides methods for hashing and verifying passwords using SHA-256.
/// </summary>
public class PasswordHasher
{
    /// <summary>
    /// Hashes a password using SHA-256.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password as a hexadecimal string.</returns>
    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var stringBuilder = new StringBuilder();

            foreach (var b in hashedBytes)
            {
                stringBuilder.Append(b.ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// Verifies if the entered password matches the stored hashed password.
    /// </summary>
    /// <param name="enteredPassword">The password entered by the user.</param>
    /// <param name="storedPassword">The stored hashed password.</param>
    /// <returns>True if the entered password matches the stored hashed password, otherwise false.</returns>
    public static bool VerifyPassword(string enteredPassword, string storedPassword)
    {
        return HashPassword(enteredPassword) == storedPassword;
    }
}


