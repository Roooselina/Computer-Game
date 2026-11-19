using System.Collections;
using UnityEngine;
using UnityEngine.AI; // AI, 내비게이션 시스템 관련 코드 가져오기

// 좀비 AI 구현
// (교재 16.6 Enemy(Zombie) 스크립트 p.81~90 원문 기반,
//  컴포넌트/참조 누락 시 원인을 알 수 있도록 null 검사와 로그만 추가)
public class Zombie : LivingEntity
{
    public LayerMask whatIsTarget; // 추적 대상 레이어

    private LivingEntity targetEntity; // 추적 대상
    private NavMeshAgent navMeshAgent; // 경로 계산 AI 에이전트

    public ParticleSystem hitEffect; // 피격 시 재생할 파티클 효과
    public AudioClip deathSound; // 사망 시 재생할 소리
    public AudioClip hitSound; // 피격 시 재생할 소리

    private Animator zombieAnimator; // 애니메이터 컴포넌트
    private AudioSource zombieAudioPlayer; // 오디오 소스 컴포넌트
    private Renderer zombieRenderer; // 렌더러 컴포넌트

    public float damage = 20f; // 공격력
    public float timeBetAttack = 0.5f; // 공격 간격
    private float lastAttackTime; // 마지막 공격 시점

    // 추적할 대상이 존재하는지 알려주는 프로퍼티
    private bool hasTarget {
        get
        {
            // 추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
            if (targetEntity != null && !targetEntity.dead)
            {
                return true;
            }

            // 그렇지 않다면 false
            return false;
        }
    }

    private void Awake() {
        // 게임 오브젝트에서 사용할 컴포넌트 가져오기
        navMeshAgent = GetComponent<NavMeshAgent>();
        zombieAnimator = GetComponent<Animator>();
        zombieAudioPlayer = GetComponent<AudioSource>();

        // 렌더러 컴포넌트는 자식 게임 오브젝트에 있으므로
        // GetComponentInChildren() 메서드 사용
        zombieRenderer = GetComponentInChildren<Renderer>();

        // 참조 누락 검사 (원인 파악용 로그)
        if (navMeshAgent == null) Debug.LogError("[Zombie] NavMeshAgent 컴포넌트가 " + name + "에 없습니다.", this);
        if (zombieAnimator == null) Debug.LogError("[Zombie] Animator 컴포넌트가 " + name + "에 없습니다.", this);
        if (zombieAudioPlayer == null) Debug.LogError("[Zombie] AudioSource 컴포넌트가 " + name + "에 없습니다.", this);
        if (zombieRenderer == null) Debug.LogError("[Zombie] 자식 오브젝트에서 Renderer를 찾지 못했습니다.", this);
        if (hitEffect == null) Debug.LogWarning("[Zombie] Hit Effect 필드가 비어 있습니다. 자식 BloodSprayEffect를 할당하세요.", this);
        if (whatIsTarget.value == 0) Debug.LogWarning("[Zombie] What Is Target 레이어 마스크가 Nothing입니다. Player 레이어를 지정하세요.", this);
    }

    // 좀비 AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(ZombieData zombieData) {
        if (zombieData == null)
        {
            Debug.LogError("[Zombie] Setup()에 전달된 ZombieData가 null입니다.", this);
            return;
        }

        // 체력 설정
        startingHealth = zombieData.health;
        health = zombieData.health;
        // 공격력 설정
        damage = zombieData.damage;
        // 내비메시 에이전트의 이동 속도 설정
        if (navMeshAgent != null) navMeshAgent.speed = zombieData.speed;
        // 렌더러가 사용 중인 머티리얼의 컬러를 변경, 외형 색이 변함
        if (zombieRenderer != null) zombieRenderer.material.color = zombieData.skinColor;
    }

    private void Start() {
        // 게임 오브젝트 활성화와 동시에 AI의 추적 루틴 시작
        StartCoroutine(UpdatePath());
    }

    private void Update() {
        // 추적 대상의 존재 여부에 따라 다른 애니메이션 재생
        if (zombieAnimator != null) zombieAnimator.SetBool("HasTarget", hasTarget);
    }

    // 주기적으로 추적할 대상의 위치를 찾아 경로 갱신
    private IEnumerator UpdatePath() {
        // 살아 있는 동안 무한 루프
        while (!dead)
        {
            // 에이전트가 없거나 내비메시 위에 있지 않으면 경로 갱신 불가
            if (navMeshAgent == null || !navMeshAgent.isOnNavMesh)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            if (hasTarget)
            {
                // 추적 대상 존재 : 경로를 갱신하고 AI 이동을 계속 진행
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(
                    targetEntity.transform.position);
            }
            else
            {
                // 추적 대상 없음 : AI 이동 중지
                navMeshAgent.isStopped = true;

                // 20유닛의 반지름을 가진 가상의 구를 그렸을 때 구와 겹치는 모든 콜라이더를 가져옴
                // 단, whatIsTarget 레이어를 가진 콜라이더만 가져오도록 필터링
                Collider[] colliders =
                    Physics.OverlapSphere(transform.position, 20f, whatIsTarget);

                // 모든 콜라이더를 순회하면서 살아 있는 LivingEntity 찾기
                for (int i = 0; i < colliders.Length; i++)
                {
                    // 콜라이더로부터 LivingEntity 컴포넌트 가져오기
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    // LivingEntity 컴포넌트가 존재하며, 해당 LivingEntity가 살아 있다면
                    if (livingEntity != null && !livingEntity.dead)
                    {
                        // 추적 대상을 해당 LivingEntity로 설정
                        targetEntity = livingEntity;

                        // for 문 루프 즉시 정지
                        break;
                    }
                }
            }

            // 0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    // 데미지를 입었을 때 실행할 처리
    public override void OnDamage(float damage,
        Vector3 hitPoint, Vector3 hitNormal) {
        if (dead || (GameManager.instance != null && GameManager.instance.isGameover)) return;
        // 아직 사망하지 않은 경우에만 피격 효과 재생
        if (!dead)
        {
            // 공격받은 지점과 방향으로 파티클 효과 재생
            if (hitEffect != null)
            {
                hitEffect.transform.position = hitPoint;
                hitEffect.transform.rotation =
                    Quaternion.LookRotation(hitNormal);
                hitEffect.Play();
            }

            // 피격 효과음 재생
            if (zombieAudioPlayer != null) zombieAudioPlayer.PlayOneShot(hitSound);
        }

        // LivingEntity의 OnDamage()를 실행하여 데미지 적용
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    // 사망 처리
    public override void Die() {
        if (dead) return;
        // LivingEntity의 Die()를 실행하여 기본 사망 처리 실행
        base.Die();

        // 다른 AI를 방해하지 않도록 자신의 모든 콜라이더를 비활성화
        Collider[] zombieColliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < zombieColliders.Length; i++)
        {
            zombieColliders[i].enabled = false;
        }

        // AI 추적을 중지하고 내비메시 컴포넌트를 비활성화
        if (navMeshAgent != null)
        {
            if (navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        // 사망 애니메이션 재생
        if (zombieAnimator != null) zombieAnimator.SetTrigger("Die");
        // 사망 효과음 재생
        if (zombieAudioPlayer != null) zombieAudioPlayer.PlayOneShot(deathSound);
    }

    private void OnTriggerStay(Collider other) {
        if (GameManager.instance != null && GameManager.instance.isGameover) return;
        // 자신이 사망하지 않았으며,
        // 최근 공격 시점에서 timeBetAttack 이상 시간이 지났다면 공격 가능
        if (!dead && Time.time >= lastAttackTime + timeBetAttack)
        {
            // 상대방의 LivingEntity 타입 가져오기 시도
            LivingEntity attackTarget =
                other.GetComponent<LivingEntity>();

            // 상대방의 LivingEntity가 자신의 추적 대상이라면 공격 실행
            if (attackTarget != null && attackTarget == targetEntity)
            {
                // 최근 공격 시간 갱신
                lastAttackTime = Time.time;

                // 상대방의 피격 위치와 피격 방향을 근삿값으로 계산
                Vector3 hitPoint =
                    other.ClosestPoint(transform.position);
                Vector3 hitNormal =
                    transform.position - other.transform.position;

                // 공격 실행
                attackTarget.OnDamage(damage, hitPoint, hitNormal);
            }
        }
    }
}
