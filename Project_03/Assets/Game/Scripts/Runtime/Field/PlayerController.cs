using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>Native pixel side-view movement: Space interact/jump, W/S ladder, A/D exit, AA/DD dash.
    /// Dash distance, collision, invulnerability and cooldown use action_tuning.json.</summary>
    public class PlayerController : MonoBehaviour
    {
        public const float Width = 0.56f, Height = 1.3f, CrouchHeight = 0.95f;
        public float maxFall = 18f;

        public Vector2 Velocity;
        public bool Grounded { get; private set; }
        public bool Climbing { get; private set; }
        public bool Crouching { get; private set; }
        public bool Dodging => dodgeTimer > 0;
        public bool Invulnerable => invulnTimer > 0 || dodgeInvuln > 0 || ScriptControl;
        public bool Carrying;            // 베넬 부축: 느리게, 점프·회피 없음
        public bool Hidden;              // 전선 이동 중 (보이지 않음)
        /// <summary>연출이 위치를 직접 정하는 동안 (잡혀 들어 올려짐 등): 중력·충돌 없이 그 자리에 둔다.</summary>
        [System.NonSerialized] public bool Frozen;
        public int Facing { get; private set; } = 1;
        public CharacterView View;
        public Actor Actor;
        public PlayerSkills Skills { get; private set; }
        /// <summary>스킬 사용 중 이동을 잠깐 멈출 때.</summary>
        [System.NonSerialized] public float ActionLock;

        Game game;
        Ladder ladder;
        float coyote, jumpBuffer;
        Vector2 lastSafe;
        float safeTimer;
        Interactable focus;
        float dodgeTimer, dodgeCd, dodgeInvuln, invulnTimer, stunTimer, stepTimer, ladderLeave;
        int dodgeDir = 1;
        float lastDashInvuln = -99f;
        /// <summary>외부에서 미는 속도 (컨베이어 등). 매 프레임 다시 정해 준다.</summary>
        float pushX; int pushFrame = -10;
        /// <summary>컨베이어처럼 발밑에서 미는 속도. 매 프레임 불러야 유지된다 (이번 또는 직전 프레임 값만 쓴다).</summary>
        public void Push(float vx) { if (pushFrame < Time.frameCount - 1) pushX = 0; if (pushFrame != Time.frameCount) { pushX = 0; pushFrame = Time.frameCount; } pushX += vx; }
        public float PushX => pushFrame >= Time.frameCount - 1 ? pushX : 0f;
        readonly DoubleTapDash dashTap = new DoubleTapDash();
        int dashContextEpoch = -1;
        void OnApplicationFocus(bool focused) { dashTap.Reset(); if (focused) GameInput.LatchAll(); }
        void OnApplicationPause(bool paused) { dashTap.Reset(); }
        void OnDisable() { dashTap.Reset(); }
        bool wasGrounded = true;
        readonly RaycastHit2D[] hits = new RaycastHit2D[8];
        readonly Collider2D[] overlaps = new Collider2D[16];
        ContactFilter2D solidFilter, triggerFilter;
        ActionTuning.PlayerT T => ActionTuning.I.player;

        // 동료가 따라오는 발자취
        public struct Step { public Vector2 pos; public bool climbing; public bool wire; public int facing; }
        public readonly List<Step> Trail = new List<Step>();
        public float TrailLength { get; private set; }
        public System.Action<PowerLine> OnWireTravel;

        // 조종을 스크립트가 맡는 동안 (연출)
        public bool ScriptControl;
        public float ScriptMoveX;
        /// <summary>연출용 이동 속도 (0 이면 걷기 속도).</summary>
        [System.NonSerialized] public float ScriptSpeed;
        /// <summary>연출이 애니메이션을 직접 정하는 동안 (null 이면 자동).</summary>
        [System.NonSerialized] public string AnimOverride;

        public static PlayerController Spawn(Game g, Vector2 pos, int facing)
        {
            var go = new GameObject("Lu");
            go.transform.position = pos;
            var p = go.AddComponent<PlayerController>();
            p.game = g;
            p.Actor = go.AddComponent<Actor>();
            p.Actor.actorId = "lu";
            p.View = CharacterView.Create(go.transform, g.Config.GetArt("lu"), 12);
            p.Actor.view = p.View;
            p.Facing = facing; p.View.Face(facing);
            p.lastSafe = pos;
            p.Skills = go.AddComponent<PlayerSkills>();
            p.Skills.Init(g, p);
            return p;
        }

        void Awake()
        {
            solidFilter = new ContactFilter2D { useTriggers = false };
            solidFilter.SetLayerMask(Physics2D.AllLayers);
            triggerFilter = new ContactFilter2D { useTriggers = true };
            triggerFilter.SetLayerMask(Physics2D.AllLayers);
        }

        public void SetFacing(int f) { if (f != 0) { Facing = f > 0 ? 1 : -1; View.Face(Facing); } }

        /// <summary>감지용 몸 세 점 (발목·가슴·머리). 웅크리면 머리·가슴이 낮아진다.</summary>
        public IEnumerable<Vector2> SightPoints()
        {
            Vector2 p = transform.position;
            float h = Crouching ? CrouchHeight : Height;
            yield return p + new Vector2(0, 0.18f);
            yield return p + new Vector2(0, h * 0.55f);
            yield return p + new Vector2(0, h - 0.08f);
        }

        public Vector2 Chest => (Vector2)transform.position + new Vector2(0, (Crouching ? CrouchHeight : Height) * 0.55f);
        public float CurrentHeight => Crouching ? CrouchHeight : Height;

        bool Control => game.FieldInputAllowed && !Hidden && !ScriptControl && stunTimer <= 0;

        void Update()
        {
            if (game == null) game = Game.I;
            if (game == null || game.State == null) return;
            float dt = Mathf.Min(Time.deltaTime, 1f / 30f);
            if (dashContextEpoch != GameInput.ContextEpoch) { dashTap.Reset(); dashContextEpoch = GameInput.ContextEpoch; }
            if (Hidden) { dashTap.Reset(); Velocity = Vector2.zero; UpdateInteract(false); return; }
            if (Frozen)
            {
                dashTap.Reset();
                Velocity = Vector2.zero; Grounded = false;
                if (AnimOverride != null) { View.Play(AnimOverride); if (View.animator != null) View.animator.speed = 1f; }
                UpdateInteract(false);
                return;
            }
            if (invulnTimer > 0) invulnTimer -= dt;
            if (dodgeInvuln > 0) dodgeInvuln -= dt;
            if (dodgeCd > 0) dodgeCd -= dt;
            if (stunTimer > 0) stunTimer -= dt;
            if (ActionLock > 0) ActionLock -= dt;

            bool control = Control;
            float mx = ScriptControl ? ScriptMoveX : control && ActionLock <= 0 ? GameInput.MoveX : 0;
            bool upHeld = control && GameInput.Held(Act.MenuUp, InputCtx.Gameplay);
            bool downHeld = control && GameInput.Held(Act.Down, InputCtx.Gameplay);
            UpdateInteract(control);
            bool jump = control && !Carrying && GameInput.Down(Act.Jump, InputCtx.Gameplay);
            if (game.Map != null && (game.Map.zeroG || game.CurrentMapId == "P03"))
            {
                dashTap.Reset();
                Crouching = Climbing = false; Grounded = false;
                Vector2 axis = new Vector2(mx, (upHeld ? 1 : 0) - (downHeld ? 1 : 0));
                Velocity = Vector2.MoveTowards(Velocity, axis.normalized * 4.4f, 28f * dt);
                var pos = (Vector2)transform.position + Velocity * dt;
                var b = game.Map.bounds;
                pos.x = Mathf.Clamp(pos.x, b.xMin + 0.8f, b.xMax - 0.8f);
                pos.y = Mathf.Clamp(pos.y, b.yMin + 1.1f, b.yMax - Height - 0.6f);
                transform.position = pos;
                if (mx != 0) SetFacing((int)Mathf.Sign(mx));
                View.Play(AnimOverride ?? (Velocity.sqrMagnitude > 0.1f ? "Jump" : "Idle"));
                return;
            }
            bool wasClimbing = Climbing;

            // ---- 사다리: J 로 붙고 떼며, 붙은 동안 Space 위 / S 아래 (W 는 스킬 키라 쓰지 않는다)
            ladder = FindLadder();
            if (ladderLeave > 0) ladderLeave -= dt;
            if (!Climbing && ladder != null && !Carrying && ladderLeave <= 0)
            {
                // 꼭대기에 서서 S: 내려가려고 붙는다
                bool topGrab = Grounded && downHeld && GameInput.Down(Act.Down, InputCtx.Gameplay) && transform.position.y >= ladder.Top - 0.15f && Mathf.Abs(transform.position.x - ladder.CenterX) < 0.45f;
                if (control && (topGrab || (upHeld && transform.position.y < ladder.Top - 0.1f) || (downHeld && transform.position.y > ladder.Bottom + 0.2f))) AttachLadder();
            }
            if (Climbing)
            {
                if (ladder == null) Climbing = false;
                else
                {
                    var p = transform.position;
                    p.x = Mathf.MoveTowards(p.x, ladder.CenterX, 8f * dt);
                    float my = (upHeld ? 1 : 0) - (downHeld ? 1 : 0);
                    float ny = p.y + my * T.climb * dt;
                    if (ny >= ladder.Top) { ny = ladder.Top; Climbing = false; ladderLeave = 0.35f; }
                    if (ny <= ladder.Bottom) { ny = ladder.Bottom; Climbing = false; ladderLeave = 0.35f; }
                    transform.position = new Vector3(p.x, ny, 0);
                    if (my < 0 && CastGround(0.05f)) { Climbing = false; ladderLeave = 0.35f; }
                    // J: 손을 놓는다 (사다리에서 떼기)
                    if (Climbing && control && Mathf.Abs(mx) > 0.1f) { Climbing = false; ladderLeave = 0.35f; Velocity = new Vector2(mx * 2f, 0); }
                    if (Climbing)
                    {
                        Crouching = false;
                        View.Play("Climb");
                        if (View.animator != null) View.animator.speed = my == 0 ? 0f : 1f;
                        Velocity = Vector2.zero;
                        dashTap.Reset();
                        RecordTrail(true, false); UpdateInteract(control); UpdateCover();
                        return;
                    }
                }
            }

            // ---- 웅크리기 (지상에서 S 를 누르고 있는 동안). 일어설 자리가 없으면 웅크린 채 유지.
            bool wantCrouch = (downHeld && Grounded && !Carrying) || (Crouching && !CanStand());
            if (AnimOverride != null) wantCrouch = false;
            Crouching = wantCrouch;

            // ---- 회피
            // 기록 가능(조작 중, 사다리·부축 아님)과 발동 가능(대시 중 아님, 종료 뒤 짧은 대기 끝)을 나눈다.
            bool dashRecord = control && !Carrying && !Climbing && !wasClimbing && ladderLeave <= 0;
            bool dashEligible = dashRecord && ActionLock <= 0 && dodgeCd <= 0 && dodgeTimer <= 0;
            int tapDir = dashTap.Poll(Time.unscaledTime,
                GameInput.Down(Act.Left, InputCtx.Gameplay), GameInput.Down(Act.Right, InputCtx.Gameplay),
                GameInput.Up(Act.Left, InputCtx.Gameplay), GameInput.Up(Act.Right, InputCtx.Gameplay),
                dashRecord, dashEligible, Mathf.Clamp(T.doubleTapWindow, 0.1f, 0.5f));
            // Optional gamepad trigger / virtual testing action; no keyboard Shift binding.
            bool dodge = dashEligible && (tapDir != 0 || GameInput.Down(Act.Dodge, InputCtx.Gameplay));
            if (dodge)
            {
                dodgeDir = tapDir != 0 ? tapDir : Mathf.Abs(mx) > 0.1f ? (int)Mathf.Sign(mx) : Facing;
                dashTap.Reset();
                SetFacing(dodgeDir);
                dodgeTimer = T.dodgeTime; dodgeCd = 0;
                // 무적은 대시 이동과 따로: 지속은 대시 시간 이하, 연속 대시로 끝없이 이어지지 않게 재부여 간격을 둔다.
                if (Time.time - lastDashInvuln >= T.dashInvulnRearm)
                { dodgeInvuln = Mathf.Min(T.dodgeInvuln, T.dodgeTime); lastDashInvuln = Time.time; }
                Sfx.Play("sfx_dodge", 0.7f);
                Noise.Emit(transform.position, 1.2f, "dodge");
                if (Skills != null) Skills.CancelTargeting();
            }

            float speed = ScriptControl && ScriptSpeed > 0 ? ScriptSpeed : Carrying ? T.carry : Crouching ? T.crouchWalk : T.walk;
            if (dodgeTimer > 0)
            {
                dodgeTimer -= dt;
                Velocity.x = dodgeDir * T.dodgeDistance / Mathf.Max(0.05f, T.dodgeTime);
                // 재사용 대기는 끝난 시점부터 한 번만 센다 (시작 시점 쿨다운과 겹치는 이중 대기 없음)
                if (dodgeTimer <= 0) dodgeCd = T.dashCooldownFromEnd;
            }
            else if (stunTimer > 0) Velocity.x = Mathf.MoveTowards(Velocity.x, 0, 20f * dt);
            else
            {
                Velocity.x = mx * speed;
                if (mx != 0) SetFacing((int)Mathf.Sign(mx));
            }

            if (jump && !Crouching) jumpBuffer = 0.12f; else jumpBuffer -= dt;
            Velocity.y = Mathf.Max(Velocity.y - T.gravity * dt, -maxFall);
            if (Grounded) coyote = 0.1f; else coyote -= dt;
            if (jumpBuffer > 0 && coyote > 0 && dodgeTimer <= 0)
            {
                Velocity.y = T.jumpVelocity; jumpBuffer = 0; coyote = 0;
            }

            Move((Velocity + new Vector2(Grounded && !Climbing ? PushX : 0f, 0)) * dt);

            // 소리: 걷는 발소리(웅크리면 없음), 착지
            if (Grounded && Mathf.Abs(Velocity.x) > 0.5f && !Crouching && !ScriptControl)
            {
                stepTimer -= dt;
                if (stepTimer <= 0) { stepTimer = 0.42f; Noise.Emit(transform.position, T.footstepNoise, "step"); }
            }
            if (Grounded && !wasGrounded && !ScriptControl) Noise.Emit(transform.position, T.landNoise, "land");
            wasGrounded = Grounded;

            // 안전한 발판 기록 / 낙하 복귀
            if (Grounded)
            {
                safeTimer += dt;
                if (safeTimer > 0.25f) lastSafe = transform.position;
            }
            else safeTimer = 0;
            if (game.Map != null && transform.position.y < game.Map.KillY) StartCoroutine(RecoverFall());

            // 애니메이션
            if (AnimOverride != null) { View.Play(AnimOverride); if (View.animator != null) View.animator.speed = 1f; }
            else if (Skills != null && Skills.AnimBusy) { }
            else if (stunTimer > 0) View.Play("Hurt");
            else if (dodgeTimer > 0) View.Play(View.HasState("Dodge") ? "Dodge" : "Jump");
            else if (!Grounded) View.Play(Velocity.y > 0 ? "Jump" : "Fall");
            else if (Crouching) View.Play(Mathf.Abs(Velocity.x) > 0.05f && View.HasState("CrouchWalk") ? "CrouchWalk" : View.HasState("Crouch") ? "Crouch" : "Idle");
            else if (Mathf.Abs(Velocity.x) > 0.05f) View.Play(Carrying ? "Carry" : "Walk");
            else View.Play(Carrying ? "Carry" : "Idle");
            if (AnimOverride == null && View.animator != null) View.animator.speed = Carrying && Mathf.Abs(Velocity.x) < 0.05f ? 0f : 1f;
            // 무적 깜빡임
            if (View.sr != null && AnimOverride == null)
            {
                var c = View.sr.color;
                c.a = invulnTimer > 0 && (Time.time * 14f % 1f) < 0.4f ? 0.35f : 1f;
                View.sr.color = c;
            }

            RecordTrail(false, false);
            UpdateCover();
        }

        // ------------------------------------------------------------ 엄폐 (B형: 뒤에 숨는 전경 물체)
        HideSpot hideSpot, occupied;
        float coverBreak;
        SpriteRenderer silhouette;
        /// <summary>전경 엄폐물 영역 안 (아직 숨지 않았어도).</summary>
        public HideSpot InHideZone => hideSpot;
        /// <summary>숨음: 영역 안에서 웅크린 상태. CCTV·경비의 시야 판정에서 빠진다.</summary>
        public bool HiddenInCover => hideSpot != null && Crouching && !Climbing && coverBreak <= 0 && !Hidden;
        /// <summary>공격 등으로 잠깐 노출 (숨음이 풀린다).</summary>
        public void BreakCover(float seconds) { coverBreak = Mathf.Max(coverBreak, seconds); }

        void UpdateCover()
        {
            if (coverBreak > 0) coverBreak -= Time.deltaTime;
            hideSpot = Climbing || Hidden ? null : HideSpot.At(transform.position);
            var now = HiddenInCover ? hideSpot : null;
            if (now != occupied)
            {
                if (occupied != null) occupied.SetOccupied(false);
                if (now != null) { now.SetOccupied(true); Sfx.Play("sfx_cloth", 0.35f); }
                occupied = now;
            }
            // 숨은 동안: 엄폐물 앞에 옅은 윤곽 (같은 프레임 그림을 앞쪽 순서에 반투명 청색으로)
            if (now != null && View != null && View.sr != null)
            {
                if (silhouette == null)
                {
                    var go = new GameObject("CoverSilhouette");
                    go.transform.SetParent(View.sr.transform, false);
                    silhouette = go.AddComponent<SpriteRenderer>();
                }
                silhouette.enabled = true;
                silhouette.sprite = View.sr.sprite; silhouette.flipX = View.sr.flipX;
                silhouette.sortingOrder = 23;
                silhouette.color = new Color(0.55f, 0.85f, 1f, 0.38f);
            }
            else if (silhouette != null) silhouette.enabled = false;
        }

        void AttachLadder()
        {
            dashTap.Reset();
            Climbing = true; Velocity = Vector2.zero; Crouching = false;
            if (Skills != null) Skills.CancelTargeting();
        }

        bool CanStand()
        {
            Vector2 pos = transform.position;
            var f = solidFilter;
            int n = Physics2D.OverlapBox(pos + new Vector2(0, (Height + CrouchHeight) / 2f + 0.02f), new Vector2(Width - 0.1f, Height - CrouchHeight - 0.06f), 0, f, overlaps);
            for (int i = 0; i < n; i++) if (overlaps[i].GetComponent<OneWayPlatform>() == null) return false;
            return true;
        }

        // ------------------------------------------------------------ 피격
        /// <summary>피해를 받는다. 무적·회피 중이면 무시하고 false.</summary>
        public bool Hurt(int damage, Vector2 from, string kind = "physical")
        {
            if (Invulnerable || game == null || game.State == null || Hidden) return false;
            var diff = ActionTuning.CurrentDifficulty;
            int dmg = Mathf.Max(1, Mathf.RoundToInt(damage * diff.enemyDamage));
            var lu = game.State.D.lu;
            lu.hp = Mathf.Max(0, lu.hp - dmg);
            dashTap.Reset();
            invulnTimer = T.hurtInvuln; stunTimer = T.hurtStun;
            float dir = Mathf.Sign(transform.position.x - from.x); if (dir == 0) dir = -Facing;
            Velocity = new Vector2(dir * T.hurtKnockback, 4f);
            Climbing = false; Crouching = false;
            if (Skills != null) Skills.CancelTargeting();
            View.Flash(kind == "electric" ? new Color(1f, 1f, 0.5f) : new Color(1f, 0.5f, 0.5f), 0.15f);
            Sfx.Play(kind == "acid" ? "sfx_acid" : "sfx_hurt", 0.9f);
            var rig = CameraRig.Main; if (rig != null) rig.Shake(0.08f, 0.2f);
            game.UI.hud.Refresh(game.State);
            if (lu.hp <= 0) game.OnPlayerDown();
            return true;
        }

        bool recovering;
        IEnumerator RecoverFall()
        {
            if (recovering) yield break;
            recovering = true;
            dashTap.Reset();
            game.Block("fall");
            yield return game.UI.Fade(1f, 0.2f);
            transform.position = lastSafe;
            Velocity = Vector2.zero;
            Trail.Clear(); TrailLength = 0;
            if (game.Companion != null) game.Companion.Regroup();
            yield return game.UI.Fade(0f, 0.2f);
            game.Unblock("fall");
            recovering = false;
        }

        // ------------------------------------------------------------ 충돌
        bool IsOneWayBlocking(RaycastHit2D h, float dy, float footY)
        {
            var ow = h.collider.GetComponent<OneWayPlatform>();
            if (ow == null) return true;
            if (Climbing || dy > 0) return false;
            if (Crouching && GameInput.Held(Act.Down, InputCtx.Gameplay) && GameInput.Held(Act.Jump, InputCtx.Gameplay)) return false;
            return footY >= h.collider.bounds.max.y - 0.06f;
        }

        void Move(Vector2 delta)
        {
            Vector2 pos = transform.position;
            float h = CurrentHeight;
            // 수평
            if (Mathf.Abs(delta.x) > 0)
            {
                Vector2 center = pos + new Vector2(0, h / 2f + 0.02f);
                int n = Physics2D.BoxCast(center, new Vector2(Width, h - 0.2f), 0, new Vector2(Mathf.Sign(delta.x), 0), solidFilter, hits, Mathf.Abs(delta.x) + 0.02f);
                float allowed = Mathf.Abs(delta.x);
                for (int i = 0; i < n; i++)
                {
                    if (hits[i].collider.GetComponent<OneWayPlatform>() != null) continue;
                    allowed = Mathf.Min(allowed, Mathf.Max(0, hits[i].distance - 0.02f));
                }
                pos.x += Mathf.Sign(delta.x) * allowed;
            }
            // 수직
            Grounded = false;
            if (Mathf.Abs(delta.y) > 0)
            {
                Vector2 center = pos + new Vector2(0, h / 2f);
                int n = Physics2D.BoxCast(center, new Vector2(Width - 0.06f, h - 0.04f), 0, new Vector2(0, Mathf.Sign(delta.y)), solidFilter, hits, Mathf.Abs(delta.y) + 0.02f);
                float allowed = Mathf.Abs(delta.y);
                bool hit = false;
                for (int i = 0; i < n; i++)
                {
                    if (!IsOneWayBlocking(hits[i], delta.y, pos.y)) continue;
                    if (hits[i].distance <= 0f && hits[i].collider.GetComponent<OneWayPlatform>() != null) continue;
                    // 발밑 턱에 살짝 겹친 경우: 위로는 막지 않고(점프 가능), 아래로는 턱 위로 올려 세운다
                    float top = hits[i].collider.bounds.max.y;
                    if (hits[i].distance <= 0f && top > pos.y - 0.001f && top - pos.y < 0.2f)
                    {
                        if (delta.y > 0) continue;
                        pos.y = top + 0.005f; allowed = 0; hit = true;
                        break;
                    }
                    float d = Mathf.Max(0, hits[i].distance - 0.02f);
                    if (d < allowed) { allowed = d; hit = true; }
                }
                pos.y += Mathf.Sign(delta.y) * allowed;
                if (hit)
                {
                    if (delta.y < 0) Grounded = true;
                    Velocity.y = 0;
                }
            }
            if(game!=null && game.CurrentMapId=="C06" && game.State.Has("c3_boss_intro") && !game.State.Has("c3_boss_done"))
            {
                var b=Object.FindAnyObjectByType<VenelleBoss>();if(b!=null)pos.x=Mathf.Clamp(pos.x,b.corridorMin,b.corridorMax);
            }
            transform.position = pos;
        }

        bool CastGround(float dist)
        {
            Vector2 pos = transform.position;
            int n = Physics2D.BoxCast(pos + new Vector2(0, Height / 2f), new Vector2(Width - 0.06f, Height - 0.04f), 0, Vector2.down, solidFilter, hits, dist + 0.02f);
            for (int i = 0; i < n; i++) if (IsOneWayBlocking(hits[i], -1, pos.y + 0.001f) && hits[i].collider.GetComponent<OneWayPlatform>() == null) return true;
            return false;
        }

        Ladder FindLadder()
        {
            Vector2 c = (Vector2)transform.position + new Vector2(0, 0.5f);
            int n = Physics2D.OverlapBox(c, new Vector2(0.3f, 1.0f), 0, triggerFilter, overlaps);
            for (int i = 0; i < n; i++)
            {
                var l = overlaps[i].GetComponent<Ladder>();
                if (l != null) return l;
            }
            return null;
        }

        // ------------------------------------------------------------ 조사 (Enter)
        public Interactable Focus => focus;

        void UpdateInteract(bool control)
        {
            if (!control || (Skills != null && Skills.Targeting)) { focus = null; game.UI.HidePrompt(); return; }
            Interactable best = null; float bestD = 99;
            foreach (var it in Interactable.All)
            {
                if (it == null || !it.isActiveAndEnabled || !it.CanInteract(game)) continue;
                Vector2 d = (Vector2)it.transform.position - (Vector2)transform.position;
                float rx = Mathf.Max(it.range, 1.45f), ry = Mathf.Max(it.rangeY, 0.75f);
                if (Mathf.Abs(d.x) > rx || d.y < -Mathf.Min(ry,1f) || d.y > Mathf.Min(ry+0.35f,1.15f)) continue;
                if (!(game.Map != null && (game.Map.zeroG || game.CurrentMapId == "P03")) && !Sight.Clear(Chest, (Vector2)it.transform.position + Vector2.up * 0.55f)) continue;
                float score = Mathf.Abs(d.x) + Mathf.Abs(d.y) * 0.75f;
                if (it is PowerLine && (ladder != null || !Grounded)) score += 2f;
                if (score < bestD) { bestD = score; best = it; }
            }
            focus = best;
            string key = GameInput.Label(Act.Interact);
            if (best != null)
            {
                game.UI.ShowPrompt(best.PromptWorldPos, key + " · " + best.Prompt(game));
                if (GameInput.Down(Act.Interact, InputCtx.Gameplay)) { GameInput.Consume(Act.Interact); game.RunEvent(best.Interact(game)); }
            }
            else if (ladder != null || Climbing)
                game.UI.ShowPrompt(new Vector3(transform.position.x, transform.position.y + 1.8f), GameInput.Label(Act.MenuUp) + " 위 · " + GameInput.Label(Act.Down) + " 아래 · " + GameInput.Pair(Act.Left, Act.Right) + " 이탈");
            else if (hideSpot != null && !HiddenInCover && !Carrying)
                game.UI.ShowPrompt(new Vector3(transform.position.x, transform.position.y + 1.7f), GameInput.Label(Act.Down) + " · 숨기");
            else game.UI.HidePrompt();
        }

        // ------------------------------------------------------------ 발자취
        public void RecordTrail(bool climbing, bool wire)
        {
            Vector2 p = transform.position;
            if (Trail.Count > 0 && !wire)
            {
                var last = Trail[Trail.Count - 1];
                float d = Vector2.Distance(last.pos, p);
                if (d < 0.06f) return;
                TrailLength += d;
            }
            Trail.Add(new Step { pos = p, climbing = climbing, wire = wire, facing = Facing });
            if (Trail.Count > 900) { Trail.RemoveRange(0, 300); TrailBase += 300; }
        }

        /// <summary>발자취 목록 앞부분이 잘릴 때마다 늘어나는 절대 인덱스 기준.</summary>
        public int TrailBase { get; private set; }

        public void ClearTrail() { TrailBase += Trail.Count; Trail.Clear(); TrailLength = 0; }

        /// <summary>스크립트로 지정 위치까지 걷기 (연출용).</summary>
        public IEnumerator WalkTo(float x, float speedMul = 1f)
        {
            ScriptControl = true;
            float guard = 6f;
            while (Mathf.Abs(transform.position.x - x) > 0.08f && guard > 0)
            {
                ScriptMoveX = Mathf.Sign(x - transform.position.x) * speedMul;
                guard -= Time.deltaTime;
                yield return null;
            }
            ScriptMoveX = 0; ScriptControl = false;
        }

        public void Teleport(Vector2 p)
        {
            dashTap.Reset();
            transform.position = p; Velocity = Vector2.zero; lastSafe = p;
        }

        /// <summary>테스트·연출용: 회피 재사용·무적 상태를 비운다.</summary>
        public void ResetCombatTimers() { dashTap.Reset(); dodgeTimer = dodgeCd = dodgeInvuln = invulnTimer = stunTimer = 0; lastDashInvuln = -99f; }
        public float DodgeCooldownLeft => dodgeCd;
        /// <summary>던져진 뒤 등: 짧은 보호 시간 (깜빡임).</summary>
        public void GrantInvuln(float s) { invulnTimer = Mathf.Max(invulnTimer, s); }
        public float InvulnLeft => Mathf.Max(invulnTimer, dodgeInvuln);
    }
}
