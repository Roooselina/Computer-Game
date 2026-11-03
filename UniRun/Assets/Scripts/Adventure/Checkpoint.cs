using UnityEngine;

// 구역 시작 체크포인트 깃발과 결승 깃발
public class Checkpoint : MonoBehaviour {
    public bool isGoal;
    public Sprite raisedSprite;

    private bool reached;

    private void OnTriggerEnter2D(Collider2D other) {
        if (reached || !other.CompareTag("Player")) return;
        reached = true;
        if (raisedSprite != null) GetComponent<SpriteRenderer>().sprite = raisedSprite;
        if (isGoal) RunGame.instance.OnGoal(transform.position);
        else RunGame.instance.OnCheckpoint(transform.position);
    }
}
