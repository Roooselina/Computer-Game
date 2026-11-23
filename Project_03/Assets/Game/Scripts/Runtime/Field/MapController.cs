using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 맵마다 하나. 맵 ID·방 경계·낙하 복귀 높이를 갖고, 시작하면 Game 에 자신을 알린다.
    /// </summary>
    public class MapController : MonoBehaviour
    {
        public string mapId = "M00";
        public string title = "";
        public Rect bounds = new Rect(0, 0, 20, 12);
        public float killMargin = 3f;
        public Color background = new Color(0.08f, 0.08f, 0.12f);
        [Header("소리")]
        public string ambience;
        public string music;
        [Tooltip("이 플래그가 있으면 환경음을 altAmbience 로 (예: 정전 이후)")] public string altAmbienceFlag;
        public string altAmbience;
        [Tooltip("연출 전용 장소: HUD 를 숨긴다")] public bool hideHud;
        [Tooltip("시설 경보가 켜지면 경보 환경음·반복 방송을 쓰는 장소")] public bool useAlarmAmbience;
        [Tooltip("컴퓨터·포획 단말 안: WASD 무중력 이동")] public bool zeroG;
        [Tooltip("동료를 따라오게 하지 않는 장면 맵 (단말 안 등)")] public bool noCompanion;
        [Tooltip("포획 단말 안: 큰 방전 봉인 (GDD 3.3)")] public bool sealSkills;
        public float KillY => bounds.yMin - killMargin;

        readonly List<SpawnPoint> spawns = new List<SpawnPoint>();

        void Awake() { GetComponentsInChildren(true, spawns); }

        void Start()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.backgroundColor = background;
                PixelScreen.Ensure().Attach(cam);
            }
            var g = Game.Ensure();
            if (g != null) g.OnMapReady(this);
        }

        public SpawnPoint GetSpawn(string id)
        {
            if (spawns.Count == 0) GetComponentsInChildren(true, spawns);
            foreach (var s in FindSpawnsInScene()) if (s.id == id) return s;
            return null;
        }

        /// <summary>시작 위치는 MapController 아래에 둔다 (구성 도구가 그렇게 만든다).</summary>
        List<SpawnPoint> FindSpawnsInScene()
        {
            GetComponentsInChildren(true, spawns);
            return spawns;
        }

        public Vector2 DefaultSpawn()
        {
            var s = GetSpawn("start");
            if (s != null) return s.transform.position;
            var all = FindSpawnsInScene();
            return all.Count > 0 ? (Vector2)all[0].transform.position : bounds.center;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
