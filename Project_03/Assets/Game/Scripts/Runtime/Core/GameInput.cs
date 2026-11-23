using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EW
{
    /// <summary>
    /// 입력 동작. 키 변경은 동작 단위로 한다 (Docs/INPUT_GUIDE.md).
    /// Skill1~4 = Q/E/R/F 직접 슬롯. MenuUp(기본 W)은 사다리·단말·메뉴의 위 이동이다.
    /// </summary>
    public enum Act { Left, Right, Jump, Down, Dodge, Skill1, Skill2, Skill3, Skill4, Interact, Cancel, Skip, MenuUp }

    /// <summary>
    /// 입력을 받는 상태. 한 프레임에는 하나의 상태만 입력을 받는다 (위에 있는 상태가 우선).
    /// Gameplay 아래에 Targeting·Cutscene·Dialogue·Menu·Pause 가 쌓인다.
    /// </summary>
    public enum InputCtx { Gameplay, Targeting, Cutscene, Dialogue, Menu, Pause }

    /// <summary>
    /// 상태별 입력과 키 재설정. 필드 A/D 이동, Space 조사 우선/대상이 없으면 점프,
    /// W/S 사다리, AA/DD 대시, Q/E/R/F 직접 기술. 단말은 WASD 무중력 이동.
    /// 대화 Space 문장 완성/다음, 방향키 선택, Enter 컷신 스킵.
    /// 같은 키의 동작은 함께 소비하고 상태 전환 뒤에는 키를 놓기까지 잠근다.
    /// </summary>
    public static class GameInput
    {
        // ------------------------------------------------------------ 바인딩
        public static readonly Act[] AllActs = (Act[])Enum.GetValues(typeof(Act));
        /// <summary>키 변경 화면에 보이는 순서.</summary>
        public static readonly Act[] RebindOrder = { Act.Left, Act.Right, Act.Jump, Act.Down, Act.Skill1, Act.Skill2, Act.Skill3, Act.Skill4, Act.Interact, Act.Cancel, Act.Skip, Act.MenuUp };
        static readonly Dictionary<Act, Key> defaults = new Dictionary<Act, Key>
        {
            { Act.Left, Key.A }, { Act.Right, Key.D }, { Act.Jump, Key.Space }, { Act.Down, Key.S },
            { Act.Dodge, Key.None },
            { Act.Skill1, Key.Q }, { Act.Skill2, Key.E }, { Act.Skill3, Key.R }, { Act.Skill4, Key.F },
            { Act.Interact, Key.Space }, { Act.Cancel, Key.Escape }, { Act.Skip, Key.Enter }, { Act.MenuUp, Key.W },
        };
        /// <summary>바꿀 수 없는 보조 키 (방향키·숫자패드 Enter).</summary>
        static readonly Dictionary<Act, Key> fixedAlt = new Dictionary<Act, Key>
        {
            { Act.Left, Key.LeftArrow }, { Act.Right, Key.RightArrow }, { Act.MenuUp, Key.UpArrow }, { Act.Down, Key.DownArrow },
            { Act.Skip, Key.NumpadEnter },
        };
        const string Prefix = "ew.key3.";   // 키 배치 개편(2026-11-15): 옛 저장 키(ew.key.)는 읽지 않는다
        static readonly Dictionary<Act, Key> bound = new Dictionary<Act, Key>();
        static bool loaded;

        static void LoadBindings()
        {
            if (loaded) return;
            loaded = true;
            bound.Clear();
            foreach (var a in AllActs)
            {
                Key k = defaults[a];
                string s = PlayerPrefs.GetString(Prefix + a, "");
                if (a != Act.Dodge && !string.IsNullOrEmpty(s) && Enum.TryParse(s, out Key parsed) && parsed != Key.None) k = parsed;
                bound[a] = k;
            }
        }

        public static Key KeyOf(Act a) { LoadBindings(); return bound[a]; }
        public static Key DefaultKeyOf(Act a) => defaults[a];

        /// <summary>
        /// 조사/점프만 키를 공유한다. 위 이동은 사다리·단말에서도 사용하므로 기술과 겹치지 않는다.
        /// </summary>
        public static bool CanShare(Act a, Act b)
        {
            return (a == Act.Jump && b == Act.Interact) || (b == Act.Jump && a == Act.Interact);
        }

        /// <summary>키 변경. 같은 키를 쓸 수 없는 동작이 그 키를 쓰고 있으면 두 동작의 키를 맞바꾼다.</summary>
        public static void Rebind(Act a, Key k)
        {
            LoadBindings();
            if (a == Act.Dodge || k == Key.None) return;
            foreach (var other in AllActs)
                if (other != a && bound[other] == k && !CanShare(a, other)) { bound[other] = bound[a]; PlayerPrefs.SetString(Prefix + other, bound[other].ToString()); }
            bound[a] = k;
            PlayerPrefs.SetString(Prefix + a, k.ToString());
            PlayerPrefs.Save();
        }

        public static void ResetBindings()
        {
            foreach (var a in AllActs) PlayerPrefs.DeleteKey(Prefix + a);
            PlayerPrefs.Save();
            loaded = false; LoadBindings();
        }

        /// <summary>안내 문구용 키 이름 (실제 바인딩).</summary>
        public static string Label(Act a) => a == Act.Dodge ? Label(Act.Left) + Label(Act.Left) + " / " + Label(Act.Right) + Label(Act.Right) : KeyName(KeyOf(a));

        public static string KeyName(Key k)
        {
            switch (k)
            {
                case Key.Space: return "Space";
                case Key.Enter: return "Enter";
                case Key.NumpadEnter: return "Enter";
                case Key.Escape: return "Esc";
                case Key.LeftShift: case Key.RightShift: return "Shift";
                case Key.LeftCtrl: case Key.RightCtrl: return "Ctrl";
                case Key.LeftAlt: case Key.RightAlt: return "Alt";
                case Key.Tab: return "Tab";
                case Key.Backspace: return "Backspace";
                case Key.LeftArrow: return "←";
                case Key.RightArrow: return "→";
                case Key.UpArrow: return "↑";
                case Key.DownArrow: return "↓";
            }
            string s = k.ToString();
            if (s.StartsWith("Digit")) return s.Substring(5);
            return s.Length == 1 ? s.ToUpperInvariant() : s;
        }

        /// <summary>"A/D" 같은 짝 표기.</summary>
        public static string Pair(Act a, Act b) => Label(a) + "/" + Label(b);

        public static Act SkillAct(int slot) => slot == 0 ? Act.Skill1 : slot == 1 ? Act.Skill2 : slot == 2 ? Act.Skill3 : Act.Skill4;

        // ------------------------------------------------------------ 상태 스택
        struct CtxEntry { public int handle; public InputCtx ctx; }
        static readonly List<CtxEntry> stack = new List<CtxEntry>();
        static int nextHandle = 1;

        /// <summary>지금 입력을 받는 상태.</summary>
        public static InputCtx Context
        {
            get
            {
                InputCtx best = InputCtx.Gameplay;
                foreach (var e in stack) if (e.ctx > best) best = e.ctx;
                return best;
            }
        }

        /// <summary>상태를 연다. 돌려받은 번호로 Pop 한다 (순서가 엇갈려도 안전).</summary>
        public static int Push(InputCtx c)
        {
            var before = Context;
            int h = nextHandle++;
            stack.Add(new CtxEntry { handle = h, ctx = c });
            if (Context != before) OnContextChanged();
            return h;
        }

        public static void Pop(int handle)
        {
            var before = Context;
            stack.RemoveAll(e => e.handle == handle);
            if (Context != before) OnContextChanged();
        }

        public static void ClearContexts()
        {
            stack.Clear();
            OnContextChanged();
        }

        public static int ContextDepth => stack.Count;
        public static bool HasHandle(int h) { foreach (var e in stack) if (e.handle == h) return true; return false; }

        // ------------------------------------------------------------ 눌림·떼기·잠금
        static readonly int N = Enum.GetValues(typeof(Act)).Length;
        static readonly bool[] held = new bool[N], prevHeld = new bool[N], latched = new bool[N], consumed = new bool[N];
        static int lastFrame = -1;

        /// <summary>자동 테스트용 가상 입력. held 는 직접 놓을 때까지 유지, Tap 은 한 프레임 눌렀다 뗀다.</summary>
        public static class Virtual
        {
            public static readonly bool[] Held = new bool[Enum.GetValues(typeof(Act)).Length];
            internal static readonly bool[] tapQueue = new bool[Enum.GetValues(typeof(Act)).Length];
            internal static readonly bool[] tapActive = new bool[Enum.GetValues(typeof(Act)).Length];
            public static void Tap(Act a) { tapQueue[(int)a] = true; }
            public static void Hold(Act a, bool on) { Held[(int)a] = on; }
            public static void ReleaseAll() { for (int i = 0; i < Held.Length; i++) { Held[i] = false; tapQueue[i] = false; } }
            /// <summary>자동 테스트용: 켜 두면 컷신을 바로 건너뛴다 (Enter 를 누른 것과 같음).</summary>
            public static bool Skip;
        }

        /// <summary>자동 테스트용: 켜 두면 대사가 저절로 넘어간다 (선택지는 넘기지 않는다).</summary>
        public static bool AutoAdvance;
        /// <summary>녹화·재생 확인용: 0 보다 크면 글자가 다 나온 뒤 이 시간(초)만큼 읽고 넘어간다.</summary>
        public static float AutoReadDelay;

        static Keyboard K => Keyboard.current;
        static Gamepad G => Gamepad.current;

        static bool RawHeld(Act a)
        {
            LoadBindings();
            var k = K;
            bool v = false;
            if (k != null)
            {
                v = bound[a] != Key.None && k[bound[a]].isPressed;
                if (!v && fixedAlt.TryGetValue(a, out var alt) && alt != bound[a]) v = k[alt].isPressed;
            }
            var g = G;
            if (!v && g != null)
            {
                Vector2 st = g.leftStick.ReadValue() + g.dpad.ReadValue();
                switch (a)
                {
                    case Act.Left: v = st.x < -0.4f; break;
                    case Act.Right: v = st.x > 0.4f; break;
                    case Act.MenuUp: v = st.y > 0.4f; break;
                    case Act.Down: v = st.y < -0.4f; break;
                    case Act.Jump: v = g.buttonSouth.isPressed; break;
                    case Act.Dodge: v = g.rightTrigger.isPressed; break;
                    case Act.Skill1: v = g.buttonWest.isPressed; break;
                    case Act.Skill2: v = g.buttonNorth.isPressed; break;
                    case Act.Skill3: v = g.leftShoulder.isPressed; break;
                    case Act.Skill4: v = g.rightShoulder.isPressed; break;
                    case Act.Interact: v = g.buttonEast.isPressed; break;
                    case Act.Cancel: v = g.selectButton.isPressed; break;
                    case Act.Skip: v = g.startButton.isPressed; break;
                }
            }
            int i = (int)a;
            return v || Virtual.Held[i] || Virtual.tapActive[i];
        }

        static void Sync()
        {
            if (lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount;
            for (int i = 0; i < N; i++)
            {
                Virtual.tapActive[i] = Virtual.tapQueue[i];
                Virtual.tapQueue[i] = false;
            }
            for (int i = 0; i < N; i++)
            {
                prevHeld[i] = held[i];
                held[i] = RawHeld((Act)i);
                consumed[i] = false;
                if (!held[i]) latched[i] = false;
            }
        }

        /// <summary>상태가 바뀌는 순간 누르고 있던 키는 놓을 때까지 새 상태에 전달하지 않는다.</summary>
        public static int ContextEpoch { get; private set; }

        static void OnContextChanged()
        {
            ContextEpoch++;
            Sync();
            for (int i = 0; i < N; i++) if (held[i] || RawHeld((Act)i)) latched[i] = true;
        }

        static bool Active(InputCtx c) => Context == c;

        /// <summary>그 상태에서 눌린 순간 (잠긴 키·이미 쓴 눌림 제외).</summary>
        public static bool Down(Act a, InputCtx c)
        {
            Sync();
            int i = (int)a;
            if (!Active(c) || latched[i] || consumed[i]) return false;
            return held[i] && !prevHeld[i];
        }

        public static bool Held(Act a, InputCtx c)
        {
            Sync();
            int i = (int)a;
            return Active(c) && !latched[i] && held[i];
        }

        /// <summary>그 상태에서 뗀 순간.</summary>
        public static bool Up(Act a, InputCtx c)
        {
            Sync();
            int i = (int)a;
            return Active(c) && !held[i] && prevHeld[i];
        }

        /// <summary>이번 프레임의 눌림을 다 썼다고 표시 (같은 프레임에 다른 곳이 다시 읽지 않게).</summary>
        public static void Consume(Act a) { Sync(); consumed[(int)a] = true; foreach (var other in AllActs) if (KeyOf(other) == KeyOf(a)) consumed[(int)other] = true; }

        /// <summary>지금 누르고 있는 키를 모두 잠근다 (놓을 때까지 무시).</summary>
        public static void LatchAll() { Sync(); for (int i = 0; i < N; i++) if (held[i]) latched[i] = true; }

        public static bool IsLatched(Act a) { Sync(); return latched[(int)a]; }
        public static bool RawIsHeld(Act a) { Sync(); return held[(int)a]; }

        // ------------------------------------------------------------ 상태별 편의 함수
        /// <summary>필드 좌우 이동 (-1~1).</summary>
        public static float MoveX => (Held(Act.Right, InputCtx.Gameplay) ? 1 : 0) - (Held(Act.Left, InputCtx.Gameplay) ? 1 : 0);

        /// <summary>대사 넘기기: 대화 상태의 조사 키 (기본 Space).</summary>
        public static bool AdvanceDown
        {
            get
            {
                if (AutoAdvance) return true;
                bool d = Down(Act.Interact, InputCtx.Dialogue);
                if (d) Consume(Act.Interact);
                return d;
            }
        }

        /// <summary>메뉴·선택지·문서 확정 (기본 Space).</summary>
        public static bool ConfirmDown
        {
            get
            {
                var c = Context;
                if (c != InputCtx.Menu && c != InputCtx.Pause) return false;
                bool d = Down(Act.Interact, c);
                if (d) Consume(Act.Interact);
                return d;
            }
        }

        /// <summary>메뉴·문서 닫기 (Esc).</summary>
        public static bool CancelDown
        {
            get
            {
                var c = Context;
                if (c != InputCtx.Menu && c != InputCtx.Pause) return false;
                bool d = Down(Act.Cancel, c);
                if (d) Consume(Act.Cancel);
                return d;
            }
        }

        /// <summary>필드에서 일시정지 열기 (Esc).</summary>
        public static bool PauseDown
        {
            get
            {
                bool d = Down(Act.Cancel, InputCtx.Gameplay);
                if (d) Consume(Act.Cancel);
                return d;
            }
        }

        /// <summary>컷신 건너뛰기: Enter 한 번. 컷신 상태(그 안의 대사 포함)에서만 받는다. 선택지가 열려 있으면 받지 않는다.</summary>
        public static bool SkipDown
        {
            get
            {
                Sync();
                if (Virtual.Skip) return true;
                var c = Context;
                if (c != InputCtx.Cutscene && c != InputCtx.Dialogue) return false;
                int i = (int)Act.Skip;
                return held[i] && !prevHeld[i] && !latched[i];
            }
        }

        /// <summary>이미 읽은 대사 빨리 넘기기 (Ctrl 누르고 있기).</summary>
        public static bool FastForward { get { var k = K; return k != null && (k[Key.LeftCtrl].isPressed || k[Key.RightCtrl].isPressed); } }

        // ---- 메뉴 이동(누르고 있으면 반복): 메뉴·일시정지 상태에서 W/S(↑/↓), A/D
        static float repeatTimer; static int repeatDir;
        static int navAxisOwner = -1;
        public static int NavVertical()
        {
            var c = Context; if (c != InputCtx.Menu && c != InputCtx.Pause) return 0;
            int d = (Held(Act.Down, c) ? 1 : 0) - (Held(Act.MenuUp, c) ? 1 : 0);
            return Nav(d, 0);
        }
        public static int NavHorizontal()
        {
            var c = Context; if (c != InputCtx.Menu && c != InputCtx.Pause) return 0;
            int d = (Held(Act.Right, c) ? 1 : 0) - (Held(Act.Left, c) ? 1 : 0);
            return Nav(d, 1);
        }
        static int Nav(int dir, int axis)
        {
            if (dir == 0)
            {
                if (navAxisOwner == axis) { repeatDir = 0; navAxisOwner = -1; }
                return 0;
            }
            if (navAxisOwner != axis || dir != repeatDir)
            {
                navAxisOwner = axis; repeatDir = dir; repeatTimer = 0.38f;
                return dir;
            }
            repeatTimer -= Time.unscaledDeltaTime;
            if (repeatTimer <= 0) { repeatTimer = 0.09f; return dir; }
            return 0;
        }

        /// <summary>키 변경 화면: 이번 프레임에 새로 눌린 키 (없으면 None).</summary>
        public static Key AnyKeyDown()
        {
            var k = K; if (k == null) return Key.None;
            foreach (var key in k.allKeys) if (key != null && key.wasPressedThisFrame) return key.keyCode;
            return Key.None;
        }
    }
}
