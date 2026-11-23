using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>
    /// Native 640x360 Point rendering, 32 PPU, integer presentation scale.
    /// The signal distortion and safe camera/texture lifetime handling remain available to cutscenes.
    /// </summary>
    public class PixelScreen : MonoBehaviour
    {
        public const int W = 640, H = 360, PPU = 32;
        public const int RenderWidth = W, RenderHeight = H;
        public static float OrthoSize => H / 2f / PPU;   // 5.625

        public static PixelScreen I { get; private set; }
        RenderTexture rt;
        RawImage img;
        Material signalMaterial;
        public void SetSignalDistortion(float amount) { if(signalMaterial!=null)signalMaterial.SetFloat("_Amount",amount*Mathf.Lerp(0.35f,1,GameSettings.Flash)); }
        RectTransform imgRt;
        Camera clearCam;
        public Rect DisplayRect { get; private set; }
        /// <summary>화면 확대 배율 (창이 640×360 이상이면 정수).</summary>
        public float Scale { get; private set; } = 1f;

        public static PixelScreen Ensure()
        {
            if (I != null) return I;
            var go = new GameObject("[PixelScreen]");
            DontDestroyOnLoad(go);
            I = go.AddComponent<PixelScreen>();
            I.Build();
            return I;
        }

        void Build()
        {
            rt = new RenderTexture(RenderWidth, RenderHeight, 16, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Point, name = "EW_PixelRT" };
            rt.Create();
            var cgo = new GameObject("ScreenCanvas", typeof(RectTransform));
            cgo.transform.SetParent(transform, false);
            var c = cgo.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = -100;
            var igo = new GameObject("Screen", typeof(RectTransform));
            igo.transform.SetParent(cgo.transform, false);
            img = igo.AddComponent<RawImage>();
            img.texture = rt;
            var shader=Resources.Load<Shader>("EW/SignalDistortion");
            if(shader!=null){signalMaterial=new Material(shader);img.material=signalMaterial;}
            img.raycastTarget = false;
            imgRt = (RectTransform)igo.transform;
            imgRt.anchorMin = imgRt.anchorMax = new Vector2(0.5f, 0.5f);
            imgRt.pivot = new Vector2(0.5f, 0.5f);
            var cam = new GameObject("ClearCamera").AddComponent<Camera>();
            cam.transform.SetParent(transform, false);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.cullingMask = 0;
            cam.depth = -100;
            clearCam = cam;
            Fit();
        }

        public void Attach(Camera worldCam)
        {
            worldCam.orthographic = true;
            worldCam.orthographicSize = OrthoSize;
            worldCam.targetTexture = rt;
        }

        void Update() { Fit(); }
        void OnDestroy() { if(rt!=null){
            foreach(var camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include)) if(camera.targetTexture==rt)camera.targetTexture=null;
            if(RenderTexture.active==rt)RenderTexture.active=null;
            rt.Release();Destroy(rt);} if(signalMaterial!=null)Destroy(signalMaterial); if(I==this)I=null; }

        void Fit()
        {
            float s = Mathf.Min((float)Screen.width / W, (float)Screen.height / H);
            if (s >= 1f) s = Mathf.Floor(s); // Native 32 PPU pixels use whole-number display scaling.
            Scale = s;
            imgRt.sizeDelta = new Vector2(W * s, H * s);
            imgRt.anchoredPosition = Vector2.zero;
            DisplayRect = new Rect((Screen.width - W * s) / 2f, (Screen.height - H * s) / 2f, W * s, H * s);
        }

        /// <summary>월드 좌표 → 실제 화면 픽셀 좌표.</summary>
        public static Vector2 WorldToScreen(Vector3 world)
        {
            var cam = Camera.main;
            if (cam == null || I == null) return Vector2.zero;
            Vector3 vp = cam.WorldToViewportPoint(world);
            var r = I.DisplayRect;
            return new Vector2(r.x + vp.x * r.width, r.y + vp.y * r.height);
        }
    }
}
