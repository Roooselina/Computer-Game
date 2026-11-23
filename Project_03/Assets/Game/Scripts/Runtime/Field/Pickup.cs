using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>줍는 물건: 과자·회복 붕대. 한 번 주우면 저장되어 다시 나타나지 않는다.</summary>
    public class Pickup : Interactable
    {
        public string itemId = "snack";
        public int count = 1;

        void Reset() { promptText = "줍기"; }

        public override void OnStateLoaded(GameState s) { if (s.D.pickedUp.Contains(id)) Destroy(gameObject); }

        public override IEnumerator Interact(Game g)
        {
            if (!GameState.AddUnique(g.State.D.pickedUp, id)) { Destroy(gameObject); yield break; }
            g.State.AddItem(itemId, count);
            var def = g.Db.GetItem(itemId);
            g.UI.Toast((def != null ? def.name : itemId) + " 을(를) 얻었다." + (count > 1 ? " ×" + count : ""));
            if (itemId == "snack") g.Bubbles.Request("snack");
            Fx.Spawn("Heal", transform.position + Vector3.up * 0.3f);
            Destroy(gameObject);
            yield break;
        }
    }
}
