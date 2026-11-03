using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// 목숨, 점수, 피격/부활, 체크포인트, 게임오버/클리어를 관리하는 게임 매니저
public class RunGame : MonoBehaviour {
    public static RunGame instance;

    public InputActionAsset actions;
    public int startLives = 3;
    public float loadSettleTime = 0.8f;   // 씬 로딩 직후 프레임이 튀는 동안은 카운트다운을 시작하지 않는다
    public int maxLives = 5;

    public int Lives { get; private set; }
    public int Score { get; private set; }
    public int Gems { get; private set; }
    public bool Frozen { get; private set; }
    public bool Over { get; private set; }
    public bool Started { get; private set; }
    public bool InTitle { get; private set; }

    private bool startRequested;
    public void RequestStart() { startRequested = true; }   // 자동 플레이 검증용
    private static bool skipTitle;   // 재시작할 때는 오프닝을 건너뛰고 바로 카운트다운

    private InputAction jumpAction;
    private bool respawning;
    private float lastHurt;
    private int zoneShown = -1;
    private float overTime;
    private float magnetSeconds = 10f;

    private void Awake() {
        instance = this;
        Lives = startLives;
        jumpAction = actions.FindAction("Player/Jump", true);
        Frozen = true;
    }

    private void Start() {
        StartCoroutine(Opening());
    }

    // 오프닝: 검은 화면에서 밝아지며 카메라가 맵을 훑고, 타이틀 로고가 떨어진다. 입력하면 카운트다운.
    private IEnumerator Opening() {
        RunnerPlayer.instance.Hold(true);
        if (!skipTitle) {
            InTitle = true;
            Frozen = false;   // 타이틀 동안 적은 움직인다 (플레이어는 멈춤, 피격 없음)
            RunCamera.instance.BeginFlyover();
            RunHud.instance.ShowTitle(true);
            MusicDirector.instance.Begin();
            yield return null;
            yield return new WaitForSecondsRealtime(loadSettleTime);
            yield return RunHud.instance.FadeIn(1.2f);
            float shown = Time.unscaledTime;
            while (!((jumpAction.WasPressedThisFrame() || startRequested) && Time.unscaledTime > shown + 0.8f)) yield return null;
            Sfx.instance.Play(Sfx.instance.go, 0.8f);
            RunHud.instance.ShowTitle(false);
            RunCamera.instance.EndFlyover(1.4f);
            RunCamera.instance.Shake(0.15f);
            InTitle = false;
            Frozen = true;
            yield return new WaitForSecondsRealtime(1.5f);
        } else {
            MusicDirector.instance.Begin();
            yield return null;
            yield return new WaitForSecondsRealtime(0.3f);
            yield return RunHud.instance.FadeIn(0.5f);
        }
        RunHud.instance.ShowPlayHud();
        yield return Countdown();
    }

    // 3, 2, 1, GO! 가 끝날 때까지 플레이어와 적이 멈춰 있는다
    private IEnumerator Countdown() {
        Frozen = true;
        for (int i = 3; i >= 1; i--) {
            RunHud.instance.ShowCount(i.ToString(), Color.white);
            Sfx.instance.Play(Sfx.instance.beep, 0.7f);
            RunCamera.instance.Shake(0.08f);
            yield return new WaitForSecondsRealtime(0.8f);
        }
        RunHud.instance.ShowCount("GO!", new Color(1f, 0.9f, 0.35f));
        Sfx.instance.Play(Sfx.instance.go, 0.8f);
        RunCamera.instance.Shake(0.2f);
        RunCamera.instance.Punch(0.5f);
        Fx.instance.Dust((Vector2)RunnerPlayer.instance.transform.position + Vector2.up * RunnerPlayer.instance.FeetOffset, 10, 4f);
        RunnerPlayer.instance.Hold(false);
        Frozen = false;
        Started = true;
    }

    private void Update() {
        RunnerPlayer p = RunnerPlayer.instance;
        if (Over) {
            if (Time.unscaledTime > overTime + 1.2f && jumpAction.WasPressedThisFrame()) {
                Time.timeScale = 1f;
                skipTitle = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }
        if (!Started) return;
        int zone = MapLoader.instance.ZoneIndexAt(p.transform.position.x);
        if (zone != zoneShown) {
            if (zoneShown >= 0) {
                Sfx.instance.Play(Sfx.instance.fanfare, 0.8f);
                RunCamera.instance.Punch(0.6f);
            }
            zoneShown = zone;
            RunHud.instance.ShowZone(zone + 1, MapLoader.instance.Data.zones[zone].name);
        }
    }

    // ---------------------------------------------------------------- 아이템
    public void OnPickup(Pickup item) {
        Vector2 pos = item.transform.position;
        switch (item.kind) {
            case Pickup.Kind.Gem:
                Gems++;
                AddScore(10, pos, false);
                Fx.instance.Sparkle(pos, new Color(1f, 1f, 0.7f), 6);
                Sfx.instance.Play(Sfx.instance.coin, 0.45f, 1f + Mathf.Repeat(Gems, 8) * 0.04f);
                break;
            case Pickup.Kind.Heart:
                if (Lives < maxLives) {
                    Lives++;
                    Fx.instance.Popup("+1 LIFE", pos + Vector2.up, new Color(1f, 0.4f, 0.5f), 1.4f);
                } else {
                    AddScore(500, pos, true);
                }
                Fx.instance.Burst(pos, new Color(1f, 0.35f, 0.5f), 40, 9f, 0.3f, 0.9f, -4f);
                Fx.instance.Ring(pos, new Color(1f, 0.6f, 0.7f));
                RunHud.instance.Flash(new Color(1f, 0.4f, 0.55f, 0.35f));
                RunHud.instance.PulseLives();
                RunCamera.instance.Shake(0.25f);
                RunCamera.instance.Punch(-0.5f);
                StartCoroutine(SlowMo(0.35f, 0.4f));
                Sfx.instance.Play(Sfx.instance.oneUp, 0.9f);
                break;
            case Pickup.Kind.Shield:
                RunnerPlayer.instance.GiveShield();
                Fx.instance.Popup("SHIELD", pos + Vector2.up, new Color(0.5f, 0.9f, 1f), 1.2f);
                Fx.instance.Burst(pos, new Color(0.5f, 0.9f, 1f), 30, 7f, 0.25f, 0.7f, 0f);
                RunHud.instance.Flash(new Color(0.5f, 0.85f, 1f, 0.25f));
                RunCamera.instance.Shake(0.15f);
                Sfx.instance.Play(Sfx.instance.powerUp, 0.8f);
                break;
            case Pickup.Kind.Magnet:
                RunnerPlayer.instance.GiveMagnet(magnetSeconds);
                RunHud.instance.ShowMagnet(magnetSeconds);
                Fx.instance.Popup("MAGNET!", pos + Vector2.up, new Color(1f, 0.85f, 0.3f), 1.2f);
                Fx.instance.Burst(pos, new Color(1f, 0.8f, 0.3f), 30, 7f, 0.25f, 0.7f, 0f);
                RunCamera.instance.Shake(0.15f);
                Sfx.instance.Play(Sfx.instance.powerUp, 0.8f, 1.15f);
                break;
        }
    }

    public void AddScore(int amount, Vector2 pos, bool popup) {
        Score += amount;
        if (popup) Fx.instance.Popup("+" + amount, pos + Vector2.up * 0.6f, Color.white, 0.9f);
        RunHud.instance.PulseScore();
    }

    // ---------------------------------------------------------------- 적
    public void OnEnemyContact(PatrolEnemy enemy) {
        RunnerPlayer p = RunnerPlayer.instance;
        if (Over || respawning) return;
        float feet = p.transform.position.y + p.FeetOffset;
        float enemyTop = enemy.GetComponent<Collider2D>().bounds.max.y;
        bool fromAbove = p.Velocity.y < 1f && feet > enemyTop - 0.35f;
        if (enemy.Stompable && fromAbove) {
            enemy.Kill(p.transform.position);
            p.StompBounce();
            AddScore(50, enemy.transform.position, true);
            Fx.instance.Burst(enemy.transform.position, new Color(1f, 1f, 1f, 0.9f), 14, 6f, 0.2f, 0.4f, 0f);
            Fx.instance.Ring(enemy.transform.position, Color.white);
            RunCamera.instance.Shake(0.3f);
            Sfx.instance.Play(Sfx.instance.stomp, 0.9f);
            StartCoroutine(HitStop(0.06f));
        } else {
            HurtPlayer(enemy.transform.position, false);
        }
    }

    // ---------------------------------------------------------------- 피격
    public void HurtPlayer(Vector2 source, bool fluid) {
        RunnerPlayer p = RunnerPlayer.instance;
        if (Over || respawning || !Started) return;
        if (!fluid && p.Invincible) return;
        if (Time.time - lastHurt < 0.1f) return;
        lastHurt = Time.time;

        if (!fluid && p.HasShield) {
            p.PopShield();
            Fx.instance.Popup("BLOCK!", (Vector2)p.transform.position + Vector2.up, new Color(0.6f, 0.9f, 1f), 1f);
            RunCamera.instance.Shake(0.35f);
            Sfx.instance.Play(Sfx.instance.shieldPop, 0.9f);
            StartCoroutine(HitStop(0.08f));
            return;
        }

        Lives--;
        RunHud.instance.PulseLives();
        RunHud.instance.Flash(new Color(1f, 0.1f, 0.1f, 0.45f));
        RunCamera.instance.Shake(0.7f);
        Sfx.instance.Play(Sfx.instance.hurt, 1f);
        Fx.instance.Burst(p.transform.position, new Color(1f, 0.35f, 0.3f), 22, 8f, 0.22f, 0.6f, -10f);

        if (Lives <= 0) {
            StartCoroutine(GameOver());
        } else if (fluid) {
            StartCoroutine(Respawn());
        } else {
            p.Knockback();
            StartCoroutine(HitStop(0.12f));
        }
    }

    private IEnumerator Respawn() {
        respawning = true;
        RunnerPlayer p = RunnerPlayer.instance;
        p.SetControl(false);
        Frozen = true;
        float deathX = p.transform.position.x;
        yield return HitStop(0.15f);
        yield return RunHud.instance.FadeOut(0.25f);
        // 낭떠러지 바로 앞이 아니라 한참 뒤에서, 앞으로 달릴 거리가 있는 곳에 부활
        p.Teleport(p.RespawnPoint(deathX, 4f, 6));
        p.Hold(true);
        RunCamera.instance.Snap();
        yield return RunHud.instance.FadeIn(0.3f);
        Fx.instance.Ring(p.transform.position, Color.white);
        RunHud.instance.ShowCount("READY", Color.white);
        Sfx.instance.Play(Sfx.instance.beep, 0.6f);
        yield return new WaitForSecondsRealtime(1.0f);
        RunHud.instance.ShowCount("GO!", new Color(1f, 0.9f, 0.35f));
        Sfx.instance.Play(Sfx.instance.go, 0.7f);
        p.Hold(false);
        p.MakeInvincible(2f);
        Frozen = false;
        respawning = false;
    }

    private IEnumerator GameOver() {
        Over = true;
        RunnerPlayer.instance.Die();
        Sfx.instance.Play(Sfx.instance.die, 1f);
        MusicDirector.instance.Stop();
        Time.timeScale = 0.3f;
        yield return new WaitForSecondsRealtime(0.8f);
        Time.timeScale = 1f;
        overTime = Time.unscaledTime;
        RunHud.instance.ShowEnd(false, Score, RunnerPlayer.instance.transform.position.x);
    }

    // ---------------------------------------------------------------- 체크포인트 / 결승
    public void OnCheckpoint(Vector2 pos) {
        Fx.instance.Burst(pos + Vector2.up * 1.5f, new Color(0.5f, 1f, 0.6f), 30, 8f, 0.25f, 0.8f, -6f);
        Fx.instance.Popup("CHECKPOINT", pos + Vector2.up * 2.5f, new Color(0.6f, 1f, 0.7f), 1.2f);
        AddScore(200, pos, false);
    }

    public void OnGoal(Vector2 pos) {
        if (Over) return;
        Over = true;
        RunnerPlayer.instance.SetControl(false);
        MusicDirector.instance.Stop();
        Sfx.instance.Play(Sfx.instance.fanfare, 1f);
        StartCoroutine(Confetti(pos));
        AddScore(1000 + Lives * 500, pos, true);
        overTime = Time.unscaledTime;
        RunHud.instance.ShowEnd(true, Score, pos.x);
    }

    private IEnumerator Confetti(Vector2 pos) {
        Color[] colors = { new Color(1f, .35f, .4f), new Color(1f, .85f, .3f), new Color(.4f, .9f, .5f), new Color(.4f, .7f, 1f) };
        for (int i = 0; i < 12; i++) {
            Fx.instance.Burst(pos + new Vector2(Random.Range(-4f, 4f), Random.Range(1f, 5f)), colors[i % 4], 25, 9f, 0.22f, 1.4f, -8f);
            RunCamera.instance.Shake(0.12f);
            yield return new WaitForSeconds(0.18f);
        }
    }

    // ---------------------------------------------------------------- 연출
    private IEnumerator HitStop(float seconds) {
        Time.timeScale = 0.02f;
        yield return new WaitForSecondsRealtime(seconds);
        if (!Over) Time.timeScale = 1f;
    }

    private IEnumerator SlowMo(float seconds, float scale) {
        Time.timeScale = scale;
        yield return new WaitForSecondsRealtime(seconds);
        if (!Over) Time.timeScale = 1f;
    }
}
