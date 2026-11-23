using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EW.EditorTools
{
    /// <summary>
    /// 시연판 맵 정의 (사이드뷰). 오프닝 4장면 + 첫 탈출 구역부터 베넬 동행까지.
    /// 범례: '#' 벽/바닥, '|' 보이지 않는 벽, '=' 위에서만 밟히는 발판, 'H' 사다리, '^' 사다리 꼭대기(덮개), 'k' 과자, 'b' 회복 붕대, '1'~'9' 연구동 장식.
    /// 모든 방은 맨 아래에 바닥 4줄을 덧대어(R) 바닥이 화면 아래 약 40% 높이에 오게 한다 — 화면 아래 대화창이 발을 가리지 않도록.
    /// 맵 통합 기록 (옛 ID → 새 ID): M00~M03 → P01~P03 (새 오프닝), M04 → C01, M05+M06 → C02, M07+M08 → C03, M10 → C04, M12 → C05.
    /// 저장 호환은 SaveSystem.Migrate (v1 → v2).
    /// </summary>
    public static partial class EWRooms
    {
        public const int FloorPad = 4;

        static RoomDef R(string id, string scene, string title, Theme th, params string[] rows)
        {
            int w = rows[0].Length;
            for (int i = 0; i < rows.Length; i++)
                if (rows[i].Length != w) throw new Exception(id + " 행 " + i + " 길이 " + rows[i].Length + " ≠ " + w);
            var all = rows.ToList();
            for (int i = 0; i < FloorPad; i++) all.Add(new string('#', w));
            var r = new RoomDef { id = id, scene = scene, title = title, theme = th, rows = all.ToArray() };
            r.legend['k'] = (c, x, y) => c.Pickup(x, y, "snack", id + "_snack_" + x + "_" + y);
            r.legend['b'] = (c, x, y) => c.Pickup(x, y, "bandage", id + "_bandage_" + x + "_" + y);
            for (int i = 1; i <= 9; i++)
            {
                int k = i - 1;
                r.legend[(char)('0' + i)] = (c, x, y) => c.Decor(c.Cell(x, y), Ctx.Named("Foozle_SciFiLab_Tileset/full_decor_tiles.png", "decor_" + k), -4, 1, "Decor_" + k);
            }
            switch (th)
            {
                case Theme.Lab: r.bg = new Color(0.05f, 0.06f, 0.09f); r.ambience = "amb_lab"; break;
                case Theme.Village: r.bg = new Color(0.06f, 0.03f, 0.05f); r.ambience = "amb_fire"; break;
                case Theme.House: r.bg = new Color(0.08f, 0.05f, 0.05f); r.ambience = "amb_house"; break;
                case Theme.Desktop: r.bg = new Color(0.1f, 0.18f, 0.28f); r.ambience = "amb_digital"; break;
            }
            return r;
        }

        /// <summary>바닥 덧대기 없는 방 (컴퓨터 안: 화면 한 장, 맨 아래 줄이 작업표시줄).</summary>
        static RoomDef RNoPad(string id, string scene, string title, Theme th, params string[] rows)
        {
            var r = new RoomDef { id = id, scene = scene, title = title, theme = th, rows = rows };
            r.ambience = "amb_digital";
            return r;
        }

        /// <summary>격자 그리기 도우미. (열, 행) 행은 위에서부터.</summary>
        class Grid
        {
            public readonly char[,] c; public readonly int W, H;
            public Grid(int w, int h, char fill = '.') { W = w; H = h; c = new char[h, w]; for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) c[y, x] = fill; }
            public Grid Rect(int x0, int y0, int x1, int y1, char ch) { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) c[y, x] = ch; return this; }
            public Grid Set(int x, int y, char ch) { c[y, x] = ch; return this; }
            public string[] Rows() { var l = new string[H]; for (int y = 0; y < H; y++) { var a = new char[W]; for (int x = 0; x < W; x++) a[x] = c[y, x]; l[y] = new string(a); } return l; }
        }

        static string Fill(char wall, int w, char inner = '.') => wall + new string(inner, w - 2) + wall;

        /// <summary>맵 이동 출구: 해당 열의 벽을 행 범위만큼 연다.</summary>
        static void Open(RoomDef r, int col, int r0, int r1)
        {
            for (int i = r0; i <= r1; i++)
            {
                var a = r.rows[i].ToCharArray();
                if (a[col] == '#' || a[col] == '|') a[col] = '.';
                r.rows[i] = new string(a);
            }
        }

        static List<RoomDef> cache;
        public static List<RoomDef> All()
        {
            if (cache != null) return cache;
            cache = new List<RoomDef> { P01(), P02(), P03(), C01(), C02(), C03(), M09(), C04(), C05(), C06(), M14(), M13(), M15() };
            cache.AddRange(UndergroundRooms().Concat(LateRooms()));   // 4~6장: EWRoomsUnderground.cs
            return cache;
        }

        public static void ClearCache() { cache = null; }

        static readonly Color Ink = new Color(0.07f, 0.04f, 0.06f);

        // ================================================================ 오프닝 A: 불타는 마을 (연출 전용)
        static RoomDef P01()
        {
            var g = new Grid(72, 14);
            g.Rect(0, 0, 0, 12, '|').Rect(71, 0, 71, 12, '|').Rect(0, 13, 71, 13, '#');
            var r = R("P01", "P01_BurningVillage", "불타는 마을", Theme.Village, g.Rows());
            r.hideHud = true;
            r.layers = c =>
            {
                float floor = c.H - 13;                 // 바닥 윗면 y
                var sky = c.Layer(Ctx.S("V_Sky"), new Vector2(c.Ref.x, c.Ref.y - 5.625f), -70, 1f, null, 1f, "Sky");
                sky.transform.localScale = new Vector3(220, 1, 1); sky.GetComponent<Parallax>().vertical = 1f;
                // 원경: 멀리 불타는 성벽·지붕·연기 기둥
                c.Band(Ctx.S("V_Far"), new Vector2(c.Ref.x, floor - 0.6f), r.W + 50, -60, 0.75f, new Color(0.85f, 0.7f, 0.72f), "FarTown");
                var smokeFr = Ctx.SF("FX_Smoke");
                foreach (var x in new[] { 8f, 24f, 41f, 58f, 70f })
                {
                    var sm = c.Anim(new Vector2(x, floor + 5.5f), smokeFr, 3, -58, "SmokeColumn");
                    sm.transform.localScale = new Vector3(4f, 6f, 1f); sm.GetComponent<SpriteRenderer>().color = new Color(0.4f, 0.3f, 0.32f, 0.28f);
                    var px = sm.AddComponent<EW.Parallax>(); px.follow = 0.6f; px.reference = c.Ref;
                }
                // 연기 낀 하늘과 지평선의 뜨거운 아지랑이 (참고 이미지의 연무)
                var fade = Ctx.S("FX_FadeDown"); var band = Ctx.S("FX_HazeBand");
                var smokeTop = c.Layer(fade, new Vector2(c.Ref.x, floor + 9.0f), -59, 0.9f, new Color(0.06f, 0.04f, 0.06f, 0.7f), 1f, "SmokeCeiling");
                smokeTop.transform.localScale = new Vector3((r.W + 60) / 0.125f, 3.2f, 1);
                var haze = c.Layer(band, new Vector2(c.Ref.x, floor + 1.4f), -55, 0.6f, new Color(1f, 0.55f, 0.28f, 0.35f), 1f, "HorizonHaze");
                haze.transform.localScale = new Vector3((r.W + 60) / 0.125f, 1.6f, 1);
                // 중경: 불타는 집들 (어둡게, 지붕 불)
                string[] houses = { "V_HouseA", "V_HouseB", "V_HouseC", "V_HouseA", "V_HouseC", "V_HouseB", "V_HouseA", "V_HouseC" };
                for (int i = 0; i < houses.Length; i++)
                {
                    float x = 4 + i * 9.5f;
                    var h = c.Layer(Ctx.S(houses[i]), new Vector2(x, floor - 0.1f), -40, 0.35f, new Color(0.62f, 0.44f, 0.42f), 1f, "MidHouse");
                    if (houses[i] != "V_HouseB")
                    {
                        var f = c.Anim(new Vector2(x + 0.4f, floor + 2.6f), Ctx.SF("FX_FireBig"), 10, -39, "RoofFire");
                        f.transform.SetParent(h.transform, true);
                        c.Glow(new Vector2(x + 0.4f, floor + 3.0f), 2.2f, new Color(1f, 0.45f, 0.15f, 0.16f), -38, h.transform);
                    }
                }
                // 가까운 중경: 크게 무너진 집 실루엣 (가까울수록 어둡게, 불빛 테두리)
                foreach (var (x, n) in new[] { (12f, "V_HouseC"), (31f, "V_HouseA"), (50f, "V_HouseC"), (69f, "V_HouseA") })
                {
                    var nh = c.Layer(Ctx.S(n), new Vector2(x, floor - 0.2f), -20, 0.12f, new Color(0.3f, 0.2f, 0.2f), 1.6f, "NearHouse");
                    var nf = c.Anim(new Vector2(x - 0.8f, floor + 3.6f), Ctx.SF("FX_FireBig"), 9, -19, "NearFire");
                    nf.transform.SetParent(nh.transform, true); nf.transform.localScale = Vector3.one * 0.9f;
                }
                var groundHaze = c.Layer(Ctx.S("FX_HazeBand"), new Vector2(c.Ref.x, floor + 0.5f), -18, 0.1f, new Color(0.95f, 0.45f, 0.22f, 0.22f), 1f, "GroundHaze");
                groundHaze.transform.localScale = new Vector3((r.W + 30) / 0.125f, 0.8f, 1);
                // 근경(플레이 영역): 잔해·수레·울타리·무너진 벽·작은 불
                c.Decor(new Vector2(5f, floor), Ctx.S("V_RuinWall"), -8, 1, "RuinWall");
                c.Decor(new Vector2(9f, floor), Ctx.S("V_RubbleM"), -6, 1, "Rubble");
                c.Decor(new Vector2(21f, floor), Ctx.S("V_Cart"), -6, 1, "Cart");
                c.Decor(new Vector2(27f, floor), Ctx.S("V_Fence"), -6, 1, "Fence");
                c.Decor(new Vector2(33f, floor), Ctx.S("V_RubbleL"), -6, 1, "Rubble");
                c.Decor(new Vector2(40f, floor), Ctx.S("V_RuinWall"), -8, 1, "RuinWall");
                c.Decor(new Vector2(46f, floor), Ctx.S("V_RubbleS"), -6, 1, "Rubble");
                c.Decor(new Vector2(52f, floor), Ctx.S("V_RubbleL"), -6, 1, "Rubble");
                c.Decor(new Vector2(67.5f, floor), Ctx.S("V_RubbleM"), -6, 1, "Rubble");
                foreach (var x in new[] { 9.6f, 22.4f, 33.6f, 47f, 52.8f, 68f })
                {
                    c.Anim(new Vector2(x, floor + 0.15f), Ctx.SF("FX_Fire"), 10, -5, "Fire");
                    c.Glow(new Vector2(x, floor + 0.6f), 1.6f, new Color(1f, 0.5f, 0.2f, 0.16f), 41, null);
                }
                // 마을 사이에 숨겨진 작은 집 (루의 집)
                c.Decor(new Vector2(62.2f, floor), Ctx.S("V_LuHouse"), -7, 1, "LuHouse").GetComponent<SpriteRenderer>().color = new Color(0.78f, 0.62f, 0.56f);
                c.Mark("house_door", new Vector2(62f, floor));
                c.Mark("cam_open", new Vector2(44f, floor + 4.6f));
                // 전경: 카메라 가까운 그을린 기둥과 잔해 (상호작용 대상 없음)
                foreach (var x in new[] { 0.7f, 55.2f, 75f })
                    c.Layer(Ctx.S("V_ForeBeam"), new Vector2(x, floor - 4.6f), 60, -0.4f, null, 1f, "ForeBeam");
                c.Band(Ctx.S("V_ForeRubble"), new Vector2(c.Ref.x, floor - 3.2f), r.W + 40, 61, -0.25f, null, "ForeRubble");
                // 불티(위로)와 재(아래로)
                c.Particles("Embers", Ctx.S("FX_Ember"), 46, new Vector2(0.25f, 0.75f), new Vector2(0.35f, 0.3f), new Color(1f, 0.75f, 0.3f, 0.9f), new Color(1f, 0.4f, 0.15f, 0.6f), 5f, 45, true);
                c.Particles("Ash", Ctx.S("FX_Ash"), 34, new Vector2(0.2f, -0.35f), new Vector2(0.25f, 0.12f), new Color(0.7f, 0.66f, 0.66f, 0.7f), new Color(0.5f, 0.46f, 0.46f, 0.4f), 9f, 44, false, null, 0.6f);
            };
            r.legend['.'] = (c, x, y) => { };
            r.extra = c => c.Spawn(14, 12, "start", 1);
            return r;
        }

        // ================================================================ 오프닝 B: 무너진 작은 집 (연출 전용, 2026-11-15 축소)
        // 집은 14유닛 폭으로 줄이고 뒤쪽 벽이 날아간 그림(H_RuinBack)만 둔다. 집 바깥은 아무것도 그리지 않아 카메라 배경(검정)이 보인다.
        static RoomDef P02()
        {
            var g = new Grid(16, 10);
            g.Rect(0, 0, 0, 8, '|').Rect(15, 0, 15, 8, '|').Rect(0, 9, 15, 9, '#');
            var r = R("P02", "P02_RuinedHouse", "무너진 작은 집", Theme.House, g.Rows());
            r.hideHud = true;
            r.bg = Color.black;
            r.layers = c =>
            {
                float floor = c.H - 9;
                c.Decor(new Vector2(8f, floor), Ctx.S("H_RuinBack"), -45, 1, "RuinBack");
                c.Decor(new Vector2(2.4f, floor), Ctx.S("H_Shelf"), -6, 1, "Shelf");
                c.Decor(new Vector2(4.6f, floor), Ctx.S("H_Bed"), -4, 1, "Bed");
                c.Decor(new Vector2(7.6f, floor), Ctx.S("H_MaterialsA"), -3, 1, "Materials");
                c.Decor(new Vector2(12.6f, floor), Ctx.S("H_Desk"), -5, 1, "Desk");
                var pc = c.Frames(new Vector2(12.6f, floor + 0.69f), Ctx.SF("H_PC"), -4, "PC");
                c.Mark("pc", Vector2.zero, pc.gameObject);
                c.Mark("pc_screen", new Vector2(12.6f, floor + 1.3f));
                c.Mark("pc_touch", new Vector2(12.2f, floor + 1.15f));
                c.Darkness(0.16f, 40);
                c.Particles("Dust", Ctx.S("FX_Dust"), 10, new Vector2(0.05f, -0.05f), new Vector2(0.08f, 0.05f), new Color(0.9f, 0.8f, 0.7f, 0.3f), new Color(0.8f, 0.6f, 0.5f, 0.12f), 9f, 45, false);
            };
            r.extra = c => c.Spawn(9, 8, "start", 1);
            return r;
        }

        // ================================================================ 오프닝 C: 컴퓨터 안 (직접 탐색, 2026-11-15)
        // 화면 한 장 = 방 한 개 (20x12, 바닥 덧대기 없음). 작업표시줄은 맨 아래 한 줄에 한 개만 (보이지 않는 바닥 '|' 위에 그림 한 장).
        // 아이콘·창 제목줄이 발판(보이지 않는 '='). 안내서(J) → 연습 대상(Q/W/E/R) → 접속 기록·출구 조사 → 강제 전송.
        static RoomDef P03()
        {
            var g = new Grid(20, 12);
            g.Rect(0, 0, 0, 10, '|').Rect(19, 0, 19, 10, '|').Rect(0, 0, 19, 0, '|').Rect(0, 11, 19, 11, '|');
            var r = RNoPad("P03", "P03_FriendDesktop", "컴퓨터 안", Theme.Desktop, g.Rows());
            r.hideHud = false;
            r.hidePlatforms = true;
            r.bg = new Color(0.08f, 0.12f, 0.2f);
            r.layers = c =>
            {
                c.Decor(new Vector2(10f, 1f), Ctx.S("D_Desktop"), -60, 1, "Desktop");
                c.Decor(new Vector2(10f, 0f), Ctx.S("D_Taskbar"), -2, 1, "Taskbar");

                c.Particles("Dots", Ctx.P0("FX_LightDot"), 8, new Vector2(0f, 0.2f), new Vector2(0.15f, 0.1f), new Color(0.7f, 0.9f, 1f, 0.45f), new Color(0.5f, 0.8f, 1f, 0.15f), 6f, 30, true);
            };
            r.extra = c =>
            {
                c.Spawn(5, 10, "start", 1);
                c.Mark("land", c.Cell(5, 10));
                Inspect Icon(float x, float yTop, string sprite, string id, string dlg, string ev, string prompt)
                {
                    var it = c.Inspect(new Vector2(x, yTop), Ctx.S(sprite), id, dlg, ev: ev, order: -5, prompt: prompt, range: 1.0f);
                    it.promptOffset = new Vector2(0, 1.25f);
                    return it;
                }
                Icon(6f, 5f, "D_Icon_guide", "pc_guide", null, "pc_guide", "E-D05-47 안내서");
                Icon(14f, 5f, "D_Icon_status", "pc_internet", null, "pc_internet", "인터넷 · 잠김");
                c.EventZone(1, 1, 18, 10, "pc_enter");
            };
            return r;
        }

        // ================================================================ 오프닝 D·E + 정전된 실험실 (첫 조작: 조사 → 깨달음 → 경보 → 문틈)
        // 왼쪽(0~24)은 인체 개조 실험실(조사 공간), 가운데(28.5)는 실험 통, 오른쪽(43.5)은 연구원이 들어온 문(방전에 비틀림).
        static RoomDef C01()
        {
            const float OX = 22f;   // 옛 실험 통 방 좌표를 오른쪽으로 옮긴 양
            var g = new Grid(48, 13);
            g.Rect(0, 0, 47, 0, '#').Rect(0, 0, 0, 11, '#').Rect(45, 0, 47, 11, '#').Rect(0, 12, 47, 12, '#');
            // 낮은 단 (1칸, 구속대 침대·용기 구역) — 쉬운 높낮이
            g.Rect(1, 11, 9, 11, '#');
            var r = R("C01", "C01_TankLab", "실험실", Theme.Lab, g.Rows());
            r.altAmbienceFlag = "c1_tank_broken"; r.altAmbience = "amb_lab_dark";
            r.useAlarmAmbience = true;
            r.layers = c => EWSceneBuilder.LabLayers(c, 0, c.H - 12 + 1.6f, c.H - 2.6f);
            r.extra = c =>
            {
                float floor = c.H - 12;
                // ---- 실험 통 (뒤판은 루 뒤, 유리는 루 앞)
                var tankPos = new Vector2(6.5f + OX, floor);
                c.Frames(tankPos, Ctx.SF("L_TankBack"), 5, "TankBack", ("c1_tank_broken", 1));
                c.Frames(tankPos, Ctx.SF("L_TankGlass"), 14, "TankGlass", ("c1_tank_broken", 1));
                c.Mark("tank", tankPos);
                c.Spawn(28, 11, "tank", -1);
                c.Spawn(28, 11, "start", -1);
                var walls = new List<Collider2D>();
                // 통 안 바닥 (받침 윗면 0.5): 루가 받침 위에 서서 몸 전체가 유리 안쪽에 들어간다
                foreach (var (ox, oy, w, h) in new[] { (-1.1f, 1.7f, 0.15f, 3.4f), (1.1f, 1.7f, 0.15f, 3.4f), (0f, 3.4f, 2.4f, 0.12f), (0f, 0.25f, 2.05f, 0.5f) })
                {
                    var wgo = c.Obj("TankWall", tankPos + new Vector2(ox, oy));
                    var bc = wgo.AddComponent<BoxCollider2D>(); bc.size = new Vector2(w, h); walls.Add(bc);
                }
                foreach (var (i, ex) in new[] { (1, -0.8f), (2, 0f), (3, 0.8f) }) c.Mark("electrode_" + i, tankPos + new Vector2(ex, 2.95f));
                c.Decor(new Vector2(9.2f + OX, floor), Ctx.S("L_Cables"), -3, 1, "Cables");
                // 통 표식 (핵심 단서): 깨진 통 옆 판
                var label = c.Decor(tankPos + new Vector2(-1.5f, 1.3f), Ctx.S("S_TankLabel"), 6, 1, "TankLabel");
                c.Mark("clue_tank", tankPos + new Vector2(-1.5f, 1.5f));
                var tl = c.Inspect(tankPos + new Vector2(-1.6f, 0f), null, "c01_tank_label", null, ev: "lab_tank_label", prompt: "통 표식", range: 0.8f);
                tl.requireFlag = "op_tank_done"; tl.promptOffset = new Vector2(0, 2.2f);
                // ---- 문 (닫힘 → 벌컥 열림 → 비틀려 멈춤). 문틈은 핵심 단서이자 출구
                var doorPos = new Vector2(21.5f + OX, floor);
                var door = c.Frames(doorPos, Ctx.SF("L_Door"), -2, "LabDoor", ("c1_tank_broken", 2), ("op_r_in", 1));
                c.Mark("door", Vector2.zero, door.gameObject);
                c.Decor(doorPos + new Vector2(0, 3.0f), Ctx.S("S_SignIsolation"), -1, 1, "SignIsolation");
                c.Mark("clue_gap", doorPos + new Vector2(0, 2.2f));
                var gap = c.Inspect(doorPos, null, "c01_gap", null, ev: "op_gap", prompt: "문틈", range: 1.3f);
                gap.requireFlag = "op_tank_done"; gap.promptOffset = new Vector2(0, 2.8f);
                // ---- 압수품 바구니 (탈출 경로 위): 루의 겉옷 주머니에 초콜릿이 남아 있다 (베넬에게 건넬 수 있는 근거)
                var basket = c.Decor(new Vector2(18.6f + OX, floor), Ctx.P0("Prop_Clothes"), -1, 1, "Confiscated");
                var bk = c.Inspect(new Vector2(18.6f + OX, floor), null, "c01_confiscated", null, ev: "lab_pouch", prompt: "압수품 바구니", range: 1.0f);
                bk.requireFlag = "op_tank_done"; bk.hideFlag = "got_pouch";
                c.FlagSwitch("PouchTaken", "got_pouch", null, new[] { basket });
                // ---- 제어 콘솔 (켜짐 → 파손, 잔류 스파크)
                foreach (var (x, n) in new[] { (10.6f + OX, 1), (14.8f + OX, 2) })
                {
                    c.Frames(new Vector2(x, floor), Ctx.SF("L_Console"), -2, "Console" + n, ("c1_tank_broken", 2));
                    c.Sparks(new Vector2(x, floor + 0.9f), "c1_tank_broken", 0.4f);
                    var ci = c.Inspect(new Vector2(x, floor), null, "c01_console_" + n, "c1_console_broken", prompt: "콘솔");
                    ci.requireFlag = "op_tank_done";
                }
                // ---- 천장 조명 (방전 때 꺼짐)
                var glows = new List<GameObject>();
                var lightFr = Ctx.P("Prop_CeilingLight");
                foreach (var x in new[] { 4.5f + OX, 11.5f + OX, 18.5f + OX })
                {
                    var cord = c.Decor(new Vector2(x, floor + 9.3f), Ctx.P0("Tile_Pixel"), 2, 1, "LightCord");
                    cord.transform.localScale = new Vector3(0.5f, 40f, 1); cord.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.22f, 0.28f);
                    c.Frames(new Vector2(x, floor + 6.7f), lightFr, 3, "CeilingLight", ("c1_tank_broken", 1));
                    glows.Add(c.Glow(new Vector2(x, floor + 5.6f), 3.6f, new Color(0.75f, 0.9f, 1f, 0.2f), 41, null, true).gameObject);
                }
                // ---- 독립 비상등 (배터리: 정전에도 켜짐) — 정전 동안 거의 유일한 빛
                var em = Ctx.P("Prop_EmergencyLight");
                c.Decor(new Vector2(17f + OX, floor + 3.6f), em.FirstOrDefault(), 2, 1, "EmergencyLight");
                c.Glow(new Vector2(17f + OX, floor + 3.8f), 1.6f, new Color(1f, 0.25f, 0.2f, 0.45f), 42, null, true);
                var ei = c.Inspect(new Vector2(17f + OX, floor), null, "c01_emergency", null, ev: "lab_emergency", prompt: "비상등", range: 1.0f);
                ei.requireFlag = "op_tank_done";
                c.Decor(new Vector2(5f, floor + 4.2f), em.FirstOrDefault(), 2, 1, "EmergencyLight2");
                c.Glow(new Vector2(5f, floor + 4.4f), 1.4f, new Color(1f, 0.3f, 0.22f, 0.35f), 42, null, true);
                // ---- 정전된 실험실 (조사 공간). 진행 대상은 밝게, 장식은 어둡게.
                float step = floor + 1f;   // 낮은 단 윗면
                // 불투명 실험 용기 (소리만)
                c.Decor(new Vector2(2.2f, step), Ctx.S("S_Jar1"), -2, 1, "Jar"); c.Decor(new Vector2(3.2f, step), Ctx.S("S_Jar0"), -2, 1, "Jar");
                var jars = c.Inspect(new Vector2(2.7f, step), null, "c01_jars", null, ev: "lab_jars", prompt: "용기", range: 0.9f);
                jars.requireFlag = "op_tank_done";
                // 구속대 침대 + 꺼진 수술등 (다가가기 꺼림)
                c.Decor(new Vector2(6.5f, step), Ctx.S("S_RestraintBed"), -1, 1, "RestraintBed");
                c.Decor(new Vector2(6.5f, floor + 6.2f), Ctx.S("S_SurgLamp"), 2, 1, "SurgLamp");
                c.Mark("bed", new Vector2(6.5f, step));
                var bed = c.Inspect(new Vector2(8.6f, step), null, "c01_bed", null, ev: "lab_bed", prompt: "침대", range: 1.6f);
                bed.requireFlag = "op_tank_done"; bed.promptOffset = new Vector2(-1.5f, 1.6f);
                // 의료 도구 쟁반 (만지는 동작)
                var tray = c.Decor(new Vector2(11.5f, floor), Ctx.S("S_ToolTray"), -1, 1, "ToolTray");
                c.Mark("tools", Vector2.zero, tray);
                var ti = c.Inspect(new Vector2(11.5f, floor), null, "c01_tools", null, ev: "lab_tools", prompt: "도구", range: 0.9f);
                ti.requireFlag = "op_tank_done";
                // 신체 도식 판 (핵심 단서: 확대)
                c.Decor(new Vector2(14.5f, floor + 1.2f), Ctx.S("S_BodyBoard"), -3, 1, "BodyBoard");
                c.Mark("clue_board", new Vector2(14.5f, floor + 1.9f));
                var bd = c.Inspect(new Vector2(14.5f, floor), null, "c01_board", null, ev: "lab_board", prompt: "벽의 도식", range: 1.1f);
                bd.requireFlag = "op_tank_done"; bd.promptOffset = new Vector2(0, 2.8f);
                // 개조 부품 수거함 (소리만 들리는 반응)
                var bin = c.Decor(new Vector2(17.6f, floor), Ctx.S("S_PartsBin"), -1, 1, "PartsBin");
                c.Mark("bin", Vector2.zero, bin);
                var bi = c.Inspect(new Vector2(17.6f, floor), null, "c01_bin", null, ev: "lab_bin", prompt: "수거함", range: 0.9f);
                bi.requireFlag = "op_tank_done";
                // 관찰창 (유리 뒤 실루엣)
                c.Decor(new Vector2(20.8f, floor + 1.2f), Ctx.S("S_ObsWindow"), -20, 1, "ObsWindow");
                var wi = c.Inspect(new Vector2(20.8f, floor), null, "c01_window", null, ev: "lab_window", prompt: "관찰창", range: 1.0f);
                wi.requireFlag = "op_tank_done"; wi.promptOffset = new Vector2(0, 3.2f);
                // 번호가 붙은 보관함
                c.Decor(new Vector2(24.2f, floor), Ctx.S("S_Lockers"), -3, 1, "Lockers");
                var lk = c.Inspect(new Vector2(24.2f, floor), null, "c01_lockers", null, ev: "lab_lockers", prompt: "보관함 (47번)", range: 1.0f);
                lk.requireFlag = "op_tank_done"; lk.promptOffset = new Vector2(0, 1.8f);
                // ---- 작업등 4개 (깨달음 뒤 차례로 켜짐) + 경광등 (경보 때만)
                int li = 1;
                foreach (var x in new[] { 26f, 18f, 10f, 3.5f }) c.WorkLamp(new Vector2(x, floor + 6.4f), "lab_power_" + (li++));
                foreach (var x in new[] { 8.5f, 33f, 41.5f }) c.Alarm(new Vector2(x, floor + 7.2f));
                // ---- 방전 후 상태 (파편·그을음·연기·어둠). 정전 중에는 짙게, 전력 복구 뒤에는 옅게.
                var after = new List<GameObject>();
                var dark = c.Darkness(0.5f, 40); after.Add(dark);
                var d1 = c.Decor(new Vector2(8.8f + OX, floor), Ctx.S("L_FallenPanel"), 3, 1, "Debris1"); d1.AddComponent<EW.DebrisHome>(); c.Mark("debris_1", Vector2.zero, d1); after.Add(d1);
                var d2 = c.Decor(new Vector2(18.3f + OX, floor), Ctx.S("L_DebrisS"), 3, 1, "Debris2"); d2.AddComponent<EW.DebrisHome>(); c.Mark("debris_2", Vector2.zero, d2); after.Add(d2);
                after.Add(c.Decor(new Vector2(6.5f + OX, floor + 0.02f), Ctx.S("L_Scorch"), 4, 1, "Scorch"));
                after.Add(c.Particles("Smoke", Ctx.S("FX_Smoke"), 10, new Vector2(0.05f, 0.25f), new Vector2(0.1f, 0.1f), new Color(0.35f, 0.35f, 0.4f, 0.35f), new Color(0.25f, 0.25f, 0.3f, 0.2f), 7f, 35, false, "c1_tank_broken").gameObject);
                c.Sparks(tankPos + new Vector2(0, 3.0f), "c1_tank_broken", 0.8f);
                c.FlagSwitch("TankBroken", "c1_tank_broken", after.ToArray(), glows.ToArray(), walls.ToArray());
                c.FlagSwitch("PowerBack", "lab_power", null, new[] { dark });
                var darkLight = c.Darkness(0.22f, 40, "DarknessLight");
                c.FlagSwitch("PowerBackLight", "lab_power", new[] { darkLight });
                // ---- 연구원 두 명 (문 → 진입 → 대화 → 접근 → 방전에 쓰러짐). 생사는 확정하지 않는다.
                c.ActorEx(doorPos, "r1", "researcher", -1, "op_r_in", "c1_researchers_down", new Vector2(12.2f + OX, floor));
                c.ActorEx(doorPos + new Vector2(0.6f, 0), "r2", "researcher2", -1, "op_r_in", "c1_researchers_down", new Vector2(15.6f + OX, floor));
                c.Mark("r1_stop", new Vector2(17.6f + OX, floor)); c.Mark("r2_stop", new Vector2(19.3f + OX, floor));
                c.Mark("r1_near", new Vector2(9.0f + OX, floor)); c.Mark("r2_near", new Vector2(10.3f + OX, floor));
                c.Mark("r1_down", new Vector2(12.2f + OX, floor)); c.Mark("r2_down", new Vector2(15.6f + OX, floor));
                foreach (var (n, x) in new[] { (1, 12.2f + OX), (2, 15.6f + OX) })
                {
                    var ri = c.Inspect(new Vector2(x, floor), null, "c01_researcher_" + n, null, ev: "lab_researcher", prompt: "연구원");
                    ri.requireFlag = "c1_researchers_down";
                }
                c.Mark("cam_lab", new Vector2(14f + OX, floor + 1.9f));
            };
            return r;
        }

        // ================================================================ C02 감시 복도 (2026-11-15): 짧은 직선 가속 → CCTV(W 로 잠깐 끄기) → 전경 엄폐 구간 → 출구
        // 경비가 없는 구역: CCTV 가 신고하면 오른쪽 끝 출입구에서 지원 드론이 온다 (Reinforcements).
        static RoomDef C02()
        {
            var g = new Grid(58, 12);
            g.Rect(0, 0, 57, 0, '#').Rect(0, 0, 0, 11, '#').Rect(57, 0, 57, 11, '#').Rect(0, 11, 57, 11, '#');
            g.Rect(57, 6, 57, 10, '.');
            // 엄폐 구간 위의 짧은 높은 통로 (CCTV2 를 머리 위로 지나가는 다른 길)
            g.Rect(37, 9, 45, 9, '=');
            var r = R("C02", "C02_EscapeWing", "격리 실험동 복도", Theme.Lab, g.Rows());
            r.useAlarmAmbience = true;
            r.layers = c => EWSceneBuilder.LabLayers(c, 1);
            r.extra = c =>
            {
                float floor = c.H - 11;
                c.Spawn(2, 10, "from_lab", 1);
                c.Spawn(2, 10, "start", 1);
                c.Spawn(55, 10, "from_c03", -1);
                c.Decor(new Vector2(1.3f, floor), Ctx.SF("L_Door").ElementAtOrDefault(2), -6, 1, "GapDoor");
                c.Checkpoint(2, 6, 4, 10);
                foreach (var x in new[] { 6f, 20f, 34f, 50f }) c.Alarm(new Vector2(x, floor + 5.6f));
                // A. 직선 가속 구간 (x 3~13): 아무것도 없다 — 달린다
                // B. CCTV 1 (x 14~24): W 로 잠깐 끄고 지나간다. 고체 상자(발판)는 시야를 막는 엄폐가 아니라 길막·발판
                c.Camera(new Vector2(19.5f, floor + 4.6f), "C02_cam1", -150f, -30f, 0.25f);
                c.Crate(23.25f, floor);
                c.EventZone(11, 6, 12, 10, "c2s_jam_tut");
                // C. CCTV 2 + 전경 엄폐물 (x 27~46): 시선이 지나갈 때 선반 뒤에 S 로 숨는다 (또는 W, 또는 위 통로)
                c.Checkpoint(25, 6, 26, 10);
                c.Camera(new Vector2(36.5f, floor + 4.6f), "C02_cam2", -165f, -15f, 0.6f);
                c.HideCover(30.2f, floor, "S_HideRack");
                c.HideCover(35.6f, floor, "S_HideSacks");
                c.HideCover(41.4f, floor, "S_HideRack");
                c.Crate(39.0f, floor, "S_CrateS");
                c.EventZone(27, 6, 28, 10, "c2s_cover_tut");
                // D. 출구 앞 짧은 직선 (지원 드론이 들어오는 문)
                c.Reinforce(new Vector2(56.2f, floor + 2.6f));
                c.Checkpoint(48, 6, 49, 10);
                c.Exit(57, 6, 57, 10, "C03", "start");
            };
            return r;
        }

        // ================================================================ C03 설비실 (2026-11-15): 높낮이 + 순찰 경비(Q 경직 → Shift 로 빠져나감) + 노출 전원으로 셔터 제어판 파괴(E)
        static RoomDef C03()
        {
            var g = new Grid(64, 14);
            g.Rect(0, 0, 63, 0, '#').Rect(0, 0, 0, 13, '#').Rect(63, 0, 63, 13, '#').Rect(0, 13, 63, 13, '#');
            g.Rect(0, 9, 0, 12, '.').Rect(63, 9, 63, 12, '.');
            // 높낮이: 왼쪽 설비 단 (발판 두 층) + 오른쪽 자료실 위층 (사다리)
            g.Rect(8, 11, 13, 11, '=').Rect(14, 9, 19, 9, '=');
            g.Set(39, 7, '^'); g.Rect(39, 8, 39, 12, 'H'); g.Rect(40, 7, 58, 7, '=');
            g.Set(50, 6, 'k');
            var r = R("C03", "C03_SpecimenArchive", "설비실 · 자료실", Theme.Lab, g.Rows());
            r.layers = c =>
            {
                EWSceneBuilder.LabLayers(c, 0);
                float floor = c.H - 13;
                foreach (var x in new[] { 33f })
                    c.Layer(Ctx.S("L_Chamber"), new Vector2(x, floor + 1.7f), -42, 0.12f, new Color(0.8f, 0.85f, 0.95f), 1f, "Chamber");
            };
            r.extra = c =>
            {
                float floor = c.H - 13;
                c.Spawn(2, 12, "start", 1);
                c.Spawn(2, 12, "from_c02", 1);
                c.Spawn(61, 12, "from_cargo", -1);
                c.Spawn(54, 6, "from_cafe", -1);
                c.Checkpoint(1, 9, 2, 12);
                // A. 설비 단 아래 순찰 경비: Q(톡!)로 경직시킨 뒤 Shift 로 지나가거나, 위 발판으로 넘어간다
                c.Guard(18, 12, "C03_guard1", "patrol_guard", 14f, 23f, -1);
                c.HideCover(6.2f, floor, "S_HideSacks");
                c.Crate(24.25f, floor, "S_CrateS");
                // B. 셔터 (x 27): 옆 노출 전원에서 E 로 제어판을 터뜨려 연다
                c.PowerSrc(new Vector2(22.4f, floor + 3.1f), "C03_src", 1);
                var sp = c.Panel(new Vector2(26.0f, floor), "C03_panel", "c03_shutter_open");
                var shutterPos = new Vector2(27.5f, floor);
                var sh = c.Frames(shutterPos, Ctx.SF("L_Door"), -1, "Shutter", ("c03_shutter_open", 1));
                var blk = c.Obj("ShutterBlock", shutterPos + new Vector2(0, 2f));
                var bbc = blk.AddComponent<BoxCollider2D>(); bbc.size = new Vector2(0.9f, 4f);
                c.FlagSwitch("ShutterC03", "c03_shutter_open", null, null, new Collider2D[] { bbc });
                var si = c.Inspect(shutterPos + new Vector2(-0.7f, 0), null, "c03_shutter", "c3_shutter_locked", prompt: "셔터");
                si.hideFlag = "c03_shutter_open";
                c.Checkpoint(29, 9, 29, 12);
                // C. 자료실 (광고·오래된 양식·기록은 그대로) + CCTV + 순찰 경비 + 엄폐
                Inspect Poster(int x, string id, string dlg, string ev = null, string prompt = "광고") =>
                    c.Inspect(c.Cell(x, 12) + new Vector2(0, 0.9f), Ctx.P0("Prop_PosterEmblem"), id, dlg, ev: ev, order: -6, prompt: prompt);
                Poster(31, "m08_ad_aurelius", "c2_ad_aurelius");
                Poster(34, "m08_ad_meilin", "c2_ad_meilin");
                Poster(37, "m08_ad_lacrimosa", "c2_ad_lacrimosa");
                Poster(44, "m08_old_form", null, "c2_ad_first", "오래된 양식");
                c.Decor(c.Cell(48, 12), Ctx.P0("Prop_Desk"), -2); c.Doc(48, 12, "D01", "m08_doc_d01");
                c.Camera(new Vector2(35.5f, floor + 4.2f), "C03_cam", -160f, -60f, 0.3f);
                c.Guard(52, 12, "C03_guard", "patrol_guard", 45.5f, 55.5f, -1);
                c.HideCover(47.2f, floor, "S_HideRack");
                c.Crate(53.75f, floor);
                c.Tray(new Vector2(57.5f, floor + 5.7f), "C03_tray");
                c.Door(56, 6, "M09", "from_archive", "직원 식당");
                c.Reinforce(new Vector2(62.5f, floor + 2.8f));
                c.Exit(0, 9, 0, 12, "C02", "from_c03");
                c.Exit(63, 9, 63, 12, "C04", "start");
            };
            r.useAlarmAmbience = true;
            return r;
        }

        static RoomDef M09()
        {
            var r = R("M09", "M09_StaffCafeteria", "직원 식당", Theme.Lab,
                "######################",
                "#....................#",
                "#....................#",
                "#....................#",
                "#....................#",
                "#....................#",
                "#....................#",
                "#....................#",
                "#....................#",
                "#....................#",
                "#.Df......V.R..T.T...#",
                "######################");
            r.legend['D'] = (c, x, y) => c.Door(x, y, "C03", "from_cafe", "자료실로");
            r.legend['f'] = (c, x, y) => c.Spawn(x, y, "from_archive", 1);
            r.legend['V'] = (c, x, y) =>
            {
                var fr = Ctx.P("Prop_Vending");
                var d = c.Device(c.Cell(x, y), "M09_vending", DeviceKind.Vending, new[] { fr[0] }, new[] { fr[1] }, new[] { fr[2] }, essential: true, order: 1);
                var it = d.gameObject.AddComponent<Inspect>(); it.id = "m09_vending"; it.eventId = "c2_vending"; it.promptText = "자판기"; it.promptOffset = new Vector2(0, 2.3f);
            };
            r.legend['R'] = (c, x, y) => c.Inspect(c.Cell(x, y), Ctx.P0("Prop_Lever"), "m09_lever", null, ev: "c2_lever", prompt: "수동 리셋");
            r.legend['T'] = (c, x, y) => c.Decor(c.Cell(x, y), Ctx.P0("Prop_Desk"), -2);
            r.extra = c => c.BubbleZone(8, 2, 12, 10, "vending");
            return r;
        }

        // ================================================================ 화물 적재장: 가운데 큰 기계, 두 높이, 전원선 우회
        static RoomDef C04()
        {
            string inner = "#" + new string('.', 44) + ".";
            var rows = new List<string>
            {
                new string('#', 46),
                inner, inner, inner,
                "#" + new string('.', 27) + "k" + new string('.', 16) + ".",
                "#" + new string('.', 21) + new string('=', 12) + "^" + new string('.', 8) + "r" + "..",
                "#" + new string('.', 33) + "H" + new string('.', 5) + new string('#', 6),
                "#" + new string('.', 33) + "H" + new string('.', 10) + "#",
                "#" + new string('.', 33) + "H" + new string('.', 10) + "#",
                "#" + new string('.', 33) + "H" + new string('.', 10) + "#",
                ".." + "s.k..C" + new string('.', 23) + "E.." + "H" + new string('.', 10) + "#",
                new string('#', 20) + new string('.', 8) + new string('#', 18),
            };
            var r = R("C04", "C04_CargoBay", "화물 적재장", Theme.Lab, rows.ToArray());
            Open(r, 0, 1, 10);
            r.layers = c =>
            {
                EWSceneBuilder.LabLayers(c, 1);
                // 중경의 큰 크레인 (공간의 중심)
                c.Layer(Ctx.S("L_Crane"), new Vector2(24f, c.H - 12f), -30, 0.15f, new Color(0.75f, 0.72f, 0.7f), 1f, "Crane");
            };
            r.legend['s'] = (c, x, y) => { c.Spawn(x, y, "start", 1); c.Spawn(x, y, "from_c03", 1); };
            r.legend['r'] = (c, x, y) => c.Spawn(x, y, "from_lab", -1);
            r.legend['C'] = (c, x, y) => c.Guard(x, y, "C04_carrier", "carrier_bot", x - 2.5f, x + 6f, 1);
            r.legend['E'] = (c, x, y) => c.Guard(x, y, "C04_drone", "capture_drone", x - 6f, x + 1f, -1, null, true, 3.4f);
            r.useAlarmAmbience = true;
            r.extra = c =>
            {
                c.PowerLinePair("C04_line1", 17, 10, 23, 4, new[] { c.Cell(17, 7) + new Vector2(0, 0.5f), c.Cell(23, 5) + new Vector2(0, 0.5f) });
                c.EventZone(13, 1, 15, 10, "c2_line_tutorial");
                c.PowerLinePair("C04_line2", 38, 10, 43, 5, new[] { c.Cell(38, 6) + new Vector2(0, 0.5f), c.Cell(43, 7) });
                c.Exit(0, 1, 0, 10, "C03", "from_cargo");
                c.Exit(45, 1, 45, 5, "C05", "start");
                float floor = c.H - 11;
                c.Checkpoint(1, 1, 2, 10);
                c.Crate(13.25f, floor); c.Crate(26.25f, floor, "S_CrateS");
                c.Camera(new Vector2(30.5f, floor + 4.3f), "C04_cam", -170f, -40f, 0.5f);
                c.HideCover(8.6f, floor, "S_HideRack");
                c.Reinforce(new Vector2(44.5f, floor + 8.6f));
                c.Tray(new Vector2(9.5f, floor + 5.6f), "C04_tray");
                c.Decor(c.Cell(10, 10), Ctx.Named("Foozle_SciFiLab_Tileset/full_decor_tiles.png", "decor_6"), -4);
                c.Decor(c.Cell(11, 10), Ctx.Named("Foozle_SciFiLab_Tileset/full_decor_tiles.png", "decor_7"), -4);
                c.Inspect(c.Cell(5, 10), Ctx.Named("Foozle_SciFiLab_Tileset/full_decor_tiles.png", "decor_6"), "m10_crate", "sys_crate", order: -3, prompt: "상자");
            };
            return r;
        }

        // ================================================================ 3장
        static RoomDef C05()
        {
            var r = R("C05", "C05_CultureRoom", "배양실", Theme.Lab,
                "################################################",
                "#..............................................#",
                "#..............................................#",
                "#..............................................#",
                "#...................................u.T........#",
                "#...............................^========......#",
                "#...............................H..............#",
                "#...............................H..............#",
                "#...............................H..............#",
                "#...............................H..............#",
                "#.s.1.k....G.....k...w....K.....H.6.....B.k..r.#",
                "################################################");
            Open(r, 0, 1, 10);
            Open(r, 47, 1, 10);
            r.legend['s'] = (c, x, y) => c.Spawn(x, y, "start", 1);
            r.legend['r'] = (c, x, y) => c.Spawn(x, y, "from_mix", -1);
            r.legend['u'] = (c, x, y) => c.Spawn(x, y, "from_greenhouse", -1);
            r.legend['T'] = (c, x, y) => c.Door(x, y, "M14", "from_lab", "약품 온실");
            r.legend['G'] = (c, x, y) => c.Guard(x, y, "C05_guard", "patrol_guard", x - 4f, x + 5f, 1);
            r.legend['K'] = (c, x, y) => c.Guard(x, y, "C05_carrier", "carrier_bot", x - 4f, x + 4f, -1);
            r.legend['w'] = (c, x, y) => c.ExposedWire(x, y, "C05_wire", 1);
            r.useAlarmAmbience = true;
            r.legend['B'] = (c, x, y) => c.Rest(c.Cell(x, y), Ctx.P0("Prop_Bench"), "pre_boss", "Bench_C05");
            r.layers = c =>
            {
                EWSceneBuilder.LabLayers(c, 0);
                foreach (var x in new[] { 10f, 24f, 37f })
                    c.Layer(Ctx.S("L_Chamber"), new Vector2(x, c.H - 11f + 1.7f), -42, 0.12f, new Color(0.6f, 0.85f, 0.7f), 1f, "CultureTank");
            };
            r.extra = c =>
            {
                c.BubbleZone(18, 2, 26, 10, "smell");
                c.Inspect(c.Cell(8, 10), Ctx.Named("Foozle_SciFiLab_Tileset/full_decor_tiles.png", "decor_5"), "m12_tank", "c3_lab_note", order: -3, prompt: "배양 통");
                c.Exit(0, 2, 0, 10, "C04", "from_lab");
                c.Exit(47, 2, 47, 10, "C06", "start");
                float floor = c.H - 11;
                c.Checkpoint(1, 2, 2, 10);
                c.Crate(14.25f, floor); c.Crate(19.75f, floor, "S_CrateTall"); c.Crate(29.25f, floor);
                c.Camera(new Vector2(38.5f, floor + 4.6f), "C05_cam", -165f, -50f, 0.2f);
                c.HideCover(12.2f, floor, "S_HideSacks");
                c.HideCover(25.6f, floor, "S_HideRack");
                c.Reinforce(new Vector2(46.5f, floor + 3f));
            };
            return r;
        }

        // ================================================================ 베넬 복도: 위아래 발판 없는 좁고 긴 단층 복도 (실시간 첫 만남)
        static RoomDef C06()
        {
            var g = new Grid(48, 8);
            g.Rect(0, 0, 47, 0, '#').Rect(0, 0, 0, 7, '#').Rect(47, 0, 47, 7, '#').Rect(0, 7, 47, 7, '#');
            g.Rect(0, 3, 0, 6, '.').Rect(47, 3, 47, 6, '.');
            var r = R("C06", "C06_LongCorridor", "연결 복도", Theme.Lab, g.Rows());
            r.useAlarmAmbience = true;
            r.layers = c => EWSceneBuilder.LabLayers(c, 0);
            r.extra = c =>
            {
                float floor = c.H - 7;
                c.Spawn(2, 6, "start", 1);
                c.Spawn(45, 6, "from_mix", -1);
                c.Mark("lu_enter", new Vector2(1.2f, floor));
                c.Mark("lu_lean", new Vector2(6.2f, floor));
                c.Mark("venelle_start", new Vector2(19.1f, floor));
                var entry=c.Frames(new Vector2(19.1f,floor),Ctx.SF("L_Door"),-2,"VenelleEntrance");
                c.Mark("venelle_door",Vector2.zero,entry.gameObject);
                c.Mark("venelle_stop", new Vector2(14.6f, floor));
                // 루 뒤의 문: 들어올 땐 열려 있고, 베넬이 루를 알아보는 순간 '쾅' 닫힌다 (뒤로 도망칠 수 없음)
                var bdoor = c.Frames(new Vector2(3.4f, floor), Ctx.SF("L_Door"), 2, "BackDoor", ("c6_door_open", 1));
                c.Mark("back_door", Vector2.zero, bdoor.gameObject);
                // Entrance door is scenery only: never obstruct post-fight passage.
                c.Mark("v_alarm", new Vector2(8f, floor + 5.4f));
                foreach (var x in new[] { 8f, 24f, 40f }) c.Alarm(new Vector2(x, floor + 5.4f));
                foreach (var x in new[] { 12f, 30f }) c.WorkLamp(new Vector2(x, floor + 5.6f), "lab_power", 2.6f);
                // 베넬 (등장 연출 → 실시간 만남). 결말 뒤에는 숨긴다.
                var v = c.ActorEx(new Vector2(44f, floor), "venelle", "venelle", -1, null, null, Vector2.zero);
                v.hideFlag = "c3_boss_done";
                var boss = v.gameObject.AddComponent<VenelleBoss>();
                boss.corridorMin = 4.6f; boss.corridorMax = 42.4f;
                // 3단계 위험 장치: 과부하 분전함 (벽)
                var jgo = c.Obj("Junction", new Vector2(24.5f, floor + 0.6f));
                c.Sprite(jgo, Ctx.S("S_Junction"), 2);
                var jh = jgo.AddComponent<JunctionHazard>(); jh.id = "C06_junction"; jh.displayName = "과부하 분전함"; jh.size = new Vector2(0.8f, 0.9f); jh.aimOffset = new Vector2(0, 0.45f);
                boss.junction = jgo.transform;
                c.Exit(0, 3, 0, 6, "C05", "from_mix", blockFlag: "c6_combat_locked", blockedDialogue: "sys_locked");
                c.Exit(47, 3, 47, 6, "M13", "start", requireFlag: "c3_boss_done", blockedDialogue: "sys_locked");
            };
            return r;
        }

        static RoomDef M13()
        {
            var r = R("M13", "M13_MixingRoom", "혼합실", Theme.Lab,
                "##############################",
                "#............................#",
                "#............................#",
                "#............................#",
                "#............................#",
                "#..................====......#",
                "#............................#",
                "#............................#",
                "#............................#",
                "#............................#",
                "#.s..k..Q..2.....p.V......r..#",
                "##############################");
            Open(r, 0, 1, 10);
            Open(r, 29, 1, 10);
            r.legend['s'] = (c, x, y) => c.Spawn(x, y, "start", 1);
            r.legend['r'] = (c, x, y) => c.Spawn(x, y, "from_rest", -1);
            r.legend['Q'] = (c, x, y) => c.Inspect(c.Cell(x, y), Ctx.Named("Foozle_SciFiLab_Tileset/full_decor_tiles.png", "decor_1"), "m13_shelf", "c3_chem_shelf", order: -3, prompt: "약품 선반");
            r.legend['p'] = (c, x, y) => c.Puddle(x, y, 5, "M13_puddle");
            r.legend['V'] = (c, x, y) => c.Inspect(c.Cell(x, y), Ctx.TP("Foozle_SciFiLab_Pickups/chemical_pot_item.png").FirstOrDefault(), "m13_mix", "c3_chem_shelf", order: -3, prompt: "혼합 장치");
            r.layers = c =>
            {
                EWSceneBuilder.LabLayers(c, 1);
                // 약품 통이 늘어선 높은 단 (베넬이 처음 서 있는 곳)
                foreach (var x in new[] { 17f, 23.5f })
                    c.Layer(Ctx.S("L_Chamber"), new Vector2(x, c.H - 6f), -30, 0.05f, new Color(0.55f, 0.85f, 0.6f), 0.8f, "MixTank");
            };
            r.extra = c =>
            {
                c.BubbleZone(15, 2, 21, 10, "puddle");
                c.Exit(0, 2, 0, 10, "C06", "from_mix", blockFlag: "venelle_carried", blockedDialogue: "sys_locked");
                c.Exit(29, 2, 29, 10, "M15", "start");
                for (int i = 0; i < 3; i++)
                    c.Decor(c.Cell(22 + i, 10), Ctx.TP("Foozle_SciFiLab_Pickups/chemical_pot_item.png").FirstOrDefault(), -3);
            };
            return r;
        }

        static RoomDef M14()
        {
            var r = R("M14", "M14_ChemGreenhouse", "약품 온실", Theme.Lab,
                "############################",
                "#..........................#",
                "#..........................#",
                "#..........................#",
                "#..........................#",
                "#..........................#",
                "#..........................#",
                "#..........................#",
                "#..........................#",
                "#..........................#",
                "#Df...6.k...p...E...6.k.6..#",
                "############################");
            r.legend['D'] = (c, x, y) => c.Door(x, y, "C05", "from_greenhouse", "배양실로");
            r.legend['f'] = (c, x, y) => c.Spawn(x, y, "from_lab", 1);
            r.legend['p'] = (c, x, y) => c.Puddle(x, y, 4, "M14_puddle");
            r.legend['E'] = (c, x, y) => c.Guard(x, y, "M14_drone", "capture_drone", x - 4f, x + 4f, -1, null, true, 3.2f);
            r.extra = c => c.BubbleZone(4, 2, 10, 10, "smell");
            return r;
        }

        static RoomDef M15()
        {
            var r = R("M15", "M15_ObservationLounge", "관찰 휴게실", Theme.Lab,
                "##################################################",
                "#................................................#",
                "#................................................#",
                "#................................................#",
                "#.........................k......................#",
                "#................^==============.................#",
                "#................H...............................#",
                "#................H...............................#",
                "#................H...##########..................#",
                "#................H...............................#",
                "#.s...B...k......H...............................#",
                "####################################......########");
            Open(r, 0, 1, 10);
            Open(r, 49, 1, 10);
            r.legend['s'] = (c, x, y) => { c.Spawn(x, y, "start", 1); };
            r.legend['B'] = (c, x, y) =>
            {
                c.Rest(c.Cell(x, y), Ctx.P0("Prop_Bench"), "m15_rest", "RestBench", null);
                c.Spawn(x + 1, y, "rest_seat", -1);
                c.Actor(x, y, "venelle_rest", "venelle", 1, "venelle_bench", "c3_joined", "venelle_dead", "Sit", -0.4f);
                c.Decor(c.Cell(x, y) + new Vector2(0.6f, 0.4f), Ctx.P0("Prop_Water"), 3, 1, "Water");
            };
            r.extra = c =>
            {
                c.BubbleZone(5, 2, 8, 10, "bench");
                c.BubbleZone(13, 2, 15, 10, "fork");
                c.BubbleZone(16, 6, 18, 10, "ladder");
                c.BubbleZone(19, 9, 21, 10, "low_ceiling");
                c.PowerLinePair("M15_line", 34, 10, 42, 10, new[] { c.Cell(35, 5), c.Cell(41, 5) });
                c.Inspect(c.Cell(45, 10), Ctx.P0("Prop_Door"), "m15_lift_sign", "c3_lift_sign", order: -3, prompt: "표지판");
                // 2026-11-15 (4장 연결): 시연 종료 대신 실제 출구 → 화물 승강장. 돌아올 때 도착 지점.
                c.Exit(49, 1, 49, 10, "M16", "start", requireFlag: "c3_boss_done", blockedDialogue: "sys_locked");
                c.Spawn(47, 10, "from_lift", -1);
                c.EventZone(33, 1, 34, 10, "m15_gap_look");
                c.Exit(0, 2, 0, 10, "M13", "from_rest");
            };
            return r;
        }
    }
}
