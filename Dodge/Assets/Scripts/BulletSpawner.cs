using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 일정 주기마다 플레이어를 조준해 탄알을 생성
public class BulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab; // 생성할 탄알 프리팹
    public float spawnRateMin = 0.5f; // 최소 생성 주기
    public float spawnRateMax = 3f; // 최대 생성 주기

    private Transform target; // 조준 대상 (플레이어)
    private float spawnRate; // 이번 생성 주기
    private float timeAfterSpawn; // 마지막 생성 이후 경과 시간

    void Start()
    {
        timeAfterSpawn = 0f;
        // 생성 주기를 최소~최대 사이에서 랜덤 지정
        spawnRate = Random.Range(spawnRateMin, spawnRateMax);
        target = FindObjectOfType<PlayerController>().transform;
    }

    void Update()
    {
        timeAfterSpawn += Time.deltaTime;

        // 생성 주기가 지나면 탄알 생성
        if(timeAfterSpawn >= spawnRate) {
            timeAfterSpawn = 0f;

            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);

            // 탄알 정면이 플레이어를 향하도록 회전
            bullet.transform.LookAt(target);
            // 다음 생성 주기를 다시 랜덤 지정
            spawnRate = Random.Range(spawnRateMin, spawnRateMax);
        }
    }
}
