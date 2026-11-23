using System;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 대사 한 줄. s = 화자 id, t = 본문.
    /// 스탠딩 일러스트(선택): e = 표정 (지정한 줄에만 스탠딩을 띄운다), p = 그림 인물 id (생략 시 화자),
    /// pos = "L"/"R" (생략 시 화자 기본 위치), hide = true 이면 이 줄부터 스탠딩을 모두 숨긴다 (컷신 동작이 가려질 때).
    /// wait = 이 줄을 띄우기 전 침묵(초). added = GDD 원문에 없는 연결용 대사.
    /// </summary>
    [Serializable] public class DLine { public string s; public string t; public bool added; public string e; public string p; public string pos; public bool hide; public float wait; }
    [Serializable] public class DEntry { public string id; public string src; public DLine[] lines; public string[] choices; }
    [Serializable] public class DBubble { public string id; public string cond; public string who; public string map; public string need; public DLine[] lines; }
    [Serializable] public class DDoc { public string id; public string title; public string body; public bool added; }
    [Serializable] public class DSpeaker { public string id; public string name; public string side; }
    [Serializable] public class DChapter { public string id; public string title; }

    [Serializable]
    public class DialogueFile
    {
        public int version;
        public DSpeaker[] speakers;
        public DChapter[] chapters;
        public DEntry[] dialogues;
        public DBubble[] bubbles;
        public DDoc[] docs;
    }

    /// <summary>
    /// 모든 대사·말풍선·기록 원문은 Assets/Game/Data/Dialogue/dialogue_ko.json 한 곳에 둔다.
    /// 장면·장치·이벤트는 안정적인 ID 로 이 데이터를 참조한다.
    /// 'added: true' 표시 줄은 GDD 원문에 없는 연결용 대사(조사 문구 등)다.
    /// </summary>
    public class DialogueDB
    {
        public readonly Dictionary<string, DEntry> entries = new Dictionary<string, DEntry>();
        public readonly Dictionary<string, string> speakers = new Dictionary<string, string>();
        public readonly Dictionary<string, string> speakerSide = new Dictionary<string, string>();
        public readonly Dictionary<string, DDoc> docs = new Dictionary<string, DDoc>();
        public readonly Dictionary<string, string> chapters = new Dictionary<string, string>();
        public readonly List<DBubble> bubbles = new List<DBubble>();
        public readonly List<string> errors = new List<string>();

        public static DialogueDB Parse(string json)
        {
            var db = new DialogueDB();
            DialogueFile f = null;
            try { f = JsonUtility.FromJson<DialogueFile>(json); }
            catch (Exception e) { db.errors.Add("JSON 오류: " + e.Message); return db; }
            if (f == null) { db.errors.Add("빈 대사 파일"); return db; }
            if (f.speakers != null) foreach (var s in f.speakers) { db.speakers[s.id] = s.name; db.speakerSide[s.id] = string.IsNullOrEmpty(s.side) ? "R" : s.side; }
            if (f.chapters != null) foreach (var c in f.chapters) db.chapters[c.id] = c.title;
            if (f.dialogues != null)
                foreach (var e in f.dialogues)
                {
                    if (string.IsNullOrEmpty(e.id)) { db.errors.Add("id 없는 대사"); continue; }
                    if (db.entries.ContainsKey(e.id)) db.errors.Add("중복 대사 id: " + e.id);
                    db.entries[e.id] = e;
                    if (e.lines != null)
                        foreach (var l in e.lines)
                            if (!string.IsNullOrEmpty(l.s) && !db.speakers.ContainsKey(l.s)) db.errors.Add("알 수 없는 화자 " + l.s + " in " + e.id);
                }
            if (f.bubbles != null)
            {
                var ids = new HashSet<string>();
                foreach (var b in f.bubbles)
                {
                    if (!ids.Add(b.id)) db.errors.Add("중복 말풍선 id: " + b.id);
                    db.bubbles.Add(b);
                }
            }
            if (f.docs != null) foreach (var d in f.docs) db.docs[d.id] = d;
            return db;
        }

        public DDoc Doc(string id) => id != null && docs.TryGetValue(id, out var d) ? d : null;
        public DEntry Get(string id) => id != null && entries.TryGetValue(id, out var e) ? e : null;
        public string SpeakerName(string id) => id != null && speakers.TryGetValue(id, out var n) ? n : "";
        /// <summary>장 제목은 개발 문서·내부 데이터용이다. 플레이 화면에 장 번호 카드를 띄우지 않는다.</summary>
        public string ChapterTitle(string id) => id != null && chapters.TryGetValue(id, out var n) ? n : "";
        public string SideOf(string speaker) => speaker != null && speakerSide.TryGetValue(speaker, out var v) ? v : "R";
    }
}
