using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 오브젝트를 Y축 기준으로 계속 회전
public class Rotator : MonoBehaviour
{
    public float rotationSpeed = 60f; // 초당 회전 각도

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
