using UnityEngine;

namespace EW
{
    /// <summary>플래그에 따라 그림·충돌을 바꾸는 단순 장면 요소 (깨진 실험 통, 밀린 돌 등).</summary>
    public class FlagVisual : MonoBehaviour
    {
        public string flag;
        public SpriteRenderer target;
        public Sprite unsetSprite, setSprite;
        public Vector3 setOffset;
        [Tooltip("플래그가 없을 때만 막는 콜라이더")] public Collider2D[] blockWhileUnset;
        [Tooltip("플래그가 있을 때만 막는 콜라이더 (내려온 셔터 등)")] public Collider2D[] blockWhileSet;
        [Tooltip("플래그가 있을 때만 보이는 오브젝트")] public GameObject[] showWhenSet;
        [Tooltip("플래그가 있으면 숨기는 오브젝트")] public GameObject[] hideWhenSet;
        Vector3 basePos;
        bool init;
        int last = -1;

        void Update()
        {
            var g = Game.I;
            if (g == null || g.State == null) return;
            if (!init) { init = true; if (target != null) basePos = target.transform.localPosition; }
            bool set = g.State.Has(flag);
            if ((set ? 1 : 0) == last) return;
            last = set ? 1 : 0;
            if (target != null)
            {
                if (set && setSprite != null) target.sprite = setSprite;
                if (!set && unsetSprite != null) target.sprite = unsetSprite;
                target.transform.localPosition = basePos + (set ? setOffset : Vector3.zero);
            }
            if (blockWhileUnset != null) foreach (var c in blockWhileUnset) if (c != null) c.enabled = !set;
            if (blockWhileSet != null) foreach (var c in blockWhileSet) if (c != null) c.enabled = set;
            if (showWhenSet != null) foreach (var o in showWhenSet) if (o != null) o.SetActive(set);
            if (hideWhenSet != null) foreach (var o in hideWhenSet) if (o != null) o.SetActive(!set);
        }
    }
}
