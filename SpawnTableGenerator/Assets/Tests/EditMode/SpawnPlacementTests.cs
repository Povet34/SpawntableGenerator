using NUnit.Framework;
using SpawnSystem.Spawning;
using UnityEngine;

namespace SpawnSystem.Tests
{
    /// <summary>예측 스폰 배치(<see cref="SpawnPlacement"/>) EditMode 테스트.</summary>
    public class SpawnPlacementTests
    {
        const float Eps = 1e-4f;

        [Test]
        public void Predict_LeadsAlongVelocity()
        {
            var r = SpawnPlacement.Predict(Vector3.zero, new Vector3(2f, 0f, 0f), 1.5f);
            Assert.AreEqual(3f, r.x, Eps); // 0 + 2*1.5
            Assert.AreEqual(0f, r.z, Eps);
        }

        [Test]
        public void Predict_ZeroVelocity_ReturnsCurrent()
        {
            var r = SpawnPlacement.Predict(new Vector3(5f, 0f, 7f), Vector3.zero, 2f);
            Assert.AreEqual(5f, r.x, Eps);
            Assert.AreEqual(7f, r.z, Eps);
        }

        [Test]
        public void Predict_IgnoresY()
        {
            var r = SpawnPlacement.Predict(new Vector3(0f, 5f, 0f), new Vector3(0f, 9f, 1f), 1f);
            Assert.AreEqual(0f, r.y, Eps);
            Assert.AreEqual(1f, r.z, Eps); // z velocity 1 * 1
        }
    }
}
