using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>문·출입구 (조사해서 이동). 장치가 연결되면 전원·제어 단말 상태를 따른다.</summary>
    public class DoorExit : Interactable
    {
        public string targetMap;
        public string targetSpawn;
        public string requireFlag;
        public string lockedDialogue;
        public Device power;          // 선택: 전원이 필요한 문

        void Reset() { promptText = "들어가기"; }

        public override IEnumerator Interact(Game g)
        {
            if (power != null && !power.Operable)
            {
                yield return g.Info(power.Model.power == DevicePower.Broken ? "문이 고장 났다." : "문 전원이 꺼져 있다. 전원을 켜자.");
                yield break;
            }
            if (!string.IsNullOrEmpty(requireFlag) && !g.State.Has(requireFlag))
            {
                if (!string.IsNullOrEmpty(lockedDialogue)) yield return g.Say(lockedDialogue);
                else yield return g.Info("지금은 지나갈 수 없다.");
                yield break;
            }
            yield return g.LoadMap(targetMap, targetSpawn);
        }
    }
}
