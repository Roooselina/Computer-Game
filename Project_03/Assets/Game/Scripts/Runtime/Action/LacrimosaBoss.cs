using System.Collections;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 9장 공명 공연장 — 라크리모사 (GDD 3.6·9장).
    /// 공격: 울림파(바닥을 따라 퍼지는 파동, 0.9초 준비 · 점프로 넘음) / 공명 기둥(켜진 스피커 자리에 0.8초 표식).
    /// 해법: 믹서에서 마이크 방향·스피커 경로·입력 게인을 맞추면 피드백 게이지가 차고, 가득 차면 장치가 '보호 정지' → 4초 빈틈.
    ///  게이지·마이크→스피커 연결선·글자 표시가 있어 소리 없이도 풀 수 있다. 일반 공격만으로도 이긴다.
    /// 녹음 재생(선택 녹음실의 테이프)은 약점이 아니라 기억에 반응해 잠시 공격을 멈추는 인물 행동이다.
    /// 빈틈·멈춤 중 가까이서 말을 걸면 설득 분기, HP 0 이면 제압 분기.
    /// </summary>
    public class LacrimosaBoss : BossBase, IArenaUser
    {
        public enum LState { Idle, Sing, WaveWindup, Wave, PillarWindup, Pillar, Stopped, Listening, Recover }
        public LState State { get; private set; } = LState.Idle;
        public Transform mic; public SpriteRenderer micArt; public Sprite[] micFrames;
        public Transform speakerL, speakerR; public SpriteRenderer spkLArt, spkRArt; public Sprite[] spkFrames;
        public SpriteRenderer mixerArt; public Sprite[] mixerFrames;
        public Transform gaugeAt;
        public float floorY;

        // 믹서 상태
        public int MicDir { get; private set; } = 0;        // 0 관객석, 1 왼쪽 스피커, 2 오른쪽 스피커
        public bool SpkL { get; private set; } = true;
        public bool SpkR { get; private set; } = true;
        public int Gain { get; private set; } = 0;          // 0 낮음, 1 보통, 2 높음
        public float Feedback { get; private set; }         // 0~1
        public int ProtectStops { get; private set; }
        public int ListenCount { get; private set; }
        public bool TalkOpen => (State == LState.Stopped || State == LState.Listening) && Active;
        float timer, waveX, waveDir; int cycle;
        LineRenderer link; SpriteRenderer gaugeBack, gaugeFill;

        public override string BarPhase => State == LState.Stopped ? "보호 정지 (빈틈)" : State == LState.Listening ? "멈춰서 듣는 중" : "피드백 " + Mathf.RoundToInt(Feedback * 100) + "%";

        public override void Activate()
        {
            bossName = "라크리모사";
            base.Activate();
            ArenaSwitch.User = this;
            State = LState.Recover; timer = 1.5f;
            BuildGauge();
            Refresh();
        }

        public override void Stop()
        {
            base.Stop();
            if (ArenaSwitch.User == (IArenaUser)this) ArenaSwitch.User = null;
            if (link != null) link.enabled = false;
        }

        void BuildGauge()
        {
            if (g == null || g.Config.pixel == null || gaugeAt == null) return;
            float s = g.Config.pixel.bounds.size.x;
            var b = new GameObject("FeedbackGaugeBack"); b.transform.position = gaugeAt.position;
            gaugeBack = b.AddComponent<SpriteRenderer>(); gaugeBack.sprite = g.Config.pixel; gaugeBack.sortingOrder = 33; gaugeBack.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);
            b.transform.localScale = new Vector3(2.4f / s, 0.22f / s, 1);
            var f = new GameObject("FeedbackGaugeFill"); f.transform.position = gaugeAt.position;
            gaugeFill = f.AddComponent<SpriteRenderer>(); gaugeFill.sprite = g.Config.pixel; gaugeFill.sortingOrder = 34;
            var lgo = new GameObject("MicLink");
            link = lgo.AddComponent<LineRenderer>(); link.positionCount = 2; link.startWidth = link.endWidth = 0.06f; link.sortingOrder = 33;
            link.material = new Material(Shader.Find("Sprites/Default"));
        }

        Transform Target => MicDir == 1 ? speakerL : MicDir == 2 ? speakerR : null;
        bool TargetOn => MicDir == 1 ? SpkL : MicDir == 2 ? SpkR : false;
        /// <summary>초당 피드백 상승량 (마이크가 켜진 스피커를 향하고 게인이 보통 이상일 때만).</summary>
        public float FeedbackRate => (Target != null && TargetOn && Gain >= 1) ? (Gain == 2 ? 0.34f : 0.14f) : -0.2f;

        void Refresh()
        {
            if (micArt != null && micFrames != null && micFrames.Length > 1) { micArt.sprite = micFrames[MicDir == 0 ? 0 : 1]; micArt.flipX = MicDir == 1; }
            if (spkLArt != null && spkFrames != null && spkFrames.Length > 1) spkLArt.sprite = spkFrames[SpkL ? 1 : 0];
            if (spkRArt != null && spkFrames != null && spkFrames.Length > 1) spkRArt.sprite = spkFrames[SpkR ? 1 : 0];
            if (mixerArt != null && mixerFrames != null && mixerFrames.Length > 2) mixerArt.sprite = mixerFrames[Gain];
        }

        protected override void Update()
        {
            base.Update();
            if (!Active || P == null) return;
            // 표시 (멈춤 중에도 갱신)
            if (gaugeFill != null && gaugeAt != null)
            {
                float s = g.Config.pixel.bounds.size.x;
                gaugeFill.transform.localScale = new Vector3(2.4f * Feedback / s, 0.16f / s, 1);
                gaugeFill.transform.position = gaugeAt.position + new Vector3(-1.2f + 1.2f * Feedback, 0, 0);
                gaugeFill.color = Color.Lerp(new Color(0.4f, 0.9f, 0.5f), new Color(1f, 0.3f, 0.25f), Feedback);
            }
            if (link != null)
            {
                var t = Target;
                link.enabled = t != null && mic != null;
                if (link.enabled)
                {
                    link.SetPosition(0, mic.position + Vector3.up * 1.0f);
                    link.SetPosition(1, t.position + Vector3.up * 1.0f);
                    var c = TargetOn ? Color.Lerp(new Color(0.5f, 0.8f, 1f), new Color(1f, 0.35f, 0.3f), Feedback) : new Color(0.4f, 0.4f, 0.45f);
                    link.startColor = link.endColor = c;
                    link.startWidth = link.endWidth = 0.04f + 0.08f * Feedback;
                }
            }
            float dt = Dt;
            if (dt <= 0) return;
            if (stun > 0) stun -= dt;
            if (State != LState.Stopped)
            {
                float before = Feedback;
                Feedback = Mathf.Clamp01(Feedback + FeedbackRate * dt);
                if (before < 0.5f && Feedback >= 0.5f) g.UI.Toast("피드백 50% — 마이크가 스피커 소리를 다시 받는다", 1.6f);
                if (Feedback >= 1f) ProtectStop();
            }
            switch (State)
            {
                case LState.Recover:
                    timer -= dt; Anim("Idle"); FaceLu();
                    if (timer <= 0) NextAttack();
                    break;
                case LState.WaveWindup:
                    timer -= dt; Anim("Windup"); Blink(new Color(0.6f, 0.85f, 1f), 9f);
                    if (timer <= 0) { White(); ClearTele(); State = LState.Wave; waveX = transform.position.x; Anim("Sing"); Sfx.Play("sfx_broadcast", 0.9f, 0.7f); timer = 2.2f; }
                    break;
                case LState.Wave:
                    {
                        timer -= dt;
                        float prev = waveX;
                        waveX += waveDir * 7f * dt;
                        if (Random.value < 0.5f) Fx.Spawn("Bubbles", new Vector2(waveX, floorY + 0.3f), false, 31, 0.5f);
                        float lx = P.transform.position.x;
                        if ((prev - lx) * (waveX - lx) <= 0 && P.transform.position.y < floorY + 0.7f) P.Hurt(NearSpeaker ? 8 : 6, new Vector2(prev, floorY), "physical");
                        if (timer <= 0 || waveX < arenaMin - 1 || waveX > arenaMax + 1) { State = LState.Recover; timer = 1.3f; }
                    }
                    break;
                case LState.PillarWindup:
                    timer -= dt; Anim("Sing");
                    if (timer <= 0)
                    {
                        ClearTele(); Sfx.Play("sfx_discharge_small", 0.9f, 0.6f); Cutscene.Shake(0.04f, 0.2f);
                        foreach (var sp in new[] { (speakerL, SpkL), (speakerR, SpkR) })
                            if (sp.Item1 != null && sp.Item2)
                            {
                                Fx.Spawn("ReactBurst", sp.Item1.position + Vector3.up, false, 31, 1.2f);
                                if (Mathf.Abs(P.transform.position.x - sp.Item1.position.x) < 1.2f) P.Hurt(7, sp.Item1.position, "physical");
                            }
                        State = LState.Recover; timer = 1.4f;
                    }
                    break;
                case LState.Stopped:
                    timer -= dt; Anim("Stagger");
                    if (timer <= 0) { stun = 0; Feedback = 0; State = LState.Recover; timer = 0.8f; }
                    break;
                case LState.Listening:
                    timer -= dt; Anim("Listen");
                    if (timer <= 0) { State = LState.Recover; timer = 1.0f; }
                    break;
            }
        }

        bool NearSpeaker
        {
            get
            {
                float x = transform.position.x;
                return (SpkL && speakerL != null && Mathf.Abs(speakerL.position.x - x) < 3f) || (SpkR && speakerR != null && Mathf.Abs(speakerR.position.x - x) < 3f);
            }
        }

        void NextAttack()
        {
            cycle++;
            if ((SpkL || SpkR) && cycle % 3 == 0)
            {
                State = LState.PillarWindup; timer = 0.8f;
                foreach (var sp in new[] { (speakerL, SpkL), (speakerR, SpkR) })
                    if (sp.Item1 != null && sp.Item2) ColumnTele(sp.Item1.position.x, floorY, 3.2f, 2.4f, TeleBlue);
                return;
            }
            waveDir = Mathf.Sign(DxToLu); if (waveDir == 0) waveDir = 1;
            State = LState.WaveWindup; timer = 0.9f;
            FloorTele(transform.position.x, transform.position.x + waveDir * 14f, floorY, TeleBlue);
        }

        void ProtectStop()
        {
            ProtectStops++;
            Feedback = 0;
            ClearTele();
            stun = 6.5f; State = LState.Stopped; timer = 6.5f;
            Sfx.Play("sfx_power_down", 1f, 1.2f);
            Fx.Spawn("Smoke", Center, false, 31, 0.8f);
            if (MicDir == 1) SpkL = false; else if (MicDir == 2) SpkR = false;
            Refresh();
            g.UI.Toast("보호 정지 — 증폭 장치가 멈췄다 (6초). 가까이 가서 말을 걸 수도 있다", 2.6f);
            if (ProtectStops == 1) g.RunEvent(StoryEvents.LacrimosaStopLine(g));
        }

        protected override float DamageMul(string kind) => State == LState.Stopped ? 2f : 1f;

        public bool CanUse(ArenaSwitch s) => Active && (s.kind == "mixer" || (s.kind == "talk" && TalkOpen));
        public string PromptFor(ArenaSwitch s) => s.kind == "talk" ? "말 걸기" : "믹서";

        public IEnumerator Use(ArenaSwitch s)
        {
            if (s.kind == "talk") { yield return StoryEvents.LacrimosaTalk(g, this); yield break; }
            while (true)
            {
                string[] dirs = { "관객석", "왼쪽 스피커", "오른쪽 스피커" };
                string[] gains = { "낮음", "보통", "높음" };
                var opts = new System.Collections.Generic.List<string>
                {
                    "마이크 방향: " + dirs[MicDir] + "  → 바꾸기",
                    "왼쪽 스피커: " + (SpkL ? "켜짐" : "꺼짐"),
                    "오른쪽 스피커: " + (SpkR ? "켜짐" : "꺼짐"),
                    "입력 게인: " + gains[Gain] + "  → 올리기/내리기",
                };
                bool tape = g.State.Count("edward_tape") > 0;
                if (tape) opts.Add("녹음 재생 (선택 녹음실의 테이프)");
                opts.Add("닫기");
                g.UI.dialogue.SetBodyImmediate("", "믹서 — 피드백 " + Mathf.RoundToInt(Feedback * 100) + "%  · 켜진 스피커를 마이크가 향하고 게인이 보통 이상이면 오른다.");
                yield return g.Choose(opts);
                int r = g.LastChoice;
                if (r == 0) MicDir = (MicDir + 1) % 3;
                else if (r == 1) SpkL = !SpkL;
                else if (r == 2) SpkR = !SpkR;
                else if (r == 3) Gain = (Gain + 1) % 3;
                else if (tape && r == 4) { ListenCount++; g.UI.dialogue.Show(false); yield return StoryEvents.LacrimosaListen(g, this); State = LState.Listening; timer = 8f; ClearTele(); yield break; }
                else break;
                Sfx.Play("sfx_ui_move", 0.7f);
                Refresh();
            }
            g.UI.dialogue.Show(false);
        }

        protected override void OnZero()
        {
            base.OnZero();
            Stop();
            Anim("Down");
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (gaugeBack != null) Destroy(gaugeBack.gameObject);
            if (gaugeFill != null) Destroy(gaugeFill.gameObject);
            if (link != null) Destroy(link.gameObject);
        }

        /// <summary>테스트: 믹서 상태를 바로 맞춘다.</summary>
        public void SetMixer(int micDir, bool l, bool r, int gain) { MicDir = micDir; SpkL = l; SpkR = r; Gain = gain; Refresh(); }
        public void EndTalkWindow() { State = LState.Idle; Stop(); }
    }
}
