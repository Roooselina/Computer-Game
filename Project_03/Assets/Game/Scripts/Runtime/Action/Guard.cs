using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 경비 기계 (순찰 경비·포획 드론·운반 로봇). 같은 필드에서 실시간으로 움직이며 별도 전투 화면은 없다.
    /// 순찰 → 의심(멈춰서 바라봄) → 확인(마지막으로 본 곳으로 다가감) → 추적(보이는 동안만 위치 갱신)
    /// → 마지막 발견 위치 수색 → 순찰 복귀.
    /// 벽 너머를 보지 않고, 보이지 않는 루의 위치를 계속 알지 못한다. 소리·CCTV 신고는 '그 위치'만 알려 준다.
    /// 공격은 준비 동작(빛·소리·자세)으로 예고하고, 피격 경직은 연속 잠금이 되지 않게 면역 시간을 둔다.
    /// 쓰러뜨리면 '일시 무력화'(재부팅까지 정지)이며 죽이지 않는다. 쓰러뜨려도 경험치는 없다.
    /// </summary>
    public class Guard : MonoBehaviour, IDamageable, ISensor
    {
        public static readonly List<Guard> All = new List<Guard>();
        public enum GState { Patrol, Suspicious, TurnToNoise, Investigate, Chase, Windup, Strike, Recover, Search, Return, Stagger, Jammed, Down }

        public string guardId;
        public string enemyId = "patrol_guard";
        public float patrolMin, patrolMax;
        public int startFacing = 1;
        public bool flying;
        [Tooltip("이 플래그가 있어야 나타남 (예: 경보 이후 배치)")] public string requireFlag;
        [Tooltip("이 플래그가 있으면 나타나지 않음")] public string hideFlag;
        [Tooltip("순찰 끝에서 멈추는 시간")] public float endWait = 1.4f;
        /// <summary>CCTV 신고로 출입구에서 온 지원 드론 (Reinforcements 가 관리).</summary>
        [System.NonSerialized] public bool Reinforcement;
        /// <summary>지원 드론이 수색을 마치고 돌아가는 중.</summary>
        public bool ReturnedFromHunt { get; private set; }
        public void FlyAway(float dt) { transform.position += new Vector3(Facing * 2.5f * dt, 1.2f * dt, 0); homeY = transform.position.y; }

        public CharacterView View;
        public GState State { get; private set; } = GState.Patrol;
        public int Facing { get; private set; } = 1;
        public int Hp { get; private set; }
        float susp, grace, timer, lostT, attackCd, lookT, homeY;
        Vector2 lastKnown, target;
        bool seen;
        readonly List<float> staggerTimes = new List<float>();
        float staggerImmune;
        VisionConeView cone;
        BoxCollider2D body;
        GState resumeState;
        ActionTuning.EnemyT T;
        bool present = true;
        bool pendingAlert;

        public bool Alive => present && State != GState.Down;
        public Vector2 Center => (Vector2)transform.position + new Vector2(0, flying ? 0 : 0.5f);
        public bool SensorActive => Alive && State != GState.Jammed && State != GState.Stagger;
        public Vector2 MarkPos => (Vector2)transform.position + new Vector2(0, flying ? 0.7f : 1.45f);
        public float Suspicion => susp;
        public bool SeesPlayer => seen;
        public SensorState SState
        {
            get
            {
                switch (State)
                {
                    case GState.Suspicious: case GState.TurnToNoise: return SensorState.Suspicious;
                    case GState.Investigate: return SensorState.Checking;
                    case GState.Chase: case GState.Windup: case GState.Strike: case GState.Recover: return SensorState.Alert;
                    case GState.Search: return SensorState.Searching;
                    case GState.Jammed: case GState.Down: return SensorState.Off;
                }
                return SensorState.Idle;
            }
        }
        public float AttackWindupLeft => State == GState.Windup ? timer : 0;

        void OnEnable() { if (!All.Contains(this)) All.Add(this); Damageables.Add(this); Sensors.Add(this); Noise.OnNoise += Hear; }
        void OnDisable() { All.Remove(this); Damageables.Remove(this); Sensors.Remove(this); Noise.OnNoise -= Hear; }

        void Start()
        {
            T = ActionTuning.I.Enemy(enemyId);
            Hp = T.hp;
            Facing = startFacing >= 0 ? 1 : -1;
            homeY = transform.position.y;
            if (patrolMax <= patrolMin) { patrolMin = transform.position.x - 1.5f; patrolMax = transform.position.x + 1.5f; }
            var g = Game.I;
            if (View == null && g != null)
            {
                var def = g.Db.GetEnemy(enemyId);
                var art = g.Config.GetArt(def != null ? def.artId : "droid03");
                if (art != null) View = CharacterView.Create(transform, art, 9);
            }
            if (View != null) View.Face(Facing);
            body = GetComponent<BoxCollider2D>();
            if (body == null) { body = gameObject.AddComponent<BoxCollider2D>(); body.size = new Vector2(0.8f, flying ? 0.7f : 1.1f); body.offset = new Vector2(0, flying ? 0 : 0.55f); }
            body.isTrigger = true;
            if (GetComponent<EnemyBody>() == null) gameObject.AddComponent<EnemyBody>();
            cone = VisionConeView.Create(transform, 8);
            var z = GetComponent<GuardZap>() ?? gameObject.AddComponent<GuardZap>();
            z.guard = this; z.id = guardId; z.displayName = def0Name(g); z.size = new Vector2(0.9f, flying ? 0.7f : 1.1f); z.aimOffset = new Vector2(0, flying ? 0 : 0.55f);
            target = new Vector2(Facing > 0 ? patrolMax : patrolMin, homeY);
            if (pendingAlert) { pendingAlert=false; Alert(lastKnown,hunting); }
        }

        string def0Name(Game g) { var d = g != null ? g.Db.GetEnemy(enemyId) : null; return d != null ? d.name : "경비"; }

        float Dir => flying ? (Facing > 0 ? -32f : 212f) : (Facing > 0 ? -4f : 184f);
        Vector2 Eye => (Vector2)transform.position + new Vector2(Facing * 0.25f, flying ? 0f : T.eyeHeight);

        void Update()
        {
            var g = Game.I;
            if (g == null || g.State == null || T == null) return;
            bool want = (string.IsNullOrEmpty(requireFlag) || g.State.Has(requireFlag)) && (string.IsNullOrEmpty(hideFlag) || !g.State.Has(hideFlag));
            if (want != present) { present = want; if (View != null) View.SetVisible(want); }
            if (!present) { if (cone != null) cone.Visible = false; seen = false; return; }
            float dt = Hazard.Dt;
            if (attackCd > 0) attackCd -= dt;
            if (staggerImmune > 0) staggerImmune -= dt;
            var p = g.Player;
            UpdateSight(p, dt);
            float speedMul = Facility.AlarmOn ? 1.2f : 1f;

            switch (State)
            {
                case GState.Patrol:
                    if (timer > 0) { timer -= dt; Anim("Idle"); break; }
                    if (MoveTo(target.x, T.walk * speedMul, dt))
                    {
                        timer = endWait; Facing = -Facing;
                        target = new Vector2(Facing > 0 ? patrolMax : patrolMin, homeY);
                    }
                    if (seen && susp > 0) Enter(GState.Suspicious);
                    break;
                case GState.Return:
                    if (Reinforcement) ReturnedFromHunt = true;
                    if (MoveTo(Mathf.Clamp(transform.position.x, patrolMin, patrolMax), T.walk * speedMul, dt)) { Enter(GState.Patrol); target = new Vector2(Facing > 0 ? patrolMax : patrolMin, homeY); }
                    if (seen && susp > 0) Enter(GState.Suspicious);
                    break;
                case GState.Suspicious:
                    Anim("Idle");
                    if (seen) FaceTo(lastKnown.x);
                    if (susp >= 1f) Enter(GState.Chase);
                    else if (susp >= T.checkThreshold && !seen) BeginInvestigate(lastKnown);
                    else if (susp >= T.checkThreshold + 0.25f) BeginInvestigate(lastKnown);
                    else if (susp <= 0) Enter(resumeState == GState.Search ? GState.Return : GState.Return);
                    break;
                case GState.TurnToNoise:
                    Anim("Idle");
                    timer -= dt;
                    if (timer <= 0) BeginInvestigate(target);
                    if (seen && susp >= T.checkThreshold) Enter(GState.Suspicious);
                    break;
                case GState.Investigate:
                    if (lookT > 0) { lookT -= dt; Anim("Idle"); if (lookT <= 0) { hunting = false; Enter(GState.Search); } }
                    else if (MoveTo(target.x, (hunting ? T.chase : T.investigate) * speedMul, dt)) lookT = T.lookAroundTime;
                    if (susp >= 1f) Enter(GState.Chase);
                    break;
                case GState.Chase:
                    if (seen)
                    {
                        lostT = 0;
                        float dx = p.transform.position.x - transform.position.x;
                        FaceTo(p.transform.position.x);
                        bool inRange = Mathf.Abs(dx) <= T.attackRange && Mathf.Abs(p.transform.position.y - (flying ? transform.position.y - 2.5f : transform.position.y)) < (flying ? 3.5f : 1.2f);
                        if (inRange && attackCd <= 0) { Enter(GState.Windup); break; }
                        if (!inRange) MoveTo(p.transform.position.x - Mathf.Sign(dx) * T.attackRange * 0.7f, T.chase, dt);
                        else Anim("Idle");
                    }
                    else
                    {
                        lostT += dt;
                        MoveTo(lastKnown.x, T.chase * 0.8f, dt);
                        if (lostT >= T.loseSightTime) { susp = Mathf.Min(susp, 0.7f); target = lastKnown; Enter(GState.Search); }
                    }
                    break;
                case GState.Windup:
                    timer -= dt;
                    Anim("Idle");
                    if (View != null && View.sr != null) View.sr.color = (Time.time * 12f % 1f) < 0.5f ? new Color(1f, 0.95f, 0.55f) : Color.white;
                    if (timer <= 0) { Enter(GState.Strike); }
                    break;
                case GState.Strike:
                    timer -= dt;
                    if (!flying && T.attackLunge > 0) MoveTo(transform.position.x + Facing * 2f, T.attackLunge / Mathf.Max(0.05f, T.attackActive), dt, false);
                    CheckStrike(p);
                    if (timer <= 0) Enter(GState.Recover);
                    break;
                case GState.Recover:
                    timer -= dt; Anim("Idle");
                    if (timer <= 0) { attackCd = T.attackCooldown; Enter(GState.Chase); }
                    break;
                case GState.Search:
                    // 마지막 발견 위치까지 가서 좌우를 살핀다
                    bool arrived = lookT > 0 || MoveTo(target.x, T.investigate * speedMul, dt);
                    if (arrived)
                    {
                        if (lookT <= 0) { lookT = T.searchTime; timer = 0.9f; }
                        lookT -= dt; timer -= dt; Anim("Idle");
                        if (timer <= 0) { timer = 1.1f; Facing = -Facing; }
                        if (lookT <= 0) { susp = 0; Enter(GState.Return); }
                    }
                    if (susp >= 1f) Enter(GState.Chase);
                    break;
                case GState.Stagger:
                    timer -= dt; Anim("Hurt");
                    if (timer <= 0) Enter(GState.Chase);
                    break;
                case GState.Jammed:
                    timer -= dt; Anim("Idle");
                    if (View != null && View.sr != null) View.sr.color = new Color(0.55f, 0.6f, 0.75f);
                    if (timer <= 0) { if (View != null && View.sr != null) View.sr.color = Color.white; susp = Mathf.Min(susp, 0.3f); Enter(resumeState == GState.Chase ? GState.Search : GState.Return); }
                    break;
                case GState.Down:
                    timer -= dt;
                    if (View != null && View.sr != null) View.sr.color = new Color(0.5f, 0.5f, 0.6f);
                    if (timer <= 0) Reboot();
                    break;
            }
            if (View != null) View.Face(Facing);
            if (cone != null)
            {
                cone.Visible = State != GState.Down && State != GState.Jammed;
                cone.Eye = Eye; cone.DirDeg = Dir; cone.HalfAngle = T.visionHalfAngle; cone.Range = T.visionRange;
                cone.State = SState;
            }
        }

        void UpdateSight(PlayerController p, float dt)
        {
            seen = false;
            if (dt <= 0 || p == null || !SensorActive) return;
            if (State == GState.Down || State == GState.Jammed) return;
            int pts = Sight.SeePlayer(Eye, Dir, T.visionHalfAngle, T.visionRange, p, out float dist);
            // 보는 앞에서 숨은 경우: 숨는 순간 시야 안이었다면 그 엄폐물을 기억하고, 바로 옆까지 오면 찾아낸다
            if (p.HiddenInCover)
            {
                if (!sawHide && Time.time - lastSeenTime < 0.35f) { sawHide = true; }
                if (sawHide && Mathf.Abs(p.transform.position.x - transform.position.x) < 1.15f && Mathf.Abs(p.transform.position.y - transform.position.y) < 1.2f)
                { pts = 3; dist = 0.8f; }
            }
            else sawHide = false;
            if (pts > 0) lastSeenTime = Time.time;
            // 등 뒤에 바짝 붙으면 기척을 느낀다 (그냥 통과는 위험하다)
            bool hiddenInCover = p.HiddenInCover || (p.Crouching && Cover.Near(p.transform.position) != null);
            bool touch = !flying && !hiddenInCover && Mathf.Abs(p.transform.position.x - transform.position.x) < 0.7f && Mathf.Abs(p.transform.position.y - transform.position.y) < 0.8f && !p.Hidden;
            var diff = ActionTuning.CurrentDifficulty;
            if (pts > 0)
            {
                seen = true; lastKnown = p.transform.position; grace = T.suspicionGrace;
                float near = Mathf.Lerp(2f, 0.6f, Mathf.Clamp01(dist / T.visionRange));
                float rate = T.suspicionRate * near * (p.Crouching ? 0.6f : 1f) * (Facility.AlarmOn ? 1.25f : 1f) * diff.suspicionMul * (pts >= 2 ? 1f : 0.7f) * (hunting ? 3f : 1f);
                susp = Mathf.Min(1f, susp + rate * dt);
            }
            else if (touch)
            {
                lastKnown = p.transform.position; grace = T.suspicionGrace;
                susp = Mathf.Min(1f, susp + (p.Crouching ? 0.25f : 0.55f) * diff.suspicionMul * dt);
                if (susp >= T.checkThreshold && State == GState.Patrol) { FaceTo(p.transform.position.x); }
            }
            else
            {
                if (grace > 0) grace -= dt;
                else if (State != GState.Chase) susp = Mathf.Max(0, susp - T.suspicionDecay * dt);
            }
        }

        void CheckStrike(PlayerController p)
        {
            if (p == null || timer < 0) return;
            if (flying)
            {
                // 드론: 아래로 그물 전류 (원뿔 앞쪽 아래)
                Vector2 d = (Vector2)p.transform.position - (Vector2)transform.position;
                if (d.y < 0.5f && d.y > -4f && Mathf.Abs(d.x) < 1.2f + Mathf.Abs(d.y) * 0.4f && Sight.Clear(transform.position, p.Chest))
                    p.Hurt(T.attackDamage, transform.position, "electric");
                return;
            }
            var box = new Rect(new Vector2(transform.position.x + (Facing > 0 ? 0 : -T.attackRange), transform.position.y), new Vector2(T.attackRange, 1.3f));
            if (box.Contains(p.Chest) || box.Contains((Vector2)p.transform.position + Vector2.up * 0.3f))
                p.Hurt(T.attackDamage, transform.position, "electric");
        }

        void Enter(GState s)
        {
            if (State == GState.Down && s != GState.Search) return;
            var prev = State;
            State = s;
            if (View != null && View.sr != null && s != GState.Windup && s != GState.Jammed && s != GState.Down) View.sr.color = Color.white;
            switch (s)
            {
                case GState.Suspicious: if (prev != GState.Suspicious) { resumeState = prev; Sfx.Play("sfx_suspect", 0.6f); } break;
                case GState.Chase:
                    hunting = false;
                    if (prev != GState.Chase && prev != GState.Recover && prev != GState.Stagger) { Sfx.Play("sfx_alert", 0.8f); susp = 1f; MarkSpotted(); Sensors.LastCallTime = Time.time; Sensors.LastCallText = "현재 위치 발각 — 경비 추격"; Sensors.AlertGuards(lastKnown, 12f, this, true); }
                    lostT = 0; break;
                case GState.Windup:
                    timer = T.attackWindup * ActionTuning.CurrentDifficulty.windupMul;
                    Sfx.Play(flying ? "sfx_charge_up" : "sfx_baton_on", 0.8f);
                    break;
                case GState.Strike:
                    timer = T.attackActive; if (View != null) View.Play("Attack", true);
                    Sfx.Play(flying ? "sfx_zap" : "sfx_baton_hit", 0.8f);
                    Fx.Spawn(flying ? "Arc" : "Spark", (Vector2)transform.position + new Vector2(Facing * T.attackRange * 0.6f, flying ? -1f : 0.7f), Facing < 0, 31, 0.5f);
                    break;
                case GState.Recover: timer = T.attackRecovery; break;
                case GState.Search: lookT = 0; timer = 0; break;
            }
        }

        static void MarkSpotted()
        {
            var g = Game.I;
            if (g != null && g.State != null && g.CurrentMapId != null) g.State.Set("spotted_" + g.CurrentMapId);
        }

        void BeginInvestigate(Vector2 pos)
        {
            target = pos; lookT = 0;
            Enter(GState.Investigate);
        }

        /// <summary>
        /// CCTV·다른 경비의 신고: 그 위치로 간다. hunt(CCTV 발각 신고)면 추적 속도로 달려오고,
        /// 도착해 루를 보면 의심이 3배로 빨리 차서 곧 추적·공격으로 넘어간다 (보지 못하면 그 자리를 수색).
        /// </summary>
        public void Alert(Vector2 pos, bool hunt = false)
        {
            if (T == null) { pendingAlert=true; lastKnown=pos; hunting=hunt; return; }
            if (!Alive || State == GState.Jammed) return;
            if (State == GState.Chase || State == GState.Windup || State == GState.Strike || State == GState.Recover) return;
            hunting = hunt;
            susp = Mathf.Max(susp, hunt ? 0.8f : T.checkThreshold + 0.05f);
            lastKnown = pos;
            FaceTo(pos.x);
            BeginInvestigate(pos);
        }

        void Hear(Noise.Event e)
        {
            if (!Alive || T == null || !present) return;
            if (State == GState.Jammed || State == GState.Stagger) return;
            if (Vector2.Distance(e.pos, transform.position) > e.radius * T.hearing) return;
            if (e.source == "step" || e.source == "land")
            {
                // 발소리는 의심만 조금 올린다 (위치를 정확히 알지 못함)
                if (State == GState.Patrol || State == GState.Return) { susp = Mathf.Min(T.checkThreshold - 0.05f, susp + 0.12f); FaceTo(e.pos.x); }
                return;
            }
            if (State == GState.Chase && seen) return;
            if (State == GState.Windup || State == GState.Strike || State == GState.Recover) return;
            target = e.pos;
            lastKnown = e.pos;
            susp = Mathf.Max(susp, 0.3f);
            FaceTo(e.pos.x);
            resumeState = State;
            State = GState.TurnToNoise; timer = 0.55f;
            Sfx.Play("sfx_suspect", 0.5f, 1.1f);
        }

        bool hunting, sawHide;
        float lastSeenTime = -99;
        public bool Hunting => hunting;

        public void TakeHit(int damage, float stagger, Vector2 from, string kind)
        {
            if (!Alive) return;
            Hp -= Mathf.Max(0, damage);
            if (View != null) View.Flash(new Color(1f, 1f, 0.6f), 0.12f);
            Sfx.Play("sfx_robot_hit", 0.8f);
            if (Hp <= 0) { KnockOut(); return; }
            // 맞았으니 공격한 쪽을 안다 (붙어 있는 루). 원거리면 맞은 방향만 안다.
            lastKnown = from; susp = 1f;
            FaceTo(from.x);
            // 경직 잠금 방지
            staggerTimes.RemoveAll(t => Time.time - t > T.staggerWindow);
            if (staggerImmune > 0 || stagger <= 0) { if (State != GState.Windup && State != GState.Strike) Enter(GState.Chase); return; }
            staggerTimes.Add(Time.time);
            if (staggerTimes.Count >= T.maxStaggers) { staggerImmune = T.staggerImmune; staggerTimes.Clear(); }
            State = GState.Stagger; timer = stagger;
            if (!flying) transform.position += new Vector3(Mathf.Sign(transform.position.x - from.x) * 0.25f, 0, 0);
        }

        public bool StaggerImmune => staggerImmune > 0;

        /// <summary>약한 간섭: 잠깐 멈춤 (조용함).</summary>
        public void Jam(float seconds)
        {
            if (!Alive) return;
            resumeState = State;
            State = GState.Jammed; timer = Mathf.Max(seconds, T.jamTime);
            Sfx.Play("sfx_power_down", 0.4f, 1.6f);
        }

        public void KnockOut()
        {
            State = GState.Down; timer = T.rebootTime; susp = 0; seen = false;
            if (View != null) View.Play("Death", true);
            Sfx.Play("sfx_power_down", 0.7f);
            Fx.Spawn("Spark", Center, false, 31, 0.6f);
            Game.I?.State?.Set("guard_down_any");
        }

        void Reboot()
        {
            Hp = T.hp; susp = 0.5f; State = GState.Search; target = transform.position; lookT = 0;
            if (View != null) { View.sr.color = Color.white; View.Play("Idle", true); }
            Sfx.Play("sfx_pc_on", 0.5f, 0.8f);
        }

        // ------------------------------------------------------------ 이동
        bool MoveTo(float x, float speed, float dt, bool anim = true)
        {
            float dx = x - transform.position.x;
            if (Mathf.Abs(dx) < 0.06f) { if (anim) Anim("Idle"); return true; }
            int dir = dx > 0 ? 1 : -1;
            if (anim) Facing = dir;
            float step = Mathf.Min(Mathf.Abs(dx), speed * dt);
            Vector2 pos = transform.position;
            Vector2 next = pos + new Vector2(dir * step, 0);
            // 벽과 낭떠러지에서 멈춘다 (드론은 벽만)
            Vector2 probe = next + new Vector2(dir * 0.45f, flying ? 0 : 0.6f);
            if (Blocked(pos + new Vector2(0, flying ? 0 : 0.6f), probe)) { if (anim) Anim("Idle"); return true; }
            if (!flying && !GroundAt(next.x + dir * 0.35f)) { if (anim) Anim("Idle"); return true; }
            if (!flying && InNoGo(next + new Vector2(dir * 0.3f, 0.3f)) && !InNoGo(pos + new Vector2(0, 0.3f))) { if (anim) Anim("Idle"); return true; }
            transform.position = new Vector3(next.x, flying ? homeY + Mathf.Sin(Time.time * 2f) * 0.08f : pos.y, 0);
            if (anim) Anim("Move");
            return Mathf.Abs(x - next.x) < 0.06f;
        }

        static readonly RaycastHit2D[] mh = new RaycastHit2D[8];
        /// <summary>벽에는 막히고, 낮은 엄폐 상자는 지나간다 (상자 앞뒤 깊이로 지나가는 것으로 본다 — 경비가 상자에 갇히지 않게).</summary>
        static bool Blocked(Vector2 a, Vector2 b)
        {
            Vector2 d = b - a; float len = d.magnitude; if (len < 0.001f) return false;
            var f = new ContactFilter2D { useTriggers = false }; f.SetLayerMask(Physics2D.AllLayers);
            int n = Physics2D.Raycast(a, d / len, f, mh, len);
            for (int i = 0; i < n; i++)
            {
                var c = mh[i].collider;
                if (c.GetComponent<Cover>() != null || c.GetComponent<OneWayPlatform>() != null || c.GetComponentInParent<PlayerController>() != null) continue;
                return true;
            }
            return false;
        }

        static readonly RaycastHit2D[] gh = new RaycastHit2D[6];
        bool GroundAt(float x)
        {
            var f = new ContactFilter2D { useTriggers = false }; f.SetLayerMask(Physics2D.AllLayers);
            int n = Physics2D.Raycast(new Vector2(x, transform.position.y + 0.3f), Vector2.down, f, gh, 0.8f);
            for (int i = 0; i < n; i++) if (gh[i].collider.GetComponentInParent<PlayerController>() == null) return true;
            return false;
        }

        void FaceTo(float x) { if (Mathf.Abs(x - transform.position.x) > 0.05f) Facing = x > transform.position.x ? 1 : -1; }
        void Anim(string s) { if (View != null && View.Current != "Death") View.Play(s); }

        // ------------------------------------------------------------ 지형 효과 (4~6장)
        /// <summary>경비가 걸어 들어가지 않는 바닥 (산성 약품이 고인 곳 등). 루는 막지 않는다. 맵이 바뀌면 비운다.</summary>
        public static readonly System.Collections.Generic.List<Rect> NoGo = new System.Collections.Generic.List<Rect>();
        static bool InNoGo(Vector2 p) { foreach (var r in NoGo) if (r.Contains(p)) return true; return false; }

        /// <summary>발밑이 미는 힘 (컨베이어). 벽·낭떠러지 앞에서는 멈춘다. 날아다니는 드론은 밀리지 않는다.</summary>
        public void Nudge(float dx)
        {
            if (flying || !present || Mathf.Abs(dx) < 1e-5f) return;
            Vector2 pos = transform.position;
            int dir = dx > 0 ? 1 : -1;
            Vector2 next = pos + new Vector2(dx, 0);
            if (Blocked(pos + new Vector2(0, 0.6f), next + new Vector2(dir * 0.45f, 0.6f))) return;
            if (!GroundAt(next.x + dir * 0.35f)) return;
            transform.position = new Vector3(next.x, pos.y, 0);
        }

        /// <summary>테스트용: 상태를 바로 정한다.</summary>
        public void DebugSet(GState s, float susp01) { State = s; susp = susp01; }
    }
}
