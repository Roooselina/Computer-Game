using UnityEngine;
using UnityEngine.UI;

public class CombatHUD : MonoBehaviour {
    public Text grenadeText, dashText;
    public GameObject crosshair;
    PlayerGrenades grenades;
    PlayerDash dash;
    void Start() {
        grenades = FindAnyObjectByType<PlayerGrenades>(); dash = FindAnyObjectByType<PlayerDash>();
    }
    void Update() {
        bool over = GameManager.instance != null && GameManager.instance.isGameover;
        if (crosshair) crosshair.SetActive(!over && Time.timeScale > 0);
        if (grenadeText && grenades) grenadeText.text = "G  GRENADE  " + grenades.count;
        if (dashText && dash) {
            dashText.text = dash.cooldownRemaining > 0 ? "SPACE  " + dash.cooldownRemaining.ToString("0.0") + "s" : "SPACE  DASH READY";
            dashText.color = dash.cooldownRemaining > 0 ? Color.gray : new Color(.4f, 1f, .8f);
        }
    }
}
