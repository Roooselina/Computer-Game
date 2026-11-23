using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EW.Tests
{
    /// <summary>
    /// 4~6장 (승강장 이후) 필수 검증: 실제 씬 + 실제 이벤트 코드 + 가상 입력. 대사는 자동 넘김.
    /// 자동 테스트이며 사람의 조작감·재미 판단을 대신하지 않는다. 전체 연결 경로는 Ch46Demo(실제 출구로 이동) 로 따로 확인한다.
    /// </summary>
    public class Ch46Tests
    {
        string dir;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "ew_c46_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            SaveSystem.OverrideDirectory = dir;
            GameInput.AutoAdvance = false; GameInput.AutoReadDelay = 0;
            GameInput.Virtual.ReleaseAll(); GameInput.Virtual.Skip = false;
            GameInput.ClearContexts();
            MenuList.ScriptedChoices.Clear();
            TimeCtl.ClearAll();
            Hazard.ForcePause = false;
            Guard.NoGo.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            GameInput.AutoAdvance = false; GameInput.AutoReadDelay = 0;
            GameInput.Virtual.ReleaseAll(); GameInput.Virtual.Skip = false;
            MenuList.ScriptedChoices.Clear();
            if (Game.I != null) UnityEngine.Object.DestroyImmediate(Game.I.gameObject);
            if (PixelScreen.I != null) UnityEngine.Object.DestroyImmediate(PixelScreen.I.gameObject);
            if (AudioManager.I != null) UnityEngine.Object.DestroyImmediate(AudioManager.I.gameObject);
            GameInput.ClearContexts();
            TimeCtl.ClearAll();
            Guard.NoGo.Clear();
            SaveSystem.OverrideDirectory = null;
            try { Directory.Delete(dir, true); } catch { }
        }

        static IEnumerator Until(Func<bool> cond, float timeout, string what)
        {
            float t = 0;
            while (!cond())
            {
                t += Time.unscaledDeltaTime;
                if (t > timeout) Assert.Fail("시간 초과: " + what + " | map=" + Game.I?.CurrentMapId + " ev=" + Game.I?.InEvent + " ctx=" + GameInput.Context);
                yield return null;
            }
        }
        static IEnumerator Secs(float s) { for (float t = 0; t < s; t += Time.unscaledDeltaTime) yield return null; }
        static T Find<T>() where T : UnityEngine.Object => UnityEngine.Object.FindAnyObjectByType<T>();
        Game G => Game.I;

        /// <summary>승강장 도착 시점과 같은 진행 상태 (3장 결말 뒤). venelle: ally / apart / dead.</summary>
        static void After3(GameState gs, string venelle, int snacks)
        {
            gs.D.flags.AddRange(RealtimeStealthTests.AfterAlarm);
            foreach (var f in new[] { "c3_boss_intro", "c3_boss_done", "seen_m15_lift_sign" }) gs.Set(f);
            if (venelle == "ally") { gs.D.venelleState = "ally"; gs.Set("c3_joined"); gs.Set("c3_rest_done"); gs.Set("c3_negotiated"); }
            else if (venelle == "apart") { gs.D.venelleState = "apart"; gs.Set("venelle_apart"); gs.Set("c3_rest_done"); }
            else { gs.D.venelleState = "dead"; gs.D.venelle.state = "dead"; gs.Set("venelle_dead"); gs.Set("c3_killed"); }
            gs.D.items.RemoveAll(i => i.k == "snack");
            if (snacks > 0) gs.AddItem("snack", snacks);
        }

        IEnumerator StartAt(string map, string spawn, string venelle, int snacks, Action<GameState> prep = null)
        {
            SceneManager.LoadScene("Title");
            yield return Until(() => Game.I != null, 10, "Game 생성");
            yield return null;
            var d = SaveData.NewGame();
            var gs = new GameState(d, G.Db);
            After3(gs, venelle, snacks);
            prep?.Invoke(gs);
            G.State = gs;
            G.GoToMap(map, spawn);
            yield return Until(() => G.Map != null && G.Map.mapId == map && G.Player != null && !G.Loading, 25, map + " 로드");
            yield return Until(() => !G.InEvent && !Cutscene.Running, 30, map + " 도입 끝");
            yield return null; yield return null;
            GameInput.AutoAdvance = true;
        }

        IEnumerator Ev(string id, params int[] choices)
        {
            MenuList.ScriptedChoices.Clear(); foreach (var c in choices) MenuList.ScriptedChoices.Enqueue(c);
            yield return Until(() => !G.InEvent, 20, "이전 이벤트 끝");
            G.RunEvent(StoryEvents.Run(G, id));
            yield return null;
            yield return Until(() => !G.InEvent && !Cutscene.Running, 40, id + " 끝");
            yield return null;
        }

        // ================================================================ 4장: 교섭
        [UnityTest]
        public IEnumerator Waste_GiveTenSnacks_Sufficient_DeductsOnce_NoDuplicateOnReload()
        {
            yield return StartAt("M17", "start", "apart", 12);
            int exp0 = G.State.D.lu.exp;
            yield return Ev("waste_meet", 0);
            Assert.That(G.State.Has("waste_met") && G.State.Has("waste_resolved"), Is.True);
            Assert.That(G.State.D.wasteResult, Is.EqualTo("gift"));
            Assert.That(G.State.Count("snack"), Is.EqualTo(2), "실제 소지량에서 10개만 뺀다");
            Assert.That(G.State.D.shelterGroups, Does.Contain("waste_residents"), "대피실에 모인 사람 상태 구조");
            Assert.That(G.State.D.shelterGroups, Has.No.Member("holding_released"), "아직 일어나지 않은 구출을 넣지 않는다");
            int exp1 = G.State.D.lu.exp;
            Assert.That(exp1, Is.GreaterThan(exp0));
            // 다시 만남 구역·F 대화: 다시 깎지 않고 같은 보상을 주지 않는다. F 이름 장면은 한 번.
            yield return Ev("waste_meet", 0);
            yield return Ev("waste_f");
            yield return Ev("waste_f");
            Assert.That(G.State.Count("snack"), Is.EqualTo(2));
            Assert.That(G.State.D.lu.exp, Is.EqualTo(exp1));
            Assert.That(G.State.D.dialogueSeen, Does.Contain("c4_f_name").And.Contain("c4_f_box_gift").And.Contain("c4_f_again"));
            // 저장 → 이어하기 → 같은 상태, 같은 보상 없음
            G.SaveNow("t_save", false);
            Assert.That(G.ContinueGame(out _), Is.True);
            yield return Until(() => G.CurrentMapId == "M17" && G.Player != null && !G.Loading && !G.InEvent, 20, "이어하기");
            Assert.That(G.State.Count("snack"), Is.EqualTo(2));
            Assert.That(G.State.D.wasteResult, Is.EqualTo("gift"));
            yield return Ev("waste_meet", 0);
            Assert.That(G.State.Count("snack"), Is.EqualTo(2));
            Assert.That(G.State.D.lu.exp, Is.EqualTo(exp1));
        }

        [UnityTest]
        public IEnumerator Waste_InsufficientSnacks_OptionDisabled_NeverNegative_PumpRouteInstead()
        {
            yield return StartAt("M17", "start", "ally", 4);
            // 0 (과자 10개) 은 흐린 항목이라 무시되고, 1 (펌프) 이 골라진다
            yield return Ev("waste_meet", 0, 1);
            Assert.That(G.State.Count("snack"), Is.EqualTo(4), "없는 과자를 주거나 음수로 빼지 않는다");
            Assert.That(G.State.Has("waste_pump_promised"), Is.True);
            Assert.That(G.State.Has("waste_resolved"), Is.False, "약속만으로 해결되지 않는다");
            var exit = UnityEngine.Object.FindObjectsByType<TriggerZone>().FirstOrDefault(z => z.targetMap == "M20");
            Assert.That(exit, Is.Not.Null);
            Assert.That(exit.requireFlag, Is.EqualTo("waste_resolved"), "해결 전에는 정비동으로 나가는 길이 닫혀 있다");
        }

        // ================================================================ 4장: 물 펌프
        [UnityTest]
        public IEnumerator Pump_WrongOrdersRecoverable_PartNeverLost_ThenRepairResolves_RewardOnce()
        {
            yield return StartAt("M18", "start", "apart", 3, gs => { gs.Set("waste_met"); gs.Set("waste_pump_promised"); });
            var rig = PumpRig.I; Assert.That(rig, Is.Not.Null);
            IEnumerator U(PumpRig.Role r) { G.RunEvent(rig.Use(G, r)); yield return null; yield return Until(() => !G.InEvent, 15, r.ToString()); }
            Assert.That(PumpRig.Water, Is.EqualTo(100));
            yield return U(PumpRig.Role.Breaker);
            Assert.That(PumpRig.PowerOn, Is.False, "물 위로 전원 → 차단기가 튄다");
            Assert.That(G.State.GetVar("pump_trips"), Is.EqualTo(1));
            yield return U(PumpRig.Role.Coupling);
            Assert.That(PumpRig.PartFixed, Is.False, "물에 잠긴 이음쇠는 손이 닿지 않는다");
            yield return U(PumpRig.Role.Shelf);
            Assert.That(G.State.Count("pump_part"), Is.EqualTo(1));
            yield return U(PumpRig.Role.Shelf);
            Assert.That(G.State.Count("pump_part"), Is.EqualTo(1), "부품은 한 번만");
            yield return U(PumpRig.Role.Valve);
            yield return Until(() => PumpRig.Water == 0, 8, "배수");
            yield return U(PumpRig.Role.Breaker);
            Assert.That(PumpRig.PowerOn && !PumpRig.Running, Is.True, "깨진 이음쇠 → 헛돎");
            yield return U(PumpRig.Role.Coupling);
            Assert.That(PumpRig.PartFixed, Is.False, "전원 켠 채로는 바꾸지 않는다");
            Assert.That(G.State.Count("pump_part"), Is.EqualTo(1), "잘못 조작해도 부품을 잃지 않는다");
            yield return U(PumpRig.Role.Breaker);
            yield return U(PumpRig.Role.Coupling);
            Assert.That(PumpRig.PartFixed, Is.True);
            Assert.That(G.State.Count("pump_part"), Is.EqualTo(0));
            yield return U(PumpRig.Role.Breaker);
            Assert.That(PumpRig.Running, Is.True);
            Assert.That(G.State.Has("waste_pump_fixed") && G.State.Has("waste_resolved"), Is.True);
            Assert.That(G.State.D.wasteResult, Is.EqualTo("repair"));
            int exp = G.State.D.lu.exp;
            yield return U(PumpRig.Role.Breaker); yield return U(PumpRig.Role.Breaker);
            Assert.That(G.State.D.lu.exp, Is.EqualTo(exp), "다시 켜도 보상 없음");
            Assert.That(G.State.Count("snack"), Is.EqualTo(3), "과자를 쓰지 않았다");
        }

        // ================================================================ 4장: 비살상 충돌
        [UnityTest]
        public IEnumerator Waste_DealFails_RealtimeNonLethalFight_SurvivorsHurtLuButOnlyCalm_ThenTalkAndExit()
        {
            yield return StartAt("M17", "start", "dead", 0, gs => gs.Set("waste_met"));
            yield return Ev("waste_f", 2);
            Assert.That(G.State.Has("waste_fight"), Is.True);
            yield return Secs(0.3f);
            var svs = Scavenger.All.ToList();
            Assert.That(svs.Count, Is.EqualTo(3));
            Assert.That(svs.All(s => s.Active), Is.True, "실시간으로 움직인다 (별도 전투 화면 없음)");
            // 생존자도 실제로 휘두른다
            var p = G.Player; var a = svs[0];
            p.ResetCombatTimers();
            int hp0 = G.State.D.lu.hp;
            p.Teleport(new Vector2(a.transform.position.x - 0.7f, a.transform.position.y));
            yield return Until(() => G.State.D.lu.hp < hp0, 6, "생존자의 휘두르기에 맞음");
            // 루의 전기에 맞으면 겁이 줄고, 0 이 되면 주저앉는다 (사망·쓰러짐 그림 없음)
            foreach (var s in svs) for (int i = 0; i < 3; i++) s.TakeHit(1, 0.4f, p.transform.position, "electric");
            Assert.That(svs.All(s => s.Calmed), Is.True);
            Assert.That(svs.All(s => s.View == null || (s.View.Current != "Death" && s.View.Current != "Down")), Is.True, "죽거나 쓰러지지 않는다");
            yield return Until(() => G.State.Has("waste_fight_done") && !G.InEvent, 15, "진정 뒤 대화");
            Assert.That(G.State.Has("waste_resolved"), Is.True);
            Assert.That(G.State.D.wasteResult, Is.EqualTo("fight"));
            Assert.That(G.State.Has("waste_fight"), Is.False);
            Assert.That(G.State.D.dialogueSeen, Does.Contain("c4_fight_end"));
            // 이후 펌프를 고치면 화해
            G.State.Set("pump_water_set"); G.State.SetVar("pump_water", 0); G.State.Set("pump_part_fixed"); G.State.Set("pump_power_on");
            G.RunEvent(StoryEvents.PumpStarted(G)); yield return Until(() => !G.InEvent, 10, "펌프");
            Assert.That(G.State.D.wasteReconciled, Is.True);
            Assert.That(G.State.D.wasteResult, Is.EqualTo("fight"), "해결 방식 기록은 바뀌지 않는다");
        }

        // ================================================================ 4장: 008 에게 과자 (베넬 세 갈래)
        IEnumerator Kid(string venelle)
        {
            yield return StartAt("M17", "start", venelle, 2, gs => { gs.Set("waste_met"); gs.Set("waste_resolved"); gs.D.wasteResult = "gift"; });
            Assert.That(G.Companion != null, Is.EqualTo(venelle == "ally"), "동행일 때만 베넬이 따라온다");
            yield return Ev("waste_008", 0);
            Assert.That(G.State.D.child008Fed, Is.True);
            Assert.That(G.State.D.dialogueSeen, Does.Contain("c4_008_fed"));
        }

        [UnityTest] public IEnumerator Kid008_Ally_VenelleLine_LuSplitsOwnSnack()
        {
            yield return Kid("ally");
            Assert.That(G.State.D.dialogueSeen, Does.Contain("c4_008_venelle").And.Contain("c4_008_split"));
            Assert.That(G.State.Count("snack"), Is.EqualTo(0), "하나는 008, 하나는 루가 반으로 나눔");
            Assert.That(G.State.D.venelleTrust, Is.EqualTo(1));
            yield return Ev("waste_008", 0);
            Assert.That(G.State.Count("snack"), Is.EqualTo(0));
            Assert.That(G.State.D.venelleTrust, Is.EqualTo(1), "한 번만");
        }

        [UnityTest] public IEnumerator Kid008_Apart_NoLivingVenelleLine()
        {
            yield return Kid("apart");
            Assert.That(G.State.D.dialogueSeen, Has.No.Member("c4_008_venelle"));
            Assert.That(G.State.D.dialogueSeen, Does.Contain("c4_008_solo"));
            Assert.That(G.State.Count("snack"), Is.EqualTo(1));
        }

        [UnityTest] public IEnumerator Kid008_Dead_NoLivingVenelleLine()
        {
            yield return Kid("dead");
            Assert.That(G.State.D.dialogueSeen, Has.No.Member("c4_008_venelle"));
            Assert.That(G.State.Count("snack"), Is.EqualTo(1));
        }

        // ================================================================ 5장
        [UnityTest]
        public IEnumerator Workshop_PulseIsNewSkill_SlotChoiceKeepsWire_BatteryLampStaysOn()
        {
            yield return StartAt("M21", "start", "apart", 0);
            Assert.That(G.State.D.skills, Has.No.Member("pulse"), "아직 배우지 않은 기술");
            yield return Ev("ws_manual", 2);
            Assert.That(G.State.D.skills, Does.Contain("pulse"));
            Assert.That(G.State.D.slots[2], Is.EqualTo("pulse"));
            Assert.That(G.State.D.slots, Does.Contain("wire"), "전선 칸은 바꾸지 않는다");
            Assert.That(G.State.D.skills, Does.Contain("pull"), "뺀 기술은 잃지 않는다 (메뉴에서 되돌림)");
            var p = G.Player;
            p.Teleport(new Vector2(17f, p.transform.position.y));
            G.State.D.lu.charge = 4;
            yield return Secs(0.2f);
            p.Skills.UseSlot(2);
            yield return Until(() => G.State.Has("ws_tested") && !G.InEvent, 15, "시험 뒤 반응");
            Assert.That(Device.Find("ws_lamp").Model.power, Is.EqualTo(DevicePower.Off), "같은 선의 일반 등은 꺼진다");
            Assert.That(Device.Find("ws_lamp_battery").Model.power, Is.EqualTo(DevicePower.On), "배터리 등은 켜져 있다");
            Assert.That(G.State.D.dialogueSeen, Does.Contain("c5_hair_solo").And.Not.Contain("c5_hair_v"));
        }

        [UnityTest]
        public IEnumerator Hall_OverloadStopsBeltPcDoor_BatteryPcStays_RestoreOrVentToProceed()
        {
            yield return StartAt("M22", "start", "ally", 0);
            var belt = Conveyor.Find("hall"); var door = Device.Find("hall_door");
            Assert.That(belt.Running && door.Operable, Is.True);
            var box = Find<GridBox>();
            Assert.That(box.CanPull, Is.True);
            box.Pull(null);
            yield return null;
            Assert.That(belt.Running, Is.False, "벨트 정지");
            Assert.That(Device.Find("hall_pc").Model.power, Is.EqualTo(DevicePower.Off));
            Assert.That(Device.Find("hall_pc_battery").Model.power, Is.EqualTo(DevicePower.On), "배터리가 보이는 PC 는 계속 켜져 있다");
            Assert.That(door.Operable, Is.False, "같은 선의 문도 꺼졌다");
            Assert.That(door.blocker.enabled, Is.True);
            yield return Until(() => G.State.Has("hall_off_said") && !G.InEvent, 15, "문도 껐다 대사");
            Assert.That(G.State.D.dialogueSeen, Does.Contain("c5_hall_off_v"), "동행 대사");
            // 우회로: 문 위 환기 통로 사다리가 있다
            Assert.That(UnityEngine.Object.FindObjectsByType<Ladder>().Any(l => Mathf.Abs(l.transform.position.x - 56.5f) < 0.6f), Is.True);
            // 재가동
            var rb = UnityEngine.Object.FindObjectsByType<RestoreButton>().First(b => b.line == "hall_grid");
            G.RunEvent(rb.Interact(G)); yield return Until(() => !G.InEvent, 10, "복구");
            Assert.That(door.Operable && belt.Running, Is.True);
            Assert.That(G.State.Has("hall_pc_unlocked"), Is.False, "전기를 썼다고 파일 권한이 바뀌지 않는다");
        }

        [UnityTest]
        public IEnumerator Belt_PushesLuAndGuard_DirectionControl()
        {
            yield return StartAt("M22", "start", "apart", 0);
            foreach (var gd in Guard.All.ToArray()) if (gd.guardId == "M22_cat") UnityEngine.Object.Destroy(gd.gameObject);
            foreach (var c in UnityEngine.Object.FindObjectsByType<Cctv>()) UnityEngine.Object.Destroy(c.gameObject);
            var belt = Conveyor.Find("hall");
            var p = G.Player;
            p.Teleport(new Vector2(30f, belt.topY)); yield return Secs(0.1f);
            float x0 = p.transform.position.x; yield return Secs(0.5f);
            Assert.That(p.transform.position.x, Is.LessThan(x0 - 0.5f), "기본 방향: 서 있으면 왼쪽으로 밀린다");
            var ctl = Find<BeltControl>();
            G.RunEvent(ctl.Interact(G)); yield return Until(() => !G.InEvent, 5, "방향 바꿈");
            Assert.That(belt.Dir, Is.EqualTo(1));
            x0 = p.transform.position.x; yield return Secs(0.5f);
            Assert.That(p.transform.position.x, Is.GreaterThan(x0 + 0.5f));
        }

        [UnityTest]
        public IEnumerator Shaft_ChaseDeathRetry_RestartsFromCheckpoint_NoStaleChase()
        {
            yield return StartAt("M23", "start", "apart", 0);
            var p = G.Player;
            G.Checkpoint("m23_test");
            yield return Ev("m23_chase");
            Assert.That(G.State.Has("m23_chase"), Is.True);
            yield return Secs(0.3f);
            var g1 = Guard.All.First(x => x.guardId == "M23_g1");
            Assert.That(g1.Alive && g1.Hunting, Is.True, "뒤에서 경비가 쫓아온다");
            G.State.D.lu.hp = 1; p.ResetCombatTimers();
            p.Hurt(50, (Vector2)p.transform.position + Vector2.left);
            yield return Until(() => G.Player != null && G.Player != p && !G.Loading && G.FieldInputAllowed, 20, "재시도");
            Assert.That(G.State.Has("m23_chase"), Is.False, "구간 시작점으로 (추격 전)");
            yield return Secs(0.2f);
            Assert.That(Guard.All.First(x => x.guardId == "M23_g1").Alive, Is.False, "추격 전에는 경비가 없다");
            yield return Ev("m23_chase");
            Assert.That(G.State.Has("m23_chase"), Is.True, "다시 추격 가능");
        }

        [UnityTest]
        public IEnumerator Shaft_Ally_AcidBlocksGuard_PlateZapKnocksOut()
        {
            yield return StartAt("M23", "start", "ally", 0, gs => gs.Set("m23_chase"));
            var p = G.Player;
            float l2 = p.transform.position.y + 8f;
            p.Teleport(new Vector2(25.5f, l2)); G.Companion.PlaceBehindPlayer();
            yield return Ev("m23_l2");
            Assert.That(G.State.Has("m23_acid"), Is.True, "베넬이 약품으로 길을 막는다");
            Assert.That(Guard.NoGo.Count, Is.EqualTo(1));
            var g2 = Guard.All.First(x => x.guardId == "M23_g2");
            yield return Secs(4f);
            Assert.That(g2.transform.position.x, Is.LessThan(8.2f), "경비가 약품 웅덩이를 넘어오지 못한다");
            p.Teleport(new Vector2(14.5f, l2));
            yield return Ev("m23_plate_zap");
            Assert.That(g2.State, Is.EqualTo(Guard.GState.Down), "이어진 바닥의 전류로 쓰러짐");
            Assert.That(G.State.D.dialogueSeen, Does.Contain("c5_plate_v"));
        }

        [UnityTest]
        public IEnumerator Shaft_Solo_NoInvisibleVenelle_ShutterBlocks()
        {
            yield return StartAt("M23", "start", "dead", 0, gs => gs.Set("m23_chase"));
            var p = G.Player;
            p.Teleport(new Vector2(25.5f, p.transform.position.y + 8f));
            yield return Ev("m23_l2");
            Assert.That(G.State.Has("m23_acid"), Is.False, "베넬이 없으면 약품이 나타나지 않는다");
            Assert.That(Guard.NoGo.Count, Is.EqualTo(0));
            yield return Ev("m23_shutter");
            Assert.That(G.State.Has("m23_shutter"), Is.True);
            yield return null; yield return null;
            var blk = GameObject.Find("ShutterBlock").GetComponent<BoxCollider2D>();
            Assert.That(blk.enabled, Is.True, "셔터가 실제로 길을 막는다");
            Assert.That(Conveyor.Find("shaft").Running, Is.True, "벨트가 경비를 되민다");
        }

        // ================================================================ 6장
        [UnityTest]
        public IEnumerator Room_Ally_MemoryOnceThenTalk_Absent_NoLiveLines()
        {
            yield return StartAt("M25", "start", "ally", 0);
            yield return Ev("room_photo"); yield return Ev("room_letter");
            Assert.That(G.State.Has("ivy_memory_done"), Is.False, "세 가지를 다 보기 전에는 과거 장면이 없다");
            yield return Ev("room_wrappers");
            Assert.That(G.State.Has("ivy_memory_done") && G.State.Has("ivy_room_done"), Is.True);
            Assert.That(G.State.D.dialogueSeen, Does.Contain("iv_mono").And.Contain("c6_v_confused").And.Contain("c6_room_talk_3"));
            Assert.That(G.State.D.docs, Does.Contain("D_IVY"));
            int seen = G.State.D.dialogueSeen.Count;
            yield return Ev("room_wrappers");
            Assert.That(G.State.D.dialogueSeen.Contains("c6_wrappers_after_v"), Is.True, "다시 봐도 과거 장면은 반복하지 않는다");
        }

        [UnityTest]
        public IEnumerator Room_Dead_PhotoLetterOnly_NoMemoryNoVenelle()
        {
            yield return StartAt("M25", "start", "dead", 0);
            yield return Ev("room_photo"); yield return Ev("room_letter"); yield return Ev("room_wrappers");
            Assert.That(G.State.Has("ivy_memory_done"), Is.False);
            Assert.That(G.State.D.dialogueSeen, Does.Contain("c6_photo_dead").And.Contain("c6_letter_dead"));
            Assert.That(G.State.D.dialogueSeen.Any(x => x.StartsWith("iv_") || x == "c6_photo_v" || x == "c6_v_confused"), Is.False, "살아 있는 베넬의 말·과거 장면 없음");
        }

        [UnityTest]
        public IEnumerator Laundry_ThreeBranches_DistinctLines()
        {
            yield return StartAt("M24", "start", "apart", 0);
            yield return Ev("laundry_twins");
            Assert.That(G.State.D.dialogueSeen, Does.Contain("c6_twins_apart").And.Not.Contain("c6_twins_v"));
        }

        [UnityTest]
        public IEnumerator Gate_Checklist_ChoiceNotAutoConfirmed_NoEntryOnStay()
        {
            yield return StartAt("M24", "start", "ally", 0);
            G.RunEvent(StoryEvents.Run(G, "ch7_gate"));
            yield return Until(() => Game.ChoiceOpen, 20, "선택지");
            // 직전 입력(조사·건너뛰기)이 그대로 넘어와 확정되지 않는다
            GameInput.Virtual.Tap(Act.Interact); GameInput.Virtual.Tap(Act.Skip);
            yield return Secs(0.4f);
            Assert.That(Game.ChoiceOpen, Is.True, "기본 선택 없음 + 입력 대기");
            MenuList.ScriptedChoices.Enqueue(1);
            yield return Until(() => !G.InEvent, 10, "선택 뒤");
            Assert.That(G.State.Has("ch7_ready"), Is.False);
            Assert.That(StoryEvents.Ch7Checklist(G, 0), Does.Contain("줍지 않은 과자"));
        }

        /// <summary>15~26 의 모든 출구·문이 실제로 있는 도착 지점으로 이어진다 (되돌아가기 포함, 6장까지 지름길로 돌아갈 수 있음).</summary>
        [UnityTest]
        public IEnumerator Connections_AllExitsAndDoorsLeadToExistingSpawns_BothWays()
        {
            string[] maps = { "M15", "M16", "M17", "M18", "M19", "M20", "M21", "M22", "M23", "M24", "M25", "M26" };
            var links = new System.Collections.Generic.List<(string from, string to, string spawn)>();
            var spawns = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>();
            yield return StartAt("M15", "start", "ally", 0);
            foreach (var m in maps)
            {
                G.GoToMap(m, null);
                yield return Until(() => G.CurrentMapId == m && G.Player != null && !G.Loading, 20, m);
                yield return Until(() => !G.InEvent && !Cutscene.Running, 40, m + " 도입");
                spawns[m] = new System.Collections.Generic.HashSet<string>(UnityEngine.Object.FindObjectsByType<SpawnPoint>().Select(sp => sp.id));
                foreach (var z in UnityEngine.Object.FindObjectsByType<TriggerZone>()) if (z.kind == TriggerZone.Kind.MapExit) links.Add((m, z.targetMap, z.targetSpawn));
                foreach (var d in UnityEngine.Object.FindObjectsByType<DoorExit>()) links.Add((m, d.targetMap, d.targetSpawn));
            }
            foreach (var l in links)
            {
                if (!spawns.ContainsKey(l.to)) { Assert.That(l.from == "M15" && l.to == "M13", Is.True, "범위 밖 연결: " + l.from + "→" + l.to); continue; }
                Assert.That(spawns[l.to], Does.Contain(l.spawn), l.from + " → " + l.to + " 도착 지점 '" + l.spawn + "' 없음");
            }
            // 되돌아가기: 연결된 쌍은 양방향
            foreach (var l in links.Where(x => spawns.ContainsKey(x.to)))
                Assert.That(links.Any(b => b.from == l.to && b.to == l.from), Is.True, l.from + " → " + l.to + " 돌아오는 길 없음");
        }

        // ================================================================ 연결 (승강장 → 폐기소)
        [UnityTest]
        public IEnumerator Connection_M15RightExit_ToCargoLift_OldDemoEndRemoved()
        {
            yield return StartAt("M15", "start", "ally", 0);
            var exit = UnityEngine.Object.FindObjectsByType<TriggerZone>().FirstOrDefault(z => z.targetMap == "M16");
            Assert.That(exit, Is.Not.Null, "M15 오른쪽 출구 → 화물 승강장");
            Assert.That(UnityEngine.Object.FindObjectsByType<TriggerZone>().Any(z => z.eventId == "demo_end"), Is.False, "옛 시연 종료 지점 없음");
            var p = G.Player;
            p.Teleport(new Vector2(47.5f, p.transform.position.y));
            GameInput.Virtual.Hold(Act.Right, true);
            yield return Until(() => G.CurrentMapId == "M16" && !G.Loading && !G.InEvent, 10, "M16 도착");
            GameInput.Virtual.ReleaseAll();
            Assert.That(G.Companion, Is.Not.Null, "동행 상태를 초기화하지 않는다");
            Assert.That(G.State.D.chapterId, Is.EqualTo("ch4"));
        }
    }
}

