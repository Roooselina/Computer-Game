using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>
    /// 대화 선택지 (2026-11-15). 조작에 맞춰 가로로 늘어놓는다: A/D 로 고르고 J 로 확정.
    /// - 열린 직후 0.35초는 확정을 받지 않는다 (이전 대사를 넘긴 J 연타가 선택으로 이어지지 않게).
    ///   열릴 때 누르고 있던 키는 놓을 때까지 무시된다 (GameInput 상태 잠금).
    /// - noDefault 이면 처음에 아무것도 고르지 않은 상태로 열린다: A/D 를 한 번 눌러야 확정할 수 있다 (처치 여부 같은 무거운 선택).
    /// - Esc 로 닫을 수 없다 (선택을 피해 갈 수 없는 대화). 컷신 건너뛰기(Enter)도 받지 않는다 (Game.ChoiceOpen).
    /// </summary>
    public class ChoiceBar
    {
        public readonly RectTransform root;
        readonly List<(RectTransform rt, Image bg, Text t)> items = new List<(RectTransform, Image, Text)>();
        readonly List<bool> enabled = new List<bool>();
        readonly Text help;
        int index;
        float openedAt;
        readonly InputCtxToken token;
        public int Index => index;

        public ChoiceBar(Transform parent, IList<string> labels, IList<bool> en, bool noDefault)
        {
            root = UIKit.Rect("ChoiceBar", parent, new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, Vector2.zero);
            root.pivot = new Vector2(0.5f, 0);
            root.anchoredPosition = new Vector2(0, DialogueBox.AtTop ? 360 - DialogueBox.BoxBottom - DialogueBox.BoxHeight - 46 : DialogueBox.BoxBottom + DialogueBox.BoxHeight + 6);
            float gap = 6, h = 22;
            var widths = new List<float>();
            float total = 0;
            for (int i = 0; i < labels.Count; i++)
            {
                float w = Mathf.Clamp(labels[i].Length * 10.5f + 22, 70, 210);
                widths.Add(w); total += w + (i > 0 ? gap : 0);
            }
            float scale = total > 620 ? 620 / total : 1f;
            root.sizeDelta = new Vector2(total * scale, h + 14);
            float x = -total * scale / 2f;
            for (int i = 0; i < labels.Count; i++)
            {
                float w = widths[i] * scale;
                var rt = UIKit.Rect("Opt" + i, root, new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, Vector2.zero);
                rt.pivot = new Vector2(0, 0); rt.anchoredPosition = new Vector2(x, 12); rt.sizeDelta = new Vector2(w, h);
                var bg = UIKit.Panel(rt, "Bg", new Color(0.1f, 0.12f, 0.18f, 0.95f), true);
                var t = UIKit.Label(rt, "T", labels[i], 11, TextAnchor.MiddleCenter, UIKit.Ink);
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
                t.resizeTextForBestFit = true; t.resizeTextMinSize = 8; t.resizeTextMaxSize = 11;
                items.Add((rt, bg, t));
                enabled.Add(en == null || i >= en.Count || en[i]);
                x += w + gap;
            }
            help = UIKit.Label(root, "Help", GameInput.Pair(Act.Left, Act.Right) + " 고르기 · " + GameInput.Label(Act.Interact) + " 확정", 8, TextAnchor.LowerCenter, UIKit.Dim);
            help.rectTransform.anchorMin = new Vector2(0, 0); help.rectTransform.anchorMax = new Vector2(1, 0);
            help.rectTransform.offsetMin = new Vector2(0, 0); help.rectTransform.offsetMax = new Vector2(0, 11);
            index = noDefault ? -1 : FirstEnabled();
            openedAt = Time.unscaledTime;
            token = InputCtxToken.Attach(root.gameObject, InputCtx.Menu);
            root.SetAsLastSibling();
            Refresh();
        }

        int FirstEnabled() { for (int i = 0; i < enabled.Count; i++) if (enabled[i]) return i; return 0; }

        void Refresh()
        {
            for (int i = 0; i < items.Count; i++)
            {
                bool sel = i == index;
                var (rt, bg, t) = items[i];
                t.color = !enabled[i] ? UIKit.Dim * 0.75f : sel ? new Color(0.1f, 0.1f, 0.15f) : UIKit.Ink;
                bg.color = !enabled[i] ? new Color(0.08f, 0.09f, 0.12f, 0.8f) : sel ? new Color(1f, 0.86f, 0.4f, 1f) : new Color(0.1f, 0.12f, 0.18f, 0.95f);
                rt.localScale = sel ? new Vector3(1.04f, 1.04f, 1) : Vector3.one;
            }
            help.text = index < 0
                ? GameInput.Pair(Act.Left, Act.Right) + " 로 먼저 고르세요"
                : GameInput.Pair(Act.Left, Act.Right) + " 고르기 · " + GameInput.Label(Act.Interact) + " 확정";
        }

        /// <summary>-1 아직, 0 이상 선택.</summary>
        public int Tick()
        {
            if (MenuList.ScriptedChoices.Count > 0)
            {
                int s = MenuList.ScriptedChoices.Dequeue();
                if (s >= 0 && s < items.Count && enabled[s]) { index = s; Refresh(); return s; }
                Debug.LogWarning("[EW] 자동 선택 " + s + " 을(를) 쓸 수 없어 무시함 (항목 " + items.Count + ")");
            }
            int d = GameInput.NavHorizontal();
            if (d != 0 && items.Count > 0)
            {
                int start = index < 0 ? (d > 0 ? -1 : items.Count) : index;
                int n = start;
                for (int k = 0; k < items.Count; k++)
                {
                    n = (n + d + items.Count) % items.Count;
                    if (enabled[n]) break;
                }
                index = n; Refresh(); Sfx.Play("move");
            }
            if (Time.unscaledTime - openedAt < 0.35f) return -1;
            if (GameInput.ConfirmDown)
            {
                if (index < 0 || !enabled[index]) { Sfx.Play("no", 0.5f); return -1; }
                Sfx.Play("text", 0.8f);
                return index;
            }
            return -1;
        }

        public void Destroy() { if (root != null) Object.Destroy(root.gameObject); }
    }
}
