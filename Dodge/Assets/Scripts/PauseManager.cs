using UnityEngine;
using UnityEngine.SceneManagement;

// ESC 일시정지 및 Pause Menu 버튼(Continue / Restart / Main Menu / Quit Game) 처리
public class PauseManager : MonoBehaviour
{
    // 다른 스크립트(플레이어, 쉴드 등)가 일시정지 여부를 확인할 때 사용
    public static bool IsPaused { get; private set; }

    public KeyCode pauseKey = KeyCode.Escape; // 일시정지 토글 키
    public GameObject pauseMenuPanel; // 일시정지 시 표시할 Pause Menu 패널
    public GameObject quitConfirmPanel; // 게임 종료 확인 패널 (없으면 바로 종료)
    public string mainMenuSceneName = "MainMenu"; // 메인 메뉴 씬 이름 (Build Settings에 등록 필요)

    // Domain Reload를 끈 상태로 플레이 모드에 들어가도 static 값이 남지 않도록 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        IsPaused = false;
    }

    void Awake()
    {
        // 씬 시작(재시작/씬 전환 포함) 시 항상 정상 시간으로 시작
        SetPaused(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            // 종료 확인 창이 열려 있으면 ESC는 확인 창만 닫음
            if (quitConfirmPanel != null && quitConfirmPanel.activeSelf)
            {
                CancelQuit();
            }
            else if (IsPaused)
            {
                Continue();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        SetPaused(true);
    }

    // Continue 버튼: Pause Menu를 닫고 이전 상태에서 계속
    public void Continue()
    {
        SetPaused(false);
    }

    public void Restart()
    {
        SetPaused(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName) || !Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogWarning("[PauseManager] Main Menu 씬 '" + mainMenuSceneName +
                "'을(를) 로드할 수 없습니다. 메인 메뉴 씬을 만들고 Build Settings에 추가한 뒤 PauseManager의 Main Menu Scene Name을 설정하세요.");
            return;
        }

        SetPaused(false);
        // 게임 씬 안의 Main Menu를 다시 표시 (Restart/R 재시작은 메뉴 없이 바로 시작)
        MainMenuManager.ShowMenuOnLoad = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Quit Game 버튼: 확인 패널이 있으면 확인 창을 띄우고, 없으면 바로 종료
    public void QuitGame()
    {
        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(true);
        }
        else
        {
            ConfirmQuit();
        }
    }

    public void CancelQuit()
    {
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);
    }

    public void ConfirmQuit()
    {
        // 종료 전에 시간 배율을 정상으로 복구
        SetPaused(false);
        QuitApplication();
    }

    // 게임 종료 (Main Menu의 Quit Game에서도 사용)
    public static void QuitApplication()
    {
#if UNITY_EDITOR
        // 에디터에서는 Application.Quit()이 동작하지 않으므로 플레이 모드를 종료
        Debug.Log("[Quit Game] Editor: 플레이 모드 종료");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(paused);
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);
    }

    void OnDestroy()
    {
        // 일시정지 상태로 씬이 내려가도 다음 씬에 timeScale 0이 남지 않도록 복구
        if (IsPaused)
        {
            IsPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
