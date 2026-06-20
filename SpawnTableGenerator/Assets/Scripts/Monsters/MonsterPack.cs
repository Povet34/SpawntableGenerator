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

        public List<Monster> members = new List<Monster>();

        readonly List<Vector3> _positions = new List<Vector3>();
        readonly List<Monster> _active = new List<Monster>();

        static MovementProfile _defaultProfile;

        public Vector3 AnchorPosition => anchor != null ? anchor.position : transform.position;

        public void RegisterMember(Monster m)
        {
            if (m == null || members.Contains(m))
                return;
            members.Add(m);
            m.Pack = this;
        }

        /// <summary>군집 전체를 목표 지점으로 이동(앵커 길찾기). 에이전트 없으면 무시.</summary>
        public void MoveTo(Vector3 worldPos)
        {
            if (anchorAgent != null && anchorAgent.isOnNavMesh)
                anchorAgent.SetDestination(worldPos);
        }

        void Update()
        {
            StepMembers(Time.deltaTime);
        }

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

            Vector3 playerPos = player != null ? player.position : AnchorPosition;
            Vector3 playerForward = player != null ? player.forward : transform.forward;

            var ctx = new SteerContext(
                playerPos, playerForward,
                Vector3.zero, 999f,            // 직전 공격 — 공격 시스템(후속) 연결 전엔 없음
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
