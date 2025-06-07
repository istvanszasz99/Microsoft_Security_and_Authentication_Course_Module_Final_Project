using Microsoft.Data.SqlClient;
using System.IO;

namespace SafeVault.Helpers
{
    public static class DatabaseSetup
    {
        public static async Task InitializeDatabase(string connectionString)
        {
            try
            {
                // Ensure the database exists
                await EnsureDatabaseCreated(connectionString);
                
                // Execute SQL script to create tables and seed data
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DB", "InitializeDatabase.sql");
                
                if (File.Exists(scriptPath))
                {
                    string script = await File.ReadAllTextAsync(scriptPath);
                    await ExecuteSqlScript(connectionString, script);
                }
                else
                {
                    throw new FileNotFoundException($"Database initialization script not found at: {scriptPath}");
                }
            }
            catch (Exception ex)
            {
                // In a real application, you'd want to use proper logging here
                Console.WriteLine($"Error initializing database: {ex.Message}");
                throw;
            }
        }
        
        private static async Task EnsureDatabaseCreated(string connectionString)
        {
            // Extract database name from connection string
            var builder = new SqlConnectionStringBuilder(connectionString);
            string databaseName = builder.InitialCatalog;
            
            // Remove the database name for connecting to master
            builder.InitialCatalog = "master";
            string masterConnectionString = builder.ConnectionString;
            
            using (var connection = new SqlConnection(masterConnectionString))
            {
                await connection.OpenAsync();
                
                // Check if database exists
                string checkDbQuery = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";
                using (var command = new SqlCommand(checkDbQuery, connection))
                {
                    int dbExists = Convert.ToInt32(await command.ExecuteScalarAsync());
                    
                    if (dbExists == 0)
                    {
                        // Create the database if it doesn't exist
                        string createDbQuery = $"CREATE DATABASE [{databaseName}]";
                        using (var createCommand = new SqlCommand(createDbQuery, connection))
                        {
                            await createCommand.ExecuteNonQueryAsync();
                            Console.WriteLine($"Database '{databaseName}' created");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Database '{databaseName}' already exists");
                    }
                }
            }
        }
        
        private static async Task ExecuteSqlScript(string connectionString, string script)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                // Split script by GO statements if present
                string[] commands = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string command in commands)
                {
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        using (var cmd = new SqlCommand(command, connection))
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }
    }
}
