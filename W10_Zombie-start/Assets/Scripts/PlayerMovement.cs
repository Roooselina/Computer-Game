using UnityEngine;

// The Rigidbody is the only movement owner, including dash, sliding and small steps.
public class PlayerMovement : MonoBehaviour {
    public float moveSpeed = 5f, rotateSpeed = 1440f;
    [Range(0f, 180f)] public float snapTurnAngle = 60f;
    [Range(.005f, .05f)] public float skinWidth = .02f;
    [Range(0f, .3f)] public float stepHeight = .2f;
    [Range(1, 8)] public int slideIterations = 4;
    [Range(0f, .15f)] public float maxOverlapRecovery = .08f;
    [Range(10f, 70f)] public float maxSlopeAngle = 50f;
    public Vector3 moveDirection { get; private set; }
    PlayerInput input;
    Rigidbody body;
    Animator animator;
    PlayerAim aim;
    PlayerDash dash;
    CapsuleCollider capsule;
    PhysicsMaterial ownedMaterial;
    readonly RaycastHit[] castBuffer = new RaycastHit[64];
    readonly Collider[] overlapBuffer = new Collider[32];
    int solidMask;

    void Awake() {
        input = GetComponent<PlayerInput>(); body = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>(); aim = GetComponent<PlayerAim>();
        dash = GetComponent<PlayerDash>(); capsule = GetComponent<CapsuleCollider>();
        // Respect the project's collision matrix rather than borrowing the aim ray's layer mask.
        for (int layer = 0; layer < 32; layer++)
            if (!Physics.GetIgnoreLayerCollision(gameObject.layer, layer)) solidMask |= 1 << layer;
        if (capsule && capsule.sharedMaterial == null) {
            ownedMaterial = new PhysicsMaterial("W10 Player Locomotion") {
                staticFriction = 0, dynamicFriction = 0, bounciness = 0,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
            capsule.sharedMaterial = ownedMaterial;
        }
    }
    void OnDestroy() { if (ownedMaterial) Destroy(ownedMaterial); }

    void FixedUpdate() {
        if (!input || !body || Time.timeScale == 0 ||
            (GameManager.instance != null && GameManager.instance.isGameover)) return;
        moveDirection = GetMoveDirection(input.movement);
        Vector3 velocity = dash && dash.isDashing ? dash.direction * dash.speed : moveDirection * moveSpeed;
        Vector3 delta = velocity * Time.fixedDeltaTime;
        Vector3 next = capsule ? ResolveMovement(body.position, delta) : body.position + delta;
        if (next.y > body.position.y + skinWidth && body.linearVelocity.y < 0)
            body.linearVelocity = new Vector3(body.linearVelocity.x, 0, body.linearVelocity.z);
        body.MovePosition(next);

        Vector3 facing = aim ? aim.aimPoint - body.position : transform.forward;
        facing.y = 0;
        if (facing.sqrMagnitude > .01f) {
            Quaternion target = Quaternion.LookRotation(facing);
            body.MoveRotation(Quaternion.Angle(body.rotation, target) > snapTurnAngle ? target :
                Quaternion.RotateTowards(body.rotation, target, rotateSpeed * Time.fixedDeltaTime));
        }
        if (animator) {
            Vector3 animationDirection = dash && dash.isDashing ? dash.direction : moveDirection;
            float sign = Vector3.Dot(animationDirection, transform.forward) < -.1f ? -1 : 1;
            animator.SetFloat("Move", animationDirection.magnitude * sign);
        }
    }

    void Shape(Vector3 position, bool inset, out Vector3 a, out Vector3 b, out float radius) {
        Vector3 scale = transform.lossyScale;
        int axis = capsule.direction;
        float along = Mathf.Abs(scale[axis]);
        float across = Mathf.Max(Mathf.Abs(scale[(axis + 1) % 3]), Mathf.Abs(scale[(axis + 2) % 3]));
        float realRadius = capsule.radius * across;
        radius = inset ? Mathf.Max(.01f, realRadius - skinWidth) : realRadius;
        Vector3 localAxis = axis == 0 ? Vector3.right : axis == 1 ? Vector3.up : Vector3.forward;
        Vector3 worldAxis = body.rotation * localAxis;
        Vector3 center = position + body.rotation * Vector3.Scale(capsule.center, scale);
        float half = Mathf.Max(0, capsule.height * along * .5f - realRadius);
        a = center + worldAxis * half;
        b = center - worldAxis * half;
    }

    bool IsSelf(Collider other) {
        return !other || other.attachedRigidbody == body || other.transform.IsChildOf(transform) ||
            Physics.GetIgnoreCollision(capsule, other);
    }
    bool BlockingHit(Vector3 position, Vector3 delta, out RaycastHit closest) {
        closest = default;
        float distance = delta.magnitude;
        if (distance < .00001f) return false;
        Vector3 direction = delta / distance;
        Shape(position, true, out Vector3 a, out Vector3 b, out float radius);
        int count = Physics.CapsuleCastNonAlloc(a, b, radius, direction, castBuffer,
            distance + skinWidth, solidMask, QueryTriggerInteraction.Ignore);
        RaycastHit[] hits = castBuffer;
        if (count == castBuffer.Length) {
            // Dense crowds must not hide an obstacle due to a truncated NonAlloc query.
            hits = Physics.CapsuleCastAll(a, b, radius, direction, distance + skinWidth,
                solidMask, QueryTriggerInteraction.Ignore);
            count = hits.Length;
        }
        bool found = false;
        float nearest = float.PositiveInfinity;
        for (int i = 0; i < count; i++) {
            RaycastHit hit = hits[i];
            if (IsSelf(hit.collider) || Vector3.Dot(direction, hit.normal) >= -.0001f) continue;
            if (hit.distance < nearest) { closest = hit; nearest = hit.distance; found = true; }
        }
        return found;
    }

    Vector3 RecoverOverlap(Vector3 position) {
        if (maxOverlapRecovery <= 0) return position;
        Shape(position, false, out Vector3 a, out Vector3 b, out float radius);
        int count = Physics.OverlapCapsuleNonAlloc(a, b, radius, overlapBuffer,
            solidMask, QueryTriggerInteraction.Ignore);
        Collider[] others = overlapBuffer;
        if (count == overlapBuffer.Length) {
            others = Physics.OverlapCapsule(a, b, radius, solidMask, QueryTriggerInteraction.Ignore);
            count = others.Length;
        }
        Vector3 original = position;
        float walkable = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        for (int i = 0; i < count; i++) {
            Collider other = others[i];
            if (IsSelf(other)) continue;
            if (!Physics.ComputePenetration(capsule, position, body.rotation, other,
                other.transform.position, other.transform.rotation, out Vector3 normal, out float depth)) continue;
            // Ground contact is handled by the Rigidbody; do not turn it into a vertical pop.
            if (normal.y >= walkable || depth <= .001f) continue;
            Vector3 horizontal = Vector3.ProjectOnPlane(normal, Vector3.up);
            if (horizontal.sqrMagnitude < .0001f) continue;
            float budget = maxOverlapRecovery - Vector3.Distance(original, position);
            if (budget <= 0) break;
            position += horizontal.normalized * Mathf.Min(depth + .001f, budget);
        }
        return position;
    }

    bool TryStep(Vector3 position, Vector3 remaining, out Vector3 stepped) {
        stepped = position;
        if (stepHeight <= 0 || (dash && dash.isDashing)) return false;
        float walkable = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        if (!BlockingHit(position, Vector3.down * .1f, out RaycastHit ground) || ground.normal.y < walkable) return false;
        Vector3 lift = Vector3.up * (stepHeight + skinWidth);
        if (BlockingHit(position, lift, out _)) return false;
        Vector3 raised = position + lift;
        if (BlockingHit(raised, remaining, out _)) return false;
        raised += remaining;
        if (!BlockingHit(raised, Vector3.down * (stepHeight + .1f), out RaycastHit landing) ||
            landing.normal.y < walkable) return false;
        Vector3 candidate = raised + Vector3.down * Mathf.Max(0, landing.distance - skinWidth);
        float rise = candidate.y - position.y;
        if (rise <= skinWidth || rise > stepHeight + .005f) return false;
        stepped = candidate;
        return true;
    }

    Vector3 ResolveMovement(Vector3 position, Vector3 delta) {
        position = RecoverOverlap(position);
        Vector3 remaining = delta;
        Vector3 previousNormal = Vector3.zero;
        float walkable = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        for (int i = 0; i < slideIterations && remaining.sqrMagnitude > .0000001f; i++) {
            if (!BlockingHit(position, remaining, out RaycastHit hit)) { position += remaining; break; }
            float travel = Mathf.Clamp(hit.distance - skinWidth, 0, remaining.magnitude);
            Vector3 advance = remaining.normalized * travel;
            position += advance; remaining -= advance;
            if (i == 0 && hit.normal.y < walkable && TryStep(position, remaining, out Vector3 stepped)) return stepped;
            Vector3 normal = hit.normal;
            if (normal.y >= 0 && normal.y < walkable)
                normal = Vector3.ProjectOnPlane(normal, Vector3.up).normalized;
            Vector3 sliding = Vector3.ProjectOnPlane(remaining, normal);
            if (previousNormal != Vector3.zero && Vector3.Dot(sliding, previousNormal) < -.0001f) {
                Vector3 crease = Vector3.Cross(previousNormal, normal);
                sliding = crease.sqrMagnitude > .0001f ? Vector3.Project(remaining, crease.normalized) : Vector3.zero;
            }
            previousNormal = normal;
            remaining = sliding;
        }
        return position;
    }

    public Vector3 GetMoveDirection(Vector2 axes) {
        Transform basis = aim && aim.cameraTarget ? aim.cameraTarget : transform;
        Vector3 forward = Vector3.ProjectOnPlane(basis.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        return Vector3.ClampMagnitude(forward * axes.y + right * axes.x, 1);
    }
}
