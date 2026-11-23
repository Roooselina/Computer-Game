using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 모터 컨베이어 (GDD 4.3). 전원이 들어와 있는 동안 벨트 윗면에 선 루·경비를 정해진 방향으로 민다.
    /// - 전원: 같은 전원선의 Device(motor) 가 켜져 있을 때만 돈다. 정전되면 벨트·위의 상자가 멈춘다 (화면과 판정이 같다).
    /// - 방향: 제어반(BeltControl)의 정상 조작으로 바꾼다. 방향은 저장된다 (var belt_dir_<id>).
    /// - 낙하 없는 밀어내기만 한다. 벽·끝에서는 멈춘다 (Guard.Nudge).
    /// </summary>
    public class Conveyor : MonoBehaviour
    {
        public static readonly List<Conveyor> All = new List<Conveyor>();
        public string id;
        public Device motor;
        public float x0, x1, topY;
        public float speed = 2.2f;
        public int defaultDir = -1;
        /// <summary>0 = 항상, 2 = 베넬이 동행하지 않을 때만 돈다 (동행 경로는 다른 협력 해법을 쓴다 — 베넬 없는 경로에 보이지 않는 도움을 넣지 않기 위한 반대 경우).</summary>
        public int allyMode;
        public SpriteRenderer belt;
        public Sprite[] beltFrames;
        public Transform[] crates;
        float anim;

        void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public bool Running => (motor == null || motor.Operable) && (allyMode != 2 || Game.I == null || Game.I.State == null || !Game.I.State.VenelleIsAlly);
        public int Dir
        {
            get { var s = Game.I != null ? Game.I.State : null; if (s == null) return defaultDir; int v = s.GetVar("belt_dir_" + id); return v == 0 ? defaultDir : v; }
            set { var s = Game.I != null ? Game.I.State : null; if (s != null) s.SetVar("belt_dir_" + id, value >= 0 ? 1 : -1); }
        }

        public static Conveyor Find(string id) { foreach (var c in All) if (c.id == id) return c; return null; }

        bool OnTop(Vector2 feet) => feet.x >= x0 && feet.x <= x1 && Mathf.Abs(feet.y - topY) < 0.2f;

        void Update()
        {
            var g = Game.I;
            if (g == null || g.State == null) return;
            float dt = Hazard.Dt;
            bool run = Running && dt > 0;
            int dir = Dir;
            float v = dir * speed;
            if (run)
            {
                anim += dt * Mathf.Abs(speed) * 4f;
                if (belt != null && beltFrames != null && beltFrames.Length > 0)
                {
                    int k = (int)anim % beltFrames.Length;
                    belt.sprite = beltFrames[dir > 0 ? beltFrames.Length - 1 - k : k];
                }
                if (crates != null)
                    foreach (var c in crates)
                    {
                        if (c == null) continue;
                        var p = c.position; p.x += v * dt;
                        if (p.x > x1 - 0.3f) p.x = x0 + 0.3f; else if (p.x < x0 + 0.3f) p.x = x1 - 0.3f;
                        c.position = p;
                    }
                var pl = g.Player;
                if (pl != null && pl.Grounded && !pl.Climbing && !pl.Hidden && OnTop(pl.transform.position)) pl.Push(v);
                foreach (var gd in Guard.All.ToArray())
                    if (gd != null && !gd.flying && gd.Alive && OnTop(gd.transform.position)) gd.Nudge(v * dt);
            }
        }
    }
}
