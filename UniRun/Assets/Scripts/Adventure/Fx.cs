using System.Collections.Generic;
using UnityEngine;

// 파티클 없이 스프라이트 풀로 만드는 가벼운 이펙트 (먼지, 물보라, 반짝이, 링, 떠오르는 글자)
public class Fx : MonoBehaviour {
    public static Fx instance;

    public Sprite circle;
    public Sprite spark;
    public Font font;

    private class Bit {
        public SpriteRenderer sr;
        public Vector2 velocity;
        public float life, maxLife, gravity, size, grow, spin;
        public Color color;
    }

    private class Label {
        public TextMesh text;
        public MeshRenderer renderer;
        public float life, maxLife;
        public Vector3 start;
    }

    private readonly List<Bit> active = new List<Bit>();
    private readonly Stack<Bit> pool = new Stack<Bit>();
    private readonly List<Label> labels = new List<Label>();

    private void Awake() {
        instance = this;
    }

    private Bit Spawn(Sprite s, Vector2 pos, Color c, Vector2 vel, float size, float life, float gravity, int order = 25) {
        Bit b = pool.Count > 0 ? pool.Pop() : null;
        if (b == null) {
            var go = new GameObject("fx");
            go.transform.SetParent(transform, false);
            b = new Bit { sr = go.AddComponent<SpriteRenderer>() };
        }
        b.sr.gameObject.SetActive(true);
        b.sr.sprite = s;
        b.sr.sortingOrder = order;
        b.sr.transform.position = pos;
        b.sr.transform.localRotation = Quaternion.identity;
        b.velocity = vel; b.size = size; b.life = b.maxLife = life; b.gravity = gravity; b.color = c; b.grow = 0f; b.spin = 0f;
        active.Add(b);
        return b;
    }

    private void Update() {
        float dt = Time.deltaTime;
        for (int i = active.Count - 1; i >= 0; i--) {
            Bit b = active[i];
            b.life -= dt;
            if (b.life <= 0f) {
                b.sr.gameObject.SetActive(false);
                pool.Push(b);
                active.RemoveAt(i);
                continue;
            }
            float t = b.life / b.maxLife;
            b.velocity += Vector2.down * b.gravity * dt;
            b.velocity *= 1f - 2f * dt;
            Transform tr = b.sr.transform;
            tr.position += (Vector3)(b.velocity * dt);
            tr.Rotate(0, 0, b.spin * dt);
            float s = b.size * (b.grow > 0f ? Mathf.Lerp(b.grow, 1f, t) : Mathf.Lerp(0.2f, 1f, t));
            tr.localScale = new Vector3(s, s, 1f);
            b.sr.color = new Color(b.color.r, b.color.g, b.color.b, b.color.a * Mathf.Clamp01(t * 1.8f));
        }
        for (int i = labels.Count - 1; i >= 0; i--) {
            Label l = labels[i];
            l.life -= Time.unscaledDeltaTime;
            if (l.life <= 0f) { Destroy(l.text.gameObject); labels.RemoveAt(i); continue; }
            float k = 1f - l.life / l.maxLife;
            float pop = k < 0.15f ? Mathf.Lerp(0.5f, 1.25f, k / 0.15f) : Mathf.Lerp(1.25f, 1f, Mathf.Clamp01((k - 0.15f) * 4f));
            l.text.transform.position = l.start + Vector3.up * (k * 1.5f);
            l.text.transform.localScale = Vector3.one * pop;
            Color c = l.text.color; c.a = Mathf.Clamp01(l.life / l.maxLife * 3f); l.text.color = c;
        }
    }

    // ---------------------------------------------------------------- 이펙트 종류
    public void Dust(Vector2 pos, int count, float power) {
        for (int i = 0; i < count; i++) {
            Vector2 v = new Vector2(Random.Range(-1f, 1f) * power, Random.Range(0.2f, 1f) * power * 0.5f);
            Spawn(circle, pos + Vector2.right * Random.Range(-0.3f, 0.3f), new Color(1f, 1f, 1f, 0.55f), v, Random.Range(0.18f, 0.32f), Random.Range(0.3f, 0.5f), -1f, 11);
        }
    }

    public void Splash(Vector2 pos, Color c, int count) {
        for (int i = 0; i < count; i++) {
            Vector2 v = new Vector2(Random.Range(-3f, 3f), Random.Range(4f, 9f));
            Spawn(circle, pos + Vector2.right * Random.Range(-0.4f, 0.4f), new Color(c.r, c.g, c.b, 0.85f), v, Random.Range(0.12f, 0.24f), Random.Range(0.4f, 0.7f), 22f);
        }
    }

    public void Bubbles(Vector2 pos, int count) {
        for (int i = 0; i < count; i++) {
            Bit b = Spawn(circle, pos + Random.insideUnitCircle * 0.4f, new Color(1f, 1f, 1f, 0.6f), new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(1f, 2.5f)), Random.Range(0.08f, 0.16f), Random.Range(0.6f, 1.1f), -2f, 24);
            b.grow = 0.6f;
        }
    }

    public void Sparkle(Vector2 pos, Color c, int count) {
        for (int i = 0; i < count; i++) {
            Bit b = Spawn(spark, pos, c, Random.insideUnitCircle.normalized * Random.Range(2f, 5f), Random.Range(0.25f, 0.45f), Random.Range(0.25f, 0.45f), 0f);
            b.spin = Random.Range(-360f, 360f);
        }
    }

    public void Burst(Vector2 pos, Color c, int count, float speed, float size, float life, float gravity) {
        for (int i = 0; i < count; i++) {
            Sprite s = i % 3 == 0 ? spark : circle;
            Bit b = Spawn(s, pos, c, Random.insideUnitCircle.normalized * Random.Range(speed * 0.3f, speed), size * Random.Range(0.6f, 1.4f), life * Random.Range(0.6f, 1.2f), -gravity);
            b.spin = Random.Range(-500f, 500f);
        }
    }

    public void Ring(Vector2 pos, Color c) {
        const int n = 16;
        for (int i = 0; i < n; i++) {
            float a = i * Mathf.PI * 2f / n;
            Spawn(circle, pos, c, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 6f, 0.2f, 0.3f, 0f);
        }
    }

    public void Popup(string text, Vector2 pos, Color c, float life) {
        var go = new GameObject("popup");
        go.transform.position = pos;
        var tm = go.AddComponent<TextMesh>();
        tm.font = font;
        tm.text = text;
        tm.fontSize = 64;
        tm.characterSize = 0.08f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = c;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = font.material;
        mr.sortingOrder = 40;
        labels.Add(new Label { text = tm, renderer = mr, life = life, maxLife = life, start = pos });
    }
}
