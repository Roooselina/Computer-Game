using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EW.EditorTools
{
    /// <summary>
    /// 메뉴 EW 아래의 구성 도구. 여러 번 실행해도 안전하다.
    /// - 씬은 '__Generated' 아래만 다시 만든다 (그 밖의 사용자 오브젝트 보존).
    /// - GameDatabase / CharacterArt / 애니메이션 클립은 없을 때만 만든다 (사용자 수정 보존).
    /// </summary>
    public static class EWMenu
    {
        [MenuItem("EW/1. 전체 구성 (에셋·데이터·씬·빌드 목록)", priority = 1)]
        public static void BuildEverything()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            RunSetup(false);
            EditorUtility.DisplayDialog("EW", "구성을 마쳤습니다.\nAssets/Game/Scenes/Title.unity 를 열고 Play 하세요.", "확인");
        }

        /// <summary>대화상자 없이 실행 (자동화용).</summary>
        public static string RunSetup(bool forceClips)
        {
            EWRooms.ClearCache();
            EWAssetSetup.ConfigureFont();
            EWAssetSetup.ConfigureAndSliceAll();
            EWAssetSetup.ConfigurePortraitsAndAudio();
            var arts = EWAssetSetup.BuildCharacters(forceClips);
            EWAssetSetup.BuildConfig(arts);
            ConfigurePlayer();
            PixelArtRestore.Restore();
            AssetDatabase.SaveAssets();
            Debug.Log("[EW] 전체 구성 완료");
            return "ok";
        }

        [MenuItem("EW/2. 애니메이션 클립 다시 만들기 (임시 그림 교체 후)", priority = 2)]
        public static void RebuildClips()
        {
            EWAssetSetup.ConfigureAndSliceAll();
            EWAssetSetup.BuildCharacters(true);
            Debug.Log("[EW] 애니메이션 클립을 현재 시트 기준으로 다시 만들었습니다.");
        }

        [MenuItem("EW/3. 타이틀 씬 열기", priority = 20)]
        public static void OpenTitle()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene("Assets/Game/Scenes/Title.unity");
        }

        [MenuItem("EW/4. Windows 빌드", priority = 30)]
        public static void BuildWindowsMenu()
        {
            var r = BuildWindows();
            EditorUtility.DisplayDialog("EW 빌드", r, "확인");
        }

        public static string BuildWindows()
        {
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/Windows"));
            Directory.CreateDirectory(dir);
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var s in EditorBuildSettings.scenes) if (s.enabled) scenes.Add(s.path);
            var opts = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = Path.Combine(dir, "ElectricWorld_LastLight.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(opts);
            string msg = "결과: " + report.summary.result + "\n오류 " + report.summary.totalErrors + " / 경고 " + report.summary.totalWarnings + "\n" + opts.locationPathName;
            foreach (var st in report.steps)
                foreach (var m in st.messages)
                    if (m.type == LogType.Warning || m.type == LogType.Error) msg += "\n[" + m.type + "] " + m.content.Split('\n')[0];
            Debug.Log("[EW] 빌드 " + msg);
            return msg;
        }

        [MenuItem("EW/5. 저장 데이터 폴더 열기", priority = 40)]
        public static void OpenSaveFolder() { EditorUtility.RevealInFinder(Application.persistentDataPath); }

        [MenuItem("EW/6. 저장 데이터 삭제", priority = 41)]
        public static void DeleteSave()
        {
            if (EditorUtility.DisplayDialog("EW", "저장 데이터를 삭제할까요?", "삭제", "취소")) { SaveSystem.DeleteAll(); Debug.Log("[EW] 저장 삭제"); }
        }

        static void ConfigurePlayer()
        {
            PlayerSettings.productName = "ELECTRIC WORLD - LAST LIGHT (Demo)";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
        }
    }
}
