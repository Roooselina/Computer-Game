using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// CCTV 가 루를 신고했는데 근처에 달려올 경비가 없을 때 오는 지원 드론.
    /// - 맵마다 최대 2대. 같은 신고에 한 대씩, 앞 드론이 사라진 뒤에만 다음이 온다.
    /// - 화면 밖의 출입 지점(ReinforcePoint)에서 나온다. 화면 안 지점밖에 없으면 1.2초 동안 문 표시(경고)를 띄운 뒤 나온다.
    ///   출입 지점이 없는 맵이면 화면 바깥 가장자리에서 들어온다. 화면 안에서 갑자기 생겨 바로 공격하지 않는다.
    /// - 신고 위치로 추적 속도로 와서 확인하고, 루를 보면 추적·공격, 못 보면 수색 후 돌아가 사라진다.
    /// </summary>
    public class Reinforcements : MonoBehaviour
    {
        public const int MaxPerMap = 2;
        static Reinforcements host;
        static string mapKey;
        static int spawnedThisMap;
        readonly List<Guard> active = new List<Guard>();

        public static int SpawnedThisMap => spawnedThisMap;
        public static int ActiveCount => host != null ? host.active.Count : 0;

        public static bool Request(Vector2 pos)
        {
            var g = Game.I;
            if (g == null || g.CurrentMapId == null) return false;
            if (mapKey != g.CurrentMapId + "#" + g.MapLoadCount) { mapKey = g.CurrentMapId + "#" + g.MapLoadCount; spawnedThisMap = 0; if (host != null) host.active.Clear(); }
            if (host == null) host = new GameObject("Reinforcements").AddComponent<Reinforcements>();
            host.active.RemoveAll(x => x == null);
            if (host.active.Count > 0 || spawnedThisMap >= MaxPerMap) return false;
            spawnedThisMap++;
            host.StartCoroutine(host.Spawn(pos));
            return true;
        }

        static bool OnScreen(Vector2 p)
        {
            var cam = Camera.main; if (cam == null) return false;
            Vector3 v = cam.WorldToViewportPoint(p);
            return v.x > -0.02f && v.x < 1.02f && v.y > -0.02f && v.y < 1.02f;
        }

        IEnumerator Spawn(Vector2 pos)
        {
            var g = Game.I;
            Vector2 at; bool inView = false;
            ReinforcePoint best = null; float bd = 1e9f;
            foreach (var rp in ReinforcePoint.All)
            {
                if (rp == null) continue;
                float d = Vector2.Distance(rp.transform.position, pos) + (OnScreen(rp.transform.position) ? 30f : 0f);
                if (d < bd) { bd = d; best = rp; }
            }
            if (best != null) { at = best.transform.position; inView = OnScreen(at); }
            else
            {
                var cam = Camera.main;
                float half = cam != null ? cam.orthographicSize * cam.aspect : 10f;
                float cx = cam != null ? cam.transform.position.x : pos.x;
                float side = g.Player != null && g.Player.transform.position.x > cx ? -1 : 1;
                at = new Vector2(cx + side * (half + 1.5f), pos.y + 2.6f);
            }
            if (inView)
            {
                // 화면 안 출입구: 깜빡이는 경고 표시로 먼저 알린다
                var warn = Fx.Spawn("Spark", at + Vector2.up * 0.6f, false, 40, 1f);
                Sfx.Play("sfx_door_slide", 0.6f, 1.2f);
                for (float t = 0; t < 1.2f; t += Hazard.Dt) yield return null;
                if (warn != null) Destroy(warn);
            }
            else for (float t = 0; t < 0.6f; t += Hazard.Dt) yield return null;
            var go = new GameObject("BackupDrone");
            go.transform.position = new Vector3(at.x, Mathf.Max(at.y, pos.y + 2.4f), 0);
            var gd = go.AddComponent<Guard>();
            gd.enemyId = "capture_drone"; gd.flying = true; gd.guardId = "backup_" + spawnedThisMap;
            gd.patrolMin = pos.x - 2f; gd.patrolMax = pos.x + 2f; gd.startFacing = pos.x > at.x ? 1 : -1;
            gd.endWait = 0.8f; gd.Reinforcement = true;
            active.Add(gd);
            for (int i = 0; i < 30 && gd != null && !gd.Alive; i++) yield return null;   // Guard 초기화(Start·첫 Update) 뒤에 신고
            if (gd == null) yield break;
            gd.Alert(pos, true);
            Sfx.Play("sfx_alert", 0.6f, 0.9f);
            if (g != null) g.UI.Toast("지원 드론이 온다", 1.6f);
        }

        void Update()
        {
            // 수색을 마치고 돌아가는 지원 드론은 화면 밖에서 사라진다
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var gd = active[i];
                if (gd == null) { active.RemoveAt(i); continue; }
                if ((gd.State == Guard.GState.Return || gd.State == Guard.GState.Patrol) && gd.ReturnedFromHunt && !OnScreen(gd.transform.position)) { Destroy(gd.gameObject); active.RemoveAt(i); }
                else if ((gd.State == Guard.GState.Return || gd.State == Guard.GState.Patrol) && gd.ReturnedFromHunt) gd.FlyAway(Hazard.Dt);
            }
        }
    }
}
