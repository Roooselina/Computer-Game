using UnityEngine;

// 가시 함정. 배경에 묻히지 않도록 붉은 경고 빛을 깜빡이고, 판정은 그림보다 작다.
public class Spike : MonoBehaviour {
    public Sprite glow;

    private SpriteRenderer glowRenderer;

    private void Start() {
        GetComponent<SpriteRenderer>().color = new Color(1f, 0.82f, 0.82f);
        if (glow == null) return;
        var g = new GameObject("Warning");
        g.transform.SetParent(transform, false);
        g.transform.localPosition = new Vector3(0, -0.1f, 0);
        g.transform.localScale = new Vector3(1.6f, 0.9f, 1f);
        glowRenderer = g.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = glow;
        glowRenderer.sortingOrder = 6;
    }

    private void Update() {
        if (glowRenderer == null) return;
        float a = 0.25f + 0.2f * Mathf.Sin(Time.time * 6f + transform.position.x);
        glowRenderer.color = new Color(1f, 0.15f, 0.1f, a);
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("Player")) RunGame.instance.HurtPlayer(transform.position, false);
    }
}
