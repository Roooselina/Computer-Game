using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerDash : MonoBehaviour {
    public float speed = 18f, duration = .18f, cooldown = 1.8f, invulnerabilityDuration = .12f;
    public Vector3 direction { get; private set; }
    public bool isDashing => CanAct && Time.time < dashEnds;
    public bool invulnerable => isDashing && Time.time < immuneEnds;
    public float cooldownRemaining => Mathf.Max(0f, readyAt - Time.time);
    public bool ready => CanAct && cooldownRemaining <= 0;
    float dashEnds, immuneEnds, readyAt;
    PlayerInput input;
    PlayerMovement movement;
    PlayerHealth health;
    void Awake() {
        input = GetComponent<PlayerInput>(); movement = GetComponent<PlayerMovement>(); health = GetComponent<PlayerHealth>();
    }
    bool CanAct => Time.timeScale > 0 && health && !health.dead &&
        !(GameManager.instance != null && GameManager.instance.isGameover);
    void Update() {
        if (input.dash) TryDash();
    }
    public bool TryDash() {
        if (!ready || isDashing) return false;
        var grenades = GetComponent<PlayerGrenades>();
        if (grenades && grenades.isThrowing) return false;
        Vector3 requested = movement.GetMoveDirection(input.movement);
        direction = requested.sqrMagnitude > .01f ? requested.normalized : transform.forward;
        direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        dashEnds = Time.time + duration; immuneEnds = Time.time + Mathf.Min(duration, invulnerabilityDuration);
        readyAt = Time.time + cooldown;
        return true;
    }
}
