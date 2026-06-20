using System;
using UnityEngine;

namespace SpawnSystem.Environment
{
    /// <summary>
    /// 낮/밤 사이클의 튜닝 끝값(낮 값 ↔ 밤 값)을 담는 순수 데이터.
    /// <see cref="DayNightModel"/>이 daylight(0..1)로 두 끝값을 보간한다.
    /// MonoBehaviour가 아니므로 EditMode 테스트에서 자유롭게 생성 가능.
    /// 기본값은 GameDesign.md §3.2 표를 따른다.
    /// </summary>
    [Serializable]
    public class DayNightConfig
    {
        [Header("주기")]
        [Tooltip("한 사이클(자정→자정) 실시간 길이(초)")]
        [Min(1f)] public float cycleSeconds = 300f;

        [Tooltip("시작 시각(0=자정, 0.25=일출, 0.5=정오)")]
        [Range(0f, 1f)] public float startNormalizedTime = 0.3f;

        [Header("태양광(Directional Light)")]
        public float daySunIntensity = 1.0f;
        public float nightSunIntensity = 0.05f;
        public float daySunTemperature = 5500f;
        public float nightSunTemperature = 2800f;

        [Tooltip("태양 궤도의 방위각(yaw). 하루 동안 X축으로 한 바퀴 돌며 이 방향을 향한다.")]
        [Range(0f, 360f)] public float sunYaw = 330f;

        [Header("환경광(Ambient)")]
        public Color dayAmbient = new Color(0.55f, 0.57f, 0.62f);
        public Color nightAmbient = new Color(0.03f, 0.04f, 0.09f);

        [Header("안개")]
        public float dayFogDensity = 1.2f;
        public float nightFogDensity = 2.6f;

        [Header("시야 반경(FogReveal)")]
        public float dayViewRadius = 24f;
        public float nightViewRadius = 10f;

        [Header("스폰 간격 배율(밤=짧게)")]
        public float daySpawnIntervalScale = 1.0f;
        public float nightSpawnIntervalScale = 0.5f;

        /// <summary>밤일수록(daylight↓) 켜지는 분위기 포인트 라이트 강도 배율의 기준값.</summary>
        public float atmosphereLightNightIntensity = 1f;
    }
}
