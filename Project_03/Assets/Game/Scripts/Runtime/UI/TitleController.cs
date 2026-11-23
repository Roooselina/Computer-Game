using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>시작 화면: 새 게임 / 이어하기 / 종료.</summary>
    public class TitleController : MonoBehaviour
    {
        MenuList menu;
        Text info;
        Canvas canvas;
        bool busy;

        IEnumerator Start()
        {
            Game.Ensure();
            while (Game.I == null) yield return null;
            var g = Game.I;
            g.UI.hud.SetVisible(false);
            var cam = Camera.main;
            if (cam != null) PixelScreen.Ensure().Attach(cam);
            canvas = UIKit.MakeCanvas("TitleCanvas", 5, transform);
            var title = UIKit.Label(canvas.transform, "Title", "ELECTRIC WORLD: LAST LIGHT", 11, TextAnchor.UpperCenter, UIKit.Accent);
            title.fontSize = 22;
            title.rectTransform.offsetMax = new Vector2(0, -70);
            var sub = UIKit.Label(canvas.transform, "Sub", "일렉트릭 월드: 라스트 라이트", 11, TextAnchor.UpperCenter, UIKit.Ink);
            sub.rectTransform.offsetMax = new Vector2(0, -104);
            var band = UIKit.Rect("InfoBand", canvas.transform, new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, 26));
            band.gameObject.AddComponent<Image>().color = new Color(0.03f, 0.03f, 0.06f, 0.85f);
            info = UIKit.Label(canvas.transform, "Info", "", 11, TextAnchor.LowerCenter, UIKit.Ink);
            info.rectTransform.offsetMin = new Vector2(20, 8);
            info.text = "개발용 임시 그래픽 사용 중 · " + GameInput.Pair(Act.Left, Act.Right) + " 이동  " + GameInput.Label(Act.Jump) + " 점프  " + GameInput.Label(Act.Down) + " 웅크리기  "
                        + GameInput.Label(Act.Dodge) + " · 대시  " + GameInput.Label(Act.Skill1) + "/" + GameInput.Label(Act.Skill2) + "/" + GameInput.Label(Act.Skill3) + "/" + GameInput.Label(Act.Skill4) + " 기술  "
                        + GameInput.Label(Act.Interact) + " 조사·결정  " + GameInput.Label(Act.Cancel) + " 취소·일시정지 (설정에서 키 변경)";
            menu = new MenuList(canvas.transform, "Menu", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0, -6), 130, 18);
            menu.SetItems(new[] { "새 게임", "이어하기", "설정", "종료" }, new[] { true, SaveSystem.HasAnySave, true, true });
            if (SaveSystem.HasAnySave) { menu.index = 1; menu.Refresh(); }
            if (AudioManager.I != null) { AudioManager.I.PlayAmbience(null, 0.5f); AudioManager.I.PlayMusic("bgm_title", 1f, 0.6f); }
        }

        void Update()
        {
            if (menu == null || busy || Game.I == null) return;
            int r = menu.Tick(false);
            if (r < 0) return;
            if (r == 0) StartCoroutine(NewGame());
            else if (r == 1)
            {
                if (!Game.I.ContinueGame(out string msg)) { info.text = msg; info.color = UIKit.Danger; menu.enabled[1] = false; menu.Refresh(); }
                else { busy = true; if (!string.IsNullOrEmpty(msg)) Game.I.UI.Toast(msg, 4f); }
            }
            else if (r == 2) StartCoroutine(Settings());
            else if (r == 3)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        IEnumerator Settings()
        {
            busy = true;
            menu.SetActive(false);
            yield return SettingsMenu.Run(canvas.transform);
            menu.SetActive(true);
            busy = false;
        }

        IEnumerator NewGame()
        {
            busy = true;
            if (SaveSystem.HasAnySave)
            {
                info.text = "새 게임을 시작하면 첫 저장 시점에 기존 저장을 덮어씁니다.";
                yield return Game.I.Choose(new[] { "새로 시작", "취소" });
                if (Game.I.LastChoice != 0) { busy = false; yield break; }
            }
            Game.I.NewGame();
        }
    }
}
