using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 캐릭터의 보이는 부분. 로직은 Play("Walk") 같은 상태 이름과 Face(방향)만 호출한다.
    /// 피벗은 발바닥 중앙. 이 오브젝트(View)만 교체하면 그림이 바뀐다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterView : MonoBehaviour
    {
        public CharacterArt art;
        public SpriteRenderer sr;
        public Animator animator;
        string current;
        int facing = 1;
        Coroutine flashCo;

        public static CharacterView Create(Transform parent, CharacterArt art, int sortingOrder = -1)
        {
            var go = new GameObject("View");
            go.transform.SetParent(parent, false);
            var v = go.AddComponent<CharacterView>();
            v.Setup(art, sortingOrder);
            return v;
        }

        public void Setup(CharacterArt a, int sortingOrder = -1)
        {
            art = a;
            sr = GetComponent<SpriteRenderer>();
            if (art == null) return;
            sr.sprite = art.defaultSprite;
            sr.sortingOrder = sortingOrder >= 0 ? sortingOrder : art.sortingOrder;
            transform.localPosition = new Vector3(0, art.hover, 0);
            if (art.controller != null)
            {
                animator = GetComponent<Animator>();
                if (animator == null) animator = gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = art.controller;
            }
            current = null;
            Play("Idle");
        }

        public bool HasState(string state)
        {
            return animator != null && animator.runtimeAnimatorController != null && animator.HasState(0, Animator.StringToHash(state));
        }

        public void Play(string state, bool restart = false)
        {
            if (!restart && current == state) return;
            current = state;
            if (animator == null || animator.runtimeAnimatorController == null) return;
            if (!HasState(state)) state = "Idle";
            animator.Play(state, 0, 0f);
        }

        public string Current => current;

        public void Face(int dir)
        {
            if (dir == 0) return;
            facing = dir > 0 ? 1 : -1;
            if (art == null) { sr.flipX = facing < 0; return; }
            bool wantLeft = facing < 0;
            if (art.leftController != null && animator != null)
            {
                var target = wantLeft ? art.leftController : art.controller;
                if (animator.runtimeAnimatorController != target)
                {
                    animator.runtimeAnimatorController = target;
                    string s = current; current = null; Play(s ?? "Idle");
                }
                sr.flipX = false;
            }
            else
            {
                bool flip = art.flipForLeft && (art.facesRight ? wantLeft : !wantLeft);
                sr.flipX = flip;
            }
        }

        public int Facing => facing;

        public void SetVisible(bool v) { if (sr != null) sr.enabled = v; }

        public void Flash(Color c, float time = 0.15f)
        {
            if (!isActiveAndEnabled) return;
            if (flashCo != null) StopCoroutine(flashCo);
            flashCo = StartCoroutine(FlashCo(c, time));
        }

        IEnumerator FlashCo(Color c, float t)
        {
            sr.color = c;
            yield return new WaitForSeconds(t);
            sr.color = Color.white;
            flashCo = null;
        }

        public float Height => art != null ? art.height : 1.4f;
    }
}
