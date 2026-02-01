using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Unistay_Web.Models;
using Unistay_Web.Services;

namespace UnistayWeb.Tests.Services
{
    [TestClass]
    public class ZodiacServiceTests
    {
        private IMemoryCache _cache;
        private ZodiacService _zodiacService;

        [TestInitialize]
        public void Setup()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _zodiacService = new ZodiacService(_cache);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cache.Dispose();
        }

        [TestMethod]
        public async Task GetAllZodiacSignsAsync_ShouldReturn12Signs()
        {
            // Act
            var result = await _zodiacService.GetAllZodiacSignsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(12, result.Count);
        }

        [TestMethod]
        public async Task GetAllZodiacSignsAsync_ShouldUseCache()
        {
            // Act
            var firstCall = await _zodiacService.GetAllZodiacSignsAsync();
            var secondCall = await _zodiacService.GetAllZodiacSignsAsync();

            // Assert
            Assert.IsNotNull(firstCall);
            Assert.IsNotNull(secondCall);
            Assert.AreSame(firstCall, secondCall);
        }

        [TestMethod]
        public async Task GetZodiacSignByIdAsync_WithValidId_ShouldReturnSign()
        {
            // Arrange
            int validId = 1; // Aries

            // Act
            var result = await _zodiacService.GetZodiacSignByIdAsync(validId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(validId, result.Id);
            Assert.AreEqual("Bạch Dương", result.Name);
            Assert.AreEqual("Aries", result.EnglishName);
        }

        [TestMethod]
        public async Task GetZodiacSignByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            int invalidId = 999;

            // Act
            var result = await _zodiacService.GetZodiacSignByIdAsync(invalidId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetZodiacSignByNameAsync_WithValidName_ShouldReturnSign()
        {
            // Arrange
            string validName = "Aries";

            // Act
            var result = await _zodiacService.GetZodiacSignByNameAsync(validName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Aries", result.EnglishName);
            Assert.AreEqual("Bạch Dương", result.Name);
        }

        [TestMethod]
        public async Task GetZodiacSignByNameAsync_CaseInsensitive_ShouldReturnSign()
        {
            // Arrange
            string validName = "aries"; // lowercase

            // Act
            var result = await _zodiacService.GetZodiacSignByNameAsync(validName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Aries", result.EnglishName);
        }

        [TestMethod]
        public async Task GetZodiacSignByNameAsync_WithInvalidName_ShouldReturnNull()
        {
            // Arrange
            string invalidName = "InvalidSign";

            // Act
            var result = await _zodiacService.GetZodiacSignByNameAsync(invalidName);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetZodiacSignByDateAsync_Aries_ShouldReturnCorrectSign()
        {
            // Arrange - Aries: March 21 - April 19
            DateTime ariesDate = new DateTime(2000, 3, 25);

            // Act
            var result = await _zodiacService.GetZodiacSignByDateAsync(ariesDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Aries", result.EnglishName);
            Assert.AreEqual("Bạch Dương", result.Name);
        }

        [TestMethod]
        public async Task GetZodiacSignByDateAsync_Taurus_ShouldReturnCorrectSign()
        {
            // Arrange - Taurus: April 20 - May 20
            DateTime taurusDate = new DateTime(2000, 5, 1);

            // Act
            var result = await _zodiacService.GetZodiacSignByDateAsync(taurusDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Taurus", result.EnglishName);
            Assert.AreEqual("Kim Ngưu", result.Name);
        }

        [TestMethod]
        public async Task GetZodiacSignByDateAsync_Capricorn_CrossYearBoundary_ShouldReturnCorrectSign()
        {
            // Arrange - Capricorn: Dec 22 - Jan 19 (crosses year boundary)
            DateTime capricornDateDec = new DateTime(2000, 12, 25);
            DateTime capricornDateJan = new DateTime(2000, 1, 10);

            // Act
            var resultDec = await _zodiacService.GetZodiacSignByDateAsync(capricornDateDec);
            var resultJan = await _zodiacService.GetZodiacSignByDateAsync(capricornDateJan);

            // Assert
            Assert.IsNotNull(resultDec);
            Assert.AreEqual("Capricorn", resultDec.EnglishName);
            Assert.IsNotNull(resultJan);
            Assert.AreEqual("Capricorn", resultJan.EnglishName);
        }

        [TestMethod]
        public async Task GetZodiacSignsByElementAsync_Fire_ShouldReturn3Signs()
        {
            // Act
            var result = await _zodiacService.GetZodiacSignsByElementAsync(ZodiacElement.Fire);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Exists(s => s.EnglishName == "Aries"));
            Assert.IsTrue(result.Exists(s => s.EnglishName == "Leo"));
            Assert.IsTrue(result.Exists(s => s.EnglishName == "Sagittarius"));
        }

        [TestMethod]
        public async Task GetZodiacSignsByElementAsync_Earth_ShouldReturn3Signs()
        {
            // Act
            var result = await _zodiacService.GetZodiacSignsByElementAsync(ZodiacElement.Earth);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Exists(s => s.EnglishName == "Taurus"));
            Assert.IsTrue(result.Exists(s => s.EnglishName == "Virgo"));
            Assert.IsTrue(result.Exists(s => s.EnglishName == "Capricorn"));
        }

        [TestMethod]
        public async Task GetZodiacSignsByElementAsync_Air_ShouldReturn3Signs()
        {
            // Act
            var result = await _zodiacService.GetZodiacSignsByElementAsync(ZodiacElement.Air);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Exists(s => s.EnglishName == "Gemini"));
            Assert.IsTrue(result.Exists(s => s.EnglishName == "Libra"));
            Assert.IsTrue(result.Exists(s => s.EnglishName == "Aquarius"));
        }

        [TestMethod]
        public async Task GetZodiacSignsByElementAsync_Water_ShouldReturn3Signs()
        {
            // Act
            var result = await _zodiacService.GetZodiacSignsByElementAsync(ZodiacElement.Water);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Exists(s => s.EnglishName == "Cancer"));
            Assert.IsTrue(result.Exists(s => s.EnglishName == "Scorpio"));
            Assert.IsTrue(result.Exists(s => s.EnglishName == "Pisces"));
        }

        [TestMethod]
        public async Task GetDailyHoroscopeAsync_ShouldReturnHoroscope()
        {
            // Arrange
            int zodiacSignId = 1; // Aries
            DateTime date = DateTime.Today;

            // Act
            var result = await _zodiacService.GetDailyHoroscopeAsync(zodiacSignId, date);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(zodiacSignId, result.ZodiacSignId);
            Assert.AreEqual(date.Date, result.Date.Date);
            Assert.IsFalse(string.IsNullOrEmpty(result.Content));
            Assert.IsTrue(result.LoveScore >= 0 && result.LoveScore <= 100);
            Assert.IsTrue(result.CareerScore >= 0 && result.CareerScore <= 100);
            Assert.IsTrue(result.HealthScore >= 0 && result.HealthScore <= 100);
            Assert.IsTrue(result.MoneyScore >= 0 && result.MoneyScore <= 100);
        }

        [TestMethod]
        public async Task GetDailyHoroscopeAsync_ShouldUseCache()
        {
            // Arrange
            int zodiacSignId = 1;
            DateTime date = DateTime.Today;

            // Act
            var firstCall = await _zodiacService.GetDailyHoroscopeAsync(zodiacSignId, date);
            var secondCall = await _zodiacService.GetDailyHoroscopeAsync(zodiacSignId, date);

            // Assert
            Assert.IsNotNull(firstCall);
            Assert.IsNotNull(secondCall);
            Assert.AreEqual(firstCall.Content, secondCall.Content);
            Assert.AreEqual(firstCall.LoveScore, secondCall.LoveScore);
        }

        [TestMethod]
        public async Task GetAllDailyHoroscopesAsync_ShouldReturn12Horoscopes()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            var result = await _zodiacService.GetAllDailyHoroscopesAsync(date);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(12, result.Count);
        }

        [TestMethod]
        public async Task AreCompatibleAsync_CompatibleSigns_ShouldReturnTrue()
        {
            // Arrange - Aries is compatible with Leo
            string sign1 = "Aries";
            string sign2 = "Leo";

            // Act
            var result = await _zodiacService.AreCompatibleAsync(sign1, sign2);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task AreCompatibleAsync_IncompatibleSigns_ShouldReturnFalse()
        {
            // Arrange - Aries is not compatible with Cancer
            string sign1 = "Aries";
            string sign2 = "Cancer";

            // Act
            var result = await _zodiacService.AreCompatibleAsync(sign1, sign2);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task AreCompatibleAsync_InvalidSign_ShouldReturnFalse()
        {
            // Arrange
            string sign1 = "InvalidSign";
            string sign2 = "Leo";

            // Act
            var result = await _zodiacService.AreCompatibleAsync(sign1, sign2);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ClearCache_ShouldClearCachedData()
        {
            // Arrange
            var signs = _zodiacService.GetAllZodiacSignsAsync().Result;
            Assert.IsNotNull(signs);

            // Act
            _zodiacService.ClearCache();
            var signsAfterClear = _zodiacService.GetAllZodiacSignsAsync().Result;

            // Assert
            Assert.IsNotNull(signsAfterClear);
            // Should still work after cache clear
            Assert.AreEqual(12, signsAfterClear.Count);
        }

        [TestMethod]
        public async Task UpdateDailyHoroscopeAsync_ShouldUpdateCache()
        {
            // Arrange
            var horoscope = new DailyHoroscope
            {
                Id = 1,
                ZodiacSignId = 1,
                Date = DateTime.Today,
                Content = "Test horoscope content",
                LoveScore = 85,
                CareerScore = 90,
                HealthScore = 80,
                MoneyScore = 75,
                Mood = "Happy",
                LuckyColor = "Red",
                LuckyNumber = "7"
            };

            // Act
            var result = await _zodiacService.UpdateDailyHoroscopeAsync(horoscope);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(horoscope.Content, result.Content);
            Assert.AreEqual(horoscope.LoveScore, result.LoveScore);
        }

        [TestMethod]
        public async Task AllZodiacSigns_ShouldHaveRequiredProperties()
        {
            // Act
            var signs = await _zodiacService.GetAllZodiacSignsAsync();

            // Assert
            foreach (var sign in signs)
            {
                Assert.IsFalse(string.IsNullOrEmpty(sign.Name), $"Sign {sign.Id} missing Name");
                Assert.IsFalse(string.IsNullOrEmpty(sign.EnglishName), $"Sign {sign.Id} missing EnglishName");
                Assert.IsFalse(string.IsNullOrEmpty(sign.Symbol), $"Sign {sign.Id} missing Symbol");
                Assert.IsFalse(string.IsNullOrEmpty(sign.DateRange), $"Sign {sign.Id} missing DateRange");
                Assert.IsFalse(string.IsNullOrEmpty(sign.Description), $"Sign {sign.Id} missing Description");
                Assert.IsTrue(sign.Strengths.Count > 0, $"Sign {sign.Id} has no Strengths");
                Assert.IsTrue(sign.Weaknesses.Count > 0, $"Sign {sign.Id} has no Weaknesses");
                Assert.IsTrue(sign.Compatible.Count > 0, $"Sign {sign.Id} has no Compatible signs");
            }
        }
    }
}
