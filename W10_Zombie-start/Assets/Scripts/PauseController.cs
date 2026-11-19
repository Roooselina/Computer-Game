using UnityEngine;

[DefaultExecutionOrder(-300)]
public class PauseController : MonoBehaviour {
    public bool paused { get; private set; }
    void Start() { SetPaused(false); }
    void Update() {
        bool over = GameManager.instance != null && GameManager.instance.isGameover;
        if (over) { if (paused) { paused = false; Time.timeScale = 1; } SetCursor(false); return; }
        if (Input.GetKeyDown(KeyCode.Escape)) SetPaused(!paused);
    }
    public void SetPaused(bool value) {
        paused = value; Time.timeScale = value ? 0 : 1; SetCursor(!value);
    }
    static void SetCursor(bool locked) {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
    void OnApplicationFocus(bool focus) {
        if (!focus && !paused && !(GameManager.instance != null && GameManager.instance.isGameover)) SetPaused(true);
    }
    void OnDestroy() { Time.timeScale = 1; SetCursor(false); }
    void OnGUI() {
        if (!paused) return;
        var aim = FindAnyObjectByType<PlayerAim>();
        float x = Screen.width / 2 - 180, y = Screen.height / 2 - 170;
        GUI.Box(new Rect(x, y, 360, 340), "PAUSED / MOUSE SETTINGS");
        if (aim) {
            GUI.Label(new Rect(x + 20, y + 45, 320, 25), "Horizontal: " + aim.horizontalSensitivity.ToString("0.0"));
            float horizontal = GUI.HorizontalSlider(new Rect(x + 20, y + 80, 320, 25), aim.horizontalSensitivity, .5f, 12f);
            GUI.Label(new Rect(x + 20, y + 105, 320, 25), "Vertical: " + aim.verticalSensitivity.ToString("0.0"));
            float vertical = GUI.HorizontalSlider(new Rect(x + 20, y + 140, 320, 25), aim.verticalSensitivity, .5f, 12f);
            bool inverted = GUI.Toggle(new Rect(x + 20, y + 175, 320, 25), aim.invertY, "Invert vertical look");
            if (!Mathf.Approximately(horizontal, aim.horizontalSensitivity) || !Mathf.Approximately(vertical, aim.verticalSensitivity) || inverted != aim.invertY)
                aim.SetSensitivity(horizontal, vertical, inverted);
            if (GUI.Button(new Rect(x + 20, y + 210, 320, 35), "Reset sensitivity")) aim.SetSensitivity(5f, 4f, false);
        }
        if (GUI.Button(new Rect(x + 20, y + 275, 320, 45), "Resume (ESC)")) SetPaused(false);
    }
}
