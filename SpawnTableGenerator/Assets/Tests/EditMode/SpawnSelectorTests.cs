using System.Collections.Generic;
using NUnit.Framework;
using SpawnSystem.Spawning;

namespace SpawnSystem.Tests
{
    /// <summary>예산 기반 가중 선택(<see cref="SpawnSelector"/>) EditMode 테스트. 설계 §2 BudgetFill.</summary>
    public class SpawnSelectorTests
    {
        static SpawnEntry E(float cost, float weight, float minDiff = 0f)
            => new SpawnEntry { cost = cost, weight = weight, minDifficulty = minDiff };

        static System.Func<float> Rng(float v) => () => v;

        [Test]
        public void NoBudget_NoPicks()
        {
            var entries = new List<SpawnEntry> { E(3, 1) };
            Assert.AreEqual(0, SpawnSelector.SelectWithinBudget(entries, 0f, 0f, Rng(0f)).Count);
        }

        [Test]
        public void PicksUntilBudgetExhausted()
        {
            var entries = new List<SpawnEntry> { E(3, 1) };
            // 예산 10 / cost 3 → 3회(9 소비), 잔여 1 < 3 → 정지
            Assert.AreEqual(3, SpawnSelector.SelectWithinBudget(entries, 10f, 0f, Rng(0f)).Count);
        }

        [Test]
        public void TotalSpent_WithinBudget()
        {
            var entries = new List<SpawnEntry> { E(3, 1), E(8, 1) };
            var picks = SpawnSelector.SelectWithinBudget(entries, 10f, 0f, Rng(0.99f));
            float spent = 0f;
            foreach (var i in picks) spent += entries[i].cost;
            Assert.LessOrEqual(spent, 10f);
        }

        [Test]
        public void MinDifficulty_FiltersByDifficulty()
        {
            var entries = new List<SpawnEntry> { E(2, 1, minDiff: 5f) };
            Assert.AreEqual(0, SpawnSelector.SelectWithinBudget(entries, 10f, 1f, Rng(0f)).Count, "난이도 미달 → 제외");
            Assert.Greater(SpawnSelector.SelectWithinBudget(entries, 10f, 6f, Rng(0f)).Count, 0, "난이도 충족 → 등장");
        }

        [Test]
        public void Weighted_Rng0_PicksFirstEligible()
        {
            var entries = new List<SpawnEntry> { E(3, 1), E(3, 1) };
            var picks = SpawnSelector.SelectWithinBudget(entries, 3f, 0f, Rng(0f));
            Assert.AreEqual(1, picks.Count);
            Assert.AreEqual(0, picks[0]);
        }

        [Test]
        public void AllPicks_AreEligible()
        {
            var entries = new List<SpawnEntry> { E(2, 1), E(5, 1, minDiff: 3f) };
            var picks = SpawnSelector.SelectWithinBudget(entries, 6f, 1f, Rng(0f)); // diff 1 → entry0만 자격
            foreach (var i in picks)
                Assert.AreEqual(0, i, "난이도 1에선 entry0만 선택돼야");
        }
    }
}
