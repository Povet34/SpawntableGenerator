using UnityEngine;

namespace SpawnSystem.Environment
{
    /// <summary>
    /// 특정 시각의 환경 값 묶음(불변). <see cref="DayNightModel.Evaluate"/>의 결과이며
    /// <see cref="IDayNightResponder"/> 구현체들에 그대로 전달된다.
    /// 값 타입이라 테스트에서 비교/검증이 쉽고 부수효과가 없다.
    /// </summary>
    public readonly struct DayNightState
    {
        /// <summary>0=자정 … 0.5=정오 … 1=자정.</summary>
        public readonly float NormalizedTime;
        /// <summary>0=완전한 밤, 1=한낮. (코사인 곡선)</summary>
        public readonly float Daylight01;
        public readonly DayNightPhase Phase;

        public readonly float SunIntensity;
        public readonly float SunTemperature;
        /// <summary>태양(Directional Light)이 향할 회전. 하루 동안 X축으로 한 바퀴.</summary>
        public readonly Quaternion SunRotation;
        public readonly Color Ambient;
        public readonly float FogDensity;
        public readonly float ViewRadius;
        public readonly float SpawnIntervalScale;

        public DayNightState(float normalizedTime, float daylight01, DayNightPhase phase,
                             float sunIntensity, float sunTemperature, Quaternion sunRotation,
                             Color ambient, float fogDensity, float viewRadius, float spawnIntervalScale)
        {
            NormalizedTime = normalizedTime;
            Daylight01 = daylight01;
            Phase = phase;
            SunIntensity = sunIntensity;
            SunTemperature = sunTemperature;
            SunRotation = sunRotation;
            Ambient = ambient;
            FogDensity = fogDensity;
            ViewRadius = viewRadius;
            SpawnIntervalScale = spawnIntervalScale;
        }

        /// <summary>밤일수록 1에 가까운 값(분위기 라이트 on/off 게이트 등에 사용).</summary>
        public float Darkness01 => 1f - Daylight01;
    }
}
