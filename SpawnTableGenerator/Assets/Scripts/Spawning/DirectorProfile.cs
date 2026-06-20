using UnityEngine;

namespace SpawnSystem.Spawning
{
    /// <summary>
    /// 스폰 디렉터 튜닝(설계 §2, §6). 긴장도 가중치 + 스폰 페이싱 + 예산 + 동시 상한.
    /// </summary>
    [CreateAssetMenu(fileName = "DirectorProfile", menuName = "SpawnSystem/Director Profile")]
    public class DirectorProfile : ScriptableObject
    {
        [Header("긴장도 (§6)")]
        public float wTime = 0.5f;        // 경과시간 가중치
        public float wObjective = 0.5f;   // 목표 진행 가중치
        public float maxTime = 600f;      // 긴장도 100%에 도달하는 기준 시간(초)

        [Header("스폰 페이싱 (§6)")]
        public float maxSpawnInterval = 12f; // 긴장도 0일 때
        public float minSpawnInterval = 2f;  // 긴장도 1일 때 (무한 스폰 방지 하한)

        [Header("예산 (난이도 스케일링, §2)")]
        public float startingBudget = 5f;
        public float budgetPerSecond = 1f;   // 회복률
        public float maxBudget = 40f;

        [Header("상한")]
        [Min(1)] public int maxConcurrentPacks = 12;
    }
}
