using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 목숨(하트), 점수, 진행도 바, 구역 배너, 화면 번쩍임, 결과 화면. UI 는 전부 코드로 만든다.
public class RunHud : MonoBehaviour {
    public static RunHud instance;

    public Font font;
    public Sprite heartSprite;
    public Sprite magnetSprite;
    public Sprite pixel;

    private Image[] hearts;
    private Text scoreText;
    private Text distanceText;
    private Text countText;
    private float countTime = -10f;
    private RectTransform progressFill;
    private RectTransform progressMarker;
    private Text bannerTitle;
    private Text bannerSub;
    private CanvasGroup banner;
    private Image flash;
    private Image fade;
    private CanvasGroup endPanel;
    private Text endTitle;
    private Text endBody;
    private Image magnetIcon;
    private float magnetUntil;
    private float scorePulse;
    private float livesPulse;
    private float bannerTime = -10f;
    private int shownScore;
    private CanvasGroup playGroup;        // 플레이 중에만 보이는 HUD (타이틀 화면에서는 숨김)
    private CanvasGroup titleGroup;
    private Text[] titleLetters;
    private Text titleSub;
    private Text pressText;
    private RectTransform barTop, barBottom; // 시네마틱 레터박스
    private bool titleOn;
    private float titleTime;

    private void Awake() {
        instance = this;
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Transform play = Stretch("Play HUD", transform);
        playGroup = play.gameObject.AddComponent<CanvasGroup>();
        playGroup.alpha = 0f;

        hearts = new Image[5];
        for (int i = 0; i < hearts.Length; i++) {
            hearts[i] = NewImage("Heart" + i, heartSprite, new Vector2(0, 1), new Vector2(60 + i * 78, -60), new Vector2(68, 68), play);
            hearts[i].preserveAspect = true;
        }
        magnetIcon = NewImage("Magnet", magnetSprite, new Vector2(0, 1), new Vector2(60, -140), new Vector2(56, 56), play);
        scoreText = NewText("Score", new Vector2(1, 1), new Vector2(-40, -50), 56, TextAnchor.UpperRight, new Vector2(700, 80), play);
        distanceText = NewText("Distance", new Vector2(1, 1), new Vector2(-40, -112), 34, TextAnchor.UpperRight, new Vector2(700, 60), play);
        distanceText.color = new Color(1, 1, 1, 0.8f);

        // 진행도 바 (구역 4칸 + 현재 위치)
        Image bar = NewImage("Progress", pixel, new Vector2(0.5f, 1), new Vector2(0, -44), new Vector2(720, 14), play);
        bar.color = new Color(0, 0, 0, 0.35f);
        progressFill = NewImage("Fill", pixel, new Vector2(0.5f, 1), new Vector2(-360, -44), new Vector2(0, 14), play).rectTransform;
        progressFill.pivot = new Vector2(0, 0.5f);
        progressFill.GetComponent<Image>().color = new Color(1f, 0.85f, 0.3f, 0.9f);
        progressMarker = NewImage("Marker", heartSprite, new Vector2(0.5f, 1), new Vector2(-360, -44), new Vector2(34, 34), play).rectTransform;
        progressMarker.GetComponent<Image>().color = Color.white;

        // 구역 배너
        var bannerGo = new GameObject("Banner", typeof(RectTransform));
        bannerGo.transform.SetParent(transform, false);
        banner = bannerGo.AddComponent<CanvasGroup>();
        banner.alpha = 0f;
        bannerSub = NewText("Sub", new Vector2(0.5f, 0.5f), new Vector2(0, 230), 40, TextAnchor.MiddleCenter, new Vector2(1200, 60), bannerGo.transform);
        bannerTitle = NewText("Title", new Vector2(0.5f, 0.5f), new Vector2(0, 160), 110, TextAnchor.MiddleCenter, new Vector2(1600, 140), bannerGo.transform);

        countText = NewText("Count", new Vector2(0.5f, 0.5f), new Vector2(0, -90), 220, TextAnchor.MiddleCenter, new Vector2(1000, 300));
        countText.color = new Color(1, 1, 1, 0);

        flash = NewImage("Flash", pixel, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000, 4000));
        flash.color = new Color(1, 1, 1, 0);
        flash.raycastTarget = false;
        BuildTitle();

        fade = NewImage("Fade", pixel, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000, 4000));
        fade.color = Color.black;   // 첫 프레임부터 검은 화면에서 서서히 밝아진다 (멈춘 화면이 보이지 않게)

        var endGo = new GameObject("End", typeof(RectTransform));
        endGo.transform.SetParent(transform, false);
        endPanel = endGo.AddComponent<CanvasGroup>();
        endPanel.alpha = 0f;
        Image dim = NewImage("Dim", pixel, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000, 4000), endGo.transform);
        dim.color = new Color(0, 0, 0, 0.55f);
        endTitle = NewText("EndTitle", new Vector2(0.5f, 0.5f), new Vector2(0, 120), 130, TextAnchor.MiddleCenter, new Vector2(1600, 170), endGo.transform);
        endBody = NewText("EndBody", new Vector2(0.5f, 0.5f), new Vector2(0, -60), 46, TextAnchor.MiddleCenter, new Vector2(1600, 260), endGo.transform);
    }

    private Transform Stretch(string objName, Transform parent) {
        var go = new GameObject(objName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    // ---------------------------------------------------------------- 타이틀 (오프닝)
    private void BuildTitle() {
        barTop = NewImage("Letterbox Top", pixel, new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(4000, 130)).rectTransform;
        barTop.pivot = new Vector2(0.5f, 1);
        barTop.GetComponent<Image>().color = Color.black;
        barBottom = NewImage("Letterbox Bottom", pixel, new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(4000, 130)).rectTransform;
        barBottom.pivot = new Vector2(0.5f, 0);
        barBottom.GetComponent<Image>().color = Color.black;

        Transform t = Stretch("Title", transform);
        titleGroup = t.gameObject.AddComponent<CanvasGroup>();
        const string word = "UNIRUN";
        titleLetters = new Text[word.Length];
        Color[] colors = { new Color(1f, .45f, .45f), new Color(1f, .78f, .3f), new Color(.5f, .9f, .45f), new Color(.45f, .8f, 1f), new Color(.75f, .6f, 1f), new Color(1f, .55f, .8f) };
        for (int i = 0; i < word.Length; i++) {
            titleLetters[i] = NewText("Letter" + i, new Vector2(0.5f, 0.5f), new Vector2((i - (word.Length - 1) * 0.5f) * 150f, 170), 200, TextAnchor.MiddleCenter, new Vector2(200, 240), t);
            titleLetters[i].text = word[i].ToString();
            titleLetters[i].color = colors[i % colors.Length];
            titleLetters[i].GetComponent<Shadow>().effectDistance = new Vector2(8, -8);
        }
        titleSub = NewText("Subtitle", new Vector2(0.5f, 0.5f), new Vector2(0, 30), 64, TextAnchor.MiddleCenter, new Vector2(1400, 90), t);
        titleSub.text = "ADVENTURE";
        pressText = NewText("Press", new Vector2(0.5f, 0.5f), new Vector2(0, -230), 48, TextAnchor.MiddleCenter, new Vector2(1400, 80), t);
        pressText.text = "PRESS SPACE OR CLICK TO START";
        Text hint = NewText("Hint", new Vector2(0.5f, 0), new Vector2(0, 160), 30, TextAnchor.MiddleCenter, new Vector2(1600, 50), t);
        hint.text = "JUMP  x2  DOUBLE JUMP     STOMP ENEMIES     DIVE INTO WATER";
        hint.color = new Color(1, 1, 1, 0.75f);
    }

    public void ShowTitle(bool on) {
        titleOn = on;
        titleTime = Time.unscaledTime;
    }

    public void ShowPlayHud() { playGroup.alpha = 0.01f; }

    private void UpdateTitle(float dt) {
        float since = Time.unscaledTime - titleTime;
        float target = titleOn ? 1f : 0f;
        titleGroup.alpha = Mathf.MoveTowards(titleGroup.alpha, target, dt * (titleOn ? 1.5f : 3f));
        // 레터박스는 타이틀 동안 내려와 있다가 시작하면 위아래로 빠진다
        float bar = Mathf.Lerp(barTop.anchoredPosition.y, titleOn ? 0f : 140f, dt * 5f);
        barTop.anchoredPosition = new Vector2(0, bar);
        barBottom.anchoredPosition = new Vector2(0, -bar);
        if (titleGroup.alpha <= 0f) return;
        for (int i = 0; i < titleLetters.Length; i++) {
            // 글자가 하나씩 떨어져 내려와 통통 튀고, 이후 파도처럼 출렁인다
            float k = Mathf.Clamp01((since - i * 0.12f) / 0.5f);
            float drop = titleOn ? (1f - k) * 600f * (1f - k) : -since * 1500f * (i % 2 == 0 ? 1 : -1);
            float wave = Mathf.Sin(Time.unscaledTime * 3f + i * 0.7f) * 14f;
            float bounce = titleOn ? Mathf.Sin(k * Mathf.PI) * 40f * (1f - k) : 0f;
            titleLetters[i].rectTransform.anchoredPosition = new Vector2((i - (titleLetters.Length - 1) * 0.5f) * 150f, 170 + drop + wave + bounce);
            titleLetters[i].rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.unscaledTime * 2f + i) * 6f);
        }
        float subK = Mathf.Clamp01((since - 0.9f) / 0.4f);
        titleSub.rectTransform.localScale = Vector3.one * (titleOn ? Mathf.Lerp(0f, 1f, subK) + Mathf.Sin(Time.unscaledTime * 4f) * 0.03f : 1f);
        pressText.color = new Color(1, 1, 1, since < 1.4f && titleOn ? 0f : 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 5f));
    }

    private Image NewImage(string objName, Sprite s, Vector2 anchor, Vector2 pos, Vector2 size, Transform parent = null) {
        var go = new GameObject(objName, typeof(RectTransform));
        go.transform.SetParent(parent != null ? parent : transform, false);
        var img = go.AddComponent<Image>();
        img.sprite = s;
        img.raycastTarget = false;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return img;
    }

    private Text NewText(string objName, Vector2 anchor, Vector2 pos, int size, TextAnchor align, Vector2 box, Transform parent = null) {
        var go = new GameObject(objName, typeof(RectTransform));
        go.transform.SetParent(parent != null ? parent : transform, false);
        var t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.alignment = align;
        t.color = Color.white;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;   // 큰 글자가 상자보다 커도 잘려 사라지지 않게
        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(3, -3);
        RectTransform rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(anchor.x, anchor.y);
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;
        return t;
    }

    private void Update() {
        RunGame g = RunGame.instance;
        RunnerPlayer p = RunnerPlayer.instance;
        float dt = Time.unscaledDeltaTime;
        UpdateTitle(dt);
        if (playGroup.alpha > 0f && playGroup.alpha < 1f) playGroup.alpha = Mathf.MoveTowards(playGroup.alpha, 1f, dt * 2f);

        livesPulse = Mathf.MoveTowards(livesPulse, 0f, dt * 3f);
        for (int i = 0; i < hearts.Length; i++) {
            bool on = i < g.Lives;
            hearts[i].color = on ? Color.white : new Color(0, 0, 0, 0.25f);
            float bob = on ? Mathf.Sin(Time.unscaledTime * 4f + i * 0.6f) * 3f : 0f;
            hearts[i].rectTransform.anchoredPosition = new Vector2(60 + i * 78, -60 + bob);
            hearts[i].rectTransform.localScale = Vector3.one * (1f + livesPulse * 0.5f);
        }

        // 점수는 숫자가 올라가는 애니메이션
        shownScore = (int)Mathf.MoveTowards(shownScore, g.Score, Mathf.Max(1f, (g.Score - shownScore) * 10f * dt));
        scoreText.text = shownScore.ToString("N0");
        scorePulse = Mathf.MoveTowards(scorePulse, 0f, dt * 4f);
        scoreText.rectTransform.localScale = Vector3.one * (1f + scorePulse * 0.25f);
        distanceText.text = Mathf.FloorToInt(p.transform.position.x) + " m   " + g.Gems + " GEMS";

        float progress = MapLoader.instance.Progress(p.transform.position.x);
        progressFill.sizeDelta = new Vector2(720 * progress, 14);
        progressMarker.anchoredPosition = new Vector2(-360 + 720 * progress, -44);

        magnetIcon.enabled = Time.time < magnetUntil && (magnetUntil - Time.time > 2f || Mathf.Repeat(Time.time, 0.3f) < 0.15f);

        float bt = Time.unscaledTime - bannerTime;
        banner.alpha = bt < 0.3f ? bt / 0.3f : bt < 2.2f ? 1f : Mathf.Clamp01(1f - (bt - 2.2f) / 0.6f);
        bannerTitle.rectTransform.localScale = Vector3.one * (bt < 0.3f ? Mathf.Lerp(1.6f, 1f, bt / 0.3f) : 1f);

        // 카운트다운 숫자: 크게 튀어나왔다가 줄어들며 사라짐
        float ct = Time.unscaledTime - countTime;
        Color cc = countText.color;
        cc.a = ct < 0.7f ? 1f : Mathf.Clamp01(1f - (ct - 0.7f) * 4f);
        countText.color = cc;
        countText.rectTransform.localScale = Vector3.one * (ct < 0.12f ? Mathf.Lerp(2f, 1f, ct / 0.12f) : Mathf.Lerp(1f, 0.85f, Mathf.Clamp01((ct - 0.12f) / 0.7f)));

        Color f = flash.color;
        f.a = Mathf.MoveTowards(f.a, 0f, dt * 1.6f);
        flash.color = f;
        if (endPanel.alpha > 0f && endPanel.alpha < 1f) endPanel.alpha = Mathf.MoveTowards(endPanel.alpha, 1f, dt * 2f);
    }

    public void ShowCount(string text, Color c) {
        countText.text = text;
        countText.color = c;
        countTime = Time.unscaledTime;
    }

    public void PulseScore() { scorePulse = 1f; }
    public void PulseLives() { livesPulse = 1f; }
    public void Flash(Color c) { flash.color = c; }
    public void ShowMagnet(float seconds) { magnetUntil = Time.time + seconds; }

    public void ShowZone(int number, string zoneName) {
        bannerSub.text = "ZONE " + number;
        bannerTitle.text = zoneName;
        bannerTime = Time.unscaledTime;
    }

    public IEnumerator FadeOut(float seconds) {
        for (float t = 0; t < seconds; t += Time.unscaledDeltaTime) {
            fade.color = new Color(0, 0, 0, t / seconds);
            yield return null;
        }
        fade.color = Color.black;
    }

    public IEnumerator FadeIn(float seconds) {
        for (float t = 0; t < seconds; t += Time.unscaledDeltaTime) {
            fade.color = new Color(0, 0, 0, 1f - t / seconds);
            yield return null;
        }
        fade.color = new Color(0, 0, 0, 0);
    }

    public void ShowEnd(bool clear, int score, float distance) {
        endTitle.text = clear ? "CLEAR!" : "GAME OVER";
        endTitle.color = clear ? new Color(1f, 0.9f, 0.4f) : new Color(1f, 0.45f, 0.45f);
        endBody.text = "SCORE  " + score.ToString("N0") + "\nDISTANCE  " + Mathf.FloorToInt(distance) + " m\n\nJUMP TO RESTART";
        endPanel.alpha = 0.01f;
    }
}
