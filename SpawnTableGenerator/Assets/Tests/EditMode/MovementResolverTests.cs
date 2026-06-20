using NUnit.Framework;
using SpawnSystem.Player;
using UnityEngine;

namespace SpawnSystem.Tests
{
    /// <summary>
    /// 클릭 이동의 순수 기하 로직(<see cref="MovementResolver"/>)에 대한 EditMode 단위 테스트.
    /// NavMesh/Input/씬 없이 빠르게 돌아가며, 움직임 동작의 회귀를 막는다.
    /// </summary>
    public class MovementResolverTests
    {
        const float Eps = 1e-4f;
        const float Hug = 0.6f; // 보통 에이전트 반경(0.5) + 0.1

        // --- 바닥/윗면(법선이 위쪽): 표면점을 그대로 사용 ---

        [Test]
        public void FloorClick_NormalUp_ReturnsPointUnchanged()
        {
            var hit = new Vector3(3f, 0f, 5f);
            var result = MovementResolver.ResolveClickTarget(hit, Vector3.up, 0f, Hug);
            Assert.AreEqual(hit, result);
        }

        [Test]
        public void WallTopClick_NormalUp_ReturnsPointUnchanged()
        {
            // 벽 '위'를 찍음 → 못 가는 곳이지만 여기선 그대로 두고(이후 NavMesh가 가장 가까운 가장자리로 처리)
            var hit = new Vector3(4f, 3f, 1f);
            var result = MovementResolver.ResolveClickTarget(hit, Vector3.up, 0f, Hug);
            Assert.AreEqual(hit, result);
        }

        // --- 수직 벽면(법선이 수평): 바닥 투영 + 법선 바깥으로 밀어 밀착 ---

        [Test]
        public void VerticalWallFace_ProjectsToGroundHeight()
        {
            var hit = new Vector3(10f, 1.5f, 2f);
            var result = MovementResolver.ResolveClickTarget(hit, Vector3.right, 0f, Hug);
            Assert.AreEqual(0f, result.y, Eps, "수직 벽면 클릭은 바닥 높이로 내려야 한다");
        }

        [Test]
        public void VerticalWallFace_PushesOutAlongPositiveXNormal()
        {
            var hit = new Vector3(10f, 1.5f, 2f);
            var result = MovementResolver.ResolveClickTarget(hit, Vector3.right, 0f, Hug);
            Assert.AreEqual(10f + Hug, result.x, Eps, "법선(+X) 바깥으로 hugOffset 만큼 밀어야 한다");
            Assert.AreEqual(2f, result.z, Eps);
        }

        [Test]
        public void VerticalWallFace_PushesOutAlongNegativeZNormal()
        {
            var hit = new Vector3(0f, 2f, 7f);
            var result = MovementResolver.ResolveClickTarget(hit, Vector3.back, 0f, Hug); // (0,0,-1)
            Assert.AreEqual(0f, result.y, Eps);
            Assert.AreEqual(7f - Hug, result.z, Eps);
        }

        [Test]
        public void GroundHeightOffset_IsRespected()
        {
            var hit = new Vector3(0f, 5f, 0f);
            var result = MovementResolver.ResolveClickTarget(hit, Vector3.right, 1.5f, Hug);
            Assert.AreEqual(1.5f, result.y, Eps, "groundHeight 가 0이 아니면 그 높이로 내려야 한다");
        }

        // --- 임계값(threshold) 경계 동작 ---

        [Test]
        public void NearlyHorizontalNormal_BelowThreshold_TreatedAsVertical()
        {
            var normal = new Vector3(1f, 0.4f, 0f).normalized; // |y| ~ 0.37 < 0.5
            var hit = new Vector3(0f, 2f, 0f);
            var result = MovementResolver.ResolveClickTarget(hit, normal, 0f, Hug);
            Assert.AreEqual(0f, result.y, Eps, "임계값 미만이면 수직면으로 보고 바닥 투영");
        }

        [Test]
        public void SteepNormal_AboveThreshold_TreatedAsHorizontal()
        {
            var normal = new Vector3(0.8f, 0.6f, 0f).normalized; // |y| = 0.6 > 0.5
            var hit = new Vector3(0f, 2f, 0f);
            var result = MovementResolver.ResolveClickTarget(hit, normal, 0f, Hug);
            Assert.AreEqual(hit, result, "임계값 이상이면 윗면으로 보고 그대로 둔다");
        }

        [Test]
        public void IsVerticalFace_ClassifiesNormals()
        {
            Assert.IsTrue(MovementResolver.IsVerticalFace(Vector3.right));
            Assert.IsTrue(MovementResolver.IsVerticalFace(Vector3.forward));
            Assert.IsFalse(MovementResolver.IsVerticalFace(Vector3.up));
            Assert.IsFalse(MovementResolver.IsVerticalFace(Vector3.down));
        }
    }
}
