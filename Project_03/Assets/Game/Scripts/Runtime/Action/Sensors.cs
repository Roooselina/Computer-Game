using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    public enum SensorState { Idle, Suspicious, Checking, Alert, Searching, Off }

    /// <summary>발각 게이지를 가진 감시자 (경비·드론·CCTV). HUD 가 이 목록으로 머리 위 표시를 그린다.</summary>
    public interface ISensor
    {
        bool SensorActive { get; }
        Vector2 MarkPos { get; }
        float Suspicion { get; }
        SensorState SState { get; }
        /// <summary>지금 루를 직접 보고 있는지.</summary>
        bool SeesPlayer { get; }
    }

    public static class Sensors
    {
        public static readonly List<ISensor> All = new List<ISensor>();
        public static void Add(ISensor s) { if (!All.Contains(s)) All.Add(s); }
        public static void Remove(ISensor s) { All.Remove(s); }

        /// <summary>가장 높은 위험 단계 (HUD 표시용).</summary>
        public static SensorState Worst(out float maxSusp)
        {
            maxSusp = 0; var w = SensorState.Idle;
            foreach (var s in All)
            {
                if (s == null || !s.SensorActive) continue;
                maxSusp = Mathf.Max(maxSusp, s.Suspicion);
                if (Rank(s.SState) > Rank(w)) w = s.SState;
            }
            return w;
        }

        static int Rank(SensorState s)
        {
            switch (s) { case SensorState.Alert: return 4; case SensorState.Searching: return 3; case SensorState.Checking: return 2; case SensorState.Suspicious: return 1; }
            return 0;
        }

        /// <summary>근처 경비에게 위치를 알린다. hunt 면 추적 속도로 달려와 그 자리를 확인한다 (계속 위치를 알려 주지는 않는다).</summary>
        public static int AlertGuards(Vector2 pos, float radius, Guard except = null, bool hunt = false)
        {
            int n = 0;
            foreach (var gd in Guard.All)
            {
                if (gd == null || gd == except || !gd.Alive) continue;
                if (Vector2.Distance(gd.transform.position, pos) > radius) continue;
                gd.Alert(pos, hunt); n++;
            }
            return n;
        }

        /// <summary>마지막으로 감시 장치가 경비를 부른 시각 (HUD 경고용).</summary>
        public static float LastCallTime = -99;
        public static string LastCallText = "";
    }

    /// <summary>시설 경보 (사건 상태). 경보가 켜져 있어도 경비가 루의 위치를 아는 것은 아니다.</summary>
    public static class Facility
    {
        public const string AlarmFlag = "lab_alarm";
        /// <summary>탈출 직후부터 베넬 만남까지의 시설 경보. 3장 결말 뒤에는 (옛 저장 변환으로 플래그가 남았더라도) 울리지 않는다.</summary>
        public static bool AlarmOn => Game.I != null && Game.I.State != null && Game.I.State.Has(AlarmFlag) && !Game.I.State.Has("c3_boss_done");
    }
}
