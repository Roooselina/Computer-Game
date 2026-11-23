using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// Esc 검수 도구 (2026-11-15). 사용자가 중간중간 직접 검수할 수 있게 맵·주요 사건으로 이동한다.
    /// - 처음 검수 이동을 하는 순간 '검수 시작 전 상태'를 보관하고, 이후 모든 저장(자동 저장 포함)을 별도 폴더(…/review)로 보낸다.
    ///   일반 저장 파일은 검수 중에 쓰이지 않는다. '검수 시작 전 상태로 복귀'하면 원래 저장 폴더·상태로 돌아간다.
    /// - 이동 방식 A: 현재 상태 유지(인벤토리·분기·관계 그대로, 맵만). B: 사건 프리셋(필요한 최소 선행 상태 적용, 바뀌는 내용을 보여 주고 확인).
    /// - 이동할 때 진행 중 대화·컷신·이벤트·지연 코루틴·투사체·추종자·일시정지·슬로모션·페이드·레터박스를 정리하고 안전한 스폰에 둔다.
    /// 노출: 에디터·개발 빌드는 항상, 일반 빌드는 GameConfig.reviewToolsInRelease (기본 켬 — 사용자가 실행 파일에서 검수).
    /// </summary>
    public static class ReviewMode
    {
        public static bool Active { get; private set; }
        public static SaveData StartSnapshot { get; private set; }
        public static string OriginalDirectory { get; private set; }
        public static string ReviewDirectory { get; private set; }
        public static string Destination { get; private set; } = "";
        public static int Jumps { get; private set; }
        public const string SlotFile = "review_slot.json";

        public static bool Available(Game g)
        {
            if (Application.isEditor || Debug.isDebugBuild) return true;
            return g != null && g.Config != null && g.Config.reviewToolsInRelease;
        }

        /// <summary>검수 시작: 현재 상태를 보관하고 저장 폴더를 검수용으로 바꾼다 (이미 켜져 있으면 그대로).</summary>
        public static void Begin(Game g)
        {
            if (Active || g == null || g.State == null) return;
            StartSnapshot = g.State.D.Clone();
            OriginalDirectory = SaveSystem.OverrideDirectory;
            ReviewDirectory = Path.Combine(SaveSystem.Directory, "review");
            Directory.CreateDirectory(ReviewDirectory);
            try { File.WriteAllText(Path.Combine(ReviewDirectory, "start_snapshot.json"), JsonUtility.ToJson(StartSnapshot, true)); } catch { }
            SaveSystem.OverrideDirectory = ReviewDirectory;
            Active = true;
            Debug.Log("[EW] 검수 모드 시작 — 저장 위치: " + ReviewDirectory);
        }

        /// <summary>검수 끝 (타이틀로·복귀). restore 면 시작 전 상태·맵으로 돌아간다.</summary>
        public static void End(Game g, bool restore)
        {
            if (!Active) return;
            SaveSystem.OverrideDirectory = OriginalDirectory;
            Active = false; Destination = "";
            var snap = StartSnapshot; StartSnapshot = null;
            if (restore && snap != null && g != null)
            {
                var d = snap.Clone();
                Run(g, d, d.mapId, d.hasPos ? null : d.spawnId, d.hasPos, "검수 시작 전 상태");
            }
        }

        public static void OnReturnToTitle()
        {
            if (!Active) return;
            SaveSystem.OverrideDirectory = OriginalDirectory;
            Active = false; StartSnapshot = null; Destination = "";
        }

        // ------------------------------------------------------------ 이동
        /// <summary>A: 상태 유지 맵 이동.</summary>
        public static void GoMap(Game g, string map, string spawn)
        {
            Begin(g);
            Run(g, g.State.D.Clone(), map, spawn, false, WorldLayout.I.NameOf(map));
        }

        /// <summary>B: 사건 프리셋 적용 후 이동.</summary>
        public static void GoEvent(Game g, WorldLayout.Event e, string venelleVariant)
        {
            Begin(g);
            var d = BuildPreset(g.State.D, e, venelleVariant, out _);
            Run(g, d, e.map, string.IsNullOrEmpty(e.spawn) ? "start" : e.spawn, false, e.label);
        }

        public static void RestartMap(Game g)
        {
            Begin(g);
            var snap = g.LastCheckpoint != null ? g.LastCheckpoint.Clone() : g.State.D.Clone();
            Run(g, snap, snap.mapId, snap.hasPos ? null : snap.spawnId, snap.hasPos, "마지막 체크포인트 (" + snap.checkpointId + ")");
        }

        public static bool SaveSlot(Game g)
        {
            Begin(g);
            try { g.StoreDevices(); File.WriteAllText(Path.Combine(ReviewDirectory, SlotFile), JsonUtility.ToJson(g.State.D, true)); return true; }
            catch (System.Exception e) { Debug.LogWarning("[EW] 검수 슬롯 저장 실패: " + e.Message); return false; }
        }

        public static bool HasSlot => ReviewDirectory != null && File.Exists(Path.Combine(ReviewDirectory, SlotFile));

        public static bool LoadSlot(Game g)
        {
            if (!HasSlot) return false;
            Begin(g);
            var d = JsonUtility.FromJson<SaveData>(File.ReadAllText(Path.Combine(ReviewDirectory, SlotFile)));
            if (d == null) return false;
            Run(g, d, d.mapId, d.hasPos ? null : d.spawnId, d.hasPos, "검수 슬롯");
            return true;
        }

        static void Run(Game g, SaveData d, string map, string spawn, bool usePos, string label)
        {
            Destination = label + " (" + map + ")";
            Jumps++;
            Runner.Get().StartCoroutine(JumpCo(g, d, map, spawn, usePos));
        }

        static IEnumerator JumpCo(Game g, SaveData d, string map, string spawn, bool usePos)
        {
            yield return null;   // 메뉴가 닫히는 프레임을 지난 뒤
            g.HardReset();
            d.Sanitize();
            if (!usePos) { d.hasPos = false; d.spawnId = spawn; }
            d.mapId = map;
            g.SetState(new GameState(d, g.Db));
            g.RunEvent(g.LoadMap(map, usePos ? null : spawn, usePos));
        }

        // ------------------------------------------------------------ 프리셋
        /// <summary>
        /// 사건 프리셋: 선행 사건(before) 사슬의 플래그를 차례로 적용한다. 현재 상태에서 출발하고, 모순되는 베넬 상태는 고를 수 없게 한다.
        /// changes 에 바뀌는 내용을 사람이 읽을 문장으로 돌려준다.
        /// </summary>
        public static SaveData BuildPreset(SaveData from, WorldLayout.Event e, string venelle, out List<string> changes)
        {
            changes = new List<string>();
            var d = from.Clone();
            var chain = new List<WorldLayout.Event>();
            var cur = e; int guard = 0;
            while (cur != null && guard++ < 64) { chain.Insert(0, cur); cur = WorldLayout.I.GetEvent(cur.before); }
            foreach (var ev in chain)
            {
                foreach (var f in ev.flagsOn) if (!d.flags.Contains(f)) { d.flags.Add(f); if (ev == e || true) changes.Add("+ " + f); }
                foreach (var f in ev.flagsOff) if (d.flags.Remove(f)) changes.Add("− " + f);
                if (!string.IsNullOrEmpty(ev.chapterId) && d.chapterId != ev.chapterId) { changes.Add("장: " + d.chapterId + " → " + ev.chapterId); d.chapterId = ev.chapterId; }
                foreach (var sk in ev.skills) if (!d.skills.Contains(sk)) { d.skills.Add(sk); changes.Add("기술 +" + sk); }
                foreach (var it in ev.items)
                {
                    var p = it.Split(':'); int n = p.Length > 1 ? int.Parse(p[1]) : 1;
                    var kv = d.items.Find(x => x.k == p[0]);
                    if (kv == null) { d.items.Add(new KV(p[0], n)); changes.Add("물건 " + p[0] + " ×" + n); }
                    else if (kv.v < n) { changes.Add("물건 " + p[0] + " " + kv.v + " → " + n); kv.v = n; }
                }
                foreach (var kv in ev.sets) ApplySet(d, kv, changes);
                if (!string.IsNullOrEmpty(ev.raviel) && ev.raviel != "keep" && d.ravielState != ev.raviel) { changes.Add("라비엘: " + d.ravielState + " → " + ev.raviel); d.ravielState = ev.raviel; }
            }
            if (string.IsNullOrEmpty(venelle) || venelle == "keep") { if (e.venelle != null && e.venelle.StartsWith("set:")) venelle = e.venelle.Substring(4); }
            if (!string.IsNullOrEmpty(venelle) && venelle != "keep") ApplyVenelle(d, venelle, e.chapter, changes);
            // 맵 이동 정보
            changes.Add("이동: " + WorldLayout.I.NameOf(e.map) + " (" + e.map + ")");
            d.hasPos = false;
            return d;
        }

        static void ApplySet(SaveData d, string kv, List<string> changes)
        {
            if (kv.StartsWith("shelter+")) { var grp = kv.Substring(8); if (!d.shelterGroups.Contains(grp)) { d.shelterGroups.Add(grp); changes.Add("대피실 +" + grp); } return; }
            var p = kv.Split('='); if (p.Length != 2) return;
            switch (p[0])
            {
                case "friendRoute": d.friendRoute = p[1]; break;
                case "reunionChoice": d.reunionChoice = p[1]; break;
                case "lacrimosaResult": d.lacrimosaResult = p[1]; break;
                case "wasteResult": d.wasteResult = p[1]; break;
                case "limitReleases":
                    {
                        int n = int.Parse(p[1]);
                        if (d.limitBaseMax <= 0) { var t = ActionTuning.I.player; d.limitBaseMax = t.baseStability + t.stabilityPerLevel * (d.lu.level - 1); }
                        d.limitReleases = n;
                        for (int i = 1; i <= n; i++) if (!d.flags.Contains("limit_release_" + i)) d.flags.Add("limit_release_" + i);
                        break;
                    }
                default: return;
            }
            changes.Add(p[0] + " = " + p[1]);
        }

        public static List<string> VenelleOptions(WorldLayout.Event e)
        {
            var l = new List<string>();
            if (e == null || string.IsNullOrEmpty(e.venelle) || !e.venelle.StartsWith("choose:")) return l;
            l.AddRange(e.venelle.Substring(7).Split(','));
            return l;
        }

        static void ApplyVenelle(SaveData d, string v, string chapter, List<string> changes)
        {
            string before = d.venelleState;
            int ch = ChapterNum(chapter);
            foreach (var f in new[] { "c3_joined", "venelle_spared", "venelle_apart", "venelle_bench", "venelle_dead", "c3_killed", "venelle_bulkhead", "venelle_shelter", "venelle_to_shelter" }) d.flags.Remove(f);
            switch (v)
            {
                case "pending":
                    d.venelleState = "pending"; d.venelle.state = "stable"; d.venelleDeathScene = "none";
                    break;
                case "ally":
                    d.venelleState = "ally"; d.venelle.state = "stable"; d.venelleDeathScene = "none";
                    d.flags.Add("c3_joined"); d.flags.Add("venelle_spared"); d.flags.Add("c3_rest_done");
                    break;
                case "apart":
                    d.venelleState = "apart"; d.venelle.state = "stable"; d.venelleDeathScene = "none";
                    d.flags.Add("venelle_spared"); d.flags.Add("venelle_apart"); d.flags.Add("venelle_bench");
                    if (ch >= 7 && d.flags.Contains("c7_feeds_seen")) { d.flags.Add("venelle_to_shelter"); if (!d.shelterGroups.Contains("venelle")) d.shelterGroups.Add("venelle"); }
                    if (ch >= 9 && d.flags.Contains("shelter_collapsed")) { d.venelleState = "dead"; d.venelle.state = "dead"; d.venelleDeathScene = "shelter"; d.flags.Add("venelle_dead"); d.flags.Add("venelle_shelter"); }
                    break;
                case "ally_bulkhead":   // 동행했다가 7장 격벽에서 사망 (8장 이후 프리셋)
                    d.venelleState = "dead"; d.venelle.state = "dead"; d.venelleDeathScene = "bulkhead";
                    d.flags.Add("c3_joined"); d.flags.Add("venelle_spared"); d.flags.Add("venelle_dead"); d.flags.Add("venelle_bulkhead");
                    break;
                default:   // dead (3장에서 사망)
                    d.venelleState = "dead"; d.venelle.state = "dead"; d.venelleDeathScene = "c06_corridor";
                    d.flags.Add("venelle_dead"); d.flags.Add("c3_killed");
                    break;
            }
            changes.Add("베넬: " + before + " → " + VenelleLabel(v));
        }

        public static string VenelleLabel(string v) => v == "ally" ? "동행" : v == "apart" ? "생존·비동행" : v == "ally_bulkhead" ? "동행 후 격벽 사망" : "3장 사망";
        static int ChapterNum(string ch) { if (string.IsNullOrEmpty(ch) || !ch.StartsWith("ch")) return 0; int.TryParse(ch.Substring(2), out int n); return n; }

        /// <summary>코루틴 실행용 (Game 의 코루틴을 모두 멈춰도 살아 있어야 한다).</summary>
        class Runner : MonoBehaviour
        {
            static Runner inst;
            public static Runner Get()
            {
                if (inst == null) { var go = new GameObject("[ReviewRunner]"); Object.DontDestroyOnLoad(go); inst = go.AddComponent<Runner>(); }
                return inst;
            }
        }
    }
}
