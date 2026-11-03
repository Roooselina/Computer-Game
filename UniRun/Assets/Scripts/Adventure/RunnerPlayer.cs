using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 오른쪽으로 자동으로 달리는 플레이어. 점프/이단 점프/수영, 경사면 밀착, 통나무 탑승을 처리한다.
[DefaultExecutionOrder(-50)]
public class RunnerPlayer : MonoBehaviour {
    public static RunnerPlayer instance;

    public InputActionAsset actions;
    public Transform visual;              // 스프라이트와 애니메이터가 붙은 자식 (찌그러짐 연출용)
    public Sprite shieldSprite;
    public float baseSpeed = 8.5f;
    public float maxSpeed = 11.5f;
    public float jumpVelocity = 14f;
    public float doubleJumpVelocity = 12.5f;
    public float swimStroke = 5.5f;
    public int maxJumpCount = 2;
    public float feetOffset = -0.72f;     // 스케일 1 기준 발 위치. 실제 값은 FeetOffset (스케일 반영)

    public float Scale { get { return transform.localScale.y; } }
    public float FeetOffset { get { return feetOffset * Scale; } }

    public bool Grounded { get; private set; }
    public bool Submerged { get; private set; }
    public bool HasShield { get; private set; }
    public bool MagnetActive { get { return Time.time < magnetUntil; } }
    public bool Invincible { get { return Time.time < invincibleUntil || !controlEnabled; } }
    public Vector2 LastSafePosition { get; private set; }
    public float GroundY { get; private set; }   // 마지막으로 서 있던 발 높이 (카메라 기준)
    public float Speed { get; private set; }
    public Vector2 Velocity { get { return body.linearVelocity; } }

    private Rigidbody2D body;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer bubble;
    private InputAction jumpAction;
    private ContactFilter2D groundFilter;
    private readonly RaycastHit2D[] hits = new RaycastHit2D[4];
    private readonly ContactPoint2D[] contacts = new ContactPoint2D[8];

    private int jumpCount;
    private float coyoteTimer;
    private float bufferTimer;
    private float strokeCooldown;
    private float invincibleUntil;
    private float magnetUntil;
    private float dustTimer;
    private float minAirVelocity;
    private float jumpLock;               // 점프 직후 바닥 밀착 금지 시간
    private bool controlEnabled = true;
    private bool wasSubmerged;
    private Vector3 squash = Vector3.one;
    private float stuckTimer;
    private float stuckX;
    private readonly List<Vector2> safeTrail = new List<Vector2>();   // 지나온 안전 지점들 (부활 위치 후보)

    private void Awake() {
        instance = this;
        body = GetComponent<Rigidbody2D>();
        animator = visual.GetComponent<Animator>();
        spriteRenderer = visual.GetComponent<SpriteRenderer>();
        groundFilter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = ~(1 << 2) };

        var b = new GameObject("Shield Bubble");
        b.transform.SetParent(transform, false);
        b.transform.localPosition = new Vector3(0, -0.05f, 0);
        b.transform.localScale = Vector3.one * 1.9f;
        bubble = b.AddComponent<SpriteRenderer>();
        bubble.sprite = shieldSprite;
        bubble.sortingOrder = 12;
        bubble.enabled = false;

        // 스케일이 바뀌어도 발이 지면 위에 오도록 시작 위치를 맞춘다
        Vector2 start = MapLoader.instance.PlayerStart;
        start.y = MapLoader.instance.ColumnTop(start.x) - FeetOffset + 0.05f;
        transform.position = start;
        LastSafePosition = start;
        safeTrail.Add(start);
        GroundY = start.y + FeetOffset;
        stuckX = transform.position.x;
    }

    private void OnEnable() {
        jumpAction = actions.FindAction("Player/Jump", true);
        jumpAction.Enable();
        jumpAction.performed += OnJumpPerformed;
        jumpAction.canceled += OnJumpCanceled;
    }

    private void OnDisable() {
        jumpAction.performed -= OnJumpPerformed;
        jumpAction.canceled -= OnJumpCanceled;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx) {
        RequestJump();
    }

    public void RequestJump() {
        if (controlEnabled) bufferTimer = 0.2f;   // 착지 직전에 눌러도 점프가 나가도록 입력을 잠깐 기억
    }

    public int JumpCount { get { return jumpCount; } }

    private void OnJumpCanceled(InputAction.CallbackContext ctx) {
        // 짧게 누르면 낮게 뛴다
        if (controlEnabled && !Submerged && body.linearVelocity.y > 0f) {
            body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y * 0.5f);
        }
    }

    private void FixedUpdate() {
        if (!controlEnabled) return;
        float dt = Time.fixedDeltaTime;
        MapLoader map = MapLoader.instance;
        Vector2 pos = body.position;
        Vector2 v = body.linearVelocity;

        float progress = map.Progress(pos.x);
        Speed = Mathf.Lerp(baseSpeed, maxSpeed, progress);

        // ---- 액체
        ZoneData fluidZone = map.FluidAt(pos.x);
        float surface = fluidZone.water + 0.7f;
        if (fluidZone.lethal) {
            Submerged = false;
            if (pos.y + FeetOffset < surface - 0.15f) RunGame.instance.HurtPlayer(pos, true);
        } else {
            Submerged = pos.y < surface - 0.1f;
        }
        if (Submerged != wasSubmerged) {
            Fx.instance.Splash(new Vector2(pos.x, surface), Color.white, Mathf.Clamp((int)Mathf.Abs(v.y) * 2, 8, 30));
            Sfx.instance.Play(Sfx.instance.splash, 0.8f);
            if (Submerged) RunCamera.instance.Shake(0.15f);
            wasSubmerged = Submerged;
        }

        // ---- 바닥 검사: 발 아래 캐스트 + 실제 충돌 접촉을 둘 다 본다 (어느 하나라도 땅이면 땅)
        bool wasGrounded = Grounded;
        Grounded = false;
        jumpLock -= dt;
        float sc = Scale;
        Collider2D groundCol = null;
        Rigidbody2D groundBody = null;
        Vector2 groundNormal = Vector2.up;
        Vector2 groundPoint = pos + new Vector2(0, FeetOffset);
        float groundGap = 0f;                 // 발과 땅 사이 거리
        float nearGround = float.MaxValue;    // 착지 직전 판정용
        int n = Physics2D.CircleCast(pos + new Vector2(0, FeetOffset + 0.38f * sc), 0.27f * sc, Vector2.down, groundFilter, hits, 1.2f * sc);
        for (int i = 0; i < n; i++) {
            if (hits[i].normal.y <= 0.55f) continue;
            float d = hits[i].distance - 0.11f * sc;
            nearGround = Mathf.Min(nearGround, d);
            if (groundCol == null && d < (wasGrounded ? 0.32f : 0.14f) * sc) {
                groundCol = hits[i].collider; groundBody = hits[i].rigidbody;
                groundNormal = hits[i].normal; groundPoint = hits[i].point; groundGap = Mathf.Max(0f, d);
            }
        }
        if (groundCol == null) {
            int c = body.GetContacts(groundFilter, contacts);
            for (int i = 0; i < c; i++) {
                if (contacts[i].normal.y > 0.55f) {
                    groundCol = contacts[i].collider; groundBody = contacts[i].rigidbody;
                    groundNormal = contacts[i].normal; groundPoint = contacts[i].point; groundGap = 0f;
                    nearGround = 0f;
                    break;
                }
            }
        }
        Vector2 platformVelocity = groundBody != null && groundBody.bodyType == RigidbodyType2D.Kinematic ? groundBody.linearVelocity : Vector2.zero;
        // 경사를 따라 오를 때 생기는 위쪽 속도까지는 땅으로 인정하고, 그보다 빠르게 솟을 때(점프)만 공중으로 본다.
        float slopeRise = Speed * Mathf.Abs(groundNormal.x) / Mathf.Max(0.2f, groundNormal.y);
        if (groundCol != null && jumpLock <= 0f && v.y - platformVelocity.y < slopeRise + 1.5f) Grounded = true;

        if (Grounded) {
            coyoteTimer = 0.12f;
            jumpCount = 0;
            if (!wasGrounded) Land(-minAirVelocity, groundPoint);
            minAirVelocity = 0f;
            FloatingLog log = groundCol.GetComponent<FloatingLog>();
            if (log != null) log.Stand();
            else if (map.IsSafeGround(pos.x)) {
                LastSafePosition = new Vector2(pos.x, groundPoint.y - FeetOffset + 0.05f);
                if (pos.x > safeTrail[safeTrail.Count - 1].x + 1f) safeTrail.Add(LastSafePosition);
            }
            GroundY = groundPoint.y;
        } else {
            coyoteTimer -= dt;
            minAirVelocity = Mathf.Min(minAirVelocity, v.y);
        }

        // ---- 이동
        if (Submerged) {
            body.gravityScale = 0.2f;
            v.x = Speed * 0.6f;
            if (pos.y < surface - 0.6f) v.y += 7f * dt;        // 부력
            v.y = Mathf.Clamp(v.y * (1f - 2.5f * dt), -4f, 9f);
            jumpCount = 0;
        } else if (Grounded) {
            body.gravityScale = 1f;
            Vector2 tangent = new Vector2(groundNormal.y, -groundNormal.x);
            v = tangent * (Speed / tangent.x) + new Vector2(0, platformVelocity.y);
            if (groundGap > 0.01f) body.position = pos + Vector2.down * groundGap;   // 내리막에서 튕겨 나가지 않게 밀착
        } else {
            body.gravityScale = v.y < 0f ? 1.6f : 1f;
            v.x = Speed;
            v.y = Mathf.Max(v.y, -24f);
        }

        // ---- 점프
        bufferTimer -= dt;
        strokeCooldown -= dt;
        if (bufferTimer > 0f) {
            if (Submerged) {
                if (pos.y > surface - 1.0f) { v.y = jumpVelocity; Jumped(false); jumpCount = 1; }
                else if (strokeCooldown <= 0f) {
                    v.y = Mathf.Max(v.y, swimStroke); strokeCooldown = 0.2f; bufferTimer = 0f;
                    Fx.instance.Bubbles(pos, 5);
                }
            } else if (Grounded || coyoteTimer > 0f || (v.y < 0f && nearGround < 0.6f * sc)) {
                // 땅 위, 땅에서 막 떨어진 직후, 착지 직전이면 모두 지상 점프로 처리한다 (이단 점프를 아껴 준다)
                v.y = jumpVelocity; jumpCount = 1; Jumped(false);
            } else if (jumpCount < maxJumpCount) {
                if (jumpCount == 0) jumpCount = 1;   // 발판에서 떨어진 뒤의 첫 점프는 한 번으로 친다
                v.y = doubleJumpVelocity; jumpCount++; Jumped(true);
            }
        }
        body.linearVelocity = v;

        // ---- 끼임 구조: 3초 동안 앞으로 못 가면 안전지대로 복귀
        if (pos.x > stuckX + 1f) { stuckX = pos.x; stuckTimer = 0f; }
        else if ((stuckTimer += dt) > 3f) { stuckTimer = 0f; RunGame.instance.HurtPlayer(pos, true); }
    }

    private static Vector2 PlatformVelocity(RaycastHit2D hit) {
        return hit.rigidbody != null && hit.rigidbody.bodyType == RigidbodyType2D.Kinematic ? hit.rigidbody.linearVelocity : Vector2.zero;
    }

    private void Jumped(bool doubleJump) {
        bufferTimer = 0f;
        coyoteTimer = 0f;
        jumpLock = 0.12f;
        Grounded = false;
        squash = new Vector3(0.75f, 1.3f, 1f);
        Vector2 feet = (Vector2)transform.position + new Vector2(0, FeetOffset);
        if (doubleJump) {
            Fx.instance.Ring(feet, new Color(1f, 1f, 1f, 0.8f));
            Sfx.instance.Play(Sfx.instance.doubleJump, 0.7f, 1.2f);
        } else {
            Fx.instance.Dust(feet, 6, 2.5f);
            Sfx.instance.Play(Sfx.instance.jump, 0.6f);
        }
    }

    private void Land(float impact, Vector2 point) {
        float k = Mathf.Clamp01((impact - 6f) / 20f);
        squash = new Vector3(1f + 0.45f * k + 0.1f, 1f - 0.4f * k - 0.05f, 1f);
        Fx.instance.Dust(point, 4 + (int)(k * 14), 2f + k * 4f);
        if (impact > 14f) {
            RunCamera.instance.Shake(0.15f + k * 0.35f);
            Sfx.instance.Play(Sfx.instance.land, 0.4f + k * 0.5f, 1f - k * 0.3f);
        }
    }

    private void Update() {
        // 찌그러짐 복원, 경사면 따라 살짝 기울이기, 무적 깜빡임
        squash = Vector3.Lerp(squash, Vector3.one, Time.deltaTime * 12f);
        visual.localScale = squash;
        float tilt = 0f;
        if (Grounded) tilt = Mathf.Atan2(body.linearVelocity.y, Mathf.Max(0.1f, body.linearVelocity.x)) * Mathf.Rad2Deg * 0.5f;
        else if (Submerged) tilt = Mathf.Sin(Time.time * 4f) * 8f;
        visual.localRotation = Quaternion.Lerp(visual.localRotation, Quaternion.Euler(0, 0, tilt), Time.deltaTime * 10f);

        if (controlEnabled) animator.SetBool("Grounded", Grounded);
        bool blink = Time.time < invincibleUntil && Mathf.Repeat(Time.time, 0.16f) < 0.08f;
        spriteRenderer.color = blink ? new Color(1, 1, 1, 0.25f) : Color.white;

        bubble.enabled = HasShield;
        if (HasShield) {
            float s = 1.9f + Mathf.Sin(Time.time * 6f) * 0.08f;
            bubble.transform.localScale = new Vector3(s, s, 1);
            bubble.color = new Color(1, 1, 1, 0.6f + Mathf.Sin(Time.time * 8f) * 0.2f);
        }
        if (Grounded && controlEnabled && (dustTimer -= Time.deltaTime) < 0f) {
            dustTimer = 0.11f;
            Fx.instance.Dust((Vector2)transform.position + new Vector2(-0.25f * Scale, FeetOffset), 1, 0.8f);
        }
    }

    // ---------------------------------------------------------------- RunGame 에서 호출
    public void GiveShield() { HasShield = true; }

    public void PopShield() {
        HasShield = false;
        invincibleUntil = Time.time + 1.2f;
        Fx.instance.Burst(transform.position, new Color(0.6f, 0.9f, 1f), 24, 7f, 0.25f, 0.6f, 0f);
    }

    public void GiveMagnet(float seconds) { magnetUntil = Time.time + seconds; }

    public void Knockback() {
        invincibleUntil = Time.time + 2f;
        body.linearVelocity = new Vector2(body.linearVelocity.x, 11f);
        jumpCount = 1;
        squash = new Vector3(1.3f, 0.7f, 1f);
    }

    public void StompBounce() {
        body.linearVelocity = new Vector2(body.linearVelocity.x, 13f);
        jumpCount = 1;
        squash = new Vector3(1.35f, 0.7f, 1f);
    }

    public void SetControl(bool on) {
        controlEnabled = on;
        body.simulated = on;
        if (on) { body.linearVelocity = Vector2.zero; stuckX = transform.position.x; stuckTimer = 0f; }
    }

    public void Teleport(Vector2 p) {
        body.position = p;
        transform.position = p;
        body.linearVelocity = Vector2.zero;
        minAirVelocity = 0f;
        wasSubmerged = Submerged = false;
        // 점프 상태를 초기화해야 부활 직후 점프/이단 점프가 정상으로 나간다
        jumpCount = 0;
        coyoteTimer = 0f;
        bufferTimer = 0f;
        jumpLock = 0f;
        Grounded = true;
        GroundY = p.y + FeetOffset;
        squash = Vector3.one;
        animator.SetBool("Grounded", true);
        animator.Play("Run", 0, 0f);
    }

    // 죽은 곳에서 충분히 뒤, 앞쪽으로 달릴 거리가 확보된 안전 지점을 고른다
    public Vector2 RespawnPoint(float deathX, float backTiles, int runway) {
        MapLoader map = MapLoader.instance;
        Vector2 best = safeTrail[0];
        int bestRun = -1;
        for (int i = safeTrail.Count - 1; i >= 0; i--) {
            Vector2 c = safeTrail[i];
            if (c.x > deathX - backTiles) continue;
            if (c.x < deathX - 40f) break;                 // 너무 멀리 되돌아가지는 않는다
            int run = map.RunwayAhead(c.x);
            if (run >= runway) return c;
            if (run > bestRun) { bestRun = run; best = c; }
        }
        if (bestRun >= 0) return best;
        for (int i = safeTrail.Count - 1; i >= 0; i--) if (safeTrail[i].x < deathX) return safeTrail[i];
        return safeTrail[0];
    }

    public void MakeInvincible(float seconds) { invincibleUntil = Time.time + seconds; }

    // 시작 카운트다운 동안 멈춰 서 있기
    public void Hold(bool hold) {
        SetControl(!hold);
        animator.speed = hold ? 0f : 1f;
    }

    public void Die() {
        SetControl(false);
        animator.SetTrigger("Die");
    }
}
