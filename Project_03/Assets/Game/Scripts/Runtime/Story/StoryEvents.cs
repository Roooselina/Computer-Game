using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 장면 연출. 대사 원문은 dialogue_ko.json 에 있고 여기서는 ID 로만 부른다.
    /// 모든 1회성 사건은 플래그로 중복 실행을 막고, 컷신은 Cutscene.Play 로 실행해 건너뛰기·완료 상태 적용을 보장한다.
    /// 장면별 카메라·인물·소리·입력 설계: Docs/CUTSCENE_DESIGN.md
    /// 범위: 오프닝(불타는 마을 → 무너진 집 → 친구 컴퓨터 → 실험 통) ~ 3장 베넬 동행.
    /// 장 번호는 내부 데이터(chapterId)에만 두고 화면에 장 제목 카드를 띄우지 않는다.
    /// </summary>
    public static partial class StoryEvents
    {
        static readonly Dictionary<string, Func<Game, IEnumerator>> events = new Dictionary<string, Func<Game, IEnumerator>>
        {
            { "op_gap", DoorGap },
            { "c1_shortcut", ShortcutOpen },
            { "c1_rest_room_enter", g => SayOnce(g, "c1_rest_room") },
            { "c2_glass", SpecimenGlass },
            { "c2_ad_first", AdMemory },
            { "c2_vending", Vending },
            { "c2_lever", VendingLever },
            { "c2_line_tutorial", g => { if (g.State.Once("mono_c4_line")) g.Monologue("번개 표시 콘센트... 가까이서 " + GameInput.Label(Act.Skill4) + " 를 누르면 선을 타고 건너갈 수 있어."); return Nothing(); } },
            { "lab_bed", LabBed }, { "lab_tools", LabTools }, { "lab_bin", LabBin },
            { "lab_lockers", g => Mono(g, "lab_m_lockers") }, { "lab_window", g => Mono(g, "lab_m_window") },
            { "lab_jars", LabJars }, { "lab_researcher", LabResearcher }, { "lab_emergency", g => Mono(g, "lab_m_emergency") },
            { "lab_board", g => Clue(g, "board", "lab_clue_board") }, { "lab_tank_label", g => Clue(g, "tank", "lab_clue_tank") },
            { "c2s_pull_tut", PullTutorial }, { "c2s_cover_tut", g => MonoOnce(g, "c2s_cover_tut") },
            { "c2s_jam_tut", g => MonoOnce(g, "c2s_jam_tut") }, { "c2s_tray_tut", g => MonoOnce(g, "c2s_tray_tut") },
            { "checkpoint", g => CheckpointHere(g) },
            { "c3_rest", RestScene },
            // 2026-11-15: 컴퓨터 안 탐색·압수품·전원선
            { "pc_photo", g => Mono(g, "pc_photo_m") }, { "pc_trash", g => Mono(g, "pc_trash_m") },
            { "pc_internet", PcInternet }, { "pc_guide", PcGuide }, { "pc_status", g => g.Say("pc_status") },
            { "pc_log", g => PcLog(g, false) }, { "pc_exit", g => PcLog(g, true) },
            { "pc_enter", g => { return Nothing(); } },
            { "lab_pouch", LabPouch }, { "m15_gap_look", M15GapLook },
            { "demo_end", DemoEnd },   // 4장 연결 뒤에는 쓰지 않는다 (옛 시연판 저장 호환용으로 남김)
        };

        public static IEnumerator Run(Game g, string id)
        {
            if (string.IsNullOrEmpty(id)) yield break;
            if (events.TryGetValue(id, out var f)) yield return f(g);
            else if (TryUnderground(id, out var f2)) yield return f2(g);
            else if (late.TryGetValue(id, out var f3)) yield return f3(g);
            else if (FinalEvents(id, out var f4)) yield return f4(g);
            else Debug.LogError("[EW] 없는 이벤트 id: " + id);
        }

        public static IEnumerable<string> EventIds { get { foreach (var k in events.Keys) yield return k; foreach (var k in underground.Keys) yield return k; foreach (var k in late.Keys) yield return k; foreach (var k in final.Keys) yield return k; } }

        static IEnumerator Nothing() { yield break; }

        static IEnumerator SayOnce(Game g, string id)
        {
            if (!g.State.Once("done_" + id)) yield break;
            yield return g.Say(id);
        }

        static Actor A(string id) => Actor.Find(id);
        static Vector2 M(string id, Vector2 fb = default) => Marker.Pos(id, fb);
        static Vector2 LuPos(Game g) => g.Player != null ? (Vector2)g.Player.transform.position : Vector2.zero;

        // ============================================================ 맵 진입
        /// <summary>도입 연출이 직접 화면을 여는 장소 (불러오기 직후 검은 화면 유지).</summary>
        public static bool WantsBlackIntro(Game g, string mapId)
        {
            var s = g.State;
            if (s == null) return false;
            switch (mapId)
            {
                case "P01": return !s.Has("op_village_done");
                case "P02": return !s.Has("op_house_done");
                case "P03": return !s.Has("op_desktop_land");
                case "C01": return !s.Has("op_tank_done");
            }
            return false;
        }

        public static IEnumerator OnMapEnter(Game g, string mapId)
        {
            var s = g.State;
            switch (mapId)
            {
                case "P01":
                    s.D.chapterId = "prologue";
                    if (!s.Has("op_village_done")) yield return Cutscene.Play(g, "op_village", Village2(g), FinVillage2);
                    yield return g.LoadMap("P02", "start");
                    break;
                case "P02":
                    if (!s.Has("op_house_done")) yield return Cutscene.Play(g, "op_house", House2(g), FinHouse2);
                    yield return g.LoadMap("P03", "start");
                    break;
                case "P03":
                    s.D.chapterId = "ch1";
                    // 착지 연출 뒤 바로 조작 — 안내서·연습·접속 기록 조사 후에 강제 전송 (PcLog)
                    if (!s.Has("op_desktop_land")) yield return Cutscene.Play(g, "op_desktop_land", DesktopLand(g), FinDesktopLand);
                    else if (s.Has("op_desktop_done")) yield return g.LoadMap("C01", "tank");
                    break;
                case "C01":
                    if (!s.Has("op_tank_done"))
                    {
                        yield return Cutscene.Play(g, "op_tank", Tank2(g), FinTank2);
                        AfterTankControl(g);
                    }
                    break;
                case "C02":
                    if (!s.Has("corridor_memory")) yield return Cutscene.Play(g, "corridor_memory", CorridorMemory(g), gg => gg.State.Set("corridor_memory"));
                    if (s.Once("c2s_enter_said")) g.MonologueId("c2s_enter");
                    break;
                case "C06":
                    yield return VenelleCorridor2(g);
                    break;
                case "C03":
                    if (s.Once("ev_c2_start")) { s.D.chapterId = "ch2"; g.SaveNow("ch2_start", false); }
                    break;
                case "C05":
                    if (s.Once("ev_c3_start")) { s.D.chapterId = "ch3"; g.SaveNow("ch3_start", false); }
                    break;
                case "M15":
                    if (s.Has("c3_to_rest") && !s.Has("c3_rest_done")) yield return RestScene(g);
                    break;
                case "M13":
                    if (s.VenelleIsAlly && s.Has("c3_boss_done")) g.Bubbles.Request("mixroom_revisit");
                    break;
                default:
                    yield return OnEnterUnderground(g, mapId);     // 4~6장 (StoryEventsUnderground.cs)
                    yield return OnEnterLate(g, mapId);            // 7~11장 (StoryEventsLate.cs / StoryEventsFinal.cs)
                    break;
            }
        }








        static IEnumerator DropDebris(Transform t, Vector3 final, float delay)
        {
            yield return Cutscene.Wait(delay);
            Vector3 from = t.position;
            for (float k = 0; k < 0.35f; k += Time.deltaTime)
            {
                if (t == null) yield break;
                float e = k / 0.35f; t.position = Vector3.Lerp(from, final, e * e);
                yield return null;
            }
            if (t == null) yield break;
            t.position = final;
            Sfx.Play("sfx_debris", 0.5f, 1.1f);
            Fx.Spawn("Smoke", final + Vector3.up * 0.3f, false, 20, 1f);
        }


        // ---- 문틈으로 빠져나가기 (첫 조작의 목표)
        static IEnumerator DoorGap(Game g)
        {
            if (!g.State.Has("lab_realized")) { yield return Clue(g, "gap", "lab_clue_gap"); yield break; }
            var p = g.Player;
            Vector2 door = Marker.Pos("door", LuPos(g));
            p.ScriptControl = true;
            yield return Cutscene.LuMove(g, door.x - 0.7f, 2.5f, "Walk");
            p.SetFacing(1);
            p.AnimOverride = "Squeeze";
            Sfx.Play("sfx_squeeze", 1f);
            Sfx.Play("sfx_metal_creak", 0.5f, 1.3f);
            int order = p.View.sr.sortingOrder;
            p.View.sr.sortingOrder = 2;     // 문짝 뒤로
            yield return Cutscene.LuMove(g, door.x + 0.9f, 0.7f, "Squeeze");
            p.View.sr.sortingOrder = order;
            p.AnimOverride = null; p.ScriptControl = false;
            g.State.Set("c1_left_lab");
            yield return g.LoadMap("C02", "from_lab");
        }


        static IEnumerator ShortcutOpen(Game g)
        {
            if (g.State.Has("c1_shortcut_open")) yield break;
            yield return Cutscene.Play(g, "c1_shortcut", ShortcutBody(g), gg => gg.State.Set("c1_shortcut_open"));
        }

        static IEnumerator ShortcutBody(Game g)
        {
            Vector2 sh = M("shortcut", LuPos(g) + new Vector2(4, 0));
            yield return Cutscene.Pan(sh + new Vector2(-1, 1.6f), 1.0f);
            yield return Cutscene.Wait(0.3f);
            g.State.Set("c1_shortcut_open");
            Sfx.Play("sfx_door_slide", 0.9f, 0.9f);
            Cutscene.Shake(0.04f, 0.3f);
            yield return Cutscene.Wait(1.0f);
            yield return g.Say("c1_shortcut_line");
        }

        // ============================================================ 2장
        static IEnumerator SpecimenGlass(Game g)
        {
            if (!g.State.Once("done_c2_glass")) yield break;
            g.Player.SetFacing(1);
            yield return Cutscene.Play(g, "c2_glass", GlassBody(g), null);
        }

        static IEnumerator GlassBody(Game g)
        {
            var p = g.Player;
            p.ScriptControl = true; p.AnimOverride = "Idle";
            Vector2 w = M("glass_view", LuPos(g) + new Vector2(2, 2));
            yield return Cutscene.Pan(w, 1.4f);
            yield return Cutscene.Wait(1.0f);
            yield return g.Say("c2_glass");
        }

        static IEnumerator AdMemory(Game g)
        {
            if (!g.State.Once("done_c2_memory")) { yield return g.Say("c2_ad_old_again"); yield break; }
            yield return g.Say("c2_ads_intro");
            yield return g.UI.Fade(0.85f, 0.5f);
            yield return g.Say("c2_memory");
            yield return g.UI.Fade(0f, 0.5f);
            yield return g.Say("c2_memory_after");
            var doc = g.Dialogue.docs.TryGetValue("D03", out var d) ? d : null;
            if (doc != null && GameState.AddUnique(g.State.D.docs, "D03"))
            {
                yield return DocPickup.DocView(g, doc);
                g.UI.Toast("기록 획득: D03 " + doc.title);
            }
        }

        static IEnumerator Vending(Game g)
        {
            var dev = Device.Find("M09_vending");
            var s = g.State;
            if (s.VenelleIsAlly) g.Bubbles.Request("vending");
            if (dev != null && dev.Model.power != DevicePower.On) { yield return g.Say("c2_vend_off"); yield break; }
            yield return g.Ask("c2_vend_intro");
            switch (g.LastChoice)
            {
                case 0: yield return g.Say("c2_vend_button"); break;
                case 1:
                    if (s.GetVar("vend_snacks") >= 3) { yield return g.Say("c2_vend_empty"); break; }
                    s.SetVar("vend_snacks", s.GetVar("vend_snacks") + 1);
                    Fx.Spawn("Spark", dev.transform.position + Vector3.up * 0.8f, false, 30, 0.6f);
                    Sfx.Play("sfx_spark_small");
                    yield return g.Say("c2_vend_weak");
                    s.AddItem("snack");
                    g.UI.Toast("과자 을(를) 얻었다.");
                    g.Bubbles.Request("snack");
                    break;
                case 2:
                    Fx.Spawn("Explosion", dev.transform.position + Vector3.up * 1f);
                    Sfx.Play("sfx_power_down");
                    PowerRules.SetSwitch(dev.Model, false);
                    dev.SetPower(DevicePower.Off);
                    yield return g.Say("c2_vend_overload");
                    break;
            }
        }

        static IEnumerator VendingLever(Game g)
        {
            var dev = Device.Find("M09_vending");
            if (dev == null) yield break;
            if (dev.Model.power == DevicePower.On) { yield return g.Say("c2_lever_on"); yield break; }
            dev.SetPower(DevicePower.On);
            Fx.Spawn("Spark", dev.transform.position + Vector3.up * 1f);
            Sfx.Play("sfx_pc_on");
            yield return g.Say("c2_lever");
        }


        /// <summary>옛 시연판의 '시연 끝' 지점 (옛 저장 호환). 이제 타이틀로 돌려보내지 않고 화물 승강장으로 정상 진행한다.</summary>
        static IEnumerator DemoEnd(Game g)
        {
            if (!g.State.Has("c3_boss_done")) yield break;
            g.State.Set("demo_finished");
            yield return g.LoadMap("M16", "start");
        }
    }
}
