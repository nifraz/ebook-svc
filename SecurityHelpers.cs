using ebook_svc.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ebook_svc
{
    public static class SecurityHelpers
    {
        private static readonly byte[] AesKey = Encoding.UTF8.GetBytes("123456$#@$^@1ERF");
        private static readonly byte[] AesIV = Encoding.UTF8.GetBytes("123456$#@$^@1ERF");

        // Decrypts the AES-encrypted password from the client
        public static string DecryptPassword(string encryptedBase64)
        {
            if (string.IsNullOrEmpty(encryptedBase64))
                return string.Empty;
            using var aes = Aes.Create();
            aes.Key = AesKey;
            aes.IV = AesIV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var ms = new MemoryStream(Convert.FromBase64String(encryptedBase64));
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            string plaintext = reader.ReadToEnd();
            return plaintext;
        }

        // Hash password for storage (using a simple SHA256 here for demonstration; in production use BCrypt or similar)
        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // Verify a plaintext password against a stored hash
        public static bool VerifyPassword(string plaintext, string hash)
        {
            return HashPassword(plaintext) == hash;
        }

        // Generate a JWT for the authenticated user
        public static string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("Your_JWT_Secret_Key"); // same key as configured in JWT options
            var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email ?? user.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString())  // e.g. "Customer", "Vendor", "Admin"
        };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

}
