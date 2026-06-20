using NUnit.Framework;
using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Tests
{
    /// <summary>플레이어 시야 압박 순수 로직(<see cref="ViewPressure"/>) EditMode 테스트. 설계 §12.2.</summary>
    public class ViewPressureTests
    {
        const float Range = 18f;
        const float Cone = 70f;

        [Test]
        public void Cone_DirectlyAhead_Close_IsHigh()
        {
            float p = ViewPressure.Cone(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 2f), Cone, Range);
            Assert.Greater(p, 0.5f);
        }

        [Test]
        public void Cone_Behind_IsZero()
        {
            float p = ViewPressure.Cone(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, -2f), Cone, Range);
            Assert.AreEqual(0f, p, 1e-4f);
        }

        [Test]
        public void Cone_BeyondRange_IsZero()
        {
            float p = ViewPressure.Cone(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 30f), Cone, Range);
            Assert.AreEqual(0f, p, 1e-4f);
        }

        [Test]
        public void Cone_CloserIsStronger()
        {
            float near = ViewPressure.Cone(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 2f), Cone, Range);
            float far = ViewPressure.Cone(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 12f), Cone, Range);
            Assert.Greater(near, far);
        }

        [Test]
        public void Cone_OutsideAngle_IsZero()
        {
            // 콘 70° → 반각 35°. 90° 옆(+x)은 콘 밖.
            float p = ViewPressure.Cone(Vector3.zero, Vector3.forward, new Vector3(5f, 0f, 0f), Cone, Range);
            Assert.AreEqual(0f, p, 1e-4f);
        }

        [Test]
        public void Total_FreshLastAttack_AddsPressureFromThatDirection()
        {
            // 플레이어는 +z 를 보지만, 직전 공격은 +x 로 함. 타깃은 +x(현재 forward 콘 밖).
            var target = new Vector3(3f, 0f, 0f);
            float p = ViewPressure.Total(Vector3.zero, Vector3.forward, Vector3.right, 0f, target, Cone, Range, 3f);
            Assert.Greater(p, 0f, "직전 공격 방향(+x) 때문에 압박이 있어야");
        }

        [Test]
        public void Total_OldLastAttack_DecaysToForwardOnly()
        {
            var target = new Vector3(3f, 0f, 0f);
            // lastAttackAge > memory → 감쇠 0 → forward 콘만(타깃은 콘 밖) → 0.
            float p = ViewPressure.Total(Vector3.zero, Vector3.forward, Vector3.right, 5f, target, Cone, Range, 3f);
            Assert.AreEqual(0f, p, 1e-4f);
        }
    }
}
