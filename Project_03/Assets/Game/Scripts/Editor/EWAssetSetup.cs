using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace EW.EditorTools
{
    /// <summary>
    /// 텍스처 가져오기 설정(Point 필터·무압축), 시트 분할, 애니메이션 클립·컨트롤러, 캐릭터 그래픽 에셋을 만든다.
    /// 여러 번 실행해도 같은 결과가 되도록 기존 스프라이트 ID 와 사용자가 고친 클립을 보존한다.
    /// </summary>
    public static class EWAssetSetup
    {
        public const string GenDir = "Assets/Game/Art/Generated";
        public const string AnimDir = GenDir + "/Anim";
        public const string ArtDir = "Assets/Game/Data/Art";
        public const string AdditivePath = "Assets/Game/Data/EW_Additive.mat";
        public const string PortraitDir = "Assets/Game/Art/Portraits";
        public const string AudioDir = "Assets/Game/Audio/Generated";

        // ------------------------------------------------------------ 텍스처
        static void Ensure(string dir)
        {
            if (AssetDatabase.IsValidFolder(dir)) return;
            string parent = Path.GetDirectoryName(dir).Replace('\\', '/');
            Ensure(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(dir));
        }

        public static void ConfigureAndSliceAll()
        {
            var specs = new List<SheetSpec>();
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Game/Art/Placeholder" }))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                specs.Add(EWArtSpecs.ForPlaceholder(p, tex.width, tex.height));
            }
            specs.AddRange(EWArtSpecs.ThirdParty());
            int n = 0;
            AssetDatabase.StartAssetEditing();
            try { foreach (var s in specs) if (ConfigureImporter(s)) n++; }
            finally { AssetDatabase.StopAssetEditing(); }
            foreach (var s in specs) Slice(s);
            Debug.Log("[EW] 텍스처 설정 " + specs.Count + "개 (변경 " + n + ")");
        }

        static bool ConfigureImporter(SheetSpec s)
        {
            var imp = AssetImporter.GetAtPath(s.path) as TextureImporter;
            if (imp == null) { Debug.LogWarning("[EW] 텍스처 없음: " + s.path); return false; }
            bool dirty = false;
            void Set<T>(T cur, T want, System.Action<T> apply) { if (!EqualityComparer<T>.Default.Equals(cur, want)) { apply(want); dirty = true; } }
            Set(imp.textureType, TextureImporterType.Sprite, v => imp.textureType = v);
            Set(imp.spriteImportMode, SpriteImportMode.Multiple, v => imp.spriteImportMode = v);
            Set(imp.filterMode, FilterMode.Point, v => imp.filterMode = v);
            Set(imp.textureCompression, TextureImporterCompression.Uncompressed, v => imp.textureCompression = v);
            Set(imp.mipmapEnabled, false, v => imp.mipmapEnabled = v);
            Set(imp.alphaIsTransparency, true, v => imp.alphaIsTransparency = v);
            Set(imp.spritePixelsPerUnit, s.ppu, v => imp.spritePixelsPerUnit = v);
            Set(imp.wrapMode, TextureWrapMode.Clamp, v => imp.wrapMode = v);
            var settings = new TextureImporterSettings();
            imp.ReadTextureSettings(settings);
            var mesh = s.fullRect || s.border != Vector4.zero || (s.rects != null && s.rects.Any(r => r.border != Vector4.zero)) ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
            if (settings.spriteMeshType != mesh) { settings.spriteMeshType = mesh; imp.SetTextureSettings(settings); dirty = true; }
            if (dirty) imp.SaveAndReimport();
            return dirty;
        }

        static void Slice(SheetSpec s)
        {
            var imp = AssetImporter.GetAtPath(s.path) as TextureImporter;
            if (imp == null) return;
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(s.path);
            if (tex == null) return;
            int w = tex.width, h = tex.height;
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dp = factory.GetSpriteEditorDataProviderFromObject(imp);
            dp.InitSpriteEditorDataProvider();
            var old = dp.GetSpriteRects().ToDictionary(r => r.name, r => r);
            string baseName = Path.GetFileNameWithoutExtension(s.path);
            var rects = new List<SpriteRect>();
            void Add(string name, Rect r, Vector2 pivot, Vector4 border)
            {
                // 기존 영역 객체를 직접 고치면 아래 '바뀐 것 없음' 비교가 항상 참이 되므로 새 객체에 ID 만 이어받는다
                var sr = new SpriteRect { spriteID = old.TryGetValue(name, out var o) ? o.spriteID : GUID.Generate() };
                sr.name = name; sr.rect = r; sr.alignment = SpriteAlignment.Custom; sr.pivot = pivot; sr.border = border;
                rects.Add(sr);
            }
            if (s.rects != null)
                foreach (var nr in s.rects) Add(nr.name, nr.rect, nr.pivot, nr.border);
            else
            {
                int fw = s.frameW == -1 ? h : (s.frameW > 0 ? s.frameW : w);
                int fh = s.frameH == -1 ? h : (s.frameH > 0 ? s.frameH : h);
                int count = Mathf.Max(1, w / fw);
                for (int i = 0; i < count; i++) Add(baseName + "_" + i, new Rect(i * fw, h - fh, fw, fh), s.pivot, s.border);
            }
            // 바뀐 것이 없으면 다시 가져오지 않는다
            bool same = rects.Count == old.Count && rects.All(r => old.TryGetValue(r.name, out var o) && o.rect == r.rect && o.pivot == r.pivot && o.border == r.border && o.alignment == r.alignment);
            if (same) return;
            dp.SetSpriteRects(rects.ToArray());
            var ids = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (ids != null) ids.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            dp.Apply();
            imp.SaveAndReimport();
        }

        // ------------------------------------------------------------ 스프라이트 조회
        public static Sprite[] Frames(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                .OrderBy(sp => { var i = sp.name.LastIndexOf('_'); return i >= 0 && int.TryParse(sp.name.Substring(i + 1), out int k) ? k : 0; })
                .ToArray();
        }

        public static Sprite Named(string path, string name) => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault(s => s.name == name);

        /// <summary>임시 그림 이름(Prop_Door 등) 또는 제3자 경로로 프레임 배열.</summary>
        public static Sprite[] P(string placeholderName) => Frames(EWArtSpecs.Placeholder + placeholderName + ".png");

        // ------------------------------------------------------------ 애니메이션
        /// <summary>캐릭터마다 상태별 AnimationClip 과 AnimatorController, CharacterArt 를 만든다.</summary>
        public static List<CharacterArt> BuildCharacters(bool forceClips)
        {
            Ensure(AnimDir); Ensure(ArtDir);
            var arts = new List<CharacterArt>();
            foreach (var c in EWArtSpecs.Characters())
            {
                string dir = AnimDir + "/" + c.id;
                Ensure(dir);
                string ctrlPath = dir + "/" + c.id + ".controller";
                var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
                if (ctrl == null) ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
                var sm = ctrl.layers[0].stateMachine;
                Sprite first = null;
                foreach (var a in c.anims)
                {
                    var frames = Frames(a.sheet);
                    if (frames.Length == 0) { Debug.LogWarning("[EW] 프레임 없음: " + a.sheet); continue; }
                    if (first == null) first = frames[0];
                    string clipPath = dir + "/" + c.id + "_" + a.state + ".anim";
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    if (clip == null || forceClips)
                    {
                        bool isNew = clip == null;
                        if (isNew) clip = new AnimationClip();
                        FillClip(clip, frames, a.fps, a.loop);
                        if (isNew) AssetDatabase.CreateAsset(clip, clipPath); else EditorUtility.SetDirty(clip);
                    }
                    var st = sm.states.Select(x => x.state).FirstOrDefault(x => x.name == a.state);
                    if (st == null) { st = sm.AddState(a.state); st.motion = clip; }
                    else if (st.motion == null) st.motion = clip;
                    if (a.state == "Idle") sm.defaultState = st;
                }
                EditorUtility.SetDirty(ctrl);
                string artPath = ArtDir + "/Art_" + c.id + ".asset";
                var art = AssetDatabase.LoadAssetAtPath<CharacterArt>(artPath);
                if (art == null)
                {
                    art = ScriptableObject.CreateInstance<CharacterArt>();
                    art.id = c.id; art.displayName = c.name; art.height = c.height; art.hover = c.hover; art.sortingOrder = c.order;
                    art.facesRight = c.id == "lu" || c.id == "venelle" || c.id.StartsWith("researcher");
                    AssetDatabase.CreateAsset(art, artPath);
                }
                if (art.controller == null) art.controller = ctrl;
                if (art.defaultSprite == null) art.defaultSprite = first;
                EditorUtility.SetDirty(art);
                arts.Add(art);
            }
            AssetDatabase.SaveAssets();
            return arts;
        }

        static void FillClip(AnimationClip clip, Sprite[] frames, float fps, bool loop)
        {
            clip.frameRate = Mathf.Max(fps, 1);
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[frames.Length + 1];
            for (int i = 0; i < frames.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
            keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / fps, value = frames[frames.Length - 1] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var set = AnimationUtility.GetAnimationClipSettings(clip);
            set.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, set);
        }

        // ------------------------------------------------------------ 데이터·설정
        public static GameConfig BuildConfig(List<CharacterArt> arts)
        {
            Ensure("Assets/Game/Data"); Ensure("Assets/Game/Resources/EW");
            string dbPath = "Assets/Game/Data/GameDatabase.asset";
            var db = AssetDatabase.LoadAssetAtPath<GameDatabase>(dbPath);
            if (db == null) { db = DefaultContent.Create(); AssetDatabase.CreateAsset(db, dbPath); }
            else UpgradeDatabase(db);

            string matPath = "Assets/Game/Data/EW_Line.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) { mat = new Material(Shader.Find("Sprites/Default")); AssetDatabase.CreateAsset(mat, matPath); }

            string cfgPath = "Assets/Game/Resources/EW/GameConfig.asset";
            var cfg = AssetDatabase.LoadAssetAtPath<GameConfig>(cfgPath);
            if (cfg == null) { cfg = ScriptableObject.CreateInstance<GameConfig>(); AssetDatabase.CreateAsset(cfg, cfgPath); }
            cfg.database = db;
            cfg.dialogueJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Game/Data/Dialogue/dialogue_ko.json");
            if (cfg.font == null) cfg.font = AssetDatabase.LoadAssetAtPath<Font>(EWArtSpecs.TP + "quiple_Galmuri/Galmuri11.ttf");
            if (cfg.fontBold == null) cfg.fontBold = AssetDatabase.LoadAssetAtPath<Font>(EWArtSpecs.TP + "quiple_Galmuri/Galmuri11-Bold.ttf");
            // 긴 한글 대사·메뉴용 본문 글꼴 (Pretendard 정적 TTF, OFL — Assets/Game/Fonts/Pretendard/SOURCE.md)
            if (cfg.fontBody == null) cfg.fontBody = AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Fonts/Pretendard/Pretendard-Medium.ttf");
            if (cfg.fontBodyBold == null) cfg.fontBodyBold = AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Fonts/Pretendard/Pretendard-SemiBold.ttf");
            if (cfg.panel == null) cfg.panel = Frames(EWArtSpecs.TP + "Kenney_PixelUI/space.png").FirstOrDefault();
            if (cfg.panelInlay == null) cfg.panelInlay = Frames(EWArtSpecs.TP + "Kenney_PixelUI/space_inlay.png").FirstOrDefault();
            if (cfg.selectFrame == null) cfg.selectFrame = Frames(EWArtSpecs.TP + "Kenney_PixelUI/outline_yellow.png").FirstOrDefault();
            if (cfg.pixel == null) cfg.pixel = P("Tile_Pixel").FirstOrDefault();
            cfg.lineMaterial = mat;
            // 빛 표현용 가산 혼합 재질
            var add = AssetDatabase.LoadAssetAtPath<Material>(AdditivePath);
            if (add == null)
            {
                var sh = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Mobile/Particles/Additive") ?? Shader.Find("Sprites/Default");
                add = new Material(sh); AssetDatabase.CreateAsset(add, AdditivePath);
            }
            cfg.additive = add;
            // 소리: 합성 임시 음원 + 타이틀 음악 (이름으로 찾는다)
            cfg.audio.RemoveAll(a => a == null || a.clip == null);
            void SetAudio(string name, AudioClip clip)
            {
                if (clip == null) return;
                var e = cfg.audio.FirstOrDefault(x => x.name == name);
                if (e == null) cfg.audio.Add(new AudioEntry { name = name, clip = clip }); else if (e.clip == null) e.clip = clip;
            }
            SetAudio("bgm_title", cfg.titleBgm);
            if (AssetDatabase.IsValidFolder(AudioDir))
                foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { AudioDir }))
                {
                    string ap = AssetDatabase.GUIDToAssetPath(guid);
                    SetAudio(Path.GetFileNameWithoutExtension(ap), AssetDatabase.LoadAssetAtPath<AudioClip>(ap));
                }
            // 스탠딩 일러스트 (<인물>_<표정>.png). 사용자가 그림을 같은 이름으로 덮어쓰면 그대로 연결된다.
            cfg.portraits.RemoveAll(x => x == null || x.sprite == null);
            if (AssetDatabase.IsValidFolder(PortraitDir))
                foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { PortraitDir }))
                {
                    string pp = AssetDatabase.GUIDToAssetPath(guid);
                    var sp = AssetDatabase.LoadAllAssetsAtPath(pp).OfType<Sprite>().FirstOrDefault();
                    string id = Path.GetFileNameWithoutExtension(pp);
                    if (sp == null) continue;
                    var e = cfg.portraits.FirstOrDefault(x => x.id == id);
                    if (e == null) cfg.portraits.Add(new PortraitEntry { id = id, sprite = sp }); else e.sprite = sp;
                }
            if (cfg.titleBgm == null) cfg.titleBgm = AssetDatabase.LoadAssetAtPath<AudioClip>(EWArtSpecs.TP + "ansimuz_SynthCities/cyberpunk-street.ogg");
            foreach (var a in arts) if (!cfg.characters.Contains(a)) cfg.characters.Add(a);
            cfg.characters.RemoveAll(a => a == null);
            void SetSet(string name, Sprite[] frames, float fps)
            {
                var s = cfg.spriteSets.FirstOrDefault(x => x.name == name);
                if (s == null) { s = new SpriteSet { name = name, fps = fps }; cfg.spriteSets.Add(s); }
                if (s.frames == null || s.frames.Length == 0 || s.frames.Any(f => f == null)) s.frames = frames;
            }
            foreach (var fx in new[] { "Spark", "Bolt", "Explosion", "Arc", "Heal", "Acid", "LightDot" })
                SetSet("FX_" + fx, P("FX_" + fx), fx == "LightDot" ? 12 : 16);
            foreach (var fx in new[] { "Glow", "GlowSoft", "Smoke", "Fire", "FireBig", "Ember", "Ash", "Dust" })
                SetSet("FX_" + fx, P("Scene/FX_" + fx), fx == "Smoke" ? 6 : 12);
            // 2026-11-15: 베넬 화학 효과, 스킬 아이콘·안내서 미리보기 (Tools/qwer_art_gen.py)
            foreach (var fx in new[] { "AcidSplash", "Bubbles", "ReactBurst", "ToxicMist", "Flask", "AcidFan", "PuddleAcid" })
                SetSet("FX_" + fx, P("FX_" + fx), fx == "ToxicMist" ? 5 : fx == "Bubbles" ? 8 : 14);
            foreach (var path in System.IO.Directory.GetFiles(EWArtSpecs.Placeholder, "UI_*.png"))
            {
                string n = System.IO.Path.GetFileNameWithoutExtension(path);
                SetSet(n, P(n), 1);
            }
            // 2026-11-15 4~6장 소품 (Tools/ch4_6_art_gen.py): 연출이 이름으로 꺼내 쓴다 (StoryEvents.PropSprite)
            foreach (var path in System.IO.Directory.GetFiles(EWArtSpecs.Placeholder + "Scene", "U_*.png"))
            {
                string n = System.IO.Path.GetFileNameWithoutExtension(path);
                SetSet(n, P("Scene/" + n), 4);
            }
            cfg.maps.Clear();
            foreach (var r in EWRooms.All()) cfg.maps.Add(new MapEntry { id = r.id, scene = r.scene, title = r.title });
            EditorUtility.SetDirty(cfg);
            AssetDatabase.SaveAssets();
            return cfg;
        }

        /// <summary>스탠딩: 고해상도 그대로 (부드러운 필터, 밉맵 없음). 도트 그림과 다르게 Point 필터를 쓰지 않는다.</summary>
        public static void ConfigurePortraitsAndAudio()
        {
            if (AssetDatabase.IsValidFolder(PortraitDir))
                foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { PortraitDir }))
                {
                    var imp = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;
                    if (imp == null) continue;
                    bool dirty = false;
                    if (imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; dirty = true; }
                    if (imp.spriteImportMode != SpriteImportMode.Single) { imp.spriteImportMode = SpriteImportMode.Single; dirty = true; }
                    if (imp.filterMode != FilterMode.Bilinear) { imp.filterMode = FilterMode.Bilinear; dirty = true; }
                    if (imp.mipmapEnabled) { imp.mipmapEnabled = false; dirty = true; }
                    if (imp.textureCompression != TextureImporterCompression.CompressedHQ) { imp.textureCompression = TextureImporterCompression.CompressedHQ; dirty = true; }
                    if (!imp.alphaIsTransparency) { imp.alphaIsTransparency = true; dirty = true; }
                    if (imp.maxTextureSize < 1024) { imp.maxTextureSize = 1024; dirty = true; }
                    var st = new TextureImporterSettings(); imp.ReadTextureSettings(st);
                    if (st.spriteAlignment != (int)SpriteAlignment.BottomCenter) { st.spriteAlignment = (int)SpriteAlignment.BottomCenter; imp.SetTextureSettings(st); dirty = true; }
                    if (dirty) imp.SaveAndReimport();
                }
            if (AssetDatabase.IsValidFolder(AudioDir))
                foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { AudioDir }))
                {
                    string ap = AssetDatabase.GUIDToAssetPath(guid);
                    var imp = AssetImporter.GetAtPath(ap) as AudioImporter;
                    if (imp == null) continue;
                    string n = Path.GetFileNameWithoutExtension(ap);
                    bool longClip = n.StartsWith("amb_") || n.StartsWith("bgm_");
                    var set = imp.defaultSampleSettings;
                    var want = longClip ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
                    if (set.loadType != want || imp.forceToMono != true)
                    {
                        set.loadType = want; imp.defaultSampleSettings = set; imp.forceToMono = true;
                        imp.SaveAndReimport();
                    }
                }
        }

        /// <summary>기존 GameDatabase 에 새 항목·새 필드 기본값을 채운다 (사용자가 고친 값은 덮어쓰지 않음).</summary>
        public static void UpgradeDatabase(GameDatabase db)
        {
            if (db == null) return;
            var def = DefaultContent.Create();
            foreach (var e in def.enemies)
            {
                var cur = db.enemies.FirstOrDefault(x => x.id == e.id);
                if (cur == null) db.enemies.Add(e);
                else if (cur.shield <= 0 && e.shield > 0) cur.shield = e.shield;
            }
            foreach (var sk in def.skills) if (!db.skills.Any(x => x.id == sk.id)) db.skills.Add(sk);
            foreach (var it in def.items) if (!db.items.Any(x => x.id == it.id)) db.items.Add(it);
            Object.DestroyImmediate(def);
            EditorUtility.SetDirty(db);
        }

        public static void ConfigureFont()
        {
            foreach (var f in new[] { "Galmuri11.ttf", "Galmuri11-Bold.ttf" })
            {
                var imp = AssetImporter.GetAtPath(EWArtSpecs.TP + "quiple_Galmuri/" + f) as TrueTypeFontImporter;
                if (imp == null) continue;
                bool dirty = false;
                if (imp.fontRenderingMode != FontRenderingMode.HintedRaster) { imp.fontRenderingMode = FontRenderingMode.HintedRaster; dirty = true; }
                if (imp.fontTextureCase != FontTextureCase.Dynamic) { imp.fontTextureCase = FontTextureCase.Dynamic; dirty = true; }
                if (!imp.includeFontData) { imp.includeFontData = true; dirty = true; }
                if (dirty) imp.SaveAndReimport();
            }
        }
    }
}
