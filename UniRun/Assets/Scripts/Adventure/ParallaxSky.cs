using UnityEngine;

// 구역 색에 맞춰 바뀌는 하늘 그라데이션 + 원근감 있는 배경 실루엣 2겹
public class ParallaxSky : MonoBehaviour {
    public Sprite[] hills;          // 구역 순서대로
    public float[] layerFactor = { 0.15f, 0.35f };
    public float[] layerAlpha = { 0.35f, 0.6f };
    public float[] layerHeight = { 0.1f, -0.25f };   // 화면 높이 비율 기준 위치
    public float blendWidth = 30f;

    private Camera cam;
    private SpriteRenderer sky;
    private Texture2D skyTex;
    private SpriteRenderer[,] layers;   // [층, 0 현재 구역 / 1 다음 구역]

    private void Start() {
        cam = GetComponentInParent<Camera>();
        skyTex = new Texture2D(1, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        sky = NewRenderer("Sky", Sprite.Create(skyTex, new Rect(0, 0, 1, 64), new Vector2(0.5f, 0.5f), 64f), -100);

        layers = new SpriteRenderer[layerFactor.Length, 2];
        for (int l = 0; l < layerFactor.Length; l++) {
            for (int k = 0; k < 2; k++) {
                SpriteRenderer sr = NewRenderer("Hills " + l + "_" + k, hills[0], -90 + l * 2 + k);
                sr.drawMode = SpriteDrawMode.Tiled;
                layers[l, k] = sr;
            }
        }
    }

    private SpriteRenderer NewRenderer(string objName, Sprite s, int order) {
        var go = new GameObject(objName);
        go.transform.SetParent(transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.sortingOrder = order;
        return sr;
    }

    private void LateUpdate() {
        MapLoader map = MapLoader.instance;
        float camX = cam.transform.position.x;
        float h = cam.orthographicSize * 2f, w = h * cam.aspect;
        transform.rotation = Quaternion.identity;

        // 현재 구역과 다음 구역 사이 섞는 비율
        int zi = map.ZoneIndexAt(camX);
        ZoneData z = map.Data.zones[zi];
        int ni = Mathf.Min(zi + 1, map.Data.zones.Length - 1);
        float blend = zi == ni ? 0f : Mathf.Clamp01((camX - (z.endX - blendWidth * 0.5f)) / blendWidth);
        if (zi > 0 && camX < z.startX + blendWidth * 0.5f) {
            ni = zi - 1;
            blend = Mathf.Clamp01((z.startX + blendWidth * 0.5f - camX) / blendWidth);
        }
        ZoneData nz = map.Data.zones[ni];

        // 하늘: 위쪽 색 → 아래쪽 색, 고도가 높아질수록 위쪽 색이 진해진다
        Color top = Color.Lerp(C(z.skyTop), C(nz.skyTop), blend);
        Color bottom = Color.Lerp(C(z.skyBottom), C(nz.skyBottom), blend);
        float altitude = Mathf.InverseLerp(10f, map.Data.height, cam.transform.position.y);
        top = Color.Lerp(top, top * 0.75f, altitude);
        var px = new Color[64];
        for (int i = 0; i < 64; i++) px[i] = Color.Lerp(bottom, top, i / 63f);
        skyTex.SetPixels(px);
        skyTex.Apply();
        sky.transform.localPosition = new Vector3(0, 0, 20f);
        sky.transform.localScale = new Vector3(w * 64f * 1.2f, h * 1.2f, 1f);

        for (int l = 0; l < layerFactor.Length; l++) {
            for (int k = 0; k < 2; k++) {
                SpriteRenderer sr = layers[l, k];
                int zoneIdx = k == 0 ? zi : ni;
                sr.sprite = hills[Mathf.Min(zoneIdx, hills.Length - 1)];
                float tileW = sr.sprite.bounds.size.x;
                float scale = h / sr.sprite.bounds.size.y * 0.9f;
                sr.size = new Vector2(w / scale + tileW * 2f, sr.sprite.bounds.size.y);
                float offset = Mathf.Repeat(camX * layerFactor[l] / scale, tileW);
                sr.transform.localScale = new Vector3(scale, scale, 1f);
                sr.transform.localPosition = new Vector3(-offset * scale, layerHeight[l] * h, 10f - l);
                float a = layerAlpha[l] * (k == 0 ? 1f - blend : blend);
                sr.color = new Color(1f, 1f, 1f, a);
            }
        }
    }

    private static Color C(int[] rgb) { return new Color(rgb[0] / 255f, rgb[1] / 255f, rgb[2] / 255f); }
}
