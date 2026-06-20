using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpawnSystem.Environment
{
    /// <summary>
    /// 낮/밤 사이클의 시계 + 분배기. 매 프레임 시각을 진행시키고
    /// <see cref="DayNightModel"/>로 환경 값을 계산해 등록된 <see cref="IDayNightResponder"/>들에 전달한다.
    /// 컨트롤러는 구체 반응자(빛/안개/스폰)를 전혀 알지 못한다(DIP/OCP).
    /// 반응자는 자신의 OnEnable에서 <see cref="Register"/>로 자가 등록한다(Observer).
    /// </summary>
    public class DayNightController : MonoBehaviour
    {
        [Tooltip("낮/밤 끝값 설정 (GameDesign.md §3.2)")]
        public DayNightConfig config = new DayNightConfig();

        [Tooltip("끄면 시간이 멈추고 currentNormalizedTime을 직접 스크럽 가능")]
        public bool autoAdvance = true;

        [Tooltip("현재 시각(0=자정, 0.5=정오). autoAdvance가 꺼져 있으면 인스펙터로 직접 조절")]
        [Range(0f, 1f)] public float currentNormalizedTime;

        readonly List<IDayNightResponder> _responders = new List<IDayNightResponder>();
        float _elapsed;
        bool _hasState;

        /// <summary>마지막으로 계산된 환경 상태. 모델 어댑터(시계 UI 등)가 읽는다.</summary>
        public DayNightState Current { get; private set; }

        /// <summary>상태가 갱신될 때마다 발행(IClockModel 어댑터 등이 구독).</summary>
        public event Action<DayNightState> StateChanged;

        void Awake()
        {
            _elapsed = config.startNormalizedTime * config.cycleSeconds;
            Recompute();
        }

        void Update()
        {
            if (autoAdvance)
            {
                _elapsed += Time.deltaTime;
                currentNormalizedTime = DayNightModel.NormalizedFromElapsed(
                    _elapsed, config.cycleSeconds, 0f) + config.startNormalizedTime;
            }
            Recompute();
        }

        void Recompute()
        {
            float t = autoAdvance
                ? DayNightModel.NormalizedFromElapsed(_elapsed, config.cycleSeconds, config.startNormalizedTime)
                : currentNormalizedTime;

            Current = DayNightModel.Evaluate(t, config);
            currentNormalizedTime = Current.NormalizedTime;
            _hasState = true;
            Dispatch(Current);
            StateChanged?.Invoke(Current);
        }

        void Dispatch(in DayNightState state)
        {
            for (int i = 0; i < _responders.Count; i++)
                _responders[i]?.OnDayNight(in state);
        }

        /// <summary>반응자 등록. 등록 즉시 현재 상태를 1회 푸시해 늦게 합류해도 동기화된다.</summary>
        public void Register(IDayNightResponder responder)
        {
            if (responder == null || _responders.Contains(responder)) return;
            _responders.Add(responder);
            if (_hasState) responder.OnDayNight(Current);
        }

        public void Unregister(IDayNightResponder responder)
        {
            _responders.Remove(responder);
        }
    }
}
