using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SpawnSystem.Monsters;
using SpawnSystem.Spawning;
using UnityEngine;
using UnityEngine.TestTools;

namespace SpawnSystem.Tests
{
    /// <summary>
    /// 몬스터 사망 → 파괴가 아니라 풀로 회수(재사용 대기), 전멸하면 군집 디스폰. 풀링이 실제로 작동하는 트리거.
    /// </summary>
    public class MonsterDeathTests
    {
        [UnityTest]
        public IEnumerator DeadMember_ReleasedToPool_PackDespawnsWhenEmpty()
        {
            using var env = NavTestEnvironment.WithCenterWall(60f, new Vector3(0f, 1.5f, 30f), new Vector3(1f, 3f, 1f));

            var def = ScriptableObject.CreateInstance<MonsterDef>();
            def.scale = 0.6f; def.moveSpeed = 4f; def.color = Color.red; def.preferredRange = new Vector2(2f, 5f); def.maxHP = 5f;

            var poolRoot = new GameObject("PoolRoot");
            var pool = new MonsterPool(poolRoot.transform);
            var player = new GameObject("Player");
            var holder = new GameObject("Holder");

            var pack = PackFactory.BuildFromDef(def, 3, Vector3.zero, player.transform, holder.transform, pool);
            yield return null;
            Assert.AreEqual(3, pack.members.Count);
            Assert.AreEqual(3, pool.ActiveCount);

            // 한 마리 사망
            pack.members[0].Health.TakeDamage(99f, DamageType.Normal);
            for (int f = 0; f < 5; f++) yield return null;
            Assert.AreEqual(2, pack.members.Count, "사망 멤버는 군집에서 빠져야 한다");
            Assert.AreEqual(2, pool.ActiveCount, "사망 멤버는 풀로 반환(활성 감소)");
            Assert.AreEqual(3, pool.CreatedCount, "파괴가 아니라 재사용 대기");

            // 전멸
            foreach (var m in new List<Monster>(pack.members))
                m.Health.TakeDamage(99f, DamageType.Normal);
            for (int f = 0; f < 5; f++) yield return null;
            Assert.IsTrue(pack == null, "전멸하면 군집이 디스폰돼야 한다");
            Assert.AreEqual(0, pool.ActiveCount, "전원 풀 반환");
            Assert.AreEqual(3, pool.CreatedCount, "여전히 재사용 대기 3(파괴 X)");

            Object.DestroyImmediate(holder);
            Object.DestroyImmediate(poolRoot);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(def);
        }
    }
}
