using System.Collections;
using NUnit.Framework;
using SpawnSystem.Player;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace SpawnSystem.Tests
{
    /// <summary>
    /// 베이크된 NavMesh 위에서 "벽 우회 / 벽면 밀착"을 실제 NavMesh·에이전트로 검증하는 통합 테스트.
    /// 순수 로직(MovementResolver)과 NavMesh 샘플링/길찾기를 함께 본다.
    /// </summary>
    public class MovementIntegrationTests
    {
        const float Hug = 0.6f;

        [Test]
        public void NavTestEnvironment_BakesNavMesh()
        {
            using var env = NavTestEnvironment.WithCenterWall();
            Assert.IsTrue(env.HasNavMesh(), "런타임 navmesh 가 베이크되어야 한다");
        }

        [Test]
        public void VerticalWallFaceClick_ResolvesToHuggingNavmeshPoint()
        {
            using var env = NavTestEnvironment.WithCenterWall(); // 벽 앞면 z = -0.5

            // 벽 앞면(-z쪽) 중간 높이를 클릭했다고 가정.
            Vector3 hitPoint = new Vector3(0f, 1.5f, -0.5f);
            Vector3 hitNormal = Vector3.back; // (0,0,-1): 벽 바깥(플레이어 쪽)
            Vector3 target = MovementResolver.ResolveClickTarget(hitPoint, hitNormal, 0f, Hug);

            Assert.IsTrue(NavMesh.SamplePosition(target, out var hit, 3f, NavMesh.AllAreas),
                "밀착 타깃 근처에 navmesh 가 있어야 한다");
            Assert.Less(hit.position.z, -0.5f + 0.01f, "벽 앞면(z=-0.5)보다 앞(더 작은 z)에 붙어야 한다");
            Assert.Greater(hit.position.z, -0.5f - (Hug + 1.5f), "벽에 바짝 붙어야 한다(너무 멀면 안 됨)");
            Assert.AreEqual(0f, hit.position.y, 0.3f, "바닥 높이여야 한다");
        }

        [Test]
        public void DestinationBehindWall_PathRoutesAround()
        {
            using var env = NavTestEnvironment.WithCenterWall();

            Assert.IsTrue(NavMesh.SamplePosition(new Vector3(0f, 0f, -4f), out var front, 2f, NavMesh.AllAreas));
            Assert.IsTrue(NavMesh.SamplePosition(new Vector3(0f, 0f, 4f), out var behind, 2f, NavMesh.AllAreas));

            var path = new NavMeshPath();
            bool ok = NavMesh.CalculatePath(front.position, behind.position, NavMesh.AllAreas, path);
            Assert.IsTrue(ok && path.status == NavMeshPathStatus.PathComplete, "완전한 경로가 나와야 한다");

            float straight = Vector3.Distance(front.position, behind.position);
            float pathLen = 0f;
            for (int i = 1; i < path.corners.Length; i++)
                pathLen += Vector3.Distance(path.corners[i - 1], path.corners[i]);

            Assert.Greater(path.corners.Length, 2, "벽을 우회하면 중간 코너가 생긴다");
            Assert.Greater(pathLen, straight + 1f, "우회 경로는 직선보다 충분히 길어야 한다");
        }

        [UnityTest]
        public IEnumerator Agent_MovesAroundWall_AndArrives()
        {
            using var env = NavTestEnvironment.WithCenterWall();

            var agentGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            var agent = agentGo.AddComponent<NavMeshAgent>();
            agent.radius = 0.5f;
            agent.height = 2f;
            agent.baseOffset = 1f;
            agent.speed = 12f;
            agent.acceleration = 60f;
            agent.angularSpeed = 999f;

            Assert.IsTrue(NavMesh.SamplePosition(new Vector3(0f, 0f, -4f), out var start, 2f, NavMesh.AllAreas));
            agent.Warp(start.position);

            Assert.IsTrue(NavMesh.SamplePosition(new Vector3(0f, 0f, 4f), out var dest, 2f, NavMesh.AllAreas));
            agent.SetDestination(dest.position);

            float timeout = 10f;
            float t = 0f;
            while (t < timeout)
            {
                if (!agent.pathPending
                    && agent.remainingDistance <= agent.stoppingDistance + 0.25f
                    && agent.velocity.sqrMagnitude < 0.05f)
                    break;
                t += Time.deltaTime;
                yield return null;
            }

            Vector3 flatAgent = new Vector3(agent.transform.position.x, 0f, agent.transform.position.z);
            Vector3 flatDest = new Vector3(dest.position.x, 0f, dest.position.z);
            Assert.Less(Vector3.Distance(flatAgent, flatDest), 1.5f,
                "에이전트가 벽을 돌아 목적지에 도착해야 한다");

            Object.DestroyImmediate(agentGo);
        }
    }
}
