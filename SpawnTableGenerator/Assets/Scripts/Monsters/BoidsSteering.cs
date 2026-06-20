using System.Collections.Generic;
using UnityEngine;

namespace SpawnSystem.Monsters
{
    /// <summary>군집 boids 추종 파라미터.</summary>
    public struct BoidsSettings
    {
        public float CohesionWeight;    // 앵커로 모이는 가중치
        public float SeparationWeight;  // 서로 밀어내는 가중치
        public float SeparationRadius;  // 이 거리 안의 이웃만 밀어냄
        public float MaxSpeed;          // desired velocity 상한

        public static BoidsSettings Default => new BoidsSettings
        {
            CohesionWeight = 1f,
            SeparationWeight = 1.5f,
            SeparationRadius = 2f,
            MaxSpeed = 5f,
        };
    }

    /// <summary>
    /// 군집 멤버의 boids 조향(응집/분리)을 계산하는 순수 로직.
    /// NavMesh/Transform/MonoBehaviour 의존이 없어 EditMode 단위 테스트로 검증한다(움직임 TDD).
    /// 모든 계산은 XZ 평면(높이 무시).
    /// </summary>
    public static class BoidsSteering
    {
        /// <summary>앵커로 향하는 단위 방향(XZ). 이미 앵커 위면 0.</summary>
        public static Vector3 CohesionDirection(Vector3 self, Vector3 anchor)
        {
            Vector3 d = anchor - self;
            d.y = 0f;
            return d.sqrMagnitude > 1e-6f ? d.normalized : Vector3.zero;
        }

        /// <summary>
        /// 반경 내 이웃들로부터 밀려나는 합력. 각 이웃은 (이웃 반대 단위방향 × 근접도(0~1))로 기여하며,
        /// 가까울수록 강하다. 이웃이 없거나 모두 반경 밖이면 0.
        /// </summary>
        public static Vector3 Separation(Vector3 self, IReadOnlyList<Vector3> neighbors, float radius)
        {
            Vector3 push = Vector3.zero;
            if (neighbors == null || radius <= 0f)
                return push;

            for (int i = 0; i < neighbors.Count; i++)
            {
                Vector3 away = self - neighbors[i];
                away.y = 0f;
                float dist = away.magnitude;
                if (dist > 1e-6f && dist < radius)
                    push += away / dist * (1f - dist / radius);
            }
            return push;
        }

        /// <summary>
        /// 응집(앵커로) + 분리(이웃 반대)를 가중 합산해 maxSpeed 로 클램프한 desired velocity.
        /// 이웃이 없으면 앵커 방향으로 maxSpeed(가중치 1 기준).
        /// </summary>
        public static Vector3 DesiredVelocity(Vector3 self, Vector3 anchor, IReadOnlyList<Vector3> neighbors, BoidsSettings settings)
        {
            Vector3 cohesion = CohesionDirection(self, anchor) * (settings.MaxSpeed * settings.CohesionWeight);
            Vector3 separation = Separation(self, neighbors, settings.SeparationRadius) * (settings.MaxSpeed * settings.SeparationWeight);

            Vector3 steer = cohesion + separation;
            if (steer.magnitude > settings.MaxSpeed)
                steer = steer.normalized * settings.MaxSpeed;
            return steer;
        }
    }
}
