using UnityEngine;

namespace SpawnSystem.Environment
{
    /// <summary>씬 전역 환경광(RenderSettings.ambientLight)을 낮/밤에 맞춰 어둡힌다.</summary>
    public class AmbientLightResponder : DayNightResponderBehaviour
    {
        public override void OnDayNight(in DayNightState state)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = state.Ambient;
        }
    }
}
