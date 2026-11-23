using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>
    /// Esc → '검수 도구'. 목적지를 보고 고른다 (Esc 를 누를 때마다 다음 맵으로 보내지 않는다).
    /// 고른 이동은 일시정지 메뉴를 닫은 뒤 ReviewMode 가 정리 후 실행한다.
    /// </summary>
    public static class ReviewMenu
    {
        /// <summary>메뉴가 닫힌 뒤 실행할 이동.</summary>
        public static System.Action Pending;

        static Text info;

        public static IEnumerator Run(Game g)
        {
            var root = UIKit.Rect("ReviewInfo", g.UI.Root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            UIKit.Place(root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -8), new Vector2(560, 46));
            UIKit.Panel(root, "Bg");
            info = UIKit.Label(root, "T", "", 10, TextAnchor.UpperLeft);
            info.rectTransform.offsetMin = new Vector2(8, 4); info.rectTransform.offsetMax = new Vector2(-8, -4);
            try
            {
                while (Pending == null)
                {
                    info.text = Header(g);
                    var items = new[] { "정상 진행의 다음 맵", "이전 방문 맵", "전체 맵 목록", "주요 사건 목록 (프리셋)", "현재 맵 다시 시작 (마지막 체크포인트)",
                                        "검수 슬롯 저장", "검수 슬롯 이어하기", "검수 시작 전 상태로 복귀", "돌아가기" };
                    var en = new[] { true, true, true, true, true, true, ReviewMode.HasSlot, ReviewMode.Active, true };
                    int r = -1;
                    yield return Pick(g, "Review", items, en, 250, v => r = v);
                    if (r < 0 || r == 8) break;
                    if (r == 0) yield return NextMaps(g);
                    else if (r == 1) yield return History(g);
                    else if (r == 2) yield return AllMaps(g);
                    else if (r == 3) yield return Events(g);
                    else if (r == 4) { yield return Confirm(g, "마지막 체크포인트에서 다시 시작합니다. (검수 저장 폴더에만 기록)"); if (g.LastChoice == 0) Pending = () => ReviewMode.RestartMap(g); }
                    else if (r == 5) { bool ok = ReviewMode.SaveSlot(g); g.UI.Toast(ok ? "검수 슬롯에 저장했습니다 (일반 저장과 별도)." : "검수 슬롯 저장 실패", 2.5f); }
                    else if (r == 6) Pending = () => ReviewMode.LoadSlot(g);
                    else if (r == 7) { yield return Confirm(g, "검수 시작 전 상태로 돌아갑니다. 검수 중의 진행은 일반 저장에 남지 않습니다."); if (g.LastChoice == 0) Pending = () => ReviewMode.End(g, true); }
                }
            }
            finally { if (root != null) Object.Destroy(root.gameObject); }
        }

        static string Header(Game g)
        {
            var s = g.State; var L = WorldLayout.I;
            var m = L.Get(g.CurrentMapId);
            string mode = ReviewMode.Active ? "검수 모드 켜짐 · 저장: " + ReviewMode.ReviewDirectory : "검수 모드 꺼짐 (이동하면 켜지고 일반 저장과 분리)";
            return "현재: " + g.CurrentMapId + " " + (m != null ? m.name : g.Map != null ? g.Map.title : "") + "   장: " + s.D.chapterId + "   체크포인트: " + s.D.checkpointId
                + "\n베넬: " + s.D.venelleState + "  라비엘: " + s.D.ravielState + "  한계 방출: " + s.D.limitReleases + "\n" + mode;
        }

        static IEnumerator Pick(Game g, string name, IList<string> items, IList<bool> en, float width, System.Action<int> result)
        {
            var menu = new MenuList(g.UI.Root, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -10), width, 15, 10);
            menu.SetItems(items, en);
            yield return null;
            int r;
            while ((r = menu.Tick(true)) == -1) yield return null;
            menu.Destroy();
            result(r == -2 ? -1 : r);
        }

        static IEnumerator Confirm(Game g, string text)
        {
            g.UI.dialogue.SetBodyImmediate("", text);
            yield return g.Choose(new[] { "예", "아니요" });
            g.UI.dialogue.Show(false);
        }

        // ------------------------------------------------------------ 맵
        static IEnumerator NextMaps(Game g)
        {
            var L = WorldLayout.I;
            var next = L.NextOf(g.CurrentMapId);
            if (next.Count == 0) { yield return g.Info("배치 데이터에 다음 맵 후보가 없습니다 (마지막 장면이거나 장면 링크로 넘어가는 곳)."); yield break; }
            var labels = new List<string>();
            foreach (var n in next) labels.Add(n.map + " " + L.NameOf(n.map) + (string.IsNullOrEmpty(n.label) ? "" : " — " + n.label) + (string.IsNullOrEmpty(n.cond) ? "" : "  [조건: " + n.cond + "]"));
            labels.Add("돌아가기");
            int r = -1; yield return Pick(g, "ReviewNext", labels, null, 420, v => r = v);
            if (r < 0 || r >= next.Count) yield break;
            yield return GoMapChecked(g, next[r].map);
        }

        static IEnumerator History(Game g)
        {
            var v = new List<string>(g.State.D.visitedMaps);
            v.Reverse();
            var uniq = new List<string>();
            foreach (var id in v) if (id != g.CurrentMapId && !uniq.Contains(id)) uniq.Add(id);
            if (uniq.Count > 12) uniq.RemoveRange(12, uniq.Count - 12);
            if (uniq.Count == 0) { yield return g.Info("이전에 방문한 다른 맵이 없습니다."); yield break; }
            var labels = new List<string>(); foreach (var id in uniq) labels.Add(id + " " + WorldLayout.I.NameOf(id)); labels.Add("돌아가기");
            int r = -1; yield return Pick(g, "ReviewHist", labels, null, 300, x => r = x);
            if (r < 0 || r >= uniq.Count) yield break;
            yield return GoMapChecked(g, uniq[r]);
        }

        static IEnumerator AllMaps(Game g)
        {
            var L = WorldLayout.I;
            // 장 묶음 먼저
            var chapters = new List<string>();
            foreach (var m in L.Maps) if (!chapters.Contains(m.chapter)) chapters.Add(m.chapter);
            var cl = new List<string>(chapters); cl.Add("돌아가기");
            int c = -1; yield return Pick(g, "ReviewCh", cl, null, 200, x => c = x);
            if (c < 0 || c >= chapters.Count) yield break;
            var maps = L.Maps.FindAll(m => m.chapter == chapters[c]);
            var labels = new List<string>(); foreach (var m in maps) labels.Add(m.id + " " + m.name + (m.optional ? " ★선택" : "")); labels.Add("돌아가기");
            int r = -1; yield return Pick(g, "ReviewMaps", labels, null, 300, x => r = x);
            if (r < 0 || r >= maps.Count) yield break;
            yield return GoMapChecked(g, maps[r].id);
        }

        /// <summary>A 방식: 상태 유지 이동. 필수 조건이 부족하면 알리고 고르게 한다.</summary>
        static IEnumerator GoMapChecked(Game g, string mapId)
        {
            var L = WorldLayout.I; var m = L.Get(mapId);
            if (m == null || g.Config.GetMap(mapId) == null) { yield return g.Info("맵을 찾을 수 없습니다: " + mapId); yield break; }
            var missing = new List<string>();
            foreach (var f in m.requires) if (!g.State.Has(f)) missing.Add(f);
            if (missing.Count > 0)
            {
                var ev = L.Events.Find(e => e.map == mapId);
                g.UI.dialogue.SetBodyImmediate("", "필수 조건 부족: " + string.Join(", ", missing) + "\n현재 상태로 가면 출구가 잠겨 있거나 사건이 순서대로 나오지 않을 수 있다.");
                var opts = new List<string> { "그래도 상태 유지로 이동", "이 맵의 사건 프리셋으로", "취소" };
                yield return g.Choose(opts, new[] { true, ev != null, true }, true);
                g.UI.dialogue.Show(false);
                if (g.LastChoice == 1 && ev != null) { yield return EventPreset(g, ev); yield break; }
                if (g.LastChoice != 0) yield break;
            }
            string spawn = string.IsNullOrEmpty(m.safeSpawn) ? "start" : m.safeSpawn;
            Pending = () => ReviewMode.GoMap(g, mapId, spawn);
        }

        // ------------------------------------------------------------ 사건
        static IEnumerator Events(Game g)
        {
            var L = WorldLayout.I;
            var labels = new List<string>(); foreach (var e in L.Events) labels.Add("[" + e.chapter + "] " + e.label); labels.Add("돌아가기");
            int r = -1; yield return Pick(g, "ReviewEvents", labels, null, 360, x => r = x);
            if (r < 0 || r >= L.Events.Count) yield break;
            yield return EventPreset(g, L.Events[r]);
        }

        static IEnumerator EventPreset(Game g, WorldLayout.Event e)
        {
            string variant = "keep";
            var opts = ReviewMode.VenelleOptions(e);
            if (opts.Count > 0)
            {
                var labels = new List<string>(); foreach (var o in opts) labels.Add("베넬: " + ReviewMode.VenelleLabel(o)); labels.Add("현재 상태 그대로 (" + g.State.D.venelleState + ")"); labels.Add("취소");
                int r = -1; yield return Pick(g, "ReviewVen", labels, null, 260, x => r = x);
                if (r < 0 || r == labels.Count - 1) yield break;
                variant = r < opts.Count ? opts[r] : "keep";
                if (variant == "keep" && !VariantConsistent(g, e)) { yield return g.Info("현재 베넬 상태(" + g.State.D.venelleState + ")는 이 사건 시점과 맞지 않습니다. 위의 프리셋 중 하나를 고르세요."); yield break; }
            }
            ReviewMode.BuildPreset(g.State.D, e, variant, out var changes);
            string summary = changes.Count > 10 ? string.Join(" · ", changes.GetRange(0, 10)) + " … (+" + (changes.Count - 10) + ")" : string.Join(" · ", changes);
            g.UI.dialogue.SetBodyImmediate("", "적용할 변경: " + summary);
            yield return g.Choose(new[] { "적용하고 이동", "취소" }, null, true);
            g.UI.dialogue.Show(false);
            if (g.LastChoice != 0) yield break;
            Pending = () => ReviewMode.GoEvent(g, e, variant);
        }

        static bool VariantConsistent(Game g, WorldLayout.Event e)
        {
            var opts = ReviewMode.VenelleOptions(e);
            string v = g.State.D.venelleState;
            if (v == "ally") return opts.Contains("ally");
            if (v == "apart") return opts.Contains("apart");
            if (v == "dead") return opts.Contains("dead") || opts.Contains("ally_bulkhead");
            return false;
        }
    }
}
