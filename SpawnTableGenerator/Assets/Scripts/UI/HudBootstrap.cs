using System;
using System.Collections.Generic;
using SpawnSystem.Combat;
using SpawnSystem.Environment;
using SpawnSystem.Monsters;
using UnityEngine;
using UnityEngine.UI;

namespace SpawnSystem.UI
{
    /// <summary>
    /// HUD 컴포지션 루트. 유일하게 구체 타입(Model/View/Presenter)을 모두 아는 곳.
    /// 런타임에 Canvas와 뷰를 만들고, 게임플레이 소스(Health/PlayerCombat/DayNightController)를
    /// Model 어댑터로 감싸 Presenter에 주입한다(의존성 주입 + MVP 조립).
    /// 씬에는 이 컴포넌트 하나만 두면 HUD 전체가 구성된다.
    /// </summary>
    public class HudBootstrap : MonoBehaviour
    {
        [Tooltip("비우면 'Player' 태그로 자동 탐색")]
        public GameObject player;
        [Tooltip("비우면 씬에서 자동 탐색")]
        public DayNightController dayNight;
        [Tooltip("플레이어에 Health가 없으면 이 최대 체력으로 자동 부착")]
        public float playerMaxHpIfMissing = 100f;

        readonly List<IPresenter> _presenters = new List<IPresenter>();
        readonly List<IDisposable> _models = new List<IDisposable>();
        Canvas _canvas;

        void Start()
        {
            BuildCanvas();
            ResolveReferences();
            WireHealth();
            WireWeapon();
            WireClock();
        }

        void BuildCanvas()
        {
            var go = new GameObject("HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            _canvas = go.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        void ResolveReferences()
        {
            if (player == null)
            {
                var tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged != null) player = tagged;
            }
            if (dayNight == null)
                dayNight = UnityEngine.Object.FindAnyObjectByType<DayNightController>();
        }

        void WireHealth()
        {
            if (player == null) return;
            var health = player.GetComponent<Health>();
            if (health == null)
            {
                health = player.AddComponent<Health>();
                health.Init(playerMaxHpIfMissing, null);
                Debug.Log($"[HudBootstrap] Player에 Health 자동 부착(maxHP={playerMaxHpIfMissing})");
            }

            var model = new HealthModel(health);
            var view = HealthBarView.Create(_canvas.transform);
            AddPresenter(new HealthPresenter(model, view), model);
        }

        void WireWeapon()
        {
            if (player == null) return;
            var combat = player.GetComponent<PlayerCombat>();
            if (combat == null) return;

            var model = new WeaponModel(combat);
            var view = WeaponSlotView.Create(_canvas.transform);
            AddPresenter(new WeaponPresenter(model, view), model);
        }

        void WireClock()
        {
            if (dayNight == null) return;
            var model = new ClockModel(dayNight);
            var view = ClockView.Create(_canvas.transform);
            AddPresenter(new ClockPresenter(model, view), model);
        }

        void AddPresenter(IPresenter presenter, IDisposable model)
        {
            presenter.Initialize();
            _presenters.Add(presenter);
            if (model != null) _models.Add(model);
        }

        void OnDestroy()
        {
            foreach (var p in _presenters) p.Dispose();
            foreach (var m in _models) m.Dispose();
            _presenters.Clear();
            _models.Clear();
        }
    }
}
