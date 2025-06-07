using System;
using System.Threading.Tasks;

namespace SafeVault.Tests
{
    public class RunTests
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("SafeVault Authentication and Authorization Test Runner");
            Console.WriteLine("=====================================================");
            Console.WriteLine("IMPORTANT: Make sure the SafeVault application is running before starting tests.");
            Console.WriteLine("Press any key to start tests...");
            Console.ReadKey();
            
            var authTests = new AuthTests();
            await authTests.RunAllTests();
            
            Console.WriteLine("\nAll tests completed. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
