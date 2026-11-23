using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 낮은 엄폐물 표식. 실제 판정은 시야선이 이 고체에 막히는지로 하고, 이 표식은
    /// '뒤에서 S 로 웅크리면 숨는다' 안내와 HUD 의 '숨음' 표시에만 쓴다.
    /// </summary>
    public class Cover : MonoBehaviour
    {
        public static readonly List<Cover> All = new List<Cover>();
        public float height = 1f;
        void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        void OnDisable() { All.Remove(this); }
        public Bounds B { get { var c = GetComponent<Collider2D>(); return c != null ? c.bounds : new Bounds(transform.position, Vector3.one); } }

        /// <summary>루가 이 엄폐물 바로 옆(0.9 유닛 안)에 있는지.</summary>
        public static Cover Near(Vector2 feet)
        {
            foreach (var c in All)
            {
                if (c == null) continue;
                var b = c.B;
                if (feet.y < b.min.y - 0.2f || feet.y > b.max.y + 0.1f) continue;
                if (feet.x > b.min.x - 0.9f && feet.x < b.max.x + 0.9f) return c;
            }
            return null;
        }
    }
}
