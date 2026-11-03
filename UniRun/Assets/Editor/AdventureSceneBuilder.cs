using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

// Adventure 씬을 처음부터 만드는 에디터 도구. 메뉴: UniRun > Build Adventure Scene
// 맵을 다시 생성했을 때(Tools/MapGen/gamemap.py) 씬은 다시 만들 필요 없이 JSON 만 바뀌면 된다.
public static class AdventureSceneBuilder {
    const string ScenePath = "Assets/Scenes/Adventure.unity";
    const string AtlasPath = "Assets/Art/Generated/tiles_atlas.png";
    const string GenDir = "Assets/Art/Generated";
    const string KenneyDir = "Assets/Art/Kenney/PNG";
    const int Cell = 68, Pad = 2;

    public static void Build() {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        AssetOrganizer.Organize();
        ConfigureTextures();
        BuildScene();
    }

    // 배치 모드용: unity run <project> -- -executeMethod AdventureSceneBuilder.BuildFromCommandLine
    public static void BuildFromCommandLine() {
        AssetOrganizer.Organize();
        ConfigureTextures();
        BuildScene();
    }

    // 배치 모드용: 씬 생성 후 Windows 실행 파일 빌드 (Build/UniRunAdventure.exe)
    public static void BuildPlayerFromCommandLine() {
        BuildFromCommandLine();
        BuildWindows();
    }

    public static void BuildWindows() {
        PlayerSettings.productName = "UniRun Adventure";
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { ScenePath },
            locationPathName = "Build/UniRunAdventure.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        Debug.Log("ADVENTURE BUILD: " + report.summary.result + " errors=" + report.summary.totalErrors);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded && Application.isBatchMode) EditorApplication.Exit(1);
    }

    // ---------------------------------------------------------------- 텍스처 임포트 설정
    static void ConfigureTextures() {
        AssetDatabase.Refresh();
        var atlas = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
        SetCommon(atlas, SpriteImportMode.Multiple, SpriteMeshType.FullRect);
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);
        int texH = 5 * Cell;
        var metas = new List<SpriteMetaData>();
        for (int z = 0; z < 4; z++)
            for (int n = 1; n <= 27; n++)
                metas.Add(Meta(string.Format("terrain_{0}_{1:00}", z, n), (n - 1) * Cell + Pad, texH - (z + 1) * Cell + Pad));
        for (int f = 0; f < 4; f++) {
            metas.Add(Meta("fluid_" + f + "_body", (f * 2) * Cell + Pad, texH - 5 * Cell + Pad));
            metas.Add(Meta("fluid_" + f + "_top", (f * 2 + 1) * Cell + Pad, texH - 5 * Cell + Pad));
        }
#pragma warning disable 618
        atlas.spritesheet = metas.ToArray();
#pragma warning restore 618
        atlas.SaveAndReimport();

        foreach (string dir in new[] { GenDir, KenneyDir + "/Other", KenneyDir + "/Enemies", KenneyDir + "/Items", KenneyDir + "/Backgrounds" }) {
            foreach (string path in Directory.GetFiles(dir, "*.png")) {
                string p = path.Replace('\\', '/');
                if (p == AtlasPath) continue;
                var imp = (TextureImporter)AssetImporter.GetAtPath(p);
                bool fullRect = p.Contains("Backgrounds") || p.EndsWith("log.png");
                SetCommon(imp, SpriteImportMode.Single, fullRect ? SpriteMeshType.FullRect : SpriteMeshType.Tight);
                if (p.EndsWith("log.png")) imp.spriteBorder = new Vector4(22, 0, 22, 0);
                imp.SaveAndReimport();
            }
        }
    }

    static SpriteMetaData Meta(string spriteName, int x, int y) {
        return new SpriteMetaData { name = spriteName, rect = new Rect(x, y, 64, 64), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) };
    }

    static void SetCommon(TextureImporter imp, SpriteImportMode mode, SpriteMeshType mesh) {
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = mode;
        imp.spritePixelsPerUnit = 64;
        imp.filterMode = FilterMode.Bilinear;
        imp.mipmapEnabled = false;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.wrapMode = TextureWrapMode.Clamp;
        imp.alphaIsTransparency = true;
        imp.maxTextureSize = 4096;
        var s = new TextureImporterSettings();
        imp.ReadTextureSettings(s);
        s.spriteMeshType = mesh;
        s.spriteGenerateFallbackPhysicsShape = true;
        s.spriteExtrude = 1;
        imp.SetTextureSettings(s);
    }

    // ---------------------------------------------------------------- 씬
    static T Load<T>(string path) where T : Object {
        var a = AssetDatabase.LoadAssetAtPath<T>(path);
        if (a == null) throw new System.Exception("Missing asset: " + path);
        return a;
    }

    static Sprite KSprite(string folder, string spriteName) { return Load<Sprite>(KenneyDir + "/" + folder + "/" + spriteName + ".png"); }
    static Sprite GSprite(string spriteName) { return Load<Sprite>(GenDir + "/" + spriteName + ".png"); }

    static void BuildScene() {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var actions = Load<InputActionAsset>("Assets/Settings/InputActions.inputactions");
        var font = Load<Font>("Assets/Art/Fonts/Kenney Mini Square.ttf");
        Sprite[] atlasSprites = AssetDatabase.LoadAllAssetsAtPath(AtlasPath).OfType<Sprite>().ToArray();
        Sprite Atlas(string n) { return atlasSprites.First(s => s.name == n); }

        // 카메라 + 하늘
        var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(RunCamera));
        camGo.tag = "MainCamera";
        var cam = camGo.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color32(120, 170, 230, 255);
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 100f;
        camGo.transform.position = new Vector3(0, 0, -10);
        var skyGo = new GameObject("Parallax Sky", typeof(ParallaxSky));
        skyGo.transform.SetParent(camGo.transform, false);
        skyGo.GetComponent<ParallaxSky>().hills = new[] {
            KSprite("Backgrounds", "set3_hills"), KSprite("Backgrounds", "set1_hills"),
            KSprite("Backgrounds", "set2_hills"), KSprite("Backgrounds", "set4_hills") };

        // 타일맵
        var grid = new GameObject("Grid", typeof(Grid));
        Tilemap terrain = NewTilemap(grid, "Terrain", 0, true);
        Tilemap collision = NewTilemap(grid, "Collision", 0, false);
        var tc = collision.gameObject.AddComponent<TilemapCollider2D>();
        tc.compositeOperation = Collider2D.CompositeOperation.Merge;
        var rb = collision.gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        var comp = collision.gameObject.AddComponent<CompositeCollider2D>();
        comp.geometryType = CompositeCollider2D.GeometryType.Polygons;
        Tilemap fluid = NewTilemap(grid, "Fluid", 20, true);

        var floor = new GameObject("Sea Floor", typeof(BoxCollider2D));
        floor.transform.position = new Vector3(600, -0.5f, 0);
        floor.GetComponent<BoxCollider2D>().size = new Vector2(1400, 1);

        // 맵 로더
        var loaderGo = new GameObject("Map Loader", typeof(MapLoader));
        var loader = loaderGo.GetComponent<MapLoader>();
        loader.mapJson = Load<TextAsset>("Assets/Data/adventure_map.json");
        loader.terrain = terrain;
        loader.collision = collision;
        loader.fluid = fluid;
        loader.terrainSprites = new Sprite[4 * 27];
        for (int z = 0; z < 4; z++)
            for (int n = 1; n <= 27; n++)
                loader.terrainSprites[z * 27 + n - 1] = Atlas(string.Format("terrain_{0}_{1:00}", z, n));
        loader.fluidSprites = new Sprite[8];
        for (int f = 0; f < 4; f++) {
            loader.fluidSprites[f * 2] = Atlas("fluid_" + f + "_body");
            loader.fluidSprites[f * 2 + 1] = Atlas("fluid_" + f + "_top");
        }
        var props = new List<Sprite>();
        foreach (string folder in new[] { "Other", "Enemies", "Items" })
            foreach (string path in Directory.GetFiles(KenneyDir + "/" + folder, "*.png"))
                props.Add(Load<Sprite>(path.Replace('\\', '/')));
        foreach (string n in new[] { "heart", "shield", "magnet" }) props.Add(GSprite(n));
        loader.props = props.ToArray();
        loader.logSprite = GSprite("log");
        loader.glowSprite = GSprite("fx_circle");

        // 플레이어
        var player = new GameObject("Player", typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(RunnerPlayer));
        player.tag = "Player";
        player.layer = 2;
        player.transform.localScale = new Vector3(1.5f, 1.5f, 1f);   // Ignore Raycast: 바닥 검사 캐스트에 자기 자신이 걸리지 않게
        var body = player.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.sleepMode = RigidbodySleepMode2D.NeverSleep;
        var capsule = player.GetComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(0.6f, 1.36f);
        capsule.offset = new Vector2(0.03f, -0.04f);
        const string matPath = "Assets/Settings/PlayerNoFriction.physicsMaterial2D";
        Directory.CreateDirectory("Assets/Settings");
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(matPath);
        if (mat == null) {
            mat = new PhysicsMaterial2D("PlayerNoFriction") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(mat, matPath);
        }
        capsule.sharedMaterial = mat;
        var visual = new GameObject("Visual", typeof(SpriteRenderer), typeof(Animator));
        visual.transform.SetParent(player.transform, false);
        var vsr = visual.GetComponent<SpriteRenderer>();
        vsr.sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Characters/Toko_Run.png").OfType<Sprite>().OrderBy(s => s.name).First();
        vsr.sortingOrder = 10;
        visual.GetComponent<Animator>().runtimeAnimatorController = Load<RuntimeAnimatorController>("Assets/Animations/Player.controller");
        var rp = player.GetComponent<RunnerPlayer>();
        rp.actions = actions;
        rp.visual = visual.transform;
        rp.shieldSprite = GSprite("shield");

        // 게임 매니저, 이펙트, 사운드, HUD
        var game = new GameObject("Run Game", typeof(RunGame), typeof(Fx), typeof(Sfx), typeof(AutoPilot));
        game.GetComponent<RunGame>().actions = actions;
        var fx = game.GetComponent<Fx>();
        fx.circle = GSprite("fx_circle");
        fx.spark = GSprite("fx_spark");
        fx.font = font;
        var sfx = game.GetComponent<Sfx>();
        sfx.jump = Load<AudioClip>("Assets/Audio/jump.wav");
        sfx.die = Load<AudioClip>("Assets/Audio/die.wav");

        var music = new GameObject("Music", typeof(AudioSource), typeof(MusicDirector));
        var src = music.GetComponent<AudioSource>();
        src.clip = Load<AudioClip>("Assets/Audio/music.wav");
        src.loop = true;
        src.playOnAwake = true;
        src.volume = 0.55f;

        var hudGo = new GameObject("HUD", typeof(RectTransform), typeof(RunHud));
        var hud = hudGo.GetComponent<RunHud>();
        hud.font = font;
        hud.heartSprite = GSprite("heart");
        hud.magnetSprite = GSprite("magnet");
        hud.pixel = GSprite("fx_pixel");

        EditorSceneManager.SaveScene(scene, ScenePath);
        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == ScenePath)) {
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        AssetDatabase.SaveAssets();
        Debug.Log("ADVENTURE SCENE BUILT: " + ScenePath);
    }

    static Tilemap NewTilemap(GameObject grid, string objName, int order, bool render) {
        var go = new GameObject(objName, typeof(Tilemap));
        go.transform.SetParent(grid.transform, false);
        if (render) {
            var r = go.AddComponent<TilemapRenderer>();
            r.sortingOrder = order;
            r.mode = TilemapRenderer.Mode.Chunk;
        }
        return go.GetComponent<Tilemap>();
    }
}
