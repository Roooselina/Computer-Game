using UnityEngine;

namespace EW
{
    /// <summary>
    /// 소품의 여러 상태 그림 (예: 문 닫힘/열림/비틀림, 컴퓨터 꺼짐/켜짐/빛남).
    /// rules 는 위에서부터 검사해 처음 맞는 플래그의 그림을 쓴다 (불러오기 후 상태 복원). 연출이 Show 로 직접 바꿀 수도 있다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class FrameSet : MonoBehaviour
    {
        [System.Serializable] public class Rule { public string flag; public int index; }
        public Sprite[] frames;
        public Rule[] rules;
        public int defaultIndex;
        SpriteRenderer sr;
        int lastComputed = -1;
        public int Current { get; private set; } = -1;

        void Awake() { sr = GetComponent<SpriteRenderer>(); }

        public void Show(int i)
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (frames == null || frames.Length == 0) return;
            i = Mathf.Clamp(i, 0, frames.Length - 1);
            Current = i;
            sr.sprite = frames[i];
        }

        void Update()
        {
            var g = Game.I;
            int idx = defaultIndex;
            if (g != null && g.State != null && rules != null)
                foreach (var r in rules) if (!string.IsNullOrEmpty(r.flag) && g.State.Has(r.flag)) { idx = r.index; break; }
            if (idx != lastComputed) { lastComputed = idx; Show(idx); }
        }
    }
}
