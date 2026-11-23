using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EW
{
    /// <summary>
    /// 2026-11-15 추가 수정 (실제 플레이 피드백). 순서·스킵 종료 상태 표: Docs/CUTSCENE_FLOW.md
    /// - 오프닝: 마을(넓은 구도, 걷기는 필요한 구간만) → 집 도착 → 컴퓨터 앞 대사 → 전기화·흡입 → 컴퓨터 안(직접 탐색).
    /// - 컴퓨터 안: 착지 뒤 바로 조작. 안내서(J) → 연습 → 접속 기록/연결 끊기 조사 → 터미널 강제 전송 → 실험 통.
    /// - 실험 통: 몸 전체가 통 안 (받침 위 바닥·웅크린 수용 자세). 연구원은 걸으며 말한다 → 방전 → 탈출 시작 대사 → 즉시 조작.
    /// - 베넬: 좁은 복도 한 화면 (루·베넬·뒤 문·돌진 거리) → 실시간 전투(화학 패턴·잡기) → 협상/제압 → 처치 선택 → 휴식·초콜릿 → 임시 동행.
    /// 연출 길이는 Cutscene 이 Temp/EWBridge/cuts.txt 에 자동 연출/대화 시간을 나눠 기록한다.
    /// </summary>
    public static partial class StoryEvents
    {
        // ============================================================ 공통 연출 도우미
        /// <summary>의성어 글자 (화면 안 짧게 떠올랐다 사라짐). 소리를 못 듣는 환경에서도 사건이 읽히게.</summary>
        public static void Onoma(Game g, Vector2 world, string text, Color c, float time = 1.1f, int size = 14)
        {
            // Sound-effect captions are deliberately disabled. Audio and physical reactions carry the event.
            return;
        }

        static IEnumerator OnomaCo(Game g, Vector2 world, string text, Color c, float time, int size)
        {
            var rt = UIKit.Rect("Onoma", g.UI.worldLayer, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.sizeDelta = new Vector2(200, 24);
            var t = UIKit.Label(rt, "T", text, size, TextAnchor.MiddleCenter, c);
            t.font = UIKit.Bold;
            var sh = rt.gameObject.AddComponent<Outline>(); sh.effectColor = new Color(0, 0, 0, 0.85f); sh.effectDistance = new Vector2(1, -1);
            for (float k = 0; k < time; k += Time.unscaledDeltaTime)
            {
                float e = k / time;
                rt.anchoredPosition = g.UI.WorldToCanvas(world + Vector2.up * (0.35f * e));
                rt.localScale = Vector3.one * (e < 0.12f ? Mathf.Lerp(1.4f, 1f, e / 0.12f) : 1f);
                var cc = c; cc.a = e < 0.7f ? 1f : 1f - (e - 0.7f) / 0.3f; t.color = cc;
                yield return null;
            }
            Object.Destroy(rt.gameObject);
        }

        static LineRenderer Line(Game g, Color c, float w, int order)
        {
            var go = new GameObject("FxLine");
            Cutscene.Temp(go);
            var lr = go.AddComponent<LineRenderer>();
            lr.material = g.Config.lineMaterial != null ? g.Config.lineMaterial : new Material(Shader.Find("Sprites/Default"));
            lr.startColor = lr.endColor = c; lr.startWidth = lr.endWidth = w; lr.sortingOrder = order; lr.useWorldSpace = true; lr.numCapVertices = 0;
            return lr;
        }

        static float FlashK => Mathf.Clamp(GameSettings.Flash, 0.25f, 1f);

        // ============================================================ 오프닝 A: 불타는 마을 (짧게)
        static IEnumerator Village2(Game g)
        {
            var ui = g.UI; var p = g.Player;
            ui.SetFade(1f);
            ui.LetterboxImmediate(18);
            p.ScriptControl = true;
            p.AnimOverride = "BackIdle";
            Vector2 lu = LuPos(g);
            // 루를 왼쪽 1/3 에 작게, 불타는 마을이 오른쪽으로 펼쳐진 넓은 구도 (확대 없음)
            Cutscene.CamSnap(new Vector2(lu.x + 5.2f, lu.y + 2.4f));
            Cutscene.Parallel(g, Cutscene.Pan(new Vector2(lu.x + 6.6f, lu.y + 2.4f), 6f, false));
            yield return ui.Fade(0f, 0.9f);
            yield return g.Say("op_v_2");
            p.AnimOverride = "BackLook";
            yield return Cutscene.Wait(0.35f);
            yield return g.Say("op_v_4");
            // 몸을 돌려 걷기 시작 — 걷는 장면은 2초 정도만 보여 주고 끊는다
            p.SetFacing(1);
            p.AnimOverride = "LimpIdle";
            yield return Cutscene.Wait(0.25f);
            Cutscene.Parallel(g, Cutscene.Pan(new Vector2(lu.x + 7.6f, lu.y + 2.4f), 1.8f));
            yield return Cutscene.LuLimp(g, lu.x + 2.1f, 2.2f, 2.2f);
            p.SetFacing(-1); p.AnimOverride = "BackLook";
            yield return Cutscene.Wait(0.65f);
            yield return g.Say("op_v_no");
            p.AnimOverride = "Sad";
            yield return Cutscene.Wait(1.15f);
            p.SetFacing(1);
            yield return Cutscene.LuLimp(g, lu.x + 5.4f, 2.1f, 2.5f);
            // 편집 컷: 짧은 암전 → 집 문 앞 (같은 진행 방향, 문은 화면 오른쪽, 루는 왼쪽에서 들어옴)
            yield return ui.Fade(1f, 0.55f);
            Vector2 door = M("house_door", new Vector2(lu.x + 30, lu.y));
            p.Teleport(new Vector2(door.x - 2.0f, door.y));
            Cutscene.CamSnap(new Vector2(door.x - 0.6f, door.y + 2.4f));
            yield return ui.Fade(0f, 0.55f);
            yield return Cutscene.LuLimp(g, door.x - 0.3f, 9f, 2.2f);
            Sfx.Play("sfx_metal_creak", 0.4f, 1.6f);
            yield return Cutscene.FadeSprite(p.View.sr, 0f, 0.35f);
            yield return ui.Fade(1f, 0.3f);
        }

        static void FinVillage2(Game g) { g.State.Set("op_village_done"); g.UI.SetFade(1f); g.UI.LetterboxImmediate(0); }

        // ============================================================ 오프닝 B: 무너진 집 → 컴퓨터 → 전기화
        static IEnumerator House2(Game g)
        {
            var ui = g.UI; var p = g.Player;
            ui.SetFade(1f);
            p.ScriptControl = true;
            p.AnimOverride = "LimpIdle";
            p.SetFacing(1);
            Vector2 pc = M("pc", new Vector2(12.6f, 5));
            Vector2 lu = LuPos(g);
            // 컴퓨터를 오른쪽 1/3, 루는 왼쪽 — 시선 방향(오른쪽)에 여백. 집 바깥은 검정.
            Cutscene.CamSnap(new Vector2(pc.x - 3.0f, lu.y + 2.3f));
            yield return ui.Fade(0f, 0.4f);
            yield return Cutscene.LuLimp(g, pc.x - 1.25f, 9f, 3.0f);
            p.AnimOverride = "Sad";
            yield return g.Say("op_h_a");
            yield return g.Say("op_h_b");
            yield return g.Say("op_h_c");
            // 눈을 질끈 감는다
            p.AnimOverride = p.View.HasState("EyesClosed") ? "EyesClosed" : "Sad";
            Sfx.Play("sfx_heartbeat", 0.35f);
            yield return Cutscene.Wait(0.55f);
            yield return g.Say("op_h_d");
            yield return g.Say("op_h_return");
            yield return Cutscene.Wait(0.6f);
            // 부팅음 "위잉—"
            var pcm = Marker.Find("pc");
            var frames = pcm != null ? pcm.GetComponent<FrameSet>() : null;
            if (frames != null) frames.Show(1);
            Vector2 screen = M("pc_screen", pc + new Vector2(0, 0.8f));
            Vector2 touch = M("pc_touch", pc + new Vector2(-0.4f, 0.6f));
            Sfx.Play("sfx_pc_on", 0.9f);
            var gl = Cutscene.Glow(screen, 0.55f, new Color(0.5f, 0.75f, 1f, 0.0f));
            Cutscene.Temp(gl != null ? gl.gameObject : null);
            Cutscene.Parallel(g, Cutscene.FadeSprite(gl, 0.55f, 0.4f));
            yield return Cutscene.Wait(0.45f);
            p.AnimOverride = "ReachPC";
            yield return Cutscene.Wait(0.3f);
            if (frames != null) frames.Show(2);
            yield return Electrify(g, p, touch, screen, gl);
            if (frames != null) frames.Show(3);
            yield return ui.Fade(1f, 0.15f);
        }

        /// <summary>
        /// 전기화·흡입: ① 접촉 지점으로 작은 전기·입자가 모인다(짧은 충전) ② 윤곽이 빛나며 몸이 전기 신호로 갈라진다
        /// ③ 주변의 가는 빛줄기가 컴퓨터로 빨려 든다 ④ 실루엣이 방향성 잔상과 함께 화면 쪽으로 압축된다
        /// ⑤ "위이잉—팍! 촥!" 에 맞춰 마지막 전기 꼬리가 화면 안으로 사라진다. 무작위 파티클을 흩뿌리지 않고 모든 움직임이 한 점(화면)을 향한다.
        /// </summary>
        static IEnumerator Electrify(Game g, PlayerController p, Vector2 touch, Vector2 screen, SpriteRenderer screenGlow)
        {
            var view = p.View; var sr = view.sr;
            Vector2 body = (Vector2)p.transform.position + new Vector2(0, 0.85f);
            // ① 수렴 (0.55초)
            Sfx.Play("sfx_charge_up", 0.8f, 1.3f);
            var dots = new List<(LineRenderer lr, Vector2 from)>();
            for (int i = 0; i < 9; i++)
            {
                float a = i * Mathf.PI * 2 / 9f + 0.3f;
                var from = touch + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * Random.Range(0.9f, 1.4f);
                dots.Add((Line(g, new Color(0.7f, 0.95f, 1f, 0.9f), 0.05f, 46), from));
            }
            var tg = Cutscene.Glow(touch, 0.2f, new Color(0.8f, 0.95f, 1f, 0.9f), 45);
            Cutscene.Temp(tg != null ? tg.gameObject : null);
            for (float t = 0; t < 0.85f; t += Time.deltaTime)
            {
                float k = t / 0.85f; float e = k * k;
                foreach (var (lr, from) in dots)
                {
                    Vector2 head = Vector2.Lerp(from, touch, e), tail = Vector2.Lerp(from, touch, Mathf.Max(0, e - 0.25f));
                    lr.positionCount = 2; lr.SetPosition(0, tail); lr.SetPosition(1, head);
                }
                if (tg != null) tg.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.2f, k);
                yield return null;
            }
            foreach (var (lr, _) in dots) lr.enabled = false;
            // ② 윤곽이 빛나며 전기 신호로 분해 (0.3초)
            p.AnimOverride = view.HasState("Electrify") ? "Electrify" : "Connect";
            Sfx.Play("sfx_connect", 0.9f, 1.2f);
            var strips = new List<LineRenderer>();
            for (int i = 0; i < 7; i++) strips.Add(Line(g, new Color(0.85f, 0.97f, 1f, 0.95f), 0.04f, 47));
            var cage = Line(g,new Color(1f,0.35f,0.25f),0.045f,40);
            var tether = Line(g,new Color(0.5f,0.9f,1f),0.04f,41);
            for (float t = 0; t < 2.2f; t += Time.deltaTime)
            {
                float k = t / 2.2f;
                if(PixelScreen.I!=null)PixelScreen.I.SetSignalDistortion(k*0.85f);
                float dx=(GameInput.RawIsHeld(Act.Right)?1:0)-(GameInput.RawIsHeld(Act.Left)?1:0);
                float dy=(GameInput.RawIsHeld(Act.MenuUp)?1:0)-(GameInput.RawIsHeld(Act.Down)?1:0);
                p.transform.position+=new Vector3(dx,dy,0)*Mathf.Lerp(1.2f,0,k)*Time.deltaTime;
                sr.color = Color.Lerp(Color.white, new Color(0.7f, 0.95f, 1f, 1f), k);
                for (int i = 0; i < strips.Count; i++)
                {
                    float y = p.transform.position.y + 0.2f + i * 0.24f;
                    float half = 0.25f + 0.1f * Mathf.Sin(i * 1.7f);
                    var lr = strips[i]; lr.positionCount = 2;
                    float off = (i % 2 == 0 ? 1 : -1) * k * 0.15f;
                    lr.SetPosition(0, new Vector2(p.transform.position.x - half + off, y)); lr.SetPosition(1, new Vector2(p.transform.position.x + half + off, y));
                }
                yield return null;
            }
            // ③ 가는 빛줄기 흡입 + ④ 실루엣 압축·잔상 (0.38초, 빠르고 강하게)
            Sfx.Play("sfx_absorb", 1f, 1.1f);
            var threads = new List<(LineRenderer lr, Vector2 from)>();
            for (int i = 0; i < 6; i++)
            {
                Vector2 from = body + new Vector2(Random.Range(-2.6f, -0.6f), Random.Range(-0.6f, 1.6f));
                threads.Add((Line(g, new Color(0.6f, 0.9f, 1f, 0.75f), 0.025f, 45), from));
            }
            var vt = view.transform; Vector3 start = vt.position;
            float afterT = 0;
            for (float t = 0; t < 0.95f; t += Time.deltaTime)
            {
                float k = t / 0.95f; float e = k * k * k;
                vt.position = Vector3.Lerp(start, (Vector3)screen + Vector3.down * 0.4f, e);
                vt.localScale = Vector3.one;
                sr.enabled = k < 0.65f || (int)(k * 30) % 3 != 0;
                foreach (var lr in strips)
                {
                    var a = lr.GetPosition(0); var b = lr.GetPosition(1);
                    lr.SetPosition(0, Vector3.Lerp(a, (Vector3)screen, e * 0.3f)); lr.SetPosition(1, Vector3.Lerp(b, (Vector3)screen, e * 0.3f));
                }
                foreach (var (lr, from) in threads)
                {
                    Vector2 head = Vector2.Lerp(from, screen, e), tail = Vector2.Lerp(from, screen, Mathf.Max(0, e - 0.35f));
                    lr.positionCount = 2; lr.SetPosition(0, tail); lr.SetPosition(1, head);
                }
                afterT -= Time.deltaTime;
                if (afterT <= 0 && sr.sprite != null)
                {
                    afterT = 0.045f;
                    var ghost = new GameObject("Afterimage"); Cutscene.Temp(ghost);
                    ghost.transform.position = vt.position; ghost.transform.localScale = vt.localScale;
                    var gs = ghost.AddComponent<SpriteRenderer>(); gs.sprite = sr.sprite; gs.flipX = sr.flipX; gs.sortingOrder = 11;
                    gs.color = new Color(0.6f, 0.9f, 1f, 0.55f);
                    g.StartCoroutine(Cutscene.FadeSprite(gs, 0f, 0.22f));
                }
                yield return null;
            }
            // ⑤ 팍! 촥! — 마지막 전기 꼬리가 화면 안으로
            sr.enabled = true; view.SetVisible(false);
            vt.localPosition = Vector3.zero; vt.localScale = Vector3.one; sr.color = Color.white;
            foreach (var lr in strips) lr.enabled = false;
            foreach (var (lr, _) in threads) lr.enabled = false;
            var tail2 = Line(g, new Color(0.95f, 1f, 1f, 1f), 0.08f, 48);
            Sfx.Play("sfx_discharge_small", 1f, 1.4f);
            Cutscene.Shake(0.1f, 0.18f);
            for (float t = 0; t < 0.18f; t += Time.deltaTime)
            {
                float k = t / 0.18f;
                tail2.positionCount = 2; tail2.SetPosition(0, Vector2.Lerp(body, screen, 0.4f + k * 0.6f)); tail2.SetPosition(1, screen);
                if (screenGlow != null) screenGlow.transform.localScale = Vector3.one * Mathf.Lerp(1f, 2.2f, k);
                yield return null;
            }
            tail2.enabled = false;
            yield return g.UI.Flash(new Color(0.85f, 0.95f, 1f), 0.45f * FlashK, 0.35f);
            yield return Cutscene.Wait(0.5f);
        }

        static void FinHouse2(Game g)
        {
            g.State.Set("op_house_done");
            if (g.Player != null) { g.Player.View.transform.localPosition = Vector3.zero; g.Player.View.transform.localScale = Vector3.one; }
            g.UI.SetFade(1f);
        }

        // ============================================================ 오프닝 C: 컴퓨터 안 — 착지 뒤 바로 조작
        static IEnumerator DesktopLand(Game g)
        {
            var ui = g.UI; var p = g.Player;
            p.ScriptControl = true;
            Vector2 land = LuPos(g);
            p.Teleport(land + new Vector2(1f, 3.5f));
            p.Frozen = true; p.AnimOverride = "Jump";
            ui.SetFade(0f);
            Sfx.Play("sfx_connect",0.65f);
            yield return Cutscene.Wait(0.7f);
            yield return g.Say("op_d_1");
            p.AnimOverride = "GetUp";
            yield return Cutscene.Wait(0.4f);
            p.AnimOverride = "LookAround";
            yield return Cutscene.Wait(0.5f);
        }

        static void FinDesktopLand(Game g)
        {
            var s = g.State;
            s.Set("op_desktop_land");
            g.UI.SetFade(0f);
            if (g.Player != null && !g.Player.Grounded) g.Player.Teleport(Marker.Pos("land", new Vector2(5.5f, 1f)));
            g.MonologueId("op_d_2");
            g.UI.hud.SetHint(GameInput.Pair(Act.Left,Act.Right) + " / " + GameInput.Pair(Act.MenuUp,Act.Down) + " 자유 이동 · " + GameInput.Label(Act.Interact) + " 조사 — 인터넷과 안내서를 확인하자");
            g.Checkpoint("pc_land");
        }

        static IEnumerator PcInternet(Game g)
        {
            g.State.Set("pc_internet_checked");
            var p = g.Player;
            Sfx.Play("sfx_pc_beep",0.7f,0.7f);
            Fx.Spawn("Spark",p.Chest,false,30,0.5f);
            p.Teleport((Vector2)p.transform.position + Vector2.left * 0.32f);
            yield return g.Say("pc_internet_denied");
            if (g.State.Has("pc_guide_read"))
            {
                g.UI.hud.SetHint("안내서를 닫으면 감지 사건이 시작된다");
            }
        }

        static IEnumerator PcGuide(Game g)
        {
            yield return GuideBook.Run(g);
            g.SaveNow("pc_guide",false);
            if (g.State.Has("pc_internet_checked"))
            {
                yield return Cutscene.Play(g,"op_transfer",Transfer(g),FinTransfer);
                yield return g.LoadMap("C01","tank");
            }
            else g.UI.hud.SetHint("인터넷 아이콘을 확인한 뒤 안내서를 다시 닫아 보자");
        }

        static IEnumerator PcLog(Game g, bool exit)
        {
            if (!g.State.Has("pc_guide_read")) { yield return g.Say("pc_log_early"); yield break; }
            yield return g.Say(exit ? "pc_exit" : "pc_log");
            g.UI.hud.SetHint("");
            yield return Cutscene.Play(g, "op_transfer", Transfer(g), FinTransfer);
            yield return g.LoadMap("C01", "tank");
        }

        // ============================================================ 감지·강제 전송 (작중 단말 연출 — 실제 셸 명령 없음)
        static IEnumerator Transfer(Game g)
        {
            var ui = g.UI; var p = g.Player;
            p.ScriptControl = true; p.AnimOverride = "Idle";
            // 1) 작은 시스템 알림 (오른쪽 아래)
            var note = new SystemWindow(ui.Root, "알림", new Vector2(206, -118), new Vector2(180, 44));
            Cutscene.Temp(note.root.gameObject);
            note.Style(new Color(0.25f, 0.4f, 0.62f), new Color(0.1f, 0.13f, 0.2f, 0.96f), new Color(0.85f, 0.9f, 1f));
            note.SetBody("이상 접속이 감지되었습니다.");
            Sfx.Play("sfx_window_pop", 0.8f);
            yield return note.Pop(true);
            p.SetFacing(1); p.AnimOverride = "LookAround";
            yield return Cutscene.Wait(0.55f);
            // 2) 터미널 창: 빠른 로그 (대부분은 속도감, 중요한 3줄만 잠깐 강조)
            Object.Destroy(note.root.gameObject);
            var term = new TerminalWindow(ui.Root);
            Cutscene.Temp(term.root.gameObject);
            Sfx.Play("sfx_pc_beep", 0.6f, 1.3f);
            string[] fast =
            {
                "> attach session  remote://7동/retrieval", "  handshake ............ ok", "  route table .......... 14 hops",
                "> scan local bus", "  device: display ok  input ok  power ok",
                "> scan bio-electric signature",
                "!대상 식별: E-D05-47 (전기 반응 생체 1)",
                "  match 0.993  last seen: 격리 실험동", "> lock session",
                "!세션 고정 — 입력 권한 회수 / 연결 끊기 차단",
                "  input .......... revoked", "  disconnect ..... denied", "> prepare transfer  target: 7동 실험 통 3",
                "  buffer ......... 64%", "  buffer ......... 100%",
                "!강제 전송 준비 완료 — 단말 → 제7연구동 실험 통 3",
            };
            foreach (var line in fast)
            {
                bool key = line.StartsWith("!");
                term.Add(key ? line.Substring(1) : line, key);
                Sfx.Play("sfx_text", 0.25f, 1.6f);
                if (key)
                {
                    Sfx.Play("sfx_pc_beep", 0.6f, 0.9f);
                    if (line.Contains("대상")) p.AnimOverride = "Fear";
                    yield return Cutscene.Wait(0.55f);
                }
                else yield return Cutscene.Wait(0.045f);
            }
            // 3) 붉은 경고창: 입력 권한 차단
            Sfx.Play("sfx_alarm", 0.8f);
            var warn = new SystemWindow(ui.Root, "경고", new Vector2(140, -90), new Vector2(240, 70));
            Cutscene.Temp(warn.root.gameObject);
            warn.SetBody("입력 권한이 차단되었습니다.\n강제 전송을 시작합니다.");
            yield return warn.Pop(true);
            // 4) 창과 루가 흔들리며 압축된다 (붙잡혀 끌려가는 느낌)
            Sfx.Play("sfx_transfer", 1f);
            var vt = p.View.transform;
            Vector3 basePos = vt.localPosition;
            Vector2 wBase = warn.root.anchoredPosition, tBase = term.root.anchoredPosition;
            var cage = Line(g,new Color(1f,0.35f,0.25f),0.045f,40);
            var tether = Line(g,new Color(0.5f,0.9f,1f),0.04f,41);
            for (float t = 0; t < 2.2f; t += Time.deltaTime)
            {
                float k = t / 2.2f;
                float j = (1 - k * 0.3f) * 3f * GameSettings.Shake;
                warn.root.anchoredPosition = wBase + new Vector2(Random.Range(-j, j), Random.Range(-j, j));
                term.root.anchoredPosition = tBase + new Vector2(Random.Range(-j, j), Random.Range(-j, j));
                warn.SetProgress(k);
                vt.localPosition = basePos + new Vector3(Random.Range(-0.06f, 0.06f) * GameSettings.Shake, 0, 0);
                vt.localScale = Vector3.one;
                float radius = Mathf.Lerp(2.8f,0.65f,k);
                Vector2 center = p.Chest;
                cage.positionCount = 5;
                cage.SetPosition(0, center + new Vector2(-radius,-radius)); cage.SetPosition(1,center + new Vector2(-radius,radius));
                cage.SetPosition(2,center + new Vector2(radius,radius)); cage.SetPosition(3,center + new Vector2(radius,-radius)); cage.SetPosition(4,cage.GetPosition(0));
                tether.positionCount = 9;
                for(int jx=0;jx<9;jx++) tether.SetPosition(jx, center + new Vector2((jx-4)*0.35f,Mathf.Sin(jx*1.8f+t*32)*0.45f*(0.3f+k)));
                p.View.sr.enabled = k < 0.65f || (int)(t*18)%3 != 0;
                p.View.sr.color = Color.Lerp(Color.white, new Color(1f, 0.6f, 0.6f), k);
                yield return null;
            }
            ReactionParticles.Emit(p.Chest,1,false,48,1.1f);
            p.View.sr.enabled = true; p.View.SetVisible(false);
            vt.localPosition = Vector3.zero; vt.localScale = Vector3.one; p.View.sr.color = Color.white;
            // 5) 짧은 흰 섬광 한 번 + 충격음 (섬광 줄이기 설정이면 옅게, 대신 글자로 사건을 알린다)
            Sfx.Play("sfx_discharge_big", 0.9f, 1.3f);
            Cutscene.Shake(0.18f, 0.25f);
            if (FlashK < 0.6f) term.Add("[전송됨]", true);
            yield return ui.Flash(Color.white, 0.95f * FlashK, 0.18f);
            yield return ui.Fade(1f, 0.12f);
            yield return Cutscene.Wait(0.25f);
        }

        static void FinTransfer(Game g)
        {
            g.State.Set("op_desktop_done");
            if (g.Player != null) { g.Player.View.transform.localPosition = Vector3.zero; g.Player.View.transform.localScale = Vector3.one; }
            g.UI.SetFade(1f);
        }

        // ============================================================ 실험 통 → 방전 → 탈출 시작
        static IEnumerator Tank2(Game g)
        {
            var ui = g.UI; var p = g.Player; var s = g.State;
            p.ScriptControl = true;
            Vector2 tank = M("tank", LuPos(g));
            Vector2 door = M("door", tank + new Vector2(15, 0));
            // 통은 왼쪽 1/3, 연구원이 들어올 오른쪽 공간을 비워 둔다
            Cutscene.CamSnap(new Vector2(tank.x + 2.2f, tank.y + 2.3f));   // 왼쪽 스탠딩에 통이 가리지 않게 가운데 왼쪽
            p.Teleport(tank + new Vector2(0, 2.0f));
            p.AnimOverride = "Fall";
            ui.SetFade(0f);
            Cutscene.Parallel(g, ui.Flash(new Color(0.8f, 0.95f, 1f), 0.8f * FlashK, 0.35f));
            Sfx.Play("sfx_fall_whoosh", 0.6f, 1.2f);
            for (float t = 0; t < 1.2f && !(p.Grounded && t > 0.08f); t += Time.deltaTime) yield return null;
            p.AnimOverride = p.View.HasState("TankCrouch") ? "TankCrouch" : "Land";
            Sfx.Play("sfx_land", 1f); Sfx.Play("sfx_glass_knock", 0.4f, 0.7f);
            Cutscene.Shake(0.06f, 0.15f);
            yield return g.Say("op_t_1");
            p.AnimOverride = p.View.HasState("TankStruggle") ? "TankStruggle" : "Knock";
            for (int i = 0; i < 3; i++)
            {
                p.AnimOverride = i == 1 ? "ReachPC" : "Knock";
                Sfx.Play("sfx_glass_knock",0.5f+i*0.18f,0.78f+i*0.17f);
                Cutscene.Shake(0.025f+i*0.008f,0.15f);
                Fx.Spawn("Bubbles",p.Chest + new Vector2(i%2==0?0.45f:-0.45f,0),false,13,0.6f);
                yield return Cutscene.Wait(i == 1 ? 0.95f : 0.65f);
            }
            p.AnimOverride = "Fear";
            yield return g.Say("op_t_2");
            yield return Cutscene.Wait(0.9f);
            yield return g.Say("op_t_routine");
            yield return g.Say("op_t_discovery");
            // 문이 열리고 연구원 둘이 곧장 걸어온다 — 걸으면서 말한다 (멈춰 서서 기다리는 공백 없음)
            var doorObj = Marker.Find("door");
            var df = doorObj != null ? doorObj.GetComponent<FrameSet>() : null;
            Sfx.Play("sfx_door_bang", 1f); Sfx.Play("sfx_door_slide", 0.8f, 1.2f);
            if (df != null) df.Show(1);
            Cutscene.Shake(0.05f, 0.15f);
            s.Set("op_r_in");
            yield return null;
            var r1 = A("r1"); var r2 = A("r2");
            Vector2 r1Near = M("r1_near", tank + new Vector2(2.6f, 0)), r2Near = M("r2_near", tank + new Vector2(3.8f, 0));
            if (r1 != null) r1.transform.position = door;
            if (r2 != null) r2.transform.position = door + new Vector2(0.7f, 0);
            Cutscene.Parallel(g, Cutscene.Walk(r1, r1Near.x + 0.4f, 3.4f));
            Cutscene.Parallel(g, Cutscene.Walk(r2, r2Near.x, 3.1f));
            p.AnimOverride = p.View.HasState("TankStruggle") ? "TankStruggle" : "Fear";
            yield return g.Say("op_t_r1");
            yield return g.Say("op_t_r3");
            for (float t = 0; t < 1.2f && r1 != null && Mathf.Abs(r1.transform.position.x - (r1Near.x + 0.4f)) > 0.05f; t += Time.deltaTime) yield return null;
            // 제압봉 → 루가 움츠림 → 한 걸음 더 → 방전
            if (r1 != null) { r1.view.Face(-1); r1.view.Play("BatonOut", true); }
            if (r2 != null) { r2.view.Face(-1); r2.view.Play("Talk"); }
            Sfx.Play("sfx_baton_on", 0.9f);
            p.AnimOverride = p.View.HasState("TankCower") ? "TankCower" : "Fear";
            yield return g.Say("op_t_calm1");
            if (r1 != null) r1.view.Play("BatonIdle");
            yield return g.Say("op_t_no");
            if (r1 != null) Cutscene.Parallel(g, Cutscene.Walk(r1, r1Near.x - 0.2f, 1.2f, "BatonWalk", "BatonIdle"));
            yield return g.Say("op_t_stop");
            // ==== 방전 (충전은 짧게, 터지는 순간은 강하게)
            if (AudioManager.I != null) AudioManager.I.PlayAmbience(null, 0.2f);
            p.AnimOverride = "ChargeEyes";
            Cutscene.Parallel(g, LabDischarge(g,tank));
            Sfx.Play("sfx_charge_up", 1f);
            var luGlow = Cutscene.Glow(p.transform.position + Vector3.up * 0.8f, 0.5f, new Color(1f, 0.95f, 0.6f, 0f));
            Cutscene.Temp(luGlow != null ? luGlow.gameObject : null);
            for (float t = 0; t < 0.8f; t += Time.deltaTime)
            {
                float k = t / 0.8f;
                if (luGlow != null) { luGlow.color = new Color(1f, 0.95f, 0.6f, k * 0.85f); luGlow.transform.localScale = Vector3.one * Mathf.Lerp(0.9f, 2.0f, k); }
                if (t > 0.5f) Cutscene.Shake(0.03f, 0.08f);
                yield return null;
            }
            yield return Cutscene.HitStop(0.14f);
            Sfx.Play("sfx_discharge_big", 1f);
            Cutscene.Parallel(g, ui.Flash(new Color(1f, 1f, 0.85f), 0.6f * FlashK, 0.4f));
            Cutscene.Shake(0.35f, 0.7f);
            p.AnimOverride = "Discharge";
            Fx.Spawn("Explosion", p.transform.position + Vector3.up * 0.9f, false, 42, 3f);
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                Fx.Spawn("Arc", p.transform.position + new Vector3(Mathf.Cos(a) * 1.4f, 0.9f + Mathf.Sin(a) * 1.0f), i % 2 == 0, 42, 1.2f);
            }
            s.Set("c1_tank_broken");
            Sfx.Play("sfx_glass_break", 1f);
            yield return null;
            Vector2 r1Down = M("r1_down", tank + new Vector2(5.5f, 0)), r2Down = M("r2_down", tank + new Vector2(7.2f, 0));
            if (r1 != null) { r1.view.Play("Hurt", true); Cutscene.Parallel(g, Cutscene.Arc(r1.transform, r1Down, 0.8f, 0.45f)); }
            if (r2 != null) { r2.view.Play("Hurt", true); Cutscene.Parallel(g, Cutscene.Arc(r2.transform, r2Down, 0.9f, 0.5f)); }
            ReactionParticles.Emit(p.Chest,1,false,60,1.4f);
            foreach (var d in Marker.Prefix("debris"))
            {
                var t = d.transform; Vector3 final = t.position;
                t.position = final + Vector3.up * 5f;
                Cutscene.Parallel(g, DropDebris(t, final, Random.Range(0.15f, 0.4f)));
            }
            yield return Cutscene.Wait(0.8f);
            Sfx.Play("sfx_power_down", 1f);
            s.Set("c1_researchers_down");
            if (df != null) df.Show(2);
            if (luGlow != null) Cutscene.Parallel(g, Cutscene.FadeSprite(luGlow, 0f, 0.8f));
            p.AnimOverride = "Recover";
            yield return Cutscene.Wait(0.8f);
            yield return EscapeStart(g);
        }

        /// <summary>
        /// 탈출 시작 (사용자 지정 장면): "여.. 여긴 뭐야?" → 카메라가 수술대·구속 장치·다른 실험통·출구를 짧게 훑음 →
        /// "여긴... 실험실?" "욱..." (입을 가리고 몸을 숙임) → "젠장.. 내가 또 이런 데에 오게 될 줄이야." "윽..."
        /// (머리 근처 잔류 스파크, 비틀거리며 머리를 감싸 쥠) → "젠장... 뭐가 됐든 여길 나가야겠어..." → 즉시 조작.
        /// </summary>
        static IEnumerator EscapeStart(Game g)
        {
            var p = g.Player; var s = g.State;
            Vector2 tank = M("tank", LuPos(g));
            p.AnimOverride = "GetUp";
            yield return Cutscene.Wait(0.35f);
            p.AnimOverride = "Idle"; p.SetFacing(-1);
            yield return g.Say("esc_1");
            // 짧은 훑기: 왼쪽 수술대·구속대 → 오른쪽 출구 (관광용 긴 이동 없이)
            Vector2 bed = M("bed", tank + new Vector2(-20f, 0));
            Vector2 exit = M("door", tank + new Vector2(15, 0));
            p.AnimOverride = "LookAround";
            yield return Cutscene.Pan(new Vector2(bed.x + 5.5f, bed.y + 2.3f), 0.85f);
            yield return Cutscene.Wait(0.3f);
            p.SetFacing(1);
            yield return Cutscene.Pan(new Vector2(exit.x - 4.5f, exit.y + 2.3f), 1.1f);
            yield return Cutscene.Wait(0.2f);
            yield return Cutscene.Pan(new Vector2(tank.x + 2.5f, tank.y + 2.0f), 0.6f);
            p.SetFacing(-1);
            p.AnimOverride = "Idle";
            yield return g.Say("esc_2");
            yield return Cutscene.Wait(0.55f);

        }

        static IEnumerator CorridorMemory(Game g)
        {
            var p = g.Player;
            p.ScriptControl = true;
            p.AnimOverride = p.View.HasState("Disgust") ? "Disgust" : "Sad";
            yield return g.Say("esc_3");
            p.AnimOverride = p.View.HasState("HeadHold") ? "HeadHold" : "Hurt";
            Fx.Spawn("Spark",p.Chest+Vector2.up*0.55f,false,31,0.7f);
            yield return Cutscene.Wait(0.75f);
            yield return g.Say("esc_4");
        }

        static IEnumerator LabDischarge(Game g, Vector2 tank)
        {
            var arcs = new List<LineRenderer>();
            for(int i=0;i<8;i++) arcs.Add(Line(g,new Color(0.65f,0.88f,1f,0.9f),i<3?0.065f:0.035f,33));
            for(float t=0;t<3.4f;t+=Time.deltaTime)
            {
                float reach = t<0.85f ? Mathf.Lerp(0.4f,1.4f,t/0.85f) : t<1.7f ? 6f : Mathf.Lerp(4f,0.3f,(t-1.7f)/1.7f);
                for(int i=0;i<arcs.Count;i++)
                {
                    var lr=arcs[i]; lr.positionCount=9;
                    Vector2 origin=tank+Vector2.up*0.9f;
                    Vector2 end=origin+new Vector2(Mathf.Cos(i*Mathf.PI/4)*reach,Mathf.Sin(i*Mathf.PI/4)*reach*0.55f);
                    for(int j=0;j<9;j++) lr.SetPosition(j,Vector2.Lerp(origin,end,j/8f)+(j==0||j==8?Vector2.zero:Random.insideUnitCircle*0.22f));
                    lr.enabled=t<1.8f || (int)(t*12+i)%4==0;
                }
                yield return null;
            }
            foreach(var lr in arcs) if(lr!=null) lr.enabled=false;
        }

        static void FinTank2(Game g)
        {
            var s = g.State;
            foreach (var f in new[] { "op_r_in", "c1_tank_broken", "c1_researchers_down", "op_tank_done", "lab_woke", "lab_realized", "lab_power" }) s.Set(f);
            for (int i = 1; i <= 4; i++) s.Set("lab_power_" + i);
            s.Set(Facility.AlarmFlag);
            GameState.AddUnique(s.D.dialogueSeen, "lab_broadcast");
            var door = Marker.Find("door");
            var df = door != null ? door.GetComponent<FrameSet>() : null;
            if (df != null) df.Show(2);
            foreach (var d in Marker.Prefix("debris")) d.GetComponent<DebrisHome>()?.Home();
            ActorSpawn.ApplyDownAll();
            var p = g.Player;
            if (p != null)
            {
                Vector2 tank = Marker.Pos("tank", p.transform.position);
                if (Mathf.Abs(p.transform.position.x - tank.x) > 2.5f) p.Teleport(tank);
                p.SetFacing(1);
            }
            if (AudioManager.I != null) AudioManager.I.PlayAmbience("amb_lab_alarm", 0.5f);
            g.UI.SetFade(0f);
        }

        /// <summary>탈출 시작 직후: 붉은 경보 + 방송 (조작을 막지 않는 띠 안내), 구간 시작점 저장.</summary>
        static void AfterTankControl(Game g)
        {
            Sfx.Play("sfx_alarm", 0.8f);
            g.UI.Toast("〔방송〕 격리 구역 이탈 감지. 실험체 E-D05-47 — 보안 요원은 확보하십시오.", 4f);
            g.UI.hud.SetHint("오른쪽 비틀린 문틈으로 — " + GameInput.Label(Act.Interact) + " 조사");
            g.StartCoroutine(ClearHintLater(g, 7f));
            g.Checkpoint("c1_escape");
        }

        static IEnumerator LabPouch(Game g)
        {
            var s = g.State;
            if (!s.Once("got_pouch")) yield break;
            s.AddItem("chocolate");
            yield return g.Say("lab_pouch");
            g.UI.Toast("초콜릿을 챙겼다.", 2f);
        }

        // ============================================================ 전원선 이동 (R) — 동행 중 첫 장면
        public static IEnumerator WireTravel(Game g, PowerLine line)
        {
            var s = g.State; var p = g.Player;
            var comp = g.Companion;
            bool ally = comp != null && s.VenelleIsAlly;
            if (!ally) { yield return line.Travel(g, p.transform, p.View, true); yield break; }
            bool first = !s.Has("wire_duo_done");
            if (first)
            {
                bool skipped = false;
                yield return Cutscene.Play(g, "wire_duo_first", WireDuoA(g, comp), gg => { gg.State.Set("wire_duo_done"); skipped = Cutscene.LastSkipped; });
                line.carryCompanion = comp;
                if (!skipped) g.StartCoroutine(WireEnQuip(g, line));
                yield return line.Travel(g, p.transform, p.View, true);
                if (!skipped) yield return Cutscene.Play(g, "wire_duo_arrive", WireDuoB(g, comp), null);
            }
            else
            {
                // 이후: 짧은 손잡기 동작만 보이고 바로 이동 (같은 대화 반복 없음)
                Vector2 side = (Vector2)p.transform.position + Vector2.left * p.Facing * 0.7f;
                yield return comp.PullTo(side, 0.18f);
                p.AnimOverride = p.View.HasState("HoldHand") ? "HoldHand" : "Idle";
                yield return new WaitForSeconds(0.12f);
                p.AnimOverride = null;
                line.carryCompanion = comp;
                yield return line.Travel(g, p.transform, p.View, true);
            }
        }

        static IEnumerator WireDuoA(Game g, Companion comp)
        {
            var p = g.Player;
            p.ScriptControl = true; p.AnimOverride = "Idle";
            comp.View.Face(p.transform.position.x > comp.transform.position.x ? 1 : -1);
            yield return g.Say("w_first_1");
            yield return g.Say("w_first_2");
            // 루가 베넬의 손을 붙잡는다 (접촉 유지) → 둘이 함께 전기화
            Vector2 side = (Vector2)p.transform.position + Vector2.left * p.Facing * 0.7f;
            yield return comp.PullTo(side, 0.25f);
            p.AnimOverride = p.View.HasState("HoldHand") ? "HoldHand" : "Idle";
            Sfx.Play("sfx_connect", 0.8f, 1.2f);
            Fx.Spawn("Spark", (p.transform.position + comp.transform.position) / 2f + Vector3.up * 0.7f, false, 31, 0.6f);
            yield return Cutscene.Wait(0.3f);
        }

        static IEnumerator WireEnQuip(Game g, PowerLine line)
        {
            yield return new WaitForSeconds(0.35f);
            var mid = line.path != null && line.path.Length > 0 ? (Vector2)line.path[line.path.Length / 2] : (Vector2)line.transform.position + Vector2.up;
            g.UI.AddLog("베넬", "엥?!");
        }

        static IEnumerator WireDuoB(Game g, Companion comp)
        {
            var p = g.Player;
            p.ScriptControl = true; p.AnimOverride = "Idle";
            comp.View.Face(p.transform.position.x > comp.transform.position.x ? 1 : -1);
            yield return g.Say("w_first_4");
        }

        static IEnumerator M15GapLook(Game g)
        {
            var s = g.State;
            if (s.VenelleIsAlly) yield break;      // 동행 중이면 첫 전선 장면에서 베넬이 말한다
            if (s.Once("m15_gap_said")) g.Monologue("끊겼어... 저 콘센트로 건너가자. (" + GameInput.Label(Act.Skill4) + ")");
        }

        // ============================================================ 베넬 복도
        static IEnumerator VenelleCorridor2(Game g)
        {
            var s = g.State;
            if (s.Has("c3_boss_done")) yield break;
            s.D.chapterId = "ch3";
            if (!s.Has("c3_boss_intro"))
                yield return Cutscene.Play(g, "c3_boss_intro", VenelleIntro2(g), FinVenelleIntro2);
            StartVenelleFight2(g);
        }

        static VenelleBoss Boss() => Object.FindAnyObjectByType<VenelleBoss>();

        /// <summary>
        /// 헉헉대며 들어옴 → 드르륵, 쾅! → 또각, 또각 → 걸으며 투덜거림 → 알아봄 → 루 주춤 → 뒤 문 쾅! → 경광등·위이잉 →
        /// 표정 변화 "내가 그 꼬맹이를 찾은 것 같네!!!!" → (조작) 첫 돌진. 대사와 걸음이 겹친다. 확대 없이 한 화면.
        /// </summary>
        static IEnumerator VenelleIntro2(Game g)
        {
            var p = g.Player; var v = A("venelle"); var s = g.State;
            p.ScriptControl = true;
            s.Set("c6_door_open");
            Vector2 enter = M("lu_enter", LuPos(g));
            Vector2 lean = M("lu_lean", enter + new Vector2(5f, 0));
            Vector2 vStart = M("venelle_start", enter + new Vector2(21f, 0));
            Vector2 vStop = M("venelle_stop", lean + new Vector2(8.4f, 0));
            Vector2 bdoor = M("back_door", enter + new Vector2(2.2f, 0));
            p.Teleport(enter);
            if (v != null) { v.transform.position = vStart; v.view.Face(-1); v.view.SetVisible(false); var spawn=v.GetComponent<ActorSpawn>();if(spawn!=null)spawn.HideForCutscene=true; }
            // 한 화면: 왼쪽 뒤 문 · 루 · 돌진 거리 · 베넬이 멈출 자리
            Cutscene.CamSnap(new Vector2((bdoor.x + vStop.x) / 2f + 1.6f, lean.y + 2.2f));
            if (AudioManager.I != null) { AudioManager.I.PlayMusic(null, 1f); AudioManager.I.PlayAmbience("amb_lab_alarm", 0.4f, 0.45f); }
            g.UI.SetFade(1f);
            yield return g.UI.Fade(0f, 0.3f);
            yield return Cutscene.LuMove(g, lean.x, 5.8f, "Walk");
            p.AnimOverride = p.View.HasState("WallLean") ? "WallLean" : "LimpIdle";
            p.SetFacing(1);
            yield return g.Say("v_run_in");
            p.AnimOverride = "LookAround";
            yield return g.Say("v_where");
            yield return Cutscene.Wait(0.65f);
            p.AnimOverride = "Sit";
            Sfx.Play("sfx_cloth",0.4f);
            yield return Cutscene.Wait(1.55f);
            var entryDoor=Marker.Find("venelle_door");
            var entryFrames=entryDoor!=null?entryDoor.GetComponent<FrameSet>():null;
            if(entryFrames!=null)entryFrames.Show(1);
            if(v != null) { var spawn=v.GetComponent<ActorSpawn>();if(spawn!=null)spawn.HideForCutscene=false; v.view.SetVisible(true); }
            // 드르륵, 쾅! (복도 저편 시설 문)
            Sfx.Play("sfx_door_slide", 0.8f, 0.9f);
            yield return Cutscene.Wait(0.4f);
            p.AnimOverride = "Fear";
            yield return g.Say("v_yawn");
            Sfx.Play("sfx_door_bang", 0.9f);
            Cutscene.Shake(0.04f, 0.15f);
            // 또각, 또각 — 걸으며 말한다
            if (v != null) Cutscene.Parallel(g, VenelleWalkIn(g, v, vStop.x + 1.4f));
            yield return Cutscene.Wait(0.35f);
            yield return g.Say("v_intro_1");
            yield return g.Say("v_intro_2");
            if (v != null) yield return Cutscene.Walk(v, vStop.x, 1.8f, v.view.HasState("TalkWalk") ? "TalkWalk" : "Walk", "Idle");
            // 알아봄 → 루가 살짝 주춤
            if (v != null) { v.view.Play("Idle"); v.view.Face(-1); }
            yield return Cutscene.Wait(0.2f);
            p.AnimOverride = "BackAway";
            yield return Cutscene.LuMove(g, lean.x - 0.45f, 1.6f, "BackAway", 0.6f);
            // 뒤 문이 쾅 닫힌다
            s.Set("c6_door_open", false);
            Sfx.Play("sfx_door_bang", 1f, 0.85f);
            Cutscene.Shake(0.1f, 0.2f);
            p.SetFacing(-1);
            yield return Cutscene.Wait(0.25f);
            p.SetFacing(1);
            // 경광등 회전 + 위이잉
            Sfx.Play("sfx_alarm", 1f, 0.9f);
            Cutscene.Parallel(g, g.UI.Flash(new Color(1f, 0.15f, 0.1f), 0.22f * FlashK, 0.4f));
            // 표정·자세가 바뀐다
            if (v != null) v.view.Play(v.view.HasState("Laugh") ? "Laugh" : "Idle");
            yield return g.Say("v_intro_3");
            if (v != null) v.view.Play(v.view.HasState("Windup") ? "Windup" : "Idle");
            p.AnimOverride = "Fear";
            yield return Cutscene.Wait(0.15f);
            p.AnimOverride = null;
        }

        static IEnumerator VenelleWalkIn(Game g, Actor v, float x)
        {
            float step = 0;
            var anim = v.view.HasState("TalkWalk") ? "TalkWalk" : "Walk";
            v.view.Face(-1); v.view.Play(anim);
            bool onoma = false;
            while (v != null && v.transform.position.x > x + 0.02f)
            {
                var pos = v.transform.position;
                pos.x = Mathf.MoveTowards(pos.x, x, 1.9f * Time.deltaTime);
                v.transform.position = pos;
                step += Time.deltaTime;
                if (step > 0.42f) { step = 0; Sfx.Play("sfx_step", 0.55f, 1.25f); if (!onoma) { onoma = true; Onoma(g, (Vector2)pos + new Vector2(0, 0.2f), "또각, 또각...", new Color(0.85f, 0.85f, 0.95f), 1.4f, 11); } }
                yield return null;
            }
            if (v != null) v.view.Play("Idle");
        }

        static void FinVenelleIntro2(Game g)
        {
            var s = g.State;
            s.Set("c3_boss_intro");
            s.Set("c6_combat_locked",true);
            s.Set("c6_door_open", false);
            var v = Actor.Find("venelle");
            var stop = Marker.Find("venelle_stop");
            if (v != null && stop != null) { var spawn=v.GetComponent<ActorSpawn>();if(spawn!=null)spawn.HideForCutscene=false; v.transform.position = stop.transform.position; v.view.Face(-1); v.view.SetVisible(true); }
            var lean = Marker.Find("lu_lean");
            // 정상 진입·건너뛰기 진입 모두 같은 거리에서 시작 (첫 돌진을 피할 여유 동일)
            if (g.Player != null && lean != null) g.Player.Teleport((Vector2)lean.transform.position + Vector2.left * 0.45f);
            if (g.Player != null) g.Player.SetFacing(1);
            if (AudioManager.I != null) AudioManager.I.PlayMusic("bgm_tension", 0.8f, 0.8f);
        }

        static void StartVenelleFight2(Game g)
        {
            var b = Boss();
            if (b == null) { Debug.LogError("[EW] 베넬 복도에 VenelleBoss 가 없습니다"); return; }
            var s = g.State;
            var stop = Marker.Find("venelle_stop");
            if (stop != null && b.State == VenelleBoss.VState.Inactive) b.transform.position = stop.transform.position;
            // 시설 경보는 베넬이 나타난 뒤 여기서 끝난다 (복도 잠금 경보 이후 전투 음악)
            s.Set(Facility.AlarmFlag, false);
            if (AudioManager.I != null) { AudioManager.I.PlayAmbience("amb_lab_dark", 1f); if (AudioManager.I.CurrentMusic != "bgm_tension") AudioManager.I.PlayMusic("bgm_tension", 0.8f, 0.8f); }
            VenelleMind.InitIfNeeded();
            b.Activate();
            if (s.Once("cp_venelle_fight")) g.Checkpoint("venelle_fight");
            g.UI.hud.SetHint(GameInput.Label(Act.Dodge) + " · 대시 · 숨 고를 때·약을 섞을 때 " + GameInput.Label(Act.Interact) + " 말 걸기");
            g.StartCoroutine(ClearHintLater(g, 8f));
        }

        // ---- 협상: 선택 하나의 효과 (한 번만 적용, 반복하면 기억하는 반응)
        static IEnumerator ApplyOption(Game g, VenelleBoss b, string opt)
        {
            var s = g.State;
            bool first = s.Once("v_opt_" + opt);
            bool hitRecently = Time.time - b.LastHitByLuTime < 8f;
            switch (opt)
            {
                case "release":
                    if (hitRecently) { yield return g.Say("v_r_release_bad"); VenelleMind.AddAgit(1); if (first) s.Set("v_opt_release", false); }
                    else if (first) { yield return g.Say("v_r_release_ok"); VenelleMind.AddThreat(-2); VenelleMind.AddTrust(1); }
                    else yield return g.Say("v_r_release_again");
                    break;
                case "leave":
                    if (first) { yield return g.Say("v_r_leave_first"); VenelleMind.AddTrust(2); }
                    else yield return g.Say("v_r_leave_again");
                    break;
                case "why":
                    if (first) { yield return g.Say("v_r_why_first"); VenelleMind.AddTrust(1); VenelleMind.AddAgit(-1); }
                    else { yield return g.Say("v_r_why_again"); VenelleMind.AddAgit(1); }
                    break;
                case "provoke":
                    yield return g.Say(first ? "v_r_provoke_first" : "v_r_provoke_again");
                    VenelleMind.AddAgit(2); VenelleMind.AddThreat(1);
                    b.Express("Laugh", 0.8f);
                    break;
                case "hands":
                    if (hitRecently) { yield return g.Say("v_r_hands_bad"); if (first) s.Set("v_opt_hands", false); }
                    else if (first) { yield return g.Say("v_r_hands_ok"); VenelleMind.AddThreat(-2); VenelleMind.AddTrust(1); }
                    else yield return g.Say("v_r_hands_again");
                    break;
            }
            s.SetVar("v_talks", s.GetVar("v_talks") + 1);
        }

        /// <summary>잡혀 들어 올려진 채 대화 (피해·독·위험 타이머 정지). 처음은 길게, 다시 잡히면 짧게.</summary>
        public static IEnumerator VenelleGrabTalk(Game g, VenelleBoss b)
        {
            var s = g.State;
            bool first = s.Once("v_grab_talked");
            yield return g.Ask(first ? "v_grab_first" : "v_grab_again");
            string[] map = { "release", "leave", "why", "provoke" };
            yield return ApplyOption(g, b, map[Mathf.Clamp(g.LastChoice, 0, 3)]);
            if (VenelleMind.WillStop)
            {
                // 내려놓는다 (던지지 않음)
                var p = g.Player;
                p.Frozen = false; p.ScriptControl = false; p.AnimOverride = null;
                p.Teleport(new Vector2(b.transform.position.x + b.Facing * 1.0f, b.transform.position.y));
                yield return VenelleEnd2(g, true);
                yield break;
            }
            yield return b.ThrowLu();
            g.Player.Skills?.CancelTargeting();
        }

        /// <summary>숨 고르기·약 섞기 빈틈에 J 로 말 걸기.</summary>
        public static IEnumerator VenelleTalk(Game g, VenelleBoss b)
        {
            g.Player.SetFacing(b.transform.position.x > g.Player.transform.position.x ? 1 : -1);
            yield return g.Ask("v_talk_open");
            string[] map = { "leave", "why", "provoke", "hands" };
            yield return ApplyOption(g, b, map[Mathf.Clamp(g.LastChoice, 0, 3)]);
            if (VenelleMind.WillStop) { yield return VenelleEnd2(g, true); yield break; }
            g.Player.Skills?.CancelTargeting();
            b.AfterTalk();
        }

        /// <summary>쓰러진 베넬에게 다가가 J: 죽인다 / 죽이지 않는다 (기본 선택 없음 — A/D 로 고른 뒤 J).</summary>
        public static IEnumerator VenelleDownedChoice(Game g, VenelleBoss b)
        {
            var s = g.State; var p = g.Player;
            if (s.Has("c3_boss_done")) yield break;
            p.SetFacing(b.transform.position.x > p.transform.position.x ? 1 : -1);
            p.ScriptControl = true; p.AnimOverride = "Idle";
            yield return g.Say("v_down_1");
            yield return g.AskEx("v_down_ask", null, true);
            if (g.LastChoice == 0)
            {
                yield return g.Say("v_kill");
                p.AnimOverride = "Discharge";
                Sfx.Play("sfx_discharge_big", 0.9f);
                Cutscene.Shake(0.2f, 0.4f);
                yield return g.UI.Flash(Color.white, 0.8f * FlashK, 0.3f);
                b.Deactivate();
                var v = A("venelle"); if (v != null) v.view.Play("Down", true);
                s.D.venelleState = "dead"; s.D.venelle.state = "dead"; s.D.venelleDeathScene = "c06_corridor";
                s.Set("venelle_dead"); s.Set("c3_killed"); s.Set("c3_boss_done"); s.Set("c6_combat_locked",false); s.Set("c3_subdued");
                yield return g.UI.Fade(1f, 0.6f);
                if (v != null) v.view.SetVisible(false);
                p.AnimOverride = "Idle";
                yield return g.UI.Fade(0f, 0.6f);
                yield return g.Say("v_kill_after");
                p.ScriptControl = false; p.AnimOverride = null;
                if (AudioManager.I != null) AudioManager.I.PlayMusic(null, 2f);
                g.UI.hud.SetHint("");
                g.Checkpoint("c3_after_kill");
                yield break;
            }
            yield return g.Say("v_spare");
            s.Set("venelle_spared");
            if (s.Once("v_trust_spare")) VenelleMind.AddTrust(2);
            yield return VenelleEnd2(g, false);
        }

        /// <summary>협상 성립(공격 중단) 또는 제압 뒤 살림 → 긴장이 풀린 뒤 쓰러지는 장면 → 짧은 부축 → 컷 → 관찰 휴게실 벤치.</summary>
        static IEnumerator VenelleEnd2(Game g, bool negotiated)
        {
            var s = g.State; var p = g.Player;
            if (s.Has("c3_boss_done")) yield break;
            var b = Boss();
            if (b != null) b.Stop();
            g.UI.hud.SetHint("");
            if (AudioManager.I != null) AudioManager.I.PlayMusic(null, 2f);
            var v = A("venelle");
            p.ScriptControl = true; p.AnimOverride = "Idle";
            if (negotiated)
            {
                s.Set("c3_negotiated"); s.Reward("negotiate", ActionTuning.I.expNegotiate);
                if (v != null) v.view.Play("Idle");
                yield return new WaitForSeconds(0.6f);
                yield return g.Say("v_stop");
                yield return new WaitForSeconds(0.5f);
                yield return g.Say("v_release");
                yield return g.Say("c3_success");
            }
            else s.Set("c3_subdued");
            s.Reward("section_C06", ActionTuning.I.expSection);
            if (v != null) v.view.Play("Collapsed");
            yield return g.Say("c3_collapse_common");
            if (v != null) yield return p.WalkTo(v.transform.position.x - 0.8f * Mathf.Sign(v.transform.position.x - p.transform.position.x + 0.001f));
            yield return g.Say(negotiated ? "c3_collapse_nego" : "c3_collapse_sub");
            // 짧은 부축 (같이 걷기 시작하는 모습만) → 컷
            s.Set("c3_boss_done"); s.Set("c6_combat_locked",false);
            s.D.venelle.hp = Mathf.CeilToInt(s.MaxHp("venelle") * 0.5f);
            if (v != null) v.view.Play(v.view.HasState("Supported") ? "Supported" : "Walk");
            p.AnimOverride = "Carry";
            // The short supported walk conveys the move to the lounge without an explanatory subtitle.
            float dir = 1;
            for (float t = 0; t < 1.1f; t += Time.deltaTime)
            {
                p.transform.position += new Vector3(dir * 1.3f * Time.deltaTime, 0, 0);
                if (v != null) v.transform.position += new Vector3(dir * 1.3f * Time.deltaTime, 0, 0);
                yield return null;
            }
            yield return g.UI.Fade(1f, 0.35f);
            p.AnimOverride = null; p.ScriptControl = false;
            s.Set("venelle_bench"); s.Set("c3_to_rest");
            g.Checkpoint("c3_after");
            yield return g.LoadMap("M15", "rest_seat");
        }

        // ============================================================ 휴식 (관찰 휴게실 벤치): 짧은 이동, 충분한 대화
        static IEnumerator RestScene(Game g)
        {
            var s = g.State; var p = g.Player;
            s.Set("c3_to_rest", false);
            s.Set("c3_rest_done");
            var bench = GameObject.Find("RestBench");
            float bx = bench != null ? bench.transform.position.x : p.transform.position.x;
            p.Teleport(new Vector2(bx + 1.1f, p.transform.position.y));
            p.ScriptControl = true; p.SetFacing(-1); p.AnimOverride = "Sit";
            g.UI.SetFade(1f);
            yield return g.UI.Fade(0f, 0.5f);
            yield return g.Say("r_1");
            yield return g.Say("r_2");
            yield return g.Say("r_3");
            yield return g.Say("r_4");
            bool has = s.Count("chocolate") > 0;
            if (has)
            {
                yield return g.Ask("r_choc");
                if (g.LastChoice == 0)
                {
                    s.UseItem("chocolate");
                    yield return g.Say("r_choc_give");
                    if (s.Once("gave_chocolate")) { VenelleMind.AddTrust(2); VenelleMind.AddThreat(-1); }
                }
                else yield return g.Say("r_choc_keep");
            }
            else yield return g.Say("r_nochoc");
            yield return g.Say("r_6");
            if (s.Once("v_trust_honest")) VenelleMind.AddTrust(1);
            yield return g.Say("r_7");
            yield return g.Ask("r_8");
            bool join = false;
            if (g.LastChoice == 0)
            {
                if (VenelleMind.Trust >= 3) { yield return g.Say("r_join_ok"); join = true; }
                else yield return g.Say("r_join_no");
            }
            else yield return g.Say("r_apart");
            p.AnimOverride = null; p.ScriptControl = false;
            if (join)
            {
                s.D.venelleState = "ally"; s.D.venelle.state = "stable";
                s.D.venelle.hp = s.MaxHp("venelle");
                s.Set("c3_joined"); s.Set("venelle_bench", false);
                var seat = A("venelle_rest");
                Vector2 at = seat != null ? (Vector2)seat.transform.position : (Vector2)p.transform.position + Vector2.left;
                g.Companion = Companion.Spawn(g, "venelle", at, 1);
                g.Bubbles.Request("first_walk");
            }
            else
            {
                s.D.venelleState = "apart"; s.Set("venelle_apart");
            }
            g.Checkpoint("c3_rest");
        }
    }

    /// <summary>작중 단말의 명령 프롬프트 같은 창 (연출 전용 — 실제 명령을 실행하지 않는다). 중요 줄은 노란색으로 강조.</summary>
    public class TerminalWindow
    {
        public readonly RectTransform root;
        readonly Text body;
        readonly List<string> lines = new List<string>();
        public TerminalWindow(Transform parent)
        {
            root = UIKit.Rect("Terminal", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UIKit.Place(root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(140, 80), new Vector2(260, 150));
            UIKit.Solid(root, "Bg", new Color(0.03f, 0.04f, 0.05f, 0.96f));
            var bar = UIKit.Rect("Bar", root, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -13), Vector2.zero);
            UIKit.Solid(bar, "B", new Color(0.2f, 0.22f, 0.26f));
            var t = UIKit.Label(bar, "T", "retrieval.exe — 원격 세션", 8, TextAnchor.MiddleLeft, new Color(0.8f, 0.82f, 0.88f));
            t.rectTransform.offsetMin = new Vector2(5, 0);
            body = UIKit.Label(root, "Body", "", 9, TextAnchor.LowerLeft, new Color(0.6f, 0.85f, 0.65f));
            body.font = UIKit.Pixel; body.supportRichText = true;
            body.rectTransform.offsetMin = new Vector2(6, 4); body.rectTransform.offsetMax = new Vector2(-6, -16);
            body.verticalOverflow = VerticalWrapMode.Truncate;
            root.SetAsLastSibling();
        }

        public void Add(string line, bool key)
        {
            lines.Add(key ? "<color=#ffe680><b>" + line + "</b></color>" : line);
            if (lines.Count > 13) lines.RemoveAt(0);
            body.text = string.Join("\n", lines);
        }
    }
}
