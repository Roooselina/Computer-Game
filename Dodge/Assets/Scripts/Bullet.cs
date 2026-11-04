using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어를 향해 날아가는 탄알
public class Bullet : MonoBehaviour
{
    public float speed = 8f; // 탄알 이동 속력
    public float damage = 1f; // 플레이어에게 주는 피해량
    private Rigidbody bulletRigidbody;

    void Start()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
        // 정면 방향으로 발사
        bulletRigidbody.linearVelocity = transform.forward * speed;
        // 3초 뒤 자동 파괴
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") {
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerController != null) {
                // 트리거라 접촉점이 없으므로 플레이어 콜라이더에서 탄알과 가장 가까운 점을 피격 위치로 사용
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                playerController.TakeDamage(damage, hitPoint);

                // 쉴드로 막아서 플레이어가 살아남았다면 탄알 제거
                if (playerController.gameObject.activeSelf) {
                    Destroy(gameObject);
                }
            }
        }
    }
}
