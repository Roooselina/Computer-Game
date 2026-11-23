using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// B형 엄폐물: '뒤에 숨는' 전경 물체 (선반·적재 자루·칸막이 등). 고체 상자(Cover, 발판·길막)와 역할을 나눈다.
    /// - 충돌체는 트리거라 루가 뒤쪽 영역으로 들어갈 수 있다. 그림은 루보다 앞에 그린다.
    /// - 영역 안에서 S 로 웅크리면 '숨음': CCTV·경비의 시야 판정(Sight.SeePlayer)에서 빠진다.
    /// - 숨어 있는 동안 루는 엄폐물 앞에 옅은 윤곽으로 보인다 (플레이어가 위치를 안다).
    /// - 영역 밖으로 나가거나, 일어서거나, 공격하면 바로 노출된다.
    /// - 숨었다고 적의 기억이 지워지지 않는다: 추적 중이던 적은 마지막 발견 위치를 수색하고,
    ///   '보는 앞에서' 숨었다면(숨는 순간 시야 안) 바로 옆까지 온 경비에게 들킨다 (Guard.cs).
    /// </summary>
    public class HideSpot : MonoBehaviour
    {
        public static readonly List<HideSpot> All = new List<HideSpot>();
        public SpriteRenderer art;
        [Tooltip("루가 숨었을 때 엄폐물 그림의 불투명도 (윤곽이 비쳐 보이게)")] public float hiddenAlpha = 0.92f;
        BoxCollider2D box;

        void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        void OnDisable() { All.Remove(this); }
        void Awake() { box = GetComponent<BoxCollider2D>(); }

        public Bounds B => box != null ? box.bounds : new Bounds(transform.position, Vector3.one);

        /// <summary>루의 몸(발 위치 기준 폭 0.56)이 영역 안에 거의 다 들어와 있는지.</summary>
        public bool Contains(Vector2 feet)
        {
            var b = B;
            if (feet.y < b.min.y - 0.15f || feet.y > b.min.y + 0.6f) return false;
            return feet.x - 0.16f >= b.min.x && feet.x + 0.16f <= b.max.x;
        }

        public static HideSpot At(Vector2 feet)
        {
            foreach (var h in All) if (h != null && h.isActiveAndEnabled && h.Contains(feet)) return h;
            return null;
        }

        public void SetOccupied(bool on)
        {
            if (art == null) return;
            var c = art.color; c.a = on ? hiddenAlpha : 1f; art.color = c;
        }
    }
}
