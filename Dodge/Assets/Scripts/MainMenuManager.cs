using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Main Menu(Game Start / Score Ranking / Exit Game)와 Score Ranking(Top 3) 화면 처리
// 버튼 OnClick은 Tools > Dodge > Setup Main Menu & Records 메뉴에서 연결한다.
//
// 게임 씬 안에 배치하면 실제 게임 화면을 메뉴 배경으로 사용한다.
// 메뉴 중: 게임플레이 스크립트(GameManager, PlayerController, ShieldController, BulletSpawner, PauseManager)와 HUD를 꺼 둠
// Game Start: 스크립트를 켜면 미뤄 둔 Start()가 실행되어 기존 초기화가 그대로 진행됨
public class MainMenuManager : MonoBehaviour
{
    // 씬이 로드될 때 메뉴를 보여줄지 여부 (Restart/R 재시작 시에는 메뉴 없이 바로 시작)
    public static bool ShowMenuOnLoad = true;

    public string gameSceneName = "SampleScene"; // 별도 메뉴 씬에서 사용할 때 로드할 게임 씬 (Build Settings 등록 필요)
    public GameObject mainPanel; // Game Start / Score Ranking / Exit Game 버튼 패널
    public GameObject recordsPanel; // Top 3 기록 패널
    public Text[] recordTexts; // 1~3위 기록 텍스트 (위에서부터 순서대로)
    public string emptyRecordText = "---"; // 기록이 없는 순위에 표시할 문자열
    public KeyCode backKey = KeyCode.Escape; // Records 화면에서 Main Menu로 돌아가는 키

    // 게임 씬 안에 배치한 경우에 사용
    public GameObject menuRoot; // 메뉴 전체(어두운 오버레이 + 패널). 게임 시작 시 비활성화
    public Canvas hudCanvas; // 게임 HUD Canvas (Time / Score / Shield 등). 메뉴 중에는 숨김
    public KeyCode startKey = KeyCode.Return; // Game Start 단축키 (Keypad Enter도 동일하게 처리)

    private readonly List<Behaviour> gameplayBehaviours = new List<Behaviour>();
    private bool isInGameScene; // 이 메뉴가 게임 씬 안에 있는지 (GameManager 존재 여부)
    private bool gameStarted; // 게임 시작이 한 번만 실행되도록 막는 플래그

    public bool IsMenuOpen { get { return isInGameScene && !gameStarted; } }

    // Domain Reload를 끈 상태로 플레이 모드에 들어가도 첫 실행 시 메뉴가 보이도록 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        ShowMenuOnLoad = true;
    }

    void Awake()
    {
        // 일시정지 상태에서 넘어오더라도 정상 시간/오디오로 시작
        Time.timeScale = 1f;
        AudioListener.pause = false;

        GameManager gameManager = FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
        isInGameScene = gameManager != null;

        if (!isInGameScene)
        {
            // 별도 메뉴 씬에 배치된 경우
            ShowMainMenu();
            return;
        }

        if (hudCanvas == null && gameManager.timeText != null)
        {
            Canvas timeCanvas = gameManager.timeText.GetComponentInParent<Canvas>(true);
            if (timeCanvas != null) hudCanvas = timeCanvas.rootCanvas;
        }
        CollectGameplayBehaviours();

        if (!ShowMenuOnLoad)
        {
            // Restart/R 재시작: 메뉴 없이 바로 게임 진행
            BeginGame();
            return;
        }

        // Awake는 씬의 모든 Start()보다 먼저 실행되므로, 여기서 꺼 두면 게임플레이 Start()는 Game Start 시점까지 미뤄짐
        SetGameplayEnabled(false);
        SetHudVisible(false);
        if (menuRoot != null) menuRoot.SetActive(true);
        ShowMainMenu();
    }

    void Update()
    {
        if (gameStarted) return;

        if (recordsPanel != null && recordsPanel.activeSelf)
        {
            if (Input.GetKeyDown(backKey)) ShowMainMenu();
        }
        else if (Input.GetKeyDown(startKey) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            StartGame();
        }
    }

    // Game Start 버튼과 Enter 키가 공통으로 호출
    public void StartGame()
    {
        if (gameStarted) return; // 버튼 연타/Enter 연타 시 중복 실행 방지

        if (isInGameScene)
        {
            ShowMenuOnLoad = false; // 이후 Restart/R 재시작은 메뉴 없이 바로 시작
            BeginGame();
            return;
        }

        if (string.IsNullOrEmpty(gameSceneName) || !Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogWarning("[MainMenuManager] 게임 씬 '" + gameSceneName +
                "'을(를) 로드할 수 없습니다. Build Settings에 게임 씬을 추가하고 Game Scene Name을 설정하세요.");
            return;
        }
        gameStarted = true;
        ShowMenuOnLoad = false; // 게임 씬에 메뉴가 있어도 다시 띄우지 않음
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowRecords()
    {
        if (gameStarted) return;
        RefreshRecords();
        ClearSelection();
        if (mainPanel != null) mainPanel.SetActive(false);
        if (recordsPanel != null) recordsPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        ClearSelection();
        if (recordsPanel != null) recordsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void QuitGame()
    {
        PauseManager.QuitApplication();
    }

    // 메뉴 → 게임 상태 전환: 메뉴/오버레이 숨김, 게임플레이 스크립트 활성화(기존 Start() 실행), HUD 표시
    void BeginGame()
    {
        gameStarted = true;
        ClearSelection();
        if (menuRoot != null) menuRoot.SetActive(false);
        SetGameplayEnabled(true);
        SetHudVisible(true);
    }

    void CollectGameplayBehaviours()
    {
        gameplayBehaviours.Clear();
        gameplayBehaviours.AddRange(FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        gameplayBehaviours.AddRange(FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        gameplayBehaviours.AddRange(FindObjectsByType<ShieldController>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        gameplayBehaviours.AddRange(FindObjectsByType<BulletSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        gameplayBehaviours.AddRange(FindObjectsByType<PauseManager>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }

    void SetGameplayEnabled(bool enabled)
    {
        foreach (Behaviour b in gameplayBehaviours)
        {
            if (b != null) b.enabled = enabled;
        }
    }

    void SetHudVisible(bool visible)
    {
        if (hudCanvas != null) hudCanvas.enabled = visible;
    }

    // 클릭으로 선택된 버튼이 남아 있으면 Enter(Submit)가 그 버튼까지 눌러 버리므로 선택 해제
    static void ClearSelection()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    void RefreshRecords()
    {
        if (recordTexts == null) return;

        var records = RecordStore.Load();
        for (int i = 0; i < recordTexts.Length; i++)
        {
            if (recordTexts[i] == null) continue;
            string value = i < records.Count ? RecordStore.Format(records[i]) : emptyRecordText;
            recordTexts[i].text = (i + 1) + ".  " + value;
        }
    }
}
