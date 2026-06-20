using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SpawnSystem.Monsters;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace SpawnSystem.Tests
{
    /// <summary>
    /// AgentMover(NavMeshAgent.Move) 로 구동되는 멤버가 벽을 통과하지 못함을 실제 navmesh 로 검증.
    /// (사용자 보고: 기존 transform 적분 멤버가 벽을 파고듦 → 수정 확인.)
    /// </summary>
    public class AgentContainmentTests
    {
        [UnityTest]
        public IEnumerator AgentMembers_DoNotPenetrateWall()
        {
            using var env = NavTestEnvironment.WithCenterWall(); // 벽: x∈[-5,5], z∈[-0.5,0.5], 높이 3

            var packGo = new GameObject("Pack");
            var pack = packGo.AddComponent<MonsterPack>();
            // player 미설정 → 앵커(벽 너머)를 기준으로 컨텍스트 스티어링 → 벽을 향해 밀어붙임.

            var anchorGo = new GameObject("Anchor");
            anchorGo.transform.position = new Vector3(0f, 0f, 3f); // 벽 너머
            anchorGo.transform.SetParent(packGo.transform);
            pack.anchor = anchorGo.transform;

            var members = new List<GameObject>();
            for (int i = 0; i < 5; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.SetParent(packGo.transform);
                Vector3 p = new Vector3(-2f + i, 0f, -3f); // 벽 앞
                NavMesh.SamplePosition(p, out var h, 3f, NavMesh.AllAreas);
                go.transform.position = h.position;

                var agent = go.AddComponent<NavMeshAgent>();
                agent.radius = 0.4f;
                agent.height = 2f;
                agent.baseOffset = 1f;
                agent.speed = 6f;
                agent.angularSpeed = 0f;
                agent.updateRotation = false;
                agent.autoBraking = false;

                var m = go.AddComponent<Monster>();
                m.Mover = new AgentMover(agent);
                pack.RegisterMember(m);
                members.Add(go);
            }

            // MonsterPack.Update 가 매 프레임 StepMembers 호출 → 멤버가 벽을 향해 이동 시도.
            for (int f = 0; f < 180; f++)
                yield return null;

            foreach (var go in members)
            {
                Vector3 pos = go.transform.position;
                var ag = go.GetComponent<NavMeshAgent>();
                Assert.IsTrue(ag.isOnNavMesh, "멤버 에이전트는 navmesh 에 구속돼 있어야 한다(벽 통과 X)");

                bool insideWallFootprint = Mathf.Abs(pos.x) < 5f && Mathf.Abs(pos.z) < 0.5f;
                Assert.IsFalse(insideWallFootprint, "멤버가 벽 내부에 들어가면 안 된다");
            }

            Object.DestroyImmediate(packGo);
        }
    }
}
