using System;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    [Serializable]
    public class SpriteSet
    {
        public string name;
        public Sprite[] frames;
        public float fps = 12f;
        public bool loop;
    }

    [Serializable]
    public class AudioEntry
    {
        public string name;
        public AudioClip clip;
    }

    [Serializable]
    public class PortraitEntry
    {
        public string id;          // <인물>_<표정>  예: lu_sad
        public Sprite sprite;
    }

    [Serializable]
    public class MapEntry
    {
        public string id;          // M00 ...
        public string scene;       // 씬 이름
        public string title;       // 화면 표시용
    }

    /// <summary>
    /// 게임 전체 참조 모음. Resources/EW/GameConfig.asset 하나만 두며 에디터 구성 도구가 채운다.
    /// 씬 오브젝트에 Inspector 연결을 흩어 두지 않기 위해 여기에서 모든 공용 자산을 찾는다.
    /// </summary>
    [CreateAssetMenu(menuName = "EW/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public const string ResourcePath = "EW/GameConfig";

        [Header("데이터")]
        public GameDatabase database;
        public TextAsset dialogueJson;

        [Header("UI")]
        public Font font;
        public Font fontBold;
        [Tooltip("본문 글꼴 (Pretendard Medium)")] public Font fontBody;
        [Tooltip("본문 굵은 글꼴 (Pretendard SemiBold)")] public Font fontBodyBold;
        public Sprite panel;
        public Sprite panelInlay;
        public Sprite selectFrame;
        public Sprite pixel;
        [Tooltip("일반(비개발) 빌드에서도 Esc 메뉴에 검수 도구를 보인다")] public bool reviewToolsInRelease = true;
        public Material lineMaterial;
        public AudioClip titleBgm;
        [Tooltip("빛 표현용 가산 혼합 재질")] public Material additive;

        [Header("소리 (이름으로 찾는다)")]
        public List<AudioEntry> audio = new List<AudioEntry>();

        [Header("스탠딩 일러스트 (교체 지점: Assets/Game/Art/Portraits/<인물>_<표정>.png)")]
        public List<PortraitEntry> portraits = new List<PortraitEntry>();

        [Header("캐릭터 그래픽 (교체 지점)")]
        public List<CharacterArt> characters = new List<CharacterArt>();

        [Header("효과·아이콘")]
        public List<SpriteSet> spriteSets = new List<SpriteSet>();

        [Header("맵")]
        public string titleScene = "Title";
        public List<MapEntry> maps = new List<MapEntry>();

        [Header("시연판 설정")]
        [Tooltip("맵 이동 때도 자동 저장한다 (GDD 저장 지점 외 편의 기능).")]
        public bool autosaveOnMapEnter = true;

        public CharacterArt GetArt(string id) { foreach (var c in characters) if (c != null && c.id == id) return c; return null; }
        public SpriteSet GetSet(string name) { foreach (var s in spriteSets) if (s != null && s.name == name) return s; return null; }
        public MapEntry GetMap(string id) { foreach (var m in maps) if (m.id == id) return m; return null; }
        public Sprite GetPortrait(string character, string expr)
        {
            string key = character + "_" + expr;
            foreach (var p in portraits) if (p != null && p.id == key) return p.sprite;
            return null;
        }

        static GameConfig cached;
        public static GameConfig Load()
        {
            if (cached == null) cached = Resources.Load<GameConfig>(ResourcePath);
            return cached;
        }
    }
}
