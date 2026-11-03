using UnityEngine;

// 플레이어를 따라가는 카메라.
// 세로 위치는 "플레이어 + 앞으로 펼쳐질 지형"이 함께 보이도록 잡는다.
//  - 점프 중에는 마지막으로 서 있던 높이를 기준으로 삼아서 점프할 때마다 들썩이지 않는다.
//  - 오르막을 오를 때는 앞쪽의 낮은 곳까지, 높은 곳에서 뛰어내리기 전에는 아래 착지 지점까지 보이게 한다.
//  - 앞쪽 높낮이 차가 크면 그만큼 줌아웃한다.
public class RunCamera : MonoBehaviour {
    public static RunCamera instance;

    public float lookAhead = 6f;
    public float scanFrom = 1f;          // 앞쪽 지형을 훑는 범위 (플레이어 기준 칸)
    public float scanTo = 20f;
    public float baseSize = 7.5f;
    public float maxExtraSize = 4f;      // 높낮이 차가 클 때 최대 줌아웃 양
    public float topMargin = 3.2f;       // 플레이어 머리 위로 최소 이만큼은 화면이 남게
    public float bottomMargin = 2.2f;    // 플레이어 발 아래로 최소 이만큼은 화면이 남게
    public float maxShake = 0.6f;

    private Camera cam;
    private float camY;
    private float yVelocity;
    private float trauma;
    private float punch;
    private float size;
    private float sizeVelocity;
    private float flyBlend;          // 1 = 오프닝 카메라, 0 = 플레이 카메라
    private float flyBlendSpeed;
    private bool flying;
    private float flyX;
    private float flyY;
    private float flyYVelocity;

    private void Awake() {
        instance = this;
        cam = GetComponent<Camera>();
        size = baseSize;
    }

    private void Start() {
        Snap();
    }

    public void Snap() {
        float target, targetSize;
        Frame(out target, out targetSize);
        camY = target;
        size = targetSize;
        yVelocity = sizeVelocity = 0f;
        Transform p = RunnerPlayer.instance.transform;
        transform.position = new Vector3(p.position.x + lookAhead, camY, -10f);
        cam.orthographicSize = size;
    }

    // 오프닝: 맵 위를 천천히 날아가며 보여 주는 카메라
    public void BeginFlyover() {
        flying = true;
        flyBlend = 1f;
        flyX = RunnerPlayer.instance.transform.position.x + 10f;
        flyY = FlyTargetY(flyX);
    }

    public void EndFlyover(float seconds) {
        flying = false;
        flyBlendSpeed = 1f / Mathf.Max(0.1f, seconds);
    }

    private float FlyTargetY(float x) {
        MapLoader map = MapLoader.instance;
        float sum = 0f; int n = 0;
        for (int dx = -12; dx <= 12; dx += 2) {
            int t = map.ColumnTop(x + dx);
            sum += t < 0 ? map.FluidSurface(x + dx) : t; n++;
        }
        return sum / n + 4f;
    }

    public void Shake(float amount) { trauma = Mathf.Clamp01(trauma + amount); }

    // 양수 = 확대, 음수 = 축소 (순간적인 줌 펀치)
    public void Punch(float amount) { punch = amount; }

    // 플레이어와 앞쪽 지형이 함께 들어오는 화면 중심/크기 계산
    private void Frame(out float centerY, out float halfHeight) {
        RunnerPlayer p = RunnerPlayer.instance;
        MapLoader map = MapLoader.instance;
        float px = p.transform.position.x;
        float feet = p.transform.position.y + p.FeetOffset;
        float head = p.transform.position.y - p.FeetOffset * 0.9f;

        // 공중에서는 마지막으로 딛은 높이를 기준으로 (떨어져서 그보다 낮아지면 실제 위치)
        float refFeet = p.Grounded || p.Submerged ? feet : Mathf.Min(p.GroundY, feet + 1.5f);
        float lo = refFeet, hi = refFeet + (head - feet);

        bool anyLand = false;
        for (float dx = scanFrom; dx <= scanTo; dx += 1f) {
            int top = map.ColumnTop(px + dx);
            if (top < 0) continue;
            anyLand = true;
            float w = dx < scanTo * 0.6f ? 1f : 0.6f;              // 먼 곳은 덜 반영
            lo = Mathf.Min(lo, Mathf.Lerp(refFeet, top, w));
            hi = Mathf.Max(hi, Mathf.Lerp(refFeet, top + 1.5f, w));
        }
        if (!anyLand) lo = Mathf.Min(lo, map.FluidSurface(px + scanTo * 0.5f));   // 앞이 전부 틈이면 수면까지

        halfHeight = Mathf.Clamp((hi - lo) * 0.5f + 2.6f, baseSize, baseSize + maxExtraSize);
        centerY = (lo + hi) * 0.5f + 0.4f;

        // 플레이어 자신은 반드시 화면 안 여유 있는 곳에
        float pTop = Mathf.Max(head, p.transform.position.y - p.FeetOffset);
        centerY = Mathf.Min(centerY, feet + halfHeight - bottomMargin);
        centerY = Mathf.Max(centerY, pTop - halfHeight + topMargin);

        float minY = halfHeight - 0.5f;
        float maxY = map.Data.height - halfHeight;
        centerY = Mathf.Clamp(centerY, minY, Mathf.Max(minY, maxY));
    }

    private void LateUpdate() {
        RunnerPlayer p = RunnerPlayer.instance;
        Vector3 pp = p.transform.position;
        float dt = Time.unscaledDeltaTime;

        float targetY, targetSize;
        Frame(out targetY, out targetSize);

        // 플레이어가 화면 가장자리에 가까우면 빨리, 평소에는 부드럽게
        float feet = pp.y + p.FeetOffset;
        bool nearEdge = feet < camY - size + bottomMargin * 0.7f || pp.y > camY + size - topMargin * 0.7f;
        camY = Mathf.SmoothDamp(camY, targetY, ref yVelocity, nearEdge ? 0.1f : 0.4f, Mathf.Infinity, dt);

        // 속도가 빨라질수록 조금 멀리 보이게 + 줌 펀치
        float speedZoom = Mathf.InverseLerp(p.baseSpeed, p.maxSpeed, p.Speed) * 1f;
        punch = Mathf.Lerp(punch, 0f, dt * 4f);
        size = Mathf.SmoothDamp(size, targetSize + speedZoom, ref sizeVelocity, 0.6f, Mathf.Infinity, dt);
        cam.orthographicSize = size - punch;

        // 트라우마 기반 흔들림 (Perlin)
        trauma = Mathf.MoveTowards(trauma, 0f, dt * 1.4f);
        float s = trauma * trauma;
        float t = Time.unscaledTime * 25f;
        Vector3 shake = new Vector3((Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f, (Mathf.PerlinNoise(0f, t) - 0.5f) * 2f, 0f) * maxShake * s;
        float angle = (Mathf.PerlinNoise(t, t) - 0.5f) * 2f * 3f * s;

        Vector3 playPos = new Vector3(pp.x + lookAhead, camY, -10f);
        if (flying) {
            // 맵 앞쪽 300칸을 오가며 천천히 비행, 끝에 닿으면 처음으로
            flyX += dt * 5f;
            if (flyX > 320f) flyX = pp.x + 10f;
            flyY = Mathf.SmoothDamp(flyY, FlyTargetY(flyX), ref flyYVelocity, 1.2f, Mathf.Infinity, dt);
        } else {
            flyBlend = Mathf.MoveTowards(flyBlend, 0f, dt * flyBlendSpeed);
        }
        float k = flyBlend * flyBlend * (3f - 2f * flyBlend);   // smoothstep
        Vector3 flyPos = new Vector3(flyX, flyY, -10f);
        transform.position = Vector3.Lerp(playPos, flyPos, k) + shake;
        cam.orthographicSize = Mathf.Lerp(size - punch, baseSize + 2.5f + Mathf.Sin(Time.unscaledTime * 0.3f) * 0.5f, k);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
