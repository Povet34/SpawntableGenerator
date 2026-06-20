using UnityEngine;

namespace SpawnSystem.Environment
{
    /// <summary>태양(Directional Light)의 밝기/색온도/각도를 낮/밤에 맞춘다.</summary>
    [RequireComponent(typeof(Light))]
    public class SunLightResponder : DayNightResponderBehaviour
    {
        [Tooltip("해가 하루 동안 하늘을 가로지르도록 라이트를 회전시킨다.")]
        public bool rotateWithTime = true;

        Light _light;

        protected override void OnEnable()
        {
            _light = GetComponent<Light>();
            _light.useColorTemperature = true;
            base.OnEnable();
        }

        public override void OnDayNight(in DayNightState state)
        {
            if (_light == null) return;
            _light.intensity = state.SunIntensity;
            _light.colorTemperature = state.SunTemperature;
            if (rotateWithTime) _light.transform.rotation = state.SunRotation;
        }
    }
}
