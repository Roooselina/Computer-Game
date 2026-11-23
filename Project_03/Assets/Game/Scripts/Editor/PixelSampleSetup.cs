using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EW.EditorTools
{
    /// <summary>Isolated sample workspace. Never rewrites production map placement or sprites.</summary>
    public static class PixelSampleSetup
    {
        public const string Root="Assets/Game/Art/PixelSamples";
        public const string ScenePath=Root+"/Tests/PixelConnectionTest.unity";
        static Sprite S(string n)=>AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/"+n+".png");
        static GameObject Obj(string n,Transform parent,Vector2 pos)
        {var go=new GameObject(n);go.transform.SetParent(parent,false);go.transform.localPosition=pos;return go;}
        static Tile Tile(string n)
        {
            string p=Root+"/Tiles/"+n+".asset";
            var t=AssetDatabase.LoadAssetAtPath<Tile>(p);
            if(t==null){t=ScriptableObject.CreateInstance<Tile>();AssetDatabase.CreateAsset(t,p);}
            t.sprite=S(n);t.colliderType=UnityEngine.Tilemaps.Tile.ColliderType.None;EditorUtility.SetDirty(t);return t;
        }
        static Tilemap Layer(string n,Transform parent,Vector3 size,int order)
        {
            var grid=Obj(n+"Grid",parent,Vector2.zero).AddComponent<Grid>();grid.cellSize=size;
            var go=Obj(n,grid.transform,Vector2.zero);var map=go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>().sortingOrder=order;
            return map;
        }
        static void Strip(Tilemap map,int x,int y,int count,Transform collision)
        {
            for(int i=0;i<count;i++)map.SetTile(new Vector3Int(x+i,y,0),Tile(i==0?"platform_left":i==count-1?"platform_right":"platform_middle"));
            var go=Obj("FunctionalPlatform_"+x+"_"+y,collision,new Vector2(x+count*.5f,y+1-.125f));
            var box=go.AddComponent<BoxCollider2D>();box.size=new Vector2(count,.25f);
            go.AddComponent<OneWayPlatform>();
        }
        static void Decor(string n,Sprite s,Transform root,Vector2 pos,int order)
        {if(s==null)throw new Exception("Missing sample/reference sprite: "+n);var sr=Obj(n,root,pos).AddComponent<SpriteRenderer>();sr.sprite=s;sr.sortingOrder=order;}
        static Transform Depth(string n,Transform root,float follow)
        {
            var tr=Obj(n,root,Vector2.zero).transform;var p=tr.gameObject.AddComponent<Parallax>();
            p.follow=follow;p.reference=new Vector2(10,6);return tr;
        }
        [MenuItem("EW/아트/개별 도트 샘플 테스트 씬 만들기")]
        public static string Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play first");
            Directory.CreateDirectory(Root+"/Tiles");Directory.CreateDirectory(Root+"/Tests");
            AssetDatabase.Refresh();
            foreach(var file in Directory.GetFiles(Root,"*.png"))
            {
                string p=file.Replace('\\','/');var ti=(TextureImporter)AssetImporter.GetAtPath(p);
                ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Single;
                ti.spritePixelsPerUnit=32;ti.filterMode=FilterMode.Point;ti.mipmapEnabled=false;
                ti.textureCompression=TextureImporterCompression.Uncompressed;ti.alphaIsTransparency=true;
                ti.npotScale=TextureImporterNPOTScale.None;ti.wrapMode=TextureWrapMode.Clamp;
                var set=new TextureImporterSettings();ti.ReadTextureSettings(set);set.spriteAlignment=(int)SpriteAlignment.Custom;
                set.spritePivot=p.Contains("platform_")?new Vector2(.5f,1):p.Contains("specimen_tank")?new Vector2(.5f,0):new Vector2(.5f,.5f);
                ti.SetTextureSettings(set);ti.SaveAndReimport();
            }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("PixelSampleTest");var mc=root.AddComponent<MapController>();
            mc.mapId="ART_TEST";mc.title="개별 도트 · 연결 테스트";mc.bounds=new Rect(0,0,32,12);
            mc.background=new Color(.06f,.08f,.105f);
            var sp=Obj("Spawn_start",root.transform,new Vector2(8,3)).AddComponent<SpawnPoint>();sp.id="start";
            var cam=new GameObject("Main Camera").AddComponent<Camera>();cam.tag="MainCamera";
            cam.orthographic=true;cam.orthographicSize=PixelScreen.OrthoSize;cam.transform.position=new Vector3(10,6,-10);
            cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=mc.background;cam.gameObject.AddComponent<AudioListener>();
            var far=Depth("Far_0.7",root.transform,.7f);var mid=Depth("Mid_0.3",root.transform,.3f);
            var fore=Depth("Foreground_-0.06",root.transform,-.06f);
            for(int i=-2;i<10;i++)
            {
                Decor("RepeatableFar_"+i,Ctx.S("L_Far"),far,new Vector2(i*20,3),-60);
                Decor("MidMachine_"+i,Ctx.S(i%2==0?"L_MidA":"L_MidB"),mid,new Vector2(i*10,3),-40);
            }
            foreach(var sr in far.GetComponentsInChildren<SpriteRenderer>())sr.color=new Color(.4f,.5f,.55f);
            foreach(var sr in mid.GetComponentsInChildren<SpriteRenderer>())sr.color=new Color(.4f,.5f,.55f);
            Decor("ForegroundPipe",Ctx.S("L_ForePipe"),fore,new Vector2(20,1),25);
            var collision=Obj("Collision",root.transform,Vector2.zero).transform;
            var floor=Layer("PlatformTiles32",root.transform,Vector3.one,0);floor.tileAnchor=new Vector3(.5f,1,0);
            Strip(floor,0,2,32,collision);Strip(floor,3,5,5,collision);Strip(floor,19,5,7,collision);
            var pipes=Layer("PipeTiles16",root.transform,new Vector3(.5f,.5f,1),-3);
            // Long horizontal run, T branch, an elbow and vertical return; exact 16px joints.
            for(int x=3;x<=15;x++)pipes.SetTile(new Vector3Int(x,16,0),Tile(x==8?"pipe_t":x==15?"pipe_elbow":"pipe_horizontal"));
            for(int y=10;y<16;y++){pipes.SetTile(new Vector3Int(15,y,0),Tile("pipe_vertical"));pipes.SetTile(new Vector3Int(8,y,0),Tile("pipe_vertical"));}
            pipes.SetTile(new Vector3Int(15,12,0),Tile("pipe_t_right"));
            pipes.SetTile(new Vector3Int(16,12,0),Tile("pipe_horizontal"));pipes.SetTile(new Vector3Int(17,12,0),Tile("pipe_cap"));
            Decor("SpecimenTank_native80x112",S("specimen_tank"),root.transform,new Vector2(12.5f,3),-1);
            // Side walls are functional boundaries, not baked art.
            foreach(float x in new[]{-.25f,32.25f}){var box=Obj("Boundary",collision,new Vector2(x,6)).AddComponent<BoxCollider2D>();box.size=new Vector2(.5f,12);}
            var ladder=Obj("SampleLadder",root.transform,new Vector2(6,4.5f));
            var lc=ladder.AddComponent<BoxCollider2D>();lc.isTrigger=true;lc.size=new Vector2(.8f,3);
            var climb=ladder.AddComponent<Ladder>();climb.bottomY=3;climb.topY=6;
            for(int y=3;y<6;y++)Decor("OriginalLadder_"+y,Ctx.P0("Prop_Ladder"),ladder.transform,new Vector2(0,y-4.5f),-2);
            // A reusable Grid/Tilemap prefab records the connection example; not part of production maps.
            PrefabUtility.SaveAsPrefabAsset(pipes.transform.parent.gameObject,Root+"/Tests/PipeConnectionGrid.prefab");
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,ScenePath);
            Validate();
            return ScenePath+"; 2 tilemaps: platform/pipe, 4 independent depth groups, native character spawns at Play. Production scenes untouched.";
        }
        public static string Validate()
        {
            foreach(string n in new[]{"platform_left","platform_middle","platform_right","pipe_horizontal","pipe_vertical","pipe_elbow","pipe_t","pipe_t_right","pipe_cap","specimen_tank"})
            {
                var s=S(n);if(s==null||s.pixelsPerUnit!=32)throw new Exception("Missing sprite or PPU: "+n);
                var ti=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(s));
                if(ti.filterMode!=FilterMode.Point||ti.textureCompression!=TextureImporterCompression.Uncompressed||ti.mipmapEnabled)throw new Exception("Pixel import mismatch "+n);
            }
            Debug.Log("[PIXEL_SAMPLE] 10 native sprites: Point, 32PPU, no compression/mips; separate tiles and prop.");
            return "sample imports verified";
        }
    }
}

