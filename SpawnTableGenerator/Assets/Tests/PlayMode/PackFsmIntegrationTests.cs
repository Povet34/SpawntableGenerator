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
    /// 군집 인지(FSM) 통합 검증(설계 §5): 플레이어를 못 보면 순찰, 한 멤버라도 발각하면 전원 교전(공유 상태)
    /// 으로 전환하고 앵커가 플레이어를 추격한다.
    /// </summary>
    public class PackFsmIntegrationTests
    {
        [UnityTest]
        public IEnumerator Pack_Patrols_ThenEngages_WhenOneMemberDetectsPlayer()
        {
            using var env = NavTestEnvironment.WithCenterWall(80f, new Vector3(0f, 1.5f, 40f), new Vector3(1f, 3f, 1f));

            var playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, 0f, 30f); // 멀리(시야 밖)
            playerGo.transform.forward = Vector3.forward;

            var packGo = new GameObject("Pack");
            var pack = packGo.AddComponent<MonsterPack>();
            pack.sightRange = 14f;
            pack.closeSightRange = 4f;
            pack.memberMoveSpeed = 5f;
            pack.preferredRange = new Vector2(2f, 5f);
            pack.player = playerGo.transform;

            var anchorGo = new GameObject("Anchor");
            anchorGo.transform.SetParent(packGo.transform);
            NavMesh.SamplePosition(Vector3.zero, out var ah, 4f, NavMesh.AllAreas);
            anchorGo.transform.position = ah.position;
            var anchorAgent = anchorGo.AddComponent<NavMeshAgent>();
            anchorAgent.radius = 0.5f;
            anchorAgent.height = 2f;
            anchorAgent.speed = 4f;
            anchorAgent.stoppingDistance = 1f;
            pack.anchor = anchorGo.transform;
            pack.anchorAgent = anchorAgent;

            var members = new List<GameObject>();
            for (int i = 0; i < 5; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.SetParent(packGo.transform);
                float a = i / 5f * Mathf.PI * 2f;
                NavMesh.SamplePosition(anchorGo.transform.position + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 3f, out var mh, 3f, NavMesh.AllAreas);
                go.transform.position = mh.position;
                var ag = go.AddComponent<NavMeshAgent>();
                ag.radius = 0.4f; ag.height = 2f; ag.baseOffset = 1f; ag.speed = 5f;
                ag.angularSpeed = 0f; ag.updateRotation = false; ag.autoBraking = false; ag.avoidancePriority = 60;
                var m = go.AddComponent<Monster>();
                m.Mover = new AgentMover(ag);
                pack.RegisterMember(m);
                members.Add(go);
            }

            // 순찰: 플레이어가 멀어 못 봄
            for (int f = 0; f < 30; f++) yield return null;
            Assert.AreEqual(PackState.Patrol, pack.State, "멀어서 못 보면 순찰이어야 한다");

            // 한 멤버 바로 옆으로 플레이어 이동 → 근거리 발각
            Vector3 near = members[0].transform.position + new Vector3(2f, 0f, 0f);
            NavMesh.SamplePosition(near, out var ph, 4f, NavMesh.AllAreas);
            playerGo.transform.position = ph.position;

            for (int f = 0; f < 45; f++) yield return null; // 발각 + 앵커 리패스
            Assert.AreEqual(PackState.Engage, pack.State, "한 멤버라도 발각하면 전원 교전(공유 상태)");

            // 교전 시 앵커가 플레이어를 추격(목적지가 플레이어 근처)
            if (anchorAgent.hasPath)
                Assert.Less(Vector3.Distance(anchorAgent.destination, playerGo.transform.position), 3f,
                    "교전 시 앵커는 플레이어를 추격해야 한다");

            Object.DestroyImmediate(packGo);
            Object.DestroyImmediate(playerGo);
        }

        [UnityTest]
        public IEnumerator Patrol_AnchorKeepsMoving_NotStandingStill()
        {
            using var env = NavTestEnvironment.WithCenterWall(80f, new Vector3(0f, 1.5f, 40f), new Vector3(1f, 3f, 1f));

            var packGo = new GameObject("Pack");
            var pack = packGo.AddComponent<MonsterPack>();
            pack.player = null; // 플레이어 모름 → 순찰 유지

            var anchorGo = new GameObject("Anchor");
            anchorGo.transform.SetParent(packGo.transform);
            NavMesh.SamplePosition(Vector3.zero, out var ah, 4f, NavMesh.AllAreas);
            anchorGo.transform.position = ah.position;
            var ag = anchorGo.AddComponent<NavMeshAgent>();
            ag.radius = 0.5f; ag.height = 2f; ag.speed = 4f; ag.stoppingDistance = 1f;
            pack.anchor = anchorGo.transform;
            pack.anchorAgent = ag;

            Vector3 start = anchorGo.transform.position;
            for (int f = 0; f < 120; f++) yield return null; // ~2초

            Assert.AreEqual(PackState.Patrol, pack.State, "플레이어 모름 → 순찰");
            Assert.Greater(Vector3.Distance(anchorGo.transform.position, start), 2f, "순찰 중 앵커가 계속 전진해야(가만히 X)");

            Object.DestroyImmediate(packGo);
        }
    }
}
