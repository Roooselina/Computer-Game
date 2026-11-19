using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class Grenade : MonoBehaviour {
    public float fuse = 2.2f, radius = 5f, damage = 150f;
    public GameObject explosionPrefab;
    public LayerMask damageMask = ~0;
    public LayerMask obstructionMask = ~0;
    float detonateAt;
    bool exploded;
    GameObject owner;
    void Awake() { detonateAt = Time.time + fuse; }
    public void Launch(GameObject source, Vector3 velocity) {
        owner = source;
        foreach (Collider own in GetComponentsInChildren<Collider>())
            foreach (Collider other in source.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(own, other);
        Rigidbody body = GetComponent<Rigidbody>();
        body.linearVelocity = velocity; body.angularVelocity = Random.insideUnitSphere * 8f;
    }
    void Update() {
        if (GameManager.instance != null && GameManager.instance.isGameover) { Destroy(gameObject); return; }
        if (Time.time >= detonateAt) Explode();
    }
    public void Explode() {
        if (exploded) return;
        exploded = true;
        if (GameManager.instance != null && GameManager.instance.isGameover) { Destroy(gameObject); return; }
        Vector3 center = transform.position;
        var targets = new Dictionary<Zombie, Vector3>();
        foreach (Collider col in Physics.OverlapSphere(center, radius, damageMask, QueryTriggerInteraction.Ignore)) {
            Zombie zombie = col.GetComponentInParent<Zombie>();
            if (!zombie || zombie.dead || (owner && zombie.transform.IsChildOf(owner.transform))) continue;
            Vector3 point = col.ClosestPoint(center);
            if (targets.TryGetValue(zombie, out Vector3 previous) &&
                (previous - center).sqrMagnitude <= (point - center).sqrMagnitude) continue;
            targets[zombie] = point;
        }
        foreach (var pair in targets) {
            Vector3 delta = pair.Value - center;
            float distance = delta.magnitude;
            // Environment shields targets; another zombie does not absorb a blast.
            bool blocked = false;
            foreach (RaycastHit hit in Physics.RaycastAll(center + Vector3.up * .03f,
                distance > .001f ? delta / distance : Vector3.up, distance, obstructionMask, QueryTriggerInteraction.Ignore)) {
                if (hit.collider.transform.IsChildOf(transform) || hit.collider.GetComponentInParent<LivingEntity>()) continue;
                blocked = true; break;
            }
            if (blocked) continue;
            Vector3 normal = distance > .001f ? -delta / distance : Vector3.up;
            ((IDamageable)pair.Key).OnDamage(damage * Mathf.Clamp01(1f - distance / radius), pair.Value, normal);
        }
        if (explosionPrefab) {
            GameObject effect = Instantiate(explosionPrefab, center, Quaternion.identity);
            Destroy(effect, 4f);
        }
        Destroy(gameObject);
    }
}
