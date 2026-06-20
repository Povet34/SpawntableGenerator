using System.Collections.Generic;
using UnityEngine;

namespace SpawnSystem.Combat
{
    /// <summary>범위 공격 타겟 선정(순수, XZ 평면). 폭발/근접 휘두르기 등에 공용.</summary>
    public static class AoeTargets
    {
        /// <summary>XZ 평면 부채꼴 — center 기준 forward 방향 arcDeg° 범위.</summary>
        public static List<int> InArc(Vector3 center, Vector3 forward, float radius, float arcDeg, IReadOnlyList<Vector3> positions)
        {
            var result = new List<int>();
            if (positions == null || radius <= 0f || arcDeg <= 0f) return result;

            float r2 = radius * radius;
            Vector3 fwd = new Vector3(forward.x, 0f, forward.z);
            if (fwd.sqrMagnitude < 1e-6f) return result;
            fwd.Normalize();
            float cosHalf = Mathf.Cos(arcDeg * 0.5f * Mathf.Deg2Rad);

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 d = positions[i] - center;
                d.y = 0f;
                if (d.sqrMagnitude > r2) continue;
                if (d.sqrMagnitude < 1e-6f) { result.Add(i); continue; } // 발 아래
                if (Vector3.Dot(d.normalized, fwd) >= cosHalf) result.Add(i);
            }
            return result;
        }

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
