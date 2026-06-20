using NUnit.Framework;
using SpawnSystem.Spawning;

namespace SpawnSystem.Tests
{
    /// <summary>긴장도/페이싱 순수 로직(<see cref="TensionCalculator"/>) EditMode 테스트. 설계 §6.</summary>
    public class TensionCalculatorTests
    {
        const float Eps = 1e-4f;

        [Test]
        public void Intensity_AtStart_IsZero()
        {
            Assert.AreEqual(0f, TensionCalculator.Intensity(0f, 600f, 3, 3, 0.5f, 0.5f), Eps);
        }

        [Test]
        public void Intensity_TimeRaisesIt()
        {
            float a = TensionCalculator.Intensity(0f, 600f, 3, 3, 0.5f, 0.5f);
            float b = TensionCalculator.Intensity(300f, 600f, 3, 3, 0.5f, 0.5f);
            Assert.Greater(b, a);
        }

        [Test]
        public void Intensity_ObjectivesCompletedRaiseIt()
        {
            float a = TensionCalculator.Intensity(0f, 600f, 3, 3, 0.5f, 0.5f);
            float b = TensionCalculator.Intensity(0f, 600f, 1, 3, 0.5f, 0.5f); // 2개 처리됨
            Assert.Greater(b, a);
        }

        [Test]
        public void Intensity_AtEnd_IsOne()
        {
            Assert.AreEqual(1f, TensionCalculator.Intensity(600f, 600f, 0, 3, 0.5f, 0.5f), Eps);
        }

        [Test]
        public void Intensity_Clamped01()
        {
            float v = TensionCalculator.Intensity(9999f, 600f, 0, 3, 2f, 2f);
            Assert.LessOrEqual(v, 1f);
            Assert.GreaterOrEqual(v, 0f);
        }

        [Test]
        public void SpawnInterval_AtZero_IsMax()
        {
            Assert.AreEqual(12f, TensionCalculator.SpawnInterval(0f, 12f, 2f), Eps);
        }

        [Test]
        public void SpawnInterval_AtOne_IsMin()
        {
            Assert.AreEqual(2f, TensionCalculator.SpawnInterval(1f, 12f, 2f), Eps);
        }

        [Test]
        public void SpawnInterval_DecreasesWithIntensity()
        {
            Assert.Greater(
                TensionCalculator.SpawnInterval(0.2f, 12f, 2f),
                TensionCalculator.SpawnInterval(0.8f, 12f, 2f));
        }
    }
}
