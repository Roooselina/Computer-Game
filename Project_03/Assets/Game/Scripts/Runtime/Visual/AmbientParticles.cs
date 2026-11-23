using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 화면 안에 떠다니는 작은 입자 (불티·재·먼지·잔류 스파크). 카메라 주변에서만 만들고 재사용한다.
    /// 강한 점멸을 만들지 않도록 밝기 변화는 느리게 둔다.
    /// </summary>
    public class AmbientParticles : MonoBehaviour
    {
        public Sprite sprite;
        public int count = 40;
        public Vector2 velocity = new Vector2(0.3f, 0.8f);
        public Vector2 velocityJitter = new Vector2(0.4f, 0.3f);
        public float sway = 0.4f;
        public Color colorA = new Color(1f, 0.7f, 0.3f, 1f);
        public Color colorB = new Color(1f, 0.4f, 0.2f, 0.6f);
        public float life = 4f;
        public int sortingOrder = 25;
        public bool additive;
        [Tooltip("이 플래그가 있을 때만 (비우면 항상)")] public string requireFlag;
        [Tooltip("이 플래그가 있으면 끈다")] public string hideFlag;

        class P { public Transform t; public SpriteRenderer sr; public Vector2 v; public float age, life, phase; }
        readonly List<P> ps = new List<P>();

        void Start()
        {
            var cfg = GameConfig.Load();
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("p");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite; sr.sortingOrder = sortingOrder;
                if (additive && cfg != null && cfg.additive != null) sr.sharedMaterial = cfg.additive;
                var p = new P { t = go.transform, sr = sr };
                Respawn(p, true);
                ps.Add(p);
            }
        }

        bool Active()
        {
            var g = Game.I;
            if (g == null || g.State == null) return string.IsNullOrEmpty(requireFlag);
            if (!string.IsNullOrEmpty(requireFlag) && !g.State.Has(requireFlag)) return false;
            if (!string.IsNullOrEmpty(hideFlag) && g.State.Has(hideFlag)) return false;
            return true;
        }

        void Respawn(P p, bool anywhere)
        {
            var cam = Camera.main;
            Vector2 c = cam != null ? (Vector2)cam.transform.position : (Vector2)transform.position;
            float hw = 11f, hh = 6.5f;
            float x = c.x + Random.Range(-hw, hw);
            float y = anywhere ? c.y + Random.Range(-hh, hh) : c.y - Mathf.Sign(velocity.y + 0.0001f) * hh;
            p.t.position = new Vector3(x, y, 0);
            p.v = velocity + new Vector2(Random.Range(-1f, 1f) * velocityJitter.x, Random.Range(-1f, 1f) * velocityJitter.y);
            p.age = anywhere ? Random.Range(0, life) : 0;
            p.life = life * Random.Range(0.6f, 1.2f);
            p.phase = Random.Range(0, 6.28f);
            p.sr.color = Color.Lerp(colorA, colorB, Random.value);
        }

        void Update()
        {
            bool on = Active();
            foreach (var p in ps)
            {
                if (!on) { p.sr.enabled = false; continue; }
                p.sr.enabled = true;
                p.age += Time.deltaTime;
                if (p.age > p.life) { Respawn(p, false); continue; }
                var pos = p.t.position;
                pos.x += (p.v.x + Mathf.Sin(p.age * 1.7f + p.phase) * sway) * Time.deltaTime;
                pos.y += p.v.y * Time.deltaTime;
                p.t.position = pos;
                var col = p.sr.color;
                float fade = Mathf.Clamp01(Mathf.Min(p.age, p.life - p.age) / 0.6f);
                col.a = Mathf.Lerp(colorB.a, colorA.a, 0.5f) * fade;
                p.sr.color = col;
            }
        }
    }
}
