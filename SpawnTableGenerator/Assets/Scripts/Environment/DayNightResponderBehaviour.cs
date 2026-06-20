using UnityEngine;

namespace SpawnSystem.Environment
{
    /// <summary>
    /// <see cref="IDayNightResponder"/> MonoBehaviour의 공통 토대: 컨트롤러를 찾아
    /// OnEnable에서 자가 등록하고 OnDisable에서 해제한다(자가 등록 Observer).
    /// 구체 반응자는 <see cref="OnDayNight"/>만 구현하면 된다(SRP).
    /// </summary>
    public abstract class DayNightResponderBehaviour : MonoBehaviour, IDayNightResponder
    {
        [Tooltip("비우면 씬에서 DayNightController를 자동 탐색")]
        public DayNightController controller;

        protected virtual void OnEnable()
        {
            if (controller == null)
                controller = Object.FindAnyObjectByType<DayNightController>();
            if (controller != null)
                controller.Register(this);
        }

        protected virtual void OnDisable()
        {
            if (controller != null)
                controller.Unregister(this);
        }

        public abstract void OnDayNight(in DayNightState state);
    }
}
