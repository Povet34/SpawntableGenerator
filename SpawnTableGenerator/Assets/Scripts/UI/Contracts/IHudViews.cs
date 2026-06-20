using SpawnSystem.Environment;

namespace SpawnSystem.UI
{
    /// <summary>체력 뷰: 받은 값을 그리기만 하는 humble object(로직 없음).</summary>
    public interface IHealthView
    {
        void Render(float normalized, float current, float max);
    }

    /// <summary>무기 슬롯 뷰.</summary>
    public interface IWeaponView
    {
        void Render(int activeSlot, string activeName);
    }

    /// <summary>낮/밤 시계 뷰.</summary>
    public interface IClockView
    {
        void Render(DayNightPhase phase, float normalizedTime, float daylight01, string label);
    }
}
