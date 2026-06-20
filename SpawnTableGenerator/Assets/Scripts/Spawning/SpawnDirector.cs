using System.Collections.Generic;
using SpawnSystem.Monsters;
using UnityEngine;
using UnityEngine.AI;

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

        [Header("배치 / 회수")]
        [Tooltip("순찰 군집이 플레이어로부터 이 거리보다 멀어지면 풀로 회수(→ 예측 위치로 재스폰)")]
        public float despawnDistance = 45f;
        [Tooltip("플레이어 예측 위치 선행 시간(초) — 자주 만나게 하기 위함")]
        public float spawnLeadTime = 1.5f;
        [Tooltip("예측 위치 주변에 스폰할 거리(안 보이는 곳 근사)")]
        public float spawnAroundRadius = 18f;

        [Header("맵 경계")]
        [Tooltip("맵 중심(0,0,0) 기준 스폰 가능 반경. 0이면 제한 없음.")]
        public float mapHalfSize = 42f;

        [Header("외부 변조")]
        [Tooltip("스폰 간격 배율(낮=1, 밤<1). Environment.SpawnRateResponder가 낮/밤에 맞춰 설정.")]
        public float spawnIntervalScale = 1f;

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
            RecycleDistantPatrols();

            _spawnTimer -= dt;
            if (_spawnTimer > 0f) return;

            Intensity = TensionCalculator.Intensity(_elapsed, profile.maxTime, objectivesRemaining, objectivesTotal, profile.wTime, profile.wObjective);
            _spawnTimer = TensionCalculator.SpawnInterval(Intensity, profile.maxSpawnInterval, profile.minSpawnInterval)
                          * Mathf.Max(0.1f, spawnIntervalScale);
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
                Vector3 pos = SpawnPos();

                var pack = PackFactory.BuildFromDef(entry.monster, count, pos, player, transform, _pool);
                _activePacks.Add(pack);
                _budget -= entry.cost;
            }
        }

        /// <summary>플레이어 예측 위치 주변(안 보이는 곳 근사)으로 스폰 위치 결정. 맵 경계 클램핑 포함.</summary>
        Vector3 SpawnPos()
        {
            Vector3 pos;
            if (spawnOrigin != null)
            {
                pos = spawnOrigin.position;
            }
            else if (player == null)
            {
                pos = transform.position;
            }
            else
            {
                Vector3 predicted = SpawnPlacement.Predict(player.position, PlayerVelocity(), spawnLeadTime);
                Vector2 ring = Random.insideUnitCircle.normalized * spawnAroundRadius;
                pos = predicted + new Vector3(ring.x, 0f, ring.y);
            }

            // 맵 경계 클램핑 (NavMesh 밖 스폰 방지)
            if (mapHalfSize > 0f)
            {
                float limit = mapHalfSize - 2f; // 벽 두께 여유
                pos.x = Mathf.Clamp(pos.x, -limit, limit);
                pos.z = Mathf.Clamp(pos.z, -limit, limit);
            }

            return pos;
        }

        Vector3 PlayerVelocity()
        {
            var ag = player != null ? player.GetComponent<NavMeshAgent>() : null;
            return ag != null ? ag.velocity : Vector3.zero;
        }

        /// <summary>플레이어로부터 너무 멀어진 '순찰' 군집을 풀로 회수(교전 중인 군집은 유지).</summary>
        void RecycleDistantPatrols()
        {
            if (player == null) return;
            Vector3 pp = player.position;
            for (int i = _activePacks.Count - 1; i >= 0; i--)
            {
                var pack = _activePacks[i];
                if (pack == null) { _activePacks.RemoveAt(i); continue; }
                if (pack.State == PackState.Patrol && Flat(pack.AnchorPosition - pp).magnitude > despawnDistance)
                {
                    pack.Despawn(go => _pool.Release(go));
                    _activePacks.RemoveAt(i);
                }
            }
        }

        static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }

        void PrunePacks()
        {
            for (int i = _activePacks.Count - 1; i >= 0; i--)
                if (_activePacks[i] == null)
                    _activePacks.RemoveAt(i);
        }
    }
}
