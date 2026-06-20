using NUnit.Framework;
using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Tests
{
    /// <summary>
    /// 군집 스포너가 실제로 멤버 N마리 + navmesh 위 앵커를 만들어 군집을 구성하는지 검증.
    /// (몬스터가 "실제로 나오는가"를 코드로 확인.)
    /// </summary>
    public class SpawnerTests
    {
        [Test]
        public void Spawn_CreatesMembersAndAnchorOnNavMesh()
        {
            // 벽은 멀리(z=30) 두어 스폰 지점(원점)은 열린 평지.
            using var env = NavTestEnvironment.WithCenterWall(60f, new Vector3(0f, 1.5f, 30f), new Vector3(1f, 3f, 1f));

            var go = new GameObject("Spawner");
            var spawner = go.AddComponent<MonsterPackSpawner>();
            spawner.spawnOnStart = false;
            spawner.memberCount = 7;
            spawner.spawnCenter = Vector3.zero;
            spawner.target = null;

            try
            {
                var pack = spawner.Spawn();

                Assert.AreEqual(7, pack.members.Count, "memberCount 만큼 몬스터가 생성되어야 한다");
                Assert.IsNotNull(pack.anchor, "가상 앵커가 있어야 한다");
                Assert.IsNotNull(pack.anchorAgent, "앵커 길찾기 에이전트가 있어야 한다");
                Assert.IsTrue(pack.anchorAgent.isOnNavMesh, "앵커 에이전트가 navmesh 위에 있어야 한다");
                foreach (var m in pack.members)
                {
                    Assert.IsNotNull(m);
                    Assert.AreSame(pack, m.Pack, "멤버가 군집을 역참조해야 한다");
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Spawn_WithMonsterDef_AppliesScale()
        {
            using var env = NavTestEnvironment.WithCenterWall(60f, new Vector3(0f, 1.5f, 30f), new Vector3(1f, 3f, 1f));

            var def = ScriptableObject.CreateInstance<MonsterDef>();
            def.scale = 1.6f;
            def.moveSpeed = 2.5f;
            def.color = Color.magenta;

            var go = new GameObject("Spawner");
            var spawner = go.AddComponent<MonsterPackSpawner>();
            spawner.spawnOnStart = false;
            spawner.memberCount = 3;
            spawner.spawnCenter = Vector3.zero;
            spawner.monsterDef = def;

            try
            {
                var pack = spawner.Spawn();
                Assert.AreEqual(3, pack.members.Count);
                foreach (var m in pack.members)
                    Assert.AreEqual(1.6f, m.transform.localScale.x, 0.01f, "멤버 크기가 def.scale 을 따라야 한다");
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(def);
            }
        }
    }
}
