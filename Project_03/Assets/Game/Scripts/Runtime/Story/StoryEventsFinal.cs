using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 10~11장·마지막 (2026-11-15): 승선 대기실 → 캡슐 회랑(메이린의 비상 해제) → 전이 장치실(라비엘 부상·루의 말투 변화·최종전 3단계·한계 방출 3회)
    /// → 붕괴 연결로(이미 시작된 구조 붕괴) → 무너진 중정(마지막 대화·라비엘 사망·손잡기/이름/눈 감겨주기) → 잔해 더미(회수 장치·마지막 방출) → 크레딧 → 가상 바탕화면.
    /// GDD 9.1 사망 확인표: 메이린·아우렐리우스·캡슐 인원·단원 전원 사망, 현실로 전이 완료 0명. 생존 암시 없음.
    /// 회상 몽타주는 이번 플레이에서 실제로 겪은 장면만 고른다.
    /// </summary>
    public static partial class StoryEvents
    {
        static readonly Dictionary<string, System.Func<Game, IEnumerator>> final = new Dictionary<string, System.Func<Game, IEnumerator>>
        {
            { "c10_enter", BoardingEnter }, { "c10_board", g => SayRepeat(g, "c10_board", "c10_board_again") },
            { "c10_member1", g => SayRepeat(g, "c10_member1", "c10_member1_again") }, { "c10_member2", g => SayRepeat(g, "c10_member2", "c10_member2_again") },
            { "c10_capsule", CapsuleFail }, { "c10_meilin", MeilinRelease },
            { "c10_final_enter", FinalEnter }, { "c10_fall", FallScene },
            { "c11_last", LastScene },
        };

        static readonly string[] LimitLine = { "c10_limit_1", "c10_limit_2", "c10_limit_3" };
        static readonly string[] LimitAfter = { "c10_limit_after_1", "c10_limit_after_2", "c10_limit_after_3" };
        static readonly string[] FoodLine = { "c11_food_1", "c11_food_2", "c11_food_3" };

        static bool FinalEvents(string id, out System.Func<Game, IEnumerator> f) => final.TryGetValue(id, out f);

        static IEnumerator OnEnterFinal(Game g, string mapId)
        {
            var s = g.State;
            switch (mapId)
            {
                case "M42": ChapterStart(g, "ch10", "ch10_start"); break;
                case "M43": if (s.Has("c10_meilin_seen")) ShowActor("meilin", false); break;
                case "M44":
                    if (s.Has("c10_boss_done")) { ShowActor("aurelius", false); ShowActor("meilin", false); }
                    else if (s.Has("c10_confront_done")) { yield return ResumeFinal(g); }
                    break;
                case "M45":
                    if (!s.Has("c10_fall_done")) StartDebrisRun(g);
                    break;
                case "M46":
                    ChapterStart(g, "ch11", "ch11_start");
                    if (!s.Has("c11_talk_done")) yield return Cutscene.Play(g, "c11_wake", WakeBody(g), FinWake);
                    if (!s.Has("c11_stood")) yield return FarewellChoices(g);
                    break;
            }
        }

        // ================================================================ 6장 끝: 수용동 연결문 (7장 진입 — 이전 구역 봉쇄)
        public static IEnumerator Ch7Enter(Game g)
        {
            var s = g.State;
            s.Set("ch7_ready"); s.Set("ch7_entered");
            s.D.chapterId = "ch7";
            g.SaveNow("ch7_gate", false);
            Sfx.Play("sfx_door_slide", 0.9f);
            yield return g.LoadMap("M27", "start");
        }

        // ================================================================ 10장 42 승선 대기실
        static IEnumerator BoardingEnter(Game g)
        {
            if (!g.State.Once("c10_boarding_seen")) yield break;
            yield return g.Say("c10_boarding");
        }

        // ================================================================ 10장 43 캡슐 회랑
        static IEnumerator CapsuleFail(Game g)
        {
            var s = g.State;
            if (s.Has("c10_capsule_failed")) yield break;
            yield return Cutscene.Play(g, "c10_capsule", CapsuleBody(g), gg => { gg.State.Set("c10_capsule_failed"); });
        }

        static IEnumerator CapsuleBody(Game g)
        {
            g.Player.ScriptControl = true;
            Sfx.Play("sfx_transfer", 0.9f, 0.7f);
            g.State.Set("c10_capsule_failed");
            Cutscene.Shake(0.06f, 0.5f);
            yield return Cutscene.Wait(0.6f);
            var m = A("cap_member"); if (m != null) m.view.Play(m.view.HasState("Down") ? "Down" : "Hurt");
            yield return g.Say("c10_capsule");
        }

        static IEnumerator MeilinRelease(Game g)
        {
            var s = g.State;
            if (s.Has("c10_meilin_seen")) yield break;
            yield return Cutscene.Play(g, "c10_meilin", MeilinReleaseBody(g), gg => { gg.State.Set("c10_meilin_seen"); ShowActor("meilin", false); gg.SaveNow("c10_before_final", false); });
        }

        static IEnumerator MeilinReleaseBody(Game g)
        {
            g.Player.ScriptControl = true;
            var m = A("meilin"); if (m != null) m.view.Play(m.view.HasState("Pull") ? "Pull" : "Idle");
            yield return g.Say("c10_meilin");
            if (m != null) { m.view.Face(1); yield return Cutscene.Walk(m, m.transform.position.x + 8f, 5f, m.view.HasState("Roll") ? "Roll" : "Walk"); }
        }

        // ================================================================ 10장 44 전이 장치실: 최종전
        static IEnumerator FinalEnter(Game g)
        {
            var s = g.State;
            if (s.Has("c10_boss_done") || s.Has("c10_confront_done")) yield break;
            yield return Cutscene.Play(g, "c10_confront", ConfrontBody(g), FinConfront);
            yield return ResumeFinal(g);
        }

        static IEnumerator ConfrontBody(Game g)
        {
            var s = g.State;
            var p = g.Player; p.ScriptControl = true;
            yield return Cutscene.LuMove(g, M("confront", LuPos(g)).x, 3f);
            yield return g.Say("c10_aur_1");
            // 루를 장치에 다시 연결하려 한다 → 라비엘이 막다가 크게 다친다
            Sfx.Play("sfx_transfer", 1f);
            yield return g.Say("c10_aur_reconnect");
            var r = g.Companion != null ? g.Companion.Actor : A("raviel");
            if (g.Companion != null) g.Companion.enabled = false;
            if (r != null) { yield return Cutscene.Walk(r, p.transform.position.x + 0.8f, 6f); }
            Sfx.Play("sfx_discharge_big", 1f); Cutscene.Shake(0.1f, 0.5f);
            yield return g.UI.FlashWhite(0.3f);
            if (r != null) { r.view.Play(r.view.HasState("Injured") ? "Injured" : "Down"); }
            yield return g.Say("c10_raviel_hurt");
            // 처음으로 루의 말투가 낮고 짧아진다
            Cutscene.LuAnim(g, "Cold");
            yield return g.Say("c10_lu_cold");
        }

        static void FinConfront(Game g)
        {
            var s = g.State;
            s.Set("c10_confront_done");
            s.D.ravielState = "injured";
            if (g.Companion != null) { Object.Destroy(g.Companion.gameObject); g.Companion = null; }
            var rp = Marker.Find("raviel_spot");
            ShowActor("raviel_inj", true);
            g.SaveNow("c10_phase1", false);
        }

        static IEnumerator ResumeFinal(Game g)
        {
            var s = g.State;
            ShowActor("raviel_inj", true);
            var ri = A("raviel_inj"); if (ri != null) ri.view.Play(ri.view.HasState("Injured") ? "Injured" : "Sit");
            var mm = A("meilin"); if (mm != null) mm.view.Play(mm.view.HasState("Pull") ? "Pull" : "Idle");
            int phase = 1 + Mathf.Clamp(s.D.limitReleases, 0, 2);
            // 전하 모으기: 빈 슬롯이 없으면 전원선 이동 칸에 임시로 넣는다 (전투 뒤 되돌림)
            var sk = g.Player != null ? g.Player.Skills : null;
            if (sk != null && !s.D.skills.Contains("gather")) sk.Unlock("gather", false);
            if (!s.D.slots.Contains("gather"))
            {
                int slot = s.D.slots.IndexOf("wire"); if (slot < 0) slot = 3;
                s.SetVar("gather_swapped_" + slot, 1);
                s.D.slots[slot] = "gather";
                g.UI.Toast("전하 모으기 → [" + GameInput.Label(GameInput.SkillAct(slot)) + "] (전투 뒤 되돌림)", 3f);
            }
            ArenaGuard.Reset();
            var a = A("aurelius");
            if (a == null) yield break;
            var b = a.GetComponent<AureliusBoss>(); if (b == null) b = a.gameObject.AddComponent<AureliusBoss>();
            b.floorY = M("floor", Vector2.zero).y;
            b.arenaMin = M("arena0", new Vector2(3, 0)).x; b.arenaMax = M("arena1", new Vector2(45, 0)).x;
            var mg = Marker.Find("magnet"); b.magnetAt = mg != null ? mg.transform : null;
            var cp = Marker.Find("coupling"); b.couplingAt = cp != null ? cp.transform : null; b.couplingArt = cp != null ? cp.SR : null;
            b.couplingFrames = g.Config.GetSet("U_Coupling")?.frames;
            var caps = new List<Transform>(); foreach (var m in Marker.Prefix("cap")) caps.Add(m.transform);
            b.capacitorAt = caps.ToArray();
            b.OnPhaseCleared = n => { if (g.State.Once("aur_cleared_hint_" + n)) g.Monologue(n == 1 ? "...지금이야. 결합부." : "한 번 더.", 2f); };
            b.OnDefeated = () => g.RunEvent(Cutscene.Play(g, "c10_end", FinalEnd(g), FinFinal));
            b.StartPhase(phase);
            if (phase == 1 && s.Once("c10_fight_hint")) g.Monologue("외피를 지키는 구... 전자석이면 붙잡을 수 있어.", 3f);
        }

        /// <summary>한계 방출 n 번째 (결합부 앞에서 조사 키). 시간제 입력 없음.</summary>
        public static IEnumerator LimitRelease(Game g, AureliusBoss b, int n)
        {
            var s = g.State;
            yield return Cutscene.Play(g, "c10_limit_" + n, LimitBody(g, n), gg =>
            {
                gg.State.ApplyLimitRelease(n);
                gg.State.Set("aur_phase_" + n + "_done");
                var lu = gg.State.D.lu;
                // 단계 전환 회복: 줄어든 최대치까지 채우고 전하 2 (필수 진행이 막히지 않게)
                lu.hp = gg.State.MaxHp("lu"); lu.charge = Mathf.Min(gg.State.MaxCharge("lu"), 2);
                if (n < 3) gg.SaveNow("c10_phase" + (n + 1), false);
            }, false);
            b.AfterRelease(n);
        }

        static IEnumerator LimitBody(Game g, int n)
        {
            var p = g.Player; p.ScriptControl = true;
            Cutscene.LuAnim(g, "Limit");
            Sfx.Play("sfx_charge_up", 1f, 0.6f + n * 0.1f);
            yield return g.Say(LimitLine[Mathf.Clamp(n - 1, 0, 2)]);
            var cp = Marker.Find("coupling");
            if (cp != null) Fx.Spawn("Arc", (Vector2)cp.transform.position, false, 31, 2.2f);
            Sfx.Play("sfx_discharge_big", 1f);
            Cutscene.Shake(0.12f, 0.6f);
            yield return g.UI.Flash(new Color(0.85f, 0.95f, 1f), 0.9f, 0.5f);
            if (cp != null && cp.SR != null) { var fr = g.Config.GetSet("U_Coupling")?.frames; if (fr != null && fr.Length > 2) cp.SR.sprite = fr[n >= 3 ? 2 : 1]; }
            yield return g.Say(LimitAfter[Mathf.Clamp(n - 1, 0, 2)]);
        }

        static IEnumerator FinalEnd(Game g)
        {
            var s = g.State;
            var a = A("aurelius");
            if (a != null) a.view.Play(a.view.HasState("Down") ? "Down" : "Ruined");
            yield return g.Say("c10_aur_last");
            Sfx.Play("sfx_power_down", 1f, 0.5f);
            yield return Cutscene.Wait(1.0f);
            if (a != null) { Fx.Spawn("Explosion", a.transform.position + Vector3.up, false, 31, 1.6f); a.gameObject.SetActive(false); }
            yield return g.Say("c10_aur_dead");
            // 메이린: 마지막 캡슐의 수동 손잡이를 당기다 붕괴에 휘말림
            var m = A("meilin");
            if (m != null) { Cutscene.CamSnap(m.transform.position + Vector3.up); m.view.Play(m.view.HasState("Pull") ? "Pull" : "Idle"); }
            yield return g.Say("c10_meilin_last");
            Sfx.Play("sfx_debris", 1f, 0.6f); Cutscene.Shake(0.15f, 0.8f);
            foreach (var r in Marker.Prefix("mei_rubble")) if (r.SR != null) r.SR.enabled = true;
            if (m != null) m.gameObject.SetActive(false);
            foreach (var r in Marker.Prefix("mei_after")) if (r.SR != null) r.SR.enabled = true;
            yield return Cutscene.Wait(2.0f);
            yield return g.Say("c10_capsules_dead");
            var rig = CameraRig.Main; if (rig != null) rig.Release();
        }

        static void FinFinal(Game g)
        {
            var s = g.State;
            s.Set("c10_boss_done"); s.Set("aurelius_dead"); s.Set("meilin_dead"); s.Set("capsules_dead"); s.Set("transfer_zero");
            ShowActor("aurelius", false); ShowActor("meilin", false);
            foreach (var r in Marker.Prefix("mei_rubble")) if (r.SR != null) r.SR.enabled = true;
            foreach (var r in Marker.Prefix("mei_after")) if (r.SR != null) r.SR.enabled = true;
            // 전하 모으기 임시 배치 되돌리기
            for (int i = 0; i < 4; i++) if (s.GetVar("gather_swapped_" + i) == 1) { s.D.slots[i] = s.D.skills.Contains("wire") ? "wire" : ""; s.SetVar("gather_swapped_" + i, 0); }
            ArenaGuard.Reset();
            g.SaveNow("c10_after_boss", false);
        }

        // ================================================================ 10장 45 붕괴 연결로
        static void StartDebrisRun(Game g)
        {
            var go = new GameObject("DebrisRun");
            var d = go.AddComponent<DebrisRun>();
            d.floorY = M("floor", Vector2.zero).y;
            d.endX = M("fall", new Vector2(64, 0)).x;
            if (g.State.Once("c10_run_hint")) g.Monologue("무너진다... 가자, 라비엘. 그림자가 보이면 피해서!", 3f);
        }

        static IEnumerator FallScene(Game g)
        {
            var s = g.State;
            if (s.Has("c10_fall_done")) yield break;
            yield return Cutscene.Play(g, "c10_fall", FallBody(g), gg => { gg.State.Set("c10_fall_done"); gg.State.D.ravielState = "injured"; });
            yield return g.LoadMap("M46", "start");
        }

        static IEnumerator FallBody(Game g)
        {
            var p = g.Player; p.ScriptControl = true;
            foreach (var d in Object.FindObjectsByType<DebrisRun>()) Object.Destroy(d.gameObject);
            Sfx.Play("sfx_debris", 1f, 0.5f); Cutscene.Shake(0.18f, 1.2f);
            yield return g.Say("c10_floor_breaks");
            var r = g.Companion != null ? g.Companion.Actor : null;
            if (g.Companion != null) g.Companion.enabled = false;
            yield return g.UI.FlashWhite(0.25f);
            // 라비엘이 괴물 토끼의 몸으로 루를 끌어안아 충격을 받는다
            if (r != null) r.gameObject.SetActive(false);
            ShowActor("fall_beast", true);
            var b = A("fall_beast");
            if (b != null) { b.transform.position = p.transform.position + Vector3.left * 0.3f; b.view.Play(b.view.HasState("Hold") ? "Hold" : "Idle"); }
            p.View.SetVisible(false);
            yield return g.Say("c10_hold");
            Sfx.Play("sfx_fall_whoosh", 1f);
            if (b != null) yield return Cutscene.MoveObj(b.transform, b.transform.position + Vector3.down * 8f, 1.0f, false);
            yield return g.UI.Fade(1f, 0.4f);
            Sfx.Play("sfx_land", 1f, 0.5f);
            yield return Cutscene.Wait(1.2f);
        }

        // ================================================================ 11장 46 무너진 중정
        static IEnumerator WakeBody(Game g)
        {
            var s = g.State;
            var p = g.Player; p.ScriptControl = true;
            Cutscene.LuAnim(g, "Lie");
            yield return g.UI.Fade(0f, 2.0f);
            yield return Cutscene.Wait(1.0f);
            Cutscene.LuAnim(g, "Kneel");
            yield return g.Say("c11_wake");
            yield return g.Say("c11_breath");
            // 도망/대결 경로의 덧붙임
            yield return g.Say(s.D.friendRoute == "flee" ? "c11_route_flee" : "c11_route_fight");
            if (s.Has("c8_food_named")) yield return g.Say(FoodLine[Mathf.Clamp(s.GetVar("food_choice") - 1, 0, 2)]);
            if (s.D.sideStoryDone.Contains("cup")) yield return g.Say("c11_cup");
            yield return g.Say("c11_hand_loose");
            yield return Cutscene.Wait(2.0f);
            yield return g.Say("c11_no_answer");
            yield return Cutscene.Wait(1.5f);
        }

        static void FinWake(Game g)
        {
            var s = g.State;
            s.Set("c11_talk_done"); s.Set("raviel_dead");
            s.D.ravielState = "dead";
            if (g.Companion != null) { Object.Destroy(g.Companion.gameObject); g.Companion = null; }
            g.SaveNow("c11_after_talk", false);
        }

        static IEnumerator FarewellChoices(Game g)
        {
            var s = g.State;
            Cutscene.LuAnim(g, "HandHold");
            g.Player.ScriptControl = true;
            while (!s.Has("c11_stood"))
            {
                var opts = new List<string>(); var keys = new List<string>();
                if (!s.Has("c11_hand")) { opts.Add("손을 잡는다"); keys.Add("hand"); }
                if (!s.Has("c11_name")) { opts.Add("이름을 부른다"); keys.Add("name"); }
                if (!s.Has("c11_eyes")) { opts.Add("눈을 감겨 준다"); keys.Add("eyes"); }
                opts.Add("일어선다"); keys.Add("stand");
                g.UI.dialogue.SetBodyImmediate("", "");
                yield return g.Choose(opts, null, true);
                g.UI.dialogue.Show(false);
                string k = keys[Mathf.Clamp(g.LastChoice, 0, keys.Count - 1)];
                if (k == "stand") { s.Set("c11_stood"); break; }
                s.Set("c11_" + k);
                yield return g.Say(k == "hand" ? "c11_hand" : k == "name" ? "c11_name" : "c11_eyes");
            }
            yield return g.Say("c11_stand");
            g.Player.ScriptControl = false; Cutscene.LuAnim(g, null);
            g.Player.AnimOverride = null;
            g.SaveNow("c11_stood", false);
        }

        // ================================================================ 11장 47 잔해 더미: 마지막
        static IEnumerator LastScene(Game g)
        {
            var s = g.State;
            if (s.Has("ending_seen") && !s.Has("ending_replay_ok")) { }
            yield return Cutscene.Play(g, "c11_last", LastBody(g), gg => gg.State.Set("c11_release_done"), false);
            yield return EndingScreens.Credits(g);
            s.Set("ending_seen");
            yield return EndingScreens.Desktop(g);
        }

        static IEnumerator LastBody(Game g)
        {
            var s = g.State;
            var p = g.Player; p.ScriptControl = true;
            g.SaveNow("c11_last_before", false);
            var dev = A("recovery");
            if (dev != null) g.StartCoroutine(Cutscene.Walk(dev, p.transform.position.x + 3.2f, 0.8f));
            yield return g.Say("c11_recovery");
            yield return g.Say("c11_last_lines");
            yield return g.AskEx("c11_release_ask", null, true);
            // 하나뿐인 선택 (기본 선택 없음 — 흘러 들어온 입력으로 확정되지 않는다)
            Cutscene.LuAnim(g, "Release");
            yield return Montage(g);
            Sfx.Play("sfx_discharge_big", 1f, 0.5f);
            yield return g.UI.Flash(new Color(1f, 1f, 0.95f), 1f, 1.2f);
            if (dev != null) dev.gameObject.SetActive(false);
            g.UI.SetFade(1f);
            // 마지막 빛: 검은 화면 가운데 작은 빛 하나가 천천히 꺼진다 (다시 켜지지 않는다)
            var canvas = UIKit.MakeCanvas("LastLight", 59, g.transform);
            UIKit.Solid(canvas.transform, "Black", Color.black);
            var dot = UIKit.Rect("Light", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-2, -2), new Vector2(2, 2));
            var im = dot.gameObject.AddComponent<UnityEngine.UI.Image>(); im.color = new Color(1f, 0.95f, 0.75f);
            for (float t = 0; t < 4f; t += Time.deltaTime) { im.color = new Color(1f, 0.95f, 0.75f, 1f - t / 4f); yield return null; }
            Object.Destroy(canvas.gameObject);
            yield return Cutscene.Wait(2.0f);
        }

        /// <summary>마지막 열 초: 이번 플레이에서 실제로 함께한 장면만 (없는 장면은 넣지 않는다).</summary>
        public static List<string> MontageLines(Game g)
        {
            var s = g.State; var l = new List<string>();
            if (s.Has("c3_joined") || s.Has("c3_rest_done")) l.Add("c11_mem_venelle");
            if (s.D.child008Fed) l.Add("c11_mem_008");
            if (s.Has("waste_f_name")) l.Add("c11_mem_f");
            if (s.D.sideStoryDone.Contains("clothes") || s.Has("laundry_twins_v")) l.Add("c11_mem_clothes");
            l.Add("c11_mem_raviel");
            return l;
        }

        static IEnumerator Montage(Game g)
        {
            foreach (var id in MontageLines(g))
            {
                yield return g.UI.Fade(0.7f, 0.25f);
                var e = g.Dialogue.Get(id);
                if (e != null && e.lines.Length > 0) { g.UI.dialogue.SetBodyImmediate("", e.lines[0].t); GameState.AddUnique(g.State.D.dialogueSeen, id); }
                yield return Cutscene.Wait(1.8f);
            }
            g.UI.dialogue.Show(false);
        }
    }

    /// <summary>붕괴 연결로: 루 앞쪽에 1초 그림자 표식 → 낙하 (즉사 없음, 피해 6). 이미 시작된 구조 붕괴의 표현.</summary>
    public class DebrisRun : MonoBehaviour
    {
        public float floorY, endX;
        float t = 1.5f;
        public int Drops { get; private set; }
        void Update()
        {
            var g = Game.I; if (g == null || g.Player == null) return;
            float dt = Hazard.Dt; if (dt <= 0) return;
            if (g.Player.transform.position.x > endX) return;
            t -= dt;
            if (t <= 0) { t = Random.Range(1.3f, 2.0f); StartCoroutine(Drop(g, g.Player.transform.position.x + Random.Range(0.5f, 3.5f) * Mathf.Max(1, g.Player.Facing))); }
        }
        IEnumerator Drop(Game g, float x)
        {
            Drops++;
            var tele = new GameObject("DebrisShadow");
            var sr = tele.AddComponent<SpriteRenderer>(); sr.sprite = g.Config.pixel; sr.sortingOrder = 30; sr.color = new Color(1f, 0.25f, 0.2f, 0.35f);
            float s = g.Config.pixel.bounds.size.x;
            tele.transform.position = new Vector3(x, floorY + 2.5f, 0); tele.transform.localScale = new Vector3(1.2f / s, 5f / s, 1);
            for (float k = 0; k < 1.0f; k += Hazard.Dt) yield return null;
            Destroy(tele);
            Fx.Spawn("Smoke", new Vector2(x, floorY + 0.3f), false, 31, 0.8f);
            Sfx.Play("sfx_debris", 0.6f);
            var p = g.Player;
            if (p != null && Mathf.Abs(p.transform.position.x - x) < 0.8f) p.Hurt(6, new Vector2(x, floorY + 3f), "physical");
        }
    }
}
