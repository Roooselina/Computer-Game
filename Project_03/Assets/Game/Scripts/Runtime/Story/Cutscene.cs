using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 순차 연출 실행기 (Timeline 대신 코드 기반 — 기존 이벤트 코루틴과 같은 방식으로 유지보수).
    /// - 본문 코루틴을 직접 한 단계씩 실행하므로 도중에 안전하게 멈출 수 있다.
    /// - Enter 를 한 번 누르면 건너뛰고, finalize 로 그 장면의 완료 상태(플래그·인물·장치·카메라)를 적용한다.
    ///   오른쪽 아래에 'Enter · 연출 건너뛰기' 안내가 상시 보인다. 선택지가 열려 있는 동안에는 건너뛰기를 받지 않는다
    ///   (중요한 선택은 사용자가 고른다). 건너뛴 뒤 카메라·입력·시간 속도·화면 페이드·인물 위치를 정상으로 되돌린다.
    ///   finalize 는 끝까지 본 경우에도 실행되며 여러 번 실행해도 결과가 같아야 한다.
    /// - 연출 중 필드 입력은 차단되고, 끝나면 이동 입력·카메라 추적이 복구된다.
    /// - '렉 걸리는 느낌'은 HitStop(timeScale 일시 0)으로 만든다. 메인 스레드를 멈추지 않으며 UI·소리는 계속 동작한다.
    /// </summary>
    public static class Cutscene
    {
        public const float SkipHoldSeconds = 0f;   // 옛 길게 누르기 (사용 안 함)
        public static bool Running { get; private set; }
        public static string CurrentId { get; private set; }
        /// <summary>마지막 컷신이 건너뛰어졌는지 (테스트·확인용).</summary>
        public static bool LastSkipped { get; private set; }
        static readonly List<Coroutine> parallel = new List<Coroutine>();
        static readonly List<GameObject> temporary = new List<GameObject>();

        /// <summary>플레이 모드 재진입(도메인 재시작 없음) 때 이전 세션의 정지 상태가 남지 않게.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { ResetState(); }
        public static void ResetState() { Running = false; CurrentId = null; LastSkipped = false; parallel.Clear(); temporary.Clear(); TimeCtl.Clear("hitstop"); }

        public static IEnumerator Play(Game g, string id, IEnumerator body, Action<Game> finalize, bool skippable = true)
        {
            Running = true; CurrentId = id; LastSkipped = false;
            g.Block("cutscene");
            int ctx = GameInput.Push(InputCtx.Cutscene);
            if (g.Player != null && g.Player.Skills != null) g.Player.Skills.CancelTargeting();
            g.UI.HidePrompt();
            var stack = new Stack<IEnumerator>();
            stack.Push(body);
            float waitUntil = 0;
            bool skipped = false;
            float tStart = Time.unscaledTime, tDlg = 0;
            while (stack.Count > 0)
            {
                if (g.UI.dialogue.Open) tDlg += Time.unscaledDeltaTime;
                // ---- 건너뛰기 (Enter 한 번). 선택지가 열려 있으면 받지 않는다.
                if (skippable)
                {
                    bool choiceOpen = Game.ChoiceOpen;
                    g.UI.ShowSkip(choiceOpen ? -1 : 0);
                    if (!choiceOpen && GameInput.SkipDown) { skipped = true; break; }
                }
                if (Time.time < waitUntil) { yield return null; continue; }
                var top = stack.Peek();
                bool moved;
                try { moved = top.MoveNext(); }
                catch (Exception e) { Debug.LogException(e); moved = false; }
                if (!moved) { stack.Pop(); continue; }
                var cur = top.Current;
                if (cur == null) yield return null;
                else if (cur is IEnumerator ie) stack.Push(ie);
                else if (cur is WaitForSeconds ws) { waitUntil = Time.time + SecondsOf(ws); yield return null; }
                else if (cur is Coroutine co) yield return co;
                else yield return null;
            }
            g.UI.ShowSkip(-1);
            g.UI.ClearFlash();
            if(PixelScreen.I!=null)PixelScreen.I.SetSignalDistortion(0);
            LogTiming(id, Time.unscaledTime - tStart, tDlg, skipped);
            if (skipped)
            {
                LastSkipped = true;
                Debug.Log("[EW] 컷신 건너뜀: " + id);
                foreach (var c in parallel) if (c != null) g.StopCoroutine(c);
                TimeCtl.Clear("hitstop");
            }
            parallel.Clear();
            foreach (var o in temporary) if (o != null) UnityEngine.Object.Destroy(o);
            temporary.Clear();
            // 공통 정리: 대화창·스탠딩·레터박스·연출용 조종 해제
            g.UI.dialogue.Show(false);
            g.UI.portraits.HideAll();
            g.UI.Letterbox(0);
            if (g.Player != null)
            {
                g.Player.ScriptControl = false; g.Player.ScriptMoveX = 0; g.Player.ScriptSpeed = 0; g.Player.AnimOverride = null; g.Player.Frozen = false;
                if (g.Player.View != null) { g.Player.View.transform.localPosition = Vector3.zero; g.Player.View.transform.localRotation = Quaternion.identity; g.Player.View.transform.localScale = Vector3.one; g.Player.View.sr.enabled = true; }
                g.Player.transform.localScale = Vector3.one;
                if (g.Player.View != null) { g.Player.View.SetVisible(true); g.Player.View.sr.color = Color.white; }
            }
            try { finalize?.Invoke(g); }
            catch (Exception e) { Debug.LogException(e); }
            var rig = CameraRig.Main;
            if (rig != null) { rig.Release(); rig.SetZoom(1); if (skipped) rig.Snap(); }
            if (skipped && g.UI.FadeAlpha > 0.01f && g.UI.FadeAlpha < 0.99f) g.UI.SetFade(0);
            TimeCtl.Clear("hitstop");
            GameInput.Pop(ctx);
            g.Unblock("cutscene");
            Running = false; CurrentId = null;
        }

        /// <summary>연출 시간 기록: 전체 / 대화창이 열린 시간(읽기·타자) / 자동 연출(나머지). Temp/EWBridge/cuts.txt</summary>
        static void LogTiming(string id, float total, float dlg, bool skipped)
        {
            string line = id + " total=" + total.ToString("0.0") + " dialogue=" + dlg.ToString("0.0") + " auto=" + (total - dlg).ToString("0.0") + (skipped ? " SKIPPED" : "");
            Debug.Log("[EW] 컷신 시간 " + line);
#if UNITY_EDITOR
            try { System.IO.File.AppendAllText(System.IO.Path.Combine(Application.dataPath, "../Temp/EWBridge/cuts.txt"), line + System.Environment.NewLine); } catch { }
#endif
        }

        static float SecondsOf(WaitForSeconds w)
        {
            var f = typeof(WaitForSeconds).GetField("m_Seconds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return f != null ? (float)f.GetValue(w) : 0.5f;
        }

        /// <summary>동시에 진행할 동작 (건너뛰면 함께 멈춘다).</summary>
        public static Coroutine Parallel(Game g, IEnumerator e)
        {
            var c = g.StartCoroutine(e);
            parallel.Add(c);
            return c;
        }

        /// <summary>연출이 끝나면 지울 임시 오브젝트 (효과 등).</summary>
        public static GameObject Temp(GameObject o) { if (o != null) temporary.Add(o); return o; }

        // ============================================================ 시간
        public static IEnumerator Wait(float s)
        {
            for (float t = 0; t < s; t += Time.deltaTime) yield return null;
        }

        /// <summary>전투 타격감용 짧은 정지 (코루틴 없이 호출). 화면 흔들림 줄이기 설정이면 생략.</summary>
        public static void HitStopSoft(float s)
        {
            if (Game.I == null || GameSettings.Shake <= 0.01f) return;
            Game.I.StartCoroutine(HitStop(s));
        }

        /// <summary>짧은 정지 연출 (히트스톱·렉 느낌). 실제 시간 기준으로 기다리며 UI·소리는 계속된다.</summary>
        public static IEnumerator HitStop(float s)
        {
            TimeCtl.Set("hitstop", 0f);
            for (float t = 0; t < s; t += Time.unscaledDeltaTime) yield return null;
            TimeCtl.Clear("hitstop");
        }

        // ============================================================ 인물
        /// <summary>등장인물(Actor)을 x 까지 걷게 한다.</summary>
        public static IEnumerator Walk(Actor a, float x, float speed, string anim = "Walk", string after = "Idle")
        {
            if (a == null) yield break;
            var v = a.view;
            int dir = x > a.transform.position.x ? 1 : -1;
            if (v != null) { v.Face(dir); v.Play(anim); }
            while (a != null && Mathf.Abs(a.transform.position.x - x) > 0.03f)
            {
                var p = a.transform.position;
                p.x = Mathf.MoveTowards(p.x, x, speed * Time.deltaTime);
                a.transform.position = p;
                yield return null;
            }
            if (a == null) yield break;     // 맵이 바뀌어 인물이 사라졌다 (병렬 걷기)
            var q = a.transform.position; q.x = x; a.transform.position = q;
            if (v != null && after != null) v.Play(after);
        }

        /// <summary>루를 연출로 x 까지 이동 (지형 충돌·중력 유지). anim 을 주면 그 동작으로.</summary>
        public static IEnumerator LuMove(Game g, float x, float speed, string anim = "Walk", float timeout = 15f)
        {
            var p = g.Player;
            if (p == null) yield break;
            p.ScriptControl = true; p.ScriptSpeed = speed; p.AnimOverride = anim;
            float t = 0;
            while (Mathf.Abs(p.transform.position.x - x) > 0.05f && t < timeout)
            {
                p.ScriptMoveX = Mathf.Sign(x - p.transform.position.x);
                t += Time.deltaTime;
                yield return null;
            }
            p.ScriptMoveX = 0; p.ScriptSpeed = 0;
        }

        /// <summary>
        /// 다친 다리로 절뚝이며 걷기: 성한 발로 디딤(빠름) → 다친 발 끌기(느림, 몸 기울임) → 가끔 짧게 멈춤.
        /// 동작은 임시 그림 'Limp' 6프레임 (최종 그림 교체 지점: Docs/ART_REQUESTS.md).
        /// </summary>
        public static IEnumerator LuLimp(Game g, float x, float pauseEvery = 3f, float timeout = 40f)
        {
            var p = g.Player;
            if (p == null) yield break;
            p.ScriptControl = true; p.AnimOverride = "Limp";
            float t = 0, cycle = 0, sincePause = 0;
            int lastPhase = -1;
            while (Mathf.Abs(p.transform.position.x - x) > 0.05f && t < timeout)
            {
                float dt = Time.deltaTime;
                t += dt; cycle += dt; sincePause += dt;
                float c = (cycle % 1.1f) / 1.1f;
                int phase = c < 0.32f ? 0 : c < 0.72f ? 1 : 2;
                p.ScriptSpeed = phase == 0 ? 1.45f : phase == 1 ? 0.35f : 0.9f;
                if (phase != lastPhase)
                {
                    if (phase == 0) Sfx.Play("sfx_step", 0.55f, 0.9f);
                    if (phase == 1) Sfx.Play("sfx_step_drag", 0.5f);
                    lastPhase = phase;
                }
                p.ScriptMoveX = Mathf.Sign(x - p.transform.position.x);
                if (sincePause > pauseEvery)
                {
                    // 짧은 멈춤: 숨 고르기
                    sincePause = 0;
                    p.ScriptMoveX = 0; p.AnimOverride = "LimpIdle";
                    yield return Wait(0.7f);
                    p.AnimOverride = "Limp";
                    continue;
                }
                yield return null;
            }
            p.ScriptMoveX = 0; p.ScriptSpeed = 0; p.AnimOverride = "LimpIdle";
        }

        public static void LuAnim(Game g, string anim) { if (g.Player != null) g.Player.AnimOverride = anim; }
        public static void LuFace(Game g, int dir) { if (g.Player != null) g.Player.SetFacing(dir); }

        // ============================================================ 카메라
        public static IEnumerator Pan(Vector2 to, float time, bool ease = true)
        {
            var rig = CameraRig.Main;
            if (rig == null) yield break;
            yield return rig.Pan(to, time, ease);
        }

        public static void CamSnap(Vector2 to)
        {
            var rig = CameraRig.Main;
            if (rig == null) return;
            rig.Scripted = true;
            rig.SnapTo(to);
        }

        public static void Shake(float amp, float time) { var rig = CameraRig.Main; if (rig != null) rig.Shake(amp, time); }

        // ============================================================ 효과
        /// <summary>오브젝트를 일정 시간 동안 이동 (파편 낙하·밀려남 등).</summary>
        public static IEnumerator MoveObj(Transform t, Vector3 to, float time, bool easeOut = true)
        {
            if (t == null) yield break;
            Vector3 from = t.position;
            for (float k = 0; k < time; k += Time.deltaTime)
            {
                if (t == null) yield break;
                float e = k / time;
                if (easeOut) e = 1 - (1 - e) * (1 - e);
                t.position = Vector3.Lerp(from, to, e);
                yield return null;
            }
            if (t != null) t.position = to;
        }

        /// <summary>포물선으로 날아가 떨어짐 (밀려나 쓰러짐).</summary>
        public static IEnumerator Arc(Transform t, Vector3 to, float height, float time)
        {
            if (t == null) yield break;
            Vector3 from = t.position;
            for (float k = 0; k < time; k += Time.deltaTime)
            {
                if (t == null) yield break;
                float e = k / time;
                t.position = Vector3.Lerp(from, to, e) + Vector3.up * (4 * height * e * (1 - e));
                yield return null;
            }
            if (t != null) t.position = to;
        }

        /// <summary>가산 혼합 빛 (반지름 유닛). 연출 종료 시 지우려면 Temp 로 등록.</summary>
        public static SpriteRenderer Glow(Vector3 pos, float radius, Color c, int order = 41)
        {
            var cfg = GameConfig.Load();
            var set = cfg != null ? cfg.GetSet("FX_Glow") : null;
            if (set == null || set.frames == null || set.frames.Length == 0) return null;
            var go = new GameObject("Glow");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = set.frames[0];
            sr.color = c;
            sr.sortingOrder = order;
            if (cfg.additive != null) sr.sharedMaterial = cfg.additive;
            float size = set.frames[0].bounds.size.x;
            go.transform.localScale = Vector3.one * (radius * 2f / Mathf.Max(0.01f, size));
            return sr;
        }

        public static IEnumerator FadeSprite(SpriteRenderer sr, float toAlpha, float time)
        {
            if (sr == null) yield break;
            float from = sr.color.a;
            for (float k = 0; k < time; k += Time.deltaTime)
            {
                if (sr == null) yield break;
                var c = sr.color; c.a = Mathf.Lerp(from, toAlpha, k / time); sr.color = c;
                yield return null;
            }
            if (sr != null) { var c = sr.color; c.a = toAlpha; sr.color = c; }
        }

        public static IEnumerator ScaleTo(Transform t, Vector3 to, float time)
        {
            if (t == null) yield break;
            Vector3 from = t.localScale;
            for (float k = 0; k < time; k += Time.deltaTime)
            {
                if (t == null) yield break;
                t.localScale = Vector3.Lerp(from, to, k / time);
                yield return null;
            }
            if (t != null) t.localScale = to;
        }
    }
}
