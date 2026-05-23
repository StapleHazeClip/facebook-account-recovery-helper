using System;
using System.IO;
using Newtonsoft.Json;

namespace FacebookAccountRecoveryHelper
{
    /// <summary>
    /// Configuration settings for the recovery process.
    /// </summary>
    public class Config
    {
        /// <summary>Target Facebook account email address.</summary>
        public string TargetEmail { get; set; }

        /// <summary>Path to the wordlist file containing potential passwords.</summary>
        public string WordlistPath { get; set; }

        /// <summary>Delay between attempts in milliseconds to avoid rate limiting.</summary>
        public int DelayMs { get; set; } = 2000;

        /// <summary>Maximum number of attempts before stopping.</summary>
        public int MaxAttempts { get; set; } = 1000;

        /// <summary>Path to ChromeDriver executable.</summary>
        public string ChromeDriverPath { get; set; } = "chromedriver";

        /// <summary>Whether to use headless mode for the browser.</summary>
        public bool Headless { get; set; } = true;
    }

    /// <summary>
    /// Loads configuration from a JSON file.
    /// </summary>
    public static class ConfigLoader
    {
        /// <summary>
        /// Loads and deserializes the configuration from the specified file path.
        /// </summary>
        /// <param name="filePath">Path to the JSON config file.</param>
        /// <returns>Config object if successful, null otherwise.</returns>
        public static Config LoadConfig(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Config file not found: {filePath}. Using default settings.");
                    return new Config
                    {
                        TargetEmail = "example@facebook.com",
                        WordlistPath = "wordlist.txt",
                        DelayMs = 2000,
                        MaxAttempts = 100,
                        ChromeDriverPath = "chromedriver",
                        Headless = true
                    };
                }

                var json = File.ReadAllText(filePath);
                var config = JsonConvert.DeserializeObject<Config>(json);
                Console.WriteLine("Configuration loaded successfully.");
                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
                return null;
            }
        }
    }
}
