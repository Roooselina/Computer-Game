using UnityEngine;

namespace EW
{
    /// <summary>부축받는 베넬: 루의 뒤쪽 어깨 옆에 붙어 같이 걷는다 (GDD 3장, 10.2 '루의 부축').</summary>
    public class CarriedFollow : MonoBehaviour
    {
        PlayerController p;
        CharacterView v;

        public void Init(PlayerController player, CharacterView view) { p = player; v = view; }

        void LateUpdate()
        {
            if (p == null) return;
            transform.localPosition = new Vector3(-p.Facing * 0.42f, 0, 0);
            v.Face(p.Facing);
            v.sr.sortingOrder = 11;
            if (v.animator != null) v.animator.speed = Mathf.Abs(p.Velocity.x) > 0.05f ? 1f : 0f;
            v.Play("Supported");
        }
    }
}
