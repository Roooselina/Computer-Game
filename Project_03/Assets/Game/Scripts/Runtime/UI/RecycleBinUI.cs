using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>
    /// 9장 휴지통 (GDD 3.5) — 포획 단말 안의 '복구 대기 보관함' 목록. 게임 안 가상 창이며 실제 사용자 PC 의 파일을 건드리지 않는다.
    /// 순서: 목록(이름·원래 위치·삭제한 날짜) → '삭제한 날짜' 정렬 → E-D05-47 항목 선택 → 원래 위치 확인 → '선택한 항목 복원'
    /// → 라비엘이 바깥에서 '구속 해제' → 루의 '내부 확인'. 복원과 구속 해제는 어느 쪽이 먼저여도 대기 상태로 유지된다 (반사신경 없음).
    /// 휴지통 비우기 예약은 라비엘이 일시 중지해 기본 난이도에서는 제한시간이 없다. 전기 공격·밸브로 파일을 복원하는 기능은 없다.
    /// </summary>
    public class RecycleBinUI
    {
        public class Item { public string name, origin, deleted; public int order; public bool isLu, isDoc, restored; }

        public readonly List<Item> Items = new List<Item>();
        public bool SortedByDate { get; private set; }
        public int Selected { get; set; } = -1;
        public bool LuRestored { get; private set; }
        public bool Released { get; set; }
        public bool DocRestored { get; private set; }
        public int WrongRestores { get; private set; }

        public const string LuName = "E-D05-47.lux";
        public const string LuOrigin = @"C:\Vesper\Transfer\Intake\E-D05-47.lux";

        public static RecycleBinUI MakeDefault()
        {
            var b = new RecycleBinUI();
            void Add(string n, string o, string d, int ord, bool lu = false, bool doc = false) => b.Items.Add(new Item { name = n, origin = o, deleted = d, order = ord, isLu = lu, isDoc = doc });
            Add("intake_queue_0412.tmp", @"C:\Vesper\Transfer\Intake\", "04-12 09:14", 2);
            Add("모집 승인 대장 (사본).doc", @"C:\Vesper\Admin\Recruit\", "05-02 22:41", 5, doc: true);
            Add("A-M21-12_trial.log", @"C:\Vesper\Lab\Chem\", "03-30 17:02", 1);
            Add("boarding_seat_map_v3.png", @"C:\Vesper\Exodus\Board\", "05-03 03:10", 8);
            Add("cooling_alert_off.cfg", @"C:\Vesper\Transfer\Core\", "05-03 03:41", 9);
            Add(LuName, LuOrigin.Substring(0, LuOrigin.LastIndexOf('\\') + 1), "05-03 04:02", 12, lu: true);
            Add("recital_program_old.txt", @"C:\Vesper\Hall\", "04-28 20:15", 4);
            Add("waste_ledger_B2.xls", @"C:\Vesper\Under\Waste\", "04-20 11:30", 3);
            Add("E-D05-47_backup_old.lux (손상)", @"C:\Vesper\Transfer\Intake\Old\", "02-11 06:20", 0);
            Add("reception_tickets.db", @"C:\Vesper\Reception\", "05-03 01:55", 7);
            Add("guard_rota_wk18.pdf", @"C:\Vesper\Security\", "05-01 08:00", 6);
            Add("capsule_check_skip.cfg", @"C:\Vesper\Exodus\Capsule\", "05-03 03:58", 11);
            Add("broadcast_draft.wav", @"C:\Vesper\Hall\Broadcast\", "05-03 03:50", 10);
            // 처음엔 이름 순 (루의 항목이 중간에 묻혀 있다)
            b.Items.Sort((x, y) => string.CompareOrdinal(x.name, y.name));
            return b;
        }

        public void SortByDeleted()
        {
            Items.Sort((x, y) => y.order.CompareTo(x.order));
            SortedByDate = true; Selected = -1;
        }

        public Item Sel => Selected >= 0 && Selected < Items.Count ? Items[Selected] : null;

        /// <summary>선택한 항목 복원. 루의 항목이면 원래 위치로 (구속은 아직). 결과 문구를 돌려준다.</summary>
        public string RestoreSelected()
        {
            var it = Sel;
            if (it == null) return "먼저 항목을 고르세요.";
            if (it.restored) return "이미 원래 위치로 복원한 항목입니다.";
            if (it.isLu) { it.restored = true; LuRestored = true; return "복원: " + it.name + " → " + LuOrigin; }
            if (it.isDoc) { it.restored = true; DocRestored = true; return "복원: " + it.name + " → " + it.origin + " (문서 보기에서 읽을 수 있다)"; }
            if (it.name.Contains("손상")) { WrongRestores++; return "손상된 옛 사본은 복원할 수 없습니다. (지금의 루가 아니다)"; }
            WrongRestores++;
            it.restored = true;
            return "복원: " + it.name + " → " + it.origin;
        }

        public bool CanConfirm => LuRestored && Released;

        // ============================================================ 화면
        public static IEnumerator Run(Game g, RecycleBinUI bin, System.Func<RecycleBinUI, IEnumerator> onLuRestored, System.Func<string, IEnumerator> onDoc)
        {
            int ctx = GameInput.Push(InputCtx.Menu);
            g.Block("bin");
            var canvas = UIKit.MakeCanvas("BinCanvas", 30, g.transform);
            var win = UIKit.Rect("Window", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UIKit.Place(win, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(560, 270));
            UIKit.Solid(win, "Bg", new Color(0.86f, 0.88f, 0.92f));
            var bar = UIKit.Rect("TitleBar", win, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -16), Vector2.zero);
            UIKit.Solid(bar, "Bg", new Color(0.22f, 0.3f, 0.46f));
            var title = UIKit.Label(bar, "T", "복구 대기 보관함 — 포획 단말 CAP-02 (베스페르 내부 단말)", 10, TextAnchor.MiddleLeft, Color.white);
            title.rectTransform.offsetMin = new Vector2(6, 0);
            var head = UIKit.Label(win, "Head", "", 10, TextAnchor.UpperLeft, new Color(0.1f, 0.12f, 0.18f));
            head.rectTransform.offsetMin = new Vector2(8, 0); head.rectTransform.offsetMax = new Vector2(-8, -20);
            var list = UIKit.Label(win, "List", "", 10, TextAnchor.UpperLeft, new Color(0.08f, 0.1f, 0.14f));
            list.rectTransform.offsetMin = new Vector2(8, 46); list.rectTransform.offsetMax = new Vector2(-8, -48);
            list.font = UIKit.Pixel != null ? list.font : list.font;
            var status = UIKit.Label(win, "Status", "", 10, TextAnchor.LowerLeft, new Color(0.15f, 0.2f, 0.3f));
            status.rectTransform.offsetMin = new Vector2(8, 26); status.rectTransform.offsetMax = new Vector2(-8, 0);
            var toolbar = UIKit.Label(win, "Toolbar", "", 10, TextAnchor.LowerLeft, new Color(0.1f, 0.1f, 0.1f));
            toolbar.rectTransform.offsetMin = new Vector2(8, 6); toolbar.rectTransform.offsetMax = new Vector2(-8, 0);
            string[] buttons = { "정렬: 삭제한 날짜", "선택한 항목 복원", "내부 확인", "닫기" };
            int focusList = 0, btn = 0; bool inToolbar = false;
            string msg = "항목을 고르고 아래 버튼을 쓴다. (W/S 또는 ↑/↓ 이동 · A/D 버튼 · " + GameInput.Label(Act.Interact) + " 결정)";
            bool done = false;
            yield return null;
            while (!done)
            {
                // 표시
                head.text = string.Format("{0,-34}{1,-34}{2}", "이름", "원래 위치", "삭제한 날짜") + (bin.SortedByDate ? "   ▼ 최근 삭제 순" : "   (이름 순)");
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < bin.Items.Count; i++)
                {
                    var it = bin.Items[i];
                    bool cur = !inToolbar && i == focusList;
                    bool sel = i == bin.Selected;
                    string mark = sel ? "■ " : cur ? "▶ " : "   ";
                    sb.AppendLine(mark + Pad(it.name, 32) + Pad(it.origin, 32) + it.deleted + (it.restored ? "  (복원됨)" : ""));
                }
                list.text = sb.ToString();
                status.text = "휴지통 비우기 예약: 일시 중지됨 (라비엘)   ·   복원: " + (bin.LuRestored ? "완료" : "대기") + "   ·   구속 해제: " + (bin.Released ? "완료" : "대기") + "\n" + msg;
                var tb = new System.Text.StringBuilder();
                for (int i = 0; i < buttons.Length; i++)
                {
                    bool en = i != 2 || bin.CanConfirm;
                    tb.Append(inToolbar && i == btn ? "[▶ " + buttons[i] + "]  " : en ? "[ " + buttons[i] + " ]  " : "( " + buttons[i] + " )  ");
                }
                toolbar.text = tb.ToString();
                yield return null;
                // 입력
                if (MenuList.ScriptedChoices.Count > 0)
                {
                    int s = MenuList.ScriptedChoices.Dequeue();
                    // 자동 테스트: 0 정렬, 1 루 항목 선택, 2 복원, 3 내부 확인, 4 닫기, 10+i 항목 i 선택
                    if (s == 0) { bin.SortByDeleted(); msg = "삭제한 날짜 순으로 정렬했다."; }
                    else if (s == 1) { bin.Selected = bin.Items.FindIndex(x => x.isLu); msg = Detail(bin.Sel); }
                    else if (s == 2) { msg = bin.RestoreSelected(); if (bin.Sel != null && bin.Sel.isLu && onLuRestored != null) yield return onLuRestored(bin); else if (bin.Sel != null && bin.Sel.isDoc && onDoc != null) yield return onDoc("D15"); }
                    else if (s == 3) { if (bin.CanConfirm) done = true; else msg = "아직 원래 위치가 아니다 / 구속이 풀리지 않았다."; }
                    else if (s == 4) done = true;
                    else if (s >= 10) { bin.Selected = Mathf.Clamp(s - 10, 0, bin.Items.Count - 1); msg = Detail(bin.Sel); }
                    continue;
                }
                int v = GameInput.NavVertical();
                int h = GameInput.NavHorizontal();
                if (v != 0)
                {
                    if (inToolbar && v < 0) { inToolbar = false; focusList = bin.Items.Count - 1; }
                    else if (!inToolbar)
                    {
                        focusList += v;
                        if (focusList >= bin.Items.Count) { inToolbar = true; focusList = bin.Items.Count - 1; }
                        focusList = Mathf.Clamp(focusList, 0, bin.Items.Count - 1);
                    }
                    Sfx.Play("move");
                }
                if (h != 0) { inToolbar = true; btn = (btn + h + buttons.Length) % buttons.Length; Sfx.Play("move"); }
                if (GameInput.CancelDown) { done = true; break; }
                if (GameInput.ConfirmDown)
                {
                    if (!inToolbar) { bin.Selected = focusList; msg = Detail(bin.Sel); Sfx.Play("ok"); continue; }
                    switch (btn)
                    {
                        case 0: bin.SortByDeleted(); focusList = 0; msg = "삭제한 날짜 순으로 정렬했다. 맨 위가 가장 최근."; Sfx.Play("ok"); break;
                        case 1:
                            {
                                var sel = bin.Sel;
                                msg = bin.RestoreSelected(); Sfx.Play(sel != null && sel.restored ? "ok" : "no");
                                if (sel != null && sel.isLu && onLuRestored != null) yield return onLuRestored(bin);
                                else if (sel != null && sel.isDoc && sel.restored && onDoc != null) yield return onDoc("D15");
                                break;
                            }
                        case 2:
                            if (bin.CanConfirm) { done = true; Sfx.Play("ok"); }
                            else { msg = !bin.LuRestored ? "아직 원래 위치가 아니야. 먼저 내 항목을 복원해야 해." : "라비엘이 구속 해제를 누를 때까지 기다린다."; Sfx.Play("no"); }
                            break;
                        case 3: done = true; break;
                    }
                }
            }
            Object.Destroy(canvas.gameObject);
            g.Unblock("bin");
            GameInput.Pop(ctx);
        }

        static string Detail(Item it) => it == null ? "" : "선택: " + it.name + " · 원래 위치 " + it.origin + " · 삭제 " + it.deleted;

        static string Pad(string s, int n)
        {
            // 한글은 두 칸으로 센다 (고정 폭 정렬 근사)
            int w = 0; var sb = new System.Text.StringBuilder();
            foreach (var ch in s) { int cw = ch > 0x1100 ? 2 : 1; if (w + cw > n - 1) { sb.Append('…'); w++; break; } sb.Append(ch); w += cw; }
            while (w < n) { sb.Append(' '); w++; }
            return sb.ToString();
        }
    }
}
