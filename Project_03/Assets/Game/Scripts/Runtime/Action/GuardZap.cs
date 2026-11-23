using UnityEngine;

namespace EW
{
    /// <summary>경비 기계를 타깃형 스킬로 고를 수 있게 한다 (간섭: 잠깐 멈춤 / 끌어오기: 일시 무력화).</summary>
    public class GuardZap : ZapTarget
    {
        public Guard guard;
        public override bool CanJam => guard != null && guard.Alive;
        public override bool CanPull => guard != null && guard.Alive;
        public override string JamResult => "경비 " + ActionTuning.I.Enemy(guard != null ? guard.enemyId : "").jamTime.ToString("0.#") + "초 멈춤 (조용함)";
        public override string PullResult => "경비 일시 무력화 (큰 소리·빛)";
        public override void Jam(float seconds) { if (guard != null) guard.Jam(ActionTuning.I.Enemy(guard.enemyId).jamTime); }
        public override void Pull(PowerSource src) { if (guard != null) guard.KnockOut(); }
    }
}
