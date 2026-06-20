using System;
using SpawnSystem.Monsters;

namespace SpawnSystem.UI
{
    /// <summary><see cref="Health"/> 컴포넌트를 <see cref="IHealthModel"/>로 감싸는 어댑터.</summary>
    public sealed class HealthModel : IHealthModel, IDisposable
    {
        readonly Health _health;

        public HealthModel(Health health)
        {
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _health.Changed += OnSourceChanged;
        }

        public float Current => _health != null ? _health.Current : 0f;
        public float Max => _health != null ? _health.MaxHP : 0f;
        public float Normalized => _health != null ? _health.Normalized : 0f;

        public event Action Changed;

        void OnSourceChanged() => Changed?.Invoke();

        public void Dispose()
        {
            if (_health != null) _health.Changed -= OnSourceChanged;
        }
    }
}
