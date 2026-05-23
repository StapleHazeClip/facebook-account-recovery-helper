using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FacebookAccountRecoveryHelper;
using NUnit.Framework;

namespace FacebookAccountRecoveryHelper.Tests
{
    /// <summary>
    /// Unit tests for RecoveryEngine and related components.
    /// </summary>
    [TestFixture]
    public class RecoveryEngineTests
    {
        private string _testWordlistPath;

        [SetUp]
        public void Setup()
        {
            // Create a temporary wordlist for testing
            _testWordlistPath = Path.GetTempFileName();
            File.WriteAllLines(_testWordlistPath, new[] { "testpass1", "testpass2", "testpass3" });
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(_testWordlistPath))
                File.Delete(_testWordlistPath);
        }

        /// <summary>
        /// Tests that ConfigLoader returns default config when file doesn't exist.
        /// </summary>
        [Test]
        public void ConfigLoader_FileNotFound_ReturnsDefault()
        {
            var config = ConfigLoader.LoadConfig("nonexistent.json");
            Assert.IsNotNull(config);
            Assert.AreEqual("example@facebook.com", config.TargetEmail);
            Assert.AreEqual("wordlist.txt", config.WordlistPath);
        }

        /// <summary>
        /// Tests that ConfigLoader correctly parses a valid JSON file.
        /// </summary>
        [Test]
        public void ConfigLoader_ValidFile_ReturnsConfig()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "{\"TargetEmail\":\"test@test.com\",\"WordlistPath\":\"test.txt\",\"DelayMs\":1000}");
            var config = ConfigLoader.LoadConfig(tempFile);
            Assert.IsNotNull(config);
            Assert.AreEqual("test@test.com", config.TargetEmail);
            Assert.AreEqual("test.txt", config.WordlistPath);
            Assert.AreEqual(1000, config.DelayMs);
            File.Delete(tempFile);
        }

        /// <summary>
        /// Tests that RecoveryEngine.RunAsync returns failure when wordlist is empty.
        /// </summary>
        [Test]
        public async Task RecoveryEngine_EmptyWordlist_ReturnsFailure()
        {
            var emptyWordlist = Path.GetTempFileName();
            File.WriteAllText(emptyWordlist, "");
            var config = new Config
            {
                TargetEmail = "test@test.com",
                WordlistPath = emptyWordlist,
                DelayMs = 100,
                MaxAttempts = 10,
                Headless = true
            };
            var engine = new RecoveryEngine(config);
            var result = await engine.RunAsync();
            Assert.IsFalse(result.Success);
            Assert.AreEqual(0, result.AttemptsMade);
            File.Delete(emptyWordlist);
        }

        /// <summary>
        /// Tests that Config properties are set correctly.
        /// </summary>
        [Test]
        public void Config_DefaultValues_Correct()
        {
            var config = new Config();
            Assert.AreEqual(2000, config.DelayMs);
            Assert.AreEqual(1000, config.MaxAttempts);
            Assert.AreEqual("chromedriver", config.ChromeDriverPath);
            Assert.IsTrue(config.Headless);
        }
    }
}
