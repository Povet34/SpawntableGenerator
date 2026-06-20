using UnityEngine;
using UnityEngine.UI;

namespace SpawnSystem.UI
{
    /// <summary>
    /// 코드로 uGUI 요소를 만드는 소소한 헬퍼(DRY). 뷰들이 자신의 위젯을 직접 구성할 때 사용.
    /// 폰트는 런타임 내장 폰트(LegacyRuntime.ttf)를 사용해 별도 에셋 배선이 필요 없다.
    /// </summary>
    public static class UiBuilder
    {
        static Font _font;
        static Font Font
        {
            get
            {
                if (_font == null)
                {
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                return _font;
            }
        }

        public static RectTransform Panel(Transform parent, string name, Vector2 anchorMin,
                                          Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos,
                                          Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = color;
            return rt;
        }

        public static Image FilledBar(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            Stretch(rt);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)Image.OriginHorizontal.Left;
            img.fillAmount = 1f;
            // 단색 채움을 위해 흰 1px 스프라이트 대용 — 기본 Image는 sprite 없으면 사각형으로 그려짐
            return img;
        }

        public static Text Label(Transform parent, string name, int fontSize, TextAnchor anchor, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            Stretch(rt);
            var txt = go.GetComponent<Text>();
            txt.font = Font;
            txt.fontSize = fontSize;
            txt.alignment = anchor;
            txt.color = color;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
