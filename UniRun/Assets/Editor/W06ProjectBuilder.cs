using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

// Editor-only tooling. The saved scene and assets work without running this tool.
public static class W06ProjectBuilder
{
    [MenuItem("W06/Rebuild PDF Main Scene")]
    public static void Build()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory("Assets/Animations");
        SliceRun();
        AssetDatabase.Refresh();
        var run = CreateClip("Run", 8, 16, true);
        var jump = CreateClip("Jump", 5, 6, true);
        var die = CreateClip("Die", 6, 6, false);
        const string controllerPath = "Assets/Animations/Player.controller";
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(controllerPath)) AssetDatabase.DeleteAsset(controllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);
        var sm = controller.layers[0].stateMachine;
        var runState = sm.AddState("Run", new Vector3(300, 0)); runState.motion = run;
        var jumpState = sm.AddState("Jump", new Vector3(300, 100)); jumpState.motion = jump;
        var dieState = sm.AddState("Die", new Vector3(300, 240)); dieState.motion = die;
        sm.defaultState = runState;
        Transition(runState.AddTransition(jumpState), AnimatorConditionMode.IfNot, "Grounded");
        Transition(jumpState.AddTransition(runState), AnimatorConditionMode.If, "Grounded");
        Transition(sm.AddAnyStateTransition(dieState), AnimatorConditionMode.If, "Die");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0, 0, -10);
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = 5;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(49, 77, 121, 255);
        camera.nearClipPlane = 0.3f; camera.farClipPlane = 1000; camera.depth = -1;

        var platform = new GameObject("Start Platform", typeof(SpriteRenderer), typeof(BoxCollider2D));
        platform.transform.position = new Vector3(0, -1, 0);
        platform.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Classic/Platform_Long.png");
        platform.GetComponent<BoxCollider2D>().size = new Vector2(16, 2);
        Deadzone("Deadzone", -8); Deadzone("Deadzone_temp", 6);

        var player = new GameObject("Player", typeof(SpriteRenderer), typeof(Rigidbody2D),
            typeof(CircleCollider2D), typeof(AudioSource), typeof(Animator), typeof(PlayerController));
        player.tag = "Player"; player.transform.position = new Vector3(-6, 2, 0);
        player.GetComponent<SpriteRenderer>().sprite = Sprites("Run")[0];
        var body = player.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic; body.mass = 1; body.gravityScale = 1;
        body.linearDamping = 0; body.angularDamping = 0.05f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.sleepMode = RigidbodySleepMode2D.NeverSleep;
        body.interpolation = RigidbodyInterpolation2D.None;
        var collider = player.GetComponent<CircleCollider2D>();
        collider.offset = new Vector2(0, -0.57f); collider.radius = 0.2f;
        var audio = player.GetComponent<AudioSource>();
        audio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/jump.wav");
        audio.playOnAwake = false;
        player.GetComponent<Animator>().runtimeAnimatorController = controller;
        player.GetComponent<PlayerController>().deathClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/die.wav");

        // Page 58 uses CallbackContext: connect both performed and canceled via Unity Events.
        var input = player.AddComponent<PlayerInput>();
        input.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Settings/InputActions.inputactions");
        input.defaultActionMap = "Player";
        input.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        var action = input.actions.FindAction("Player/Jump", true);
        var actionEvent = new PlayerInput.ActionEvent(action);
        UnityEventTools.AddPersistentListener(actionEvent, player.GetComponent<PlayerController>().OnJump);
        input.actionEvents = new[] { actionEvent };

        // Retain the supplied starter project's global gravity (-20).
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
        var previous = EditorBuildSettings.scenes.Where(s => s.path != "Assets/Scenes/Main.unity").ToList();
        previous.Insert(0, new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true));
        EditorBuildSettings.scenes = previous.ToArray();
        AssetDatabase.SaveAssets();
        Validate();
        Debug.Log("W06_BUILD_OK");
    }

    static void SliceRun()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Art/Characters/Toko_Run.png");
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 32; importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        var sprites = new SpriteMetaData[8];
        for (int i = 0; i < sprites.Length; i++)
            sprites[i] = new SpriteMetaData { name = "Toko_Run_" + i, rect = new Rect(i * 64, 0, 64, 64),
                alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) };
#pragma warning disable 0618
        importer.spritesheet = sprites;
#pragma warning restore 0618
        importer.SaveAndReimport();
    }

    static Sprite[] Sprites(string name) => AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Characters/Toko_" + name + ".png")
        .OfType<Sprite>().OrderBy(s => s.name, StringComparer.Ordinal).ToArray();

    static AnimationClip CreateClip(string name, int count, int rate, bool loop)
    {
        var frames = Sprites(name);
        if (frames.Length != count) throw new Exception(name + ": incorrect sprite count " + frames.Length);
        var path = "Assets/Animations/" + name + ".anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (!clip) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
        clip.frameRate = rate;
        var keys = new ObjectReferenceKeyframe[count + 1];
        for (int i = 0; i <= count; i++)
            keys[i] = new ObjectReferenceKeyframe { time = (float)i / rate, value = frames[Math.Min(i, count - 1)] };
        AnimationUtility.SetObjectReferenceCurve(clip,
            EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop; AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip); return clip;
    }

    static void Transition(AnimatorStateTransition transition, AnimatorConditionMode mode, string parameter)
    {
        transition.hasExitTime = false; transition.duration = 0;
        transition.canTransitionToSelf = false; transition.AddCondition(mode, 0, parameter);
    }

    static void Deadzone(string name, float y)
    {
        var obj = new GameObject(name, typeof(BoxCollider2D));
        obj.tag = "Dead"; obj.transform.position = new Vector3(0, y, 0);
        var collider = obj.GetComponent<BoxCollider2D>();
        collider.isTrigger = true; collider.size = new Vector2(50, 2);
    }

    [MenuItem("W06/Validate PDF Main Scene")]
    public static void Validate()
    {
        var roots = EditorSceneManager.GetActiveScene().GetRootGameObjects();
        var names = new[] { "Main Camera", "Start Platform", "Deadzone", "Deadzone_temp", "Player" };
        if (!roots.Select(r => r.name).OrderBy(n => n).SequenceEqual(names.OrderBy(n => n)))
            throw new Exception("Unexpected root hierarchy");
        foreach (var root in roots)
        {
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root) != 0)
                throw new Exception("Missing Script: " + root.name);
            foreach (var component in root.GetComponents<Component>())
            {
                var serialized = new SerializedObject(component);
                var property = serialized.GetIterator();
                while (property.NextVisible(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference &&
                        property.objectReferenceValue == null && !property.objectReferenceEntityIdValue.Equals(default(UnityEngine.EntityId)))
                        throw new Exception("Broken reference: " + root.name + "/" + property.propertyPath);
            }
        }
        var player = GameObject.Find("Player");
        if (!player.GetComponent<Animator>().runtimeAnimatorController || !player.GetComponent<PlayerController>().deathClip ||
            !player.GetComponent<AudioSource>().clip || !player.GetComponent<PlayerInput>().actions)
            throw new Exception("Required player reference missing");
        if (Sprites("Run").Length != 8 || Sprites("Jump").Length != 5 || Sprites("Die").Length != 6)
            throw new Exception("Sprite slices missing");
        Debug.Log("W06_REFERENCES_OK");
    }
}
