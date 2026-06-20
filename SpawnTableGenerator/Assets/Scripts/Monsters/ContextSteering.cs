using System.Collections.Generic;
using UnityEngine;

namespace SpawnSystem.Monsters
{
    /// <summary>
    /// 컨텍스트 스티어링(설계 §12.4): D개 방향을 interest−danger 로 점수화해 최적 이동 방향을 고른다.
    /// 길찾기/점 샘플링 없이 방향만 평가 → 가볍고 확장적. 시야/벽/지형 질의는 델리게이트로 주입해 순수 유지.
    /// </summary>
    public static class ContextSteering
    {
        /// <param name="self">현재 위치</param>
        /// <param name="currentHeading">현재 진행 방향(관성용, 0이면 무시)</param>
        /// <param name="playerPos">선호 거리 계산 대상</param>
        /// <param name="preferredRange">선호 교전 거리(min,max)</param>
        /// <param name="neighbors">이웃 위치(밀집 회피)</param>
        /// <param name="profile">가중치/비용</param>
        /// <param name="pressureAt">후보 지점의 플레이어 시야 압박(0~1). null 허용</param>
        /// <param name="blockedAt">후보 지점의 벽/지형 패널티(0~1, 막힘=1). null 허용</param>
        /// <returns>정규화된 이동 방향(XZ).</returns>
        public static Vector3 ChooseDirection(
            Vector3 self,
            Vector3 currentHeading,
            Vector3 playerPos,
            Vector2 preferredRange,
            IReadOnlyList<Vector3> neighbors,
            MovementProfile profile,
            System.Func<Vector3, float> pressureAt,
            System.Func<Vector3, float> blockedAt,
            int dirCount = 12,
            float step = 1.5f)
        {
            if (dirCount < 4)
                dirCount = 4;

            Vector3 heading = new Vector3(currentHeading.x, 0f, currentHeading.z);
            bool hasHeading = heading.sqrMagnitude > 1e-6f;
            if (hasHeading)
                heading.Normalize();

            float best = float.NegativeInfinity;
            Vector3 bestDir = hasHeading ? heading : Vector3.forward;

            for (int i = 0; i < dirCount; i++)
            {
                float a = i / (float)dirCount * Mathf.PI * 2f;
                Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Vector3 cand = self + dir * step;

                float danger = 0f;
                if (pressureAt != null) danger += profile.wViewAvoid * pressureAt(cand);
                if (blockedAt != null) danger += blockedAt(cand);  // 막힘은 그대로 큰 패널티
                danger += profile.wNeighborSpacing * NeighborDanger(cand, neighbors);

                float interest = profile.wPreferredDist * DistInterest(Flat(playerPos - cand).magnitude, preferredRange);
                if (hasHeading)
                    interest += profile.wInertia * Mathf.Max(0f, Vector3.Dot(dir, heading));

                float score = interest - danger;
                if (score > best)
                {
                    best = score;
                    bestDir = dir;
                }
            }
            return bestDir;
        }

        static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v;
        }

        /// <summary>
        /// 선호 거리대[min,max] 안이면 0(최고), 밖이면 벗어난 만큼 음수(단조). clamp 없이 기울기를 유지해
        /// 아주 멀거나 가까워도 밴드 쪽으로 끌리게 한다. (argmax 비교용이라 0~1 정규화 불필요.)
        /// </summary>
        static float DistInterest(float dist, Vector2 range)
        {
            float min = range.x, max = range.y;
            if (dist < min)
                return -(min - dist);
            if (dist > max)
                return -(dist - max);
            return 0f;
        }

        /// <summary>후보 지점이 이웃과 가까울수록 커지는 위험(반경 2 안에서 근접도 합).</summary>
        static float NeighborDanger(Vector3 cand, IReadOnlyList<Vector3> neighbors)
        {
            if (neighbors == null)
                return 0f;
            float d = 0f;
            for (int i = 0; i < neighbors.Count; i++)
            {
                Vector3 v = neighbors[i] - cand;
                v.y = 0f;
                float dist = v.magnitude;
                if (dist > 1e-3f && dist < 2f)
                    d += 1f - dist / 2f;
            }
            return d;
        }
    }
}
