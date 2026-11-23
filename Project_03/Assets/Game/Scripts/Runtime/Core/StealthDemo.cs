#if UNITY_EDITOR
using System.Collections;
using System.Text;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 녹화·시간 측정용 자동 플레이 (에디터 전용). 실제 플레이와 같은 가상 입력(키 누르기·떼기)만 넣는다.
    /// 경로: 정전된 실험실 조사(침대·도구·벽 도식) → 깨달음·경보 → 문틈 → 대표 잠입 구간 C02 를
    /// 적을 하나도 쓰러뜨리지 않고 통과 (제어함 간섭 → 상자 사이 이동 → 사다리·위 통로 → 전원선 우회 → 출구).
    /// 시간은 '강제 진행'(컷신·대화·이벤트)과 '조작 시간'을 나눠 기록한다.
    /// </summary>
    public static class StealthDemo
    {
        public static float Forced, Play;
        public static readonly StringBuilder Report = new StringBuilder();
        static bool tracking;

        static IEnumerator Track(Game g)
        {
            tracking = true; Forced = Play = 0;
            while (tracking)
            {
                float dt = Time.unscaledDeltaTime;
                bool forced = Cutscene.Running || g.UI.dialogue.Open || g.InEvent || g.Loading || g.Player == null;
                if (forced) Forced += dt; else Play += dt;
                yield return null;
            }
        }

        static void Mark(Game g, string what)
        {
            string line = "[EW] 시간 " + what + ": 조작 " + Play.ToString("0.0") + "s / 강제 " + Forced.ToString("0.0") + "s / 합계 " + (Play + Forced).ToString("0.0") + "s  (map=" + g.CurrentMapId + " hp=" + g.State.D.lu.hp + " spotted=" + (g.State.Has("spotted_" + g.CurrentMapId)) + " retries=" + g.State.D.retries + ")";
            Report.AppendLine(line);
            Debug.Log(line);
            Out(line);
        }

        public static string OutFile => System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWBridge/demo.txt"));
        public static void Out(string s) { try { System.IO.File.AppendAllText(OutFile, System.DateTime.Now.ToString("HH:mm:ss ") + s + System.Environment.NewLine); } catch { } }
        static bool hooked;
        static void HookLog()
        {
            if (hooked) return; hooked = true;
            Application.logMessageReceived += (msg, st, type) =>
            {
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert || msg.StartsWith("[EW]"))
                    Out(type + " " + msg + (type == LogType.Exception ? " @ " + st.Split((char)10)[0] : ""));
            };
        }

        static IEnumerator Until(System.Func<bool> c, float timeout = 60f)
        {
            float t = 0;
            while (!c() && t < timeout) { t += Time.unscaledDeltaTime; yield return null; }
            if (t >= timeout) Debug.Log("[EW] 자동 재생: 조건 대기 시간 초과 " + DevHooks.Status());
        }

        static IEnumerator Idle(Game g) => Until(() => g.FieldInputAllowed && !Cutscene.Running && g.Player != null && !g.UI.dialogue.Open, 120f);

        static IEnumerator Walk(Game g, float x, float timeout = 25f, bool crouch = false) { yield return Idle(g); yield return DevHooks.Walk(g, x, timeout, crouch); }

        static IEnumerator Tap(Act a) { GameInput.Virtual.Tap(a); yield return null; yield return null; }

        static IEnumerator Inspect(Game g, float x)
        {
            yield return Walk(g, x);
            yield return new WaitForSeconds(0.2f);
            yield return Tap(Act.Interact);
            yield return new WaitForSeconds(0.3f);
            yield return Idle(g);
            yield return new WaitForSeconds(0.3f);
        }

        /// <summary>제자리 점프 (한 번).</summary>
        static IEnumerator Jump(Game g)
        {
            yield return Idle(g);
            yield return Tap(Act.Jump);
            yield return new WaitForSeconds(0.1f);
            yield return Until(() => g.Player.Grounded, 3f);
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>슬롯 기술 사용 (Q/E/R/F 를 눌렀다 뗀다).</summary>
        static IEnumerator Use(Game g, int slot)
        {
            yield return Idle(g);
            yield return Tap(GameInput.SkillAct(slot));
            yield return new WaitForSeconds(0.45f);
        }

        /// <summary>
        /// 처음부터: 마을·집 연출(대사는 사람이 읽는 속도) → 컴퓨터 안 탐색(안내서 열람, Q/E/R/F 연습) → 접속 기록 → 강제 전송 →
        /// 실험 통 → 탈출 시작 → C02~C05 → 베넬 → 휴식 → 첫 전선 동행 → 시연 끝.
        /// </summary>
        public static IEnumerator RunFull(Game g)
        {
            Report.Clear();
            HookLog();
            g.StartCoroutine(Track(g));
            GameInput.AutoReadDelay = 0.9f;
            Mark(g, "새 게임");
            yield return Until(() => g.CurrentMapId == "P03" && g.State.Has("op_desktop_land") && !Cutscene.Running && g.FieldInputAllowed, 240f);
            Mark(g, "컴퓨터 안 첫 조작");
            yield return Desktop(g);
            yield return Until(() => g.CurrentMapId == "C01" && g.State.Has("op_tank_done") && !Cutscene.Running && g.FieldInputAllowed, 240f);
            yield return Run(g);
        }

        static IEnumerator Desktop(Game g)
        {
            yield return FloatTo(g,new Vector2(14f,4.7f));
            yield return Tap(Act.Interact);
            yield return Idle(g);
            yield return FloatTo(g,new Vector2(6f,4.7f));
            yield return Tap(Act.Interact);
            yield return Until(()=>GuideBook.Open,5f);
            for(int i=0;i<4;i++){yield return new WaitForSecondsRealtime(1.8f);yield return Tap(Act.Right);}
            yield return new WaitForSecondsRealtime(1f);
            yield return Tap(Act.Interact);
            yield return Until(()=>!GuideBook.Open,5f);
            Mark(g,"안내서 닫음 · 탐색 완료");
        }

        static IEnumerator FloatTo(Game g,Vector2 dest)
        {
            yield return Idle(g);
            float t=0;
            while(g.Player!=null && Vector2.Distance(g.Player.transform.position,dest)>0.18f && t<12f)
            {
                Vector2 d=dest-(Vector2)g.Player.transform.position;
                GameInput.Virtual.Hold(Act.Right,d.x>0.12f); GameInput.Virtual.Hold(Act.Left,d.x<-0.12f);
                GameInput.Virtual.Hold(Act.MenuUp,d.y>0.12f); GameInput.Virtual.Hold(Act.Down,d.y<-0.12f);
                t+=Time.deltaTime;yield return null;
            }
            GameInput.Virtual.ReleaseAll();yield return new WaitForSeconds(0.25f);
        }

        public static IEnumerator Run(Game g)
        {
            if (Report.Length == 0) { HookLog(); g.StartCoroutine(Track(g)); }
            GameInput.AutoReadDelay = 0.9f;
            yield return Idle(g);
            Mark(g, "실험실 첫 조작 (탈출 시작)");
            // 압수품 바구니 (초콜릿) → 문틈
            var basket = GameObject.Find("Confiscated");
            if (basket != null) yield return Inspect(g, basket.transform.position.x);
            var door = Marker.Find("door");
            yield return Inspect(g, door != null ? door.transform.position.x - 0.7f : 43f);
            yield return Until(() => g.CurrentMapId == "C02" && !g.Loading, 30f);
            yield return Idle(g);
            Mark(g, "C02 감시 복도");
            yield return Walk(g, 13.5f);
            g.Player.SetFacing(1);
            yield return Use(g, 1);                     // W: CCTV 1
            yield return Walk(g, 27.5f);
            yield return Walk(g, 30.2f);
            yield return HideFor(g, 1.6f);              // 선반 뒤 숨기
            g.Player.SetFacing(1);
            yield return Use(g, 1);                     // W: CCTV 2
            yield return Walk(g, 57.5f);
            yield return Until(() => g.CurrentMapId == "C03" && !g.Loading, 30f);
            yield return Idle(g);
            Mark(g, "C03 설비실");
            yield return Traverse(g, 23.6f);
            g.Player.SetFacing(1);
            yield return Use(g, 2);                     // E: 노출 전원 → 셔터 제어판
            yield return Until(() => g.State.Has("c03_shutter_open"), 4f);
            yield return Traverse(g, 63.5f);
            yield return Until(() => g.CurrentMapId == "C04" && !g.Loading, 30f);
            yield return Idle(g);
            Mark(g, "C04 화물 적재장");
            yield return Walk(g, 17.5f);
            yield return Use(g, 3);                     // R: 끊긴 바닥 위로
            yield return Until(() => !g.InEvent && g.Player.transform.position.y > 9f, 6f);
            yield return Traverse(g, 33.6f);
            yield return Walk(g, 34.5f);
            GameInput.Virtual.Hold(Act.Down, true);     // 사다리 꼭대기에서 S: 내려가기
            yield return new WaitForSeconds(0.1f);
            yield return Until(() => g.Player.Climbing, 2f);
            yield return Until(() => !g.Player.Climbing, 8f);
            GameInput.Virtual.Hold(Act.Down, false);
            yield return Traverse(g, 38.5f);
            yield return Use(g, 3);                     // R: 출구 높이로
            yield return Until(() => !g.InEvent && g.Player.transform.position.y > 9f, 6f);
            yield return Walk(g, 46f);
            yield return Until(() => g.CurrentMapId == "C05" && !g.Loading, 30f);
            yield return Idle(g);
            Mark(g, "C05 배양실");
            yield return Traverse(g, 47.5f);
            yield return Until(() => g.CurrentMapId == "C06" && !g.Loading, 30f);
            Mark(g, "베넬 복도 진입 (등장 연출 시작)");
            yield return Until(() => !Cutscene.Running && g.FieldInputAllowed, 120f);
            Mark(g, "베넬 실시간 조작 시작");
            yield return FightVenelle(g);
            Mark(g, "베넬 결말 (" + (g.State.Has("c3_negotiated") ? "협상" : g.State.Has("venelle_dead") ? "처치" : "제압 후 살림") + ")");
            yield return Until(() => g.CurrentMapId == "M15" && !g.InEvent && g.FieldInputAllowed, 120f);
            Mark(g, "휴식 끝 (" + g.State.D.venelleState + ")");
            yield return Walk(g, 34.5f);
            yield return Use(g, 3);
            yield return Until(() => !g.InEvent && !Cutscene.Running && g.Player.transform.position.x > 40f, 30f);
            Mark(g, "첫 전선 이동 끝");
            yield return Walk(g, 47.5f);
            Mark(g, "시연 끝 지점");
        }

        static IEnumerator HideFor(Game g, float t)
        {
            GameInput.Virtual.Hold(Act.Down, true);
            yield return new WaitForSeconds(t);
            Debug.Log("[EW] 숨음=" + g.Player.HiddenInCover);
            GameInput.Virtual.Hold(Act.Down, false);
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>오른쪽으로 걸으며: 앞쪽 가까운 CCTV·경비는 W 로 잠깐 끄고, 붙은 경비는 Q 로 경직시킨 뒤 Shift 로 지나간다.</summary>
        static IEnumerator Traverse(Game g, float x, float timeout = 60f)
        {
            float t = 0, last = -1, stuck = 0;
            yield return Idle(g);
            while (g.Player != null && g.Player.transform.position.x < x - 0.15f && t < timeout)
            {
                if (!g.FieldInputAllowed) { GameInput.Virtual.Hold(Act.Right, false); t += Time.unscaledDeltaTime; yield return null; continue; }
                var p = g.Player;
                var sk = p.Skills;
                sk.UpdateSuggestions();
                // 켜져 있는 CCTV 가 앞에 있으면 W
                if (sk.SuggestJam is Cctv cam && !cam.Jammed && sk.CooldownLeft("jam") <= 0 && g.State.D.lu.charge >= 1 && cam.transform.position.x > p.transform.position.x - 1f)
                { GameInput.Virtual.Hold(Act.Right, false); p.SetFacing(1); yield return Use(g, 1); }
                Guard near = null;
                foreach (var gd in Guard.All)
                    if (gd != null && gd.Alive && !gd.flying && gd.State != Guard.GState.Jammed && gd.State != Guard.GState.Stagger && gd.transform.position.x > p.transform.position.x - 0.5f && gd.transform.position.x - p.transform.position.x < 1.4f && Mathf.Abs(gd.transform.position.y - p.transform.position.y) < 1.5f)
                    { near = gd; break; }
                if (near != null)
                {
                    GameInput.Virtual.Hold(Act.Right, false);
                    p.SetFacing(1);
                    yield return Use(g, 0);           // Q: 경직
                    GameInput.Virtual.Hold(Act.Right, true);
                    yield return Tap(Act.Dodge);       // Shift: 빠져나감
                    yield return new WaitForSeconds(0.3f);
                }
                if (GameInput.IsLatched(Act.Right)) { GameInput.Virtual.Hold(Act.Right, false); yield return null; }
                GameInput.Virtual.Hold(Act.Right, true);
                float px = p.transform.position.x;
                stuck = Mathf.Abs(px - last) < 0.002f ? stuck + Time.deltaTime : 0; last = px;
                if (stuck > 0.25f && p.Grounded) { GameInput.Virtual.Tap(Act.Jump); stuck = 0; }
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            GameInput.Virtual.Hold(Act.Right, false);
        }

        /// <summary>
        /// 베넬: 돌진·분사는 회피(Shift)/점프로 피하고 공격하지 않는다. 숨 고를 때·약 섞을 때 다가가 J 로 말을 건다:
        /// '나가려는 거야' → '왜 찾아?' → '(손을 내린다)' 순. 잡히면 '공격 안 할게'. 협상 성립(신뢰·위협 조건)까지.
        /// </summary>
        public static IEnumerator FightVenelle(Game g)
        {
            var b = Object.FindAnyObjectByType<VenelleBoss>();
            float t = 0;
            int talks = 0;
            int[] talkOrder = { 0, 1, 3, 0, 1, 3 };
            while (b != null && !g.State.Has("c3_boss_done") && t < 240f)
            {
                var p = g.Player;
                if (p == null) { yield return null; continue; }
                if (b.State == VenelleBoss.VState.Holding)
                {
                    MenuList.ScriptedChoices.Clear(); MenuList.ScriptedChoices.Enqueue(0);
                    yield return Until(() => b.State != VenelleBoss.VState.Holding && b.State != VenelleBoss.VState.Thrown && !g.InEvent, 60f);
                    MenuList.ScriptedChoices.Clear();
                    Mark(g, "베넬 잡기 대화 (위협 " + VenelleMind.Threat + " 신뢰 " + VenelleMind.Trust + ")");
                    continue;
                }
                if (!g.FieldInputAllowed) { t += Time.unscaledDeltaTime; yield return null; continue; }
                float dx = b.transform.position.x - p.transform.position.x;
                if ((b.State == VenelleBoss.VState.Dash && Mathf.Abs(dx) < 2.8f && Mathf.Abs(dx) > 0.4f) || (b.State == VenelleBoss.VState.GrabWindup && b.WindupLeft < 0.25f && Mathf.Abs(dx) < 2.6f))
                {
                    GameInput.Virtual.Hold(dx > 0 ? Act.Right : Act.Left, true);
                    yield return Tap(Act.Dodge);
                    GameInput.Virtual.Hold(Act.Right, false); GameInput.Virtual.Hold(Act.Left, false);
                }
                else if ((b.State == VenelleBoss.VState.Spray || (b.State == VenelleBoss.VState.SprayWindup && b.WindupLeft < 0.15f)) && Mathf.Abs(dx) < 5f && p.Grounded)
                    yield return Tap(Act.Jump);
                else if (b.TalkAvailable && Mathf.Abs(dx) < 3.8f && talks < talkOrder.Length)
                {
                    MenuList.ScriptedChoices.Clear(); MenuList.ScriptedChoices.Enqueue(talkOrder[talks++]);
                    yield return Tap(Act.Interact);
                    yield return Until(() => !g.InEvent || g.CurrentMapId == "M15", 60f);
                    MenuList.ScriptedChoices.Clear();
                    Mark(g, "베넬 대화 " + talks + " (위협 " + VenelleMind.Threat + " 신뢰 " + VenelleMind.Trust + ")");
                }
                else if (b.TalkAvailable)
                {
                    GameInput.Virtual.Hold(dx > 0 ? Act.Right : Act.Left, true);
                    yield return null;
                    GameInput.Virtual.Hold(Act.Right, false); GameInput.Virtual.Hold(Act.Left, false);
                }
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            yield return Until(() => !g.InEvent || g.CurrentMapId == "M15", 60f);
            // 휴식: 초콜릿 건넴(0), 같이 가자(0)
            MenuList.ScriptedChoices.Clear(); MenuList.ScriptedChoices.Enqueue(0); MenuList.ScriptedChoices.Enqueue(0);
        }

        /// <summary>처음(타이틀 새 게임)부터 끝까지 자동 재생 (사용자 저장을 건드리지 않음).</summary>
        /// <summary>새 게임 자동 진행이 끝난 뒤 이어서 실행할 경로 (새 게임 → 엔딩 연속 검증).</summary>
        public static System.Func<Game, IEnumerator> After;

        public static void StartFull()
        {
            var g = Game.Ensure();
            SaveSystem.OverrideDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWDevSave"));
            g.StartCoroutine(StartFullCo(g));
        }

        static IEnumerator StartFullCo(Game g)
        {
            while (g.Config == null) yield return null;
            g.NewGame();
            yield return RunFull(g);
            if (After != null) { var a = After; After = null; yield return a(g); }
        }

        /// <summary>정전된 실험실부터 시작하는 상태 (오프닝을 다시 보지 않음).</summary>
        public static void StartFromLab()
        {
            var g = Game.Ensure();
            // 자동 재생은 사용자 저장을 건드리지 않는다
            SaveSystem.OverrideDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWDevSave"));
            g.StartCoroutine(StartFromLabCo(g));
        }

        static IEnumerator StartFromLabCo(Game g)
        {
            while (g.Config == null) yield return null;
            var d = SaveData.NewGame();
            foreach (var f in new[] { "op_village_done", "op_house_done", "op_desktop_land", "op_desktop_done", "pc_guide_read", "op_tank_done", "lab_woke", "lab_realized", "lab_power", "lab_alarm", "c1_tank_broken", "c1_researchers_down", "op_r_in" }) d.flags.Add(f);
            d.mapId = "C01"; d.spawnId = "tank";
            g.State = new GameState(d, g.Db);
            yield return g.LoadMap("C01", "tank");
            yield return Run(g);
        }
    }
}
#endif
