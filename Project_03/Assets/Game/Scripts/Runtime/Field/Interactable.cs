using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>조사 키(Z/E)로 상호작용할 수 있는 물건. 가까이 가면 'Z 조사' 표시가 뜬다.</summary>
    public abstract class Interactable : MonoBehaviour
    {
        public static readonly List<Interactable> All = new List<Interactable>();
        [Tooltip("저장·중복 방지에 쓰는 고정 ID")]
        public string id;
        public float range = 0.9f;
        public float rangeY = 0.8f;
        public string promptText = "조사";
        public Vector2 promptOffset = new Vector2(0, 1.3f);

        protected virtual void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        protected virtual void OnDisable() { All.Remove(this); }

        public virtual Vector3 PromptWorldPos => transform.position + (Vector3)promptOffset;
        public virtual string Prompt(Game g) => promptText;
        public virtual bool CanInteract(Game g) => true;
        public abstract IEnumerator Interact(Game g);
        public virtual void OnStateLoaded(GameState s) { }
    }
}
