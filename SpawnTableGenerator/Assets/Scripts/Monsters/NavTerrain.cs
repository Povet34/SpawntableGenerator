using UnityEngine;
using UnityEngine.AI;

namespace SpawnSystem.Monsters
{
    /// <summary>
    /// navmesh area cost 기반 지형 인지(설계 §12 — 물/진흙 속도 저하·국소 우회). 지금은 area 가 전부
    /// walkable(cost 1)이라 효과가 1.0/막힘만이지만, 물/진흙 area 가 베이크되면 자동 반영되는 hook.
    /// (지역 전체 우회·큰 장애물 우회는 앵커의 길찾기가 담당 — 여긴 국소만.)
    /// </summary>
    public static class NavTerrain
    {
        /// <summary>pos 가 선 area 의 cost 로 속도 배율(cost↑ → 느림). navmesh 밖이면 1.</summary>
        public static float SpeedMultiplier(Vector3 pos, float sampleRadius = 1.5f)
        {
            if (NavMesh.SamplePosition(pos, out var hit, sampleRadius, NavMesh.AllAreas))
            {
                float cost = AreaCost(hit.mask);
                return cost > 0f ? 1f / cost : 1f;
            }
            return 1f;
        }

        /// <summary>
        /// from→to 직선의 패널티(0~1). navmesh 가장자리/벽/장애물에 막히거나 navmesh 밖이면 1(완전 회피),
        /// 아니면 도착 지점 area cost 에 비례한 소량 패널티(비싼 지형 약하게 회피).
        /// </summary>
        public static float PointPenalty(Vector3 from, Vector3 to)
        {
            if (NavMesh.Raycast(from, to, out _, NavMesh.AllAreas))
                return 1f;
            if (NavMesh.SamplePosition(to, out var hit, 0.5f, NavMesh.AllAreas))
                return Mathf.Clamp01((AreaCost(hit.mask) - 1f) * 0.25f);
            return 1f; // navmesh 밖
        }

        static float AreaCost(int areaMask)
        {
            int area = LowestSetBit(areaMask);
            return area >= 0 ? NavMesh.GetAreaCost(area) : 1f;
        }

        static int LowestSetBit(int mask)
        {
            for (int i = 0; i < 32; i++)
                if ((mask & (1 << i)) != 0)
                    return i;
            return -1;
        }
    }
}
