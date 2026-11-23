using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>스토리용 등장인물 배치 표시 (베넬·연구원 등). 시작하면 그래픽을 붙인다.</summary>
    public class ActorSpawn : MonoBehaviour
    {
        public string actorId;
        public string artId;
        public int facing = -1;
        public string requireFlag;
        public string hideFlag;
        public string hideFlag2;
        public string idleState = "Idle";
        [Tooltip("이 플래그가 있으면 쓰러진 상태로 downPos 에 둔다 (불러오기 후 복원)")] public string downFlag;
        public string downState = "Down";
        public Vector3 downPos;
        Actor actor;
        bool wasDown;
        [System.NonSerialized] public bool HideForCutscene;

        void Start()
        {
            var g = Game.I;
            if (g == null) return;
            actor = gameObject.AddComponent<Actor>();
            actor.actorId = actorId;
            actor.view = CharacterView.Create(transform, g.Config.GetArt(string.IsNullOrEmpty(artId) ? actorId : artId), 9);
            actor.view.Face(facing);
            actor.view.Play(idleState);
            if (!string.IsNullOrEmpty(downFlag) && g.State != null && g.State.Has(downFlag))
            {
                transform.position = downPos; actor.view.Play(downState); wasDown = true;
            }
            Refresh();
        }

        void Update() { Refresh(); }

        /// <summary>쓰러진 상태를 즉시 적용 (컷신을 건너뛸 때 등장 전이었던 인물도 정리).</summary>
        public void ApplyDown()
        {
            if (string.IsNullOrEmpty(downFlag)) return;
            transform.position = downPos;
            wasDown = true;
            if (actor != null && actor.view != null) actor.view.Play(downState);
        }

        public static void ApplyDownAll()
        {
            foreach (var a in Object.FindObjectsByType<ActorSpawn>())
                if (!string.IsNullOrEmpty(a.downFlag) && Game.I != null && Game.I.State != null && Game.I.State.Has(a.downFlag)) a.ApplyDown();
        }

        public void ForceRefresh() { Refresh(); }

        void Refresh()
        {
            var g = Game.I;
            if (g == null || g.State == null || actor == null) return;
            bool show = (string.IsNullOrEmpty(requireFlag) || g.State.Has(requireFlag)) && (string.IsNullOrEmpty(hideFlag) || !g.State.Has(hideFlag))
                        && (string.IsNullOrEmpty(hideFlag2) || !g.State.Has(hideFlag2));
            if (actor.view != null) actor.view.SetVisible(show && !HideForCutscene);
            actor.enabled = show;
            if (!wasDown && !string.IsNullOrEmpty(downFlag) && g.State.Has(downFlag)) { wasDown = true; if (actor.view != null) actor.view.Play(downState); }
        }
    }
}
