using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 베넬 (화학 개조) 복도 전투 — 2026-11-15 개편.
    /// 한 번에 한 패턴만 쓴다. 패턴마다 준비 동작·소리·사거리·회피 방법이 다르다 (Docs/STEALTH_COMBAT_SPEC.md §베넬).
    ///  1 돌진→휘두르기 : 몸을 낮추고 초록빛 깜빡임(1.1초, 첫 돌진 1.7초) → 직선 돌진 → 끝에서 짧은 휘두르기. 회피(Shift)로 통과하거나 점프.
    ///  2 산성 부채 분사 : 팔 장치를 앞으로 내밀고 바닥에 사거리 줄이 깔림(0.7초) → 낮게 깔리는 부채꼴. 점프로 넘거나 거리를 벌린다.
    ///  3 약병 투척     : 팔을 뒤로 젖히고 바닥에 착지 표식(0.85초) → 곡선으로 날아와 터짐 → 짧게 남는 웅덩이.
    ///  4 독성 웅덩이   : 투척 자리에 4초 남음. 동시에 최대 2개, 폭 1.6 — 복도 전체를 덮지 않는다.
    ///  5 재혼합(빈틈)  : 세 번 공격한 뒤 팔 장치를 다시 섞으며 2.6초 멈춤. 이때 맞으면 피해 2배, 말을 걸 수도 있다.
    ///  6 잡기→던지기   : 팔을 높이 들고 '!'(0.75초) → 짧게 파고들며 잡는다. 잡히면 목덜미를 쥐고 들어 올린 채 대화(피해·독·타이머 정지),
    ///                    대화가 끝나면 앞으로 집어 던진다 (포물선 → 착지 → 일어남 → 1.3초 보호). 잡기 피해는 착지 때 한 번만.
    /// 체력이 0 이 되면 죽지 않고 쓰러진 상태(Downed)가 된다: 남은 약병·웅덩이를 모두 지우고, 루가 다가가 J 로 상호작용해야 다음 장면.
    /// 협상 반응 축(위협·신뢰·흥분)은 VenelleMind 가 들고, 이 클래스는 공격의 중단·재개·속도로 결과를 보여 준다.
    /// </summary>
    public class VenelleBoss : MonoBehaviour, IDamageable
    {
        public enum VState { Inactive, PressureWindup, PressureBurst, Stalk, DashWindup, Dash, Swipe, SprayWindup, Spray, TossWindup, GrabWindup, GrabLunge, Holding, Thrown, Remix, Recover, Backoff, Downed, Stopped, Done }
        public VState State { get; private set; } = VState.Inactive;
        public CharacterView View;
        public Transform junction;
        public float corridorMin, corridorMax;

        bool sprayRetargeted;
        float pressureCd=12f, safeX; GameObject safeMarker;
        public float SafeZoneX => safeX;
        public bool Enraged => hp > 0 && hp <= T.hp * 0.45f;
        int hp; float timer, gap; int facing = -1; int dashDir;
        float dashLeft; int attacksSinceRemix; float grabCd; Vector2 tossTarget;
        public bool TalkAvailable => State == VState.Recover || State == VState.Remix;
        public bool Downed => State == VState.Downed;
        public bool Kneeling => State == VState.Remix;
        public int Hp => hp;
        public bool Alive => State != VState.Inactive && State != VState.Done && State != VState.Downed && State != VState.Stopped && hp > 0;
        public Vector2 Center => (Vector2)transform.position + new Vector2(0, 0.7f);
        public float WindupLeft => State == VState.DashWindup || State == VState.SprayWindup || State == VState.TossWindup || State == VState.GrabWindup ? timer : 0;
        public int Facing => facing;
        public bool HitDuringKneel;
        public int MumblesShown;
        public float LastHitByLuTime = -99;
        public System.Action OnSubdued;
        LuBubble bubble;
        readonly List<GameObject> puddles = new List<GameObject>();
        readonly Dictionary<GameObject, float> puddleLife = new Dictionary<GameObject, float>();
        readonly List<GameObject> projectiles = new List<GameObject>();
        GameObject telegraph;
        ActionTuning.BossT T => ActionTuning.I.venelle;
        Game g => Game.I;
        float Agit => VenelleMind.Agitation;

        /// <summary>잡기 손 위치 (발 기준, 오른쪽을 볼 때). Venelle_Grab 그림의 손 (+13px, 46px) 과 같다.</summary>
        public static readonly Vector2 GrabHand = new Vector2(0.41f, 1.44f);
        /// <summary>루의 목덜미 (발 기준 높이). Lu_Lifted 그림의 목 (y 33px → 64-33).</summary>
        public const float LuNeck = 0.97f;

        static readonly string[] mumbles =
        {
            "약... 섞어야 하는데... 몇 개였더라...",
            "데려다줘야... 하는데... 누구를...?",
            "명령... 명령은 싫은데에...",
            "딸꾹— 아, 또 딸꾹질...",
        };

        void OnEnable() { Damageables.Add(this); }
        void OnDisable() { Damageables.Remove(this); }

        public void Activate()
        {
            if (View == null) { var a = GetComponent<Actor>(); if (a != null) View = a.view; }
            if (GetComponent<VenelleTalk>() == null) gameObject.AddComponent<VenelleTalk>().boss = this;
            hp = T.hp; attacksSinceRemix = 0; grabCd = 4f;
            var col = GetComponent<BoxCollider2D>();
            if (col == null) { col = gameObject.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = new Vector2(0.7f, 1.3f); col.offset = new Vector2(0, 0.65f); }
            if (GetComponent<EnemyBody>() == null) gameObject.AddComponent<EnemyBody>();
            EnsureBubble();
            FaceLu();
            BeginDash(true);
        }

        void EnsureBubble()
        {
            if (g != null && bubble == null) { bubble = new LuBubble(g.UI.worldLayer, g.UI) { Follow = transform, FollowHeight = 1.85f, LogName = "베넬" }; bubble.Tint(new Color(0.88f, 1f, 0.86f, 0.94f)); }
        }

        /// <summary>협상으로 공격 중단 / 결말 처리 뒤.</summary>
        public void Deactivate() { State = VState.Done; ClearHazards(); if (bubble != null) bubble.Hide(); }
        public void Stop() { State = VState.Stopped; ClearHazards(); Anim("Idle"); if (View != null && View.sr != null) View.sr.color = Color.white; }

        void FaceLu()
        {
            if (g == null || g.Player == null) return;
            facing = g.Player.transform.position.x > transform.position.x ? 1 : -1;
            if (View != null) View.Face(facing);
        }

        float SprayRange => T.sprayRange * (Enraged ? 1.2f : 1f);
        float Wm => ActionTuning.CurrentDifficulty.windupMul * (Agit > 5 ? 0.85f : 1f);
        float Sp => Agit > 5 ? 1.2f : 1f;

        void Update()
        {
            if (bubble != null) bubble.Tick();
            if (State == VState.Inactive || State == VState.Done || State == VState.Stopped || g == null || g.Player == null) return;
            float dt = Hazard.Dt;
            if (State == VState.Holding) { HoldLu(g.Player); return; }   // 대화 중에도 손 위치에 붙여 둔다
            UpdatePuddles(dt, g.Player);
            if (dt <= 0) return;
            var p = g.Player;
            pressureCd -= dt;
            if (Enraged && Random.value < dt*4) ReactionParticles.Emit(Center,facing,true,3,0.3f);
            if (grabCd > 0) grabCd -= dt;
            VenelleMind.Tick(dt, this);
            float dx = p.transform.position.x - transform.position.x;
            switch (State)
            {
                case VState.PressureWindup:
                    timer -= dt; Anim("Charge");
                    Blink(new Color(0.92f,0.72f,0.3f),5);
                    if(timer<=0)
                    {
                        White();State=VState.PressureBurst;timer=0.8f;
                        Sfx.Play("sfx_acid",1f,0.6f);
                        for(float x=corridorMin;x<=corridorMax;x+=1.5f)
                            if(Mathf.Abs(x-safeX)>1.7f) ReactionParticles.Emit(new Vector2(x,transform.position.y+0.1f),x<safeX?-1:1,true,20,0.3f);
                    }
                    break;
                case VState.PressureBurst:
                    timer -= dt;
                    if(Mathf.Abs(p.transform.position.x-safeX)>1.7f)p.Hurt(26,Center,"acid");
                    if(timer<=0){ClearTelegraph();if(safeMarker!=null)Destroy(safeMarker);pressureCd=18f;EnterRecover(2.6f);}
                    break;
                case VState.Stalk:
                    FaceLu();
                    gap -= dt;
                    float wob = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 2.3f));
                    if (Mathf.Abs(dx) > 1.8f) Step(Mathf.Sign(dx) * T.stalk * wob * Sp, dt);
                    Anim(View != null && View.HasState("Stalk") ? "Stalk" : "Walk");
                    if (gap <= 0) ChooseAttack(Mathf.Abs(dx));
                    break;
                case VState.DashWindup:
                    timer -= dt;
                    Anim(View != null && View.HasState("Windup") ? "Windup" : "Down");
                    Blink(new Color(0.7f, 1f, 0.6f), 10f);
                    if (timer <= 0) { White(); State = VState.Dash; dashLeft = T.dashDistance; dashDir = facing; Sfx.Play("sfx_dash", 0.9f); }
                    break;
                case VState.Dash:
                    {
                        float step = T.dashSpeed * Sp * dt;
                        float before = transform.position.x;
                        Step(dashDir * T.dashSpeed * Sp, dt);
                        dashLeft -= Mathf.Abs(transform.position.x - before);
                        Anim(View != null && View.HasState("Dash") ? "Dash" : "Jump");
                        if (Random.value < 0.4f) Fx.Spawn("ToxicMist", (Vector2)transform.position + new Vector2(-dashDir * 0.5f, 0.4f), false, 8, 0.6f);
                        if (Mathf.Abs(dx) < 0.6f && Mathf.Abs(p.transform.position.y - transform.position.y) < 1.2f)
                            p.Hurt(T.dashDamage, (Vector2)transform.position - new Vector2(dashDir, 0), "physical");
                        bool blocked = Mathf.Abs(transform.position.x - before) < step * 0.3f;
                        if (dashLeft <= 0 || blocked)
                        {
                            Sfx.Play("sfx_land", 0.6f, 1.1f);
                            // 끝에서 짧은 휘두르기 (가까울 때만)
                            if (Mathf.Abs(dx) < T.swipeRange + 0.8f) { State = VState.Swipe; timer = T.swipeWindup * Wm; FaceLu(); Anim("Attack"); Sfx.Play("sfx_acid", 0.5f, 1.4f); }
                            else EnterRecover();
                        }
                    }
                    break;
                case VState.Swipe:
                    timer -= dt;
                    Blink(new Color(1f, 1f, 0.7f), 14f);
                    if (timer <= 0)
                    {
                        White();
                        Sfx.Play("sfx_baton_hit", 0.7f, 1.3f);
                        Fx.Spawn("AcidSplash", (Vector2)transform.position + new Vector2(facing * 0.9f, 0.7f), facing < 0, 31, 0.8f);
                        var box = new Rect(new Vector2(transform.position.x + (facing > 0 ? 0 : -T.swipeRange), transform.position.y), new Vector2(T.swipeRange, 1.4f));
                        if (box.Contains(p.Chest)) p.Hurt(T.swipeDamage, transform.position, "acid");
                        EnterRecover();
                    }
                    break;
                case VState.SprayWindup:
                    timer -= dt;
                    if(Enraged&&!sprayRetargeted&&timer<0.8f)
                    {
                        sprayRetargeted=true;FaceLu();timer=Mathf.Max(timer,0.65f);
                        Telegraph(transform.position.x+facing*0.5f,transform.position.x+facing*(0.5f+SprayRange),new Color(0.8f,0.85f,0.3f,0.7f));
                    }
                    Anim(View != null && View.HasState("Spray") ? "Spray" : "Attack");
                    if (timer <= 0)
                    {
                        ClearTelegraph();
                        State = VState.Spray; timer = 0.45f;
                        Sfx.Play("sfx_acid", 0.9f, 0.8f);
                        ReactionParticles.Emit(Center+Vector2.right*facing*0.4f,facing,true,Enraged?54:32,Enraged?1.2f:1f);
                        var fx = Fx.Spawn("AcidFan", (Vector2)transform.position + new Vector2(facing * 0.6f, 0.45f), facing < 0, 31, 1f);
                        if (fx != null) fx.transform.localScale = new Vector3(SprayRange / 2f, 1, 1);
                    }
                    break;
                case VState.Spray:
                    timer -= dt;
                    {
                        // 낮게 깔리는 판정: 발목~허리 (점프로 넘을 수 있다)
                        float x0 = transform.position.x + facing * 0.5f, x1 = transform.position.x + facing * (0.5f + SprayRange);
                        float px = p.transform.position.x;
                        bool inX = facing > 0 ? px > x0 && px < x1 : px < x0 && px > x1;
                        if (inX && p.transform.position.y - transform.position.y < 0.55f) p.Hurt(T.sprayDamage, transform.position, "acid");
                    }
                    if (timer <= 0) { Attacked(); if(Enraged&&puddles.Count<T.puddleMax)BeginToss();else Back(0.7f); }
                    break;
                case VState.TossWindup:
                    timer -= dt;
                    Anim(View != null && View.HasState("Toss") ? "Toss" : "Attack");
                    if (timer <= 0) { ClearTelegraph(); StartCoroutine(Toss(tossTarget)); Attacked(); Back(0.9f); }
                    break;
                case VState.GrabWindup:
                    timer -= dt;
                    Anim(View != null && View.HasState("Grab") ? "Grab" : "Attack");
                    Blink(new Color(1f, 0.85f, 0.6f), 12f);
                    if (timer <= 0) { White(); State = VState.GrabLunge; timer = 0.28f; Sfx.Play("sfx_dash", 0.6f, 1.3f); }
                    break;
                case VState.GrabLunge:
                    timer -= dt;
                    Step(facing * T.grabLunge / 0.28f, dt);
                    if (Mathf.Abs(dx) < 0.95f && Mathf.Abs(p.transform.position.y - transform.position.y) < 1.0f && !p.Invulnerable && !p.Dodging)
                    {
                        StartGrab(p);
                        break;
                    }
                    if (timer <= 0) { grabCd = 5f; Attacked(); EnterRecover(0.9f); }
                    break;
                case VState.Holding:
                    HoldLu(p);
                    break;
                case VState.Remix:
                    timer -= dt; FaceLu();
                    Anim(View != null && View.HasState("Mix") ? "Mix" : "Dizzy");
                    if (Random.value < dt * 3f) Fx.Spawn("Bubbles", (Vector2)transform.position + new Vector2(facing * 0.4f, 1.1f), false, 32, 0.6f);
                    if (timer <= 0) { Fx.Spawn("ReactBurst", (Vector2)transform.position + new Vector2(facing * 0.4f, 1.0f), false, 32, 0.8f); Sfx.Play("sfx_acid", 0.6f, 1.6f); State = VState.Stalk; gap = T.idleGap; attacksSinceRemix = 0; }
                    break;
                case VState.Recover:
                    timer -= dt; FaceLu();
                    Anim(View != null && View.HasState("Pant") ? "Pant" : "Dizzy");
                    if (timer <= 0) { State = VState.Stalk; gap = T.idleGap; }
                    break;
                case VState.Backoff:
                    timer -= dt;
                    FaceLu();
                    if (Mathf.Abs(dx) < 3.5f) Step(-facing * T.stalk * 1.4f, dt);
                    Anim("Walk");
                    if (View != null) View.Face(facing);
                    if (timer <= 0) { State = VState.Stalk; gap = T.idleGap * 0.6f; }
                    break;
                case VState.Downed:
                    Anim(View != null && View.HasState("Downed") ? "Downed" : "Collapsed");
                    break;
            }
        }

        void Blink(Color c, float hz) { if (View != null && View.sr != null) View.sr.color = (Time.time * hz % 1f) < 0.5f ? c : Color.white; }
        void White() { if (View != null && View.sr != null) View.sr.color = Color.white; }

        void ChooseAttack(float dist)
        {
            FaceLu();
            if (Enraged && pressureCd <= 0 && g.Player.InvulnLeft <= 0) { BeginPressure(); return; }
            if (attacksSinceRemix >= 3) { BeginRemix(); return; }
            if (dist < 1.9f && grabCd <= 0) { BeginGrab(); return; }
            if (dist < 1.9f) { State = VState.Swipe; timer = T.swipeWindup * Wm; Anim("Attack"); Sfx.Play("sfx_acid", 0.5f, 1.4f); attacksSinceRemix++; return; }
            if (dist < SprayRange + 0.3f) { if (grabCd <= 0 && Random.value < 0.35f) BeginGrab(); else BeginSpray(); return; }
            if (Random.value < 0.5f && puddles.Count < T.puddleMax) BeginToss(); else BeginDash(false);
        }

        void Attacked() { attacksSinceRemix++; }

        void EnterRecover(float t = -1)
        {
            State = VState.Recover; timer = t > 0 ? t : T.recoverTime * (Agit > 5 ? 0.7f : 1f);
            Attacked();
            if (Random.value < 0.4f) Say(mumbles[MumblesShown++ % mumbles.Length], 2.6f);
        }

        void Back(float t) { State = VState.Backoff; timer = t; }

        public void BeginDash(bool first)
        {
            State = VState.DashWindup;
            timer = (first ? T.firstDashWindup : T.dashWindup) * Wm;
            FaceLu();
            Sfx.Play("sfx_charge_up", 0.5f, 0.7f);
            if (!first && bubble != null && Random.value < 0.4f) bubble.Say(Random.value < 0.5f ? "흐흥~" : "잡았... 다아~", 1.2f);
        }

        void BeginPressure()
        {
            ClearHazards();
            State=VState.PressureWindup; timer=2.8f;
            safeX=Mathf.Clamp(g.Player.transform.position.x+(g.Player.transform.position.x<Center.x?-4f:4f),corridorMin+2f,corridorMax-2f);
            Telegraph(corridorMin,corridorMax,new Color(0.7f,0.35f,0.14f,0.38f));
            safeMarker=new GameObject("PressureSafeZone");
            var sr=safeMarker.AddComponent<SpriteRenderer>();sr.sprite=g.Config.pixel;sr.sortingOrder=8;sr.color=new Color(0.45f,0.9f,0.9f,0.85f);
            float px=g.Config.pixel.bounds.size.x;
            safeMarker.transform.position=new Vector3(safeX,transform.position.y+0.08f,0);
            safeMarker.transform.localScale=new Vector3(3.4f/px,0.16f/px,1);
            Say("압력이... 멈추질 않아!",2.6f);
            g.UI.Toast("압력 폭주 — 청록색 배기 구역으로!",2.8f);
            Sfx.Play("sfx_charge_up",0.9f,0.55f);
        }

        void BeginSpray()
        {
            sprayRetargeted=false; State = VState.SprayWindup; timer = T.sprayWindup * Wm;
            Sfx.Play("sfx_bubble", 0.6f, 1.4f);
            Telegraph(transform.position.x + facing * 0.5f, transform.position.x + facing * (0.5f + SprayRange), new Color(0.6f, 1f, 0.4f, 0.55f));
        }

        void BeginToss()
        {
            var p = g.Player;
            tossTarget = new Vector2(Mathf.Clamp(p.transform.position.x + p.Velocity.x * 0.3f, corridorMin + 0.5f, corridorMax - 0.5f), transform.position.y);
            State = VState.TossWindup; timer = T.throwWindup * Wm;
            Sfx.Play("sfx_bubble", 0.5f, 0.9f);
            Telegraph(tossTarget.x - T.puddleWidth / 2f, tossTarget.x + T.puddleWidth / 2f, new Color(0.7f, 1f, 0.45f, 0.6f));
        }

        void BeginGrab()
        {
            State = VState.GrabWindup; timer = T.grabWindup * Wm;
            Sfx.Play("sfx_suspect", 0.7f, 0.8f);
            Say("이리 와~!", 1.0f);
        }

        void BeginRemix()
        {
            State = VState.Remix; timer = T.remixTime;
            Sfx.Play("sfx_bubble", 0.7f, 0.7f);
            Say(Random.value < 0.5f ? "어라... 비었네... 섞어야지..." : "잠깐— 딸꾹— 섞는 중이야아...", 2.2f);
        }

        void Telegraph(float x0, float x1, Color c)
        {
            ClearTelegraph();
            telegraph = new GameObject("VenelleTelegraph");
            var sr = telegraph.AddComponent<SpriteRenderer>(); sr.sprite = g.Config.pixel; sr.sortingOrder = 7; sr.color = c;
            float a = Mathf.Min(x0, x1), b = Mathf.Max(x0, x1);
            telegraph.transform.position = new Vector3((a + b) / 2f, transform.position.y + 0.04f, 0);
            float px = g.Config.pixel != null ? g.Config.pixel.bounds.size.x : 0.125f;
            telegraph.transform.localScale = new Vector3((b - a) / px, 0.08f / px, 1);
        }

        void ClearTelegraph() { if (telegraph != null) Destroy(telegraph); telegraph = null; }

        void Step(float vx, float dt)
        {
            var pos = transform.position;
            pos.x = Mathf.Clamp(pos.x + vx * dt, corridorMin, corridorMax);
            transform.position = pos;
            if (View != null && Mathf.Abs(vx) > 0.01f) View.Face(vx > 0 ? 1 : -1);
        }

        void Anim(string s) { if (View != null) View.Play(View.HasState(s) ? s : "Idle"); }

        // ============================================================ 약병 투척 → 반응 폭발 → 웅덩이
        IEnumerator Toss(Vector2 b)
        {
            var go = new GameObject("Flask");
            projectiles.Add(go);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sortingOrder = 30;
            var set = g.Config.GetSet("FX_Flask"); var fb = go.AddComponent<Flipbook>(); if (set != null) fb.SetFrames(set.frames, 12, true);
            Vector2 a = (Vector2)transform.position + new Vector2(facing * 0.3f, 1.6f);
            float t = 0, dur = 0.7f;
            Sfx.Play("sfx_dash", 0.4f, 1.6f);
            while (t < dur)
            {
                if (go == null) yield break;
                float dt = Hazard.Dt; t += dt;
                float k = t / dur;
                go.transform.position = Vector2.Lerp(a, b + Vector2.up * 0.15f, k) + Vector2.up * Mathf.Sin(k * Mathf.PI) * 2.4f;
                yield return null;
            }
            projectiles.Remove(go);
            if (go != null) Destroy(go);
            if (State == VState.Downed || State == VState.Done || State == VState.Stopped) yield break;
            Sfx.Play("sfx_glass_break", 0.6f, 1.5f);
            Fx.Spawn("ReactBurst", b + Vector2.up * 0.4f, false, 32, 1.2f);
            ReactionParticles.Emit(b+Vector2.up*0.2f,1,true,26,0.6f);
            Fx.Spawn("AcidSplash", b + Vector2.up * 0.3f, false, 31, 1f);
            var p = g.Player;
            if (p != null && Mathf.Abs(p.transform.position.x - b.x) < T.puddleWidth * 0.55f && p.transform.position.y - b.y < 0.9f) p.Hurt(T.acidDamage, b, "acid");
            // 웅덩이: 동시에 최대 puddleMax 개 (가장 오래된 것부터 지운다)
            while (puddles.Count >= T.puddleMax) { var old = puddles[0]; puddles.RemoveAt(0); puddleLife.Remove(old); if (old != null) Destroy(old); }
            var pd = new GameObject("AcidPuddle");
            pd.transform.position = new Vector3(b.x, transform.position.y + 0.01f, 0);
            var psr = pd.AddComponent<SpriteRenderer>(); psr.sortingOrder = 6;
            var pset = g.Config.GetSet("FX_PuddleAcid"); var pfb = pd.AddComponent<Flipbook>(); if (pset != null) pfb.SetFrames(pset.frames, 5, true);
            pd.transform.localScale = new Vector3(T.puddleWidth / 2f, 1, 1);
            puddles.Add(pd); puddleLife[pd] = T.acidPuddleTime;
        }

        float puddleTick;
        void UpdatePuddles(float dt, PlayerController p)
        {
            if (dt <= 0) return;
            puddleTick -= dt;
            foreach (var pd in puddles.ToArray())
            {
                if (pd == null) { puddles.Remove(pd); continue; }
                puddleLife[pd] -= dt;
                if (Random.value < dt * 1.5f) Fx.Spawn("Bubbles", pd.transform.position + new Vector3(Random.Range(-0.6f, 0.6f), 0.2f), false, 7, 0.4f);
                if (puddleLife[pd] <= 0) { puddles.Remove(pd); puddleLife.Remove(pd); Destroy(pd); continue; }
                if (puddleTick <= 0 && Mathf.Abs(p.transform.position.x - pd.transform.position.x) < T.puddleWidth * 0.5f && Mathf.Abs(p.transform.position.y - pd.transform.position.y) < 0.3f)
                    p.Hurt(T.acidDamage, pd.transform.position + Vector3.down, "acid");
            }
            if (puddleTick <= 0) puddleTick = 0.6f;
        }

        void ClearHazards()
        {
            foreach (var pd in puddles) if (pd != null) Destroy(pd);
            puddles.Clear(); puddleLife.Clear();
            foreach (var pr in projectiles) if (pr != null) Destroy(pr);
            projectiles.Clear();
            ClearTelegraph();
            if(safeMarker!=null)Destroy(safeMarker);
            StopAllCoroutines();
        }

        public int PuddleCount => puddles.Count;

        // ============================================================ 잡기 → 대화 → 던지기
        void StartGrab(PlayerController p)
        {
            State = VState.Holding;
            p.Skills?.CancelTargeting();
            p.ScriptControl = true; p.ScriptMoveX = 0; p.Velocity = Vector2.zero; p.Frozen = true;
            p.SetFacing(-facing);
            p.AnimOverride = p.View.HasState("Lifted") ? "Lifted" : "Fear";
            Anim(View != null && View.HasState("Grab") ? "Grab" : "Attack");
            // 잡은 손이 루의 목덜미 앞에 보이도록 베넬을 루보다 앞에 그린다 (던진 뒤 원래 순서)
            if (View != null && View.sr != null && p.View != null && p.View.sr != null) { keepOrder = View.sr.sortingOrder; View.sr.sortingOrder = p.View.sr.sortingOrder + 1; }
            Sfx.Play("sfx_baton_hit", 0.6f, 0.8f);
            HoldLu(p);
            g.State.Set("v_grabbed_once");
            g.RunEvent(StoryEvents.VenelleGrabTalk(g, this));
        }

        /// <summary>루를 손 위치에 붙인다 (손 = 목덜미). 매 프레임 맞춰 두 그림의 손·몸이 어긋나지 않게.</summary>
        void HoldLu(PlayerController p)
        {
            Vector2 hand = (Vector2)transform.position + new Vector2(GrabHand.x * facing, GrabHand.y);
            p.transform.position = new Vector3(hand.x, hand.y - LuNeck, 0);
            p.Velocity = Vector2.zero;
        }

        int keepOrder = -1;
        public Vector2 HandWorld => (Vector2)transform.position + new Vector2(GrabHand.x * facing, GrabHand.y);

        /// <summary>대화가 끝나면: 앞으로 집어 던진다. 포물선 → 착지(피해 한 번) → 일어남 → 짧은 보호 시간.</summary>
        public IEnumerator ThrowLu()
        {
            var p = g.Player;
            State = VState.Thrown;
            Anim(View != null && View.HasState("Throw") ? "Throw" : "Attack");
            Sfx.Play("sfx_dash", 0.8f, 0.8f);
            p.AnimOverride = p.View.HasState("Thrown") ? "Thrown" : "Hurt";
            p.Frozen = true;
            Vector2 from = p.transform.position;
            float land = Mathf.Clamp(from.x + facing * 4.2f, corridorMin + 0.6f, corridorMax - 0.6f);
            Vector2 to = new Vector2(land, transform.position.y);
            for (float t = 0; t < 0.55f; t += Time.deltaTime)
            {
                float k = t / 0.55f;
                p.transform.position = Vector2.Lerp(from, to, k) + Vector2.up * (4f * 1.1f * k * (1 - k));
                if (p.View != null) p.View.transform.localRotation = Quaternion.Euler(0, 0, -facing * k * 140f);
                yield return null;
            }
            if (p.View != null) p.View.transform.localRotation = Quaternion.identity;
            if (keepOrder >= 0 && View != null && View.sr != null) { View.sr.sortingOrder = keepOrder; keepOrder = -1; }
            p.transform.position = to;
            Sfx.Play("sfx_land", 0.9f, 0.9f);
            Cutscene.Shake(0.08f, 0.2f);
            Fx.Spawn("Smoke", to + Vector2.up * 0.2f, false, 20, 0.6f);
            // 잡기 피해는 착지 때 한 번만
            p.Frozen = false;
            p.ScriptControl = false;
            p.ResetCombatTimers();
            p.Hurt(T.grabDamage, (Vector2)transform.position, "physical");
            p.ScriptControl = true;
            p.AnimOverride = "Down";
            yield return new WaitForSeconds(0.35f);
            p.AnimOverride = "GetUp";
            yield return new WaitForSeconds(0.3f);
            p.AnimOverride = null; p.ScriptControl = false;
            p.GrantInvuln(ActionTuning.I.venelle.thrownInvuln);
            grabCd = 7f;
            Back(1.4f);
        }

        // ============================================================ 피격
        public void TakeHit(int damage, float stagger, Vector2 from, string kind)
        {
            if (!Alive) return;
            if (State == VState.Holding || State == VState.Thrown) return;
            bool opening = State == VState.Remix;
            hp -= Mathf.Max(0, damage) * (opening ? 2 : 1);
            LastHitByLuTime = Time.time;
            VenelleMind.OnHit(opening || State == VState.Recover);
            if (View != null) View.Flash(new Color(1f, 1f, 0.6f), 0.12f);
            Sfx.Play("sfx_hurt", 0.6f, 1.3f);
            if (opening) { Fx.Spawn("ReactBurst", Center + Vector2.up * 0.3f, false, 32, 0.7f); Say("아얏— 섞던 거 쏟았잖아!", 1.6f); State = VState.Backoff; timer = 1.0f; attacksSinceRemix = 0; }
            else if (State == VState.Stalk || State == VState.DashWindup) gap = Mathf.Max(gap, 0.3f);
            if (hp <= 0) { hp = 0; EnterDowned(); }
        }

        void EnterDowned()
        {
            ClearHazards();
            White();
            State = VState.Downed;
            Anim(View != null && View.HasState("Downed") ? "Downed" : "Collapsed");
            Sfx.Play("sfx_power_down", 0.6f, 0.7f);
            Say("하아... 하아... 몸이... 안 움직여...", 3f);
            OnSubdued?.Invoke();
        }

        /// <summary>대화가 끝난 뒤: 물러나서 공정한 준비 시간을 둔다. 흥분이 높으면 더 빨리 돌아온다.</summary>
        public void AfterTalk()
        {
            if (State == VState.Done || State == VState.Downed || State == VState.Stopped) return;
            if (State == VState.Holding) return;
            State = VState.Backoff; timer = T.resumeGrace * (Agit > 5 ? 0.7f : 1f);
        }

        public void Say(string s, float t = 2.5f) { EnsureBubble(); if (bubble != null) bubble.Say(s, t); }
        public void Express(string anim, float t) { StartCoroutine(ExpressCo(anim, t)); }
        IEnumerator ExpressCo(string anim, float t) { var keep = State; Anim(anim); yield return new WaitForSeconds(t); }

        /// <summary>테스트용.</summary>
        public void DebugSetState(VState s, float t = 1f) { State = s; timer = t; }
        public void DebugForceGrab() { if (g != null && g.Player != null) { FaceLu(); StartGrab(g.Player); } }
        public void DebugDamage(int d) { TakeHit(d, 0, transform.position, "test"); }
    }
}
