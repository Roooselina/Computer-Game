using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>지원 병력 출입 지점 (맵 구성 도구가 문·환기구 앞에 둔다).</summary>
    public class ReinforcePoint : MonoBehaviour
    {
        public static readonly List<ReinforcePoint> All = new List<ReinforcePoint>();
        void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        void OnDisable() { All.Remove(this); }
    }

}
