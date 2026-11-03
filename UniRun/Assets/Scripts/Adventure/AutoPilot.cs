using System.IO;
using UnityEngine;

// 검증용 자동 플레이. 실행 인자에 -autopilot 이 있을 때만 동작한다 (일반 플레이에는 영향 없음).
// 맵 데이터를 보고 점프하며 진행하고, 일정 간격으로 스크린샷과 로그를 남긴 뒤 종료한다.
public class AutoPilot : MonoBehaviour {
    private string outDir;
    private float nextShot = 6.5f;   // 첫 장은 오프닝 로고가 다 나온 뒤
    private int shot;
    private StreamWriter log;
    private float lastLog;
    private float holdTimer;

    private void Awake() {
        string[] args = System.Environment.GetCommandLineArgs();
        bool on = false;
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == "-autopilot") {
                on = true;
                outDir = i + 1 < args.Length ? args[i + 1] : "autopilot";
            }
        }
        if (!on) { enabled = false; return; }
        Directory.CreateDirectory(outDir);
        log = new StreamWriter(Path.Combine(outDir, "autopilot.log"));
        Application.logMessageReceived += (msg, stack, type) => {
            if (type != LogType.Log) log.WriteLine("[" + type + "] " + msg + "\n" + stack);
        };
    }

    private void Start() {
        if (!enabled) return;
        RunGame.instance.GetType().GetField("maxLives", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).SetValue(RunGame.instance, 99);
    }

    private void Update() {
        RunnerPlayer p = RunnerPlayer.instance;
        MapLoader map = MapLoader.instance;
        Vector2 pos = p.transform.position;
        if (RunGame.instance.InTitle && Time.time > 9f) RunGame.instance.RequestStart();   // 오프닝을 충분히 보여 준 뒤 시작

        // 목숨이 다 떨어지지 않도록 검증 중에는 계속 채워 준다
        if (RunGame.instance.Lives < 2) {
            typeof(RunGame).GetProperty("Lives").SetValue(RunGame.instance, 3);
        }

        float look = p.Speed * 0.28f;
        int here = map.ColumnTop(pos.x);
        int ahead = map.ColumnTop(pos.x + look);
        int aheadFar = map.ColumnTop(pos.x + look * 2.2f);
        bool danger = false;
        foreach (Collider2D c in Physics2D.OverlapBoxAll(pos + new Vector2(look + 0.8f, 0.3f), new Vector2(1.8f, 2.2f), 0f)) {
            if (c.GetComponent<Spike>() != null) danger = true;
            PatrolEnemy e = c.GetComponent<PatrolEnemy>();
            if (e != null && !e.Dead) danger = true;
        }
        if (p.Submerged) {
            if ((holdTimer -= Time.deltaTime) < 0f) { p.RequestJump(); holdTimer = 0.35f; }
        } else if (p.Grounded) {
            if (ahead < 0 || ahead > here || aheadFar > here + 1 || danger) p.RequestJump();
        } else if (p.Velocity.y < -2f && p.JumpCount < 2) {
            // 떨어지는 중에 앞에 내릴 땅이 없거나 땅이 더 높으면 이단 점프
            int below = map.ColumnTop(pos.x + 1.5f);
            if (below < 0 || pos.y + p.FeetOffset < below + 0.8f) p.RequestJump();
        }

        if (Time.time > lastLog + 1f) {
            lastLog = Time.time;
            log.WriteLine(string.Format("t={0:F1} x={1:F1} y={2:F1} vx={3:F1} vy={4:F1} g={5} sw={6} lives={7} score={8} zone={9} fps={10:F0}",
                Time.time, pos.x, pos.y, p.Velocity.x, p.Velocity.y, p.Grounded, p.Submerged,
                RunGame.instance.Lives, RunGame.instance.Score, map.ZoneIndexAt(pos.x), 1f / Mathf.Max(0.001f, Time.smoothDeltaTime)));
            log.Flush();
        }
        if (Time.time > nextShot) {
            nextShot = Time.time + 6f;
            ScreenCapture.CaptureScreenshot(Path.Combine(outDir, "shot_" + (shot++).ToString("00") + ".png"));
        }
        if (Time.time > 150f || RunGame.instance.Over && Time.time > nextShot - 4f) {
            log.WriteLine("END x=" + pos.x + " over=" + RunGame.instance.Over);
            log.Close();
            Application.Quit();
            enabled = false;
        }
    }
}
