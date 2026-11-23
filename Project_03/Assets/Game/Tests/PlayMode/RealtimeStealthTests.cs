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
    /// 실시간 잠입·액션 필수 검증 (실제 씬 + 가상 입력). 자동 테스트이며 사람이 하는 플레이 확인·재미 판단을 대신하지 않는다.
    /// 사전 조건: EW > 1. 전체 구성 으로 씬·GameConfig 생성.
    /// </summary>
    public class RealtimeStealthTests
    {
        string dir;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "ew_rt_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            SaveSystem.OverrideDirectory = dir;
            GameInput.AutoAdvance = false;
            GameInput.AutoReadDelay = 0;
            GameInput.Virtual.ReleaseAll(); GameInput.Virtual.Skip = false;
            GameInput.ClearContexts();
            MenuList.ScriptedChoices.Clear();
            TimeCtl.ClearAll();
            Hazard.ForcePause = false;
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
            SaveSystem.OverrideDirectory = null;
            try { Directory.Delete(dir, true); } catch { }
        }

        static IEnumerator Until(Func<bool> cond, float timeout, string what)
        {
            float t = 0;
            while (!cond())
            {
                t += Time.unscaledDeltaTime;
                if (t > timeout) Assert.Fail("시간 초과: " + what + " | " + Status());
                yield return null;
            }
        }

        static string Status()
        {
            var g = Game.I; if (g == null || g.State == null) return "no game";
            return "map=" + g.CurrentMapId + " ctx=" + GameInput.Context + " cut=" + Cutscene.CurrentId + " ev=" + g.InEvent + " pos=" + (g.Player != null ? (Vector2)g.Player.transform.position : Vector2.zero) + " ts=" + Time.timeScale;
        }

        static IEnumerator Frames(int n) { for (int i = 0; i < n; i++) yield return null; }
        static IEnumerator Secs(float s) { for (float t = 0; t < s; t += Time.unscaledDeltaTime) yield return null; }

        Game G => Game.I;

        public static readonly string[] AfterAlarm =
        {
            "op_village_done", "op_house_done", "op_desktop_done", "op_r_in", "c1_tank_broken", "c1_researchers_down", "op_tank_done",
            "lab_woke", "lab_realized", "lab_power", "lab_power_1", "lab_power_2", "lab_power_3", "lab_power_4", "lab_alarm",
            "corridor_memory", "c1_left_lab", "c1_escape_view", "c2s_enter_said", "pull_unlocked", "op_desktop_land", "pc_guide_read", "got_pouch",
        };

        IEnumerator StartAt(string map, string spawn, Action<GameState> prep = null, bool afterAlarm = true)
        {
            SceneManager.LoadScene("Title");
            yield return Until(() => Game.I != null, 10, "Game 생성");
            yield return null;
            var d = SaveData.NewGame();
            if (afterAlarm) d.flags.AddRange(AfterAlarm);
            var gs = new GameState(d, G.Db);
            prep?.Invoke(gs);
            G.State = gs;
            G.GoToMap(map, spawn);
            yield return Until(() => G.Map != null && G.Map.mapId == map && G.Player != null && !G.Loading, 25, map + " 로드");
            yield return Until(() => !G.InEvent && !Cutscene.Running, 30, map + " 도입 끝");
            yield return Frames(3);
        }

        static T Find<T>() where T : UnityEngine.Object => UnityEngine.Object.FindAnyObjectByType<T>();

        [UnityTest]
        public IEnumerator DoubleTapDash_ActualPlayer_DirectionCooldownAndContext()
        {
            yield return StartAt("C03", "start", st=>st.Set("c3_enter_said"));
            var p=G.Player;
            foreach(var e in UnityEngine.Object.FindObjectsByType<Guard>()) UnityEngine.Object.Destroy(e.gameObject);
            p.Teleport(new Vector2(3,p.transform.position.y)); p.ResetCombatTimers();
            yield return Frames(3);
            float x=p.transform.position.x;
            GameInput.Virtual.Hold(Act.Right,true); yield return Frames(3);
            Assert.That(p.transform.position.x,Is.GreaterThan(x),"first press moves immediately | "+Status()+" v="+p.Velocity+" dt="+Time.deltaTime+" hits="+string.Join(",",Physics2D.OverlapBoxAll((Vector2)p.transform.position+Vector2.up*.67f,new Vector2(.56f,1.1f),0).Select(c=>c.name+":"+c.isTrigger+":"+c.bounds)));
            Assert.That(p.Dodging,Is.False,"single held press is not dash");
            GameInput.Virtual.Hold(Act.Right,false); yield return Frames(2);
            GameInput.Virtual.Hold(Act.Right,true); yield return Frames(2);
            Assert.That(p.Dodging,Is.True,"DD fires"); Assert.That(p.Velocity.x,Is.GreaterThan(0));
            GameInput.Virtual.ReleaseAll(); yield return Frames(2);
            GameInput.Virtual.Tap(Act.Left); yield return Frames(2);
            GameInput.Virtual.Tap(Act.Left); yield return Frames(2);
            yield return Secs(.7f);
            Assert.That(p.Dodging,Is.False,"cooldown taps cannot cause delayed dash");
            p.ResetCombatTimers();
            GameInput.Virtual.Tap(Act.Left); yield return Frames(2);
            int h=GameInput.Push(InputCtx.Menu); GameInput.Pop(h);
            GameInput.Virtual.Tap(Act.Left); yield return Frames(2);
            Assert.That(p.Dodging,Is.False,"even same-frame UI context changes reset tap history");
            GameInput.Virtual.ReleaseAll(); yield return Secs(.3f);
            GameInput.Virtual.Tap(Act.Left); yield return Frames(2);
            GameInput.Virtual.Tap(Act.Right); yield return Frames(2);
            Assert.That(p.Dodging,Is.False,"AD does not dash");
            yield return Secs(.3f);
            GameInput.Virtual.Tap(Act.Left); yield return Frames(2);
            GameInput.Virtual.Tap(Act.Left); yield return Frames(2);
            Assert.That(p.Dodging,Is.True,"AA fires"); Assert.That(p.Velocity.x,Is.LessThan(0));
        }

        [UnityTest]
        public IEnumerator DoubleTapKeyboard_RealEdges_NoShiftOrRepeat_FocusAndTeleportReset()
        {
            yield return StartAt("C03","start");
            var p=G.Player;p.ResetCombatTimers();p.Teleport(new Vector2(10,p.transform.position.y));
            foreach(var guard in UnityEngine.Object.FindObjectsByType<Guard>()) UnityEngine.Object.Destroy(guard.gameObject);
            var settings=UnityEngine.InputSystem.InputSystem.settings;
            var oldBackground=settings.backgroundBehavior;
            settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            var oldEditor=settings.editorInputBehaviorInPlayMode;
            settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            var kb=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
            UnityEngine.InputSystem.InputSystem.EnableDevice(kb);kb.MakeCurrent();
            try
            {
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.LeftShift));
                yield return Frames(3);Assert.That(p.Dodging,Is.False,"Shift removed");
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb,new UnityEngine.InputSystem.LowLevel.KeyboardState());yield return Frames(2);
                var right=new UnityEngine.InputSystem.LowLevel.KeyboardState(GameInput.KeyOf(Act.Right));
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb,right);yield return Frames(3);
                for(int i=0;i<3;i++){UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb,right);yield return Frames(2);Assert.That(p.Dodging,Is.False,"held/repeated states never double-tap");}
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb,new UnityEngine.InputSystem.LowLevel.KeyboardState());yield return Frames(3);
                yield return Secs(.3f);
                var left=new UnityEngine.InputSystem.LowLevel.KeyboardState(GameInput.KeyOf(Act.Left));
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb,left);yield return Frames(2);
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb,new UnityEngine.InputSystem.LowLevel.KeyboardState());yield return Frames(2);
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb,left);yield return Frames(2);
                Assert.That(p.Dodging,Is.True,"real keyboard AA fires | pressed="+kb[GameInput.KeyOf(Act.Left)].isPressed+" current="+(UnityEngine.InputSystem.Keyboard.current==kb)+" "+Status());Assert.That(p.Velocity.x,Is.LessThan(0));
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb,new UnityEngine.InputSystem.LowLevel.KeyboardState());yield return Secs(.7f);p.ResetCombatTimers();
                GameInput.Virtual.Tap(Act.Left);yield return Frames(2);
                p.SendMessage("OnApplicationFocus",false);p.SendMessage("OnApplicationFocus",true);
                GameInput.Virtual.Tap(Act.Left);yield return Frames(2);Assert.That(p.Dodging,Is.False,"focus resets");
                yield return Secs(.3f);
                GameInput.Virtual.Tap(Act.Right);yield return Frames(2);p.Teleport(p.transform.position);
                GameInput.Virtual.Tap(Act.Right);yield return Frames(2);Assert.That(p.Dodging,Is.False,"scene/teleport resets");
            }
            finally {
                UnityEngine.InputSystem.InputSystem.RemoveDevice(kb);
                settings.backgroundBehavior=oldBackground;
#if UNITY_EDITOR
                settings.editorInputBehaviorInPlayMode=oldEditor;
#endif
            }
        }

#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator PixelSamples_ActualTilemaps_OriginalLu_CollisionAndIndependentParallax()
        {
            yield return StartAt("C03","start");
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/Game/Art/PixelSamples/Tests/PixelConnectionTest.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return Until(()=>G.Map!=null&&G.Map.mapId=="ART_TEST"&&G.Player!=null,10,"pixel sample scene");
            yield return Frames(8);
            var maps=UnityEngine.Object.FindObjectsByType<UnityEngine.Tilemaps.Tilemap>();
            Assert.That(maps.Length,Is.EqualTo(2));
            Assert.That(maps.All(m=>m.GetUsedTilesCount()>0),Is.True);
            Assert.That(G.Player.View.sr.sprite.pixelsPerUnit,Is.EqualTo(32));
            Assert.That(G.Player.Grounded,Is.True,"functional collision supports character on visible platform top");
            Assert.That(G.Player.transform.position.y,Is.EqualTo(3).Within(.05f));
            var far=GameObject.Find("Far_0.7").transform;var mid=GameObject.Find("Mid_0.3").transform;
            Assert.That(far.GetComponentsInChildren<SpriteRenderer>().All(sr=>sr.sprite!=null),Is.True);
            Assert.That(mid.GetComponentsInChildren<SpriteRenderer>().All(sr=>sr.sprite!=null),Is.True);
            float f=far.position.x,m=mid.position.x,x=G.Player.transform.position.x;
            GameInput.Virtual.Hold(Act.Right,true);yield return Secs(1.3f);GameInput.Virtual.ReleaseAll();yield return Frames(5);
            Assert.That(G.Player.transform.position.x,Is.GreaterThan(x+3.5f),"native sample platform is playable");
            Assert.That(far.position.x-f,Is.GreaterThan(mid.position.x-m+.1f),"far/mid scroll independently");
            Assert.That(G.Player.Dodging,Is.False,"holding movement is not double tap");
            var tank=GameObject.Find("SpecimenTank_native80x112").GetComponent<SpriteRenderer>();
            Assert.That(tank.sprite.rect.size,Is.EqualTo(new Vector2(80,112)));
            Assert.That(tank.transform.localScale,Is.EqualTo(Vector3.one));
            // Ladder has ordinary W/S motion and A/D exit; exit key cannot seed a dash.
            G.Player.Teleport(new Vector2(6,3));yield return Frames(2);
            GameInput.Virtual.Hold(Act.MenuUp,true);yield return Frames(5);
            Assert.That(G.Player.Climbing,Is.True);
            GameInput.Virtual.ReleaseAll();GameInput.Virtual.Hold(Act.Right,true);yield return Frames(2);
            Assert.That(G.Player.Climbing,Is.False);GameInput.Virtual.ReleaseAll();yield return Frames(2);
            GameInput.Virtual.Tap(Act.Right);yield return Frames(2);
            Assert.That(G.Player.Dodging,Is.False,"ladder exit does not seed double tap");
        }
#endif

        class TestInteraction : Interactable
        {
            public int calls;
            public override IEnumerator Interact(Game g){calls++; yield return null;}
        }

        [UnityTest]
        public IEnumerator Space_InteractionConsumesJump_HeightAndWallRejectTargets()
        {
            yield return StartAt("C03","start");var p=G.Player;
            yield return Until(()=>p.Grounded,3,"지상");
            var go=new GameObject("InteractionProbe");go.transform.position=p.transform.position+Vector3.right*0.45f;
            var target=go.AddComponent<TestInteraction>();target.promptText="조사";
            float y=p.transform.position.y;
            GameInput.Virtual.Tap(Act.Interact);GameInput.Virtual.Tap(Act.Jump);
            yield return Secs(0.2f);
            Assert.That(target.calls,Is.EqualTo(1));Assert.That(p.transform.position.y,Is.EqualTo(y).Within(0.05f),"같은 Space가 점프하지 않음");
            go.transform.position=p.transform.position+Vector3.up*3;
            yield return Frames(3);Assert.That(p.Focus,Is.Not.EqualTo(target),"다른 층 제외");
            go.transform.position=p.transform.position+Vector3.right*1.1f;
            var wall=new GameObject("InteractionWall");wall.transform.position=p.Chest+Vector2.right*0.55f;wall.AddComponent<BoxCollider2D>().size=new Vector2(0.16f,2);
            yield return Frames(3);Assert.That(p.Focus,Is.Not.EqualTo(target),"벽 너머 제외");
            UnityEngine.Object.Destroy(go);UnityEngine.Object.Destroy(wall);
        }

        [UnityTest]
        public IEnumerator Venelle_PressureAttackHasReachableSafeZone_AndNoExitDoorBlock()
        {
            yield return StartAt("C06","start",gs=>gs.Set("c3_boss_intro"));
            var b=Find<VenelleBoss>();var p=G.Player;
            Assert.That(GameObject.Find("BackDoorBlock"),Is.Null,"물리 진행문 제거");
            b.TakeHit(Mathf.CeilToInt(ActionTuning.I.venelle.hp*0.6f),0,p.transform.position,"test");
            Assert.That(b.Enraged,Is.True);
            typeof(VenelleBoss).GetMethod("BeginPressure",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(b,null);
            Assert.That(Mathf.Abs(b.SafeZoneX-p.transform.position.x),Is.LessThanOrEqualTo(5f),"도달 가능한 안전 구역");
            p.Teleport(new Vector2(b.SafeZoneX,p.transform.position.y));int hp=G.State.D.lu.hp;
            yield return Secs(3.7f);
            Assert.That(G.State.D.lu.hp,Is.EqualTo(hp),"폭주 중 안전 구역은 피해 없음");
            Assert.That(GameObject.Find("PressureSafeZone"),Is.Null,"효과 종료 뒤 정리");
        }

        // ================================================================ 1. 입력: Space 점프 / J 조사 / Q·W 즉시 사용 / W 와 사다리 분리 / 대화 종료 J 가 다음 조사로 새지 않음
        [UnityTest]
        public IEnumerator Input_SpaceContext_QERF_WSLadder_ADExit()
        {
            yield return StartAt("C03", "start");
            var p = G.Player; var sk = p.Skills;
            yield return Until(() => G.FieldInputAllowed, 5, "조작 가능");
            // Space = 점프
            float y0 = p.transform.position.y;
            GameInput.Virtual.Tap(Act.Jump);
            yield return Secs(0.18f);
            Assert.That(p.transform.position.y, Is.GreaterThan(y0 + 0.5f), "Space 점프");
            yield return Until(() => p.Grounded, 3, "착지");
            // Q = 톡! 즉시 (순환 없이)
            yield return Secs(0.3f);
            GameInput.Virtual.Tap(Act.Skill1);
            yield return Frames(3);
            Assert.That(sk.CooldownLeft("tok"), Is.GreaterThan(0f), "Q 즉시 사용 | " + Status());
            var lad = UnityEngine.Object.FindObjectsByType<Ladder>().OrderBy(l => Mathf.Abs(l.CenterX - 39.5f)).First();
            p.Teleport(new Vector2(lad.CenterX, lad.Bottom));
            yield return Secs(0.4f);
            float cy=p.transform.position.y;
            GameInput.Virtual.Hold(Act.MenuUp,true);
            yield return Secs(0.45f);
            GameInput.Virtual.Hold(Act.MenuUp,false);
            Assert.That(p.Climbing,Is.True,"W 바로 사다리 붙기");
            Assert.That(p.transform.position.y,Is.GreaterThan(cy+0.4f),"W 위로");
            float up=p.transform.position.y;
            GameInput.Virtual.Hold(Act.Down,true);
            yield return Secs(0.16f);
            GameInput.Virtual.Hold(Act.Down,false);
            Assert.That(p.transform.position.y,Is.LessThan(up),"S 아래로");
            GameInput.Virtual.Hold(Act.Right,true);
            yield return Frames(3);
            GameInput.Virtual.Hold(Act.Right,false);
            Assert.That(p.Climbing,Is.False,"D 이탈");
            yield return Until(()=>p.Grounded,4,"착지");

            // W: 보이는 CCTV 를 한 번에 끈다
            var cam = UnityEngine.Object.FindObjectsByType<Cctv>().First(c => c.id == "C03_cam");
            p.Teleport(new Vector2(cam.transform.position.x - 3f, y0)); p.SetFacing(1);
            G.State.D.lu.charge = 4;
            yield return Secs(0.4f);
            GameInput.Virtual.Tap(Act.Skill2);
            yield return Secs(0.5f);
            Assert.That(cam.Jammed, Is.True, "W 한 번으로 CCTV 정지 | 제안=" + G.UI.hud.SuggestText);
            // 대상이 없으면 비용 없이 이유만
            p.Teleport(new Vector2(4f, y0)); p.SetFacing(-1);
            yield return Secs(1.0f);
            int ch = G.State.D.lu.charge;
            GameInput.Virtual.Tap(Act.Skill3);
            yield return Secs(0.3f);
            Assert.That(G.State.D.lu.charge, Is.EqualTo(ch), "대상이 없으면 전하를 쓰지 않는다");
            Assert.That(sk.LastFail, Is.Not.Empty, "짧은 실패 이유");

            // 대화: Space 는 넘기지 않고 J 로 넘긴다. 닫은 J 를 누른 채면 다음 조사가 실행되지 않는다
            G.RunEvent(G.Say("c2s_quiet"));
            yield return Until(() => G.UI.dialogue.Open, 5, "대화 열림");
            yield return Secs(1.2f);
            GameInput.Virtual.Tap(Act.Jump); GameInput.Virtual.Tap(Act.Skip);
            yield return Secs(0.3f);
            Assert.That(G.UI.dialogue.Open, Is.True, "Space·Enter 는 대사를 넘기지 않는다");
            GameInput.Virtual.Hold(Act.Interact, true);
            yield return Until(() => !G.UI.dialogue.Open && G.FieldInputAllowed, 5, "J 로 닫힘");
            yield return Frames(10);
            Assert.That(G.InEvent, Is.False, "대화를 닫은 J 가 다음 상호작용으로 넘어가지 않는다");
            GameInput.Virtual.Hold(Act.Interact, false);
        }

        // ================================================================ 2. 엄폐: 고체 상자(발판)와 전경 엄폐물(뒤에 숨기) 분리, 숨으면 CCTV 판정에서 빠짐
        [UnityTest]
        public IEnumerator Cover_ForegroundHideSpotHidesFromCctv_SolidCrateBlocks()
        {
            yield return StartAt("C02", "from_lab");
            var p = G.Player;
            var hide = UnityEngine.Object.FindObjectsByType<HideSpot>().OrderBy(h => Mathf.Abs(h.transform.position.x - 30.2f)).First();
            var col = hide.GetComponent<BoxCollider2D>();
            Assert.That(col.isTrigger, Is.True, "전경 엄폐물은 뒤로 들어갈 수 있다");
            Assert.That(hide.art.sortingOrder, Is.GreaterThan(p.View.sr.sortingOrder), "엄폐물이 루보다 앞에 그려진다");
            var crate = UnityEngine.Object.FindObjectsByType<Cover>().First();
            Assert.That(crate.GetComponent<BoxCollider2D>().isTrigger, Is.False, "고체 상자는 통과할 수 없는 발판");
            p.Teleport(new Vector2(hide.transform.position.x, hide.transform.position.y));
            var cam = UnityEngine.Object.FindObjectsByType<Cctv>().First(c => c.id == "C02_cam2");
            yield return Frames(3);
            float aim = Mathf.Atan2(p.Chest.y - cam.Eye.y, p.Chest.x - cam.Eye.x) * Mathf.Rad2Deg;
            cam.DebugSetAngle(aim);
            Assert.That(Sight.SeePlayer(cam.Eye, aim, cam.HalfAngle, cam.Range, p, out _), Is.GreaterThan(0), "서 있으면 보인다");
            GameInput.Virtual.Hold(Act.Down, true);
            yield return Frames(4);
            Assert.That(p.HiddenInCover, Is.True, "엄폐 영역에서 S = 숨음");
            Assert.That(Sight.SeePlayer(cam.Eye, aim, cam.HalfAngle, cam.Range, p, out _), Is.EqualTo(0), "숨으면 CCTV 판정에서 빠진다");
            float s0 = cam.Suspicion;
            yield return Secs(1.0f);
            Assert.That(cam.Suspicion, Is.LessThanOrEqualTo(s0 + 0.001f), "숨은 동안 의심이 오르지 않는다");
            // 공격하면 노출
            GameInput.Virtual.Tap(Act.Skill1);
            yield return Frames(3);
            Assert.That(p.HiddenInCover, Is.False, "공격하면 숨음이 풀린다");
            GameInput.Virtual.Hold(Act.Down, false);
        }

        // ================================================================ 3. 발각: CCTV 신고 → 경비가 실제로 달려와 추적·공격 / 시야를 끊으면 수색 후 복귀
        [UnityTest]
        public IEnumerator Detection_CctvCallsGuardWhoChasesAndAttacks_BreakingSightEndsChase()
        {
            yield return StartAt("C03", "start");
            var p = G.Player;
            var cam = UnityEngine.Object.FindObjectsByType<Cctv>().First(c => c.id == "C03_cam");
            var gd = UnityEngine.Object.FindObjectsByType<Guard>().First(x => x.guardId == "C03_guard");
            G.State.Set("c03_shutter_open");
            p.Teleport(new Vector2(cam.transform.position.x + 1.5f, p.transform.position.y));
            yield return Frames(3);
            float aim = Mathf.Atan2(p.Chest.y - cam.Eye.y, p.Chest.x - cam.Eye.x) * Mathf.Rad2Deg;
            cam.DebugSetAngle(aim);
            int hp0 = G.State.D.lu.hp;
            yield return Until(() => cam.Suspicion >= 1f, 6, "CCTV 게이지 가득");
            yield return Until(() => gd.Hunting || gd.State == Guard.GState.Chase, 4, "경비 호출");
            yield return Until(() => gd.State == Guard.GState.Chase || gd.State == Guard.GState.Windup || gd.State == Guard.GState.Strike, 15, "경비가 와서 추적");
            yield return Until(() => G.State.D.lu.hp < hp0, 15, "실제 공격");
            // 시야를 끊는다 (위층으로 순간 이동 + 엄폐) → 수색 → 복귀
            p.Teleport(new Vector2(6.2f, p.transform.position.y));
            yield return Until(() => gd.State == Guard.GState.Search || gd.State == Guard.GState.Return || gd.State == Guard.GState.Patrol, 12, "추적 해제 → 수색");
            yield return Until(() => gd.State == Guard.GState.Return || gd.State == Guard.GState.Patrol, 15, "수색 뒤 복귀");
        }

        /// <summary>경비가 없는 구역: CCTV 가 신고하면 화면 밖 출입구에서 지원 드론이 온다 (맵당 최대 2대).</summary>
        [UnityTest]
        public IEnumerator Detection_NoGuardZone_BackupDroneComesFromOffscreen_Limited()
        {
            yield return StartAt("C02", "from_lab");
            var p = G.Player;
            var cam = UnityEngine.Object.FindObjectsByType<Cctv>().First(c => c.id == "C02_cam1");
            p.Teleport(new Vector2(cam.transform.position.x + 1f, p.transform.position.y));
            yield return Frames(3);
            float aim = Mathf.Atan2(p.Chest.y - cam.Eye.y, p.Chest.x - cam.Eye.x) * Mathf.Rad2Deg;
            cam.DebugSetAngle(aim);
            yield return Until(() => Reinforcements.SpawnedThisMap >= 1, 8, "지원 요청");
            yield return Until(() => UnityEngine.Object.FindObjectsByType<Guard>().Any(x => x.Reinforcement), 4, "드론 등장");
            var dr = UnityEngine.Object.FindObjectsByType<Guard>().First(x => x.Reinforcement);
            var v = Camera.main.WorldToViewportPoint(dr.transform.position);
            Assert.That(v.x < 0 || v.x > 1 || v.y < 0 || v.y > 1, Is.True, "화면 밖에서 나타난다");
            Assert.That(dr.Hunting, Is.True, "신고 위치로 달려온다");
            Assert.That(Reinforcements.SpawnedThisMap, Is.LessThanOrEqualTo(Reinforcements.MaxPerMap));
        }

        // ================================================================ 4. 시설 경보: 탈출 뒤 ~ 베넬 등장까지
        [UnityTest]
        public IEnumerator FacilityAlarm_LastsFromEscapeUntilVenelle()
        {
            yield return StartAt("C04", "start");
            Assert.That(Facility.AlarmOn, Is.True, "탈출 뒤 이동 중 경보 유지");
            yield return StartAt("C06", "start", gs => gs.Set("c3_boss_intro"));
            Assert.That(Facility.AlarmOn, Is.False, "베넬이 나타난 뒤 끝남");
        }

        // ================================================================ 5. 컷신 스킵 종료 상태 + 선택지는 스킵으로 확정되지 않음
        [UnityTest]
        public IEnumerator Skip_TankCutscene_RestoresState_ChoiceNotAutoConfirmed()
        {
            yield return StartAt("C01", "tank", gs => { foreach (var f in new[] { "op_village_done", "op_house_done", "op_desktop_land", "op_desktop_done", "pc_guide_read", "op_tank_done" }) gs.Set(f); }, false);
            // StartAt 은 도입 끝까지 기다리므로 여기선 이미 끝난 상태일 수 있어 직접 다시 재생
            G.State.Set("op_tank_done", false);
            G.RunEvent(StoryEvents.OnMapEnter(G, "C01"));
            yield return Until(() => Cutscene.Running, 5, "통 연출 시작");
            yield return Secs(1.0f);
            GameInput.Virtual.Tap(Act.Skip);
            yield return Until(() => !Cutscene.Running && !G.InEvent, 10, "건너뛰기 완료");
            Assert.That(Cutscene.LastSkipped, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(GameInput.Context, Is.EqualTo(InputCtx.Gameplay));
            Assert.That(G.UI.FadeAlpha, Is.LessThan(0.01f), "화면 페이드 정상");
            Assert.That(G.FieldInputAllowed, Is.True);
            Assert.That(G.State.Has("op_tank_done") && G.State.Has("lab_alarm"), Is.True, "완료 상태 적용 + 경보");
            Assert.That(G.Player.View.sr.enabled && G.Player.View.transform.localScale == Vector3.one, Is.True, "루 모습 복구");
            var rig = CameraRig.Main; Assert.That(rig.Scripted, Is.False, "카메라 추적 복구");

            // 선택지가 열린 동안 Enter 는 받지 않는다
            G.RunEvent(Cutscene.Play(G, "test_choice", ChoiceBody(), null));
            yield return Until(() => Game.ChoiceOpen, 10, "선택지 열림");
            GameInput.Virtual.Tap(Act.Skip);
            yield return Secs(0.5f);
            Assert.That(Game.ChoiceOpen, Is.True, "Enter 로 선택지를 넘기지 않는다");
            MenuList.ScriptedChoices.Enqueue(1);
            yield return Until(() => !Cutscene.Running, 5, "선택 후 종료");
            Assert.That(G.LastChoice, Is.EqualTo(1));
        }

        IEnumerator ChoiceBody() { yield return G.Choose(new[] { "가", "나" }); }

        // ================================================================ 6. 실험 통: 몸 전체가 통 안 (모든 관련 동작)
        [UnityTest]
        public IEnumerator Tank_LuBodyStaysInsideTank_AllPoses()
        {
            yield return StartAt("C01", "tank", gs => { foreach (var f in new[] { "op_village_done", "op_house_done", "op_desktop_land", "op_desktop_done", "op_tank_done" }) gs.Set(f); }, false);
            G.State.Set("op_tank_done", false); G.State.Set("c1_tank_broken", false); G.State.Set("op_r_in", false); G.State.Set("c1_researchers_down", false);
            GameInput.AutoReadDelay = 0.3f;
            G.RunEvent(StoryEvents.OnMapEnter(G, "C01"));
            var tank = Marker.Pos("tank", Vector2.zero);
            var seen = new System.Collections.Generic.HashSet<string>();
            float t = 0;
            while (!G.State.Has("c1_tank_broken") && t < 60f)
            {
                var p = G.Player;
                string a = p.AnimOverride ?? "";
                if ((a == "TankCrouch" || a == "TankStruggle" || a == "TankCower" || a == "ChargeEyes") && p.Grounded)
                {
                    seen.Add(a);
                    var b = p.View.sr.bounds;
                    Assert.That(b.min.y, Is.GreaterThanOrEqualTo(tank.y + 0.5f - 0.02f), a + ": 발이 받침 위 (통 밖으로 나오지 않음)");
                    Assert.That(b.max.y, Is.LessThanOrEqualTo(tank.y + 3.04f), a + ": 머리가 뚜껑 아래");
                    Assert.That(b.min.x, Is.GreaterThanOrEqualTo(tank.x - 1.0f), a + ": 왼쪽 벽 안");
                    Assert.That(b.max.x, Is.LessThanOrEqualTo(tank.x + 1.0f), a + ": 오른쪽 벽 안");
                }
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            Assert.That(seen.Count, Is.GreaterThanOrEqualTo(3), "대기·몸부림·움츠림·방전 직전 자세 확인: " + string.Join(",", seen));
            GameInput.AutoReadDelay = 0;
        }

        // ================================================================ 7. 컴퓨터 안: 작업표시줄 하나, 안내서 전에는 납치되지 않음, 안내서 수치 = 실제 수치
        [UnityTest]
        public IEnumerator Desktop_OneTaskbar_ExploreBeforeAbduction_GuideMatchesTuning()
        {
            yield return StartAt("P03", "start", gs => { foreach (var f in new[] { "op_village_done", "op_house_done", "op_desktop_land" }) gs.Set(f); }, false);
            Assert.That(UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Count(sr => sr.gameObject.name == "Taskbar"), Is.EqualTo(1), "작업표시줄 그림 하나");
            Assert.That(UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Count(sr => sr.gameObject.name == "Solid" || sr.gameObject.name == "SolidTop"), Is.EqualTo(0), "반복 바닥 타일 없음");
            Assert.That(UnityEngine.Object.FindObjectsByType<OneWayPlatform>().Length, Is.EqualTo(0), "점프맵 제거");
            Assert.That(G.CurrentMapId, Is.EqualTo("P03"));
            yield return Secs(2f);
            Assert.That(G.CurrentMapId, Is.EqualTo("P03"), "몇 초 지났다고 납치되지 않는다");
            // 안내서 전 접속 기록: 납치 없음
            GameInput.AutoAdvance = true;
            yield return G.RunEvent(StoryEvents.Run(G, "pc_log"));
            Assert.That(Cutscene.CurrentId, Is.Not.EqualTo("op_transfer"));
            Assert.That(G.CurrentMapId, Is.EqualTo("P03"));
            // 안내서: 간섭 쪽 숫자가 실제 수치와 같다
            G.RunEvent(StoryEvents.Run(G, "pc_guide"));
            yield return Until(() => GuideBook.Open, 5, "안내서 열림");
            var jam = ActionTuning.I.Skill("jam");
            bool found = false;
            for (int i = 0; i < 6 && !found; i++)
            {
                var body = GameObject.Find("GuideBook").GetComponentsInChildren<UnityEngine.UI.Text>().First(x => x.gameObject.name == "Body").text;
                if (body.Contains(jam.desc))
                {
                    found = true;
                    StringAssert.Contains(jam.cooldown.ToString("0.0#") + "초", body);
                    StringAssert.Contains("◆" + jam.cost, body);
                    StringAssert.Contains(jam.effectTime.ToString("0") + "초", body);
                    var chip = GameObject.Find("GuideBook").GetComponentsInChildren<UnityEngine.UI.Text>().First(x => x.gameObject.name == "K").text;
                    StringAssert.Contains(GameInput.Label(Act.Skill2), chip, "실제 키 표시");
                }
                else { GameInput.Virtual.Tap(Act.Right); yield return Secs(0.2f); }
            }
            Assert.That(found, Is.True, "안내서에 간섭 쪽이 있다");
            GameInput.Virtual.Tap(Act.Interact);
            yield return Until(() => !GuideBook.Open && !G.InEvent, 5, "J 로 닫기");
            Assert.That(G.State.Has("pc_guide_read"), Is.True);
            Assert.That(UnityEngine.Object.FindObjectsByType<PracticeTarget>().Length,Is.EqualTo(0),"불필요한 연습 기물 제거");
            var p=G.Player; float y=p.transform.position.y;
            GameInput.Virtual.Hold(Act.MenuUp,true); yield return Secs(0.4f); GameInput.Virtual.Hold(Act.MenuUp,false);
            Assert.That(p.transform.position.y,Is.GreaterThan(y+0.5f),"무중력 상향 이동");
            yield return Secs(0.4f); float hover=p.transform.position.y;
            yield return Secs(0.5f); Assert.That(p.transform.position.y,Is.EqualTo(hover).Within(0.04f),"낙하 없음");
            // 이제 접속 기록 → 강제 전송 → 실험 통
            yield return G.RunEvent(StoryEvents.Run(G, "pc_log"));
            yield return Until(() => G.CurrentMapId == "C01" && !G.Loading, 30, "강제 전송 후 실험 통");
            GameInput.AutoAdvance = false;
        }

        // ================================================================ 8. 베넬: 정상·스킵 진입 모두 첫 돌진을 피할 시간 / 잡기 자세·대화 중 정지·피해 1회
        [UnityTest]
        public IEnumerator Venelle_FairFirstDash_BothEntries()
        {
            // 정상 진입 (대사는 자동으로 넘김)
            GameInput.AutoAdvance = true;
            yield return StartAt("C06", "start", null, true);
            GameInput.AutoAdvance = false;
            var boss = Find<VenelleBoss>();
            Assert.That(boss.State, Is.EqualTo(VenelleBoss.VState.DashWindup));
            Assert.That(boss.WindupLeft, Is.GreaterThan(1.2f), "첫 돌진 준비 시간");
            float dist = Mathf.Abs(boss.transform.position.x - G.Player.transform.position.x);
            Assert.That(dist, Is.GreaterThan(6f), "돌진 거리 확보");
            int hp = G.State.D.lu.hp;
            yield return Secs(0.6f);
            Assert.That(G.State.D.lu.hp, Is.EqualTo(hp), "컷신 종료와 동시에 확정 피격 없음");

            // 건너뛰기 진입 (Enter): 같은 시작 상태
            if (Game.I != null) UnityEngine.Object.DestroyImmediate(Game.I.gameObject);
            GameInput.Virtual.Skip = true;
            yield return StartAt("C06", "start", gs => { }, true);
            GameInput.Virtual.Skip = false;
            boss = Find<VenelleBoss>();
            Assert.That(Cutscene.LastSkipped, Is.True);
            Assert.That(boss.State, Is.EqualTo(VenelleBoss.VState.DashWindup));
            Assert.That(boss.WindupLeft, Is.GreaterThan(1.2f), "건너뛰어도 첫 돌진 준비 시간");
            float d2 = Mathf.Abs(boss.transform.position.x - G.Player.transform.position.x);
            Assert.That(d2, Is.EqualTo(dist).Within(0.6f), "정상·건너뛰기 진입 같은 거리");
        }

        [UnityTest]
        public IEnumerator Venelle_GrabHoldsByCollar_PausesHazards_SingleDamage_ThenThrowWithProtection()
        {
            yield return StartAt("C06", "start", gs => gs.Set("c3_boss_intro"));
            var boss = Find<VenelleBoss>(); var p = G.Player;
            boss.transform.position = new Vector3(p.transform.position.x + 0.8f, boss.transform.position.y);
            int hp0 = G.State.D.lu.hp;
            MenuList.ScriptedChoices.Enqueue(1);   // '나가려는 거야'
            GameInput.AutoReadDelay = 0.6f;
            boss.DebugForceGrab();
            yield return Until(() => G.UI.dialogue.Open, 5, "잡기 대화");
            // 손 = 목덜미, 발이 바닥에서 떨어짐
            Vector2 hand = boss.HandWorld;
            Assert.That(Mathf.Abs(p.transform.position.x - hand.x), Is.LessThan(0.05f), "손과 몸 x 정렬");
            Assert.That(Mathf.Abs(p.transform.position.y + VenelleBoss.LuNeck - hand.y), Is.LessThan(0.05f), "손 높이 = 목덜미");
            Assert.That(p.transform.position.y - boss.transform.position.y, Is.GreaterThan(0.25f), "발이 바닥에서 떨어짐");
            Assert.That(p.AnimOverride, Is.EqualTo("Lifted"));
            Assert.That(Hazard.Paused, Is.True, "대화 중 위험 정지");
            Assert.That(G.State.D.lu.hp, Is.EqualTo(hp0), "잡는 순간 반복 피해 없음");
            yield return Until(() => boss.State != VenelleBoss.VState.Holding && boss.State != VenelleBoss.VState.Thrown && !G.InEvent, 20, "던지기 끝");
            int dmg = hp0 - G.State.D.lu.hp;
            Assert.That(dmg, Is.EqualTo(Mathf.Max(1, Mathf.RoundToInt(ActionTuning.I.venelle.grabDamage * ActionTuning.CurrentDifficulty.enemyDamage))), "착지 때 한 번만");
            Assert.That(p.InvulnLeft, Is.GreaterThan(0.8f), "던져진 뒤 보호 시간");
            Assert.That(p.Frozen || p.ScriptControl, Is.False, "조작 복구");
            // 두 번째 잡기는 짧은 대사
            Assert.That(G.State.Has("v_grab_talked"), Is.True);
            GameInput.AutoReadDelay = 0;
        }

        // ================================================================ 9. 분기: 협상 성공 / 제압 후 살림 / 죽임 — 각각 진행이 막히지 않고 상태가 맞다
        [UnityTest]
        public IEnumerator Venelle_NegotiationNeedsSeveralTalks_RepeatGivesNoGain()
        {
            yield return StartAt("C06", "start", gs => gs.Set("c3_boss_intro"));
            var boss = Find<VenelleBoss>();
            Hazard.ForcePause = true;
            GameInput.AutoAdvance = true;
            MenuList.ScriptedChoices.Enqueue(0);  // 나가려는 거야 (신뢰 +2)
            yield return G.RunEvent(StoryEvents.VenelleTalk(G, boss));
            Assert.That(G.State.Has("c3_boss_done"), Is.False, "한 번의 선택으로 끝나지 않는다");
            int trust = VenelleMind.Trust;
            MenuList.ScriptedChoices.Enqueue(0);  // 같은 말 반복
            yield return G.RunEvent(StoryEvents.VenelleTalk(G, boss));
            Assert.That(VenelleMind.Trust, Is.EqualTo(trust), "반복해도 무한히 오르지 않는다");
            MenuList.ScriptedChoices.Enqueue(2);  // 자극 (역효과)
            yield return G.RunEvent(StoryEvents.VenelleTalk(G, boss));
            Assert.That(VenelleMind.Threat, Is.GreaterThan(5), "역효과");
            Assert.That(G.State.Has("c3_boss_done"), Is.False, "역효과 한 번으로 초기화·실패 없음");
            MenuList.ScriptedChoices.Enqueue(1);  // 왜 찾아
            yield return G.RunEvent(StoryEvents.VenelleTalk(G, boss));
            MenuList.ScriptedChoices.Enqueue(3);  // 손을 내림
            yield return G.RunEvent(StoryEvents.VenelleTalk(G, boss));
            // 위협이 아직 높으면 계속 싸운다. 시간이 지나 위협이 내려간 뒤 다시 손을 내리면… (반복 효과 없음) → 행동(분전함)으로 증명
            if (!VenelleMind.WillStop) { G.State.Set("v_junction_live"); Find<JunctionHazard>().Jam(1f); VenelleMind.AddThreat(-3); }
            MenuList.ScriptedChoices.Enqueue(1);
            MenuList.ScriptedChoices.Enqueue(0);   // 휴식: 같이 가자 (초콜릿이 없으므로 초콜릿 질문 없음)
            G.RunEvent(StoryEvents.VenelleTalk(G, boss));
            yield return Until(() => G.State.Has("c3_boss_done") || G.CurrentMapId == "M15", 40, "협상 성립");
            Assert.That(G.State.Has("c3_negotiated"), Is.True);
            yield return Until(() => G.CurrentMapId == "M15" && !G.Loading, 30, "휴식 장소");
            GameInput.AutoAdvance = false;
            Hazard.ForcePause = false;
        }

        [UnityTest]
        public IEnumerator Venelle_DefeatThenKill_DeadState_NoCompanionScenes_RouteOpen_SaveKeeps()
        {
            yield return StartAt("C06", "start", gs => gs.Set("c3_boss_intro"));
            var boss = Find<VenelleBoss>(); var p = G.Player;
            boss.DebugDamage(999);
            Assert.That(boss.State, Is.EqualTo(VenelleBoss.VState.Downed), "바로 죽지 않고 쓰러짐");
            Assert.That(boss.PuddleCount, Is.EqualTo(0), "남은 웅덩이 제거");
            yield return Secs(1f);
            Assert.That(G.State.Has("c3_boss_done"), Is.False, "가까이 가서 J 하기 전엔 결정되지 않는다");
            p.Teleport(new Vector2(boss.transform.position.x - 1.2f, boss.transform.position.y));
            yield return Secs(0.4f);
            GameInput.AutoAdvance = true;
            // J 연타가 처치 선택을 자동 확정하지 않는다
            G.RunEvent(StoryEvents.VenelleDownedChoice(G, boss));
            yield return Until(() => Game.ChoiceOpen, 15, "처치 선택");
            GameInput.AutoAdvance = false;
            for (int i = 0; i < 4; i++) { GameInput.Virtual.Tap(Act.Interact); yield return Secs(0.15f); }
            Assert.That(Game.ChoiceOpen, Is.True, "기본 선택이 없어 J 연타로 확정되지 않는다");
            MenuList.ScriptedChoices.Enqueue(0);
            GameInput.AutoAdvance = true;
            yield return Until(() => G.State.Has("c3_boss_done") && !G.InEvent, 20, "처치 장면");
            Assert.That(G.State.D.venelleState, Is.EqualTo("dead"));
            Assert.That(G.State.Has("venelle_dead"), Is.True);
            Assert.That(G.State.Has("c3_to_rest"), Is.False, "휴식·초콜릿 장면 없음");
            Assert.That(G.Companion, Is.Null);
            p.Teleport(new Vector2(5.1f,p.transform.position.y));
            GameInput.Virtual.Hold(Act.Left,true);yield return Secs(0.72f);GameInput.Virtual.Hold(Act.Left,false);
            Assert.That(p.transform.position.x,Is.LessThan(3.3f),"전투 후 왼쪽 문 자리 통과");
            // 저장·불러오기 후에도 유지
            G.SaveNow("test", false);
            Assert.That(SaveSystem.TryLoad(out var loaded, out _), Is.EqualTo(SaveSystem.LoadResult.Ok));
            Assert.That(loaded.venelleState, Is.EqualTo("dead"));
            // 주 경로 계속: 오른쪽 출구가 열려 있다
            G.GoToMap("M13", "start");
            yield return Until(() => G.CurrentMapId == "M13" && !G.Loading, 20, "다음 맵");
            G.GoToMap("M15", "start");
            yield return Until(() => G.CurrentMapId == "M15" && !G.Loading && !G.InEvent, 20, "휴게실 (휴식 없음)");
            Assert.That(G.State.Has("c3_rest_done"), Is.False);
            GameInput.AutoAdvance = false;
        }

        [UnityTest]
        public IEnumerator Venelle_DefeatThenSpare_RestChocolateOnce_JoinOrApart_FirstWireOnce()
        {
            yield return StartAt("C06", "start", gs => { gs.Set("c3_boss_intro"); gs.AddItem("chocolate"); });
            var boss = Find<VenelleBoss>(); var p = G.Player;
            boss.DebugDamage(999);
            p.Teleport(new Vector2(boss.transform.position.x - 1.2f, boss.transform.position.y));
            GameInput.AutoAdvance = true;
            MenuList.ScriptedChoices.Enqueue(1);   // 죽이지 않는다
            MenuList.ScriptedChoices.Enqueue(0);   // 초콜릿 건넴
            MenuList.ScriptedChoices.Enqueue(0);   // 출구까지 같이
            yield return G.RunEvent(StoryEvents.VenelleDownedChoice(G, boss));
            yield return Until(() => G.CurrentMapId == "M15" && G.State.Has("c3_rest_done") && !G.InEvent, 60, "휴식 장면 끝");
            Assert.That(G.State.Has("venelle_spared"), Is.True);
            Assert.That(G.State.Count("chocolate"), Is.EqualTo(0), "초콜릿 한 번 소비");
            Assert.That(G.State.Has("gave_chocolate"), Is.True);
            int trust = VenelleMind.Trust;
            Assert.That(G.State.D.venelleState, Is.EqualTo("ally"), "경계 속 대화 뒤 임시 동행");
            Assert.That(G.Companion, Is.Not.Null);
            // 첫 전선 동행: 한 번만 대사, 함께 도착
            var line = UnityEngine.Object.FindObjectsByType<PowerLine>().OrderBy(l => l.transform.position.x).First();
            p = G.Player;
            p.Teleport(line.transform.position);
            G.Companion.PlaceBehindPlayer();
            yield return Secs(0.3f);
            yield return G.RunEvent(StoryEvents.WireTravel(G, line));
            Assert.That(G.State.Has("wire_duo_done"), Is.True);
            Assert.That(Vector2.Distance(G.Companion.transform.position, p.transform.position), Is.LessThan(1.6f), "동료가 반대편에 남지 않는다");
            Assert.That(G.Companion.View.sr.enabled, Is.True);
            int seenCount = G.State.D.dialogueSeen.Count(x => x == "w_first_1");
            var back = line.exit.GetComponent<PowerLine>();
            yield return G.RunEvent(StoryEvents.WireTravel(G, back));
            Assert.That(G.State.D.dialogueSeen.Count(x => x == "w_first_1"), Is.EqualTo(seenCount), "두 번째부터 대사 반복 없음");
            Assert.That(Vector2.Distance(G.Companion.transform.position, p.transform.position), Is.LessThan(1.6f));
            Assert.That(VenelleMind.Trust, Is.EqualTo(trust), "반복 대화·이동으로 신뢰가 오르지 않는다");
            GameInput.AutoAdvance = false;
        }

        [UnityTest]
        public IEnumerator Venelle_SpareButGoAlone_ApartStateConsistent()
        {
            yield return StartAt("C06", "start", gs => gs.Set("c3_boss_intro"));
            var boss = Find<VenelleBoss>(); var p = G.Player;
            boss.DebugDamage(999);
            p.Teleport(new Vector2(boss.transform.position.x - 1.2f, boss.transform.position.y));
            GameInput.AutoAdvance = true;
            MenuList.ScriptedChoices.Enqueue(1);   // 죽이지 않는다
            MenuList.ScriptedChoices.Enqueue(1);   // 혼자 간다
            yield return G.RunEvent(StoryEvents.VenelleDownedChoice(G, boss));
            yield return Until(() => G.CurrentMapId == "M15" && G.State.Has("c3_rest_done") && !G.InEvent, 60, "휴식 끝");
            Assert.That(G.State.D.venelleState, Is.EqualTo("apart"));
            Assert.That(G.Companion, Is.Null);
            Assert.That(G.State.Has("venelle_bench"), Is.True, "벤치에 남아 있다");
            var line = UnityEngine.Object.FindObjectsByType<PowerLine>().OrderBy(l => l.transform.position.x).First();
            G.Player.Teleport(line.transform.position);
            yield return Secs(0.2f);
            yield return G.RunEvent(StoryEvents.WireTravel(G, line));
            Assert.That(G.State.Has("wire_duo_done"), Is.False, "혼자 이동 (동행 장면 없음)");
            GameInput.AutoAdvance = false;
        }

        // ================================================================ 10. 재시도 뒤 분기 상태 유지
        [UnityTest]
        public IEnumerator Retry_KeepsNegotiationState()
        {
            yield return StartAt("C06", "start", gs => gs.Set("c3_boss_intro"));
            var boss = Find<VenelleBoss>();
            Hazard.ForcePause = true; GameInput.AutoAdvance = true;
            MenuList.ScriptedChoices.Enqueue(0);
            yield return G.RunEvent(StoryEvents.VenelleTalk(G, boss));
            int trust = VenelleMind.Trust;
            G.Checkpoint("test_cp");
            Hazard.ForcePause = false; GameInput.AutoAdvance = false;
            var p = G.Player;
            G.State.D.lu.hp = 1; p.ResetCombatTimers();
            p.Hurt(50, (Vector2)p.transform.position + Vector2.right);
            yield return Until(() => G.Player != null && G.Player != p && !G.Loading && G.FieldInputAllowed, 20, "재시도");
            Assert.That(VenelleMind.Trust, Is.EqualTo(trust), "재시도 뒤에도 협상 상태 유지");
            Assert.That(G.State.Has("v_opt_leave"), Is.True, "이전 선택 기억");
        }
    }
}

