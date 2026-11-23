using UnityEngine;

namespace EW
{
    /// <summary>
    /// 컴퓨터 안 바탕화면의 연습 대상 (안내서를 닫은 뒤 기술을 안전하게 시험하는 곳).
    /// - dummy  : 'test.exe' 더미. 톡!·스파크 샷을 맞으면 번쩍이고 흔들린다. 쓰러지거나 사라지지 않는다.
    /// - webcam : '카메라 앱'. 간섭(W)을 받으면 렌즈가 꺼졌다가 효과 시간이 끝나면 다시 켜진다.
    /// - panel  : '멈춘 창'. 외부 전력 방전(E)을 맞으면 깨진 모습이 되었다가 4초 뒤 원래대로 돌아온다.
    /// 필수 파일(안내서·접속 기록)과 별개 오브젝트라 연습 공격으로 진행이 막히지 않는다. 소음·경보를 만들지 않는다.
    /// </summary>
    public class PracticeTarget : ZapTarget, IDamageable
    {
        public enum Kind { Dummy, Webcam, Panel }
        public Kind kind;
        public SpriteRenderer art;
        public Sprite normal, hitSprite;
        float hitT, offT;
        Vector3 basePos;

        protected override void OnEnable() { base.OnEnable(); if (kind == Kind.Dummy) Damageables.Add(this); basePos = transform.position; }
        protected override void OnDisable() { base.OnDisable(); Damageables.Remove(this); }

        public bool Alive => kind == Kind.Dummy;
        public Vector2 Center => (Vector2)transform.position + aimOffset;
        public void TakeHit(int damage, float stagger, Vector2 from, string kind_)
        {
            hitT = 0.35f;
            Fx.Spawn("Spark", Center, false, 31, 0.6f);
            Game.I?.State?.Set("pc_try_q");
        }

        public override bool CanJam => kind == Kind.Webcam && offT <= 0;
        public override bool CanPull => kind == Kind.Panel && offT <= 0;
        public override float JamPriority => 1f;
        public override string JamResult => "카메라 앱 잠깐 정지";
        public override string PullResult => "멈춘 창 강제 종료 (연습)";
        public override void Jam(float seconds) { offT = Mathf.Max(2f, seconds); Game.I?.State?.Set("pc_try_w"); }
        public override void Pull(PowerSource src) { offT = 4f; Fx.Spawn("Explosion", AimPoint, false, 42, 1f); Game.I?.State?.Set("pc_try_e"); }

        void Update()
        {
            float dt = Time.deltaTime;
            if (hitT > 0) hitT -= dt;
            if (offT > 0) offT -= dt;
            if (art == null) return;
            switch (kind)
            {
                case Kind.Dummy:
                    art.color = hitT > 0 ? new Color(1f, 1f, 0.55f) : Color.white;
                    transform.position = basePos + new Vector3(hitT > 0 ? Mathf.Sin(Time.time * 70f) * 0.04f : 0, 0, 0);
                    break;
                case Kind.Webcam:
                case Kind.Panel:
                    art.sprite = offT > 0 && hitSprite != null ? hitSprite : normal;
                    art.color = offT > 0 && hitSprite == null ? new Color(0.45f, 0.45f, 0.5f) : Color.white;
                    break;
            }
        }
    }
}
