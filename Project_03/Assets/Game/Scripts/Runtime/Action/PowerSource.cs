using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 끌어올 수 있는 외부 전원 (노출된 배전함·끊어진 굵은 선 끝).
    /// 스파크가 튄다고 다 쓸 수 있는 것은 아니다: 출력이 남은 전원에만 번개 표식이 붙는다.
    /// 화면 표시는 '끌어올 전력 있음 / 전력 없음 / 사용 중' 세 가지 (모양+색+움직임).
    /// 사용 횟수는 저장된다 (재시도하면 구간 시작 상태로 돌아온다).
    /// </summary>
    public class PowerSource : MonoBehaviour
    {
        public static readonly List<PowerSource> All = new List<PowerSource>();
        public string id;
        public string displayName = "노출된 전원";
        [Tooltip("끌어올 수 있는 횟수 (0 이면 처음부터 전력 없음 — 장식용 스파크)")] public int charges = 1;
        public SpriteRenderer icon;
        public Sprite iconAvailable, iconEmpty, iconInUse;
        public SpriteRenderer glow;
        public Vector2 tapOffset = new Vector2(0, 0.6f);
        float inUse;

        public enum PState { Available, Empty, InUse }

        void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public Vector2 Tap => (Vector2)transform.position + tapOffset;
        int Used => Game.I != null && Game.I.State != null ? Game.I.State.GetVar("psrc_" + id) : 0;
        public int Left => Mathf.Max(0, charges - Used);
        public PState State => inUse > 0 ? PState.InUse : Left > 0 ? PState.Available : PState.Empty;
        public string StateText => State == PState.InUse ? "사용 중" : State == PState.Available ? "끌어올 전력 있음" : "전력 없음";

        public void Consume(float busySeconds)
        {
            var g = Game.I;
            if (g != null && g.State != null) g.State.SetVar("psrc_" + id, Used + 1);
            inUse = busySeconds;
        }

        void Update()
        {
            if (inUse > 0) inUse -= Time.deltaTime;
            var st = State;
            if (icon != null)
            {
                icon.sprite = st == PState.Available ? iconAvailable : st == PState.InUse ? iconInUse : iconEmpty;
                float bob = st == PState.Available ? Mathf.Round(Mathf.Sin(Time.time * 3f) * 1.5f) / 32f : 0;
                icon.transform.localPosition = new Vector3(0, tapOffset.y + 0.75f + bob, 0);
                icon.color = st == PState.Empty ? new Color(0.6f, 0.6f, 0.65f, 0.75f) : Color.white;
            }
            if (glow != null)
            {
                float a = st == PState.Available ? 0.28f + 0.1f * Mathf.Sin(Time.time * 5f) : st == PState.InUse ? 0.6f : 0f;
                glow.color = new Color(1f, 0.9f, 0.45f, a);
            }
        }
    }
}
