using UnityEngine;

namespace EW
{
    /// <summary>
    /// 베넬의 반응 축 (HUD 에 수치를 보이지 않는다 — 표정·말투·공격 중단/재개로 드러난다). 저장되는 값: GameState vars.
    ///  - 위협(v_threat 0~10): 루를 위협으로 보는 정도. 루가 때리면 오르고, 공격을 멈추고 지켜보면 조금씩 내려간다.
    ///  - 신뢰(v_trust 0~10)  : 루의 말을 믿는 정도. 처음 듣는 설득·위험 장치를 꺼 준 행동으로 오른다.
    ///  - 흥분(v_agit 0~10)   : 혼란·흥분. 자극하는 말과 빈틈에 맞은 일로 오르고, 시간이 지나면 가라앉는다. 높으면 공격이 빨라진다.
    /// 같은 말·행동의 효과는 한 번만 (v_opt_* 기록) — 반복해서 무한히 올릴 수 없다. 반복하면 '아까 그 말' 반응만 나온다.
    /// 틀린 선택을 해도 진행이 처음으로 돌아가지 않는다 (축이 조금 나빠질 뿐, 다음 기회가 온다).
    /// 협상 성립: 신뢰 ≥ 4 이고 위협 ≤ 2 인 상태에서 대화를 마치면 공격을 멈춘다.
    /// </summary>
    public static class VenelleMind
    {
        public const int TrustToStop = 4, ThreatToStop = 2;
        static GameState S => Game.I != null ? Game.I.State : null;
        public static int Threat => S != null ? S.GetVar("v_threat") : 0;
        public static int Trust => S != null ? S.GetVar("v_trust") : 0;
        public static float Agitation => S != null ? S.GetVar("v_agit") : 0;
        public static bool WillStop => Trust >= TrustToStop && Threat <= ThreatToStop;

        static void Add(string k, int d, int min = 0, int max = 10) { var s = S; if (s == null) return; s.SetVar(k, Mathf.Clamp(s.GetVar(k) + d, min, max)); }
        public static void AddThreat(int d) => Add("v_threat", d);
        public static void AddTrust(int d) => Add("v_trust", d);
        public static void AddAgit(int d) => Add("v_agit", d);

        /// <summary>전투 시작 기본값 (처음 만났을 때만).</summary>
        public static void InitIfNeeded()
        {
            var s = S; if (s == null || s.Has("v_mind_init")) return;
            s.Set("v_mind_init");
            s.SetVar("v_threat", 5); s.SetVar("v_trust", 0); s.SetVar("v_agit", 4);
        }

        static float calmT, agitT;
        /// <summary>전투 중 시간 경과: 루가 때리지 않고 가까이서 버티면 위협이 조금 내려간다 (최대 3까지만, 12초마다 1).</summary>
        public static void Tick(float dt, VenelleBoss b)
        {
            var s = S; if (s == null || b == null) return;
            agitT += dt;
            if (agitT > 8f) { agitT = 0; if (Agitation > 2) AddAgit(-1); }
            var p = Game.I.Player;
            bool near = p != null && Mathf.Abs(p.transform.position.x - b.transform.position.x) < 6.5f;
            if (near && Time.time - b.LastHitByLuTime > 12f && s.GetVar("v_calm_gain") < 3)
            {
                calmT += dt;
                if (calmT > 12f) { calmT = 0; AddThreat(-1); s.SetVar("v_calm_gain", s.GetVar("v_calm_gain") + 1); b.Say(Random.value < 0.5f ? "...안 때리네...?" : "흐응... 이상한 꼬맹이...", 2f); }
            }
            else calmT = 0;
        }

        /// <summary>루에게 맞음: 위협↑. 숨을 고르거나 섞는 빈틈에 맞으면 신뢰도 조금 깎인다.</summary>
        public static void OnHit(bool duringOpening)
        {
            AddThreat(1); AddAgit(1);
            if (duringOpening && S != null && S.Once("v_hit_in_opening")) AddTrust(-1);
        }

        /// <summary>위험 장치(과부하 분전함)를 꺼 준 행동: 신뢰 +2 (한 번).</summary>
        public static void OnJunctionOff()
        {
            if (S != null && S.Once("v_act_junction")) { AddTrust(2); AddThreat(-1); }
        }

        public static void Reset()
        {
            var s = S; if (s == null) return;
            foreach (var k in new[] { "v_threat", "v_trust", "v_agit", "v_calm_gain" }) s.SetVar(k, 0);
            s.Set("v_mind_init", false);
        }
    }
}
