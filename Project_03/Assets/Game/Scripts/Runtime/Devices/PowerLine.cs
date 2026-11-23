using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 전원선 입구 (GDD 3.2). 2026-11-15: J 조사가 아니라 R(전원선 이동 기술)로 들어간다 (PlayerSkills).
    /// 들어가는 순간 몸이 빛으로 바뀌어 선을 따라 달리고, 출구 콘센트에서 다시 나온다.
    /// 동료(베넬)는 루와 닿아 있을 때만 함께 간다: 루가 손을 잡아 끌어당긴 뒤 둘이 같이 전기화한다
    /// (따로 순간이동하지 않는다). 장거리 전송·벽 통과 능력이 아니다 — 이어진 선의 입구·출구 사이만.
    /// 끊어진 선은 교체 선을 연결해야 한다. 전기가 꺼져 있어도 루 자신의 전기로 이동한다.
    /// </summary>
    public class PowerLine : Interactable
    {
        public Transform exit;
        public Vector3[] path;            // 월드 좌표 경로 (입구 → 출구)
        public string fixFlag;            // 비어 있으면 처음부터 연결됨
        public SpriteRenderer socket;
        public Sprite idleSprite, glowSprite, brokenSprite;
        public SpriteRenderer exitSocket;
        public LineRenderer lineRenderer;

        void Reset() { promptText = "전원선"; }

        public bool Connected(Game g) => string.IsNullOrEmpty(fixFlag) || (g != null && g.State != null && g.State.Has(fixFlag));

        void Update()
        {
            var g = Game.I;
            bool ok = Connected(g);
            if (socket != null) socket.sprite = ok ? ((Time.time * 2f % 1f) < 0.5f ? idleSprite : glowSprite) : brokenSprite;
            if (lineRenderer != null) lineRenderer.startColor = lineRenderer.endColor = ok ? new Color(0.25f, 0.24f, 0.2f) : new Color(0.3f, 0.12f, 0.1f);
        }

        public override string Prompt(Game g) => Connected(g) ? "전원선 이동" : "전원선 (끊어짐)";
        /// <summary>J 로는 열지 않는다 (R 기술 프롬프트가 대신 뜬다).</summary>
        public override bool CanInteract(Game g) => false;
        /// <summary>함께 이동할 동료 (루와 닿아 있는 경우). Travel 동안만 쓴다.</summary>
        [System.NonSerialized] public Companion carryCompanion;

        public override IEnumerator Interact(Game g)
        {
            if (!Connected(g)) { yield return g.Say("sys_line_broken"); yield break; }
            var rig = Camera.main != null ? Camera.main.GetComponent<CameraRig>() : null;
            // 출구 미리 표시
            if (rig != null) rig.FocusOverride = (Vector2)exit.position + Vector2.up * 1f;
            if (exitSocket != null) StartCoroutine(Blink(exitSocket));
            Fx.Spawn("Spark", exit.position + Vector3.up * 0.6f);
            yield return new WaitForSeconds(0.7f);
            yield return g.Choose(new[] { "이동한다", "그만둔다" });
            if (rig != null) rig.FocusOverride = null;
            if (g.LastChoice != 0) yield break;
            yield return Travel(g, g.Player.transform, g.Player.View, true);
        }

        IEnumerator Blink(SpriteRenderer s)
        {
            for (int i = 0; i < 8; i++) { s.color = i % 2 == 0 ? new Color(1, 1, 0.5f) : Color.white; yield return new WaitForSeconds(0.12f); }
            s.color = Color.white;
        }

        /// <summary>루 또는 동료를 경로를 따라 이동시킨다. 전선 안에서 조작 입력은 없다.</summary>
        public IEnumerator Travel(Game g, Transform who, CharacterView view, bool isPlayer)
        {
            var pc = who.GetComponent<PlayerController>();
            var mate = isPlayer ? carryCompanion : null;
            if (isPlayer && pc != null) { pc.Hidden = true; if (mate == null) { pc.RecordTrail(false, true); pc.OnWireTravel?.Invoke(this); } }
            Fx.Spawn("Spark", who.position + Vector3.up * 0.6f);
            Sfx.Play("sfx_absorb", 0.7f, 1.3f);
            view.SetVisible(false);
            if (mate != null) { mate.BeginCarried(); Fx.Spawn("Spark", mate.transform.position + Vector3.up * 0.6f); }
            var dot = new GameObject("LightDot");
            var dsr = dot.AddComponent<SpriteRenderer>(); dsr.sortingOrder = 40;
            var set = g.Config.GetSet("FX_LightDot");
            var fb = dot.AddComponent<Flipbook>();
            if (set != null) fb.SetFrames(set.frames, 12, true);
            var pts = new List<Vector3>();
            pts.Add(transform.position + Vector3.up * 0.5f);
            if (path != null) pts.AddRange(path);
            pts.Add(exit.position + Vector3.up * 0.5f);
            float total = 0; for (int i = 1; i < pts.Count; i++) total += Vector3.Distance(pts[i - 1], pts[i]);
            float speed = Mathf.Max(8f, total / 1.2f);
            var rig = Camera.main != null ? Camera.main.GetComponent<CameraRig>() : null;
            for (int i = 1; i < pts.Count; i++)
            {
                Vector3 a = pts[i - 1], b = pts[i];
                float len = Vector3.Distance(a, b);
                for (float d = 0; d < len; d += speed * Time.deltaTime)
                {
                    dot.transform.position = Vector3.Lerp(a, b, d / len);
                    if (isPlayer && rig != null) rig.FocusOverride = dot.transform.position;
                    yield return null;
                }
            }
            Destroy(dot);
            who.position = exit.position;
            if (pc != null) { pc.Teleport(exit.position); if (mate == null) pc.RecordTrail(false, true); }
            if (mate != null) { mate.EndCarried(); carryCompanion = null; }
            if (isPlayer && rig != null) rig.FocusOverride = null;
            Fx.Spawn("Spark", exit.position + Vector3.up * 0.6f);
            view.SetVisible(true);
            if (isPlayer && pc != null) pc.Hidden = false;
            g.State.Set("used_powerline");
        }
    }
}
