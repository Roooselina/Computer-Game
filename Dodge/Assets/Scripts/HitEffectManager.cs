using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 플레이어 피격 연출 (파티클 / 연기 / 충격파 / Hit Flash / 화면 Flash / 카메라 흔들림 / Hit Stop / 사운드)
// 실제 피격은 "쨍그랑!(파편 + 흰 화면 Flash) → 펑!(폭발 + 연기 + 충격파 + 붉은 Flash)" 2단계로 재생한다.
// 미리 만든 Particle System을 재사용하고 Emit()으로 피격 위치에 뿌리므로
// 피격 때마다 GameObject를 Instantiate/Destroy하지 않는다.
public class HitEffectManager : MonoBehaviour
{
    public static HitEffectManager Instance { get; private set; }

    public Material particleMaterial; // Additive 파티클 머티리얼 (빌드에 셰이더가 포함되도록 에셋으로 지정 권장)
    public Material smokeMaterial; // Alpha Blended 연기 머티리얼 (빌드에 셰이더가 포함되도록 에셋으로 지정 권장)
    public Image screenFlashImage; // 화면 전체 Flash 이미지 (비우면 런타임에 자동 생성)
    public Transform cameraTransform; // 흔들 카메라 (비우면 Main Camera)

    // 파티클
    public float hitEffectDuration = 0.35f; // 파편/빛 입자 최대 수명(초)
    public int particleCount = 18; // 피격 1회당 파편 수
    public float particleSpeed = 9f; // 파편 비산 속도
    public float particleSize = 0.15f; // 파편 크기

    // 연기
    public int smokeCount = 6; // 피격 1회당 연기 수
    public float smokeDuration = 0.7f; // 연기 수명(초)
    public Color smokeColor = new Color(0.25f, 0.22f, 0.2f, 0.6f);

    public float explosionDelay = 0.05f; // 쨍그랑(파편) → 펑(폭발) 사이 간격(초, 실시간)

    // 충격파
    public float shockwaveSize = 3f; // 충격파 링 최대 지름
    public float shockwaveDuration = 0.2f; // 충격파 링 지속 시간(초)

    // 플레이어 Hit Flash
    public float flashDuration = 0.08f; // 플레이어가 밝게 깜빡이는 시간(초)
    public Color flashColor = Color.white;

    // 화면 Flash
    public float screenFlashDuration = 0.12f; // 화면 Flash 전체 시간(초)
    public Color screenFlashColor = new Color(1f, 1f, 1f, 0.7f); // 쨍그랑 순간의 흰색 Flash
    public Color damageScreenFlashColor = new Color(1f, 0.1f, 0.05f, 0.45f); // 펑 순간의 붉은 Flash

    // 카메라 흔들림 (실제 피해를 받았을 때만)
    public float shakeDuration = 0.3f;
    public float shakeStrength = 0.6f; // 위치 흔들림 크기
    public float shakeRotation = 2.5f; // 회전 흔들림 크기(도)

    // 사운드 (선택)
    public AudioClip shatterClip; // 쨍그랑 (파편)
    public AudioClip explosionClip; // 펑 (폭발)
    public AudioClip shieldHitClip; // 쉴드로 막았을 때
    [Range(0f, 1f)] public float hitVolume = 1f;

    // Hit Stop
    public float hitStopDuration = 0.06f; // 0이면 사용 안 함
    public bool hitStopOnShieldHit = false; // 쉴드로 막았을 때도 Hit Stop 적용 여부

    // 색상
    public Color damageColor = new Color(1f, 0.35f, 0.1f); // 일반 피격: 빨강/주황
    public Color damageCoreColor = new Color(1f, 0.85f, 0.5f);
    public Color shieldColor = new Color(0.2f, 0.9f, 1f); // 쉴드 피격: 청록
    public Color shieldCoreColor = new Color(0.75f, 1f, 1f);

    private ParticleSystem sparks, glow, core, ring, smoke;
    private Coroutine playerFlashRoutine, shieldFlashRoutine, screenFlashRoutine;
    private AudioSource audioSource;
    private Renderer flashingPlayer, flashingShield;
    private Vector3 shieldBaseScale;
    private bool isHitStopping;

    private float shakeRemaining;
    private Vector3 cameraOrigin;
    private Quaternion cameraOriginRotation;

    private static readonly int ColorId = Shader.PropertyToID("_Color");

    void Awake()
    {
        Instance = this;
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        BuildParticleSystems();
        EnsureScreenFlash();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D 사운드
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void OnDisable()
    {
        // 씬 전환/비활성화 시 연출 상태가 남지 않도록 원상 복구
        if (isHitStopping && !PauseManager.IsPaused) Time.timeScale = 1f;
        isHitStopping = false;
        if (shakeRemaining > 0f) RestoreCamera();
        shakeRemaining = 0f;
        SetScreenFlash(Color.clear);
        ClearFlash(flashingPlayer);
        ClearFlash(flashingShield);
        if (flashingShield != null && shieldBaseScale != Vector3.zero) flashingShield.transform.localScale = shieldBaseScale;
    }

    // ===================== 외부 호출 =====================

    // 실제 피해를 받았을 때
    // 1단계 쨍그랑: 흰 파편 + 흰 화면 Flash + 플레이어 Flash + 카메라 흔들림 + Hit Stop
    // 2단계 펑: 폭발 코어 + 불꽃 + 연기 + 충격파 + 붉은 화면 Flash + 흔들림 재가속
    public void PlayDamageHit(Vector3 hitPoint, Vector3 playerCenter, Renderer playerRenderer)
    {
        Vector3 normal = FlatNormal(hitPoint, playerCenter);

        EmitCore(hitPoint, Color.white, 0.8f);
        EmitSparks(hitPoint, normal, Color.white, Mathf.Max(1, particleCount / 2), 1.4f); // 유리 파편처럼 빠르게 튐
        PlaySound(shatterClip);

        if (playerRenderer != null)
        {
            if (playerFlashRoutine != null) StopCoroutine(playerFlashRoutine);
            playerFlashRoutine = StartCoroutine(FlashRoutine(playerRenderer, flashColor, flashDuration, false));
        }

        if (screenFlashRoutine != null) StopCoroutine(screenFlashRoutine);
        screenFlashRoutine = StartCoroutine(ScreenFlashRoutine());

        StartShake();
        StartHitStop();
        StartCoroutine(ExplosionRoutine(hitPoint, normal));
    }

    IEnumerator ExplosionRoutine(Vector3 hitPoint, Vector3 normal)
    {
        // Hit Stop(timeScale 0) 중에도 진행되도록 실시간 대기
        if (explosionDelay > 0f) yield return new WaitForSecondsRealtime(explosionDelay);

        EmitCore(hitPoint, damageCoreColor, 2.2f);
        EmitSparks(hitPoint, normal, damageColor, particleCount, 1f);
        EmitGlow(hitPoint, normal, damageColor, Mathf.Max(1, particleCount / 2));
        EmitRing(hitPoint, damageColor, shockwaveSize);
        EmitSmoke(hitPoint, smokeCount);
        PlaySound(explosionClip);

        StartShake(); // 폭발 순간 흔들림을 다시 최대로
    }

    // 쉴드가 피해를 완전히 막았을 때 (청록 에너지 충격파 + 쉴드 표면 파동)
    public void PlayShieldHit(Vector3 hitPoint, Vector3 playerCenter, GameObject shieldEffect)
    {
        Vector3 normal = FlatNormal(hitPoint, playerCenter);
        Vector3 surfacePoint = hitPoint;
        Renderer shieldRenderer = shieldEffect != null ? shieldEffect.GetComponent<Renderer>() : null;
        if (shieldRenderer != null)
        {
            // 쉴드 구체 표면 위치에서 충격이 퍼지도록
            surfacePoint = playerCenter + normal * shieldRenderer.bounds.extents.x;
        }

        EmitCore(surfacePoint, shieldCoreColor, 1.2f);
        EmitSparks(surfacePoint, normal, shieldColor, Mathf.Max(1, particleCount * 2 / 3), 0.8f);
        EmitGlow(surfacePoint, normal, shieldColor, Mathf.Max(1, particleCount / 3));
        EmitRing(surfacePoint, shieldColor, shockwaveSize * 0.8f);
        EmitRing(playerCenter, shieldColor, shockwaveSize * 1.3f); // 쉴드 전체로 번지는 큰 링

        if (shieldRenderer != null)
        {
            if (shieldFlashRoutine != null) StopCoroutine(shieldFlashRoutine);
            shieldFlashRoutine = StartCoroutine(FlashRoutine(shieldRenderer, shieldCoreColor, flashDuration * 2.5f, true));
        }

        PlaySound(shieldHitClip);
        if (hitStopOnShieldHit) StartHitStop();
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip, hitVolume);
    }

    // ===================== 파티클 =====================

    void EmitSparks(Vector3 pos, Vector3 normal, Color color, int count, float speedScale)
    {
        if (sparks == null) return;
        var p = new ParticleSystem.EmitParams();
        p.applyShapeToPosition = false;
        for (int i = 0; i < count; i++)
        {
            // 피격면 바깥쪽 반구 방향으로 튀도록
            Vector3 dir = (Random.onUnitSphere + normal * 0.8f).normalized;
            dir.y = Mathf.Abs(dir.y) * 0.6f + 0.1f;
            p.position = pos;
            p.velocity = dir.normalized * particleSpeed * speedScale * Random.Range(0.5f, 1.2f);
            p.startLifetime = hitEffectDuration * Random.Range(0.5f, 1f);
            p.startSize = particleSize * Random.Range(0.6f, 1.3f);
            p.startColor = Color.Lerp(color, Color.white, Random.Range(0f, 0.5f));
            sparks.Emit(p, 1);
        }
    }

    void EmitGlow(Vector3 pos, Vector3 normal, Color color, int count)
    {
        if (glow == null) return;
        var p = new ParticleSystem.EmitParams();
        p.applyShapeToPosition = false;
        for (int i = 0; i < count; i++)
        {
            Vector3 dir = (Random.onUnitSphere + normal * 0.5f).normalized;
            p.position = pos;
            p.velocity = dir * particleSpeed * Random.Range(0.15f, 0.4f);
            p.startLifetime = hitEffectDuration * Random.Range(0.4f, 0.7f);
            p.startSize = particleSize * Random.Range(3f, 5f);
            p.startColor = color;
            glow.Emit(p, 1);
        }
    }

    void EmitCore(Vector3 pos, Color color, float size)
    {
        if (core == null) return;
        var p = new ParticleSystem.EmitParams();
        p.applyShapeToPosition = false;
        p.position = pos;
        p.velocity = Vector3.zero;
        p.startLifetime = Mathf.Max(0.05f, hitEffectDuration * 0.3f);
        p.startSize = size;
        p.startColor = color;
        core.Emit(p, 1);
    }

    void EmitRing(Vector3 pos, Color color, float size)
    {
        if (ring == null) return;
        var p = new ParticleSystem.EmitParams();
        p.applyShapeToPosition = false;
        p.position = pos;
        p.velocity = Vector3.zero;
        p.startLifetime = shockwaveDuration;
        p.startSize = size;
        p.startColor = color;
        ring.Emit(p, 1);
    }

    void EmitSmoke(Vector3 pos, int count)
    {
        if (smoke == null) return;
        var p = new ParticleSystem.EmitParams();
        p.applyShapeToPosition = false;
        for (int i = 0; i < count; i++)
        {
            Vector3 dir = Random.onUnitSphere;
            dir.y = Mathf.Abs(dir.y) + 0.5f; // 위로 피어오름
            p.position = pos + Random.insideUnitSphere * 0.2f;
            p.velocity = dir.normalized * Random.Range(0.6f, 1.6f);
            p.startLifetime = smokeDuration * Random.Range(0.7f, 1f);
            p.startSize = Random.Range(0.6f, 1.1f);
            p.rotation = Random.Range(0f, 360f);
            p.startColor = smokeColor;
            smoke.Emit(p, 1);
        }
    }

    void BuildParticleSystems()
    {
        Material baseMat = particleMaterial;
        if (baseMat == null)
        {
            Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogWarning("[HitEffectManager] 파티클 셰이더를 찾을 수 없어 파티클 효과를 끕니다. Particle Material을 지정하세요.");
                return;
            }
            baseMat = new Material(shader);
        }

        Material dotMat = new Material(baseMat) { mainTexture = MakeRadialTexture(false) };
        Material ringMat = new Material(baseMat) { mainTexture = MakeRadialTexture(true) };

        // 파편: 속도 방향으로 늘어나는 빛나는 조각
        sparks = CreateSystem("Hit Sparks", dotMat, ParticleSystemRenderMode.Stretch, 400, 0.6f);
        var sr = sparks.GetComponent<ParticleSystemRenderer>();
        sr.velocityScale = 0.035f;
        sr.lengthScale = 1.5f;
        SetSizeCurve(sparks, AnimationCurve.Linear(0f, 1f, 1f, 0f));
        var sDrag = sparks.limitVelocityOverLifetime;
        sDrag.enabled = true;
        sDrag.drag = 4f; // 빠르게 튀어나간 뒤 감속

        // 빛 입자: 폭발 주변의 부드러운 글로우 (Bloom 느낌)
        glow = CreateSystem("Hit Glow", dotMat, ParticleSystemRenderMode.Billboard, 200, 0f);
        SetSizeCurve(glow, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f));

        // 코어 플래시: 피격 순간 한 프레임 번쩍
        core = CreateSystem("Hit Core Flash", dotMat, ParticleSystemRenderMode.Billboard, 30, 0f);
        SetSizeCurve(core, new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(0.25f, 1f), new Keyframe(1f, 0.8f)));

        // 충격파 링: 바닥면에 평행하게 빠르게 퍼짐
        ring = CreateSystem("Hit Shockwave", ringMat, ParticleSystemRenderMode.HorizontalBillboard, 40, 0f);
        SetSizeCurve(ring, new AnimationCurve(new Keyframe(0f, 0.15f, 0f, 4f), new Keyframe(1f, 1f, 0f, 0f)));

        // 연기: Additive로는 어두운 연기가 보이지 않으므로 Alpha Blended 머티리얼 사용 (없으면 연기만 생략)
        Material smokeBase = smokeMaterial;
        if (smokeBase == null)
        {
            Shader smokeShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            if (smokeShader != null) smokeBase = new Material(smokeShader);
        }
        if (smokeBase != null)
        {
            Material smokeMat = new Material(smokeBase) { mainTexture = dotMat.mainTexture };
            smoke = CreateSystem("Hit Smoke", smokeMat, ParticleSystemRenderMode.Billboard, 120, -0.05f);
            SetSizeCurve(smoke, AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1.8f)); // 퍼지면서 커짐
            var smokeDrag = smoke.limitVelocityOverLifetime;
            smokeDrag.enabled = true;
            smokeDrag.drag = 2f;
        }
    }

    ParticleSystem CreateSystem(string name, Material mat, ParticleSystemRenderMode mode, int maxParticles, float gravity)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = maxParticles;
        main.startSpeed = 0f;
        main.gravityModifier = gravity;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;

        var emission = ps.emission;
        emission.enabled = false; // Emit()으로만 생성
        var shape = ps.shape;
        shape.enabled = false;

        // 수명 동안 서서히 투명해짐
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.4f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = mode;
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;

        ps.Play();
        return ps;
    }

    static void SetSizeCurve(ParticleSystem ps, AnimationCurve curve)
    {
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, curve);
    }

    // 부드러운 원(dot) 또는 링 텍스처를 코드로 생성 (별도 텍스처 에셋 불필요)
    static Texture2D MakeRadialTexture(bool isRing)
    {
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a;
                if (isRing)
                {
                    float band = (d - 0.82f) / 0.12f;
                    a = Mathf.Exp(-band * band) * (d <= 1f ? 1f : 0f);
                }
                else
                {
                    a = Mathf.Clamp01(1f - d);
                    a *= a;
                }
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return tex;
    }

    // ===================== Hit Flash =====================

    // MaterialPropertyBlock으로 색만 덮어쓰고, 끝나면 블록을 지워 원래 Material 색으로 정확히 복구
    IEnumerator FlashRoutine(Renderer target, Color color, float duration, bool pulseScale)
    {
        if (pulseScale)
        {
            if (flashingShield != target)
            {
                flashingShield = target;
                shieldBaseScale = target.transform.localScale;
            }
        }
        else
        {
            flashingPlayer = target;
        }

        Color original = target.sharedMaterial != null && target.sharedMaterial.HasProperty(ColorId)
            ? target.sharedMaterial.GetColor(ColorId) : Color.white;
        var block = new MaterialPropertyBlock();
        float t = 0f;

        while (t < duration && target != null)
        {
            float k = 1f - t / duration; // 1 -> 0
            Color c = Color.Lerp(original, color, k);
            if (pulseScale) c.a = Mathf.Lerp(original.a, 0.85f, k); // 쉴드 표면이 순간 밝아짐
            block.SetColor(ColorId, c);
            target.SetPropertyBlock(block);
            if (pulseScale) target.transform.localScale = shieldBaseScale * (1f + 0.12f * Mathf.Sin((1f - k) * Mathf.PI));

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        ClearFlash(target);
        if (pulseScale && target != null) target.transform.localScale = shieldBaseScale;
    }

    static void ClearFlash(Renderer target)
    {
        if (target != null) target.SetPropertyBlock(null);
    }

    // ===================== Screen Flash =====================

    // 화면 Flash용 Image가 지정되지 않았으면 최상단 Overlay Canvas와 함께 생성 (클릭을 막지 않음)
    void EnsureScreenFlash()
    {
        if (screenFlashImage == null)
        {
            GameObject canvasGo = new GameObject("Hit Flash Canvas", typeof(Canvas));
            canvasGo.transform.SetParent(transform, false);
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            GameObject imageGo = new GameObject("Hit Flash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageGo.transform.SetParent(canvasGo.transform, false);
            RectTransform rt = (RectTransform)imageGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            screenFlashImage = imageGo.GetComponent<Image>();
        }
        screenFlashImage.raycastTarget = false;
        SetScreenFlash(Color.clear);
    }

    void SetScreenFlash(Color color)
    {
        if (screenFlashImage == null) return;
        screenFlashImage.color = color;
        screenFlashImage.enabled = color.a > 0f;
    }

    // 흰색 → 붉은색 → 투명으로 짧게 전환 (실시간 기준이라 Hit Stop 중에도 진행)
    IEnumerator ScreenFlashRoutine()
    {
        float t = 0f;
        float whiteTime = Mathf.Min(explosionDelay, screenFlashDuration * 0.5f);
        while (t < screenFlashDuration)
        {
            Color c;
            if (t < whiteTime)
            {
                c = screenFlashColor;
            }
            else
            {
                float k = (t - whiteTime) / Mathf.Max(0.0001f, screenFlashDuration - whiteTime); // 0 -> 1
                c = damageScreenFlashColor;
                c.a *= 1f - k;
            }
            SetScreenFlash(c);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        SetScreenFlash(Color.clear);
        screenFlashRoutine = null;
    }

    // ===================== Camera Shake =====================

    void StartShake()
    {
        if (cameraTransform == null || shakeDuration <= 0f || (shakeStrength <= 0f && shakeRotation <= 0f)) return;
        if (shakeRemaining <= 0f)
        {
            // 흔들리는 중이 아닐 때만 원래 위치/회전 저장 (연속 피격에도 원위치가 틀어지지 않음)
            cameraOrigin = cameraTransform.localPosition;
            cameraOriginRotation = cameraTransform.localRotation;
        }
        shakeRemaining = shakeDuration;
    }

    void RestoreCamera()
    {
        if (cameraTransform == null) return;
        cameraTransform.localPosition = cameraOrigin;
        cameraTransform.localRotation = cameraOriginRotation;
    }

    void LateUpdate()
    {
        if (shakeRemaining <= 0f || cameraTransform == null) return;

        if (PauseManager.IsPaused)
        {
            RestoreCamera(); // 일시정지 중에는 흔들림 정지
            return;
        }

        shakeRemaining -= Time.unscaledDeltaTime; // Hit Stop 중에도 흔들리도록 unscaled 사용
        if (shakeRemaining <= 0f)
        {
            shakeRemaining = 0f;
            RestoreCamera();
            return;
        }

        float damper = shakeRemaining / shakeDuration;
        damper *= damper; // 처음에 강하고 빠르게 잦아드는 흔들림
        cameraTransform.localPosition = cameraOrigin + Random.insideUnitSphere * shakeStrength * damper;
        cameraTransform.localRotation = cameraOriginRotation *
            Quaternion.Euler(Random.Range(-1f, 1f) * shakeRotation * damper, 0f, Random.Range(-1f, 1f) * shakeRotation * damper);
    }

    // ===================== Hit Stop =====================

    void StartHitStop()
    {
        // ESC 일시정지 중이거나 이미 Hit Stop 중이면 실행하지 않음
        if (hitStopDuration <= 0f || PauseManager.IsPaused || isHitStopping) return;
        StartCoroutine(HitStopRoutine());
    }

    IEnumerator HitStopRoutine()
    {
        isHitStopping = true;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        // 그 사이에 ESC로 일시정지했다면 PauseManager가 시간을 관리하므로 건드리지 않음
        if (!PauseManager.IsPaused) Time.timeScale = 1f;
        isHitStopping = false;
    }

    static Vector3 FlatNormal(Vector3 hitPoint, Vector3 center)
    {
        Vector3 n = hitPoint - center;
        n.y = 0f;
        return n.sqrMagnitude > 0.0001f ? n.normalized : Vector3.forward;
    }
}
