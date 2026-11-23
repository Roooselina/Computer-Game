using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EW.Tests
{
    /// <summary>
    /// AA/DD 대시의 실제 재사용 간격 측정 (실제 플레이어 + 가상 키 입력, 사람의 리듬을 흉내 낸 누름/뗌).
    /// 결과를 Temp/EWBridge/dash_metrics.txt 에 남긴다 (수정 전후 비교용). 무한 무적이 되지 않는지도 본다.
    /// </summary>
    public class DashTimingTests
    {
        string dir;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "ew_dash_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            SaveSystem.OverrideDirectory = dir;
            GameInput.AutoAdvance = false; GameInput.AutoReadDelay = 0;
            GameInput.Virtual.ReleaseAll(); GameInput.ClearContexts();
            MenuList.ScriptedChoices.Clear(); TimeCtl.ClearAll(); Hazard.ForcePause = false;
        }

        [TearDown]
        public void TearDown()
        {
            GameInput.Virtual.ReleaseAll();
            if (Game.I != null) UnityEngine.Object.DestroyImmediate(Game.I.gameObject);
            if (PixelScreen.I != null) UnityEngine.Object.DestroyImmediate(PixelScreen.I.gameObject);
            if (AudioManager.I != null) UnityEngine.Object.DestroyImmediate(AudioManager.I.gameObject);
            GameInput.ClearContexts(); TimeCtl.ClearAll();
            SaveSystem.OverrideDirectory = null;
            try { Directory.Delete(dir, true); } catch { }
        }

        static IEnumerator Until(Func<bool> c, float timeout, string what)
        {
            float t = 0;
            while (!c()) { t += Time.unscaledDeltaTime; if (t > timeout) Assert.Fail("시간 초과: " + what); yield return null; }
        }

        static string Metrics => Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/EWBridge/dash_metrics.txt"));
        static void Out(string s) { try { Directory.CreateDirectory(Path.GetDirectoryName(Metrics)); File.AppendAllText(Metrics, DateTime.Now.ToString("HH:mm:ss ") + s + Environment.NewLine); } catch { } }

        IEnumerator Start()
        {
            SceneManager.LoadScene("Title");
            yield return Until(() => Game.I != null, 10, "Game");
            yield return null;
            var d = SaveData.NewGame();
            d.flags.AddRange(RealtimeStealthTests.AfterAlarm);
            Game.I.State = new GameState(d, Game.I.Db);
            Game.I.GoToMap("C03", "start");
            yield return Until(() => Game.I.Map != null && Game.I.Map.mapId == "C03" && Game.I.Player != null && !Game.I.Loading, 25, "C03");
            yield return Until(() => !Game.I.InEvent && !Cutscene.Running, 30, "intro");
            foreach (var e in UnityEngine.Object.FindObjectsByType<Guard>()) UnityEngine.Object.Destroy(e.gameObject);
            foreach (var e in UnityEngine.Object.FindObjectsByType<Cctv>()) UnityEngine.Object.Destroy(e.gameObject);
            yield return null;
        }

        /// <summary>같은 방향을 일정한 리듬으로 계속 두드린다 (press 시간, release 시간). 대시 시작 시각을 모은다.</summary>
        IEnumerator Drum(Act dir, float press, float release, float total, List<float> starts, List<float> invuln)
        {
            var p = Game.I.Player;
            float t0 = Time.time; bool wasDodging = p.Dodging; bool down = false; float phase = 0;
            while (Time.time - t0 < total)
            {
                phase += Time.deltaTime;
                if (!down && phase >= release) { down = true; phase = 0; GameInput.Virtual.Hold(dir, true); }
                else if (down && phase >= press) { down = false; phase = 0; GameInput.Virtual.Hold(dir, false); }
                yield return null;
                if (p.Dodging && !wasDodging) starts.Add(Time.time - t0);
                wasDodging = p.Dodging;
                invuln.Add(p.InvulnLeft > 0 ? 1 : 0);
            }
            GameInput.Virtual.ReleaseAll();
        }

        [UnityTest]
        public IEnumerator Dash_ReuseInterval_Distance_PostLock_InvulnShare()
        {
            yield return Start();
            var g = Game.I; var p = g.Player; var T = ActionTuning.I.player;
            Out("---- 설정: dodgeTime=" + T.dodgeTime + " dodgeDistance=" + T.dodgeDistance + " dodgeInvuln=" + T.dodgeInvuln + " doubleTapWindow=" + T.doubleTapWindow
                + " (cooldownFromEnd=" + T.dashCooldownFromEnd + " invulnRearm=" + T.dashInvulnRearm + ")");
            p.Teleport(new Vector2(4f, p.transform.position.y)); p.ResetCombatTimers();
            yield return new WaitForSeconds(0.3f);

            // 1) 단일 대시: 실제 이동 거리·지속·끝난 뒤 입력이 바로 이어지는지
            float x0 = p.transform.position.x;
            GameInput.Virtual.Tap(Act.Right); yield return null; yield return null;
            GameInput.Virtual.Hold(Act.Right, true); yield return null;
            Assert.That(p.Dodging, Is.True, "DD 두 번째 누름에 즉시 대시");
            float ds = Time.time;
            while (p.Dodging) yield return null;
            float dur = Time.time - ds;
            float xEnd = p.transform.position.x;
            // 대시가 끝난 바로 다음 프레임: 누르고 있는 D 로 걷기 속도가 바로 나오는가 (이동 후딜 없음)
            yield return null;
            float vAfter = p.Velocity.x;
            GameInput.Virtual.ReleaseAll();
            Out(string.Format("단일 대시: 지속 {0:0.000}s, 거리 {1:0.00}u (대시만), 끝난 다음 프레임 속도 {2:0.00} (걷기 {3})", dur, xEnd - x0, vAfter, T.walk));
            Assert.That(vAfter, Is.EqualTo(T.walk).Within(0.05f), "대시 뒤 이동 잠금 없음");

            // 2) 사람 리듬으로 연타: DD DD DD ... (누름 0.06, 뗌 0.07)
            yield return new WaitForSeconds(0.8f);
            p.Teleport(new Vector2(3f, p.transform.position.y)); p.ResetCombatTimers();
            yield return new WaitForSeconds(0.2f);
            var starts = new List<float>(); var inv = new List<float>();
            yield return Drum(Act.Right, 0.06f, 0.07f, 1.6f, starts, inv);
            yield return new WaitForSeconds(0.5f);
            p.Teleport(new Vector2(30f, p.transform.position.y)); p.ResetCombatTimers();
            yield return new WaitForSeconds(0.2f);
            var startsL = new List<float>(); var invL = new List<float>();
            yield return Drum(Act.Left, 0.06f, 0.07f, 1.6f, startsL, invL);
            float Mean(List<float> s) => s.Count < 2 ? -1 : (s.Last() - s.First()) / (s.Count - 1);
            float invShare = inv.Count == 0 ? 0 : inv.Average();
            Out(string.Format("연타 DD 1.6s: 대시 {0}회, 평균 간격 {1:0.000}s, 시작 [{2}] / 무적 비율 {3:0.00}", starts.Count, Mean(starts), string.Join(", ", starts.Select(v => v.ToString("0.00"))), invShare));
            Out(string.Format("연타 AA 1.6s: 대시 {0}회, 평균 간격 {1:0.000}s", startsL.Count, Mean(startsL)));
            Assert.That(starts.Count, Is.GreaterThanOrEqualTo(2), "연타로 대시가 다시 나간다");
            Assert.That(invShare, Is.LessThan(0.6f), "대시를 이어 써도 무적이 계속 유지되지 않는다");

            // 3) AA → DD 방향 바꾸기 재입력: 첫 대시 직후 반대쪽 DD
            yield return new WaitForSeconds(0.6f);
            p.Teleport(new Vector2(16f, p.transform.position.y)); p.ResetCombatTimers();
            yield return new WaitForSeconds(0.2f);
            GameInput.Virtual.Tap(Act.Left); yield return new WaitForSeconds(0.06f);
            GameInput.Virtual.Tap(Act.Left); yield return null; yield return null;
            Assert.That(p.Dodging, Is.True, "AA");
            float a0 = Time.time;
            float dd = -1;
            for (int i = 0; i < 40 && dd < 0; i++)
            {
                GameInput.Virtual.Tap(Act.Right); yield return new WaitForSeconds(0.05f);
                GameInput.Virtual.Tap(Act.Right); yield return null; yield return null;
                if (p.Dodging && p.Velocity.x > 0) dd = Time.time - a0;
                else yield return new WaitForSeconds(0.04f);
            }
            Out(string.Format("AA → DD 재입력: 첫 대시 시작 뒤 {0:0.000}s 에 반대 대시", dd));
            Assert.That(dd, Is.GreaterThan(0), "AA → DD 재입력 가능");

            // 4) 막힌 곳(단단한 상자)을 향한 빠른 연속 대시: 뚫고 지나가지 않는다 (맵 끝은 출구라 쓰지 않는다)
            yield return new WaitForSeconds(0.4f);
            var crate = UnityEngine.Object.FindObjectsByType<Cover>().Select(c => c.GetComponent<Collider2D>()).Where(c => c != null && !c.isTrigger)
                .OrderBy(c => Mathf.Abs(c.bounds.center.x - 24f)).First();
            float face = crate.bounds.min.x;
            float y0 = p.transform.position.y;
            p.Teleport(new Vector2(face - 3.2f, y0)); p.ResetCombatTimers();
            yield return new WaitForSeconds(0.2f);
            var s4 = new List<float>(); var i4 = new List<float>();
            yield return Drum(Act.Right, 0.06f, 0.07f, 0.9f, s4, i4);
            yield return new WaitForSeconds(0.3f);
            float px4 = p.transform.position.x, py4 = p.transform.position.y;
            Out(string.Format("상자 쪽 연타: 대시 {0}회, 멈춘 x={1:0.00} y={2:0.00} (상자 왼쪽 면 {3:0.00}, 윗면 {4:0.00})", s4.Count, px4, py4, face, crate.bounds.max.y));
            bool stoppedBefore = px4 + PlayerController.Width / 2f <= face + 0.06f;
            bool onTop = py4 >= crate.bounds.max.y - 0.06f;
            Assert.That(stoppedBefore || onTop, Is.True, "상자 안으로 파고들지 않는다 (앞에서 멈추거나 위에 선다)");
            // 5) 연타 후 대화가 열리면 남은 탭이 대화 뒤 대시로 새지 않는다
            GameInput.Virtual.Tap(Act.Right); yield return null; yield return null;
            int h = GameInput.Push(InputCtx.Dialogue); yield return new WaitForSeconds(0.2f); GameInput.Pop(h);
            GameInput.Virtual.Tap(Act.Right); yield return null; yield return null;
            Assert.That(p.Dodging, Is.False, "대화 전 탭 + 대화 뒤 탭은 대시가 아니다");
        }
    }
}
