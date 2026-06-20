using SpawnSystem.Environment;
using UnityEngine;
using UnityEngine.UI;

namespace SpawnSystem.UI
{
    /// <summary>낮/밤 시계 뷰(humble object). 우상단에 시간대 색 스와치 + 라벨.</summary>
    public sealed class ClockView : MonoBehaviour, IClockView
    {
        Image _swatch;
        Text _label;

        static readonly Color NightTint = new Color(0.12f, 0.16f, 0.40f);
        static readonly Color DayTint = new Color(1f, 0.93f, 0.6f);

        public static ClockView Create(Transform parent)
        {
            var panel = UiBuilder.Panel(parent, "Clock",
                anchorMin: Vector2.one, anchorMax: Vector2.one, pivot: Vector2.one,
                anchoredPos: new Vector2(-24f, -24f), size: new Vector2(220f, 40f),
                color: new Color(0f, 0f, 0f, 0.5f));

            var swatch = UiBuilder.Panel(panel, "Swatch",
                anchorMin: new Vector2(0f, 0.5f), anchorMax: new Vector2(0f, 0.5f), pivot: new Vector2(0f, 0.5f),
                anchoredPos: new Vector2(8f, 0f), size: new Vector2(24f, 24f), color: Color.white).GetComponent<Image>();

            var label = UiBuilder.Label(panel, "Label", 16, TextAnchor.MiddleRight, Color.white);
            label.rectTransform.offsetMax = new Vector2(-12f, 0f);

            var view = panel.gameObject.AddComponent<ClockView>();
            view._swatch = swatch;
            view._label = label;
            return view;
        }

        public void Render(DayNightPhase phase, float normalizedTime, float daylight01, string label)
        {
            if (_swatch != null)
                _swatch.color = Color.Lerp(NightTint, DayTint, daylight01);
            if (_label != null)
                _label.text = label;
        }
    }
}
