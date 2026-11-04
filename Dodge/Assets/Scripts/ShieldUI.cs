using UnityEngine;
using UnityEngine.UI;

// 쉴드 상태를 텍스트(+선택적으로 게이지)로 표시
// 사용 가능: SHIELD [Space] / 활성화: SHIELD ACTIVE X.Xs / 쿨다운: SHIELD X.Xs
public class ShieldUI : MonoBehaviour
{
    public ShieldController shieldController; // 표시할 플레이어의 쉴드
    public Text shieldText; // 상태를 표시할 텍스트
    public Image gaugeFill; // (선택) Filled 타입 게이지: 쿨다운 중 차오르고, 활성화 중 줄어듦

    public Color readyColor = new Color(0.4f, 0.85f, 1f);
    public Color activeColor = new Color(0.3f, 1f, 0.5f);
    public Color cooldownColor = new Color(1f, 0.6f, 0.3f);

    void Start()
    {
        if (shieldText == null) shieldText = GetComponent<Text>();
        if (shieldController == null) shieldController = FindAnyObjectByType<ShieldController>();
    }

    // ShieldController.Update에서 타이머가 갱신된 뒤 같은 프레임의 값을 표시하도록 LateUpdate 사용
    void LateUpdate()
    {
        if (shieldText == null || shieldController == null) return;

        Color color;
        float fill;

        if (shieldController.IsShieldActive)
        {
            shieldText.text = "SHIELD ACTIVE " + FormatSeconds(shieldController.RemainingDuration);
            color = activeColor;
            fill = Ratio(shieldController.RemainingDuration, shieldController.shieldDuration);
        }
        else if (shieldController.IsReady)
        {
            shieldText.text = "SHIELD [" + shieldController.shieldKey + "]";
            color = readyColor;
            fill = 1f;
        }
        else
        {
            shieldText.text = "SHIELD " + FormatSeconds(shieldController.RemainingCooldown);
            color = cooldownColor;
            fill = 1f - Ratio(shieldController.RemainingCooldown, shieldController.shieldCooldown);
        }

        shieldText.color = color;
        if (gaugeFill != null)
        {
            gaugeFill.fillAmount = fill;
            gaugeFill.color = color;
        }
    }

    // 소수 첫째 자리 올림: 남은 시간이 0보다 크면 0.0s가 표시되지 않고, 0이 되는 순간 [Space]로 바뀜
    static string FormatSeconds(float seconds)
    {
        return (Mathf.Ceil(seconds * 10f) / 10f).ToString("0.0") + "s";
    }

    static float Ratio(float value, float max)
    {
        return max > 0f ? Mathf.Clamp01(value / max) : 0f;
    }
}
