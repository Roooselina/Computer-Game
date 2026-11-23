using UnityEngine;

namespace EW
{
    /// <summary>
    /// 전력이 끊긴 셔터 제어판. 외부 전력을 끌어와 넣으면 셔터가 열린다 (setFlag).
    /// 필수 통로에는 쓰지 않는다 — 지름길·선택 방 전용 (영구 진행 불가 방지).
    /// </summary>
    public class ShutterPanel : ZapTarget
    {
        public string setFlag;
        public SpriteRenderer lamp;
        bool Done => Game.I != null && Game.I.State != null && Game.I.State.Has(setFlag);
        public override bool CanJam => false;
        public override bool CanPull => !Done;
        public override string PullResult => "제어판에 전력 공급 → 셔터 열림";

        public override void Pull(PowerSource src)
        {
            var g = Game.I;
            if (g == null || g.State == null) return;
            g.State.Set(setFlag);
            Sfx.Play("sfx_door_slide", 0.9f);
            g.State.Reward("panel_" + id, ActionTuning.I.expClue);
        }

        void Update()
        {
            if (lamp != null) lamp.color = Done ? new Color(0.5f, 1f, 0.6f) : new Color(1f, 0.35f, 0.3f);
        }
    }
}
