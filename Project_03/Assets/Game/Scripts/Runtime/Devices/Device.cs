using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 필드의 장치 하나 (기계·컴퓨터·문·조명·제어 단말·센서·자판기).
    /// 상태는 저장(world_devices)되어 필드와 전투가 같은 값을 쓴다.
    /// </summary>
    public class Device : MonoBehaviour
    {
        public static readonly List<Device> All = new List<Device>();
        public DeviceModel Model = new DeviceModel();
        [Header("그림 (켜짐/꺼짐/고장/부팅)")]
        public Sprite[] onFrames;
        public Sprite[] offFrames;
        public Sprite[] brokenFrames;
        public Sprite[] bootFrames;
        public float fps = 8f;
        [Tooltip("배터리 잔량 막대 (선택)")] public SpriteRenderer batteryBar;
        public float batteryBarWidth = 0.5f;
        [Tooltip("문처럼 막히는 콜라이더 (동작 불가일 때 막힘)")] public Collider2D blocker;
        [Tooltip("이 문은 열린 상태가 기본")] public bool openWhenOperable = true;
        [Tooltip("이 플래그가 없으면 잠김")] public string requireFlag;

        SpriteRenderer sr;
        float t;
        float bootTimer;
        bool highlight;
        public bool Booting => bootTimer > 0;

        void Awake() { sr = GetComponent<SpriteRenderer>(); }
        void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public static Device Find(string id) { foreach (var d in All) if (d.Model.id == id) return d; return null; }
        public static DeviceModel FindModel(string id) => Find(id)?.Model;

        public bool Operable => PowerRules.Operable(Model, FindModel) && (string.IsNullOrEmpty(requireFlag) || (Game.I != null && Game.I.State != null && Game.I.State.Has(requireFlag)));

        public void LoadFrom(GameState gs)
        {
            var s = gs.GetDevice(Model.id);
            if (s != null) { Model.power = (DevicePower)s.power; Model.hasBattery = s.hasBattery; Model.battery = s.battery; }
            Refresh();
        }

        public void StoreTo(GameState gs) => gs.StoreDevice(Model.id, Model.power, Model.hasBattery, Model.battery);

        public void SetPower(DevicePower p)
        {
            var old = Model.power;
            Model.power = p;
            if (old != DevicePower.On && p == DevicePower.On && Model.kind == DeviceKind.Computer) bootTimer = 1.6f;
            Changed();
        }

        /// <summary>상태가 바뀌면 저장값과 연결된 장치(문 등)를 갱신한다.</summary>
        public void Changed()
        {
            if (Game.I != null && Game.I.State != null) StoreTo(Game.I.State);
            foreach (var d in All) d.Refresh();
        }

        public void Refresh()
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (blocker != null) blocker.enabled = !(Operable && openWhenOperable);
            if (batteryBar != null)
            {
                batteryBar.gameObject.SetActive(Model.hasBattery);
                float w = batteryBarWidth * Model.battery / 100f;
                batteryBar.transform.localScale = new Vector3(Mathf.Max(0.001f, w * 8f), batteryBar.transform.localScale.y, 1);
                batteryBar.color = Model.battery > 40 ? new Color(0.5f, 1f, 0.5f) : Model.battery > 0 ? new Color(1f, 0.8f, 0.3f) : new Color(0.4f, 0.2f, 0.2f);
            }
        }

        Sprite[] CurrentFrames()
        {
            if (Booting && bootFrames != null && bootFrames.Length > 0) return bootFrames;
            if (Model.kind == DeviceKind.Door) return Operable && openWhenOperable ? onFrames : offFrames;
            switch (Model.power)
            {
                case DevicePower.Broken: return brokenFrames != null && brokenFrames.Length > 0 ? brokenFrames : offFrames;
                case DevicePower.Off: return offFrames;
                default: return onFrames;
            }
        }

        void Update()
        {
            if (bootTimer > 0) { bootTimer -= Time.deltaTime; if (bootTimer <= 0) Refresh(); }
            var f = CurrentFrames();
            if (sr != null && f != null && f.Length > 0)
            {
                t += Time.deltaTime * fps;
                sr.sprite = f[(int)t % f.Length];
            }
            if (sr != null)
                sr.color = highlight && (Time.time * 4f % 1f) < 0.5f ? new Color(1f, 1f, 0.55f) : Color.white;
        }

        public void SetHighlight(bool on) { highlight = on; }
    }
}
