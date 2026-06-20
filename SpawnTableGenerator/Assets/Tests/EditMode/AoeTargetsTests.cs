using System.Collections.Generic;
using NUnit.Framework;
using SpawnSystem.Combat;
using UnityEngine;

namespace SpawnSystem.Tests
{
    /// <summary>범위 타겟 선정(<see cref="AoeTargets"/>) EditMode 테스트.</summary>
    public class AoeTargetsTests
    {
        static List<Vector3> P(params Vector3[] v) => new List<Vector3>(v);

        // ---- InArc ----

        [Test]
        public void InArc_FrontInRange_Included()
        {
            // 바로 앞 3m, 전방 90° 범위
            var hits = AoeTargets.InArc(Vector3.zero, Vector3.forward, 5f, 90f,
                P(new Vector3(0f, 0f, 3f)));
            Assert.AreEqual(1, hits.Count);
        }

        [Test]
        public void InArc_Behind_Excluded()
        {
            var hits = AoeTargets.InArc(Vector3.zero, Vector3.forward, 10f, 120f,
                P(new Vector3(0f, 0f, -3f)));
            Assert.AreEqual(0, hits.Count);
        }

        [Test]
        public void InArc_ExactEdgeAngle_Included()
        {
            // 120° 부채꼴 → halfAngle=60°. 정확히 60° 방향 타겟.
            float ang = 60f * Mathf.Deg2Rad;
            Vector3 target = new Vector3(Mathf.Sin(ang), 0f, Mathf.Cos(ang)) * 3f;
            var hits = AoeTargets.InArc(Vector3.zero, Vector3.forward, 5f, 120f, P(target));
            Assert.AreEqual(1, hits.Count);
        }

        [Test]
        public void InArc_OutsideRange_Excluded()
        {
            var hits = AoeTargets.InArc(Vector3.zero, Vector3.forward, 2f, 120f,
                P(new Vector3(0f, 0f, 5f)));
            Assert.AreEqual(0, hits.Count);
        }

        [Test]
        public void InArc_IgnoresY()
        {
            // 높이 차이가 있어도 XZ 거리/각도로만 판단
            var hits = AoeTargets.InArc(Vector3.zero, Vector3.forward, 5f, 120f,
                P(new Vector3(0f, 9f, 3f)));
            Assert.AreEqual(1, hits.Count);
        }

        [Test]
        public void InArc_ZeroRadiusOrArc_ReturnsEmpty()
        {
            Assert.AreEqual(0, AoeTargets.InArc(Vector3.zero, Vector3.forward, 0f, 120f, P(Vector3.forward)).Count);
            Assert.AreEqual(0, AoeTargets.InArc(Vector3.zero, Vector3.forward, 5f, 0f, P(Vector3.forward)).Count);
        }

        [Test]
        public void InRadius_SelectsWithin_ExcludesOutside()
        {
            var hits = AoeTargets.InRadius(Vector3.zero, 5f,
                P(new Vector3(3f, 0f, 0f), new Vector3(10f, 0f, 0f), new Vector3(0f, 0f, 4f)));
            CollectionAssert.AreEquivalent(new[] { 0, 2 }, hits);
        }

        [Test]
        public void InRadius_IgnoresY()
        {
            var hits = AoeTargets.InRadius(Vector3.zero, 2f, P(new Vector3(0f, 9f, 0f)));
            Assert.AreEqual(1, hits.Count);
        }

        [Test]
        public void InRadius_OnBoundary_Included()
        {
            var hits = AoeTargets.InRadius(Vector3.zero, 5f, P(new Vector3(5f, 0f, 0f)));
            Assert.AreEqual(1, hits.Count);
        }

        [Test]
        public void InRadius_EmptyOrZero_ReturnsEmpty()
        {
            Assert.AreEqual(0, AoeTargets.InRadius(Vector3.zero, 0f, P(Vector3.zero)).Count);
            Assert.AreEqual(0, AoeTargets.InRadius(Vector3.zero, 5f, P()).Count);
        }
    }
}
