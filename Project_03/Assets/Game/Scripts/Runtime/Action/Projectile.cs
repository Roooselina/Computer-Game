using UnityEngine;

namespace EW
{
    /// <summary>스파크 샷의 작은 전기탄. 벽에 닿으면 사라지고, 맞은 대상에 피해·경직을 준다.</summary>
    public class Projectile : MonoBehaviour
    {
        Vector2 dir; float speed, life; ActionTuning.SkillT s;
        Vector2 last;
        public bool hostile;
        public int damage;

        public static Projectile Fire(Vector2 from, Vector2 dir, ActionTuning.SkillT s)
        {
            var go = new GameObject("SparkShot");
            go.transform.position = from;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sortingOrder = 32;
            var g = Game.I;
            var set = g != null ? g.Config.GetSet("FX_LightDot") : null;
            var fb = go.AddComponent<Flipbook>();
            if (set != null) fb.SetFrames(set.frames, 14, true);
            var p = go.AddComponent<Projectile>();
            p.dir = dir.normalized; p.speed = s.speed; p.life = s.range / Mathf.Max(1f, s.speed); p.s = s; p.last = from; p.damage = s.damage;
            return p;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            life -= dt;
            Vector2 pos = transform.position;
            Vector2 next = pos + dir * speed * dt;
            if (!Sight.Clear(pos, next)) { Fx.Spawn("Spark", next, false, 31, 0.4f); Destroy(gameObject); return; }
            transform.position = next;
            foreach (var d in Damageables.All.ToArray())
            {
                if (d == null || !d.Alive) continue;
                if (Vector2.Distance(d.Center, next) > 0.55f) continue;
                d.TakeHit(damage, s.stagger, pos - dir, "electric");
                Fx.Spawn("Spark", next, false, 31, 0.5f);
                Destroy(gameObject); return;
            }
            if (life <= 0) Destroy(gameObject);
        }
    }
}
