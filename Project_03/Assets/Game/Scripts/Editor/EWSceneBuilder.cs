using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EW.EditorTools
{
    public enum Theme { Home, Exterior, Digital, Lab, Village, House, Desktop }

    /// <summary>방 하나의 정의. rows 는 위쪽 행부터. '#' 벽/바닥, '=' 위에서만 밟히는 발판, 'H' 사다리, '^' 사다리+덮개, '.' 빈칸.</summary>
    public class RoomDef
    {
        public string id, scene, title;
        public Theme theme;
        public string[] rows;
        public Dictionary<char, Action<Ctx, int, int>> legend = new Dictionary<char, Action<Ctx, int, int>>();
        public Action<Ctx> extra;
        /// <summary>깊이 레이어(원경·중경·전경)를 직접 꾸민다. 비우면 테마 기본 레이어.</summary>
        public Action<Ctx> layers;
        public Color bg = new Color(0.07f, 0.07f, 0.1f);
        public string ambience, music, altAmbienceFlag, altAmbience;
        public bool hideHud, useAlarmAmbience;
        /// <summary>'=' 발판 그림을 그리지 않는다 (컴퓨터 안: 아이콘·창 제목줄 그림이 발판).</summary>
        public bool hidePlatforms;
        /// <summary>컴퓨터·포획 단말 안 (WASD 무중력). 동료는 따라오지 않는다.</summary>
        public bool zeroG, noCompanion, sealSkills;
        public int W => rows[0].Length;
        public int H => rows.Length;
    }

    /// <summary>방을 지을 때 쓰는 도우미. 좌표: (열, 행) → 칸의 아래 가운데 월드 좌표.</summary>
    public class Ctx
    {
        public Transform root;
        public RoomDef room;
        public readonly Dictionary<string, Device> devices = new Dictionary<string, Device>();
        public int H => room.H;

        public Vector2 Cell(int c, int r) => new Vector2(c + 0.5f, H - 1 - r);
        public Vector2 CellTop(int c, int r) => new Vector2(c + 0.5f, H - r);

        public GameObject Obj(string name, Vector2 pos, Transform parent = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent != null ? parent : root, false);
            go.transform.position = pos;
            return go;
        }

        public SpriteRenderer Sprite(GameObject go, Sprite s, int order)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s; sr.sortingOrder = order;
            return sr;
        }

        public static Sprite[] P(string n) => EWAssetSetup.P(n);
        public static Sprite P0(string n) => EWAssetSetup.P(n).FirstOrDefault();
        public static Sprite[] TP(string rel) => EWAssetSetup.Frames(EWArtSpecs.TP + rel);
        public static Sprite Named(string rel, string name) => EWAssetSetup.Named(EWArtSpecs.TP + rel, name);

        public GameObject Decor(Vector2 pos, Sprite s, int order = -5, float scale = 1f, string name = "Decor")
        {
            var go = Obj(name, pos);
            Sprite(go, s, order);
            go.transform.localScale = Vector3.one * scale;
            return go;
        }

        public GameObject Anim(Vector2 pos, Sprite[] frames, float fps, int order = 2, string name = "Anim")
        {
            var go = Obj(name, pos);
            Sprite(go, frames.FirstOrDefault(), order);
            var fb = go.AddComponent<Flipbook>(); fb.frames = frames; fb.fps = fps; fb.loop = true;
            return go;
        }

        BoxCollider2D Trigger(GameObject go, Vector2 size, Vector2 offset)
        {
            var bc = go.AddComponent<BoxCollider2D>();
            bc.isTrigger = true; bc.size = size; bc.offset = offset;
            return bc;
        }

        // ---------------- 위치·영역
        public void Spawn(int c, int r, string id, int facing = 1)
        {
            var go = Obj("Spawn_" + id, Cell(c, r));
            var sp = go.AddComponent<SpawnPoint>(); sp.id = id; sp.facing = facing;
        }

        /// <summary>타일 영역(c0..c1, r0..r1 포함) 트리거.</summary>
        public TriggerZone Zone(string name, int c0, int r0, int c1, int r1)
        {
            float x0 = c0, x1 = c1 + 1, yTop = H - r0, yBot = H - 1 - r1;
            var go = Obj(name, new Vector2((x0 + x1) / 2f, (yTop + yBot) / 2f));
            Trigger(go, new Vector2(x1 - x0, yTop - yBot), Vector2.zero);
            return go.AddComponent<TriggerZone>();
        }

        public TriggerZone Exit(int c0, int r0, int c1, int r1, string map, string spawn, string blockFlag = null, string blockedDialogue = null, string requireFlag = null)
        {
            var z = Zone("Exit_" + map + "_" + spawn, c0, r0, c1, r1);
            z.kind = TriggerZone.Kind.MapExit; z.targetMap = map; z.targetSpawn = spawn;
            z.blockFlag = blockFlag; z.blockedDialogue = blockedDialogue; z.requireFlag = requireFlag;
            return z;
        }

        public TriggerZone EventZone(int c0, int r0, int c1, int r1, string ev, string requireFlag = null, string blockFlag = null)
        {
            var z = Zone("Event_" + ev, c0, r0, c1, r1);
            z.kind = TriggerZone.Kind.Event; z.eventId = ev; z.requireFlag = requireFlag; z.blockFlag = blockFlag;
            return z;
        }

        public TriggerZone BubbleZone(int c0, int r0, int c1, int r1, string cond)
        {
            var z = Zone("Bubble_" + cond, c0, r0, c1, r1);
            z.kind = TriggerZone.Kind.Bubble; z.bubbleCond = cond;
            return z;
        }

        // ---------------- 조사 물건
        public Inspect Inspect(Vector2 pos, Sprite s, string id, string dialogue, string repeat = null, string ev = null, int order = 2, string prompt = "조사", float range = 0.9f)
        {
            var go = Obj("Inspect_" + id, pos);
            if (s != null) Sprite(go, s, order);
            var it = go.AddComponent<Inspect>();
            it.id = id; it.dialogueId = dialogue; it.repeatDialogueId = repeat; it.eventId = ev; it.promptText = prompt; it.range = range;
            float hgt = s != null ? s.bounds.size.y : 1f;
            it.promptOffset = new Vector2(0, Mathf.Max(1.2f, hgt + 0.25f));
            return it;
        }

        public void Pickup(int c, int r, string item, string id)
        {
            var go = Obj("Pickup_" + id, Cell(c, r));
            if (item == "bandage") { Sprite(go, TP("Foozle_SciFiLab_Pickups/medkit_item.png").FirstOrDefault(), 3); var fb = go.AddComponent<Flipbook>(); fb.frames = TP("Foozle_SciFiLab_Pickups/medkit_item.png"); fb.fps = 10; }
            else Sprite(go, P0("Prop_Snack"), 3);
            var p = go.AddComponent<Pickup>(); p.id = id; p.itemId = item; p.promptText = "줍기"; p.promptOffset = new Vector2(0, 0.9f); p.range = 0.7f;
        }

        public void Doc(int c, int r, string docId, string id)
        {
            var go = Obj("Doc_" + docId, Cell(c, r) + new Vector2(0, 0.6f));
            Sprite(go, P0("Prop_Doc"), 3);
            var d = go.AddComponent<DocPickup>(); d.id = id; d.docId = docId; d.promptText = "읽기"; d.promptOffset = new Vector2(0, 0.7f);
        }

        public RestPoint Rest(Vector2 pos, Sprite s, string checkpoint, string name = "Rest", string beforeEvent = null)
        {
            var go = Obj(name, pos);
            Sprite(go, s, 2);
            var rp = go.AddComponent<RestPoint>(); rp.id = name; rp.checkpointId = checkpoint; rp.promptText = "쉬기"; rp.beforeEventId = beforeEvent;
            rp.range = 1.3f; rp.promptOffset = new Vector2(0, 1.2f);
            return rp;
        }

        public DoorExit Door(int c, int r, string map, string spawn, string prompt, Sprite s = null)
        {
            var go = Obj("Door_" + map, Cell(c, r));
            Sprite(go, s ?? P("Prop_Door").ElementAtOrDefault(0), -2);
            var d = go.AddComponent<DoorExit>(); d.id = "door_" + map + spawn; d.targetMap = map; d.targetSpawn = spawn; d.promptText = prompt;
            d.promptOffset = new Vector2(0, 2.2f);
            return d;
        }

        public void Actor(int c, int r, string actorId, string art, int facing, string requireFlag = null, string hideFlag = null, string hideFlag2 = null, string idle = "Idle", float dx = 0)
        {
            var go = Obj("Actor_" + actorId, Cell(c, r) + new Vector2(dx, 0));
            var a = go.AddComponent<ActorSpawn>(); a.actorId = actorId; a.artId = art; a.facing = facing; a.requireFlag = requireFlag; a.hideFlag = hideFlag; a.hideFlag2 = hideFlag2; a.idleState = idle;
        }

        // ---------------- 실시간 잠입: 경비·CCTV·기물·엄폐
        /// <summary>순찰 경비 (x0~x1 사이 순찰). 바닥 칸 (c, r) 에 선다.</summary>
        public Guard Guard(int c, int r, string id, string enemyId, float x0, float x1, int facing = 1, string requireFlag = null, bool flying = false, float flyHeight = 3f)
        {
            var pos = Cell(c, r) + (flying ? new Vector2(0, flyHeight) : Vector2.zero);
            var go = Obj("Guard_" + id, pos);
            var gd = go.AddComponent<Guard>();
            gd.guardId = id; gd.enemyId = enemyId; gd.patrolMin = x0; gd.patrolMax = x1; gd.startFacing = facing; gd.requireFlag = requireFlag; gd.flying = flying;
            return gd;
        }

        /// <summary>천장 CCTV. at 은 받침 위치(월드). 각도 범위 a~b (도, -90 = 아래).</summary>
        public Cctv Camera(Vector2 at, string id, float a, float b, float phase = 0, string onFlag = null)
        {
            var go = Obj("Cctv_" + id, at);
            Sprite(go, S("S_CctvMount"), 20);
            var head = Obj("Head", at + new Vector2(0, -0.42f), go.transform);
            Sprite(head, S("S_CctvHead"), 21);
            var lens = Obj("Lens", at + new Vector2(0.62f, -0.42f), head.transform);
            var lsr = Sprite(lens, S("S_Lens"), 22);
            var cam = go.AddComponent<Cctv>();
            cam.id = id; cam.displayName = "CCTV"; cam.angleA = a; cam.angleB = b; cam.startPhase = phase; cam.onFlag = onFlag;
            cam.head = head.transform; cam.lens = lsr; cam.lensOn = S("S_Lens"); cam.lensOff = S("S_LensOff");
            cam.size = new Vector2(1f, 0.6f); cam.aimOffset = new Vector2(0, -0.4f);
            return cam;
        }

        public CctvControl CameraControl(Vector2 at, string id, params Cctv[] cams)
        {
            var go = Obj("CctvControl_" + id, at);
            Sprite(go, S("S_CtrlBox"), 3);
            var scr = Obj("Screen", at + new Vector2(0, 0.6f), go.transform);
            var ssr = Sprite(scr, S("S_CtrlScreen"), 4);
            var cc = go.AddComponent<CctvControl>();
            cc.id = id; cc.displayName = "CCTV 제어함"; cc.cameras = cams.ToList(); cc.screen = ssr; cc.size = new Vector2(0.75f, 1f); cc.aimOffset = new Vector2(0, 0.5f);
            // 연결선 (제어함 → 카메라): 무엇이 연결됐는지 보이게 흐리게 그린다
            foreach (var cm in cams)
            {
                var lgo = Obj("Wire", at, go.transform);
                var lr = lgo.AddComponent<LineRenderer>();
                lr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                lr.positionCount = 3; lr.useWorldSpace = true; lr.startWidth = lr.endWidth = 0.05f; lr.sortingOrder = -3;
                var top = new Vector2(at.x, cm.transform.position.y + 0.1f);
                lr.SetPositions(new Vector3[] { at + new Vector2(0, 0.95f), top, cm.transform.position });
                lr.startColor = lr.endColor = new Color(0.25f, 0.4f, 0.36f, 0.8f);
            }
            return cc;
        }

        /// <summary>외부 전원. charges 0 이면 '전력 없음' (스파크 장식만 있는 죽은 전원).</summary>
        public PowerSource PowerSrc(Vector2 at, string id, int charges = 1, string name = "노출된 배전함")
        {
            var go = Obj("Power_" + id, at);
            Sprite(go, S(charges > 0 ? "S_PowerBox" : "S_PowerBoxDead"), 2);
            var ps = go.AddComponent<PowerSource>(); ps.id = id; ps.charges = charges; ps.displayName = name; ps.tapOffset = new Vector2(0, 0.55f);
            var ic = Obj("Icon", at + new Vector2(0, 1.3f), go.transform);
            ps.icon = Sprite(ic, S("S_IconPower"), 23);
            ps.iconAvailable = S("S_IconPower"); ps.iconEmpty = S("S_IconPowerEmpty"); ps.iconInUse = S("S_IconPowerBusy");
            ps.glow = Glow(at + new Vector2(0, 0.55f), 0.9f, new Color(1f, 0.9f, 0.45f, 0.3f), 41, go.transform, true);
            Sparks(at + new Vector2(0, 0.5f), null, charges > 0 ? 0.25f : 0.15f, go.transform);
            return ps;
        }

        public NoiseMaker Tray(Vector2 at, string id)
        {
            var go = Obj("Tray_" + id, at);
            Sprite(go, S("S_Tray"), 3);
            var nm = go.AddComponent<NoiseMaker>(); nm.id = id; nm.radius = 9f; nm.displayName = "매달린 공구 쟁반"; nm.size = new Vector2(0.9f, 0.6f); nm.aimOffset = new Vector2(0, -0.5f);
            return nm;
        }

        public ShutterPanel Panel(Vector2 at, string id, string setFlag)
        {
            var go = Obj("ShutterPanel_" + id, at);
            Sprite(go, S("S_ShutterPanel"), 3);
            var lamp = Obj("Lamp", at + new Vector2(0, 0.9f), go.transform);
            var sp = go.AddComponent<ShutterPanel>(); sp.id = id; sp.displayName = "셔터 제어판"; sp.setFlag = setFlag; sp.lamp = Sprite(lamp, S("S_Lamp"), 4);
            sp.size = new Vector2(0.6f, 0.8f); sp.aimOffset = new Vector2(0, 0.4f);
            return sp;
        }

        /// <summary>
        /// 낮은 엄폐물 (막는 상자). 고체라 지나갈 수 없고(1칸이라 쉽게 넘음) 시야선을 막는다.
        /// 바닥 윗면 (x 중심, floorY) 에 놓는다. S_Crate 1.5x1, S_CrateS 1x1, S_CrateTall 1.5x1.5
        /// </summary>
        public Cover Crate(float x, float floorY, string sprite = "S_CrateSolid")
        {
            var sp = S(sprite);
            var go = Obj("Cover_" + sprite, new Vector2(x, floorY));
            Sprite(go, sp, 4);
            Vector2 size = sp != null ? (Vector2)sp.bounds.size : new Vector2(1.5f, 1f);
            var bc = go.AddComponent<BoxCollider2D>(); bc.size = size; bc.offset = new Vector2(0, size.y / 2f);
            var cv = go.AddComponent<Cover>(); cv.height = size.y;
            return cv;
        }

        /// <summary>
        /// B형 엄폐물: 뒤에 숨는 전경 물체 (선반·자루). 트리거라 뒤로 들어갈 수 있고, 루보다 앞(정렬 20)에 그린다.
        /// 영역 안에서 S 로 웅크리면 숨음 (HideSpot). 고체 상자(A형, Crate)와 모양·충돌을 나눈다.
        /// </summary>
        public HideSpot HideCover(float x, float floorY, string sprite = "S_HideRack")
        {
            var sp = S(sprite);
            var go = Obj("HideCover_" + sprite, new Vector2(x, floorY));
            var sr = Sprite(go, sp, 20);
            Vector2 size = sp != null ? (Vector2)sp.bounds.size : new Vector2(1.75f, 1.4f);
            var bc = go.AddComponent<BoxCollider2D>(); bc.isTrigger = true;
            bc.size = new Vector2(size.x - 0.1f, 1.2f); bc.offset = new Vector2(0, 0.6f);
            var hs = go.AddComponent<HideSpot>(); hs.art = sr;
            go.AddComponent<SeeThrough>();
            return hs;
        }

        /// <summary>지원 드론 출입 지점 (CCTV 신고에 달려올 경비가 없을 때).</summary>
        public void Reinforce(Vector2 at) { Obj("ReinforcePoint", at).AddComponent<ReinforcePoint>(); }

        /// <summary>통과 가능한 배경 상자 (어둡고 테두리 없음, 충돌·시야 차단 없음).</summary>
        public void BgBox(float x, float floorY) { Decor(new Vector2(x, floorY), S("S_BgBox"), -8, 1, "BgBox"); }

        /// <summary>경광등: 경보 플래그가 켜지면 붉은 빔이 돌고 깜빡인다.</summary>
        public AlarmLight Alarm(Vector2 at)
        {
            var go = Obj("AlarmLight", at);
            Sprite(go, S("S_AlarmLamp"), 20);
            var beam = Obj("Beam", at, go.transform);
            var bsr = Sprite(beam, S("S_AlarmBeam"), 42);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(EWAssetSetup.AdditivePath); if (mat != null) bsr.sharedMaterial = mat;
            var glow = Glow(at, 2.2f, new Color(1f, 0.2f, 0.15f, 0f), 41, go.transform, true);
            var al = go.AddComponent<AlarmLight>(); al.beam = beam.transform; al.beamSr = bsr; al.glow = glow;
            return al;
        }

        /// <summary>작업등 (천장): 정전 중 꺼짐, flag 가 켜지면 빛.</summary>
        public void WorkLamp(Vector2 at, string flag, float radius = 3.2f)
        {
            Decor(at, S("S_WorkLamp"), 3, 1, "WorkLamp");
            var glow = Glow(at + new Vector2(0, -2.2f), radius, new Color(0.75f, 0.92f, 0.85f, 0.18f), 41, null, true);
            var cone = Glow(at + new Vector2(0, -0.5f), 0.7f, new Color(1f, 1f, 0.85f, 0.5f), 41, null, true);
            FlagSwitch("WorkLamp", flag, new[] { glow.gameObject, cone.gameObject });
        }

        /// <summary>구간 시작점 (재시도 지점): 들어서면 체크포인트.</summary>
        public void Checkpoint(int c0, int r0, int c1, int r1) { EventZone(c0, r0, c1, r1, "checkpoint"); }

        // ---------------- 장치
        public Device Device(Vector2 pos, string id, DeviceKind kind, Sprite[] on, Sprite[] off, Sprite[] broken = null, Sprite[] boot = null,
                             string line = null, bool battery = false, int level = 100, bool essential = false, bool fragile = false, string controller = null, int order = 1)
        {
            var go = Obj("Device_" + id, pos);
            Sprite(go, on.FirstOrDefault(), order);
            var d = go.AddComponent<Device>();
            d.Model = new DeviceModel { id = id, kind = kind, line = line, hasBattery = battery, battery = level, essential = essential, fragile = fragile, controllerId = controller };
            d.onFrames = on; d.offFrames = off; d.brokenFrames = broken; d.bootFrames = boot; d.fps = 8;
            if (battery)
            {
                var bar = Obj("BatteryBar", pos + new Vector2(-0.25f, 0.3f), go.transform);
                var bsr = Sprite(bar, P0("Tile_Pixel"), order + 1);
                bar.transform.localScale = new Vector3(4f, 0.6f, 1);
                d.batteryBar = bsr; d.batteryBarWidth = 0.5f;
                bsr.transform.localPosition = new Vector3(-0.25f, 0.18f, 0);
            }
            devices[id] = d;
            return d;
        }

        public void DeviceInspect(Device d, string extra = null, float range = 0.9f, string prompt = "조사")
        {
            var di = d.gameObject.AddComponent<DeviceInspect>();
            di.id = "inspect_" + d.Model.id; di.device = d; di.extraDialogueId = extra; di.range = range; di.promptText = prompt;
            di.promptOffset = new Vector2(0, 1.4f);
        }

        public PowerSwitch Switch(int c, int r, params Device[] targets)
        {
            var go = Obj("Switch", Cell(c, r) + new Vector2(0, 0.3f));
            var fr = P("Prop_Switch");
            Sprite(go, fr.ElementAtOrDefault(1), 2);
            var s = go.AddComponent<PowerSwitch>(); s.id = "switch_" + c + "_" + r; s.targets = targets.ToList(); s.offSprite = fr.ElementAtOrDefault(0); s.onSprite = fr.ElementAtOrDefault(1);
            s.promptText = "스위치"; s.promptOffset = new Vector2(0, 1.3f);
            return s;
        }

        public RestoreButton Restore(int c, int r, string line, string after = null)
        {
            var go = Obj("RestoreButton_" + line, Cell(c, r) + new Vector2(0, 0.3f));
            Sprite(go, P0("Prop_RestoreButton"), 2);
            var b = go.AddComponent<RestoreButton>(); b.id = "restore_" + line; b.line = line; b.afterDialogueId = after; b.promptText = "전원 복구";
            b.promptOffset = new Vector2(0, 1.3f);
            return b;
        }

        public BatteryPack Battery(int c, int r, string id, int level, Device target = null)
        {
            var d = Device(Cell(c, r), id, DeviceKind.Machine, new[] { P0("Prop_Battery") }, new[] { P0("Prop_Battery") }, battery: true, level: level, order: 2);
            var b = d.gameObject.AddComponent<BatteryPack>(); b.id = "battery_" + id; b.self = d; b.chargeTarget = target; b.promptText = "배터리";
            b.promptOffset = new Vector2(0, 1.3f);
            return b;
        }

        /// <summary>전원선 한 쌍 (양방향). path 는 입구→출구 사이 꺾이는 점(월드).</summary>
        public void PowerLinePair(string id, int ac, int ar, int bc, int br, Vector2[] path, string fixFlag = null)
        {
            var a = Cell(ac, ar); var b = Cell(bc, br);
            var fr = P("Prop_PowerSocket");
            var pa = Socket(id + "_A", a, fr); var pb = Socket(id + "_B", b, fr);
            var pts = new List<Vector3> { (Vector3)a + Vector3.up * 0.5f };
            if (path != null) foreach (var p in path) pts.Add(p);
            pts.Add((Vector3)b + Vector3.up * 0.5f);
            var lineGo = Obj("Cable_" + id, Vector2.zero);
            var lr = lineGo.AddComponent<LineRenderer>();
            lr.positionCount = pts.Count; lr.SetPositions(pts.ToArray());
            lr.startWidth = lr.endWidth = 0.14f; lr.useWorldSpace = true; lr.numCapVertices = 0;
            lr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Data/EW_Line.mat");
            lr.sortingOrder = -3;
            lr.startColor = lr.endColor = new Color(0.25f, 0.24f, 0.2f);
            var la = pa.AddComponent<PowerLine>(); var lb = pb.AddComponent<PowerLine>();
            Setup(la, pb.transform, pts.Skip(1).Take(pts.Count - 2).ToArray(), fixFlag, fr, pb.GetComponent<SpriteRenderer>(), lr, id + "_A");
            var rev = pts.Skip(1).Take(pts.Count - 2).Reverse().ToArray();
            Setup(lb, pa.transform, rev, fixFlag, fr, pa.GetComponent<SpriteRenderer>(), null, id + "_B");
        }

        GameObject Socket(string name, Vector2 pos, Sprite[] fr)
        {
            var go = Obj("PowerLine_" + name, pos);
            Sprite(go, fr.ElementAtOrDefault(0), 2);
            return go;
        }

        void Setup(PowerLine l, Transform exit, Vector3[] path, string fix, Sprite[] fr, SpriteRenderer exitSr, LineRenderer lr, string id)
        {
            l.id = "line_" + id; l.exit = exit; l.path = path; l.fixFlag = fix; l.socket = l.GetComponent<SpriteRenderer>();
            l.idleSprite = fr.ElementAtOrDefault(0); l.glowSprite = fr.ElementAtOrDefault(1); l.brokenSprite = fr.ElementAtOrDefault(2);
            l.exitSocket = exitSr; l.lineRenderer = lr; l.promptText = "전원선"; l.promptOffset = new Vector2(0, 1.3f);
        }

        public void Coil(int c, int r, string fixFlag)
        {
            var go = Obj("WireCoil", Cell(c, r));
            Sprite(go, P0("Prop_WireCoil"), 2);
            var w = go.AddComponent<WireCoil>(); w.id = "coil_" + fixFlag; w.fixFlag = fixFlag; w.promptText = "교체 선";
            w.promptOffset = new Vector2(0, 1.2f);
        }

        /// <summary>끊어진 노출 전선 (장식). 출력이 남아 있으면 외부 전원으로 쓸 수 있다.</summary>
        public void ExposedWire(int c, int r, string id, int charges = 1)
        {
            Anim(Cell(c, r), P("Prop_ExposedWire"), 8, 2, "ExposedWire_" + id);
            if (charges > 0)
            {
                var ps = PowerSrc(Cell(c, r) + new Vector2(0, -0.5f), id, charges, "끊어진 굵은 전선");
                var sr = ps.GetComponent<SpriteRenderer>(); if (sr != null) sr.enabled = false;
            }
        }

        public void Puddle(int c, int r, int widthTiles, string id)
        {
            var go = Obj("Puddle_" + id, Cell(c, r) + new Vector2(0, 0.02f));
            Sprite(go, P0("Prop_Puddle"), 1);
            go.transform.localScale = new Vector3(widthTiles / 2f, 1, 1);
            var it = go.AddComponent<Inspect>(); it.id = id; it.dialogueId = "c3_puddle_inspect"; it.promptText = "조사"; it.promptOffset = new Vector2(0, 0.9f);
        }

        public Device Terminal(int c, int r, string id, string line)
        {
            var d = Device(Cell(c, r), id, DeviceKind.Terminal, TP("Foozle_SciFiLab_Tileset/control_panel_idle.png"), TP("Foozle_SciFiLab_Tileset/control_panel_deactivate.png").Take(1).ToArray(), line: line, essential: true, order: 1);
            d.fps = 10;
            DeviceInspect(d, "c1_terminal_inspect");
            return d;
        }

        // ================= 깊이 레이어·연출용 도우미
        public static Sprite S(string sceneArt) => P0("Scene/" + sceneArt);
        public static Sprite[] SF(string sceneArt) => P("Scene/" + sceneArt);
        public Vector2 Ref => new Vector2(room.W / 2f, room.H / 2f);

        /// <summary>깊이 레이어 하나. follow: 0 = 플레이 영역, 0.8 = 아주 먼 원경, 음수 = 카메라 앞 전경.</summary>
        public GameObject Layer(Sprite sp, Vector2 pos, int order, float follow, Color? tint = null, float scale = 1f, string name = "Layer", Transform parent = null)
        {
            var go = Obj(name, pos, parent);
            var sr = Sprite(go, sp, order);
            if (tint.HasValue) sr.color = tint.Value;
            go.transform.localScale = Vector3.one * scale;
            if (Mathf.Abs(follow) > 0.001f)
            {
                var px = go.AddComponent<Parallax>(); px.follow = follow; px.reference = Ref; px.vertical = 0.5f;
            }
            return go;
        }

        /// <summary>가로로 반복되는 레이어 (Tiled). width 는 유닛.</summary>
        public GameObject Band(Sprite sp, Vector2 bottomCenter, float width, int order, float follow, Color? tint = null, string name = "Band")
        {
            var go = Layer(sp, bottomCenter, order, follow, tint, 1f, name);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(width, sp != null ? sp.bounds.size.y : 1);
            return go;
        }

        public Marker Mark(string id, Vector2 pos, GameObject on = null)
        {
            var go = on ?? Obj("Marker_" + id, pos);
            var m = go.AddComponent<Marker>(); m.id = id;
            return m;
        }

        public FrameSet Frames(Vector2 pos, Sprite[] frames, int order, string name, params (string flag, int index)[] rules)
        {
            var go = Obj(name, pos);
            Sprite(go, frames.FirstOrDefault(), order);
            var fs = go.AddComponent<FrameSet>();
            fs.frames = frames;
            fs.rules = rules.Select(r => new FrameSet.Rule { flag = r.flag, index = r.index }).ToArray();
            return fs;
        }

        /// <summary>플래그에 따라 보이기/숨기기·충돌 해제를 담당하는 관리 오브젝트.</summary>
        public FlagVisual FlagSwitch(string name, string flag, GameObject[] showWhenSet = null, GameObject[] hideWhenSet = null, Collider2D[] blockWhileUnset = null)
        {
            var go = Obj("Flag_" + name, Vector2.zero);
            var fv = go.AddComponent<FlagVisual>();
            fv.flag = flag; fv.showWhenSet = showWhenSet; fv.hideWhenSet = hideWhenSet; fv.blockWhileUnset = blockWhileUnset;
            return fv;
        }

        public SpriteRenderer Glow(Vector2 pos, float radius, Color c, int order = 41, Transform parent = null, bool soft = false)
        {
            var sp = S(soft ? "FX_GlowSoft" : "FX_Glow");
            var go = Obj("Glow", pos, parent);
            var sr = Sprite(go, sp, order);
            sr.color = c;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(EWAssetSetup.AdditivePath);
            if (mat != null) sr.sharedMaterial = mat;
            float size = sp != null ? sp.bounds.size.x : 2f;
            go.transform.localScale = Vector3.one * (radius * 2f / size);
            return sr;
        }

        public AmbientParticles Particles(string name, Sprite sp, int count, Vector2 vel, Vector2 jitter, Color a, Color b, float life, int order, bool additive, string requireFlag = null, float sway = 0.4f)
        {
            var go = Obj("Particles_" + name, Vector2.zero);
            var ap = go.AddComponent<AmbientParticles>();
            ap.sprite = sp; ap.count = count; ap.velocity = vel; ap.velocityJitter = jitter; ap.colorA = a; ap.colorB = b; ap.life = life; ap.sortingOrder = order; ap.additive = additive; ap.requireFlag = requireFlag; ap.sway = sway;
            return ap;
        }

        public SparkEmitter Sparks(Vector2 pos, string requireFlag, float radius = 0.3f, Transform parent = null)
        {
            var go = Obj("Sparks", pos, parent);
            var se = go.AddComponent<SparkEmitter>(); se.requireFlag = requireFlag; se.radius = radius;
            return se;
        }

        /// <summary>화면 전체를 어둡게 하는 막 (정전 등). follow 1 로 카메라에 붙는다.</summary>
        public GameObject Darkness(float alpha, int order = 40, string name = "Darkness")
        {
            var go = Layer(P0("Tile_Pixel"), Ref, order, 1f, new Color(0.02f, 0.02f, 0.06f, alpha), 1f, name);
            var sp = P0("Tile_Pixel");
            float sz = sp != null ? sp.bounds.size.x : 0.125f;
            go.transform.localScale = new Vector3(24f / sz, 14f / sz, 1);
            go.GetComponent<Parallax>().vertical = 1f;
            return go;
        }

        public ActorSpawn ActorEx(Vector2 pos, string actorId, string art, int facing, string requireFlag, string downFlag, Vector2 downPos)
        {
            var go = Obj("Actor_" + actorId, pos);
            var a = go.AddComponent<ActorSpawn>(); a.actorId = actorId; a.artId = art; a.facing = facing; a.requireFlag = requireFlag;
            a.downFlag = downFlag; a.downPos = downPos;
            return a;
        }
    }

    public static class EWSceneBuilder
    {
        public const string SceneDir = "Assets/Game/Scenes";
        public const string GeneratedRoot = "__Generated";

        /// <summary>
        /// 씬을 만들거나 갱신한다. 씬 안의 '__Generated' 아래만 다시 만들고, 사용자가 그 밖에 추가한 오브젝트는 그대로 둔다.
        /// </summary>
        public static void BuildAll()
        {
            if (!AssetDatabase.IsValidFolder(SceneDir)) AssetDatabase.CreateFolder("Assets/Game", "Scenes");
            var active = EditorSceneManager.GetActiveScene().path;
            BuildTitle();
            EWRooms.ResetLayoutCache();
            EWLayoutCheck.BeginReport();
            foreach (var r in EWRooms.All()) BuildRoom(r);
            EWLayoutCheck.WriteReport();
            MoveObsoleteScenes();
            UpdateBuildSettings();
            if (!string.IsNullOrEmpty(active) && System.IO.File.Exists(active)) EditorSceneManager.OpenScene(active);
            else EditorSceneManager.OpenScene(SceneDir + "/Title.unity");
        }

        /// <summary>오프닝 교체·맵 통합으로 쓰지 않게 된 옛 생성 씬은 지우지 않고 Scenes/_Obsolete 로 옮긴다.</summary>
        static void MoveObsoleteScenes()
        {
            var live = new HashSet<string>(EWRooms.All().Select(r => r.scene)) { "Title" };
            string obs = SceneDir + "/_Obsolete";
            foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { SceneDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/_Obsolete/")) continue;
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (live.Contains(name)) continue;
                if (!System.Text.RegularExpressions.Regex.IsMatch(name, "^M\\d\\d_")) continue;   // 이 도구가 만든 옛 맵만
                if (!AssetDatabase.IsValidFolder(obs)) AssetDatabase.CreateFolder(SceneDir, "_Obsolete");
                string err = AssetDatabase.MoveAsset(path, obs + "/" + name + ".unity");
                Debug.Log("[EW] 옛 씬 이동: " + path + (string.IsNullOrEmpty(err) ? "" : " 실패: " + err));
            }
        }

        static Scene OpenOrCreate(string path, out bool created)
        {
            created = !System.IO.File.Exists(path);
            if (!created) return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var s = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(s, path);
            return s;
        }

        static Camera EnsureCamera(Color bg)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = PixelScreen.OrthoSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bg;
            cam.transform.position = new Vector3(10, 6, -10);
            return cam;
        }

        static void BuildTitle()
        {
            string path = SceneDir + "/Title.unity";
            var scene = OpenOrCreate(path, out _);
            var old = GameObject.Find(GeneratedRoot);
            if (old != null) UnityEngine.Object.DestroyImmediate(old);
            var cam = EnsureCamera(new Color(0.05f, 0.05f, 0.09f));
            var root = new GameObject(GeneratedRoot);
            var t = new GameObject("Title");
            t.transform.SetParent(root.transform);
            t.AddComponent<TitleController>();
            // 배경: 도시 원경
            var ctx = new Ctx { root = root.transform, room = new RoomDef { rows = new[] { "...................." } } };
            var back = ctx.Decor(new Vector2(10, 0.5f), Ctx.TP("ansimuz_SynthCities/back.png").FirstOrDefault(), -20);
            var sr = back.GetComponent<SpriteRenderer>(); sr.drawMode = SpriteDrawMode.Tiled; sr.size = new Vector2(24, 8.5f); sr.color = new Color(0.55f, 0.55f, 0.75f);
            var mid = ctx.Decor(new Vector2(10, 0.5f), Ctx.TP("ansimuz_SynthCities/middle.png").FirstOrDefault(), -19);
            var msr = mid.GetComponent<SpriteRenderer>(); msr.drawMode = SpriteDrawMode.Tiled; msr.size = new Vector2(24, 8.5f); msr.color = new Color(0.45f, 0.45f, 0.6f);
            cam.transform.position = new Vector3(10, 5, -10);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        static void BuildRoom(RoomDef r)
        {
            string path = SceneDir + "/" + r.scene + ".unity";
            var scene = OpenOrCreate(path, out _);
            // MapController 는 유지 (사용자가 붙인 값 보존), 생성물만 교체
            // 기존 Map 루트를 찾는다 (스크립트 참조가 깨진 경우도 이름으로 찾아 복구)
            var roots = scene.GetRootGameObjects();
            var mapGo = roots.FirstOrDefault(g => g.GetComponent<MapController>() != null) ?? roots.FirstOrDefault(g => g.name == "Map");
            if (mapGo == null) mapGo = new GameObject("Map");
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(mapGo);
            var map = mapGo.GetComponent<MapController>();
            if (map == null) map = mapGo.AddComponent<MapController>();
            map.mapId = r.id; map.title = r.title; map.background = r.bg;
            map.bounds = new Rect(0, 0, r.W, r.H);
            map.ambience = r.ambience; map.music = r.music; map.altAmbienceFlag = r.altAmbienceFlag; map.altAmbience = r.altAmbience; map.hideHud = r.hideHud; map.useAlarmAmbience = r.useAlarmAmbience; map.zeroG = r.zeroG || r.id == "P03"; map.noCompanion = r.noCompanion || r.zeroG; map.sealSkills = r.sealSkills;
            var oldGen = map.transform.Find(GeneratedRoot);
            if (oldGen != null) UnityEngine.Object.DestroyImmediate(oldGen.gameObject);
            var gen = new GameObject(GeneratedRoot).transform;
            gen.SetParent(map.transform, false);
            EnsureCamera(r.bg);
            var ctx = new Ctx { root = gen, room = r };
            if (r.layers != null) r.layers(ctx); else BuildBackground(ctx);
            BuildSolids(ctx);
            BuildLadders(ctx);
            for (int row = 0; row < r.H; row++)
                for (int col = 0; col < r.W; col++)
                {
                    char ch = r.rows[row][col];
                    if (r.legend.TryGetValue(ch, out var a)) a(ctx, col, row);
                }
            r.extra?.Invoke(ctx);
            EWLayoutCheck.Record(r, gen);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        static void BuildBackground(Ctx ctx)
        {
            var r = ctx.room;
            var center = new Vector2(r.W / 2f, r.H / 2f);
            switch (r.theme)
            {
                case Theme.Lab:
                    LabLayers(ctx, 0);
                    break;
                case Theme.Home:
                {
                    var go = ctx.Decor(center, Ctx.P0("Tile_HomeWall"), -30, 1, "BG");
                    var sr = go.GetComponent<SpriteRenderer>(); sr.drawMode = SpriteDrawMode.Tiled; sr.size = new Vector2(r.W, r.H);
                    break;
                }
                case Theme.Digital:
                {
                    var go = ctx.Decor(center, Ctx.P0("Tile_Digital"), -30, 1, "BG");
                    var sr = go.GetComponent<SpriteRenderer>(); sr.drawMode = SpriteDrawMode.Tiled; sr.size = new Vector2(r.W, r.H);
                    break;
                }
                case Theme.Exterior:
                {
                    var sky = ctx.Decor(center, Ctx.P0("Tile_Pixel"), -40, 1, "Sky");
                    var ssr = sky.GetComponent<SpriteRenderer>(); ssr.color = new Color(0.98f, 0.62f, 0.52f);
                    sky.transform.localScale = new Vector3(r.W * 8f, r.H * 8f, 1);   // 4px 스프라이트를 확대 (반복 타일링 금지)
                    string[] layers = { "back", "middle", "foreground" };
                    for (int i = 0; i < 3; i++)
                    {
                        var go = ctx.Decor(new Vector2(r.W / 2f, 1f), Ctx.TP("ansimuz_SynthCities/" + layers[i] + ".png").FirstOrDefault(), -35 + i, 1, "City_" + layers[i]);
                        var sr = go.GetComponent<SpriteRenderer>(); sr.drawMode = SpriteDrawMode.Tiled; sr.size = new Vector2(r.W, 8.5f);
                        float k = 0.6f + i * 0.15f; sr.color = new Color(k, k, k);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 실험실 기본 깊이: 원경(먼 층·조명, 느리게) → 중경(장비·배관) → 유리 띠가 뚫린 안쪽 벽 → 플레이 영역 → 전경 기둥(빠르게).
        /// 벽 위·아래만 막고 가운데를 유리로 두어 깊이가 보이게 한다. 장식은 상호작용 대상을 가리지 않게 화면 가장자리에 둔다.
        /// </summary>
        public static void LabLayers(Ctx ctx, int variant, float glassBottom = -1, float glassTop = -1)
        {
            var r = ctx.room;
            float floorY = FloorTop(r);
            if (glassBottom < 0) glassBottom = floorY + 1.5f;
            if (glassTop < 0) glassTop = Mathf.Min(r.H - 2.5f, floorY + 6.5f);
            var far = Ctx.S("L_Far"); var mid = Ctx.S(variant % 2 == 0 ? "L_MidA" : "L_MidB");
            ctx.Band(Ctx.P0("Tile_Pixel"), new Vector2(r.W / 2f, 0), r.W + 30, -60, 0.9f, new Color(0.05f, 0.06f, 0.09f), "FarFill").transform.localScale = new Vector3(1, (r.H + 10) / 0.125f, 1);
            ctx.Band(far, new Vector2(r.W / 2f, floorY - 1.5f), r.W + 24, -50, 0.6f, new Color(0.3f, 0.36f, 0.36f), "Far");
            ctx.Band(mid, new Vector2(r.W / 2f, floorY - 0.2f), r.W + 12, -45, 0.3f, new Color(0.42f, 0.48f, 0.47f), "Mid");
            // 안쪽 벽 (유리 띠 위아래)
            var wall = Ctx.S("L_WallPanel");
            var lower = ctx.Layer(wall, new Vector2(r.W / 2f, (floorY + glassBottom) / 2f), -40, 0, null, 1, "WallLower");
            var lsr = lower.GetComponent<SpriteRenderer>(); lsr.drawMode = SpriteDrawMode.Tiled; lsr.size = new Vector2(r.W, glassBottom - floorY);
            var upper = ctx.Layer(wall, new Vector2(r.W / 2f, (glassTop + r.H) / 2f), -40, 0, null, 1, "WallUpper");
            var usr = upper.GetComponent<SpriteRenderer>(); usr.drawMode = SpriteDrawMode.Tiled; usr.size = new Vector2(r.W, Mathf.Max(0.5f, r.H - glassTop));
            // 유리 띠: 얇은 반사광과 창틀
            var px = Ctx.P0("Tile_Pixel"); float pxs = px != null ? px.bounds.size.x : 0.125f;
            var glass = ctx.Layer(px, new Vector2(r.W / 2f, (glassBottom + glassTop) / 2f), -39, 0, new Color(0.4f, 0.62f, 0.56f, 0.05f), 1, "Glass");
            glass.transform.localScale = new Vector3(r.W / pxs, (glassTop - glassBottom) / pxs, 1);
            foreach (float y in new[] { glassBottom, glassTop })
            {
                var bar = ctx.Layer(px, new Vector2(r.W / 2f, y), -38, 0, new Color(0.19f, 0.23f, 0.24f), 1, "Frame");
                bar.transform.localScale = new Vector3(r.W / pxs, 0.25f / pxs, 1);
            }
            for (float x = 3; x < r.W; x += 6)
            {
                var m = ctx.Layer(px, new Vector2(x, (glassBottom + glassTop) / 2f), -38, 0, new Color(0.16f, 0.2f, 0.21f), 1, "Mullion");
                m.transform.localScale = new Vector3(0.25f / pxs, (glassTop - glassBottom) / pxs, 1);
            }
            // 전경 기둥 (양 끝, 상호작용을 가리지 않는 위치)
            // 짧은 방에서만 양 끝에 둔다 (긴 방에서는 카메라를 따라 화면 가운데를 지나가며 인물·조사 대상을 가리므로 쓰지 않는다)
            if (r.W <= 30)
            {
                ctx.Layer(Ctx.S("L_ForePillar"), new Vector2(0.6f, 0), 60, -0.15f, null, 1, "ForePillarL");
                ctx.Layer(Ctx.S("L_ForePillar"), new Vector2(r.W - 0.6f, 0), 60, -0.15f, null, 1, "ForePillarR");
            }
            ctx.Band(Ctx.S("L_ForePipe"), new Vector2(r.W / 2f, r.H - 1.2f), r.W + 8, 60, -0.25f, null, "ForePipe");
            // 떠다니는 먼지
            ctx.Particles("Dust", Ctx.S("FX_Dust"), 18, new Vector2(0.05f, 0.08f), new Vector2(0.1f, 0.06f), new Color(0.7f, 0.8f, 0.75f, 0.22f), new Color(0.6f, 0.7f, 0.65f, 0.1f), 8f, 30, false);
            // 낮은 대비의 시설 흔적 (번호표·보관함·불투명 용기·관찰창 실루엣): 진행 대상보다 어둡게
            var junk = new[] { "S_Lockers", "S_Jar1", "S_ObsWindow", "S_Jar0", "S_BodyBoard" };
            int k = variant;
            for (float x = 4.5f; x < r.W - 3; x += 7.5f)
            {
                var sp = Ctx.S(junk[k++ % junk.Length]);
                if (sp == null) continue;
                ctx.Layer(sp, new Vector2(x, floorY), -36, 0.06f, new Color(0.3f, 0.36f, 0.35f), 1, "WallJunk");
            }
        }

        /// <summary>바닥 윗면 높이 (마지막 줄부터 위로 올라가며 처음 빈칸이 나오는 곳).</summary>
        public static float FloorTop(RoomDef r)
        {
            int col = Mathf.Min(2, r.W - 1);
            for (int row = r.H - 1; row >= 0; row--)
                if (r.rows[row][col] != '#') return r.H - 1 - row;
            return 1;
        }

        static Sprite SolidSprite(Theme t)
        {
            switch (t)
            {
                case Theme.Lab: return Ctx.S("L_FloorFill");
                case Theme.Home: return Ctx.P0("Tile_HomeFloor");
                case Theme.Digital: return Ctx.P0("Tile_DigitalBlock");
                case Theme.Village: return Ctx.S("V_GroundFill");
                case Theme.House: return Ctx.S("H_Floor");
                case Theme.Desktop: return Ctx.S("D_Bar");
                default: return Ctx.P0("Tile_Roof");
            }
        }

        static Sprite TopSprite(Theme t)
        {
            switch (t)
            {
                case Theme.Lab: return Ctx.S("L_Floor");
                case Theme.Village: return Ctx.S("V_Ground");
                default: return null;
            }
        }

        /// <summary>'#' 칸을 가로로 묶고, 같은 폭의 아래 행과 다시 묶어 사각형으로 만든다.</summary>
        static void BuildSolids(Ctx ctx)
        {
            var r = ctx.room;
            var used = new bool[r.H, r.W];
            var parent = ctx.Obj("Solids", Vector2.zero).transform;
            var sprite = SolidSprite(r.theme);
            for (int row = 0; row < r.H; row++)
                for (int col = 0; col < r.W; col++)
                {
                    char ch = r.rows[row][col];
                    if (used[row, col] || (ch != '#' && ch != '=' && ch != '|')) continue;
                    int c1 = col;
                    while (c1 + 1 < r.W && r.rows[row][c1 + 1] == ch && !used[row, c1 + 1]) c1++;
                    int r1 = row;
                    if (ch == '#' || ch == '|')
                        while (r1 + 1 < r.H && SameRun(r, r1 + 1, col, c1, used, ch)) r1++;
                    for (int y = row; y <= r1; y++) for (int x = col; x <= c1; x++) used[y, x] = true;
                    float x0 = col, x1 = c1 + 1, yTop = r.H - row, yBot = r.H - 1 - r1;
                    if (ch == '=')
                    {
                        var go = ctx.Obj("OneWay", new Vector2((x0 + x1) / 2f, yTop - 0.15f), parent);
                        if (!r.hidePlatforms) { var sr = ctx.Sprite(go, Ctx.P0("Tile_Platform"), 0); sr.drawMode = SpriteDrawMode.Tiled; sr.size = new Vector2(x1 - x0, 0.25f); }
                        var bc = go.AddComponent<BoxCollider2D>(); bc.size = new Vector2(x1 - x0, 0.3f); bc.offset = Vector2.zero;
                        go.AddComponent<OneWayPlatform>();
                    }
                    else
                    {
                        var go = ctx.Obj(ch == '|' ? "InvisibleWall" : "Solid", new Vector2((x0 + x1) / 2f, (yTop + yBot) / 2f), parent);
                        if (ch == '#') { var sr = ctx.Sprite(go, sprite, 0); sr.drawMode = SpriteDrawMode.Tiled; sr.size = new Vector2(x1 - x0, yTop - yBot); }
                        var bc = go.AddComponent<BoxCollider2D>(); bc.size = new Vector2(x1 - x0, yTop - yBot); bc.offset = Vector2.zero;   // 그림 피벗과 무관하게
                        // 밟는 윗면이 드러난 곳에는 바닥 윗면 그림을 한 줄 덧댄다
                        var top = TopSprite(r.theme);
                        if (ch == '#' && top != null && row > 0 && r.rows[row - 1][col] != '#')
                        {
                            var tg = ctx.Obj("SolidTop", new Vector2((x0 + x1) / 2f, yTop - 0.5f), parent);
                            var tsr = ctx.Sprite(tg, top, 1); tsr.drawMode = SpriteDrawMode.Tiled; tsr.size = new Vector2(x1 - x0, 1);
                        }
                    }
                }
        }

        /// <summary>아래 행이 정확히 같은 가로 구간(col..c1)의 '#' 인가.</summary>
        static bool SameRun(RoomDef r, int row, int col, int c1, bool[,] used, char ch)
        {
            for (int c = col; c <= c1; c++) if (r.rows[row][c] != ch || used[row, c]) return false;
            if (col > 0 && r.rows[row][col - 1] == ch) return false;
            if (c1 + 1 < r.W && r.rows[row][c1 + 1] == ch) return false;
            return true;
        }

        static void BuildLadders(Ctx ctx)
        {
            var r = ctx.room;
            var ladderSprite = Ctx.P0("Prop_Ladder");
            for (int col = 0; col < r.W; col++)
            {
                int row = 0;
                while (row < r.H)
                {
                    char ch = r.rows[row][col];
                    if (ch != 'H' && ch != '^') { row++; continue; }
                    int top = row;
                    while (row < r.H && (r.rows[row][col] == 'H' || r.rows[row][col] == '^')) row++;
                    int bottom = row - 1;
                    float yTop = r.H - top, yBot = r.H - 1 - bottom;
                    // 그림 피벗(보통 아래 가운데)에 맞춰 위치를 잡아 그림과 오르는 구간이 일치하게 한다
                    float pv = ladderSprite != null && ladderSprite.rect.height > 0 ? ladderSprite.pivot.y / ladderSprite.rect.height : 0.5f;
                    float gy = yBot + (yTop - yBot) * pv;
                    var go = ctx.Obj("Ladder", new Vector2(col + 0.5f, gy));
                    var sr = ctx.Sprite(go, ladderSprite, -1); sr.drawMode = SpriteDrawMode.Tiled; sr.size = new Vector2(1, yTop - yBot);
                    var bc = go.AddComponent<BoxCollider2D>(); bc.isTrigger = true;
                    bc.size = new Vector2(0.7f, yTop - yBot + 0.7f); bc.offset = new Vector2(0, (yTop + yBot) / 2f - gy + 0.35f);
                    var l = go.AddComponent<Ladder>(); l.topY = yTop; l.bottomY = yBot;
                    if (r.rows[top][col] == '^')
                    {
                        var cover = ctx.Obj("LadderCover", new Vector2(col + 0.5f, yTop - 0.05f));
                        var cbc = cover.AddComponent<BoxCollider2D>(); cbc.size = new Vector2(1f, 0.1f);
                        cover.AddComponent<OneWayPlatform>();
                    }
                }
            }
        }

        static void UpdateBuildSettings()
        {
            var list = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(SceneDir + "/Title.unity", true) };
            foreach (var r in EWRooms.All()) list.Add(new EditorBuildSettingsScene(SceneDir + "/" + r.scene + ".unity", true));
            // 기존 항목 중 실제 파일이 있는 것은 뒤에 보존 (비활성)
            foreach (var s in EditorBuildSettings.scenes)
                if (!list.Any(x => x.path == s.path) && System.IO.File.Exists(s.path)) list.Add(new EditorBuildSettingsScene(s.path, false));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
