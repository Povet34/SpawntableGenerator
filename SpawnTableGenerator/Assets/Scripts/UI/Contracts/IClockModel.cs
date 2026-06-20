using System;
using SpawnSystem.Environment;

namespace SpawnSystem.UI
{
    /// <summary>낮/밤 시계 표시용 데이터 + 변경 알림.</summary>
    public interface IClockModel
    {
        DayNightPhase Phase { get; }
        float NormalizedTime { get; }
        float Daylight01 { get; }
        string Label { get; }
        event Action Changed;
    }
}
