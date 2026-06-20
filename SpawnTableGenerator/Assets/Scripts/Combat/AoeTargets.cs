using System.Collections.Generic;
using UnityEngine;

namespace SpawnSystem.Combat
{
    /// <summary>범위 공격 타겟 선정(순수, XZ 평면). 폭발/근접 휘두르기 등에 공용.</summary>
    public static class AoeTargets
    {
        public static List<int> InRadius(Vector3 center, float radius, IReadOnlyList<Vector3> positions)
        {
            var result = new List<int>();
            if (positions == null || radius <= 0f)
                return result;

            float r2 = radius * radius;
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 d = positions[i] - center;
                d.y = 0f;
                if (d.sqrMagnitude <= r2)
                    result.Add(i);
            }
            return result;
        }
    }
}
