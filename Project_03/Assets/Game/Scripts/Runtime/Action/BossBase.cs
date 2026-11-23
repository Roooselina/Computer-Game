using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 7~10장 보스 공용 (2026-11-15): 실시간 피격·경직·준비 동작 표시(바닥 띠 + 깜빡임 + 소리 + 글자)·HP 바.
    /// 모든 공격은 준비 동작이 보이고, 기물 해법은 '빈틈'을 만드는 유리한 선택일 뿐 일반 공격만으로도 끝낼 수 있다.
    /// 대화·메뉴·컷신 중에는 Hazard.Dt 가 0 이라 공격·타이머가 함께 멈춘다.
    /// </summary>
    public abstract class BossBase : MonoBehaviour, IDamageable, IBossBar
    {
        public string bossName = "보스";
        public int maxHp = 40;
        public float arenaMin = 2f, arenaMax = 30f;
        public CharacterView View;
        public float bodyHeight = 1.4f;
        /// <summary>맞는 몸 너비 (큰 몸집은 넓게 — 근접 공격이 닿아야 한다).</summary>
        public float BodyWidth => Mathf.Max(0.8f, bodyHeight * 0.6f);

        protected int hp;
        protected int facing = -1;
        protected float stun;
        public bool Active { get; protected set; }
        public bool Defeated { get; protected set; }
        public int Hp => hp;
        public float StunLeft => stun;
        public System.Action OnDefeated;
        public int HitsTaken { get; protected set; }
        public float LastHitTime { get; protected set; } = -99f;

        protected Game g => Game.I;
        protected PlayerController P => g != null ? g.Player : null;
        protected float Dt => Hazard.Dt;
        protected LuBubble bubble;
        readonly List<GameObject> tele = new List<GameObject>();

        public virtual bool BarVisible => Active && !Defeated;
        public virtual string BarName => bossName;
        public virtual string BarPhase => stun > 0 ? "빈틈" : "";
        public int BarHp => hp;
        public virtual int BarMax => maxHp;
        public virtual bool Alive => Active && !Defeated && hp > 0;
        public virtual Vector2 Center => (Vector2)transform.position + new Vector2(0, bodyHeight * 0.55f);

        protected virtual void OnEnable() { Damageables.Add(this); BossBars.Add(this); }
        protected virtual void OnDisable() { Damageables.Remove(this); BossBars.Remove(this); ClearTele(); if (bubble != null) bubble.Hide(); }

        public virtual void Activate()
        {
            if (View == null) { var a = GetComponent<Actor>(); if (a != null) View = a.view; }
            hp = maxHp; Active = true; Defeated = false; stun = 0;
            var col = GetComponent<BoxCollider2D>();
            if (col == null) { col = gameObject.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = new Vector2(BodyWidth, bodyHeight); col.offset = new Vector2(0, bodyHeight / 2f); }
            else { col.size = new Vector2(BodyWidth, bodyHeight); col.offset = new Vector2(0, bodyHeight / 2f); }
            if (GetComponent<EnemyBody>() == null) gameObject.AddComponent<EnemyBody>();
            if (g != null && bubble == null) bubble = new LuBubble(g.UI.worldLayer, g.UI) { Follow = transform, FollowHeight = bodyHeight + 0.45f, LogName = bossName };
            FaceLu();
        }

        /// <summary>전투를 멈춘다 (패배·연출). 남은 위험물은 지운다.</summary>
        public virtual void Stop() { Active = false; ClearTele(); }

        public void SayBubble(string s, float sec = 0) { if (bubble != null) bubble.Say(s, sec); }

        protected virtual void Update() { if (bubble != null) bubble.Tick(); }

        // ------------------------------------------------------------ 피격
        public virtual void TakeHit(int damage, float stagger, Vector2 from, string kind)
        {
            if (!Alive) return;
            float mul = DamageMul(kind);
            int d = Mathf.Max(1, Mathf.RoundToInt(damage * mul));
            hp = Mathf.Max(0, hp - d);
            HitsTaken++; LastHitTime = Time.time;
            if (View != null) View.Flash(mul > 1.2f ? new Color(1f, 1f, 0.5f) : new Color(1f, 0.6f, 0.6f), 0.12f);
            Sfx.Play(mul > 1.2f ? "sfx_hit_elec" : "sfx_robot_hit", 0.7f, Random.Range(0.95f, 1.1f));
            OnHit(d, stagger, kind);
            if (hp <= 0) { ClearTele(); OnZero(); }
        }

        /// <summary>빈틈(경직) 중 2배. 하위 클래스가 외피·보호 등을 더한다.</summary>
        protected virtual float DamageMul(string kind) => stun > 0 ? 2f : 1f;
        protected virtual void OnHit(int dealt, float stagger, string kind) { }
        /// <summary>HP 가 0 이 되었을 때 (죽이지 않는다 — 각 인물의 장면으로 넘어간다).</summary>
        protected virtual void OnZero() { Defeated = true; Active = false; OnDefeated?.Invoke(); }

        // ------------------------------------------------------------ 이동·방향
        protected void FaceLu()
        {
            if (P == null) return;
            facing = P.transform.position.x > transform.position.x ? 1 : -1;
            if (View != null) View.Face(facing);
        }

        protected float DxToLu => P != null ? P.transform.position.x - transform.position.x : 0f;
        protected bool LuNear(float range, float dy = 1.4f) => P != null && Mathf.Abs(DxToLu) < range && Mathf.Abs(P.transform.position.y - transform.position.y) < dy;

        protected void StepX(float vx, float dt)
        {
            var p = transform.position;
            p.x = Mathf.Clamp(p.x + vx * dt, arenaMin, arenaMax);
            transform.position = p;
        }

        protected void Anim(string s) { if (View != null && View.HasState(s)) View.Play(s); else if (View != null && s != "Idle" && View.HasState("Idle")) View.Play("Idle"); }

        protected void Blink(Color c, float hz)
        {
            if (View == null || View.sr == null) return;
            View.sr.color = (Time.time * hz % 1f) < 0.5f ? c : Color.white;
        }
        protected void White() { if (View != null && View.sr != null) View.sr.color = Color.white; }

        // ------------------------------------------------------------ 준비 동작 표시
        /// <summary>바닥 위험 띠 (x0~x1). 색 + 위쪽 경계선 + 작은 글자 — 소리 없이도 보인다.</summary>
        public GameObject FloorTele(float x0, float x1, float y, Color c, string label = null, float height = 0.18f)
        {
            if (g == null || g.Config == null || g.Config.pixel == null) return null;
            var go = new GameObject("Telegraph");
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = g.Config.pixel; sr.sortingOrder = 30; sr.color = c;
            float s = g.Config.pixel.bounds.size.x;
            float a = Mathf.Min(x0, x1), b = Mathf.Max(x0, x1);
            go.transform.position = new Vector3((a + b) / 2f, y + height / 2f, 0);
            go.transform.localScale = new Vector3((b - a) / s, height / s, 1);
            var edge = new GameObject("Edge"); edge.transform.SetParent(go.transform, false);
            var esr = edge.AddComponent<SpriteRenderer>(); esr.sprite = g.Config.pixel; esr.sortingOrder = 31; esr.color = new Color(c.r, c.g, c.b, 1f);
            edge.transform.localPosition = new Vector3(0, 0.5f, 0); edge.transform.localScale = new Vector3(1, 0.25f, 1);
            if (!string.IsNullOrEmpty(label) && g.UI != null) g.UI.Toast(label, 1.0f);
            tele.Add(go);
            return go;
        }

        /// <summary>세로 기둥 (낙하·공명 기둥).</summary>
        public GameObject ColumnTele(float x, float y0, float h, float w, Color c)
        {
            var go = FloorTele(x - w / 2f, x + w / 2f, y0, c, null, h);
            return go;
        }

        public void ClearTele()
        {
            foreach (var t in tele) if (t != null) Destroy(t);
            tele.Clear();
            White();
        }

        protected static readonly Color TeleRed = new Color(1f, 0.25f, 0.2f, 0.35f);
        protected static readonly Color TeleYellow = new Color(1f, 0.85f, 0.3f, 0.32f);
        protected static readonly Color TeleBlue = new Color(0.4f, 0.75f, 1f, 0.3f);
    }

    /// <summary>적의 투사체 (호위 장치·라크리모사 음파 조각). 대시 무적으로 피할 수 있다.</summary>
    public class EnemyShot : MonoBehaviour
    {
        Vector2 vel; float life; int damage; string kind;
        public static EnemyShot Fire(Vector2 from, Vector2 vel, int damage, string kind = "electric", float life = 3f, Color? tint = null)
        {
            var go = new GameObject("EnemyShot");
            go.transform.position = from;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sortingOrder = 32; sr.color = tint ?? new Color(1f, 0.75f, 0.4f);
            var g = Game.I;
            var set = g != null ? g.Config.GetSet("FX_LightDot") : null;
            if (set != null) { var fb = go.AddComponent<Flipbook>(); fb.SetFrames(set.frames, 14, true); }
            go.transform.localScale = Vector3.one * 1.3f;
            var s = go.AddComponent<EnemyShot>(); s.vel = vel; s.life = life; s.damage = damage; s.kind = kind;
            return s;
        }

        void Update()
        {
            float dt = Hazard.Dt;
            if (dt <= 0) return;
            life -= dt;
            Vector2 pos = transform.position, next = pos + vel * dt;
            if (!Sight.Clear(pos, next) || life <= 0) { Fx.Spawn("Spark", next, false, 31, 0.35f); Destroy(gameObject); return; }
            transform.position = next;
            var p = Game.I != null ? Game.I.Player : null;
            if (p != null && Vector2.Distance(p.Chest, next) < 0.45f)
            {
                if (ArenaGuard.TryBlock(next)) { Destroy(gameObject); return; }
                p.Hurt(damage, pos, kind);
                Destroy(gameObject);
            }
        }

        public static void ClearAll() { foreach (var s in Object.FindObjectsByType<EnemyShot>()) Destroy(s.gameObject); }
    }

    /// <summary>동료의 엄호 (최종전 라비엘): 다음 한 번의 피해를 막는 막.</summary>
    public static class ArenaGuard
    {
        public static float ShieldUntil = -1f;
        public static int Blocks;
        public static bool Active => Time.time < ShieldUntil;
        public static bool TryBlock(Vector2 at)
        {
            if (!Active) return false;
            ShieldUntil = -1f; Blocks++;
            Fx.Spawn("Spark", at, false, 31, 0.8f);
            Sfx.Play("sfx_absorb", 0.8f);
            return true;
        }
        public static void Reset() { ShieldUntil = -1f; Blocks = 0; }
    }

}
