using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EW.EditorTools
{
    /// <summary>
    /// 건물 배치 검사 (Docs/BUILDING_LAYOUT_GUIDE.md).
    /// 씬을 만들 때(EWSceneBuilder.BuildRoom) 실제 씬의 출구·문·스폰·사건 ID 를 Docs/Generated/built_layout_report.json 으로 내보내고,
    /// 이 검사기가 배치 데이터(world_layout.json)·대사·사건 코드와 맞춰 본다:
    ///  끊긴 출구(없는 맵·없는 스폰) / 데이터와 씬의 출구 불일치 / 새 게임에서 도달할 수 없는 맵 / 중복 ID / 없는 사건·대사 ID / 프리셋의 없는 맵·스폰.
    /// 메뉴 EW > 7. 배치 데이터 검사. 같은 검사를 EditMode 테스트(LayoutTests)가 돌린다.
    /// </summary>
    public static class EWLayoutCheck
    {
        public const string ReportPath = "Docs/Generated/built_layout_report.json";

        [System.Serializable] public class RExit { public string id, to, spawn, kind, require, block; }
        [System.Serializable] public class RMap { public string id, scene; public List<string> spawns = new List<string>(); public List<RExit> exits = new List<RExit>(); public List<string> events = new List<string>(); }
        [System.Serializable] public class Report { public List<RMap> maps = new List<RMap>(); }

        static Report pending;
        public static void BeginReport() { pending = new Report(); }

        /// <summary>BuildRoom 끝에서 호출: 방금 만든 씬 내용 기록.</summary>
        public static void Record(RoomDef r, Transform gen)
        {
            if (pending == null) pending = new Report();
            var m = new RMap { id = r.id, scene = r.scene };
            foreach (var s in gen.GetComponentsInChildren<SpawnPoint>(true)) m.spawns.Add(s.id);
            foreach (var z in gen.GetComponentsInChildren<TriggerZone>(true))
            {
                if (z.kind == TriggerZone.Kind.MapExit) m.exits.Add(new RExit { id = z.gameObject.name, to = z.targetMap, spawn = z.targetSpawn, kind = "edge", require = z.requireFlag, block = z.blockFlag });
                else if (z.kind == TriggerZone.Kind.Event && !string.IsNullOrEmpty(z.eventId)) m.events.Add(z.eventId);
            }
            foreach (var d in gen.GetComponentsInChildren<DoorExit>(true)) m.exits.Add(new RExit { id = d.id, to = d.targetMap, spawn = d.targetSpawn, kind = "door", require = d.requireFlag });
            foreach (var it in gen.GetComponentsInChildren<Inspect>(true)) if (!string.IsNullOrEmpty(it.eventId)) m.events.Add(it.eventId);
            foreach (var rp in gen.GetComponentsInChildren<RestPoint>(true)) if (!string.IsNullOrEmpty(rp.beforeEventId)) m.events.Add(rp.beforeEventId);
            foreach (var ft in gen.GetComponentsInChildren<FlagTrigger>(true)) if (!string.IsNullOrEmpty(ft.eventId)) m.events.Add(ft.eventId);
            pending.maps.RemoveAll(x => x.id == r.id);
            pending.maps.Add(m);
        }

        public static void WriteReport()
        {
            if (pending == null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, JsonUtility.ToJson(pending, true));
            pending = null;
        }

        public static Report LoadReport() => File.Exists(ReportPath) ? JsonUtility.FromJson<Report>(File.ReadAllText(ReportPath)) : null;

        [MenuItem("EW/7. 배치 데이터 검사 (끊긴 출구·도달 불가·중복 ID)", priority = 50)]
        public static void MenuCheck()
        {
            var problems = Check();
            if (problems.Count == 0) EditorUtility.DisplayDialog("배치 데이터 검사", "문제 없음.", "확인");
            else { foreach (var p in problems) Debug.LogWarning("[EW] 배치 검사: " + p); EditorUtility.DisplayDialog("배치 데이터 검사", problems.Count + "건 — 콘솔을 보세요.\n" + string.Join("\n", problems.Take(12)), "확인"); }
        }

        /// <summary>문제 목록 (빈 목록이면 통과).</summary>
        public static List<string> Check()
        {
            var P = new List<string>();
            var L = WorldLayout.Parse(File.ReadAllText("Assets/Game/Resources/EW/world_layout.json"));
            P.AddRange(L.Errors.Select(e => "배치 데이터: " + e));
            var rep = LoadReport();
            if (rep == null) { P.Add("씬 보고서 없음 (" + ReportPath + ") — EW > 1. 전체 구성을 먼저 실행"); return P; }
            var byId = new Dictionary<string, RMap>();
            foreach (var m in rep.maps) { if (byId.ContainsKey(m.id)) P.Add("중복 맵 ID (씬): " + m.id); byId[m.id] = m; }
            // 1) 씬 출구: 대상 맵·스폰
            foreach (var m in rep.maps)
            {
                foreach (var dup in m.spawns.GroupBy(x => x).Where(x => x.Count() > 1)) P.Add(m.id + ": 중복 스폰 " + dup.Key);
                foreach (var e in m.exits)
                {
                    if (!byId.TryGetValue(e.to, out var t)) { P.Add(m.id + " 출구 " + e.id + " → 없는 맵 " + e.to); continue; }
                    if (!string.IsNullOrEmpty(e.spawn) && !t.spawns.Contains(e.spawn)) P.Add(m.id + " 출구 " + e.id + " → " + e.to + " 에 없는 스폰 '" + e.spawn + "'");
                }
            }
            // 2) 배치 데이터 출구 ↔ 씬 출구
            foreach (var lm in L.Maps)
            {
                if (!byId.TryGetValue(lm.id, out var sm)) { P.Add("배치 데이터의 맵이 씬에 없음: " + lm.id); continue; }
                foreach (var le in lm.exits)
                {
                    var match = sm.exits.FirstOrDefault(x => x.to == le.to && x.spawn == le.spawn);
                    if (match == null) P.Add(lm.id + ": 배치 데이터 출구 " + le.id + " (→" + le.to + "/" + le.spawn + ") 가 씬에 없음");
                    else if ((match.require ?? "") != (le.require ?? "")) P.Add(lm.id + ": 출구 " + le.id + " 잠금 조건 불일치 (씬 '" + match.require + "' / 데이터 '" + le.require + "')");
                }
                foreach (var se in sm.exits)
                    if (!lm.exits.Any(x => x.to == se.to && x.spawn == se.spawn)) P.Add(lm.id + ": 씬 출구 " + se.id + " (→" + se.to + "/" + se.spawn + ") 가 배치 데이터에 없음");
                foreach (var s in lm.spawns) if (!sm.spawns.Contains(s)) P.Add(lm.id + ": 배치 데이터의 스폰 '" + s + "' 이 씬에 없음");
                if (!string.IsNullOrEmpty(lm.safeSpawn) && !sm.spawns.Contains(lm.safeSpawn)) P.Add(lm.id + ": 안전 스폰 '" + lm.safeSpawn + "' 이 씬에 없음");
                foreach (var n in lm.next) if (L.Get(n.map) == null) P.Add(lm.id + ": 다음 맵 후보 " + n.map + " 없음");
            }
            foreach (var m in rep.maps) if (L.Get(m.id) == null) P.Add("씬 맵이 배치 데이터에 없음: " + m.id);
            // 3) 새 게임(P01)에서 도달 가능 (출구 + 장면 링크)
            var adj = new Dictionary<string, HashSet<string>>();
            void Edge(string a, string b) { if (!adj.TryGetValue(a, out var s)) adj[a] = s = new HashSet<string>(); s.Add(b); }
            foreach (var m in rep.maps) foreach (var e in m.exits) Edge(m.id, e.to);
            foreach (var k in L.Links) Edge(k.from, k.to);
            var seen = new HashSet<string> { "P01" }; var q = new Queue<string>(); q.Enqueue("P01");
            while (q.Count > 0) { var c = q.Dequeue(); if (adj.TryGetValue(c, out var ns)) foreach (var n in ns) if (seen.Add(n)) q.Enqueue(n); }
            foreach (var m in rep.maps) if (!seen.Contains(m.id)) P.Add("새 게임에서 도달할 수 없는 맵: " + m.id);
            if (!seen.Contains("M47")) P.Add("마지막 맵 M47 에 도달할 수 없음");
            // 4) 사건 ID
            var evIds = new HashSet<string>(StoryEvents.EventIds);
            foreach (var m in rep.maps) foreach (var ev in m.events.Distinct()) if (!evIds.Contains(ev)) P.Add(m.id + ": 없는 사건 ID " + ev);
            // 5) 대사·문서 ID (중복·화자)
            var db = DialogueDB.Parse(File.ReadAllText("Assets/Game/Data/Dialogue/dialogue_ko.json"));
            P.AddRange(db.errors.Select(e => "대사 데이터: " + e));
            foreach (var lm in L.Maps)
            {
                foreach (var e in lm.exits) if (!string.IsNullOrEmpty(e.blocked) && db.Get(e.blocked) == null) P.Add(lm.id + ": 출구 " + e.id + " 의 잠김 대사 없음 " + e.blocked);
                foreach (var s in lm.spaceLines) if (db.Get(s) == null) P.Add(lm.id + ": 공간 의존 대사 목록의 없는 ID " + s);
            }
            // 6) 검수 프리셋
            foreach (var ev in L.Events)
            {
                if (!byId.TryGetValue(ev.map, out var sm)) { P.Add("프리셋 " + ev.id + ": 없는 맵 " + ev.map); continue; }
                if (!sm.spawns.Contains(string.IsNullOrEmpty(ev.spawn) ? "start" : ev.spawn)) P.Add("프리셋 " + ev.id + ": " + ev.map + " 에 없는 스폰 " + ev.spawn);
                if (!string.IsNullOrEmpty(ev.before) && L.GetEvent(ev.before) == null) P.Add("프리셋 " + ev.id + ": 없는 선행 사건 " + ev.before);
            }
            return P;
        }
    }
}
