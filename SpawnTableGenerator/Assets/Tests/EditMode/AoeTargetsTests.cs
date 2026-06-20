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
