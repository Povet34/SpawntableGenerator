using System;

namespace SpawnSystem.UI
{
    /// <summary>
    /// 체력 MVP의 Presenter. Model의 변경을 받아 View에 그릴 값을 넘긴다.
    /// UnityEngine.UI에 의존하지 않으므로 가짜 Model/View로 EditMode 단위 테스트 가능.
    /// </summary>
    public sealed class HealthPresenter : IPresenter
    {
        readonly IHealthModel _model;
        readonly IHealthView _view;
        bool _initialized;

        public HealthPresenter(IHealthModel model, IHealthView view)
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
            _view.Render(_model.Normalized, _model.Current, _model.Max);
        }

        public void Dispose()
        {
            if (!_initialized) return;
            _model.Changed -= Render;
            _initialized = false;
        }
    }
}
