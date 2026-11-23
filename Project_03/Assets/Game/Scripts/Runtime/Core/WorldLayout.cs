using System;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 건물 배치 데이터 (Resources/EW/world_layout.json, 2026-11-15).
    /// 맵 고유 ID·표시 이름·장·구역·층, 출입구 ID → 도착 맵·스폰, 잠금·봉쇄 조건, 정상 진행의 다음 맵 후보,
    /// 장면 링크(출구가 아닌 사건으로 넘어가는 연결), 사라진 맵의 안전 지점 매핑, 공간 의존 대사 목록, 검수용 주요 사건.
    /// 지금의 연결은 GDD 기준 임시 배치다 — 도면이 오면 이 파일과 방 정의를 함께 고친다 (Docs/BUILDING_LAYOUT_GUIDE.md).
    /// 이 파일만 바꾼다고 맵 지형·대사·연출이 자동으로 바뀌지는 않는다.
    /// </summary>
    public class WorldLayout
    {
        [Serializable] public class Exit { public string id; public string to; public string spawn; public string kind; public string require; public string block; public string blocked; public string note; }
        [Serializable] public class Next { public string map; public string label; public string cond; }
        [Serializable] public class Map
        {
            public string id, scene, name, chapter, zone, floor, goal, safeSpawn = "start";
            public bool optional;
            public Exit[] exits = new Exit[0];
            public Next[] next = new Next[0];
            public string[] spawns = new string[0];
            public string[] spaceLines = new string[0];
            public string[] anchors = new string[0];
            /// <summary>정상 진행으로 이 맵에 올 때 이미 있어야 하는 플래그 (검수 이동 안내용).</summary>
            public string[] requires = new string[0];
        }
        [Serializable] public class Link { public string from, to, spawn, by; }
        [Serializable] public class Remap { public string map, to, spawn; }
        [Serializable] public class ChapterSafe { public string chapter, map, spawn; }
        [Serializable] public class Event
        {
            public string id, label, chapter, map, spawn, chapterId, venelle = "keep", raviel = "keep", note;
            public string[] flagsOn = new string[0];
            public string[] flagsOff = new string[0];
            public string[] skills = new string[0];
            public string[] items = new string[0];
            /// <summary>저장 필드 지정: friendRoute=flee, reunionChoice=hand, lacrimosaResult=subdued, wasteResult=repair, limitReleases=N, shelter+그룹</summary>
            public string[] sets = new string[0];
            public string before;           // 이 사건 전 단계의 사건 id (선행 상태를 이어 붙인다)
        }
        [Serializable] class File
        {
            public int version;
            public Map[] maps = new Map[0];
            public Link[] storyLinks = new Link[0];
            public Remap[] remap = new Remap[0];
            public ChapterSafe[] chapterSafe = new ChapterSafe[0];
            public Event[] events = new Event[0];
        }

        static WorldLayout inst;
        public static WorldLayout I { get { if (inst == null) inst = Load(); return inst; } }
        public static void Reload() { inst = null; }

        public readonly List<Map> Maps = new List<Map>();
        public readonly List<Link> Links = new List<Link>();
        public readonly List<Remap> Remaps = new List<Remap>();
        public readonly List<ChapterSafe> ChapterSafes = new List<ChapterSafe>();
        public readonly List<Event> Events = new List<Event>();
        public readonly List<string> Errors = new List<string>();
        readonly Dictionary<string, Map> byId = new Dictionary<string, Map>();
        readonly Dictionary<string, Event> evById = new Dictionary<string, Event>();

        public static WorldLayout Parse(string json)
        {
            var w = new WorldLayout();
            File f = null;
            try { f = JsonUtility.FromJson<File>(json); }
            catch (Exception e) { w.Errors.Add("JSON 오류: " + e.Message); return w; }
            if (f == null) { w.Errors.Add("빈 배치 데이터"); return w; }
            foreach (var m in f.maps ?? new Map[0])
            {
                if (string.IsNullOrEmpty(m.id)) { w.Errors.Add("id 없는 맵"); continue; }
                if (w.byId.ContainsKey(m.id)) { w.Errors.Add("중복 맵 id: " + m.id); continue; }
                m.exits = m.exits ?? new Exit[0]; m.next = m.next ?? new Next[0]; m.spawns = m.spawns ?? new string[0]; m.spaceLines = m.spaceLines ?? new string[0]; m.requires = m.requires ?? new string[0];
                w.byId[m.id] = m; w.Maps.Add(m);
            }
            var exitIds = new HashSet<string>();
            foreach (var m in w.Maps) foreach (var e in m.exits)
                {
                    if (string.IsNullOrEmpty(e.id)) w.Errors.Add(m.id + ": id 없는 출구");
                    else if (!exitIds.Add(e.id)) w.Errors.Add("중복 출구 id: " + e.id);
                }
            w.Links.AddRange(f.storyLinks ?? new Link[0]);
            w.Remaps.AddRange(f.remap ?? new Remap[0]);
            w.ChapterSafes.AddRange(f.chapterSafe ?? new ChapterSafe[0]);
            foreach (var e in f.events ?? new Event[0])
            {
                if (string.IsNullOrEmpty(e.id)) continue;
                if (w.evById.ContainsKey(e.id)) { w.Errors.Add("중복 사건 id: " + e.id); continue; }
                e.flagsOn = e.flagsOn ?? new string[0]; e.flagsOff = e.flagsOff ?? new string[0]; e.skills = e.skills ?? new string[0]; e.items = e.items ?? new string[0]; e.sets = e.sets ?? new string[0];
                w.evById[e.id] = e; w.Events.Add(e);
            }
            return w;
        }

        static WorldLayout Load()
        {
            var ta = Resources.Load<TextAsset>("EW/world_layout");
            if (ta == null) { var w = new WorldLayout(); w.Errors.Add("Resources/EW/world_layout.json 없음"); return w; }
            var r = Parse(ta.text);
            foreach (var e in r.Errors) Debug.LogError("[EW] 배치 데이터: " + e);
            return r;
        }

        public Map Get(string id) => id != null && byId.TryGetValue(id, out var m) ? m : null;
        public Event GetEvent(string id) => id != null && evById.TryGetValue(id, out var e) ? e : null;
        public string NameOf(string id) { var m = Get(id); return m != null ? m.name : id; }

        public Exit FindExit(string exitId)
        {
            foreach (var m in Maps) foreach (var e in m.exits) if (e.id == exitId) return e;
            return null;
        }

        /// <summary>정상 진행의 다음 맵 후보 (조건 문구 포함). 배열 다음 칸으로 임의 이동하지 않는다.</summary>
        public List<Next> NextOf(string mapId) { var m = Get(mapId); return m != null ? new List<Next>(m.next) : new List<Next>(); }

        public class Spot { public string map, spawn; public Spot(string m, string s) { map = m; spawn = s; } }

        /// <summary>저장된 맵이 사라졌거나 바뀌었을 때 옮길 안전 지점: remap → 장별 안전 지점 → null.</summary>
        public Spot SafeSpawnFor(string mapId, string chapterId)
        {
            foreach (var r in Remaps) if (r.map == mapId) return new Spot(r.to, string.IsNullOrEmpty(r.spawn) ? "start" : r.spawn);
            foreach (var c in ChapterSafes) if (c.chapter == chapterId) return new Spot(c.map, string.IsNullOrEmpty(c.spawn) ? "start" : c.spawn);
            return null;
        }

        /// <summary>맵의 스폰 이름이 바뀌었을 때: 배치 데이터의 안전 스폰.</summary>
        public string SafeSpawnIn(string mapId) { var m = Get(mapId); return m != null && !string.IsNullOrEmpty(m.safeSpawn) ? m.safeSpawn : "start"; }
    }
}
