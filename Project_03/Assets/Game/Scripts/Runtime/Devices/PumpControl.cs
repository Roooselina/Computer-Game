using System.Collections;

namespace EW
{
    /// <summary>펌프실의 손잡이 하나 (밸브·차단기·이음쇠·상태판·부품 선반). 실제 동작은 PumpRig 가 한다.</summary>
    public class PumpControl : Interactable
    {
        public PumpRig.Role role;

        public override string Prompt(Game g)
        {
            switch (role)
            {
                case PumpRig.Role.Valve: return PumpRig.ValveOpen ? "배수 밸브 잠그기" : "배수 밸브 열기";
                case PumpRig.Role.Breaker: return PumpRig.PowerOn ? "차단기 내리기" : "차단기 올리기";
                case PumpRig.Role.Coupling: return "이음쇠";
                case PumpRig.Role.Board: return "상태판";
                default: return "부품 선반";
            }
        }

        public override IEnumerator Interact(Game g)
        {
            var rig = PumpRig.I;
            if (rig == null) yield break;
            yield return rig.Use(g, role);
        }
    }
}
