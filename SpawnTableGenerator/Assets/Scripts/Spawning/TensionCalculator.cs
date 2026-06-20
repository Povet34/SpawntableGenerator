using UnityEngine;

namespace SpawnSystem.Spawning
{
    /// <summary>
    /// 긴장도/스폰 페이싱 순수 로직(설계 §6). 경과시간 + 목표 진행 → 0~1 긴장도 → 스폰 간격 lerp.
    /// </summary>
    public static class TensionCalculator
    {
        /// <summary>
        /// intensity = wTime·(경과/최대) + wObj·(1 − 남은/전체), 0~1 clamp.
        /// 목표를 깰수록(남은↓) 긴장도↑.
        /// </summary>
        public static float Intensity(float elapsed, float maxTime, int objectivesRemaining, int objectivesTotal, float wTime, float wObj)
        {
            float t = maxTime > 1e-4f ? Mathf.Clamp01(elapsed / maxTime) : 0f;
            float o = objectivesTotal > 0 ? Mathf.Clamp01(1f - (float)objectivesRemaining / objectivesTotal) : 0f;
            return Mathf.Clamp01(wTime * t + wObj * o);
        }

        /// <summary>긴장도로 스폰 간격을 lerp(긴장↑ → 간격↓). 하한은 minInterval.</summary>
        public static float SpawnInterval(float intensity, float maxInterval, float minInterval)
        {
            return Mathf.Lerp(maxInterval, minInterval, Mathf.Clamp01(intensity));
        }
    }
}
