using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// CCTV 제어함. 약한 간섭: 연결된 CCTV 가 일정 시간 꺼진다 (조용함, 화면에서 렌즈가 꺼지고 고개가 처짐).
    /// 전력 끌어오기: 제어함이 타면서 연결된 CCTV 가 계속 꺼진다 (큰 소리).
    /// </summary>
    public class CctvControl : ZapTarget
    {
        public List<Cctv> cameras = new List<Cctv>();
        public SpriteRenderer screen;
        float jamT;

        public bool Burned => Game.I != null && Game.I.State != null && Game.I.State.Has("ctrl_broken_" + id);
        public override bool CanJam => !Burned;
        public override bool CanPull => !Burned;
        public override string JamResult => "연결된 CCTV " + cameras.Count + "대 " + Mathf.RoundToInt(ActionTuning.I.Skill("jam")?.effectTime ?? 8f) + "초 정지 (조용함)";
        public override string PullResult => "제어함을 태워 CCTV 를 계속 끔 (큰 소리)";

        public override void Jam(float seconds)
        {
            jamT = seconds;
            foreach (var c in cameras) if (c != null) c.Jam(seconds);
            Sfx.Play("sfx_power_down", 0.45f, 1.7f);
        }

        public override void Pull(PowerSource src)
        {
            var g = Game.I;
            if (g != null && g.State != null) g.State.Set("ctrl_broken_" + id);
            foreach (var c in cameras) if (c != null) c.Break();
        }

        void Update()
        {
            if (jamT > 0) jamT -= Hazard.Dt;
            if (screen != null)
                screen.color = Burned ? new Color(0.2f, 0.2f, 0.22f) : jamT > 0 ? ((Time.time * 6f % 1f) < 0.5f ? new Color(0.9f, 0.9f, 1f) : new Color(0.3f, 0.35f, 0.4f)) : new Color(0.5f, 1f, 0.75f);
        }
    }
}
