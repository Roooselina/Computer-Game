using UnityEngine;

namespace EW
{
    /// <summary>
    /// 깊이 레이어. follow = 카메라를 따라가는 비율 (0 = 플레이 영역과 같이 움직임, 0.8 = 아주 먼 원경, 음수 = 카메라 앞 전경).
    /// 기준점(reference)에서 카메라가 있을 때 저작한 배치 그대로 보인다. 위치는 픽셀 격자에 맞춘다.
    /// </summary>
    public class Parallax : MonoBehaviour
    {
        public float follow = 0.5f;
        [Tooltip("세로 이동 비율 (가로 비율에 곱함)")] public float vertical = 0.5f;
        public Vector2 reference;
        Vector3 basePos;
        bool init;

        void Start() { Init(); }

        void Init()
        {
            if (init) return;
            init = true;
            basePos = transform.position;
        }

        void LateUpdate()
        {
            Init();
            var cam = Camera.main;
            if (cam == null) return;
            Vector2 c = cam.transform.position;
            Vector2 d = c - reference;
            Vector3 p = basePos + new Vector3(d.x * follow, d.y * follow * vertical, 0);
            const float px = 1f / PixelScreen.PPU;
            p.x = Mathf.Round(p.x / px) * px; p.y = Mathf.Round(p.y / px) * px;
            transform.position = p;
        }
    }
}
