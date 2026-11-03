using UnityEngine;

// 물 위에 떠서 출렁이는 통나무. 밟으면 살짝 가라앉았다가 떠오른다.
public class FloatingLog : MonoBehaviour {
    public float bobHeight = 0.12f;
    public float sinkDepth = 0.32f;

    private Rigidbody2D body;
    private float baseY;
    private float phase;
    private float sink;
    private float tilt;
    private bool playerOn;

    private void Start() {
        body = GetComponent<Rigidbody2D>();
        baseY = transform.position.y;
        phase = transform.position.x;
    }

    private void FixedUpdate() {
        sink = Mathf.Lerp(sink, playerOn ? sinkDepth : 0f, Time.fixedDeltaTime * (playerOn ? 10f : 3f));
        float targetY = baseY + Mathf.Sin(Time.time * 2.2f + phase) * bobHeight - sink;
        float vy = (targetY - body.position.y) / Time.fixedDeltaTime;
        body.linearVelocity = new Vector2(0f, vy);
        tilt = Mathf.Lerp(tilt, Mathf.Sin(Time.time * 1.7f + phase) * 3f, Time.fixedDeltaTime * 4f);
        body.MoveRotation(tilt);
        playerOn = false;
    }

    // 플레이어가 위에 서 있는 동안 매 물리 프레임 호출
    public void Stand() {
        if (!playerOn && sink < 0.05f) {
            Fx.instance.Splash(new Vector2(transform.position.x, baseY), new Color(1f, 1f, 1f, 0.9f), 10);
        }
        playerOn = true;
    }
}
