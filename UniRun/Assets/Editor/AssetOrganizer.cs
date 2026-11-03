using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// 에셋 폴더 정리. AssetDatabase.MoveAsset 으로 옮기기 때문에 GUID 와 씬/프리팹 참조가 그대로 유지된다.
// 메뉴: UniRun > Organize Assets  (이미 정리된 항목은 건너뛰므로 여러 번 실행해도 안전)
//
// Assets/
//   Scenes/      Main, UniRun, Adventure
//   Scripts/     Classic (기존 UniRun 스크립트), Adventure (새 게임)
//   Art/         Characters, Classic, Kenney (타일/적/아이템 원본), Generated (생성 아틀라스/아이콘), Fonts
//   Audio/  Animations/  Prefabs/  Data/ (맵 JSON)  Settings/ (입력, 물리 재질)  Editor/
public static class AssetOrganizer {
    static readonly string[,] Moves = {
        { "Assets/Main.unity", "Assets/Scenes/Main.unity" },
        { "Assets/UniRun.unity", "Assets/Scenes/UniRun.unity" },
        { "Assets/Adventure.unity", "Assets/Scenes/Adventure.unity" },
        { "Assets/InputActions.inputactions", "Assets/Settings/InputActions.inputactions" },
        { "Assets/Physics/PlayerNoFriction.physicsMaterial2D", "Assets/Settings/PlayerNoFriction.physicsMaterial2D" },
        { "Assets/Maps/adventure_map.json", "Assets/Data/adventure_map.json" },
        { "Assets/Scripts/BackgroundLoop.cs", "Assets/Scripts/Classic/BackgroundLoop.cs" },
        { "Assets/Scripts/GameManager.cs", "Assets/Scripts/Classic/GameManager.cs" },
        { "Assets/Scripts/Platform.cs", "Assets/Scripts/Classic/Platform.cs" },
        { "Assets/Scripts/PlatformSpawner.cs", "Assets/Scripts/Classic/PlatformSpawner.cs" },
        { "Assets/Scripts/PlayerController.cs", "Assets/Scripts/Classic/PlayerController.cs" },
        { "Assets/Scripts/ScrollingObject.cs", "Assets/Scripts/Classic/ScrollingObject.cs" },
        { "Assets/Scripts/idea note.cs", "Assets/Scripts/Classic/idea note.cs" },
        { "Assets/Sprites/Toko_Run.png", "Assets/Art/Characters/Toko_Run.png" },
        { "Assets/Sprites/Toko_Jump.png", "Assets/Art/Characters/Toko_Jump.png" },
        { "Assets/Sprites/Toko_Die.png", "Assets/Art/Characters/Toko_Die.png" },
        { "Assets/Sprites/Obstacle.png", "Assets/Art/Classic/Obstacle.png" },
        { "Assets/Sprites/Platform.png", "Assets/Art/Classic/Platform.png" },
        { "Assets/Sprites/Platform_Long.png", "Assets/Art/Classic/Platform_Long.png" },
        { "Assets/Sprites/Sky.png", "Assets/Art/Classic/Sky.png" },
        { "Assets/Sprites/License", "Assets/Art/Classic/License" },
        { "Assets/Sprites/tile_data", "Assets/Art/Kenney" },
        { "Assets/Sprites/Adventure", "Assets/Art/Generated" },
        { "Assets/Font", "Assets/Art/Fonts" },
    };
    public static void Organize() {
        var log = new List<string>();
        for (int i = 0; i < Moves.GetLength(0); i++) {
            string from = Moves[i, 0], to = Moves[i, 1];
            if (!Exists(from) || Exists(to)) continue;
            EnsureFolder(Path.GetDirectoryName(to).Replace('\\', '/'));
            string err = AssetDatabase.MoveAsset(from, to);
            log.Add(string.IsNullOrEmpty(err) ? from + " -> " + to : "FAILED " + from + ": " + err);
        }
        // 윈도우 탐색기가 만든 썸네일 캐시 정리
        foreach (string guid in AssetDatabase.FindAssets("Thumbs", new[] { "Assets/Art" })) {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileName(p) == "Thumbs.db") { AssetDatabase.DeleteAsset(p); log.Add("deleted " + p); }
        }
        foreach (string dir in new[] { "Assets/Sprites", "Assets/Maps", "Assets/Physics" }) {
            if (AssetDatabase.IsValidFolder(dir) && AssetDatabase.FindAssets("", new[] { dir }).Length == 0) {
                AssetDatabase.DeleteAsset(dir);
                log.Add("removed empty " + dir);
            }
        }
        // 빌드 설정의 씬 경로를 GUID 기준으로 다시 맞춘다
        var scenes = EditorBuildSettings.scenes;
        for (int i = 0; i < scenes.Length; i++) {
            string p = AssetDatabase.GUIDToAssetPath(scenes[i].guid.ToString());
            if (!string.IsNullOrEmpty(p) && p != scenes[i].path) scenes[i] = new EditorBuildSettingsScene(p, scenes[i].enabled);
        }
        EditorBuildSettings.scenes = scenes;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ASSETS ORGANIZED (" + log.Count + ")\n" + string.Join("\n", log));
    }

    static bool Exists(string path) {
        return AssetDatabase.IsValidFolder(path) || !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets));
    }

    static void EnsureFolder(string path) {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
