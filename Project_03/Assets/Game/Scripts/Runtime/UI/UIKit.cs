using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>런타임 UI 생성 도우미. 기준 해상도 640×360, Galmuri11.</summary>
    public static class UIKit
    {
        public static readonly Color Ink = new Color(0.95f, 0.95f, 0.98f);
        public static readonly Color Dim = new Color(0.62f, 0.66f, 0.75f);
        public static readonly Color Accent = new Color(1f, 0.86f, 0.35f);
        public static readonly Color Narr = new Color(0.78f, 0.86f, 1f);
        public static readonly Color PanelTint = new Color(0.16f, 0.2f, 0.32f, 0.96f);
        public static readonly Color Danger = new Color(1f, 0.45f, 0.4f);
        public static readonly Color Good = new Color(0.55f, 1f, 0.6f);

        static Font fallbackFont;
        /// <summary>긴 한글 대사·메뉴·설명용 본문 글꼴 (Pretendard). 없으면 도트 글꼴.</summary>
        public static Font Font
        {
            get
            {
                var cfg = GameConfig.Load();
                if (cfg != null && cfg.fontBody != null) return cfg.fontBody;
                return Pixel;
            }
        }
        /// <summary>본문 굵은 글꼴 (이름표·제목).</summary>
        public static Font Bold
        {
            get
            {
                var cfg = GameConfig.Load();
                if (cfg != null && cfg.fontBodyBold != null) return cfg.fontBodyBold;
                return Font;
            }
        }
        /// <summary>짧은 단말 표기·도트 연출용 (Galmuri).</summary>
        public static Font Pixel
        {
            get
            {
                var cfg = GameConfig.Load();
                if (cfg != null && cfg.font != null) return cfg.font;
                if (fallbackFont == null) fallbackFont = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Gulim", "Noto Sans KR" }, 11);
                return fallbackFont;
            }
        }

        public static Canvas MakeCanvas(string name, int order, Transform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            var c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = order;
            c.pixelPerfect = false;
            var s = go.AddComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(640, 360);
            s.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            s.matchWidthOrHeight = 0.5f;
            return c;
        }

        public static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offMin, Vector2 offMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.offsetMin = offMin; rt.offsetMax = offMax;
            return rt;
        }

        public static RectTransform Fill(string name, Transform parent) => Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        public static Image Panel(Transform parent, string name, Color? tint = null, bool inlay = false)
        {
            var rt = Fill(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            var cfg = GameConfig.Load();
            var sp = cfg != null ? (inlay ? cfg.panelInlay : cfg.panel) : null;
            img.sprite = sp;
            img.type = sp != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = tint ?? PanelTint;
            return img;
        }

        public static Image Solid(Transform parent, string name, Color c)
        {
            var rt = Fill(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = c;
            return img;
        }

        public static Text Label(Transform parent, string name, string text, int size = 11, TextAnchor anchor = TextAnchor.UpperLeft, Color? color = null)
        {
            var rt = Fill(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.fontSize = size;
            t.text = text;
            t.alignment = anchor;
            t.color = color ?? Ink;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.lineSpacing = 1.25f;
            t.supportRichText = true;
            t.raycastTarget = false;
            return t;
        }

        public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        public static string Bar(int value, int max, int cells = 10)
        {
            int f = max <= 0 ? 0 : Mathf.Clamp(Mathf.RoundToInt((float)value / max * cells), 0, cells);
            return new string('■', f) + new string('□', cells - f);
        }
    }

    /// <summary>키보드로 고르는 목록. 마우스 클릭도 지원.</summary>
    public class MenuList
    {
        public RectTransform root;
        public readonly List<Text> items = new List<Text>();
        public readonly List<bool> enabled = new List<bool>();
        public int index;
        public Text help;
        Image highlight;
        readonly int fontSize;
        readonly float rowH;

        public MenuList(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos, float width, float rowHeight = 16, int size = 11)
        {
            root = UIKit.Rect(name, parent, anchor, anchor, Vector2.zero, Vector2.zero);
            root.pivot = pivot; root.anchoredPosition = pos; root.sizeDelta = new Vector2(width, 20);
            UIKit.Panel(root, "Bg");
            highlight = UIKit.Solid(root, "Highlight", new Color(1f, 0.86f, 0.35f, 0.16f));
            var hr = highlight.rectTransform;
            hr.anchorMin = new Vector2(0, 1); hr.anchorMax = new Vector2(1, 1); hr.pivot = new Vector2(0.5f, 1);
            fontSize = size; rowH = rowHeight;
            token = InputCtxToken.Attach(root.gameObject, InputCtx.Menu);
        }
        InputCtxToken token;

        public void SetItems(IList<string> labels, IList<bool> en = null)
        {
            foreach (var t in items) if (t != null) Object.Destroy(t.gameObject);
            items.Clear(); enabled.Clear();
            for (int i = 0; i < labels.Count; i++)
            {
                var t = UIKit.Label(root, "Item" + i, labels[i], fontSize, TextAnchor.MiddleLeft);
                var rt = t.rectTransform;
                rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1); rt.pivot = new Vector2(0.5f, 1);
                rt.offsetMin = new Vector2(14, 0); rt.offsetMax = new Vector2(-6, 0);
                rt.anchoredPosition = new Vector2(4, -6 - i * rowH); rt.sizeDelta = new Vector2(-20, rowH);
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
                t.raycastTarget = true;
                items.Add(t);
                enabled.Add(en == null || en[i]);
            }
            root.sizeDelta = new Vector2(root.sizeDelta.x, 12 + labels.Count * rowH);
            index = Mathf.Clamp(index, 0, Mathf.Max(0, labels.Count - 1));
            Refresh();
        }

        public void Refresh()
        {
            for (int i = 0; i < items.Count; i++)
            {
                bool sel = i == index;
                string raw = items[i].text.TrimStart('▶', ' ');
                items[i].text = (sel ? "▶ " : "  ") + raw;
                items[i].color = !enabled[i] ? UIKit.Dim * 0.8f : sel ? UIKit.Accent : UIKit.Ink;
            }
            if (highlight != null)
            {
                highlight.gameObject.SetActive(items.Count > 0);
                highlight.rectTransform.offsetMin = new Vector2(4, 0); highlight.rectTransform.offsetMax = new Vector2(-4, 0);
                highlight.rectTransform.anchoredPosition = new Vector2(0, -6 - index * rowH);
                highlight.rectTransform.sizeDelta = new Vector2(-8, rowH);
            }
        }

        /// <summary>자동 테스트용: 다음에 열리는 메뉴들이 차례로 고를 항목 번호 (-2 = 취소).</summary>
        public static readonly Queue<int> ScriptedChoices = new Queue<int>();

        /// <summary>-1 없음, -2 취소, 0 이상 선택. 비활성 항목은 선택되지 않는다.</summary>
        public int Tick(bool allowCancel = true)
        {
            if (ScriptedChoices.Count > 0)
            {
                int s = ScriptedChoices.Dequeue();
                if (s == -2 && allowCancel) return -2;
                if (s >= 0 && s < items.Count && enabled[s]) { index = s; Refresh(); return s; }
                Debug.LogWarning("[EW] 자동 선택 " + s + " 을(를) 쓸 수 없어 무시함 (항목 " + items.Count + ")");
            }
            if (items.Count == 0) return allowCancel && GameInput.CancelDown ? -2 : -1;
            int d = GameInput.NavVertical();
            if (d != 0) { index = (index + d + items.Count) % items.Count; Refresh(); Sfx.Play("move"); }
            // 마우스
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                Vector2 mp = mouse.position.ReadValue();
                for (int i = 0; i < items.Count; i++)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(items[i].rectTransform, mp, null))
                    {
                        if (mouse.delta.ReadValue().sqrMagnitude > 0.1f && index != i) { index = i; Refresh(); }
                        if (mouse.leftButton.wasPressedThisFrame && enabled[i]) { index = i; Refresh(); Sfx.Play("ok"); return i; }
                    }
                }
            }
            if (GameInput.ConfirmDown)
            {
                if (enabled[index]) { Sfx.Play("ok"); return index; }
                Sfx.Play("no");
            }
            if (allowCancel && GameInput.CancelDown) { Sfx.Play("cancel"); return -2; }
            return -1;
        }

        public void SetActive(bool on) { root.gameObject.SetActive(on); }
        public void Destroy()
        {
            if (token != null) token.Release();
            if (root != null) Object.Destroy(root.gameObject);
        }
    }
}
