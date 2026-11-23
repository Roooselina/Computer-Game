using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>
    /// 시설 카메라 영상 (7장 영상실·9장 서버실 통신 단말). 같은 씬의 화면 밖 '촬영 구역'(마커 feed_*)을
    /// 별도 카메라로 찍어 모니터 틀 안에 띄운다. 이 카메라는 월드만 찍으므로 플레이어 HUD·메뉴가 영상에 들어가지 않는다.
    /// 화면 아래에는 카메라 번호·장소·시각과 '시설 카메라' 표시만 둔다.
    /// </summary>
    public static class FeedView
    {
        public static bool Open { get; private set; }
        public static string Current { get; private set; }

        public static IEnumerator Show(Game g, string marker, string label, IEnumerator body, float minSeconds = 2.5f, float size = 2.6f)
        {
            var m = Marker.Find(marker);
            if (m == null) { Debug.LogWarning("[EW] 촬영 구역 없음: " + marker); if (body != null) yield return body; yield break; }
            Open = true; Current = marker;
            var rt = new RenderTexture(320, 180, 16) { filterMode = FilterMode.Point };
            var camGo = new GameObject("FeedCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = size;
            cam.transform.position = new Vector3(m.transform.position.x, m.transform.position.y + size * 0.8f, -10);
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.04f, 0.05f, 0.06f);
            cam.targetTexture = rt; cam.depth = -50;
            var canvas = UIKit.MakeCanvas("FeedCanvas", 40, g.transform);
            var dim = UIKit.Solid(canvas.transform, "Dim", new Color(0, 0, 0, 0.88f));
            var frame = UIKit.Rect("Frame", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UIKit.Place(frame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 26), new Vector2(392, 236));
            UIKit.Solid(frame, "Bezel", new Color(0.16f, 0.17f, 0.2f));
            var view = UIKit.Rect("View", frame, Vector2.zero, Vector2.one, new Vector2(8, 22), new Vector2(-8, -8));
            var raw = view.gameObject.AddComponent<RawImage>(); raw.texture = rt; raw.color = new Color(0.82f, 0.95f, 0.88f);
            // 주사선
            for (int i = 0; i < 40; i++)
            {
                var line = UIKit.Rect("Scan" + i, view, new Vector2(0, i / 40f), new Vector2(1, i / 40f), Vector2.zero, new Vector2(0, 1));
                line.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.18f);
            }
            var cap = UIKit.Label(frame, "Caption", label, 10, TextAnchor.LowerLeft, new Color(0.7f, 1f, 0.8f));
            cap.rectTransform.offsetMin = new Vector2(10, 4); cap.rectTransform.offsetMax = new Vector2(-10, -4);
            var rec = UIKit.Label(frame, "Rec", "● 시설 카메라", 10, TextAnchor.LowerRight, new Color(1f, 0.5f, 0.45f));
            rec.rectTransform.offsetMin = new Vector2(10, 4); rec.rectTransform.offsetMax = new Vector2(-10, -4);
            Sfx.Play("sfx_pc_on", 0.6f);
            float t0 = Time.time;
            if (body != null) yield return body;
            while (Time.time - t0 < minSeconds) yield return null;
            Sfx.Play("sfx_power_down", 0.4f, 1.4f);
            Object.Destroy(canvas.gameObject);
            Object.Destroy(camGo);
            rt.Release(); Object.Destroy(rt);
            Open = false; Current = null;
        }

        /// <summary>영상 안 인물을 걸게 한다 (촬영 구역 좌표 기준).</summary>
        public static IEnumerator WalkActor(string actorId, float dx, float speed, string anim = "Walk")
        {
            var a = Actor.Find(actorId);
            if (a == null) yield break;
            yield return Cutscene.Walk(a, a.transform.position.x + dx, speed, anim);
        }
    }
}
