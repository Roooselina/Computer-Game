using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>수집 기록 (GDD 10.1). 읽으면 기록 메뉴에 남는다.</summary>
    public class DocPickup : Interactable
    {
        public string docId = "D01";
        public string afterDialogueId;

        void Reset() { promptText = "읽기"; }

        public override IEnumerator Interact(Game g)
        {
            var doc = g.Dialogue.docs.TryGetValue(docId, out var dd) ? dd : null;
            if (doc == null) { Debug.LogError("[EW] 없는 기록 " + docId); yield break; }
            bool isNew = GameState.AddUnique(g.State.D.docs, docId);
            yield return DocView(g, doc);
            if (isNew)
            {
                g.UI.Toast("기록 획득: " + doc.id + " " + doc.title);
                g.State.GainExp("lu", 5);
            }
            if (!string.IsNullOrEmpty(afterDialogueId)) yield return g.Say(afterDialogueId);
        }

        public static IEnumerator DocView(Game g, DDoc doc)
        {
            var pages = doc.body.Split(new[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in pages) yield return g.UI.dialogue.Line("[" + doc.id + "] " + doc.title, p, true);
            g.UI.dialogue.Show(false);
        }
    }
}
