using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(150)]
public class PlayerAim : MonoBehaviour {
    public Camera viewCamera;
    public Transform cameraTarget;
    [FormerlySerializedAs("sensitivity"), Range(.5f, 12f)]
    public float horizontalSensitivity = 5f;
    [Range(.5f, 12f)] public float verticalSensitivity = 4f;
    public bool invertY;
    public float range = 50f;
    public LayerMask hitMask = ~0;
    public Vector3 aimPoint { get; private set; }
    public float yawAngle => yaw;
    public float pitchAngle => pitch;
    float yaw, pitch = 15f;
    const string HorizontalKey = "ZombieSurvivor.Mouse.Horizontal";
    const string VerticalKey = "ZombieSurvivor.Mouse.Vertical";
    const string InvertKey = "ZombieSurvivor.Mouse.InvertY";

    void Awake() {
        if (!viewCamera) viewCamera = Camera.main;
        SetSensitivity(PlayerPrefs.GetFloat(HorizontalKey, horizontalSensitivity),
            PlayerPrefs.GetFloat(VerticalKey, verticalSensitivity), PlayerPrefs.GetInt(InvertKey, invertY ? 1 : 0) == 1, false);
        yaw = transform.eulerAngles.y;
        aimPoint = transform.position + transform.forward * range;
        if (cameraTarget) cameraTarget.SetParent(null, true);
    }
    void OnDestroy() { if (cameraTarget) Destroy(cameraTarget.gameObject); }
    public void SetSensitivity(float horizontal, float vertical, bool inverted, bool save = true) {
        horizontalSensitivity = Mathf.Clamp(horizontal, .5f, 12f);
        verticalSensitivity = Mathf.Clamp(vertical, .5f, 12f);
        invertY = inverted;
        if (!save) return;
        PlayerPrefs.SetFloat(HorizontalKey, horizontalSensitivity);
        PlayerPrefs.SetFloat(VerticalKey, verticalSensitivity);
        PlayerPrefs.SetInt(InvertKey, invertY ? 1 : 0);
        PlayerPrefs.Save();
    }
    // Legacy mouse axes already represent this frame's delta: do not multiply by deltaTime.
    public void ApplyLookDelta(Vector2 delta) {
        yaw = Mathf.Repeat(yaw + delta.x * horizontalSensitivity, 360f);
        pitch = Mathf.Clamp(pitch + delta.y * verticalSensitivity * (invertY ? 1f : -1f), -35f, 65f);
    }
    void Update() {
        if (!viewCamera || !cameraTarget) return;
        if (Time.timeScale > 0 && Cursor.lockState == CursorLockMode.Locked &&
            !(GameManager.instance != null && GameManager.instance.isGameover))
            ApplyLookDelta(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")));
        cameraTarget.SetPositionAndRotation(transform.position + Vector3.up * 1.4f, Quaternion.Euler(pitch, yaw, 0));
    }
    // Cinemachine's LateUpdate runs first. The shooter (order 200) then uses this exact screen-center ray.
    void LateUpdate() { RefreshAimPoint(); }
    public void RefreshAimPoint() {
        if (!viewCamera) return;
        Ray ray = viewCamera.ViewportPointToRay(new Vector3(.5f, .5f));
        aimPoint = Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore)
            ? hit.point : ray.GetPoint(range);
    }
}
