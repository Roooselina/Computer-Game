using UnityEngine;

// 보석, 하트(추가 목숨), 실드, 자석
public class Pickup : MonoBehaviour {
    public enum Kind { Gem, Heart, Shield, Magnet }

    public Kind kind;
    public Sprite glow;

    private Vector3 basePos;
    private float phase;
    private bool collected;
    private SpriteRenderer glowRenderer;

    private void Start() {
        basePos = transform.position;
        phase = basePos.x * 0.7f;
        if (kind != Kind.Gem && glow != null) {
            // 귀한 아이템은 뒤에서 빛나게 해서 멀리서도 보이게 한다
            var g = new GameObject("Glow");
            g.transform.SetParent(transform, false);
            g.transform.localScale = Vector3.one * 2.2f;
            glowRenderer = g.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = glow;
            glowRenderer.sortingOrder = 7;
            glowRenderer.color = GlowColor();
        }
    }

    private Color GlowColor() {
        switch (kind) {
            case Kind.Heart: return new Color(1f, 0.3f, 0.45f, 0.45f);
            case Kind.Shield: return new Color(0.4f, 0.85f, 1f, 0.45f);
            default: return new Color(1f, 0.9f, 0.3f, 0.45f);
        }
    }

    private void Update() {
        if (collected) return;
        RunnerPlayer p = RunnerPlayer.instance;
        if (kind == Kind.Gem && p != null && p.MagnetActive) {
            Vector3 to = p.transform.position - transform.position;
            if (to.sqrMagnitude < 36f) {
                basePos = Vector3.MoveTowards(transform.position, p.transform.position, Time.deltaTime * 18f);
                transform.position = basePos;
                return;
            }
        }
        float bob = Mathf.Sin(Time.time * 3f + phase) * (kind == Kind.Gem ? 0.08f : 0.18f);
        transform.position = basePos + Vector3.up * bob;
        if (kind != Kind.Gem) {
            float s = 1f + Mathf.Sin(Time.time * 5f + phase) * 0.08f;
            transform.localScale = Vector3.one * s;
            if (glowRenderer != null) glowRenderer.transform.localRotation = Quaternion.Euler(0, 0, Time.time * 40f);
        } else {
            transform.localRotation = Quaternion.Euler(0, Mathf.Sin(Time.time * 2f + phase) * 50f, 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;
        RunGame.instance.OnPickup(this);
        Destroy(gameObject);
    }
}
