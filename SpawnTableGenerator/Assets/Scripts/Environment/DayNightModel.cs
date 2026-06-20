using UnityEngine;

namespace SpawnSystem.Environment
{
    /// <summary>
    /// 낮/밤 사이클의 순수 로직 시임(scene/MonoBehaviour 무관 → EditMode 단위 테스트).
    /// 시각(0..1)과 <see cref="DayNightConfig"/>만으로 <see cref="DayNightState"/>를 계산한다.
    /// 프로젝트의 다른 순수 로직(TensionCalculator, DamageResolver)과 동일한 패턴.
    /// </summary>
    public static class DayNightModel
    {
        /// <summary>경과 실시간(초)을 [0,1) 시각으로 변환. 시작 오프셋을 더하고 wrap.</summary>
        public static float NormalizedFromElapsed(float elapsedSeconds, float cycleSeconds, float startNormalizedTime)
        {
            if (cycleSeconds <= 0f) return Mathf.Repeat(startNormalizedTime, 1f);
            float t = startNormalizedTime + elapsedSeconds / cycleSeconds;
            return Mathf.Repeat(t, 1f);
        }

        /// <summary>0=자정/자정에서 0, 0.5=정오에서 1인 부드러운 일조량 곡선.</summary>
        public static float Daylight01(float normalizedTime)
        {
            float t = Mathf.Repeat(normalizedTime, 1f);
            // (1 - cos(2πt))/2 : t=0 → 0, t=0.5 → 1, t=1 → 0. 단조 증가/감소.
            return (1f - Mathf.Cos(t * 2f * Mathf.PI)) * 0.5f;
        }

        /// <summary>
        /// 시각 → 태양(Directional Light)의 회전. 하루 동안 X축으로 한 바퀴 돈다.
        /// t=0(자정) 지평선 아래 → t=0.25(일출) 동쪽 지평선 → t=0.5(정오) 머리 위 →
        /// t=0.75(일몰) 서쪽 지평선. yaw로 궤도의 나침반 방향을 돌린다.
        /// </summary>
        public static Quaternion SunRotation(float normalizedTime, float yaw)
        {
            float t = Mathf.Repeat(normalizedTime, 1f);
            // 빛 진행 방향(forward) 기준 pitch: 정오에 90°(수직 하강), 일출/일몰에 0°/180°(수평).
            float pitch = t * 360f - 90f;
            return Quaternion.Euler(pitch, yaw, 0f);
        }

        /// <summary>시각 → 4구간. 경계: 일출 0.25, 일몰 0.75 기준 ±0.05 전이대.</summary>
        public static DayNightPhase PhaseOf(float normalizedTime)
        {
            float t = Mathf.Repeat(normalizedTime, 1f);
            if (t >= 0.20f && t < 0.30f) return DayNightPhase.Dawn;
            if (t >= 0.30f && t < 0.70f) return DayNightPhase.Day;
            if (t >= 0.70f && t < 0.80f) return DayNightPhase.Dusk;
            return DayNightPhase.Night;
        }

        /// <summary>시각 + 설정 → 환경 값 묶음. 모든 끝값을 daylight로 선형 보간.</summary>
        public static DayNightState Evaluate(float normalizedTime, DayNightConfig cfg)
        {
            float t = Mathf.Repeat(normalizedTime, 1f);
            float day = Daylight01(t);

            return new DayNightState(
                normalizedTime: t,
                daylight01: day,
                phase: PhaseOf(t),
                sunIntensity: Mathf.Lerp(cfg.nightSunIntensity, cfg.daySunIntensity, day),
                sunTemperature: Mathf.Lerp(cfg.nightSunTemperature, cfg.daySunTemperature, day),
                sunRotation: SunRotation(t, cfg.sunYaw),
                ambient: Color.Lerp(cfg.nightAmbient, cfg.dayAmbient, day),
                fogDensity: Mathf.Lerp(cfg.nightFogDensity, cfg.dayFogDensity, day),
                viewRadius: Mathf.Lerp(cfg.nightViewRadius, cfg.dayViewRadius, day),
                spawnIntervalScale: Mathf.Lerp(cfg.nightSpawnIntervalScale, cfg.daySpawnIntervalScale, day));
        }
    }
}
