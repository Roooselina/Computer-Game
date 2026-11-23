using UnityEngine;

namespace EW
{
    /// <summary>
    /// 베넬 복도의 과부하 분전함. 베넬이 처음 약을 다시 섞은 뒤(v_junction_live) 불꽃이 튀기 시작해 근처(베넬 쪽)를 위협한다.
    /// 약한 간섭으로 꺼 주면 '행동으로 증명' 조건(c3_proof)이 된다. 꺼진 뒤에는 다시 켜지지 않는다.
    /// </summary>
    public class JunctionHazard : ZapTarget
    {
        public float radius = 1.6f;
        public int damage = 5;
        public string activeFlagStage = "2";
        float arcT;

        bool Active => Game.I != null && Game.I.State != null && Game.I.State.Has("v_junction_live") && !Game.I.State.Has("junction_off") && !Game.I.State.Has("c3_boss_done");
        public override bool CanJam => Active;
        public override bool CanPull => false;
        public override string JamResult => "튀는 불꽃을 끈다 (베넬이 본다)";

        public override void Jam(float seconds)
        {
            var g = Game.I; if (g == null) return;
            g.State.Set("junction_off");
            g.State.Set("c3_proof");
            VenelleMind.OnJunctionOff();
            Sfx.Play("sfx_power_down", 0.7f, 1.2f);
            var v = FindAnyObjectByType<VenelleBoss>();
            if (v != null) v.Say("...어? 따끔한 거... 꺼졌네...?", 3f);
        }

        void Update()
        {
            if (!Active) return;
            float dt = Hazard.Dt; if (dt <= 0) return;
            arcT -= dt;
            if (arcT > 0) return;
            arcT = 1.8f;
            Fx.Spawn("Arc", AimPoint + new Vector2(Random.Range(-0.6f, 0.6f), Random.Range(-0.4f, 0.2f)), Random.value < 0.5f, 31, 0.7f);
            Sfx.Play("sfx_spark_small", 0.6f);
            var p = Game.I.Player;
            if (p != null && Vector2.Distance(p.Chest, AimPoint) < radius) p.Hurt(damage, AimPoint, "electric");
        }
    }
}
