using UnityEngine;

namespace EW
{
    /// <summary>
    /// 플레이어 설정 (저장 데이터와 별개, PlayerPrefs). 음악·환경음·효과음 볼륨, 화면 흔들림·번쩍임 강도, 글자 속도.
    /// 흔들림·번쩍임은 0 으로 끌 수 있다 (광과민 대응).
    /// </summary>
    public static class GameSettings
    {
        const string P = "ew_set_";
        static bool loaded;
        static float master = 1f, music = 0.7f, ambience = 0.8f, sfx = 0.9f, shake = 1f, flash = 1f, textSpeed = 1f, battleFast = 0f;

        static void Load()
        {
            if (loaded) return;
            loaded = true;
            master = PlayerPrefs.GetFloat(P + "master", master);
            music = PlayerPrefs.GetFloat(P + "music", music);
            ambience = PlayerPrefs.GetFloat(P + "amb", ambience);
            sfx = PlayerPrefs.GetFloat(P + "sfx", sfx);
            shake = PlayerPrefs.GetFloat(P + "shake", shake);
            flash = PlayerPrefs.GetFloat(P + "flash", flash);
            textSpeed = PlayerPrefs.GetFloat(P + "text", textSpeed);
            battleFast = PlayerPrefs.GetFloat(P + "bfast", battleFast);
        }

        static void Save(string k, float v) { PlayerPrefs.SetFloat(P + k, v); PlayerPrefs.Save(); }

        public static float Master { get { Load(); return master; } set { master = Mathf.Clamp01(value); Save("master", master); } }
        public static float Music { get { Load(); return music; } set { music = Mathf.Clamp01(value); Save("music", music); } }
        public static float Ambience { get { Load(); return ambience; } set { ambience = Mathf.Clamp01(value); Save("amb", ambience); } }
        public static float Sfx { get { Load(); return sfx; } set { sfx = Mathf.Clamp01(value); Save("sfx", sfx); } }
        /// <summary>화면 흔들림 배율 (0 = 끔).</summary>
        public static float Shake { get { Load(); return shake; } set { shake = Mathf.Clamp01(value); Save("shake", shake); } }
        /// <summary>화면 번쩍임 배율 (0 = 끔, 장면 전환 페이드는 유지).</summary>
        public static float Flash { get { Load(); return flash; } set { flash = Mathf.Clamp01(value); Save("flash", flash); } }
        /// <summary>글자 출력 속도 배율 (0.5 ~ 2).</summary>
        public static float TextSpeed { get { Load(); return textSpeed; } set { textSpeed = Mathf.Clamp(value, 0.5f, 2f); Save("text", textSpeed); } }
        /// <summary>전투 연출 빠르기 (0 = 보통, 1 = 두 배). 반복 전투의 연출을 줄인다.</summary>
        public static float BattleFast { get { Load(); return battleFast; } set { battleFast = Mathf.Clamp01(value); Save("bfast", battleFast); } }
        public static float BattleSpeed => 1f + BattleFast;
    }
}
