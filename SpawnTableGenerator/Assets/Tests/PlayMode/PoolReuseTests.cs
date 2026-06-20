using System.Collections;
using NUnit.Framework;
using SpawnSystem.Monsters;
using SpawnSystem.Spawning;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace SpawnSystem.Tests
{
    /// <summary>
    /// 풀링 통합 검증(설계 §3): 군집 디스폰 시 멤버가 파괴되지 않고 풀로 반환되어, 재스폰 때 재사용됨
    /// (CreatedCount 가 늘어나지 않음). 재사용 멤버도 navmesh 위에 정상 배치.
    /// </summary>
    public class PoolReuseTests
    {
        [UnityTest]
        public IEnumerator Pool_ReusesMembers_AcrossDespawnRespawn()
        {
            using var env = NavTestEnvironment.WithCenterWall(60f, new Vector3(0f, 1.5f, 30f), new Vector3(1f, 3f, 1f));

            var def = ScriptableObject.CreateInstance<MonsterDef>();
            def.scale = 0.6f;
            def.moveSpeed = 5f;
            def.color = Color.red;
            def.preferredRange = new Vector2(2f, 5f);

            var poolRoot = new GameObject("PoolRoot");
            var pool = new MonsterPool(poolRoot.transform);
            var player = new GameObject("Player");
            var holder = new GameObject("Holder");

            var pack1 = PackFactory.BuildFromDef(def, 5, Vector3.zero, player.transform, holder.transform, pool);
            yield return null;
            Assert.AreEqual(5, pool.CreatedCount, "첫 스폰: 5개 생성");
            Assert.AreEqual(5, pool.ActiveCount);

            pack1.Despawn(go => pool.Release(go));
            yield return null;
            Assert.AreEqual(0, pool.ActiveCount, "디스폰 후 활성 0");
            Assert.AreEqual(5, pool.CreatedCount, "디스폰은 파괴하지 않는다(재사용 대기)");

            var pack2 = PackFactory.BuildFromDef(def, 5, new Vector3(3f, 0f, 3f), player.transform, holder.transform, pool);
            yield return null;
            Assert.AreEqual(5, pool.CreatedCount, "재스폰: 재사용 → 생성 개수 그대로");
            Assert.AreEqual(5, pool.ActiveCount);

            foreach (var m in pack2.members)
            {
                var ag = m.GetComponent<NavMeshAgent>();
                Assert.IsTrue(ag.isOnNavMesh, "재사용 멤버가 navmesh 위에 있어야 한다");
            }

            Object.DestroyImmediate(holder);
            Object.DestroyImmediate(poolRoot);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(def);
        }
    }
}
