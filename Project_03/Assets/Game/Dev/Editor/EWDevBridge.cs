using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace EW.Dev
{
    /// <summary>
    /// 개발 자동화용 파일 명령 연결 (게임 기능 아님, 지워도 됨).
    /// 열린 에디터에서 Temp/EWBridge/cmd.txt 에 명령을 쓰면 실행하고 Temp/EWBridge/result.txt 에 결과를 남긴다.
    /// 명령: refresh / setup / setup_force / test editmode / test playmode / build / shot 씬경로 초 파일 / call 형식.메서드
    /// 게임 코드에 컴파일 오류가 있어도 동작하도록 게임 어셈블리를 직접 참조하지 않는다.
    /// </summary>
    [InitializeOnLoad]
    public static class EWDevBridge
    {
        static string Dir => Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/EWBridge"));
        static string CmdFile => Path.Combine(Dir, "cmd.txt");
        static string ResultFile => Path.Combine(Dir, "result.txt");
        static double next;

        static EWDevBridge()
        {
            Directory.CreateDirectory(Dir);
            EditorApplication.update += Tick;
            EditorApplication.update += RecTick;
            CompilationPipeline.assemblyCompilationFinished += OnCompiled;
            Log("bridge loaded; playing=" + EditorApplication.isPlaying);
        }

        public static void Log(string s)
        {
            try { File.AppendAllText(ResultFile, DateTime.Now.ToString("HH:mm:ss") + " " + s + "\n", Encoding.UTF8); } catch { }
        }

        static void OnCompiled(string asm, CompilerMessage[] msgs)
        {
            var errs = msgs.Where(m => m.type == CompilerMessageType.Error).ToArray();
            if (errs.Length > 0)
            {
                Log("COMPILE ERRORS in " + Path.GetFileName(asm) + ": " + errs.Length);
                foreach (var e in errs.Take(30)) Log("  " + e.file + "(" + e.line + "): " + e.message);
            }
        }

        static void Tick()
        {
            if (EditorApplication.timeSinceStartup < next) return;
            next = EditorApplication.timeSinceStartup + 0.5;
            PendingShot();
            if (!File.Exists(CmdFile)) return;
            string cmd;
            try { cmd = File.ReadAllText(CmdFile).Trim(); File.Delete(CmdFile); } catch { return; }
            if (string.IsNullOrEmpty(cmd)) return;
            Log("CMD " + cmd);
            try { Run(cmd); }
            catch (Exception e) { Log("ERROR " + e); }
        }

        static void Run(string cmd)
        {
            var parts = cmd.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            switch (parts[0])
            {
                case "ping": Log("pong compiling=" + EditorApplication.isCompiling + " playing=" + EditorApplication.isPlaying); break;
                case "refresh": AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate); Log("refreshed"); break;
                case "setup": Call("EW.EditorTools.EWMenu", "RunSetup", false); break;
                case "setup_force": Call("EW.EditorTools.EWMenu", "RunSetup", true); break;
                case "build": Call("EW.EditorTools.EWMenu", "BuildWindows"); break;
                case "call": Call(parts[1].Substring(0, parts[1].LastIndexOf('.')), parts[1].Substring(parts[1].LastIndexOf('.') + 1)); break;
                case "test": RunTests(parts.Length > 1 && parts[1] == "playmode" ? TestMode.PlayMode : TestMode.EditMode, parts.Length > 2 ? parts[2] : null); break;
                case "shot":
                    SessionState.SetInt("ew_rec_stage", 0);
                    SessionState.SetString("ew_shot_scene", parts[1]);
                    SessionState.SetFloat("ew_shot_secs", float.Parse(parts[2]));
                    SessionState.SetString("ew_shot_file", parts[3]);
                    SessionState.SetString("ew_shot_call", parts.Length > 4 ? parts[4] : "");
                    SessionState.SetInt("ew_shot_stage", 1);
                    break;
                case "rec":
                    SessionState.SetInt("ew_shot_stage", 0);
                    // rec 씬경로 총초 간격초 폴더 [형식.메서드] — 실제 시간으로 재생하며 일정 간격으로 화면을 저장
                    SessionState.SetString("ew_rec_scene", parts[1]);
                    SessionState.SetFloat("ew_rec_secs", float.Parse(parts[2]));
                    SessionState.SetFloat("ew_rec_int", float.Parse(parts[3]));
                    SessionState.SetString("ew_rec_dir", parts[4]);
                    SessionState.SetString("ew_rec_call", parts.Length > 5 ? parts[5] : "");
                    SessionState.SetInt("ew_rec_stage", 1);
                    break;
                case "stop": EditorApplication.isPlaying = false; SessionState.SetInt("ew_rec_stage", 0); SessionState.SetInt("ew_shot_stage", 0); Log("stopped"); break;
                case "clearlog": File.WriteAllText(ResultFile, ""); break;
                default: Log("unknown command"); break;
            }
        }

        static void Call(string type, string method, params object[] args)
        {
            var t = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(type)).FirstOrDefault(x => x != null);
            if (t == null) { Log("type not found: " + type); return; }
            var m = t.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (m == null) { Log("method not found: " + method); return; }
            var r = m.Invoke(null, args.Length == m.GetParameters().Length ? args : null);
            Log("RESULT " + type + "." + method + " => " + r);
        }

        // ---------------- 테스트
        static void RunTests(TestMode mode, string filter)
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new Callbacks());
            var f = new Filter { testMode = mode };
            if (!string.IsNullOrEmpty(filter)) f.groupNames = new[] { filter };
            api.Execute(new ExecutionSettings(f));
            Log("tests started: " + mode);
        }

        class Callbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                Log("TESTS FINISHED pass=" + result.PassCount + " fail=" + result.FailCount + " skip=" + result.SkipCount + " inconclusive=" + result.InconclusiveCount + " duration=" + result.Duration.ToString("0.0") + "s");
            }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.HasChildren) return;
                string st = result.TestStatus.ToString().ToUpper();
                Log("  [" + st + "] " + result.FullName + (result.TestStatus == TestStatus.Failed ? " :: " + Trim(result.Message) + " @ " + Trim(result.StackTrace, 400) : ""));
            }
            static string Trim(string s, int n = 600) => string.IsNullOrEmpty(s) ? "" : (s.Length > n ? s.Substring(0, n) : s).Replace("\n", " | ");
        }

        // ---------------- 실시간 녹화 (프레임 PNG)
        static void RecTick()
        {
            int stage = SessionState.GetInt("ew_rec_stage", 0);
            if (stage == 0) return;
            double now = EditorApplication.timeSinceStartup;
            if (stage == 1)
            {
                if (EditorApplication.isPlaying) { EditorApplication.isPlaying = false; return; }
                EditorSceneManager.OpenScene(SessionState.GetString("ew_rec_scene", ""));
                SessionState.SetInt("ew_rec_stage", 2);
                EditorApplication.isPlaying = true;
                return;
            }
            if (!EditorApplication.isPlaying) return;
            if (stage == 2)
            {
                if (Time.frameCount < 5) return;
                string call = SessionState.GetString("ew_rec_call", "");
                if (!string.IsNullOrEmpty(call)) { int dot = call.LastIndexOf('.'); try { Call(call.Substring(0, dot), call.Substring(dot + 1)); } catch (Exception e) { Log("call failed " + e.Message); } }
                SessionState.SetFloat("ew_rec_t0", (float)now);
                SessionState.SetFloat("ew_rec_next", (float)now);
                SessionState.SetInt("ew_rec_frame", 0);
                SessionState.SetFloat("ew_rec_status", (float)now);
                Directory.CreateDirectory(SessionState.GetString("ew_rec_dir", "rec"));
                SessionState.SetInt("ew_rec_stage", 3);
                return;
            }
            if (stage == 3)
            {
                float t0 = SessionState.GetFloat("ew_rec_t0", (float)now);
                if (now >= SessionState.GetFloat("ew_rec_next", 0))
                {
                    int f = SessionState.GetInt("ew_rec_frame", 0);
                    ScreenCapture.CaptureScreenshot(Path.Combine(SessionState.GetString("ew_rec_dir", "rec"), "f_" + f.ToString("0000") + ".png"));
                    SessionState.SetInt("ew_rec_frame", f + 1);
                    SessionState.SetFloat("ew_rec_next", SessionState.GetFloat("ew_rec_next", 0) + SessionState.GetFloat("ew_rec_int", 0.5f));
                }
                if (now - SessionState.GetFloat("ew_rec_status", 0) > 10)
                {
                    SessionState.SetFloat("ew_rec_status", (float)now);
                    try { Call("EW.DevHooks", "Status"); } catch { }
                }
                if (now - t0 > SessionState.GetFloat("ew_rec_secs", 10))
                {
                    try { Call("EW.DevHooks", "Status"); } catch { }
                    SessionState.SetInt("ew_rec_stage", 4);
                    SessionState.SetFloat("ew_rec_end", (float)now + 1.5f);
                }
                return;
            }
            if (stage == 4 && now >= SessionState.GetFloat("ew_rec_end", 0))
            {
                EditorApplication.isPlaying = false;
                SessionState.SetInt("ew_rec_stage", 0);
                Log("rec done frames=" + SessionState.GetInt("ew_rec_frame", 0));
            }
        }

        // ---------------- 플레이 화면 캡처
        static double shotAt;
        static void PendingShot()
        {
            int stage = SessionState.GetInt("ew_shot_stage", 0);
            if (stage == 0) return;
            if (stage == 1)
            {
                if (EditorApplication.isPlaying) { EditorApplication.isPlaying = false; return; }
                EditorSceneManager.OpenScene(SessionState.GetString("ew_shot_scene", ""));
                SessionState.SetInt("ew_shot_stage", 2);
                EditorApplication.isPlaying = true;
                return;
            }
            if (stage == 2 && EditorApplication.isPlaying)
            {
                string call = SessionState.GetString("ew_shot_call", "");
                if (!string.IsNullOrEmpty(call))
                {
                    if (EditorApplication.timeSinceStartup - SessionState.GetFloat("ew_shot_t0", 0) < 0) return;
                    int dot = call.LastIndexOf('.');
                    try { Call(call.Substring(0, dot), call.Substring(dot + 1)); } catch (Exception e) { Log("call failed " + e.Message); }
                }
                SessionState.SetInt("ew_shot_stage", 3);
                shotAt = EditorApplication.timeSinceStartup + SessionState.GetFloat("ew_shot_secs", 3f);
                return;
            }
            if (stage == 3 && EditorApplication.isPlaying)
            {
                if (shotAt == 0) shotAt = EditorApplication.timeSinceStartup + 2;
                if (EditorApplication.timeSinceStartup < shotAt) return;
                string file = SessionState.GetString("ew_shot_file", "shot.png");
                ScreenCapture.CaptureScreenshot(file);
                Log("screenshot requested " + file);
                SessionState.SetInt("ew_shot_stage", 4);
                shotAt = EditorApplication.timeSinceStartup + 1.5;
                return;
            }
            if (stage == 4 && EditorApplication.timeSinceStartup >= shotAt)
            {
                EditorApplication.isPlaying = false;
                SessionState.SetInt("ew_shot_stage", 0);
                Log("shot done");
            }
        }
    }
}
