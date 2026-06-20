using System;
using SpawnSystem.Environment;
using SpawnSystem.UI;

namespace SpawnSystem.Tests
{
    // MVP 테스트용 손수 만든 테스트 더블(가짜). Presenter가 UnityEngine.UI에 의존하지 않기에
    // 이런 순수 가짜만으로 EditMode에서 상호작용을 검증할 수 있다.

    internal sealed class FakeHealthModel : IHealthModel
    {
        public float Current { get; set; }
        public float Max { get; set; }
        public float Normalized { get; set; }
        public event Action Changed;
        public void Raise() => Changed?.Invoke();
    }

    internal sealed class FakeHealthView : IHealthView
    {
        public int RenderCount;
        public float LastNormalized, LastCurrent, LastMax;
        public void Render(float normalized, float current, float max)
        {
            RenderCount++;
            LastNormalized = normalized;
            LastCurrent = current;
            LastMax = max;
        }
    }

    internal sealed class FakeWeaponModel : IWeaponModel
    {
        public int ActiveSlot { get; set; }
        public string ActiveName { get; set; } = "근접";
        public event Action Changed;
        public void Raise() => Changed?.Invoke();
    }

    internal sealed class FakeWeaponView : IWeaponView
    {
        public int RenderCount;
        public int LastSlot;
        public string LastName;
        public void Render(int activeSlot, string activeName)
        {
            RenderCount++;
            LastSlot = activeSlot;
            LastName = activeName;
        }
    }

    internal sealed class FakeClockModel : IClockModel
    {
        public DayNightPhase Phase { get; set; } = DayNightPhase.Day;
        public float NormalizedTime { get; set; }
        public float Daylight01 { get; set; } = 1f;
        public string Label { get; set; } = "낮 12:00";
        public event Action Changed;
        public void Raise() => Changed?.Invoke();
    }

    internal sealed class FakeClockView : IClockView
    {
        public int RenderCount;
        public DayNightPhase LastPhase;
        public float LastNormalizedTime, LastDaylight;
        public string LastLabel;
        public void Render(DayNightPhase phase, float normalizedTime, float daylight01, string label)
        {
            RenderCount++;
            LastPhase = phase;
            LastNormalizedTime = normalizedTime;
            LastDaylight = daylight01;
            LastLabel = label;
        }
    }
}
