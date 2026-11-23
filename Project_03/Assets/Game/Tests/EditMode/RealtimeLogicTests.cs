using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace EW.Tests
{
    /// <summary>
    /// 실시간 잠입 개편 (2026-11-14) 의 데이터·규칙 검사. 장면이 필요 없는 것만 (판정·입력·카메라는 PlayMode).
    /// 유지하는 것: 저장·진행·보상 중복 방지. 폐기한 것: 턴 순서·턴 시작 회복·턴 단위 비용.
    /// </summary>
    public class RealtimeLogicTests
    {
        static ActionTuning Tuning()
        {
            var ta = Resources.Load<TextAsset>("EW/action_tuning");
            Assert.That(ta, Is.Not.Null, "Resources/EW/action_tuning.json");
            var t = ActionTuning.Parse(ta.text);
            Assert.That(t, Is.Not.Null);
            return t;
        }

        [Test]
        public void Tuning_AllSkillsHaveSecondsBasedTiming_AndBasicAttackIsFree()
        {
            var t = Tuning();
            foreach (var id in new[] { "tok", "spark_shot", "jam", "pull" })
            {
                var s = t.Skill(id);
                Assert.That(s, Is.Not.Null, id);
                Assert.That(s.cooldown, Is.GreaterThan(0f), id + " 재사용 시간");
                Assert.That(s.windup, Is.GreaterThan(0f), id + " 준비 동작");
                Assert.That(s.recovery, Is.GreaterThanOrEqualTo(0f), id);
            }
            // 루는 자체 발전: 기본 공격은 전하·외부 전원 없이 쓴다
            Assert.That(t.Skill("tok").cost, Is.EqualTo(0));
            Assert.That(t.Skill("tok").kind, Is.EqualTo("attack"));
            // 타깃형 두 가지는 입력 방식이 다르다 (길게 눌러 대상 선택)
            Assert.That(PlayerSkills.IsTargeted(t.Skill("jam")), Is.True);
            Assert.That(PlayerSkills.IsTargeted(t.Skill("pull")), Is.True);
            Assert.That(PlayerSkills.IsTargeted(t.Skill("tok")), Is.False);
            Assert.That(t.player.chargeRegenSeconds, Is.GreaterThan(0f));
        }

        [Test]
        public void Tuning_EnemiesTelegraph_AndCannotStunLock()
        {
            var t = Tuning();
            foreach (var e in t.enemies)
            {
                Assert.That(e.attackWindup, Is.GreaterThanOrEqualTo(0.5f), e.id + ": 공격은 준비 동작으로 예고");
                Assert.That(e.maxStaggers, Is.GreaterThanOrEqualTo(1), e.id);
                Assert.That(e.staggerImmune, Is.GreaterThan(0f), e.id + ": 경직 잠금 방지");
                Assert.That(e.suspicionRate, Is.LessThan(2f), e.id + ": 잠깐 닿았다고 즉시 발견되지 않음");
                Assert.That(e.rebootTime, Is.GreaterThan(0f), e.id + ": 쓰러뜨려도 일시 무력화");
            }
            Assert.That(t.player.hurtInvuln, Is.GreaterThan(0.5f), "피격 후 무적 시간");
            Assert.That(t.player.dashCooldownFromEnd, Is.InRange(0.1f, 0.2f)); Assert.That(t.player.dodgeTime, Is.InRange(0.12f, 0.18f));
            Assert.That(t.player.dodgeDistance, Is.GreaterThan(1f));
            Assert.That(t.venelle.firstDashWindup, Is.GreaterThan(t.venelle.dashWindup), "첫 돌진은 피할 준비 시간이 더 길다");
            Assert.That(t.Difficulty("story").targetingTimeScale, Is.LessThanOrEqualTo(0.15f), "기본 난이도: 조준 중 위험 진행을 충분히 늦춘다");
        }

        [Test]
        public void Rewards_NoKillExp_QuietAndClueRewardsGrantedOnce()
        {
            var t = Tuning();
            Assert.That(t.expQuiet, Is.GreaterThan(0)); Assert.That(t.expClue, Is.GreaterThan(0)); Assert.That(t.expSection, Is.GreaterThan(0));
            var gs = T.State();
            gs.Reward("clue_board", 8);
            int exp = gs.D.lu.exp;
            gs.Reward("clue_board", 8);
            Assert.That(gs.D.lu.exp, Is.EqualTo(exp), "같은 보상은 한 번만");
            // 적 처치로 경험치를 주는 코드가 없어야 한다 (Guard 는 경험치를 다루지 않는다)
            string guard = File.ReadAllText(Path.Combine(Application.dataPath, "Game/Scripts/Runtime/Action/Guard.cs"));
            Assert.That(guard.Contains("GainExp") || guard.Contains("Reward("), Is.False);
        }

        [Test]
        public void TimeCtl_RestoresExactlyOne_AfterAllRequestsCleared()
        {
            TimeCtl.ClearAll();
            TimeCtl.Set("targeting", 0.08f);
            TimeCtl.Set("hitstop", 0f);
            Assert.That(Time.timeScale, Is.EqualTo(0f));
            TimeCtl.Clear("hitstop");
            Assert.That(Time.timeScale, Is.EqualTo(0.08f).Within(1e-5));
            TimeCtl.Clear("targeting");
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        [Test]
        public void SaveV2_MigratesToV3_MovesOldBossRoomSave_AndKeepsOriginal()
        {
            var d = SaveData.NewGame();
            d.version = 2; d.mapId = "M13"; d.flags.Add("c1_left_lab"); d.flags.Add("op_tank_done");
            d.clearedEncounters.Add("C02_guard");
            SaveSystem.Migrate(d);
            Assert.That(d.version, Is.EqualTo(SaveSystem.CurrentVersion));
            Assert.That(d.mapId, Is.EqualTo("C06"), "베넬 만남 전 혼합실 저장은 새 복도로");
            Assert.That(d.flags, Does.Contain("lab_alarm"));
            Assert.That(d.flags, Does.Contain("lab_realized"));
            Assert.That(d.clearedEncounters, Does.Contain("C02_guard"), "옛 기록은 지우지 않는다");
            Assert.That(d.skills, Does.Contain("tok"));

            // 파일로 읽을 때 원본은 따로 보존
            string dir = Path.Combine(Path.GetTempPath(), "ew_mig_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                SaveSystem.OverrideDirectory = dir;
                var old = SaveData.NewGame(); old.version = 2; old.mapId = "C03";
                File.WriteAllText(SaveSystem.MainPath, JsonUtility.ToJson(old));
                var r = SaveSystem.TryLoad(out var loaded, out _);
                Assert.That(r, Is.EqualTo(SaveSystem.LoadResult.Ok));
                Assert.That(File.Exists(SaveSystem.MainPath + ".v2_original"), Is.True, "변환 전 원본 보존");
                Assert.That(loaded.version, Is.EqualTo(SaveSystem.CurrentVersion));
                Assert.That(loaded.slots, Is.EqualTo(PlayerSkills.DefaultSlots), "v4: Q/W/E/R 기본 슬롯");
            }
            finally { SaveSystem.OverrideDirectory = null; try { Directory.Delete(dir, true); } catch { } }
        }

        [Test]
        public void TurnBasedBattle_IsGoneFromPlay()
        {
            string root = Path.Combine(Application.dataPath, "Game/Scripts/Runtime");
            Assert.That(Directory.Exists(Path.Combine(root, "Battle")), Is.False, "턴제 전투 코드 제거");
            foreach (var f in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                string t = File.ReadAllText(f);
                Assert.That(t.Contains("StartEncounter") || t.Contains("BattleController"), Is.False, Path.GetFileName(f));
            }
        }

        [Test]
        public void Dialogue_NewLinesExist_UserTextVerbatim()
        {
            var json = File.ReadAllText(Path.Combine(Application.dataPath, "Game/Data/Dialogue/dialogue_ko.json"));
            var db = DialogueDB.Parse(json);
            Assert.That(db.errors, Is.Empty);
            string Text(string id) => string.Join("|", db.Get(id).lines.Select(l => l.t));
            Assert.That(Text("lab_wake"), Is.EqualTo("여기 어디야...?|무서워..."));
            Assert.That(Text("lab_realize"), Is.EqualTo("에...?|연구실...?|나, 연구실에 끌려온 거야...?"));
            Assert.That(Text("lab_broadcast"), Is.EqualTo("격리 구역 이탈 감지.|실험체 E-D05-47.|보안 요원은 해당 구역을 봉쇄하고 실험체를 확보하십시오."));
            Assert.That(Text("v_run_in"), Is.EqualTo("헉... 헉..."));
            Assert.That(Text("v_meet_1"), Is.EqualTo("어라라?|찾으라고 했던 그 꼬맹이 아냐아아?"));
            // 2026-11-15 사용자 지정 문구 (그대로)
            Assert.That(Text("op_h_a") + "|" + Text("op_h_b") + "|" + Text("op_h_c") + "|" + Text("op_h_d"), Is.EqualTo("나... 로젤리나...|네가 없으면 난 어떡해..?|...|.......|이대로,... 이대로 그저 사라지고 싶어."));
            Assert.That(Text("esc_1") + "|" + Text("esc_2") + "|" + Text("esc_3") + "|" + Text("esc_4"), Is.EqualTo("여긴... 실험실?|일단... 밖으로 나가자.|내가 또 이런 곳에 잡혀오다니...|젠장... 뭐가 됐든 여길 나가야겠어..."));
            Assert.That(Text("v_intro_1") + "|" + Text("v_intro_2") + "|" + Text("v_intro_3"), Is.EqualTo("아이 젠장... 뭘 이렇게—딸꾹—많이 시키는 거야...|뭔 꼬맹이를 찾으라고...|난 원래 이 담당도 아니거든, 알아?|그런데...|내가 그 꼬맹이를 찾은 것 같네!!!!"));
            Assert.That(Text("w_first_1") + "|" + Text("w_first_2") + "|" + Text("w_first_3") + "|" + Text("w_first_4"), Is.EqualTo("흠... 여긴 길이 끊겼네?|괜찮아. 나는 이런 전선을 타고 이동할 수 있으니까.|이얍!|엥?!|와! 너 이런 능력도 있어?|응, 신기하지?"));
            Assert.That(db.Get("v_down_ask").choices, Is.EqualTo(new[] { "죽인다.", "죽이지 않는다." }));
            foreach (var id in new[] { "v_grab_first", "v_talk_open", "r_8" })
                Assert.That(string.Join("", db.Get(id).choices).Contains("린든"), Is.False, id);
            // 루가 아직 모르는 린든의 이름을 정답 선택지로 쓰지 않는다
            foreach (var id in new[] { "v_talk1", "v_talk2", "v_talk3" })
            {
                var e = db.Get(id);
                Assert.That(e.choices.Length, Is.InRange(2, 3), id);
                Assert.That(string.Join("", e.choices).Contains("린든"), Is.False, id);
            }
            // 깨달음 대사는 어떤 단서를 먼저 봐도 어색하지 않게 단서 이름을 직접 말하지 않는다
            Assert.That(Text("lab_realize").Contains("도식") || Text("lab_realize").Contains("번호") || Text("lab_realize").Contains("표지판"), Is.False);
        }

        [Test]
        public void InputBindings_HaveDefaults_AndLabelsAreReal()
        {
            Assert.That(GameInput.DefaultKeyOf(Act.Jump), Is.EqualTo(UnityEngine.InputSystem.Key.Space), "Space 점프");
            Assert.That(GameInput.DefaultKeyOf(Act.Interact), Is.EqualTo(UnityEngine.InputSystem.Key.Space), "Space 조사·대화");
            Assert.That(GameInput.DefaultKeyOf(Act.Skill1), Is.EqualTo(UnityEngine.InputSystem.Key.Q));
            Assert.That(GameInput.DefaultKeyOf(Act.Skill2), Is.EqualTo(UnityEngine.InputSystem.Key.E));
            Assert.That(GameInput.DefaultKeyOf(Act.Skill3), Is.EqualTo(UnityEngine.InputSystem.Key.R));
            Assert.That(GameInput.DefaultKeyOf(Act.Skill4), Is.EqualTo(UnityEngine.InputSystem.Key.F));
            Assert.That(GameInput.DefaultKeyOf(Act.Dodge), Is.EqualTo(UnityEngine.InputSystem.Key.None));
            Assert.That(GameInput.DefaultKeyOf(Act.Down), Is.EqualTo(UnityEngine.InputSystem.Key.S));
            Assert.That(GameInput.DefaultKeyOf(Act.Cancel), Is.EqualTo(UnityEngine.InputSystem.Key.Escape));
            Assert.That(GameInput.DefaultKeyOf(Act.Skip), Is.EqualTo(UnityEngine.InputSystem.Key.Enter), "Enter 는 컷신 건너뛰기 전용");
            // 필드에서 같은 키를 두 동작이 쓰지 않는다 (메뉴 전용 '위'는 W 와 겹쳐도 됨)
            foreach (var a in GameInput.AllActs) foreach (var b in GameInput.AllActs)
                    if (a < b && GameInput.DefaultKeyOf(a) == GameInput.DefaultKeyOf(b)) Assert.That(GameInput.CanShare(a, b), Is.True, a + "/" + b);
            Assert.That(GameInput.CanShare(Act.MenuUp,Act.Skill2),Is.False,"위 이동과 스킬 키 중복 금지");
            Assert.That(GameInput.CanShare(Act.Jump,Act.Interact),Is.True,"Space 문맥 동작 허용");
            // 기본 슬롯: Q 기본 공격 / W 간섭 / E 외부 전력 / R 전원선
            Assert.That(PlayerSkills.DefaultSlots, Is.EqualTo(new[] { "tok", "jam", "pull", "wire" }));
            Assert.That(ActionTuning.I.Skill("tok").cost, Is.EqualTo(0), "기본 공격은 외부 전원 없이");
            Assert.That(ActionTuning.I.Skill("wire").kind, Is.EqualTo("wire"));
            // Z 를 필수 조작으로 남기지 않는다
            foreach (var a in GameInput.AllActs) Assert.That(GameInput.DefaultKeyOf(a), Is.Not.EqualTo(UnityEngine.InputSystem.Key.Z));
            // 안내 문구에 고정된 키 이름을 박아 두지 않는다 (예: "Z 조사")
            string root = Path.Combine(Application.dataPath, "Game/Scripts/Runtime");
            foreach (var f in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                string t = File.ReadAllText(f);
                Assert.That(Regex.IsMatch(t, "\"Z "), Is.False, Path.GetFileName(f) + " 에 'Z ' 안내");
            }
        }
    }
}
