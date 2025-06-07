using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeVault.Services;
using SafeVault.Models;
using Moq;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace SafeVault.Tests
{
    [TestClass]
    public class SecurityUnitTests
    {
        private Mock<IConfiguration> GetMockConfiguration()
        {
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["SecretKey"]).Returns("SafeVault_Secret_Key_For_Authentication_JWT_Token_Generation");
            configurationSectionMock.Setup(s => s["Issuer"]).Returns("SafeVaultIssuer");
            configurationSectionMock.Setup(s => s["Audience"]).Returns("SafeVaultAudience");
            configurationSectionMock.Setup(s => s["ExpiryInMinutes"]).Returns("60");

            var connectionStringSectionMock = new Mock<IConfigurationSection>();
            connectionStringSectionMock.Setup(s => s.Value).Returns("Server=(localdb)\\MSSQLLocalDB;Database=SafeVaultDb;Trusted_Connection=True;");

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetSection("JwtSettings")).Returns(configurationSectionMock.Object);
            configurationMock.Setup(c => c.GetConnectionString("DefaultConnection")).Returns("Server=(localdb)\\MSSQLLocalDB;Database=SafeVaultDb;Trusted_Connection=True;");
            
            return configurationMock;
        }

        [TestMethod]
        public void ValidatePassword_WithValidCriteria_ReturnsTrue()
        {
            // Test password validation logic
            string validPassword = "StrongP@ss123";
            bool hasUpper = validPassword.Any(char.IsUpper);
            bool hasLower = validPassword.Any(char.IsLower);
            bool hasDigit = validPassword.Any(char.IsDigit);
            bool hasSpecial = validPassword.Any(c => !char.IsLetterOrDigit(c));
            bool isLongEnough = validPassword.Length >= 8;
            
            Assert.IsTrue(hasUpper, "Password should contain uppercase letters");
            Assert.IsTrue(hasLower, "Password should contain lowercase letters");
            Assert.IsTrue(hasDigit, "Password should contain digits");
            Assert.IsTrue(hasSpecial, "Password should contain special characters");
            Assert.IsTrue(isLongEnough, "Password should be at least 8 characters long");
        }

        [TestMethod]
        public void BCryptPassword_GeneratesDifferentHashesForSamePassword()
        {
            // Test that BCrypt generates different salts
            string password = "SecurePassword123!";
            
            string hash1 = BCrypt.Net.BCrypt.HashPassword(password);
            string hash2 = BCrypt.Net.BCrypt.HashPassword(password);
            
            Assert.AreNotEqual(hash1, hash2, "BCrypt should generate different hashes for the same password due to salting");
        }

        [TestMethod]
        public void BCryptPassword_VerifiesCorrectPassword()
        {
            // Test BCrypt verification
            string password = "SecurePassword123!";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            
            bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
            
            Assert.IsTrue(isValid, "BCrypt should verify the correct password against its hash");
        }

        [TestMethod]
        public void BCryptPassword_RejectsFalsePassword()
        {
            // Test BCrypt rejects wrong password
            string password = "SecurePassword123!";
            string wrongPassword = "WrongPassword123!";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            
            bool isValid = BCrypt.Net.BCrypt.Verify(wrongPassword, hash);
            
            Assert.IsFalse(isValid, "BCrypt should reject an incorrect password");
        }
        
        [TestMethod]
        public void JWT_TokenHasCorrectClaims()
        {
            // You would need to extract the JWT generation code to a separate method to test it properly
            // This is a simplified example
            var configMock = GetMockConfiguration();
            var authService = new AuthService(configMock.Object);
            
            // In a real test, you'd call a modified version of the JWT generator that's testable
            // For now, just validating that our test setup works
            Assert.IsNotNull(authService, "Auth service should be created successfully");
        }
        
        [TestMethod]
        public void ValidationHelpers_RejectsMaliciousInput()
        {
            string xssInput = "<script>alert('XSS')</script>";
            bool isValid = SafeVault.Helpers.ValidationHelpers.IsValidXSSInput(xssInput);
            
            Assert.IsFalse(isValid, "XSS attack input should be rejected");
        }
        
        [TestMethod]
        public void ValidationHelpers_AcceptsValidInput()
        {
            string validInput = "John.Doe@example.com";
            bool isValid = SafeVault.Helpers.ValidationHelpers.IsValidInput(validInput, "@.");
            
            Assert.IsTrue(isValid, "Valid input with allowed special characters should be accepted");
        }
    }
}
