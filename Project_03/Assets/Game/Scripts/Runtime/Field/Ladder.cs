using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>사다리. 트리거 콜라이더 범위가 오를 수 있는 구간이다.</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class Ladder : MonoBehaviour
    {
        [Tooltip("올라간 뒤 서게 되는 바닥 높이(월드 y).")]
        public float topY;
        public float bottomY;
        public float CenterX => transform.position.x;
        public float Top => topY;
        public float Bottom => bottomY;
    }
}
