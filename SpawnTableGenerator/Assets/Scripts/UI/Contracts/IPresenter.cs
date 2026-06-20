using System;

namespace SpawnSystem.UI
{
    /// <summary>
    /// 프레젠터 수명주기. Initialize에서 모델 구독 + 첫 렌더, Dispose에서 구독 해제.
    /// 컴포지션 루트(HudBootstrap)가 생성/파기를 관장한다.
    /// </summary>
    public interface IPresenter : IDisposable
    {
        void Initialize();
    }
}
