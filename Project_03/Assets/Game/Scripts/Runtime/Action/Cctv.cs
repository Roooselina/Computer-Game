using UnityEngine;

namespace EW
{
    /// <summary>
    /// CCTV. 좌우로 천천히 돌며 끝에서 잠깐 멈춘다. 시야는 벽·불투명 상자에 잘린 부채꼴로 그대로 보인다.
    /// 범위에 잠깐 닿았다고 바로 들키지 않는다: 게이지가 차는 동안 렌즈가 루를 따라가고, 다 차면 근처 경비에게
    /// '그 위치'를 신고한다 (경비가 확인하러 올 뿐 계속 추적하지 않음). 몸이 시야에서 사라지면 게이지가 줄어든다.
    /// 간섭(약한 전기)을 받으면 렌즈가 꺼지고 고개가 처지며 일정 시간 멈춘다. 전력 끌어오기를 맞으면 부서진다.
    /// </summary>
    public class Cctv : ZapTarget, ISensor
    {
        [Tooltip("회전 범위 (도). 0 = 오른쪽, -90 = 아래")] public float angleA = -150f, angleB = -100f;
        public Transform head;
        public SpriteRenderer lens;
        public Sprite lensOn, lensOff;
        public float startPhase;
        [Tooltip("이 플래그가 있으면 꺼져 있다 (정전 등)")] public string offFlag;
        [Tooltip("이 플래그가 있어야 켜진다 (경보 이후 등)")] public string onFlag;

        float angle, pauseT, susp, grace, jamT, realertT, lostT;
        int sweepDir = 1;
        bool seen, tracking;
        VisionConeView cone;
        ActionTuning.CameraT C => ActionTuning.I.cctv;

        public bool Broken => Game.I != null && Game.I.State != null && Game.I.State.Has("cctv_broken_" + id);
        public bool Jammed => jamT > 0;
        public bool Powered
        {
            get
            {
                var s = Game.I != null ? Game.I.State : null;
                if (s == null) return false;
                if (!string.IsNullOrEmpty(offFlag) && s.Has(offFlag)) return false;
                if (!string.IsNullOrEmpty(onFlag) && !s.Has(onFlag)) return false;
                return true;
            }
        }
        public float JamLeft => jamT;
        public float Angle => angle;
        public float HalfAngle => C.halfAngle;
        public float Range => C.range;
        public Vector2 Eye => head != null ? (Vector2)head.position : (Vector2)transform.position;

        public bool SensorActive => Powered && !Broken && !Jammed;
        public Vector2 MarkPos => (Vector2)transform.position + new Vector2(0, 0.55f);
        public float Suspicion => susp;
        public bool SeesPlayer => seen;
        public SensorState SState => !SensorActive ? SensorState.Off : susp >= 1f ? SensorState.Alert : susp > 0 ? SensorState.Suspicious : SensorState.Idle;

        public override bool CanJam => Powered && !Broken;
        public override float JamPriority => Jammed ? -4f : 2.5f + susp * 2f;
        public override float PullPriority => 1f;
        public override bool CanPull => Powered && !Broken;
        public override string JamResult => "CCTV " + Mathf.RoundToInt(ActionTuning.I.Skill("jam")?.effectTime ?? C.jamTime) + "초 정지 (조용함)";
        public override string PullResult => "CCTV 파괴 (큰 소리·빛)";

        protected override void OnEnable() { base.OnEnable(); Sensors.Add(this); }
        protected override void OnDisable() { base.OnDisable(); Sensors.Remove(this); }

        void Start()
        {
            angle = Mathf.Lerp(angleA, angleB, Mathf.Repeat(startPhase, 1f));
            cone = VisionConeView.Create(transform, 7);
            displayName = string.IsNullOrEmpty(displayName) || displayName == "장치" ? "CCTV" : displayName;
        }

        void Update()
        {
            float dt = Hazard.Dt;
            var g = Game.I;
            if (jamT > 0) jamT -= dt;
            bool active = SensorActive;
            seen = false;
            if (active && g != null && g.Player != null && dt > 0)
            {
                int pts = Sight.SeePlayer(Eye, angle, C.halfAngle, C.range, g.Player, out float dist);
                var diff = ActionTuning.CurrentDifficulty;
                if (pts > 0)
                {
                    seen = true; grace = C.suspicionGrace; lostT = 0;
                    float near = Mathf.Lerp(1.6f, 0.7f, Mathf.Clamp01(dist / C.range));
                    susp = Mathf.Min(1f, susp + C.suspicionRate * near * (g.Player.Crouching ? 0.7f : 1f) * diff.suspicionMul * dt);
                    // 게이지가 차는 동안 렌즈가 루를 따라간다
                    Vector2 d = g.Player.Chest - Eye;
                    float want = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                    float lo = Mathf.Min(angleA, angleB) - 6f, hi = Mathf.Max(angleA, angleB) + 6f;
                    want = Mathf.Clamp(Mathf.DeltaAngle(0, want) < lo - 180 ? want + 360 : want, lo, hi);
                    angle = Mathf.MoveTowards(angle, want, 40f * dt);
                    tracking = true;
                    if (susp >= 1f)
                    {
                        realertT -= dt;
                        if (realertT <= 0)
                        {
                            realertT = C.realertInterval;
                            Sfx.Play("sfx_alert", 0.7f, 1.2f);
                            // 근처 경비를 부른다. 부를 경비가 없으면 출입구에서 지원 드론이 온다 (횟수·위치 제한)
                            int n = Sensors.AlertGuards(g.Player.transform.position, C.alertRadius, null, true);
                            if (n == 0) n = Reinforcements.Request(g.Player.transform.position) ? 1 : 0;
                            Sensors.LastCallTime = Time.time;
                            Sensors.LastCallText = n > 0 ? "! 경비 호출 — 시야를 끊어!" : "! 발각 — 시야를 끊어!";
                            g.State.Set("seen_by_cctv");
                            if (g.CurrentMapId != null) g.State.Set("spotted_" + g.CurrentMapId);
                        }
                    }
                }
                else
                {
                    if (grace > 0) grace -= dt;
                    else susp = Mathf.Max(0, susp - C.suspicionDecay * dt);
                    lostT += dt;
                    if (susp <= 0) { tracking = false; realertT = 0; }
                }
            }
            if (active && !tracking && dt > 0)
            {
                if (pauseT > 0) pauseT -= dt;
                else
                {
                    float to = sweepDir > 0 ? Mathf.Max(angleA, angleB) : Mathf.Min(angleA, angleB);
                    angle = Mathf.MoveTowards(angle, to, C.sweepSpeed * dt);
                    if (Mathf.Abs(angle - to) < 0.01f) { sweepDir = -sweepDir; pauseT = C.endPause; }
                }
            }
            if (!active) { susp = Mathf.Max(0, susp - dt); tracking = false; }

            // 모양: 고장·간섭이면 고개가 처지고 렌즈가 꺼진다
            float shown = angle;
            if (Broken) shown = -90f + 25f;
            else if (Jammed || !Powered) shown = Mathf.Lerp(angle, -90f, 0.55f);
            if (head != null)
            {
                bool flip = Mathf.Cos(shown * Mathf.Deg2Rad) < 0;
                head.localRotation = Quaternion.Euler(0, 0, flip ? shown + 180f : shown);
                head.localScale = new Vector3(flip ? -1 : 1, 1, 1);
            }
            if (lens != null)
            {
                bool on = active;
                lens.sprite = on ? lensOn : lensOff;
                lens.color = !on ? new Color(0.35f, 0.35f, 0.4f) : susp >= 1f ? new Color(1f, 0.3f, 0.25f) : susp > 0 ? new Color(1f, 0.85f, 0.3f) : new Color(0.6f, 1f, 0.9f);
            }
            if (cone != null)
            {
                cone.Visible = active;
                cone.Eye = Eye; cone.DirDeg = angle; cone.HalfAngle = C.halfAngle; cone.Range = C.range; cone.State = SState;
            }
        }

        public override void Jam(float seconds)
        {
            if (Broken) return;
            jamT = Mathf.Max(jamT, seconds > 0 ? seconds : C.jamTime);
            susp = 0; tracking = false;
            Sfx.Play("sfx_power_down", 0.5f, 1.5f);
        }

        public override void Pull(PowerSource src) { Break(); }

        public void Break()
        {
            var g = Game.I;
            if (g != null && g.State != null) g.State.Set("cctv_broken_" + id);
            susp = 0;
            Sfx.Play("sfx_glass_break", 0.7f, 1.4f);
            Fx.Spawn("Spark", transform.position, false, 31, 0.8f);
        }

        /// <summary>테스트용: 지정 점이 시야 판정상 보이는지 (표시와 같은 계산).</summary>
        public bool SeesPoint(Vector2 pt)
        {
            Vector2 d = pt - Eye; float m = d.magnitude;
            if (m > C.range) return false;
            if (Mathf.Abs(Mathf.DeltaAngle(angle, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg)) > C.halfAngle) return false;
            return Sight.Clear(Eye, pt);
        }
        public VisionConeView Cone => cone;
        public void DebugSetAngle(float a) { angle = a; pauseT = 999; }
    }
}
