using System.Collections.Generic;
using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Spawning
{
    /// <summary>
    /// 연속 디렉터(설계 §3, §6). 긴장도(경과시간+목표진행)로 스폰 간격을 정하고, 회복되는 예산을
    /// 스폰 테이블에서 가중·예산 선택해 군집을 스폰한다. 동시 군집 상한을 지킨다.
    /// 길찾기/이동/회피는 각 군집(앵커+멤버)이 담당.
    /// </summary>
    public class SpawnDirector : MonoBehaviour
    {
        public DirectorProfile profile;
        public SpawnTable table;
        [Tooltip("시야/선호 거리 기준 대상(보통 Player)")]
        public Transform player;
        [Tooltip("군집이 생성될 위치. 비우면 이 오브젝트 위치(추후 '안 보이는 곳' 알고리즘 연결)")]
        public Transform spawnOrigin;

        [Header("목표 (긴장도용)")]
        [Min(0)] public int objectivesTotal = 3;
        [Min(0)] public int objectivesRemaining = 3;

        float _elapsed;
        float _budget;
        float _spawnTimer;
        MonsterPool _pool;
        readonly List<MonsterPack> _activePacks = new List<MonsterPack>();

        public float Budget => _budget;
        public float Intensity { get; private set; }
        public IReadOnlyList<MonsterPack> ActivePacks => _activePacks;
        public int PoolCreatedCount => _pool != null ? _pool.CreatedCount : 0;

        void Start()
        {
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }
            if (profile != null) _budget = profile.startingBudget;
            _spawnTimer = 0f;

            var poolRoot = new GameObject("MonsterPool").transform;
            poolRoot.SetParent(transform, false);
            _pool = new MonsterPool(poolRoot);
        }

        void Update()
        {
            if (profile == null || table == null) return;

            float dt = Time.deltaTime;
            _elapsed += dt;
            _budget = Mathf.Min(profile.maxBudget, _budget + profile.budgetPerSecond * dt);
            PrunePacks();

            _spawnTimer -= dt;
            if (_spawnTimer > 0f) return;

            Intensity = TensionCalculator.Intensity(_elapsed, profile.maxTime, objectivesRemaining, objectivesTotal, profile.wTime, profile.wObjective);
            _spawnTimer = TensionCalculator.SpawnInterval(Intensity, profile.maxSpawnInterval, profile.minSpawnInterval);
            TrySpawnWave(Intensity);
        }

        void TrySpawnWave(float difficulty)
        {
            if (_activePacks.Count >= profile.maxConcurrentPacks)
                return;

            var picks = SpawnSelector.SelectWithinBudget(table.entries, _budget, difficulty, () => Random.value);
            for (int k = 0; k < picks.Count; k++)
            {
                if (_activePacks.Count >= profile.maxConcurrentPacks)
                    break;

                var entry = table.entries[picks[k]];
                if (entry == null || entry.monster == null)
                    continue;

                int count = Mathf.Max(1, Random.Range(entry.groupSize.x, entry.groupSize.y + 1));
                Vector3 pos = spawnOrigin != null ? spawnOrigin.position : transform.position;

                var pack = PackFactory.BuildFromDef(entry.monster, count, pos, player, transform, _pool);
                _activePacks.Add(pack);
                _budget -= entry.cost;
            }
        }

        void PrunePacks()
        {
            for (int i = _activePacks.Count - 1; i >= 0; i--)
                if (_activePacks[i] == null)
                    _activePacks.RemoveAt(i);
        }
    }
}
