using UnityEngine;

namespace SpawnSystem.Spawning
{
    /// <summary>
    /// 스폰 테이블(설계 §2). 항목 묶음 + 선택 모드. 여러 개 만들어 상황/난이도/테마별로 사용.
    /// </summary>
    [CreateAssetMenu(fileName = "SpawnTable", menuName = "SpawnSystem/Spawn Table")]
    public class SpawnTable : ScriptableObject
    {
        public enum SelectionMode
        {
            WeightedRandom, // 가중치로 1개씩
            BudgetFill,     // 예산이 닿는 한 가중치로 채움
            Sequential,     // 순서대로
        }

        public SelectionMode mode = SelectionMode.BudgetFill;
        public SpawnEntry[] entries = new SpawnEntry[0];
    }
}
