using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SafeVault.Helpers;
using SafeVault.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SafeVault.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Token, string Role)> LoginUser(string username, string password);
        Task<(bool Success, string Message)> RegisterUser(RegisterModel model);
        Task<bool> IsUserInRole(string username, string role);
    }

    public class AuthService : IAuthService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<(bool Success, string Token, string Role)> LoginUser(string username, string password)
        {
            string allowedSpecialCharacters = "!@#$%^&*?";

            if (!ValidationHelpers.IsValidInput(username) ||
                !ValidationHelpers.IsValidInput(password, allowedSpecialCharacters))
                return (false, "", "");

            string query = "SELECT Password, Role FROM Users WHERE Username = @Username";
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string storedHash = reader.GetString(0);
                            string userRole = reader.GetString(1);
                            
                            // Verify the password hash
                            bool passwordValid = BCrypt.Net.BCrypt.Verify(password, storedHash);
                            
                            if (passwordValid)
                            {
                                // Generate JWT token
                                string token = GenerateJwtToken(username, userRole);
                                return (true, token, userRole);
                            }
                        }
                    }
                }
            }
            
            return (false, "", "");
        }

        public async Task<(bool Success, string Message)> RegisterUser(RegisterModel model)
        {
            // Validate input
            string allowedSpecialCharacters = "!@#$%^&*?";
            if (!ValidationHelpers.IsValidInput(model.Username) ||
                !ValidationHelpers.IsValidInput(model.Password, allowedSpecialCharacters))
            {
                return (false, "Invalid username or password format");
            }
            
            // Check if username already exists
            if (await UsernameExists(model.Username))
            {
                return (false, "Username already exists");
            }
            
            // Check if email already exists
            if (await EmailExists(model.Email))
            {
                return (false, "Email already exists");
            }
            
            // Hash the password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            
            // Insert the new user
            string query = @"
                INSERT INTO Users (Username, Password, Email, Role, CreatedAt)
                VALUES (@Username, @Password, @Email, @Role, @CreatedAt)";
                
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", model.Username);
                    command.Parameters.AddWithValue("@Password", passwordHash);
                    command.Parameters.AddWithValue("@Email", model.Email);
                    command.Parameters.AddWithValue("@Role", "User"); // Default role
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                    
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        return (true, "User registered successfully");
                    }
                }
            }
            
            return (false, "Failed to register user");
        }

        public async Task<bool> IsUserInRole(string username, string role)
        {
            string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND Role = @Role";
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Role", role);
                    
                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        private string GenerateJwtToken(string username, string role)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? 
                throw new InvalidOperationException("JWT Secret Key not configured");
                
            var key = Encoding.ASCII.GetBytes(secretKey);
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] 
                { 
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(jwtSettings["ExpiryInMinutes"] ?? "60")),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        private async Task<bool> UsernameExists(string username)
        {
            string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    
                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }
        
        private async Task<bool> EmailExists(string email)
        {
            string query = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    
                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }
    }
}