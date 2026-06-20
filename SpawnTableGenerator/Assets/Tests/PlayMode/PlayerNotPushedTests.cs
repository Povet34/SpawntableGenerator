using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace SpawnSystem.Tests
{
    /// <summary>
    /// 회피 우선순위로 무리가 플레이어를 밀어내지 못함을 검증.
    /// 플레이어 에이전트가 몬스터보다 '더 중요'(avoidancePriority 숫자 작음)하면, 몬스터는 플레이어를
    /// 피하되 플레이어는 떠밀리지 않는다.
    /// </summary>
    public class PlayerNotPushedTests
    {
        [UnityTest]
        public IEnumerator HigherPriorityPlayer_IsNotPushedBySwarm()
        {
            using var env = NavTestEnvironment.WithCenterWall(60f, new Vector3(0f, 1.5f, 30f), new Vector3(1f, 3f, 1f));

            // 플레이어: 더 중요(낮은 숫자), 정지 상태.
            var playerGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            var pAgent = playerGo.AddComponent<NavMeshAgent>();
            pAgent.radius = 0.5f;
            pAgent.height = 2f;
            pAgent.baseOffset = 1f;
            pAgent.avoidancePriority = 50; // 실제 씬 플레이어 기본값과 동일
            NavMesh.SamplePosition(Vector3.zero, out var ph, 2f, NavMesh.AllAreas);
            pAgent.Warp(ph.position);
            Vector3 start = playerGo.transform.position;

            // 무리: 덜 중요(높은 숫자), 매 프레임 플레이어로 직진해 밀어붙임.
            var swarm = new List<GameObject>();
            for (int i = 0; i < 8; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                float ang = i / 8f * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(ang) * 3f, 0f, Mathf.Sin(ang) * 3f);
                NavMesh.SamplePosition(p, out var h, 3f, NavMesh.AllAreas);
                go.transform.position = h.position;
                var a = go.AddComponent<NavMeshAgent>();
                a.radius = 0.4f;
                a.height = 2f;
                a.baseOffset = 1f;
                a.speed = 6f;
                a.updateRotation = false;
                a.autoBraking = false;
                a.avoidancePriority = 60; // 플레이어(50)보다 덜 중요 → 플레이어를 못 민다
                swarm.Add(go);
            }

            for (int f = 0; f < 150; f++)
            {
                foreach (var go in swarm)
                {
                    var a = go.GetComponent<NavMeshAgent>();
                    Vector3 toPlayer = playerGo.transform.position - go.transform.position;
                    toPlayer.y = 0f;
                    if (toPlayer.sqrMagnitude > 1e-4f)
                        a.Move(toPlayer.normalized * (a.speed * Time.deltaTime));
                }
                yield return null;
            }

            float moved = Vector3.Distance(
                new Vector3(playerGo.transform.position.x, 0f, playerGo.transform.position.z),
                new Vector3(start.x, 0f, start.z));
            Assert.Less(moved, 1.5f, "플레이어는 무리에 둘러싸여도 거의 안 움직여야 한다");

            Object.DestroyImmediate(playerGo);
            foreach (var go in swarm)
                Object.DestroyImmediate(go);
        }
    }
}
