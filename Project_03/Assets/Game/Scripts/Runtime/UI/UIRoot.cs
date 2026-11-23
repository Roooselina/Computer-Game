using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>
    /// 게임 전체에서 하나만 존재하는 UI.
    /// - 도트 UI 층: 640×360 기준 프레임(Root)을 게임 화면과 같은 정수 배율로 맞춘다 (글자가 흐려지지 않게).
    ///   대화창(화면 아래)·선택지·HUD·안내·말풍선·페이드.
    /// - 스탠딩 층(PortraitLayer): 도트 렌더 텍스처와 분리된 고해상도 캔버스. 일러스트를 축소해 뭉개지 않는다.
    /// </summary>
    public class UIRoot : MonoBehaviour
    {
        public Canvas canvas;
        CanvasScaler scaler;
        /// <summary>모든 도트 UI 의 부모 (640×360 프레임).</summary>
        public RectTransform Root { get; private set; }
        public RectTransform worldLayer;    // 말풍선·조사 표시
        [System.NonSerialized] public DialogueBox dialogue;
        [System.NonSerialized] public Hud hud;
        [System.NonSerialized] public PortraitLayer portraits;
        [System.NonSerialized] public LuBubble bubble;
        Image fader, flash;
        Image barTop, barBottom; float barTarget, barCur;
        Text toast; float toastTimer;
        RectTransform toastRt;
        Text prompt; RectTransform promptRt;
        RectTransform skipRt; Image skipFill; Text skipText;
        public readonly List<string> log = new List<string>();

        public static UIRoot Create(Transform parent)
        {
            var go = new GameObject("[UI]");
            go.transform.SetParent(parent, false);
            var ui = go.AddComponent<UIRoot>();
            ui.Build();
            return ui;
        }

        void Build()
        {
            canvas = UIKit.MakeCanvas("Canvas", 10, transform);
            scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            Root = UIKit.Rect("Frame", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Root.sizeDelta = new Vector2(PixelScreen.W, PixelScreen.H);
            portraits = new PortraitLayer(transform);

            worldLayer = UIKit.Fill("World", Root);
            barTop = UIKit.Solid(Root, "BarTop", Color.black); barBottom = UIKit.Solid(Root, "BarBottom", Color.black);
            SetupBar(barTop.rectTransform, true); SetupBar(barBottom.rectTransform, false);
            hud = new Hud(Root, this);
            bubble = new LuBubble(worldLayer, this);
            dialogue = new DialogueBox(Root, this);

            promptRt = UIKit.Rect("Prompt", worldLayer, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            promptRt.sizeDelta = new Vector2(80, 16); promptRt.pivot = new Vector2(0.5f, 0);
            UIKit.Panel(promptRt, "Bg", new Color(0.1f, 0.12f, 0.2f, 0.85f), true);
            prompt = UIKit.Label(promptRt, "T", "", 11, TextAnchor.MiddleCenter, UIKit.Accent);
            promptRt.gameObject.SetActive(false);

            toastRt = UIKit.Rect("Toast", Root, new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
            UIKit.Place(toastRt, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(20, -68), new Vector2(330, 28));
            UIKit.Panel(toastRt, "Bg", new Color(0.08f, 0.1f, 0.18f, 0.9f), true);
            toast = UIKit.Label(toastRt, "T", "", 10, TextAnchor.MiddleCenter);
            toastRt.gameObject.SetActive(false);

            // 컷신 건너뛰기 안내: 오른쪽 아래 작게 (대화창이 열리면 그 위로 비켜 선다)
            skipRt = UIKit.Rect("Skip", Root, new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, Vector2.zero);
            UIKit.Place(skipRt, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-6, 4), new Vector2(96, 13));
            UIKit.Solid(skipRt, "Bg", new Color(0.03f, 0.03f, 0.05f, 0.6f));
            skipFill = UIKit.Solid(skipRt, "Fill", new Color(1f, 0.86f, 0.35f, 0.0f));
            skipFill.rectTransform.anchorMax = new Vector2(0, 1);
            skipText = UIKit.Label(skipRt, "T", "", 9, TextAnchor.MiddleCenter, new Color(0.85f, 0.88f, 0.95f));
            skipRt.gameObject.SetActive(false);

            flash = UIKit.Solid(Root, "Flash", new Color(1, 1, 1, 0)); flash.raycastTarget = false;
            // 페이드는 프레임 밖(레터박스 영역)까지 덮는다
            fader = UIKit.Solid(canvas.transform, "Fader", new Color(0, 0, 0, 0));
            fader.raycastTarget = false;
        }

        void SetupBar(RectTransform rt, bool top)
        {
            rt.anchorMin = new Vector2(0, top ? 1 : 0); rt.anchorMax = new Vector2(1, top ? 1 : 0);
            rt.pivot = new Vector2(0.5f, top ? 1 : 0);
            rt.sizeDelta = new Vector2(0, 0);
        }

        void Update()
        {
            // 도트 UI 를 게임 화면과 같은 배율로
            var ps = PixelScreen.I;
            if (ps != null) scaler.scaleFactor = ps.Scale;
            if (toastTimer > 0)
            {
                toastTimer -= Time.unscaledDeltaTime;
                if (toastTimer <= 0) toastRt.gameObject.SetActive(false);
            }
            barCur = Mathf.MoveTowards(barCur, barTarget, Time.unscaledDeltaTime * 60f);
            barTop.rectTransform.sizeDelta = new Vector2(0, Mathf.Round(barCur));
            barBottom.rectTransform.sizeDelta = new Vector2(0, Mathf.Round(barCur));
            if (portraits == null || hud == null || bubble == null) return;   // 플레이 중 스크립트 다시 불러오기 직후 (초기화 전)
            portraits.Tick();
            hud.Tick();
            bubble.Tick();
        }

        // ------------------------------------------------------------ 페이드·번쩍임·레터박스
        public float FadeAlpha => fader.color.a;
        public void SetFade(float a) { fader.color = new Color(0, 0, 0, a); fader.transform.SetAsLastSibling(); }
        public IEnumerator Fade(float to, float time = 0.35f)
        {
            float from = fader.color.a;
            for (float t = 0; t < time; t += Time.unscaledDeltaTime)
            {
                SetFade(Mathf.Lerp(from, to, t / time));
                yield return null;
            }
            SetFade(to);
        }

        /// <summary>화면 번쩍임. 설정의 번쩍임 강도를 곱한다 (0 이면 생략).</summary>
        public void ClearFlash() { if (flash != null) flash.color = Color.clear; }

        public IEnumerator Flash(Color c, float peak = 0.8f, float time = 0.3f)
        {
            float k = GameSettings.Flash;
            if (k <= 0.01f) yield break;
            flash.transform.SetAsLastSibling();
            for (float t = 0; t < time; t += Time.unscaledDeltaTime)
            {
                flash.color = new Color(c.r, c.g, c.b, Mathf.Lerp(peak * k, 0, t / time));
                yield return null;
            }
            flash.color = new Color(c.r, c.g, c.b, 0);
        }

        public IEnumerator FlashWhite(float time = 0.3f) => Flash(new Color(1, 1, 0.9f), 0.8f, time);

        /// <summary>영화식 위아래 검은 띠 (0 = 없음). 대화창은 그 위에 그린다.</summary>
        public void Letterbox(float px) { barTarget = px; barTop.transform.SetAsFirstSibling(); barBottom.transform.SetAsFirstSibling(); }
        public void LetterboxImmediate(float px) { barTarget = barCur = px; }

        // ------------------------------------------------------------ 안내
        public void Toast(string msg, float time = 2.4f)
        {
            toast.text = msg;
            toastRt.gameObject.SetActive(true);
            toastTimer = time;
            toastRt.SetAsLastSibling();
        }

        public void ShowSkip(float progress)
        {
            bool on = progress >= 0;
            if (skipRt.gameObject.activeSelf != on) skipRt.gameObject.SetActive(on);
            if (!on) return;
            skipRt.SetAsLastSibling();
            skipText.text = GameInput.Label(Act.Skip) + " · 연출 건너뛰기";
            skipRt.anchoredPosition = new Vector2(-6, dialogue != null && dialogue.Open && !DialogueBox.AtTop ? DialogueBox.BoxBottom + DialogueBox.BoxHeight + 3 : 4);
            skipFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(progress), 1);
        }

        // ------------------------------------------------------------ 월드 위치 표시
        public Vector2 WorldToCanvas(Vector3 world)
        {
            Vector2 sp = PixelScreen.WorldToScreen(world);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Root, sp, null, out var local);
            return local;
        }

        public void ShowPrompt(Vector3 world, string text)
        {
            if (string.IsNullOrEmpty(text)) { HidePrompt(); return; }
            prompt.text = text;
            promptRt.sizeDelta = new Vector2(Mathf.Max(44, prompt.preferredWidth + 14), 16);
            var p = WorldToCanvas(world);
            promptRt.anchorMin = promptRt.anchorMax = new Vector2(0.5f, 0.5f);
            promptRt.anchoredPosition = p;
            promptRt.gameObject.SetActive(true);
        }

        public void HidePrompt() { if (promptRt.gameObject.activeSelf) promptRt.gameObject.SetActive(false); }

        public void AddLog(string speaker, string text)
        {
            log.Add(string.IsNullOrEmpty(speaker) ? text : speaker + ": " + text);
            if (log.Count > 200) log.RemoveAt(0);
        }
    }

    // ================================================================ 대화창 (화면 아래)
    public class DialogueBox
    {
        public const float BoxHeight = 88f;     // 360 의 약 24%
        public const float BoxBottom = 6f;
        readonly RectTransform root;
        readonly Text nameText, bodyText, arrow;
        readonly RectTransform nameRt;
        readonly Image nameBg;
        readonly UIRoot ui;
        public bool Open => root.gameObject.activeSelf;
        public RectTransform Rect => root;
        /// <summary>현재 출력 중인 페이지(테스트·확인용).</summary>
        public string CurrentText => bodyText.text;
        public string CurrentSpeaker => nameRt.gameObject.activeSelf ? nameText.text : "";
        float openedAt;

        public DialogueBox(Transform parent, UIRoot ui)
        {
            this.ui = ui;
            root = UIKit.Rect("Dialogue", parent, new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, Vector2.zero);
            UIKit.Place(root, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, BoxBottom), new Vector2(616, BoxHeight));
            UIKit.Panel(root, "Bg", new Color(0.1f, 0.12f, 0.2f, 0.94f));
            // 이름 영역: 상자 왼쪽 위에 걸친 별도 띠
            nameRt = UIKit.Rect("Name", root, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, Vector2.zero);
            UIKit.Place(nameRt, new Vector2(0, 1), new Vector2(0, 0), new Vector2(12, -4), new Vector2(80, 20));
            nameBg = UIKit.Panel(nameRt, "Bg", new Color(0.32f, 0.38f, 0.6f, 1f), true);
            nameText = UIKit.Label(nameRt, "T", "", 12, TextAnchor.MiddleCenter, UIKit.Accent);
            nameText.font = UIKit.Bold;
            // 본문
            bodyText = UIKit.Label(root, "Body", "", 13, TextAnchor.UpperLeft);
            bodyText.rectTransform.offsetMin = new Vector2(22, 14); bodyText.rectTransform.offsetMax = new Vector2(-34, -18);
            bodyText.verticalOverflow = VerticalWrapMode.Truncate;
            bodyText.lineSpacing = 1.2f;
            // 진행 표시
            arrow = UIKit.Label(root, "Arrow", "▼", 11, TextAnchor.LowerRight, UIKit.Accent);
            arrow.rectTransform.offsetMin = new Vector2(0, 6); arrow.rectTransform.offsetMax = new Vector2(-10, 0);
            root.gameObject.SetActive(false);
        }

        int ctxHandle = -1;
        /// <summary>
        /// 카메라가 위아래로 움직일 수 없는 한 화면짜리 방(컴퓨터 안)에서는 대화창을 화면 위쪽에 둔다
        /// (아래 대화창이 바닥의 루를 가리지 않게). 그 밖에는 아래쪽 + 카메라 재구도.
        /// </summary>
        public static bool AtTop { get; private set; }
        void Place()
        {
            var g = Game.I; var rig = CameraRig.Main;
            bool top = false;
            if (g != null && g.Map != null && rig != null && g.Player != null && g.Map.bounds.height <= rig.HalfH * 2f + 0.8f)
            {
                // 카메라가 위아래로 거의 못 움직이는 방에서, 루의 발이 아래 대화창 높이에 걸리면 위쪽에 둔다
                var cam = Camera.main;
                float vy = cam != null ? cam.WorldToViewportPoint(g.Player.transform.position).y : 1f;
                top = vy < (BoxBottom + BoxHeight) / 360f + 0.03f;
            }
            AtTop = top;
            if (top)
            {
                root.anchorMin = root.anchorMax = new Vector2(0.5f, 1); root.pivot = new Vector2(0.5f, 1); root.anchoredPosition = new Vector2(0, -BoxBottom);
                nameRt.anchorMin = nameRt.anchorMax = new Vector2(0, 0); nameRt.pivot = new Vector2(0, 1); nameRt.anchoredPosition = new Vector2(12, 4);
            }
            else
            {
                root.anchorMin = root.anchorMax = new Vector2(0.5f, 0); root.pivot = new Vector2(0.5f, 0); root.anchoredPosition = new Vector2(0, BoxBottom);
                nameRt.anchorMin = nameRt.anchorMax = new Vector2(0, 1); nameRt.pivot = new Vector2(0, 0); nameRt.anchoredPosition = new Vector2(12, -4);
            }
        }

        public void Show(bool on)
        {
            if (on && !root.gameObject.activeSelf) { openedAt = Time.unscaledTime; Place(); }
            // 대화 상태: 이 동안 Enter/Space 는 대사 진행 전용, 위험 요소는 멈춘다
            if (on && (ctxHandle < 0 || !GameInput.HasHandle(ctxHandle))) ctxHandle = GameInput.Push(InputCtx.Dialogue);
            if (!on && ctxHandle >= 0) { GameInput.Pop(ctxHandle); ctxHandle = -1; }
            root.gameObject.SetActive(on);
            if (on) root.SetAsLastSibling();
            if (!on) ui.portraits.HideAll();
        }

        void SetSpeaker(string speakerName, bool narration)
        {
            bool has = !string.IsNullOrEmpty(speakerName);
            nameRt.gameObject.SetActive(has);
            nameText.text = speakerName;
            if (has) nameRt.sizeDelta = new Vector2(Mathf.Max(60, nameText.preferredWidth + 24), 20);
            bodyText.color = narration ? UIKit.Narr : UIKit.Ink;
        }

        /// <summary>긴 문장을 상자 높이에 맞게 자연스럽게 나눈다 (단어 경계 우선).</summary>
        public List<string> Paginate(string text)
        {
            var pages = new List<string>();
            if (string.IsNullOrEmpty(text)) { pages.Add(""); return pages; }
            float maxH = Mathf.Max(20, bodyText.rectTransform.rect.height > 1 ? bodyText.rectTransform.rect.height : BoxHeight - 32);
            float width = bodyText.rectTransform.rect.width > 1 ? bodyText.rectTransform.rect.width : 568;
            var gen = new TextGenerator();
            var settings = bodyText.GetGenerationSettings(new Vector2(width, 0));
            settings.verticalOverflow = VerticalWrapMode.Overflow;
            float scale = bodyText.pixelsPerUnit > 0 ? bodyText.pixelsPerUnit : 1f;
            bool Fits(string s) => gen.GetPreferredHeight(s, settings) / scale <= maxH + 0.5f;
            if (Fits(text)) { pages.Add(text); return pages; }
            string rest = text;
            int guard = 0;
            while (rest.Length > 0 && guard++ < 20)
            {
                if (Fits(rest)) { pages.Add(rest); break; }
                int cut = rest.Length - 1;
                while (cut > 1 && !Fits(rest.Substring(0, cut))) cut--;
                // 마침표·쉼표·공백에서 끊는다
                int best = -1;
                for (int i = cut; i > cut / 2; i--)
                {
                    char c = rest[i - 1];
                    if (c == '.' || c == '!' || c == '?' || c == '…' || c == ',') { best = i; break; }
                }
                if (best < 0) for (int i = cut; i > cut / 2; i--) if (rest[i - 1] == ' ') { best = i; break; }
                if (best < 0) best = cut;
                pages.Add(rest.Substring(0, best).TrimEnd());
                rest = rest.Substring(best).TrimStart();
            }
            return pages;
        }

        /// <summary>한 줄을 타자기 효과로 출력하고 넘기기 입력을 기다린다. 출력 중 결정 키는 현재 페이지를 완성한다.</summary>
        public IEnumerator Line(string speakerName, string text, bool narration, bool alreadySeen = false)
        {
            Show(true);
            SetSpeaker(speakerName, narration);
            ui.AddLog(speakerName, text);
            bodyText.text = "";
            arrow.enabled = false;
            yield return null;   // 레이아웃 계산 + 이전 프레임의 확인 입력이 바로 넘기지 않도록
            var pages = Paginate(text);
            for (int p = 0; p < pages.Count; p++)
            {
                string page = pages[p];
                float cps = 23f * GameSettings.TextSpeed;
                float shown = 0;
                int blip = 0;
                arrow.enabled = false;
                while (shown < page.Length)
                {
                    bool ff = alreadySeen && GameInput.FastForward;
                    shown += Time.unscaledDeltaTime * cps * (ff ? 8 : 1);
                    int n = Mathf.Min(page.Length, (int)shown);
                    if (n / 3 > blip) { blip = n / 3; if (!narration) Sfx.Play("sfx_text", 0.35f, 1f + (blip % 3) * 0.04f); }
                    bodyText.text = page.Substring(0, n);
                    // 대화창이 막 열린 순간의 같은 입력은 무시한다
                    if (GameInput.AdvanceDown && Time.unscaledTime - openedAt > 0.12f) { shown = page.Length; break; }
                    yield return null;
                }
                bodyText.text = page;
                arrow.text = p < pages.Count - 1 ? "▶" : "▼";
                yield return null;   // 완성에 쓴 입력과 다음 진행 입력을 분리
                float blink = 0;
                while (true)
                {
                    blink += Time.unscaledDeltaTime;
                    arrow.enabled = (blink % 0.8f) < 0.5f;
                    if (GameInput.AdvanceDown) break;
                    if (alreadySeen && GameInput.FastForward && blink > 0.05f) break;
                    if (GameInput.AutoReadDelay > 0 && blink >= GameInput.AutoReadDelay + page.Length * 0.03f) break;
                    yield return null;
                }
                Sfx.Play("text", 0.6f);
            }
        }

        public void SetBodyImmediate(string speakerName, string text)
        {
            Show(true);
            SetSpeaker(speakerName, string.IsNullOrEmpty(speakerName));
            bodyText.text = text;
            arrow.enabled = false;
        }
    }

    // ================================================================ 스탠딩 일러스트 층
    /// <summary>
    /// 고해상도 스탠딩. 도트 렌더 텍스처를 거치지 않는 별도 캔버스(화면 실제 픽셀 기준)에 그려 얼굴이 뭉개지지 않는다.
    /// 왼쪽/오른쪽 자리, 표정 교체, 말하는 인물 강조(듣는 인물은 어둡게), 숨기기.
    /// 그림 비율을 유지하고 화면 높이의 일정 비율로 맞춰 얼굴이 잘리지 않게 한다.
    /// </summary>
    public class PortraitLayer
    {
        class Slot { public RectTransform rt; public Image img; public string who; public float alpha, targetAlpha, dim, targetDim; public float slide; }
        readonly Canvas canvas;
        readonly RectTransform frame;
        readonly Slot left, right;
        public bool Visible => left.targetAlpha > 0 || right.targetAlpha > 0;
        public string LeftWho => left.targetAlpha > 0 ? left.who : null;
        public string RightWho => right.targetAlpha > 0 ? right.who : null;
        /// <summary>화면 높이 대비 스탠딩 높이.</summary>
        public float heightRatio = 0.82f;

        public PortraitLayer(Transform parent)
        {
            var go = new GameObject("PortraitCanvas", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9;     // 도트 UI(10, 대화창) 아래, 게임 화면(-100) 위
            go.AddComponent<CanvasScaler>();   // 기본: 화면 픽셀 1:1
            frame = UIKit.Rect("Frame", go.transform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            frame.pivot = Vector2.zero;
            left = MakeSlot("Left", true);
            right = MakeSlot("Right", false);
        }

        Slot MakeSlot(string name, bool isLeft)
        {
            var rt = UIKit.Rect(name, frame, new Vector2(isLeft ? 0 : 1, 0), new Vector2(isLeft ? 0 : 1, 0), Vector2.zero, Vector2.zero);
            rt.pivot = new Vector2(0.5f, 0);
            var img = rt.gameObject.AddComponent<Image>();
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.color = new Color(1, 1, 1, 0);
            return new Slot { rt = rt, img = img };
        }

        public void Tick()
        {
            var ps = PixelScreen.I;
            Rect r = ps != null ? ps.DisplayRect : new Rect(0, 0, Screen.width, Screen.height);
            frame.anchoredPosition = r.position;
            frame.sizeDelta = r.size;
            foreach (var s in new[] { left, right })
            {
                s.alpha = Mathf.MoveTowards(s.alpha, s.targetAlpha, Time.unscaledDeltaTime * 6f);
                s.dim = Mathf.MoveTowards(s.dim, s.targetDim, Time.unscaledDeltaTime * 5f);
                s.slide = Mathf.MoveTowards(s.slide, s.targetAlpha > 0 ? 0 : 1, Time.unscaledDeltaTime * 6f);
                float h = r.height * heightRatio;
                float w = h * (s.img.sprite != null ? s.img.sprite.rect.width / s.img.sprite.rect.height : 0.66f);
                s.rt.sizeDelta = new Vector2(w, h);
                bool isLeft = s == left;
                float x = (isLeft ? 1 : -1) * (r.width * 0.16f) + (isLeft ? -1 : 1) * s.slide * r.width * 0.04f;
                s.rt.anchoredPosition = new Vector2(x, 0);
                float k = 1f - 0.45f * s.dim;
                s.img.color = new Color(k, k, k, s.alpha);
                s.rt.localScale = Vector3.one * (1f - 0.03f * s.dim);
                s.img.enabled = s.alpha > 0.001f && s.img.sprite != null;
            }
        }

        /// <summary>대사 한 줄에 맞춰 스탠딩을 갱신한다.</summary>
        public void Apply(DLine l, DialogueDB db)
        {
            if (l.hide) { HideAll(); return; }
            if (string.IsNullOrEmpty(l.e)) { Focus(null); return; }   // 표정 지정이 없는 줄: 그대로 두고 강조만 해제
            string who = string.IsNullOrEmpty(l.p) ? l.s : l.p;
            string side = string.IsNullOrEmpty(l.pos) ? db.SideOf(who) : l.pos;
            Show(who, l.e, side == "L");
            Focus(who);
        }

        public void Show(string who, string expr, bool onLeft)
        {
            var cfg = GameConfig.Load();
            var sp = cfg != null ? cfg.GetPortrait(who, expr) : null;
            if (sp == null) return;
            var s = onLeft ? left : right;
            var other = onLeft ? right : left;
            if (other.who == who) other.targetAlpha = 0;
            if (s.who != who) { s.alpha = 0; s.slide = 1; }
            s.who = who;
            s.img.sprite = sp;
            s.targetAlpha = 1;
        }

        /// <summary>말하는 인물만 밝게. null 이면 모두 보통 밝기.</summary>
        public void Focus(string who)
        {
            foreach (var s in new[] { left, right })
                s.targetDim = who == null || s.targetAlpha <= 0 ? 0 : (s.who == who ? 0 : 1);
        }

        public void Hide(string who) { foreach (var s in new[] { left, right }) if (s.who == who) s.targetAlpha = 0; }
        public void HideAll() { left.targetAlpha = 0; right.targetAlpha = 0; }
    }

    // ================================================================ 시스템 창 (강제 전송 등)
    /// <summary>작중 장치 프로그램의 짧은 상태 창. 실제 명령 실행·네트워크 접속은 하지 않는 연출용 UI 다.</summary>
    public class SystemWindow
    {
        public readonly RectTransform root;
        readonly Text title, body;
        readonly Image bar;
        readonly RectTransform barRt;
        public SystemWindow(Transform parent, string titleText, Vector2 pos, Vector2 size)
        {
            root = UIKit.Rect("SysWindow", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UIKit.Place(root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            UIKit.Solid(root, "Shadow", new Color(0, 0, 0, 0.5f)).rectTransform.offsetMin = new Vector2(4, -4);
            bgImg = UIKit.Solid(root, "Bg", new Color(0.12f, 0.06f, 0.08f, 0.97f));
            var head = UIKit.Rect("Head", root, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -16), Vector2.zero);
            headImg = UIKit.Solid(head, "Bg", new Color(0.72f, 0.16f, 0.2f, 1f));
            title = UIKit.Label(head, "T", titleText, 11, TextAnchor.MiddleLeft, Color.white);
            title.rectTransform.offsetMin = new Vector2(6, 0);
            body = UIKit.Label(root, "Body", "", 11, TextAnchor.UpperLeft, new Color(1f, 0.85f, 0.85f));
            body.rectTransform.offsetMin = new Vector2(8, 22); body.rectTransform.offsetMax = new Vector2(-8, -20);
            var track = UIKit.Rect("Track", root, new Vector2(0, 0), new Vector2(1, 0), new Vector2(8, 8), new Vector2(-8, 16));
            UIKit.Solid(track, "Bg", new Color(0.3f, 0.1f, 0.12f));
            bar = UIKit.Solid(track, "Fill", new Color(1f, 0.4f, 0.4f));
            barRt = bar.rectTransform; barRt.anchorMax = new Vector2(0, 1);
            root.localScale = Vector3.zero;
        }
        readonly Image bgImg, headImg;
        /// <summary>창 색 (기본은 붉은 경고창). 작은 알림은 차분한 색으로.</summary>
        public void Style(Color head, Color bg, Color text) { headImg.color = head; bgImg.color = bg; body.color = text; bar.transform.parent.gameObject.SetActive(false); }
        public void SetBody(string s) { body.text = s; }
        public void SetProgress(float k) { barRt.anchorMax = new Vector2(Mathf.Clamp01(k), 1); }
        public IEnumerator Pop(bool open, float time = 0.15f)
        {
            for (float t = 0; t < time; t += Time.unscaledDeltaTime)
            {
                float k = t / time; if (!open) k = 1 - k;
                root.localScale = new Vector3(Mathf.Lerp(0.6f, 1f, k), Mathf.Lerp(0.1f, 1f, k), 1) * (k > 0.01f ? 1 : 0);
                yield return null;
            }
            root.localScale = open ? Vector3.one : Vector3.zero;
        }
        public void Destroy() { if (root != null) Object.Destroy(root.gameObject); }
    }

}
