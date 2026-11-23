using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 들어가면 실행되는 영역: 맵 이동, 스토리 이벤트, 동료 말풍선 조건.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class TriggerZone : MonoBehaviour
    {
        public enum Kind { MapExit, Event, Bubble }
        public Kind kind;
        public string targetMap;
        public string targetSpawn;
        public string eventId;
        public string bubbleCond;
        [Tooltip("이 플래그가 있어야 동작")]
        public string requireFlag;
        [Tooltip("이 플래그가 있으면 동작하지 않음")]
        public string blockFlag;
        [Tooltip("조건 미충족일 때 안내 (대사 id)")]
        public string blockedDialogue;
        bool inside;
        bool armed;     // 맵에 들어온 직후 이미 영역 안이면 한 번 벗어난 뒤에 동작
        BoxCollider2D box;

        static readonly List<TriggerZone> all = new List<TriggerZone>();
        void OnEnable() { all.Add(this); box = GetComponent<BoxCollider2D>(); }
        void OnDisable() { all.Remove(this); }

        void Update()
        {
            var g = Game.I;
            if (g == null || g.Player == null || g.State == null) return;
            bool now = box.OverlapPoint((Vector2)g.Player.transform.position + Vector2.up * 0.5f);
            if (!armed) { armed = true; inside = now; return; }
            if (now && !inside && g.FieldInputAllowed)
            {
                inside = true;
                Fire(g);
            }
            else if (!now) inside = false;
        }

        void Fire(Game g)
        {
            if (!string.IsNullOrEmpty(blockFlag) && g.State.Has(blockFlag) && !(blockFlag == "c6_combat_locked" && g.State.Has("c3_boss_done")))
            {
                if (!string.IsNullOrEmpty(blockedDialogue)) g.RunEvent(Blocked(g));
                return;
            }
            if (!string.IsNullOrEmpty(requireFlag) && !g.State.Has(requireFlag))
            {
                if (!string.IsNullOrEmpty(blockedDialogue)) g.RunEvent(Blocked(g));
                return;
            }
            switch (kind)
            {
                case Kind.MapExit: g.GoToMap(targetMap, targetSpawn); break;
                case Kind.Event: g.RunEvent(StoryEvents.Run(g, eventId)); break;
                case Kind.Bubble: g.Bubbles.Request(bubbleCond); break;
            }
        }

        IEnumerator Blocked(Game g)
        {
            yield return g.Say(blockedDialogue);
            // 막힌 출구에서 한 걸음 물러난다
            var p = g.Player;
            float dir = Mathf.Sign(p.transform.position.x - transform.position.x);
            if (dir == 0) dir = -1;
            yield return p.WalkTo(transform.position.x + dir * (box.size.x / 2f + 0.8f));
        }
    }
}
