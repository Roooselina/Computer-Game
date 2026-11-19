using UnityEngine;

[DefaultExecutionOrder(-200)]
public class PlayerInput : MonoBehaviour {
    public string moveAxisName = "Vertical", rotateAxisName = "Horizontal";
    public string fireButtonName = "Fire1", reloadButtonName = "Reload";
    public float move { get; private set; }
    public float rotate { get; private set; }
    public Vector2 movement { get; private set; }
    public bool fire { get; private set; }
    public bool reload { get; private set; }
    public bool grenade { get; private set; }
    public bool dash { get; private set; }
    void Update() {
        bool blocked = Time.timeScale == 0 || Cursor.lockState != CursorLockMode.Locked ||
            (GameManager.instance != null && GameManager.instance.isGameover);
        move = blocked ? 0 : Input.GetAxis(moveAxisName);
        rotate = blocked ? 0 : Input.GetAxis(rotateAxisName);
        movement = Vector2.ClampMagnitude(new Vector2(rotate, move), 1f);
        fire = !blocked && Input.GetButton(fireButtonName);
        reload = !blocked && Input.GetButtonDown(reloadButtonName);
        grenade = !blocked && Input.GetKeyDown(KeyCode.G);
        dash = !blocked && Input.GetKeyDown(KeyCode.Space);
    }
}
