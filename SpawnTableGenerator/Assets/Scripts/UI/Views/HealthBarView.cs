using UnityEngine;
using UnityEngine.UI;

namespace SpawnSystem.UI
{
    /// <summary>체력바 뷰(humble object). 좌하단에 채움바 + 텍스트. 로직 없이 Render만.</summary>
    public sealed class HealthBarView : MonoBehaviour, IHealthView
    {
        Image _fill;
        Text _label;

        static readonly Color Low = new Color(0.85f, 0.18f, 0.15f);
        static readonly Color High = new Color(0.30f, 0.85f, 0.35f);

        public static HealthBarView Create(Transform parent)
        {
            var panel = UiBuilder.Panel(parent, "HealthBar",
                anchorMin: Vector2.zero, anchorMax: Vector2.zero, pivot: Vector2.zero,
                anchoredPos: new Vector2(24f, 24f), size: new Vector2(320f, 34f),
                color: new Color(0f, 0f, 0f, 0.55f));

            var fill = UiBuilder.FilledBar(panel, "Fill", High);
            var label = UiBuilder.Label(panel, "Label", 18, TextAnchor.MiddleCenter, Color.white);

            var view = panel.gameObject.AddComponent<HealthBarView>();
            view._fill = fill;
            view._label = label;
            return view;
        }

        public void Render(float normalized, float current, float max)
        {
            if (_fill != null)
            {
                _fill.fillAmount = Mathf.Clamp01(normalized);
                _fill.color = Color.Lerp(Low, High, normalized);
            }
            if (_label != null)
                _label.text = $"HP {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }
    }
}
