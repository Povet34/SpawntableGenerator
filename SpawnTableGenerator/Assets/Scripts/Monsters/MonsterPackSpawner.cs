using UnityEngine;
using UnityEngine.AI;

namespace SpawnSystem.Monsters
{
    /// <summary>
    /// 테스트/데모용 군집 스포너. Play 시 보이지 않는 앵커(NavMeshAgent) + 가시 몬스터 N마리로 이루어진
    /// 한 군집을 만들고, 앵커가 target(기본 Player)을 향해 길찾기 → 멤버들이 boids 로 추종한다.
    /// (로드맵 2번의 "군집 이동을 눈으로 확인"용. 정찰/FSM 은 로드맵 3번에서 대체됨.)
    /// 생성물은 모두 이 스포너의 자식으로 들어가 정리가 쉽다.
    /// </summary>
    public class MonsterPackSpawner : MonoBehaviour
    {
        [Header("스폰")]
        [Min(1)] public int memberCount = 8;
        [Tooltip("스폰 중심(월드). 비활성 시 이 오브젝트 위치 기준)")]
        public Vector3 spawnCenter = new Vector3(0f, 0f, 25f);
        [Min(0.5f)] public float spawnRadius = 3f;
        public bool spawnOnStart = true;

        [Header("몬스터 정의 (선택 — 설정 시 크기/색/속도를 이 def 에서 가져옴)")]
        public MonsterDef monsterDef;

        [Header("몬스터 외형(프리미티브)")]
        public float memberDiameter = 1f;
        public Color memberColor = new Color(0.85f, 0.2f, 0.2f);

        [Header("이동")]
        [Tooltip("앵커가 따라갈 대상. 비우면 'Player' 태그를 자동 탐색")]
        public Transform target;
        [Min(0.05f)] public float repathInterval = 0.4f;
        public float anchorSpeed = 3.5f;

        public BoidsSettings settings = BoidsSettings.Default;

        MonsterPack _pack;
        float _repathTimer;
        Material _memberMat;

        void Reset()
        {
            settings = BoidsSettings.Default;
        }

        void Start()
        {
            if (target == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null)
                    target = p.transform;
            }
            if (spawnOnStart)
                Spawn();
        }

        /// <summary>군집(앵커+멤버)을 즉시 생성하고 반환. 모든 생성물은 스포너의 자식.</summary>
        public MonsterPack Spawn()
        {
            if (monsterDef != null)
                ApplyDef(monsterDef);

            Vector3 center = SnapToNavMesh(spawnCenter, 12f);

            // 보이지 않는 가상 앵커 + 길찾기 에이전트
            var anchorGo = new GameObject("PackAnchor");
            anchorGo.transform.SetParent(transform, false);
            anchorGo.transform.position = center;
            var agent = anchorGo.AddComponent<NavMeshAgent>();
            agent.radius = 0.5f;
            agent.height = 2f;
            agent.speed = anchorSpeed;
            agent.acceleration = 30f;
            agent.angularSpeed = 720f;
            agent.stoppingDistance = 1f;

            // 군집
            var packGo = new GameObject("MonsterPack");
            packGo.transform.SetParent(transform, false);
            _pack = packGo.AddComponent<MonsterPack>();
            _pack.anchor = anchorGo.transform;
            _pack.anchorAgent = agent;
            _pack.player = target;
            _pack.profile = monsterDef != null ? monsterDef.movement : null;
            _pack.memberMoveSpeed = settings.MaxSpeed;
            if (monsterDef != null)
                _pack.preferredRange = monsterDef.preferredRange;

            EnsureMaterial();
            for (int i = 0; i < memberCount; i++)
            {
                var member = CreateMemberVisual(packGo.transform, center, i);
                var m = member.AddComponent<Monster>();

                // 고정밀 이동: NavMeshAgent.Move 로 navmesh 구속(벽 통과 차단) + 회피(RVO).
                // 길찾기(SetDestination)는 호출하지 않음 — 군집당 1 길찾기(앵커)만 유지.
                var memberAgent = member.AddComponent<NavMeshAgent>();
                memberAgent.radius = memberDiameter * 0.5f;
                memberAgent.height = memberDiameter * 2f;
                memberAgent.baseOffset = memberDiameter;
                memberAgent.speed = settings.MaxSpeed;
                memberAgent.acceleration = 9999f;
                memberAgent.angularSpeed = 0f;
                memberAgent.updateRotation = false;
                memberAgent.autoBraking = false;
                // 플레이어(기본 50)보다 '덜 중요'하게 두어, 무리가 플레이어를 피하되 밀어내지는 못하게 함.
                memberAgent.avoidancePriority = 60;
                m.Mover = new AgentMover(memberAgent);

                _pack.RegisterMember(m);
            }
            return _pack;
        }

        GameObject CreateMemberVisual(Transform parent, Vector3 center, int index)
        {
            // 링 위에 고르게 분산해 스폰(겹침 최소화).
            float ang = index / Mathf.Max(1f, memberCount) * Mathf.PI * 2f;
            Vector3 offset = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * spawnRadius;
            Vector3 pos = SnapToNavMesh(center + offset, 4f);
            pos.y = memberDiameter; // 캡슐 중심을 바닥 위로

            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = $"Monster_{index:00}";
            go.transform.SetParent(parent, true);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(memberDiameter, memberDiameter, memberDiameter);

            var r = go.GetComponent<Renderer>();
            if (r != null)
                r.sharedMaterial = _memberMat;
            return go;
        }

        void Update()
        {
            if (_pack == null || target == null)
                return;
            _repathTimer -= Time.deltaTime;
            if (_repathTimer <= 0f)
            {
                _pack.MoveTo(target.position);
                _repathTimer = repathInterval;
            }
        }

        static Vector3 SnapToNavMesh(Vector3 p, float radius)
        {
            return NavMesh.SamplePosition(p, out var hit, radius, NavMesh.AllAreas) ? hit.position : p;
        }

        void ApplyDef(MonsterDef def)
        {
            memberDiameter = def.scale;
            memberColor = def.color;
            settings.MaxSpeed = def.moveSpeed;
            anchorSpeed = def.moveSpeed;
            _memberMat = null; // 색 갱신 위해 머티리얼 재생성 유도
        }

        void EnsureMaterial()
        {
            if (_memberMat != null)
                return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            _memberMat = new Material(shader) { name = "MonsterMat", color = memberColor };
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.6f);
            Gizmos.DrawWireSphere(spawnCenter, spawnRadius);
        }
    }
}
