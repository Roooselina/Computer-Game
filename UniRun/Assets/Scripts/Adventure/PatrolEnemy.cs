using UnityEngine;

// 순찰하는 적. 좁은 발판 위에서도 계속 움직여서 착지할 틈을 만들어 준다.
public class PatrolEnemy : MonoBehaviour {
    public enum Mode { Walk, Spiky, Fly, Float, Swim }

    public Mode mode;
    public float minX;
    public float maxX;
    public Sprite[] frames;
    public Sprite deadSprite;

    public bool Stompable { get { return mode == Mode.Walk || mode == Mode.Fly || mode == Mode.Swim; } }
    public bool Dead { get; private set; }

    private SpriteRenderer sr;
    private Vector3 basePos;
    private float dir = -1f;
    private float speed;
    private float phase;
    private float frameTimer;
    private int frame;
    private Vector2 deadVelocity;

    private void Start() {
        sr = GetComponent<SpriteRenderer>();
        basePos = transform.position;
        phase = Random.value * 6f;
        float range = Mathf.Max(0.1f, maxX - minX);
        switch (mode) {
            // 좁은 곳일수록 빨리 오가서 빈자리가 자주 생긴다
            case Mode.Walk: speed = range < 3f ? 1.8f : 1.3f; break;
            case Mode.Spiky: speed = 0.9f; break;
            case Mode.Fly: speed = 2.2f; break;
            case Mode.Float: speed = 0f; break;
            default: speed = 1.6f; break;
        }
        if (maxX <= minX) { minX = basePos.x; maxX = basePos.x; speed = 0f; }
    }

    private void Update() {
        if (Dead) {
            deadVelocity += Vector2.down * 30f * Time.deltaTime;
            transform.position += (Vector3)(deadVelocity * Time.deltaTime);
            transform.Rotate(0, 0, 360f * Time.deltaTime);
            sr.color = new Color(1, 1, 1, Mathf.MoveTowards(sr.color.a, 0f, Time.deltaTime * 1.2f));
            if (sr.color.a <= 0f) Destroy(gameObject);
            return;
        }
        if (RunGame.instance != null && RunGame.instance.Frozen) return;

        Vector3 p = transform.position;
        if (speed > 0f) {
            p.x += dir * speed * Time.deltaTime;
            if (p.x < minX) { p.x = minX; dir = 1f; }
            if (p.x > maxX) { p.x = maxX; dir = -1f; }
            sr.flipX = dir > 0f;
        }
        switch (mode) {
            case Mode.Fly: p.y = basePos.y + Mathf.Sin(Time.time * 2.5f + phase) * 0.6f; break;
            case Mode.Float: p.y = basePos.y + Mathf.Sin(Time.time * 1.6f + phase) * 1.4f; break;
            case Mode.Swim: p.y = basePos.y + Mathf.Sin(Time.time * 1.8f + phase) * 0.35f; break;
        }
        transform.position = p;
        if (mode == Mode.Float) transform.localRotation = Quaternion.Euler(0, 0, Time.time * 90f);

        frameTimer += Time.deltaTime;
        if (frameTimer > 0.14f && frames.Length > 0 && frames[0] != null) {
            frameTimer = 0f;
            frame = (frame + 1) % 4;
            int f = frame == 3 ? 1 : frame;   // 1-2-3-2 왕복
            if (frames[f] != null) sr.sprite = frames[f];
        }
    }

    public void Kill(Vector2 hitFrom) {
        if (Dead) return;
        Dead = true;
        GetComponent<Collider2D>().enabled = false;
        if (deadSprite != null) sr.sprite = deadSprite;
        deadVelocity = new Vector2(Mathf.Sign(transform.position.x - hitFrom.x) * 3f, 9f);
        sr.sortingOrder = 30;
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (Dead || !other.CompareTag("Player")) return;
        RunGame.instance.OnEnemyContact(this);
    }
}
