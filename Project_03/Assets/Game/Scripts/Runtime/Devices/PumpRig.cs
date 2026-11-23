using System.Collections;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 물 펌프실 (GDD 4장): 고인 물을 빼고 → 전원을 내린 뒤 → 깨진 이음쇠를 새 부품으로 바꾸고 → 전원을 올리면 펌프가 돈다.
    /// 상태는 화면에 그대로 보인다: 수위(물 높이·상태판 파란 램프), 전원(차단기 손잡이·램프·펌프 웅웅거림), 부품(이음쇠 색·불꽃).
    /// 잘못된 순서는 막히지 않고 결과로 알려 준다 (물 위로 전원 → 차단기가 튐, 전원 켠 채 부품 → 불꽃만 튀고 손을 뗌, 깨진 채 가동 → 헛돎).
    /// 어떤 조작도 되돌릴 수 있고 부품을 잃지 않는다. 상태 저장: var pump_water(0~100), 플래그 pump_valve_open / pump_power_on / pump_part_fixed / waste_pump_fixed.
    /// 이 방은 실제 물 펌프가 있는 기계실이며, 파일 공간·휴지통의 장치가 아니다.
    /// </summary>
    public class PumpRig : MonoBehaviour
    {
        public static PumpRig I { get; private set; }
        public enum Role { Valve, Breaker, Coupling, Board, Shelf }

        public float basinX0, basinX1, basinBottom, basinFull;
        public SpriteRenderer waterBody, waterTop, pump, breaker, valve;
        public Sprite[] pumpFrames, breakerFrames, valveFrames, waterTopFrames;
        public SpriteRenderer lampWater, lampPower, lampPart;
        public SpriteRenderer pipeFlow;
        public Vector2 couplingPos;
        public float drainSeconds = 3.2f;
        float anim, sparkT, valveAnim;

        void OnEnable() { I = this; }
        void OnDisable() { if (I == this) I = null; }

        static GameState S => Game.I != null ? Game.I.State : null;
        public static int Water { get { var s = S; if (s == null) return 100; return s.Has("pump_water_set") ? s.GetVar("pump_water") : 100; } set { var s = S; if (s == null) return; s.Set("pump_water_set"); s.SetVar("pump_water", Mathf.Clamp(value, 0, 100)); } }
        public static bool ValveOpen => S != null && S.Has("pump_valve_open");
        public static bool PowerOn => S != null && S.Has("pump_power_on");
        public static bool PartFixed => S != null && S.Has("pump_part_fixed");
        public static bool Wet => Water > 15;
        public static bool Running => PowerOn && PartFixed && !Wet;

        float waterF = -1;

        void Update()
        {
            var s = S; if (s == null) return;
            float dt = Hazard.Dt;
            if (waterF < 0) waterF = Water;
            if (ValveOpen && waterF > 0) { waterF = Mathf.Max(0, waterF - dt * 100f / drainSeconds); Water = Mathf.RoundToInt(waterF); }
            else waterF = Water;
            anim += dt;
            // 물: 높이와 표면
            float k = waterF / 100f;
            float top = Mathf.Lerp(basinBottom, basinFull, k);
            if (waterBody != null)
            {
                waterBody.enabled = k > 0.01f;
                waterBody.size = new Vector2(basinX1 - basinX0, Mathf.Max(0.01f, top - basinBottom));
                waterBody.transform.position = new Vector3((basinX0 + basinX1) / 2f, (basinBottom + top) / 2f, 0);
            }
            if (waterTop != null)
            {
                waterTop.enabled = k > 0.01f;
                waterTop.transform.position = new Vector3((basinX0 + basinX1) / 2f, top, 0);
                if (waterTopFrames != null && waterTopFrames.Length > 0) waterTop.sprite = waterTopFrames[(int)(anim * 3f) % waterTopFrames.Length];
            }
            // 펌프: 0 고장 정지 / 1 정상 정지 / 2·3 가동
            if (pump != null && pumpFrames != null && pumpFrames.Length >= 4)
                pump.sprite = Running ? pumpFrames[2 + (int)(anim * 8f) % 2] : PartFixed ? pumpFrames[1] : pumpFrames[0];
            if (breaker != null && breakerFrames != null && breakerFrames.Length >= 2) breaker.sprite = breakerFrames[PowerOn ? 1 : 0];
            if (valve != null && valveFrames != null && valveFrames.Length >= 3)
            {
                if (ValveOpen && waterF > 0) valveAnim += dt * 6f;
                valve.sprite = valveFrames[(int)valveAnim % 3];
            }
            // 상태판 램프
            if (lampWater != null) lampWater.color = k > 0.15f ? new Color(0.3f, 0.6f, 1f) : new Color(0.25f, 0.3f, 0.35f);
            if (lampPower != null) lampPower.color = PowerOn ? new Color(0.35f, 1f, 0.45f) : new Color(1f, 0.3f, 0.25f);
            if (lampPart != null) lampPart.color = PartFixed ? new Color(0.35f, 1f, 0.45f) : new Color(1f, 0.55f, 0.2f);
            if (pipeFlow != null) { pipeFlow.enabled = Running; pipeFlow.color = new Color(0.5f, 0.8f, 1f, 0.55f + 0.25f * Mathf.Sin(anim * 9f)); }
            // 전원이 들어온 채 깨진 이음쇠: 불꽃이 튄다
            if (PowerOn && !PartFixed && !Wet)
            {
                sparkT -= dt;
                if (sparkT <= 0) { sparkT = 0.5f; Fx.Spawn("Spark", couplingPos, false, 30, 0.45f); }
            }
        }

        // ------------------------------------------------------------ 조작
        public IEnumerator Use(Game g, Role role)
        {
            var s = g.State;
            switch (role)
            {
                case Role.Valve:
                    if (ValveOpen) { s.Set("pump_valve_open", false); Sfx.Play("sfx_metal_creak", 0.6f, 1.2f); yield return g.Say(Water > 0 ? "pump_valve_close_mid" : "pump_valve_close"); }
                    else
                    {
                        s.Set("pump_valve_open");
                        Sfx.Play("sfx_metal_creak", 0.8f, 0.9f); Sfx.Play("sfx_bubble", 0.7f);
                        yield return g.Say(Water > 0 ? "pump_valve_open" : "pump_valve_open_empty");
                    }
                    break;
                case Role.Breaker:
                    if (PowerOn)
                    {
                        s.Set("pump_power_on", false);
                        Sfx.Play("sfx_power_down", 0.7f);
                        yield return g.Say("pump_power_off");
                        break;
                    }
                    s.Set("pump_power_on");
                    Sfx.Play("sfx_ui_ok", 0.8f);
                    if (Wet)
                    {
                        // 물 위로 전원: 수면에 불꽃 → 차단기가 다시 내려간다 (피해 없음, 되돌릴 필요 없음)
                        for (int i = 0; i < 5; i++) Fx.Spawn("Spark", new Vector2(Random.Range(basinX0, basinX1), Mathf.Lerp(basinBottom, basinFull, Water / 100f)), false, 30, 0.5f);
                        Sfx.Play("sfx_zap", 0.9f, 0.8f);
                        yield return new WaitForSeconds(0.4f);
                        s.Set("pump_power_on", false);
                        Sfx.Play("sfx_power_down", 0.8f, 1.3f);
                        StoryEvents.Onoma(g, (Vector2)breaker.transform.position + Vector2.up * 1.4f, "탁!", new Color(1f, 0.6f, 0.4f));
                        s.SetVar("pump_trips", s.GetVar("pump_trips") + 1);
                        yield return g.Say("pump_trip");
                    }
                    else if (!PartFixed)
                    {
                        Sfx.Play("sfx_clatter", 0.8f);
                        Cutscene.Shake(0.03f, 0.5f);
                        yield return new WaitForSeconds(0.6f);
                        yield return g.Say("pump_rattle");
                    }
                    else yield return StoryEvents.PumpStarted(g);
                    break;
                case Role.Coupling:
                    if (PartFixed) { yield return g.Say("pump_part_ok"); break; }
                    if (Wet) { yield return g.Say("pump_coupling_wet"); break; }
                    if (PowerOn)
                    {
                        Fx.Spawn("Spark", couplingPos, false, 31, 0.7f); Sfx.Play("sfx_spark_small", 0.9f);
                        yield return g.Say("pump_coupling_live");
                        break;
                    }
                    if (s.Count("pump_part") <= 0) { yield return g.Say(s.Has("pump_shelf_seen") ? "pump_need_part_seen" : "pump_need_part"); break; }
                    s.UseItem("pump_part");
                    s.Set("pump_part_fixed");
                    Sfx.Play("sfx_metal_creak", 0.8f, 1.4f); Sfx.Play("sfx_ui_ok", 0.6f);
                    yield return g.Say("pump_part_done");
                    break;
                case Role.Shelf:
                    s.Set("pump_shelf_seen");
                    if (!s.Has("pump_part_taken"))
                    {
                        s.Set("pump_part_taken"); s.AddItem("pump_part");
                        Sfx.Play("sfx_ui_ok", 0.7f);
                        yield return g.Say("pump_shelf");
                        g.UI.Toast("이음쇠 부품을 챙겼다.", 2f);
                    }
                    else yield return g.Say("pump_shelf_empty");
                    break;
                case Role.Board:
                    yield return g.Info("수위: " + (Water > 15 ? "가득 (" + Water + "%)" : Water > 0 ? "거의 빠짐" : "비었음")
                        + (ValveOpen ? " · 배수 밸브 열림" : " · 배수 밸브 닫힘")
                        + "\n전원: " + (PowerOn ? "켜짐" : "꺼짐")
                        + "\n이음쇠: " + (PartFixed ? "새 부품" : "깨짐") + (Running ? "\n펌프 가동 중 → 거처 물통으로" : ""));
                    break;
            }
        }
    }
}
