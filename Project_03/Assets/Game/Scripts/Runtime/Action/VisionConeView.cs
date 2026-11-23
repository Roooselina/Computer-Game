using UnityEngine;

namespace EW
{
    /// <summary>
    /// 시야 범위 표시. 감지와 같은 Sight.Cast 로 벽·엄폐물에 잘린 부채꼴을 그려, 보이는 범위와 실제 판정이 일치한다.
    /// 색: 평소 옅은 청록, 의심 노랑, 발견 빨강. 모양(테두리 점선)과 움직임(맥동)도 상태마다 다르다.
    /// </summary>
    public class VisionConeView : MonoBehaviour
    {
        public int rays = 30;
        Mesh mesh; MeshRenderer mr; Material mat;
        Vector3[] verts; Color[] cols; int[] tris;
        LineRenderer edge;
        public Vector2 Eye; public float DirDeg, HalfAngle, Range;
        public SensorState State;
        public bool Visible = true;
        /// <summary>마지막으로 그린 끝점 (테스트에서 표시와 판정을 비교).</summary>
        public float[] LastLengths;

        public static VisionConeView Create(Transform parent, int order)
        {
            var go = new GameObject("VisionCone");
            go.transform.SetParent(parent, false);
            var v = go.AddComponent<VisionConeView>();
            v.Init(order);
            return v;
        }

        void Init(int order)
        {
            mesh = new Mesh { name = "cone" };
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            mr = gameObject.AddComponent<MeshRenderer>();
            mat = new Material(Shader.Find("Sprites/Default"));
            mr.sharedMaterial = mat; mr.sortingOrder = order;
            verts = new Vector3[rays + 2]; cols = new Color[rays + 2]; tris = new int[rays * 3];
            for (int i = 0; i < rays; i++) { tris[i * 3] = 0; tris[i * 3 + 1] = i + 1; tris[i * 3 + 2] = i + 2; }
            LastLengths = new float[rays + 1];
            var eg = new GameObject("Edge"); eg.transform.SetParent(transform, false);
            edge = eg.AddComponent<LineRenderer>();
            edge.material = mat; edge.useWorldSpace = true; edge.sortingOrder = order + 1;
            edge.startWidth = edge.endWidth = 0.035f; edge.positionCount = rays + 3; edge.loop = false;
        }

        public Color Tint
        {
            get
            {
                switch (State)
                {
                    case SensorState.Alert: return new Color(1f, 0.25f, 0.2f, 0.26f);
                    case SensorState.Checking: case SensorState.Searching: return new Color(1f, 0.7f, 0.2f, 0.22f);
                    case SensorState.Suspicious: return new Color(1f, 0.92f, 0.35f, 0.2f);
                }
                return new Color(0.55f, 0.9f, 1f, 0.13f);
            }
        }

        void LateUpdate()
        {
            transform.position = Vector3.zero; transform.rotation = Quaternion.identity;
            bool on = Visible && State != SensorState.Off && Range > 0.1f;
            mr.enabled = on; edge.enabled = on;
            if (!on) return;
            Color c = Tint;
            if (State == SensorState.Alert || State == SensorState.Checking) c.a *= 0.8f + 0.35f * Mathf.Sin(Time.unscaledTime * 9f);
            Color far = new Color(c.r, c.g, c.b, c.a * 0.35f);
            verts[0] = Eye; cols[0] = c;
            for (int i = 0; i <= rays; i++)
            {
                float a = (DirDeg - HalfAngle + 2f * HalfAngle * i / rays) * Mathf.Deg2Rad;
                Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                float len = Sight.Cast(Eye, d, Range);
                LastLengths[i] = len;
                verts[i + 1] = Eye + d * len;
                cols[i + 1] = far;
                edge.SetPosition(i + 1, verts[i + 1]);
            }
            edge.SetPosition(0, Eye); edge.SetPosition(rays + 2, Eye);
            Color ec = new Color(c.r, c.g, c.b, Mathf.Min(1f, c.a * 2.6f));
            edge.startColor = edge.endColor = ec;
            mesh.Clear();
            mesh.vertices = verts; mesh.colors = cols; mesh.triangles = tris;
            mesh.RecalculateBounds();
        }

        void OnDestroy() { if (mat != null) Destroy(mat); if (mesh != null) Destroy(mesh); }
    }
}
