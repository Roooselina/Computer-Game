using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 시간 배율을 한 곳에서 관리한다. 조준 느리게·히트스톱·일시정지가 서로의 값을 덮어쓰지 않도록
    /// 이름 붙은 요청을 모아 가장 작은 값을 Time.timeScale 로 쓴다. 요청이 모두 사라지면 정확히 1 로 돌아온다.
    /// </summary>
    public static class TimeCtl
    {
        static readonly Dictionary<string, float> req = new Dictionary<string, float>();

        public static void Set(string key, float scale) { req[key] = Mathf.Clamp01(scale); Apply(); }
        public static void Clear(string key) { if (req.Remove(key)) Apply(); }
        public static bool Has(string key) => req.ContainsKey(key);
        public static void ClearAll() { req.Clear(); Apply(); }
        public static int Count => req.Count;

        static void Apply()
        {
            float s = 1f;
            foreach (var v in req.Values) s = Mathf.Min(s, v);
            Time.timeScale = s;
        }
    }

    /// <summary>
    /// 위험 요소(경비·CCTV·보스·투사체·발각 게이지)가 쓰는 시간.
    /// 대화·컷신·일시정지·불러오기 중에는 0 이라 공격과 위험 타이머가 일관되게 멈춘다.
    /// </summary>
    public static class Hazard
    {
        /// <summary>테스트·연출용 강제 정지.</summary>
        public static bool ForcePause;

        public static bool Paused
        {
            get
            {
                if (ForcePause) return true;
                var g = Game.I;
                if (g == null || g.State == null) return true;
                if (g.Loading || Cutscene.Running) return true;
                if (g.UI != null && g.UI.dialogue != null && g.UI.dialogue.Open) return true;
                var c = GameInput.Context;
                if (c == InputCtx.Dialogue || c == InputCtx.Menu || c == InputCtx.Pause || c == InputCtx.Cutscene) return true;
                // 조사 연출·문 이동 등 이벤트 중 (조준은 이벤트가 아니므로 시간만 느려진다)
                if (g.InEvent) return true;
                return false;
            }
        }

        public static float Dt => Paused ? 0f : Time.deltaTime;
    }
}
