using System.Collections.Generic;
using UnityEngine;

namespace SpawnSystem.Spawning
{
    /// <summary>
    /// 예산 기반 가중 선택 순수 로직(설계 §2 BudgetFill). 예산이 닿는 한, 난이도 자격이 되는 항목을
    /// weight 로 가중 선택하며 cost 만큼 예산을 소비한다. rng 주입으로 결정적 테스트 가능.
    /// </summary>
    public static class SpawnSelector
    {
        public static List<int> SelectWithinBudget(IReadOnlyList<SpawnEntry> entries, float budget, float difficulty, System.Func<float> rng01, int maxPicks = 64)
        {
            var result = new List<int>();
            if (entries == null || entries.Count == 0)
                return result;

            float remaining = budget;
            for (int iter = 0; iter < maxPicks; iter++)
            {
                // 자격(예산 내 + 난이도 충족 + 가중치 > 0) 항목의 가중치 합.
                float totalW = 0f;
                for (int i = 0; i < entries.Count; i++)
                    if (IsEligible(entries[i], remaining, difficulty))
                        totalW += entries[i].weight;

                if (totalW <= 0f)
                    break;

                float r = Mathf.Clamp01(rng01 != null ? rng01() : 0f) * totalW;
                int picked = -1;
                float cum = 0f;
                for (int i = 0; i < entries.Count; i++)
                {
                    if (!IsEligible(entries[i], remaining, difficulty))
                        continue;
                    cum += entries[i].weight;
                    if (r <= cum)
                    {
                        picked = i;
                        break;
                    }
                }
                if (picked < 0)
                    break;

                result.Add(picked);
                remaining -= entries[picked].cost;
            }
            return result;
        }

        static bool IsEligible(SpawnEntry e, float remainingBudget, float difficulty)
        {
            return e != null && e.weight > 0f && e.cost <= remainingBudget && e.minDifficulty <= difficulty;
        }
    }
}
