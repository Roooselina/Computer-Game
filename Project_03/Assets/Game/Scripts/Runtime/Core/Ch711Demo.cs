#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 개발용 자동 진행 (가상 입력, 에디터 전용): 7장 수용동 → 엔딩 가상 바탕화면 → 타이틀.
    /// Ch46Demo 가 through=1 이면 승강장 세이브 → 6장 끝 → 이어서 여기로. 단독 실행(StartCh711)은 배치 데이터의 사건 프리셋으로 시작한다.
    /// 사람의 손 조작이 아니며 조작감·감정선 검증을 대신하지 않는다.
    /// 설정 (Temp/EWBridge/ch46.txt 같은 줄): route=flee|fight lac=persuade|subdue tape=1|0 start=preset_id venelle=...
    /// </summary>
    public static class Ch711Demo
    {
        public static string Route = "flee", Lac = "persuade", StartPreset = "ch7_release_before", Ven = "ally";
        public static bool Tape = true, Cups = true;
        public static bool Done; public static int Deaths;
        static int deathsAtStart;

        public static void ResetConfig() { Route = "flee"; Lac = "persuade"; Tape = true; Cups = true; StartPreset = "ch7_release_before"; Ven = "ally"; }
        public static void Config(string k, string v)
        {
            switch (k)
            {
                case "route": Route = v; break;
                case "lac": Lac = v; break;
                case "tape": Tape = v == "1"; break;
                case "cups": Cups = v == "1"; break;
                case "start": StartPreset = v; break;
                case "venelle": Ven = v; break;
            }
        }

        static void Out(string s) => Ch46Demo.Out("[7-11] " + s);
        static IEnumerator Until(System.Func<bool> c, float t = 60f) => Ch46Demo.Until(c, t);
        static IEnumerator Go(Game g, float x, float t = 40f) => Ch46Demo.Go(g, x, t);
        static IEnumerator Use(Game g, float x, params int[] c) => Ch46Demo.Use(g, x, c);
        static IEnumerator Map(Game g, string id, float t = 40f) => Ch46Demo.Map(g, id, t);
        static IEnumerator Tap(Act a) => Ch46Demo.Tap(a);
        static bool Idle(Game g) => Ch46Demo.Idle(g);

        /// <summary>단독 시작: 사건 프리셋 상태를 개발 저장 폴더에 쓰고 이어하기.</summary>
        public static IEnumerator StartFromPreset(Game g)
        {
            while (g.Config == null || g.Db == null) yield return null;
            string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWDevSave"));
            SaveSystem.OverrideDirectory = dir;
            System.IO.Directory.CreateDirectory(dir);
            foreach (var f in System.IO.Directory.GetFiles(dir)) System.IO.File.Delete(f);
            var ev = WorldLayout.I.GetEvent(StartPreset);
            var d = ReviewMode.BuildPreset(SaveData.NewGame(), ev, Ven, out var ch);
            d.mapId = ev.map; d.spawnId = ev.spawn; d.hasPos = false;
            SaveSystem.Write(d, out _);
            Out("시작 (프리셋 " + StartPreset + ", 베넬 " + Ven + "): " + string.Join(" · ", ch.GetRange(0, Mathf.Min(6, ch.Count))) + " …");
            g.ContinueGame(out _);
            yield return Until(() => g.CurrentMapId == ev.map && g.Player != null && !g.Loading, 30f);
        }

        public static IEnumerator Run(Game g)
        {
            Done = false; Deaths = 0;
            GameInput.AutoReadDelay = 0.9f;
            deathsAtStart = g.State.D.retries;
            string m = g.CurrentMapId;
            Out("7~11장 시작: " + m + " 베넬=" + g.State.D.venelleState + " 과자 " + g.State.Count("snack"));
            if (m == "M27") yield return Ch7(g);
            if (g.CurrentMapId == "M32") yield return Ch8(g);
            if (g.CurrentMapId == "M36") yield return Ch9(g);
            else if (g.CurrentMapId == "M37") yield return Ch9Stage(g);
            else if (g.CurrentMapId == "M39") yield return Ch9Server(g);
            if (g.CurrentMapId == "M42") yield return Ch10(g);
            else if (g.CurrentMapId == "M44") yield return Ch10Final(g);
            if (g.CurrentMapId == "M46") yield return Ch11(g);
            Deaths = g.State != null ? g.State.D.retries - deathsAtStart : Deaths;
            Out("끝: 맵 " + g.CurrentMapId + " 타이틀=" + (g.State == null) + " 엔딩 대기 횟수=" + EndingScreens.WaitCount + " 사망 재시도=" + Deaths);
            Done = true;
        }

        // ================================================================ 7장
        static IEnumerator Ch7(Game g)
        {
            var s = g.State;
            yield return Map(g, "M27");
            yield return Use(g, 47f);
            Out("직원 카드 " + s.Count("staff_card") + " 명부 " + s.D.docs.Contains("D10"));
            yield return Use(g, 38f);
            yield return Until(() => s.Has("holding_released") && Idle(g), 60f);
            Out("수용동 해방: 대피실 무리 [" + string.Join(",", s.D.shelterGroups) + "]");
            yield return Go(g, 63.6f, 60f);
            yield return Map(g, "M28");
            yield return Go(g, 6.5f);
            yield return Until(() => Object.FindAnyObjectByType<MeilinBoss>() != null && Idle(g), 60f);
            yield return FightMeilin(g);
            yield return Until(() => s.Has("c7_meilin_done") && Idle(g), 60f);
            yield return Use(g, 44f);
            yield return Map(g, "M29");
            yield return Use(g, 16f);
            yield return Use(g, 9.1f);
            yield return Use(g, 18.5f);
            yield return Use(g, 21f);
            yield return Use(g, 27f);
            yield return Map(g, "M30");
            yield return Use(g, 15.5f);
            Out("영상실: 봉쇄=" + s.Has("c7_lockdown") + " 베넬 폐기소행=" + s.Has("venelle_to_shelter"));
            yield return Go(g, 31f, 20f);
            yield return Map(g, "M31", 60f);
            Out("격벽 뒤: 베넬=" + s.D.venelleState + "/" + s.D.venelleDeathScene + " 동료=" + (g.Companion != null ? g.Companion.actorId : "없음"));
            yield return Use(g, 15f);
            yield return Go(g, 33.6f, 30f);
            yield return Map(g, "M32");
        }

        static IEnumerator FightMeilin(Game g)
        {
            MeilinBoss b = null;
            float t = 0; int termUses = 0;
            while (!g.State.Has("c7_meilin_done") && t < 300f)
            {
                b = Object.FindAnyObjectByType<MeilinBoss>();
                if (b == null || !b.Active) { t += Time.deltaTime; yield return null; continue; }
                var p = g.Player; if (p == null || !g.FieldInputAllowed) { t += Time.deltaTime; yield return null; continue; }
                float dx = b.transform.position.x - p.transform.position.x;
                // 1) 포획팔이 켜져 있고 표적이 아직 47 이면: 유지보수 단말 (두 번까지)
                if (!b.ArmOff && !b.TargetIsDoll && termUses < 2 && b.State == MeilinBoss.MState.Approach)
                {
                    yield return Go(g, 4.5f, 8f);
                    MenuList.ScriptedChoices.Clear(); MenuList.ScriptedChoices.Enqueue(0);
                    yield return Tap(Act.Interact); yield return new WaitForSeconds(0.4f);
                    yield return Until(() => Idle(g), 10f); MenuList.ScriptedChoices.Clear();
                    termUses++; Out("유지보수 단말 사용 " + termUses + " → 표적 " + (b.TargetIsDoll ? "00" : "47"));
                    continue;
                }
                // 2) 준비 동작: 돌진 띠 / 가위 → 비켜선다 (대시)
                if (b.State == MeilinBoss.MState.DashWindup || b.State == MeilinBoss.MState.RollWindup || b.State == MeilinBoss.MState.SnipWindup || b.State == MeilinBoss.MState.ArmTrack && !b.TargetIsDoll)
                {
                    if (Mathf.Abs(dx) < 6f) yield return Dash(g, dx > 0 ? -1 : 1);
                    yield return new WaitForSeconds(0.25f); t += 0.3f; continue;
                }
                // 3) 빈틈이나 가까우면 톡!
                yield return Approach(g, b.transform.position.x, 1.1f);
                yield return Attack(g, Mathf.Sign(dx));
                t += Time.deltaTime; yield return null;
            }
            Out("메이린 전투 끝: 단말 " + termUses + "회, 부딪힘 " + (b != null ? b.Crashes : -1) + ", 포획팔 꺼짐 " + (b != null && b.ArmOff) + ", 시간 " + t.ToString("0") + "s");
        }

        // ================================================================ 8장
        static IEnumerator Ch8(Game g)
        {
            var s = g.State;
            MenuList.ScriptedChoices.Clear(); MenuList.ScriptedChoices.Enqueue(Route == "flee" ? 0 : 1);
            yield return Go(g, 13.5f);
            yield return Until(() => s.Has("c8_route_chosen") && Idle(g), 60f);
            MenuList.ScriptedChoices.Clear();
            Out("라비엘: " + s.D.friendRoute);
            if (Route == "flee") yield return Flee(g); else yield return FightBeast(g);
            yield return Until(() => s.Has("c8_duel_done") && Idle(g), 60f);
            yield return Go(g, 83.6f, 40f);
            yield return Map(g, "M33");
            yield return Use(g, 13f, 0);
            Out("재회 선택: " + s.D.reunionChoice + " 라비엘=" + s.D.ravielState + " 동료=" + (g.Companion != null ? g.Companion.actorId : "없음"));
            if (Cups) yield return Use(g, 17f);
            yield return Go(g, 27.6f);
            yield return Map(g, "M34");
            yield return Use(g, 12f);
            yield return Go(g, 43.6f, 60f);
            yield return Map(g, "M35");
            yield return Use(g, 36f, 2);
            Out("교량 잡담: 먹거리 " + StoryEvents.FoodName(g) + " 말풍선 본 수 " + s.D.bubblesSeen.Count);
            yield return Go(g, 71.6f, 60f);
            yield return Map(g, "M36");
        }

        static IEnumerator Flee(Game g)
        {
            RavielBeast b = null;
            float t = 0; int shutters = 0; var pressed = new HashSet<string>();
            while (!g.State.Has("c8_duel_done") && t < 240f)
            {
                b = Object.FindAnyObjectByType<RavielBeast>();
                if (b == null || !b.Active) { Ch46Demo.Release(); t += Time.deltaTime; yield return null; continue; }
                var p = g.Player; if (p == null || !g.FieldInputAllowed) { t += Time.deltaTime; yield return null; continue; }
                // 셔터 패널을 지나면 한 번씩 눌러 둔다
                foreach (var sw in Object.FindObjectsByType<ArenaSwitch>())
                    if (sw.kind.StartsWith("cshutter") && Mathf.Abs(sw.transform.position.x - p.transform.position.x) < 0.6f && sw.CanInteract(g) && shutters < 3 && !pressed.Contains(sw.kind))
                    { pressed.Add(sw.kind); Ch46Demo.Release(); yield return Tap(Act.Interact); shutters++; yield return new WaitForSeconds(0.2f); }
                GameInput.Virtual.Hold(Act.Right, true);
                if (b.State == RavielBeast.RState.LeapWindup && Mathf.Abs(b.transform.position.x - p.transform.position.x) < 4f) { yield return Dash(g, 1); }
                t += Time.deltaTime; yield return null;
            }
            Ch46Demo.Release();
            Out("도망 끝: 셔터 " + shutters + " 회, 셔터 부딪힘 " + (b != null ? b.ShutterHits : -1) + ", " + t.ToString("0") + "s");
        }

        static IEnumerator FightBeast(Game g)
        {
            RavielBeast b = null;
            float t = 0;
            while (!g.State.Has("c8_duel_done") && t < 300f)
            {
                b = Object.FindAnyObjectByType<RavielBeast>();
                if (b == null || !b.Active) { t += Time.deltaTime; yield return null; continue; }
                var p = g.Player; if (p == null || !g.FieldInputAllowed) { t += Time.deltaTime; yield return null; continue; }
                float dx = b.transform.position.x - p.transform.position.x;
                bool danger = (b.State == RavielBeast.RState.SwipeWindup && Mathf.Abs(dx) < 2.6f)
                              || (b.State == RavielBeast.RState.LeapWindup && Mathf.Abs(dx) < 2.5f)
                              || (b.State == RavielBeast.RState.ChargeWindup && Mathf.Abs(dx) < 6f);
                if (danger) { yield return Dash(g, dx > 0 ? -1 : 1); yield return new WaitForSeconds(0.2f); t += 0.3f; continue; }
                if (b.State == RavielBeast.RState.Leap || b.State == RavielBeast.RState.Charge || b.State == RavielBeast.RState.LeapWindup || b.State == RavielBeast.RState.ChargeWindup) { t += Time.deltaTime; yield return null; continue; }
                yield return Approach(g, b.transform.position.x, 1.3f);
                yield return Attack(g, Mathf.Sign(dx));
                t += Time.deltaTime; yield return null;
            }
            Out("멈춰 세우기 끝: 문 " + (b != null ? b.DoorsBroken : -1) + " 개 부서짐, " + t.ToString("0") + "s");
        }

        // ================================================================ 9장
        static IEnumerator Ch9(Game g)
        {
            var s = g.State;
            yield return Use(g, 9f); yield return Use(g, 22f); yield return Use(g, 14f);
            if (Tape)
            {
                yield return Use(g, 30f);
                yield return Map(g, "M38");
                yield return Use(g, 16f);
                yield return Use(g, 2f);
                yield return Map(g, "M36");
            }
            Out("대기실: D14 " + s.D.docs.Contains("D14") + " 테이프 " + s.Count("edward_tape"));
            yield return Go(g, 45.6f, 40f);
            yield return Ch9Stage(g);
        }

        static IEnumerator Ch9Stage(Game g)
        {
            var s = g.State;
            yield return Map(g, "M37");
            yield return Go(g, 9f);
            yield return Until(() => Object.FindAnyObjectByType<LacrimosaBoss>() != null && Idle(g), 60f);
            yield return FightLacrimosa(g);
            yield return Until(() => s.Has("c9_lac_done") && Idle(g), 90f);
            Out("라크리모사: " + s.D.lacrimosaResult);
            yield return Go(g, 49.6f, 40f);
            yield return Ch9Server(g);
        }

        static IEnumerator Ch9Server(Game g)
        {
            var s = g.State;
            yield return Map(g, "M39");
            yield return Go(g, 8f, 20f);
            yield return Until(() => s.Has("shelter_collapsed") && Idle(g), 120f);
            Out("붕괴: F·008 사망 " + StoryEvents.Dead(g, "f") + " 라크리모사 사망 " + StoryEvents.Dead(g, "lacrimosa") + " 베넬 " + s.D.venelleState + "/" + s.D.venelleDeathScene);
            yield return Go(g, 41.6f, 20f);
            yield return Map(g, "M40", 60f);
            yield return FloatTo(g, new Vector2(14f, 4.7f));
            yield return Tap(Act.Interact);
            yield return Map(g, "M41", 60f);
            yield return FloatTo(g, new Vector2(10f, 4.6f));
            MenuList.ScriptedChoices.Clear();
            foreach (var c in new[] { 0, 1, 2, 3 }) MenuList.ScriptedChoices.Enqueue(c);
            yield return Tap(Act.Interact);
            yield return Map(g, "M39", 90f);
            MenuList.ScriptedChoices.Clear();
            Out("휴지통: 복원 " + s.Has("c9_restored") + " 해제 " + s.Has("c9_released") + " 잡기 장면 " + s.Has("c9_caught"));
            yield return Go(g, 51.6f, 40f);
            yield return Map(g, "M42");
        }

        static IEnumerator FightLacrimosa(Game g)
        {
            LacrimosaBoss b = null;
            float t = 0; bool mixed = false, played = false; int lastRetry = g.State.D.retries;
            while (!g.State.Has("c9_lac_done") && t < 300f)
            {
                b = Object.FindAnyObjectByType<LacrimosaBoss>();
                if (b == null || !b.Active) { t += Time.deltaTime; yield return null; continue; }
                if (g.State.D.retries != lastRetry) { lastRetry = g.State.D.retries; mixed = false; played = false; }
                var p = g.Player; if (p == null || !g.FieldInputAllowed) { t += Time.deltaTime; yield return null; continue; }
                if (!mixed)
                {
                    // 믹서: 마이크 → 오른쪽 스피커(0 두 번), 게인 높음(3 두 번), 닫기
                    yield return Go(g, 6f, 10f);
                    MenuList.ScriptedChoices.Clear();
                    { int md = (2 - b.MicDir + 3) % 3, gn = (2 - b.Gain + 3) % 3; for (int i = 0; i < md; i++) MenuList.ScriptedChoices.Enqueue(0); if (!b.SpkR) MenuList.ScriptedChoices.Enqueue(2); for (int i = 0; i < gn; i++) MenuList.ScriptedChoices.Enqueue(3); MenuList.ScriptedChoices.Enqueue(g.State.Count("edward_tape") > 0 ? 5 : 4); }
                    yield return Tap(Act.Interact); yield return new WaitForSeconds(0.5f); yield return Until(() => Idle(g), 10f);
                    MenuList.ScriptedChoices.Clear(); mixed = true;
                    Out("믹서: 마이크 " + b.MicDir + " 게인 " + b.Gain + " 피드백 속도 " + b.FeedbackRate.ToString("0.00"));
                    continue;
                }
                if (mixed && !b.TalkOpen && b.FeedbackRate <= 0 && b.State == LacrimosaBoss.LState.Recover) { mixed = false; continue; }   // 보호 정지로 꺼진 스피커를 다시 켠다
                if (Tape && !played && g.State.Count("edward_tape") > 0 && b.ProtectStops >= 1 && b.State == LacrimosaBoss.LState.Recover)
                {
                    yield return Go(g, 6f, 10f);
                    MenuList.ScriptedChoices.Clear(); MenuList.ScriptedChoices.Enqueue(4);
                    yield return Tap(Act.Interact); yield return new WaitForSeconds(0.5f); yield return Until(() => Idle(g), 15f);
                    played = true; Out("녹음 재생 → 멈춤 " + b.State);
                    continue;
                }
                if (Lac == "persuade" && b.TalkOpen)
                {
                    yield return Go(g, b.transform.position.x - 1.0f, 8f);
                    MenuList.ScriptedChoices.Clear(); MenuList.ScriptedChoices.Enqueue(0);
                    yield return Tap(Act.Interact); yield return new WaitForSeconds(0.5f);
                    yield return Until(() => Idle(g) || g.State.Has("c9_lac_done"), 30f);
                    MenuList.ScriptedChoices.Clear();
                    if (!b.Active) { yield return Until(() => g.State.Has("c9_lac_done") || b.Active, 30f); continue; }
                    continue;
                }
                if (b.State == LacrimosaBoss.LState.WaveWindup || b.State == LacrimosaBoss.LState.Wave)
                {
                    // 바닥 파동은 점프로 넘는다
                    yield return new WaitForSeconds(0.15f);
                    if (b.State == LacrimosaBoss.LState.Wave) { GameInput.Virtual.Tap(Act.Jump); yield return new WaitForSeconds(0.5f); }
                    t += 0.3f; continue;
                }
                if (b.State == LacrimosaBoss.LState.PillarWindup) { yield return Go(g, 29f, 3f); t += 1f; continue; }
                if (Lac == "subdue" || b.State == LacrimosaBoss.LState.Stopped)
                {
                    yield return Approach(g, b.transform.position.x, 1.2f);
                    yield return Attack(g, Mathf.Sign(b.transform.position.x - p.transform.position.x));
                }
                t += Time.deltaTime; yield return null;
            }
            Out("라크리모사 전투 끝: 보호 정지 " + (b != null ? b.ProtectStops : -1) + " 녹음 " + played + " " + t.ToString("0") + "s");
        }

        // ================================================================ 10장
        static IEnumerator Ch10(Game g)
        {
            var s = g.State;
            yield return Use(g, 16f);
            yield return Go(g, 47.6f, 40f);
            yield return Map(g, "M43");
            yield return Go(g, 19f); yield return Until(() => Idle(g), 30f);
            yield return Go(g, 35f); yield return Until(() => s.Has("c10_meilin_seen") && Idle(g), 60f);
            yield return Use(g, 49f);
            yield return Go(g, 55.6f, 30f);
            yield return Ch10Final(g);
        }

        static IEnumerator Ch10Final(Game g)
        {
            var s = g.State;
            yield return Map(g, "M44");
            yield return Go(g, 6f);
            yield return Until(() => Object.FindAnyObjectByType<AureliusBoss>() != null && s.Has("c10_confront_done") && Idle(g), 90f);
            yield return FightAurelius(g);
            yield return Until(() => s.Has("c10_boss_done") && Idle(g), 120f);
            Out("최종전 끝: 한계 방출 " + s.D.limitReleases + " 최대 안정도 " + s.MaxHp("lu") + "/" + s.D.limitBaseMax + " 메이린 사망 " + s.Has("meilin_dead") + " 통과 0 " + s.Has("transfer_zero"));
            yield return Go(g, 47.6f, 30f);
            yield return Map(g, "M45");
            yield return Go(g, 64.5f, 90f);
            yield return Map(g, "M46", 60f);
        }

        static IEnumerator FightAurelius(Game g)
        {
            var s = g.State;
            float t = 0; int lastPhase = 0;
            while (!s.Has("c10_boss_done") && t < 600f)
            {
                var b = Object.FindAnyObjectByType<AureliusBoss>();
                var p = g.Player;
                if (b == null || p == null || !g.FieldInputAllowed) { t += Time.deltaTime; yield return null; continue; }
                if (b.Phase != lastPhase) { lastPhase = b.Phase; Out("최종전 " + b.Phase + "단계 시작 (안정도 " + s.D.lu.hp + "/" + s.MaxHp("lu") + ", 방출 " + s.D.limitReleases + ")"); }
                if (b.AwaitingRelease)
                {
                    if (s.D.lu.charge < s.MaxCharge("lu")) { int gs = Ch46Demo.Slot(g, "gather"); if (gs >= 0) yield return Tap(GameInput.SkillAct(gs)); yield return new WaitForSeconds(0.7f); t += 0.7f; continue; }
                    var cp = Marker.Find("coupling");
                    yield return Go(g, cp != null ? cp.transform.position.x - 0.5f : 37f, 10f);
                    yield return Tap(Act.Interact); yield return new WaitForSeconds(0.6f);
                    yield return Until(() => Idle(g) || s.Has("c10_boss_done"), 60f);
                    continue;
                }
                var mg = FindSwitch("magnet");
                if (b.Phase == 1 && b.EscortsShield && mg != null && b.CanUse(mg))
                {
                    yield return Go(g, mg.transform.position.x, 8f); yield return Tap(Act.Interact); yield return new WaitForSeconds(0.3f); t += 1f; continue;
                }
                if (b.Phase == 1 && b.MagnetOn)
                {
                    // 붙들린 호위 구를 부순다
                    var esc = Object.FindObjectsByType<AureliusBoss.Escort>();
                    if (esc.Length > 0 && mg != null) { yield return Approach(g, mg.transform.position.x, 0.4f); yield return Attack(g, Mathf.Sign(esc[0].transform.position.x - p.transform.position.x)); t += 0.4f; continue; }
                }
                if (b.Phase == 2 && b.StunLeft <= 0)
                {
                    var cap = FindCap(b);
                    if (cap != null) { yield return Approach(g, cap.transform.position.x, 1.0f); yield return Attack(g, Mathf.Sign(cap.transform.position.x - p.transform.position.x)); t += 0.4f; continue; }
                }
                if (b.State == AureliusBoss.AState.WaveWindup || b.State == AureliusBoss.AState.Wave) { if (b.State == AureliusBoss.AState.Wave) { GameInput.Virtual.Tap(Act.Jump); yield return new WaitForSeconds(0.5f); } else yield return null; t += 0.2f; continue; }
                if (b.State == AureliusBoss.AState.BeamWindup || b.State == AureliusBoss.AState.LinkWindup) { yield return Dash(g, p.transform.position.x < 24f ? 1 : -1); yield return new WaitForSeconds(0.4f); t += 0.5f; continue; }
                yield return Approach(g, b.transform.position.x, 1.3f);
                yield return Attack(g, Mathf.Sign(b.transform.position.x - p.transform.position.x));
                t += Time.deltaTime; yield return null;
            }
            Out("최종전 루프 " + t.ToString("0") + "s, 사망 재시도 " + (s.D.retries - deathsAtStart));
        }

        static ArenaSwitch FindSwitch(string kind) { foreach (var sw in Object.FindObjectsByType<ArenaSwitch>()) if (sw.kind == kind) return sw; return null; }
        static AureliusBoss.Capacitor FindCap(AureliusBoss b) { foreach (var c in Object.FindObjectsByType<AureliusBoss.Capacitor>()) if (c.Alive) return c; return null; }

        // ================================================================ 11장 · 엔딩
        static IEnumerator Ch11(Game g)
        {
            var s = g.State;
            // 마지막 대화 → 손잡기 / 이름 / 눈 감겨주기 → 일어선다
            MenuList.ScriptedChoices.Clear();
            foreach (var c in new[] { 0, 0, 0, 0 }) MenuList.ScriptedChoices.Enqueue(c);
            yield return Until(() => s.Has("c11_stood") && Idle(g), 240f);
            MenuList.ScriptedChoices.Clear();
            Out("중정: 라비엘 " + s.D.ravielState + " 손 " + s.Has("c11_hand") + " 이름 " + s.Has("c11_name") + " 눈 " + s.Has("c11_eyes"));
            yield return Go(g, 39.6f, 60f);
            yield return Map(g, "M47");
            MenuList.ScriptedChoices.Clear(); MenuList.ScriptedChoices.Enqueue(0);
            yield return Go(g, 14.5f);
            Out("몽타주 장면: " + string.Join(", ", StoryEvents.MontageLines(g)));
            yield return Until(() => EndingScreens.DesktopShown, 180f);
            MenuList.ScriptedChoices.Clear();
            yield return new WaitForSecondsRealtime(2f);
            MenuList.ScriptedChoices.Enqueue(1);   // 대기
            yield return new WaitForSecondsRealtime(6.5f);
            MenuList.ScriptedChoices.Enqueue(1);   // 다시 대기
            yield return new WaitForSecondsRealtime(6.5f);
            Out("대기 " + EndingScreens.WaitCount + "회 → 같은 선택 (창 닫힘 " + EndingScreens.WindowClosed + ")");
            MenuList.ScriptedChoices.Enqueue(0);   // 종료
            yield return Until(() => EndingScreens.WindowClosed, 15f);
            yield return new WaitForSecondsRealtime(4.5f);
            Out("엔딩 완료: 창 닫힘 " + EndingScreens.WindowClosed + ", 대기 " + EndingScreens.WaitCount + "회, 사망 재시도 " + (g.State.D.retries - deathsAtStart) + " → 타이틀 선택");
            MenuList.ScriptedChoices.Enqueue(0);   // 타이틀로
            yield return Until(() => g.State == null, 20f);
        }

        // ------------------------------------------------------------ 전투 도우미
        static IEnumerator Dash(Game g, int dir)
        {
            Ch46Demo.Release();
            var a = dir > 0 ? Act.Right : Act.Left;
            GameInput.Virtual.Hold(a, true); yield return new WaitForSeconds(0.05f);
            GameInput.Virtual.Hold(a, false); yield return new WaitForSeconds(0.05f);
            GameInput.Virtual.Hold(a, true); yield return new WaitForSeconds(0.2f);
            GameInput.Virtual.Hold(a, false);
        }

        static IEnumerator Approach(Game g, float x, float within)
        {
            var p = g.Player; float t = 0;
            while (p != null && Mathf.Abs(p.transform.position.x - x) > within && t < 1.2f)
            {
                bool right = x > p.transform.position.x;
                GameInput.Virtual.Hold(Act.Right, right); GameInput.Virtual.Hold(Act.Left, !right);
                t += Time.deltaTime; yield return null;
            }
            Ch46Demo.Release();
        }

        static IEnumerator Attack(Game g, float dir)
        {
            if (g.Player == null) yield break;
            g.Player.SetFacing(dir >= 0 ? 1 : -1);
            int tok = Ch46Demo.Slot(g, "tok");
            if (tok >= 0) yield return Tap(GameInput.SkillAct(tok));
            yield return new WaitForSeconds(0.34f);
        }

        static IEnumerator FloatTo(Game g, Vector2 dest)
        {
            yield return Until(() => Idle(g), 30f);
            float t = 0;
            while (g.Player != null && Vector2.Distance(g.Player.transform.position, dest) > 0.2f && t < 12f)
            {
                Vector2 d = dest - (Vector2)g.Player.transform.position;
                GameInput.Virtual.Hold(Act.Right, d.x > 0.12f); GameInput.Virtual.Hold(Act.Left, d.x < -0.12f);
                GameInput.Virtual.Hold(Act.MenuUp, d.y > 0.12f); GameInput.Virtual.Hold(Act.Down, d.y < -0.12f);
                t += Time.deltaTime; yield return null;
            }
            GameInput.Virtual.ReleaseAll(); yield return new WaitForSeconds(0.25f);
        }
    }
}
#endif
