using System.Collections;
using UnityEngine;

namespace EW
{
    /// <summary>컨베이어 제어반: 정상 조작으로 벨트 방향을 바꾼다. 전원이 없으면 아무것도 바뀌지 않는다 (화면에 정지 표시).</summary>
    public class BeltControl : Interactable
    {
        public string conveyorId;
        public SpriteRenderer art;
        public Sprite[] frames;   // 0 정방향(왼쪽) / 1 역방향(오른쪽) / 2 정지

        void Reset() { promptText = "벨트 제어반"; }

        Conveyor Belt => Conveyor.Find(conveyorId);

        void Update()
        {
            var b = Belt;
            if (art == null || frames == null || frames.Length < 3 || b == null) return;
            art.sprite = !b.Running ? frames[2] : b.Dir < 0 ? frames[0] : frames[1];
        }

        public override IEnumerator Interact(Game g)
        {
            var b = Belt;
            if (b == null) yield break;
            if (!b.Running)
            {
                Sfx.Play("sfx_ui_no", 0.6f);
                yield return g.Info("제어반 화면이 꺼져 있다. 벨트 전원이 끊겼다.");
                yield break;
            }
            b.Dir = -b.Dir;
            Sfx.Play("sfx_ui_ok", 0.8f);
            Sfx.Play("sfx_power_down", 0.3f, 1.8f);
            StoryEvents.Onoma(g, (Vector2)transform.position + new Vector2(0, 1.4f), b.Dir < 0 ? "철컥 ← 벨트" : "철컥 → 벨트", new Color(1f, 0.92f, 0.5f));
            g.State.Set("belt_touched_" + conveyorId);
        }
    }
}
