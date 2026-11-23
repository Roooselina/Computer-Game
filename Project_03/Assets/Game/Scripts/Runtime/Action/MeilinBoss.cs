using System.Collections;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 7장 접수 홀 — 메이린 (GDD 7장 보스전).
    /// 번호표는 가위를 조종하는 마법이 아니다: 천장 레일의 자동 포획팔이 '호출 번호'의 표적을 잡고, 메이린이 잡힌 대상으로 롤러 돌진하는 협동 패턴.
    ///  1 호출 → 포획팔 추적(바닥 원) → 고정(깜빡임) → 낙하. 잡히면 1초 붙들림(피해 작음) → 메이린 돌진 준비(바닥 띠 0.9초) → 돌진.
    ///  2 유지보수 단말(허용된 기능)로 호출 번호 47 → 00, 표적 위치 대기석 → 하역대로 바꾸면 포획팔이 빈 운반 인형을 잡는다
    ///    → 메이린이 인형으로 돌진해 부딪혀 3초 빈틈 (피해 2배). 단말 설정은 한 번 쓰면 시스템이 47 로 되돌린다.
    ///  3 같은 속임수를 두 번 쓰면 포획팔을 끄고 가위·롤러 패턴으로 전환 (가위 0.6초 준비, 롤러 돌진 0.8초 준비).
    ///  기물: 셔터(돌진을 막아 부딪힘), 제동 장치(제동 구간에서 롤러가 미끄러져 넘어짐), 전도성 바닥(바닥 위에서 전기에 맞으면 경직, 바닥 연결함 방전).
    ///  일반 공격만으로도 이길 수 있다. HP 0 이면 쓰러지지 않고 퇴각한다 (아직 살아 있음).
    /// </summary>
    public class MeilinBoss : BossBase, IArenaUser
    {
        public enum MState { Idle, Approach, ArmTrack, ArmDrop, Holding, DashWindup, Dash, Crash, SnipWindup, Snip, RollWindup, Roll, Recover, Retreat }
        public MState State { get; private set; } = MState.Idle;
        public Transform claw; public SpriteRenderer clawArt; public Sprite[] clawFrames;
        public SpriteRenderer callBoard; public Sprite[] callFrames;
        public Transform doll; public float dockX;
        public float wet0, wet1, brake0, brake1;
        public Collider2D shutterBlock; public SpriteRenderer shutterArt;
        public float floorY;

        public bool ArmOff { get; private set; }
        public int TricksUsed { get; private set; }
        public int Crashes { get; private set; }
        public bool TargetIsDoll { get; private set; }
        public bool BrakeOn => Time.time < brakeUntil;
        public bool ShutterDown => Time.time < shutterUntil;
        public string LastStunCause { get; private set; } = "";
        float brakeUntil = -1, shutterUntil = -1;
        float timer, cycleGap = 1.4f, dashDir, dashTargetX, holdLu;
        float clawX, clawHome;
        GameObject tele;
        bool firstTrick = true;
        int snipsSinceArm;

        public override string BarPhase => stun > 0 ? "빈틈 (" + LastStunCause + ")" : ArmOff ? "가위·롤러" : TargetIsDoll ? "포획팔 · 표적 00" : "포획팔 · 호출 47";

        public override void Activate()
        {
            bossName = "메이린";
            base.Activate();
            ArenaSwitch.User = this;
            ArenaZap.User = this;
            clawHome = claw != null ? claw.position.x : transform.position.x; clawX = clawHome;
            SetCall(TargetIsDoll ? 2 : 0);
            State = MState.Approach; timer = 1.2f;
        }

        public override void Stop()
        {
            base.Stop();
            if (ArenaSwitch.User == (IArenaUser)this) ArenaSwitch.User = null;
            if (ArenaZap.User == (IArenaUser)this) ArenaZap.User = null;
            ReleaseLu();
            SetClaw(ArmOff ? 3 : 0);
        }

        void SetCall(int i) { if (callBoard != null && callFrames != null && callFrames.Length > 0) callBoard.sprite = callFrames[Mathf.Clamp(i, 0, callFrames.Length - 1)]; }
        void SetClaw(int i) { if (clawArt != null && clawFrames != null && clawFrames.Length > 0) clawArt.sprite = clawFrames[Mathf.Clamp(i, 0, clawFrames.Length - 1)]; }

        protected override float DamageMul(string kind)
        {
            float m = base.DamageMul(kind);
            return m;
        }

        protected override void OnHit(int dealt, float stagger, string kind)
        {
            // 전도성 바닥 위에서 전기에 맞으면 짧은 경직
            if (kind == "electric" && OnWet && stun <= 0 && (State == MState.Approach || State == MState.Recover || State == MState.Idle))
                Stun(1.2f, "젖은 바닥");
        }

        bool OnWet => transform.position.x > wet0 && transform.position.x < wet1;
        bool InBrake => transform.position.x > brake0 && transform.position.x < brake1;

        public void Stun(float s, string cause)
        {
            stun = Mathf.Max(stun, s); LastStunCause = cause;
            ClearTele(); State = MState.Crash; timer = s;
            Anim("Stun"); Sfx.Play("sfx_guard_break", 0.8f);
            Cutscene.Shake(0.05f, 0.2f);
        }

        void ReleaseLu()
        {
            if (holdLu > 0 && P != null) { P.Frozen = false; P.AnimOverride = null; }
            holdLu = 0;
        }

        protected override void Update()
        {
            base.Update();
            if (!Active || P == null) return;
            float dt = Dt;
            if (dt <= 0) return;
            if (stun > 0) stun -= dt;
            // 포획팔은 레일을 따라 움직인다
            if (claw != null) claw.position = new Vector3(clawX, claw.position.y, claw.position.z);
            if (holdLu > 0)
            {
                holdLu -= dt;
                if (claw != null) P.Teleport(new Vector2(clawX, P.transform.position.y));
                if (holdLu <= 0) { ReleaseLu(); P.GrantInvuln(0.4f); SetClaw(0); }
            }
            switch (State)
            {
                case MState.Approach:
                    FaceLu(); Anim("Roll");
                    timer -= dt;
                    if (Mathf.Abs(DxToLu) > 2.2f) StepX(Mathf.Sign(DxToLu) * 2.4f * (BrakeOn && InBrake ? 0.4f : 1f), dt);
                    if (timer <= 0) NextAttack();
                    break;
                case MState.ArmTrack:
                    {
                        timer -= dt;
                        float tx = TargetIsDoll ? (doll != null ? doll.position.x : dockX) : P.transform.position.x;
                        clawX = Mathf.MoveTowards(clawX, tx, (TargetIsDoll ? 40f : timer > 0.5f ? 6f : 0.6f) * dt);   // 하역대(고정 위치)로는 레일을 빠르게 이동
                        SetClaw(0);
                        if (tele != null) tele.transform.position = new Vector3(clawX, tele.transform.position.y, 0);
                        if (timer < 0.5f && claw != null && clawArt != null) clawArt.color = (Time.time * 12 % 1f) < 0.5f ? new Color(1f, 0.5f, 0.4f) : Color.white;
                        Anim("Talk");
                        if (timer <= 0) { if (clawArt != null) clawArt.color = Color.white; State = MState.ArmDrop; timer = 0.35f; Sfx.Play("sfx_metal_creak", 0.8f, 1.3f); }
                    }
                    break;
                case MState.ArmDrop:
                    timer -= dt;
                    if (timer <= 0)
                    {
                        ClearTele(); tele = null;
                        SetClaw(1);
                        Sfx.Play("sfx_door_bang", 0.6f, 1.4f);
                        if (TargetIsDoll && doll != null && Mathf.Abs(clawX - doll.position.x) < 1.0f)
                        {
                            SetClaw(2);
                            dashTargetX = doll.position.x;
                            BeginDash(true);
                        }
                        else if (!TargetIsDoll && Mathf.Abs(clawX - P.transform.position.x) < 0.8f && !P.Invulnerable)
                        {
                            P.Hurt(3, new Vector2(clawX, P.transform.position.y + 1f), "physical");
                            holdLu = 1.0f; P.Frozen = true; P.AnimOverride = "Hurt";
                            dashTargetX = clawX;
                            BeginDash(false);
                            if (g.State.Once("mei_grab_hint")) g.Monologue("붙잡혔다...! 놓이면 바로 " + GameInput.Label(Act.Dodge) + " 로 비켜!", 2.5f);
                        }
                        else { SetClaw(0); State = MState.Recover; timer = 1.0f; }
                        if (TargetIsDoll) { TargetIsDoll = false; SetCall(0); }     // 한 번 쓰면 시스템이 호출 번호를 47 로 되돌린다
                    }
                    break;
                case MState.DashWindup:
                    timer -= dt; FaceTo(dashTargetX); Anim("Dash");
                    Blink(new Color(1f, 0.55f, 0.5f), 10f);
                    if (timer <= 0) { White(); ClearTele(); State = MState.Dash; Sfx.Play("sfx_dash", 0.9f, 0.9f); }
                    break;
                case MState.Dash:
                    {
                        float sp = 10f;
                        if (BrakeOn && InBrake) { Stun(2.0f, "제동 장치"); TricksNote("brake"); break; }
                        float before = transform.position.x;
                        if (ShutterDown && shutterBlock != null && WouldCross(before, before + dashDir * sp * dt, shutterBlock.bounds.center.x))
                        { Stun(2.5f, "셔터"); TricksNote("shutter"); break; }
                        StepX(dashDir * sp, dt);
                        if (Random.value < 0.4f) Fx.Spawn("Smoke", (Vector2)transform.position + new Vector2(-dashDir * 0.4f, 0.1f), false, 8, 0.4f);
                        if (LuNear(0.6f)) { ReleaseLu(); P.Hurt(8, (Vector2)transform.position - new Vector2(dashDir, 0), "physical"); }
                        bool reached = dashDir > 0 ? transform.position.x >= dashTargetX - 0.3f : transform.position.x <= dashTargetX + 0.3f;
                        bool wall = Mathf.Abs(transform.position.x - before) < sp * dt * 0.3f;
                        if (reached || wall)
                        {
                            if (TargetHitDoll) { Crashes++; Stun(3.0f, "빈 운반 인형"); SetClaw(0); if (firstTrick) { firstTrick = false; g.RunEvent(StoryEvents.MeilinTrickLine(g)); } }
                            else { State = MState.Recover; timer = 1.1f; SetClaw(ArmOff ? 3 : 0); }
                        }
                    }
                    break;
                case MState.Crash:
                    timer -= dt;
                    if (timer <= 0 || stun <= 0)
                    {
                        stun = 0; White();
                        if (!ArmOff && TricksUsed >= 2) { ArmOff = true; SetClaw(3); SetCall(3); g.RunEvent(StoryEvents.MeilinArmOffLine(g)); }
                        State = MState.Recover; timer = 0.8f;
                    }
                    break;
                case MState.SnipWindup:
                    timer -= dt; FaceLu(); Anim("Windup");
                    Blink(new Color(1f, 1f, 0.6f), 14f);
                    if (timer <= 0)
                    {
                        White(); ClearTele(); Anim("Snip");
                        Sfx.Play("sfx_whip", 0.8f, 1.4f);
                        var box = new Rect(new Vector2(transform.position.x + (facing > 0 ? 0 : -1.5f), transform.position.y), new Vector2(1.5f, 1.5f));
                        if (box.Contains(P.Chest)) P.Hurt(6, transform.position, "physical");
                        State = MState.Snip; timer = 0.3f;
                    }
                    break;
                case MState.Snip:
                    timer -= dt;
                    if (timer <= 0) { snipsSinceArm++; if (hp < maxHp * 0.4f && snipsSinceArm % 3 != 0) { BeginSnip(0.45f); } else { State = MState.Recover; timer = 0.9f; } }
                    break;
                case MState.RollWindup:
                    timer -= dt; FaceTo(dashTargetX); Anim("Roll");
                    Blink(new Color(1f, 0.55f, 0.5f), 10f);
                    if (timer <= 0) { White(); ClearTele(); State = MState.Dash; Sfx.Play("sfx_dash", 0.9f, 1.1f); }
                    break;
                case MState.Recover:
                    timer -= dt; Anim("Idle");
                    if (timer <= 0) { State = MState.Approach; timer = cycleGap; }
                    break;
            }
        }

        bool TargetHitDoll => doll != null && Mathf.Abs(dashTargetX - doll.position.x) < 0.5f && Mathf.Abs(transform.position.x - doll.position.x) < 1.2f;
        static bool WouldCross(float a, float b, float x) => (a - x) * (b - x) <= 0;
        void FaceTo(float x) { facing = x > transform.position.x ? 1 : -1; if (View != null) View.Face(facing); dashDir = facing; }

        void TricksNote(string what) { if (g != null) g.State.Set("mei_used_" + what); }

        void NextAttack()
        {
            if (!ArmOff)
            {
                // 포획팔 주기: 호출 → 추적 → 낙하
                State = MState.ArmTrack; timer = 1.7f;
                SetCall(TargetIsDoll ? 2 : 0);
                Sfx.Play("sfx_pc_beep", 0.8f, TargetIsDoll ? 0.8f : 1.2f);
                tele = FloorTele(clawX - 0.6f, clawX + 0.6f, floorY, TeleYellow, null, 0.12f);
                return;
            }
            if (LuNear(1.8f)) BeginSnip(0.6f);
            else
            {
                dashTargetX = Mathf.Clamp(P.transform.position.x + Mathf.Sign(DxToLu) * 1.5f, arenaMin, arenaMax);
                FaceTo(dashTargetX);
                State = MState.RollWindup; timer = 0.8f;
                FloorTele(transform.position.x, dashTargetX, floorY, TeleRed);
            }
        }

        void BeginSnip(float windup)
        {
            State = MState.SnipWindup; timer = windup;
            FloorTele(transform.position.x, transform.position.x + facing * 1.5f, floorY, TeleYellow);
        }

        void BeginDash(bool toDoll)
        {
            FaceTo(dashTargetX);
            State = MState.DashWindup; timer = toDoll ? 0.6f : 0.9f;
            FloorTele(transform.position.x, dashTargetX, floorY, TeleRed);
        }

        protected override void OnZero()
        {
            ReleaseLu();
            base.OnZero();
            Stop();
        }

        // ------------------------------------------------------------ 기물
        public bool CanUse(ArenaSwitch s)
        {
            if (!Active) return false;
            switch (s.kind)
            {
                case "terminal": return !ArmOff;
                case "brake": return !BrakeOn;
                case "shutter": return !ShutterDown;
            }
            return false;
        }

        public string PromptFor(ArenaSwitch s)
        {
            switch (s.kind)
            {
                case "terminal": return ArmOff ? "유지보수 단말 (포획팔 꺼짐)" : TargetIsDoll ? "유지보수 단말 · 표적 00 설정됨" : "유지보수 단말";
                case "brake": return "제동 장치";
                case "shutter": return "셔터 내리기";
            }
            return s.promptText;
        }

        public IEnumerator Use(ArenaSwitch s)
        {
            switch (s.kind)
            {
                case "terminal":
                    yield return g.Say("c7_mt_screen", false);
                    yield return g.Choose(new[] { "호출 번호 47 → 00 · 표적 위치 대기석 → 하역대", "그대로 둔다" });
                    g.UI.dialogue.Show(false);
                    if (g.LastChoice == 0)
                    {
                        TargetIsDoll = true; TricksUsed++;
                        SetCall(2); s.SetFrame(1);
                        Sfx.Play("sfx_pc_on", 0.7f);
                        g.State.Set("mei_used_terminal");
                    }
                    break;
                case "brake":
                    brakeUntil = Time.time + 6f; s.SetFrame(1);
                    Sfx.Play("sfx_metal_creak", 0.8f, 0.8f);
                    g.UI.Toast("제동 구간 작동 (6초)", 1.6f);
                    yield return null;
                    break;
                case "shutter":
                    shutterUntil = Time.time + 5f;
                    Sfx.Play("sfx_door_slide", 0.9f);
                    g.UI.Toast("셔터 내림 (5초)", 1.6f);
                    yield return null;
                    break;
            }
        }

        void LateUpdate()
        {
            if (shutterBlock != null) shutterBlock.enabled = ShutterDown;
            if (shutterArt != null) shutterArt.enabled = ShutterDown;
            if (!BrakeOn) foreach (var s in Object.FindObjectsByType<ArenaSwitch>()) if (s.kind == "brake") s.SetFrame(0);
        }

        /// <summary>바닥 연결함 방전: 메이린이 전도성 바닥 위면 경직 + 피해.</summary>
        public bool FloorZap()
        {
            if (!Active) return false;
            Fx.Spawn("Arc", new Vector2((wet0 + wet1) / 2f, floorY + 0.2f), false, 31, 1.2f);
            Sfx.Play("sfx_zap", 1f);
            if (OnWet) { TakeHit(4, 1f, transform.position, "electric"); if (Active) Stun(2.6f, "전도성 바닥"); TricksNote("floor"); return true; }
            return false;
        }
    }

}
