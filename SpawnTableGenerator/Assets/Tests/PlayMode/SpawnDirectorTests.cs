using System.Collections;
using NUnit.Framework;
using SpawnSystem.Monsters;
using SpawnSystem.Spawning;
using UnityEngine;
using UnityEngine.TestTools;

namespace SpawnSystem.Tests
{
    /// <summary>
    /// SpawnDirector 통합 검증: 긴장도/예산으로 시간 경과에 따라 스폰 테이블에서 군집을 실제로 스폰.
    /// </summary>
    public class SpawnDirectorTests
    {
        [UnityTest]
        public IEnumerator Director_SpawnsPacks_OverTime()
        {
            using var env = NavTestEnvironment.WithCenterWall(80f, new Vector3(0f, 1.5f, 40f), new Vector3(1f, 3f, 1f));

            var def = ScriptableObject.CreateInstance<MonsterDef>();
            def.scale = 0.6f;
            def.moveSpeed = 5f;
            def.color = Color.red;
            def.preferredRange = new Vector2(2f, 5f);

            var table = ScriptableObject.CreateInstance<SpawnTable>();
            table.entries = new[]
            {
                new SpawnEntry { monster = def, weight = 1f, cost = 3f, groupSize = new Vector2Int(3, 4), minDifficulty = 0f },
            };

            var profile = ScriptableObject.CreateInstance<DirectorProfile>();
            profile.startingBudget = 10f;
            profile.budgetPerSecond = 5f;
            profile.maxBudget = 20f;
            profile.maxSpawnInterval = 0.5f;
            profile.minSpawnInterval = 0.2f;
            profile.maxTime = 10f;
            profile.maxConcurrentPacks = 10;

            var playerGo = new GameObject("Player");
            var dirGo = new GameObject("Director");
            var dir = dirGo.AddComponent<SpawnDirector>();
            dir.profile = profile;
            dir.table = table;
            dir.player = playerGo.transform;
            dir.objectivesTotal = 3;
            dir.objectivesRemaining = 3;

            for (int f = 0; f < 120; f++) // ~2초
                yield return null;

            var packs = Object.FindObjectsByType<MonsterPack>(FindObjectsSortMode.None);
            var monsters = Object.FindObjectsByType<Monster>(FindObjectsSortMode.None);
            Assert.Greater(packs.Length, 0, "디렉터가 시간 경과로 군집을 스폰해야 한다");
            Assert.Greater(monsters.Length, 0, "군집 멤버가 생겨야 한다");
            Assert.Less(dir.Budget, profile.maxBudget, "스폰으로 예산이 소비되어야 한다");

            Object.DestroyImmediate(dirGo); // 자식(앵커/군집/멤버) 함께 정리
            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(def);
            Object.DestroyImmediate(table);
            Object.DestroyImmediate(profile);
        }
    }
}
