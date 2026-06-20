using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SpawnSystem.Monsters
{
    /// <summary>
    /// 군집(Pack) — 가상 앵커 소유(설계 §4). 길찾기는 앵커(NavMeshAgent)만 수행(군집당 1경로, 글로벌
    /// 우회 + area cost). 멤버는 앵커를 따라온 뒤 근접 교전권에서 컨텍스트 스티어링(§12)으로 플레이어
    /// 시야를 피하며 선호 거리 유지 → 포위는 창발. 멤버 스텝을 한곳(Update)에서 순서대로 돌린다.
    /// </summary>
    public class MonsterPack : MonoBehaviour
    {
        [Tooltip("멤버가 모일 가상 앵커. 비우면 이 Transform 사용.")]
        public Transform anchor;

        [Tooltip("앵커의 길찾기 에이전트(선택). 있으면 MoveTo 로 군집을 이동.")]
        public NavMeshAgent anchorAgent;

        [Tooltip("시야/선호 거리의 기준이 되는 대상(보통 Player). 비우면 앵커를 기준으로.")]
        public Transform player;

        [Tooltip("멤버 행동 프로필. 비우면 기본값 사용.")]
        public MovementProfile profile;

        [Header("멤버 이동")]
        public float memberMoveSpeed = 4f;
        [Tooltip("선호 교전 거리(min,max)")]
        public Vector2 preferredRange = new Vector2(1.5f, 4f);
        [Tooltip("이 거리 안이면 컨텍스트 스티어링, 밖이면 앵커로 직진")]
        public float engageRange = 12f;
        [Range(4, 24)] public int dirCount = 12;

        [Header("인지 (FSM, §5)")]
        [Tooltip("끄면 항상 교전(시야 무시) — 이동 단위 테스트용")]
        public bool useFsm = true;
        [Tooltip("멤버 시야 콘 각도(도)")]
        public float sightConeAngle = 100f;
        [Tooltip("멤버 시야 사거리")]
        public float sightRange = 14f;
        [Tooltip("이 거리 안이면 방향 무관 발각(근거리 직접 시야)")]
        public float closeSightRange = 4f;
        public float investigateTimeout = 5f;
        public float loseSightTime = 3f;

        public PackState State { get; private set; } = PackState.Patrol;

        Vector3 _knownPlayerPos;
        Vector3 _lastStimulus;
        Vector3 _lastPlayerAttackDir;
        float _lastPlayerAttackAge = 999f;
        Vector3 _patrolTarget;
        Vector3 _patrolDir;
        bool _patrolInit;
        float _timeInState;
        float _timeSinceSight = 999f;
        float _anchorRepath;
        bool _pendingNoise;
        Vector3 _noisePos;

        public List<Monster> members = new List<Monster>();

        /// <summary>사망한 멤버를 어떻게 회수할지(보통 풀 반환). 없으면 파괴.</summary>
        [System.NonSerialized] public System.Action<GameObject> MemberReleaser;

        readonly List<Vector3> _positions = new List<Vector3>();
        readonly List<Monster> _active = new List<Monster>();
        bool _hadMembers;

        static MovementProfile _defaultProfile;

        public Vector3 AnchorPosition => anchor != null ? anchor.position : transform.position;

        public void RegisterMember(Monster m)
        {
            if (m == null || members.Contains(m))
                return;
            members.Add(m);
            m.Pack = this;
            _hadMembers = true;
        }

        /// <summary>군집 전체를 목표 지점으로 이동(앵커 길찾기). 에이전트 없으면 무시.</summary>
        public void MoveTo(Vector3 worldPos)
        {
            if (anchorAgent != null && anchorAgent.isOnNavMesh)
                anchorAgent.SetDestination(worldPos);
        }

        /// <summary>플레이어 공격 방향 갱신 — ViewPressure 의 lastAttackDir 에 반영.</summary>
        public void NotifyPlayerAttack(Vector3 dir)
        {
            _lastPlayerAttackDir = dir;
            _lastPlayerAttackAge = 0f;
        }

        /// <summary>소음 자극 입력(플레이어 고속 이동/공격 등). 다음 인지 틱에서 경계 트리거.</summary>
        public void HearNoise(Vector3 worldPos)
        {
            _pendingNoise = true;
            _noisePos = worldPos;
        }

        void Update()
        {
            if (!PruneDead()) return; // 전멸 → 디스폰됨
            float dt = Time.deltaTime;
            if (useFsm)
            {
                UpdatePerception(dt);
                DriveAnchor(dt);
            }
            StepMembers(dt);
        }

        /// <summary>사망/소멸한 멤버를 회수(풀 반환 또는 파괴). 전멸하면 군집을 디스폰하고 false 반환.</summary>
        bool PruneDead()
        {
            for (int i = members.Count - 1; i >= 0; i--)
            {
                var m = members[i];
                bool dead = m == null || (m.Health != null && m.Health.IsDead);
                if (!dead) continue;

                if (m != null)
                {
                    m.Pack = null;
                    if (MemberReleaser != null) MemberReleaser(m.gameObject);
                    else DestroySafe(m.gameObject);
                }
                members.RemoveAt(i);
            }

            if (_hadMembers && members.Count == 0)
            {
                if (anchor != null) DestroySafe(anchor.gameObject);
                DestroySafe(gameObject);
                return false;
            }
            return true;
        }

        void UpdatePerception(float dt)
        {
            _timeInState += dt;
            _timeSinceSight += dt;

            bool sight = SenseSight();
            if (sight)
            {
                _knownPlayerPos = player.position;
                _timeSinceSight = 0f;
            }

            var senses = new PackSenses
            {
                SightContact = sight,
                NoiseHeard = _pendingNoise,
                TimeInState = _timeInState,
                TimeSinceSight = _timeSinceSight,
            };
            var perception = new PackPerception { investigateTimeout = investigateTimeout, loseSightTime = loseSightTime };
            var next = PackFsm.Next(State, senses, perception);

            if (next != State)
            {
                if (next == PackState.Alert)               // 조사 지점: 소음원 또는 마지막 목격 위치
                    _lastStimulus = _pendingNoise ? _noisePos : _knownPlayerPos;
                State = next;
                _timeInState = 0f;
            }
            _pendingNoise = false;
        }

        /// <summary>한 멤버라도 플레이어를 보면 전원 발각(공유 상태). 시야 = 콘+사거리+LoS, 근거리는 방향 무관.</summary>
        bool SenseSight()
        {
            if (player == null) return false;
            Vector3 pp = player.position;
            for (int i = 0; i < members.Count; i++)
            {
                var m = members[i];
                if (m == null) continue;
                Vector3 mp = m.transform.position;
                float d = Vector3.Distance(Flat(mp), Flat(pp));
                if (d > sightRange) continue;

                bool inCone = ViewPressure.Cone(mp, m.transform.forward, pp, sightConeAngle, sightRange) > 0f;
                bool close = d <= closeSightRange;
                if (!(inCone || close)) continue;

                if (!NavMesh.Raycast(mp, pp, out _, NavMesh.AllAreas)) // 벽에 안 막히면 보임
                    return true;
            }
            return false;
        }

        void DriveAnchor(float dt)
        {
            if (!_patrolInit)
            {
                _patrolDir = player != null ? Flat(player.position - AnchorPosition) : Flat(transform.forward);
                if (_patrolDir.sqrMagnitude < 1e-4f) _patrolDir = Vector3.forward;
                _patrolDir.Normalize();
                _patrolTarget = NextPatrolPoint();
                _patrolInit = true;
            }

            _anchorRepath -= dt;
            if (_anchorRepath > 0f) return;
            _anchorRepath = 0.5f;

            Vector3 target;
            switch (State)
            {
                case PackState.Engage:
                    target = _knownPlayerPos;
                    break;
                case PackState.Alert:
                    target = _lastStimulus;
                    break;
                default: // 순찰 — 목표에 닿으면 계속 전진(가만히 안 있음). 플레이어 멀면 단순(앵커만 길찾기).
                    if (Flat(AnchorPosition - _patrolTarget).magnitude < 3f)
                        _patrolTarget = NextPatrolPoint();
                    target = _patrolTarget;
                    break;
            }
            MoveTo(target);
        }

        /// <summary>스폰 방향(+약간의 흔들림)으로 계속 나아갈 다음 순찰 지점.</summary>
        Vector3 NextPatrolPoint()
        {
            Vector2 jit = Random.insideUnitCircle * 0.4f;
            Vector3 dir = _patrolDir + new Vector3(jit.x, 0f, jit.y);
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-4f) dir = Vector3.forward;
            return SnapNav(AnchorPosition + dir.normalized * 12f);
        }

        static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }

        static Vector3 SnapNav(Vector3 p)
            => NavMesh.SamplePosition(p, out var hit, 6f, NavMesh.AllAreas) ? hit.position : p;

        public void StepMembers(float dt)
        {
            _positions.Clear();
            _active.Clear();
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i] == null)
                    continue;
                _active.Add(members[i]);
                _positions.Add(members[i].transform.position);
            }
            if (_active.Count == 0)
                return;

            // 교전(또는 FSM 비활성)이면 플레이어를 쫓고, 순찰/경계면 앵커에 응집(플레이어 모름 → 시야압박 없음).
            bool engaged = !useFsm || State == PackState.Engage;
            Vector3 playerPos;
            Vector3 playerForward;
            if (engaged)
            {
                playerPos = useFsm ? _knownPlayerPos : (player != null ? player.position : AnchorPosition);
                playerForward = player != null ? player.forward : Vector3.zero;
            }
            else
            {
                playerPos = AnchorPosition;
                playerForward = Vector3.zero;
            }

            _lastPlayerAttackAge += dt;
            var ctx = new SteerContext(
                playerPos, playerForward,
                _lastPlayerAttackDir, _lastPlayerAttackAge,
                AnchorPosition,
                EnsureProfile(),
                memberMoveSpeed, preferredRange, engageRange, dirCount);

            for (int i = 0; i < _active.Count; i++)
                _active[i].Step(ctx, _positions, dt);
        }

        /// <summary>
        /// 군집 디스폰. 멤버는 풀이 있으면 반환(재사용), 없으면 파괴. 앵커와 자신은 파괴.
        /// (풀은 SpawnSystem.Spawning 의 MonsterPool — 순환참조 피하려 object 로 받아 동적 호출 대신
        /// 인터페이스 없이 멤버 GameObject 만 넘기는 콜백 형태로 둔다.)
        /// </summary>
        public void Despawn(System.Action<GameObject> releaseMember)
        {
            for (int i = 0; i < members.Count; i++)
            {
                var m = members[i];
                if (m == null) continue;
                m.Pack = null;
                if (releaseMember != null) releaseMember(m.gameObject);
                else DestroySafe(m.gameObject);
            }
            members.Clear();
            if (anchor != null) DestroySafe(anchor.gameObject);
            DestroySafe(gameObject);
        }

        static void DestroySafe(GameObject go)
        {
            if (go == null) return;
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }

        MovementProfile EnsureProfile()
        {
            if (profile != null)
                return profile;
            if (_defaultProfile == null)
                _defaultProfile = ScriptableObject.CreateInstance<MovementProfile>();
            return _defaultProfile;
        }
    }
}
