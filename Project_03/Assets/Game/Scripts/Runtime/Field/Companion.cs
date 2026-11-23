using System.Collections;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 동료 추종 (GDD 4.1, 10.2). 루가 지나간 발자취를 순서대로 따라간다.
    /// 그래서 사다리·좁은 통로를 같은 경로로 통과하고, 전원선은 같은 선을 타고 따라온다.
    /// 너무 멀어지거나 움직이지 못하는 상태가 이어지면 루 뒤의 안전한 위치에서 합류한다.
    /// </summary>
    public class Companion : MonoBehaviour
    {
        public string actorId = "venelle";
        public float followDistance = 1.15f;
        public CharacterView View;
        public Actor Actor;
        Game game;
        int cursorAbs;         // 다음에 향할 발자취 절대 인덱스
        float stuckTimer;
        bool wiring;
        PowerLine pendingLine;
        Vector2 lastPos;
        static ContactFilter2D solid;

        public static Companion Spawn(Game g, string id, Vector2 pos, int facing)
        {
            var go = new GameObject("Companion_" + id);
            go.transform.position = pos;
            var c = go.AddComponent<Companion>();
            c.game = g; c.actorId = id;
            c.Actor = go.AddComponent<Actor>(); c.Actor.actorId = id;
            c.View = CharacterView.Create(go.transform, g.Config.GetArt(id), 11);
            c.Actor.view = c.View;
            c.View.Face(facing);
            g.Player.OnWireTravel = c.OnPlayerWire;
            c.ResetCursor();
            c.lastPos = pos;
            solid = new ContactFilter2D { useTriggers = false };
            solid.SetLayerMask(Physics2D.AllLayers);
            return c;
        }

        void ResetCursor()
        {
            var p = game.Player;
            cursorAbs = p.TrailBase + Mathf.Max(0, p.Trail.Count - 1);
        }

        void OnPlayerWire(PowerLine line) { pendingLine = line; }

        void Update()
        {
            if (game == null) game = Game.I;
            var p = game != null ? game.Player : null;
            if (p == null || wiring) return;
            var trail = p.Trail;
            if (trail.Count == 0) { View.Play("Idle"); return; }

            int local = Mathf.Clamp(cursorAbs - p.TrailBase, 0, trail.Count - 1);
            Vector2 pos = transform.position;

            // 남은 경로 길이 (전선 구간 포함: 전선 너머에 루가 있으면 계속 따라간다)
            float remain = Vector2.Distance(pos, trail[local].pos);
            for (int i = local; i < trail.Count - 1; i++) remain += Vector2.Distance(trail[i].pos, trail[i + 1].pos);

            Vector2 start = pos;
            bool climbing = false;
            if (remain > followDistance + 0.05f)
            {
                var pt = ActionTuning.I.player; float speed = p.Carrying ? pt.carry : (remain > 3f ? pt.walk + 2.5f : pt.walk + 0.3f);
                float step = speed * Time.deltaTime;
                while (step > 0 && local < trail.Count && remain > followDistance)
                {
                    var s = trail[local];
                    bool entrance = s.wire && local + 1 < trail.Count && trail[local + 1].wire;
                    bool orphanExit = s.wire && !entrance && (local == 0 || !trail[local - 1].wire);
                    // 전원선 입구 표시 + 다음 칸이 출구 표시면 같은 선을 탄다
                    if (entrance && Vector2.Distance(pos, s.pos) < 0.05f)
                    {
                        int exitAbs = p.TrailBase + local + 1;
                        StartCoroutine(Wire(pendingLine, trail[local + 1].pos, exitAbs));
                        pendingLine = null;
                        return;
                    }
                    // 입구 표시가 사라진 출구(합류 직후 등): 걸어서 허공을 건너지 않고 전선으로 이동
                    if (orphanExit)
                    {
                        StartCoroutine(Wire(pendingLine, s.pos, p.TrailBase + local));
                        pendingLine = null;
                        return;
                    }
                    float d = Vector2.Distance(pos, s.pos);
                    climbing |= s.climbing;
                    if (d <= step) { pos = s.pos; step -= d; remain -= d; local++; }
                    else { pos = Vector2.MoveTowards(pos, s.pos, step); remain -= step; step = 0; }
                }
                cursorAbs = p.TrailBase + Mathf.Min(local, trail.Count - 1);
                transform.position = pos;
            }

            float dx = pos.x - start.x;
            if (Mathf.Abs(dx) > 0.002f) View.Face(dx > 0 ? 1 : -1);
            else if (!climbing) View.Face(p.transform.position.x > transform.position.x ? 1 : -1);
            bool moved = (pos - start).sqrMagnitude > 0.00001f;
            if (climbing && moved) View.Play("Climb");
            else if (moved) View.Play(game.State != null && game.State.Has("venelle_carried") ? "Supported" : "Walk");
            else View.Play("Idle");

            // 끼임·이탈 방지
            float dist = Vector2.Distance(pos, p.transform.position);
            if (p.Hidden) stuckTimer = 0;   // 루가 전선 이동 중이면 합류하지 않는다
            else if (dist > 14f || (!moved && remain > followDistance + 2.5f)) stuckTimer += Time.deltaTime; else stuckTimer = 0;
            if (stuckTimer > 2.0f) Regroup();
            lastPos = pos;
        }

        IEnumerator Wire(PowerLine line, Vector2 exitPos, int exitAbs)
        {
            wiring = true;
            if (line != null) yield return line.Travel(game, transform, View, false);
            else { View.SetVisible(false); yield return new WaitForSeconds(0.4f); transform.position = exitPos; View.SetVisible(true); }
            transform.position = exitPos;
            cursorAbs = exitAbs + 1;
            wiring = false;
        }

        /// <summary>루가 손을 잡고 함께 전원선을 타는 동안 (모습 숨김, 따라가기 정지).</summary>
        public void BeginCarried() { wiring = true; pendingLine = null; View.SetVisible(false); }
        /// <summary>전원선 출구에서 루 바로 뒤에 함께 나온다 (반대편에 남거나 떨어지지 않게).</summary>
        public void EndCarried()
        {
            View.SetVisible(true);
            PlaceBehindPlayer();
            Fx.Spawn("Spark", transform.position + Vector3.up * 0.6f, false, 30, 0.6f);
            wiring = false;
        }

        /// <summary>루 쪽으로 짧게 끌려오기 (손 잡기). 거리가 멀면 바로 옆으로.</summary>
        public IEnumerator PullTo(Vector2 at, float time)
        {
            Vector2 from = transform.position;
            View.Play("Walk"); View.Face(at.x > from.x ? 1 : -1);
            for (float t = 0; t < time; t += Time.deltaTime) { transform.position = Vector2.Lerp(from, at, t / time); yield return null; }
            transform.position = at; View.Play("Idle");
        }

        /// <summary>루 뒤 안전한 바닥으로 순간 합류.</summary>
        public void Regroup()
        {
            stuckTimer = 0;
            var p = game.Player;
            if (p == null) return;
            Vector2 at = (Vector2)p.transform.position + Vector2.left * p.Facing * 0.9f;
            var buf = new Collider2D[4];
            if (Physics2D.OverlapBox(at + Vector2.up * 0.7f, new Vector2(0.5f, 1.2f), 0, solid, buf) > 0) at = p.transform.position;
            transform.position = at;
            Fx.Spawn("Spark", (Vector3)at + Vector3.up * 0.6f, false, 30, 0.6f);
            p.ClearTrail(); p.RecordTrail(false, false);
            ResetCursor();
            pendingLine = null;
        }

        public void PlaceBehindPlayer()
        {
            var p = game.Player;
            transform.position = (Vector2)p.transform.position + Vector2.left * p.Facing * followDistance;
            p.ClearTrail(); p.RecordTrail(false, false); ResetCursor();
        }
    }
}
