using System.Collections.Generic;
using NUnit.Framework;
using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Tests
{
    /// <summary>
    /// 군집 boids 조향 순수 로직(<see cref="BoidsSteering"/>)에 대한 EditMode 단위 테스트.
    /// 응집은 앵커로, 분리는 이웃 반대로, 합력은 maxSpeed 로 클램프됨을 고정한다.
    /// </summary>
    public class BoidsSteeringTests
    {
        const float Eps = 1e-4f;

        static List<Vector3> N(params Vector3[] v) => new List<Vector3>(v);

        // --- 응집 (Cohesion) ---

        [Test]
        public void CohesionDirection_PointsTowardAnchor()
        {
            var dir = BoidsSteering.CohesionDirection(Vector3.zero, new Vector3(10f, 0f, 0f));
            Assert.AreEqual(1f, dir.x, Eps);
            Assert.AreEqual(0f, dir.z, Eps);
        }

        [Test]
        public void CohesionDirection_IsUnitLength()
        {
            var dir = BoidsSteering.CohesionDirection(Vector3.zero, new Vector3(3f, 0f, 4f));
            Assert.AreEqual(1f, dir.magnitude, Eps);
        }

        [Test]
        public void CohesionDirection_AtAnchor_IsZero()
        {
            var dir = BoidsSteering.CohesionDirection(new Vector3(5f, 0f, 5f), new Vector3(5f, 0f, 5f));
            Assert.AreEqual(Vector3.zero, dir);
        }

        // --- 분리 (Separation) ---

        [Test]
        public void Separation_NoNeighbors_IsZero()
        {
            var push = BoidsSteering.Separation(Vector3.zero, N(), 2f);
            Assert.AreEqual(Vector3.zero, push);
        }

        [Test]
        public void Separation_NeighborOutsideRadius_IsZero()
        {
            var push = BoidsSteering.Separation(Vector3.zero, N(new Vector3(5f, 0f, 0f)), 2f);
            Assert.AreEqual(Vector3.zero, push);
        }

        [Test]
        public void Separation_PushesAwayFromNeighbor()
        {
            // 오른쪽(+x)에 이웃 → 왼쪽(-x)으로 밀려야 한다.
            var push = BoidsSteering.Separation(Vector3.zero, N(new Vector3(1f, 0f, 0f)), 2f);
            Assert.Less(push.x, 0f);
            Assert.AreEqual(0f, push.z, Eps);
        }

        [Test]
        public void Separation_StrongerWhenCloser()
        {
            var near = BoidsSteering.Separation(Vector3.zero, N(new Vector3(0.5f, 0f, 0f)), 2f);
            var far = BoidsSteering.Separation(Vector3.zero, N(new Vector3(1.5f, 0f, 0f)), 2f);
            Assert.Greater(near.magnitude, far.magnitude);
        }

        [Test]
        public void Separation_SymmetricNeighbors_Cancel()
        {
            var push = BoidsSteering.Separation(Vector3.zero, N(new Vector3(1f, 0f, 0f), new Vector3(-1f, 0f, 0f)), 2f);
            Assert.AreEqual(0f, push.x, Eps);
            Assert.AreEqual(0f, push.z, Eps);
        }

        // --- 합력 (DesiredVelocity) ---

        [Test]
        public void DesiredVelocity_NoNeighbors_HeadsToAnchorAtMaxSpeed()
        {
            var s = BoidsSettings.Default;
            s.MaxSpeed = 5f;
            s.CohesionWeight = 1f;
            var v = BoidsSteering.DesiredVelocity(Vector3.zero, new Vector3(100f, 0f, 0f), N(), s);
            Assert.AreEqual(5f, v.magnitude, 1e-3f, "이웃 없으면 앵커로 최고속");
            Assert.Greater(v.x, 0f);
            Assert.AreEqual(0f, v.z, Eps);
        }

        [Test]
        public void DesiredVelocity_ClampedToMaxSpeed()
        {
            var s = BoidsSettings.Default;
            s.MaxSpeed = 3f;
            // 먼 앵커 + 아주 가까운 이웃 → 큰 합력이지만 maxSpeed 로 잘려야 한다.
            var v = BoidsSteering.DesiredVelocity(Vector3.zero, new Vector3(100f, 0f, 0f),
                N(new Vector3(0.1f, 0f, 0f)), s);
            Assert.LessOrEqual(v.magnitude, 3f + 1e-3f);
        }

        [Test]
        public void DesiredVelocity_CloseNeighbor_PushesOffAnchorAxis()
        {
            var s = BoidsSettings.Default;
            // 앵커는 +x, 이웃은 바로 +z 옆 → 결과는 +x(응집) & -z(분리) 성분을 가져야.
            var v = BoidsSteering.DesiredVelocity(Vector3.zero, new Vector3(10f, 0f, 0f),
                N(new Vector3(0f, 0f, 0.5f)), s);
            Assert.Greater(v.x, 0f, "앵커(+x)로 향함");
            Assert.Less(v.z, 0f, "이웃(+z) 반대(-z)로 밀림");
        }
    }
}
