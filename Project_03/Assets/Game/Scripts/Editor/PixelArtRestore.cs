using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EW.EditorTools
{
    public static class PixelArtRestore
    {
        const string Previous = "Assets/Game/Resources/EW/Illustrated";
        public const string Archive = "Assets/Game/Art/Archived/Illustrated_2026_11_15";

        [MenuItem("EW/아트/원래 개별 픽셀 에셋 구성 복구")]
        public static void MenuRestore() { Debug.Log(Restore()); }

        public static string Restore()
        {
            // Only the eight sets overwritten by the retired illustration pass are restored.
            // A user supplied replacement outside that directory is preserved.
            var cfg=GameConfig.Load();int restored=0;
            if(cfg!=null)
            {
                foreach(var name in new[]{"FX_Arc","FX_Explosion","FX_AcidSplash","FX_ReactBurst","UI_Skill_tok","UI_Skill_jam","UI_Skill_pull","UI_Skill_wire"})
                {
                    var set=cfg.GetSet(name);if(set==null||set.frames==null)continue;
                    bool overwritten=set.frames.Any(sp=>sp!=null&&AssetDatabase.GetAssetPath(sp).Contains("/Illustrated"));
                    if(!overwritten)continue;
                    var original=EWAssetSetup.P(name);
                    if(original.Length==0)throw new InvalidOperationException("Missing ORIGINAL sprites: "+name);
                    set.frames=original;set.fps=name.StartsWith("UI_")?1:name=="FX_Arc"||name=="FX_Explosion"?16:14;
                    restored++;
                }
                EditorUtility.SetDirty(cfg);
            }
            EWRooms.ClearCache();
            EWSceneBuilder.BuildAll(); // Existing room data, gameplay edits and authored roots are retained.
            AssetDatabase.SaveAssets();
            if(AssetDatabase.IsValidFolder(Previous))
            {
                if(!AssetDatabase.IsValidFolder("Assets/Game/Art/Archived"))AssetDatabase.CreateFolder("Assets/Game/Art","Archived");
                if(AssetDatabase.IsValidFolder(Archive))throw new InvalidOperationException("Archive already exists; refusing to overwrite it.");
                string error=AssetDatabase.MoveAsset(Previous,Archive);
                if(!string.IsNullOrEmpty(error))throw new InvalidOperationException(error);
            }
            AssetDatabase.SaveAssets();
            return "Original individual sprites/parallax restored; overridden sets="+restored+"; illustration archive="+Archive;
        }
    }
}
