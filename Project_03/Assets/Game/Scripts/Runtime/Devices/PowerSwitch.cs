using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>스위치: 연결된 장치를 켜거나 끈다. 가까이 가면 영향을 받는 물건이 깜빡인다.</summary>
    public class PowerSwitch : Interactable
    {
        public List<Device> targets = new List<Device>();
        public bool isOn = true;
        public Sprite onSprite, offSprite;
        SpriteRenderer sr;
        bool near;

        void Reset() { promptText = "스위치"; }
        void Start() { sr = GetComponent<SpriteRenderer>(); SyncFromTargets(); }

        public override void OnStateLoaded(GameState s) { SyncFromTargets(); }

        void SyncFromTargets()
        {
            if (targets.Count > 0 && targets[0] != null) isOn = targets[0].Model.power == DevicePower.On;
            if (sr != null) sr.sprite = isOn ? onSprite : offSprite;
        }

        void Update()
        {
            var g = Game.I;
            bool n = g != null && g.Player != null && Vector2.Distance(g.Player.transform.position, transform.position) < range + 0.6f;
            if (n != near) { near = n; foreach (var d in targets) if (d != null) d.SetHighlight(n); }
            if (sr != null) sr.sprite = isOn ? onSprite : offSprite;
        }

        public override string Prompt(Game g) => isOn ? "스위치 끄기" : "스위치 켜기";

        public override IEnumerator Interact(Game g)
        {
            bool want = !isOn;
            int changed = 0;
            foreach (var d in targets)
                if (d != null && PowerRules.SetSwitch(d.Model, want)) { d.SetPower(d.Model.power); changed++; }
            isOn = want;
            Fx.Spawn("Spark", transform.position + Vector3.up * 0.6f, false, 30, 0.6f);
            var names = new List<string>();
            foreach (var d in targets) if (d != null) names.Add(d.name);
            yield return g.Info((want ? "켰다: " : "껐다: ") + string.Join(", ", names));
        }
    }
}
