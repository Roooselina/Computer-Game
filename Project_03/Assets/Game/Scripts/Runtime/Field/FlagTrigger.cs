using UnityEngine;

namespace EW
{
    /// <summary>
    /// 조건이 갖춰지면 이벤트를 한 번 실행한다 (조작 중일 때만). 저장·불러오기 뒤에도 doneFlag 로 다시 실행하지 않는다.
    /// 조건: allFlags(쉼표 구분)가 모두 있음 + noneFlag 없음 + (deviceOffId 가 있으면 그 장치가 꺼져 있음) + (zone 이 있으면 루가 그 안).
    /// 예: 생존자 셋이 모두 진정 → 대화, 시험대 근처에서 펄스 사용 → 머리 곤두섬, 홀 전원선 정전 → 문도 꺼졌다는 반응.
    /// </summary>
    public class FlagTrigger : MonoBehaviour
    {
        public string allFlags;
        public string noneFlag;
        public string doneFlag;
        public string deviceOffId;
        public Rect zone;
        public bool useZone;
        public string eventId;
        string[] parts;

        void Update()
        {
            var g = Game.I;
            if (g == null || g.State == null || !g.FieldInputAllowed || g.Player == null) return;
            var s = g.State;
            if (!string.IsNullOrEmpty(doneFlag) && s.Has(doneFlag)) return;
            if (!string.IsNullOrEmpty(noneFlag) && s.Has(noneFlag)) return;
            parts ??= string.IsNullOrEmpty(allFlags) ? new string[0] : allFlags.Split(',');
            foreach (var f in parts) if (!s.Has(f.Trim())) return;
            if (!string.IsNullOrEmpty(deviceOffId))
            {
                var d = Device.Find(deviceOffId);
                if (d == null || d.Model.power == DevicePower.On) return;
            }
            if (useZone && !zone.Contains((Vector2)g.Player.transform.position)) return;
            if (!string.IsNullOrEmpty(doneFlag)) s.Set(doneFlag);
            g.RunEvent(StoryEvents.Run(g, eventId));
        }
    }
}
