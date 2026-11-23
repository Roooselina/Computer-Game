using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EW.EditorTools
{
    /// <summary>
    /// 7~11장 맵 (2026-11-15). GDD 5장 표의 맵 ID 27~47 을 그대로 쓴다.
    /// 출구는 Resources/EW/world_layout.json 의 출입구 ID 로 만든다 (LExit/LDoor): 도착 맵·스폰·잠금 조건을 데이터에서 읽는다.
    /// 지형은 GDD 기준 임시 배치 — 건물 도면이 오면 이 파일의 격자·좌표와 배치 데이터를 함께 고친다 (Docs/BUILDING_LAYOUT_GUIDE.md).
    /// 그림은 기존 개별 도트(연구소 타일·소품·패럴랙스 레이어)와 Tools/ch7_11_art_gen.py 의 개별 임시 소품·인물로 조립 — 통짜 배경 없음.
    /// 화면 밖 '촬영 구역'(feed_*)은 시설 카메라 영상용 무대다 (FeedView).
    /// </summary>
    public static partial class EWRooms
    {
        static IEnumerable<RoomDef> LateRooms()
        {
            yield return M27(); yield return M28(); yield return M29(); yield return M30(); yield return M31();
            yield return M32(); yield return M33(); yield return M34(); yield return M35();
            yield return M36(); yield return M37(); yield return M38(); yield return M39(); yield return M40(); yield return M41();
            yield return M42(); yield return M43(); yield return M44(); yield return M45(); yield return M46(); yield return M47();
        }

        // ---------------------------------------------------------------- 배치 데이터 출구
        static EW.WorldLayout layoutCache;
        static EW.WorldLayout Layout
        {
            get
            {
                if (layoutCache == null)
                {
                    var path = "Assets/Game/Resources/EW/world_layout.json";
                    layoutCache = EW.WorldLayout.Parse(System.IO.File.ReadAllText(path));
                    foreach (var e in layoutCache.Errors) Debug.LogError("[EW] 배치 데이터: " + e);
                }
                return layoutCache;
            }
        }
        public static void ResetLayoutCache() { layoutCache = null; }

        static EW.WorldLayout.Exit LX(string exitId)
        {
            var e = Layout.FindExit(exitId);
            if (e == null) throw new System.Exception("배치 데이터에 없는 출구: " + exitId);
            return e;
        }

        /// <summary>가장자리 출구 (배치 데이터의 도착 맵·스폰·잠금).</summary>
        static void LExit(Ctx c, string exitId, int c0, int r0, int c1, int r1)
        {
            var e = LX(exitId);
            var z = c.Exit(c0, r0, c1, r1, e.to, e.spawn, string.IsNullOrEmpty(e.block) ? null : e.block, string.IsNullOrEmpty(e.blocked) ? null : e.blocked, string.IsNullOrEmpty(e.require) ? null : e.require);
            z.name = "Exit_" + exitId;
        }

        /// <summary>문 출구 (조사 키로 연다).</summary>
        static DoorExit LDoor(Ctx c, string exitId, int col, int row, string prompt, Sprite s = null)
        {
            var e = LX(exitId);
            var d = c.Door(col, row, e.to, e.spawn, prompt, s);
            d.id = exitId;
            if (!string.IsNullOrEmpty(e.require)) d.requireFlag = e.require;
            if (!string.IsNullOrEmpty(e.blocked)) d.lockedDialogue = e.blocked;
            d.gameObject.name = "Door_" + exitId;
            return d;
        }

        /// <summary>표준 인물: 기본으로 보이면 hide_id 로 숨기고, 기본으로 숨으면 show_id 로 보인다 (StoryEvents.ShowActor).</summary>
        static void Who(Ctx c, int col, int row, string id, string art, int facing, bool hidden = false, string idle = "Idle", string hideFlag2 = null, float dx = 0)
            => c.Actor(col, row, id, art, facing, hidden ? "show_" + id : null, "hide_" + id, hideFlag2, idle, dx);

        static void Lamps(Ctx c, float y, params float[] xs) { foreach (var x in xs) c.WorkLamp(new Vector2(x, y), null, 2.6f); }

        /// <summary>화면 밖 촬영 구역의 배경 (벽·바닥 띠). 시설 카메라 영상에만 보인다.</summary>
        static void FeedSet(Ctx c, string id, Vector2 at, float w, Color wall)
        {
            var px = Ctx.P0("Tile_Pixel"); float s = px != null ? px.bounds.size.x : 0.125f;
            var bg = c.Obj("FeedBg_" + id, at + new Vector2(0, 2.6f)); var sr = c.Sprite(bg, px, -40); sr.color = wall; bg.transform.localScale = new Vector3(w / s, 5.4f / s, 1);
            var fl = c.Obj("FeedFloor_" + id, at + new Vector2(0, -0.25f)); var fsr = c.Sprite(fl, px, -39); fsr.color = new Color(0.16f, 0.17f, 0.19f); fl.transform.localScale = new Vector3(w / s, 0.5f / s, 1);
            c.Mark(id, at);
        }

        static int FeedRow(Ctx c, float y) => Mathf.RoundToInt(c.H - 1 - y);

        // ================================================================ 27 수용동 (7장 시작)
        static RoomDef M27()
        {
            var g = Box(64, 12);
            g.Rect(0, 7, 0, 10, '.').Rect(63, 7, 63, 10, '.');
            g.Set(24, 10, 'k');
            var r = R("M27", "M27_HoldingBlock", "수용동", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_dark";
            r.layers = c => UnderLayers(c, 1, 0.24f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1); c.Spawn(61, 10, "from_hall", -1);
                LExit(c, "M27.west", 0, 7, 0, 10);
                LExit(c, "M27.east", 63, 7, 63, 10);
                c.Checkpoint(1, 7, 2, 10); c.Checkpoint(41, 7, 42, 10);
                c.EventZone(4, 7, 5, 10, "c7_enter");
                // 비상 계단 (지하 배수로 → 폐기소 방향). 풀려난 사람들이 이리로 내려간다.
                c.Decor(c.Cell(6, 10), Ctx.P("Prop_Door").ElementAtOrDefault(2), -3, 1, "StairDoor");
                c.Mark("stairs", c.Cell(6, 10));
                c.Inspect(c.Cell(7, 10), null, "m27_stairs", null, ev: "c7_stairs", prompt: "비상 계단", range: 1.0f).promptOffset = new Vector2(0, 2.3f);
                // 수용칸 셋 (창살: 닫힘 → holding_open 이면 열림)
                int k = 1;
                foreach (var x in new[] { 14f, 22f, 30f })
                {
                    c.Frames(new Vector2(x, f), Ctx.SF("U_CellBars"), 3, "CellBars", ("holding_open", 1));
                    c.Decor(new Vector2(x, f), Ctx.S("U_CrateBed"), -4, 1, "CellBed");
                }
                foreach (var (col, art) in new[] { (13, "survivorA"), (15, "survivorB"), (21, "survivorB"), (29, "survivorA"), (31, "survivorB") })
                { Who(c, col, 10, "held" + k, art, -1, false, "Sit", "holding_released"); k++; }
                c.Mark("held_center", c.Cell(22, 10));
                var panel = c.Inspect(c.Cell(38, 10), Ctx.SF("U_CellPanel").FirstOrDefault(), "m27_panel", null, ev: "c7_release", order: 1, prompt: "수용칸 제어반");
                panel.hideFlag = "holding_released";
                c.Frames(c.Cell(38, 10), Ctx.SF("U_CellPanel"), 0, "CellPanelLamp", ("holding_open", 1));
                // 경비 책상: 직원 카드 + 수용동 명부(D10)
                c.Decor(c.Cell(47, 10), Ctx.P0("Prop_Desk"), -2, 1, "GuardDesk");
                c.Decor(c.Cell(47, 10) + new Vector2(0, 0.9f), Ctx.P("Prop_Computer").ElementAtOrDefault(0), 1, 1, "DeskPC");
                c.Inspect(c.Cell(47, 10), null, "m27_desk", null, ev: "c7_desk", prompt: "경비 책상", range: 1.0f);
                // 감시: 책상 쪽 순찰 + 수용칸을 보는 CCTV (고정) / 드론 (선택 — 숨으면 피할 수 있다)
                c.Guard(52, 10, "M27_g1", "patrol_guard", 43.5f, 58.5f, -1);
                c.Camera(new Vector2(34.5f, f + 4.4f), "M27_cam", -160f, -40f, 0.3f);
                c.Guard(20, 10, "M27_d1", "capture_drone", 16f, 28f, 1, null, true, 3.0f);
                c.HideCover(10.2f, f, "S_HideRack"); c.HideCover(42.4f, f, "S_HideSacks"); c.HideCover(55.6f, f, "S_HideRack");
                c.ExposedWire(36, 10, "M27_wire", 1);
                c.Reinforce(new Vector2(62.5f, f + 3f));
                Lamps(c, f + 4.6f, 9f, 26f, 50f);
                c.BubbleZone(16, 7, 32, 10, "walk");
            };
            r.useAlarmAmbience = false;
            return r;
        }

        // ================================================================ 28 접수 홀 · 메이린 보스전
        static RoomDef M28()
        {
            var g = Box(46, 14);
            g.Rect(0, 9, 0, 12, '.');
            var r = R("M28", "M28_ReceptionHall", "접수 홀", Theme.Lab, g.Rows());
            r.ambience = "amb_lab"; r.music = "bgm_battle";
            r.layers = c => { EWSceneBuilder.LabLayers(c, 0); };
            r.extra = c =>
            {
                float f = c.H - 13;
                c.Spawn(2, 12, "start", 1); c.Spawn(42, 12, "from_booth", -1);
                LExit(c, "M28.west", 0, 9, 0, 12);
                LDoor(c, "M28.booth", 44, 12, "상담 부스");
                c.Checkpoint(1, 9, 2, 12);
                c.Mark("floor", new Vector2(0, f)); c.Mark("arena0", new Vector2(3f, f)); c.Mark("arena1", new Vector2(42f, f));
                // 번호표 발급기 · 호출 번호판 · 천장 레일 포획팔
                c.Inspect(c.Cell(8, 12), Ctx.S("U_TicketMachine"), "m28_ticket", null, ev: "c7_ticket", order: 1, prompt: "번호표 발급기");
                c.Mark("ticket", c.Cell(9, 12));
                var board = c.Decor(new Vector2(22f, f + 4.8f), Ctx.SF("U_CallBoard").FirstOrDefault(), 4, 1, "CallBoard");
                c.Mark("callboard", Vector2.zero, board);
                var rail = c.Obj("ClawRail", new Vector2(23f, f + 6.0f));
                var rsr = c.Sprite(rail, Ctx.S("U_ClawRail"), 3); rsr.drawMode = SpriteDrawMode.Tiled; rsr.size = new Vector2(36f, 0.19f);
                var claw = c.Decor(new Vector2(24f, f + 5.9f), Ctx.SF("U_Claw").FirstOrDefault(), 6, 1, "Claw");
                c.Mark("claw", Vector2.zero, claw);
                // 빈 운반 인형 (하역대)
                var doll = c.Decor(new Vector2(36.5f, f), Ctx.S("U_Doll"), 2, 1, "TransportDoll");
                c.Mark("doll", Vector2.zero, doll);
                c.Decor(new Vector2(36.5f, f - 0.05f), Ctx.S("U_BeltCrate"), 1, 1, "Dock");
                // 유지보수 단말 (허용된 기능: 호출 번호·표적 위치)
                var mt = c.Obj("MaintTerminal", c.Cell(4, 12));
                var mts = c.Sprite(mt, Ctx.SF("U_MaintTerm").FirstOrDefault(), 1);
                var asw = mt.AddComponent<EW.ArenaSwitch>(); asw.kind = "terminal"; asw.id = "m28_terminal"; asw.art = mts; asw.frames = Ctx.SF("U_MaintTerm"); asw.promptText = "유지보수 단말"; asw.promptOffset = new Vector2(0, 1.6f);
                // 전도성 바닥 (젖은 금속) + 바닥 연결함 + 배전함
                var wet = c.Obj("ConductFloor", new Vector2(18f, f + 0.03f));
                var wsr = c.Sprite(wet, Ctx.S("U_ConductFloor"), 1); wsr.drawMode = SpriteDrawMode.Tiled; wsr.size = new Vector2(6f, 0.19f);
                c.Mark("wet0", new Vector2(15f, f)); c.Mark("wet1", new Vector2(21f, f));
                var fb = c.Obj("FloorBox", new Vector2(18f, f + 1.6f));
                c.Sprite(fb, Ctx.S("S_Junction"), 2);
                var az = fb.AddComponent<EW.ArenaZap>(); az.kind = "floor"; az.id = "m28_floorbox"; az.displayName = "바닥 연결함"; az.size = new Vector2(0.8f, 0.9f); az.aimOffset = new Vector2(0, 0.4f);
                c.PowerSrc(new Vector2(12.2f, f), "M28_power", 2);
                // 제동 장치 (롤러 제동 구간 28~34)
                var br = c.Obj("Brake", c.Cell(31, 12));
                var bsr = c.Sprite(br, Ctx.SF("U_Brake").FirstOrDefault(), 1);
                var bsw = br.AddComponent<EW.ArenaSwitch>(); bsw.kind = "brake"; bsw.id = "m28_brake"; bsw.art = bsr; bsw.frames = Ctx.SF("U_Brake"); bsw.promptText = "제동 장치"; bsw.promptOffset = new Vector2(0, 1.4f);
                c.Mark("brake0", new Vector2(28f, f)); c.Mark("brake1", new Vector2(34f, f));
                var stripe = c.Obj("BrakeStripe", new Vector2(31f, f + 0.03f)); var ssr = c.Sprite(stripe, Ctx.P0("Tile_Pixel"), 1); ssr.color = new Color(0.9f, 0.75f, 0.2f, 0.5f);
                stripe.transform.localScale = new Vector3(6f / 0.125f, 0.08f / 0.125f, 1);
                // 셔터 (제어반을 누르면 5초 내려온다)
                var shp = c.Obj("ShutterPanel", c.Cell(10, 12));
                c.Sprite(shp, Ctx.S("S_ShutterPanel"), 1);
                var ssw = shp.AddComponent<EW.ArenaSwitch>(); ssw.kind = "shutter"; ssw.id = "m28_shutter"; ssw.promptText = "셔터"; ssw.promptOffset = new Vector2(0, 1.4f);
                var sh = c.Obj("HallShutter", new Vector2(13.6f, f));
                var shsr = c.Sprite(sh, Ctx.SF("L_Door").ElementAtOrDefault(1) ?? Ctx.SF("L_Door").FirstOrDefault(), 5); shsr.enabled = false;
                var shc = sh.AddComponent<BoxCollider2D>(); shc.size = new Vector2(0.8f, 3.4f); shc.offset = new Vector2(0, 1.7f); shc.enabled = false;
                c.Mark("hall_shutter", Vector2.zero, sh);
                // 메이린
                Who(c, 30, 12, "meilin", "meilin", -1);
                c.EventZone(5, 9, 6, 12, "c7_hall_enter", null, "c7_meilin_done");
                c.Decor(new Vector2(40f, f), Ctx.P0("Prop_Bench"), -3, 1, "WaitBench");
                c.Decor(new Vector2(26f, f + 3.6f), Ctx.P0("Prop_PosterEmblem"), -6, 1, "Emblem");
                Lamps(c, f + 6.6f, 8f, 36f);
            };
            return r;
        }

        // ================================================================ 29 상담 부스
        static RoomDef M29()
        {
            var g = Box(30, 12);
            g.Set(24, 10, 'k');
            var r = R("M29", "M29_CounselBooth", "상담 부스", Theme.Lab, g.Rows());
            r.layers = c => UnderLayers(c, 0, 0.1f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(3, 10, "start", 1); c.Spawn(26, 10, "from_video", -1);
                LDoor(c, "M29.hall", 2, 10, "접수 홀로");
                LDoor(c, "M29.video", 27, 10, "영상실 (직원 카드)");
                c.EventZone(4, 7, 5, 10, "c7_booth_enter");
                foreach (var x in new[] { 8f, 12f })
                {
                    c.Decor(new Vector2(x, f), Ctx.P0("Prop_Desk"), -3, 1, "BoothDesk");
                    c.Decor(new Vector2(x + 1.1f, f), Ctx.P0("Prop_Bench"), -2, 0.7f, "BoothChair");
                }
                c.Inspect(new Vector2(9.1f, f), null, "m29_chair", null, ev: "c7_chair", prompt: "상담 의자", range: 0.9f);
                c.Decor(new Vector2(16f, f + 1.6f), Ctx.S("U_BoothLock"), -1, 1, "BoothLock");
                High(c, new Vector2(16f, f), null, "m29_lock", null, "부스 잠금장치", "c7_booth_lock", 2.4f);
                c.Doc(18, 10, "D11", "m29_d11");
                c.Decor(new Vector2(21f, f), Ctx.S("S_PartsBin"), -2, 1, "LostFound");
                c.Inspect(new Vector2(21f, f), null, "m29_lostfound", null, ev: "c7_lostfound", prompt: "분실물 상자", range: 0.9f);
                c.Rest(c.Cell(13, 10) + new Vector2(0.5f, 0), Ctx.P0("Prop_Bench"), "c7_booth_rest", "Bench_M29");
            };
            return r;
        }

        // ================================================================ 30 영상실 (시설 카메라) + 격벽
        static RoomDef M30()
        {
            var g = Box(40, 12);
            g.Rect(39, 7, 39, 10, '.');
            var r = R("M30", "M30_VideoRoom", "영상실", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_dark";
            r.layers = c => UnderLayers(c, 1, 0.22f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(3, 10, "start", 1); c.Spawn(37, 10, "from_tower", -1);
                LDoor(c, "M30.booth", 2, 10, "상담 부스로");
                LExit(c, "M30.east", 39, 7, 39, 10);
                foreach (var (x, i) in new[] { (11f, 1), (14f, 2), (17f, 1), (20f, 2) })
                {
                    c.Frames(new Vector2(x, f + 2.0f), Ctx.SF("U_Monitor"), -2, "Monitor", ("c7_feeds_seen", i));
                }
                c.Decor(new Vector2(15.5f, f), Ctx.P0("Prop_Desk"), -3, 1, "ConsoleDesk");
                c.Inspect(new Vector2(15.5f, f), null, "m30_monitors", null, ev: "c7_monitors", prompt: "감시 모니터", range: 1.2f).promptOffset = new Vector2(0, 3.4f);
                // 격벽 (닫히면 루는 관제탑 쪽)
                c.Frames(new Vector2(34f, f), Ctx.SF("U_Bulkhead"), 4, "Bulkhead", ("c7_bulkhead_closed", 1));
                c.Mark("bulkhead", new Vector2(34f, f));
                c.EventZone(30, 7, 31, 10, "c7_bulkhead", "c7_feeds_seen", "c7_bulkhead_closed");
                Who(c, 4, 10, "warden1", "warden", 1, true, "Guard");
                Who(c, 6, 10, "warden2", "warden", 1, true, "Guard");
                c.Alarm(new Vector2(25f, f + 4.4f));
                // ---- 촬영 구역 (화면 밖): 비상 계단 / 화물 승강장 / 관제탑 앞
                float y = -24f;
                FeedSet(c, "feed_stairs", new Vector2(8f, y), 12f, new Color(0.2f, 0.22f, 0.24f));
                c.Decor(new Vector2(4f, y), Ctx.P("Prop_Door").ElementAtOrDefault(2), -30, 1, "FeedStairDoor");
                for (int i = 1; i <= 4; i++) Who(c, 8 + i, FeedRow(c, y), "feed_held" + i, i % 2 == 0 ? "survivorA" : "survivorB", -1, false, "Idle");
                FeedSet(c, "feed_lift", new Vector2(28f, y), 12f, new Color(0.18f, 0.2f, 0.22f));
                c.Decor(new Vector2(30f, y), Ctx.S("U_LiftDeck"), -30, 1, "FeedLiftDeck");
                c.Decor(new Vector2(31f, y + 2.6f), Ctx.S("U_SealShutter"), -31, 1, "FeedSeal");
                Who(c, 25, FeedRow(c, y), "feed_venelle", "venelle", 1, true, "Walk");
                FeedSet(c, "feed_tower", new Vector2(48f, y), 12f, new Color(0.22f, 0.18f, 0.18f));
                c.Decor(new Vector2(48f, y), Ctx.SF("U_Bulkhead").FirstOrDefault(), -30, 1, "FeedBulkhead");
                Who(c, 52, FeedRow(c, y), "feed_warden", "warden", -1, false, "Walk");
            };
            return r;
        }

        // ================================================================ 31 관제탑 (7장 끝)
        static RoomDef M31()
        {
            var g = Box(34, 12);
            g.Rect(33, 7, 33, 10, '.');
            var r = R("M31", "M31_ControlTower", "관제탑", Theme.Lab, g.Rows());
            r.layers = c => UnderLayers(c, 0, 0.18f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(4, 10, "start", 1); c.Spawn(31, 10, "from_corridor", -1);
                LExit(c, "M31.west", 0, 7, 0, 10);
                LExit(c, "M31.east", 33, 7, 33, 10);
                c.Frames(new Vector2(1.6f, f), Ctx.SF("U_Bulkhead"), 4, "Bulkhead", ("c7_bulkhead_closed", 1));
                c.Mark("bulkhead", new Vector2(1.6f, f));
                c.EventZone(6, 7, 7, 10, "c7_tower_enter");
                c.Decor(c.Cell(15, 10), Ctx.SF("L_Console").FirstOrDefault(), -2, 1, "ControlConsole");
                c.Inspect(c.Cell(15, 10), null, "m31_control", null, ev: "c7_control", prompt: "중앙 통제 단말", range: 1.2f).promptOffset = new Vector2(0, 2.2f);
                foreach (var x in new[] { 11f, 19f }) c.Frames(new Vector2(x, f + 2.2f), Ctx.SF("U_Monitor"), -2, "TowerMon", ("c7_control_done", 2));
                c.Rest(c.Cell(25, 10), Ctx.P0("Prop_Bench"), "c7_tower_rest", "Bench_M31");
                c.Checkpoint(26, 7, 27, 10);
            };
            return r;
        }

        // ================================================================ 32 상층 회랑 (8장): 라비엘
        static RoomDef M32()
        {
            var g = Box(84, 12);
            g.Rect(0, 7, 0, 10, '.').Rect(83, 7, 83, 10, '.');
            g.Set(30, 10, 'k');
            var r = R("M32", "M32_UpperCorridor", "상층 회랑", Theme.Lab, g.Rows());
            r.ambience = "amb_lab"; r.music = "bgm_tension";
            r.layers = c => EWSceneBuilder.LabLayers(c, 1);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1); c.Spawn(81, 10, "from_room", -1);
                LExit(c, "M32.west", 0, 7, 0, 10);
                LExit(c, "M32.east", 83, 7, 83, 10);
                c.Checkpoint(1, 7, 2, 10);
                c.Mark("floor", new Vector2(0, f)); c.Mark("meet", c.Cell(14, 10));
                c.Mark("arena0", new Vector2(3f, f)); c.Mark("arena1", new Vector2(40f, f));
                c.Mark("chase0", new Vector2(2f, f)); c.Mark("chase1", new Vector2(81f, f)); c.Mark("goal", new Vector2(78f, f));
                Who(c, 22, 10, "raviel", "raviel", -1, false, "Idle", "c8_duel_done");
                Who(c, 22, 10, "ravielBeast", "ravielBeast", -1, true, "Idle");
                c.EventZone(12, 7, 13, 10, "c8_meet", null, "c8_duel_done");
                // 멈춰 세우기: 옆 문 둘 (돌진을 유도하면 부서진다)
                foreach (var (x, i) in new[] { (9.5f, 0), (35.5f, 1) })
                {
                    var d = c.Decor(new Vector2(x, f), Ctx.P("Prop_Door").ElementAtOrDefault(1), 2, 1, "SideDoor");
                    c.Mark("bdoor" + i, Vector2.zero, d);
                }
                // 도망: 셔터 셋 (패널을 누르면 4초, 라비엘만 막힌다)
                for (int i = 0; i < 3; i++)
                {
                    float sx = 46f + i * 12f;
                    var sh = c.Obj("ChaseShutter" + i, new Vector2(sx, f));
                    var sr = c.Sprite(sh, Ctx.SF("L_Door").ElementAtOrDefault(1) ?? Ctx.SF("L_Door").FirstOrDefault(), 5); sr.enabled = false;
                    var col = sh.AddComponent<BoxCollider2D>(); col.size = new Vector2(0.8f, 3.4f); col.offset = new Vector2(0, 1.7f); col.enabled = false;
                    c.Mark("cshutter" + i, Vector2.zero, sh);
                    var pn = c.Obj("ChasePanel" + i, new Vector2(sx - 2.2f, f));
                    c.Sprite(pn, Ctx.S("S_ShutterPanel"), 1);
                    var sw = pn.AddComponent<EW.ArenaSwitch>(); sw.kind = "cshutter" + i; sw.id = "m32_cshutter" + i; sw.promptText = "셔터"; sw.promptOffset = new Vector2(0, 1.4f); sw.range = 1.2f;
                }
                c.Decor(new Vector2(79.5f, f), Ctx.P("Prop_Door").ElementAtOrDefault(0), -2, 1, "GoalDoor");
                Lamps(c, f + 4.6f, 8f, 24f, 40f, 56f, 72f);
            };
            return r;
        }

        // ================================================================ 33 라비엘의 방
        static RoomDef M33()
        {
            var g = Box(28, 12);
            g.Rect(0, 7, 0, 10, '.').Rect(27, 7, 27, 10, '.');
            var r = R("M33", "M33_RavielRoom", "라비엘의 방", Theme.Home, g.Rows());
            r.ambience = "amb_lab"; r.bg = new Color(0.07f, 0.07f, 0.09f);
            r.layers = c =>
            {
                var wall = c.Layer(Ctx.P0("Tile_HomeWall"), new Vector2(c.room.W / 2f, c.room.H / 2f), -30, 0, new Color(0.5f, 0.52f, 0.6f), 1, "Wall");
                var wsr = wall.GetComponent<SpriteRenderer>(); wsr.drawMode = SpriteDrawMode.Tiled; wsr.size = new Vector2(c.room.W, c.room.H);
            };
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1); c.Spawn(25, 10, "from_comm", -1);
                LExit(c, "M33.west", 0, 7, 0, 10);
                LExit(c, "M33.east", 27, 7, 27, 10);
                c.EventZone(3, 7, 4, 10, "c8_room_enter");
                c.Decor(new Vector2(7.5f, f), Ctx.P0("Prop_Bed"), -2, 1, "Bed");
                c.Decor(new Vector2(13f, f), Ctx.P0("Prop_Desk"), -2, 1, "Desk");
                c.Decor(new Vector2(12.6f, f + 0.95f), Ctx.S("U_ChildPhoto"), 1, 1, "PhotoA");
                c.Decor(new Vector2(13.4f, f + 0.95f), Ctx.S("U_ChildPhoto"), 1, 1, "PhotoB");
                c.Inspect(new Vector2(13f, f), null, "m33_photo", null, ev: "c8_photo", prompt: "액자 두 개", range: 1.0f).promptOffset = new Vector2(0, 2.2f);
                c.Decor(new Vector2(17f, f), Ctx.P0("Prop_Cupboard"), -3, 1, "Shelf");
                c.Frames(new Vector2(17f, f + 1.55f), Ctx.SF("U_Cups"), 1, "Cups", ("side_cup_done", 1));
                High(c, new Vector2(17f, f), null, "m33_cups", null, "컵 두 개", "c8_cups", 2.2f);
                Who(c, 10, 10, "raviel", "raviel", -1, false, "Sit", "c8_room_done");
                c.Rest(c.Cell(21, 10), Ctx.P0("Prop_Bench"), "c8_room_rest", "Bench_M33");
                c.BubbleZone(19, 7, 23, 10, "safe_room");
                c.Glow(new Vector2(13f, f + 3.4f), 3f, new Color(0.9f, 0.9f, 1f, 0.12f), 41, null, true);
            };
            return r;
        }

        // ================================================================ 34 통신실
        static RoomDef M34()
        {
            var g = Box(44, 12);
            g.Rect(0, 7, 0, 10, '.').Rect(43, 7, 43, 10, '.');
            g.Set(20, 10, 'k');
            var r = R("M34", "M34_CommRoom", "통신실", Theme.Lab, g.Rows());
            r.layers = c => UnderLayers(c, 0, 0.14f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1); c.Spawn(41, 10, "from_bridge", -1);
                LExit(c, "M34.west", 0, 7, 0, 10);
                LExit(c, "M34.east", 43, 7, 43, 10);
                c.Checkpoint(1, 7, 2, 10);
                c.EventZone(4, 7, 5, 10, "c8_comm_enter");
                c.Decor(c.Cell(12, 10), Ctx.SF("L_Console").FirstOrDefault(), -2, 1, "CommConsole");
                c.Inspect(c.Cell(12, 10), null, "m34_console", null, ev: "c8_comm_console", prompt: "통신 단말", range: 1.2f).promptOffset = new Vector2(0, 2.2f);
                var pcf = Ctx.P("Prop_Computer");
                c.Decor(c.Cell(16, 10), Ctx.P0("Prop_Desk"), -2); c.Decor(c.Cell(16, 10) + new Vector2(0, 0.9f), pcf.ElementAtOrDefault(3), 1, 1, "BatteryPC");
                c.Decor(c.Cell(17, 10) + new Vector2(0.4f, 0.9f), Ctx.P0("Prop_Battery"), 2, 0.6f, "Battery");
                c.BubbleZone(14, 7, 18, 10, "r_battery");
                c.Doc(26, 10, "D12", "m34_d12");
                c.Decor(new Vector2(29f, f), Ctx.S("U_ServerRack"), -3, 1, "Rack");
                c.Guard(36, 10, "M34_g1", "patrol_guard", 31.5f, 40.5f, -1);
                c.HideCover(33.4f, f, "S_HideRack");
                c.BubbleZone(6, 7, 9, 10, "r_lowdoor");
                Lamps(c, f + 4.6f, 10f, 30f);
            };
            return r;
        }

        // ================================================================ 35 연결 교량
        static RoomDef M35()
        {
            var g = Box(72, 12);
            g.Rect(0, 7, 0, 10, '.').Rect(71, 7, 71, 10, '.');
            var r = R("M35", "M35_SkyBridge", "연결 교량", Theme.Lab, g.Rows());
            r.bg = new Color(0.04f, 0.05f, 0.09f);
            r.layers = c =>
            {
                EWSceneBuilder.LabLayers(c, 1, c.H - 11 + 1.2f, c.H - 3f);
                // 창 너머: 멀리 어두운 시설 동 (밖으로 나갈 수 있는 인상을 주지 않게 어둡게)
                c.Band(Ctx.TP("ansimuz_SynthCities/back.png").FirstOrDefault(), new Vector2(c.room.W / 2f, c.H - 11f - 1f), c.room.W + 30, -50, 0.6f, new Color(0.12f, 0.13f, 0.2f), "FarFacility");
            };
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1); c.Spawn(69, 10, "from_wait", -1);
                LExit(c, "M35.west", 0, 7, 0, 10);
                LExit(c, "M35.east", 71, 7, 71, 10);
                c.EventZone(8, 7, 9, 10, "c8_bridge1"); c.EventZone(24, 7, 25, 10, "c8_bridge2"); c.EventZone(50, 7, 51, 10, "c8_bridge3");
                c.Inspect(c.Cell(36, 10), Ctx.P0("Prop_Bench"), "m35_bench", null, ev: "c8_bridge_bench", order: 1, prompt: "앉기", range: 1.2f);
                c.Guard(60, 10, "M35_d1", "capture_drone", 55f, 66f, -1, null, true, 3.0f);
                c.HideCover(57.6f, f, "S_HideSacks");
                Lamps(c, f + 4.6f, 12f, 36f, 60f);
            };
            return r;
        }

        // ================================================================ 36 대기실 (9장 시작)
        static RoomDef M36()
        {
            var g = Box(46, 12);
            g.Rect(0, 7, 0, 10, '.').Rect(45, 7, 45, 10, '.');
            g.Set(12, 10, 'k');
            var r = R("M36", "M36_WaitingRoom", "대기실", Theme.Lab, g.Rows());
            r.layers = c => UnderLayers(c, 0, 0.08f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1); c.Spawn(43, 10, "from_hall", -1); c.Spawn(31, 10, "from_archive", -1);
                LExit(c, "M36.west", 0, 7, 0, 10);
                LExit(c, "M36.east", 45, 7, 45, 10);
                LDoor(c, "M36.archive", 30, 10, "녹음 보관실");
                c.EventZone(4, 7, 5, 10, "c9_enter");
                c.Decor(new Vector2(9f, f + 1.6f), Ctx.S("U_PlanNotes"), -4, 1, "PlanNotes");
                High(c, new Vector2(9f, f), null, "m36_plans", null, "벽의 종이들", "c9_plans", 2.6f);
                c.Decor(new Vector2(22f, f + 1.8f), Ctx.S("U_CommandChart"), -4, 1, "Chart");
                High(c, new Vector2(22f, f), null, "m36_chart", null, "배치도·지휘 체계도", "c9_chart", 2.8f);
                foreach (var (col, id, art) in new[] { (14, "c9_member1", "memberA"), (17, "c9_member2", "memberB"), (26, "c9_member3", "memberA") })
                {
                    Who(c, col, 10, id, art, -1, false, "Wait");
                    Talk(c, c.Cell(col, 10), "m36_" + id, id, "단원");
                }
                foreach (var x in new[] { 15.5f, 27f }) c.Decor(new Vector2(x, f), Ctx.P0("Prop_Bench"), -3, 1, "Bench");
                Lamps(c, f + 4.6f, 10f, 24f, 38f);
            };
            return r;
        }

        // ================================================================ 37 공명 공연장 · 라크리모사
        static RoomDef M37()
        {
            var g = Box(50, 16);
            g.Rect(0, 11, 0, 14, '.').Rect(49, 11, 49, 14, '.');
            var r = R("M37", "M37_ResonanceHall", "공명 공연장", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_dark"; r.music = "bgm_battle";
            r.layers = c => UnderLayers(c, 1, 0.3f);
            r.extra = c =>
            {
                float f = c.H - 15;
                c.Spawn(2, 14, "start", 1); c.Spawn(47, 14, "from_server", -1);
                LExit(c, "M37.west", 0, 11, 0, 14);
                LExit(c, "M37.east", 49, 11, 49, 14);
                c.Checkpoint(1, 11, 2, 14);
                c.Mark("floor", new Vector2(0, f)); c.Mark("arena0", new Vector2(3f, f)); c.Mark("arena1", new Vector2(46f, f));
                c.Mark("stage_stop", c.Cell(12, 14));
                // 무대 장치
                var mic = c.Decor(new Vector2(29f, f), Ctx.SF("U_Mic").FirstOrDefault(), 3, 1, "Mic"); c.Mark("mic", Vector2.zero, mic);
                var sl = c.Decor(new Vector2(18f, f), Ctx.SF("U_Speaker").ElementAtOrDefault(1), 2, 1, "SpeakerL"); c.Mark("spkL", Vector2.zero, sl);
                var srr = c.Decor(new Vector2(41f, f), Ctx.SF("U_Speaker").ElementAtOrDefault(1), 2, 1, "SpeakerR"); c.Mark("spkR", Vector2.zero, srr);
                var mxo = c.Obj("Mixer", c.Cell(6, 14));
                var mxs = c.Sprite(mxo, Ctx.SF("U_Mixer").FirstOrDefault(), 2);
                var msw = mxo.AddComponent<EW.ArenaSwitch>(); msw.kind = "mixer"; msw.id = "m37_mixer"; msw.art = mxs; msw.frames = Ctx.SF("U_Mixer"); msw.promptText = "믹서"; msw.promptOffset = new Vector2(0, 1.3f);
                c.Mark("mixer_art", Vector2.zero, mxo);
                c.Mark("gauge", c.Cell(6, 14) + new Vector2(0, 2.0f));
                for (int i = 0; i < 3; i++) c.Decor(new Vector2(12f + i * 13f, f + 12.2f), Ctx.S("U_Truss"), -2, 1, "Truss");
                Who(c, 31, 14, "lacrimosa", "lacrimosa", -1, false, "Idle", "c9_lac_done");
                c.EventZone(8, 11, 9, 14, "c9_stage_enter", null, "c9_lac_done");
                // 회상 무대 (화면 밖): 어린 에드워드와 TV / 음향실의 에드워드
                float y = -24f;
                FeedSet(c, "mem_edward", new Vector2(14f, y), 16f, new Color(0.16f, 0.13f, 0.12f));
                Who(c, 13, FeedRow(c, y), "edwardKid", "edwardKid", 1, true, "Listen");
                Who(c, 15, FeedRow(c, y), "edward", "edward", 1, true, "Write");
                c.Decor(new Vector2(18f, y), Ctx.P0("Prop_Desk"), -30, 1, "MemDesk");
            };
            return r;
        }

        // ================================================================ 38 녹음 보관실 (선택)
        static RoomDef M38()
        {
            var g = Box(26, 12);
            var r = R("M38", "M38_RecordArchive", "녹음 보관실", Theme.Lab, g.Rows());
            r.layers = c => UnderLayers(c, 0, 0.2f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(3, 10, "start", 1);
                LDoor(c, "M38.wait", 2, 10, "대기실로");
                c.EventZone(4, 7, 5, 10, "c9_archive_enter");
                foreach (var x in new[] { 8f, 12f, 20f }) c.Decor(new Vector2(x, f), Ctx.P0("Prop_Cupboard"), -3, 1, "TapeShelf");
                c.Decor(new Vector2(16f, f), Ctx.P0("Prop_Desk"), -2, 1, "Desk");
                c.Decor(new Vector2(16f, f + 0.95f), Ctx.S("U_Tape"), 1, 1, "Tape");
                c.Inspect(new Vector2(16f, f), null, "m38_tape", null, ev: "c9_tape", prompt: "녹음 테이프", range: 1.0f).promptOffset = new Vector2(0, 2f);
                c.Rest(c.Cell(22, 10), Ctx.P0("Prop_Bench"), "c9_archive_rest", "Bench_M38");
            };
            return r;
        }

        // ================================================================ 39 서버실: 강제 가동·사고 영상·포획 단말
        static RoomDef M39()
        {
            var g = Box(52, 12);
            g.Rect(0, 7, 0, 10, '.').Rect(51, 7, 51, 10, '.');
            var r = R("M39", "M39_ServerRoom", "서버실", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_dark";
            r.layers = c => UnderLayers(c, 1, 0.26f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1); c.Spawn(44, 10, "from_terminal", -1); c.Spawn(49, 10, "from_boarding", -1);
                LExit(c, "M39.west", 0, 7, 0, 10);
                LExit(c, "M39.east", 51, 7, 51, 10);
                c.Checkpoint(1, 7, 2, 10);
                c.EventZone(6, 7, 7, 10, "c9_force", null, "shelter_collapsed");
                foreach (var x in new[] { 11f, 14f, 24f, 27f, 30f, 36f }) c.Anim(new Vector2(x, f), Ctx.SF("U_ServerRack"), 2, -3, "Rack");
                c.Decor(c.Cell(18, 10), Ctx.SF("L_Console").FirstOrDefault(), -2, 1, "CommRelay");
                c.Decor(new Vector2(44f, f), Ctx.SF("U_CaptureTerm").FirstOrDefault(), -1, 1, "CaptureTerminal");
                c.EventZone(41, 7, 42, 10, "c9_capture", "shelter_collapsed", "c9_bin_done");
                // ---- 촬영 구역: 지하 대피실 / 공명 공연장
                float y = -24f;
                FeedSet(c, "feed_shelter", new Vector2(10f, y), 14f, new Color(0.2f, 0.17f, 0.14f));
                c.Decor(new Vector2(6f, y), Ctx.S("U_Tarp"), -31, 1, "FeedTarp");
                c.Anim(new Vector2(9f, y), Ctx.SF("U_Stove"), 4, -30, "FeedStove");
                Who(c, 11, FeedRow(c, y), "feed_f", "f", -1, false, "Guard");
                Who(c, 10, FeedRow(c, y), "feed_kid", "kid008", 1, false, "Hold");
                Who(c, 14, FeedRow(c, y), "feed_sv1", "survivorA", -1, false, "Scared");
                Who(c, 15, FeedRow(c, y), "feed_sv2", "survivorB", -1, false, "Scared");
                Who(c, 7, FeedRow(c, y), "feed_venelle_b2", "venelle", 1, true, "SitQuiet");
                var rub = c.Decor(new Vector2(11f, y), Ctx.S("U_Rubble"), -20, 1.6f, "FeedRubble"); rub.GetComponent<SpriteRenderer>().enabled = false; c.Mark("feed_rubble0", Vector2.zero, rub);
                var tag = c.Decor(new Vector2(13.5f, y + 0.05f), Ctx.S("U_BrokenTag"), -19, 1, "FeedTag"); tag.GetComponent<SpriteRenderer>().enabled = false; c.Mark("feed_after0", Vector2.zero, tag);
                var hand = c.Decor(new Vector2(9.2f, y + 0.05f), Ctx.S("U_StillHand"), -19, 1, "FeedHand"); hand.GetComponent<SpriteRenderer>().enabled = false; c.Mark("feed_after1", Vector2.zero, hand);
                FeedSet(c, "feed_hall", new Vector2(34f, y), 14f, new Color(0.14f, 0.14f, 0.2f));
                Who(c, 35, FeedRow(c, y), "feed_lac", "lacrimosa", -1, false, "Sing");
                Who(c, 31, FeedRow(c, y), "feed_mb1", "memberA", 1, false, "Walk");
                Who(c, 32, FeedRow(c, y), "feed_mb2", "memberB", 1, false, "Walk");
                var truss = c.Decor(new Vector2(35f, y + 4.4f), Ctx.S("U_Truss"), -20, 1, "FeedTruss"); c.Mark("feed_truss", Vector2.zero, truss);
            };
            return r;
        }

        // ================================================================ 40 포획 단말 (단말 안)
        static RoomDef M40()
        {
            var g = new Grid(20, 12);
            g.Rect(0, 0, 0, 10, '|').Rect(19, 0, 19, 10, '|').Rect(0, 0, 19, 0, '|').Rect(0, 11, 19, 11, '|');
            var r = RNoPad("M40", "M40_CaptureTerminal", "포획 단말", Theme.Desktop, g.Rows());
            r.hidePlatforms = true; r.zeroG = true; r.sealSkills = true; r.noCompanion = true;
            r.bg = new Color(0.12f, 0.06f, 0.08f);
            r.layers = c =>
            {
                var px = Ctx.P0("Tile_Pixel"); float s = px != null ? px.bounds.size.x : 0.125f;
                var bg = c.Layer(px, new Vector2(10f, 6f), -60, 1, new Color(0.18f, 0.08f, 0.1f), 1, "CaptureBg"); bg.transform.localScale = new Vector3(22 / s, 14 / s, 1);
                c.Particles("Dots", Ctx.P0("FX_LightDot"), 10, new Vector2(0f, -0.3f), new Vector2(0.15f, 0.1f), new Color(1f, 0.6f, 0.5f, 0.45f), new Color(1f, 0.4f, 0.4f, 0.15f), 6f, 30, true);
            };
            r.extra = c =>
            {
                c.Spawn(5, 8, "start", 1);
                c.EventZone(1, 1, 18, 10, "c9_terminal_in");
                var it = c.Inspect(new Vector2(14f, 5f), Ctx.S("U_IconDoc"), "m40_d16", null, ev: "c9_d16", order: -5, prompt: "전이 장치 정비도", range: 1.2f);
                it.promptOffset = new Vector2(0, 1.0f);
                it.transform.localScale = new Vector3(2f, 2f, 1);
            };
            return r;
        }

        // ================================================================ 41 휴지통 목록 (복구 대기 보관함)
        static RoomDef M41()
        {
            var g = new Grid(20, 12);
            g.Rect(0, 0, 0, 10, '|').Rect(19, 0, 19, 10, '|').Rect(0, 0, 19, 0, '|').Rect(0, 11, 19, 11, '|');
            var r = RNoPad("M41", "M41_RecycleList", "휴지통 목록", Theme.Desktop, g.Rows());
            r.hidePlatforms = true; r.zeroG = true; r.sealSkills = true; r.noCompanion = true;
            r.bg = new Color(0.08f, 0.1f, 0.14f);
            r.layers = c =>
            {
                var px = Ctx.P0("Tile_Pixel"); float s = px != null ? px.bounds.size.x : 0.125f;
                var bg = c.Layer(px, new Vector2(10f, 6f), -60, 1, new Color(0.1f, 0.12f, 0.17f), 1, "BinBg"); bg.transform.localScale = new Vector3(22 / s, 14 / s, 1);
                for (int i = 0; i < 6; i++) c.Decor(new Vector2(4f + i * 2.4f, 9.5f - (i % 2) * 0.8f), Ctx.S("U_IconDoc"), -50, 1.5f, "FloatingItem");
            };
            r.extra = c =>
            {
                c.Spawn(4, 7, "start", 1);
                c.EventZone(1, 1, 18, 10, "c9_bin_enter");
                var list = c.Inspect(new Vector2(10f, 4f), Ctx.S("U_IconBin"), "m41_list", null, ev: "c9_bin_list", order: -5, prompt: "항목 목록 열기", range: 1.4f);
                list.transform.localScale = new Vector3(2.5f, 2.5f, 1); list.promptOffset = new Vector2(0, 1.3f);
                var st = c.Inspect(new Vector2(15.5f, 7f), Ctx.S("U_IconDoc"), "m41_status", null, ev: "c9_bin_status", order: -5, prompt: "정리 예약 상태", range: 1.2f);
                st.transform.localScale = new Vector3(1.6f, 1.6f, 1);
            };
            return r;
        }

        // ================================================================ 42 승선 대기실 (10장 시작)
        static RoomDef M42()
        {
            var g = Box(48, 12);
            g.Rect(0, 7, 0, 10, '.').Rect(47, 7, 47, 10, '.');
            var r = R("M42", "M42_BoardingLounge", "승선 대기실", Theme.Lab, g.Rows());
            r.layers = c => UnderLayers(c, 0, 0.12f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1); c.Spawn(45, 10, "from_capsule", -1);
                LExit(c, "M42.west", 0, 7, 0, 10);
                LExit(c, "M42.east", 47, 7, 47, 10);
                c.EventZone(4, 7, 5, 10, "c10_enter");
                c.Decor(new Vector2(16f, f + 2.4f), Ctx.S("U_BoardStatus"), -3, 1.2f, "StatusBoard");
                High(c, new Vector2(16f, f), null, "m42_board", null, "탑승 현황판", "c10_board", 3.2f);
                foreach (var (col, id, art, anim) in new[] { (10, "c10_member1", "memberA", "Cheer"), (22, "c10_member2", "memberB", "Wait"), (26, "c10_m3", "memberA", "Wait"), (30, "c10_m4", "memberB", "Idle") })
                    Who(c, col, 10, id, art, -1, false, anim);
                Talk(c, c.Cell(10, 10), "m42_m1", "c10_member1", "단원");
                Talk(c, c.Cell(22, 10), "m42_m2", "c10_member2", "단원");
                foreach (var x in new[] { 23f, 31f }) c.Decor(new Vector2(x, f), Ctx.P0("Prop_Bench"), -3, 1, "Bench");
                Lamps(c, f + 4.6f, 8f, 24f, 40f);
            };
            return r;
        }

        // ================================================================ 43 캡슐 회랑
        static RoomDef M43()
        {
            var g = Box(56, 12);
            g.Rect(0, 7, 0, 10, '.').Rect(55, 7, 55, 10, '.');
            var r = R("M43", "M43_CapsuleCorridor", "캡슐 회랑", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_dark";
            r.layers = c => UnderLayers(c, 1, 0.2f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1); c.Spawn(53, 10, "from_device", -1);
                LExit(c, "M43.west", 0, 7, 0, 10);
                LExit(c, "M43.east", 55, 7, 55, 10);
                for (int i = 0; i < 6; i++)
                {
                    float x = 8f + i * 6f;
                    if (i == 2) c.Frames(new Vector2(x, f), Ctx.SF("U_Capsule"), -2, "CapsuleFail", ("c10_capsule_failed", 1));
                    else c.Decor(new Vector2(x, f), Ctx.SF("U_Capsule").FirstOrDefault(), -2, 1, "Capsule");
                }
                Who(c, 21, 10, "cap_member", "memberB", -1, false, "Idle");
                c.EventZone(18, 7, 19, 10, "c10_capsule", null, "c10_capsule_failed");
                Who(c, 40, 10, "meilin", "meilin", -1, false, "Pull");
                c.Decor(new Vector2(41f, f + 0.9f), Ctx.S("U_Register"), 3, 1, "Register");
                c.EventZone(34, 7, 35, 10, "c10_meilin", null, "c10_meilin_seen");
                c.Rest(c.Cell(49, 10), Ctx.P0("Prop_Bench"), "c10_rest", "Bench_M43");
                c.BubbleZone(47, 7, 51, 10, "safe_room");
            };
            return r;
        }

        // ================================================================ 44 전이 장치실 · 아우렐리우스
        static RoomDef M44()
        {
            var g = Box(48, 16);
            g.Rect(47, 11, 47, 14, '.');
            var r = R("M44", "M44_TransferChamber", "전이 장치실", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_alarm"; r.music = "bgm_battle"; r.noCompanion = true;
            r.layers = c => UnderLayers(c, 1, 0.28f);
            r.extra = c =>
            {
                float f = c.H - 15;
                c.Spawn(3, 14, "start", 1); c.Spawn(45, 14, "from_corridor", -1);
                LExit(c, "M44.east", 47, 11, 47, 14);
                c.Mark("floor", new Vector2(0, f)); c.Mark("arena0", new Vector2(3f, f)); c.Mark("arena1", new Vector2(45f, f));
                c.Mark("confront", c.Cell(12, 14));
                var cp = c.Decor(new Vector2(38f, f + 6f), Ctx.SF("U_Coupling").FirstOrDefault(), -1, 1.4f, "Coupling");
                c.Mark("coupling", Vector2.zero, cp);
                foreach (var y in new[] { 2f, 4f, 9f }) c.Decor(new Vector2(38f, f + y), Ctx.S("U_CoolPipe"), -3, 1, "Pipe");
                foreach (var (x, i) in new[] { (14f, 0), (44f, 1) })
                {
                    var cap = c.Obj("Capacitor" + i, new Vector2(x, f));
                    c.Sprite(cap, Ctx.SF("U_Capacitor").FirstOrDefault(), 1);
                    var az = cap.AddComponent<EW.ArenaZap>(); az.kind = "capacitor"; az.id = "m44_cap" + i; az.displayName = "축전 모듈"; az.size = new Vector2(0.9f, 1.6f); az.aimOffset = new Vector2(0, 0.8f);
                    c.Mark("cap" + i, Vector2.zero, cap);
                }
                var mg = c.Obj("Magnet", c.Cell(24, 14));
                c.Sprite(mg, Ctx.S("U_Magnet"), 1);
                var msw = mg.AddComponent<EW.ArenaSwitch>(); msw.kind = "magnet"; msw.id = "m44_magnet"; msw.promptText = "정비용 전자석"; msw.promptOffset = new Vector2(0, 1.2f);
                c.Mark("magnet", Vector2.zero, mg);
                var vl = c.Obj("CoolValve", c.Cell(20, 14));
                c.Sprite(vl, Ctx.SF("U_Valve").FirstOrDefault(), 1);
                var vsw = vl.AddComponent<EW.ArenaSwitch>(); vsw.kind = "valve"; vsw.id = "m44_valve"; vsw.promptText = "냉각 밸브"; vsw.promptOffset = new Vector2(0, 1.4f);
                c.PowerSrc(new Vector2(18f, f), "M44_power", 3);
                Who(c, 32, 14, "aurelius", "aurelius", -1, false, "Idle", "c10_boss_done");
                Who(c, 2, 14, "meilin", "meilin", 1, false, "Pull", "c10_boss_done");
                c.Decor(new Vector2(1.8f, f), Ctx.SF("U_Capsule").FirstOrDefault(), -3, 1, "LastCapsule");
                Who(c, 9, 14, "raviel_inj", "raviel", 1, true, "Injured");
                var mr = c.Decor(new Vector2(2.5f, f), Ctx.S("U_Rubble"), 6, 1, "MeiRubble"); mr.GetComponent<SpriteRenderer>().enabled = false; c.Mark("mei_rubble0", Vector2.zero, mr);
                var rg = c.Decor(new Vector2(4.4f, f + 0.05f), Ctx.S("U_Register"), 7, 1, "MeiRegister"); rg.GetComponent<SpriteRenderer>().enabled = false; c.Mark("mei_after0", Vector2.zero, rg);
                var ro = c.Decor(new Vector2(3.6f, f + 0.05f), Ctx.S("U_Roller"), 7, 1, "MeiRoller"); ro.GetComponent<SpriteRenderer>().enabled = false; c.Mark("mei_after1", Vector2.zero, ro);
                c.EventZone(5, 11, 6, 14, "c10_final_enter", null, "c10_boss_done");
                c.Alarm(new Vector2(24f, f + 10f));
            };
            return r;
        }

        // ================================================================ 45 붕괴 연결로
        static RoomDef M45()
        {
            var g = Box(70, 12);
            var r = R("M45", "M45_CollapseWay", "붕괴 연결로", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_alarm";
            r.layers = c => UnderLayers(c, 1, 0.34f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1);
                c.Checkpoint(1, 7, 2, 10); c.Checkpoint(34, 7, 35, 10);
                c.Mark("floor", new Vector2(0, f)); c.Mark("fall", new Vector2(64f, f));
                for (int i = 0; i < 6; i++) c.Decor(new Vector2(8f + i * 10f, f), Ctx.S("U_Rubble"), -2, 0.8f, "Rubble");
                c.Sparks(new Vector2(20f, f + 4f), null); c.Sparks(new Vector2(46f, f + 3.5f), null);
                Who(c, 2, 10, "fall_beast", "ravielBeast", 1, true, "Hold");
                c.EventZone(64, 7, 65, 10, "c10_fall", null, "c10_fall_done");
                c.Alarm(new Vector2(30f, f + 4.6f));
            };
            return r;
        }

        // ================================================================ 46 무너진 중정 (11장): 시설 안의 무너진 열린 공간
        static RoomDef M46()
        {
            var g = Box(40, 12);
            g.Rect(10, 0, 30, 0, '.');            // 무너진 천장 (위로 어두운 하늘이 조금 보이지만 둘레는 시설 벽)
            g.Rect(39, 7, 39, 10, '.');
            var r = R("M46", "M46_CollapsedCourt", "무너진 중정", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_dark"; r.noCompanion = true;
            r.bg = new Color(0.04f, 0.04f, 0.06f);
            r.layers = c =>
            {
                UnderLayers(c, 0, 0.45f);
                c.Particles("Ash", Ctx.S("FX_Dust"), 18, new Vector2(0.05f, -0.25f), new Vector2(0.1f, 0.05f), new Color(0.8f, 0.8f, 0.85f, 0.25f), new Color(0.6f, 0.6f, 0.7f, 0.1f), 8f, 30, false);
            };
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(14, 10, "start", 1);
                LExit(c, "M46.east", 39, 7, 39, 10);
                foreach (var x in new[] { 4f, 22f, 30f, 35f }) c.Decor(new Vector2(x, f), Ctx.S("U_Rubble"), -2, 1f, "Rubble");
                Who(c, 15, 10, "raviel_end", "raviel", -1, false, "Down");
                c.Glow(new Vector2(20f, f + 7f), 5f, new Color(0.6f, 0.65f, 0.8f, 0.12f), 41, null, true);
            };
            return r;
        }

        // ================================================================ 47 잔해 더미: 마지막
        static RoomDef M47()
        {
            var g = Box(36, 12);
            var r = R("M47", "M47_RubblePile", "잔해 더미", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_dark"; r.noCompanion = true;
            r.bg = new Color(0.03f, 0.03f, 0.05f);
            r.layers = c => UnderLayers(c, 1, 0.5f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1);
                foreach (var x in new[] { 10f, 18f, 26f, 32f }) c.Decor(new Vector2(x, f), Ctx.S("U_Rubble"), -2, 1.2f, "Rubble");
                Who(c, 33, 10, "recovery", "droid02", -1, false, "Idle");
                c.EventZone(14, 7, 15, 10, "c11_last");
            };
            return r;
        }
    }
}
