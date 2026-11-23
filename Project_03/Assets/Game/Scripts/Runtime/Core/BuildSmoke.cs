using System.Collections;
using System.IO;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 실행 파일 자체 점검 (개발 확인용): `ElectricWorld_LastLight.exe -ewSmoke` 로 실행하면
    /// 별도 임시 저장 폴더에서 검수 도구의 사건 프리셋으로 주요 신규 구간을 차례로 열고, 화면을 찍고, 결과를 적은 뒤 종료한다.
    /// 사용자 저장 폴더는 쓰지 않는다. 일반 실행(인자 없음)에서는 아무 일도 하지 않는다.
    /// 결과: %TEMP%/ew_smoke/smoke.txt, shot_*.png
    /// </summary>
    public static class BuildSmoke
    {
        static string dir;
        static readonly System.Text.StringBuilder log = new System.Text.StringBuilder();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-ewSmoke") < 0) return;
            dir = Path.Combine(Path.GetTempPath(), "ew_smoke");
            Directory.CreateDirectory(dir);
            foreach (var f in Directory.GetFiles(dir)) File.Delete(f);
            SaveSystem.OverrideDirectory = Path.Combine(dir, "save");
            var go = new GameObject("[Smoke]"); Object.DontDestroyOnLoad(go);
            go.AddComponent<Runner>().StartCoroutine(Run());
        }

        class Runner : MonoBehaviour { }

        static void Out(string s) { log.AppendLine(System.DateTime.Now.ToString("HH:mm:ss ") + s); File.WriteAllText(Path.Combine(dir, "smoke.txt"), log.ToString()); }

        static IEnumerator Wait(System.Func<bool> c, float t)
        {
            float k = 0; while (!c() && k < t) { k += Time.unscaledDeltaTime; yield return null; }
        }

        static IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(dir, "shot_" + name + ".png"));
            yield return null;
        }

        static IEnumerator Run()
        {
            Out("빌드 자체 점검 시작 (저장: " + SaveSystem.Directory + ")");
            yield return Wait(() => Game.I != null && Game.I.Config != null, 20);
            var g = Game.I;
            yield return new WaitForSeconds(1.5f);
            yield return Shot("00_title");
            Out("타이틀: " + (Game.I != null) + " 배치 데이터 맵 " + WorldLayout.I.Maps.Count + " 사건 " + WorldLayout.I.Events.Count + " 오류 " + WorldLayout.I.Errors.Count);
            // 정상 새 게임 시작점
            g.NewGame();
            yield return Wait(() => g.CurrentMapId == "P01" && !g.Loading, 20);
            Out("새 게임 시작 맵: " + g.CurrentMapId + " (오프닝)");
            yield return new WaitForSeconds(2f);
            yield return Shot("01_newgame");
            GameInput.AutoAdvance = true; GameInput.AutoReadDelay = 0.4f;
            string[] evs = { "ch7_meilin", "ch7_bulkhead", "ch8_reunion", "ch9_lacrimosa", "ch9_bin", "ch10_phase1", "ch11_talk", "ch11_ending" };
            foreach (var id in evs)
            {
                var e = WorldLayout.I.GetEvent(id);
                string ven = e.venelle != null && e.venelle.StartsWith("choose:") ? e.venelle.Substring(7).Split(',')[0] : "keep";
                ReviewMode.GoEvent(g, e, ven);
                yield return Wait(() => g.CurrentMapId == e.map && !g.Loading && g.Player != null, 30);
                yield return new WaitForSeconds(2.5f);
                yield return Shot(id);
                Out("검수 이동 " + id + " → " + g.CurrentMapId + " 검수모드=" + ReviewMode.Active + " 저장=" + SaveSystem.Directory + " 이벤트중=" + g.InEvent);
            }
            // 검수 시작 전으로 복귀
            ReviewMode.End(g, true);
            yield return Wait(() => !ReviewMode.Active && !g.Loading && g.Player != null, 30);
            yield return new WaitForSeconds(1.5f);
            Out("검수 전 상태 복귀: " + g.CurrentMapId + " 저장=" + SaveSystem.Directory + " 검수 슬롯 폴더 존재=" + Directory.Exists(Path.Combine(dir, "save", "review")));
            // 엔딩 가상 바탕화면
            EndingScreens.WaitSeconds = 1f;
            g.StartCoroutine(EndingScreens.Desktop(g));
            yield return Wait(() => EndingScreens.DesktopShown, 10);
            yield return new WaitForSecondsRealtime(1.6f);
            yield return Shot("90_desktop");
            MenuList.ScriptedChoices.Enqueue(1);
            yield return new WaitForSecondsRealtime(2.5f);
            MenuList.ScriptedChoices.Enqueue(0);
            yield return Wait(() => EndingScreens.WindowClosed, 10);
            yield return new WaitForSecondsRealtime(1f);
            yield return Shot("91_closed");
            Out("엔딩 화면: 대기 " + EndingScreens.WaitCount + "회, 창 닫힘 " + EndingScreens.WindowClosed);
            Out("끝");
            yield return new WaitForSecondsRealtime(1f);
            Application.Quit();
        }
    }
}
