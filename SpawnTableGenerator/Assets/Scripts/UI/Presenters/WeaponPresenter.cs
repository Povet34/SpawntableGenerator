using System;

namespace SpawnSystem.UI
{
    /// <summary>무기 슬롯 MVP의 Presenter.</summary>
    public sealed class WeaponPresenter : IPresenter
    {
        readonly IWeaponModel _model;
        readonly IWeaponView _view;
        bool _initialized;

        public WeaponPresenter(IWeaponModel model, IWeaponView view)
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
            _view.Render(_model.ActiveSlot, _model.ActiveName);
        }

        public void Dispose()
        {
            if (!_initialized) return;
            _model.Changed -= Render;
            _initialized = false;
        }
    }
}
