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
    /// 7~11장·엔딩·검수 도구 필수 검증 (2026-11-15). 실제 씬 + 실제 사건 코드 + 배치 데이터 프리셋. 대사는 자동 넘김.
    /// 자동 테스트이며 사람의 조작감·감정선 판단을 대신하지 않는다. 연결 경로 전체는 Ch711Demo/StartGameToEndRecorded 로 따로 확인한다.
    /// </summary>
    public class Ch711Tests
    {
        string dir;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "ew_c711_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            SaveSystem.OverrideDirectory = dir;
            GameInput.AutoAdvance = false; GameInput.AutoReadDelay = 0;
            GameInput.Virtual.ReleaseAll(); GameInput.Virtual.Skip = false;
            GameInput.ClearContexts(); MenuList.ScriptedChoices.Clear();
            TimeCtl.ClearAll(); Hazard.ForcePause = false; Guard.NoGo.Clear();
            EndingScreens.EndingTestMode = true;
            WorldLayout.Reload();
        }

        [TearDown]
        public void TearDown()
        {
            GameInput.AutoAdvance = false; GameInput.AutoReadDelay = 0;
            GameInput.Virtual.ReleaseAll(); GameInput.Virtual.Skip = false;
            MenuList.ScriptedChoices.Clear();
            if (ReviewMode.Active) ReviewMode.OnReturnToTitle();
            if (Game.I != null) UnityEngine.Object.DestroyImmediate(Game.I.gameObject);
            if (PixelScreen.I != null) UnityEngine.Object.DestroyImmediate(PixelScreen.I.gameObject);
            if (AudioManager.I != null) UnityEngine.Object.DestroyImmediate(AudioManager.I.gameObject);
            GameInput.ClearContexts(); TimeCtl.ClearAll(); Guard.NoGo.Clear();
            EndingScreens.WaitSeconds = 5f;
            SaveSystem.OverrideDirectory = null;
            try { Directory.Delete(dir, true); } catch { }
        }

        static IEnumerator Until(Func<bool> cond, float timeout, string what)
        {
            float t = 0;
            while (!cond())
            {
                t += Time.unscaledDeltaTime;
                if (t > timeout) Assert.Fail("시간 초과: " + what + " | map=" + Game.I?.CurrentMapId + " ev=" + Game.I?.InEvent + " cut=" + Cutscene.CurrentId + " ctx=" + GameInput.Context);
                yield return null;
            }
        }
        static IEnumerator Secs(float s) { for (float t = 0; t < s; t += Time.unscaledDeltaTime) yield return null; }
        static T Find<T>() where T : UnityEngine.Object => UnityEngine.Object.FindAnyObjectByType<T>();
        Game G => Game.I;

        /// <summary>배치 데이터의 사건 프리셋 상태로 해당 맵에서 시작 (검수 도구와 같은 프리셋).</summary>
        IEnumerator StartPreset(string evId, string venelle = "keep", bool skipIntro = true)
        {
            SceneManager.LoadScene("Title");
            yield return Until(() => Game.I != null, 10, "Game 생성");
            yield return null;
            var e = WorldLayout.I.GetEvent(evId);
            Assert.That(e, Is.Not.Null, evId);
            var d = ReviewMode.BuildPreset(SaveData.NewGame(), e, venelle, out _);
            G.SetState(new GameState(d, G.Db));
            G.GoToMap(e.map, e.spawn);
            yield return Until(() => G.Map != null && G.Map.mapId == e.map && G.Player != null && !G.Loading, 30, e.map + " 로드");
            GameInput.AutoAdvance = true;
            if (skipIntro) yield return Until(() => !G.InEvent && !Cutscene.Running, 60, e.map + " 도입 끝");
            yield return null;
        }

        IEnumerator Ev(string id, params int[] choices)
        {
            MenuList.ScriptedChoices.Clear(); foreach (var c in choices) MenuList.ScriptedChoices.Enqueue(c);
            yield return Until(() => !G.InEvent, 30, "이전 이벤트 끝");
            G.RunEvent(StoryEvents.Run(G, id));
            yield return null;
            yield return Until(() => !G.InEvent && !Cutscene.Running, 90, id + " 끝");
            yield return null;
        }

        /// <summary>실제 기본 공격(톡!)이 보스 몸에 닿는지: 바로 옆에서 몇 번 써서 HP 가 줄어야 한다.</summary>
        IEnumerator RealTokHits(BossBase b)
        {
            int hp0 = b.Hp;
            for (int i = 0; i < 6 && b.Hp == hp0; i++)
            {
                var p = G.Player;
                float side = p.transform.position.x < b.transform.position.x ? -1 : 1;
                p.Teleport(new Vector2(b.transform.position.x + side * (b.BodyWidth / 2f + 0.5f), b.transform.position.y));
                p.SetFacing(-(int)side);
                p.Skills.UseSlot(G.State.D.slots.IndexOf("tok"));
                yield return Secs(0.4f);
            }
            Assert.That(b.Hp, Is.LessThan(hp0), "톡!이 " + b.bossName + " 에게 닿는다");
        }

        static void HitUntilZero(IDamageable b, int guard = 400)
        {
            for (int i = 0; i < guard && b.Alive; i++) b.TakeHit(1, 0.3f, b.Center + Vector2.left, "electric");
        }

        // ================================================================ 7장
        [UnityTest]
        public IEnumerator Ch6Gate_EntersHolding_SealsBehind_ReleaseAddsShelterGroup()
        {
            yield return StartPreset("ch6_gate", "ally");
            yield return Ev("ch7_gate", 0);
            yield return Until(() => G.CurrentMapId == "M27" && !G.Loading && !G.InEvent, 30, "수용동 진입");
            Assert.That(G.State.Has("ch7_entered"), Is.True);
            Assert.That(G.State.D.chapterId, Is.EqualTo("ch7"));
            Assert.That(SaveSystem.TryLoad(out var saved, out _), Is.Not.EqualTo(SaveSystem.LoadResult.NoSave));
            Assert.That(saved.flags, Does.Contain("ch7_entered"), "진입 직후 저장");
            // 뒤 출구는 봉쇄
            var west = UnityEngine.Object.FindObjectsByType<TriggerZone>().First(z => z.kind == TriggerZone.Kind.MapExit && z.targetMap == "M24");
            Assert.That(west.blockFlag, Is.EqualTo("ch7_entered"));
            yield return Ev("c7_release");
            Assert.That(G.State.Has("holding_released"), Is.True);
            Assert.That(G.State.D.shelterGroups, Does.Contain("holding_released"));
            Assert.That(G.State.D.shelterGroups, Does.Contain("waste_residents").Or.Not.Contain("x"));
            int groups = G.State.D.shelterGroups.Count;
            yield return Ev("c7_release");
            Assert.That(G.State.D.shelterGroups.Count, Is.EqualTo(groups), "다시 실행해도 중복 없음");
        }

        [UnityTest]
        public IEnumerator Ch7_StaffCard_AlternateSource_WhenDeskMissed()
        {
            yield return StartPreset("ch7_meilin", "apart");
            G.State.D.items.RemoveAll(i => i.k == "staff_card"); G.State.Set("has_staff_card", false);
            G.State.Set("c7_meilin_done"); G.State.Set("c7_meilin_met");
            G.GoToMap("M29", "start");
            yield return Until(() => G.CurrentMapId == "M29" && G.Player != null && !G.Loading && !G.InEvent, 30, "M29");
            var door = UnityEngine.Object.FindObjectsByType<DoorExit>().First(d => d.targetMap == "M30");
            Assert.That(door.requireFlag, Is.EqualTo("has_staff_card"));
            yield return Ev("c7_lostfound");
            Assert.That(G.State.Count("staff_card"), Is.EqualTo(1));
            Assert.That(G.State.Has("has_staff_card"), Is.True);
            yield return Ev("c7_lostfound");
            Assert.That(G.State.Count("staff_card"), Is.EqualTo(1), "두 번 받지 않는다");
        }

        [UnityTest]
        public IEnumerator Meilin_BeatableWithNormalAttacks_RetreatsAlive_DoorOpens()
        {
            yield return StartPreset("ch7_meilin", "dead");
            yield return Ev("c7_hall_enter");
            var b = Find<MeilinBoss>();
            Assert.That(b, Is.Not.Null); Assert.That(b.Active, Is.True);
            Assert.That(BossBars.Current, Is.SameAs(b), "보스 HP 바");
            yield return RealTokHits(b);
            HitUntilZero(b);
            yield return Until(() => G.State.Has("c7_meilin_done") && !G.InEvent, 60, "메이린 퇴각");
            Assert.That(G.State.Has("meilin_dead"), Is.False, "패배 후에도 살아 있음");
            var door = UnityEngine.Object.FindObjectsByType<DoorExit>().First(d => d.targetMap == "M29");
            Assert.That(door.CanInteract(G), Is.True);
        }

        [UnityTest]
        public IEnumerator Meilin_TerminalTrick_DollGrab_CausesOpening_ThenArmOffAfterRepeat()
        {
            yield return StartPreset("ch7_meilin", "apart");
            yield return Ev("c7_hall_enter");
            var b = Find<MeilinBoss>();
            var term = UnityEngine.Object.FindObjectsByType<ArenaSwitch>().First(s => s.kind == "terminal");
            for (int round = 0; round < 2; round++)
            {
                yield return Until(() => b.State == MeilinBoss.MState.Approach || b.State == MeilinBoss.MState.Recover, 20, "대기");
                G.Player.Teleport(new Vector2(term.transform.position.x, G.Player.transform.position.y));
                MenuList.ScriptedChoices.Clear(); MenuList.ScriptedChoices.Enqueue(0);
                G.RunEvent(term.Interact(G));
                yield return Until(() => !G.InEvent, 15, "단말");
                Assert.That(b.TargetIsDoll, Is.True, "호출 번호 00");
                G.Player.Teleport(new Vector2(4f, G.Player.transform.position.y));
                yield return Until(() => b.StunLeft > 0 && b.LastStunCause == "빈 운반 인형", 30, "인형에 부딪힘 " + round);
            }
            yield return Until(() => b.ArmOff, 15, "같은 속임수 반복 → 포획팔 끔");
            Assert.That(b.Crashes, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator Bulkhead_AllyVenelleDies_NoCompanionOrLinesAfter()
        {
            yield return StartPreset("ch7_bulkhead", "ally");
            Assert.That(G.Companion, Is.Not.Null);
            yield return Ev("c7_monitors");
            Assert.That(G.State.Has("c7_feeds_seen"), Is.True);
            G.RunEvent(StoryEvents.Run(G, "c7_bulkhead"));
            yield return Until(() => G.CurrentMapId == "M31" && !G.Loading && !G.InEvent, 90, "관제탑");
            Assert.That(G.State.D.venelleState, Is.EqualTo("dead"));
            Assert.That(G.State.D.venelleDeathScene, Is.EqualTo("bulkhead"));
            Assert.That(G.Companion, Is.Null, "사망 후 추종 없음");
            Assert.That(Actor.Find("venelle"), Is.Null);
            yield return Ev("c7_control");
            Assert.That(G.State.D.dialogueSeen, Does.Contain("c7_knock"));
            // 다시 불러와도 살아나지 않는다
            G.GoToMap("M31", "start");
            yield return Until(() => G.Player != null && !G.Loading, 30, "재로드");
            Assert.That(G.Companion, Is.Null);
        }

        [UnityTest]
        public IEnumerator Feeds_ApartVenelle_GoesToShelter_NoHudInFeed()
        {
            yield return StartPreset("ch7_bulkhead", "apart");
            Assert.That(G.Companion, Is.Null);
            bool sawFeed = false, hudVisibleDuringFeed = false;
            G.RunEvent(StoryEvents.Run(G, "c7_monitors"));
            for (float t = 0; t < 60 && (G.InEvent || Cutscene.Running); t += Time.unscaledDeltaTime)
            {
                if (FeedView.Open) { sawFeed = true; var cam = GameObject.Find("FeedCamera"); if (cam != null && cam.GetComponent<Camera>().targetTexture == null) hudVisibleDuringFeed = true; }
                yield return null;
            }
            Assert.That(sawFeed, Is.True);
            Assert.That(hudVisibleDuringFeed, Is.False, "영상은 별도 카메라의 렌더 텍스처 (HUD 캔버스 없음)");
            Assert.That(G.State.Has("venelle_to_shelter"), Is.True);
            Assert.That(G.State.D.shelterGroups, Does.Contain("venelle"));
        }

        // ================================================================ 8장
        [UnityTest]
        public IEnumerator Raviel_FightRoute_StopsNotKills_ThenRoomChoiceIsStagingOnly()
        {
            yield return StartPreset("ch8_reunion", "dead");
            yield return Ev("c8_meet", 1);
            Assert.That(G.State.D.friendRoute, Is.EqualTo("fight"));
            var b = Find<RavielBeast>();
            Assert.That(b != null && b.Active && !b.chaseMode, Is.True);
            yield return RealTokHits(b);
            HitUntilZero(b);
            yield return Until(() => G.State.Has("c8_duel_done") && !G.InEvent, 60, "멈춰 세움");
            Assert.That(G.State.D.ravielState, Is.Not.EqualTo("dead"));
            G.GoToMap("M33", "start");
            yield return Until(() => G.CurrentMapId == "M33" && !G.Loading && !G.InEvent, 30, "방");
            int trust0 = G.State.D.venelleTrust;
            yield return Ev("c8_photo", 1);
            Assert.That(G.State.D.reunionChoice, Is.EqualTo("sit"));
            Assert.That(G.State.D.ravielState, Is.EqualTo("ally"));
            Assert.That(G.Companion != null && G.Companion.actorId == "raviel", Is.True, "라비엘 동행");
            Assert.That(G.State.D.venelleTrust, Is.EqualTo(trust0), "선택은 점수가 아니다");
        }

        [UnityTest]
        public IEnumerator Raviel_FleeRoute_EndsAtGoal_WithoutDamagingHer()
        {
            yield return StartPreset("ch8_reunion", "apart");
            yield return Ev("c8_meet", 0);
            var b = Find<RavielBeast>();
            Assert.That(b.chaseMode, Is.True);
            Assert.That(BossBars.Current.BarName, Does.Contain("거리"));
            b.TakeHit(50, 1, b.Center, "electric");
            Assert.That(b.Active, Is.True, "도망 경로에서 공격으로 쓰러뜨리지 않는다");
            G.Player.Teleport(new Vector2(b.goalX + 0.5f, G.Player.transform.position.y));
            yield return Until(() => G.State.Has("c8_duel_done") && !G.InEvent, 60, "도망 끝");
        }

        // ================================================================ 9장
        [UnityTest]
        public IEnumerator Lacrimosa_FeedbackSolvableWithoutSound_PersuadeBranch()
        {
            yield return StartPreset("ch9_lacrimosa", "dead");
            AudioListener.volume = 0f;
            yield return Ev("c9_stage_enter");
            var b = Find<LacrimosaBoss>();
            Assert.That(b.FeedbackRate, Is.LessThan(0), "처음엔 피드백이 오르지 않는다");
            b.SetMixer(2, true, true, 2);
            Assert.That(b.FeedbackRate, Is.GreaterThan(0));
            yield return Until(() => b.ProtectStops >= 1, 20, "보호 정지 (게이지·연결선만으로)");
            yield return Until(() => !G.InEvent, 20, "대사");
            Assert.That(b.TalkOpen, Is.True);
            G.Player.Teleport((Vector2)b.transform.position + Vector2.left);
            MenuList.ScriptedChoices.Clear(); MenuList.ScriptedChoices.Enqueue(0);
            var talk = b.GetComponentInChildren<ArenaSwitch>();
            G.RunEvent(talk.Interact(G));
            yield return Until(() => G.State.Has("c9_lac_done") && !G.InEvent && !Cutscene.Running, 90, "설득");
            Assert.That(G.State.D.lacrimosaResult, Is.EqualTo("persuaded"));
            Assert.That(G.State.Has("lacrimosa_dead"), Is.False, "설득·제압 모두 여기서 죽지 않는다 (9장 붕괴에서 확정)");
            AudioListener.volume = 1f;
        }

        [UnityTest]
        public IEnumerator Collapse_ConfirmsDeaths_ApartVenelleDiesInShelter_NoLaterLines()
        {
            yield return StartPreset("ch9_collapse", "apart");
            Assert.That(G.State.Has("venelle_to_shelter"), Is.True);
            yield return Ev("c9_force");
            yield return Until(() => G.State.Has("shelter_collapsed") && !G.InEvent && !Cutscene.Running, 120, "붕괴");
            Assert.That(StoryEvents.Dead(G, "f"), Is.True); Assert.That(StoryEvents.Dead(G, "kid008"), Is.True);
            Assert.That(StoryEvents.Dead(G, "lacrimosa"), Is.True);
            Assert.That(G.State.D.venelleState, Is.EqualTo("dead")); Assert.That(G.State.D.venelleDeathScene, Is.EqualTo("shelter"));
            Assert.That(G.State.D.ravielState, Is.EqualTo("ally"));
        }

        [UnityTest]
        public IEnumerator RecycleBin_RestoreThenRelease_ReturnsToServerRoom_Caught()
        {
            yield return StartPreset("ch9_bin", "dead");
            Assert.That(G.Map.zeroG && G.Map.sealSkills, Is.True);
            MenuList.ScriptedChoices.Clear();
            foreach (var c in new[] { 3, 0, 1, 2, 3 }) MenuList.ScriptedChoices.Enqueue(c);   // 먼저 확인(대기) → 정렬 → 선택 → 복원 → 확인
            G.RunEvent(StoryEvents.Run(G, "c9_bin_list"));
            yield return Until(() => G.CurrentMapId == "M39" && !G.Loading && G.State.Has("c9_caught") && !G.InEvent, 90, "방출 접점");
            Assert.That(G.State.Has("c9_restored") && G.State.Has("c9_released"), Is.True);
            Assert.That(G.Companion != null && G.Companion.actorId == "raviel", Is.True);
        }

        // ================================================================ 10장
        [UnityTest]
        public IEnumerator FinalBoss_ThreeLimitReleases_MaxTo40_RetryNoDoubleReduction()
        {
            yield return StartPreset("ch10_phase1", "dead");
            int baseMax = G.State.MaxHp("lu");
            yield return Ev("c10_final_enter");
            var b = Find<AureliusBoss>();
            Assert.That(b.Phase, Is.EqualTo(1));
            Assert.That(G.State.D.ravielState, Is.EqualTo("injured"));
            Assert.That(G.State.Has("meilin_dead"), Is.False, "메이린은 전투 대상이 아니다");
            yield return RealTokHits(b);
            for (int ph = 1; ph <= 3; ph++)
            {
                HitUntilZero(b, 2000);
                yield return Until(() => b.AwaitingRelease, 10, "결합부 노출 " + ph);
                var sw = UnityEngine.Object.FindObjectsByType<ArenaSwitch>().First(s => s.kind == "limit");
                Assert.That(Mathf.Abs(sw.transform.position.y - G.Player.transform.position.y), Is.LessThan(1.1f), "한계 방출 지점은 바닥 높이 (손이 닿는다)");
                // 전하가 모자라면 방출하지 않고 안내만 (시간제 입력 없음)
                G.State.D.lu.charge = 0;
                G.RunEvent(sw.Interact(G)); yield return Until(() => !G.InEvent, 20, "안내");
                Assert.That(G.State.D.limitReleases, Is.EqualTo(ph - 1));
                G.State.D.lu.charge = G.State.MaxCharge("lu");
                G.RunEvent(sw.Interact(G));
                yield return Until(() => !G.InEvent && !Cutscene.Running && (ph == 3 ? G.State.Has("c10_boss_done") : b.Phase == ph + 1), 120, "방출 " + ph);
                Assert.That(G.State.D.limitReleases, Is.EqualTo(ph));
                Assert.That(G.State.MaxHp("lu"), Is.EqualTo(baseMax - ph * Mathf.RoundToInt(baseMax * 0.2f)));
                if (ph == 1)
                {
                    // 2단계에서 쓰러져 재시도 → 방출 1회가 다시 누적되지 않는다
                    G.State.D.lu.hp = 1; G.Player.Hurt(99, G.Player.transform.position + Vector3.right);
                    yield return Until(() => G.State.D.retries > 0 && !G.Loading && G.Player != null && !G.InEvent, 60, "재시도");
                    b = Find<AureliusBoss>();
                    yield return Until(() => b != null && b.Active, 20, "2단계 재개");
                    Assert.That(b.Phase, Is.EqualTo(2));
                    Assert.That(G.State.D.limitReleases, Is.EqualTo(1));
                    Assert.That(G.State.MaxHp("lu"), Is.EqualTo(baseMax - Mathf.RoundToInt(baseMax * 0.2f)));
                }
            }
            Assert.That(G.State.MaxHp("lu"), Is.EqualTo(Mathf.RoundToInt(baseMax * 0.4f)).Within(1));
            Assert.That(G.State.Has("aurelius_dead") && G.State.Has("meilin_dead") && G.State.Has("transfer_zero"), Is.True, "사망 확인표");
            Assert.That(G.State.D.slots, Does.Not.Contain("gather").Or.Contain("wire"), "임시 슬롯 되돌림");
        }

        // ================================================================ 11장 · 엔딩
        [UnityTest]
        public IEnumerator LastTalk_RavielDies_FreeChoices_ThenEndingDesktopWaitAndQuit()
        {
            yield return StartPreset("ch11_talk", "dead", false);
            MenuList.ScriptedChoices.Clear(); foreach (var c in new[] { 1, 0, 0, 0 }) MenuList.ScriptedChoices.Enqueue(c);   // 이름 → 손 → 눈 → 일어선다
            yield return Until(() => G.State.Has("c11_stood") && !G.InEvent && !Cutscene.Running, 180, "중정");
            Assert.That(G.State.D.ravielState, Is.EqualTo("dead"));
            Assert.That(G.Companion, Is.Null);
            Assert.That(G.State.Has("c11_name") && G.State.Has("c11_hand") && G.State.Has("c11_eyes"), Is.True);
            // 회상은 실제로 겪은 장면만 (이 프리셋은 008 에게 과자를 주지 않았다)
            Assert.That(StoryEvents.MontageLines(G), Does.Not.Contain("c11_mem_008"));
            EndingScreens.WaitSeconds = 0.4f;
            G.GoToMap("M47", "start");
            yield return Until(() => G.CurrentMapId == "M47" && !G.Loading, 30, "잔해 더미");
            MenuList.ScriptedChoices.Clear(); MenuList.ScriptedChoices.Enqueue(0);
            GameInput.Virtual.Skip = true;   // 크레딧 빨리 넘김
            G.RunEvent(StoryEvents.Run(G, "c11_last"));
            yield return Until(() => EndingScreens.DesktopShown, 120, "가상 바탕화면");
            GameInput.Virtual.Skip = false;
            foreach (var c in new[] { 1, 1, 1, 0 }) MenuList.ScriptedChoices.Enqueue(c);   // 대기 ×3 → 종료
            yield return Until(() => EndingScreens.WindowClosed, 30, "창 닫힘");
            Assert.That(EndingScreens.WaitCount, Is.EqualTo(3), "대기는 같은 선택으로 돌아온다");
            yield return Secs(4f);
            MenuList.ScriptedChoices.Enqueue(0);   // 타이틀로
            yield return Until(() => G.State == null, 20, "타이틀");
        }

        // ================================================================ 검수 도구
        [UnityTest]
        public IEnumerator Review_JumpUsesSeparateSave_CleansUp_AndRestoresStart()
        {
            yield return StartPreset("ch4_lift", "ally");
            G.SaveNow("normal_point", false);
            string normal = SaveSystem.MainPath;
            var stamp = File.ReadAllText(normal);
            var startMap = G.CurrentMapId;
            // 진행 중 대화가 있는 상태에서 이동
            G.RunEvent(G.Say("c4_seal_look"));
            ReviewMode.GoEvent(G, WorldLayout.I.GetEvent("ch8_reunion"), "ally_bulkhead");
            yield return Until(() => G.CurrentMapId == "M32" && !G.Loading && G.Player != null, 40, "검수 이동");
            yield return Until(() => !G.InEvent && !Cutscene.Running, 60, "도입");
            Assert.That(ReviewMode.Active, Is.True);
            Assert.That(SaveSystem.Directory, Does.EndWith("review"));
            Assert.That(File.ReadAllText(normal), Is.EqualTo(stamp), "일반 저장은 검수 이동으로 바뀌지 않는다");
            Assert.That(G.Companion, Is.Null, "죽은 베넬이 추종자로 생기지 않는다");
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(GameInput.Context, Is.EqualTo(InputCtx.Gameplay));
            Assert.That(UnityEngine.Object.FindObjectsByType<EnemyShot>().Length, Is.EqualTo(0));
            G.SaveNow("review_point", false);
            Assert.That(File.Exists(Path.Combine(dir, "review", SaveSystem.FileName)), Is.True);
            ReviewMode.End(G, true);
            yield return Until(() => G.CurrentMapId == startMap && !G.Loading && G.Player != null, 40, "검수 전 상태로 복귀");
            Assert.That(ReviewMode.Active, Is.False);
            Assert.That(SaveSystem.Directory, Is.EqualTo(dir));
            Assert.That(G.State.D.venelleState, Is.EqualTo("ally"));
        }
    }
}

