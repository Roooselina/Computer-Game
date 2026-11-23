using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 소리 재생. 음악·환경음은 두 개의 소스를 교차 페이드하고, 효과음은 작은 풀에서 재생한다.
    /// 볼륨은 GameSettings (음악/환경음/효과음 별도). 소리 이름은 GameConfig.audio 목록에서 찾는다.
    /// 현재 효과음·환경음은 Tools/audio_gen.py 로 합성한 임시 음원이다.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }
        AudioSource[] music = new AudioSource[2];
        AudioSource[] amb = new AudioSource[2];
        readonly List<AudioSource> pool = new List<AudioSource>();
        int musicIdx, ambIdx;
        string musicName, ambName;
        float musicBase = 1f, ambBase = 1f;
        Coroutine musicFade, ambFade;
        readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        readonly Dictionary<string, float> lastPlay = new Dictionary<string, float>();
        /// <summary>테스트·확인용: 최근 재생 요청 기록 (이름).</summary>
        public static readonly List<string> History = new List<string>();

        public static AudioManager Ensure()
        {
            if (I != null) return I;
            var go = new GameObject("[Audio]");
            DontDestroyOnLoad(go);
            I = go.AddComponent<AudioManager>();
            return I;
        }

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            for (int i = 0; i < 2; i++)
            {
                music[i] = gameObject.AddComponent<AudioSource>(); music[i].loop = true; music[i].playOnAwake = false;
                amb[i] = gameObject.AddComponent<AudioSource>(); amb[i].loop = true; amb[i].playOnAwake = false;
            }
            for (int i = 0; i < 10; i++) { var s = gameObject.AddComponent<AudioSource>(); s.playOnAwake = false; pool.Add(s); }
            var cfg = GameConfig.Load();
            if (cfg != null) foreach (var a in cfg.audio) if (a != null && a.clip != null) cache[a.name] = a.clip;
        }

        public AudioClip Get(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return cache.TryGetValue(name, out var c) ? c : null;
        }

        void Update()
        {
            float m = GameSettings.Master;
            // 페이드 중이 아닌 소스의 볼륨을 설정에 맞춘다
            if (musicFade == null) for (int i = 0; i < 2; i++) music[i].volume = i == musicIdx ? musicBase * GameSettings.Music * m : 0;
            // 대사 중에는 환경음(경보음 등)을 낮춘다 — 대사를 읽고 듣기 쉽게. 같은 이름의 환경음은 맵이 바뀌어도 다시 틀지 않는다(PlayAmbience).
            var gm = Game.I;
            bool talk = gm != null && gm.UI != null && gm.UI.dialogue != null && gm.UI.dialogue.Open;
            duck = Mathf.MoveTowards(duck, talk ? 0.4f : 1f, Time.unscaledDeltaTime * 2.5f);
            if (ambFade == null) for (int i = 0; i < 2; i++) amb[i].volume = i == ambIdx ? ambBase * GameSettings.Ambience * m * duck : 0;
        }
        float duck = 1f;
        public float AmbienceDuck => duck;

        // ------------------------------------------------------------ 음악·환경음
        public void PlayMusic(string name, float fade = 1f, float volume = 1f)
        {
            if (name == musicName) { musicBase = volume; return; }
            musicName = name;
            musicBase = volume;
            if (musicFade != null) StopCoroutine(musicFade);
            musicFade = StartCoroutine(CrossFade(music, true, Get(name), fade));
        }

        public void PlayAmbience(string name, float fade = 1f, float volume = 1f)
        {
            if (name == ambName) { ambBase = volume; return; }
            ambName = name;
            ambBase = volume;
            if (ambFade != null) StopCoroutine(ambFade);
            ambFade = StartCoroutine(CrossFade(amb, false, Get(name), fade));
        }

        public string CurrentMusic => musicName;
        public string CurrentAmbience => ambName;

        IEnumerator CrossFade(AudioSource[] pair, bool isMusic, AudioClip clip, float time)
        {
            int from = isMusic ? musicIdx : ambIdx;
            int to = 1 - from;
            if (isMusic) musicIdx = to; else ambIdx = to;
            var src = pair[to];
            src.clip = clip;
            if (clip != null) { src.time = 0; src.Play(); } else src.Stop();
            float fromStart = pair[from].volume;
            for (float t = 0; t < time; t += Time.unscaledDeltaTime)
            {
                float k = t / Mathf.Max(0.01f, time);
                float target = (isMusic ? musicBase * GameSettings.Music : ambBase * GameSettings.Ambience) * GameSettings.Master;
                src.volume = Mathf.Lerp(0, target, k);
                pair[from].volume = Mathf.Lerp(fromStart, 0, k);
                yield return null;
            }
            pair[from].Stop();
            if (isMusic) musicFade = null; else ambFade = null;
        }

        // ------------------------------------------------------------ 효과음
        public void PlayOne(string name, float volume = 1f, float pitch = 1f)
        {
            var c = Get(name);
            History.Add(name);
            if (History.Count > 200) History.RemoveAt(0);
            if (c == null) return;
            // 같은 소리가 한 프레임에 여러 번 겹치지 않게
            if (lastPlay.TryGetValue(name, out var t) && Time.unscaledTime - t < 0.03f) return;
            lastPlay[name] = Time.unscaledTime;
            AudioSource s = null;
            foreach (var p in pool) if (!p.isPlaying) { s = p; break; }
            if (s == null) s = pool[0];
            s.pitch = pitch;
            s.PlayOneShot(c, Mathf.Clamp01(volume) * GameSettings.Sfx * GameSettings.Master);
        }

        public void StopAll(float fade = 0.5f)
        {
            PlayMusic(null, fade); PlayAmbience(null, fade);
        }
    }

    /// <summary>효과음 바로가기. 메뉴·대화용 짧은 이름도 받는다.</summary>
    public static class Sfx
    {
        public static void Play(string id, float volume = 1f, float pitch = 1f)
        {
            string n = id;
            switch (id)
            {
                case "move": n = "sfx_ui_move"; break;
                case "ok": n = "sfx_ui_ok"; break;
                case "cancel": n = "sfx_ui_cancel"; break;
                case "no": n = "sfx_ui_no"; break;
                case "text": n = "sfx_advance"; break;
                default: if (!id.StartsWith("sfx_")) n = "sfx_" + id; break;
            }
            var a = AudioManager.I;
            if (a != null) a.PlayOne(n, volume, pitch);
        }
    }
}
