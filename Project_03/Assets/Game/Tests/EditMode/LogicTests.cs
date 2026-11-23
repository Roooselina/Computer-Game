using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace EW.Tests
{
    static class T
    {
        public static GameDatabase Db() => DefaultContent.Create();
        public static GameState State(GameDatabase db = null) => new GameState(SaveData.NewGame(), db ?? Db());
    }

    public class RewardAndRetryTests
    {

        [Test]
        public void Retry_RestoresConsumables_ExpAndDevices_ButKeepsSeenHistory()
        {
            var gs = T.State();
            gs.AddItem("snack", 2);
            gs.StoreDevice("M05_terminal", DevicePower.On, false, 100);
            var snap = gs.D.Clone();
            // 전투 중: 과자 사용, 장치 꺼짐, 대사 출력
            gs.UseItem("snack");
            gs.StoreDevice("M05_terminal", DevicePower.Off, false, 100);
            gs.D.dialogueSeen.Add("c1_wire_first");
            gs.D.bubblesSeen.Add("b_v_first");
            gs.GainExp("lu", 10);
            gs.RestoreForRetry(snap);
            Assert.That(gs.Count("snack"), Is.EqualTo(2));
            Assert.That(gs.D.lu.exp, Is.EqualTo(0));
            Assert.That((DevicePower)gs.GetDevice("M05_terminal").power, Is.EqualTo(DevicePower.On));
            Assert.That(gs.D.dialogueSeen, Contains.Item("c1_wire_first"));
            Assert.That(gs.D.bubblesSeen, Contains.Item("b_v_first"));
        }
    }

    public class PowerRuleTests
    {
        static DeviceModel D(DeviceKind k, string line = "L", bool bat = false, int lv = 100, bool fragile = false, bool essential = false)
            => new DeviceModel { id = k + line, kind = k, line = line, hasBattery = bat, battery = lv, fragile = fragile, essential = essential };

        [Test]
        public void Outage_StopsLinePowered_ButBatteryKeepsRunning()
        {
            var lamp = D(DeviceKind.Light); var emergency = D(DeviceKind.Light, bat: true, lv: 60);
            PowerRules.Outage(lamp); PowerRules.Outage(emergency);
            Assert.That(lamp.power, Is.EqualTo(DevicePower.Off));
            Assert.That(emergency.power, Is.EqualTo(DevicePower.On));
        }

        [Test]
        public void Discharge_DrainsBattery_ThenStops()
        {
            var m = D(DeviceKind.Machine, bat: true, lv: 40);
            PowerRules.Discharge(m);
            Assert.That(m.power, Is.EqualTo(DevicePower.On)); Assert.That(m.battery, Is.EqualTo(20));
            PowerRules.Discharge(m);
            Assert.That(m.battery, Is.EqualTo(0)); Assert.That(m.power, Is.EqualTo(DevicePower.Off));
        }

        [Test]
        public void Discharge_BreaksFragile_EssentialOnlyTurnsOff()
        {
            var sensor = D(DeviceKind.Sensor, fragile: true, bat: true);
            var essential = D(DeviceKind.Terminal, fragile: true, essential: true);
            PowerRules.Discharge(sensor); PowerRules.Discharge(essential);
            Assert.That(sensor.power, Is.EqualTo(DevicePower.Broken), "직접 손상은 배터리가 있어도 고장");
            Assert.That(essential.power, Is.EqualTo(DevicePower.Off), "필수 장치는 영구 고장 나지 않는다");
        }

        [Test]
        public void Restore_TurnsOnOnlyOffDevicesOnLine_NotBroken()
        {
            var a = D(DeviceKind.Light, "A"); a.power = DevicePower.Off;
            var b = D(DeviceKind.Sensor, "A"); b.power = DevicePower.Broken;
            var c = D(DeviceKind.Light, "B"); c.power = DevicePower.Off;
            int n = PowerRules.RestoreLine(new[] { a, b, c }, "A");
            Assert.That(n, Is.EqualTo(1));
            Assert.That(a.power, Is.EqualTo(DevicePower.On));
            Assert.That(b.power, Is.EqualTo(DevicePower.Broken));
            Assert.That(c.power, Is.EqualTo(DevicePower.Off));
            Assert.That(PowerRules.SetSwitch(b, true), Is.False, "스위치로 고장을 고칠 수 없다");
        }

        [Test]
        public void Door_NotOperable_WhenControllerOff()
        {
            var term = D(DeviceKind.Terminal); term.id = "T";
            var door = D(DeviceKind.Door); door.controllerId = "T";
            System.Func<string, DeviceModel> find = id => id == "T" ? term : null;
            Assert.That(PowerRules.Operable(door, find), Is.True);
            term.power = DevicePower.Off;
            Assert.That(PowerRules.Operable(door, find), Is.False);
            PowerRules.RestoreLine(new[] { term }, term.line);
            Assert.That(PowerRules.Operable(door, find), Is.True, "복구 버튼으로 다시 동작 (필수 동선 복구)");
        }

        [Test]
        public void ComputerOff_DescriptionSaysFilesRemain()
        {
            var pc = D(DeviceKind.Computer); pc.power = DevicePower.Off;
            StringAssert.Contains("저장된 파일은 그대로", PowerRules.Describe(pc));
        }
    }

    public class SaveTests
    {
        string dir;
        [SetUp] public void Setup() { dir = Path.Combine(Path.GetTempPath(), "ew_save_test_" + System.Guid.NewGuid().ToString("N")); Directory.CreateDirectory(dir); SaveSystem.OverrideDirectory = dir; }
        [TearDown] public void Teardown() { SaveSystem.OverrideDirectory = null; try { Directory.Delete(dir, true); } catch { } }

        [Test]
        public void RoundTrip_KeepsAllProgress()
        {
            var gs = T.State();
            gs.Set("c3_boss_done"); gs.AddItem("snack", 4); gs.D.lu.level = 3; gs.D.lu.exp = 90; gs.D.lu.hp = 21; gs.D.lu.charge = 5;
            gs.D.venelleState = "ally"; gs.D.venelleTrust = 1; gs.StoreDevice("M05_terminal", DevicePower.Off, false, 100);
            gs.D.dialogueSeen.Add("c3_rest"); gs.D.bubblesSeen.Add("b_v_first"); gs.D.docs.Add("D01"); gs.D.clearedEncounters.Add("M05_e1");
            gs.D.mapId = "M15"; gs.D.hasPos = true; gs.D.posX = 12.5f; gs.D.posY = 1f;
            Assert.That(SaveSystem.Write(gs.D, out _), Is.True);
            Assert.That(SaveSystem.TryLoad(out var d, out _), Is.EqualTo(SaveSystem.LoadResult.Ok));
            Assert.That(d.flags, Contains.Item("c3_boss_done"));
            Assert.That(d.items.First(i => i.k == "snack").v, Is.EqualTo(4));
            Assert.That(d.lu.level, Is.EqualTo(3)); Assert.That(d.lu.exp, Is.EqualTo(90)); Assert.That(d.lu.hp, Is.EqualTo(21)); Assert.That(d.lu.charge, Is.EqualTo(5));
            Assert.That(d.venelleState, Is.EqualTo("ally"));
            Assert.That(d.devices.First(x => x.id == "M05_terminal").power, Is.EqualTo((int)DevicePower.Off));
            Assert.That(d.dialogueSeen, Contains.Item("c3_rest")); Assert.That(d.clearedEncounters, Contains.Item("M05_e1"));
            Assert.That(d.mapId, Is.EqualTo("M15")); Assert.That(d.posX, Is.EqualTo(12.5f));
            Assert.That(d.version, Is.EqualTo(SaveSystem.CurrentVersion));
        }

        [Test]
        public void CorruptMain_RecoversFromBackup()
        {
            var gs = T.State(); gs.Set("first");
            SaveSystem.Write(gs.D, out _);
            gs.Set("second");
            SaveSystem.Write(gs.D, out _);   // 첫 저장이 .bak 으로
            File.WriteAllText(SaveSystem.MainPath, "{ broken json");
            var r = SaveSystem.TryLoad(out var d, out var msg);
            Assert.That(r, Is.EqualTo(SaveSystem.LoadResult.RecoveredFromBackup));
            Assert.That(d.flags, Contains.Item("first"));
            Assert.That(msg, Is.Not.Empty);
        }

        [Test]
        public void BothCorrupt_ReportsCorrupt_NoException()
        {
            File.WriteAllText(SaveSystem.MainPath, "garbage");
            File.WriteAllText(SaveSystem.BackupPath, "");
            Assert.That(SaveSystem.TryLoad(out var d, out var msg), Is.EqualTo(SaveSystem.LoadResult.Corrupt));
            Assert.That(d, Is.Null);
            StringAssert.Contains("손상", msg);
        }

        [Test]
        public void MissingVersion_IsRejected_TooNew_IsRefused()
        {
            File.WriteAllText(SaveSystem.MainPath, "{\"mapId\":\"M05\"}");
            Assert.That(SaveSystem.TryLoad(out _, out _), Is.EqualTo(SaveSystem.LoadResult.Corrupt));
            File.WriteAllText(SaveSystem.MainPath, "{\"version\":99,\"mapId\":\"M05\"}");
            Assert.That(SaveSystem.TryLoad(out _, out _), Is.EqualTo(SaveSystem.LoadResult.TooNew));
        }

        [Test]
        public void PartialData_IsSanitized()
        {
            File.WriteAllText(SaveSystem.MainPath, "{\"version\":1,\"mapId\":\"\",\"flags\":null,\"venelleTrust\":9,\"hasPos\":true,\"posX\":\"NaN\"}");
            Assert.That(SaveSystem.TryLoad(out var d, out _), Is.EqualTo(SaveSystem.LoadResult.Ok));
            Assert.That(d.mapId, Is.EqualTo("P01"));
            Assert.That(d.flags, Is.Not.Null);
            Assert.That(d.venelleTrust, Is.EqualTo(3));
            Assert.That(d.items, Is.Not.Null);
        }

        [Test]
        public void NoSave_ReportsNoSave()
        {
            Assert.That(SaveSystem.HasAnySave, Is.False);
            Assert.That(SaveSystem.TryLoad(out _, out _), Is.EqualTo(SaveSystem.LoadResult.NoSave));
        }
    }

    public class GameStateTests
    {
        [Test]
        public void Once_PreventsDuplicateEvents()
        {
            var gs = T.State();
            Assert.That(gs.Once("ev"), Is.True);
            Assert.That(gs.Once("ev"), Is.False);
        }

        [Test]
        public void HealAll_RestoresAllyAndCharge()
        {
            var gs = T.State(); gs.D.venelleState = "ally";
            gs.D.lu.hp = 1; gs.D.lu.charge = 0; gs.D.venelle.hp = 1;
            gs.HealAll();
            Assert.That(gs.D.lu.hp, Is.EqualTo(gs.MaxHp("lu")));
            Assert.That(gs.D.lu.charge, Is.EqualTo(gs.MaxCharge("lu")));
            Assert.That(gs.D.venelle.hp, Is.EqualTo(gs.MaxHp("venelle")));
        }

        [Test]
        public void Items_ClampAndUse()
        {
            var gs = T.State();
            gs.AddItem("snack", 200);
            Assert.That(gs.Count("snack"), Is.EqualTo(99));
            Assert.That(gs.UseItem("snack"), Is.True);
            Assert.That(gs.UseItem("bandage"), Is.False);
        }
    }

    public class DataIntegrityTests
    {
        const string JsonPath = "Assets/Game/Data/Dialogue/dialogue_ko.json";

        [Test]
        public void DialogueJson_ParsesWithoutErrors()
        {
            var db = DialogueDB.Parse(File.ReadAllText(JsonPath));
            Assert.That(db.errors, Is.Empty, string.Join("\n", db.errors));
            Assert.That(db.entries.Count, Is.GreaterThan(50));
        }

        [Test]
        public void EveryReferencedDialogueId_Exists()
        {
            var db = DialogueDB.Parse(File.ReadAllText(JsonPath));
            var used = new HashSet<string>();
            string[] patterns =
            {
                @"(?:Say|Ask)\(""([a-z0-9_]+)""",
                @"SayOnce\(g, ""([a-z0-9_]+)""",
                @"speakerLine = ""([a-z0-9_]+)""",
                @"dialogueId = ""([a-z0-9_]+)""",
                @"blockedDialogue: ""([a-z0-9_]+)""",
                @"DeviceInspect\(\w+, ""([a-z0-9_]+)""",
                @"Inspect\([^;\n]*?, ""[a-z0-9_]+"", ""([a-z0-9_]+)""",
                @"(?:Say|Ask)\([^;\n]*\? ""([a-z0-9_]+)"" : ""[a-z0-9_]+""",
                @"(?:Say|Ask)\([^;\n]*\? ""[a-z0-9_]+"" : ""([a-z0-9_]+)""",
            };
            foreach (var f in Directory.GetFiles("Assets/Game/Scripts", "*.cs", SearchOption.AllDirectories))
            {
                var t = File.ReadAllText(f);
                foreach (var p in patterns)
                    foreach (Match m in Regex.Matches(t, p)) used.Add(m.Groups[1].Value);
            }
            Assert.That(used.Count, Is.GreaterThan(40), "검사 대상 수집 실패");
            // 소리 이름(sfx_/amb_/bgm_)은 대사 ID 가 아니다
            var missing = used.Where(u => !db.entries.ContainsKey(u) && !u.StartsWith("sfx_") && !u.StartsWith("amb_") && !u.StartsWith("bgm_")).ToList();
            Assert.That(missing, Is.Empty, "없는 대사 ID: " + string.Join(", ", missing));
        }

        [Test]
        public void NamesFollowGdd_NoForbiddenSetup()
        {
            string json = File.ReadAllText(JsonPath);
            // 오프닝 집 안에 모집 전단지를 넣지 않는다 (GDD 6 프롤로그), 게임 밖 관리자·신·플레이어 언급 없음
            var db = DialogueDB.Parse(json);
            foreach (var e in db.entries.Values.Where(e => e.id.StartsWith("op_")))
                foreach (var l in e.lines)
                {
                    StringAssert.DoesNotContain("전단", l.t);
                    StringAssert.DoesNotContain("플레이어", l.t);
                    StringAssert.DoesNotContain("관리자", l.t);
                }
            // 사용자 지정 오프닝 대사 원문 유지
            Assert.That(db.Get("op_v_2").lines[0].t, Is.EqualTo("다... 끝난 거야...?"));
            Assert.That(db.Get("op_h_3").lines[0].t, Is.EqualTo("난 이제 어떻게 살아야 해..?"));
            Assert.That(db.Get("op_t_stop").lines[0].t, Is.EqualTo("오지 말랬잖아!"));
            Assert.That(db.SpeakerName("researcher1"), Is.EqualTo("연구원 1"));
            // 화자 이름 고정
            Assert.That(db.SpeakerName("venelle"), Is.EqualTo("베넬"));
            Assert.That(db.SpeakerName("lu"), Is.EqualTo("루"));
        }
    }

    public class OverhaulTests
    {
        [Test]
        public void SaveV1_MigratesOldMapIds_AndOpeningFlags()
        {
            var d = SaveData.NewGame(); d.version = 1; d.mapId = "M05"; d.hasPos = true; d.flags.Add("c1_tank_broken"); d.clearedEncounters.Add("M05_e1");
            SaveSystem.Migrate(d);
            Assert.That(d.version, Is.EqualTo(SaveSystem.CurrentVersion));
            Assert.That(d.mapId, Is.EqualTo("C02")); Assert.That(d.hasPos, Is.False, "옛 좌표는 버린다");
            foreach (var f in new[] { "op_village_done", "op_house_done", "op_desktop_done", "op_tank_done", "c1_researchers_down", "c1_shortcut_open" }) Assert.That(d.flags, Contains.Item(f), f);
            var p = SaveData.NewGame(); p.version = 1; p.mapId = "M01";
            SaveSystem.Migrate(p);
            Assert.That(p.mapId, Is.EqualTo("P01")); Assert.That(p.flags, Does.Not.Contain("op_village_done"), "옛 프롤로그는 새 오프닝 처음부터");
            var t = SaveData.NewGame(); t.version = 1; t.mapId = "M04";
            SaveSystem.Migrate(t);
            Assert.That(t.mapId, Is.EqualTo("P03"), "방전 전 실험 통 저장은 바탕화면 장면부터");
        }

        [Test]
        public void PortraitReferences_HaveImages_AndOpeningVillageShowsNoFace()
        {
            var db = DialogueDB.Parse(File.ReadAllText("Assets/Game/Data/Dialogue/dialogue_ko.json"));
            foreach (var e in db.entries.Values)
                foreach (var l in e.lines)
                {
                    if (string.IsNullOrEmpty(l.e)) continue;
                    string who = string.IsNullOrEmpty(l.p) ? l.s : l.p;
                    Assert.That(File.Exists("Assets/Game/Art/Portraits/" + who + "_" + l.e + ".png"), "스탠딩 그림 없음: " + who + "_" + l.e + " (" + e.id + ")");
                }
            foreach (var id in new[] { "op_v_1", "op_v_2", "op_v_3", "op_v_4" })
                Assert.That(db.Get(id).lines.All(l => string.IsNullOrEmpty(l.e)), id + ": 마을 장면은 얼굴을 바로 보여 주지 않는다");
        }

        [Test]
        public void NoChapterTitleCard_InStoryCode()
        {
            foreach (var f in Directory.GetFiles("Assets/Game/Scripts/Runtime", "*.cs", SearchOption.AllDirectories))
            {
                var t = File.ReadAllText(f);
                StringAssert.DoesNotContain("ShowTitleCard", t, f + ": 장 번호 전체 화면 안내 금지");
            }
        }

        [Test]
        public void HitStop_DoesNotBlockThread()
        {
            foreach (var f in Directory.GetFiles("Assets/Game/Scripts/Runtime", "*.cs", SearchOption.AllDirectories))
                StringAssert.DoesNotContain("Thread.Sleep", File.ReadAllText(f), f);
        }
    }
}
