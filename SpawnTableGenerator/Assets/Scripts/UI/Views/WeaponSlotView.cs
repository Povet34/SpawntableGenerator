using UnityEngine;
using UnityEngine.UI;

namespace SpawnSystem.UI
{
    /// <summary>무기 슬롯 뷰(humble object). 좌하단 체력바 위에 현재 무기 표시.</summary>
    public sealed class WeaponSlotView : MonoBehaviour, IWeaponView
    {
        Text _label;

        public static WeaponSlotView Create(Transform parent)
        {
            var panel = UiBuilder.Panel(parent, "WeaponSlot",
                anchorMin: Vector2.zero, anchorMax: Vector2.zero, pivot: Vector2.zero,
                anchoredPos: new Vector2(24f, 64f), size: new Vector2(320f, 28f),
                color: new Color(0f, 0f, 0f, 0.4f));

            var label = UiBuilder.Label(panel, "Label", 16, TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.4f));
            label.rectTransform.offsetMin = new Vector2(10f, 0f);

            var view = panel.gameObject.AddComponent<WeaponSlotView>();
            view._label = label;
            return view;
        }

        public void Render(int activeSlot, string activeName)
        {
            if (_label != null)
                _label.text = $"무기 [{activeSlot + 1}] {activeName}";
        }
    }
}
