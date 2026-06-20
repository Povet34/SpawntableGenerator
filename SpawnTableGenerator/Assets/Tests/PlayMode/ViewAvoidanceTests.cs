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
    /// 컨텍스트 스티어링(§12) 통합 검증: 플레이어 시야 콘 안에 둔 멤버들이 시간이 지나며 콘 밖으로
    /// 회피하는지 확인. navmesh 구속도 유지.
    /// </summary>
    public class ViewAvoidanceTests
    {
        const float ConeDeg = 70f;
        const float ConeRange = 18f;

        [UnityTest]
        public IEnumerator Members_MoveOutOfPlayerViewCone()
        {
            // 벽은 멀리(z=40) 두어 원점 주변은 열린 평지.
            using var env = NavTestEnvironment.WithCenterWall(60f, new Vector3(0f, 1.5f, 40f), new Vector3(1f, 3f, 1f));

            var playerGo = new GameObject("Player");
            playerGo.transform.position = Vector3.zero;
            playerGo.transform.forward = Vector3.forward; // +z 응시

            var packGo = new GameObject("Pack");
            var pack = packGo.AddComponent<MonsterPack>();
            var anchorGo = new GameObject("Anchor");
            anchorGo.transform.SetParent(packGo.transform);
            anchorGo.transform.position = Vector3.zero;
            pack.anchor = anchorGo.transform;
            pack.player = playerGo.transform;
            pack.useFsm = false; // 시야 회피는 교전 행동 — FSM 끄고 항상 교전으로 검증
            pack.engageRange = 20f;
            pack.preferredRange = new Vector2(3f, 6f);

            var members = new List<GameObject>();
            for (int i = 0; i < 6; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.SetParent(packGo.transform);
                Vector3 p = new Vector3((i - 2.5f) * 0.6f, 0f, 5f); // 전부 +z 콘 안, 거리 ~5
                NavMesh.SamplePosition(p, out var h, 3f, NavMesh.AllAreas);
                go.transform.position = h.position;

                var a = go.AddComponent<NavMeshAgent>();
                a.radius = 0.4f;
                a.height = 2f;
                a.baseOffset = 1f;
                a.speed = 6f;
                a.angularSpeed = 0f;
                a.updateRotation = false;
                a.autoBraking = false;
                a.avoidancePriority = 60;

                var m = go.AddComponent<Monster>();
                m.Mover = new AgentMover(a);
                pack.RegisterMember(m);
                members.Add(go);
            }

            int beforeCount = CountInCone(members, playerGo.transform);
            float beforeExposure = SumPressure(members, playerGo.transform);
            Assert.AreEqual(6, beforeCount, "시작 시 전원 콘 안이어야 한다");

            for (int f = 0; f < 300; f++)
                yield return null; // MonsterPack.Update 가 매 프레임 스티어링

            int afterCount = CountInCone(members, playerGo.transform);
            float afterExposure = SumPressure(members, playerGo.transform);

            // 핵심 지표: '보여지는 정도'(압박 합) 감소. 멤버는 만족 임계 미만이면 가장자리에서 멈추므로
            // 기하학적 콘을 완전히 벗어나진 않을 수 있음 — 노출 감소가 올바른 검증.
            Assert.Less(afterExposure, beforeExposure * 0.6f, "시야 노출(압박 합)이 크게 줄어야 한다");
            Assert.Less(afterCount, beforeCount, "콘 안 멤버 수도 줄어야 한다");

            foreach (var go in members)
            {
                var a = go.GetComponent<NavMeshAgent>();
                Assert.IsTrue(a.isOnNavMesh, "멤버는 navmesh 에 구속돼 있어야 한다");
            }

            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(packGo);
        }

        static int CountInCone(List<GameObject> members, Transform viewer)
        {
            int c = 0;
            foreach (var go in members)
                if (ViewPressure.Cone(viewer.position, viewer.forward, go.transform.position, ConeDeg, ConeRange) > 0f)
                    c++;
            return c;
        }

        static float SumPressure(List<GameObject> members, Transform viewer)
        {
            float s = 0f;
            foreach (var go in members)
                s += ViewPressure.Cone(viewer.position, viewer.forward, go.transform.position, ConeDeg, ConeRange);
            return s;
        }
    }
}
