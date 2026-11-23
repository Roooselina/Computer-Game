using System.Collections;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 8장 상층 회랑 — 괴물 토끼로 변한 라비엘 (GDD 8장).
    /// 친구를 죽이는 전투가 아니다. 두 갈래 모두 HP 0 = '멈춰 섬' (무릎 꿇고 변신이 풀린다).
    ///  Chase (도망): 다치게 하지 않으려 거리를 벌린다. 라비엘이 뛰어 따라오고(착지 표식 0.9초), 셔터 패널로 앞을 막으면 부딪혀 멈칫한다.
    ///                회랑 끝 문까지 가면 끝. 공격으로 쓰러뜨릴 필요 없음 (HP 바 대신 '거리' 표시).
    ///  Stop (멈춰 세우기): 도약 내려찍기(착지 표식) · 앞발 휘두르기(0.6초) · 돌진(0.8초 띠). 루가 가리키는 '옆 문'으로 돌진을 유도하면 문을 부수고 3초 멈칫.
    /// </summary>
    public class RavielBeast : BossBase, IArenaUser
    {
        public enum RState { Idle, Chase, LeapWindup, Leap, Swipe, SwipeWindup, ChargeWindup, Charge, Stunned, Recover }
        public RState State { get; private set; } = RState.Idle;
        public bool chaseMode;
        public float goalX = 76f;
        public float floorY;
        public Transform[] breakDoors;
        public Collider2D[] chaseShutters;
        public SpriteRenderer[] chaseShutterArt;
        public int DoorsBroken { get; private set; }
        public int ShutterHits { get; private set; }
        float timer, leapFrom, leapTo, chargeDir;
        readonly float[] shutterUntil = new float[8];
        bool[] doorGone;

        public override string BarName => chaseMode ? "라비엘 — 거리 벌리기" : "라비엘 — 멈춰 세우기";
        public override string BarPhase => stun > 0 ? "멈칫" : chaseMode ? "회랑 끝 문까지" : "";
        public override int BarMax => chaseMode ? 100 : maxHp;
        public int Distance => P == null ? 0 : Mathf.RoundToInt(Mathf.Clamp01((P.transform.position.x - arenaMin) / Mathf.Max(1f, goalX - arenaMin)) * 100f);

        public override void Activate()
        {
            bossName = "라비엘";
            bodyHeight = 2.4f;
            base.Activate();
            ArenaSwitch.User = this;
            doorGone = new bool[breakDoors != null ? breakDoors.Length : 0];
            State = chaseMode ? RState.Chase : RState.Recover; timer = 1.2f;
            if (chaseMode) hp = 0;   // 거리 표시용
        }

        public override bool Alive => Active && !Defeated && (chaseMode || hp > 0);
        public override bool BarVisible => Active && !Defeated;

        public override void Stop()
        {
            base.Stop();
            if (ArenaSwitch.User == (IArenaUser)this) ArenaSwitch.User = null;
        }

        protected override float DamageMul(string kind) => stun > 0 ? 2f : 1f;

        public override void TakeHit(int damage, float stagger, Vector2 from, string kind)
        {
            if (chaseMode)
            {
                // 도망 중에는 맞아도 줄지 않는다 — 잠깐 멈칫 (무리하게 싸우지 않아도 된다)
                if (stun <= 0 && Random.value < 0.35f) { stun = 0.5f; State = RState.Stunned; timer = 0.5f; Anim("Hurt"); }
                Sfx.Play("sfx_robot_hit", 0.5f, 0.8f);
                return;
            }
            base.TakeHit(damage, stagger, from, kind);
        }

        protected override void Update()
        {
            base.Update();
            if (!Active || P == null) return;
            if (chaseMode) hp = Distance;
            float dt = Dt;
            if (dt <= 0) return;
            if (stun > 0) stun -= dt;
            UpdateShutters();
            switch (State)
            {
                case RState.Chase:
                    {
                        FaceLu(); Anim("Walk");
                        float dx = DxToLu;
                        float sp = Mathf.Abs(dx) > 7f ? 4.8f : 3.4f;
                        float before = transform.position.x;
                        StepX(Mathf.Sign(dx) * sp, dt);
                        if (BlockedByShutter(before, transform.position.x)) { transform.position = new Vector3(before, transform.position.y, 0); ShutterHits++; StunFor(1.6f, "셔터"); break; }
                        timer -= dt;
                        if (LuNear(1.1f, 2f)) { P.Hurt(4, transform.position, "physical"); State = RState.Recover; timer = 1.6f; }
                        else if (timer <= 0 && Mathf.Abs(dx) > 5f && Mathf.Abs(dx) < 10f) BeginLeap();
                        if (P.transform.position.x >= goalX) { Defeated = true; Active = false; ClearTele(); OnDefeated?.Invoke(); }
                    }
                    break;
                case RState.LeapWindup:
                    timer -= dt; Anim("Windup"); Blink(new Color(1f, 0.6f, 0.7f), 10f);
                    if (timer <= 0) { White(); State = RState.Leap; timer = 0.55f; leapFrom = transform.position.x; Sfx.Play("sfx_fall_whoosh", 0.8f); Anim("Leap"); }
                    break;
                case RState.Leap:
                    {
                        timer -= dt;
                        float k = 1f - Mathf.Clamp01(timer / 0.55f);
                        float x = Mathf.Lerp(leapFrom, leapTo, k);
                        if (BlockedByShutter(transform.position.x, x)) { ClearTele(); ShutterHits++; StunFor(1.8f, "셔터"); break; }
                        transform.position = new Vector3(Mathf.Clamp(x, arenaMin, arenaMax), floorY + Mathf.Sin(k * Mathf.PI) * 2.2f, 0);
                        if (timer <= 0)
                        {
                            transform.position = new Vector3(transform.position.x, floorY, 0);
                            ClearTele(); Anim("Slam");
                            Sfx.Play("sfx_debris", 0.9f); Cutscene.Shake(0.08f, 0.25f);
                            if (Mathf.Abs(P.transform.position.x - transform.position.x) < 1.6f && P.Grounded) P.Hurt(chaseMode ? 5 : 8, transform.position, "physical");
                            State = RState.Recover; timer = chaseMode ? 1.0f : 1.8f;
                        }
                    }
                    break;
                case RState.SwipeWindup:
                    timer -= dt; FaceLu(); Anim("Windup"); Blink(new Color(1f, 1f, 0.6f), 14f);
                    if (timer <= 0)
                    {
                        White(); ClearTele(); Anim("Swipe"); Sfx.Play("sfx_whip", 0.9f, 0.7f);
                        var box = new Rect(new Vector2(transform.position.x + (facing > 0 ? 0 : -2f), transform.position.y), new Vector2(2f, 2.2f));
                        if (box.Contains(P.Chest)) P.Hurt(7, transform.position, "physical");
                        State = RState.Recover; timer = 1.5f;
                    }
                    break;
                case RState.ChargeWindup:
                    timer -= dt; Anim("Windup"); Blink(new Color(1f, 0.55f, 0.5f), 10f);
                    if (timer <= 0) { White(); ClearTele(); State = RState.Charge; timer = 1.6f; Sfx.Play("sfx_dash", 0.9f, 0.7f); }
                    break;
                case RState.Charge:
                    {
                        timer -= dt; Anim("Walk");
                        float before = transform.position.x;
                        StepX(chargeDir * 9f, dt);
                        int d = DoorHit(before, transform.position.x);
                        if (d >= 0) { BreakDoor(d); break; }
                        if (LuNear(1.0f, 2f)) { P.Hurt(8, (Vector2)transform.position - new Vector2(chargeDir, 0), "physical"); State = RState.Recover; timer = 1.2f; break; }
                        bool wall = Mathf.Abs(transform.position.x - before) < 0.01f;
                        if (timer <= 0 || wall) { State = RState.Recover; timer = 1.6f; if (wall) { Cutscene.Shake(0.05f, 0.2f); Sfx.Play("sfx_door_bang", 0.7f, 0.7f); } }
                    }
                    break;
                case RState.Stunned:
                    timer -= dt; Anim("Stun");
                    if (timer <= 0) { stun = 0; White(); State = chaseMode ? RState.Chase : RState.Recover; timer = chaseMode ? 1.6f : 0.7f; }
                    break;
                case RState.Recover:
                    timer -= dt; Anim("Idle");
                    if (timer <= 0)
                    {
                        if (chaseMode) { State = RState.Chase; timer = 2.4f; break; }
                        FaceLu();
                        float adx = Mathf.Abs(DxToLu);
                        if (adx < 2.2f) { State = RState.SwipeWindup; timer = 0.6f; FloorTele(transform.position.x, transform.position.x + facing * 2f, floorY, TeleYellow); }
                        else if (Random.value < 0.5f || adx > 7f) BeginLeap();
                        else { chargeDir = Mathf.Sign(DxToLu); State = RState.ChargeWindup; timer = 0.8f; FloorTele(transform.position.x, Mathf.Clamp(transform.position.x + chargeDir * 12f, arenaMin, arenaMax), floorY, TeleRed); }
                    }
                    break;
            }
        }

        void BeginLeap()
        {
            // 도망: 루가 서 있던 자리보다 조금 뒤에 떨어진다 (계속 달리면 맞지 않는다)
            float lx = P.transform.position.x - (chaseMode ? Mathf.Sign(P.transform.position.x - transform.position.x) * 1.2f : 0f);
            leapTo = Mathf.Clamp(lx, arenaMin, arenaMax);
            State = RState.LeapWindup; timer = 0.9f;
            FloorTele(leapTo - 1.4f, leapTo + 1.4f, floorY, TeleRed);
        }

        void StunFor(float s, string why)
        {
            stun = s; State = RState.Stunned; timer = s; ClearTele();
            Anim("Stun"); Sfx.Play("sfx_door_bang", 0.9f, 0.6f); Cutscene.Shake(0.06f, 0.25f);
            if (g != null) g.State.Set("rav_stun_" + why);
        }

        // ---- 옆 문 (멈춰 세우기): 돌진이 문을 지나가면 문이 부서지고 멈칫
        int DoorHit(float a, float b)
        {
            if (breakDoors == null) return -1;
            for (int i = 0; i < breakDoors.Length; i++)
            {
                if (doorGone[i] || breakDoors[i] == null) continue;
                float x = breakDoors[i].position.x;
                if ((a - x) * (b - x) <= 0) return i;
            }
            return -1;
        }

        void BreakDoor(int i)
        {
            doorGone[i] = true; DoorsBroken++;
            Fx.Spawn("Explosion", breakDoors[i].position + Vector3.up, false, 31, 0.8f);
            breakDoors[i].gameObject.SetActive(false);
            TakeHit(4, 1f, transform.position, "physical");
            if (Active) StunFor(3f, "door");
            if (DoorsBroken == 1 && g != null) g.Monologue("문! 그래, 문 부숴! 나 말고!", 2.5f);
        }

        // ---- 도망: 셔터
        bool BlockedByShutter(float a, float b)
        {
            if (chaseShutters == null) return false;
            for (int i = 0; i < chaseShutters.Length; i++)
            {
                if (chaseShutters[i] == null || Time.time >= shutterUntil[i]) continue;
                float x = chaseShutters[i].bounds.center.x;
                if ((a - x) * (b - x) <= 0 || Mathf.Abs(b - x) < 0.6f) return true;
            }
            return false;
        }

        void UpdateShutters()
        {
            if (chaseShutters == null) return;
            for (int i = 0; i < chaseShutters.Length; i++)
            {
                bool on = Time.time < shutterUntil[i];
                if (chaseShutters[i] != null) chaseShutters[i].enabled = false;     // 루는 지나갈 수 있다 (라비엘만 막힌다)
                if (chaseShutterArt != null && i < chaseShutterArt.Length && chaseShutterArt[i] != null) chaseShutterArt[i].enabled = on;
            }
        }

        public bool CanUse(ArenaSwitch s) => Active && chaseMode && s.kind.StartsWith("cshutter");
        public string PromptFor(ArenaSwitch s) => "셔터 내리기";
        public IEnumerator Use(ArenaSwitch s)
        {
            int i = 0; int.TryParse(s.kind.Substring("cshutter".Length), out i);
            if (i >= 0 && i < shutterUntil.Length) shutterUntil[i] = Time.time + 4f;
            Sfx.Play("sfx_door_slide", 0.9f);
            yield return null;
        }

        protected override void OnZero()
        {
            ClearTele();
            Defeated = true; Active = false; Anim("Down");
            if (ArenaSwitch.User == (IArenaUser)this) ArenaSwitch.User = null;
            OnDefeated?.Invoke();
        }
    }
}
