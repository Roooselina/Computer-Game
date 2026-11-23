using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>교체 선: 조사하면 끊어진 전원선이 자동으로 연결된다 (배선 미니게임 없음).</summary>
    public class WireCoil : Interactable
    {
        public string fixFlag;
        public PowerLine line;
        void Reset() { promptText = "교체 선"; }
        public override bool CanInteract(Game g) => !g.State.Has(fixFlag);
        public override void OnStateLoaded(GameState s) { if (s.Has(fixFlag)) GetComponent<SpriteRenderer>().enabled = false; }

        public override IEnumerator Interact(Game g)
        {
            Fx.Spawn("Spark", transform.position + Vector3.up * 0.4f);
            if (line != null) Fx.Spawn("Spark", line.transform.position + Vector3.up * 0.6f);
            g.State.Set(fixFlag);
            GetComponent<SpriteRenderer>().enabled = false;
            yield return g.Say("sys_line_fixed");
        }
    }
}
