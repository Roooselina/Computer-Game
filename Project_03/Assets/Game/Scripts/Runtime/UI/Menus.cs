using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>Esc 메뉴: 아이템·기록·대화 로그·상태·타이틀로. 메뉴가 열린 동안 필드 입력은 차단된다.</summary>
    public static class PauseMenu
    {
        public static IEnumerator Run(Game g)
        {
            g.Block("menu");
            int ctx = GameInput.Push(InputCtx.Pause);
            TimeCtl.Set("pause", 0f);
            var dim = UIKit.Solid(g.UI.Root, "PauseDim", new Color(0, 0, 0, 0.45f));
            var menu = new MenuList(g.UI.Root, "Pause", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-180, 40), 130);
            var optList = new List<string> { "계속하기", "아이템", "기록", "안내서", "기술 장착", "대화 로그", "상태", "현재 목표", "설정" };
            bool review = ReviewMode.Available(g);
            if (review) optList.Add("검수 도구");
            optList.Add("타이틀로");
            menu.SetItems(optList);
            ReviewMenu.Pending = null;
            yield return null;
            while (true)
            {
                int r = menu.Tick(true);
                if (r == -1) { yield return null; continue; }
                if (r == -2 || r == 0) break;
                string pick = optList[r];
                menu.SetActive(false);
                if (pick == "아이템") yield return Items(g);
                if (pick == "기록") yield return Docs(g);
                if (pick == "안내서") yield return GuideBook.Run(g);
                if (pick == "기술 장착") yield return Loadout(g);
                if (pick == "대화 로그") yield return Log(g);
                if (pick == "상태") yield return Status(g);
                if (pick == "현재 목표") yield return Goal(g);
                if (pick == "설정") yield return SettingsMenu.Run(g.UI.Root);
                if (pick == "검수 도구")
                {
                    yield return ReviewMenu.Run(g);
                    if (ReviewMenu.Pending != null) break;
                }
                if (pick == "타이틀로")
                {
                    yield return Confirm(g, ReviewMode.Active ? "타이틀로 돌아갈까요? 검수 모드가 끝나고 일반 저장 위치로 돌아갑니다." : "타이틀로 돌아갈까요? 마지막 저장 이후의 진행은 사라집니다.");
                    if (g.LastChoice == 0)
                    {
                        TimeCtl.Clear("pause"); GameInput.Pop(ctx);
                        Object.Destroy(dim.gameObject); menu.Destroy();
                        g.Unblock("menu");
                        g.ReturnToTitle();
                        yield break;
                    }
                }
                menu.SetActive(true);
                yield return null;
            }
            TimeCtl.Clear("pause");
            Object.Destroy(dim.gameObject);
            menu.Destroy();
            GameInput.Pop(ctx);
            g.Unblock("menu");
            var act = ReviewMenu.Pending; ReviewMenu.Pending = null;
            act?.Invoke();
        }

        /// <summary>현재 목표: 배치 데이터의 맵별 안내 문구 (도면이 바뀌면 world_layout.json 에서 함께 고친다).</summary>
        static IEnumerator Goal(Game g)
        {
            var m = WorldLayout.I.Get(g.CurrentMapId);
            string goal = m != null && !string.IsNullOrEmpty(m.goal) ? m.goal : "주변을 살펴보자.";
            yield return g.Info((m != null ? m.name + " — " : "") + goal);
        }

        /// <summary>기술 장착: Q/E/R/F 슬롯마다 가진 기술 중 하나를 고른다 (같은 기술을 두 슬롯에 넣으면 자리를 맞바꾼다).</summary>
        static IEnumerator Loadout(Game g)
        {
            var s = g.State;
            while (true)
            {
                var names = new List<string>();
                for (int i = 0; i < 4; i++)
                {
                    var id = s.D.slots.Count > i ? s.D.slots[i] : "";
                    var sk = string.IsNullOrEmpty(id) ? null : ActionTuning.I.Skill(id);
                    names.Add("[" + GameInput.Label(GameInput.SkillAct(i)) + "]  " + (sk != null ? sk.name : "(비어 있음)"));
                }
                names.Add("돌아가기");
                var m = new MenuList(g.UI.Root, "Loadout", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40, 40), 220);
                m.SetItems(names);
                yield return null;
                int r;
                while ((r = m.Tick(true)) == -1) yield return null;
                m.Destroy();
                if (r < 0 || r >= 4) yield break;
                var owned = new List<string>(s.D.skills);
                var labels = new List<string>();
                foreach (var id in owned) { var sk = ActionTuning.I.Skill(id); labels.Add(sk != null ? sk.name + "  — " + (sk.target ?? "") : id); }
                labels.Add("비우기"); labels.Add("돌아가기");
                var m2 = new MenuList(g.UI.Root, "LoadoutPick", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(60, 30), 300);
                m2.SetItems(labels);
                yield return null;
                int r2;
                while ((r2 = m2.Tick(true)) == -1) yield return null;
                m2.Destroy();
                if (r2 < 0 || r2 == labels.Count - 1) continue;
                string pick = r2 < owned.Count ? owned[r2] : "";
                int other = pick != "" ? s.D.slots.IndexOf(pick) : -1;
                if (other >= 0 && other != r) s.D.slots[other] = s.D.slots[r];
                s.D.slots[r] = pick;
                Sfx.Play("text", 0.7f);
            }
        }

        static IEnumerator Confirm(Game g, string text)
        {
            g.UI.dialogue.SetBodyImmediate("", text);
            yield return g.Choose(new[] { "예", "아니요" });
            g.UI.dialogue.Show(false);
        }

        static RectTransform Window(Game g, string name, out Text body, string title)
        {
            var rt = UIKit.Rect(name, g.UI.Root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UIKit.Place(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(60, 10), new Vector2(420, 250));
            UIKit.Panel(rt, "Bg");
            var t = UIKit.Label(rt, "Title", title, 11, TextAnchor.UpperLeft, UIKit.Accent);
            t.rectTransform.offsetMin = new Vector2(12, 0); t.rectTransform.offsetMax = new Vector2(-12, -8);
            body = UIKit.Label(rt, "Body", "", 11, TextAnchor.UpperLeft);
            body.rectTransform.offsetMin = new Vector2(12, 26); body.rectTransform.offsetMax = new Vector2(-12, -26);
            var hint = UIKit.Label(rt, "Hint", "C: 닫기", 11, TextAnchor.LowerRight, UIKit.Dim);
            hint.rectTransform.offsetMin = new Vector2(12, 6); hint.rectTransform.offsetMax = new Vector2(-12, -6);
            return rt;
        }

        static IEnumerator Items(Game g)
        {
            while (true)
            {
                var ids = new List<string>(); var names = new List<string>(); var ok = new List<bool>();
                foreach (var it in g.Db.items) { int n = g.State.Count(it.id); ids.Add(it.id); names.Add(it.name + " ×" + n + "   " + it.desc); ok.Add(n > 0 && it.fieldUse); }
                names.Add("돌아가기"); ok.Add(true);
                var m = new MenuList(g.UI.Root, "Items", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(60, 40), 320);
                m.SetItems(names, ok);
                yield return null;
                int r;
                while ((r = m.Tick(true)) == -1) yield return null;
                m.Destroy();
                if (r < 0 || r >= ids.Count) yield break;
                var def = g.Db.GetItem(ids[r]);
                string who = "lu";
                if (g.State.VenelleIsAlly)
                {
                    yield return g.Choose(new[] { "루에게", "베넬에게" });
                    if (g.LastChoice == 1) who = "venelle";
                }
                g.State.UseItem(def.id);
                var mem = g.State.Member(who);
                mem.hp = Mathf.Min(g.State.MaxHp(who), mem.hp + Mathf.CeilToInt(g.State.MaxHp(who) * def.healPct));
                if (who == "lu" && def.charge > 0) mem.charge = Mathf.Min(g.State.MaxCharge("lu"), mem.charge + def.charge);
                g.UI.hud.Refresh(g.State);
                g.UI.Toast(def.name + " 사용.");
                g.Bubbles.Request("rest");
            }
        }

        static IEnumerator Docs(Game g)
        {
            var have = new List<DDoc>();
            foreach (var id in g.State.D.docs) if (g.Dialogue.docs.TryGetValue(id, out var d)) have.Add(d);
            if (have.Count == 0) { yield return g.Info("아직 모은 기록이 없다."); yield break; }
            var names = new List<string>();
            foreach (var d in have) names.Add(d.id + "  " + d.title);
            names.Add("돌아가기");
            while (true)
            {
                var m = new MenuList(g.UI.Root, "Docs", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(60, 40), 260);
                m.SetItems(names);
                yield return null;
                int r;
                while ((r = m.Tick(true)) == -1) yield return null;
                m.Destroy();
                if (r < 0 || r >= have.Count) yield break;
                yield return DocPickup.DocView(g, have[r]);
            }
        }

        static IEnumerator Log(Game g)
        {
            var rt = Window(g, "Log", out var body, "대화 로그  (" + GameInput.Pair(Act.MenuUp, Act.Down) + " 스크롤 · " + GameInput.Label(Act.Cancel) + " 닫기)");
            int offset = 0;
            const int rows = 13;
            yield return null;
            while (true)
            {
                var lines = g.UI.log;
                int end = Mathf.Max(0, lines.Count - offset);
                int start = Mathf.Max(0, end - rows);
                body.text = lines.Count == 0 ? "(기록 없음)" : string.Join("\n", lines.GetRange(start, end - start));
                int d = GameInput.NavVertical();
                if (d < 0) offset = Mathf.Min(Mathf.Max(0, lines.Count - rows), offset + 1);
                if (d > 0) offset = Mathf.Max(0, offset - 1);
                if (GameInput.CancelDown || GameInput.ConfirmDown) break;
                yield return null;
            }
            Object.Destroy(rt.gameObject);
        }

        static IEnumerator Status(Game g)
        {
            var s = g.State;
            var rt = Window(g, "Status", out var body, "상태");
            var lu = s.D.lu;
            string txt = "루  Lv." + lu.level + "   경험치 " + lu.exp + (lu.level < 10 ? " / 다음 " + GameState.ExpForLevel(lu.level + 1) : "") +
                         "\n안정도 " + lu.hp + "/" + s.MaxHp("lu") + "   전하 " + lu.charge + "/" + s.MaxCharge("lu") +
                         "\n경험치는 탐색·단서·목표 달성으로 얻습니다 (적을 쓰러뜨려도 오르지 않음).\n\n기술";
            for (int i = 0; i < 4; i++)
            {
                var id = s.D.slots.Count > i ? s.D.slots[i] : "";
                var sk = string.IsNullOrEmpty(id) ? null : ActionTuning.I.Skill(id);
                txt += "\n · [" + GameInput.Label(GameInput.SkillAct(i)) + "] " + (sk != null ? sk.name + "  " + sk.desc : "(비어 있음)");
            }
            if (s.VenelleIsAlly)
                txt += "\n\n베넬  Lv." + s.D.venelle.level + "   안정도 " + s.D.venelle.hp + "/" + s.MaxHp("venelle");
            txt += "\n\n장소: " + (g.Map != null ? g.Map.title : "") +
                   "\n난이도: " + (s.D.difficulty == "story" ? "이야기" : s.D.difficulty) +
                   "\n플레이 시간: " + Mathf.FloorToInt(s.D.playSeconds / 60) + "분";
            body.text = txt;
            yield return null;
            while (!(GameInput.CancelDown || GameInput.ConfirmDown)) yield return null;
            Object.Destroy(rt.gameObject);
        }
    }

    /// <summary>설정: 음악·환경음·효과음 볼륨, 화면 흔들림·번쩍임 강도, 글자 속도. ←/→ 로 조절.</summary>
    public static class SettingsMenu
    {
        public static IEnumerator Run(Transform parent)
        {
            var m = new MenuList(parent, "Settings", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), 300, 17);
            var help = UIKit.Label(m.root, "Help", GameInput.Pair(Act.Left, Act.Right) + " 조절 · " + GameInput.Label(Act.Interact) + " 선택 · " + GameInput.Label(Act.Cancel) + " 닫기", 11, TextAnchor.UpperCenter, UIKit.Dim);
            help.rectTransform.anchorMin = new Vector2(0, 0); help.rectTransform.anchorMax = new Vector2(1, 0);
            help.rectTransform.pivot = new Vector2(0.5f, 1);
            help.rectTransform.offsetMin = new Vector2(0, -18); help.rectTransform.offsetMax = new Vector2(0, -2);
            string[] names = { "전체 음량", "음악", "환경음", "효과음", "화면 흔들림", "화면 번쩍임", "글자 속도", "키 변경", "닫기" };
            System.Func<int, float> get = i => i == 0 ? GameSettings.Master : i == 1 ? GameSettings.Music : i == 2 ? GameSettings.Ambience : i == 3 ? GameSettings.Sfx
                : i == 4 ? GameSettings.Shake : i == 5 ? GameSettings.Flash : i == 6 ? (GameSettings.TextSpeed - 0.5f) / 1.5f : 0;
            System.Action<int, float> set = (i, v) =>
            {
                v = Mathf.Clamp01(Mathf.Round(v * 10f) / 10f);
                if (i == 0) GameSettings.Master = v;
                if (i == 1) GameSettings.Music = v;
                if (i == 2) GameSettings.Ambience = v;
                if (i == 3) GameSettings.Sfx = v;
                if (i == 4) GameSettings.Shake = v;
                if (i == 5) GameSettings.Flash = v;
                if (i == 6) GameSettings.TextSpeed = 0.5f + v * 1.5f;
            };
            System.Func<string[]> labels = () =>
            {
                var l = new string[names.Length];
                for (int i = 0; i < names.Length; i++)
                    l[i] = i < 7 ? names[i].PadRight(7) + " " + UIKit.Bar(Mathf.RoundToInt(get(i) * 10), 10, 10) + ((i == 4 || i == 5) && get(i) <= 0.001f ? " 끔" : "") : names[i];
                return l;
            };
            m.SetItems(labels());
            yield return null;
            while (true)
            {
                int r = m.Tick(true);
                int h = GameInput.NavHorizontal();
                if (h != 0 && m.index < 7)
                {
                    set(m.index, get(m.index) + h * 0.1f);
                    int keep = m.index; m.SetItems(labels()); m.index = keep; m.Refresh();
                    Sfx.Play("move");
                }
                if (r == 7) { m.SetActive(false); yield return KeyBindMenu.Run(parent); m.SetActive(true); yield return null; continue; }
                if (r == -2 || r == names.Length - 1) break;
                yield return null;
            }
            m.Destroy();
        }
    }

    /// <summary>키 변경: 동작을 고르고 새 키를 누른다. 같은 키를 쓰던 동작과는 맞바꾼다. 안내 문구는 모두 실제 바인딩을 읽는다.</summary>
    public static class KeyBindMenu
    {
        static readonly string[] names = { "왼쪽 이동", "오른쪽 이동", "점프", "아래 / 엄폐", "기본 공격", "기계 간섭", "강한 방전", "전선 이동", "상호작용", "취소 / 메뉴", "컷신 스킵", "위 / 사다리" };

        public static IEnumerator Run(Transform parent)
        {
            var m = new MenuList(parent, "Keys", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), 320, 14);
            var help = UIKit.Label(m.root, "Help", "", 11, TextAnchor.UpperCenter, UIKit.Dim);
            help.rectTransform.anchorMin = new Vector2(0, 0); help.rectTransform.anchorMax = new Vector2(1, 0);
            help.rectTransform.pivot = new Vector2(0.5f, 1);
            help.rectTransform.offsetMin = new Vector2(0, -32); help.rectTransform.offsetMax = new Vector2(0, -2);
            System.Func<string[]> labels = () =>
            {
                var l = new List<string>();
                for (int i = 0; i < names.Length; i++) l.Add(names[i].PadRight(12) + " " + GameInput.Label(GameInput.RebindOrder[i]));
                l.Add("기본값으로"); l.Add("닫기");
                return l.ToArray();
            };
            m.SetItems(labels());
            yield return null;
            while (true)
            {
                help.text = GameInput.Label(Act.Dodge) + " · 대시\n" + GameInput.Label(Act.Interact) + " 바꿀 동작 선택 · " + GameInput.Label(Act.Cancel) + " 닫기";
                int r = m.Tick(true);
                if (r == -1) { yield return null; continue; }
                if (r == -2 || r == names.Length + 1) break;
                if (r == names.Length) { GameInput.ResetBindings(); int k0 = m.index; m.SetItems(labels()); m.index = k0; m.Refresh(); yield return null; continue; }
                help.text = "새 키를 누르세요 (" + names[r] + ")";
                yield return null;
                var key = UnityEngine.InputSystem.Key.None;
                while (key == UnityEngine.InputSystem.Key.None) { key = GameInput.AnyKeyDown(); yield return null; }
                GameInput.Rebind(GameInput.RebindOrder[r], key);
                GameInput.LatchAll();
                int keep = m.index; m.SetItems(labels()); m.index = keep; m.Refresh();
                Sfx.Play("ok");
                yield return null;
            }
            m.Destroy();
        }
    }
}
