using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EW
{
    /// <summary>
    /// 게임 전체를 관리하는 단일 오브젝트. 씬 전환·이벤트 실행·입력 차단·저장을 맡는다.
    /// 어떤 씬에서 Play 를 눌러도 Resources/EW/GameConfig 를 읽어 스스로 생성된다.
    /// </summary>
    public class Game : MonoBehaviour
    {
        public static Game I { get; private set; }

        public GameConfig Config;
        public GameDatabase Db;
        [System.NonSerialized] public DialogueDB Dialogue;
        [System.NonSerialized] public GameState State;
        public UIRoot UI;
        public Bubbles Bubbles;

        public PlayerController Player;
        public Companion Companion;
        public MapController Map;
        public string CurrentMapId => Map != null ? Map.mapId : null;
        /// <summary>맵을 불러올 때마다 늘어난다 (같은 맵 재진입 구분).</summary>
        public int MapLoadCount { get; private set; }

        readonly HashSet<string> blockers = new HashSet<string>();
        bool loading;
        bool eventRunning;
        string pendingSpawn;
        bool downHandling;
        float broadcastT;
        public int LastChoice { get; private set; }
        public SaveData LastCheckpoint { get; private set; }
        public bool InEvent => eventRunning;
        public bool Loading => loading;

        /// <summary>메뉴·대화·전투·연출 중에는 필드 입력을 받지 않는다.</summary>
        public bool FieldInputAllowed => blockers.Count == 0 && !loading && !eventRunning && State != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (I != null) return;
            var cfg = GameConfig.Load();
            if (cfg == null) { Debug.LogWarning("[EW] Resources/EW/GameConfig 가 없습니다. 메뉴 EW > 1. 전체 구성 을 먼저 실행하세요."); return; }
            bool isGameScene = SceneManager.GetActiveScene().name == cfg.titleScene || Object.FindAnyObjectByType<MapController>() != null;
            if (!isGameScene) return;
            Ensure();
        }

        /// <summary>게임 관리자가 없으면 만든다 (타이틀·맵 씬이 시작할 때 호출).</summary>
        public static Game Ensure()
        {
            if (I != null) return I;
            if (GameConfig.Load() == null) return null;
            var go = new GameObject("[Game]");
            DontDestroyOnLoad(go);
            return go.AddComponent<Game>();
        }

        void OnDestroy() { if (I == this) I = null; }

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            Cutscene.ResetState();          // 이전 게임 오브젝트가 연출 도중 사라졌을 때 남은 '연출 중' 상태 정리
            GameInput.ClearContexts();      // 같은 이유로 남은 입력 상태(대화·메뉴) 정리 — 플레이 모드 도메인 리로드가 꺼져 있다
            Game.ChoiceOpen = false;
            Config = GameConfig.Load();
            Db = Config.database;
            Dialogue = DialogueDB.Parse(Config.dialogueJson != null ? Config.dialogueJson.text : "{}");
            foreach (var e in Dialogue.errors) Debug.LogError("[EW] 대사 데이터: " + e);
            PixelScreen.Ensure();
            AudioManager.Ensure();
            UI = UIRoot.Create(transform);
            Bubbles = gameObject.AddComponent<Bubbles>();
            Bubbles.Init(this);
            Application.targetFrameRate = 60;
        }

        void Start()
        {
            // 에디터에서 맵 씬을 바로 실행한 경우: 그 맵에서 시험용 새 상태로 시작
            var map = Object.FindAnyObjectByType<MapController>();
            if (State == null && map != null)
            {
#if UNITY_EDITOR
                // 시험용 상태는 사용자 저장 파일을 덮어쓰지 않는다 (개발 저장 폴더로)
                if (string.IsNullOrEmpty(SaveSystem.OverrideDirectory))
                    SaveSystem.OverrideDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/EWDevSave"));
#endif
                var d = SaveData.NewGame();
                d.mapId = map.mapId; d.spawnId = "";
                State = new GameState(d, Db);
                LastCheckpoint = d.Clone();
                Debug.Log("[EW] 맵 씬 직접 실행: " + map.mapId + " (시험용 상태)");
                OnMapReady(map);
                ApplyMapAudio(map);
                if (StoryEvents.WantsBlackIntro(this, map.mapId)) UI.SetFade(1f);
                RunEvent(StoryEvents.OnMapEnter(this, map.mapId));
            }
            UI.hud.SetVisible(State != null);
        }

        // ------------------------------------------------------------ 입력 차단
        public void Block(string reason) { blockers.Add(reason); }
        public void Unblock(string reason) { blockers.Remove(reason); }
        public bool IsBlockedBy(string reason) => blockers.Contains(reason);

        // ------------------------------------------------------------ 이벤트
        /// <summary>이벤트 하나를 실행한다. 동시에 하나만 실행되며 실행 중에는 필드 입력이 차단된다.</summary>
        public Coroutine RunEvent(IEnumerator e) { var c = StartCoroutine(EventWrap(e)); currentEvent = c; return c; }
        Coroutine currentEvent, eventInner;

        IEnumerator EventWrap(IEnumerator e)
        {
            while (eventRunning) yield return null;
            eventRunning = true;
            UI.HidePrompt();
            if (Player != null && Player.Skills != null) Player.Skills.CancelTargeting();
            eventInner = StartCoroutine(e);
            yield return eventInner;
            UI.dialogue.Show(false);
            eventRunning = false;
        }

        // ------------------------------------------------------------ 대사
        /// <summary>
        /// 대사 묶음 출력. 줄마다 스탠딩(표정·위치·숨김)과 앞 침묵(wait)을 적용한다.
        /// 이미 읽은 대사만 Ctrl 로 빨리 넘길 수 있다 (자동으로 넘기지 않는다).
        /// </summary>
        public IEnumerator Say(string id, bool close = true)
        {
            var e = Dialogue.Get(id);
            if (e == null) { Debug.LogError("[EW] 없는 대사 id: " + id); yield break; }
            bool seen = State.D.dialogueSeen.Contains(id);
            if (e.lines != null)
                foreach (var l in e.lines)
                {
                    if (l.wait > 0)
                    {
                        if (UI.dialogue.Open) UI.dialogue.Show(false);
                        for (float t = 0; t < l.wait; t += Time.deltaTime) yield return null;
                    }
                    UI.portraits.Apply(l, Dialogue);
                    yield return UI.dialogue.Line(Dialogue.SpeakerName(l.s), l.t, l.s == "narr" || l.s == "sys", seen);
                }
            GameState.AddUnique(State.D.dialogueSeen, id);
            if (close) { UI.dialogue.Show(false); UI.portraits.HideAll(); }
        }

        public IEnumerator Line(string speakerId, string text)
        {
            yield return UI.dialogue.Line(Dialogue.SpeakerName(speakerId), text, speakerId == "narr" || speakerId == "sys");
        }

        /// <summary>시스템 한 줄 안내 (장치 상태 등).</summary>
        public IEnumerator Info(string text)
        {
            yield return UI.dialogue.Line("", text, true);
            UI.dialogue.Show(false);
        }

        /// <summary>대사 뒤 선택지. 결과는 LastChoice (취소하면 마지막 항목).</summary>
        public IEnumerator Ask(string id)
        {
            var e = Dialogue.Get(id);
            if (e == null) { Debug.LogError("[EW] 없는 대사 id: " + id); LastChoice = 0; yield break; }
            yield return Say(id, false);
            yield return Choose(e.choices);
        }

        /// <summary>선택지가 열려 있는 동안 true (컷신 건너뛰기를 받지 않는다).</summary>
        public static bool ChoiceOpen { get; internal set; }

        /// <summary>
        /// 선택지: 대화창 위에 가로로 (A/D 고르기, J 확정). Esc·Enter 로 넘어가지 않는다.
        /// enabled 로 고를 수 없는 항목(예: 가지지 않은 물건)을 흐리게 보인다. noDefault 면 처음엔 아무것도 고르지 않은 상태.
        /// </summary>
        public IEnumerator Choose(IList<string> options, IList<bool> enabled = null, bool noDefault = false)
        {
            ChoiceOpen = true;
            var bar = new ChoiceBar(UI.Root, options, enabled, noDefault);
            yield return null;
            int r;
            try
            {
                while ((r = bar.Tick()) == -1) yield return null;
            }
            finally { ChoiceOpen = false; bar.Destroy(); }
            LastChoice = r;
            UI.AddLog("", "▷ " + options[r]);
        }

        /// <summary>대사 뒤 선택지 (항목별 사용 가능 여부·기본 선택 없음 지정).</summary>
        public IEnumerator AskEx(string id, IList<bool> enabled, bool noDefault = false, IList<string> overrideOptions = null)
        {
            var e = Dialogue.Get(id);
            if (e == null) { Debug.LogError("[EW] 없는 대사 id: " + id); LastChoice = 0; yield break; }
            yield return Say(id, false);
            yield return Choose(overrideOptions ?? e.choices, enabled, noDefault);
        }

        // ------------------------------------------------------------ 진행
        public void NewGame()
        {
            State = new GameState(SaveData.NewGame(), Db);
            LastCheckpoint = State.D.Clone();
            Bubbles.ResetState();
            UI.log.Clear();
            RunEvent(LoadMap(State.D.mapId, "start"));
        }

        public bool ContinueGame(out string message)
        {
            var res = SaveSystem.TryLoad(out var data, out message);
            if (data == null) return false;
            if (Config.GetMap(data.mapId) == null)
            {
                // 도면 변경으로 맵이 사라졌으면 배치 데이터의 안전 지점으로 옮긴다 (Docs/BUILDING_LAYOUT_GUIDE.md)
                var safe = WorldLayout.I != null ? WorldLayout.I.SafeSpawnFor(data.mapId, data.chapterId) : null;
                if (safe != null && Config.GetMap(safe.map) != null) { message = "저장된 장소가 바뀌어 가까운 안전 지점에서 이어 합니다."; data.mapId = safe.map; data.spawnId = safe.spawn; data.hasPos = false; }
                else { message = "저장된 장소를 찾을 수 없어 첫 장소에서 시작합니다."; data.mapId = "P01"; data.hasPos = false; data.spawnId = "start"; }
            }
            State = new GameState(data, Db);
            LastCheckpoint = data.Clone();
            Bubbles.ResetState();
            UI.log.Clear();
            RunEvent(LoadMap(data.mapId, data.hasPos ? null : data.spawnId, true));
            return true;
        }

        public void GoToMap(string mapId, string spawnId) => RunEvent(LoadMap(mapId, spawnId));

        public IEnumerator LoadMap(string mapId, string spawnId, bool useSavedPos = false)
        {
            var entry = Config.GetMap(mapId);
            if (entry == null) { Debug.LogError("[EW] 맵 없음: " + mapId); yield break; }
            Bubbles.OnMapChange();
            if (UI.FadeAlpha < 0.99f) yield return UI.Fade(1f, 0.25f);
            loading = true;
            if (Map != null && Map.mapId != mapId) { StoreDevices(); StoryEvents.OnLeaveMap(this, Map.mapId); }
            State.D.mapId = mapId;
            if (!useSavedPos) { State.D.spawnId = spawnId; State.D.hasPos = false; }
            pendingSpawn = spawnId;
            Map = null; Player = null; Companion = null;
            var op = SceneManager.LoadSceneAsync(entry.scene);
            while (op != null && !op.isDone) yield return null;
            float wait = 0;
            while ((Map == null || Map.mapId != mapId) && wait < 5f) { wait += Time.unscaledDeltaTime; yield return null; }
            loading = false;
            UI.hud.SetVisible(true);
            UI.hud.Refresh(State);
            if (Config.autosaveOnMapEnter && !useSavedPos) SaveNow("map:" + mapId, false);
            if (Map != null) ApplyMapAudio(Map);
            // 도입 연출이 있는 장소는 연출이 직접 화면을 연다 (장 번호 카드 없음)
            if (!StoryEvents.WantsBlackIntro(this, mapId)) yield return UI.Fade(0f, 0.3f);
            yield return StoryEvents.OnMapEnter(this, mapId);
            if (UI.FadeAlpha > 0.01f) yield return UI.Fade(0f, 0.3f);
        }

        public void ApplyMapAudio(MapController m)
        {
            var a = AudioManager.I;
            if (a == null || m == null) return;
            string amb = m.ambience;
            if (!string.IsNullOrEmpty(m.altAmbienceFlag) && State != null && State.Has(m.altAmbienceFlag)) amb = m.altAmbience;
            if (m.useAlarmAmbience && Facility.AlarmOn) amb = "amb_lab_alarm";
            a.PlayAmbience(string.IsNullOrEmpty(amb) ? null : amb, 1.2f);
            a.PlayMusic(string.IsNullOrEmpty(m.music) ? null : m.music, 1.5f, 0.8f);
        }

        /// <summary>MapController.Start 에서 호출된다.</summary>
        public void OnMapReady(MapController m)
        {
            MapLoadCount++;
            Map = m;
            if (State == null) return;
            // 장치·처리 완료된 적·주운 물건 복원
            foreach (var d in Device.All) d.LoadFrom(State);
            foreach (var it in Interactable.All.ToArray()) it.OnStateLoaded(State);
            // 플레이어 배치
            Vector2 pos;
            int facing = 1;
            if (State.D.hasPos && State.D.mapId == m.mapId && (pendingSpawn == null || pendingSpawn == ""))
            {
                pos = new Vector2(State.D.posX, State.D.posY); facing = State.D.facing;
            }
            else
            {
                var sp = m.GetSpawn(pendingSpawn);
                if (sp == null && !string.IsNullOrEmpty(pendingSpawn) && WorldLayout.I != null) sp = m.GetSpawn(WorldLayout.I.SafeSpawnIn(m.mapId));   // 도면 변경으로 스폰이 사라졌을 때
                pos = sp != null ? (Vector2)sp.transform.position : m.DefaultSpawn();
                facing = sp != null ? sp.facing : 1;
            }
            Player = PlayerController.Spawn(this, pos, facing);
            if (State.Has("venelle_carried")) AttachCarried();
            else if (State.VenelleIsAlly)
                Companion = Companion.Spawn(this, "venelle", pos + Vector2.left * facing * 1.1f, facing);
            else if ((State.RavielIsAlly || State.D.ravielState == "injured") && !m.noCompanion)
                Companion = Companion.Spawn(this, "raviel", pos + Vector2.left * facing * 1.1f, facing);
            if (State.D.visitedMaps.Count == 0 || State.D.visitedMaps[State.D.visitedMaps.Count - 1] != m.mapId) State.D.visitedMaps.Add(m.mapId);
            var cam = Camera.main;
            if (cam != null)
            {
                var rig = cam.GetComponent<CameraRig>();
                if (rig == null) rig = cam.gameObject.AddComponent<CameraRig>();
                rig.Bind(Player.transform, m.bounds);
            }
            pendingSpawn = null;
            UI.hud.Refresh(State);
        }

        /// <summary>베넬 부축 상태: 루 옆에 기대어 걷는 베넬을 붙이고 이동을 느리게 한다.</summary>
        public void AttachCarried()
        {
            if (Player == null) return;
            Player.Carrying = true;
            var t = Player.transform.Find("Carried");
            if (t != null) return;
            var go = new GameObject("Carried");
            go.transform.SetParent(Player.transform, false);
            var a = go.AddComponent<Actor>(); a.actorId = "venelle";
            var v = CharacterView.Create(go.transform, Config.GetArt("venelle"), 11);
            a.view = v;
            go.AddComponent<CarriedFollow>().Init(Player, v);
            UI.hud.SetHint("");
        }

        public void DetachCarried()
        {
            if (Player == null) return;
            Player.Carrying = false;
            var t = Player.transform.Find("Carried");
            if (t != null) Destroy(t.gameObject);
            UI.hud.SetHint("");
        }

        public void StoreDevices()
        {
            if (State == null) return;
            foreach (var d in Device.All) d.StoreTo(State);
        }

        // ------------------------------------------------------------ 저장
        public void SaveNow(string checkpointId, bool toast = true)
        {
            if (State == null) return;
            StoreDevices();
            State.D.checkpointId = checkpointId;
            if (Player != null)
            {
                State.D.hasPos = true;
                State.D.posX = Player.transform.position.x; State.D.posY = Player.transform.position.y;
                State.D.facing = Player.Facing;
            }
            if (SaveSystem.Write(State.D, out string err))
            {
                LastCheckpoint = State.D.Clone();
                if (toast) UI.Toast("저장했습니다.");
            }
            else UI.Toast("저장 실패: " + err, 4f);
        }

        /// <summary>검수 이동 전 정리: 진행 중 이벤트·대화·컷신·지연 코루틴·입력 상태·시간 배율·페이드·화면 위 임시 창·투사체.</summary>
        public void HardReset()
        {
            StopAllCoroutines();
            eventRunning = false; loading = false; blockers.Clear(); downHandling = false;
            currentEvent = null; eventInner = null;
            GameInput.ClearContexts(); GameInput.Virtual.ReleaseAll();
            TimeCtl.ClearAll(); Hazard.ForcePause = false;
            Cutscene.ResetState(); ChoiceOpen = false;
            Bubbles.ResetState();
            Noise.Clear(); Guard.NoGo.Clear();
            EnemyShot.ClearAll(); ArenaGuard.Reset(); ArenaSwitch.User = null; ArenaZap.User = null;
            foreach (var pr in Object.FindObjectsByType<Projectile>()) Destroy(pr.gameObject);
            foreach (var n in new[] { "FeedCanvas", "FeedCamera", "BinCanvas", "Credits", "EndingDesktop", "LastLight" })
                for (var go = GameObject.Find(n); go != null; go = GameObject.Find(n)) { go.name = "_dead"; Destroy(go); }
            UI.dialogue.Show(false); UI.portraits.HideAll(); UI.HidePrompt(); UI.Letterbox(0); UI.LetterboxImmediate(0); UI.ClearFlash();
            UI.SetFade(1f);
            if (UI.bubble != null) UI.bubble.Hide();
            if (Player != null) { Player.ScriptControl = false; Player.Frozen = false; Player.AnimOverride = null; }
        }

        /// <summary>새 상태로 교체 (검수 이동·테스트). 재시도 지점도 이 상태로.</summary>
        public void SetState(GameState s)
        {
            State = s;
            LastCheckpoint = s.D.Clone();
            Bubbles.ResetState();
        }

        public void ReturnToTitle()
        {
            ReviewMode.OnReturnToTitle();
            StopAllCoroutines();
            eventRunning = false; loading = false; blockers.Clear();
            GameInput.ClearContexts();
            TimeCtl.ClearAll();
            downHandling = false;
            Bubbles.ResetState();
            State = null; Map = null; Player = null; Companion = null;
            UI.dialogue.Show(false); UI.HidePrompt(); UI.hud.SetVisible(false); UI.SetFade(0); UI.Letterbox(0);
            if (AudioManager.I != null) AudioManager.I.StopAll(0.5f);
            SceneManager.LoadScene(Config.titleScene);
        }

        /// <summary>마지막 체크포인트에서 다시 시작 (패배 후 선택).</summary>
        public IEnumerator ReloadCheckpoint()
        {
            var snap = LastCheckpoint ?? SaveData.NewGame();
            var seenD = State != null ? new List<string>(State.D.dialogueSeen) : new List<string>();
            var seenB = State != null ? new List<string>(State.D.bubblesSeen) : new List<string>();
            State = new GameState(snap.Clone(), Db);
            State.D.dialogueSeen = seenD; State.D.bubblesSeen = seenB;
            yield return LoadMap(State.D.mapId, State.D.hasPos ? null : State.D.spawnId, State.D.hasPos);
        }

        // ------------------------------------------------------------ 매 프레임
        void Update()
        {
            if (State == null) return;
            State.D.playSeconds += Time.unscaledDeltaTime;
            UI.hud.SetVisible(Map != null && !UI.dialogue.Open && !loading && !Cutscene.Running && !(Map != null && Map.hideHud));
            // 경보 방송 반복: 대사·연출과 겹치지 않게 간격을 두고 화면 위 자막 + 소리만 (조작은 막지 않음)
            if (Facility.AlarmOn && Map != null && Map.useAlarmAmbience && FieldInputAllowed && !State.Has("c3_boss_done"))
            {
                broadcastT += Time.deltaTime;
                if (broadcastT >= 55f)
                {
                    broadcastT = 0;
                    var e = Dialogue.Get("broadcast_repeat");
                    if (e != null && e.lines.Length > 0) { UI.Toast("〔방송〕 " + e.lines[0].t, 3.2f); Sfx.Play("sfx_broadcast", 0.55f); }
                }
            }
            else if (!FieldInputAllowed) broadcastT = Mathf.Min(broadcastT, 40f);
            if (FieldInputAllowed && Map != null && GameInput.PauseDown) RunEvent(PauseMenu.Run(this));
        }

        // ------------------------------------------------------------ 구간 재시도
        /// <summary>구간 시작점 기록 (재시도 지점). 저장 파일에도 쓴다.</summary>
        public void Checkpoint(string id, bool toast = false)
        {
            if (State == null) return;
            SaveNow(id, toast);
        }

        /// <summary>안정도가 0 이 되면: 짧게 쓰러짐 → 현재 짧은 구간 시작점에서 바로 재시도. 긴 오프닝은 다시 보지 않는다.</summary>
        public void OnPlayerDown()
        {
            if (downHandling) return;
            downHandling = true;
            StartCoroutine(PlayerDownRoutine());
        }

        IEnumerator PlayerDownRoutine()
        {
            Block("down");
            if (Player != null) { Player.Skills?.CancelTargeting(); Player.AnimOverride = "Down"; }
            Sfx.Play("sfx_power_down", 0.8f, 0.8f);
            TimeCtl.Set("down", 0.35f);
            yield return new WaitForSecondsRealtime(0.7f);
            TimeCtl.Clear("down");
            yield return UI.Fade(1f, 0.35f);
            Unblock("down");
            downHandling = false;
            var snap = LastCheckpoint ?? SaveData.NewGame();
            State.RestoreForRetry(snap);
            State.D.lu.hp = State.MaxHp("lu");
            State.D.lu.charge = Mathf.Max(State.D.lu.charge, 2);
            State.D.retries++;
            Noise.Clear();
            RestartEvent(LoadMap(State.D.mapId, State.D.hasPos ? null : State.D.spawnId, State.D.hasPos));
        }

        /// <summary>진행 중인 이벤트를 끊고 새 이벤트를 시작 (재시도용).</summary>
        public void RestartEvent(IEnumerator e)
        {
            if (eventRunning && currentEvent != null) StopCoroutine(currentEvent);
            if (eventInner != null) StopCoroutine(eventInner);
            eventRunning = false;
            UI.dialogue.Show(false); UI.portraits.HideAll();
            blockers.Remove("cutscene");
            GameInput.ClearContexts();
            TimeCtl.ClearAll();
            RunEvent(e);
        }

        /// <summary>루의 짧은 독백 말풍선 (이동을 막지 않음).</summary>
        public void Monologue(string text, float seconds = 0)
        {
            if (UI != null && UI.bubble != null) UI.bubble.Say(text, seconds);
        }

        /// <summary>대사 데이터의 한 줄을 말풍선으로 (id 의 첫 줄).</summary>
        public void MonologueId(string id, float seconds = 0)
        {
            var e = Dialogue.Get(id);
            if (e == null || e.lines == null || e.lines.Length == 0) { Debug.LogError("[EW] 없는 대사 id: " + id); return; }
            Monologue(e.lines[0].t, seconds);
            GameState.AddUnique(State.D.dialogueSeen, id);
        }
    }
}
