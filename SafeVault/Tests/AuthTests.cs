using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SafeVault.Models;

namespace SafeVault.Tests
{
    public class AuthTests
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        
        public AuthTests()
        {
            _client = new HttpClient();
            _baseUrl = "https://localhost:5001";  // Update with your application's URL
        }
        
        public async Task RunAllTests()
        {
            try
            {
                Console.WriteLine("Starting Authentication and Authorization Tests");
                Console.WriteLine("===============================================");
                
                // Test 1: Valid Login
                await TestValidLogin();
                
                // Test 2: Invalid Login
                await TestInvalidLogin();
                
                // Test 3: Registration
                await TestRegistration();
                
                // Test 4: Access Protected Route with Admin Role
                await TestAdminAccess();
                
                // Test 5: Access Protected Route with User Role
                await TestUserAccess();
                
                // Test 6: Unauthorized Access Attempt
                await TestUnauthorizedAccess();
                
                Console.WriteLine("All tests completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running tests: {ex.Message}");
            }
        }
        
        private async Task TestValidLogin()
        {
            Console.WriteLine("\nTest 1: Valid Login");
            
            var loginModel = new LoginModel
            {
                Username = "admin",
                Password = "Admin@123"
            };
            
            var response = await _client.PostAsJsonAsync($"{_baseUrl}/api/auth/login", loginModel);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                Console.WriteLine("SUCCESS: Valid login succeeded");
                Console.WriteLine($"Token: {result?.Token.Substring(0, 20)}...");
                Console.WriteLine($"Role: {result?.Role}");
                
                // Store token for subsequent tests
                _client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", result?.Token);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"FAILED: Valid login failed with status {response.StatusCode}");
                Console.WriteLine($"Error: {error}");
            }
        }
        
        private async Task TestInvalidLogin()
        {
            Console.WriteLine("\nTest 2: Invalid Login");
            
            var loginModel = new LoginModel
            {
                Username = "admin",
                Password = "WrongPassword"
            };
            
            var response = await _client.PostAsJsonAsync($"{_baseUrl}/api/auth/login", loginModel);
            
            if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("SUCCESS: Invalid login correctly rejected");
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"FAILED: Invalid login returned unexpected status {response.StatusCode}");
                Console.WriteLine($"Content: {content}");
            }
        }
        
        private async Task TestRegistration()
        {
            Console.WriteLine("\nTest 3: User Registration");
            
            var registrationModel = new RegisterModel
            {
                Username = $"testuser_{DateTime.Now.Ticks}",
                Password = "TestUser@123",
                ConfirmPassword = "TestUser@123",
                Email = $"testuser_{DateTime.Now.Ticks}@example.com"
            };
            
            var response = await _client.PostAsJsonAsync($"{_baseUrl}/api/auth/register", registrationModel);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine("SUCCESS: User registration successful");
                Console.WriteLine($"Result: {result}");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"FAILED: User registration failed with status {response.StatusCode}");
                Console.WriteLine($"Error: {error}");
            }
        }
        
        private async Task TestAdminAccess()
        {
            Console.WriteLine("\nTest 4: Admin Access");
            
            // First login as admin to get token
            var loginModel = new LoginModel
            {
                Username = "admin",
                Password = "Admin@123"
            };
            
            var loginResponse = await _client.PostAsJsonAsync($"{_baseUrl}/api/auth/login", loginModel);
            
            if (loginResponse.IsSuccessStatusCode)
            {
                var loginResult = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
                _client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", loginResult?.Token);
                    
                // Try to access admin dashboard
                var adminResponse = await _client.GetAsync($"{_baseUrl}/api/admin/dashboard");
                
                if (adminResponse.IsSuccessStatusCode)
                {
                    var dashboardData = await adminResponse.Content.ReadAsStringAsync();
                    Console.WriteLine("SUCCESS: Admin successfully accessed admin dashboard");
                    Console.WriteLine($"Dashboard data: {dashboardData}");
                }
                else
                {
                    var error = await adminResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"FAILED: Admin access to dashboard failed with status {adminResponse.StatusCode}");
                    Console.WriteLine($"Error: {error}");
                }
            }
            else
            {
                Console.WriteLine("FAILED: Could not log in as admin to test admin access");
            }
        }
        
        private async Task TestUserAccess()
        {
            Console.WriteLine("\nTest 5: Regular User Access");
            
            // Login as regular user
            var loginModel = new LoginModel
            {
                Username = "user",
                Password = "User@123"
            };
            
            var loginResponse = await _client.PostAsJsonAsync($"{_baseUrl}/api/auth/login", loginModel);
            
            if (loginResponse.IsSuccessStatusCode)
            {
                var loginResult = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
                _client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", loginResult?.Token);
                    
                // Try to access user dashboard
                var dashboardResponse = await _client.GetAsync($"{_baseUrl}/api/dashboard");
                
                if (dashboardResponse.IsSuccessStatusCode)
                {
                    var dashboardData = await dashboardResponse.Content.ReadAsStringAsync();
                    Console.WriteLine("SUCCESS: User successfully accessed user dashboard");
                    Console.WriteLine($"Dashboard data: {dashboardData}");
                    
                    // Try to access admin dashboard (should be forbidden)
                    var adminResponse = await _client.GetAsync($"{_baseUrl}/api/admin/dashboard");
                    
                    if (!adminResponse.IsSuccessStatusCode && 
                        adminResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        Console.WriteLine("SUCCESS: Regular user correctly denied access to admin dashboard");
                    }
                    else
                    {
                        Console.WriteLine($"FAILED: Regular user was not denied access to admin dashboard. Status: {adminResponse.StatusCode}");
                    }
                }
                else
                {
                    var error = await dashboardResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"FAILED: User access to dashboard failed with status {dashboardResponse.StatusCode}");
                    Console.WriteLine($"Error: {error}");
                }
            }
            else
            {
                Console.WriteLine("FAILED: Could not log in as regular user to test user access");
            }
        }
        
        private async Task TestUnauthorizedAccess()
        {
            Console.WriteLine("\nTest 6: Unauthorized Access Attempt");
            
            // Remove authorization header
            _client.DefaultRequestHeaders.Authorization = null;
            
            // Try to access protected route without token
            var response = await _client.GetAsync($"{_baseUrl}/api/dashboard");
            
            if (!response.IsSuccessStatusCode && 
                response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("SUCCESS: Unauthorized access correctly rejected");
            }
            else
            {
                Console.WriteLine($"FAILED: Unauthorized access not rejected properly. Status: {response.StatusCode}");
            }
        }
    }
    
    public class TokenResponse
    {
        public string Token { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
