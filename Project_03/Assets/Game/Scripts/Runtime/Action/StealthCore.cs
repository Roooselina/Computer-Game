using System;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>맞을 수 있는 것 (경비·드론·보스·소리 나는 물건).</summary>
    public interface IDamageable
    {
        bool Alive { get; }
        Vector2 Center { get; }
        /// <summary>피해·경직. from 은 공격한 위치 (밀려나는 방향).</summary>
        void TakeHit(int damage, float stagger, Vector2 from, string kind);
    }

    /// <summary>맞을 수 있는 것 목록 (근접·투사체 판정용).</summary>
    public static class Damageables
    {
        public static readonly List<IDamageable> All = new List<IDamageable>();
        public static void Add(IDamageable d) { if (!All.Contains(d)) All.Add(d); }
        public static void Remove(IDamageable d) { All.Remove(d); }
    }

    /// <summary>
    /// 소리. 경비는 반경 안의 소리 쪽으로 고개를 돌리고 확인하러 간다.
    /// 소리는 벽을 넘어 들리지만 위치만 알려 줄 뿐 루를 계속 추적하게 하지 않는다.
    /// </summary>
    public static class Noise
    {
        public struct Event { public Vector2 pos; public float radius; public string source; public float time; }
        public static readonly List<Event> Recent = new List<Event>();
        public static event Action<Event> OnNoise;

        public static void Emit(Vector2 pos, float radius, string source)
        {
            if (radius <= 0.01f) return;
            var e = new Event { pos = pos, radius = radius, source = source, time = Time.time };
            Recent.Add(e);
            if (Recent.Count > 32) Recent.RemoveAt(0);
            OnNoise?.Invoke(e);
        }

        public static void Clear() { Recent.Clear(); }
    }

    /// <summary>시야선 계산. 감지와 화면의 시야 표시가 같은 함수를 쓴다.</summary>
    public static class Sight
    {
        static readonly RaycastHit2D[] hits = new RaycastHit2D[16];

        public static bool Blocks(Collider2D c)
        {
            if (c == null) return false;
            if (c.isTrigger) return c.GetComponent<VisionBlocker>() != null;
            if (c.GetComponent<OneWayPlatform>() != null) return false;
            if (c.GetComponent<SeeThrough>() != null) return false;
            if (c.GetComponentInParent<PlayerController>() != null) return false;
            if (c.GetComponentInParent<EnemyBody>() != null) return false;
            return true;
        }

        /// <summary>a → b 사이를 막는 것이 없으면 true.</summary>
        public static bool Clear(Vector2 a, Vector2 b)
        {
            Vector2 d = b - a; float len = d.magnitude;
            if (len < 0.001f) return true;
            var f = new ContactFilter2D { useTriggers = true }; f.SetLayerMask(Physics2D.AllLayers);
            int n = Physics2D.Raycast(a, d / len, f, hits, len);
            for (int i = 0; i < n; i++) if (Blocks(hits[i].collider)) return false;
            return true;
        }

        /// <summary>a 에서 dir 방향으로 range 만큼 쐈을 때 처음 막히는 거리 (시야 표시용).</summary>
        public static float Cast(Vector2 a, Vector2 dir, float range)
        {
            var f = new ContactFilter2D { useTriggers = true }; f.SetLayerMask(Physics2D.AllLayers);
            int n = Physics2D.Raycast(a, dir, f, hits, range);
            float best = range;
            for (int i = 0; i < n; i++) if (Blocks(hits[i].collider) && hits[i].distance < best) best = hits[i].distance;
            return best;
        }

        /// <summary>
        /// 원뿔 시야로 루를 볼 수 있는지. 루의 몸 세 점(발목·가슴·머리, 웅크리면 낮아짐) 중 하나라도
        /// 원뿔 안에 있고 시야선이 막히지 않으면 보인다. 반환값은 보이는 점 수 (0~3).
        /// </summary>
        public static int SeePlayer(Vector2 eye, float dirDeg, float halfAngle, float range, PlayerController p, out float dist)
        {
            dist = 99;
            if (p == null || p.Hidden || p.View == null) return 0;
            if (p.HiddenInCover) return 0;      // 전경 엄폐물 뒤에 숨음 (HideSpot)
            int seen = 0;
            foreach (var pt in p.SightPoints())
            {
                Vector2 d = pt - eye;
                float m = d.magnitude;
                if (m > range) continue;
                float ang = Mathf.Abs(Mathf.DeltaAngle(dirDeg, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg));
                if (ang > halfAngle) continue;
                if (!Clear(eye, pt)) continue;
                seen++; dist = Mathf.Min(dist, m);
            }
            return seen;
        }
    }

}
