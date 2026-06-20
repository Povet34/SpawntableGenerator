using System.Collections.Generic;
using NUnit.Framework;
using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Tests
{
    /// <summary>컨텍스트 스티어링 순수 로직(<see cref="ContextSteering"/>) EditMode 테스트. 설계 §12.4.</summary>
    public class ContextSteeringTests
    {
        static MovementProfile Profile()
        {
            var p = ScriptableObject.CreateInstance<MovementProfile>();
            p.wViewAvoid = 1.5f;
            p.wPreferredDist = 1f;
            p.wNeighborSpacing = 0.6f;
            p.wInertia = 0.4f;
            return p;
        }

        static readonly List<Vector3> NoNeighbors = new List<Vector3>();

        [Test]
        public void AvoidsHighPressureDirection()
        {
            var prof = Profile();
            // +x 쪽 후보는 시야 압박 1, 나머지 0 → +x 회피.
            System.Func<Vector3, float> pressure = c => c.x > 0.1f ? 1f : 0f;
            var dir = ContextSteering.ChooseDirection(Vector3.zero, Vector3.zero, new Vector3(0f, 0f, 30f),
                new Vector2(0f, 2f), NoNeighbors, prof, pressure, null);
            Assert.LessOrEqual(dir.x, 0.01f, "시야 압박이 큰 +x 방향을 피해야 한다");
            Object.DestroyImmediate(prof);
        }

        [Test]
        public void AvoidsBlockedDirection()
        {
            var prof = Profile();
            System.Func<Vector3, float> blocked = c => c.x > 0.1f ? 1f : 0f; // +x 가 벽
            var dir = ContextSteering.ChooseDirection(Vector3.zero, Vector3.zero, new Vector3(0f, 0f, 30f),
                new Vector2(0f, 2f), NoNeighbors, prof, null, blocked);
            Assert.LessOrEqual(dir.x, 0.01f, "막힌 +x 방향을 피해야 한다");
            Object.DestroyImmediate(prof);
        }

        [Test]
        public void SeeksPlayer_WhenTooFar()
        {
            var prof = Profile();
            // 플레이어 +x 멀리, 선호 거리 가까움 → +x(플레이어 쪽)로 향함.
            var dir = ContextSteering.ChooseDirection(Vector3.zero, Vector3.zero, new Vector3(10f, 0f, 0f),
                new Vector2(0f, 2f), NoNeighbors, prof, null, null);
            Assert.Greater(dir.x, 0f, "너무 멀면 플레이어(+x) 쪽으로 가야 한다");
            Object.DestroyImmediate(prof);
        }

        [Test]
        public void MovesAway_WhenTooClose()
        {
            var prof = Profile();
            // 플레이어 바로 +x 옆(0.5), 선호 거리 멀게(3~5) → 멀어지는 -x 선호.
            var dir = ContextSteering.ChooseDirection(Vector3.zero, Vector3.zero, new Vector3(0.5f, 0f, 0f),
                new Vector2(3f, 5f), NoNeighbors, prof, null, null);
            Assert.Less(dir.x, 0f, "너무 가까우면 플레이어 반대(-x)로 가야 한다");
            Object.DestroyImmediate(prof);
        }

        [Test]
        public void ReturnsUnitDirection()
        {
            var prof = Profile();
            var dir = ContextSteering.ChooseDirection(Vector3.zero, Vector3.forward, new Vector3(5f, 0f, 0f),
                new Vector2(0f, 2f), NoNeighbors, prof, null, null);
            Assert.AreEqual(1f, dir.magnitude, 1e-3f);
            Assert.AreEqual(0f, dir.y, 1e-4f);
            Object.DestroyImmediate(prof);
        }
    }
}
