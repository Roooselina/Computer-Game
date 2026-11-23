using UnityEngine;

namespace EW
{
    /// <summary>
    /// 한 전원선의 제어함 (예: 컨베이어 홀의 벨트 제어함). '외부 전력 방전'(E/R 슬롯의 방전)으로 과부하시키면 그 선 전체가 정전된다:
    /// 같은 선의 벨트·PC·문이 함께 멈추고, 배터리가 보이는 장치만 켜져 있다. '전원 복구' 버튼으로 다시 켠다.
    /// 약한 간섭은 화면을 잠깐 깜빡이게 할 뿐 정전시키지 않는다. 전기를 썼다고 암호·파일 권한이 바뀌지는 않는다.
    /// </summary>
    public class GridBox : ZapTarget
    {
        public string line;
        public SpriteRenderer art;
        float flicker;

        public bool LineOn
        {
            get { foreach (var d in Device.All) if (d != null && d.Model.line == line && !d.Model.hasBattery) return d.Model.power == DevicePower.On; return true; }
        }

        public override bool CanPull => LineOn;
        public override bool CanJam => LineOn;
        public override string PullResult => "과부하 → 같은 선 전체 정전";
        public override string JamResult => "잠깐 깜빡임";
        public override float PullPriority => 1.5f;

        public override void Jam(float seconds) { flicker = 0.6f; }

        public override void Pull(PowerSource src)
        {
            Fx.Spawn("Explosion", AimPoint, false, 31, 0.8f);
            Sfx.Play("sfx_discharge_big", 0.9f);
            PlayerSkills.GridOutage(line);
            Noise.Emit(AimPoint, 9f, "gridbox");
            if (Game.I != null && Game.I.State != null) Game.I.State.Set("overloaded_" + line);
        }

        void Update()
        {
            if (art == null) return;
            if (flicker > 0) { flicker -= Time.deltaTime; art.color = (Time.time * 20f % 1f) < 0.5f ? new Color(0.6f, 0.6f, 0.7f) : Color.white; }
            else art.color = LineOn ? Color.white : new Color(0.45f, 0.45f, 0.5f);
        }
    }
}
