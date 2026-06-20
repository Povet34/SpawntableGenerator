using UnityEngine;
using UnityEngine.AI;

namespace SpawnSystem.Monsters
{
    /// <summary>
    /// 멤버 이동 적용 추상화. boids 가 계산한 desired velocity 를 실제 위치에 어떻게 반영할지를 분리.
    /// LOD 에 따라 교체: 화면 안/근거리 = <see cref="AgentMover"/>, 화면 밖/원거리 = <see cref="TransformMover"/>.
    /// </summary>
    public interface IMemberMover
    {
        void MoveBy(Transform t, Vector3 velocity, float dt);
    }

    /// <summary>저정밀/테스트용: NavMesh 없이 transform 적분(평면 유지). 회피·벽 충돌 없음.</summary>
    public sealed class TransformMover : IMemberMover
    {
        public void MoveBy(Transform t, Vector3 velocity, float dt)
        {
            Vector3 p = t.position + velocity * dt;
            p.y = t.position.y;
            t.position = p;
        }
    }

    /// <summary>
    /// 고정밀: NavMeshAgent.Move 로 이동. 길찾기(SetDestination)는 호출하지 않으므로 "군집당 1 길찾기"
    /// 의도를 유지하면서, navmesh 가 벽 통과를 막고 에이전트 회피(RVO)가 멤버 간 물리적 밀어냄을 준다.
    /// </summary>
    public sealed class AgentMover : IMemberMover
    {
        readonly NavMeshAgent _agent;

        public AgentMover(NavMeshAgent agent)
        {
            _agent = agent;
        }

        public void MoveBy(Transform t, Vector3 velocity, float dt)
        {
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.Move(velocity * dt);
            }
            else
            {
                Vector3 p = t.position + velocity * dt;
                p.y = t.position.y;
                t.position = p;
            }
        }
    }
}
