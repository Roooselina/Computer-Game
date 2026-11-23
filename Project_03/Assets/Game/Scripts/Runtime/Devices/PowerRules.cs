using System;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    public enum DevicePower { On = 0, Off = 1, Broken = 2 }
    public enum DeviceKind { Machine, Computer, Door, Light, Terminal, Sensor, Vending }

    /// <summary>장치 한 개의 상태. GDD 3.3: 전선·배터리·스위치만 알면 되는 규칙.</summary>
    [Serializable]
    public class DeviceModel
    {
        public string id;
        public DeviceKind kind;
        public DevicePower power = DevicePower.On;
        public bool hasBattery;
        [Range(0, 100)] public int battery = 100;
        [Tooltip("진행에 필요한 장치. 영구 고장 나지 않는다.")]
        public bool essential;
        [Tooltip("같은 전원선을 쓰는 장치 묶음. '전원 복구' 버튼이 이 이름으로 복구한다.")]
        public string line;
        [Tooltip("큰 방전에 직접 맞으면 고장 나는 장치 (센서 등).")]
        public bool fragile;
        [Tooltip("이 장치가 꺼지면 함께 조작이 멈추는 제어 장치 id (문 → 제어 단말).")]
        public string controllerId;

        public bool Running => power == DevicePower.On;
    }

    /// <summary>
    /// 장치 상태 변화 규칙. 필드와 전투가 같은 규칙을 쓴다.
    /// - 전원선으로 전기를 받는 기계: 전원이 끊기면 멈춘다. '전원 복구' 버튼으로 다시 켠다.
    /// - 배터리가 붙은 기계: 정전이 돼도 배터리가 남아 있으면 계속 작동한다.
    /// - 컴퓨터: 꺼지면 화면이 검어지고 다시 켜면 부팅. 저장된 파일은 지워지지 않는다.
    /// - 망가진 기계: 버튼으로 고쳐지지 않는다. 필수 장치는 영구 고장 대신 꺼짐으로 처리한다.
    /// </summary>
    public static class PowerRules
    {
        public const int DischargeBatteryCost = 20;

        /// <summary>정전(외부 전원 상실). 배터리 장치는 버틴다.</summary>
        public static void Outage(DeviceModel d)
        {
            if (d.power == DevicePower.Broken) return;
            if (d.hasBattery && d.battery > 0) return;
            d.power = DevicePower.Off;
        }

        /// <summary>
        /// 큰 방전에 휘말림. 직접 손상(fragile)은 배터리가 있어도 고장 날 수 있다.
        /// 배터리 장치는 잔량이 줄지만 남아 있으면 계속 작동한다.
        /// </summary>
        public static void Discharge(DeviceModel d)
        {
            if (d.power == DevicePower.Broken) return;
            if (d.fragile)
            {
                d.power = d.essential ? DevicePower.Off : DevicePower.Broken;
                return;
            }
            if (d.hasBattery && d.battery > 0)
            {
                d.battery = Mathf.Max(0, d.battery - DischargeBatteryCost);
                if (d.battery > 0) return;
            }
            d.power = DevicePower.Off;
        }

        /// <summary>스위치로 직접 켜고 끄기. 고장은 바뀌지 않는다.</summary>
        public static bool SetSwitch(DeviceModel d, bool on)
        {
            if (d.power == DevicePower.Broken) return false;
            d.power = on ? DevicePower.On : DevicePower.Off;
            return true;
        }

        /// <summary>'전원 복구' 버튼. 같은 선의 꺼진 장치만 다시 켠다. 고장은 그대로.</summary>
        public static int RestoreLine(IEnumerable<DeviceModel> devices, string line)
        {
            int n = 0;
            foreach (var d in devices)
                if (d.line == line && d.power == DevicePower.Off) { d.power = DevicePower.On; n++; }
            return n;
        }

        /// <summary>문 같은 피제어 장치가 실제로 동작하는가.</summary>
        public static bool Operable(DeviceModel d, Func<string, DeviceModel> find)
        {
            if (d.power != DevicePower.On) return false;
            if (string.IsNullOrEmpty(d.controllerId)) return true;
            var c = find(d.controllerId);
            return c == null || c.power == DevicePower.On;
        }

        public static string Describe(DeviceModel d)
        {
            switch (d.power)
            {
                case DevicePower.Broken: return "고장 났다. 버튼만으로는 고칠 수 없다.";
                case DevicePower.Off:
                    if (d.kind == DeviceKind.Computer) return "화면이 꺼져 있다. 저장된 파일은 그대로 남아 있다. 전원을 켜자.";
                    return string.IsNullOrEmpty(d.line) ? "전원이 꺼져 있다." : "전원이 꺼져 있다. 근처의 '전원 복구' 버튼으로 다시 켤 수 있다.";
                default:
                    if (d.hasBattery) return "배터리로 작동 중이다. (잔량 " + d.battery + "%)";
                    return "작동 중이다.";
            }
        }
    }
}
