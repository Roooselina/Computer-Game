using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

// Editor-only verification. Never rebuilds the scene or changes gameplay assets.
[InitializeOnLoad]
public static class W10Validation
{
    const string PhaseKey = "UniRun.W10.ValidationPhase";
    const string Output = "W10-Implementation/Validation";
    static PlayerController player;
    static Rigidbody2D body;
    static PlatformSpawner spawner;
    static GameObject[] pool;
    static EntityId[] poolIds;
    static int stage, placements;
    static float stageTime, previousSpawn = -1;
    static double deadline;
    static bool initialized, failed;
    static Vector3[] stoppedPositions;
    static ScrollingObject[] scrolling;
    static GameManager oldManager;
    static Mouse mouse;

    static W10Validation()
    {
        EditorApplication.delayCall += Resume;
    }

    static void Resume()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += Resume;
            return;
        }
        string phase = SessionState.GetString(PhaseKey, "");
        if (phase == "play")
        {
            deadline = EditorApplication.timeSinceStartup + 60;
            Application.logMessageReceived += OnLog;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnState;
        }
        else if (phase == "finish")
        {
            SessionState.EraseString(PhaseKey);
            if (SessionState.GetBool(PhaseKey + ".passed", false)) BuildWindows();
        }
    }

    static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name,
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(owner);

    static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception("W10 validation failed: " + description);
        Directory.CreateDirectory(Output);
        File.AppendAllText(Output + "/result.txt", "PASS: " + description + "\n");
    }

    [MenuItem("W10/Validate Saved Main Setup")]
    public static void Validate()
    {
        var scene = EditorSceneManager.GetActiveScene();
        Check(scene.path == "Assets/Scenes/Main.unity", "Main scene is active");
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0,
                    "No missing scripts: " + t.name);
        var generators = UnityEngine.Object.FindObjectsByType<PlatformSpawner>();
        Check(generators.Length == 1 && generators[0].name == "Platform Spawner", "One Platform Spawner");
        var s = generators[0];
        Check(s.count == 3 && s.timeBetSpawnMin == 1.25f && s.timeBetSpawnMax == 2.25f &&
            s.yMin == -3.5f && s.yMax == 1.5f, "PDF spawner Inspector values");
        Check(s.platformPrefab == AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Platform.prefab"),
            "Spawner references existing Platform prefab");
        var p = s.platformPrefab.GetComponent<Platform>();
        Check(p && p.obstacles.Length == 3 && p.obstacles.Select(o => o.name).SequenceEqual(
            new[] { "Obstacle Left", "Obstacle Mid", "Obstacle Right" }), "Three ordered obstacle references");
        Check(p.GetComponent<SpriteRenderer>().sortingLayerName == "Foreground", "Platform Foreground");
        foreach (var obstacle in p.obstacles)
            Check(obstacle.CompareTag("Dead") && obstacle.GetComponent<Collider2D>().isTrigger &&
                obstacle.GetComponent<SpriteRenderer>().sortingLayerName == "Middleground",
                "Dead trigger and Middleground: " + obstacle.name);
        Check(p.GetComponent<ScrollingObject>().speed == 10 && !p.GetComponent<Collider2D>().isTrigger,
            "Preserved platform scrolling speed and solid collider");
        var manager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        Check(manager && manager.scoreText && manager.gameoverUI && Field<PlayerInput>(manager, "playerInput"),
            "Game Manager score, gameover and restart references");
        var pc = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        Check(pc && pc.CompareTag("Player") && pc.GetComponent<Rigidbody2D>() && pc.GetComponent<Collider2D>() &&
            pc.GetComponent<Animator>().runtimeAnimatorController && pc.deathClip && pc.GetComponent<PlayerInput>().actions,
            "Existing player physics, animation, audio and Input System dependencies");
        var music = GameObject.Find("music").GetComponent<AudioSource>();
        Check(music.clip == AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/music.wav") &&
            music.loop && music.playOnAwake, "Looping music with Play On Awake");
        Check(EditorBuildSettings.scenes.Any(scn => scn.enabled && scn.path == "Assets/Scenes/Main.unity"),
            "Main included in build scenes");
        Debug.Log("W10_SETUP_OK");
    }

    [MenuItem("W10/Run Play Validation and Build Windows")]
    public static void Run()
    {
        Directory.CreateDirectory(Output);
        File.WriteAllText(Output + "/result.txt", "Unity " + Application.unityVersion + " W10 validation\n");
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Main.unity" ||
                Enumerable.Range(0, EditorSceneManager.sceneCount).Any(i => EditorSceneManager.GetSceneAt(i).isDirty))
                throw new Exception("Keep Main active in Edit Mode and resolve unsaved scene changes before validation.");
            // Reopen only this same, clean scene to read the edits saved on disk.
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            Validate();
            initialized = false;
            failed = false;
            SessionState.SetString(PhaseKey, "play");
            deadline = EditorApplication.timeSinceStartup + 60;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            Application.logMessageReceived -= OnLog;
            Application.logMessageReceived += OnLog;
            EditorApplication.playModeStateChanged -= OnState;
            EditorApplication.playModeStateChanged += OnState;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception e) { File.AppendAllText(Output + "/result.txt", "FAILED: " + e + "\n"); Debug.LogException(e); }
    }

    static void OnLog(string message, string trace, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
        failed = true;
        File.AppendAllText(Output + "/result.txt", "RUNTIME ERROR: " + message + "\n" + trace + "\n");
    }

    static void Next() { stage++; stageTime = Time.time; }

    static void DropOnPlatform()
    {
        body.simulated = false;
        body.position = new Vector2(0, 2);
        body.linearVelocity = Vector2.zero;
        body.simulated = true;
    }

    static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Play validation timeout, stage " + stage);
            if (!EditorApplication.isPlaying || Time.time < 0.2f) return;
            if (!initialized)
            {
                initialized = true; stage = 0; placements = 0; previousSpawn = -1;
                player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
                body = player.GetComponent<Rigidbody2D>();
                body.simulated = false; // Keep the test player safe while the actual spawner runs.
                spawner = UnityEngine.Object.FindAnyObjectByType<PlatformSpawner>();
                pool = Field<GameObject[]>(spawner, "platforms");
                poolIds = pool.Select(p => p.GetEntityId()).ToArray();
                Check(pool.Length == 3, "Start creates exactly three pooled platforms");
                Check(GameObject.Find("music").GetComponent<AudioSource>().isPlaying, "Music starts playing");
            }
            if (stage == 0)
            {
                float spawn = Field<float>(spawner, "lastSpawnTime");
                if (spawn == previousSpawn) return;
                if (previousSpawn >= 0)
                    Check(spawn - previousSpawn >= 1.24f && spawn - previousSpawn <= 2.5f,
                        "Placement interval follows PDF random range (frame tolerance)");
                previousSpawn = spawn;
                placements++;
                int index = (Field<int>(spawner, "currentIndex") + 2) % 3;
                var position = pool[index].transform.position;
                Check(position.x <= 20.01f && position.x > 16 && position.y >= -3.5f && position.y <= 1.5f,
                    "Current platform is repositioned at right edge and random PDF height");
                Check(pool.Select(p => p.GetEntityId()).SequenceEqual(poolIds) &&
                    UnityEngine.Object.FindObjectsByType<Platform>().Length == 3,
                    "Pool identities and count remain unchanged");
                if (placements < 4) return;
                spawner.enabled = false;
                scrolling = UnityEngine.Object.FindObjectsByType<ScrollingObject>();
                foreach (var scroll in scrolling) scroll.enabled = false;
                for (int i = 1; i < pool.Length; i++) pool[i].transform.position = new Vector2(30, -25);
                pool[0].transform.position = new Vector2(0, -1);
                foreach (var obstacle in pool[0].GetComponent<Platform>().obstacles) obstacle.SetActive(false);
                DropOnPlatform(); Next();
            }
            else if (stage == 1 && Field<bool>(pool[0].GetComponent<Platform>(), "stepped"))
            {
                Check(Field<int>(GameManager.instance, "score") == 1, "Actual landing collision adds one point");
                DropOnPlatform(); Next();
            }
            else if (stage == 2 && Time.time - stageTime > 1.5f)
            {
                Check(Field<int>(GameManager.instance, "score") == 1, "Second landing on same platform adds no points");
                pool[0].SetActive(false); pool[0].SetActive(true);
                Check(!Field<bool>(pool[0].GetComponent<Platform>(), "stepped"), "OnEnable resets stepped on reuse");
                foreach (var obstacle in pool[0].GetComponent<Platform>().obstacles) obstacle.SetActive(false);
                DropOnPlatform(); Next();
            }
            else if (stage == 3 && Field<bool>(pool[0].GetComponent<Platform>(), "stepped"))
            {
                Check(Field<int>(GameManager.instance, "score") == 2 && GameManager.instance.scoreText.text == "Score : 2",
                    "Reused platform awards a new point and updates existing UI");
                body.position = new Vector2(12, -6); body.linearVelocity = Vector2.zero;
                spawner.enabled = true;
                foreach (var scroll in scrolling) scroll.enabled = true;
                Next();
            }
            else if (stage == 4 && GameManager.instance.isGameover)
            {
                Check(GameManager.instance.gameoverUI.activeSelf, "Deadzone triggers existing gameover UI");
                GameManager.instance.AddScore(10);
                Check(Field<int>(GameManager.instance, "score") == 2, "No score increases after gameover");
                previousSpawn = Field<float>(spawner, "lastSpawnTime");
                stoppedPositions = scrolling.Select(s => s.transform.position).ToArray();
                Next();
            }
            else if (stage == 5 && Time.time - stageTime > 2.5f)
            {
                Check(previousSpawn == Field<float>(spawner, "lastSpawnTime"), "Spawner stops at gameover");
                Check(scrolling.Select((s, i) => s.transform.position == stoppedPositions[i]).All(v => v),
                    "Existing background and platform scrolling stop at gameover");
                oldManager = GameManager.instance;
                mouse = InputSystem.AddDevice<Mouse>();
                InputUser.PerformPairingWithDevice(mouse, player.GetComponent<PlayerInput>().user);
                InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left, true));
                InputSystem.Update();
                Next();
            }
            else if (stage == 6 && GameManager.instance != oldManager && GameManager.instance)
            {
                Check(!GameManager.instance.isGameover && Field<int>(GameManager.instance, "score") == 0 &&
                    !GameManager.instance.gameoverUI.activeSelf, "Jump input restarts Main with reset score and gameover");
                Check(!failed, "No runtime errors during validation");
                Finish(true);
            }
        }
        catch (Exception e)
        {
            File.AppendAllText(Output + "/result.txt", "FAILED: " + e + "\n");
            Finish(false);
        }
    }

    static void Finish(bool passed)
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
        SessionState.SetBool(PhaseKey + ".passed", passed);
        SessionState.SetString(PhaseKey, "finish");
        File.AppendAllText(Output + "/result.txt", passed ? "W10_PLAY_OK\n" : "W10_PLAY_FAILED\n");
        EditorApplication.ExitPlaymode();
    }

    static void OnState(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode) return;
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        EditorApplication.playModeStateChanged -= OnState;
        EditorApplication.delayCall += Resume;
    }

    [MenuItem("W10/Build Windows")]
    public static void BuildWindows()
    {
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                Enumerable.Range(0, EditorSceneManager.sceneCount).Any(i => EditorSceneManager.GetSceneAt(i).isDirty))
                throw new Exception("Build from Edit Mode with saved scenes.");
            Validate();
            Directory.CreateDirectory("Builds/W10");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { "Assets/Scenes/Main.unity" },
                locationPathName = "Builds/W10/Uni-Run.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            Check(report.summary.result == BuildResult.Succeeded, "Windows build succeeded");
            File.AppendAllText(Output + "/result.txt", "W10_BUILD_OK: Builds/W10/Uni-Run.exe\n");
        }
        catch (Exception e) { File.AppendAllText(Output + "/result.txt", "BUILD FAILED: " + e + "\n"); Debug.LogException(e); }
    }
}
