using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>
    /// 탐험 HUD (2026-11-15 개편). 정보를 한 덩어리로 상시 띄우지 않고 자리를 나눈다.
    /// - 왼쪽 위: 작은 루 표시 + 안정도 바, 바로 아래 전하 칸
    /// - 오른쪽 아래: Q/E/R/F 스킬 슬롯 4칸 — 기술 아이콘, 실제 키, 재사용 시간(어두운 덮개), 전하 비용, 쓸 수 없는 이유(짧게)
    /// - 화면 안: 지금 W/E/R 이 고를 대상 하나만 모서리 괄호로 강조 (키 글자 붙임). 대상이 없으면 표시하지 않는다.
    /// - 위 가운데: 발각 위험이 있을 때만 상태 표시 (의심 / 확인 중 / 발견 / 수색 / 경비 호출, 엄폐 중이면 '숨음')
    /// - 오른쪽 위: 시설 경보 (색만이 아니라 깜빡이는 삼각형 모양으로)
    /// - 감시자 머리 위: ?/! 표시와 게이지
    /// - 정밀 선택 중: 화면 아래 가운데에 공급원·목표·비용·예상 결과·실행 키
    /// - 동료: 실제 합류 이후 안정도가 줄었을 때만
    /// </summary>
    public class Hud
    {
        readonly UIRoot ui;
        readonly RectTransform root, luRt, skillRt, detectRt, alarmRt, targetRt, hintRt, venRt, failRt;
        readonly Image luIcon, hpFill, hpBack, venFill;
        readonly Text dashText;
        readonly Text hpText, detectText, alarmText, targetText, hintText, venText, failText;
        readonly List<Image> pips = new List<Image>();
        class Slot { public RectTransform rt; public Image bg, icon, cd, frame; public Text key, name; public List<Image> cost = new List<Image>(); public float flash; }
        readonly Slot[] slots = new Slot[4];
        readonly List<(RectTransform rt, Text t, Image fill)> marks = new List<(RectTransform, Text, Image)>();
        // 자동 대상 강조 (모서리 괄호 4개 + 키 글자)
        readonly RectTransform suggestRt; readonly Image[] corners = new Image[4]; readonly Text suggestKey;
        readonly RectTransform suggestRt2; readonly Image[] corners2 = new Image[4]; readonly Text suggestKey2;
        bool visible = true;
        string hint = "";
        public string SuggestText { get; private set; } = "";

        public Hud(Transform parent, UIRoot owner)
        {
            ui = owner;
            root = UIKit.Fill("Hud", parent);

            // ---- 왼쪽 위: 루 + 안정도 + 전하
            luRt = UIKit.Rect("Lu", root, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, Vector2.zero);
            UIKit.Place(luRt, new Vector2(0, 1), new Vector2(0, 1), new Vector2(8, -8), new Vector2(132, 30));
            var iconBg = UIKit.Rect("IconBg", luRt, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, Vector2.zero);
            UIKit.Place(iconBg, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(26, 26));
            UIKit.Panel(iconBg, "Bg", new Color(0.08f, 0.1f, 0.14f, 0.85f), true);
            var ic = UIKit.Rect("Icon", iconBg, Vector2.zero, Vector2.one, new Vector2(2, 2), new Vector2(-2, -2));
            luIcon = ic.gameObject.AddComponent<Image>(); luIcon.preserveAspect = true; luIcon.raycastTarget = false;
            var bar = UIKit.Rect("Bar", luRt, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, Vector2.zero);
            UIKit.Place(bar, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -3), new Vector2(84, 7));
            hpBack = UIKit.Solid(bar, "Back", new Color(0.05f, 0.06f, 0.09f, 0.9f));
            hpFill = UIKit.Solid(bar, "Fill", new Color(0.55f, 0.95f, 0.75f));
            hpFill.rectTransform.anchorMax = new Vector2(1, 1);
            hpFill.rectTransform.offsetMin = new Vector2(1, 1); hpFill.rectTransform.offsetMax = new Vector2(-1, -1);
            hpText = UIKit.Label(luRt, "Hp", "", 9, TextAnchor.UpperLeft, UIKit.Dim);
            hpText.rectTransform.offsetMin = new Vector2(117, 0); hpText.rectTransform.offsetMax = new Vector2(40, -1);
            hpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            for (int i = 0; i < 8; i++)
            {
                var pr = UIKit.Rect("Pip" + i, luRt, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, Vector2.zero);
                UIKit.Place(pr, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30 + i * 9, -14), new Vector2(7, 7));
                var im = pr.gameObject.AddComponent<Image>(); im.raycastTarget = false;
                pr.localRotation = Quaternion.Euler(0, 0, 45);
                pr.anchoredPosition += new Vector2(3, -3);
                pr.sizeDelta = new Vector2(5, 5);
                pips.Add(im);
            }
            dashText = UIKit.Label(luRt, "DashKeys", "", 9, TextAnchor.UpperLeft, UIKit.Dim);
            UIKit.Place(dashText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -24), new Vector2(155, 12));
            // 동료 (합류 이후, 필요할 때만)
            venRt = UIKit.Rect("Ven", root, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, Vector2.zero);
            UIKit.Place(venRt, new Vector2(0, 1), new Vector2(0, 1), new Vector2(38, -36), new Vector2(84, 14));
            venText = UIKit.Label(venRt, "T", "베넬", 9, TextAnchor.MiddleLeft, UIKit.Dim);
            var vb = UIKit.Rect("Bar", venRt, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(26, -2), new Vector2(0, 2));
            UIKit.Solid(vb, "Back", new Color(0.05f, 0.06f, 0.09f, 0.9f));
            venFill = UIKit.Solid(vb, "Fill", new Color(0.75f, 0.6f, 0.95f));

            // ---- 오른쪽 아래: Q/E/R/F 슬롯
            skillRt = UIKit.Rect("Skills", root, new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, Vector2.zero);
            UIKit.Place(skillRt, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-8, 8), new Vector2(4 * 34 - 4, 44));
            for (int i = 0; i < 4; i++)
            {
                var sl = new Slot();
                sl.rt = UIKit.Rect("Slot" + i, skillRt, new Vector2(0, 0), new Vector2(0, 0), Vector2.zero, Vector2.zero);
                UIKit.Place(sl.rt, new Vector2(0, 0), new Vector2(0, 0), new Vector2(i * 34, 0), new Vector2(30, 30));
                sl.bg = UIKit.Panel(sl.rt, "Bg", new Color(0.07f, 0.09f, 0.13f, 0.88f), true);
                var ir = UIKit.Rect("Icon", sl.rt, Vector2.zero, Vector2.one, new Vector2(4, 4), new Vector2(-4, -4));
                sl.icon = ir.gameObject.AddComponent<Image>(); sl.icon.preserveAspect = true; sl.icon.raycastTarget = false;
                sl.cd = UIKit.Solid(sl.rt, "Cd", new Color(0f, 0f, 0f, 0.62f));
                sl.cd.rectTransform.anchorMax = new Vector2(1, 0);
                sl.frame = UIKit.Solid(sl.rt, "Frame", new Color(1f, 0.86f, 0.35f, 0f));
                sl.frame.rectTransform.offsetMin = new Vector2(-1, -1); sl.frame.rectTransform.offsetMax = new Vector2(1, 1);
                sl.frame.transform.SetAsFirstSibling();
                // 키 글자 (왼쪽 위 작은 칩)
                var kr = UIKit.Rect("KeyChip", sl.rt, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, Vector2.zero);
                UIKit.Place(kr, new Vector2(0, 1), new Vector2(0, 1), new Vector2(-3, 4), new Vector2(12, 11));
                UIKit.Solid(kr, "Bg", new Color(0.92f, 0.94f, 1f, 0.95f));
                sl.key = UIKit.Label(kr, "K", "Q", 9, TextAnchor.MiddleCenter, new Color(0.1f, 0.12f, 0.2f));
                sl.key.font = UIKit.Pixel;
                // 기술 이름 (슬롯 위, 아주 작게)
                sl.name = UIKit.Label(sl.rt, "Name", "", 8, TextAnchor.LowerCenter, UIKit.Dim);
                sl.name.rectTransform.anchorMin = new Vector2(0, 1); sl.name.rectTransform.anchorMax = new Vector2(1, 1);
                sl.name.rectTransform.offsetMin = new Vector2(-6, 1); sl.name.rectTransform.offsetMax = new Vector2(6, 12);
                sl.name.horizontalOverflow = HorizontalWrapMode.Overflow;
                for (int c = 0; c < 3; c++)
                {
                    var pr = UIKit.Rect("Cost" + c, sl.rt, new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, Vector2.zero);
                    UIKit.Place(pr, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-3 - c * 5, 3), new Vector2(3, 3));
                    pr.localRotation = Quaternion.Euler(0, 0, 45);
                    var im = pr.gameObject.AddComponent<Image>(); im.raycastTarget = false; im.color = new Color(1f, 0.88f, 0.4f);
                    sl.cost.Add(im);
                }
                slots[i] = sl;
            }
            // 실패 이유 (슬롯 위에 잠깐)
            failRt = UIKit.Rect("Fail", root, new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, Vector2.zero);
            UIKit.Place(failRt, new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(-60, 56), new Vector2(90, 14));
            UIKit.Solid(failRt, "Bg", new Color(0.3f, 0.06f, 0.06f, 0.85f));
            failText = UIKit.Label(failRt, "T", "", 9, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.78f));
            failRt.gameObject.SetActive(false);

            // ---- 자동 대상 강조
            suggestRt = MakeBracket("Suggest", corners, out suggestKey, new Color(0.6f, 0.95f, 1f));
            suggestRt2 = MakeBracket("Suggest2", corners2, out suggestKey2, new Color(1f, 0.88f, 0.4f));

            // ---- 위 가운데: 발각 상태
            detectRt = UIKit.Rect("Detect", root, new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
            UIKit.Place(detectRt, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(150, 18));
            UIKit.Panel(detectRt, "Bg", new Color(0.07f, 0.08f, 0.12f, 0.8f), true);
            detectText = UIKit.Label(detectRt, "T", "", 11, TextAnchor.MiddleCenter, UIKit.Ink);
            detectText.font = UIKit.Bold;

            // ---- 오른쪽 위: 경보
            alarmRt = UIKit.Rect("Alarm", root, new Vector2(1, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            UIKit.Place(alarmRt, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-8, -8), new Vector2(64, 16));
            UIKit.Panel(alarmRt, "Bg", new Color(0.25f, 0.04f, 0.04f, 0.8f), true);
            alarmText = UIKit.Label(alarmRt, "T", "", 10, TextAnchor.MiddleCenter, new Color(1f, 0.7f, 0.6f));
            alarmText.font = UIKit.Pixel;

            // ---- 정밀 선택 안내
            targetRt = UIKit.Rect("Target", root, new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, Vector2.zero);
            UIKit.Place(targetRt, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-60, 8), new Vector2(290, 58));
            UIKit.Panel(targetRt, "Bg", new Color(0.06f, 0.08f, 0.12f, 0.9f), true);
            targetText = UIKit.Label(targetRt, "T", "", 10, TextAnchor.UpperLeft, UIKit.Ink);
            targetText.rectTransform.offsetMin = new Vector2(8, 4); targetText.rectTransform.offsetMax = new Vector2(-8, -4);
            targetText.lineSpacing = 1.15f;
            targetRt.gameObject.SetActive(false);

            // ---- 상황 안내 (짧게)
            hintRt = UIKit.Rect("Hint", root, new Vector2(0, 0), new Vector2(0, 0), Vector2.zero, Vector2.zero);
            UIKit.Place(hintRt, new Vector2(0, 0), new Vector2(0, 0), new Vector2(8, 8), new Vector2(330, 16));
            hintText = UIKit.Label(hintRt, "T", "", 10, TextAnchor.LowerLeft, UIKit.Dim);
        }

        RectTransform bossRoot; Image bossFill, bossTrail; Text bossName;
        float displayedBossHp = 1;
        void UpdateBossBar()
        {
            var other = BossBars.Current;
            var b = other == null ? Object.FindAnyObjectByType<VenelleBoss>() : null;
            bool show = other != null || (b != null && (b.Alive || b.Downed));
            if (bossRoot == null)
            {
                bossRoot = UIKit.Rect("BossHP", root, new Vector2(0.5f,1), new Vector2(0.5f,1), Vector2.zero, Vector2.zero);
                UIKit.Place(bossRoot, new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-31), new Vector2(346,29));
                UIKit.Panel(bossRoot,"Back",new Color(0.025f,0.035f,0.045f,0.96f),true);
                bossName = UIKit.Label(bossRoot,"Name","베넬",10,TextAnchor.UpperLeft,new Color(0.92f,0.94f,0.8f));
                bossName.rectTransform.offsetMin = new Vector2(8,4); bossName.rectTransform.offsetMax = new Vector2(-8,-2);
                var track = UIKit.Rect("Track",bossRoot,Vector2.zero,Vector2.one,new Vector2(8,5),new Vector2(-8,-17));
                UIKit.Solid(track,"Empty",new Color(0.13f,0.17f,0.18f));
                bossTrail = UIKit.Solid(track,"Damage",new Color(0.96f,0.73f,0.35f));
                bossFill = UIKit.Solid(track,"Health",new Color(0.48f,0.76f,0.52f));
            }
            bossRoot.gameObject.SetActive(show);
            if (!show) return;
            if (other != null)
            {
                float r2 = other.BarMax > 0 ? Mathf.Clamp01((float)other.BarHp / other.BarMax) : 0;
                displayedBossHp = Mathf.MoveTowards(displayedBossHp, r2, Time.unscaledDeltaTime * 0.35f);
                bossFill.rectTransform.anchorMax = new Vector2(r2, 1);
                bossTrail.rectTransform.anchorMax = new Vector2(Mathf.Max(r2, displayedBossHp), 1);
                bossName.text = other.BarName + (string.IsNullOrEmpty(other.BarPhase) ? "" : "   ·   " + other.BarPhase) + "                    " + Mathf.Max(0, other.BarHp) + " / " + other.BarMax;
                return;
            }
            float ratio = Mathf.Clamp01((float)b.Hp / ActionTuning.I.venelle.hp);
            displayedBossHp = Mathf.MoveTowards(displayedBossHp,ratio,Time.unscaledDeltaTime*0.35f);
            bossFill.rectTransform.anchorMax = new Vector2(ratio,1);
            bossTrail.rectTransform.anchorMax = new Vector2(Mathf.Max(ratio,displayedBossHp),1);
            bossName.text = "베넬   ·   " + (ratio <= 0.45f ? "불안정한 혼합" : "화학 개조체") + "                         " + b.Hp + " / " + ActionTuning.I.venelle.hp;
        }

        Text reviewTag;
        /// <summary>검수 모드 표시 (작게, 오른쪽 위 경보 아래).</summary>
        void UpdateReviewTag()
        {
            if (reviewTag == null)
            {
                reviewTag = UIKit.Label(ui.Root, "ReviewTag", "", 9, TextAnchor.UpperRight, new Color(1f, 0.8f, 0.4f));
                UIKit.Place(reviewTag.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-8, -26), new Vector2(300, 12));
                reviewTag.raycastTarget = false;
            }
            bool on = ReviewMode.Active;
            reviewTag.gameObject.SetActive(on);
            if (on) reviewTag.text = "검수 모드 · " + ReviewMode.Destination;
        }

        RectTransform MakeBracket(string name, Image[] cs, out Text key, Color c)
        {
            var rt = UIKit.Rect(name, ui.worldLayer, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
            for (int i = 0; i < 4; i++)
            {
                var cr = UIKit.Rect("C" + i, rt, new Vector2(i % 2, i / 2), new Vector2(i % 2, i / 2), Vector2.zero, Vector2.zero);
                cr.pivot = new Vector2(i % 2, i / 2); cr.sizeDelta = new Vector2(5, 5);
                var im = cr.gameObject.AddComponent<Image>(); im.raycastTarget = false; im.color = c;
                // L 자 모양: 가로·세로 막대 두 개
                var h = UIKit.Solid(cr, "H", c); h.rectTransform.anchorMin = new Vector2(0, i / 2); h.rectTransform.anchorMax = new Vector2(1, i / 2); h.rectTransform.pivot = new Vector2(0.5f, i / 2); h.rectTransform.sizeDelta = new Vector2(0, 1.5f);
                var v = UIKit.Solid(cr, "V", c); v.rectTransform.anchorMin = new Vector2(i % 2, 0); v.rectTransform.anchorMax = new Vector2(i % 2, 1); v.rectTransform.pivot = new Vector2(i % 2, 0.5f); v.rectTransform.sizeDelta = new Vector2(1.5f, 0);
                im.enabled = false;
                cs[i] = im;
            }
            var kr = UIKit.Rect("Key", rt, new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
            kr.pivot = new Vector2(0.5f, 0); kr.sizeDelta = new Vector2(70, 11); kr.anchoredPosition = new Vector2(0, 2);
            key = UIKit.Label(kr, "T", "", 8, TextAnchor.LowerCenter, c);
            key.horizontalOverflow = HorizontalWrapMode.Overflow;
            rt.gameObject.SetActive(false);
            return rt;
        }

        public void SetVisible(bool v)
        {
            visible = v;
            root.gameObject.SetActive(v);
            if (!v) { suggestRt.gameObject.SetActive(false); suggestRt2.gameObject.SetActive(false); }
        }

        public void SetHint(string s) { hint = s ?? ""; }
        public void Pulse() { }

        /// <summary>호환용: 상태 갱신은 Tick 에서 매 프레임 한다.</summary>
        public void Refresh(GameState gs) { }

        static Sprite IconFor(Game g, string id)
        {
            var set = g.Config.GetSet("UI_Skill_" + id);
            return set != null && set.frames != null && set.frames.Length > 0 ? set.frames[0] : null;
        }

        public void Tick()
        {
            UpdateBossBar();
            UpdateReviewTag();
            var g = Game.I;
            if (!visible || g == null || g.State == null) { HideMarks(); return; }
            var s = g.State; var lu = s.D.lu;
            // 루
            if (luIcon.sprite == null)
            {
                var art = g.Config.GetArt("lu");
                if (art != null && art.defaultSprite != null) luIcon.sprite = art.defaultSprite;
            }
            float k = s.MaxHp("lu") > 0 ? Mathf.Clamp01(lu.hp / (float)s.MaxHp("lu")) : 0;
            hpFill.rectTransform.anchorMax = new Vector2(k, 1);
            hpFill.color = k > 0.5f ? new Color(0.55f, 0.95f, 0.75f) : k > 0.25f ? new Color(1f, 0.85f, 0.35f) : new Color(1f, 0.4f, 0.35f) * ((Time.unscaledTime * 3f % 1f) < 0.5f ? 1f : 0.75f);
            hpText.text = "안정도 " + lu.hp;
            int max = s.MaxCharge("lu");
            for (int i = 0; i < pips.Count; i++)
            {
                pips[i].enabled = i < max;
                pips[i].color = i < lu.charge ? new Color(1f, 0.88f, 0.4f) : new Color(0.25f, 0.27f, 0.33f);
            }
            // 동료: 합류 이후 안정도가 줄었을 때만
            bool showVen = s.VenelleIsAlly && s.D.venelle.hp < s.MaxHp("venelle");
            venRt.gameObject.SetActive(showVen);
            if (showVen) venFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(s.D.venelle.hp / (float)s.MaxHp("venelle")), 1);

            // ---- 스킬 슬롯
            var skills = g.Player != null ? g.Player.Skills : null;
            bool showSkills = skills != null && s.D.slots != null && s.D.slots.Count > 0 && !s.Has("hud_no_skills");
            skillRt.gameObject.SetActive(showSkills && !(skills != null && skills.Targeting));
            if (showSkills)
                for (int i = 0; i < 4; i++)
                {
                    var sl = slots[i];
                    var sk = skills.SlotSkill(i);
                    var st = skills.StateOf(i);
                    sl.key.text = GameInput.Label(GameInput.SkillAct(i));
                    sl.icon.sprite = sk != null ? IconFor(g, sk.id) : null;
                    sl.icon.enabled = sl.icon.sprite != null;
                    sl.name.text = sk != null ? sk.shortName ?? sk.name : "";
                    float cd = sk != null ? skills.CooldownLeft(sk.id) : 0;
                    sl.cd.rectTransform.anchorMax = new Vector2(1, sk != null && sk.cooldown > 0 ? Mathf.Clamp01(cd / sk.cooldown) : 0);
                    bool usable = st == PlayerSkills.SlotState.Ready;
                    sl.icon.color = usable ? Color.white : st == PlayerSkills.SlotState.Cooldown ? new Color(1, 1, 1, 0.8f) : new Color(0.55f, 0.58f, 0.68f, 0.7f);
                    for (int c = 0; c < sl.cost.Count; c++) { sl.cost[c].enabled = sk != null && c < sk.cost; sl.cost[c].color = lu.charge >= (sk != null ? sk.cost : 0) ? new Color(1f, 0.88f, 0.4f) : new Color(0.6f, 0.3f, 0.28f); }
                    // 방금 쓴 슬롯은 테두리가 번쩍, 실패한 슬롯은 붉게
                    float used = Time.unscaledTime - skills.LastUsedTime, failed = Time.unscaledTime - skills.LastFailTime;
                    if (skills.LastUsedSlot == i && used < 0.25f) sl.frame.color = new Color(1f, 0.95f, 0.6f, 1f - used * 4f);
                    else if (skills.LastFailSlot == i && failed < 0.5f) sl.frame.color = new Color(1f, 0.35f, 0.3f, 1f - failed * 2f);
                    else sl.frame.color = new Color(0, 0, 0, 0);
                    sl.bg.color = st == PlayerSkills.SlotState.Empty ? new Color(0.06f, 0.07f, 0.1f, 0.5f) : new Color(0.07f, 0.09f, 0.13f, 0.88f);
                }
            // 실패 이유 (짧게)
            bool showFail = skills != null && Time.unscaledTime - skills.LastFailTime < 1.1f && !string.IsNullOrEmpty(skills.LastFail);
            failRt.gameObject.SetActive(showFail);
            if (showFail)
            {
                failText.text = skills.LastFail;
                int fs = Mathf.Clamp(skills.LastFailSlot, 0, 3);
                failRt.anchoredPosition = new Vector2(-8 - (4 * 34 - 4) + fs * 34 + 15, 56);
            }
            UpdateSuggest(g, skills);

            // ---- 발각 상태 (위험이 있을 때만)
            var worst = Sensors.Worst(out float maxS);
            string det = null; Color dc = UIKit.Ink;
            bool blink = (Time.unscaledTime * 4f % 1f) < 0.6f;
            switch (worst)
            {
                case SensorState.Alert: det = "! 발견됨 — 시야를 끊어!"; dc = new Color(1f, 0.45f, 0.4f); break;
                case SensorState.Searching: det = "?? 수색 중"; dc = new Color(1f, 0.75f, 0.35f); break;
                case SensorState.Checking: det = "? 확인하러 온다"; dc = new Color(1f, 0.8f, 0.35f); break;
                case SensorState.Suspicious: det = "? 의심"; dc = new Color(1f, 0.92f, 0.5f); break;
            }
            if (Time.time - Sensors.LastCallTime < 2.5f && worst != SensorState.Idle) { det = Sensors.LastCallText; dc = new Color(1f, 0.45f, 0.4f); }
            bool hidden = g.Player != null && (g.Player.HiddenInCover || (g.Player.Crouching && Cover.Near(g.Player.transform.position) != null && NearSensor(g.Player.transform.position) && !AnySees()));
            if (hidden && (det == null || worst == SensorState.Suspicious || worst == SensorState.Searching))
            { det = worst == SensorState.Searching ? "— 숨음 · 수색 중 —" : "— 숨음 —"; dc = new Color(0.6f, 0.85f, 1f); }
            detectRt.gameObject.SetActive(det != null && g.FieldInputAllowed);
            if (det != null) { detectText.text = det; detectText.color = (worst == SensorState.Alert && !blink) ? dc * 0.8f : dc; }

            // 경보
            bool alarm = Facility.AlarmOn;
            alarmRt.gameObject.SetActive(alarm);
            if (alarm) alarmText.text = blink ? "▲ 경보" : "△ 경보";

            // 상황 안내
            dashText.text = g.CurrentMapId == "P03" ? "" : GameInput.Label(Act.Dodge) + " · 대시";
            string h = hint;
            hintText.text = h;
            hintRt.gameObject.SetActive(!string.IsNullOrEmpty(h));

            UpdateMarks(g);
        }

        static bool AnySees() { foreach (var se in Sensors.All) if (se != null && se.SensorActive && se.SeesPlayer) return true; return false; }

        /// <summary>자동 대상 강조: W(간섭) 대상과 E(방전) 목표를 각각 하나씩. R 은 전원선 입구 프롬프트로 대신한다.</summary>
        void UpdateSuggest(Game g, PlayerSkills sk)
        {
            bool on = sk != null && g.FieldInputAllowed && !sk.Targeting && g.Player != null && !g.Player.Carrying && !g.State.Has("hud_no_skills");
            ZapTarget a = null, b = null; string ka = "", kb = "";
            if (on)
            {
                for (int i = 0; i < 4; i++)
                {
                    var s = sk.SlotSkill(i); if (s == null) continue;
                    if (s.kind == "targeted" && sk.SuggestJam != null) { a = sk.SuggestJam; ka = GameInput.Label(GameInput.SkillAct(i)) + " " + (s.shortName ?? s.name); }
                    if (s.kind == "source" && sk.SuggestPullTarget != null) { b = sk.SuggestPullTarget; kb = GameInput.Label(GameInput.SkillAct(i)) + " " + (s.shortName ?? s.name); }
                }
                if (a != null && a == b) { ka = ka + " / " + kb; b = null; }
            }
            Place(suggestRt, corners, suggestKey, a, ka);
            Place(suggestRt2, corners2, suggestKey2, b, kb);
            SuggestText = (a != null ? ka + ":" + a.displayName : "") + (b != null ? " | " + kb + ":" + b.displayName : "");
        }

        void Place(RectTransform rt, Image[] cs, Text key, ZapTarget t, string label)
        {
            if (t == null) { if (rt.gameObject.activeSelf) rt.gameObject.SetActive(false); return; }
            rt.gameObject.SetActive(true);
            var r = t.Outline;
            Vector2 a = ui.WorldToCanvas(r.min), b = ui.WorldToCanvas(r.max);
            float pulse = 2f + Mathf.Sin(Time.unscaledTime * 8f) * 1f;
            rt.anchoredPosition = (a + b) / 2f;
            rt.sizeDelta = new Vector2(Mathf.Abs(b.x - a.x) + pulse * 2 + 4, Mathf.Abs(b.y - a.y) + pulse * 2 + 4);
            key.text = label;
        }

        static bool NearSensor(Vector2 p)
        {
            foreach (var se in Sensors.All) if (se != null && se.SensorActive && Vector2.Distance(se.MarkPos, p) < 12f) return true;
            return false;
        }

        // ---- 감시자 머리 위 ?/! 표시
        void UpdateMarks(Game g)
        {
            int n = 0;
            bool show = g.FieldInputAllowed || (g.Player != null && g.Player.Skills != null && g.Player.Skills.Targeting);
            if (show)
                foreach (var se in Sensors.All)
                {
                    if (se == null || !se.SensorActive) continue;
                    var st = se.SState;
                    if (st == SensorState.Idle && se.Suspicion <= 0.01f) continue;
                    if (n >= marks.Count) marks.Add(MakeMark());
                    var m = marks[n++];
                    m.rt.gameObject.SetActive(true);
                    m.rt.anchoredPosition = ui.WorldToCanvas(se.MarkPos);
                    bool alert = st == SensorState.Alert;
                    m.t.text = alert ? "!" : st == SensorState.Searching ? "??" : "?";
                    m.t.color = alert ? new Color(1f, 0.4f, 0.35f) : new Color(1f, 0.85f, 0.35f);
                    m.fill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(se.Suspicion), 1);
                    m.fill.color = alert ? new Color(1f, 0.35f, 0.3f) : new Color(1f, 0.85f, 0.35f);
                }
            for (int i = n; i < marks.Count; i++) marks[i].rt.gameObject.SetActive(false);
        }

        void HideMarks() { foreach (var m in marks) m.rt.gameObject.SetActive(false); if (suggestRt != null) { suggestRt.gameObject.SetActive(false); suggestRt2.gameObject.SetActive(false); } }

        (RectTransform, Text, Image) MakeMark()
        {
            var rt = UIKit.Rect("Mark", ui.worldLayer, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0); rt.sizeDelta = new Vector2(18, 20);
            var t = UIKit.Label(rt, "T", "?", 12, TextAnchor.UpperCenter, UIKit.Accent);
            t.font = UIKit.Pixel;
            var bar = UIKit.Rect("Bar", rt, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 3));
            UIKit.Solid(bar, "Back", new Color(0, 0, 0, 0.7f));
            var fill = UIKit.Solid(bar, "Fill", UIKit.Accent);
            return (rt, t, fill);
        }

        // ---- 정밀 선택 안내
        public void ShowTargeting(PlayerSkills ps, ActionTuning.SkillT s, ZapTarget t, PowerSource src, int sourceCount)
        {
            targetRt.gameObject.SetActive(true);
            targetRt.SetAsLastSibling();
            string res = t == null ? "" : s.kind == "source" ? t.PullResult : t.JamResult;
            string txt = "<b>" + s.name + "</b>  정밀 선택   전하 " + s.cost + "\n";
            if (s.kind == "source")
                txt += "공급원: <color=#88eeff>" + (src != null ? src.displayName + " (" + src.StateText + ")" : "없음") + "</color>" + (sourceCount > 1 ? "  [" + GameInput.Label(Act.Down) + " 바꾸기]" : "") + "\n";
            txt += "목표: <color=#ffe066>" + (t != null ? t.displayName : "-") + "</color>  → " + res + "\n";
            txt += "<color=#aab4c8>떼기: 사용 · " + GameInput.Pair(Act.Left, Act.Right) + ": 대상 · " + GameInput.Label(Act.Cancel) + ": 취소</color>";
            targetText.text = txt;
        }

        public void HideTargeting() { if (targetRt != null) targetRt.gameObject.SetActive(false); }
        public bool TargetingShown => targetRt.gameObject.activeSelf;
        public string TargetingText => targetText.text;
        public string DetectText => detectRt.gameObject.activeSelf ? detectText.text : "";
    }

    /// <summary>
    /// 루의 짧은 탐험 독백 말풍선. 이동을 막지 않고 위험 시간도 멈추지 않는다.
    /// 머리 위에 띄우되, 근처 CCTV 시야나 조준 대상과 겹치지 않도록 반대쪽으로 비켜서고, 조준 중에는 숨긴다.
    /// </summary>
    public class LuBubble
    {
        readonly UIRoot ui;
        readonly RectTransform rt;
        readonly Text text;
        readonly Image bg;
        public void Tint(Color c) { if(bg!=null)bg.color = c; }
        float timer;
        public string Current => rt!=null && text!=null && rt.gameObject.activeSelf ? text.text : "";
        /// <summary>따라갈 대상 (null 이면 루). 베넬 혼잣말 등에 같은 말풍선을 쓴다.</summary>
        public Transform Follow; public float FollowHeight = 1.6f; public string LogName = "루";

        public LuBubble(Transform parent, UIRoot owner)
        {
            ui = owner;
            rt = UIKit.Rect("LuBubble", parent, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0);
            bg = UIKit.Panel(rt, "Bg", new Color(0.96f, 0.96f, 0.98f, 0.92f), true);
            text = UIKit.Label(rt, "T", "", 11, TextAnchor.MiddleCenter, new Color(0.12f, 0.12f, 0.18f));
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            rt.gameObject.SetActive(false);
        }

        public void Say(string s, float seconds = 0)
        {
            if(rt==null||text==null||ui==null)return;
            text.text = s;
            timer = seconds > 0 ? seconds : Mathf.Clamp(1.4f + s.Length * 0.07f, 1.6f, 4f);
            rt.sizeDelta = new Vector2(Mathf.Min(260, text.preferredWidth + 16), 18);
            rt.gameObject.SetActive(true);
            ui.AddLog(LogName, s);
        }

        public void Hide() { timer = 0; if(rt!=null)rt.gameObject.SetActive(false); }

        public void Tick()
        {
            if(rt==null||ui==null){timer=0;return;}
            if (timer <= 0) return;
            timer -= Time.unscaledDeltaTime;
            var g = Game.I;
            if (timer <= 0 || g == null || g.Player == null || (Follow == null && LogName != "루")) { rt.gameObject.SetActive(false); return; }
            bool suppress = ui.dialogue.Open || (g.Player.Skills != null && g.Player.Skills.Targeting);
            rt.gameObject.SetActive(!suppress);
            if (suppress) return;
            Vector3 head = Follow != null ? Follow.position + new Vector3(0, FollowHeight, 0) : g.Player.transform.position + new Vector3(0, g.Player.CurrentHeight + 0.35f, 0);
            // 근처 감시자가 있으면 그 반대쪽으로 비켜 띄운다
            float side = 0;
            foreach (var se in Sensors.All)
                if (Follow == null && se != null && se.SensorActive && Vector2.Distance(se.MarkPos, head) < 5f) side = se.MarkPos.x > head.x ? -1.2f : 1.2f;
            var p = ui.WorldToCanvas(head + new Vector3(side, 0, 0));
            p.x = Mathf.Clamp(p.x, -320 + rt.sizeDelta.x / 2 + 4, 320 - rt.sizeDelta.x / 2 - 4);
            p.y = Mathf.Min(p.y, 180 - 24);
            rt.anchoredPosition = p;
        }
    }
}
