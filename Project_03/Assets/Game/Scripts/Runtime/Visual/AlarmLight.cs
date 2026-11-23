using UnityEngine;

namespace EW
{
    /// <summary>
    /// 경광등. 시설 경보가 켜져 있을 때만 붉은 빔이 돌고 주변이 깜빡인다 (색뿐 아니라 회전하는 모양·움직임으로 상태를 보인다).
    /// 번쩍임 설정을 따른다. 평소엔 꺼진 회색 등이다.
    /// </summary>
    public class AlarmLight : MonoBehaviour
    {
        public Transform beam;
        public SpriteRenderer beamSr, glow;
        public float speed = 120f;
        float ang;

        void Update()
        {
            bool on = Facility.AlarmOn;
            float k = Mathf.Clamp(GameSettings.Flash, 0.25f, 1f);
            if (beamSr != null) beamSr.enabled = on;
            if (!on) { if (glow != null) glow.color = new Color(1f, 0.2f, 0.15f, 0f); return; }
            ang = (ang + speed * Time.deltaTime) % 360f;
            if (beam != null)
            {
                // 옆에서 보는 회전등: 빔이 좌우로 넘어가며 길이가 변한다
                float c = Mathf.Cos(ang * Mathf.Deg2Rad);
                beam.localScale = new Vector3(Mathf.Sign(c) * Mathf.Max(0.15f, Mathf.Abs(c)), 1f, 1f);
            }
            if (beamSr != null) beamSr.color = new Color(1f, 1f, 1f, 0.55f * k);
            if (glow != null) glow.color = new Color(1f, 0.2f, 0.15f, (0.12f + 0.12f * Mathf.Abs(Mathf.Sin(ang * Mathf.Deg2Rad))) * k);
        }
    }
}
