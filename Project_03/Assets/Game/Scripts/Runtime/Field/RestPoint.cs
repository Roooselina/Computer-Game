using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>안전실 침대·휴게 의자: 일행 안정도와 전하 전부 회복 + 저장 (GDD 4.7). 횟수 제한 없음.</summary>
    public class RestPoint : Interactable
    {
        public string checkpointId;
        public string beforeEventId;    // 처음 쉴 때 연출 (선택)

        void Reset() { promptText = "쉬기"; }

        public override IEnumerator Interact(Game g)
        {
            if (!string.IsNullOrEmpty(beforeEventId) && !g.State.Has("done_" + beforeEventId))
            {
                yield return StoryEvents.Run(g, beforeEventId);
                yield break;
            }
            yield return Rest(g, checkpointId);
        }

        public static IEnumerator Rest(Game g, string checkpoint)
        {
            yield return g.UI.Fade(1f, 0.4f);
            g.State.HealAll();
            g.UI.hud.Refresh(g.State);
            yield return new WaitForSecondsRealtime(0.5f);
            yield return g.UI.Fade(0f, 0.4f);
            g.SaveNow(string.IsNullOrEmpty(checkpoint) ? "rest:" + g.CurrentMapId : checkpoint, false);
            g.UI.Toast("푹 쉬었다. 안정도·전하 회복. 저장했습니다.");
            g.Bubbles.Request("rest");
        }
    }
}
