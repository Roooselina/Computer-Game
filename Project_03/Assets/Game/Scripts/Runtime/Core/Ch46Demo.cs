#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 개발용 자동 진행 (가상 입력, 에디터 전용): 승강장 세이브에서 이어하기 → 실제 출구·이벤트 조건을 거쳐 7장 수용동 연결문까지.
    /// 사람의 손 조작이 아니다. 사용자 저장(LocalLow)은 읽기만 하고 쓰지 않는다 — 복사본을 Temp/EWDevSave 에 두고 그것으로 이어한다.
    /// 경로 설정: Temp/EWBridge/ch46.txt 한 줄 "venelle=ally|apart|dead deal=gift|pump|fight snacks=N optional=1|0 hall=restore|vent wrong=1|0"
    /// 기록: Temp/EWBridge/demo.txt (StealthDemo.Out).
    /// </summary>
    public static class Ch46Demo
    {
        public static string Venelle = "ally", Deal = "gift", Hall = "restore", SkipTo = "";
        public static int Snacks = -1;
        public static bool Optional = true, Wrong = true, Through, FromCurrent;
        public static bool Done;
        static float t0;

        static string UserSave => System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            "AppData/LocalLow/DefaultCompany/ELECTRIC WORLD - LAST LIGHT (Demo)/ew_lastlight_save.json.v2_original");

        public static void ReadConfig()
        {
            // 플레이 모드 도메인 리로드가 꺼져 있어 정적 값이 남는다 → 매번 기본값부터
            Venelle = "ally"; Deal = "gift"; Hall = "restore"; SkipTo = ""; Snacks = -1; Optional = true; Wrong = true; Through = false; Ch711Demo.ResetConfig();
            try
            {
                var path = System.IO.Path.Combine(Application.dataPath, "../Temp/EWBridge/ch46.txt");
                if (!System.IO.File.Exists(path)) return;
                foreach (var kv in System.IO.File.ReadAllText(path).Trim().Split(' '))
                {
                    var p = kv.Split('='); if (p.Length != 2) continue;
                    switch (p[0])
                    {
                        case "venelle": Venelle = p[1]; Ch711Demo.Config("venelle", p[1]); break;
                        case "deal": Deal = p[1]; break;
                        case "snacks": Snacks = int.Parse(p[1]); break;
                        case "optional": Optional = p[1] == "1"; break;
                        case "hall": Hall = p[1]; break;
                        case "wrong": Wrong = p[1] == "1"; break;
                        case "skipto": SkipTo = p[1]; break;
                        case "through": Through = p[1] == "1"; break;
                        default: Ch711Demo.Config(p[0], p[1]); break;
                    }
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[EW] ch46 설정 읽기 실패: " + e.Message); }
        }

        /// <summary>승강장 세이브 복사본을 개발 저장 폴더에 두고 (선택 시 베넬 상태·과자 수만 바꿔) 이어하기.</summary>
        public static IEnumerator StartFromLiftSave(Game g)
        {
            Done = false;
            while (g.Config == null || g.Db == null) yield return null;
            string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWDevSave"));
            SaveSystem.OverrideDirectory = dir;
            System.IO.Directory.CreateDirectory(dir);
            foreach (var f in System.IO.Directory.GetFiles(dir)) System.IO.File.Delete(f);
            string json = System.IO.File.ReadAllText(UserSave);       // 읽기만
            var d = JsonUtility.FromJson<SaveData>(json);
            if (Venelle == "apart")
            {
                d.venelleState = "apart"; d.flags.Remove("c3_joined"); d.flags.Add("venelle_apart"); d.flags.Add("venelle_bench");
            }
            else if (Venelle == "dead")
            {
                d.venelleState = "dead"; d.venelle.state = "dead"; d.venelleDeathScene = "c06_corridor";
                d.flags.Remove("c3_joined"); d.flags.Remove("c3_rest_done"); d.flags.Remove("c3_negotiated");
                foreach (var f in new[] { "venelle_dead", "c3_killed", "c3_subdued" }) d.flags.Add(f);
            }
            if (Snacks >= 0) { d.items.RemoveAll(i => i.k == "snack"); if (Snacks > 0) d.items.Add(new KV("snack", Snacks)); }
            System.IO.File.WriteAllText(System.IO.Path.Combine(dir, SaveSystem.FileName), JsonUtility.ToJson(d, true));
            Out("시작: 승강장 세이브 복사본 (v" + d.version + ", " + d.mapId + " x=" + d.posX.ToString("0.0") + ", 베넬=" + Venelle + ", 과자=" + CountSnack(d) + ", 교섭=" + Deal + ", 선택 구간=" + Optional + ")");
            if (!g.ContinueGame(out var msg)) { Out("이어하기 실패: " + msg); yield break; }
            yield return Until(() => g.CurrentMapId == "M15" && g.Player != null && !g.Loading, 20f);
            Out("이어하기 성공: M15 위치 " + Pos(g) + ", 저장 이전 형식에서 변환=" + SaveSystem.LastMigratedFrom);
        }

        static int CountSnack(SaveData d) { foreach (var i in d.items) if (i.k == "snack") return i.v; return 0; }
        internal static string Pos(Game g) => g.Player != null ? ((Vector2)g.Player.transform.position).ToString("0.0") : "-";
        internal static void Out(string s) { StealthDemo.Out("[ch4-6 " + (Time.realtimeSinceStartup - t0).ToString("0.0") + "s] " + s); Debug.Log("[EW] " + s); }

        // ------------------------------------------------------------ 입력 도우미
        internal static IEnumerator Until(System.Func<bool> c, float timeout = 60f)
        {
            float t = 0;
            while (!c() && t < timeout) { t += Time.unscaledDeltaTime; yield return null; }
            if (t >= timeout) Out("대기 시간 초과 " + DevHooks.Status());
        }
        internal static bool Idle(Game g) => g.FieldInputAllowed && !Cutscene.Running && g.Player != null && !g.UI.dialogue.Open && !g.Loading;
        internal static IEnumerator WaitIdle(Game g, float timeout = 90f) => Until(() => Idle(g), timeout);
        internal static IEnumerator Tap(Act a) { GameInput.Virtual.Tap(a); yield return null; yield return null; }
        internal static int Slot(Game g, string id) => g.State.D.slots.IndexOf(id);

        internal static IEnumerator Map(Game g, string id, float timeout = 30f)
        {
            yield return Until(() => g.CurrentMapId == id && g.Player != null && !g.Loading, timeout);
            yield return WaitIdle(g);
            Out("맵 " + id + " 도착 " + Pos(g) + " 과자 " + g.State.Count("snack"));
        }

        /// <summary>걸어서 x 까지. 앞을 막는 경비·생존자는 톡! 한 번 → 대시로 빠져나간다. 켜진 CCTV 는 간섭.</summary>
        internal static IEnumerator Go(Game g, float x, float timeout = 40f, bool crouch = false)
        {
            float t = 0, last = 1e9f, stuck = 0;
            while (g.Player != null && Mathf.Abs(g.Player.transform.position.x - x) > 0.15f && t < timeout)
            {
                if (!g.FieldInputAllowed) { Release(); t += Time.unscaledDeltaTime; yield return null; continue; }
                var p = g.Player; bool right = x > p.transform.position.x;
                var sk = p.Skills; sk.UpdateSuggestions();
                int jam = Slot(g, "jam");
                if (jam >= 0 && sk.SuggestJam is Cctv cam && !cam.Jammed && sk.CooldownLeft("jam") <= 0 && g.State.D.lu.charge >= 1)
                { Release(); p.SetFacing(right ? 1 : -1); yield return Tap(GameInput.SkillAct(jam)); yield return new WaitForSeconds(0.4f); }
                foreach (var gd in Guard.All.ToArray())
                {
                    if (gd == null || !gd.Alive || gd.flying) continue;
                    float dx = gd.transform.position.x - p.transform.position.x;
                    if (Mathf.Sign(dx) == (right ? 1 : -1) && Mathf.Abs(dx) < 1.3f && Mathf.Abs(gd.transform.position.y - p.transform.position.y) < 1.2f && gd.State != Guard.GState.Stagger && gd.State != Guard.GState.Jammed)
                    {
                        Release(); p.SetFacing(right ? 1 : -1);
                        int tok = Slot(g, "tok"); if (tok >= 0) yield return Tap(GameInput.SkillAct(tok));
                        yield return new WaitForSeconds(0.25f);
                        GameInput.Virtual.Hold(right ? Act.Right : Act.Left, true);
                        yield return Tap(Act.Dodge);
                        yield return new WaitForSeconds(0.25f);
                        break;
                    }
                }
                if (GameInput.IsLatched(right ? Act.Right : Act.Left)) { Release(); yield return null; }
                GameInput.Virtual.Hold(Act.Right, right); GameInput.Virtual.Hold(Act.Left, !right); GameInput.Virtual.Hold(Act.Down, crouch);
                float px = p.transform.position.x;
                stuck = Mathf.Abs(px - last) < 0.002f ? stuck + Time.unscaledDeltaTime : 0; last = px;
                if (stuck > 0.3f && p.Grounded && !crouch) { GameInput.Virtual.Tap(Act.Jump); stuck = 0; }
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            Release();
            if (t >= timeout) Out("이동 시간 초과 → x=" + x + " " + DevHooks.Status());
        }

        internal static void Release() { GameInput.Virtual.Hold(Act.Right, false); GameInput.Virtual.Hold(Act.Left, false); GameInput.Virtual.Hold(Act.Down, false); GameInput.Virtual.Hold(Act.MenuUp, false); }

        /// <summary>x 에서 조사(Space). choices 는 이어지는 선택지 자동 응답.</summary>
        internal static IEnumerator Use(Game g, float x, params int[] choices)
        {
            yield return WaitIdle(g);
            yield return Go(g, x);
            yield return new WaitForSeconds(0.15f);
            MenuList.ScriptedChoices.Clear(); foreach (var c in choices) MenuList.ScriptedChoices.Enqueue(c);
            yield return Tap(Act.Interact);
            yield return new WaitForSeconds(0.3f);
            yield return WaitIdle(g);
            MenuList.ScriptedChoices.Clear();
            yield return new WaitForSeconds(0.2f);
        }

        internal static IEnumerator Climb(Game g, float ladderX, float topY, float timeout = 12f)
        {
            yield return Go(g, ladderX);
            float t = 0;
            GameInput.Virtual.Hold(Act.MenuUp, true);
            while (g.Player != null && !(g.Player.transform.position.y >= topY - 0.05f && !g.Player.Climbing) && t < timeout) { t += Time.unscaledDeltaTime; yield return null; }
            GameInput.Virtual.Hold(Act.MenuUp, false);
            yield return new WaitForSeconds(0.2f);
            if (t >= timeout) Out("사다리 시간 초과 " + DevHooks.Status());
        }

        internal static IEnumerator UseSkill(Game g, string id)
        {
            yield return WaitIdle(g);
            int s = Slot(g, id);
            if (s < 0) { Out("기술 " + id + " 칸에 없음"); yield break; }
            yield return Tap(GameInput.SkillAct(s));
            yield return new WaitForSeconds(0.6f);
        }

        // ------------------------------------------------------------ 전체 경로
        public static IEnumerator Run(Game g)
        {
            t0 = Time.realtimeSinceStartup;
            GameInput.AutoReadDelay = 0.9f;
            if (FromCurrent) { FromCurrent = false; Done = false; Out("이어서 진행 (새 게임부터 온 현재 상태): " + g.CurrentMapId + " " + Pos(g) + " 베넬=" + g.State.D.venelleState + " 과자=" + g.State.Count("snack")); }
            else yield return StartFromLiftSave(g);
            if (!string.IsNullOrEmpty(SkipTo)) { yield return RunFrom(g, SkipTo); Done = true; yield break; }
            // M15 → 오른쪽 출구
            yield return WaitIdle(g);
            yield return Go(g, 49.6f);
            yield return Map(g, "M16");
            // 16 화물 승강장: 상층 버튼(봉쇄) 확인 → 지하로
            yield return Use(g, 29f, 1);
            yield return Use(g, 29f, 0);
            yield return Until(() => g.State.Has("m16_lift_down") && Idle(g), 20f);
            Out("승강기로 지하 도착 " + Pos(g));
            yield return Go(g, 55.6f, 40f);
            yield return Map(g, "M17");
            // 17 폐기소: 만남 → 교섭
            int deal = Deal == "gift" ? 0 : Deal == "pump" ? 1 : 2;
            MenuList.ScriptedChoices.Clear(); MenuList.ScriptedChoices.Enqueue(deal);
            yield return Go(g, 7f);
            yield return Until(() => g.State.Has("waste_resolved") || g.State.Has("waste_fight") || g.State.Has("waste_pump_promised"), 60f);
            yield return WaitIdle(g);
            Out("교섭 결과: 선택 " + Deal + " → resolved=" + g.State.Has("waste_resolved") + " fight=" + g.State.Has("waste_fight") + " 과자 " + g.State.Count("snack"));
            if (Deal == "fight") yield return FightSurvivors(g);
            if (Deal == "pump" || (Deal == "fight" && Optional)) yield return PumpRoom(g);
            // F 이름 장면 → 008 에게 과자
            yield return Use(g, 21.9f);
            Out("F 이름 장면: " + g.State.Has("waste_f_name"));
            if (g.State.Count("snack") > 0) { yield return Use(g, 25.9f, 0); Out("008 과자: fed=" + g.State.D.child008Fed + " 남은 과자 " + g.State.Count("snack")); }
            else { yield return Use(g, 25.9f); Out("008: 과자 없음 → 이름 대화"); }
            if (Optional)
            {
                yield return Use(g, 39f);
                yield return Map(g, "M19");
                yield return Use(g, 10.9f, 0);
                yield return Use(g, 20.9f);
                yield return Use(g, 28.9f);
                Out("시장: 등불=" + g.State.Has("market_lantern_lit") + " 선=" + g.State.Has("side_lines_done") + " 과자 " + g.State.Count("snack"));
                yield return Use(g, 2f);
                yield return Map(g, "M17");
            }
            yield return Go(g, 49.6f, 40f);
            yield return Map(g, "M20");
            // 20 유지보수 터널: 낮은 천장은 웅크려서
            yield return Go(g, 20.5f);
            yield return Go(g, 33.8f, 30f, true);
            yield return new WaitForSeconds(0.3f);
            yield return Go(g, 59.6f, 60f);
            yield return Map(g, "M21");
            // 21 정비 작업장: 펄스 버스트 익힘 → 'R 칸 (방전 대신)' → 시험대에서 사용
            yield return Use(g, 9f, 2);
            Out("펄스 익힘: 칸 " + string.Join(",", g.State.D.slots));
            yield return Go(g, 17f);
            yield return UseSkill(g, "pulse");
            yield return Until(() => g.State.Has("ws_tested") && Idle(g), 45f);
            Out("시험: 작업등 " + Device.Find("ws_lamp")?.Model.power + " / 배터리 작업등 " + Device.Find("ws_lamp_battery")?.Model.power);
            yield return Use(g, 23f);
            yield return Go(g, 33.6f);
            yield return Map(g, "M22");
            yield return ConveyorHall(g);
            yield return Map(g, "M23");
            yield return Shaft(g);
            yield return Map(g, "M24");
            yield return Dorm(g);
            Out("끝: " + g.CurrentMapId + " ch7_ready=" + g.State?.Has("ch7_ready"));
            if (Through) { yield return Ch711Demo.Run(g); }
            Done = true;
        }

        /// <summary>디버그 전용: 앞 구간을 이미 지난 상태로 만들고 해당 맵부터 (최종 검증은 처음부터 이어서 한다).</summary>
        static IEnumerator RunFrom(Game g, string map)
        {
            var s = g.State;
            Out("디버그: " + map + " 부터 (앞 구간 플래그를 넣음 — 전체 연결 검증 아님)");
            foreach (var f in new[] { "waste_met", "waste_resolved", "waste_pump_fixed", "waste_water_on", "ev_c4_start", "ev_c5_start", "m16_lift_down" }) s.Set(f);
            s.D.wasteResult = "repair";
            if (map != "M21" && map != "M20") { s.Set("pulse_learned"); s.Set("ws_tested"); if (!s.D.skills.Contains("pulse")) s.D.skills.Add("pulse"); s.D.slots[2] = "pulse"; }
            g.GoToMap(map, "start");
            yield return Map(g, map);
            switch (map)
            {
                case "M22": yield return ConveyorHall(g); yield return Map(g, "M23"); goto case "M23s";
                case "M23": goto case "M23s";
                case "M23s": yield return Shaft(g); yield return Map(g, "M24"); goto case "M24s";
                case "M24": goto case "M24s";
                case "M24s": yield return Dorm(g); break;
            }
            Out("끝: " + g.CurrentMapId + " ch7_ready=" + g.State?.Has("ch7_ready"));
        }

        static IEnumerator FightSurvivors(Game g)
        {
            Out("비살상 충돌 시작");
            float t = 0;
            while (g.State != null && !g.State.Has("waste_fight_done") && t < 120f)
            {
                if (!g.FieldInputAllowed) { Release(); t += Time.unscaledDeltaTime; yield return null; continue; }
                var p = g.Player;
                Scavenger target = null; float best = 1e9f;
                foreach (var s in Scavenger.All) if (s != null && s.Active) { float dd = Mathf.Abs(s.transform.position.x - p.transform.position.x); if (dd < best) { best = dd; target = s; } }
                if (target == null) { t += Time.unscaledDeltaTime; yield return null; continue; }
                float dx = target.transform.position.x - p.transform.position.x;
                if (target.State == Scavenger.SState.Windup && Mathf.Abs(dx) < 1.6f)
                {
                    GameInput.Virtual.Hold(dx > 0 ? Act.Left : Act.Right, true);
                    yield return Tap(Act.Dodge);
                    yield return new WaitForSeconds(0.3f); Release();
                }
                else if (Mathf.Abs(dx) > 0.95f) { GameInput.Virtual.Hold(Act.Right, dx > 0); GameInput.Virtual.Hold(Act.Left, dx < 0); yield return null; }
                else { Release(); p.SetFacing(dx > 0 ? 1 : -1); yield return Tap(GameInput.SkillAct(Slot(g, "tok"))); yield return new WaitForSeconds(0.35f); }
                t += Time.unscaledDeltaTime;
            }
            Release();
            yield return WaitIdle(g);
            Out("충돌 끝: 진정 " + g.State.Has("waste_calm_a") + "/" + g.State.Has("waste_calm_b") + "/" + g.State.Has("waste_calm_c") + " 안정도 " + g.State.D.lu.hp + " 재시도 " + g.State.D.retries);
        }

        static IEnumerator PumpRoom(Game g)
        {
            yield return Use(g, 33f);
            yield return Map(g, "M18");
            if (Wrong)
            {
                // 잘못된 순서부터: 물이 찬 채 차단기 → 튐 / 물속 이음쇠 → 손 안 닿음
                yield return Use(g, 25f);
                Out("오조작: 물 위로 전원 → 전원 " + PumpRig.PowerOn + " (차단기 튐 " + g.State.GetVar("pump_trips") + "회)");
                yield return Go(g, 21.6f);
                yield return Use(g, 17.6f);
                Out("오조작: 물속 이음쇠 → 고침 " + PumpRig.PartFixed);
            }
            yield return Use(g, 27.4f);             // 부품 선반
            yield return Use(g, 5.5f);              // 상태판
            yield return Use(g, 9f);                // 배수 밸브 열기
            yield return Until(() => PumpRig.Water <= 0, 10f);
            Out("배수 끝: 수위 " + PumpRig.Water);
            if (Wrong)
            {
                yield return Use(g, 25f);           // 깨진 채 전원 → 헛돎 (전원은 켜진 채)
                yield return Go(g, 21.6f);
                yield return Use(g, 17.6f);         // 전원 켠 채 이음쇠 → 손 뗌
                Out("오조작: 전원 켠 채 이음쇠 → 고침 " + PumpRig.PartFixed);
                yield return Go(g, 22.8f); yield return Use(g, 25f);   // 전원 내림
            }
            yield return Go(g, 21.6f);
            yield return Use(g, 17.6f);             // 이음쇠 교체
            yield return Go(g, 22.8f);
            yield return Use(g, 25f);               // 전원 → 가동
            Out("펌프: 가동 " + PumpRig.Running + " fixed=" + g.State.Has("waste_pump_fixed") + " 결과=" + g.State.D.wasteResult + " 화해=" + g.State.D.wasteReconciled);
            yield return Use(g, 2f);
            yield return Map(g, "M17");
        }

        static IEnumerator ConveyorHall(Game g)
        {
            // 벨트 제어반으로 방향을 바꿔 벨트를 타고 간다
            yield return Use(g, 10f);
            Out("벨트 방향: " + Conveyor.Find("hall")?.Dir);
            yield return Go(g, 40.2f, 40f);
            // 모터 옆에서 펄스 → 홀 전원선 정전 (벨트·PC·문), 배터리 PC 는 켜짐
            yield return UseSkill(g, "pulse");
            yield return Until(() => g.State.Has("hall_off_said") && Idle(g), 15f);
            Out("과부하: 벨트 " + (Conveyor.Find("hall")?.Running) + " PC " + Device.Find("hall_pc")?.Model.power + " 배터리 PC " + Device.Find("hall_pc_battery")?.Model.power + " 문 " + Device.Find("hall_door")?.Model.power);
            if (Hall == "vent")
            {
                float floorY = g.Player.transform.position.y;
                yield return Climb(g, 56.5f, floorY + 5f);
                yield return Go(g, 61.6f);
                yield return Until(() => g.Player.Grounded && g.Player.transform.position.y < floorY + 0.2f, 5f);
                Out("환기 통로 우회 " + Pos(g));
            }
            else
            {
                yield return Use(g, 55f);
                yield return new WaitForSeconds(0.5f);
                Out("전원 복구: 문 " + Device.Find("hall_door")?.Model.power);
            }
            yield return Go(g, 63.6f, 40f);
        }

        static IEnumerator Shaft(Game g)
        {
            float l2 = g.Player.transform.position.y + 8f, l3 = g.Player.transform.position.y + 16f;
            yield return Go(g, 8f);
            Out("추격 시작: " + g.State.Has("m23_chase"));
            yield return Climb(g, 26.5f, l2);
            yield return Go(g, 22f);
            yield return Until(() => g.State.Has("m23_l2_guard") && Idle(g), 15f);
            if (g.State.VenelleIsAlly)
            {
                yield return Use(g, 14.5f);
                Out("베넬 산성 + 바닥판 전류: 판 " + g.State.Has("m23_plate_done"));
            }
            else
            {
                yield return Use(g, 16.5f);
                Out("셔터: " + g.State.Has("m23_shutter"));
            }
            yield return Climb(g, 18.5f, l3);
            yield return Go(g, 23.5f);
            yield return UseSkill(g, "wire");
            yield return Until(() => Idle(g) && g.Player.transform.position.y > l3 + 2f, 15f);
            yield return Go(g, 29.6f, 30f);
        }

        static IEnumerator Dorm(Game g)
        {
            yield return Use(g, 19.5f);
            if (g.State.VenelleIsAlly) yield return Use(g, 19.5f, 0);
            yield return Use(g, 25.5f);
            if (Optional)
            {
                yield return Use(g, 33f);
                yield return Map(g, "M26");
                yield return Use(g, 14f);
                Out("옥상: " + g.State.Has("roof_done"));
                yield return Use(g, 2f);
                yield return Map(g, "M24");
            }
            yield return Use(g, 39f);
            yield return Map(g, "M25");
            yield return Use(g, 14.6f);
            yield return Use(g, 13.3f);
            yield return Use(g, 18f);
            Out("방: 사진 " + g.State.Has("room_photo_seen") + " 편지 " + g.State.Has("room_letter_seen") + " 과거 장면 " + g.State.Has("ivy_memory_done") + " 현재 대화 " + g.State.Has("ivy_room_done") + " 신뢰 " + g.State.D.venelleTrust);
            yield return Use(g, 2f);
            yield return Map(g, "M24");
            Out("7장 직전 확인 목록:\n" + StoryEvents.Ch7Checklist(g, 0) + "\n" + StoryEvents.Ch7Checklist(g, 1));
            yield return Use(g, 49f, 0);
            yield return Until(() => g.CurrentMapId == "M27" && g.Player != null && !g.Loading, 30f);
        }
    }
}
#endif
