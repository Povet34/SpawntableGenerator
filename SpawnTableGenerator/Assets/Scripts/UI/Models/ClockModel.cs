using System;
using SpawnSystem.Environment;

namespace SpawnSystem.UI
{
    /// <summary><see cref="DayNightController"/>를 <see cref="IClockModel"/>로 감싸는 어댑터. 시각→라벨 변환 포함.</summary>
    public sealed class ClockModel : IClockModel, IDisposable
    {
        readonly DayNightController _controller;

        public ClockModel(DayNightController controller)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _controller.StateChanged += OnStateChanged;
        }

        public DayNightPhase Phase => _controller != null ? _controller.Current.Phase : DayNightPhase.Day;
        public float NormalizedTime => _controller != null ? _controller.Current.NormalizedTime : 0f;
        public float Daylight01 => _controller != null ? _controller.Current.Daylight01 : 1f;

        public string Label => $"{PhaseLabel(Phase)} {ClockString(NormalizedTime)}";

        public event Action Changed;

        void OnStateChanged(DayNightState _) => Changed?.Invoke();

        /// <summary>0..1 시각을 24시간 "HH:MM" 문자열로.</summary>
        public static string ClockString(float normalizedTime)
        {
            float hours = normalizedTime * 24f;
            int hh = (int)hours % 24;
            int mm = (int)((hours - (int)hours) * 60f) % 60;
            return $"{hh:00}:{mm:00}";
        }

        public static string PhaseLabel(DayNightPhase phase)
        {
            switch (phase)
            {
                case DayNightPhase.Dawn: return "새벽";
                case DayNightPhase.Day: return "낮";
                case DayNightPhase.Dusk: return "황혼";
                default: return "밤";
            }
        }

        public void Dispose()
        {
            if (_controller != null) _controller.StateChanged -= OnStateChanged;
        }
    }
}
