using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>
    /// 동료 말풍선 (GDD 7.2).
    /// - 동료와 평온하게 25~45초 걸으면 조건에 맞는 대화 하나.
    /// - 같은 대화는 1회차에 한 번. 출력 이력은 전투 재시도 복원 대상이 아니다.
    /// - 중요한 대화·전투·연출 중에는 미루고, 맵 전환으로 잘리면 다음 안전한 때 이어 준다.
    /// - 특정 동료가 없으면 그 동료의 대사가 나오지 않는다.
    /// </summary>
    public class Bubbles : MonoBehaviour
    {
        Game game;
        readonly List<string> pending = new List<string>();
        readonly List<DLine> resume = new List<DLine>();
        Coroutine playing;
        List<DLine> playingLines; int playingIndex;
        float calmTimer, nextCalm, idleTimer;
        RectTransform bubbleRt; Text bubbleText;
        Transform bubbleTarget;

        public bool IsPlaying => playing != null;

        public void Init(Game g)
        {
            game = g;
            nextCalm = Random.Range(25f, 45f);
            bubbleRt = UIKit.Rect("Bubble", g.UI.worldLayer, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            bubbleRt.anchorMin = bubbleRt.anchorMax = new Vector2(0.5f, 0.5f);
            bubbleRt.pivot = new Vector2(0.5f, 0);
            UIKit.Panel(bubbleRt, "Bg", new Color(0.96f, 0.96f, 0.92f, 0.95f), true);
            bubbleText = UIKit.Label(bubbleRt, "T", "", 11, TextAnchor.MiddleCenter, new Color(0.12f, 0.12f, 0.2f));
            bubbleText.rectTransform.offsetMin = new Vector2(5, 2); bubbleText.rectTransform.offsetMax = new Vector2(-5, -2);
            bubbleRt.gameObject.SetActive(false);
        }

        bool Safe => game.FieldInputAllowed && game.Player != null;

        public void Request(string cond)
        {
            if (string.IsNullOrEmpty(cond)) return;
            if (!pending.Contains(cond)) pending.Add(cond);
        }

        DBubble Find(string cond)
        {
            var gs = game.State;
            foreach (var b in game.Dialogue.bubbles)
            {
                if (b.cond != cond) continue;
                if (gs.D.bubblesSeen.Contains(b.id)) continue;
                if (!string.IsNullOrEmpty(b.map) && b.map != game.CurrentMapId) continue;
                if (b.who == "venelle" && (!gs.VenelleIsAlly || Actor.Find("venelle") == null)) continue;
                if (b.who == "raviel" && (!gs.RavielIsAlly || Actor.Find("raviel") == null)) continue;
                if (!string.IsNullOrEmpty(b.need) && !gs.Has(b.need)) continue;
                return b;
            }
            return null;
        }

        void Update()
        {
            if (game == null || game.State == null) return;
            if (playing != null)
            {
                if (bubbleTarget != null && bubbleRt.gameObject.activeSelf)
                    bubbleRt.anchoredPosition = game.UI.WorldToCanvas(bubbleTarget.position + Vector3.up * 1.75f);
                return;
            }
            if (!Safe) return;
            if (resume.Count > 0)
            {
                var lines = new List<DLine>(resume); resume.Clear();
                playing = StartCoroutine(Play(lines));
                return;
            }
            // 평온한 동행 시간
            var p = game.Player;
            bool moving = Mathf.Abs(p.Velocity.x) > 0.1f;
            if (game.State.VenelleIsAlly || (game.State.RavielIsAlly && game.Companion != null))
            {
                calmTimer += Time.deltaTime;
                idleTimer = moving ? 0 : idleTimer + Time.deltaTime;
                if (calmTimer >= nextCalm) { calmTimer = 0; nextCalm = Random.Range(25f, 45f); Request("walk"); }
                if (idleTimer > 9f) { idleTimer = -999f; Request("idle"); }
                if (game.State.D.lu.hp < game.State.MaxHp("lu") * 0.35f) Request("lu_low");
            }
            for (int i = 0; i < pending.Count; i++)
            {
                var b = Find(pending[i]);
                if (b == null) continue;
                pending.RemoveAt(i);
                game.State.D.bubblesSeen.Add(b.id);
                playing = StartCoroutine(Play(new List<DLine>(b.lines)));
                return;
            }
            // 조건을 못 찾은 요청은 버린다(동료가 없거나 이미 본 대화)
            pending.Clear();
        }

        IEnumerator Play(List<DLine> lines)
        {
            playingLines = lines;
            for (playingIndex = 0; playingIndex < lines.Count; playingIndex++)
            {
                var l = lines[playingIndex];
                var actor = Actor.Find(l.s);
                if (actor == null) continue;
                while (!Safe) yield return null;
                bubbleTarget = actor.transform;
                bubbleText.text = l.t;
                float w = Mathf.Min(220, bubbleText.preferredWidth + 14);
                bubbleRt.sizeDelta = new Vector2(Mathf.Max(40, w), bubbleText.preferredHeight > 14 ? 30 : 17);
                bubbleText.rectTransform.offsetMin = new Vector2(5, 2);
                bubbleRt.sizeDelta = new Vector2(bubbleRt.sizeDelta.x, Mathf.Max(17, Mathf.CeilToInt(PreferredHeight(bubbleRt.sizeDelta.x - 10)) + 6));
                bubbleRt.anchoredPosition = game.UI.WorldToCanvas(actor.transform.position + Vector3.up * 1.75f);
                bubbleRt.gameObject.SetActive(true);
                game.UI.AddLog(game.Dialogue.SpeakerName(l.s), l.t);
                float hold = 1.6f + l.t.Length * 0.07f;
                for (float t = 0; t < hold; t += Time.deltaTime)
                {
                    if (!Safe) { bubbleRt.gameObject.SetActive(false); while (!Safe) yield return null; bubbleRt.gameObject.SetActive(true); }
                    yield return null;
                }
                bubbleRt.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.15f);
            }
            playing = null; playingLines = null;
        }

        float PreferredHeight(float width)
        {
            var settings = bubbleText.GetGenerationSettings(new Vector2(width, 0));
            return bubbleText.cachedTextGeneratorForLayout.GetPreferredHeight(bubbleText.text, settings) / bubbleText.pixelsPerUnit;
        }

        /// <summary>맵이 바뀔 때: 진행 중인 말풍선의 남은 줄을 보관했다가 다음 안전한 때 이어 준다.</summary>
        public void OnMapChange()
        {
            if (playing != null && playingLines != null)
            {
                for (int i = playingIndex; i < playingLines.Count; i++) resume.Add(playingLines[i]);
                StopCoroutine(playing);
                playing = null; playingLines = null;
            }
            bubbleRt.gameObject.SetActive(false);
            calmTimer = 0; idleTimer = 0;
        }

        public void ResetState() { pending.Clear(); resume.Clear(); OnMapChange(); resume.Clear(); }
    }
}
