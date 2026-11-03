using UnityEngine;

// 풀과 꽃이 바람에 살랑이는 연출. 플레이어가 지나가면 크게 흔들린다.
public class Sway : MonoBehaviour {
    private float phase;
    private float kick;
    private Vector3 baseScale;

    private void Start() {
        phase = Random.value * 10f;
        baseScale = transform.localScale;
    }

    private void Update() {
        RunnerPlayer p = RunnerPlayer.instance;
        if (p != null && Mathf.Abs(p.transform.position.x - transform.position.x) < 0.6f
            && Mathf.Abs(p.transform.position.y - transform.position.y) < 1.2f) {
            kick = 1f;
        }
        kick = Mathf.MoveTowards(kick, 0f, Time.deltaTime * 1.5f);
        float angle = Mathf.Sin(Time.time * 2f + phase) * 4f + Mathf.Sin(Time.time * 18f) * 14f * kick;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        transform.localScale = new Vector3(baseScale.x, baseScale.y * (1f - 0.12f * kick), baseScale.z);
    }
}
