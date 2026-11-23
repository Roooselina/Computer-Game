using System.Collections;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 베넬에게 J 로 말 걸기 / 다가가기. 숨을 고르거나(Recover) 약을 섞는 빈틈(Remix)에만 말을 걸 수 있고,
    /// 쓰러진(Downed) 뒤에는 가까이 가서 J 로 '다가가기' — 처치 여부를 정하는 장면이 열린다.
    /// </summary>
    public class VenelleTalk : Interactable
    {
        public VenelleBoss boss;
        void Awake() { id = "venelle_talk"; range = 4.2f; rangeY = 1.2f; promptOffset = new Vector2(0, 2.1f); }
        public override bool CanInteract(Game g) => boss != null && ((boss.TalkAvailable && boss.Alive) || (boss.Downed && g.Player != null && Mathf.Abs(g.Player.transform.position.x - boss.transform.position.x) < 1.7f));
        public override string Prompt(Game g) => boss != null && boss.Downed ? "다가가기" : "말 걸기";
        public override IEnumerator Interact(Game g)
        {
            if (boss.Downed)
            {
                if (Mathf.Abs(g.Player.transform.position.x - boss.transform.position.x) > 1.8f)
                {
                    g.Player.ScriptControl = true;
                    yield return g.Player.WalkTo(boss.transform.position.x - 1.2f * Mathf.Sign(boss.transform.position.x - g.Player.transform.position.x + 0.001f));
                }
                yield return StoryEvents.VenelleDownedChoice(g, boss);
            }
            else yield return StoryEvents.VenelleTalk(g, boss);
        }
    }
}
