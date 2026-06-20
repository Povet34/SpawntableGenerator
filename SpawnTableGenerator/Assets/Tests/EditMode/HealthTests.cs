using NUnit.Framework;
using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Tests
{
    /// <summary>체력/데미지(<see cref="Health"/>) EditMode 테스트. 장갑/약점은 DamageResolver 경유.</summary>
    public class HealthTests
    {
        const float Eps = 1e-4f;

        static Health Make(float hp, DefenseProfile def = null)
        {
            var go = new GameObject("HP");
            var h = go.AddComponent<Health>();
            h.Init(hp, def);
            return h;
        }

        static DefenseProfile Heavy(DamageType vuln, float wpMult = 2f)
        {
            var d = ScriptableObject.CreateInstance<DefenseProfile>();
            d.armor = MonsterArmor.Heavy;
            d.vulnerableTo = vuln;
            d.weakPointMultiplier = wpMult;
            return d;
        }

        [Test]
        public void TakeDamage_ReducesHP()
        {
            var h = Make(10f);
            float dealt = h.TakeDamage(3f, DamageType.Normal);
            Assert.AreEqual(3f, dealt, Eps);
            Assert.AreEqual(7f, h.Current, Eps);
            Object.DestroyImmediate(h.gameObject);
        }

        [Test]
        public void Dies_WhenHpHitsZero()
        {
            var h = Make(5f);
            h.TakeDamage(5f, DamageType.Normal);
            Assert.IsTrue(h.IsDead);
            Object.DestroyImmediate(h.gameObject);
        }

        [Test]
        public void HeavyArmor_ImmuneToNonVulnerable_NoHpLoss()
        {
            var def = Heavy(DamageType.Piercing);
            var h = Make(10f, def);
            float dealt = h.TakeDamage(5f, DamageType.Normal);
            Assert.AreEqual(0f, dealt, Eps);
            Assert.AreEqual(10f, h.Current, Eps);
            Object.DestroyImmediate(h.gameObject);
            Object.DestroyImmediate(def);
        }

        [Test]
        public void HeavyArmor_TakesVulnerableType()
        {
            var def = Heavy(DamageType.Piercing);
            var h = Make(10f, def);
            h.TakeDamage(4f, DamageType.Piercing);
            Assert.AreEqual(6f, h.Current, Eps);
            Object.DestroyImmediate(h.gameObject);
            Object.DestroyImmediate(def);
        }

        [Test]
        public void WeakPointHit_IsMultiplied()
        {
            var def = Heavy(DamageType.Piercing, wpMult: 2f);
            var h = Make(10f, def);
            float dealt = h.TakeDamage(3f, DamageType.Normal, hitWeakPoint: true);
            Assert.AreEqual(6f, dealt, Eps); // 3 * 2 (약점은 장갑 무시)
            Assert.AreEqual(4f, h.Current, Eps);
            Object.DestroyImmediate(h.gameObject);
            Object.DestroyImmediate(def);
        }

        [Test]
        public void DeadHealth_IgnoresFurtherDamage()
        {
            var h = Make(2f);
            h.TakeDamage(5f, DamageType.Normal);
            float dealt = h.TakeDamage(5f, DamageType.Normal);
            Assert.AreEqual(0f, dealt, Eps);
            Assert.AreEqual(0f, h.Current, Eps);
            Object.DestroyImmediate(h.gameObject);
        }
    }
}
