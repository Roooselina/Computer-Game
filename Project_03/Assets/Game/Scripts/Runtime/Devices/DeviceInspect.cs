using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>장치 조사: 상태를 한 줄로 알려준다 (GDD 3.3).</summary>
    public class DeviceInspect : Interactable
    {
        public Device device;
        public string extraDialogueId;     // 상태 설명 앞에 나올 대사(선택)

        public override IEnumerator Interact(Game g)
        {
            if (!string.IsNullOrEmpty(extraDialogueId)) yield return g.Say(extraDialogueId);
            yield return g.Info(PowerRules.Describe(device.Model));
        }
    }
}
