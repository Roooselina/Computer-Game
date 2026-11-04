using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 메뉴: Tools > Dodge > Setup Shield, Pause Menu, High Score
// 현재 열린 게임 씬에 쉴드/일시정지/최고점수 관련 오브젝트와 UI를 생성하고 참조와 OnClick을 연결한다.
// 이미 같은 이름의 오브젝트가 있으면 새로 만들지 않고 재사용하므로 여러 번 실행해도 안전하다.
public static class DodgeSetupTool
{
    const string ShieldMaterialPath = "Assets/Materials/ShieldMaterial.mat";

    [MenuItem("Tools/Dodge/Setup Shield, Pause Menu, High Score")]
    public static void Setup()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Dodge Setup", "플레이 모드를 종료한 뒤 실행하세요.", "OK");
            return;
        }

        GameManager gameManager = Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
        PlayerController player = Object.FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
        // Main Menu Canvas가 추가된 뒤에도 HUD가 있는 게임 Canvas를 사용
        Canvas canvas = gameManager != null ? FindGameCanvas(gameManager) : null;

        if (gameManager == null || player == null || canvas == null)
        {
            EditorUtility.DisplayDialog("Dodge Setup", "GameManager, PlayerController, Canvas를 현재 씬에서 찾을 수 없습니다.\nSampleScene을 연 상태에서 실행하세요.", "OK");
            return;
        }

        Font font = gameManager.timeText != null ? gameManager.timeText.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // ---------- 1. Shield ----------
        ShieldController shield = player.GetComponent<ShieldController>();
        if (shield == null) shield = Undo.AddComponent<ShieldController>(player.gameObject);

        Transform effectTr = player.transform.Find("Shield Effect");
        if (effectTr == null)
        {
            GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.name = "Shield Effect";
            // 탄알 트리거가 쉴드 구체에 걸려 Player 판정이 깨지지 않도록 콜라이더 제거
            Object.DestroyImmediate(effect.GetComponent<Collider>());
            effect.transform.SetParent(player.transform, false);
            effect.transform.localPosition = Vector3.zero;
            effect.transform.localScale = new Vector3(2.4f, 2.4f, 2.4f);
            MeshRenderer mr = effect.GetComponent<MeshRenderer>();
            mr.sharedMaterial = GetOrCreateShieldMaterial();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            Undo.RegisterCreatedObjectUndo(effect, "Create Shield Effect");
            effectTr = effect.transform;
        }
        effectTr.gameObject.SetActive(false);
        shield.shieldEffect = effectTr.gameObject;
        EditorUtility.SetDirty(shield);

        // ---------- 2. HUD (Shield 상태 / High Score) ----------
        Text shieldText = GetOrCreateText(canvas.transform, "Shield Text", font, 28, TextAnchor.UpperLeft,
            new Vector2(0, 1), new Vector2(20, -20), new Vector2(360, 40), ShieldReadyLabel(shield));
        // 기존에 만들어진 텍스트의 초기 문구도 사용 가능 상태 표기로 갱신 (런타임에는 ShieldUI가 계속 갱신)
        Undo.RecordObject(shieldText, "Shield Text Label");
        shieldText.text = ShieldReadyLabel(shield);
        EditorUtility.SetDirty(shieldText);

        ShieldUI shieldUI = shieldText.GetComponent<ShieldUI>();
        if (shieldUI == null) shieldUI = Undo.AddComponent<ShieldUI>(shieldText.gameObject);
        shieldUI.shieldText = shieldText;
        shieldUI.shieldController = shield;
        shieldUI.gaugeFill = GetOrCreateGauge(canvas.transform, "Shield Gauge", new Vector2(20, -64), new Vector2(200, 8));
        EditorUtility.SetDirty(shieldUI);

        Text highScoreText = GetOrCreateText(canvas.transform, "High Score Text", font, 28, TextAnchor.UpperRight,
            new Vector2(1, 1), new Vector2(-20, -20), new Vector2(360, 40), "HIGH SCORE: 0");
        gameManager.highScoreText = highScoreText;

        // ---------- 3. Game Over 화면: 현재 점수 + 최고 점수 ----------
        if (gameManager.gameoverText != null)
        {
            Text scoreText = GetOrCreateText(gameManager.gameoverText.transform, "Score Text", font, 34, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(400, 40), "SCORE: 0");
            gameManager.scoreText = scoreText;
        }
        if (gameManager.recordText != null)
        {
            // 기존 Record Text 디자인은 유지하고 긴 문구가 잘리지 않도록 가로 넘침만 허용
            Undo.RecordObject(gameManager.recordText, "Record Text Overflow");
            gameManager.recordText.horizontalOverflow = HorizontalWrapMode.Overflow;
            EditorUtility.SetDirty(gameManager.recordText);
        }
        EditorUtility.SetDirty(gameManager);

        // ---------- 4. Pause Manager + Pause Menu ----------
        PauseManager pauseManager = Object.FindAnyObjectByType<PauseManager>(FindObjectsInactive.Include);
        if (pauseManager == null)
        {
            GameObject pmGo = new GameObject("PauseManager");
            Undo.RegisterCreatedObjectUndo(pmGo, "Create PauseManager");
            pauseManager = pmGo.AddComponent<PauseManager>();
        }

        GameObject pausePanel = GetOrCreatePanel(canvas.transform, "Pause Menu", new Color(0f, 0f, 0f, 0.65f));
        GetOrCreateText(pausePanel.transform, "Title", font, 64, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(500, 80), "PAUSED");
        // 이전 버전의 Resume 버튼이 있으면 Continue 버튼으로 이름/문구를 바꿔 재사용
        Transform legacyResume = pausePanel.transform.Find("Resume Button");
        if (legacyResume != null && pausePanel.transform.Find("Continue Button") == null)
        {
            Undo.RecordObject(legacyResume.gameObject, "Rename Resume Button");
            legacyResume.name = "Continue Button";
        }
        Button continueButton = GetOrCreateButton(pausePanel.transform, "Continue Button", "Continue", font, new Vector2(0, 60));
        SetButtonLabel(continueButton, "Continue");
        Button restart = GetOrCreateButton(pausePanel.transform, "Restart Button", "Restart", font, new Vector2(0, 0));
        Button mainMenu = GetOrCreateButton(pausePanel.transform, "Main Menu Button", "Main Menu", font, new Vector2(0, -60));
        Button quit = GetOrCreateButton(pausePanel.transform, "Quit Game Button", "Quit Game", font, new Vector2(0, -120));

        GameObject confirmPanel = GetOrCreatePanel(pausePanel.transform, "Quit Confirm", new Color(0f, 0f, 0f, 0.85f));
        GetOrCreateText(confirmPanel.transform, "Message", font, 40, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(500, 60), "QUIT GAME?");
        Button yes = GetOrCreateButton(confirmPanel.transform, "Yes Button", "Yes", font, new Vector2(-110, -20));
        Button no = GetOrCreateButton(confirmPanel.transform, "No Button", "No", font, new Vector2(110, -20));

        SetListener(continueButton, pauseManager.Continue);
        SetListener(restart, pauseManager.Restart);
        SetListener(mainMenu, pauseManager.LoadMainMenu);
        SetListener(quit, pauseManager.QuitGame);
        SetListener(yes, pauseManager.ConfirmQuit);
        SetListener(no, pauseManager.CancelQuit);

        pauseManager.pauseMenuPanel = pausePanel;
        pauseManager.quitConfirmPanel = confirmPanel;
        EditorUtility.SetDirty(pauseManager);

        // 게임 시작 시 보이지 않도록 비활성화, Pause Menu는 가장 위에 그려지도록 맨 뒤로
        pausePanel.transform.SetAsLastSibling();
        confirmPanel.SetActive(false);
        pausePanel.SetActive(false);

        // ---------- 5. Build Settings에 현재 씬 등록 (Restart/LoadScene에 필요) ----------
        string scenePath = gameManager.gameObject.scene.path;
        if (!string.IsNullOrEmpty(scenePath) && !EditorBuildSettings.scenes.Any(s => s.path == scenePath))
        {
            var list = EditorBuildSettings.scenes.ToList();
            list.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log("[Dodge Setup] Build Settings에 씬 추가: " + scenePath);
        }

        EditorSceneManager.MarkSceneDirty(gameManager.gameObject.scene);
        EditorSceneManager.SaveScene(gameManager.gameObject.scene);
        Debug.Log("[Dodge Setup] 쉴드 / Pause Menu / 최고 점수 설정 완료 및 씬 저장");
    }

    const string HitParticleMaterialPath = "Assets/Materials/HitParticleAdditive.mat";
    const string HitSmokeMaterialPath = "Assets/Materials/HitSmokeAlpha.mat";

    [MenuItem("Tools/Dodge/Setup Hit Effects")]
    public static void SetupHitEffects()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Dodge Setup", "플레이 모드를 종료한 뒤 실행하세요.", "OK");
            return;
        }

        HitEffectManager manager = Object.FindAnyObjectByType<HitEffectManager>(FindObjectsInactive.Include);
        if (manager == null)
        {
            GameObject go = new GameObject("HitEffectManager");
            Undo.RegisterCreatedObjectUndo(go, "Create HitEffectManager");
            manager = go.AddComponent<HitEffectManager>();
        }

        // 빌드에 셰이더가 포함되도록 Additive 파티클 머티리얼을 에셋으로 만들어 지정
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(HitParticleMaterialPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
            AssetDatabase.CreateAsset(mat, HitParticleMaterialPath);
            AssetDatabase.SaveAssets();
        }
        manager.particleMaterial = mat;

        // 연기는 Additive로 표현이 안 되므로 Alpha Blended 머티리얼 에셋 사용
        Material smokeMat = AssetDatabase.LoadAssetAtPath<Material>(HitSmokeMaterialPath);
        if (smokeMat == null)
        {
            smokeMat = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
            AssetDatabase.CreateAsset(smokeMat, HitSmokeMaterialPath);
            AssetDatabase.SaveAssets();
        }
        manager.smokeMaterial = smokeMat;

        // 이전 기본값(약한 흔들림) 그대로라면 강한 흔들림 기본값으로 갱신 (직접 조정한 값은 유지)
        if (Mathf.Approximately(manager.shakeDuration, 0.15f) && Mathf.Approximately(manager.shakeStrength, 0.25f))
        {
            manager.shakeDuration = 0.3f;
            manager.shakeStrength = 0.6f;
        }

        Camera cam = Camera.main;
        if (cam != null) manager.cameraTransform = cam.transform; // 기존 Main Camera 재사용
        EditorUtility.SetDirty(manager);

        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        EditorSceneManager.SaveScene(manager.gameObject.scene);
        Debug.Log("[Dodge Setup] HitEffectManager 설정 완료 및 씬 저장");
    }

    const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity"; // 이전 버전의 별도 메뉴 씬 (Build Settings에서 제외)
    const string MenuCanvasName = "Main Menu Canvas";

    // 메뉴: Tools > Dodge > Setup Main Menu & Records
    // 게임 씬(SampleScene)을 연 상태에서 실행. 게임 씬의 Pause/Shield/Hit FX를 갱신하고
    // 게임 씬 안에 Main Menu(어두운 오버레이 + Game Start / Score Ranking / Exit Game)를 만든다.
    // 실제 게임 화면이 메뉴 배경이 되며, 게임 씬을 Build Settings 0번에 등록한다. 여러 번 실행해도 안전하다.
    [MenuItem("Tools/Dodge/Setup Main Menu & Records")]
    public static void SetupMainMenu()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Dodge Setup", "플레이 모드를 종료한 뒤 실행하세요.", "OK");
            return;
        }

        GameManager gameManager = Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
        if (gameManager == null)
        {
            EditorUtility.DisplayDialog("Dodge Setup", "현재 씬에서 GameManager를 찾을 수 없습니다.\nSampleScene을 연 상태에서 실행하세요.", "OK");
            return;
        }

        // 1. 게임 씬 갱신 (Continue 버튼, SHIELD [Space], 쉴드 게이지, 피격 연출) - 각각 씬을 저장함
        Setup();
        SetupHitEffects();

        Scene gameScene = gameManager.gameObject.scene;
        string gameScenePath = gameScene.path;
        string gameSceneName = gameScene.name;
        Font font = gameManager.timeText != null ? gameManager.timeText.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Canvas hudCanvas = FindGameCanvas(gameManager);
        Vector2 referenceResolution = new Vector2(800, 600);
        CanvasScaler hudScaler = hudCanvas != null ? hudCanvas.GetComponent<CanvasScaler>() : null;
        if (hudScaler != null) referenceResolution = hudScaler.referenceResolution;

        // 2. 게임 씬 안에 메뉴 전용 Canvas 생성 (HUD Canvas와 분리 → 메뉴 중 HUD만 숨길 수 있음)
        MainMenuManager menu = Object.FindAnyObjectByType<MainMenuManager>(FindObjectsInactive.Include);
        if (menu == null)
        {
            GameObject menuGo = new GameObject("MainMenuManager");
            Undo.RegisterCreatedObjectUndo(menuGo, "Create MainMenuManager");
            menu = menuGo.AddComponent<MainMenuManager>();
        }

        GameObject menuRoot = menu.menuRoot;
        if (menuRoot == null) menuRoot = gameScene.GetRootGameObjects().FirstOrDefault(go => go.name == MenuCanvasName);
        if (menuRoot == null)
        {
            menuRoot = new GameObject(MenuCanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(menuRoot, "Create " + MenuCanvasName);
        }
        Canvas menuCanvas = menuRoot.GetComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        menuCanvas.sortingOrder = 10; // HUD(0) 위, 피격 Flash(100) 아래
        // 해상도가 바뀌어도 메뉴 배치가 유지되도록 화면 크기에 맞춰 스케일 (재실행 시에도 적용)
        CanvasScaler scaler = menuRoot.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = menuRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        EditorUtility.SetDirty(scaler);
        if (menuRoot.GetComponent<GraphicRaycaster>() == null) menuRoot.AddComponent<GraphicRaycaster>();

        // 실제 게임 화면 위에 반투명 검은 오버레이 (배경은 보이고 메뉴 글자는 잘 읽히도록 60%)
        // 오버레이가 화면 전체를 덮어 메뉴 중 뒤쪽 클릭도 막음
        GameObject dim = GetOrCreatePanel(menuRoot.transform, "Background Dim", Color.black);
        dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        dim.transform.SetAsFirstSibling();

        // Main 패널: 제목 + Game Start / Score Ranking / Exit Game (동일 크기, 동일 간격)
        GameObject mainPanel = GetOrCreatePanel(menuRoot.transform, "Main Panel", Color.clear);
        mainPanel.GetComponent<Image>().color = Color.clear;
        GetOrCreateText(mainPanel.transform, "Title", font, 80, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(600, 100), "DODGE");
        Button start = GetOrCreateButton(mainPanel.transform, "Game Start Button", "Game Start", font, new Vector2(0, 20));
        Button records = GetOrCreateButton(mainPanel.transform, "Score Ranking Button", "Score Ranking", font, new Vector2(0, -56));
        Button quit = GetOrCreateButton(mainPanel.transform, "Exit Game Button", "Exit Game", font, new Vector2(0, -132));
        GetOrCreateText(mainPanel.transform, "Hint", font, 20, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0, -200), new Vector2(400, 30), "Press Enter to Start");

        // Score Ranking 패널 (RecordStore Top 3) - 오버레이 위에 조금 더 어둡게
        GameObject recordsPanel = GetOrCreatePanel(menuRoot.transform, "Records Panel", Color.black);
        recordsPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);
        Text rankingTitle = GetOrCreateText(recordsPanel.transform, "Title", font, 64, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0, 160), new Vector2(600, 80), "SCORE RANKING");
        rankingTitle.text = "SCORE RANKING";
        Text[] recordTexts = new Text[RecordStore.MaxRecords];
        for (int i = 0; i < recordTexts.Length; i++)
        {
            recordTexts[i] = GetOrCreateText(recordsPanel.transform, "Record " + (i + 1), font, 40, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0, 70 - i * 55), new Vector2(400, 50), (i + 1) + ".  ---");
        }
        Button back = GetOrCreateButton(recordsPanel.transform, "Back Button", "Back", font, new Vector2(0, -132));

        foreach (Button b in new[] { start, records, quit, back }) ApplyMenuButtonStyle(b);
        SetButtonLabel(start, "Game Start");
        SetButtonLabel(records, "Score Ranking");
        SetButtonLabel(quit, "Exit Game");

        SetListener(start, menu.StartGame);
        SetListener(records, menu.ShowRecords);
        SetListener(quit, menu.QuitGame);
        SetListener(back, menu.ShowMainMenu);

        menu.gameSceneName = gameSceneName;
        menu.mainPanel = mainPanel;
        menu.recordsPanel = recordsPanel;
        menu.recordTexts = recordTexts;
        menu.menuRoot = menuRoot;
        menu.hudCanvas = hudCanvas;
        EditorUtility.SetDirty(menu);

        // 에디터에서 보이는 상태: 메뉴 표시 (런타임에는 MainMenuManager가 상태에 맞게 전환)
        menuRoot.SetActive(true);
        mainPanel.SetActive(true);
        recordsPanel.SetActive(false);

        // 3. Pause Menu의 Main Menu 버튼 → 게임 씬을 다시 로드하면 메뉴가 표시됨
        PauseManager pauseManager = Object.FindAnyObjectByType<PauseManager>(FindObjectsInactive.Include);
        if (pauseManager != null)
        {
            pauseManager.mainMenuSceneName = gameSceneName;
            EditorUtility.SetDirty(pauseManager);
        }

        EditorSceneManager.MarkSceneDirty(gameScene);
        EditorSceneManager.SaveScene(gameScene);

        // 4. Build Settings: 게임 씬을 0번(실행 시 첫 씬)으로, 이전 버전의 별도 MainMenu 씬은 제외
        var scenes = EditorBuildSettings.scenes.Where(s => s.path != MainMenuScenePath && s.path != gameScenePath).ToList();
        scenes.Insert(0, new EditorBuildSettingsScene(gameScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();

        Debug.Log("[Dodge Setup] 게임 씬 Main Menu 생성/갱신 및 Build Settings 등록 완료 (" + gameScenePath + ")");
    }

    // HUD가 들어 있는 게임 Canvas (메뉴 Canvas와 구분)
    static Canvas FindGameCanvas(GameManager gameManager)
    {
        Graphic hud = gameManager.timeText != null ? gameManager.timeText : gameManager.recordText;
        if (hud != null)
        {
            Canvas c = hud.GetComponentInParent<Canvas>(true);
            if (c != null) return c.rootCanvas;
        }
        return Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(c => c.isRootCanvas && c.gameObject.name != MenuCanvasName);
    }

    static string ShieldReadyLabel(ShieldController shield)
    {
        return "SHIELD [" + shield.shieldKey + "]";
    }

    // 배경 바 + Filled 게이지 (ShieldUI.gaugeFill)
    static Image GetOrCreateGauge(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Transform t = parent.Find(name);
        if (t == null)
        {
            GameObject bg = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Undo.RegisterCreatedObjectUndo(bg, "Create " + name);
            bg.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)bg.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image bgImage = bg.GetComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.5f);
            bgImage.raycastTarget = false;
            t = bg.transform;
        }

        Transform fillTr = t.Find("Fill");
        if (fillTr == null)
        {
            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Undo.RegisterCreatedObjectUndo(fill, "Create Gauge Fill");
            fill.transform.SetParent(t, false);
            RectTransform rt = (RectTransform)fill.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            fillTr = fill.transform;
        }

        Image fillImage = fillTr.GetComponent<Image>();
        fillImage.sprite = sprite;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;
        fillImage.raycastTarget = false;
        EditorUtility.SetDirty(fillImage);
        return fillImage;
    }

    static void SetButtonLabel(Button button, string label)
    {
        Text text = button.GetComponentInChildren<Text>(true);
        if (text == null || text.text == label) return;
        Undo.RecordObject(text, "Button Label");
        text.text = label;
        EditorUtility.SetDirty(text);
    }

    // Main Menu 버튼 공통 스타일: 크기 통일 + 마우스 오버/클릭 시 색이 눈에 띄게 바뀌도록
    // (Image 색이 어두우면 ColorTint가 곱해져 변화가 안 보이므로 Image는 흰색, 색은 ColorBlock으로 지정)
    static void ApplyMenuButtonStyle(Button button)
    {
        Vector2 size = new Vector2(280, 56);
        RectTransform rt = (RectTransform)button.transform;
        Undo.RecordObject(rt, "Menu Button Style");
        rt.sizeDelta = size;

        Image img = button.GetComponent<Image>();
        if (img != null)
        {
            Undo.RecordObject(img, "Menu Button Style");
            img.color = Color.white;
            EditorUtility.SetDirty(img);
        }

        Undo.RecordObject(button, "Menu Button Style");
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock cb = button.colors;
        cb.normalColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        cb.highlightedColor = new Color(0.25f, 0.45f, 0.6f, 1f);
        cb.pressedColor = new Color(0.12f, 0.3f, 0.45f, 1f);
        cb.selectedColor = cb.normalColor; // 클릭 후 패널을 오가도 버튼이 강조된 채 남지 않도록
        cb.disabledColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        button.colors = cb;
        EditorUtility.SetDirty(button);

        Text text = button.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            Undo.RecordObject(text, "Menu Button Style");
            text.rectTransform.sizeDelta = size;
            text.fontSize = 30;
            EditorUtility.SetDirty(text);
        }
        EditorUtility.SetDirty(rt);
    }

    static Material GetOrCreateShieldMaterial()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(ShieldMaterialPath);
        if (mat != null) return mat;

        // Built-in Standard 셰이더를 Fade(반투명) 모드로 설정
        mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.3f, 0.8f, 1f, 0.3f);
        mat.SetFloat("_Mode", 2f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.1f, 0.4f, 0.6f));
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        AssetDatabase.CreateAsset(mat, ShieldMaterialPath);
        AssetDatabase.SaveAssets();
        return mat;
    }

    static Text GetOrCreateText(Transform parent, string name, Font font, int size, TextAnchor align,
        Vector2 anchor, Vector2 pos, Vector2 sizeDelta, string content)
    {
        Transform t = parent.Find(name);
        if (t != null && t.GetComponent<Text>() != null) return t.GetComponent<Text>();

        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;

        Text text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = align;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        text.text = content;
        return text;
    }

    static GameObject GetOrCreatePanel(Transform parent, string name, Color color)
    {
        Transform t = parent.Find(name);
        if (t != null) return t.gameObject;

        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color; // 패널이 뒤쪽 게임 화면 클릭을 막음
        return go;
    }

    static Button GetOrCreateButton(Transform parent, string name, string label, Font font, Vector2 pos)
    {
        Transform t = parent.Find(name);
        if (t != null && t.GetComponent<Button>() != null) return t.GetComponent<Button>();

        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(220, 48);

        Image img = go.GetComponent<Image>();
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        img.type = Image.Type.Sliced;
        img.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        Button btn = go.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.6f, 0.85f, 1f);
        cb.pressedColor = new Color(0.4f, 0.6f, 0.8f);
        cb.selectedColor = new Color(0.6f, 0.85f, 1f);
        btn.colors = cb;

        Text text = GetOrCreateText(go.transform, "Text", font, 28, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), Vector2.zero, rt.sizeDelta, label);
        Object.DestroyImmediate(text.GetComponent<Outline>());
        return btn;
    }

    // 기존 Persistent 리스너를 지우고 하나만 연결 (Inspector의 OnClick 목록에 표시됨)
    static void SetListener(Button button, UnityAction action)
    {
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(button.onClick, i);
        }
        UnityEventTools.AddPersistentListener(button.onClick, action);
        EditorUtility.SetDirty(button);
    }
}
