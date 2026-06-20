using System;

namespace SpawnSystem.UI
{
    /// <summary>낮/밤 시계 MVP의 Presenter.</summary>
    public sealed class ClockPresenter : IPresenter
    {
        readonly IClockModel _model;
        readonly IClockView _view;
        bool _initialized;

        public ClockPresenter(IClockModel model, IClockView view)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            _model.Changed += Render;
            Render();
        }

        void Render()
        {
            _view.Render(_model.Phase, _model.NormalizedTime, _model.Daylight01, _model.Label);
        }

        public void Dispose()
        {
            if (!_initialized) return;
            _model.Changed -= Render;
            _initialized = false;
        }
    }
}
