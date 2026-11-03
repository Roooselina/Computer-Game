using UnityEngine;

// 배경 음악. 멀리 갈수록 (= 달리기 속도가 빨라질수록) 음악도 빨라진다.
public class MusicDirector : MonoBehaviour {
    public static MusicDirector instance;

    public float startPitch = 1f;
    public float endPitch = 1.3f;
    public float underwaterCutoff = 900f;

    private AudioSource source;
    private AudioLowPassFilter lowPass;
    private bool stopped;

    private void Awake() {
        instance = this;
        source = GetComponent<AudioSource>();
        lowPass = gameObject.AddComponent<AudioLowPassFilter>();
        lowPass.cutoffFrequency = 22000f;
    }

    private void Start() {
        source.Stop();   // 카운트다운이 끝나면 Begin() 으로 시작
    }

    public void Begin() {
        source.Play();
    }

    private void Update() {
        RunnerPlayer p = RunnerPlayer.instance;
        if (stopped) {
            source.pitch = Mathf.MoveTowards(source.pitch, 0.4f, Time.unscaledDeltaTime * 0.8f);
            source.volume = Mathf.MoveTowards(source.volume, 0f, Time.unscaledDeltaTime * 0.6f);
            return;
        }
        float progress = MapLoader.instance.Progress(p.transform.position.x);
        float target = Mathf.Lerp(startPitch, endPitch, progress);
        // 히트스톱/슬로모션 중에는 음악도 같이 느려진다
        target *= Mathf.Lerp(0.6f, 1f, Time.timeScale);
        source.pitch = Mathf.MoveTowards(source.pitch, target, Time.unscaledDeltaTime * 0.5f);
        // 물속에서는 먹먹하게
        lowPass.cutoffFrequency = Mathf.Lerp(lowPass.cutoffFrequency, p.Submerged ? underwaterCutoff : 22000f, Time.unscaledDeltaTime * 6f);
    }

    public void Stop() { stopped = true; }
}
