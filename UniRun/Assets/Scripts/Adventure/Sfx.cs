using UnityEngine;

// 효과음. jump/die 는 기존 wav 를 쓰고, 나머지는 시작할 때 코드로 합성한다.
public class Sfx : MonoBehaviour {
    public static Sfx instance;

    public AudioClip jump;
    public AudioClip die;
    [HideInInspector] public AudioClip doubleJump, beep, go, coin, stomp, hurt, oneUp, powerUp, shieldPop, splash, land, fanfare;

    private const int Rate = 44100;
    private AudioSource[] voices;
    private int next;

    private void Awake() {
        instance = this;
        voices = new AudioSource[8];
        for (int i = 0; i < voices.Length; i++) {
            voices[i] = gameObject.AddComponent<AudioSource>();
            voices[i].playOnAwake = false;
        }
        doubleJump = Tones("doubleJump", new[] { 660f, 990f }, 0.06f, Wave.Square, 0.25f);
        beep = Tones("beep", new[] { 660f }, 0.16f, Wave.Square, 0.25f);
        go = Tones("go", new[] { 880f, 1320f }, 0.14f, Wave.Square, 0.28f);
        coin = Tones("coin", new[] { 988f, 1319f }, 0.07f, Wave.Square, 0.25f);
        stomp = Sweep("stomp", 320f, 60f, 0.16f, Wave.Square, 0.5f, 0.3f);
        hurt = Sweep("hurt", 500f, 90f, 0.35f, Wave.Saw, 0.45f, 0.35f);
        oneUp = Tones("oneUp", new[] { 523f, 659f, 784f, 1047f, 1319f, 1568f }, 0.07f, Wave.Square, 0.25f);
        powerUp = Sweep("powerUp", 300f, 1200f, 0.4f, Wave.Square, 0.25f, 0f);
        shieldPop = Sweep("shieldPop", 900f, 200f, 0.2f, Wave.Triangle, 0.4f, 0.5f);
        splash = Sweep("splash", 180f, 60f, 0.3f, Wave.Noise, 0.35f, 1f);
        land = Sweep("land", 140f, 50f, 0.12f, Wave.Triangle, 0.6f, 0.4f);
        fanfare = Tones("fanfare", new[] { 523f, 523f, 784f, 1047f }, 0.11f, Wave.Square, 0.25f);
    }

    public void Play(AudioClip clip, float volume, float pitch = 1f) {
        if (clip == null) return;
        AudioSource v = voices[next];
        next = (next + 1) % voices.Length;
        v.pitch = pitch * Random.Range(0.96f, 1.04f);
        v.PlayOneShot(clip, volume);
    }

    // ---------------------------------------------------------------- 합성
    private enum Wave { Square, Triangle, Saw, Noise }

    private static float Osc(Wave w, float phase) {
        float p = phase - Mathf.Floor(phase);
        switch (w) {
            case Wave.Square: return p < 0.5f ? 1f : -1f;
            case Wave.Triangle: return 4f * Mathf.Abs(p - 0.5f) - 1f;
            case Wave.Saw: return 2f * p - 1f;
            default: return Random.Range(-1f, 1f);
        }
    }

    private static AudioClip Tones(string clipName, float[] freqs, float noteLen, Wave w, float vol) {
        int per = (int)(noteLen * Rate);
        var data = new float[per * freqs.Length + Rate / 20];
        float phase = 0f;
        for (int n = 0; n < freqs.Length; n++) {
            for (int i = 0; i < per; i++) {
                phase += freqs[n] / Rate;
                float env = 1f - (float)i / per * 0.6f;
                data[n * per + i] = Osc(w, phase) * vol * env;
            }
        }
        return Make(clipName, data);
    }

    private static AudioClip Sweep(string clipName, float f0, float f1, float len, Wave w, float vol, float noiseMix) {
        int count = (int)(len * Rate);
        var data = new float[count];
        float phase = 0f;
        for (int i = 0; i < count; i++) {
            float t = (float)i / count;
            phase += Mathf.Lerp(f0, f1, t) / Rate;
            float s = Mathf.Lerp(Osc(w, phase), Random.Range(-1f, 1f), noiseMix);
            data[i] = s * vol * (1f - t) * Mathf.Clamp01(i / 200f);
        }
        return Make(clipName, data);
    }

    private static AudioClip Make(string clipName, float[] data) {
        AudioClip c = AudioClip.Create(clipName, data.Length, 1, Rate, false);
        c.SetData(data, 0);
        return c;
    }
}
