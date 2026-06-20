using NUnit.Framework;
using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Tests
{
    /// <summary>장갑/약점 데미지 규칙(<see cref="DamageResolver"/>) EditMode 테스트. Monsters.md §5.</summary>
    public class DamageResolverTests
    {
        const float Eps = 1e-4f;

        static DefenseProfile Def(MonsterArmor armor, DamageType vuln, bool reqWeak = false, float wpMult = 2f)
        {
            var d = ScriptableObject.CreateInstance<DefenseProfile>();
            d.armor = armor;
            d.vulnerableTo = vuln;
            d.requiresWeakPoint = reqWeak;
            d.weakPointMultiplier = wpMult;
            return d;
        }

        [Test]
        public void NonHeavy_NeverImmune()
        {
            var d = Def(MonsterArmor.Light, DamageType.Piercing);
            Assert.IsFalse(DamageResolver.IsImmune(d, DamageType.Normal, false));
            Assert.AreEqual(1f, DamageResolver.Multiplier(d, DamageType.Normal, false), Eps);
            Object.DestroyImmediate(d);
        }

        [Test]
        public void Heavy_ImmuneToNonVulnerableType()
        {
            var d = Def(MonsterArmor.Heavy, DamageType.Piercing);
            Assert.IsTrue(DamageResolver.IsImmune(d, DamageType.Normal, false));
            Assert.AreEqual(0f, DamageResolver.Multiplier(d, DamageType.Normal, false), Eps);
            Object.DestroyImmediate(d);
        }

        [Test]
        public void Heavy_TakesVulnerableType()
        {
            var d = Def(MonsterArmor.Heavy, DamageType.Piercing);
            Assert.IsFalse(DamageResolver.IsImmune(d, DamageType.Piercing, false));
            Assert.AreEqual(1f, DamageResolver.Multiplier(d, DamageType.Piercing, false), Eps);
            Object.DestroyImmediate(d);
        }

        [Test]
        public void WeakPointHit_AlwaysDamages_WithMultiplier()
        {
            var d = Def(MonsterArmor.Heavy, DamageType.Piercing, wpMult: 3f);
            Assert.IsFalse(DamageResolver.IsImmune(d, DamageType.Normal, true));
            Assert.AreEqual(3f, DamageResolver.Multiplier(d, DamageType.Normal, true), Eps);
            Object.DestroyImmediate(d);
        }

        [Test]
        public void RequiresWeakPoint_ImmuneToBodyHits()
        {
            var d = Def(MonsterArmor.Heavy, DamageType.Piercing, reqWeak: true);
            Assert.IsTrue(DamageResolver.IsImmune(d, DamageType.Piercing, false), "약점만 데미지 → 몸통은 면역");
            Assert.IsFalse(DamageResolver.IsImmune(d, DamageType.Normal, true), "약점 타격은 통함");
            Object.DestroyImmediate(d);
        }

        [Test]
        public void NullDefense_NotImmune()
        {
            Assert.IsFalse(DamageResolver.IsImmune(null, DamageType.Normal, false));
            Assert.AreEqual(1f, DamageResolver.Multiplier(null, DamageType.Normal, false), Eps);
        }
    }
}
