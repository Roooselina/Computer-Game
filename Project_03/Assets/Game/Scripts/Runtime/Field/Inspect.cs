using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 일반 조사 물건. 처음엔 dialogueId, 다시 조사하면 repeatDialogueId.
    /// eventId 가 있으면 StoryEvents 의 연출을 실행한다.
    /// </summary>
    public class Inspect : Interactable
    {
        public string dialogueId;
        public string repeatDialogueId;
        public string eventId;
        public string setFlag;
        [Tooltip("이 플래그가 있어야 조사 가능")] public string requireFlag;
        [Tooltip("이 플래그가 있으면 조사 불가")] public string hideFlag;
        [Tooltip("동료 말풍선 조건 (조사 후 요청)")] public string bubbleCond;

        public override bool CanInteract(Game g)
        {
            if (!string.IsNullOrEmpty(requireFlag) && !g.State.Has(requireFlag)) return false;
            if (!string.IsNullOrEmpty(hideFlag) && g.State.Has(hideFlag)) return false;
            return true;
        }

        public override IEnumerator Interact(Game g)
        {
            if (!string.IsNullOrEmpty(eventId)) { yield return StoryEvents.Run(g, eventId); yield break; }
            string seen = "seen_" + id;
            bool first = !g.State.Has(seen);
            g.State.Set(seen);
            string d = !first && !string.IsNullOrEmpty(repeatDialogueId) ? repeatDialogueId : dialogueId;
            if (!string.IsNullOrEmpty(d)) yield return g.Say(d);
            if (!string.IsNullOrEmpty(setFlag)) g.State.Set(setFlag);
            if (!string.IsNullOrEmpty(bubbleCond)) g.Bubbles.Request(bubbleCond);
        }
    }
}
