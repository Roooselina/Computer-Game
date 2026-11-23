using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>'전원 복구' 버튼: 같은 선의 꺼진 장치를 다시 켠다. 고장은 고치지 못한다.</summary>
    public class RestoreButton : Interactable
    {
        public string line;
        public string afterDialogueId;
        void Reset() { promptText = "전원 복구"; }

        public override IEnumerator Interact(Game g)
        {
            var models = new List<DeviceModel>();
            foreach (var d in Device.All) models.Add(d.Model);
            var before = new Dictionary<Device, DevicePower>();
            foreach (var d in Device.All) before[d] = d.Model.power;
            int n = PowerRules.RestoreLine(models, line);
            foreach (var d in Device.All) if (before[d] != d.Model.power) d.SetPower(d.Model.power);
            Fx.Spawn("Spark", transform.position + Vector3.up * 0.6f, false, 30, 0.6f);
            if (n > 0)
            {
                yield return g.Info("전원이 다시 들어왔다. (" + n + "개 장치)");
                if (!string.IsNullOrEmpty(afterDialogueId)) yield return g.Say(afterDialogueId);
            }
            else
            {
                bool broken = false;
                foreach (var d in Device.All) if (d.Model.line == line && d.Model.power == DevicePower.Broken) broken = true;
                yield return g.Info(broken ? "고장 난 장치는 버튼으로 켜지지 않는다." : "이미 전원이 들어와 있다.");
            }
        }
    }
}
