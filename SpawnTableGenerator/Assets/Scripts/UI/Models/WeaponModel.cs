using System;
using SpawnSystem.Combat;

namespace SpawnSystem.UI
{
    /// <summary><see cref="PlayerCombat"/>를 <see cref="IWeaponModel"/>로 감싸는 어댑터. 슬롯→이름 매핑 포함.</summary>
    public sealed class WeaponModel : IWeaponModel, IDisposable
    {
        readonly PlayerCombat _combat;
        readonly string[] _names;

        public WeaponModel(PlayerCombat combat, string[] slotNames = null)
        {
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _names = slotNames ?? new[] { "근접", "원거리" };
            _combat.SlotChanged += OnSourceChanged;
        }

        public int ActiveSlot => _combat != null ? _combat.ActiveSlot : 0;

        public string ActiveName
        {
            get
            {
                int s = ActiveSlot;
                return (s >= 0 && s < _names.Length) ? _names[s] : $"슬롯 {s + 1}";
            }
        }

        public event Action Changed;

        void OnSourceChanged() => Changed?.Invoke();

        public void Dispose()
        {
            if (_combat != null) _combat.SlotChanged -= OnSourceChanged;
        }
    }
}
