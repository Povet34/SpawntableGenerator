using NUnit.Framework;
using SpawnSystem.Monsters;
using UnityEditor;

namespace SpawnSystem.Tests
{
    /// <summary>
    /// 로스터 에셋(Tools/Spawn System/Create Sample Monster Defs 로 생성)의 존재 + 3-프로필 참조 +
    /// Monsters.md 규칙(중장갑/도망/특수능력/공격수)을 고정한다.
    /// </summary>
    public class MonsterDefAssetsTests
    {
        static readonly string[] Roster =
        {
            "MD_Melee_Small", "MD_Melee_SmallJumper", "MD_Melee_Medium", "MD_Melee_MediumBurrower",
            "MD_Melee_LargeHeavy", "MD_Ranged_Small", "MD_Ranged_MediumExplosive", "MD_Ranged_LargeArtillery",
        };

        static MonsterDef Load(string name) =>
            AssetDatabase.LoadAssetAtPath<MonsterDef>($"Assets/GameData/Monsters/{name}.asset");

        [Test]
        public void AllRosterDefs_Exist_WithThreeProfiles()
        {
            foreach (var n in Roster)
            {
                var d = Load(n);
                Assert.IsNotNull(d, $"{n} 에셋이 있어야 한다 (메뉴로 생성)");
                Assert.IsNotNull(d.movement, $"{n} 은 MovementProfile 참조를 가져야 한다");
                Assert.IsNotNull(d.defense, $"{n} 은 DefenseProfile 참조를 가져야 한다");
                Assert.IsNotNull(d.attack, $"{n} 은 AttackProfile 참조를 가져야 한다");
            }
        }

        [Test]
        public void HeavyMonsters_AreHeavyArmor_AndDoNotFlee()
        {
            foreach (var n in new[] { "MD_Melee_LargeHeavy", "MD_Ranged_MediumExplosive", "MD_Ranged_LargeArtillery" })
            {
                var d = Load(n);
                Assert.AreEqual(MonsterArmor.Heavy, d.defense.armor, $"{n} 은 중장갑이어야 한다");
                Assert.IsFalse(d.canFlee, $"{n}(중장갑)은 도망치지 않아야 한다");
            }
        }

        [Test]
        public void Artillery_HasTwoAttacks()
        {
            var d = Load("MD_Ranged_LargeArtillery");
            Assert.AreEqual(2, d.attack.attacks.Length, "포대형은 폭발+기관총 = 공격 2개");
        }

        [Test]
        public void Jumper_HasLeap_Burrower_HasBurrow()
        {
            Assert.IsTrue((Load("MD_Melee_SmallJumper").abilities & MonsterAbility.Leap) != 0, "점프형 = Leap");
            Assert.IsTrue((Load("MD_Melee_MediumBurrower").abilities & MonsterAbility.Burrow) != 0, "잠복형 = Burrow");
        }

        [Test]
        public void Swarm_IsSmallerAndFasterThanLargeHeavy()
        {
            var small = Load("MD_Melee_Small");
            var large = Load("MD_Melee_LargeHeavy");
            Assert.Greater(small.moveSpeed, large.moveSpeed, "작은놈이 큰놈보다 빨라야");
            Assert.Less(small.scale, large.scale, "작은놈이 큰놈보다 작아야");
        }

        [Test]
        public void RangedSmall_KeepsDistance()
        {
            var d = Load("MD_Ranged_Small");
            Assert.GreaterOrEqual(d.preferredRange.x, 6f, "원거리 키터는 선호 거리가 멀어야");
            Assert.IsTrue(d.canFlee, "원거리 작은놈은 겁이 많아 도망 가능");
        }
    }
}
