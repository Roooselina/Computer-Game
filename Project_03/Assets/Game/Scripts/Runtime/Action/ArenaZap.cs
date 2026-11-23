using UnityEngine;

namespace EW
{
    /// <summary>보스 구역의 전기 대상 (바닥 연결함·축전 모듈). 외부 전력 방전(E)·간섭(W)이 닿는다.</summary>
    public class ArenaZap : ZapTarget
    {
        public static IArenaUser User;
        public string kind = "floor";
        public override bool CanPull => User != null;
        public override bool CanJam => false;
        public override string PullResult => kind == "floor" ? "전도성 바닥에 전류 (메이린이 위에 있으면 경직)" : kind == "capacitor" ? "축전 모듈 과부하 → 결합부 노출" : "전류";
        public override float PullPriority => 2f;
        public override void Pull(PowerSource src)
        {
            if (User is MeilinBoss m) m.FloorZap();
            else if (User is AureliusBoss a) a.CapacitorZap(this);
        }
    }
}
