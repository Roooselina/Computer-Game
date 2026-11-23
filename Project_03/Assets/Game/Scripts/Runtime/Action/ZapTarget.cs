using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 타깃형 스킬(약한 간섭·외부 전력 끌어오기)로 고를 수 있는 기물의 공통 부분.
    /// 조준 중 윤곽·이름·예상 결과를 보여 주고, 실제 효과는 화면에서 보이는 변화(렌즈 꺼짐, 고개 돌림 등)로 나타난다.
    /// </summary>
    public abstract class ZapTarget : MonoBehaviour
    {
        public static readonly List<ZapTarget> All = new List<ZapTarget>();
        [Tooltip("저장·중복 방지 ID")] public string id;
        public string displayName = "장치";
        [Tooltip("조준 윤곽 크기 (유닛)")] public Vector2 size = new Vector2(1f, 1f);
        public Vector2 aimOffset = new Vector2(0, 0.5f);

        protected virtual void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        protected virtual void OnDisable() { All.Remove(this); }

        public Vector2 AimPoint => (Vector2)transform.position + aimOffset;
        public Rect Outline => new Rect(AimPoint - size / 2f, size);

        public virtual bool CanJam => false;
        public virtual bool CanPull => false;
        /// <summary>조준 안내에 보일 예상 결과 (예: "CCTV 8초 정지").</summary>
        public virtual string JamResult => "";
        public virtual string PullResult => "";
        public virtual void Jam(float seconds) { }
        /// <summary>자동 대상 우선순위 (클수록 먼저). 켜져 있는 CCTV·추적 중인 경비처럼 지금 위협인 것을 먼저 고른다.</summary>
        public virtual float JamPriority => 0f;
        public virtual float PullPriority => 0f;
        public virtual void Pull(PowerSource src) { }
    }
}
