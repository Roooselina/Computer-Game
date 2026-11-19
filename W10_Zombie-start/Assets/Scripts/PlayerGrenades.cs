using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerGrenades : MonoBehaviour {
    public Grenade grenadePrefab;
    public Transform throwOrigin;
    public int startingCount = 3, maxCount = 9;
    public float throwSpeed = 14f;
    public int count { get; private set; }
    public bool isThrowing { get; private set; }
    public float throwProgress => isThrowing ? Mathf.Clamp01((Time.time - throwStart) / .5f) : 0;
    float throwStart, nextThrow;
    bool released;
    PlayerInput input;
    PlayerAim aim;
    void Awake() { input = GetComponent<PlayerInput>(); aim = GetComponent<PlayerAim>(); count = startingCount; }
    bool CanAct => Time.timeScale > 0 && !(GameManager.instance != null && GameManager.instance.isGameover) &&
        !GetComponent<PlayerHealth>().dead;
    void Update() {
        if (!CanAct) { isThrowing = false; return; }
        if (input.grenade) TryThrow();
        if (!isThrowing) return;
        if (!released && Time.time >= throwStart + .2f) { released = true; Release(); }
        if (Time.time >= throwStart + .5f) isThrowing = false;
    }
    public bool TryThrow() {
        if (!CanAct || count <= 0 || !grenadePrefab || !throwOrigin || isThrowing || Time.time < nextThrow) return false;
        var shooter = GetComponent<PlayerShooter>();
        if (shooter && shooter.gun && shooter.gun.state == Gun.State.Reloading) return false;
        var dash = GetComponent<PlayerDash>();
        if (dash && dash.isDashing) return false;
        isThrowing = true; released = false; throwStart = Time.time; nextThrow = Time.time + .65f;
        return true;
    }
    void Release() {
        if (!CanAct || count <= 0) return;
        Vector3 shoulder = transform.position + Vector3.up * 1.35f;
        Vector3 position = throwOrigin.position;
        Vector3 offset = position - shoulder;
        int mask = aim ? (int)aim.hitMask : ~0;
        if (Physics.SphereCast(shoulder, .13f, offset.normalized, out RaycastHit hit,
                offset.magnitude, mask, QueryTriggerInteraction.Ignore))
            position = shoulder + offset.normalized * Mathf.Max(0, hit.distance - .05f);
        if (Physics.CheckSphere(position, .12f, mask, QueryTriggerInteraction.Ignore)) return;
        Vector3 direction = aim ? (aim.aimPoint - position).normalized : transform.forward;
        Grenade grenade = Instantiate(grenadePrefab, position, Quaternion.identity);
        grenade.Launch(gameObject, (direction + Vector3.up * .35f).normalized * throwSpeed);
        count--;
    }
    public void AddGrenades(int amount) { count = Mathf.Clamp(count + Mathf.Max(0, amount), 0, maxCount); }
}
