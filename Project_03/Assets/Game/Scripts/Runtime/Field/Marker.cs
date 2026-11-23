using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>연출이 이름으로 찾는 장면 지점·소품 (카메라 위치, 문, 조명 등). 구성 도구가 배치한다.</summary>
    public class Marker : MonoBehaviour
    {
        public string id;
        static readonly List<Marker> all = new List<Marker>();
        void OnEnable() { if (!all.Contains(this)) all.Add(this); }
        void OnDisable() { all.Remove(this); }

        public static Marker Find(string id)
        {
            foreach (var m in all) if (m != null && m.id == id) return m;
            return null;
        }

        public static Vector2 Pos(string id, Vector2 fallback)
        {
            var m = Find(id);
            return m != null ? (Vector2)m.transform.position : fallback;
        }

        public static IEnumerable<Marker> Prefix(string prefix)
        {
            foreach (var m in all.ToArray()) if (m != null && m.id != null && m.id.StartsWith(prefix)) yield return m;
        }

        public SpriteRenderer SR => GetComponent<SpriteRenderer>();
    }
}
