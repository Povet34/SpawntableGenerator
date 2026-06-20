using SpawnSystem.Spawning;
using UnityEngine;

namespace SpawnSystem.Environment
{
    /// <summary>
    /// 밤이 될수록 스폰 간격을 짧게(몬스터 활동량↑). 디렉터의 노출 노브
    /// <see cref="SpawnDirector.spawnIntervalScale"/>만 건드린다 — 디렉터 내부 로직과 분리.
    /// </summary>
    public class SpawnRateResponder : DayNightResponderBehaviour
    {
        [Tooltip("비우면 씬에서 자동 탐색")]
        public SpawnDirector director;

        protected override void OnEnable()
        {
            if (director == null)
                director = Object.FindAnyObjectByType<SpawnDirector>();
            base.OnEnable();
        }

        public override void OnDayNight(in DayNightState state)
        {
            if (director != null)
                director.spawnIntervalScale = state.SpawnIntervalScale;
        }
    }
}
