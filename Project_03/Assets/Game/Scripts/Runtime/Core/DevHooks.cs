#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

namespace EW
{
    /// <summary>개발용 화면 확인 도우미 (에디터 전용). 게임 진행에는 쓰이지 않는다.</summary>
    public static class DevHooks
    {
        // ---- 스크린샷용 준비 (맵 씬을 바로 실행했을 때)
        class Runner : MonoBehaviour { }
        static Runner runner;
        static Coroutine Run(IEnumerator e) { DevSave(); if (runner == null) { var go = new GameObject("[DevRunner]"); Object.DontDestroyOnLoad(go); runner = go.AddComponent<Runner>(); } return runner.StartCoroutine(e); }

        /// <summary>개발용 자동 실행·촬영은 사용자 저장(LocalLow)을 절대 건드리지 않는다 → Temp/EWDevSave.</summary>
        public static void DevSave()
        {
            SaveSystem.OverrideDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWDevSave"));
        }

        /// <summary>Isolated native tile sample AA/DD capture. Synthetic input, developer save only.</summary>
        public static void PixelSamplesDemo() { Run(PixelSamplesDemoCo()); }
        static IEnumerator PixelSamplesDemoCo()
        {
            yield return WaitGame();var g=Game.I;
            GameInput.Virtual.ReleaseAll();GameInput.ClearContexts();
            g.Player.Teleport(new Vector2(8,3));g.Player.ResetCombatTimers();
            g.UI.hud.SetHint(GameInput.Label(Act.Dodge)+" · 대시     W/S 사다리 · A/D 이탈");
            yield return new WaitForSeconds(.5f);
            GameInput.Virtual.Tap(Act.Right);yield return new WaitForSeconds(.08f);
            GameInput.Virtual.Tap(Act.Right);yield return new WaitForSeconds(.06f);
            Debug.Log("[PIXEL_DASH_DEMO] DD dodging="+g.Player.Dodging+" vx="+g.Player.Velocity.x);
            yield return new WaitForSeconds(.9f);
            GameInput.Virtual.Tap(Act.Left);yield return new WaitForSeconds(.08f);
            GameInput.Virtual.Tap(Act.Left);yield return new WaitForSeconds(.06f);
            Debug.Log("[PIXEL_DASH_DEMO] AA dodging="+g.Player.Dodging+" vx="+g.Player.Velocity.x);
            yield return new WaitForSeconds(.9f);
            GameInput.Virtual.Hold(Act.Right,true);yield return new WaitForSeconds(.6f);
            Debug.Log("[PIXEL_DASH_DEMO] hold dodging="+g.Player.Dodging+" vx="+g.Player.Velocity.x);
            GameInput.Virtual.ReleaseAll();
        }

        static IEnumerator WaitGame()
        {
            float t = 0;
            while ((Game.I == null || Game.I.State == null || Game.I.Player == null) && t < 10f) { t += Time.unscaledDeltaTime; yield return null; }
        }

        static void StealthFlags(Game g)
        {
            foreach (var f in new[] { "op_village_done", "op_house_done", "op_desktop_done", "op_tank_done", "lab_woke", "lab_realized", "lab_power", "lab_alarm", "c1_escape_view", "c2s_enter_said", "pull_unlocked", "c1_tank_broken", "c1_researchers_down", "op_r_in" })
                g.State.Set(f);
            for (int i = 1; i <= 4; i++) g.State.Set("lab_power_" + i);
            if (!g.State.D.skills.Contains("pull")) g.State.D.skills.Add("pull");
        }

        /// <summary>C02: 첫 CCTV 앞 상자 뒤에 웅크림.</summary>
        public static void ShotC02Crouch() { Run(ShotC02CrouchCo()); }
        static IEnumerator ShotC02CrouchCo()
        {
            yield return WaitGame(); var g = Game.I; StealthFlags(g);
            while (Cutscene.Running) { GameInput.Virtual.Skip = true; yield return null; }
            GameInput.Virtual.Skip = false;
            g.Player.Teleport(new Vector2(11.6f, g.Player.transform.position.y));
            g.Player.SetFacing(1);
            GameInput.Virtual.Hold(Act.Down, true);
        }

        /// <summary>C02: 약한 간섭 조준 화면 (제어함 대상).</summary>
        public static void ShotC02Target() { Run(ShotC02TargetCo()); }
        static IEnumerator ShotC02TargetCo()
        {
            yield return WaitGame(); var g = Game.I; StealthFlags(g);
            while (Cutscene.Running) { GameInput.Virtual.Skip = true; yield return null; }
            GameInput.Virtual.Skip = false;
            g.Player.Teleport(new Vector2(6.5f, g.Player.transform.position.y));
            g.State.D.lu.charge = 4;
            yield return new WaitForSeconds(0.5f);
            GameInput.Virtual.Hold(Act.Skill2, true);
        }

        /// <summary>C02: 외부 전력 끌어오기 조준 (전원 → CCTV).</summary>
        public static void ShotC02Pull() { Run(ShotC02PullCo()); }
        static IEnumerator ShotC02PullCo()
        {
            yield return WaitGame(); var g = Game.I; StealthFlags(g);
            while (Cutscene.Running) { GameInput.Virtual.Skip = true; yield return null; }
            GameInput.Virtual.Skip = false;
            g.Player.Teleport(new Vector2(9.2f, g.Player.transform.position.y));
            g.State.D.lu.charge = 4;
            yield return new WaitForSeconds(0.5f);
            GameInput.Virtual.Hold(Act.Skill3, true);
        }

        /// <summary>C02: 순찰 경비 구역 (위 통로 아래 상자 뒤).</summary>
        public static void ShotC02Guard() { Run(ShotC02GuardCo()); }
        static IEnumerator ShotC02GuardCo()
        {
            yield return WaitGame(); var g = Game.I; StealthFlags(g);
            while (Cutscene.Running) { GameInput.Virtual.Skip = true; yield return null; }
            GameInput.Virtual.Skip = false;
            g.Player.Teleport(new Vector2(28.2f, g.Player.transform.position.y));
            g.Player.SetFacing(1);
        }

        /// <summary>C01: 정전된 실험실 (조사 단계).</summary>
        public static void ShotC01Explore() { Run(ShotC01ExploreCo()); }
        static IEnumerator ShotC01ExploreCo()
        {
            yield return WaitGame(); var g = Game.I;
            foreach (var f in new[] { "op_village_done", "op_house_done", "op_desktop_done", "op_tank_done", "lab_woke", "c1_tank_broken", "c1_researchers_down", "op_r_in" }) g.State.Set(f);
            while (Cutscene.Running) { GameInput.Virtual.Skip = true; yield return null; }
            GameInput.Virtual.Skip = false;
            yield return new WaitForSeconds(0.3f);
            g.Player.Teleport(new Vector2(12.5f, g.Player.transform.position.y));
            g.Player.SetFacing(1);
        }

        /// <summary>맵 전체를 보는 확인용: 경보 이후 상태로 시작해 루를 x 위치에 둔다 (SessionState ew_probe_x).</summary>
        /// <summary>C02: 전경 엄폐물 뒤에 숨은 상태 (CCTV 가 루 쪽을 보는 중).</summary>
        public static void ShotHide() { Run(ShotHideCo()); }
        static IEnumerator ShotHideCo()
        {
            yield return WaitGame(); var g = Game.I; StealthFlags(g);
            while (Cutscene.Running) { GameInput.Virtual.Skip = true; yield return null; }
            GameInput.Virtual.Skip = false;
            var hs = Object.FindObjectsByType<HideSpot>();
            HideSpot best = null; foreach (var h in hs) if (best == null || Mathf.Abs(h.transform.position.x - 30.2f) < Mathf.Abs(best.transform.position.x - 30.2f)) best = h;
            g.Player.Teleport(best.transform.position);
            GameInput.Virtual.Hold(Act.Down, true);
            foreach (var c in Object.FindObjectsByType<Cctv>())
                if (c.id == "C02_cam2") c.DebugSetAngle(Mathf.Atan2(g.Player.Chest.y - c.Eye.y, g.Player.Chest.x - c.Eye.x) * Mathf.Rad2Deg);
        }

        /// <summary>C06: 베넬이 루의 목덜미를 잡아 들어 올린 자세 (대화 중).</summary>
        public static void ShotGrab() { Run(ShotGrabCo()); }
        static IEnumerator ShotGrabCo()
        {
            yield return WaitGame(); var g = Game.I;
            g.State.Set("c3_boss_intro");
            while (Cutscene.Running) { GameInput.Virtual.Skip = true; yield return null; }
            GameInput.Virtual.Skip = false;
            yield return new WaitForSeconds(0.3f);
            var b = Object.FindAnyObjectByType<VenelleBoss>();
            b.transform.position = new Vector3(g.Player.transform.position.x + 0.8f, b.transform.position.y, 0);
            b.DebugForceGrab();
        }

        public static void ShotMapAt() { Run(ShotMapAtCo()); }
        static IEnumerator ShotMapAtCo()
        {
            yield return WaitGame(); var g = Game.I; StealthFlags(g);
            while (Cutscene.Running) { GameInput.Virtual.Skip = true; yield return null; }
            GameInput.Virtual.Skip = false;
            float x = 10f, y = -1f;
            try
            {
                var parts = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath, "../Temp/EWBridge/probe.txt")).Trim().Split(' ');
                x = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                if (parts.Length > 1) y = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
            }
            catch { }
            g.Player.Teleport(new Vector2(x, y < 0 ? g.Player.transform.position.y : y));
        }

        /// <summary>C06: 베넬 복도 (만남 연출 건너뛰고 대결 시작).</summary>
        public static void ShotC06() { Run(ShotC06Co()); }
        static IEnumerator ShotC06Co()
        {
            yield return WaitGame(); var g = Game.I; StealthFlags(g);
            yield return new WaitForSeconds(0.5f);
            while (Cutscene.Running) { GameInput.Virtual.Skip = true; yield return null; }
            GameInput.Virtual.Skip = false;
        }

        public static void AutoAdvance(bool on) { GameInput.AutoAdvance = on; }

        public static string Probe()
        {
            string o = "";
            var g = Game.I;
            if (g != null && g.Player != null)
            {
                var p = (Vector2)g.Player.transform.position;
                o += "lu=" + p + " grounded=" + g.Player.Grounded + " script=" + g.Player.ScriptControl + " mx=" + g.Player.ScriptMoveX + " sp=" + g.Player.ScriptSpeed + " ts=" + Time.timeScale + " hits:";
                foreach (var c in Physics2D.OverlapBoxAll(p + new Vector2(0.4f, 0.7f), new Vector2(1.2f, 1.2f), 0)) o += c.name + "(" + c.isTrigger + ") ";
                o += "; ";
            }
            foreach (var n in new[] { "Wall", "Bed", "Desk", "MidHouse", "FarTown", "Sky" })
            {
                var go = GameObject.Find(n); if (go == null) { o += n + "=none; "; continue; }
                var sr = go.GetComponent<SpriteRenderer>();
                o += n + " pos=" + go.transform.position + " b=" + (sr != null ? sr.bounds.ToString() : "") + " mode=" + (sr != null ? sr.drawMode.ToString() : "") + " size=" + (sr != null ? sr.size.ToString() : "") + " pivot=" + (sr != null && sr.sprite != null ? sr.sprite.pivot.ToString() : "") + "; ";
            }
            return o;
        }

        /// <summary>루 근처의 조사 대상과 조사 가능 여부 (디버그).</summary>
        public static string ProbeInteract()
        {
            var g = Game.I; if (g == null || g.Player == null) return "no player";
            var sb = new System.Text.StringBuilder();
            foreach (var it in Interactable.All)
            {
                if (it == null) continue;
                Vector2 d = (Vector2)it.transform.position - (Vector2)g.Player.transform.position;
                if (Mathf.Abs(d.x) > 4) continue;
                sb.Append(it.GetType().Name + ":" + it.id + " d=" + d.ToString("0.00") + " can=" + it.CanInteract(g) + " act=" + it.isActiveAndEnabled + " clear=" + Sight.Clear(g.Player.Chest, (Vector2)it.transform.position + Vector2.up * 0.55f) + "; ");
            }
            return sb.ToString();
        }

        public static void DebugRunEvent()
        {
            var g = Game.I; string id = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath, "../Temp/EWBridge/probe.txt")).Trim();
            GameInput.AutoAdvance = true; g.RunEvent(StoryEvents.Run(g, id));
        }
        public static string DebugTeleportProbe()
        {
            var g = Game.I; var parts = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath, "../Temp/EWBridge/probe.txt")).Trim().Split(' ');
            g.Player.Teleport(new Vector2(float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture), g.Player.transform.position.y));
            return "pos=" + (Vector2)g.Player.transform.position + " all=" + Interactable.All.Count + " sw=" + Object.FindObjectsByType<ArenaSwitch>().Length + " " + ProbeInteract() + " | user=" + (ArenaSwitch.User != null ? ArenaSwitch.User.GetType().Name : "null") + " ev=" + g.InEvent + " cut=" + Cutscene.Running;
        }

        public static string DebugLacProbe()
        {
            var b = Object.FindAnyObjectByType<LacrimosaBoss>(); var g = Game.I;
            if (b == null) return "no boss";
            if (b.State != LacrimosaBoss.LState.Stopped) { b.SetMixer(2, true, true, 2); return "mixer set; state=" + b.State + " fb=" + b.Feedback; }
            g.Player.Teleport((Vector2)b.transform.position + Vector2.left * 1.0f);
            return "talkOpen=" + b.TalkOpen + " " + ProbeInteract();
        }

        public static string Status()
        {
            var g = Game.I; if (g == null || g.State == null) return "no game";
            var pl = g.Player;
            string pi = pl != null ? " mx=" + pl.ScriptMoveX + " sp=" + pl.ScriptSpeed + " vel=" + pl.Velocity + " anim=" + pl.AnimOverride + " grounded=" + pl.Grounded : "";
            return "map=" + g.CurrentMapId + " cut=" + Cutscene.CurrentId + " pos=" + (pl != null ? (Vector2)pl.transform.position : Vector2.zero) + pi + " ctx=" + GameInput.Context + " ts=" + Time.timeScale + " hp=" + g.State.D.lu.hp + " flags=" + string.Join(",", g.State.D.flags);
        }

        /// <summary>
        /// 녹화용 자동 재생: 타이틀에서 새 게임 → 오프닝(사람이 읽는 속도로 대사 진행) → 문틈 탈출 → 첫 탈출 구역 → 첫 전투(노출 전선 사용)까지.
        /// 입력은 가상 패드로만 넣는다 (게임 로직은 실제 플레이와 같다).
        /// </summary>
        public static void StartOpeningDemo()
        {
            DevSave(); var g = Game.Ensure();
            GameInput.AutoReadDelay = 1.0f;
            g.StartCoroutine(OpeningDemo(g));
        }

        /// <summary>녹화용: 새 게임 자동 재생 + 0.25초마다 640×360 JPG 프레임 저장 (Temp/EWRec).</summary>
        public static void StartOpeningDemoRecorded()
        {
            DevSave(); var g = Game.Ensure();
            string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWRec"));
            if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true);
            System.IO.Directory.CreateDirectory(dir);
            g.StartCoroutine(Recorder(dir, 0.25f));
            StartOpeningDemo();
        }

        /// <summary>녹화: 정전된 실험실 첫 조작부터 C02 탈출까지 (Temp/EWRec, 4fps JPG).</summary>
        public static void StartLabDemoRecorded()
        {
            DevSave(); var g = Game.Ensure();
            string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWRec"));
            if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true);
            System.IO.Directory.CreateDirectory(dir);
            try { System.IO.File.Delete(StealthDemo.OutFile); } catch { }
            g.StartCoroutine(Recorder(dir, 0.25f));
            StealthDemo.StartFromLab();
        }

        /// <summary>녹화: 첫 조작부터 베넬 협상까지 전체 (시간 측정용).</summary>
        public static void StartFullDemoRecorded()
        {
            DevSave(); var g = Game.Ensure();
            string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWRec"));
            if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true);
            System.IO.Directory.CreateDirectory(dir);
            try { System.IO.File.Delete(StealthDemo.OutFile); } catch { }
            g.StartCoroutine(Recorder(dir, 0.25f));
            StealthDemo.StartFromLab();
        }

        /// <summary>녹화: 타이틀 새 게임부터 시연 끝까지 전체 자동 재생 (Temp/EWRec 4fps, 시간 기록 Temp/EWBridge/demo.txt).</summary>
        public static void StartWholeDemoRecorded()
        {
            DevSave(); var g = Game.Ensure();
            string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWRec"));
            if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true);
            System.IO.Directory.CreateDirectory(dir);
            try { System.IO.File.Delete(StealthDemo.OutFile); } catch { }
            try { System.IO.File.Delete(System.IO.Path.Combine(Application.dataPath, "../Temp/EWBridge/cuts.txt")); } catch { }
            g.StartCoroutine(Recorder(dir, 0.25f));
            StealthDemo.StartFull();
        }

        /// <summary>
        /// 4~6장 자동 진행: 승강장 세이브 복사본으로 이어하기 → 7장 수용동 연결문까지 (Temp/EWBridge/ch46.txt 로 분기 선택).
        /// 녹화 Temp/EWRec (4fps), 기록 Temp/EWBridge/demo.txt. 사용자 저장은 읽기만 한다.
        /// </summary>
        public static void StartCh46Recorded()
        {
            DevSave(); var g = Game.Ensure();
            string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWRec"));
            if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true);
            System.IO.Directory.CreateDirectory(dir);
            try { System.IO.File.Delete(System.IO.Path.Combine(Application.dataPath, "../Temp/EWBridge/cuts.txt")); } catch { }
            Ch46Demo.ReadConfig();
            g.StartCoroutine(Recorder(dir, 0.25f));
            g.StartCoroutine(Ch46Demo.Run(g));
        }

        /// <summary>7~11장 자동 진행: 배치 데이터의 사건 프리셋(기본 ch7_release_before)에서 엔딩까지 (녹화).</summary>
        public static void StartCh711Recorded()
        {
            DevSave(); var g = Game.Ensure();
            string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWRec"));
            if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true);
            System.IO.Directory.CreateDirectory(dir);
            try { System.IO.File.Delete(System.IO.Path.Combine(Application.dataPath, "../Temp/EWBridge/cuts.txt")); } catch { }
            Ch46Demo.ReadConfig();
            EndingScreens.EndingTestMode = true;
            g.StartCoroutine(Recorder(dir, 0.25f));
            g.StartCoroutine(Ch711Co(g));
        }

        static IEnumerator Ch711Co(Game g) { yield return Ch711Demo.StartFromPreset(g); yield return Ch711Demo.Run(g); }

        /// <summary>새 게임 → 오프닝·1~3장 → 승강장 → 4~6장 → 7~11장 → 엔딩까지 한 번에 (정상 출구·사건만, 녹화 2초 간격).</summary>
        public static void StartGameToEndRecorded()
        {
            DevSave(); var g = Game.Ensure();
            string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWRec"));
            if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true);
            System.IO.Directory.CreateDirectory(dir);
            try { System.IO.File.Delete(StealthDemo.OutFile); } catch { }
            try { System.IO.File.Delete(System.IO.Path.Combine(Application.dataPath, "../Temp/EWBridge/cuts.txt")); } catch { }
            Ch46Demo.ReadConfig(); Ch46Demo.Through = true; Ch46Demo.FromCurrent = true;
            EndingScreens.EndingTestMode = true;
            StealthDemo.After = gg => Ch46Demo.Run(gg);
            g.StartCoroutine(Recorder(dir, 2f));
            StealthDemo.StartFull();
        }

        /// <summary>4~6장 자동 진행 (녹화 없음, 빠른 확인용).</summary>
        public static void StartCh46()
        {
            DevSave(); var g = Game.Ensure();
            Ch46Demo.ReadConfig();
            g.StartCoroutine(Ch46Demo.Run(g));
        }

        public static void StartLabDemo()
        {
            try { System.IO.File.Delete(StealthDemo.OutFile); } catch { }
            StealthDemo.StartFromLab();
        }

        static IEnumerator Recorder(string dir, float interval)
        {
            int n = 0;
            float next = Time.realtimeSinceStartup;
            var rt = new RenderTexture(640, 360, 0);
            var small = new Texture2D(640, 360, TextureFormat.RGB24, false);
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (Time.realtimeSinceStartup < next) continue;
                next += interval;
                var full = ScreenCapture.CaptureScreenshotAsTexture();
                Graphics.Blit(full, rt);
                var prev = RenderTexture.active; RenderTexture.active = rt;
                small.ReadPixels(new Rect(0, 0, 640, 360), 0, 0); small.Apply();
                RenderTexture.active = prev;
                Object.Destroy(full);
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "f_" + (n++).ToString("0000") + ".jpg"), small.EncodeToJPG(88));
            }
        }

        static IEnumerator Until(System.Func<bool> c, float timeout = 300f)
        {
            float t = 0;
            while (!c() && t < timeout) { t += Time.unscaledDeltaTime; yield return null; }
        }

        public static IEnumerator Walk(Game g, float x, float timeout = 20f, bool crouch = false)
        {
            float t = 0;
            GameInput.Virtual.Hold(Act.Down, crouch);
            float last = 1e9f, stuck = 0;
            while (g.Player != null && Mathf.Abs(g.Player.transform.position.x - x) > 0.15f && t < timeout)
            {
                bool right = x > g.Player.transform.position.x;
                // 대화로 상태가 바뀐 뒤 잠긴 키는 사람처럼 한 번 뗐다가 다시 누른다
                if (GameInput.IsLatched(right ? Act.Right : Act.Left)) { GameInput.Virtual.Hold(Act.Right, false); GameInput.Virtual.Hold(Act.Left, false); yield return null; }
                GameInput.Virtual.Hold(Act.Right, right); GameInput.Virtual.Hold(Act.Left, !right);
                float px = g.Player.transform.position.x;
                stuck = g.FieldInputAllowed && Mathf.Abs(px - last) < 0.002f ? stuck + Time.deltaTime : 0; last = px;
                // 턱에 막히면 점프 (사람이 하듯 W)
                if (stuck > 0.25f && g.Player.Grounded && !crouch) { GameInput.Virtual.Tap(Act.Jump); stuck = 0; }
                t += Time.deltaTime;
                yield return null;
            }
            GameInput.Virtual.Hold(Act.Right, false); GameInput.Virtual.Hold(Act.Left, false); GameInput.Virtual.Hold(Act.Down, false);
        }

        public static IEnumerator Press(Act a = Act.Interact) { GameInput.Virtual.Tap(a); yield return null; yield return null; }

        static IEnumerator OpeningDemo(Game g)
        {
            g.NewGame();
            yield return Until(() => g.CurrentMapId == "C01" && g.State.Has("op_tank_done") && !Cutscene.Running && g.FieldInputAllowed);
            yield return new WaitForSeconds(1.0f);
            yield return StealthDemo.Run(g);
            GameInput.AutoReadDelay = 0;
            Debug.Log("[EW] 자동 재생 끝: " + Status());
        }
    }
}
#endif
