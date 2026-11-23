using System.Collections;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 카메라: 루를 따라가되 진행 방향에 여유를 두고 방 경계에서 멈춘다 (GDD 4.1). 픽셀 격자에 맞춘다.
    /// 연출: 지정 위치로 이동(Pan), 정수 배율 확대(Zoom 1×/2×, 전투 구도), 흔들림(설정 배율 적용).
    /// 탐험 기본 구도: 루의 발이 화면 위에서 약 68% (아래에서 32%) — 바닥과 앞쪽 공간이 보이고 캐릭터가 정중앙에 붙지 않는다.
    /// 하단 대화창이 열리면 발이 대화창 위로 오도록 부드럽게 다시 잡는다. 진행 방향에 시야 여유를 두되 방향 전환 때 튀지 않게 천천히 옮긴다.
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        public Transform target;
        public Rect bounds;
        public float lookAhead = 2.2f;
        /// <summary>인물 발 기준 카메라 중심 높이 (유닛). 2.0 → 발이 화면 위에서 약 68% (11.25 화면 높이 기준).</summary>
        public float framingY = 2.0f;
        /// <summary>대화창이 열렸을 때의 구도 (발이 화면 아래 약 37%, 대화창 상단 26% 위).</summary>
        public float framingDialogue = 1.45f;
        float frame = 2.0f;
        Vector2 look;
        Vector2 pos;
        [System.NonSerialized] public Vector2? FocusOverride;
        /// <summary>연출이 위치를 직접 정하는 동안 (Pan 코루틴).</summary>
        [System.NonSerialized] public bool Scripted;
        /// <summary>연출 중에도 방 경계를 지킬지.</summary>
        [System.NonSerialized] public bool ClampScripted = true;
        Camera cam;
        int zoom = 1;
        float shakeAmp, shakeTime, shakeDur;
        Vector2 shakeOff;

        public static CameraRig Main => Camera.main != null ? Camera.main.GetComponent<CameraRig>() : null;
        public Vector2 Position => pos;
        public int ZoomLevel => zoom;
        public float HalfH => PixelScreen.OrthoSize / zoom;
        public float HalfW => HalfH * PixelScreen.W / PixelScreen.H;

        public void Bind(Transform t, Rect b)
        {
            target = t; bounds = b; cam = GetComponent<Camera>();
            SetZoom(1);
            pos = Clamp(t != null ? (Vector2)t.position + Vector2.up * framingY : b.center);
            Apply();
        }

        public Vector2 Clamp(Vector2 p)
        {
            float hh = HalfH, hw = HalfW;
            float x = bounds.width <= hw * 2 ? bounds.center.x : Mathf.Clamp(p.x, bounds.xMin + hw, bounds.xMax - hw);
            float y = bounds.height <= hh * 2 ? bounds.center.y : Mathf.Clamp(p.y, bounds.yMin + hh, bounds.yMax - hh);
            return new Vector2(x, y);
        }

        /// <summary>정수 배율 확대. 2 이면 텍스처 1픽셀이 화면 2픽셀 (흐려지지 않는다).</summary>
        public void SetZoom(int z)
        {
            zoom = Mathf.Clamp(z, 1, 3);
            if (cam == null) cam = GetComponent<Camera>();
            if (cam != null) cam.orthographicSize = PixelScreen.OrthoSize / zoom;
        }

        void LateUpdate()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (!Scripted)
            {
                Vector2 want;
                if (FocusOverride.HasValue) want = FocusOverride.Value;
                else if (target != null)
                {
                    var pc = target.GetComponent<PlayerController>();
                    float f = pc != null ? pc.Facing : 1;
                    // 방향을 바꾸면 시야 여유가 천천히 반대쪽으로 넘어간다 (즉시 튀지 않음)
                    look.x = Mathf.MoveTowards(look.x, f * lookAhead, Time.deltaTime * 2.4f);
                    var gm = Game.I;
                    bool dlg = gm != null && gm.UI != null && gm.UI.dialogue.Open;
                    frame = Mathf.MoveTowards(frame, dlg ? framingDialogue : framingY, Time.deltaTime * 2.5f);
                    want = (Vector2)target.position + Vector2.up * frame + look;
                }
                else want = pos;
                pos = Vector2.Lerp(pos, Clamp(want), 1f - Mathf.Exp(-Time.deltaTime * 7f));
            }
            // 흔들림 (설정 배율, 시간이 갈수록 줄어듦)
            if (shakeTime > 0)
            {
                shakeTime -= Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(shakeTime / Mathf.Max(0.01f, shakeDur)) * shakeAmp * GameSettings.Shake;
                shakeOff = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * k;
            }
            else shakeOff = Vector2.zero;
            Apply();
        }

        public void Snap() { if (target != null) pos = Clamp((Vector2)target.position + Vector2.up * framingY); Apply(); }
        public void SnapTo(Vector2 p) { pos = Scripted && !ClampScripted ? p : Clamp(p); Apply(); }

        public void Shake(float amplitude, float time)
        {
            if (amplitude * GameSettings.Shake <= 0.001f) return;
            shakeAmp = Mathf.Max(shakeAmp * (shakeTime > 0 ? 1 : 0), amplitude);
            shakeDur = Mathf.Max(time, shakeTime);
            shakeTime = shakeDur;
        }

        /// <summary>지정 위치로 부드럽게 이동 (연출). 끝나면 Scripted 상태로 남는다 — Release() 로 추적 복귀.</summary>
        public IEnumerator Pan(Vector2 to, float time, bool ease = true)
        {
            Scripted = true;
            Vector2 from = pos;
            Vector2 dst = ClampScripted ? Clamp(to) : to;
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                float k = t / time;
                if (ease) k = k * k * (3 - 2 * k);
                pos = Vector2.Lerp(from, dst, k);
                yield return null;
            }
            pos = dst;
        }

        public void Release() { Scripted = false; FocusOverride = null; ClampScripted = true; }

        void Apply()
        {
            float px = 1f / (PixelScreen.PPU * zoom);
            Vector2 p = pos + shakeOff;
            transform.position = new Vector3(Mathf.Round(p.x / px) * px, Mathf.Round(p.y / px) * px, -10);
        }
    }
}
