using System.Collections;
using NUnit.Framework;
using SpawnSystem.Monsters;
using SpawnSystem.Spawning;
using UnityEngine;
using UnityEngine.TestTools;

namespace SpawnSystem.Tests
{
    /// <summary>
    /// 디렉터가 플레이어로부터 멀어진 '순찰' 군집을 풀로 회수(디스폰)함을 검증. (자주 만나게 하려고
    /// 먼 패트롤은 제거하고 예측 위치로 재스폰하는 흐름의 회수 부분.)
    /// </summary>
    public class DirectorRecycleTests
    {
        [UnityTest]
        public IEnumerator Director_RecyclesDistantPatrolPack()
        {
            using var env = NavTestEnvironment.WithCenterWall(160f, new Vector3(0f, 1.5f, 100f), new Vector3(1f, 3f, 1f));

            var def = ScriptableObject.CreateInstance<MonsterDef>();
            def.scale = 0.6f; def.moveSpeed = 4f; def.color = Color.red; def.preferredRange = new Vector2(2f, 5f);
            var table = ScriptableObject.CreateInstance<SpawnTable>();
            table.entries = new[] { new SpawnEntry { monster = def, weight = 1f, cost = 2f, groupSize = new Vector2Int(3, 3), minDifficulty = 0f } };
            var profile = ScriptableObject.CreateInstance<DirectorProfile>();
            profile.startingBudget = 4f; profile.budgetPerSecond = 2f; profile.maxBudget = 8f;
            profile.maxSpawnInterval = 0.4f; profile.minSpawnInterval = 0.3f; profile.maxTime = 10f; profile.maxConcurrentPacks = 6;

            var playerGo = new GameObject("Player");
            playerGo.transform.position = Vector3.zero;
            var dirGo = new GameObject("Director");
            var dir = dirGo.AddComponent<SpawnDirector>();
            dir.profile = profile; dir.table = table; dir.player = playerGo.transform;
            dir.despawnDistance = 45f; dir.spawnAroundRadius = 12f;

            MonsterPack pack0 = null;
            for (int f = 0; f < 60 && pack0 == null; f++)
            {
                yield return null;
                if (dir.ActivePacks.Count > 0) pack0 = dir.ActivePacks[0];
            }
            Assert.IsNotNull(pack0, "디렉터가 군집을 스폰해야 한다");
            Assert.AreEqual(PackState.Patrol, pack0.State, "갓 스폰된 군집은 순찰 상태");

            // 플레이어를 멀리(>despawnDistance) 이동 → 순찰 군집이 멀어짐
            playerGo.transform.position = new Vector3(0f, 0f, 70f);
            for (int f = 0; f < 120; f++) yield return null;

            Assert.IsTrue(pack0 == null, "멀어진 순찰 군집은 풀로 회수(디스폰)돼야 한다");

            Object.DestroyImmediate(dirGo);
            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(def);
            Object.DestroyImmediate(table);
            Object.DestroyImmediate(profile);
        }
    }
}
