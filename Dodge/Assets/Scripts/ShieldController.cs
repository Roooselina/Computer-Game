using UnityEngine;

// 플레이어 쉴드: 활성화 시 일정 시간 동안 받는 피해를 감소/차단하고, 종료 후 쿨다운을 관리
public class ShieldController : MonoBehaviour
{
    public KeyCode shieldKey = KeyCode.Space; // 쉴드 사용 키 (이동/R 재시작/ESC 일시정지와 겹치지 않도록)

    [Min(0f)] public float shieldDuration = 3f; // 쉴드 지속 시간(초)
    [Min(0f)] public float shieldCooldown = 5f; // 쉴드 종료 후 재사용 대기 시간(초)
    [Range(0f, 1f)] public float shieldDamageReduction = 1f; // 피해 감소율 (1 = 완전 차단, 0.5 = 절반)
    public GameObject shieldEffect; // 쉴드 활성화 중에만 켜지는 시각 효과

    private float remainingDuration; // 남은 쉴드 지속 시간
    private float remainingCooldown; // 남은 쿨다운 시간

    public bool IsShieldActive { get { return remainingDuration > 0f; } }
    public bool IsReady { get { return !IsShieldActive && remainingCooldown <= 0f; } }
    public float RemainingDuration { get { return remainingDuration; } }
    public float RemainingCooldown { get { return remainingCooldown; } }

    void Start()
    {
        // 쉴드와 시각 효과를 꺼진 상태로 초기화
        remainingDuration = 0f;
        remainingCooldown = 0f;
        SetEffect(false);
    }

    void Update()
    {
        // 일시정지 중에는 입력을 받지 않음 (타이머는 Time.deltaTime이 0이라 자동으로 멈춤)
        if (PauseManager.IsPaused) return;

        if (Input.GetKeyDown(shieldKey))
        {
            TryActivateShield();
        }

        if (IsShieldActive)
        {
            remainingDuration -= Time.deltaTime;
            if (remainingDuration <= 0f)
            {
                EndShield();
            }
        }
        else if (remainingCooldown > 0f)
        {
            remainingCooldown = Mathf.Max(0f, remainingCooldown - Time.deltaTime);
        }
    }

    // 쉴드 사용 가능 상태라면 쉴드를 활성화
    public bool TryActivateShield()
    {
        if (!IsReady || shieldDuration <= 0f) return false;

        remainingDuration = shieldDuration;
        SetEffect(true);
        return true;
    }

    // 쉴드 종료: 시각 효과 제거 후 쿨다운 시작
    private void EndShield()
    {
        remainingDuration = 0f;
        remainingCooldown = shieldCooldown;
        SetEffect(false);
    }

    // 들어오는 피해에 쉴드 감소율을 적용한 최종 피해를 반환
    public float ModifyDamage(float damage)
    {
        if (!IsShieldActive) return damage;
        return damage * (1f - shieldDamageReduction);
    }

    private void SetEffect(bool active)
    {
        if (shieldEffect != null)
        {
            shieldEffect.SetActive(active);
        }
    }

    void OnDisable()
    {
        // 플레이어가 비활성화(사망)될 때 시각 효과가 남지 않도록 정리
        SetEffect(false);
    }
}
