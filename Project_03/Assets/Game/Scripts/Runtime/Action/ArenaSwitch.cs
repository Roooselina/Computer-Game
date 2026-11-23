using System.Collections;
using UnityEngine;

namespace EW
{
    /// <summary>보스 구역의 조작 장치 (유지보수 단말·제동 장치·셔터·믹서·전자석·냉각 밸브). 실제 처리는 지금 싸우는 보스가 맡는다.</summary>
    public interface IArenaUser
    {
        bool CanUse(ArenaSwitch s);
        string PromptFor(ArenaSwitch s);
        IEnumerator Use(ArenaSwitch s);
    }

    public class ArenaSwitch : Interactable
    {
        public string kind = "terminal";
        public SpriteRenderer art;
        public Sprite[] frames;
        public static IArenaUser User;

        public override string Prompt(Game g) => User != null ? (User.PromptFor(this) ?? promptText) : promptText;
        public override bool CanInteract(Game g) => User != null && User.CanUse(this);
        public override IEnumerator Interact(Game g) { if (User != null) yield return User.Use(this); }
        public void SetFrame(int i) { if (art != null && frames != null && frames.Length > 0) art.sprite = frames[Mathf.Clamp(i, 0, frames.Length - 1)]; }
    }
}
