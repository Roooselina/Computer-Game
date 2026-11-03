using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

// Isolated batch Play Mode validation; does not add objects or gameplay to Main.
public static class W06PlayValidation
{
    static PlayerController player;
    static Rigidbody2D body;
    static Animator animator;
    static Mouse mouse;
    static int stage;
    static float start;
    static double deadline;
    static string output;
    static bool failed;
    static float initialY, shortPeak;
    static bool oldOptionsEnabled;
    static EnterPlayModeOptions oldOptions;
    static double beginAfter;

    public static void BuildAndRun()
    {
        W06ProjectBuilder.Build();
        Run();
    }

    public static void Run()
    {
        beginAfter = EditorApplication.timeSinceStartup + 2;
        EditorApplication.update += WaitForEditor;
    }

    static void WaitForEditor()
    {
        if (EditorApplication.timeSinceStartup < beginAfter || EditorApplication.isUpdating || EditorApplication.isCompiling) return;
        EditorApplication.update -= WaitForEditor;
        Begin();
    }

    static void Begin()
    {
        output = Path.GetFullPath("../play-validation");
        Directory.CreateDirectory(output);
        failed = false;
        File.WriteAllText(Path.Combine(output, "result.txt"), "Unity " + Application.unityVersion + " Play Mode\n");
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        W06ProjectBuilder.Validate();
        oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
        oldOptions = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        deadline = EditorApplication.timeSinceStartup + 60;
        stage = 0;
        Application.logMessageReceived += Log;
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
    }

    static void Log(string message, string trace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
        {
            failed = true;
            File.AppendAllText(Path.Combine(output, "result.txt"), "ERROR: " + message + "\n" + trace + "\n");
        }
    }

    static T Field<T>(string name) => (T)typeof(PlayerController).GetField(name,
        BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player);
    static void Check(bool ok, string message)
    {
        if (!ok) throw new Exception("W06 validation failed: " + message);
        File.AppendAllText(Path.Combine(output, "result.txt"), "PASS: " + message + "\n");
        Debug.Log("W06_PLAY_PASS " + message);
    }
    static void Press(bool down)
    {
        InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left, down));
        InputSystem.Update();
    }
    static void Next() { stage++; start = Time.time; }

    static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Play validation timeout at stage " + stage);
            if (stage == 12 && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.update -= Tick;
                Application.logMessageReceived -= Log;
                EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled;
                EditorSettings.enterPlayModeOptions = oldOptions;
                File.AppendAllText(Path.Combine(output, "result.txt"), failed ? "FAILED\n" : "W06_PLAY_OK\n");
                EditorApplication.Exit(failed ? 1 : 0); return;
            }
            if (!EditorApplication.isPlaying) return;
            if (stage == 0)
            {
                player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
                if (!player || Time.time < 0.1f) return;
                body = player.GetComponent<Rigidbody2D>(); animator = player.GetComponent<Animator>();
                mouse = InputSystem.AddDevice<Mouse>();
                InputUser.PerformPairingWithDevice(mouse, player.GetComponent<PlayerInput>().user);
                Next();
            }
            else if (stage == 1 && Time.time - start > 1)
            {
                Check(Field<bool>("isGrounded"), "Player lands on Start Platform");
                Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Run"), "Grounded plays Run");
                initialY = body.position.y; shortPeak = initialY;
                Capture("01-run"); Press(true);
                Check(Field<int>("jumpCount") == 1, "Mouse binding invokes OnJump through saved PlayerInput event");
                Next();
            }
            else if (stage == 2 && Time.time - start > 0.06f)
            {
                Check(body.linearVelocity.y > 0, "Jump force produces upward velocity");
                Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"), "Airborne plays Jump");
                float velocity = body.linearVelocity.y;
                Press(false);
                Check(Mathf.Abs(body.linearVelocity.y - velocity * 0.5f) < 0.001f, "Release halves rising Y velocity");
                Capture("02-jump"); Next();
            }
            else if (stage == 3)
            {
                shortPeak = Mathf.Max(shortPeak, body.position.y);
                if (Time.time - start < 0.4f || !Field<bool>("isGrounded")) return;
                Check(!Field<bool>("isDead"), "Short jump stays below temporary upper deadzone");
                Check(Field<int>("jumpCount") == 0, "Landing resets jump count");
                Press(true); Next();
            }
            else if (stage == 4 && Time.time - start > 0.12f)
            {
                Check(body.position.y > initialY, "Held jump rises");
                Press(false); Press(true);
                Check(Field<int>("jumpCount") == 2, "Second airborne jump allowed");
                Press(false); Press(true);
                Check(Field<int>("jumpCount") == 2, "Third airborne jump rejected");
                Press(false); Next();
            }
            else if (stage == 5 && Time.time - start > 0.12f)
            {
                Check(body.position.y > shortPeak, "Double jump rises higher than short jump");
                Capture("03-double-jump");
                // Deliberately enter the actual upper trigger to validate its death path.
                body.position = new Vector2(-6, 5.7f); body.linearVelocity = Vector2.zero;
                Next();
            }
            else if (stage == 6 && Time.time - start > 0.12f)
            {
                Check(Field<bool>("isDead"), "Deadzone_temp triggers death");
                Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Die"), "Death immediately plays Die");
                Check(player.GetComponent<AudioSource>().clip == player.deathClip, "Death selects die audio clip");
                int count = Field<int>("jumpCount"); Press(true); Press(false);
                Check(Field<int>("jumpCount") == count, "Dead player ignores jump input");
                Capture("04-die"); Next();
            }
            else if (stage == 7 && Time.time - start > 1.1f)
            {
                Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Die") &&
                    animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1, "Die finishes without returning to Run or Jump");
                Check(player.GetComponent<SpriteRenderer>().sprite.name == "Toko_Die_5", "Die holds final sprite");
                Capture("05-die-final");
                // Reset just the runtime player to exercise the lower trigger independently.
                typeof(PlayerController).GetField("isDead", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, false);
                body.position = new Vector2(12, -6); body.linearVelocity = Vector2.zero;
                animator.Play("Jump", 0, 0); Next();
            }
            else if (stage == 8 && Field<bool>("isDead"))
            {
                Check(true, "Falling outside platform enters lower Deadzone"); Next();
            }
            else if (stage == 9 && Time.time - start > 0.1f)
            {
                Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Die"), "Lower deadzone plays Die");
                Next();
            }
            else if (stage == 10)
            {
                Check(!failed, "No runtime exceptions or error logs"); Next();
            }
            else if (stage == 11)
            {
                InputSystem.RemoveDevice(mouse);
                stage = 12; EditorApplication.ExitPlaymode();
            }
        }
        catch (Exception exception)
        {
            failed = true;
            File.AppendAllText(Path.Combine(output, "result.txt"), exception + "\n");
            Debug.LogException(exception);
            EditorApplication.update -= Tick;
            EditorApplication.Exit(1);
        }
    }

    static void Capture(string name)
    {
        var camera = Camera.main;
        var target = RenderTexture.GetTemporary(1280, 720, 24);
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
        var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
        File.WriteAllBytes(Path.Combine(output, name + ".png"), image.EncodeToPNG());
        camera.targetTexture = previousTarget; RenderTexture.active = previousActive;
        RenderTexture.ReleaseTemporary(target); UnityEngine.Object.DestroyImmediate(image);
    }
}
