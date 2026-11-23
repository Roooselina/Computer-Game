using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>
    /// 'E-D05-47 안내서' 문서 뷰어 (컴퓨터 안 바탕화면의 문서, 이후 일시정지 메뉴 '안내서'에서도 다시 본다).
    /// - J 로 열고 닫는다 (Esc 도 닫기). A/D 로 쪽을 넘긴다.
    /// - 쪽: 기본 조작 / Q·E·R·F 슬롯의 기술 / 추가 기술. 키 글자는 실제 바인딩, 비용·재사용 시간·사거리는
    ///   action_tuning.json 에서 바로 읽는다 (문서와 실제 동작이 어긋나지 않게).
    /// - 미리보기는 UI 안에서만 움직이는 작은 시연이다 (월드에 판정·소리·소음을 만들지 않는다).
    /// - 열려 있는 동안 위험 시간이 멈춘다 (Menu 상태 + TimeCtl).
    /// </summary>
    public static class GuideBook
    {
        public static bool Open { get; private set; }
        public static int Page { get; private set; }

        class Ctx
        {
            public RectTransform root, preview;
            public Text title, body, pageNo, keys;
            public Image icon, keyChipBg; public Text keyChip;
            public readonly List<GameObject> prevObjs = new List<GameObject>();
            public string kind; public float t;
            public Image lu, target, fx, dot, src; public RectTransform boltRoot;
        }

        static List<string> PageIds(Game g)
        {
            var l = new List<string> { "basics" };
            var slots = g.State != null ? g.State.D.slots : null;
            var seen = new HashSet<string>();
            if (slots != null) foreach (var s in slots) if (!string.IsNullOrEmpty(s) && seen.Add(s)) l.Add(s);
            foreach (var s in PlayerSkills.DefaultSkills) if (seen.Add(s)) l.Add(s);
            return l;
        }

        public static IEnumerator Run(Game g, int startPage = 0)
        {
            if (Open) yield break;
            Open = true;
            TimeCtl.Set("guide", 0f);
            var c = Build(g);
            var token = InputCtxToken.Attach(c.root.gameObject, InputCtx.Menu);
            var pages = PageIds(g);
            Page = Mathf.Clamp(startPage, 0, pages.Count - 1);
            Show(g, c, pages[Page], Page, pages.Count);
            Sfx.Play("sfx_window_pop", 0.6f);
            float opened = Time.unscaledTime;
            yield return null;
            while (true)
            {
                if (MenuList.ScriptedChoices.Count > 0 && MenuList.ScriptedChoices.Peek() == -3) { MenuList.ScriptedChoices.Dequeue(); break; }
                int d = GameInput.NavHorizontal();
                if (d != 0)
                {
                    int np = Mathf.Clamp(Page + d, 0, pages.Count - 1);
                    if (np != Page) { Page = np; Show(g, c, pages[Page], Page, pages.Count); Sfx.Play("move", 0.6f); }
                }
                if (Time.unscaledTime - opened > 0.25f && (GameInput.ConfirmDown || GameInput.CancelDown)) break;
                TickPreview(g, c);
                yield return null;
            }
            Sfx.Play("cancel", 0.5f);
            token.Release();
            Object.Destroy(c.root.gameObject);
            TimeCtl.Clear("guide");
            Open = false;
            if (g.State != null) g.State.Set("pc_guide_read");
        }

        static Ctx Build(Game g)
        {
            var c = new Ctx();
            c.root = UIKit.Rect("GuideBook", g.UI.Root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UIKit.Place(c.root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 8), new Vector2(470, 262));
            UIKit.Solid(c.root, "Shadow", new Color(0, 0, 0, 0.55f)).rectTransform.offsetMin = new Vector2(4, -4);
            UIKit.Panel(c.root, "Bg", new Color(0.9f, 0.92f, 0.96f, 0.98f));
            // 창 제목줄 (단말 문서 뷰어)
            var bar = UIKit.Rect("Bar", c.root, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -18), Vector2.zero);
            UIKit.Solid(bar, "B", new Color(0.18f, 0.3f, 0.5f, 1f));
            var bt = UIKit.Label(bar, "T", "E-D05-47 안내서.doc — 문서 보기", 10, TextAnchor.MiddleLeft, new Color(0.92f, 0.96f, 1f));
            bt.rectTransform.offsetMin = new Vector2(8, 0);
            var close = UIKit.Label(bar, "X", GameInput.Label(Act.Interact) + " / " + GameInput.Label(Act.Cancel) + " 닫기", 9, TextAnchor.MiddleRight, new Color(0.85f, 0.9f, 1f));
            close.rectTransform.offsetMax = new Vector2(-8, 0);
            c.title = UIKit.Label(c.root, "Title", "", 15, TextAnchor.UpperLeft, new Color(0.1f, 0.12f, 0.2f));
            c.title.font = UIKit.Bold;
            c.title.rectTransform.offsetMin = new Vector2(46, 0); c.title.rectTransform.offsetMax = new Vector2(-170, -24);
            var ir = UIKit.Rect("Icon", c.root, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, Vector2.zero);
            UIKit.Place(ir, new Vector2(0, 1), new Vector2(0, 1), new Vector2(12, -24), new Vector2(28, 28));
            UIKit.Solid(ir, "Bg", new Color(0.12f, 0.15f, 0.24f));
            var ii = UIKit.Rect("I", ir, Vector2.zero, Vector2.one, new Vector2(3, 3), new Vector2(-3, -3));
            c.icon = ii.gameObject.AddComponent<Image>(); c.icon.preserveAspect = true;
            // 키 칩 (실제 키)
            var kr = UIKit.Rect("KeyChip", c.root, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, Vector2.zero);
            UIKit.Place(kr, new Vector2(0, 1), new Vector2(0, 1), new Vector2(46, -45), new Vector2(54, 14));
            c.keyChipBg = UIKit.Solid(kr, "Bg", new Color(0.15f, 0.18f, 0.28f));
            c.keyChip = UIKit.Label(kr, "K", "", 10, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.5f));
            c.keyChip.font = UIKit.Bold;
            c.body = UIKit.Label(c.root, "Body", "", 11, TextAnchor.UpperLeft, new Color(0.12f, 0.14f, 0.22f));
            c.body.rectTransform.offsetMin = new Vector2(14, 28); c.body.rectTransform.offsetMax = new Vector2(-176, -64);
            c.body.lineSpacing = 1.15f;
            c.body.supportRichText = true;
            // 미리보기 칸 (오른쪽)
            c.preview = UIKit.Rect("Preview", c.root, new Vector2(1, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            UIKit.Place(c.preview, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-12, -28), new Vector2(152, 104));
            UIKit.Solid(c.preview, "Bg", new Color(0.06f, 0.08f, 0.13f));
            var pl = UIKit.Label(c.preview, "L", "미리보기 (시연 화면)", 8, TextAnchor.UpperLeft, new Color(0.55f, 0.65f, 0.8f));
            pl.rectTransform.offsetMin = new Vector2(4, 0); pl.rectTransform.offsetMax = new Vector2(0, -2);
            var floor = UIKit.Rect("Floor", c.preview, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 18));
            UIKit.Solid(floor, "F", new Color(0.16f, 0.2f, 0.3f));
            c.keys = UIKit.Label(c.root, "Keys", "", 9, TextAnchor.UpperLeft, new Color(0.25f, 0.3f, 0.42f));
            c.keys.rectTransform.anchorMin = new Vector2(1, 1); c.keys.rectTransform.anchorMax = new Vector2(1, 1);
            c.keys.rectTransform.pivot = new Vector2(1, 1);
            c.keys.rectTransform.anchoredPosition = new Vector2(-12, -138); c.keys.rectTransform.sizeDelta = new Vector2(152, 90);
            c.pageNo = UIKit.Label(c.root, "Page", "", 9, TextAnchor.LowerCenter, new Color(0.3f, 0.35f, 0.5f));
            c.pageNo.rectTransform.offsetMin = new Vector2(0, 6);
            c.root.SetAsLastSibling();
            return c;
        }

        static Sprite Set(Game g, string n) { var s = g.Config.GetSet(n); return s != null && s.frames != null && s.frames.Length > 0 ? s.frames[0] : null; }
        static Sprite[] Frames(Game g, string n) { var s = g.Config.GetSet(n); return s != null ? s.frames : null; }

        static string K(Act a) => "<b><color=#1a3a7a>[" + GameInput.Label(a) + "]</color></b>";

        static void Show(Game g, Ctx c, string id, int page, int count)
        {
            foreach (var o in c.prevObjs) if (o != null) Object.Destroy(o);
            c.prevObjs.Clear(); c.t = 0; c.kind = id;
            c.pageNo.text = "◀ " + GameInput.Label(Act.Left) + "   " + (page + 1) + " / " + count + "   " + GameInput.Label(Act.Right) + " ▶";
            if (id == "basics")
            {
                c.title.text = "기본 조작";
                c.icon.sprite = g.Config.GetArt("lu") != null ? g.Config.GetArt("lu").defaultSprite : null;
                c.keyChip.text = "이동";
                c.body.text =
                    K(Act.Left) + K(Act.Right) + " 좌우 이동\n" +
                    K(Act.Jump) + " 대상이 없으면 점프\n" +
                    K(Act.Down) + " 웅크리기 · 엄폐물 뒤에서 숨기\n" +
                    K(Act.Dodge) + " · 대시 (짧은 무적, 잠깐 다시 못 씀)\n" +
                    K(Act.Interact) + " 조사 · 대화 · 문 (점프보다 우선)\n" +
                    K(Act.MenuUp) + K(Act.Down) + " 사다리 · " + GameInput.Pair(Act.Left, Act.Right) + " 이탈 · 단말 자유이동\n" +
                    K(Act.Cancel) + " 닫기 · 일시정지     " + K(Act.Skip) + " 연출 건너뛰기\n\n" +
                    "기술은 " + K(Act.Skill1) + K(Act.Skill2) + K(Act.Skill3) + K(Act.Skill4) + " 를 누르면 바로 나간다.\n" +
                    "전하(◆)는 시간이 지나면 저절로 찬다. 외부 전원은 강한 출력용이다.";
                c.keys.text = "대화: " + GameInput.Label(Act.Interact) + " 다음 · " + GameInput.Pair(Act.Left, Act.Right) + " 선택지\n" + GameInput.Label(Act.Interact) + " 문장 완성 / 다음 · " + GameInput.Label(Act.Skip) + " 연출 스킵";
                BuildPreview(g, c, "basics");
                return;
            }
            var s = ActionTuning.I.Skill(id);
            if (s == null) { c.title.text = id; c.body.text = ""; return; }
            int slot = g.State != null ? g.State.D.slots.IndexOf(id) : -1;
            c.title.text = s.name;
            c.icon.sprite = Set(g, "UI_Skill_" + id);
            c.keyChip.text = slot >= 0 ? GameInput.Label(GameInput.SkillAct(slot)) + " 누르기" : "미장착";
            string cost = s.cost > 0 ? "전하 ◆" + s.cost : "전하 없음 (자체 전기)";
            string cd = s.cooldown.ToString("0.0#") + "초";
            string effect = s.kind == "targeted" ? "\n효과 시간: CCTV·제어 장치 " + s.effectTime.ToString("0") + "초" : "";
            string howTo = s.kind == "targeted" ? "바라보는 쪽의 보이는 대상을 자동으로 고른다 (화면에 괄호 표시). 길게 누르면 직접 고르기."
                : s.kind == "source" ? "화면 안 '끌어올 전력 있음' 전원과 닿는 목표를 자동으로 잇는다. 전원이 없으면 쓰지 않는다."
                : s.kind == "wire" ? "전원선 입구 가까이에서 누른다. 닿아 있는 동료도 함께 이동한다."
                : s.kind == "projectile" ? "바라보는 방향으로 곧게 날아간다."
                : "바라보는 방향 바로 앞에 판정이 생긴다.";
            c.body.text =
                "<b>역할</b>  " + s.desc + "\n" +
                "<b>대상</b>  " + (s.target ?? "-") + "\n" +
                "<b>비용</b>  " + cost + "    <b>재사용</b>  " + cd + effect + "\n" +
                "<b>쓰는 법</b>  " + howTo + "\n\n" +
                "<color=#2b5a2b><b>예)</b> " + System.Text.RegularExpressions.Regex.Replace(s.example ?? "", @"\b[WER]\b", m => GameInput.Label(m.Value == "W" ? Act.Skill2 : m.Value == "E" ? Act.Skill3 : Act.Skill4)) + "</color>";
            c.keys.text = slot >= 0 ? "슬롯: " + GameInput.Label(GameInput.SkillAct(slot)) : "일시정지 메뉴 '기술 장착'에서\nQ/E/R/F 슬롯에 넣을 수 있다.";
            BuildPreview(g, c, s.kind == "attack" ? "tok" : s.kind);
        }

        static Image Img(Ctx c, string name, Sprite sp, Vector2 pos, Vector2 size, Color? col = null)
        {
            var rt = UIKit.Rect(name, c.preview, new Vector2(0, 0), new Vector2(0, 0), Vector2.zero, Vector2.zero);
            rt.pivot = new Vector2(0.5f, 0); rt.anchoredPosition = pos; rt.sizeDelta = size;
            var im = rt.gameObject.AddComponent<Image>(); im.sprite = sp; im.preserveAspect = true; im.raycastTarget = false;
            im.color = col ?? Color.white;
            c.prevObjs.Add(rt.gameObject);
            return im;
        }

        static void BuildPreview(Game g, Ctx c, string kind)
        {
            var luArt = g.Config.GetArt("lu");
            c.lu = Img(c, "Lu", luArt != null ? luArt.defaultSprite : null, new Vector2(34, 18), new Vector2(36, 48));
            c.fx = Img(c, "Fx", null, new Vector2(60, 34), new Vector2(26, 26)); c.fx.enabled = false;
            c.target = null; c.src = null; c.dot = null;
            var br = UIKit.Rect("Bolt", c.preview, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            br.pivot = Vector2.zero; c.boltRoot = br; c.prevObjs.Add(br.gameObject);
            switch (kind)
            {
                case "tok": case "projectile": case "basics":
                    c.target = Img(c, "Dummy", Set(g, "UI_Prev_Dummy"), new Vector2(kind == "projectile" ? 128 : 80, 18), new Vector2(24, 36)); break;
                case "targeted":
                    c.target = Img(c, "Cam", Set(g, "UI_Prev_Cctv"), new Vector2(116, 62), new Vector2(28, 22)); break;
                case "source":
                    c.src = Img(c, "Src", Set(g, "UI_Prev_Source"), new Vector2(76, 18), new Vector2(22, 30));
                    c.target = Img(c, "Panel", Set(g, "UI_Prev_Panel"), new Vector2(124, 18), new Vector2(24, 40)); break;
                case "wire":
                    c.src = Img(c, "SockA", Set(g, "UI_Prev_Socket"), new Vector2(56, 18), new Vector2(14, 18));
                    c.target = Img(c, "SockB", Set(g, "UI_Prev_Socket"), new Vector2(128, 66), new Vector2(14, 18));
                    c.dot = Img(c, "Dot", Set(g, "FX_LightDot"), new Vector2(56, 30), new Vector2(14, 14)); c.dot.enabled = false; break;
            }
        }

        /// <summary>미리보기 한 주기 (2초): 준비 → 사용 → 결과. 판정·소리·소음 없음.</summary>
        static void TickPreview(Game g, Ctx c)
        {
            c.t += Time.unscaledDeltaTime;
            float t = c.t % 2.2f;
            foreach (Transform ch in c.boltRoot) Object.Destroy(ch.gameObject);
            var spark = Frames(g, "FX_Spark"); var boom = Frames(g, "FX_Explosion");
            Sprite Fr(Sprite[] fr, float k) => fr != null && fr.Length > 0 ? fr[Mathf.Clamp((int)(k * fr.Length), 0, fr.Length - 1)] : null;
            switch (c.kind)
            {
                case "basics":
                    {
                        // 걷기 → 점프 → 웅크림
                        float x = 34 + Mathf.PingPong(c.t * 30f, 40f);
                        float y = 18 + (t > 1.0f && t < 1.5f ? Mathf.Sin((t - 1.0f) / 0.5f * Mathf.PI) * 22f : 0);
                        c.lu.rectTransform.anchoredPosition = new Vector2(x, y);
                        c.lu.rectTransform.sizeDelta = t > 1.7f ? new Vector2(36, 34) : new Vector2(36, 48);
                        break;
                    }
                case "tok":
                    {
                        bool hit = t > 0.6f && t < 0.9f;
                        c.fx.enabled = hit; c.fx.sprite = Fr(spark, (t - 0.6f) / 0.3f);
                        c.fx.rectTransform.anchoredPosition = new Vector2(64, 30);
                        if (c.target != null) c.target.color = t > 0.65f && t < 1.3f ? new Color(1f, 1f, 0.55f) : Color.white;
                        if (c.target != null) c.target.rectTransform.anchoredPosition = new Vector2(80 + (t > 0.65f && t < 1.3f ? Mathf.Sin(t * 60f) * 1.5f : 0), 18);
                        break;
                    }
                case "projectile":
                    {
                        bool fly = t > 0.5f && t < 1.1f;
                        c.fx.enabled = fly || (t >= 1.1f && t < 1.35f);
                        float k = Mathf.Clamp01((t - 0.5f) / 0.6f);
                        c.fx.sprite = Fr(spark, (t * 4f) % 1f);
                        c.fx.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(56, 126, k), 32);
                        if (c.target != null) c.target.color = t > 1.1f && t < 1.6f ? new Color(1f, 1f, 0.55f) : Color.white;
                        break;
                    }
                case "targeted":
                    {
                        if (t > 0.5f && t < 0.75f) Zig(c.boltRoot, new Vector2(52, 44), new Vector2(116, 70), new Color(0.6f, 0.95f, 1f));
                        bool off = t > 0.75f && t < 2.0f;
                        if (c.target != null) { c.target.sprite = Set(g, off ? "UI_Prev_CctvOff" : "UI_Prev_Cctv"); c.target.rectTransform.localRotation = Quaternion.Euler(0, 0, off ? -25 : Mathf.Sin(c.t * 1.5f) * 12f); }
                        c.lu.rectTransform.anchoredPosition = new Vector2(34 + (off ? Mathf.Min(70, (t - 0.75f) * 70f) : 0), 18);
                        break;
                    }
                case "source":
                    {
                        if (t > 0.5f && t < 0.75f) Zig(c.boltRoot, new Vector2(52, 44), new Vector2(76, 40), new Color(0.6f, 0.95f, 1f));
                        if (t > 0.75f && t < 1.05f) Zig(c.boltRoot, new Vector2(80, 40), new Vector2(124, 40), new Color(1f, 0.95f, 0.55f));
                        bool b = t > 1.05f && t < 1.5f;
                        c.fx.enabled = b; c.fx.sprite = Fr(boom, (t - 1.05f) / 0.45f);
                        c.fx.rectTransform.anchoredPosition = new Vector2(124, 30); c.fx.rectTransform.sizeDelta = new Vector2(40, 40);
                        if (c.target != null) c.target.color = t > 1.05f ? new Color(0.45f, 0.45f, 0.5f) : Color.white;
                        if (c.src != null) c.src.color = t > 0.75f ? new Color(0.6f, 0.6f, 0.65f) : Color.white;
                        break;
                    }
                case "wire":
                    {
                        bool inside = t > 0.5f && t < 1.4f;
                        c.lu.enabled = !inside;
                        c.dot.enabled = inside;
                        float k = Mathf.Clamp01((t - 0.5f) / 0.9f);
                        Vector2 a = new Vector2(56, 30), m = new Vector2(92, 82), bb = new Vector2(128, 78);
                        Vector2 pos = k < 0.5f ? Vector2.Lerp(a, m, k * 2) : Vector2.Lerp(m, bb, (k - 0.5f) * 2);
                        c.dot.rectTransform.anchoredPosition = pos;
                        Line(c.boltRoot, a, m, new Color(0.3f, 0.32f, 0.38f), 2); Line(c.boltRoot, m, bb, new Color(0.3f, 0.32f, 0.38f), 2);
                        c.lu.rectTransform.anchoredPosition = t >= 1.4f ? new Vector2(128, 66) : new Vector2(56, 18);
                        break;
                    }
            }
        }

        static void Zig(RectTransform root, Vector2 a, Vector2 b, Color col)
        {
            int n = 5; Vector2 prev = a;
            Vector2 d = b - a; Vector2 nrm = new Vector2(-d.y, d.x).normalized;
            for (int i = 1; i <= n; i++)
            {
                Vector2 p = a + d * (i / (float)n) + (i < n ? nrm * Random.Range(-4f, 4f) : Vector2.zero);
                Line(root, prev, p, col, 2); prev = p;
            }
        }

        static void Line(RectTransform root, Vector2 a, Vector2 b, Color col, float w)
        {
            var rt = UIKit.Rect("L", root, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = a;
            Vector2 d = b - a;
            rt.sizeDelta = new Vector2(d.magnitude, w);
            rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            var im = rt.gameObject.AddComponent<Image>(); im.color = col; im.raycastTarget = false;
        }
    }
}
