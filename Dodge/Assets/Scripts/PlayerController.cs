using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어 이동, 피해 처리, 사망 처리
public class PlayerController : MonoBehaviour
{
    public Rigidbody playerRigidbody;
    public float speed = 8f; // 이동 속력
    public float maxHealth = 1f; // 최대 체력 (1 = 탄알 한 발에 사망)

    private float currentHealth; // 현재 체력
    private ShieldController shield; // 피해 감소용 쉴드 (없어도 동작)
    private Renderer playerRenderer; // Hit Flash 대상

    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        shield = GetComponent<ShieldController>();
        playerRenderer = GetComponent<Renderer>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        // 일시정지 중에는 이동 입력을 받지 않음
        if (PauseManager.IsPaused) return;

        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        float xSpeed = xInput * speed;
        float zSpeed = zInput * speed;

        // 입력 방향으로 속도 적용 (Y축 이동 없음)
        Vector3 newVelocity = new Vector3(xSpeed, 0f, zSpeed);
        playerRigidbody.linearVelocity = newVelocity;
    }

    // 피해 처리: 쉴드가 활성화되어 있으면 감소된 피해만 적용
    public void TakeDamage(float damage) {
        TakeDamage(damage, transform.position);
    }

    // hitPoint: 탄알이 실제로 맞은 위치 (피격 이펙트 생성 위치)
    public void TakeDamage(float damage, Vector3 hitPoint) {
        // 이미 사망한 상태면 무시
        if (!gameObject.activeSelf) return;

        bool shieldActive = shield != null && shield.IsShieldActive;
        if (shield != null) {
            damage = shield.ModifyDamage(damage);
        }
        if (damage <= 0f) {
            // 쉴드가 피해를 완전히 막은 경우 쉴드 피격 이펙트만 재생
            if (shieldActive && HitEffectManager.Instance != null) {
                HitEffectManager.Instance.PlayShieldHit(hitPoint, transform.position, shield.shieldEffect);
            }
            return;
        }

        currentHealth -= damage;

        // 피격 이펙트 재생 (Die()로 비활성화되기 전에 호출)
        if (HitEffectManager.Instance != null) {
            HitEffectManager.Instance.PlayDamageHit(hitPoint, transform.position, playerRenderer);
        }

        if (currentHealth <= 0f) {
            Die();
        }
    }

    // 플레이어를 비활성화하고 게임오버 처리
    public void Die() {
        gameObject.SetActive(false);

        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null) {
            gameManager.EndGame();
        }
    }
}
