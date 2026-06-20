using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SpawnSystem.Monsters
{
    /// <summary>
    /// 군집(Pack) — 1급 객체. 보이지 않는 '가상 앵커'를 소유하고, 길찾기는 앵커(NavMeshAgent)만 수행한다
    /// (경로가 군집당 1개로 줄어 성능 확보). 멤버는 앵커를 boids 로 추종한다.
    /// 멤버 스텝을 한곳에서(Update) 순서대로 돌려 결정적으로 만든다.
    /// 설계 문서 4장(가상 앵커 모델) 구현.
    /// </summary>
    public class MonsterPack : MonoBehaviour
    {
        [Tooltip("멤버들이 모일 가상 앵커. 비우면 이 Transform 을 앵커로 사용.")]
        public Transform anchor;

        [Tooltip("앵커의 길찾기 에이전트(선택). 있으면 MoveTo 로 군집을 이동.")]
        public NavMeshAgent anchorAgent;

        public BoidsSettings settings = BoidsSettings.Default;

        public List<Monster> members = new List<Monster>();

        readonly List<Vector3> _positions = new List<Vector3>();

        public Vector3 AnchorPosition => anchor != null ? anchor.position : transform.position;

        void Reset()
        {
            settings = BoidsSettings.Default;
        }

        /// <summary>멤버를 군집에 등록(중복 방지)하고 역참조를 건다.</summary>
        public void RegisterMember(Monster m)
        {
            if (m == null || members.Contains(m))
                return;
            members.Add(m);
            m.Pack = this;
        }

        /// <summary>군집 전체를 목표 지점으로 이동(앵커 길찾기). 에이전트가 없으면 무시.</summary>
        public void MoveTo(Vector3 worldPos)
        {
            if (anchorAgent != null && anchorAgent.isOnNavMesh)
                anchorAgent.SetDestination(worldPos);
        }

        void Update()
        {
            StepMembers(Time.deltaTime);
        }

        /// <summary>
        /// 모든 멤버를 한 스텝 추종시킨다. 위치 스냅샷을 먼저 모아(같은 프레임 기준) 모든 멤버에 동일하게
        /// 전달 → 한 멤버의 이동이 같은 프레임 다른 멤버 계산에 영향을 주지 않게 한다.
        /// </summary>
        public void StepMembers(float dt)
        {
            _positions.Clear();
            for (int i = 0; i < members.Count; i++)
                if (members[i] != null)
                    _positions.Add(members[i].transform.position);

            Vector3 anchorPos = AnchorPosition;
            for (int i = 0; i < members.Count; i++)
                if (members[i] != null)
                    members[i].StepFollow(anchorPos, _positions, settings, dt);
        }
    }
}
