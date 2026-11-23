using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 루의 스킬: Q/E/R/F 고정 슬롯을 누르면 바로 쓴다 (순환 메뉴 없음). (2026-11-15 개편)
    /// 기본 배치 (SaveData.slots, 메뉴 '기술 장착'에서 바꿀 수 있다):
    ///  Q 톡!            — 즉시 근접 전기. 전하 없이 쓴다. 경비를 짧게 경직.
    ///  W 약한 간섭      — 바라보는 쪽의 가장 알맞은 기계·CCTV 를 자동으로 골라 잠깐 끈다 (조용함).
    ///  E 외부 전력 방전 — 화면 안의 '끌어올 전력 있음' 전원과 닿는 목표를 자동으로 이어 강하게 방전 (파괴·개방, 시끄러움).
    ///  R 전원선 이동    — 가까운 전원선 입구로 들어가 출구까지 이동 (닿아 있는 동료도 함께).
    /// 자동 선택은 바라보는 방향·거리·시야선·화면 안 여부로 정한다. 벽 너머·화면 밖 대상은 고르지 않는다.
    /// 지금 고른 대상은 HUD 가 화면에 하나만 강조한다 (Hud.UpdateSuggest).
    /// 대상이 없거나 쓸 수 없으면 비용을 쓰지 않고 짧은 이유('대상 없음', '전원 없음', '전하 부족')를 슬롯 옆에 띄운다.
    /// W/E 를 0.3초 넘게 누르고 있으면 정밀 선택(시간 느려짐, A/D 대상 바꾸기, S 전원 바꾸기, 떼면 사용, Esc 취소)이 된다 — 기본 진행에는 필요 없다.
    /// 수치: Resources/EW/action_tuning.json (skills).
    /// </summary>
    public class PlayerSkills : MonoBehaviour
    {
        Game g; PlayerController p;
        public bool Targeting { get; private set; }
        public bool AnimBusy => busy > 0;
        float busy, chargeTimer;
        readonly Dictionary<string, float> cooldown = new Dictionary<string, float>();
        int ctxHandle = -1;

        /// <summary>새 게임에서 가진 기술 (컴퓨터 안 안내서에서 익힌다).</summary>
        public static readonly string[] DefaultSkills = { "tok", "jam", "pull", "wire", "spark_shot" };
        /// <summary>기본 Q/E/R/F 배치.</summary>
        public static readonly string[] DefaultSlots = { "tok", "jam", "pull", "wire" };
        public const float HoldForPrecision = 0.3f;

        // 조준 상태
        ActionTuning.SkillT tSkill;
        Act tKey;
        readonly List<ZapTarget> candidates = new List<ZapTarget>();
        readonly List<PowerSource> sources = new List<PowerSource>();
        int ti, si;
        GameObject visRoot;
        LineRenderer targetBox, sourceBox, link, feed;
        float jitterT;

        // 눌림 처리 (W/E: 떼면 자동 사용, 길게 누르면 정밀 선택)
        int pressSlot = -1; float pressT;

        // 자동 제안 (HUD 강조용)
        public ZapTarget SuggestJam { get; private set; }
        public ZapTarget SuggestPullTarget { get; private set; }
        public PowerSource SuggestPullSource { get; private set; }
        public PowerLine SuggestWire { get; private set; }
        float suggestT;

        // 실패 이유 (HUD)
        public string LastFail { get; private set; } = "";
        public int LastFailSlot { get; private set; } = -1;
        public float LastFailTime { get; private set; } = -99;
        public int LastUsedSlot { get; private set; } = -1;
        public float LastUsedTime { get; private set; } = -99;

        public void Init(Game game, PlayerController pc) { g = game; p = pc; }

        public List<string> Owned => g.State.D.skills;
        public List<string> Slots => g.State.D.slots;
        public string SlotId(int i) { var s = Slots; return s != null && i >= 0 && i < s.Count ? s[i] : null; }
        public ActionTuning.SkillT SlotSkill(int i) { var id = SlotId(i); return string.IsNullOrEmpty(id) ? null : ActionTuning.I.Skill(id); }
        public float CooldownLeft(string id) => id != null && cooldown.TryGetValue(id, out var v) ? Mathf.Max(0, v) : 0;
        public static bool IsTargeted(ActionTuning.SkillT s) => s != null && (s.kind == "targeted" || s.kind == "source");
        public ZapTarget SelectedTarget => Targeting && ti < candidates.Count ? candidates[ti] : null;
        public PowerSource SelectedSource => Targeting && si < sources.Count ? sources[si] : null;
        public int CandidateCount => candidates.Count;
        public int MaxCharge => g.State.MaxCharge("lu");

        public enum SlotState { Empty, Ready, Cooldown, NoCharge, NoTarget, Blocked }

        /// <summary>HUD 표시용 슬롯 상태.</summary>
        public SlotState StateOf(int i)
        {
            var s = SlotSkill(i);
            if (s == null) return SlotState.Empty;
            if (!g.FieldInputAllowed || p.Carrying || p.Climbing) return SlotState.Blocked;
            if (CooldownLeft(s.id) > 0) return SlotState.Cooldown;
            if (g.State.D.lu.charge < s.cost) return SlotState.NoCharge;
            if (s.kind == "targeted" && SuggestJam == null) return SlotState.NoTarget;
            if (s.kind == "source" && SuggestPullTarget == null) return SlotState.NoTarget;
            if (s.kind == "wire" && SuggestWire == null) return SlotState.NoTarget;
            return SlotState.Ready;
        }

        void Update()
        {
            if (g == null) g = Game.I;
            if (g == null || g.State == null || p == null) return;
            // 자체 발전: 위험 시간이 흐르는 동안 전하가 찬다
            var lu = g.State.D.lu;
            if (lu.charge < MaxCharge)
            {
                chargeTimer += Hazard.Paused ? 0 : Time.deltaTime;
                if (chargeTimer >= ActionTuning.I.player.chargeRegenSeconds) { chargeTimer = 0; lu.charge++; }
            }
            else chargeTimer = 0;
            var keys = new List<string>(cooldown.Keys);
            foreach (var k in keys) cooldown[k] -= Time.deltaTime;
            if (busy > 0) busy -= Time.deltaTime;

            suggestT -= Time.unscaledDeltaTime;
            if (suggestT <= 0) { suggestT = 0.08f; UpdateSuggestions(); }

            if (Targeting) { UpdateTargeting(); return; }
            bool canAct = g.FieldInputAllowed && !p.ScriptControl && !p.Hidden && !p.Climbing && !p.Carrying;
            if (!canAct) { pressSlot = -1; return; }

            // 누르고 있던 W/E: 떼면 자동 사용, 오래 누르면 정밀 선택
            if (pressSlot >= 0)
            {
                var ps = SlotSkill(pressSlot);
                var act = GameInput.SkillAct(pressSlot);
                pressT += Time.unscaledDeltaTime;
                if (ps == null) pressSlot = -1;
                else if (!GameInput.Held(act, InputCtx.Gameplay))
                {
                    int sl = pressSlot; pressSlot = -1;
                    UseAuto(sl, ps);
                }
                else if (pressT >= HoldForPrecision)
                {
                    int sl = pressSlot; pressSlot = -1;
                    BeginTargeting(ps, act, sl);
                }
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                var act = GameInput.SkillAct(i);
                if (!GameInput.Down(act, InputCtx.Gameplay)) continue;
                GameInput.Consume(act);
                var s = SlotSkill(i);
                if (s == null) { Fail(i, "비어 있음"); break; }
                if (IsTargeted(s)) { if (Ready(s, i)) { pressSlot = i; pressT = 0; } }
                else UseAuto(i, s);
                break;
            }
        }

        void Fail(int slot, string why)
        {
            LastFail = why; LastFailSlot = slot; LastFailTime = Time.unscaledTime;
            Sfx.Play("no", 0.6f);
        }

        bool Ready(ActionTuning.SkillT s, int slot)
        {
            if (s == null) return false;
            if (busy > 0.05f) return false;
            if (CooldownLeft(s.id) > 0) { Fail(slot, "대기 중"); return false; }
            if (g.State.D.lu.charge < s.cost) { Fail(slot, "전하 부족"); return false; }
            return true;
        }

        void Used(int slot) { LastUsedSlot = slot; LastUsedTime = Time.unscaledTime; }

        /// <summary>슬롯 사용 (자동 대상). 테스트에서도 직접 부른다.</summary>
        public void UseSlot(int slot) { var s = SlotSkill(slot); if (s == null) { Fail(slot, "비어 있음"); return; } UpdateSuggestions(); UseAuto(slot, s); }

        /// <summary>전하 모으기 (GDD 4.2): 잠깐 멈췄다가 전하 +2. 공격 없음.</summary>
        IEnumerator Gather(ActionTuning.SkillT s)
        {
            cooldown[s.id] = s.cooldown;
            busy = s.windup + s.recovery;
            p.ActionLock = s.windup;
            p.View.Play(p.View.HasState("Charge") ? "Charge" : "Idle", true);
            Sfx.Play(s.sfx, 0.7f);
            yield return Wait(s.windup);
            var lu = g.State.D.lu;
            lu.charge = Mathf.Min(MaxCharge, lu.charge + 2);
            Fx.Spawn("Spark", p.Chest, false, 31, 0.6f);
        }

        void UseAuto(int slot, ActionTuning.SkillT s)
        {
            if (!Ready(s, slot)) return;
            if (g.Map != null && g.Map.sealSkills) { Fail(slot, "여기서 터뜨리면 나도 길 잃어!"); if (g.State.Once("mono_zero_skill")) g.Monologue("여기서 터뜨리면 나도 길 잃어!", 2.2f); return; }
            switch (s.kind)
            {
                case "targeted":
                    UpdateSuggestions();
                    if (SuggestJam == null) { Fail(slot, "대상 없음"); return; }
                    Used(slot);
                    StartCoroutine(Execute(s, SuggestJam, null));
                    break;
                case "source":
                    UpdateSuggestions();
                    if (SuggestPullTarget == null) { Fail(slot, AnySourceOnScreen() ? "닿는 목표 없음" : "전원 없음"); return; }
                    Used(slot);
                    StartCoroutine(Execute(s, SuggestPullTarget, SuggestPullSource));
                    break;
                case "wire":
                    UpdateSuggestions();
                    if (SuggestWire == null) { Fail(slot, "전원선 없음"); return; }
                    if (!SuggestWire.Connected(g)) { Fail(slot, "끊어진 선"); return; }
                    Used(slot);
                    cooldown[s.id] = s.cooldown;
                    g.RunEvent(StoryEvents.WireTravel(g, SuggestWire));
                    break;
                case "burst":
                    Used(slot);
                    StartCoroutine(Burst(s));
                    break;
                case "gather":
                    Used(slot);
                    StartCoroutine(Gather(s));
                    break;
                default:
                    Used(slot);
                    StartCoroutine(Attack(s));
                    break;
            }
        }

        /// <summary>호환: 스킬 직접 사용 (테스트·연출). 타깃형이면 자동 대상.</summary>
        public void Use(ActionTuning.SkillT s)
        {
            int slot = Slots.IndexOf(s.id);
            UseAuto(slot, s);
        }

        public void Unlock(string id, bool select = true)
        {
            if (!GameState.AddUnique(g.State.D.skills, id)) return;
            var s = ActionTuning.I.Skill(id);
            // 빈 슬롯이 있으면 넣는다
            for (int i = 0; i < 4; i++) if (string.IsNullOrEmpty(SlotId(i))) { Slots[i] = id; break; }
            g.UI.Toast("새 기술: " + (s != null ? s.name : id), 3f);
        }

        // ============================================================ 자동 제안
        static bool OnScreen(Vector2 pt, float margin = 0.2f)
        {
            var cam = Camera.main; if (cam == null) return true;
            Vector3 v = cam.WorldToViewportPoint(pt);
            return v.x > -margin * 0.05f && v.x < 1 + margin * 0.05f && v.y > -0.02f && v.y < 1.02f && v.x >= 0.01f && v.x <= 0.99f;
        }

        bool AnySourceOnScreen()
        {
            foreach (var src in PowerSource.All) if (src != null && src.State == PowerSource.PState.Available && OnScreen(src.Tap)) return true;
            return false;
        }

        float FacingPenalty(Vector2 pt) { float dx = pt.x - p.transform.position.x; return (dx * p.Facing < -0.3f) ? 3.5f : 0f; }

        public void UpdateSuggestions()
        {
            if (g == null || p == null || g.State == null) return;
            Vector2 me = p.Chest;
            // W: 간섭
            var jam = ActionTuning.I.Skill("jam");
            ZapTarget bestJ = null; float bj = 1e9f;
            if (jam != null)
                foreach (var t in ZapTarget.All)
                {
                    if (t == null || !t.isActiveAndEnabled || !t.CanJam) continue;
                    float d = Vector2.Distance(me, t.AimPoint);
                    if (d > jam.range || !OnScreen(t.AimPoint) || !Sight.Clear(me, t.AimPoint)) continue;
                    float score = d + FacingPenalty(t.AimPoint) - t.JamPriority;
                    if (score < bj) { bj = score; bestJ = t; }
                }
            SuggestJam = bestJ;
            // E: 외부 전력 방전 (전원+목표 자동 연결)
            var pull = ActionTuning.I.Skill("pull");
            ZapTarget bestT = null; PowerSource bestS = null; float bp = 1e9f;
            if (pull != null)
                foreach (var t in ZapTarget.All)
                {
                    if (t == null || !t.isActiveAndEnabled || !t.CanPull || !OnScreen(t.AimPoint)) continue;
                    foreach (var src in PowerSource.All)
                    {
                        if (src == null || src.State != PowerSource.PState.Available || !OnScreen(src.Tap)) continue;
                        if (Vector2.Distance(me, src.Tap) > pull.range || !Sight.Clear(me, src.Tap)) continue;
                        if (Vector2.Distance(src.Tap, t.AimPoint) > pull.sourceRange || !Sight.Clear(src.Tap, t.AimPoint)) continue;
                        float score = Vector2.Distance(me, t.AimPoint) + Vector2.Distance(me, src.Tap) * 0.5f + FacingPenalty(t.AimPoint) - t.PullPriority;
                        if (score < bp) { bp = score; bestT = t; bestS = src; }
                    }
                }
            SuggestPullTarget = bestT; SuggestPullSource = bestS;
            // R: 전원선
            PowerLine bestW = null; float bw = 1e9f;
            foreach (var it in Interactable.All)
            {
                if (!(it is PowerLine pl) || !pl.isActiveAndEnabled || pl.exit == null) continue;
                Vector2 d = (Vector2)pl.transform.position - (Vector2)p.transform.position;
                if (Mathf.Abs(d.x) > 1.5f || d.y < -1.0f || d.y > 1.6f) continue;
                float sc = Mathf.Abs(d.x);
                if (sc < bw) { bw = sc; bestW = pl; }
            }
            SuggestWire = bestW;
        }

        // ============================================================ 공격형
        IEnumerator Attack(ActionTuning.SkillT s)
        {
            g.State.D.lu.charge -= s.cost;
            cooldown[s.id] = s.cooldown;
            busy = s.windup + s.active + s.recovery;
            p.ActionLock = s.windup + s.active;
            p.BreakCover(0.8f);
            p.View.Play(p.View.HasState(s.anim) ? s.anim : "Flick", true);
            yield return Wait(s.windup);
            int f = p.Facing;
            Vector2 origin = (Vector2)p.transform.position + new Vector2(0, p.CurrentHeight * 0.6f);
            Sfx.Play(s.sfx, 0.85f, Random.Range(0.95f, 1.08f));
            Noise.Emit(origin, s.noise, s.id);
            if (s.kind == "projectile")
            {
                Projectile.Fire(origin + new Vector2(f * 0.45f, 0), new Vector2(f, 0), s);
                yield break;
            }
            // 근접: 앞쪽 상자 안의 대상에 한 번씩
            Vector2 c = origin + new Vector2(f * s.range * 0.5f, 0);
            Fx.Spawn(s.fx, origin + new Vector2(f * 0.6f, 0), f < 0, 31, 0.5f);
            ArcFx(origin + new Vector2(f * 0.25f, 0), origin + new Vector2(f * s.range, 0.05f), 0.09f, new Color(0.85f, 0.97f, 1f), 0.7f);
            var hitOnce = new HashSet<IDamageable>();
            for (float t = 0; t < Mathf.Max(0.02f, s.active); t += Time.deltaTime)
            {
                var box = new Rect(c - new Vector2(s.range * 0.5f, s.height * 0.5f), new Vector2(s.range, s.height));
                foreach (var d in Damageables.All.ToArray())
                {
                    if (d == null || !d.Alive || hitOnce.Contains(d)) continue;
                    if (!box.Contains(d.Center) && !Overlaps(box, d)) continue;
                    if (!Sight.Clear(origin, d.Center)) continue;
                    hitOnce.Add(d);
                    d.TakeHit(s.damage, s.stagger, p.transform.position, "electric");
                    Fx.Spawn("Spark", d.Center, false, 31, 0.6f);
                    Cutscene.HitStopSoft(0.04f);
                }
                yield return null;
            }
        }

        // ============================================================ 펄스 버스트 (정비 작업장에서 익힘, GDD 4.5)
        /// <summary>마지막 펄스 버스트가 끈 전원선 (테스트·연출 확인용).</summary>
        public readonly List<string> LastBurstLines = new List<string>();
        public float LastBurstTime { get; private set; } = -99f;

        /// <summary>
        /// 몸을 웅크렸다 둥글게 터뜨린다. 반경 안의 적은 경직(약하면 일시 무력화), CCTV·제어 장치는 잠깐 꺼진다.
        /// 반경 안 장치가 물린 '전원선'은 실제로 한 줄 전체가 정전된다 (그 선에 연결된 컨베이어·PC·문이 같이 멈춤).
        /// 배터리가 보이는 장치는 배터리가 남아 있는 동안 계속 켜져 있다 (PowerRules.Outage). 연결되지 않은 먼 장치는 건드리지 않는다.
        /// 큰 소리·빛. 숨어 있었다면 노출된다.
        /// </summary>
        IEnumerator Burst(ActionTuning.SkillT s)
        {
            g.State.D.lu.charge -= s.cost;
            cooldown[s.id] = s.cooldown;
            busy = s.windup + s.active + s.recovery + 0.6f;
            p.ActionLock = s.windup + s.active;
            p.BreakCover(1.2f);
            p.View.Play(p.View.HasState("Charge") ? "Charge" : "Flick", true);
            Sfx.Play("sfx_charge_up", 0.8f, 1.2f);
            yield return Wait(s.windup);
            Vector2 c = p.Chest;
            float r = s.range;
            Sfx.Play(s.sfx, 1f, 0.9f);
            Noise.Emit(c, s.noise, s.id);
            Cutscene.Shake(0.05f, 0.25f);
            for (int i = 0; i < 10; i++)
            {
                float a = i * Mathf.PI * 2f / 10f + Random.Range(-0.15f, 0.15f);
                ArcFx(c, c + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.7f) * r, 0.18f, new Color(0.8f, 0.95f, 1f), 0.9f);
            }
            Fx.Spawn("Spark", c, false, 31, 1.2f);
            foreach (var d in Damageables.All.ToArray())
            {
                if (d == null || !d.Alive) continue;
                if (Vector2.Distance(c, d.Center) > r || !Sight.Clear(c, d.Center)) continue;
                d.TakeHit(s.damage, s.stagger, p.transform.position, "burst");
                Fx.Spawn("Spark", d.Center, false, 31, 0.6f);
            }
            foreach (var t in ZapTarget.All.ToArray())
                if (t != null && t.isActiveAndEnabled && t.CanJam && Vector2.Distance(c, t.AimPoint) <= r && Sight.Clear(c, t.AimPoint)) t.Jam(s.effectTime);
            // 반경 안 장치 → 그 장치가 물린 전원선 전체 정전 (배터리 장치는 버팀)
            LastBurstLines.Clear(); LastBurstTime = Time.time;
            foreach (var dev in Device.All.ToArray())
            {
                if (dev == null || Vector2.Distance(c, (Vector2)dev.transform.position + Vector2.up * 0.5f) > r) continue;
                if (!string.IsNullOrEmpty(dev.Model.line)) { if (!LastBurstLines.Contains(dev.Model.line)) LastBurstLines.Add(dev.Model.line); }
                else { PowerRules.Outage(dev.Model); dev.Changed(); }
            }
            foreach (var line in LastBurstLines) GridOutage(line);
            g.State.Set("used_burst");
            g.State.SetVar("strong_uses", g.State.GetVar("strong_uses") + 1);
            yield return Wait(s.active + s.recovery);
            // 머리가 곤두선다 (짧은 연기)
            if (p.View.HasState("HairUp")) { p.View.Play("HairUp", true); yield return Wait(0.6f); }
        }

        /// <summary>한 전원선에 물린 장치를 모두 정전시킨다 (배터리 장치는 남은 동안 버팀). 컨베이어·문·PC 가 같은 선이면 같이 멈춘다.</summary>
        public static void GridOutage(string line)
        {
            foreach (var dev in Device.All.ToArray())
                if (dev != null && dev.Model.line == line) { PowerRules.Outage(dev.Model); dev.Changed(); }
            Sfx.Play("sfx_power_down", 0.8f);
            var gs = Game.I != null ? Game.I.State : null;
            if (gs != null) gs.Set("overloaded_" + line);
        }

        static bool Overlaps(Rect box, IDamageable d)
        {
            if (!(d is Component cmp)) return false;
            var col = cmp.GetComponent<Collider2D>();
            if (col == null) return false;
            var b = col.bounds;
            return box.Overlaps(new Rect(b.min, b.size));
        }

        static IEnumerator Wait(float s) { for (float t = 0; t < s; t += Time.deltaTime) yield return null; }

        // ============================================================ 정밀 선택 (선택 기능)
        void BeginTargeting(ActionTuning.SkillT s, Act key, int slot)
        {
            tSkill = s; tKey = key;
            Collect();
            if (candidates.Count == 0) { Fail(slot, s.kind == "source" ? "전원 없음" : "대상 없음"); return; }
            Targeting = true;
            ti = 0;
            // 자동 제안과 같은 대상부터 시작
            var pre = s.kind == "source" ? SuggestPullTarget : SuggestJam;
            if (pre != null && candidates.Contains(pre)) ti = candidates.IndexOf(pre);
            PickDefaultSource();
            ctxHandle = GameInput.Push(InputCtx.Targeting);
            TimeCtl.Set("targeting", ActionTuning.CurrentDifficulty.targetingTimeScale);
            BuildVisuals();
            Sfx.Play("sfx_target_on", 0.6f);
            Used(slot);
        }

        /// <summary>대상 후보: 사거리 안, 화면 안, 루(또는 전원)에서 시야선이 막히지 않는 것만.</summary>
        void Collect()
        {
            candidates.Clear();
            Vector2 me = p.Chest;
            var s = tSkill;
            var list = new List<ZapTarget>(ZapTarget.All);
            list.Sort((a, b) => a.AimPoint.x.CompareTo(b.AimPoint.x));
            foreach (var t in list)
            {
                if (t == null || !t.isActiveAndEnabled || !OnScreen(t.AimPoint)) continue;
                if (s.kind == "targeted")
                {
                    if (!t.CanJam) continue;
                    if (Vector2.Distance(me, t.AimPoint) > s.range) continue;
                    if (!Sight.Clear(me, t.AimPoint)) continue;
                    candidates.Add(t);
                }
                else
                {
                    if (!t.CanPull) continue;
                    if (SourcesFor(t).Count == 0) continue;
                    candidates.Add(t);
                }
            }
        }

        /// <summary>대상에 닿을 수 있는 전원: 루가 가까이 보고 있고, 전원에서 대상까지 막히지 않은 것. 가까운 순.</summary>
        public List<PowerSource> SourcesFor(ZapTarget t)
        {
            var r = new List<PowerSource>();
            Vector2 me = p.Chest;
            var s = tSkill ?? ActionTuning.I.Skill("pull");
            foreach (var src in PowerSource.All)
            {
                if (src == null || src.State != PowerSource.PState.Available || !OnScreen(src.Tap)) continue;
                if (Vector2.Distance(me, src.Tap) > s.range) continue;
                if (!Sight.Clear(me, src.Tap)) continue;
                if (Vector2.Distance(src.Tap, t.AimPoint) > s.sourceRange) continue;
                if (!Sight.Clear(src.Tap, t.AimPoint)) continue;
                r.Add(src);
            }
            r.Sort((a, b) => Vector2.Distance(me, a.Tap).CompareTo(Vector2.Distance(me, b.Tap)));
            return r;
        }

        void PickDefaultSource()
        {
            sources.Clear(); si = 0;
            if (tSkill.kind != "source" || ti >= candidates.Count) return;
            sources.AddRange(SourcesFor(candidates[ti]));
        }

        void UpdateTargeting()
        {
            var keep = SelectedTarget;
            Collect();
            if (candidates.Count == 0) { CancelTargeting(); Fail(-1, "놓침"); return; }
            int k = keep != null ? candidates.IndexOf(keep) : -1;
            ti = k >= 0 ? k : Mathf.Clamp(ti, 0, candidates.Count - 1);
            var keepSrc = SelectedSource;
            if (tSkill.kind == "source")
            {
                sources.Clear(); sources.AddRange(SourcesFor(candidates[ti]));
                int sk = keepSrc != null ? sources.IndexOf(keepSrc) : -1;
                si = sk >= 0 ? sk : 0;
            }

            if (GameInput.Down(Act.Right, InputCtx.Targeting)) { ti = (ti + 1) % candidates.Count; PickDefaultSource(); Sfx.Play("move", 0.5f); }
            if (GameInput.Down(Act.Left, InputCtx.Targeting)) { ti = (ti - 1 + candidates.Count) % candidates.Count; PickDefaultSource(); Sfx.Play("move", 0.5f); }
            if (sources.Count > 1 && GameInput.Down(Act.Down, InputCtx.Targeting)) { si = (si + 1) % sources.Count; Sfx.Play("move", 0.5f); }
            if (GameInput.Down(Act.Cancel, InputCtx.Targeting)) { GameInput.Consume(Act.Cancel); CancelTargeting(); Sfx.Play("cancel", 0.6f); return; }
            if (GameInput.Up(tKey, InputCtx.Targeting) || !GameInput.RawIsHeld(tKey))
            {
                var t = SelectedTarget; var src = SelectedSource; var s = tSkill;
                EndTargeting();
                if (t != null && (s.kind != "source" || src != null)) StartCoroutine(Execute(s, t, src));
                return;
            }
            UpdateVisuals();
            g.UI.hud.ShowTargeting(this, tSkill, SelectedTarget, SelectedSource, sources.Count);
        }

        public void CancelTargeting()
        {
            pressSlot = -1;
            if (!Targeting) return;
            EndTargeting();
        }

        void EndTargeting()
        {
            Targeting = false;
            if (ctxHandle >= 0) { GameInput.Pop(ctxHandle); ctxHandle = -1; }
            TimeCtl.Clear("targeting");
            if (visRoot != null) Destroy(visRoot);
            visRoot = null;
            if (g != null && g.UI != null) g.UI.hud.HideTargeting();
        }

        void OnDisable() { EndTargeting(); }
        void OnDestroy() { EndTargeting(); }

        IEnumerator Execute(ActionTuning.SkillT s, ZapTarget t, PowerSource src)
        {
            g.State.D.lu.charge = Mathf.Max(0, g.State.D.lu.charge - s.cost);
            cooldown[s.id] = s.cooldown;
            busy = s.windup + s.recovery;
            p.ActionLock = s.windup;
            if (s.kind == "source") p.BreakCover(1.0f);
            if (t != null) p.SetFacing(t.AimPoint.x >= p.transform.position.x ? 1 : -1);
            p.View.Play(p.View.HasState(s.anim) ? s.anim : "Point", true);
            yield return Wait(s.windup);
            if (t == null) yield break;
            if (s.kind == "targeted")
            {
                Sfx.Play(s.sfx, 0.6f, 1.2f);
                yield return Bolt(p.Chest, t.AimPoint, 0.16f, new Color(0.7f, 0.95f, 1f), 0.6f);
                t.Jam(s.effectTime);
                Noise.Emit(t.AimPoint, s.noise, "jam");
                Fx.Spawn("Spark", t.AimPoint, false, 31, 0.5f);
                Ring(t.AimPoint, new Color(0.6f, 0.95f, 1f, 0.8f), 0.9f);
            }
            else
            {
                if (src == null) yield break;
                src.Consume(1.4f);
                Sfx.Play("sfx_charge_up", 0.6f, 1.4f);
                yield return Bolt(p.Chest, src.Tap, 0.2f, new Color(1f, 0.95f, 0.6f), 0.5f);
                Sfx.Play(s.sfx, 1f);
                yield return Bolt(src.Tap, t.AimPoint, 0.3f, new Color(1f, 0.95f, 0.5f), 1f);
                t.Pull(src);
                g.State.SetVar("strong_uses", g.State.GetVar("strong_uses") + 1);
                Fx.Spawn("Explosion", t.AimPoint, false, 42, 1.4f);
                Ring(t.AimPoint, new Color(1f, 0.95f, 0.55f, 0.9f), 1.8f);
                Cutscene.HitStopSoft(0.07f);
                var rig = CameraRig.Main; if (rig != null) rig.Shake(0.12f, 0.3f);
                // 큰 방전은 소리와 빛으로 근처 경비를 끌어들인다 (위치를 계속 알려 주지는 않는다)
                Noise.Emit(t.AimPoint, s.noise, "pull");
                Noise.Emit(src.Tap, s.noise * 0.6f, "pull");
                g.StartCoroutine(g.UI.Flash(new Color(1f, 0.95f, 0.7f), 0.18f, 0.2f));
            }
        }

        /// <summary>a→b 로 잠깐 번쩍이는 전기 줄기 (연출).</summary>
        IEnumerator Bolt(Vector2 a, Vector2 b, float time, Color c, float width)
        {
            var go = new GameObject("Bolt");
            var lr = MakeLine(go.transform, c, 0.09f * width + 0.03f, 46);
            var lr2 = MakeLine(go.transform, new Color(1f, 1f, 1f, 0.9f), 0.03f, 47);
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                Zig(lr, a, b, 0.2f); Zig(lr2, a, b, 0.08f);
                yield return null;
            }
            Destroy(go);
        }

        void ArcFx(Vector2 a, Vector2 b, float time, Color c, float width) { StartCoroutine(Bolt(a, b, time, c, width)); }

        /// <summary>효과 범위를 보여 주는 짧은 고리 (판정 없음).</summary>
        void Ring(Vector2 at, Color c, float radius) { StartCoroutine(RingCo(at, c, radius)); }
        IEnumerator RingCo(Vector2 at, Color c, float radius)
        {
            var go = new GameObject("Ring");
            var lr = MakeLine(go.transform, c, 0.05f, 45);
            lr.loop = true; lr.positionCount = 20;
            for (float t = 0; t < 0.3f; t += Time.deltaTime)
            {
                float k = t / 0.3f, r = Mathf.Lerp(radius * 0.3f, radius, 1 - (1 - k) * (1 - k));
                for (int i = 0; i < 20; i++) { float a = i * Mathf.PI * 2 / 20; lr.SetPosition(i, at + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r); }
                var cc = c; cc.a = c.a * (1 - k); lr.startColor = lr.endColor = cc;
                yield return null;
            }
            Destroy(go);
        }

        // ============================================================ 조준 표시 (월드)
        LineRenderer MakeLine(Transform parent, Color c, float w, int order)
        {
            var go = new GameObject("Line"); go.transform.SetParent(parent, false);
            var lr = go.AddComponent<LineRenderer>();
            var cfg = g != null ? g.Config : GameConfig.Load();
            lr.material = cfg != null && cfg.lineMaterial != null ? cfg.lineMaterial : new Material(Shader.Find("Sprites/Default"));
            lr.startColor = lr.endColor = c; lr.startWidth = lr.endWidth = w;
            lr.sortingOrder = order; lr.useWorldSpace = true; lr.numCapVertices = 0;
            return lr;
        }

        void BuildVisuals()
        {
            if (visRoot != null) Destroy(visRoot);
            visRoot = new GameObject("TargetingVis");
            targetBox = MakeLine(visRoot.transform, new Color(1f, 0.85f, 0.3f), 0.07f, 47);
            targetBox.loop = true;
            sourceBox = MakeLine(visRoot.transform, new Color(0.5f, 0.95f, 1f), 0.06f, 47);
            sourceBox.loop = true;
            link = MakeLine(visRoot.transform, new Color(1f, 0.95f, 0.55f, 0.85f), 0.05f, 46);
            feed = MakeLine(visRoot.transform, new Color(0.55f, 0.95f, 1f, 0.7f), 0.035f, 46);
        }

        void UpdateVisuals()
        {
            if (visRoot == null) BuildVisuals();
            var t = SelectedTarget; var src = SelectedSource;
            float pulse = 1f + 0.08f * Mathf.Sin(Time.unscaledTime * 10f);
            if (t != null) Box(targetBox, t.Outline, pulse);
            targetBox.enabled = t != null;
            jitterT -= Time.unscaledDeltaTime;
            bool rejig = jitterT <= 0; if (rejig) jitterT = 0.07f;
            if (tSkill.kind == "source")
            {
                sourceBox.enabled = src != null; feed.enabled = src != null;
                if (src != null)
                {
                    Box(sourceBox, new Rect(src.Tap - new Vector2(0.45f, 0.45f), new Vector2(0.9f, 0.9f)), pulse);
                    if (rejig && t != null) { Zig(link, src.Tap, t.AimPoint, 0.12f); Zig(feed, p.Chest, src.Tap, 0.08f); }
                }
                link.enabled = src != null && t != null;
            }
            else
            {
                sourceBox.enabled = false; feed.enabled = false;
                link.enabled = t != null;
                if (t != null && rejig) Zig(link, p.Chest, t.AimPoint, 0.1f);
            }
        }

        static void Box(LineRenderer lr, Rect r, float k)
        {
            Vector2 c = r.center; Vector2 h = r.size * 0.5f * k + new Vector2(0.12f, 0.12f);
            lr.positionCount = 4;
            lr.SetPosition(0, c + new Vector2(-h.x, -h.y)); lr.SetPosition(1, c + new Vector2(h.x, -h.y));
            lr.SetPosition(2, c + new Vector2(h.x, h.y)); lr.SetPosition(3, c + new Vector2(-h.x, h.y));
        }

        public static void Zig(LineRenderer lr, Vector2 a, Vector2 b, float amp)
        {
            int n = Mathf.Clamp(Mathf.CeilToInt(Vector2.Distance(a, b) / 0.45f), 2, 40);
            lr.positionCount = n + 1;
            Vector2 d = b - a; Vector2 nrm = new Vector2(-d.y, d.x).normalized;
            for (int i = 0; i <= n; i++)
            {
                float k = i / (float)n;
                float off = (i == 0 || i == n) ? 0 : Random.Range(-amp, amp);
                lr.SetPosition(i, a + d * k + nrm * off);
            }
        }
    }
}
