using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>
    /// 마지막 — LAST LIGHT (GDD): 검은 화면 크레딧 → 게임 안에서 그린 '가상 바탕화면' 한 번.
    /// 방금 게임이 실행되던 창이 응답 없음 상태, '종료 / 대기'. 대기하면 5초 뒤 같은 선택으로 돌아온다 (몇 번이어도 다른 결말 없음).
    /// 종료하면 연출 속 창이 닫히고 빈 바탕화면만 남는다. 그 뒤 게임 자체 메뉴로 타이틀/게임 종료를 고른다.
    /// 실제 OS 창·파일·전원·시계는 건드리지 않는다. 스파크·움직이는 아이콘·목소리·새 파일·다음 세션 문구 없음.
    /// </summary>
    public static class EndingScreens
    {
        public static int WaitCount { get; private set; }
        public static bool DesktopShown { get; private set; }
        public static bool WindowClosed { get; private set; }
        public static float WaitSeconds = 5f;

        static readonly string[] credits =
        {
            "ELECTRIC WORLD: LAST LIGHT", "",
            "원안 · 기획", "프로젝트 작성자", "",
            "등장인물", "루 · 베넬(아이비) · 린든 · 라비엘 · 메이린 · 라크리모사(에드워드) · 아우렐리우스 · F · 008", "",
            "개발 중 임시 그래픽", "개별 도트 임시 그림 (Tools 생성기) · Foozle Sci-Fi Lab · ansimuz", "",
            "임시 효과음", "개발용 생성 효과음", "",
            "",
        };

        public static IEnumerator Credits(Game g)
        {
            var canvas = UIKit.MakeCanvas("Credits", 60, g.transform);
            UIKit.Solid(canvas.transform, "Black", Color.black);
            var t = UIKit.Label(canvas.transform, "T", string.Join("\n", credits), 12, TextAnchor.UpperCenter, new Color(0.85f, 0.86f, 0.9f));
            t.lineSpacing = 1.3f;
            var rt = t.rectTransform; rt.offsetMin = new Vector2(0, -600); rt.offsetMax = new Vector2(0, -360);
            float dur = 22f;
            for (float k = 0; k < dur; k += Time.unscaledDeltaTime)
            {
                rt.anchoredPosition = new Vector2(0, Mathf.Lerp(0, 760, k / dur));
                if (GameInput.SkipDown) break;
                yield return null;
            }
            yield return new WaitForSecondsRealtime(1f);
            Object.Destroy(canvas.gameObject);
        }

        public static IEnumerator Desktop(Game g)
        {
            WaitCount = 0; DesktopShown = true; WindowClosed = false;
            int ctx = GameInput.Push(InputCtx.Menu);
            var canvas = UIKit.MakeCanvas("EndingDesktop", 61, g.transform);
            // 바탕화면 (작품 안에서 그린 가상의 화면 — 특정 운영체제 그림을 쓰지 않는다)
            UIKit.Solid(canvas.transform, "Wallpaper", new Color(0.13f, 0.2f, 0.27f));
            var grad = UIKit.Rect("Grad", canvas.transform, new Vector2(0, 0), new Vector2(1, 0.45f), Vector2.zero, Vector2.zero);
            grad.gameObject.AddComponent<Image>().color = new Color(0.09f, 0.14f, 0.2f);
            string[] icons = { "U_IconFolder", "U_IconDoc", "U_IconBin", "U_IconGame" };
            string[] names = { "문서", "메모.txt", "휴지통", "LAST LIGHT" };
            for (int i = 0; i < icons.Length; i++)
            {
                var ic = UIKit.Rect("Icon" + i, canvas.transform, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, Vector2.zero);
                UIKit.Place(ic, new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -16 - i * 46), new Vector2(28, 28));
                var im = ic.gameObject.AddComponent<Image>(); im.preserveAspect = true;
                var set = g.Config.GetSet(icons[i]); if (set != null && set.frames.Length > 0) im.sprite = set.frames[0];
                var lb = UIKit.Label(canvas.transform, "IconName" + i, names[i], 9, TextAnchor.UpperCenter, Color.white);
                UIKit.Place(lb.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(2, -46 - i * 46), new Vector2(60, 12));
            }
            var task = UIKit.Rect("Taskbar", canvas.transform, new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, 18));
            task.gameObject.AddComponent<Image>().color = new Color(0.06f, 0.07f, 0.09f);
            var clock = UIKit.Label(task, "Clock", "03:12", 9, TextAnchor.MiddleRight, new Color(0.8f, 0.82f, 0.86f));
            clock.rectTransform.offsetMax = new Vector2(-8, 0);
            var taskBtn = UIKit.Label(task, "Btn", "  ▣ ELECTRIC WORLD: LAST LIGHT", 9, TextAnchor.MiddleLeft, new Color(0.8f, 0.82f, 0.86f));
            // 응답 없는 게임 창
            var win = UIKit.Rect("GameWindow", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UIKit.Place(win, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(20, 18), new Vector2(420, 250));
            UIKit.Solid(win, "Frame", new Color(0.75f, 0.77f, 0.8f));
            var wt = UIKit.Rect("Title", win, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -14), Vector2.zero);
            UIKit.Solid(wt, "Bg", new Color(0.55f, 0.57f, 0.6f));
            UIKit.Label(wt, "T", " ELECTRIC WORLD: LAST LIGHT (응답 없음)", 9, TextAnchor.MiddleLeft, new Color(0.15f, 0.15f, 0.18f));
            var content = UIKit.Rect("Content", win, Vector2.zero, Vector2.one, new Vector2(3, 3), new Vector2(-3, -15));
            content.gameObject.AddComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f, 1f);   // 응답 없는 창의 흐린 화면
            // 안내 상자
            var box = UIKit.Rect("Dialog", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UIKit.Place(box, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(20, 10), new Vector2(300, 90));
            UIKit.Solid(box, "Bg", new Color(0.93f, 0.94f, 0.95f));
            var bt = UIKit.Label(box, "T", "ELECTRIC WORLD: LAST LIGHT 이(가) 응답하지 않습니다.\n\n프로그램을 종료하면 저장하지 않은 정보가 사라질 수 있습니다.", 9, TextAnchor.UpperLeft, new Color(0.1f, 0.1f, 0.12f));
            bt.rectTransform.offsetMin = new Vector2(10, 26); bt.rectTransform.offsetMax = new Vector2(-10, -8);
            var opts = UIKit.Label(box, "Opts", "", 10, TextAnchor.LowerRight, new Color(0.1f, 0.1f, 0.12f));
            opts.rectTransform.offsetMin = new Vector2(10, 6); opts.rectTransform.offsetMax = new Vector2(-10, 0);
            int sel = -1;   // 기본 선택 없음 (흘러 들어온 입력으로 확정되지 않게)
            yield return new WaitForSecondsRealtime(1.2f);
            GameInput.LatchAll();
            while (true)
            {
                box.gameObject.SetActive(true);
                bool decided = false; int choice = -1;
                while (!decided)
                {
                    opts.text = (sel == 0 ? "[▶ 종료]" : "[ 종료 ]") + "   " + (sel == 1 ? "[▶ 대기]" : "[ 대기 ]");
                    if (MenuList.ScriptedChoices.Count > 0) { choice = MenuList.ScriptedChoices.Dequeue(); decided = true; break; }
                    int h = GameInput.NavHorizontal();
                    if (h != 0) { sel = sel < 0 ? (h > 0 ? 1 : 0) : (sel + h + 2) % 2; Sfx.Play("move"); }
                    var mouse = UnityEngine.InputSystem.Mouse.current;
                    if (GameInput.ConfirmDown && sel >= 0) { choice = sel; decided = true; }
                    if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                    {
                        Vector2 mp = mouse.position.ReadValue();
                        var r = opts.rectTransform; Vector2 local;
                        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(r, mp, null, out local) && r.rect.Contains(local))
                        { choice = local.x > r.rect.xMax - 60 ? 1 : 0; decided = true; }
                    }
                    yield return null;
                }
                if (choice == 1)
                {
                    WaitCount++;
                    box.gameObject.SetActive(false);
                    yield return new WaitForSecondsRealtime(WaitSeconds);
                    continue;   // 변화 없음, 같은 선택
                }
                break;
            }
            // 종료: 연출 속 창이 닫힌다
            box.gameObject.SetActive(false);
            Object.Destroy(win.gameObject);
            taskBtn.text = "";
            WindowClosed = true;
            yield return new WaitForSecondsRealtime(3.5f);
            // 게임 자체 메뉴 (이야기 밖의 정상 기능)
            var menu = new MenuList(canvas.transform, "AfterEnd", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), 140);
            menu.SetItems(new[] { "타이틀로", "게임 종료" });
            int r2;
            while ((r2 = menu.Tick(false)) < 0) yield return null;
            menu.Destroy();
            Object.Destroy(canvas.gameObject);
            GameInput.Pop(ctx);
            DesktopShown = false;
            if (r2 == 1)
            {
#if UNITY_EDITOR
                if (!Application.isBatchMode && !EndingTestMode) UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                if (EndingTestMode) g.ReturnToTitle();
            }
            else g.ReturnToTitle();
        }

        /// <summary>자동 검증 중에는 '게임 종료'가 플레이 모드를 끄지 않고 타이틀로 간다.</summary>
        public static bool EndingTestMode;
    }
}
