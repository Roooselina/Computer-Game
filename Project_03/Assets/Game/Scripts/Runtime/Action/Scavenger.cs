using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 폐기소의 겁먹은 생존자 (4장 교섭 실패 시의 짧은 비살상 충돌). 실시간 필드에서 그대로 싸운다 — 별도 전투 화면·턴 없음.
    /// - 경계(파이프를 든 채 거리를 둠) → 다가옴 → 크게 휘두르는 예고(0.8초) → 한 번 휘두름 → 숨 고르기.
    ///   멀리 있으면 고철을 포물선으로 던진다 (그림자로 떨어질 곳 표시).
    /// - 죽지 않는다. 루의 전기(톡!·샷·펄스)에 맞을 때마다 '겁'이 줄고, 0 이 되면 파이프를 놓고 주저앉는다(진정).
    ///   진정한 사람은 다시 일어나 싸우지 않는다. 쓰러뜨림·사망 그림을 쓰지 않는다.
    /// - 활성 조건: requireFlag 가 있고 calmFlag 가 없을 때. 모두 진정하면 StoryEvents 가 대화로 잇는다.
    /// - 대화·컷신 중에는 Hazard.Dt 가 0 이라 멈춘다.
    /// </summary>
    public class Scavenger : MonoBehaviour, IDamageable
    {
        public static readonly List<Scavenger> All = new List<Scavenger>();
        public enum SState { Idle, Watch, Approach, Windup, Strike, Recover, Calm }

        public string id;
        public string artId = "survivorA";
        public string requireFlag = "waste_fight", calmFlagPrefix = "waste_calm_", relaxFlag = "waste_resolved";
        public int nerve = 3;
        public float walk = 1.6f, keepAway = 2.2f, reach = 1.0f, windup = 0.8f, recover = 0.9f;
        public int damage = 6;
        public float tossEvery = 3.2f;
        public float minX = -999, maxX = 999;
        public CharacterView View;
        public Sprite junkSprite, shadowSprite;

        public SState State { get; private set; } = SState.Idle;
        int left; float timer, tossT;
        int facing = -1;
        bool present;

        void OnEnable() { if (!All.Contains(this)) All.Add(this); Damageables.Add(this); }
        void OnDisable() { All.Remove(this); Damageables.Remove(this); }

        public bool Calmed => Game.I != null && Game.I.State != null && Game.I.State.Has(calmFlagPrefix + id);
        public bool Active => present && State != SState.Calm && State != SState.Idle;
        public bool Alive => Active;
        public Vector2 Center => (Vector2)transform.position + new Vector2(0, 0.7f);

        void Start()
        {
            left = nerve;
            var g = Game.I;
            if (View == null && g != null) { var art = g.Config.GetArt(artId); if (art != null) View = CharacterView.Create(transform, art, 9); }
            var bc = GetComponent<BoxCollider2D>(); if (bc == null) bc = gameObject.AddComponent<BoxCollider2D>();
            bc.isTrigger = true; bc.size = new Vector2(0.7f, 1.3f); bc.offset = new Vector2(0, 0.65f);
            tossT = tossEvery * 0.6f;
        }

        void Update()
        {
            var g = Game.I;
            if (g == null || g.State == null) return;
            if (Calmed) { if (State != SState.Calm) { State = SState.Calm; } Play("Calm"); return; }
            bool want = string.IsNullOrEmpty(requireFlag) || g.State.Has(requireFlag);
            if (!want || g.State.Has(relaxFlag)) { present = false; State = SState.Idle; Play(g.State.Has(relaxFlag) ? "Idle" : "Guard"); return; }
            present = true;
            if (State == SState.Idle) State = SState.Watch;
            float dt = Hazard.Dt; if (dt <= 0) return;
            var p = g.Player; if (p == null) return;
            float dx = p.transform.position.x - transform.position.x;
            float dist = Mathf.Abs(dx);
            switch (State)
            {
                case SState.Watch:
                    Face(dx); Play("Guard");
                    tossT -= dt;
                    if (dist > keepAway + 1.2f && tossT <= 0) { tossT = tossEvery; StartCoroutine(Toss(p)); }
                    else if (dist > reach * 0.9f) Move(dx, dt);
                    if (dist <= reach) { State = SState.Windup; timer = windup * ActionTuning.CurrentDifficulty.windupMul; Sfx.Play("sfx_baton_swing", 0.5f, 0.7f); }
                    break;
                case SState.Windup:
                    Play("Windup"); timer -= dt;
                    if (View != null && View.sr != null) View.sr.color = (Time.time * 10f % 1f) < 0.5f ? new Color(1f, 0.85f, 0.7f) : Color.white;
                    if (timer <= 0)
                    {
                        if (View != null && View.sr != null) View.sr.color = Color.white;
                        State = SState.Strike; timer = 0.18f;
                        Sfx.Play("sfx_baton_hit", 0.6f, 0.9f);
                        if (Mathf.Abs(p.transform.position.x - transform.position.x) < reach + 0.35f && Mathf.Abs(p.transform.position.y - transform.position.y) < 1.2f && Mathf.Sign(p.transform.position.x - transform.position.x) == facing)
                            p.Hurt(damage, transform.position, "physical");
                    }
                    break;
                case SState.Strike:
                    Play("Strike"); timer -= dt;
                    if (timer <= 0) { State = SState.Recover; timer = recover; }
                    break;
                case SState.Recover:
                    Play("Guard"); timer -= dt;
                    // 한 대 휘두른 뒤 조금 물러선다
                    Move(-dx, dt, 0.6f);
                    if (timer <= 0) State = SState.Watch;
                    break;
            }
        }

        void Move(float dir, float dt, float mul = 1f)
        {
            if (Mathf.Abs(dir) < 0.01f) return;
            var p = transform.position;
            p.x = Mathf.Clamp(p.x + Mathf.Sign(dir) * walk * mul * dt, minX, maxX);
            transform.position = p;
            if (mul >= 1f) Face(dir);
            Play("Walk");
        }

        void Face(float dx) { if (Mathf.Abs(dx) > 0.05f) facing = dx > 0 ? 1 : -1; if (View != null) View.Face(facing); }
        void Play(string s) { if (View != null && View.Current != s && View.HasState(s)) View.Play(s); }

        /// <summary>고철 던지기: 떨어질 곳에 그림자 → 0.9초 뒤 착지. 착지 지점 근처에 있으면 작은 피해.</summary>
        IEnumerator Toss(PlayerController p)
        {
            Play("Toss");
            Vector2 from = Center + new Vector2(facing * 0.3f, 0.5f);
            Vector2 to = new Vector2(p.transform.position.x, p.transform.position.y);
            var shadow = new GameObject("JunkShadow");
            var ssr = shadow.AddComponent<SpriteRenderer>();
            ssr.sprite = shadowSprite;
            shadow.transform.position = to + Vector2.up * 0.05f; shadow.transform.localScale = new Vector3(1.6f, 0.5f, 1);
            ssr.color = new Color(0, 0, 0, 0.45f); ssr.sortingOrder = 5;
            var junk = new GameObject("Junk");
            var jsr = junk.AddComponent<SpriteRenderer>();
            jsr.sprite = junkSprite;
            jsr.sortingOrder = 15; jsr.color = new Color(0.7f, 0.7f, 0.75f);
            float T = 0.9f;
            for (float t = 0; t < T; t += Hazard.Dt)
            {
                if (junk == null) yield break;
                float k = t / T;
                junk.transform.position = Vector2.Lerp(from, to, k) + Vector2.up * Mathf.Sin(k * Mathf.PI) * 2.2f;
                junk.transform.Rotate(0, 0, 600f * Hazard.Dt);
                yield return null;
            }
            Sfx.Play("sfx_clatter", 0.7f);
            if (p != null && Vector2.Distance(p.transform.position, to) < 0.75f) p.Hurt(Mathf.Max(1, damage / 2), to + Vector2.up, "physical");
            Destroy(junk); Destroy(shadow);
        }

        public void TakeHit(int dmg, float stagger, Vector2 from, string kind)
        {
            if (!Active) return;
            left -= Mathf.Max(1, dmg);
            if (View != null) View.Flash(new Color(1f, 1f, 0.6f), 0.15f);
            Sfx.Play("sfx_hit_elec", 0.6f, 1.2f);
            StopAllCoroutines();
            if (left <= 0) { Calm(); return; }
            // 찌릿 — 놀라서 물러난다 (경직)
            State = SState.Recover; timer = Mathf.Max(0.5f, stagger + 0.3f);
            var p = transform.position; p.x = Mathf.Clamp(p.x + Mathf.Sign(p.x - from.x) * 0.5f, minX, maxX); transform.position = p;
            Play("Hurt");
        }

        public void Calm()
        {
            State = SState.Calm;
            var g = Game.I;
            if (g != null && g.State != null) g.State.Set(calmFlagPrefix + id);
            Sfx.Play("sfx_clatter", 0.6f, 1.3f);
            StoryEvents.Onoma(g, Center + Vector2.up * 1.1f, "덜그럭...", new Color(0.85f, 0.85f, 0.9f));
            Play("Calm");
        }
    }
}
