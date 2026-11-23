using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 실시간 잠입 개편 (2026-11-14) 사건들.
    /// - 정전된 실험실: 처음엔 적·경보 없음. 물건마다 다른 반응(짧은 말풍선 / 만지는 동작 / 단서 확대 / 소리만 / 다가가기 꺼림).
    ///   체크리스트 없이, 핵심 단서(벽 도식·통 표식·문틈 표지판) 중 하나를 보면 정체를 깨닫는다. 타이머로 경보를 켜지 않는다.
    /// - 깨달음 → 천장 조명이 차례로 켜짐 → 붉은 경광등 → 방송 (E-D05-47) → '얼른 도망쳐야 돼!'
    /// - 베넬 복도: 쫓겨 들어옴 → 숨 고르기 → 발소리 → 불균형한 걸음 → 멈춤·시선 → 웃음 → 몸 낮춤 → (조작) 첫 돌진
    /// 설계 문서: Docs/CUTSCENE_DESIGN.md, Docs/STEALTH_COMBAT_SPEC.md
    /// </summary>
    public static partial class StoryEvents
    {
        // ============================================================ 공통
        static IEnumerator Mono(Game g, string id) { g.MonologueId(id); yield break; }
        static IEnumerator MonoOnce(Game g, string id) { if (g.State.Once("mono_" + id)) g.MonologueId(id); yield break; }

        static IEnumerator CheckpointHere(Game g)
        {
            // 구간 시작점 (TriggerZone eventId = "checkpoint"): 같은 맵·같은 위치 근처면 다시 저장하지 않는다
            string key = "cp_" + g.CurrentMapId + "_" + Mathf.RoundToInt(g.Player.transform.position.x / 4f);
            if (g.State.Once(key)) g.Checkpoint(key);
            yield break;
        }

        /// <summary>단서 발견 보상 (한 번만).</summary>
        static void ClueReward(Game g, string id)
        {
            int up = g.State.Reward("clue_" + id, ActionTuning.I.expClue);
            if (up > 0) g.UI.Toast("성장: 안정도 최대치가 올랐다");
        }

        // ============================================================ 정전된 실험실: 조사 반응
        /// <summary>구속대 침대: 다가가다 멈추고 뒷걸음질 (가까이 가기 꺼림).</summary>
        static IEnumerator LabBed(Game g)
        {
            var p = g.Player;
            var bed = Marker.Find("bed");
            float bx = bed != null ? bed.transform.position.x : p.transform.position.x + 1;
            int dir = bx > p.transform.position.x ? 1 : -1;
            p.ScriptControl = true;
            p.SetFacing(dir);
            yield return Cutscene.LuMove(g, p.transform.position.x + dir * 0.35f, 1.2f, "Walk");
            p.AnimOverride = "Fear";
            Sfx.Play("sfx_heartbeat", 0.4f);
            yield return Cutscene.Wait(0.5f);
            p.AnimOverride = "BackAway";
            yield return Cutscene.LuMove(g, p.transform.position.x - dir * 0.7f, 1.6f, "BackAway");
            p.AnimOverride = null; p.ScriptControl = false;
            g.MonologueId("lab_m_bed");
            ClueReward(g, "bed");
        }

        /// <summary>의료 도구 쟁반: 손을 대는 동작 + 달그락 소리.</summary>
        static IEnumerator LabTools(Game g)
        {
            var p = g.Player;
            p.ScriptControl = true;
            p.AnimOverride = "Point";
            yield return Cutscene.Wait(0.3f);
            Sfx.Play("sfx_clatter", 0.4f, 1.3f);
            var tray = Marker.Find("tools");
            if (tray != null) g.StartCoroutine(Jiggle(tray.transform, 0.35f));
            yield return Cutscene.Wait(0.35f);
            p.AnimOverride = null; p.ScriptControl = false;
            g.MonologueId("lab_m_tools");
            ClueReward(g, "tools");
        }

        static IEnumerator Jiggle(Transform t, float time)
        {
            Vector3 b = t.localPosition;
            for (float k = 0; k < time; k += Time.deltaTime) { t.localPosition = b + new Vector3(Mathf.Round(Mathf.Sin(k * 70f) * 1.5f) / 32f, 0, 0); yield return null; }
            t.localPosition = b;
        }

        /// <summary>개조 부품 수거함: 소리만 들리는 반응.</summary>
        static IEnumerator LabBin(Game g)
        {
            var p = g.Player;
            p.ScriptControl = true; p.AnimOverride = "Idle";
            yield return Cutscene.Wait(0.4f);
            Sfx.Play("sfx_bin_twitch", 0.8f);
            var bin = Marker.Find("bin");
            if (bin != null) g.StartCoroutine(Jiggle(bin.transform, 0.25f));
            yield return Cutscene.Wait(0.25f);
            p.AnimOverride = "Fear";
            yield return Cutscene.Wait(0.5f);
            p.AnimOverride = null; p.ScriptControl = false;
            g.MonologueId("lab_m_bin");
            ClueReward(g, "bin");
        }

        static IEnumerator LabJars(Game g)
        {
            Sfx.Play("sfx_bubble", 0.6f);
            g.MonologueId("lab_m_jars");
            yield break;
        }

        static IEnumerator LabResearcher(Game g)
        {
            var p = g.Player;
            p.ScriptControl = true;
            p.AnimOverride = "BackAway";
            yield return Cutscene.LuMove(g, p.transform.position.x - p.Facing * 0.5f, 1.4f, "BackAway");
            p.AnimOverride = null; p.ScriptControl = false;
            g.MonologueId("lab_m_researcher");
        }

        // ============================================================ 핵심 단서 → 깨달음 → 경보
        /// <summary>
        /// 핵심 단서: 카메라를 2배로 당겨 단서를 확대해 보여 주고 그 단서에 맞는 대사를 한다.
        /// 아직 깨닫지 않았다면 이어서 깨달음 사건 (어느 단서를 먼저 봐도 대사가 어색하지 않다:
        /// 깨달음 대사는 본 단서 하나만 전제한다).
        /// </summary>
        static IEnumerator Clue(Game g, string id, string dialogueId)
        {
            var s = g.State;
            bool first = s.Once("clue_seen_" + id);
            if (!first && s.Has("lab_realized")) { yield return g.Say(dialogueId); yield break; }
            yield return Cutscene.Play(g, "clue_" + id, ClueBody(g, id, dialogueId), null);
            ClueReward(g, id);
            if (!s.Has("lab_realized")) yield return Cutscene.Play(g, "lab_realize", Realize(g), FinRealize);
        }

        static IEnumerator ClueBody(Game g, string id, string dialogueId)
        {
            var p = g.Player;
            p.ScriptControl = true; p.AnimOverride = "Idle";
            var m = Marker.Find("clue_" + id);
            Vector2 at = m != null ? (Vector2)m.transform.position : LuPos(g) + new Vector2(p.Facing * 1.2f, 1.4f);
            p.SetFacing(at.x >= p.transform.position.x ? 1 : -1);
            var rig = CameraRig.Main;
            // 확대하지 않는다 (2026-11-15: 캐릭터·물체를 지나치게 당기지 않기) — 단서 쪽으로 카메라만 옮김
            yield return Cutscene.Pan(at, 0.6f);
            Sfx.Play("sfx_heartbeat", 0.3f);
            yield return Cutscene.Wait(0.5f);
            yield return g.Say(dialogueId);

        }

        static IEnumerator Realize(Game g)
        {
            var p = g.Player; var s = g.State;
            p.ScriptControl = true;
            p.AnimOverride = "Fear";
            Cutscene.CamSnap(LuPos(g) + new Vector2(0, 1.6f));
            yield return g.Say("lab_realize");
            p.AnimOverride = "LookAround";
            // 전력이 일부 복구된다: 천장 작업등이 차례로 (딸깍, 딸깍)
            if (AudioManager.I != null) AudioManager.I.PlayAmbience(null, 0.6f);
            yield return Cutscene.Wait(0.8f);
            Sfx.Play("sfx_pc_on", 0.6f, 0.6f);
            Cutscene.Shake(0.03f, 0.3f);
            for (int i = 1; i <= 4; i++)
            {
                s.Set("lab_power_" + i);
                Sfx.Play("sfx_light_on", 0.7f, 1f - i * 0.05f);
                yield return Cutscene.Wait(0.45f);
            }
            s.Set("lab_power");
            yield return Cutscene.Wait(0.6f);
            // 붉은 경광등과 경보음
            s.Set(Facility.AlarmFlag);
            Sfx.Play("sfx_alarm", 0.9f);
            if (AudioManager.I != null) AudioManager.I.PlayAmbience("amb_lab_alarm", 0.8f);
            p.AnimOverride = "Fear";
            Cutscene.Shake(0.05f, 0.4f);
            yield return Cutscene.Wait(1.2f);
            Sfx.Play("sfx_broadcast", 0.9f);
            yield return Cutscene.Wait(0.5f);
            yield return g.Say("lab_broadcast");
            yield return Cutscene.Wait(0.4f);
            p.AnimOverride = "Idle";
            yield return g.Say("op_t_escape");
        }

        static void FinRealize(Game g)
        {
            var s = g.State;
            for (int i = 1; i <= 4; i++) s.Set("lab_power_" + i);
            s.Set("lab_power"); s.Set(Facility.AlarmFlag); s.Set("lab_realized");
            GameState.AddUnique(s.D.dialogueSeen, "lab_broadcast");
            if (AudioManager.I != null) AudioManager.I.PlayAmbience("amb_lab_alarm", 0.5f);
            g.Checkpoint("c1_alarm");
        }

        // ============================================================ 잠입 구간 안내
        /// <summary>처음으로 출력이 남은 전원을 봤을 때: '외부 전력 끌어오기' 해금.</summary>
        static IEnumerator PullTutorial(Game g)
        {
            if (!g.State.Once("pull_unlocked")) yield break;
            g.MonologueId("c2s_pull_tut");
            if (g.Player != null && g.Player.Skills != null) g.Player.Skills.Unlock("pull", false);
        }

        /// <summary>맵을 떠날 때: 구간 통과 보상(누구나) + 들키지 않고 통과한 보상. 적 처치 보상은 없다.</summary>
        public static void OnLeaveMap(Game g, string mapId)
        {
            Guard.NoGo.Clear();
            var s = g.State;
            if (s == null || string.IsNullOrEmpty(mapId) || !IsStealthMap(mapId)) return;
            if (s.Has("rw_section_" + mapId)) return;
            s.Reward("section_" + mapId, ActionTuning.I.expSection);
            // 감시자가 있던 장소에서만 '들키지 않고 통과' 보상 (적이 없는 곳은 구간 보상만)
            bool watched = false;
            foreach (var se in Sensors.All) if (se != null) { watched = true; break; }
            if (watched && !s.Has("spotted_" + mapId))
            {
                s.Reward("quiet_" + mapId, ActionTuning.I.expQuiet);
                s.D.quietSections++;
                g.UI.Toast("들키지 않고 통과했다");
            }
        }

        public static bool IsStealthMap(string id) => id == "C01" || id == "C02" || id == "C03" || id == "C04" || id == "C05" || id == "M09" || id == "M14"
            || id == "M16" || id == "M20" || id == "M22" || id == "M23" || id == "M27" || id == "M34" || id == "M35";






        static IEnumerator ClearHintLater(Game g, float t) { yield return new WaitForSeconds(t); g.UI.hud.SetHint(""); }


    }
}
