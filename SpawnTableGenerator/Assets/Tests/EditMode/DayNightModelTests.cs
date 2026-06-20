using NUnit.Framework;
using SpawnSystem.Environment;

namespace SpawnSystem.Tests
{
    /// <summary>낮/밤 순수 로직(<see cref="DayNightModel"/>) EditMode 테스트.</summary>
    public class DayNightModelTests
    {
        const float Eps = 1e-3f;

        [Test]
        public void Daylight_ZeroAtMidnight_OneAtNoon()
        {
            Assert.AreEqual(0f, DayNightModel.Daylight01(0f), Eps);
            Assert.AreEqual(1f, DayNightModel.Daylight01(0.5f), Eps);
            Assert.AreEqual(0f, DayNightModel.Daylight01(1f), Eps);
        }

        [Test]
        public void Daylight_HalfAtDawnAndDusk()
        {
            Assert.AreEqual(0.5f, DayNightModel.Daylight01(0.25f), Eps);
            Assert.AreEqual(0.5f, DayNightModel.Daylight01(0.75f), Eps);
        }

        [Test]
        public void Daylight_MonotonicRisingBeforeNoon()
        {
            float prev = -1f;
            for (float t = 0f; t <= 0.5f + 1e-4f; t += 0.05f)
            {
                float d = DayNightModel.Daylight01(t);
                Assert.GreaterOrEqual(d, prev - Eps, $"daylight should rise to noon (t={t})");
                prev = d;
            }
        }

        [Test]
        public void NormalizedFromElapsed_WrapsOverCycle()
        {
            Assert.AreEqual(0.3f, DayNightModel.NormalizedFromElapsed(0f, 300f, 0.3f), Eps);
            // 한 사이클 경과 → 시작 시각으로 복귀
            Assert.AreEqual(0.3f, DayNightModel.NormalizedFromElapsed(300f, 300f, 0.3f), Eps);
            // 반 사이클 → +0.5
            Assert.AreEqual(0.8f, DayNightModel.NormalizedFromElapsed(150f, 300f, 0.3f), Eps);
        }

        [Test]
        public void PhaseOf_BoundariesMatchSunriseSunset()
        {
            Assert.AreEqual(DayNightPhase.Night, DayNightModel.PhaseOf(0.0f));
            Assert.AreEqual(DayNightPhase.Dawn, DayNightModel.PhaseOf(0.25f));
            Assert.AreEqual(DayNightPhase.Day, DayNightModel.PhaseOf(0.5f));
            Assert.AreEqual(DayNightPhase.Dusk, DayNightModel.PhaseOf(0.75f));
            Assert.AreEqual(DayNightPhase.Night, DayNightModel.PhaseOf(0.95f));
        }

        [Test]
        public void Evaluate_DayValuesAtNoon()
        {
            var cfg = new DayNightConfig();
            var s = DayNightModel.Evaluate(0.5f, cfg);
            Assert.AreEqual(cfg.daySunIntensity, s.SunIntensity, Eps);
            Assert.AreEqual(cfg.dayViewRadius, s.ViewRadius, Eps);
            Assert.AreEqual(cfg.daySpawnIntervalScale, s.SpawnIntervalScale, Eps);
            Assert.AreEqual(DayNightPhase.Day, s.Phase);
        }

        [Test]
        public void Evaluate_NightValuesAtMidnight()
        {
            var cfg = new DayNightConfig();
            var s = DayNightModel.Evaluate(0f, cfg);
            Assert.AreEqual(cfg.nightSunIntensity, s.SunIntensity, Eps);
            Assert.AreEqual(cfg.nightViewRadius, s.ViewRadius, Eps);
            Assert.AreEqual(cfg.nightSpawnIntervalScale, s.SpawnIntervalScale, Eps);
        }

        [Test]
        public void SunRotation_PointsStraightDownAtNoon()
        {
            // 정오: 빛이 수직으로 내리꽂힘 → forward ≈ (0,-1,0).
            var rot = DayNightModel.SunRotation(0.5f, 0f);
            UnityEngine.Vector3 fwd = rot * UnityEngine.Vector3.forward;
            Assert.AreEqual(-1f, fwd.y, Eps, "noon sun should shine straight down");
        }

        [Test]
        public void SunRotation_HorizontalAtSunriseAndSunset()
        {
            // 일출/일몰: 빛이 거의 수평 → forward.y ≈ 0.
            float sunriseY = (DayNightModel.SunRotation(0.25f, 0f) * UnityEngine.Vector3.forward).y;
            float sunsetY = (DayNightModel.SunRotation(0.75f, 0f) * UnityEngine.Vector3.forward).y;
            Assert.AreEqual(0f, sunriseY, Eps, "sunrise sun should be near horizon");
            Assert.AreEqual(0f, sunsetY, Eps, "sunset sun should be near horizon");
        }

        [Test]
        public void SunRotation_BelowHorizonAtMidnight()
        {
            // 자정: 해가 지면 아래 → 빛이 위를 향함 → forward.y > 0.
            float y = (DayNightModel.SunRotation(0f, 0f) * UnityEngine.Vector3.forward).y;
            Assert.Greater(y, 0.5f, "midnight sun should be below the horizon");
        }

        [Test]
        public void Evaluate_NightIsDarkerAndSpawnsFasterThanDay()
        {
            var cfg = new DayNightConfig();
            var night = DayNightModel.Evaluate(0f, cfg);
            var day = DayNightModel.Evaluate(0.5f, cfg);

            Assert.Less(night.SunIntensity, day.SunIntensity);
            Assert.Less(night.ViewRadius, day.ViewRadius);
            Assert.Less(night.SpawnIntervalScale, day.SpawnIntervalScale); // 밤=간격 짧음
            Assert.Greater(night.Darkness01, day.Darkness01);
        }
    }
}
