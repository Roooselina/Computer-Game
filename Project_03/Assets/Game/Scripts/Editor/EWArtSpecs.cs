using System.Collections.Generic;
using UnityEngine;

namespace EW.EditorTools
{
    /// <summary>
    /// 스프라이트 시트 분할 규격. 실제 파일의 픽셀 크기를 확인한 값이다 (Docs/ASSET_INVENTORY.md).
    /// 모든 시트를 32×32로 자르지 않는다: 시트마다 프레임 크기·피벗이 다르다.
    /// </summary>
    public class SheetSpec
    {
        public string path;
        public int frameW, frameH;        // 0 이면 이미지 전체가 한 프레임
        public Vector2 pivot = new Vector2(0.5f, 0f);
        public float ppu = 32;
        public bool fullRect;             // 타일 반복 그림
        public Vector4 border;            // 9-slice (L,B,R,T)
        public List<NamedRect> rects;     // 직접 지정한 영역
    }

    public class NamedRect
    {
        public string name; public Rect rect; public Vector4 border; public Vector2 pivot = new Vector2(0.5f, 0.5f);
        public NamedRect(string n, Rect r, Vector4 b = default) { name = n; rect = r; border = b; }
    }

    public static class EWArtSpecs
    {
        public const string Placeholder = "Assets/Game/Art/Placeholder/";
        public const string TP = "Assets/ThirdParty/";

        static readonly Vector2 Bottom = new Vector2(0.5f, 0f);
        static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        /// <summary>임시 스프라이트: 파일 이름 접두어로 프레임 크기를 정한다 (Tools/placeholder_gen.py 와 일치).</summary>
        public static SheetSpec ForPlaceholder(string file, int texW, int texH)
        {
            string n = System.IO.Path.GetFileNameWithoutExtension(file);
            var s = new SheetSpec { path = file, pivot = Bottom };
            if (n.StartsWith("Lu_") || n.StartsWith("Venelle_") || n.StartsWith("Researcher_") || n.StartsWith("Researcher2_")
                || n.StartsWith("F_") || n.StartsWith("Kid008_") || n.StartsWith("SurvivorA_") || n.StartsWith("SurvivorB_") || n.StartsWith("Ivy_") || n.StartsWith("Linden_") || n.StartsWith("Adult_")
                || n.StartsWith("Raviel_") || n.StartsWith("Meilin_") || n.StartsWith("Lacrimosa_") || n.StartsWith("Edward_") || n.StartsWith("EdwardKid_") || n.StartsWith("MemberA_") || n.StartsWith("MemberB_") || n.StartsWith("Warden_")) { s.frameW = 48; s.frameH = 64; }
            else if (n.StartsWith("RavielBeast_")) { s.frameW = 80; s.frameH = 96; }
            else if (n.StartsWith("Aurelius_")) { s.frameW = 64; s.frameH = 96; }
            // ---- 장면 블록아웃 (Tools/scene_art_gen.py)
            else if (n == "FX_Fire") { s.frameW = 16; s.frameH = 24; }
            else if (n == "FX_FireBig") { s.frameW = 32; s.frameH = 48; }
            else if (n == "FX_Smoke") { s.frameW = 32; s.frameH = 32; s.pivot = Center; }
            else if (n == "FX_Glow" || n == "FX_GlowSoft" || n == "FX_Ember" || n == "FX_Ash" || n == "FX_Dust") { s.pivot = Center; }
            else if (n == "FX_HazeBand" || n == "FX_FadeDown") { s.pivot = Center; s.fullRect = true; }
            else if (n == "H_PC") { s.frameW = 40; s.frameH = 32; }
            else if (n == "L_TankBack" || n == "L_TankGlass") { s.frameW = 80; s.frameH = 112; }
            else if (n == "L_Door") { s.frameW = 48; s.frameH = 80; }
            else if (n == "L_Console") { s.frameW = 48; s.frameH = 40; }
            else if (n == "D_Wallpaper") { s.pivot = Center; s.fullRect = true; }
            // 바닥·벽 채움 타일: 중앙 피벗 (BuildSolids 가 칸 영역 중앙에 둔다)
            else if (n == "V_Ground" || n == "V_GroundFill" || n == "H_Wall" || n == "H_Floor" || n == "D_Bar" || n == "L_Floor" || n == "L_FloorFill" || n == "L_WallPanel") { s.fullRect = true; s.pivot = Center; }
            // 가로 반복 레이어: 아래 가운데 피벗
            else if (n == "V_Sky" || n == "V_Far" || n == "L_Far" || n == "L_MidA" || n == "L_MidB" || n == "L_ForePipe" || n == "V_ForeRubble") { s.fullRect = true; }
            else if (n == "FX_Bolt") { s.frameW = 16; s.frameH = 8; s.pivot = Center; }
            else if (n == "FX_AcidFan") { s.frameW = 64; s.frameH = 32; s.pivot = new Vector2(0f, 0.5f); }
            else if (n == "FX_PuddleAcid") { s.frameW = 64; s.frameH = 12; s.pivot = Bottom; }
            else if (n.StartsWith("UI_")) { s.pivot = Center; }
            else if (n == "FX_Arc") { s.frameW = 64; s.frameH = 32; s.pivot = Center; }
            else if (n == "FX_LightDot") { s.frameW = 16; s.frameH = 16; s.pivot = Center; }
            else if (n.StartsWith("FX_")) { s.frameW = 32; s.frameH = 32; s.pivot = Center; }
            else if (n == "Prop_Switch" || n == "Prop_RestoreButton" || n == "Prop_Lever") { s.frameW = 16; s.frameH = 32; }
            else if (n == "Prop_Door" || n == "Prop_Vending") { s.frameW = 32; s.frameH = 64; }
            else if (n == "Prop_Tank") { s.frameW = 48; s.frameH = 64; }
            else if (n == "Prop_CeilingLight") { s.frameW = 32; s.frameH = 8; s.pivot = new Vector2(0.5f, 1f); }
            else if (n == "Prop_EmergencyLight") { s.frameW = 16; s.frameH = 16; }
            else if (n == "Prop_PowerSocket" || n == "Prop_ExposedWire" || n == "Prop_Computer") { s.frameW = 32; s.frameH = 32; }
            else if (n.StartsWith("Tile_")) { s.fullRect = true; s.pivot = Center; }
            // ---- 4~6장 소품 (Tools/ch4_6_art_gen.py)
            // 7~11장 (Tools/ch7_11_art_gen.py)
            else if (n == "U_CellBars") { s.frameW = 40; s.frameH = 72; }
            else if (n == "U_CellPanel") { s.frameW = 20; s.frameH = 36; }
            else if (n == "U_CallBoard") { s.frameW = 40; s.frameH = 20; s.pivot = Center; }
            else if (n == "U_Claw") { s.frameW = 32; s.frameH = 48; s.pivot = new Vector2(0.5f, 1f); }
            else if (n == "U_MaintTerm") { s.frameW = 28; s.frameH = 36; }
            else if (n == "U_ConductFloor" || n == "U_ClawRail") { s.fullRect = true; s.pivot = Center; }
            else if (n == "U_Brake") { s.frameW = 20; s.frameH = 30; }
            else if (n == "U_Monitor") { s.frameW = 48; s.frameH = 36; }
            else if (n == "U_Bulkhead") { s.frameW = 36; s.frameH = 96; }
            else if (n == "U_Cups") { s.frameW = 24; s.frameH = 12; }
            else if (n == "U_Mic") { s.frameW = 20; s.frameH = 40; }
            else if (n == "U_Speaker") { s.frameW = 32; s.frameH = 48; }
            else if (n == "U_Mixer") { s.frameW = 32; s.frameH = 24; }
            else if (n == "U_ServerRack") { s.frameW = 28; s.frameH = 64; }
            else if (n == "U_CaptureTerm") { s.frameW = 48; s.frameH = 72; }
            else if (n == "U_Capsule") { s.frameW = 40; s.frameH = 76; }
            else if (n == "U_Escort") { s.frameW = 20; s.frameH = 20; s.pivot = Center; }
            else if (n == "U_Capacitor") { s.frameW = 28; s.frameH = 52; }
            else if (n == "U_Coupling") { s.frameW = 72; s.frameH = 72; s.pivot = Center; }
            else if (n == "U_Recovery") { s.frameW = 40; s.frameH = 36; }
            else if (n == "U_LiftPanel") { s.frameW = 16; s.frameH = 32; }
            else if (n == "U_Stove") { s.frameW = 24; s.frameH = 30; }
            else if (n == "U_Jug") { s.frameW = 16; s.frameH = 24; }
            else if (n == "U_Pump") { s.frameW = 64; s.frameH = 48; }
            else if (n == "U_Valve") { s.frameW = 24; s.frameH = 32; }
            else if (n == "U_Breaker") { s.frameW = 24; s.frameH = 36; }
            else if (n == "U_Lantern") { s.frameW = 12; s.frameH = 20; }
            else if (n == "U_ChalkLines") { s.frameW = 48; s.frameH = 10; }
            else if (n == "U_BeltCtrl") { s.frameW = 24; s.frameH = 32; }
            else if (n == "U_Washer") { s.frameW = 28; s.frameH = 32; }
            else if (n == "U_Belt" || n == "U_WaterTop") { s.frameW = n == "U_Belt" ? 32 : 32; s.frameH = n == "U_Belt" ? 10 : 6; s.fullRect = true; s.pivot = Center; }
            else if (n == "U_WaterFill" || n == "U_PipeH") { s.fullRect = true; s.pivot = Center; }
            // ---- 실시간 잠입 소품 (Tools/stealth_art_gen.py)
            else if (n == "S_CctvHead") { s.pivot = new Vector2(0.12f, 0.5f); }
            else if (n == "S_CctvMount" || n == "S_WorkLamp" || n == "S_SurgLamp" || n == "S_Tray") { s.pivot = new Vector2(0.5f, 1f); }
            else if (n == "S_Lens" || n == "S_LensOff" || n == "S_Lamp" || n == "S_CtrlScreen" || n.StartsWith("S_IconPower") || n == "S_AlarmLamp") { s.pivot = Center; }
            else if (n == "S_AlarmBeam") { s.pivot = new Vector2(0f, 0.5f); }
            return s;
        }

        public static List<SheetSpec> ThirdParty()
        {
            var l = new List<SheetSpec>();
            // Foozle Sci-fi Lab Droids: 프레임 = 시트 높이(정사각형). Droid01 은 32/48 프레임이 섞여 있어 중앙 피벗.
            foreach (var f in new[] { "Droid01Idle", "Droid01Move", "Droid01Shoot", "Droid01Hurt", "Droid01Death", "Droind01SelfDestruct" })
                l.Add(new SheetSpec { path = TP + "Foozle_SciFiLab_Droids/Droid01/" + f + ".png", frameW = -1, frameH = -1, pivot = Center });
            foreach (var f in new[] { "Droid02Idle", "Droid02Move", "Droid02Attack", "Droid02Hurt", "Droid02Death", "Droid02CountDown" })
                l.Add(new SheetSpec { path = TP + "Foozle_SciFiLab_Droids/Droid02/" + f + ".png", frameW = -1, frameH = -1, pivot = Center });
            foreach (var f in new[] { "Droid03Idle", "Droid3Move", "Droid03Attack", "Droid03Hurt", "Droid03Death", "Droid03Countdown" })
                l.Add(new SheetSpec { path = TP + "Foozle_SciFiLab_Droids/Droid03/" + f + ".png", frameW = -1, frameH = -1, pivot = Bottom });
            foreach (var f in new[] { "Droid01Projectile", "Droid01ImpactEffect", "Droid02Projectile", "Droid02Impact", "Droid03Bullet", "Droid03Impact" })
            {
                string dir = f.Contains("01") ? "Droid01" : f.Contains("02") ? "Droid02" : "Droid03";
                l.Add(new SheetSpec { path = TP + "Foozle_SciFiLab_Droids/" + dir + "/" + f + ".png", frameW = 16, frameH = 16, pivot = Center });
            }
            // Pickups: 384×32 = 32×32 × 12 프레임
            foreach (var f in new[] { "medkit_item", "chemical_pot_item", "access_card_item" })
                l.Add(new SheetSpec { path = TP + "Foozle_SciFiLab_Pickups/" + f + ".png", frameW = 32, frameH = 32, pivot = Bottom });
            // 연구동 타일셋 300×300 (32px 격자, 9×9 사용). 9-slice 블록과 배경 패널만 사용.
            l.Add(new SheetSpec
            {
                path = TP + "Foozle_SciFiLab_Tileset/level_tileset.png", fullRect = true,
                rects = new List<NamedRect>
                {
                    new NamedRect("lab_block", new Rect(0, 172, 128, 128), new Vector4(16, 16, 22, 22)),
                    new NamedRect("lab_panel", new Rect(192, 108, 96, 96)),
                    new NamedRect("lab_bricks", new Rect(128, 140, 64, 64)),
                }
            });
            // 장식 96×96 = 32×32 × 9 (위 행부터 decor_0..8)
            var decor = new SheetSpec { path = TP + "Foozle_SciFiLab_Tileset/full_decor_tiles.png", rects = new List<NamedRect>() };
            for (int r = 0; r < 3; r++) for (int c = 0; c < 3; c++)
                    decor.rects.Add(new NamedRect("decor_" + (r * 3 + c), new Rect(c * 32, 64 - r * 32, 32, 32)) { pivot = Bottom });
            l.Add(decor);
            l.Add(new SheetSpec { path = TP + "Foozle_SciFiLab_Tileset/control_panel_idle.png", frameW = 32, frameH = 32 });
            l.Add(new SheetSpec { path = TP + "Foozle_SciFiLab_Tileset/control_panel_deactivate.png", frameW = 32, frameH = 32 });
            l.Add(new SheetSpec { path = TP + "Foozle_SciFiLab_Tileset/sci_fi_mockup.png", pivot = Center, ppu = 128 });
            // Kenney UI 9-slice (UI 용 PPU 100)
            l.Add(new SheetSpec { path = TP + "Kenney_PixelUI/space.png", pivot = Center, ppu = 100, border = new Vector4(4, 4, 4, 4) });
            l.Add(new SheetSpec { path = TP + "Kenney_PixelUI/space_inlay.png", pivot = Center, ppu = 100, border = new Vector4(4, 4, 4, 4) });
            l.Add(new SheetSpec { path = TP + "Kenney_PixelUI/outline_yellow.png", pivot = Center, ppu = 100, border = new Vector4(6, 6, 6, 6) });
            // ansimuz 도시 배경 (반복 배경)
            foreach (var f in new[] { "back", "middle", "foreground" })
                l.Add(new SheetSpec { path = TP + "ansimuz_SynthCities/" + f + ".png", pivot = Bottom, fullRect = true });
            return l;
        }

        /// <summary>캐릭터 애니메이션 정의: 상태 이름 → (시트, fps, 반복).</summary>
        public class AnimDef { public string state, sheet; public float fps; public bool loop; public AnimDef(string s, string sh, float f, bool l) { state = s; sheet = sh; fps = f; loop = l; } }

        public class CharDef
        {
            public string id, name; public float height, hover; public bool flip = true; public int order = 10;
            public List<AnimDef> anims = new List<AnimDef>();
        }

        public static List<CharDef> Characters()
        {
            var l = new List<CharDef>();
            CharDef H(string id, string name, params string[] states)
            {
                var c = new CharDef { id = id, name = name, height = 1.45f };
                foreach (var s in states)
                {
                    float fps = 8; bool loop = true;
                    switch (s)
                    {
                        case "Idle": fps = 3; break;
                        case "Walk": fps = 8; break;
                        case "Climb": fps = 6; break;
                        case "Carry": case "Supported": fps = 5; break;
                        case "Charge": fps = 6; break;
                        case "Dizzy": fps = 3; break;
                        case "Flick": case "Point": case "Swing": case "Kick": case "Attack": fps = 9; loop = false; break;
                        case "Discharge": fps = 10; break;
                        // 오프닝 연출용 (임시 그림)
                        case "Limp": fps = 6; break;
                        case "LimpIdle": case "BackIdle": case "Sad": case "Recover": fps = 2; break;
                        case "ReachPC": fps = 4; loop = false; break;
                        case "Connect": fps = 6; break;
                        case "GetUp": fps = 6; loop = false; break;
                        case "LookAround": fps = 3; break;
                        case "Knock": fps = 10; break;
                        case "BackAway": fps = 6; break;
                        case "Fear": fps = 8; break;
                        case "ChargeEyes": fps = 3; loop = false; break;
                        case "Squeeze": fps = 6; break;
                        case "Talk": fps = 4; break;
                        case "BatonOut": fps = 6; loop = false; break;
                        case "BatonIdle": fps = 4; break;
                        case "BatonWalk": fps = 8; break;
                        case "Crouch": fps = 2; break;
                        case "CrouchWalk": fps = 6; break;
                        case "Dodge": fps = 1; break;
                        case "WallLean": case "Pant": case "Laugh": fps = 3; break;
                        case "Stalk": fps = 5; break;
                        case "Windup": fps = 6; break;
                        case "Dash": fps = 1; break;
                        case "TankCrouch": case "TankCower": case "EyesClosed": case "Downed": fps = 2; break;
                        case "TankStruggle": case "Lifted": fps = 7; break;
                        case "Electrify": fps = 12; break;
                        case "Disgust": case "HeadHold": fps = 3; break;
                        case "Thrown": case "HoldHand": fps = 1; break;
                        case "HairUp": case "Wind": case "SitQuiet": case "Calm": case "Hide": case "Eat": case "Draw": case "Guard": fps = 3; break;
                        case "Comb": case "Give": case "Kneel": fps = 3; loop = false; break;
                        case "Strike": fps = 1; loop = false; break;
                        case "HandsUp": case "Wave": fps = 4; break;
                        case "Share": case "Hold": case "Tag": case "Crutch": fps = 1; break;
                        case "Grab": fps = 3; break;
                        case "Throw": case "Toss": fps = 7; loop = false; break;
                        case "Spray": fps = 8; break;
                        case "Mix": fps = 6; break;
                        case "TalkWalk": fps = 6; break;
                        case "Roll": case "Type": fps = 6; break;
                        case "Snip": case "Catch": fps = 6; loop = false; break;
                        case "Sing": case "Stagger": case "Stun": case "Angry": case "Pull": case "Cheer": case "Wait": case "Injured": case "Cold": case "Limit": case "Release": fps = 4; break;
                        case "Listen": case "Bow": case "Write": case "Lie": case "HandHold": case "Cup": fps = 2; break;
                    }
                    c.anims.Add(new AnimDef(s, Placeholder + id.Substring(0, 1).ToUpper() + id.Substring(1) + "_" + s + ".png", fps, loop));
                }
                return c;
            }
            l.Add(H("lu", "루", "Idle", "Walk", "Jump", "Fall", "Climb", "Hurt", "Down", "Sit", "Flick", "Point", "Swing", "Kick", "Charge", "Discharge", "Carry",
                "BackIdle", "BackLook", "Limp", "LimpIdle", "Sad", "ReachPC", "Connect", "Land", "GetUp", "LookAround", "Knock", "BackAway", "Fear", "ChargeEyes", "Recover", "Squeeze", "Crouch", "CrouchWalk", "Dodge", "WallLean",
                "TankCrouch", "TankStruggle", "TankCower", "EyesClosed", "Electrify", "Disgust", "HeadHold", "Lifted", "Thrown", "HoldHand",
                "HairUp", "Share", "HandsUp", "Wind", "Cold", "Limit", "Lie", "Kneel", "HandHold", "Release"));
            l.Add(H("venelle", "베넬", "Idle", "Walk", "Jump", "Fall", "Climb", "Hurt", "Down", "Sit", "Attack", "Collapsed", "Supported", "Dizzy", "Stalk", "Windup", "Dash", "Pant", "Laugh",
                "Grab", "Throw", "Spray", "Mix", "Toss", "Downed", "TalkWalk", "Comb", "Wind", "SitQuiet", "Share"));
            l.Add(H("researcher", "연구원 1", "Idle", "Walk", "Hurt", "Down", "Scared", "Talk", "BatonOut", "BatonIdle", "BatonWalk"));
            l.Add(H("researcher2", "연구원 2", "Idle", "Walk", "Hurt", "Down", "Scared", "Talk", "BatonOut", "BatonIdle", "BatonWalk"));
            // 4~6장 (임시 그림: Tools/ch4_6_art_gen.py)
            l.Add(H("f", "F", "Idle", "Walk", "Hurt", "Sit", "Talk", "Point", "Toss", "Guard", "Tag"));
            l.Add(H("kid008", "008", "Idle", "Walk", "Hurt", "Sit", "Talk", "Hide", "Eat", "Hold", "Draw"));
            foreach (var sv in new[] { "survivorA", "survivorB" })
                l.Add(H(sv, "생존자", "Idle", "Walk", "Hurt", "Sit", "Scared", "Guard", "Windup", "Strike", "Toss", "Calm", "Down"));
            l.Add(H("ivy", "아이비", "Idle", "Walk", "Hurt", "Sit", "Carry", "Kneel", "Give", "Wave"));
            l.Add(H("linden", "린든", "Idle", "Walk", "Hurt", "Sit", "Hold", "Wave"));
            l.Add(H("adult", "어른", "Idle", "Walk", "Crutch"));
            // 7~11장 (임시 그림: Tools/ch7_11_art_gen.py)
            l.Add(H("raviel", "라비엘", "Idle", "Walk", "Hurt", "Sit", "Down", "Talk", "Point", "Type", "HoldHand", "Catch", "Injured", "Cup", "Sad"));
            { var b = new CharDef { id = "ravielBeast", name = "라비엘 (변신)", height = 2.6f };
              foreach (var st in new[] { "Idle", "Walk", "Windup", "Leap", "Slam", "Swipe", "Stun", "Hurt", "Hold", "Down" })
                  b.anims.Add(new AnimDef(st, Placeholder + "RavielBeast_" + st + ".png", st == "Walk" ? 7 : st == "Windup" || st == "Stun" ? 5 : 3, st != "Slam" && st != "Swipe" && st != "Leap" && st != "Hurt"));
              l.Add(b); }
            l.Add(H("meilin", "메이린", "Idle", "Walk", "Roll", "Windup", "Snip", "Dash", "Stun", "Hurt", "Talk", "Angry", "Pull", "Down"));
            l.Add(H("lacrimosa", "라크리모사", "Idle", "Walk", "Sing", "Windup", "Stagger", "Listen", "Hurt", "Bow", "Down"));
            { var a = new CharDef { id = "aurelius", name = "아우렐리우스", height = 2.4f };
              foreach (var st in new[] { "Idle", "Talk", "Windup", "Cast", "Hurt", "Broken", "Ruined", "Down" })
                  a.anims.Add(new AnimDef(st, Placeholder + "Aurelius_" + st + ".png", st == "Windup" ? 6 : 3, st != "Cast" && st != "Hurt" && st != "Down"));
              l.Add(a); }
            l.Add(H("edwardKid", "어린 에드워드", "Idle", "Hide", "Listen"));
            l.Add(H("edward", "에드워드", "Idle", "Write"));
            foreach (var mb in new[] { "memberA", "memberB" })
                l.Add(H(mb, "단원", "Idle", "Walk", "Hurt", "Sit", "Down", "Wait", "Scared", "Cheer"));
            l.Add(H("warden", "회수 경비", "Idle", "Walk", "Hurt", "Sit", "Down", "Guard", "Strike"));
            CharDef E(string id, string name, string dir, string idle, string move, string atk, string hurt, string death, float hover)
            {
                string b = TP + "Foozle_SciFiLab_Droids/" + dir + "/";
                var c = new CharDef { id = id, name = name, height = 1.0f, hover = hover, order = 9 };
                c.anims.Add(new AnimDef("Idle", b + idle + ".png", 8, true));
                c.anims.Add(new AnimDef("Move", b + move + ".png", 10, true));
                c.anims.Add(new AnimDef("Attack", b + atk + ".png", 14, false));
                c.anims.Add(new AnimDef("Hurt", b + hurt + ".png", 10, false));
                c.anims.Add(new AnimDef("Death", b + death + ".png", 12, false));
                return c;
            }
            l.Add(E("droid01", "포획 드론(임시: Foozle Droid01)", "Droid01", "Droid01Idle", "Droid01Move", "Droid01Shoot", "Droid01Hurt", "Droid01Death", 1.0f));
            l.Add(E("droid02", "운반 로봇(임시: Foozle Droid02)", "Droid02", "Droid02Idle", "Droid02Move", "Droid02Attack", "Droid02Hurt", "Droid02Death", 0.85f));
            l.Add(E("droid03", "순찰 경비(임시: Foozle Droid03)", "Droid03", "Droid03Idle", "Droid3Move", "Droid03Attack", "Droid03Hurt", "Droid03Death", 0f));
            return l;
        }
    }
}
