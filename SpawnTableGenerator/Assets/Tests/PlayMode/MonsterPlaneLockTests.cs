using System.Collections;
using NUnit.Framework;
using SpawnSystem.Monsters;
using SpawnSystem.Spawning;
using UnityEngine;
using UnityEngine.TestTools;

namespace SpawnSystem.Tests
{
    /// <summary>이 게임은 XZ 평면(y축 없음). 멤버가 머리 위로 올라가지 않고 바닥 높이에 고정됨을 검증.</summary>
    public class MonsterPlaneLockTests
    {
        [UnityTest]
        public IEnumerator Members_StayOnXZPlane_NoYRise()
        {
            using var env = NavTestEnvironment.WithCenterWall(60f, new Vector3(0f, 1.5f, 30f), new Vector3(1f, 3f, 1f));

            var def = ScriptableObject.CreateInstance<MonsterDef>();
            def.scale = 1f; def.moveSpeed = 4f; def.color = Color.red; def.preferredRange = new Vector2(2f, 5f);

            var player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 0f, 5f);
            var holder = new GameObject("Holder");

            var pack = PackFactory.BuildFromDef(def, 3, Vector3.zero, player.transform, holder.transform, null);
            pack.useFsm = false; // 항상 교전 → 멤버 이동
            yield return null;

            // 인위적으로 Y 를 띄워본다 → 잠금이 되돌려야 함.
            foreach (var m in pack.members)
            {
                var p = m.transform.position; p.y += 5f; m.transform.position = p;
            }
            for (int f = 0; f < 30; f++) yield return null; // Update → Step → Y 잠금

            foreach (var m in pack.members)
                Assert.Less(Mathf.Abs(m.transform.position.y - 1f), 0.3f, "멤버 Y 가 바닥(1)으로 고정돼야 한다");

            Object.DestroyImmediate(holder);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(def);
        }
    }
}
