using System;

namespace SpawnSystem.UI
{
    /// <summary>현재 무기 슬롯 표시용 데이터 + 변경 알림.</summary>
    public interface IWeaponModel
    {
        int ActiveSlot { get; }
        string ActiveName { get; }
        event Action Changed;
    }
}
