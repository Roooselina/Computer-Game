using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>배터리: 큰 +/− 표시와 잔량 막대. 한 번 조사로 루의 전하를 채우거나 정해진 기계에 보충한다.</summary>
    public class BatteryPack : Interactable
    {
        public Device self;           // 잔량 저장용 (kind=Machine, hasBattery=true)
        public Device chargeTarget;   // 보충 대상 (선택)
        public int perUse = 25;
        void Reset() { promptText = "배터리"; }

        public override IEnumerator Interact(Game g)
        {
            var m = self.Model;
            yield return g.Info("배터리 잔량 " + m.battery + "%.");
            if (m.battery <= 0) { yield return g.Info("배터리가 비어 있다."); yield break; }
            var opts = new List<string> { "루의 전하 채우기" };
            if (chargeTarget != null && chargeTarget.Model.hasBattery) opts.Add(chargeTarget.name + "에 보충");
            opts.Add("그만두기");
            yield return g.Choose(opts);
            if (g.LastChoice == opts.Count - 1) yield break;
            m.battery = Mathf.Max(0, m.battery - perUse);
            self.Changed();
            if (g.LastChoice == 0)
            {
                g.State.D.lu.charge = g.State.MaxCharge("lu");
                Fx.Spawn("Spark", g.Player.transform.position + Vector3.up * 0.7f);
                yield return g.Info("전하를 가득 채웠다. (배터리 " + m.battery + "%)");
            }
            else
            {
                chargeTarget.Model.battery = Mathf.Min(100, chargeTarget.Model.battery + perUse);
                if (chargeTarget.Model.power == DevicePower.Off) chargeTarget.SetPower(DevicePower.On); else chargeTarget.Changed();
                yield return g.Info(chargeTarget.name + "의 배터리를 보충했다.");
            }
        }
    }
}
