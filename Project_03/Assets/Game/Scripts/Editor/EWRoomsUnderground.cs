using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EW.EditorTools
{
    /// <summary>
    /// 4~6장 맵 (2026-11-15): 관찰 휴게실(M15) 오른쪽 출구 → 화물 승강장 이후. GDD 5장 표의 맵 ID 를 그대로 쓴다.
    /// 16 화물 승강장 / 17 폐기소 거처 / 18 물 펌프실 / 19 폐품 시장★ / 20 유지보수 터널 / 21 정비 작업장 /
    /// 22 컨베이어 홀 / 23 수직 배선통로 / 24 공동 세탁실(숙소 복도, 7장 수용동 연결문) / 25 베넬의 방 / 26 기숙사 옥상★.
    /// 그림은 기존 개별 도트(연구소 타일·소품·패럴랙스 레이어)와 Tools/ch4_6_art_gen.py 의 개별 임시 소품으로 조립한다 — 통짜 배경 없음.
    /// </summary>
    public static partial class EWRooms
    {
        static IEnumerable<RoomDef> UndergroundRooms()
        {
            yield return M16(); yield return M17(); yield return M18(); yield return M19();
            yield return M20(); yield return M21(); yield return M22(); yield return M23();
            yield return M24(); yield return M25(); yield return M26();
        }

        static GameObject PropU(Ctx c, Vector2 pos, string name, int order = -3, float scale = 1f) => c.Decor(pos, Ctx.S(name), order, scale, name);

        static Grid Box(int w, int h)
        {
            var g = new Grid(w, h);
            g.Rect(0, 0, w - 1, 0, '#').Rect(0, 0, 0, h - 1, '#').Rect(w - 1, 0, w - 1, h - 1, '#').Rect(0, h - 1, w - 1, h - 1, '#');
            return g;
        }

        /// <summary>지하 공용 깊이: 연구소 레이어 + 어둡게 깔린 낮은 조명. 장식은 진행 대상보다 어둡게.</summary>
        static void UnderLayers(Ctx c, int variant, float dim = 0.28f)
        {
            EWSceneBuilder.LabLayers(c, variant);
            var px = Ctx.P0("Tile_Pixel"); float s = px != null ? px.bounds.size.x : 0.125f;
            var shade = c.Layer(px, new Vector2(c.room.W / 2f, c.room.H / 2f), -35, 0, new Color(0.02f, 0.02f, 0.04f, dim), 1, "UnderShade");
            shade.transform.localScale = new Vector3((c.room.W + 4) / s, (c.room.H + 4) / s, 1);
        }

        /// <summary>높이 걸린 물건: 조사 지점은 발밑(상호작용 높이 제한 안), 안내 표시는 물건 높이에.</summary>
        static Inspect High(Ctx c, Vector2 floorPos, Sprite s, string id, string dialogue, string prompt, string ev, float promptY)
        {
            var it = c.Inspect(floorPos, s, id, dialogue, ev: ev, prompt: prompt, range: 1.0f);
            it.promptOffset = new Vector2(0, promptY);
            return it;
        }

        static Inspect Talk(Ctx c, Vector2 pos, string id, string ev, string prompt, string requireFlag = null, float range = 1.0f)
        {
            var it = c.Inspect(pos, null, id, null, ev: ev, prompt: prompt, range: range);
            it.requireFlag = requireFlag;
            it.promptOffset = new Vector2(0, 2.0f);
            return it;
        }

        static FlagTrigger Trigger(Ctx c, string name, string ev, string allFlags = null, string doneFlag = null, string noneFlag = null, string deviceOff = null, Rect? zone = null)
        {
            var go = c.Obj("Trigger_" + name, Vector2.zero);
            var t = go.AddComponent<FlagTrigger>();
            t.eventId = ev; t.allFlags = allFlags; t.doneFlag = doneFlag; t.noneFlag = noneFlag; t.deviceOffId = deviceOff;
            if (zone.HasValue) { t.useZone = true; t.zone = zone.Value; }
            return t;
        }

        // ================================================================ 16 화물 승강장: 위층은 봉쇄, 아래로만 가는 화물 승강기 (두 층)
        static RoomDef M16()
        {
            var g = Box(56, 16);
            g.Rect(0, 7, 30, 15, '#');           // 위 층 바닥
            g.Rect(31, 3, 31, 6, '|');           // 승강로 가장자리 (떨어지지 않게)
            g.Rect(0, 3, 0, 6, '.');             // 왼쪽 출구 (관찰 휴게실)
            g.Rect(55, 11, 55, 14, '.');         // 오른쪽 출구 (폐기소)
            g.Set(15, 6, 'k').Set(45, 14, 'k');
            var r = R("M16", "M16_CargoLift", "화물 승강장", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_dark";
            r.layers = c =>
            {
                UnderLayers(c, 1, 0.22f);
                c.Layer(Ctx.S("L_Crane"), new Vector2(46f, c.H - 16f), -30, 0.15f, new Color(0.55f, 0.52f, 0.5f), 1f, "Crane");
            };
            r.extra = c =>
            {
                float up = c.H - 7, low = c.H - 15;
                c.Spawn(2, 6, "start", 1); c.Spawn(28, 6, "lift_top", 1); c.Spawn(35, 14, "lift_bottom", 1); c.Spawn(53, 14, "from_shelter", -1);
                c.Exit(0, 3, 0, 6, "M15", "from_lift");
                c.Exit(55, 11, 55, 14, "M17", "start");
                // 승강로 틀 + 위쪽 봉쇄 셔터 (붉은 경고등)
                var shaft = PropU(c, new Vector2(34.5f, low), "U_LiftShaft", -20); shaft.transform.localScale = new Vector3(1f, 2.6f, 1);
                PropU(c, new Vector2(34.5f, up + 2.4f), "U_SealShutter", -6);
                c.Glow(new Vector2(34.5f, up + 5.1f), 1.6f, new Color(1f, 0.25f, 0.2f, 0.35f), -5);
                High(c, new Vector2(33.4f, up), null, "m16_seal", "c4_seal_look", "올려다보기", null, 2.6f);
                // 승강기 발판: 위층(기본) ↔ 아래층(m16_lift_down)
                var deck = PropU(c, new Vector2(34.5f, up - 0.3f), "U_LiftDeck", 3);
                var fv = c.FlagSwitch("LiftDeck", "m16_lift_down");
                fv.target = deck.GetComponent<SpriteRenderer>(); fv.setOffset = new Vector3(0, low - up, 0);
                c.Mark("lift_deck", Vector2.zero, deck);
                // 조작반 (위·아래 둘 다 같은 사건)
                var fr = Ctx.SF("U_LiftPanel");
                var pu = c.Inspect(c.Cell(29, 6), fr.ElementAtOrDefault(0), "m16_panel_top", null, ev: "m16_lift", prompt: "승강기 조작반");
                var pl = c.Inspect(c.Cell(38, 14), fr.ElementAtOrDefault(1), "m16_panel_bottom", null, ev: "m16_lift", prompt: "승강기 조작반");
                c.Inspect(c.Cell(5, 6), Ctx.P0("Prop_PosterEmblem"), "m16_sign", "c4_platform_sign", order: -6, prompt: "안내판");
                // 아래층: 화물 상자 사이를 도는 드론 (선택 경로: 숨거나, 간섭으로 잠깐 멈추고 지나간다)
                c.Guard(48, 14, "M16_drone", "capture_drone", 40.5f, 53.5f, -1, null, true, 3.0f);
                c.HideCover(43.2f, low, "S_HideRack");
                c.Crate(50.75f, low);
                c.BgBox(41f, low); c.BgBox(52f, low);
                foreach (var x in new[] { 8f, 20f }) c.BgBox(x, up);
                c.Checkpoint(1, 3, 2, 6);
                c.Checkpoint(33, 11, 36, 14);
                c.BubbleZone(18, 2, 24, 6, "walk");
                c.Rest(c.Cell(24, 6), Ctx.P0("Prop_Bench"), "m16_bench", "Bench_M16");
                c.WorkLamp(new Vector2(12f, up + 4.2f), "c3_boss_done", 2.8f);
                c.WorkLamp(new Vector2(46f, low + 4.6f), "c3_boss_done", 3.2f);
            };
            r.useAlarmAmbience = false;
            return r;
        }

        // ================================================================ 17 폐기소 거처: 버려진 설비로 버티는 생존자들
        static RoomDef M17()
        {
            var g = Box(50, 12);
            g.Rect(0, 7, 0, 10, '.').Rect(49, 7, 49, 10, '.');
            var r = R("M17", "M17_WasteShelter", "폐기소 거처", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_dark";
            r.layers = c =>
            {
                UnderLayers(c, 0, 0.3f);
                // 뒤쪽 벽에 쌓인 폐기 설비 (어둡게)
                foreach (var (x, n) in new[] { (4f, "S_Lockers"), (16f, "S_Jar1"), (31f, "S_PartsBin"), (43f, "S_Lockers") })
                    c.Layer(Ctx.S(n), new Vector2(x, c.H - 11f), -34, 0.05f, new Color(0.3f, 0.3f, 0.32f), 1f, "Junk");
            };
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1); c.Spawn(33, 10, "from_pump", -1); c.Spawn(39, 10, "from_market", -1); c.Spawn(47, 10, "from_tunnel", -1);
                c.Spawn(8, 10, "meet", 1);
                c.Exit(0, 7, 0, 10, "M16", "from_shelter");
                c.Exit(49, 7, 49, 10, "M20", "start", requireFlag: "waste_resolved", blockedDialogue: "waste_exit_wait");
                c.Checkpoint(1, 7, 2, 10);
                // 입구 바리케이드 (고철) — 처음엔 생존자들이 그 뒤에서 파이프를 든다
                PropU(c, new Vector2(10.2f, f), "U_Barricade", -2);
                // 생활 공간: 방수포 지붕·난로·상자 침대·빨랫줄·빈 물통·벽의 날짜 금·대피실 해치
                PropU(c, new Vector2(23f, f + 3.4f), "U_Tarp", -4, 1f);
                c.Anim(new Vector2(24.2f, f), Ctx.SF("U_Stove"), 4, -1, "Stove");
                c.Glow(new Vector2(24.2f, f + 0.8f), 1.8f, new Color(1f, 0.6f, 0.25f, 0.22f), -2, null, true);
                PropU(c, new Vector2(28.5f, f), "U_CrateBed", -2); PropU(c, new Vector2(30.4f, f), "U_CrateBed", -2);
                PropU(c, new Vector2(15.5f, f + 2.6f), "U_LaundryLine", -4);
                var jugs = Ctx.SF("U_Jug");
                for (int i = 0; i < 3; i++) c.Frames(new Vector2(19.3f + i * 0.55f, f), jugs, -1, "Jug", ("waste_water_on", 1));
                c.Inspect(new Vector2(20f, f), null, "m17_jugs", null, ev: "waste_jugs", prompt: "물통", range: 0.8f);
                PropU(c, new Vector2(31.8f, f + 1.4f), "U_Tally", -5);
                c.Inspect(new Vector2(31.8f, f + 0.2f), null, "m17_tally", "c4_tally", prompt: "벽의 금", range: 0.8f);
                var hatch = c.Inspect(c.Cell(17, 10), Ctx.P("Prop_Door").ElementAtOrDefault(0), "m17_hatch", null, ev: "waste_hatch", order: -4, prompt: "대피실");
                hatch.transform.localScale = new Vector3(0.8f, 0.8f, 1);
                // F 와 008
                c.Actor(22, 10, "f", "f", -1, null, null, null, "Idle");
                c.Actor(26, 10, "kid008", "kid008", -1, null, null, null, "Idle");
                Talk(c, c.Cell(22, 10), "m17_f", "waste_f", "F", "waste_met");
                Talk(c, c.Cell(26, 10), "m17_008", "waste_008", "008", "waste_met", 0.9f);
                // 겁먹은 생존자 셋 (교섭이 깨지면 실시간 비살상 충돌)
                var junk = Ctx.S("U_Part"); var shadow = Ctx.S("FX_GlowSoft");
                void Sv(string id, float x, string art)
                {
                    var go = c.Obj("Survivor_" + id, new Vector2(x, f));
                    var sv = go.AddComponent<Scavenger>(); sv.id = id; sv.artId = art; sv.minX = 3f; sv.maxX = 31f; sv.junkSprite = junk; sv.shadowSprite = shadow;
                }
                Sv("a", 11.6f, "survivorA"); Sv("b", 13.4f, "survivorB"); Sv("c", 16.2f, "survivorA");
                Talk(c, new Vector2(13.4f, f), "m17_survivors", "waste_survivors", "생존자들", "waste_resolved", 1.2f);
                Trigger(c, "WasteFightEnd", "waste_fight_end", "waste_fight,waste_calm_a,waste_calm_b,waste_calm_c", "waste_fight_done");
                c.EventZone(5, 7, 6, 10, "waste_meet");
                // 문: 물 펌프실 / 폐품 시장 (만난 뒤)
                var pd = c.Door(33, 10, "M18", "start", "물 펌프실"); pd.requireFlag = "waste_met"; pd.lockedDialogue = "waste_door_wait";
                var md = c.Door(39, 10, "M19", "start", "폐품 시장"); md.requireFlag = "waste_met"; md.lockedDialogue = "waste_door_wait";
                // 안쪽 통로 바리케이드: 해결되면 치운다
                var inner = PropU(c, new Vector2(46.5f, f), "U_Barricade", -2);
                c.FlagSwitch("InnerBarricade", "waste_resolved", null, new[] { inner });
                c.BubbleZone(18, 2, 30, 10, "rest");
            };
            return r;
        }

        // ================================================================ 18 물 펌프실: 고인 물 → 차단기 → 이음쇠 → 가동
        static RoomDef M18()
        {
            var g = Box(30, 12);
            g.Rect(1, 10, 11, 10, '#').Rect(22, 10, 28, 10, '#');      // 양옆 바닥이 한 칸 높고 가운데가 물 고인 구덩이
            var r = R("M18", "M18_PumpRoom", "물 펌프실", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_dark";
            r.layers = c => UnderLayers(c, 1, 0.25f);
            r.extra = c =>
            {
                float side = c.H - 10, pit = c.H - 11;
                c.Spawn(3, 9, "start", 1);
                c.Door(2, 9, "M17", "from_pump", "거처로");
                var rig = c.Obj("PumpRig", Vector2.zero).AddComponent<PumpRig>();
                rig.basinX0 = 12f; rig.basinX1 = 22f; rig.basinBottom = pit; rig.basinFull = pit + 1.75f;
                var px = Ctx.P0("Tile_Pixel");
                var body = c.Obj("WaterBody", new Vector2(17f, pit + 0.8f));
                var bsr = c.Sprite(body, Ctx.S("U_WaterFill"), 6); bsr.drawMode = SpriteDrawMode.Tiled; bsr.size = new Vector2(10f, 1.6f);
                rig.waterBody = bsr;
                var top = c.Obj("WaterTop", new Vector2(17f, pit + 1.75f));
                var tsr = c.Sprite(top, Ctx.SF("U_WaterTop").FirstOrDefault(), 7); tsr.drawMode = SpriteDrawMode.Tiled; tsr.size = new Vector2(10f, 0.19f);
                rig.waterTop = tsr; rig.waterTopFrames = Ctx.SF("U_WaterTop");
                var pumpFr = Ctx.SF("U_Pump");
                var pump = c.Obj("Pump", new Vector2(16.5f, pit)); rig.pump = c.Sprite(pump, pumpFr.FirstOrDefault(), 4); rig.pumpFrames = pumpFr;
                rig.couplingPos = new Vector2(16.9f, pit + 0.5f);
                var brFr = Ctx.SF("U_Breaker");
                var br = c.Obj("Breaker", new Vector2(25f, side + 0.4f)); rig.breaker = c.Sprite(br, brFr.FirstOrDefault(), 2); rig.breakerFrames = brFr;
                var vFr = Ctx.SF("U_Valve");
                var va = c.Obj("Valve", new Vector2(9f, side)); rig.valve = c.Sprite(va, vFr.FirstOrDefault(), 2); rig.valveFrames = vFr;
                // 상태판: 수위 / 전원 / 이음쇠 램프 (그림 아이콘 + 색)
                var board = c.Obj("StatusBoard", new Vector2(5.5f, side + 1.3f));
                var bs = c.Sprite(board, px, -2); bs.color = new Color(0.16f, 0.18f, 0.2f); board.transform.localScale = new Vector3(2.2f / 0.125f, 1.3f / 0.125f, 1);
                SpriteRenderer Lamp(float x, string icon)
                {
                    c.Decor(new Vector2(x, side + 1.45f), Ctx.S(icon), -1, 0.7f, "BoardIcon");
                    var l = c.Obj("Lamp", new Vector2(x, side + 0.95f)); var sr = c.Sprite(l, px, -1); l.transform.localScale = new Vector3(3f, 2f, 1); return sr;
                }
                rig.lampWater = Lamp(4.8f, "U_WaterTop"); rig.lampPower = Lamp(5.5f, "S_IconPower"); rig.lampPart = Lamp(6.2f, "U_Part");
                // 거처로 가는 물관 (가동하면 물빛이 흐른다)
                var pipe = c.Obj("PipeToShelter", new Vector2(11.5f, pit + 2.7f)); var psr = c.Sprite(pipe, Ctx.S("U_PipeH"), -3); psr.drawMode = SpriteDrawMode.Tiled; psr.size = new Vector2(11f, 0.5f);
                var flow = c.Obj("PipeFlow", new Vector2(11.5f, pit + 2.7f)); var fsr = c.Sprite(flow, px, -2); flow.transform.localScale = new Vector3(11f / 0.125f, 0.12f / 0.125f, 1);
                rig.pipeFlow = fsr;
                PropU(c, new Vector2(27.4f, side), "U_PartShelf", -2);
                void Ctl(string name, Vector2 at, PumpRig.Role role, float range = 0.9f)
                {
                    var go = c.Obj("Pump_" + name, at);
                    var pc = go.AddComponent<PumpControl>(); pc.id = "m18_" + name; pc.role = role; pc.range = range; pc.rangeY = 1.2f; pc.promptOffset = new Vector2(0, 1.6f);
                }
                Ctl("valve", new Vector2(9f, side), PumpRig.Role.Valve);
                Ctl("breaker", new Vector2(25f, side), PumpRig.Role.Breaker);
                Ctl("coupling", new Vector2(17.6f, pit), PumpRig.Role.Coupling, 1.1f);
                Ctl("board", new Vector2(5.5f, side), PumpRig.Role.Board);
                Ctl("shelf", new Vector2(27.4f, side), PumpRig.Role.Shelf);
                c.EventZone(4, 6, 5, 9, "m18_enter");
            };
            return r;
        }

        // ================================================================ 19 폐품 시장 (선택): 짧은 조사·교환
        static RoomDef M19()
        {
            var g = Box(34, 12);
            var r = R("M19", "M19_JunkMarket", "폐품 시장", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_dark";
            r.layers = c => UnderLayers(c, 0, 0.3f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(3, 10, "start", 1);
                c.Door(2, 10, "M17", "from_market", "거처로");
                PropU(c, new Vector2(9.5f, f), "U_StallA", -3);
                c.Frames(new Vector2(7.6f, f + 1.6f), Ctx.SF("U_Lantern"), -1, "Lantern", ("market_lantern_lit", 1));
                c.Actor(11, 10, "trader", "survivorB", -1, null, null, null, "Idle");
                Talk(c, c.Cell(11, 10), "m19_trader", "market_lantern", "등불 장수");
                PropU(c, new Vector2(19.5f, f), "U_StallB", -3);
                c.Actor(21, 10, "scrapper", "survivorA", -1, null, null, null, "Idle");
                Talk(c, c.Cell(21, 10), "m19_scrap", "market_scrap", "고물 장수");
                c.Decor(new Vector2(24.5f, f + 1.2f), Ctx.P0("Prop_PosterEmblem"), -5, 1, "NoteBoard");
                High(c, new Vector2(24.5f, f), null, "m19_board", "market_board", "쪽지 판", null, 2.6f);
                // 008 의 여덟 개의 선 (선택 이야기)
                c.Actor(29, 10, "kid008_m", "kid008", -1, "waste_met", null, null, "Draw");
                c.Frames(new Vector2(28.2f, f), Ctx.SF("U_ChalkLines"), -1, "Chalk", ("side_lines_done", 1));
                Talk(c, c.Cell(29, 10), "m19_lines", "market_lines", "008", "waste_met", 1.0f);
                c.EventZone(4, 7, 5, 10, "m19_enter");
            };
            return r;
        }

        // ================================================================ 20 유지보수 터널 (5장 시작): 낮은 천장, 순찰, CCTV
        static RoomDef M20()
        {
            var g = Box(60, 12);
            g.Rect(0, 7, 0, 10, '.').Rect(59, 7, 59, 10, '.');
            g.Rect(22, 1, 32, 9, '#');          // 낮은 천장 (웅크려 지나간다)
            g.Set(27, 10, 'k');
            var r = R("M20", "M20_MaintTunnel", "유지보수 터널", Theme.Lab, g.Rows());
            r.ambience = "amb_lab_dark";
            r.layers = c => UnderLayers(c, 1, 0.3f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1); c.Spawn(57, 10, "from_workshop", -1);
                c.Exit(0, 7, 0, 10, "M17", "from_tunnel");
                c.Exit(59, 7, 59, 10, "M21", "start");
                c.Checkpoint(1, 7, 2, 10); c.Checkpoint(34, 7, 35, 10);
                c.Puddle(13, 10, 3, "M20_puddle");
                c.EventZone(18, 7, 19, 10, "m20_crawl_hint");
                c.BubbleZone(19, 7, 22, 10, "low_ceiling");
                c.Guard(45, 10, "M20_guard", "patrol_guard", 37.5f, 52.5f, -1);
                c.Camera(new Vector2(41.5f, f + 4.4f), "M20_cam", -160f, -60f, 0.4f);
                c.HideCover(38.6f, f, "S_HideSacks"); c.HideCover(53.4f, f, "S_HideRack");
                c.Crate(47.75f, f, "S_CrateS");
                c.ExposedWire(43, 10, "M20_wire", 1);
                c.Tray(new Vector2(36.5f, f + 4.8f), "M20_tray");
                c.Reinforce(new Vector2(58.5f, f + 3f));
                foreach (var x in new[] { 8f, 40f, 50f }) c.WorkLamp(new Vector2(x, f + 4.6f), "c3_boss_done", 2.4f);
            };
            r.useAlarmAmbience = false;
            return r;
        }

        // ================================================================ 21 정비 작업장: 새 기술(펄스 버스트) 시험
        static RoomDef M21()
        {
            var g = Box(34, 12);
            g.Rect(0, 7, 0, 10, '.').Rect(33, 7, 33, 10, '.');
            var r = R("M21", "M21_Workshop", "정비 작업장", Theme.Lab, g.Rows());
            r.layers = c => UnderLayers(c, 0, 0.12f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1); c.Spawn(31, 10, "from_hall", -1);
                c.Exit(0, 7, 0, 10, "M20", "from_workshop");
                c.Exit(33, 7, 33, 10, "M22", "start");
                c.Checkpoint(1, 7, 2, 10);
                c.Decor(c.Cell(5, 10), Ctx.SF("L_Console").FirstOrDefault(), -3, 1, "Console");
                c.Decor(new Vector2(7.2f, f + 1.1f), Ctx.S("S_ToolTray"), -3, 1, "Tools");
                c.Decor(new Vector2(25.5f, f), Ctx.S("S_PartsBin"), -3, 1, "Parts");
                c.Inspect(c.Cell(9, 10), Ctx.P0("Prop_Doc"), "m21_manual", null, ev: "ws_manual", order: 2, prompt: "출력 시험 안내");
                PropU(c, new Vector2(17f, f), "U_TestRig", -2);
                var dummy = Ctx.S("S_BodyBoard") ?? Ctx.S("S_Crate");
                foreach (var x in new[] { 14.6f, 19.4f })
                {
                    var go = c.Obj("TestDummy", new Vector2(x, f));
                    var sr = c.Sprite(go, dummy, 1);
                    var pt = go.AddComponent<PracticeTarget>(); pt.kind = PracticeTarget.Kind.Dummy; pt.art = sr; pt.normal = dummy;
                    pt.id = "m21_dummy_" + x; pt.displayName = "시험 인형"; pt.size = new Vector2(0.8f, 1.2f); pt.aimOffset = new Vector2(0, 0.6f);
                }
                // 같은 선(ws_test)의 일반 작업등과 배터리 작업등: 펄스가 선을 끊으면 하나만 꺼진다
                var cl = Ctx.P("Prop_CeilingLight");
                var lampA = c.Device(new Vector2(16f, f + 2.2f), "ws_lamp", DeviceKind.Light, new[] { cl.ElementAtOrDefault(0) }, new[] { cl.ElementAtOrDefault(1) }, line: "ws_test", order: 2);
                c.DeviceInspect(lampA, null, 1.6f, "작업등");
                var lampB = c.Device(new Vector2(19f, f + 2.2f), "ws_lamp_battery", DeviceKind.Light, new[] { cl.ElementAtOrDefault(0) }, new[] { cl.ElementAtOrDefault(1) }, line: "ws_test", battery: true, level: 90, order: 2);
                c.DeviceInspect(lampB, null, 1.6f, "배터리 작업등");
                c.Glow(new Vector2(16f, f + 1.9f), 1.8f, new Color(1f, 0.95f, 0.7f, 0.12f), -1, lampA.transform, true);
                c.Glow(new Vector2(19f, f + 1.9f), 1.8f, new Color(1f, 0.95f, 0.7f, 0.12f), -1, lampB.transform, true);
                c.Restore(23, 10, "ws_test");
                Trigger(c, "WsTested", "ws_after", "pulse_learned,used_burst", "ws_tested", null, null, new Rect(11f, f - 1f, 13f, 5f));
                c.Rest(c.Cell(28, 10), Ctx.P0("Prop_Bench"), "ch5_workshop", "Bench_M21");
                c.BubbleZone(26, 2, 29, 10, "bench");
            };
            return r;
        }

        // ================================================================ 22 컨베이어 홀: 벨트 방향·노출 전선·과부하 (배터리 PC 는 켜진 채)
        static RoomDef M22()
        {
            var g = Box(64, 12);
            g.Rect(0, 7, 0, 10, '.').Rect(63, 7, 63, 10, '.');
            g.Rect(14, 6, 38, 6, '=');                   // 벨트 위 점검 통로
            g.Rect(13, 6, 13, 10, 'H').Set(13, 6, '^');
            g.Rect(56, 5, 56, 10, 'H').Set(56, 5, '^');  // 문 위로 넘어가는 환기 통로 (우회로)
            g.Rect(57, 5, 60, 5, '=');                   // 문(59) 위를 지나 61 에서 문 너머 바닥으로 내려선다
            g.Set(30, 5, 'k');
            var r = R("M22", "M22_ConveyorHall", "컨베이어 홀", Theme.Lab, g.Rows());
            r.layers = c =>
            {
                UnderLayers(c, 1, 0.15f);
                c.Layer(Ctx.S("L_Crane"), new Vector2(27f, c.H - 12f), -30, 0.15f, new Color(0.6f, 0.58f, 0.55f), 1f, "Crane");
            };
            r.extra = c =>
            {
                float f = c.H - 11, cat = c.H - 6;
                c.Spawn(2, 10, "start", 1); c.Spawn(62, 10, "from_shaft", -1);
                c.Exit(0, 7, 0, 10, "M21", "from_hall");
                c.Exit(63, 7, 63, 10, "M23", "start");
                c.Checkpoint(1, 7, 2, 10);
                // 벨트 모터 (홀 전원선)
                var motor = c.Device(new Vector2(41.6f, f), "hall_belt", DeviceKind.Machine, new[] { Ctx.S("S_PowerBox") }, new[] { Ctx.S("S_PowerBoxDead") }, line: "hall_grid", essential: true, order: 1);
                c.DeviceInspect(motor, "hall_motor", 1.0f, "벨트 모터");
                var beltGo = c.Obj("Belt", new Vector2(26f, f + 0.16f));
                var bf = Ctx.SF("U_Belt");
                var bsr = c.Sprite(beltGo, bf.FirstOrDefault(), 1); bsr.drawMode = SpriteDrawMode.Tiled; bsr.size = new Vector2(28f, 0.31f);
                var conv = c.Obj("Conveyor", Vector2.zero).AddComponent<Conveyor>();
                conv.id = "hall"; conv.motor = motor; conv.x0 = 12f; conv.x1 = 40f; conv.topY = f; conv.speed = 2.2f; conv.defaultDir = -1; conv.belt = bsr; conv.beltFrames = bf;
                var crates = new List<Transform>();
                foreach (var x in new[] { 15f, 22f, 29f, 36f }) crates.Add(c.Decor(new Vector2(x, f + 0.3f), Ctx.S("U_BeltCrate"), 2, 1, "BeltCrate").transform);
                conv.crates = crates.ToArray();
                var ctlFr = Ctx.SF("U_BeltCtrl");
                var ctl = c.Obj("BeltControl", c.Cell(10, 10));
                var csr = c.Sprite(ctl, ctlFr.FirstOrDefault(), 2);
                var bc = ctl.AddComponent<BeltControl>(); bc.id = "m22_beltctl"; bc.conveyorId = "hall"; bc.art = csr; bc.frames = ctlFr; bc.promptText = "벨트 제어반"; bc.promptOffset = new Vector2(0, 1.4f);
                // 점검 통로의 벨트 제어함 + 옆의 끊어진 굵은 전선 (외부 전력 방전으로 과부하)
                var box = c.Obj("GridBox", new Vector2(26.5f, cat + 0.1f));
                var bxsr = c.Sprite(box, Ctx.S("S_Junction"), 2);
                var gb = box.AddComponent<GridBox>(); gb.id = "hall_gridbox"; gb.displayName = "벨트 제어함"; gb.line = "hall_grid"; gb.art = bxsr; gb.size = new Vector2(0.8f, 0.9f); gb.aimOffset = new Vector2(0, 0.45f);
                c.ExposedWire(22, 5, "M22_wire", 2);
                // PC 둘: 홀 전원선 PC / 눈에 보이는 배터리를 단 PC (정전에도 켜짐)
                var pcf = Ctx.P("Prop_Computer");
                var pc1 = c.Device(c.Cell(46, 10) + new Vector2(0, 0.9f), "hall_pc", DeviceKind.Computer, new[] { pcf[0] }, new[] { pcf[1] }, null, new[] { pcf[2] }, line: "hall_grid", order: 1);
                c.DeviceInspect(pc1, "hall_pc1");
                var pc2 = c.Device(c.Cell(49, 10) + new Vector2(0, 0.9f), "hall_pc_battery", DeviceKind.Computer, new[] { pcf[3] }, new[] { pcf[1] }, null, new[] { pcf[2] }, line: "hall_grid", battery: true, level: 85, order: 1);
                c.DeviceInspect(pc2, "hall_pc2");
                c.Decor(c.Cell(47, 10), Ctx.P0("Prop_Desk"), -2); c.Decor(c.Cell(49, 10), Ctx.P0("Prop_Desk"), -2);
                c.Decor(c.Cell(50, 10) + new Vector2(0.4f, 0.9f), Ctx.P0("Prop_Battery"), 2, 0.6f, "VisibleBattery");
                // 출구 문 (홀 전원선): 정전되면 닫힌다 → 전원 복구 버튼 또는 위의 환기 통로로 우회
                var df = Ctx.P("Prop_Door");
                var door = c.Device(c.Cell(59, 10), "hall_door", DeviceKind.Door, new[] { df[1] }, new[] { df[2] }, line: "hall_grid", essential: true, order: -1);
                var blk = c.Obj("HallDoorBlock", c.Cell(59, 10) + new Vector2(0, 1.6f), door.transform);
                var bcol = blk.AddComponent<BoxCollider2D>(); bcol.size = new Vector2(0.8f, 3.2f);
                door.blocker = bcol;
                c.DeviceInspect(door, "hall_door_note", 1.2f, "출구 문");
                c.Restore(55, 10, "hall_grid", "hall_restored");
                Trigger(c, "HallOff", "hall_off_line", null, "hall_off_said", null, "hall_door");
                // 감시: 점검 통로 순찰, 바닥 순찰, 벨트를 내려다보는 CCTV
                c.Guard(24, 5, "M22_cat", "patrol_guard", 16.5f, 35.5f, 1);
                c.Guard(50, 10, "M22_floor", "patrol_guard", 44.5f, 54.5f, -1);
                c.Camera(new Vector2(33.5f, f + 4.6f), "M22_cam", -150f, -80f, 0.2f);
                c.HideCover(7.4f, f, "S_HideRack"); c.HideCover(43.6f, f, "S_HideSacks");
                c.BubbleZone(4, 7, 8, 10, "walk");
                c.EventZone(4, 7, 5, 10, "m22_enter");
            };
            return r;
        }

        // ================================================================ 23 수직 배선통로: 추격 (세 층, 사다리·넓은 발판·전원선)
        static RoomDef M23()
        {
            var g = Box(30, 26);
            g.Rect(1, 17, 28, 17, '#');                         // 2층 바닥
            g.Rect(26, 17, 26, 24, 'H').Set(26, 17, '^');      // 1→2층 사다리
            g.Rect(1, 9, 28, 9, '#');                           // 3층 바닥
            g.Rect(18, 9, 18, 16, 'H').Set(18, 9, '^');        // 2→3층 사다리
            g.Rect(21, 5, 28, 5, '#');                          // 꼭대기 출구 턱
            g.Rect(0, 21, 0, 24, '.');                          // 1층 왼쪽 입구 (컨베이어 홀)
            g.Rect(29, 1, 29, 4, '.');                          // 꼭대기 오른쪽 출구 (숙소)
            g.Set(10, 24, 'k');
            var r = R("M23", "M23_VerticalShaft", "수직 배선통로", Theme.Lab, g.Rows());
            r.layers = c => UnderLayers(c, 0, 0.2f);
            r.useAlarmAmbience = false;
            r.extra = c =>
            {
                float l1 = c.H - 25, l2 = c.H - 17, l3 = c.H - 9, ledge = c.H - 5;
                c.Spawn(2, 24, "start", 1); c.Spawn(27, 4, "from_dorm", -1);
                c.Exit(0, 21, 0, 24, "M22", "from_shaft");
                c.Exit(29, 1, 29, 4, "M24", "start");
                c.Checkpoint(1, 21, 2, 24);
                c.EventZone(6, 21, 7, 24, "m23_chase");
                c.EventZone(21, 13, 23, 16, "m23_l2");   // 사다리에서 내려 걸음을 뗀 뒤
                c.EventZone(15, 5, 19, 8, "m23_l3");
                c.EventZone(22, 1, 28, 4, "m23_top");
                // 1층: 뒤쪽 문에서 경비가 쫓아온다
                c.Decor(c.Cell(1, 24) + new Vector2(0.4f, 0), Ctx.P("Prop_Door").ElementAtOrDefault(2), -3, 1, "BackDoorL1");
                c.Guard(2, 24, "M23_g1", "patrol_guard", 1.5f, 8f, 1, "m23_chase");
                // 2층: 왼쪽 문에서 경비가 나온다 (2층 도착 후)
                c.Decor(c.Cell(1, 16) + new Vector2(0.4f, 0), Ctx.P("Prop_Door").ElementAtOrDefault(2), -3, 1, "DoorL2");
                c.Guard(2, 16, "M23_g2", "patrol_guard", 1.5f, 6f, 1, "m23_l2_guard");
                // 베넬 동행: 산성 약품이 고일 금속 바닥판 + 루가 전류를 보내는 연결함
                var plate = c.Obj("MetalPlate", new Vector2(9.5f, l2 + 0.03f));
                var plsr = c.Sprite(plate, Ctx.P0("Tile_Pixel"), 0); plsr.color = new Color(0.55f, 0.6f, 0.66f); plate.transform.localScale = new Vector3(7f / 0.125f, 0.1f / 0.125f, 1);
                var acid = c.Obj("AcidPool", new Vector2(10.5f, l2 + 0.02f));
                var asr = c.Sprite(acid, Ctx.P0("Prop_Puddle"), 1); asr.color = new Color(0.55f, 1f, 0.45f); acid.transform.localScale = new Vector3(2.2f, 1, 1);
                c.FlagSwitch("AcidPool", "m23_acid", new[] { acid }, null);
                var jb = c.Inspect(c.Cell(14, 16), Ctx.S("S_Junction"), "m23_plate", null, ev: "m23_plate_zap", order: 1, prompt: "바닥판 연결함");
                jb.requireFlag = "m23_acid"; jb.hideFlag = "m23_plate_done";
                // 베넬 비동행: 컨베이어(경비를 문 쪽으로 되돌림) + 셔터 (패널로 내림)
                var bf = Ctx.SF("U_Belt");
                var beltGo = c.Obj("Belt", new Vector2(8.5f, l2 + 0.16f));
                var bsr = c.Sprite(beltGo, bf.FirstOrDefault(), 1); bsr.drawMode = SpriteDrawMode.Tiled; bsr.size = new Vector2(8f, 0.31f);
                var conv = c.Obj("Conveyor", Vector2.zero).AddComponent<Conveyor>();
                conv.id = "shaft"; conv.x0 = 4.5f; conv.x1 = 12.5f; conv.topY = l2; conv.speed = 2.2f; conv.defaultDir = -1; conv.allyMode = 2; conv.belt = bsr; conv.beltFrames = bf;
                var shPos = new Vector2(13.6f, l2);
                var sh = c.Frames(shPos, Ctx.SF("L_Door"), -1, "Shutter", ("m23_shutter", 0)); sh.defaultIndex = 1;
                var sblk = c.Obj("ShutterBlock", shPos + new Vector2(0, 2f));
                var scol = sblk.AddComponent<BoxCollider2D>(); scol.size = new Vector2(0.9f, 4f);
                var fv = c.FlagSwitch("ShutterM23", "m23_shutter"); fv.blockWhileSet = new Collider2D[] { scol };
                c.Inspect(c.Cell(16, 16), Ctx.S("S_ShutterPanel"), "m23_shutter_panel", null, ev: "m23_shutter", order: 1, prompt: "셔터 패널");
                // 3층: 드론 + 전원선 (꼭대기 턱으로)
                c.Guard(9, 8, "M23_d1", "capture_drone", 4f, 14f, 1, "m23_l2_guard", true, 2.6f);
                c.PowerLinePair("M23_line", 23, 8, 25, 4, new[] { c.Cell(23, 6) + new Vector2(0, 0.4f), c.Cell(25, 6) });
                c.HideCover(6.2f, l3, "S_HideRack");
                foreach (var y in new[] { l1, l2, l3 }) c.WorkLamp(new Vector2(14f, y + 4.2f), "c3_boss_done", 2.4f);
                c.Decor(new Vector2(15f, ledge + 1.2f), Ctx.S("L_Cables"), -6, 1, "Cables");
            };
            return r;
        }

        // ================================================================ 24 공동 세탁실 (6장 시작) + 숙소 복도 + 수용동 연결문
        static RoomDef M24()
        {
            var g = Box(52, 12);
            g.Rect(0, 7, 0, 10, '.');
            var r = R("M24", "M24_Laundry", "공동 세탁실", Theme.Lab, g.Rows());
            r.ambience = "amb_lab";
            r.layers = c => UnderLayers(c, 0, 0.05f);
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(2, 10, "start", 1); c.Spawn(34, 10, "from_roof", 1); c.Spawn(40, 10, "from_room", -1);
                c.Exit(0, 7, 0, 10, "M23", "from_dorm");
                c.Checkpoint(1, 7, 2, 10);
                foreach (var x in new[] { 5.5f, 8f, 10.5f, 13f }) c.Anim(new Vector2(x, f), Ctx.SF("U_Washer"), 2, -2, "Washer");
                c.Decor(new Vector2(19.5f, f + 1.9f), Ctx.S("U_TwinOutfits"), -2, 1, "TwinOutfits");
                c.Decor(new Vector2(25.5f, f + 1.9f), Ctx.S("U_OtherOutfits"), -2, 1, "OtherOutfits");
                High(c, new Vector2(19.5f, f), null, "m24_twins", null, "같은 옷 두 벌", "laundry_twins", 3.2f);
                High(c, new Vector2(25.5f, f), null, "m24_others", null, "다른 빨래", "laundry_others", 3.2f);
                c.Decor(new Vector2(16.5f, f), Ctx.P0("Prop_Cupboard"), -3, 1, "Shelf");
                c.Door(33, 10, "M26", "start", "옥상 계단");
                c.Door(39, 10, "M25", "start", "숙소 3호");
                c.Rest(c.Cell(44, 10), Ctx.P0("Prop_Bench"), "ch6_dorm", "Bench_M24");
                var gate = c.Inspect(c.Cell(49, 10), Ctx.SF("L_Door").FirstOrDefault(), "m24_gate", null, ev: "ch7_gate", order: -2, prompt: "수용동 연결문", range: 1.2f);
                c.Glow(c.Cell(49, 10) + new Vector2(0, 3f), 1.2f, new Color(1f, 0.3f, 0.25f, 0.3f), -1);
                c.EventZone(4, 7, 5, 10, "m24_enter");
                c.BubbleZone(42, 7, 46, 10, "bench");
            };
            return r;
        }

        // ================================================================ 25 베넬의 방: 사진(린든)·편지(아이비)·포장지 열 장
        static RoomDef M25()
        {
            var g = Box(24, 12);
            var r = R("M25", "M25_VenelleRoom", "숙소 3호", Theme.Home, g.Rows());
            r.ambience = "amb_lab"; r.bg = new Color(0.08f, 0.06f, 0.06f);
            r.layers = c =>
            {
                // 숙소 방: 집 벽 타일(기존 개별 도트)을 낮은 밝기로 깔고 아래쪽 걸레받이 띠
                var wall = c.Layer(Ctx.P0("Tile_HomeWall"), new Vector2(c.room.W / 2f, c.room.H / 2f), -30, 0, new Color(0.62f, 0.58f, 0.6f), 1, "Wall");
                var wsr = wall.GetComponent<SpriteRenderer>(); wsr.drawMode = SpriteDrawMode.Tiled; wsr.size = new Vector2(c.room.W, c.room.H);
                var px = Ctx.P0("Tile_Pixel"); float ps = px != null ? px.bounds.size.x : 0.125f;
                float f0 = c.H - 11;
                var base0 = c.Layer(px, new Vector2(c.room.W / 2f, f0 + 0.35f), -29, 0, new Color(0.25f, 0.18f, 0.16f), 1, "Baseboard");
                base0.transform.localScale = new Vector3(c.room.W / ps, 0.7f / ps, 1);
                c.Particles("Dust", Ctx.S("FX_Dust"), 8, new Vector2(0.03f, 0.05f), new Vector2(0.05f, 0.03f), new Color(0.9f, 0.85f, 0.75f, 0.18f), new Color(0.8f, 0.75f, 0.7f, 0.08f), 9f, 30, false);
            };
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(3, 10, "start", 1);
                c.Door(2, 10, "M24", "from_room", "복도로");
                c.Decor(new Vector2(7.5f, f), Ctx.P0("Prop_Bed"), -2, 1, "Bed");
                c.Decor(new Vector2(10.6f, f), Ctx.P0("Prop_Mirror"), -3, 1, "Mirror");
                c.Decor(new Vector2(14f, f), Ctx.P0("Prop_Desk"), -2, 1, "Desk");
                c.Decor(new Vector2(15.1f, f + 0.95f), Ctx.P0("Prop_Cups"), -1, 1, "Cups");
                c.Inspect(new Vector2(14.4f, f + 0.95f), Ctx.S("U_Photo"), "m25_photo", null, ev: "room_photo", order: 1, prompt: "사진 액자", range: 0.8f);
                c.Inspect(new Vector2(13.2f, f + 0.95f), Ctx.S("U_Letter"), "m25_letter", null, ev: "room_letter", order: 1, prompt: "편지", range: 0.8f);
                c.Decor(new Vector2(18f, f), Ctx.P0("Prop_Cupboard"), -3, 1, "Drawer");
                c.Decor(new Vector2(17.6f, f + 1.55f), Ctx.S("U_Wrappers"), 1, 1, "Wrappers");
                c.Decor(new Vector2(18.7f, f + 1.55f), Ctx.S("U_ChocoBox"), 1, 1, "ChocoBox");
                High(c, new Vector2(18f, f), null, "m25_wrappers", null, "서랍 위 포장지", "room_wrappers", 2.1f);
                c.Decor(new Vector2(20.8f, f + 1.6f), Ctx.P0("Prop_Clothes"), -3, 1, "HangingCoat");
                c.Glow(new Vector2(12f, f + 3.4f), 3.2f, new Color(1f, 0.85f, 0.6f, 0.14f), 41, null, true);
                c.Mark("mem_center", new Vector2(12f, f));
                c.EventZone(4, 7, 5, 10, "m25_enter");
            };
            return r;
        }

        // ================================================================ 26 기숙사 옥상 (선택): 바람
        static RoomDef M26()
        {
            var g = Box(30, 12);
            g.Rect(1, 0, 28, 0, '.');                  // 천장 없음 (하늘)
            var r = R("M26", "M26_DormRoof", "기숙사 옥상", Theme.Exterior, g.Rows());
            r.bg = new Color(0.05f, 0.06f, 0.12f);
            r.ambience = "amb_digital";
            r.layers = c =>
            {
                var px = Ctx.P0("Tile_Pixel"); float s = px != null ? px.bounds.size.x : 0.125f;
                var sky = c.Layer(px, new Vector2(c.room.W / 2f, c.room.H / 2f), -60, 1f, new Color(0.08f, 0.09f, 0.18f), 1f, "NightSky");
                sky.transform.localScale = new Vector3((c.room.W + 40) / s, (c.room.H + 20) / s, 1);
                // 멀리 흐린 건물 실루엣만 (어둡게, 간판 색이 드러나지 않게) — 시설 밖으로 나갈 수 있다는 인상을 주지 않는다
                c.Band(Ctx.TP("ansimuz_SynthCities/back.png").FirstOrDefault(), new Vector2(c.room.W / 2f, c.H - 11f - 3.2f), c.room.W + 30, -50, 0.75f, new Color(0.13f, 0.14f, 0.24f), "FarSilhouette");
                var haze = c.Layer(Ctx.S("FX_HazeBand"), new Vector2(c.room.W / 2f, c.H - 11f + 0.6f), -49, 0.7f, new Color(0.25f, 0.28f, 0.45f, 0.35f), 1f, "NightHaze");
                haze.transform.localScale = new Vector3((c.room.W + 40) / 0.125f, 1.2f, 1);
                c.Particles("Wind", Ctx.S("FX_Dust"), 14, new Vector2(1.2f, 0.05f), new Vector2(0.4f, 0.05f), new Color(0.8f, 0.85f, 1f, 0.25f), new Color(0.7f, 0.75f, 0.9f, 0.1f), 5f, 30, false);
            };
            r.extra = c =>
            {
                float f = c.H - 11;
                c.Spawn(3, 10, "start", 1);
                c.Door(2, 10, "M24", "from_roof", "계단으로");
                for (float x = 4; x < 28; x += 1f) c.Decor(new Vector2(x + 0.5f, f), Ctx.S("U_Rail"), 3, 1, "Rail");
                PropU(c, new Vector2(23f, f), "U_WaterTower", -3);
                c.Inspect(c.Cell(14, 10), Ctx.P0("Prop_Bench"), "m26_bench", null, ev: "roof_sit", order: 1, prompt: "앉기", range: 1.2f);
            };
            return r;
        }
    }
}
