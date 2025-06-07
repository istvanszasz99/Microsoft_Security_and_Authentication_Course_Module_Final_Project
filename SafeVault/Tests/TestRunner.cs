using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace SafeVault.Tests
{
    public static class TestRunner
    {
        public static async Task RunTests(WebApplication app)
        {
            // Wait for the application to fully start
            await Task.Delay(2000);
            
            Console.WriteLine("\n-------------------------");
            Console.WriteLine("Running Security Tests");
            Console.WriteLine("-------------------------\n");
            
            var authTests = new AuthTests();
            await authTests.RunAllTests();
        }
    }
}
