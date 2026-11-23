using UnityEngine;

namespace EW
{
    /// <summary>소품·효과용 단순 프레임 재생기 (캐릭터 애니메이션은 Animator 사용).</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Flipbook : MonoBehaviour
    {
        public Sprite[] frames;
        public float fps = 10f;
        public bool loop = true;
        public bool destroyOnEnd;
        public bool unscaled;
        SpriteRenderer sr;
        float t;

        void Awake() { sr = GetComponent<SpriteRenderer>(); }

        public void SetFrames(Sprite[] f, float speed, bool doLoop)
        {
            frames = f; fps = speed; loop = doLoop; t = 0;
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (frames != null && frames.Length > 0) sr.sprite = frames[0];
        }

        void Update()
        {
            if (frames == null || frames.Length == 0) return;
            t += (unscaled ? Time.unscaledDeltaTime : Time.deltaTime) * fps;
            int i = (int)t;
            if (i >= frames.Length)
            {
                if (loop) { t %= frames.Length; i = (int)t; }
                else if (destroyOnEnd) { Destroy(gameObject); return; }
                else i = frames.Length - 1;
            }
            sr.sprite = frames[i];
        }
    }

    public static class Fx
    {
        public static GameObject Spawn(string set, Vector3 pos, bool flipX = false, int order = 30, float scale = 1f)
        {
            var cfg = GameConfig.Load();
            var s = cfg != null ? cfg.GetSet("FX_" + set) : null;
            if (s == null || s.frames == null || s.frames.Length == 0) return null;
            var go = new GameObject("FX_" + set);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order; sr.flipX = flipX;
            var fb = go.AddComponent<Flipbook>();
            fb.destroyOnEnd = true;
            fb.SetFrames(s.frames, s.fps > 0 ? s.fps : 14f, false);
            return go;
        }
    }
}
