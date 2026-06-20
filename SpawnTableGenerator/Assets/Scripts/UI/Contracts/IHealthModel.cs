using System;

namespace SpawnSystem.UI
{
    /// <summary>체력 표시에 필요한 데이터 + 변경 알림. View/Presenter는 이 추상에만 의존(DIP).</summary>
    public interface IHealthModel
    {
        float Current { get; }
        float Max { get; }
        float Normalized { get; }
        event Action Changed;
    }
}
