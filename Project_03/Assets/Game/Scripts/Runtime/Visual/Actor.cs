using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>스토리 연출에서 이름으로 찾는 등장인물(플레이어·동료·NPC·보스).</summary>
    public class Actor : MonoBehaviour
    {
        public string actorId;
        public CharacterView view;
        static readonly List<Actor> all = new List<Actor>();

        void OnEnable() { if (!all.Contains(this)) all.Add(this); }
        void OnDisable() { all.Remove(this); }

        public static Actor Find(string id)
        {
            foreach (var a in all) if (a != null && a.actorId == id && a.isActiveAndEnabled) return a;
            return null;
        }

        public Vector3 HeadPos => transform.position + Vector3.up * ((view != null ? view.Height : 1.4f) + (view != null && view.art != null ? view.art.hover : 0) + 0.1f);
    }
}
