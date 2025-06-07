using Microsoft.AspNetCore.Http;
using SafeVault.Services;
using System.Security.Claims;

namespace SafeVault.Middleware
{
    public class RoleBasedAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleBasedAuthorizationMiddleware> _logger;
        
        // Dictionary of paths that require specific roles
        private readonly Dictionary<string, string[]> _protectedRoutes = new Dictionary<string, string[]>
        {
            { "/admin", new[] { "Admin" } },
            { "/dashboard", new[] { "Admin", "User" } },
            { "/api/protected", new[] { "Admin", "User" } },
            // Add more protected routes as needed
        };

        public RoleBasedAuthorizationMiddleware(
            RequestDelegate next,
            ILogger<RoleBasedAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            string path = context.Request.Path.Value?.ToLower() ?? string.Empty;
            
            // Check if this path requires authorization
            bool isProtected = false;
            string[]? requiredRoles = null;
            
            foreach (var route in _protectedRoutes)
            {
                if (path.StartsWith(route.Key.ToLower()))
                {
                    isProtected = true;
                    requiredRoles = route.Value;
                    break;
                }
            }
            
            if (isProtected && requiredRoles != null)
            {
                // Check if user is authenticated
                if (!context.User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogWarning("Unauthorized access attempt to {Path}", path);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Authentication required");
                    return;
                }
                
                // Get username from claims
                var username = context.User.FindFirst(ClaimTypes.Name)?.Value;
                
                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Username claim not found for authenticated user");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid authentication token");
                    return;
                }
                
                // Check if user has any of the required roles
                bool authorized = false;
                foreach (var role in requiredRoles)
                {
                    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                    if (userRole == role)
                    {
                        authorized = true;
                        break;
                    }
                }
                
                if (!authorized)
                {
                    _logger.LogWarning("User {Username} attempted to access {Path} without proper authorization", username, path);
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("You do not have permission to access this resource");
                    return;
                }
                
                _logger.LogInformation("User {Username} authorized to access {Path}", username, path);
            }
            
            await _next(context);
        }
    }
    
    // Extension method used to add the middleware to the HTTP request pipeline
    public static class RoleBasedAuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleBasedAuthorization(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleBasedAuthorizationMiddleware>();
        }
    }
}
