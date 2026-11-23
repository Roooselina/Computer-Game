using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 7~9장 (2026-11-15): 수용동 해방 → 접수 홀 메이린 → 상담 부스 → 영상실(시설 카메라) → 격벽 → 관제탑
    /// → 8장 라비엘 재회·도망/멈춰 세우기·라비엘의 방·통신실·연결 교량 → 9장 대기실·공명 공연장·녹음 보관실·서버실(강제 가동·붕괴)·포획 단말·휴지통.
    /// 모든 1회성 장면·보상은 플래그로 막는다. 사망한 인물은 이후 대사·추종·지원이 없다 (StoryEvents.Dead 로 확인).
    /// 대사 원문: Data/Dialogue/dialogue_ko.json (Tools/ch7_11_dialogue.py, src 로 GDD 원문/보충 구분).
    /// 공간 의존 대사(‘아래’, ‘위층’ 등)는 world_layout.json 의 spaceLines 에 맵별로 적어 도면 교체 때 함께 본다.
    /// </summary>
    public static partial class StoryEvents
    {
        static readonly Dictionary<string, System.Func<Game, IEnumerator>> late = new Dictionary<string, System.Func<Game, IEnumerator>>
        {
            // 7장
            { "c7_enter", HoldingEnter }, { "c7_release", HoldingRelease }, { "c7_desk", HoldingDesk }, { "c7_stairs", g => SayRepeat(g, "c7_stairs", "c7_stairs_after") },
            { "c7_hall_enter", HallMeet }, { "c7_ticket", Ticket }, { "c7_chair", g => SayRepeat(g, "c7_chair", "c7_chair_again") },
            { "c7_booth_lock", BoothLock }, { "c7_lostfound", LostFound }, { "c7_booth_enter", g => MonoOnce(g, "c7_booth_enter") },
            { "c7_monitors", Monitors }, { "c7_bulkhead", Bulkhead }, { "c7_control", ControlTower }, { "c7_tower_enter", TowerEnter },
            // 8장
            { "c8_meet", RavielMeet }, { "c8_photo", RavielPhoto }, { "c8_cups", RavielCups }, { "c8_room_enter", g => MonoOnce(g, "c8_room_enter") },
            { "c8_comm_console", CommConsole }, { "c8_comm_enter", CommEnter },
            { "c8_bridge1", g => BridgeTalk(g, "bridge1") }, { "c8_bridge2", g => BridgeTalk(g, "bridge2") }, { "c8_bridge3", g => BridgeTalk(g, "bridge3") },
            { "c8_bridge_bench", BridgeBench },
            // 9장
            { "c9_enter", WaitRoomEnter }, { "c9_plans", g => SayRepeat(g, "c9_plans", "c9_plans_again") }, { "c9_chart", ChartDoc },
            { "c9_member1", g => SayRepeat(g, "c9_member1", "c9_member1_again") }, { "c9_member2", g => SayRepeat(g, "c9_member2", "c9_member2_again") },
            { "c9_member3", g => SayRepeat(g, "c9_member3", "c9_member3_again") },
            { "c9_stage_enter", StageMeet }, { "c9_tape", ArchiveTape }, { "c9_archive_enter", g => MonoOnce(g, "c9_archive_enter") },
            { "c9_force", ForcedActivation }, { "c9_capture", Capture },
            { "c9_terminal_in", TerminalIn }, { "c9_d16", D16Doc }, { "c9_bin_enter", BinEnter }, { "c9_bin_list", BinList },
            { "c9_bin_status", g => g.Say("c9_bin_status") },
        };

        public static bool Dead(Game g, string who)
        {
            var s = g.State;
            switch (who)
            {
                case "venelle": return s.D.venelleState == "dead";
                case "raviel": return s.D.ravielState == "dead";
                case "f": case "kid008": case "survivor": return s.Has("shelter_collapsed");
                case "lacrimosa": return s.Has("lacrimosa_dead");
                case "meilin": return s.Has("meilin_dead");
                case "aurelius": return s.Has("aurelius_dead");
            }
            return false;
        }

        static bool RAlly(Game g) => g.State.RavielIsAlly && g.Companion != null && g.Companion.actorId == "raviel";
        static IEnumerator SayV(Game g, string allyId, string soloId) { yield return g.Say(VAlly(g) ? allyId : soloId); }

        /// <summary>인물 보이기/숨기기: 방 정의의 규칙(requireFlag show_id / hideFlag hide_id)을 따르는 플래그로 — 저장·불러오기 뒤에도 유지.</summary>
        static void ShowActor(string id, bool on)
        {
            var s = Game.I != null ? Game.I.State : null;
            if (s != null) { s.Set("show_" + id, on); s.Set("hide_" + id, !on); }
            foreach (var a in Object.FindObjectsByType<ActorSpawn>(FindObjectsInactive.Include))
                if (a.actorId == id) { a.gameObject.SetActive(true); a.ForceRefresh(); }
        }

        /// <summary>장 시작 저장 (한 번만).</summary>
        static void ChapterStart(Game g, string ch, string checkpoint)
        {
            if (g.State.Once("ev_" + ch + "_start")) { g.State.D.chapterId = ch; g.SaveNow(checkpoint, false); }
        }

        static IEnumerator OnEnterLate(Game g, string mapId)
        {
            var s = g.State;
            switch (mapId)
            {
                case "M27": ChapterStart(g, "ch7", "ch7_start"); break;
                case "M28":
                    if (s.Has("c7_meilin_done")) ShowActor("meilin", false);
                    else if (s.Has("c7_meilin_met")) { yield return null; StartMeilin(g); }     // 쓰러진 뒤 재시도: 보스 앞에서 바로 다시
                    break;
                case "M32":
                    ChapterStart(g, "ch8", "ch8_start");
                    if (s.Has("c8_duel_done")) { ShowActor("raviel", false); ShowActor("ravielBeast", false); }
                    else if (s.Has("c8_route_chosen")) { ShowActor("raviel", false); ShowActor("ravielBeast", true); yield return null; StartBeast(g); }   // 재시도
                    else { ShowActor("raviel", true); ShowActor("ravielBeast", false); }
                    break;
                case "M33": if (!s.Has("c8_room_done")) ShowActor("raviel", true); else ShowActor("raviel", false); break;
                case "M36": ChapterStart(g, "ch9", "ch9_start"); break;
                case "M37":
                    if (s.Has("c9_lac_done")) ShowActor("lacrimosa", false);
                    else if (s.Has("c9_lac_met")) { yield return null; StartLacrimosa(g); }   // 재시도
                    break;
                case "M40": yield return TerminalIn(g); break;     // 단말 안에서 시작하므로 진입 구역 대신 맵 진입에서
                case "M41": yield return BinEnter(g); break;
                case "M39": if (s.Has("c9_bin_done") && !s.Has("c9_caught")) yield return Cutscene.Play(g, "c9_catch", CatchScene(g), gg => gg.State.Set("c9_caught")); break;
            }
            yield return OnEnterFinal(g, mapId);
        }

        // ================================================================ 7장 27 수용동
        static IEnumerator HoldingEnter(Game g)
        {
            if (!g.State.Once("c7_enter_seen")) yield break;
            yield return g.Say("c7_gate_shut");
            yield return SayV(g, "c7_holding_v", "c7_holding");
        }

        static IEnumerator HoldingDesk(Game g)
        {
            var s = g.State;
            if (s.Count("staff_card") <= 0 && s.Once("got_staff_card_desk"))
            {
                s.AddItem("staff_card"); s.Set("has_staff_card");
                yield return g.Say("c7_desk_card");
            }
            else yield return g.Say("c7_desk_again");
            var doc = g.Dialogue.Doc("D10");
            if (doc != null && GameState.AddUnique(s.D.docs, "D10")) yield return DocPickup.DocView(g, doc);
        }

        static IEnumerator HoldingRelease(Game g)
        {
            if (g.State.Has("holding_released")) yield break;
            yield return Cutscene.Play(g, "c7_release", ReleaseBody(g), FinRelease);
        }

        static IEnumerator ReleaseBody(Game g)
        {
            var p = g.Player;
            p.ScriptControl = true;
            Sfx.Play("sfx_pc_beep", 0.8f);
            yield return g.Say("c7_release_panel");
            Sfx.Play("sfx_door_slide", 1f); Cutscene.Shake(0.03f, 0.3f);
            g.State.Set("holding_open");
            yield return Cutscene.Wait(0.6f);
            yield return Cutscene.Pan(M("held_center", LuPos(g)), 1.0f);
            yield return g.Say("c7_release_line");
            yield return SayV(g, "c7_release_route_v", "c7_release_route");
            // 사람들이 한 줄로 비상 계단 쪽으로 걸어간다 (시설 안 — 아직 밖으로 나간 것이 아니다)
            float stairX = M("stairs", new Vector2(6, 0)).x;
            var walks = new List<Coroutine>();
            for (int i = 1; i <= 5; i++)
            {
                var a = A("held" + i);
                if (a != null) { walks.Add(Cutscene.Parallel(g, WalkAndHide(a, stairX, 2.6f, i * 0.45f))); }
            }
            yield return Cutscene.Wait(3.6f);
            yield return g.Say("c7_release_after");
        }

        static IEnumerator WalkAndHide(Actor a, float x, float sp, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (a == null) yield break;
            yield return Cutscene.Walk(a, x, sp);
            if (a != null) a.gameObject.SetActive(false);
        }

        static void FinRelease(Game g)
        {
            var s = g.State;
            s.Set("holding_open"); s.Set("holding_released");
            GameState.AddUnique(s.D.shelterGroups, "holding_released");
            for (int i = 1; i <= 5; i++) ShowActor("held" + i, false);
            s.Reward("c7_release", ActionTuning.I.expSection);
            g.SaveNow("c7_released", false);
        }

        // ================================================================ 7장 28 접수 홀 · 메이린
        static IEnumerator HallMeet(Game g)
        {
            var s = g.State;
            if (s.Has("c7_meilin_done")) yield break;
            if (!s.Has("c7_meilin_met")) yield return Cutscene.Play(g, "c7_meilin_meet", MeilinMeet(g), gg => gg.State.Set("c7_meilin_met"));
            g.SaveNow("c7_meilin_before", false);
            StartMeilin(g);
        }

        static IEnumerator MeilinMeet(Game g)
        {
            g.Player.ScriptControl = true;
            var mei = A("meilin");
            yield return Cutscene.LuMove(g, M("ticket", LuPos(g)).x, 3f);
            Sfx.Play("sfx_pc_beep", 0.8f); yield return Cutscene.Wait(0.3f); Sfx.Play("sfx_pc_beep", 0.8f);
            if (mei != null) { yield return Cutscene.Walk(mei, mei.transform.position.x - 4f, 3.5f, mei.view.HasState("Roll") ? "Roll" : "Walk"); mei.view.Face(-1); }
            yield return g.Say(VAlly(g) ? "c7_meilin_v" : "c7_meilin_solo");
            yield return g.Say("c7_meilin_start");
        }

        static void StartMeilin(Game g)
        {
            var a = A("meilin");
            if (a == null) return;
            var b = a.GetComponent<MeilinBoss>(); if (b == null) b = a.gameObject.AddComponent<MeilinBoss>();
            var claw = Marker.Find("claw"); var board = Marker.Find("callboard"); var doll = Marker.Find("doll");
            b.claw = claw != null ? claw.transform : null; b.clawArt = claw != null ? claw.SR : null; b.clawFrames = g.Config.GetSet("U_Claw")?.frames;
            b.callBoard = board != null ? board.SR : null; b.callFrames = g.Config.GetSet("U_CallBoard")?.frames;
            b.doll = doll != null ? doll.transform : null; b.dockX = doll != null ? doll.transform.position.x : 36f;
            b.wet0 = M("wet0", Vector2.zero).x; b.wet1 = M("wet1", Vector2.zero).x;
            b.brake0 = M("brake0", Vector2.zero).x; b.brake1 = M("brake1", Vector2.zero).x;
            var sh = Marker.Find("hall_shutter");
            if (sh != null) { b.shutterBlock = sh.GetComponent<Collider2D>(); b.shutterArt = sh.SR; }
            b.floorY = M("floor", Vector2.zero).y;
            b.arenaMin = M("arena0", new Vector2(3, 0)).x; b.arenaMax = M("arena1", new Vector2(40, 0)).x;
            b.maxHp = 36;
            b.OnDefeated = () => g.RunEvent(Cutscene.Play(g, "c7_meilin_retreat", MeilinRetreat(g), FinMeilin));
            b.Activate();
        }

        public static IEnumerator MeilinTrickLine(Game g)
        {
            if (!g.State.Once("c7_trick_line")) yield break;
            yield return g.Say("c7_meilin_arm");
        }

        public static IEnumerator MeilinArmOffLine(Game g)
        {
            if (!g.State.Once("c7_armoff_line")) yield break;
            yield return g.Say("c7_meilin_armoff");
        }

        static IEnumerator MeilinRetreat(Game g)
        {
            var a = A("meilin");
            yield return g.Say("c7_meilin_lose");
            if (a != null) { a.view.Face(1); yield return Cutscene.Walk(a, M("arena1", new Vector2(44, 0)).x + 4f, 6f, a.view.HasState("Roll") ? "Roll" : "Walk"); }
            yield return g.Say(VAlly(g) ? "c7_meilin_gone_v" : "c7_meilin_gone");
        }

        static void FinMeilin(Game g)
        {
            var s = g.State;
            s.Set("c7_meilin_done");
            ShowActor("meilin", false);
            var b = Object.FindAnyObjectByType<MeilinBoss>(); if (b != null) { b.Stop(); Object.Destroy(b); }
            s.Reward("c7_meilin", ActionTuning.I.expSection * 2);
            g.SaveNow("c7_meilin_after", false);
        }

        static IEnumerator Ticket(Game g)
        {
            var s = g.State;
            if (!s.Has("c7_meilin_done")) { yield return g.Say("c7_ticket_before"); yield break; }
            yield return SayRepeat(g, "c7_ticket_again", "c7_ticket_again2");
        }

        // ================================================================ 7장 29 상담 부스
        static IEnumerator BoothLock(Game g)
        {
            if (g.State.Once("c7_booth_memory")) yield return g.Say("c7_meilin_memory");
            else yield return g.Say("c7_booth_lock_again");
        }

        static IEnumerator LostFound(Game g)
        {
            var s = g.State;
            if (s.Count("staff_card") <= 0)
            {
                // 대체 획득처: 책상의 카드를 놓쳤어도 진행이 막히지 않는다
                s.AddItem("staff_card"); s.Set("has_staff_card");
                yield return g.Say("c7_lostfound_card");
            }
            else yield return g.Say("c7_lostfound_empty");
        }

        // ================================================================ 7장 30 영상실: 시설 카메라
        static IEnumerator Monitors(Game g)
        {
            var s = g.State;
            if (s.Has("c7_feeds_seen")) { yield return g.Say("c7_monitors_again"); yield break; }
            yield return Cutscene.Play(g, "c7_feeds", FeedsBody(g), FinFeeds);
        }

        static IEnumerator FeedsBody(Game g)
        {
            var s = g.State;
            yield return g.Say("c7_monitors");
            yield return FeedView.Show(g, "feed_stairs", "CAM 07 · 수용동 비상 계단", FeedStairs(g), 3f);
            yield return g.Say("c7_feed_stairs");
            bool apart = s.D.venelleState == "apart";
            if (apart)
            {
                s.Set("venelle_to_shelter_cam");
                ShowActor("feed_venelle", true);
                yield return FeedView.Show(g, "feed_lift", "CAM 03 · 화물 승강장 (지하행)", FeedWalk("feed_venelle", 3.0f), 3f);
                yield return g.Say("c7_feed_venelle");
            }
            yield return FeedView.Show(g, "feed_tower", "CAM 12 · 관제탑 앞 격벽", FeedWalk("feed_warden", -3f), 2.5f);
            yield return g.Say("c7_lockdown");
            Sfx.Play("sfx_alarm", 0.7f);
            yield return g.Say(VAlly(g) ? "c7_lockdown_v" : "c7_lockdown_solo");
        }

        static IEnumerator FeedStairs(Game g)
        {
            var list = new List<Coroutine>();
            for (int i = 1; i <= 4; i++) list.Add(g.StartCoroutine(FeedView.WalkActor("feed_held" + i, -4.5f, 1.6f)));
            yield return new WaitForSeconds(3.2f);
        }

        static IEnumerator FeedWalk(string id, float dx) { yield return FeedView.WalkActor(id, dx, 1.4f); }

        static void FinFeeds(Game g)
        {
            var s = g.State;
            s.Set("c7_feeds_seen"); s.Set("c7_lockdown");
            if (s.D.venelleState == "apart") { s.Set("venelle_to_shelter"); GameState.AddUnique(s.D.shelterGroups, "venelle"); }
            g.SaveNow("c7_lockdown", false);
        }

        // ================================================================ 7장 격벽 (베넬 분기)
        static IEnumerator Bulkhead(Game g)
        {
            var s = g.State;
            if (s.Has("c7_bulkhead_closed")) yield break;
            yield return Cutscene.Play(g, "c7_bulkhead", VAlly(g) ? BulkheadAlly(g) : BulkheadSolo(g), FinBulkhead);
            yield return g.LoadMap("M31", "start");
        }

        static IEnumerator BulkheadAlly(Game g)
        {
            var p = g.Player; var v = g.Companion != null ? g.Companion.Actor : A("venelle");
            p.ScriptControl = true;
            ShowActor("warden1", true); ShowActor("warden2", true);
            Sfx.Play("sfx_alert", 0.9f);
            yield return g.Say("c7_wardens");
            if (g.Companion != null) g.Companion.enabled = false;
            if (v != null) { yield return Cutscene.Walk(v, p.transform.position.x + 0.6f, 4f); v.view.Face(1); }
            // 베넬이 루를 격벽 너머로 밀어낸다
            Sfx.Play("sfx_cloth", 0.8f);
            yield return Cutscene.LuMove(g, M("bulkhead", LuPos(g)).x + 2.2f, 6f, "Hurt");
            p.SetFacing(-1);
            if (v != null) { yield return Cutscene.Walk(v, M("bulkhead", LuPos(g)).x - 1.6f, 4f); v.view.Face(-1); }
            yield return g.Say("c7_bulkhead_talk");
            g.State.Set("c7_bulkhead_closed");
            Sfx.Play("sfx_door_bang", 1f); Cutscene.Shake(0.08f, 0.4f);
            yield return Cutscene.Wait(0.5f);
            // 총성 뒤 짧게 베넬 쪽 (방어 장치가 꺼지고, 약병을 놓친 손)
            yield return g.UI.Fade(1f, 0.25f);
            Sfx.Play("sfx_shot_enemy", 1f); yield return Cutscene.Wait(0.25f); Sfx.Play("sfx_shot_enemy", 1f);
            if (v != null) { v.view.Face(-1); Cutscene.CamSnap(v.transform.position + Vector3.up); }
            p.View.SetVisible(false);
            yield return g.UI.Fade(0f, 0.6f);
            if (v != null) v.view.Play(v.view.HasState("Collapsed") ? "Collapsed" : "Down");
            Sfx.Play("sfx_glass_break", 0.6f);
            yield return Cutscene.Wait(1.2f);
            yield return g.Say("c7_venelle_last");
            yield return Cutscene.Wait(1.5f);
            yield return g.UI.Fade(1f, 1.0f);
        }

        static IEnumerator BulkheadSolo(Game g)
        {
            var p = g.Player;
            p.ScriptControl = true;
            ShowActor("warden1", true); ShowActor("warden2", true);
            Sfx.Play("sfx_alert", 0.9f);
            yield return g.Say("c7_wardens_solo");
            yield return Cutscene.LuMove(g, M("bulkhead", LuPos(g)).x + 2.2f, 6f, "Walk");
            g.State.Set("c7_bulkhead_closed");
            Sfx.Play("sfx_door_bang", 1f); Cutscene.Shake(0.08f, 0.4f);
            yield return g.Say("c7_bulkhead_solo");
            yield return g.UI.Fade(1f, 0.6f);
        }

        static void FinBulkhead(Game g)
        {
            var s = g.State;
            s.Set("c7_bulkhead_closed");
            if (s.VenelleIsAlly)
            {
                // 동행 경로: 격벽에서 사망 확정. 이후 대사·추종·회복·스킬 지원 없음.
                s.D.venelleState = "dead"; s.D.venelle.state = "dead"; s.D.venelleDeathScene = "bulkhead";
                s.Set("venelle_dead"); s.Set("venelle_bulkhead");
                if (g.Companion != null) { Object.Destroy(g.Companion.gameObject); g.Companion = null; }
            }
            if (g.Player != null && g.Player.View != null) g.Player.View.SetVisible(true);
            g.UI.SetFade(1f);
        }

        // ================================================================ 7장 31 관제탑
        static IEnumerator TowerEnter(Game g)
        {
            if (!g.State.Once("c7_tower_seen")) yield break;
            yield return g.Say(g.State.Has("venelle_bulkhead") ? "c7_tower_v" : "c7_tower");
        }

        static IEnumerator ControlTower(Game g)
        {
            var s = g.State;
            if (s.Has("c7_control_done")) { yield return g.Say("c7_control_again"); yield break; }
            yield return Cutscene.Play(g, "c7_control", ControlBody(g), gg => { gg.State.Set("c7_control_done"); gg.SaveNow("ch7_end", false); });
        }

        static IEnumerator ControlBody(Game g)
        {
            var s = g.State;
            yield return g.Say("c7_control");
            Sfx.Play("sfx_pc_on", 0.8f);
            yield return g.Say("c7_control_open");
            if (s.Has("venelle_bulkhead"))
            {
                g.Player.ScriptControl = true;
                yield return Cutscene.LuMove(g, M("bulkhead", LuPos(g)).x + 1.0f, 5f, "Walk");
                g.Player.SetFacing(-1);
                for (int i = 0; i < 3; i++) { Sfx.Play("sfx_glass_knock", 1f, 0.7f); Cutscene.LuAnim(g, "Knock"); yield return Cutscene.Wait(0.45f); }
                yield return g.Say("c7_knock");
                yield return Cutscene.Wait(2.0f);
                yield return g.Say("c7_knock_after");
            }
        }

        // ================================================================ 8장 32 상층 회랑: 라비엘
        static IEnumerator RavielMeet(Game g)
        {
            var s = g.State;
            if (s.Has("c8_duel_done")) yield break;
            if (!s.Has("c8_route_chosen")) yield return Cutscene.Play(g, "c8_reunion", ReunionBody(g), gg => { gg.State.Set("c8_met"); });
            if (!s.Has("c8_route_chosen"))
            {
                yield return g.AskEx("c8_route_ask", null, true);
                s.D.friendRoute = g.LastChoice == 0 ? "flee" : "fight";
                s.Set("c8_route_chosen");
            }
            ShowActor("raviel", false); ShowActor("ravielBeast", true);
            g.SaveNow("c8_duel_before", false);
            StartBeast(g);
        }

        static IEnumerator ReunionBody(Game g)
        {
            var p = g.Player; p.ScriptControl = true;
            var r = A("raviel");
            yield return Cutscene.LuMove(g, M("meet", LuPos(g)).x, 3f);
            yield return g.Say("c8_reunion_1");
            yield return g.Say("c8_reunion_2");
            // 변신
            Sfx.Play("sfx_charge_up", 0.9f, 0.6f);
            if (r != null) r.view.Flash(new Color(1f, 0.6f, 0.7f), 0.6f);
            yield return Cutscene.Wait(0.6f);
            Cutscene.Shake(0.08f, 0.5f);
            yield return g.UI.FlashWhite(0.3f);
            ShowActor("raviel", false); ShowActor("ravielBeast", true);
            Sfx.Play("sfx_debris", 0.8f);
            yield return g.Say("c8_transform");
        }

        static void StartBeast(Game g)
        {
            var a = A("ravielBeast");
            if (a == null) return;
            var b = a.GetComponent<RavielBeast>(); if (b == null) b = a.gameObject.AddComponent<RavielBeast>();
            bool flee = g.State.D.friendRoute == "flee";
            b.chaseMode = flee;
            b.floorY = M("floor", Vector2.zero).y;
            b.arenaMin = flee ? M("chase0", new Vector2(2, 0)).x : M("arena0", new Vector2(4, 0)).x;
            b.arenaMax = flee ? M("chase1", new Vector2(80, 0)).x : M("arena1", new Vector2(40, 0)).x;
            b.goalX = M("goal", new Vector2(78, 0)).x;
            b.maxHp = 34;
            var doors = new List<Transform>(); foreach (var m in Marker.Prefix("bdoor")) doors.Add(m.transform);
            b.breakDoors = doors.ToArray();
            var sh = new List<Collider2D>(); var sa = new List<SpriteRenderer>();
            for (int i = 0; i < 3; i++) { var m = Marker.Find("cshutter" + i); if (m != null) { sh.Add(m.GetComponent<Collider2D>()); sa.Add(m.SR); } }
            b.chaseShutters = sh.ToArray(); b.chaseShutterArt = sa.ToArray();
            if (flee)
            {
                // 도망: 라비엘은 루 뒤에서 출발
                a.transform.position = new Vector3(Mathf.Max(b.arenaMin, g.Player.transform.position.x - 5f), b.floorY, 0);
                g.Monologue(g.State.Once("c8_flee_hint") ? "다치게 하기 싫어... 거리를 벌리자! 셔터 패널로 앞을 막으면서!" : "회랑 끝 문까지!", 3f);
            }
            else if (g.State.Once("c8_fight_hint")) g.Monologue("그만! 나 네 얼굴 때리기 싫어!", 2.5f);
            b.OnDefeated = () => g.RunEvent(Cutscene.Play(g, "c8_stop", DuelEnd(g), FinDuel));
            b.Activate();
        }

        static IEnumerator DuelEnd(Game g)
        {
            var s = g.State;
            var beast = A("ravielBeast");
            yield return g.Say(s.D.friendRoute == "flee" ? "c8_flee_end" : "c8_fight_end");
            yield return g.UI.FlashWhite(0.25f);
            Vector3 at = beast != null ? beast.transform.position : (Vector3)LuPos(g);
            ShowActor("ravielBeast", false);
            var r = A("raviel");
            ShowActor("raviel", true);
            r = A("raviel");
            if (r != null) { r.transform.position = at; r.view.Play(r.view.HasState("Sit") ? "Sit" : "Idle"); }
            yield return g.Say("c8_after_duel");
        }

        static void FinDuel(Game g)
        {
            var s = g.State;
            s.Set("c8_duel_done");
            var b = Object.FindAnyObjectByType<RavielBeast>(); if (b != null) b.Stop();
            ShowActor("ravielBeast", false);
            s.Reward("c8_duel", ActionTuning.I.expSection * 2);
            g.SaveNow("c8_duel_after", false);
        }

        // ================================================================ 8장 33 라비엘의 방
        static IEnumerator RavielPhoto(Game g)
        {
            var s = g.State;
            if (s.Has("c8_room_done")) { yield return SayRepeat(g, "c8_photo_after", "c8_photo_after2"); yield break; }
            if (!s.Has("c8_duel_done")) { yield return g.Say("c8_photo_alone"); yield break; }
            yield return Cutscene.Play(g, "c8_confession", ConfessionBody(g), FinConfession);
        }

        static IEnumerator ConfessionBody(Game g)
        {
            var s = g.State;
            var r = A("raviel");
            if (r != null) { r.view.Play("Idle"); r.view.Face(g.Player.transform.position.x > r.transform.position.x ? 1 : -1); }
            yield return g.Say("c8_photo");
            yield return g.Say("c8_past");
            yield return g.Say("c8_confess");
            // 연출 선택 (용서 점수·애정 조건이 아니다)
            yield return g.AskEx("c8_choice", null, true);
            s.D.reunionChoice = g.LastChoice == 0 ? "hand" : "sit";
            if (s.D.reunionChoice == "hand") { Cutscene.LuAnim(g, "HoldHand"); if (r != null) r.view.Play(r.view.HasState("HoldHand") ? "HoldHand" : "Idle"); }
            else { Cutscene.LuAnim(g, "Sit"); if (r != null) r.view.Play("Sit"); }
            yield return g.Say(s.D.reunionChoice == "hand" ? "c8_choice_hand" : "c8_choice_sit");
            yield return Cutscene.Wait(1.5f);
            yield return g.Say("c8_raviel_mono");
        }

        static void FinConfession(Game g)
        {
            var s = g.State;
            if (s.D.reunionChoice == "none") s.D.reunionChoice = "sit";
            s.Set("c8_room_done");
            s.D.ravielState = "ally";
            ShowActor("raviel", false);
            if (g.Companion == null && g.Player != null && g.Map != null && !g.Map.noCompanion)
                g.Companion = Companion.Spawn(g, "raviel", (Vector2)g.Player.transform.position + Vector2.left * 1.1f, g.Player.Facing);
            g.SaveNow("c8_reunion_after", false);
        }

        static IEnumerator RavielCups(Game g)
        {
            var s = g.State;
            if (!s.RavielIsAlly) { yield return g.Say("c8_cups_alone"); yield break; }
            if (s.D.sideStoryDone.Contains("cup")) { yield return g.Say("c8_cups_after"); yield break; }
            yield return g.Say("c8_cups");
            GameState.AddUnique(s.D.sideStoryDone, "cup");
            s.Set("side_cup_done");
        }

        // ================================================================ 8장 34 통신실 · 35 연결 교량
        static IEnumerator CommEnter(Game g)
        {
            if (!g.State.Once("c8_comm_seen")) yield break;
            if (RAlly(g)) g.Bubbles.Request("r_first");
            yield break;
        }

        static IEnumerator CommConsole(Game g)
        {
            var s = g.State;
            if (!RAlly(g)) { yield return g.Say("c8_console_solo"); yield break; }
            yield return SayRepeat(g, "c8_console", "c8_console_again");
        }

        static IEnumerator BridgeTalk(Game g, string cond)
        {
            if (RAlly(g)) g.Bubbles.Request(cond);
            yield break;
        }

        static IEnumerator BridgeBench(Game g)
        {
            var s = g.State;
            if (!RAlly(g)) { yield return g.Say("c8_bench_solo"); yield break; }
            if (s.Has("c8_food_named")) { yield return g.Say("c8_bench_again"); yield break; }
            yield return g.AskEx("c8_bench", null, true);
            string[] foods = { "붕어빵", "떡볶이", "초코바" };
            int k = Mathf.Clamp(g.LastChoice, 0, 2);
            s.SetVar("food_choice", k + 1);
            s.Set("c8_food_named");
            yield return g.Say(k == 0 ? "c8_bench_1" : k == 1 ? "c8_bench_2" : "c8_bench_3");
        }

        public static string FoodName(Game g) { int k = g.State.GetVar("food_choice"); return k == 1 ? "붕어빵" : k == 2 ? "떡볶이" : k == 3 ? "초코바" : null; }

        // ================================================================ 9장 36 대기실
        static IEnumerator WaitRoomEnter(Game g)
        {
            if (!g.State.Once("c9_wait_seen")) yield break;
            yield return g.Say(RAlly(g) ? "c9_wait_r" : "c9_wait");
        }

        static IEnumerator ChartDoc(Game g)
        {
            yield return SayRepeat(g, "c9_chart", "c9_chart_again");
            var doc = g.Dialogue.Doc("D14");
            if (doc != null && GameState.AddUnique(g.State.D.docs, "D14")) yield return DocPickup.DocView(g, doc);
        }

        static IEnumerator ArchiveTape(Game g)
        {
            var s = g.State;
            if (s.Once("got_edward_tape"))
            {
                s.AddItem("edward_tape");
                yield return g.Say("c9_tape");
                var doc = g.Dialogue.Doc("D13");
                if (doc != null && GameState.AddUnique(s.D.docs, "D13")) yield return DocPickup.DocView(g, doc);
            }
            else yield return g.Say("c9_tape_again");
        }

        // ================================================================ 9장 37 공명 공연장: 라크리모사
        static IEnumerator StageMeet(Game g)
        {
            var s = g.State;
            if (s.Has("c9_lac_done")) yield break;
            if (!s.Has("c9_lac_met")) yield return Cutscene.Play(g, "c9_lac_meet", LacMeet(g), gg => gg.State.Set("c9_lac_met"));
            g.SaveNow("c9_lac_before", false);
            StartLacrimosa(g);
        }

        static IEnumerator LacMeet(Game g)
        {
            g.Player.ScriptControl = true;
            yield return Cutscene.LuMove(g, M("stage_stop", LuPos(g)).x, 3f);
            var l = A("lacrimosa"); if (l != null) l.view.Play("Sing");
            yield return g.Say("c9_lac_meet");
            if (RAlly(g)) yield return g.Say("c9_lac_meet_r");
        }

        static void StartLacrimosa(Game g)
        {
            var a = A("lacrimosa");
            if (a == null) return;
            var b = a.GetComponent<LacrimosaBoss>(); if (b == null) b = a.gameObject.AddComponent<LacrimosaBoss>();
            var mic = Marker.Find("mic"); var sl = Marker.Find("spkL"); var sr = Marker.Find("spkR"); var mx = Marker.Find("mixer_art");
            b.mic = mic != null ? mic.transform : null; b.micArt = mic != null ? mic.SR : null; b.micFrames = g.Config.GetSet("U_Mic")?.frames;
            b.speakerL = sl != null ? sl.transform : null; b.speakerR = sr != null ? sr.transform : null;
            b.spkLArt = sl != null ? sl.SR : null; b.spkRArt = sr != null ? sr.SR : null; b.spkFrames = g.Config.GetSet("U_Speaker")?.frames;
            b.mixerArt = mx != null ? mx.SR : null; b.mixerFrames = g.Config.GetSet("U_Mixer")?.frames;
            var ga = Marker.Find("gauge"); b.gaugeAt = ga != null ? ga.transform : null;
            b.floorY = M("floor", Vector2.zero).y;
            b.arenaMin = M("arena0", new Vector2(3, 0)).x; b.arenaMax = M("arena1", new Vector2(46, 0)).x;
            b.maxHp = 40;
            // 말 걸기 지점 (빈틈·멈춤 중에만)
            var tgo = new GameObject("TalkPoint"); tgo.transform.SetParent(a.transform, false);
            var ts = tgo.AddComponent<ArenaSwitch>(); ts.kind = "talk"; ts.range = 1.8f; ts.rangeY = 2f; ts.promptOffset = new Vector2(0, 2.1f); ts.id = "lac_talk";
            b.OnDefeated = () =>
            {
                if (g.State.D.lacrimosaResult == "none") g.State.D.lacrimosaResult = "subdued";
                g.RunEvent(Cutscene.Play(g, "c9_lac_end", LacEnd(g), FinLac));
            };
            b.Activate();
            if (g.State.Once("c9_mixer_hint")) g.Monologue("저 믹서... 마이크가 스피커를 보게 하면 소리가 빙글빙글 커질 텐데!", 3f);
        }

        public static IEnumerator LacrimosaStopLine(Game g)
        {
            if (!g.State.Once("c9_stop_line")) yield break;
            yield return g.Say("c9_lac_stop");
        }

        public static IEnumerator LacrimosaListen(Game g, LacrimosaBoss b)
        {
            Sfx.Play("sfx_text", 0.6f);
            yield return g.Say(b.ListenCount <= 1 ? "c9_lac_listen" : "c9_lac_listen_again");
        }

        public static IEnumerator LacrimosaTalk(Game g, LacrimosaBoss b)
        {
            if (!b.TalkOpen) yield break;
            yield return g.AskEx("c9_lac_talk_ask", null, true);
            if (g.LastChoice != 0) yield break;
            g.State.D.lacrimosaResult = "persuaded";
            b.EndTalkWindow();
            g.RunEvent(Cutscene.Play(g, "c9_lac_end", LacEnd(g), FinLac));
        }

        static IEnumerator LacEnd(Game g)
        {
            var s = g.State;
            var l = A("lacrimosa");
            if (l != null) l.view.Play(s.D.lacrimosaResult == "persuaded" ? "Listen" : "Down");
            // 회상: 어린 에드워드와 TV 노래 → 음향실의 에드워드
            yield return g.UI.Fade(1f, 0.6f);
            var mem = Marker.Find("mem_edward");
            if (mem != null) Cutscene.CamSnap(mem.transform.position + Vector3.up * 1.5f);
            ShowActor("edwardKid", true);
            yield return g.UI.Fade(0f, 0.8f);
            yield return g.Say("c9_edward_1");
            yield return g.UI.Fade(1f, 0.5f);
            ShowActor("edwardKid", false); ShowActor("edward", true);
            yield return g.UI.Fade(0f, 0.6f);
            yield return g.Say("c9_edward_2");
            yield return g.UI.Fade(1f, 0.6f);
            ShowActor("edward", false);
            var rig = CameraRig.Main; if (rig != null) rig.Snap();
            yield return g.UI.Fade(0f, 0.6f);
            yield return g.Say("c9_lac_lu");
            yield return g.Say(s.D.lacrimosaResult == "persuaded" ? "c9_lac_persuaded" : "c9_lac_subdued");
        }

        static void FinLac(Game g)
        {
            var s = g.State;
            if (s.D.lacrimosaResult == "none") s.D.lacrimosaResult = "subdued";
            s.Set("c9_lac_done");
            var b = Object.FindAnyObjectByType<LacrimosaBoss>(); if (b != null) b.Stop();
            ShowActor("edwardKid", false); ShowActor("edward", false);
            s.Reward("c9_lac", ActionTuning.I.expSection * 2);
            g.SaveNow("c9_lac_after", false);
        }

        // ================================================================ 9장 39 서버실: 강제 가동과 붕괴 (사망 확정)
        static IEnumerator ForcedActivation(Game g)
        {
            var s = g.State;
            if (s.Has("shelter_collapsed")) yield break;
            yield return Cutscene.Play(g, "c9_collapse", CollapseBody(g), FinCollapse);
        }

        static IEnumerator CollapseBody(Game g)
        {
            var s = g.State;
            g.Player.ScriptControl = true;
            Sfx.Play("sfx_broadcast", 0.8f);
            yield return g.Say("c9_force_broadcast");
            Sfx.Play("sfx_debris", 1f); Cutscene.Shake(0.12f, 1.2f);
            yield return Cutscene.Wait(1.0f);
            yield return g.Say(RAlly(g) ? "c9_force_r" : "c9_force");
            yield return g.Say("c9_comm_terminal");
            // 지하 대피실 (시설 카메라 + 현장 방송)
            yield return FeedView.Show(g, "feed_shelter", "CAM B2-04 · 지하 대피실", ShelterFeed(g, s.Has("venelle_to_shelter") && s.D.venelleState == "apart"), 2f);
            yield return g.Say("c9_shelter_after");
            // 공명 공연장: 대기 인원을 내보내려던 라크리모사
            yield return FeedView.Show(g, "feed_hall", "CAM H-01 · 공명 공연장", HallFeed(g), 2f);
            yield return g.Say("c9_hall_after");
            yield return g.Say(RAlly(g) ? "c9_lu_shock_r" : "c9_lu_shock");
        }

        static IEnumerator ShelterFeed(Game g, bool venelleThere)
        {
            ShowActor("feed_venelle_b2", venelleThere);
            yield return new WaitForSeconds(1.2f);
            var f = A("feed_f"); if (f != null) f.view.Play(f.view.HasState("Guard") ? "Guard" : "Idle");
            Cutscene.Shake(0.05f, 0.6f); Sfx.Play("sfx_debris", 0.6f, 0.8f);
            var surv = new[] { "feed_sv1", "feed_sv2" };
            foreach (var id in surv) g.StartCoroutine(FeedView.WalkActor(id, -1.6f, 2f));
            yield return new WaitForSeconds(1.4f);
            // 추가 붕괴 → 화면 전환 → 잔해 아래 끊어진 번호표와 움직이지 않는 손
            foreach (var m in Marker.Prefix("feed_rubble")) if (m.SR != null) m.SR.enabled = true;
            foreach (var id in new[] { "feed_f", "feed_kid", "feed_sv1", "feed_sv2", "feed_venelle_b2" }) ShowActor(id, false);
            Sfx.Play("sfx_debris", 1f, 0.6f);
            yield return new WaitForSeconds(1.8f);
            foreach (var m in Marker.Prefix("feed_after")) if (m.SR != null) m.SR.enabled = true;
            yield return new WaitForSeconds(2.2f);
        }

        static IEnumerator HallFeed(Game g)
        {
            foreach (var id in new[] { "feed_mb1", "feed_mb2" }) g.StartCoroutine(FeedView.WalkActor(id, 4f, 2.2f));
            var l = A("feed_lac"); if (l != null) l.view.Play("Sing");
            yield return new WaitForSeconds(2.0f);
            var truss = Marker.Find("feed_truss");
            if (truss != null) yield return Cutscene.MoveObj(truss.transform, truss.transform.position + Vector3.down * 3.2f, 0.35f, false);
            Sfx.Play("sfx_debris", 1f, 0.7f);
            ShowActor("feed_lac", false);
            yield return new WaitForSeconds(2.0f);
        }

        static void FinCollapse(Game g)
        {
            var s = g.State;
            s.Set("shelter_collapsed"); s.Set("c9_hall_collapsed"); s.Set("lacrimosa_dead");
            if (s.D.venelleState == "apart")
            {
                // 생존·비동행 경로: 지하 대피실 붕괴 때 사망 확정
                s.D.venelleState = "dead"; s.D.venelle.state = "dead"; s.D.venelleDeathScene = "shelter";
                s.Set("venelle_dead"); s.Set("venelle_shelter");
            }
            g.SaveNow("c9_collapse_after", false);
        }

        static IEnumerator Capture(Game g)
        {
            var s = g.State;
            if (s.Has("c9_bin_done") || !s.Has("shelter_collapsed")) yield break;
            yield return Cutscene.Play(g, "c9_capture", CaptureBody(g), gg => gg.State.Set("c9_captured"));
            yield return g.LoadMap("M40", "start");
        }

        static IEnumerator CaptureBody(Game g)
        {
            g.Player.ScriptControl = true;
            Sfx.Play("sfx_transfer", 1f);
            yield return g.Say("c9_capture");
            if (g.Companion != null) g.Companion.gameObject.SetActive(false);
            g.Player.View.Flash(new Color(0.6f, 0.9f, 1f), 0.5f);
            yield return g.UI.FlashWhite(0.4f);
            yield return g.UI.Fade(1f, 0.4f);
        }

        // ================================================================ 9장 40 포획 단말 · 41 휴지통
        static IEnumerator TerminalIn(Game g)
        {
            var s = g.State;
            if (s.Has("c9_in_bin") || s.Has("c9_bin_done")) yield break;
            if (s.Once("c9_terminal_seen")) yield return g.Say("c9_terminal_in");
            if (!s.Has("c9_judged"))
            {
                yield return Cutscene.Wait(0.5f);
                yield return g.Say("c9_incomplete");
                s.Set("c9_judged");
            }
        }

        static IEnumerator D16Doc(Game g)
        {
            var s = g.State;
            var doc = g.Dialogue.Doc("D16");
            if (doc != null) { GameState.AddUnique(s.D.docs, "D16"); yield return DocPickup.DocView(g, doc); }
            if (!s.Has("c9_judged")) { yield return g.Say("c9_incomplete"); s.Set("c9_judged"); }
            if (s.Once("c9_moved_to_bin"))
            {
                yield return g.Say("c9_moved");
                s.Set("c9_in_bin");
                yield return g.LoadMap("M41", "start");
            }
        }

        static IEnumerator BinEnter(Game g)
        {
            if (!g.State.Once("c9_bin_seen")) yield break;
            yield return g.Say("c9_bin_talk");
        }

        static IEnumerator BinList(Game g)
        {
            var s = g.State;
            if (s.Has("c9_bin_done")) yield break;
            var bin = RecycleBinUI.MakeDefault();
            yield return RecycleBinUI.Run(g, bin,
                b => BinRestored(g, b),
                id => BinDoc(g, id));
            if (bin.CanConfirm)
            {
                s.Set("c9_bin_done");
                yield return g.Say("c9_bin_exit");
                yield return g.LoadMap("M39", "from_terminal");
            }
        }

        static IEnumerator BinRestored(Game g, RecycleBinUI b)
        {
            g.State.Set("c9_restored");
            yield return g.Say("c9_bin_restored");
            b.Released = true;
            g.State.Set("c9_released");
            yield return g.Say("c9_bin_released");
        }

        static IEnumerator BinDoc(Game g, string id)
        {
            var doc = g.Dialogue.Doc(id);
            if (doc == null) yield break;
            GameState.AddUnique(g.State.D.docs, id);
            yield return DocPickup.DocView(g, doc);
        }

        static IEnumerator CatchScene(Game g)
        {
            var p = g.Player; p.ScriptControl = true;
            if (g.Companion != null) g.Companion.gameObject.SetActive(true);
            yield return g.UI.FlashWhite(0.3f);
            Cutscene.LuAnim(g, "Hurt");
            yield return Cutscene.Wait(0.4f);
            var r = g.Companion != null ? g.Companion.Actor : null;
            if (r != null) { r.transform.position = p.transform.position + Vector3.left * 0.7f; r.view.Play(r.view.HasState("Catch") ? "Catch" : "Idle"); r.view.Face(1); }
            yield return g.Say("c9_catch");
        }
    }
}
