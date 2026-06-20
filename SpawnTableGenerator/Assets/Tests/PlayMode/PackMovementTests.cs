using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SpawnSystem.Monsters;
using UnityEngine;
using UnityEngine.TestTools;

namespace SpawnSystem.Tests
{
    /// <summary>
    /// 군집 이동 통합 테스트(로드맵 2번 검증). navmesh 불필요 — 정적 앵커 주변으로 멤버들이
    /// boids 로 수렴하되 분리로 서로 겹치지 않음을 확인. 스크린샷 대신 수치로 군집 이동을 검증한다.
    /// </summary>
    public class PackMovementTests
    {
        static MonsterPack BuildPack(Vector3 anchorPos, Vector3[] memberStarts, out List<GameObject> spawned)
        {
            spawned = new List<GameObject>();

            var packGo = new GameObject("Pack");
            spawned.Add(packGo);
            var pack = packGo.AddComponent<MonsterPack>();
            pack.settings = BoidsSettings.Default;

            var anchorGo = new GameObject("Anchor");
            anchorGo.transform.position = anchorPos;
            anchorGo.transform.SetParent(packGo.transform);
            pack.anchor = anchorGo.transform;

            foreach (var p in memberStarts)
            {
                var mGo = new GameObject("Monster");
                mGo.transform.position = p;
                var m = mGo.AddComponent<Monster>();
                pack.RegisterMember(m);
                spawned.Add(mGo);
            }
            return pack;
        }

        static void Cleanup(List<GameObject> spawned)
        {
            foreach (var go in spawned)
                if (go != null)
                    Object.DestroyImmediate(go);
        }

        static float AvgDistToAnchor(MonsterPack pack)
        {
            float sum = 0f;
            int n = 0;
            foreach (var m in pack.members)
            {
                sum += Vector3.Distance(m.transform.position, pack.AnchorPosition);
                n++;
            }
            return n > 0 ? sum / n : 0f;
        }

        static float MinPairwise(MonsterPack pack)
        {
            float min = float.MaxValue;
            var ms = pack.members;
            for (int i = 0; i < ms.Count; i++)
                for (int j = i + 1; j < ms.Count; j++)
                    min = Mathf.Min(min, Vector3.Distance(ms[i].transform.position, ms[j].transform.position));
            return min;
        }

        [Test]
        public void Members_ConvergeToAnchor_WithoutCollapsing()
        {
            var starts = new[]
            {
                new Vector3(8f, 0f, 0f), new Vector3(-7f, 0f, 2f), new Vector3(0f, 0f, 9f),
                new Vector3(3f, 0f, -8f), new Vector3(-5f, 0f, -5f), new Vector3(6f, 0f, 6f),
            };
            var pack = BuildPack(Vector3.zero, starts, out var spawned);
            try
            {
                float before = AvgDistToAnchor(pack);
                for (int i = 0; i < 800; i++)
                    pack.StepMembers(0.02f);
                float after = AvgDistToAnchor(pack);

                Assert.Less(after, before, "멤버들이 앵커 쪽으로 모여야 한다");
                Assert.Less(after, 5f, "군집이 앵커 주변으로 뭉쳐야 한다");
                Assert.Greater(MinPairwise(pack), 0.25f, "분리로 인해 서로 겹치지 않아야 한다");
            }
            finally
            {
                Cleanup(spawned);
            }
        }

        [UnityTest]
        public IEnumerator Update_DrivesMemberMovement()
        {
            var pack = BuildPack(Vector3.zero, new[] { new Vector3(10f, 0f, 0f) }, out var spawned);
            try
            {
                var m = pack.members[0];
                float d0 = Vector3.Distance(m.transform.position, pack.AnchorPosition);
                for (int i = 0; i < 60; i++)
                    yield return null; // MonsterPack.Update 가 매 프레임 StepMembers 호출

                float d1 = Vector3.Distance(m.transform.position, pack.AnchorPosition);
                Assert.Less(d1, d0 - 0.5f, "Update 루프가 멤버를 앵커로 이동시켜야 한다");
            }
            finally
            {
                Cleanup(spawned);
            }
        }
    }
}
