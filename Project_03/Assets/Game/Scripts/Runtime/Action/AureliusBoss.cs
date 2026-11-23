using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 10장 전이 장치실 — 아우렐리우스 (GDD 10장 최종전).
    ///  1단계 외피와 호위 장치: 호위 구 둘이 외피를 지킨다(살아 있으면 본체 피해 절반). 정비용 전자석을 켜면 강자성 호위 구가 붙들려 5초 멈춤.
    ///  2단계 축전·냉각: 양옆 축전 모듈이 충전하면 바닥 방전(점프). 축전 모듈을 공격하거나 외부 전력으로 과부하시키면 결합부 노출(빈틈),
    ///                  냉각 밸브를 닫으면 다음 충전이 과열로 끊겨 빈틈.
    ///  3단계 전이 결합부: 결합부가 드러남. 연결 광선(1.2초 띠 — 대시로 벗어남)과 떨어지는 구조물(1초 표식).
    /// 각 단계 HP 가 0 이 되면 '한계 방출' 대기: 결합부 앞에서 조사 키(Space). 전하가 가득해야 한다 (자연 충전·전하 모으기, 시간 제한 없음).
    /// 방출마다 GameState.ApplyLimitRelease(n) — 최초 최대 안정도의 20%씩 영구 감소, 같은 번호는 한 번만. 단계 전환 때 안정도·전하 회복.
    /// 부상한 라비엘이 9초마다 엄호막(다음 피해 한 번 막음). 메이린은 전투 대상이 아니다 (별도 인물).
    /// </summary>
    public class AureliusBoss : BossBase, IArenaUser
    {
        public enum AState { Idle, BeamWindup, Beam, WaveWindup, Wave, LinkWindup, Link, Exposed, AwaitRelease, Done }
        public AState State { get; private set; } = AState.Idle;
        public int Phase { get; private set; } = 1;
        public float floorY;
        public Transform magnetAt, couplingAt, ravielAt;
        public Transform[] capacitorAt;
        public SpriteRenderer couplingArt; public Sprite[] couplingFrames;
        public const int PhaseHp = 30;
        readonly List<Escort> escorts = new List<Escort>();
        readonly List<Capacitor> caps = new List<Capacitor>();
        float timer, magnetUntil = -1, supportT = 4f, debrisT = 3f;
        bool valveClosed;
        float beamX0, beamX1, waveX, waveDir;
        ArenaSwitch limitSwitch;
        public int ReleasesDoneHere { get; private set; }
        public bool AwaitingRelease => State == AState.AwaitRelease;
        public System.Action<int> OnPhaseCleared;

        static readonly string[] phaseNames = { "", "1단계 · 외피와 호위 장치", "2단계 · 축전·냉각", "3단계 · 전이 결합부" };
        public override string BarName => "아우렐리우스";
        public override string BarPhase => phaseNames[Mathf.Clamp(Phase, 1, 3)] + (State == AState.AwaitRelease ? "  ·  한계 방출 대기" : stun > 0 ? "  ·  빈틈" : EscortsShield ? "  ·  외피 보호" : "");
        public override int BarMax => PhaseHp;
        public override bool Alive => Active && !Defeated && hp > 0 && State != AState.AwaitRelease;
        public bool EscortsShield => Phase == 1 && Time.time >= magnetUntil && escorts.Exists(e => e != null && e.Alive);

        public void StartPhase(int phase)
        {
            Phase = Mathf.Clamp(phase, 1, 3);
            bossName = "아우렐리우스"; bodyHeight = 2.4f;
            base.Activate();
            hp = PhaseHp;
            ArenaSwitch.User = this; ArenaZap.User = this;
            foreach (var e in escorts) if (e != null) Destroy(e.gameObject);
            escorts.Clear();
            foreach (var c in caps) if (c != null) Destroy(c);
            caps.Clear();
            if (Phase == 1)
                for (int i = 0; i < 2; i++) escorts.Add(Escort.Make(this, i));
            if (Phase == 2 && capacitorAt != null)
                foreach (var t in capacitorAt) if (t != null) { var c = t.gameObject.GetComponent<Capacitor>(); if (c == null) c = t.gameObject.AddComponent<Capacitor>(); c.boss = this; c.hp = 6; c.broken = false; caps.Add(c); }
            if (couplingArt != null && couplingFrames != null && couplingFrames.Length > 2) couplingArt.sprite = couplingFrames[Phase == 3 ? 1 : 0];
            if (limitSwitch == null)
            {
                var go = new GameObject("LimitPoint");
                go.transform.position = new Vector3(couplingAt != null ? couplingAt.position.x : transform.position.x, floorY, 0);   // 결합부 아래 바닥 (손이 닿는 높이)
                limitSwitch = go.AddComponent<ArenaSwitch>(); limitSwitch.kind = "limit"; limitSwitch.range = 2.4f; limitSwitch.rangeY = 3f; limitSwitch.promptOffset = new Vector2(0, 2.2f); limitSwitch.id = "aur_limit";
                var bc = go.AddComponent<BoxCollider2D>(); bc.isTrigger = true; bc.size = Vector2.one;
            }
            valveClosed = false; magnetUntil = -1;
            State = AState.Idle; timer = 2.3f;
        }

        public override void Activate() => StartPhase(1);

        public override void Stop()
        {
            base.Stop();
            foreach (var e in escorts) if (e != null) Destroy(e.gameObject);
            escorts.Clear();
            if (ArenaSwitch.User == (IArenaUser)this) ArenaSwitch.User = null;
            if (ArenaZap.User == (IArenaUser)this) ArenaZap.User = null;
            EnemyShot.ClearAll();
        }

        protected override float DamageMul(string kind)
        {
            float m = stun > 0 ? 2f : 1f;
            if (EscortsShield) m *= 0.5f;
            return m;
        }

        protected override void OnZero()
        {
            // 죽이지 않는다: 결합부가 드러나고 한계 방출을 기다린다
            ClearTele(); EnemyShot.ClearAll();
            State = AState.AwaitRelease; stun = 0;
            Anim(Phase >= 2 ? "Ruined" : "Broken");
            if (couplingArt != null && couplingFrames != null && couplingFrames.Length > 2) couplingArt.sprite = couplingFrames[1];
            Sfx.Play("sfx_power_down", 0.9f, 0.7f);
            g.UI.Toast("결합부가 드러났다 — 가까이서 " + GameInput.Label(Act.Interact) + " : 한계 방출 (전하 가득 필요)", 4f);
            OnPhaseCleared?.Invoke(Phase);
        }

        void HitLu(int dmg, Vector2 from, string kind = "electric")
        {
            if (ArenaGuard.TryBlock(P.Chest)) return;
            P.Hurt(dmg, from, kind);
        }

        protected override void Update()
        {
            base.Update();
            if (!Active || P == null) return;
            float dt = Dt;
            if (dt <= 0) return;
            if (stun > 0) stun -= dt;
            // 라비엘 엄호 (부상 중에도)
            supportT -= dt;
            if (supportT <= 0 && State != AState.AwaitRelease)
            {
                supportT = 7f;
                ArenaGuard.ShieldUntil = Time.time + 6f;
                var rv = Actor.Find("raviel");
                if (rv != null) Fx.Spawn("Arc", P.Center3(), false, 31, 0.6f);
                g.UI.Toast("라비엘의 엄호 — 다음 피해 한 번을 막는다", 1.6f);
            }
            if (State == AState.AwaitRelease) { Anim(Phase >= 2 ? "Ruined" : "Broken"); return; }
            // 3단계: 떨어지는 구조물
            if (Phase == 3)
            {
                debrisT -= dt;
                if (debrisT <= 0) { debrisT = Random.Range(4.5f, 6f); StartCoroutine(Debris(Mathf.Clamp(P.transform.position.x + Random.Range(-2f, 2f), arenaMin, arenaMax))); }
            }
            switch (State)
            {
                case AState.Idle:
                    timer -= dt; Anim(Phase >= 2 ? "Broken" : "Talk"); FaceLu();
                    if (timer <= 0) Next();
                    break;
                case AState.BeamWindup:
                    timer -= dt; Anim("Windup");
                    if (timer <= 0)
                    {
                        ClearTele(); Anim("Cast"); Sfx.Play("sfx_discharge_big", 0.8f, 1.2f);
                        Fx.Spawn("Arc", new Vector2((beamX0 + beamX1) / 2f, floorY + 0.6f), false, 31, 2.5f);
                        float lx = P.transform.position.x;
                        if (lx > Mathf.Min(beamX0, beamX1) && lx < Mathf.Max(beamX0, beamX1)) HitLu(6, transform.position);
                        State = AState.Idle; timer = 2.3f;
                    }
                    break;
                case AState.WaveWindup:
                    timer -= dt; Anim("Windup");
                    if (timer <= 0) { ClearTele(); State = AState.Wave; waveX = transform.position.x; timer = 2.2f; Sfx.Play("sfx_zap", 0.9f, 0.7f); }
                    break;
                case AState.Wave:
                    {
                        timer -= dt; float prev = waveX; waveX += waveDir * 8f * dt;
                        if (Random.value < 0.6f) Fx.Spawn("Spark", new Vector2(waveX, floorY + 0.2f), false, 31, 0.5f);
                        float lx = P.transform.position.x;
                        if ((prev - lx) * (waveX - lx) <= 0 && P.transform.position.y < floorY + 0.7f) HitLu(5, new Vector2(prev, floorY));
                        if (timer <= 0) { State = AState.Idle; timer = 2.2f; }
                    }
                    break;
                case AState.LinkWindup:
                    timer -= dt; Anim("Windup");
                    if (timer <= 0)
                    {
                        ClearTele(); Anim("Cast"); Sfx.Play("sfx_transfer", 0.9f);
                        float lx = P.transform.position.x;
                        if (lx > Mathf.Min(beamX0, beamX1) && lx < Mathf.Max(beamX0, beamX1) && !P.Dodging)
                        {
                            HitLu(4, couplingAt != null ? (Vector2)couplingAt.position : (Vector2)transform.position);
                            float cx = couplingAt != null ? couplingAt.position.x : transform.position.x;
                            P.Teleport(new Vector2(Mathf.MoveTowards(lx, cx, 2f), P.transform.position.y));
                            if (g.State.Once("aur_link_hint")) g.Monologue("끌려간다...! 띠가 보이면 " + GameInput.Label(Act.Dodge) + " 로 빠져나가!", 2.6f);
                        }
                        State = AState.Idle; timer = 2.2f;
                    }
                    break;
                case AState.Exposed:
                    timer -= dt; Anim("Hurt");
                    if (timer <= 0) { stun = 0; State = AState.Idle; timer = 1.0f; }
                    break;
            }
        }

        void Next()
        {
            FaceLu();
            int r = Random.Range(0, 3);
            if (Phase == 2)
            {
                // 축전 모듈 충전 → 바닥 방전
                if (valveClosed) { valveClosed = false; Expose(3.5f, "냉각 차단 · 과열"); return; }
                waveDir = Mathf.Sign(DxToLu); if (waveDir == 0) waveDir = 1;
                State = AState.WaveWindup; timer = 1.0f;
                FloorTele(transform.position.x, transform.position.x + waveDir * 16f, floorY, TeleBlue);
                if (r == 0) return;
            }
            if (Phase == 3 && r != 0)
            {
                beamX0 = P.transform.position.x - 1.6f; beamX1 = P.transform.position.x + 1.6f;
                State = AState.LinkWindup; timer = 1.2f;
                FloorTele(beamX0, beamX1, floorY, new Color(0.8f, 0.5f, 1f, 0.35f));
                return;
            }
            if (Phase != 2)
            {
                // 설득의 빛: 반쪽 바닥
                bool left = P.transform.position.x < (arenaMin + arenaMax) / 2f;
                beamX0 = left ? arenaMin : (arenaMin + arenaMax) / 2f; beamX1 = left ? (arenaMin + arenaMax) / 2f : arenaMax;
                State = AState.BeamWindup; timer = 1.0f;
                FloorTele(beamX0, beamX1, floorY, TeleYellow);
            }
        }

        IEnumerator Debris(float x)
        {
            ColumnTele(x, floorY, 6f, 1.2f, TeleRed);
            for (float t = 0; t < 1.0f; t += Hazard.Dt) yield return null;
            ClearTeleAt(x);
            Fx.Spawn("Smoke", new Vector2(x, floorY + 0.3f), false, 31, 0.9f);
            Sfx.Play("sfx_debris", 0.7f);
            if (P != null && Mathf.Abs(P.transform.position.x - x) < 0.8f) HitLu(5, new Vector2(x, floorY + 3f), "physical");
        }

        void ClearTeleAt(float x) { /* 낙하 표식은 다음 공격 표식 정리 때 함께 지운다 */ ClearTele(); }

        public void Expose(float s, string why)
        {
            if (State == AState.AwaitRelease) return;
            ClearTele();
            stun = s; State = AState.Exposed; timer = s;
            Sfx.Play("sfx_guard_break", 0.9f);
            g.UI.Toast("빈틈 — " + why, 1.8f);
        }

        // ------------------------------------------------------------ 기물
        public bool CanUse(ArenaSwitch s)
        {
            switch (s.kind)
            {
                case "limit": return State == AState.AwaitRelease;
                case "magnet": return Active && Phase == 1 && Time.time >= magnetUntil;
                case "valve": return Active && Phase == 2 && !valveClosed;
            }
            return false;
        }

        public string PromptFor(ArenaSwitch s)
        {
            switch (s.kind)
            {
                case "limit": return g.State.D.lu.charge >= g.State.MaxCharge("lu") ? "한계 방출" : "한계 방출 (전하 " + g.State.D.lu.charge + "/" + g.State.MaxCharge("lu") + ")";
                case "magnet": return "정비용 전자석";
                case "valve": return "냉각 밸브 닫기";
            }
            return s.promptText;
        }

        public IEnumerator Use(ArenaSwitch s)
        {
            switch (s.kind)
            {
                case "limit":
                    if (g.State.D.lu.charge < g.State.MaxCharge("lu"))
                    {
                        yield return g.Info("전하가 모자라다 (" + g.State.D.lu.charge + "/" + g.State.MaxCharge("lu") + "). 기다리면 저절로 차고, '전하 모으기'로 더 빨리 모을 수 있다. 시간 제한은 없다.");
                        yield break;
                    }
                    yield return StoryEvents.LimitRelease(g, this, Phase);
                    break;
                case "magnet":
                    magnetUntil = Time.time + 8f;
                    Sfx.Play("sfx_charge_up", 0.8f, 0.6f);
                    g.UI.Toast("전자석 작동 — 호위 장치가 붙들렸다 (8초, 붙들린 구는 톡!으로 부술 수 있다)", 2.4f);
                    g.State.Set("aur_used_magnet");
                    yield return null;
                    break;
                case "valve":
                    valveClosed = true;
                    Sfx.Play("sfx_metal_creak", 0.8f);
                    g.UI.Toast("냉각 밸브 닫음 — 다음 충전이 과열된다", 2f);
                    g.State.Set("aur_used_valve");
                    yield return null;
                    break;
            }
        }

        public void CapacitorZap(ArenaZap z)
        {
            if (!Active || Phase != 2) return;
            Fx.Spawn("Explosion", z.AimPoint, false, 31, 0.9f);
            g.State.Set("aur_used_capacitor");
            TakeHitRaw(6);
            Expose(4f, "축전 모듈 과부하");
        }

        public void CapacitorBroken(Capacitor c)
        {
            g.State.Set("aur_used_capacitor");
            Fx.Spawn("Explosion", c.transform.position + Vector3.up, false, 31, 0.9f);
            Expose(3.5f, "축전 모듈 파손");
        }

        void TakeHitRaw(int d) { if (Alive) { hp = Mathf.Max(0, hp - d); if (hp <= 0) OnZero(); } }

        /// <summary>한계 방출 뒤 다음 단계로 (연출에서 부른다).</summary>
        public void AfterRelease(int phaseCleared)
        {
            ReleasesDoneHere++;
            if (phaseCleared >= 3) { State = AState.Done; Defeated = true; Active = false; Stop(); OnDefeated?.Invoke(); return; }
            StartPhase(phaseCleared + 1);
        }

        public Vector2 MagnetPos => magnetAt != null ? (Vector2)magnetAt.position : (Vector2)transform.position;
        public bool MagnetOn => Time.time < magnetUntil;

        // ------------------------------------------------------------ 호위 구
        public class Escort : MonoBehaviour, IDamageable
        {
            public AureliusBoss boss; public int idx; public int hp = 4; float fireT; SpriteRenderer sr; Sprite[] frames;
            public bool Alive => hp > 0 && boss != null && boss.Active;
            public Vector2 Center => transform.position;
            void OnEnable() { Damageables.Add(this); }
            void OnDisable() { Damageables.Remove(this); }
            public static Escort Make(AureliusBoss b, int i)
            {
                var go = new GameObject("Escort" + i);
                var e = go.AddComponent<Escort>(); e.boss = b; e.idx = i; e.fireT = 2f + i * 1.3f;
                e.sr = go.AddComponent<SpriteRenderer>(); e.sr.sortingOrder = 12;
                var set = Game.I.Config.GetSet("U_Escort"); e.frames = set != null ? set.frames : null;
                if (e.frames != null && e.frames.Length > 0) e.sr.sprite = e.frames[0];
                go.transform.position = b.transform.position + new Vector3(i == 0 ? -1.5f : 1.5f, 2.5f, 0);
                return e;
            }
            public void TakeHit(int damage, float stagger, Vector2 from, string kind)
            {
                hp -= Mathf.Max(1, damage);
                Sfx.Play("sfx_robot_hit", 0.6f, 1.3f);
                if (hp <= 0) { Fx.Spawn("Explosion", transform.position, false, 31, 0.6f); Destroy(gameObject); }
            }
            void Update()
            {
                if (boss == null) { Destroy(gameObject); return; }
                float dt = Hazard.Dt;
                if (dt <= 0) return;
                Vector3 target;
                if (boss.MagnetOn) target = (Vector3)boss.MagnetPos + new Vector3(idx * 0.9f - 0.45f, 0.7f, 0);
                else { float a = Time.time * 1.3f + idx * Mathf.PI; target = boss.transform.position + new Vector3(Mathf.Cos(a) * 1.8f, 2.4f + Mathf.Sin(a) * 0.5f, 0); }
                transform.position = Vector3.MoveTowards(transform.position, target, (boss.MagnetOn ? 9f : 4f) * dt);
                if (boss.MagnetOn || !boss.Active || boss.AwaitingRelease) { if (frames != null && frames.Length > 1) sr.sprite = frames[1]; return; }
                fireT -= dt;
                if (frames != null && frames.Length > 1) sr.sprite = frames[fireT < 0.7f && (Time.time * 10 % 1f) < 0.5f ? 1 : 0];
                if (fireT <= 0)
                {
                    fireT = 4f;
                    var p = Game.I.Player; if (p == null) return;
                    Vector2 dir = ((Vector2)p.Chest - (Vector2)transform.position).normalized;
                    EnemyShot.Fire(transform.position, dir * 5f, 3, "electric", 3f);
                    Sfx.Play("sfx_shot_enemy", 0.6f);
                }
            }
        }

        /// <summary>축전 모듈: 일반 공격으로도 부술 수 있다 (HP 6).</summary>
        public class Capacitor : MonoBehaviour, IDamageable
        {
            public AureliusBoss boss; public int hp = 6; public bool broken;
            public bool Alive => !broken && boss != null && boss.Active && boss.Phase == 2;
            public Vector2 Center => (Vector2)transform.position + new Vector2(0, 0.8f);
            void OnEnable() { Damageables.Add(this); }
            void OnDisable() { Damageables.Remove(this); }
            public void TakeHit(int damage, float stagger, Vector2 from, string kind)
            {
                if (!Alive) return;
                hp -= Mathf.Max(1, damage);
                Sfx.Play("sfx_robot_hit", 0.6f, 0.9f);
                if (hp <= 0) { broken = true; boss.CapacitorBroken(this); }
            }
        }
    }

    public static class PlayerExt
    {
        public static Vector3 Center3(this PlayerController p) => p != null ? (Vector3)p.Chest : Vector3.zero;
    }
}
