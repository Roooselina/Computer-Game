using System.Collections.Generic;

namespace EW
{
    /// <summary>보스 HP 바에 보일 것 (메이린·라비엘 대결·라크리모사·아우렐리우스). 베넬은 기존 표시를 그대로 쓴다.</summary>
    public interface IBossBar
    {
        bool BarVisible { get; }
        string BarName { get; }
        /// <summary>단계·상태 (예: "1단계 · 외피와 호위 장치"). 색만이 아니라 글자로 알린다.</summary>
        string BarPhase { get; }
        int BarHp { get; }
        int BarMax { get; }
    }

    public static class BossBars
    {
        public static readonly List<IBossBar> All = new List<IBossBar>();
        public static void Add(IBossBar b) { if (b != null && !All.Contains(b)) All.Add(b); }
        public static void Remove(IBossBar b) { All.Remove(b); }
        public static IBossBar Current
        {
            get
            {
                for (int i = All.Count - 1; i >= 0; i--)
                {
                    var b = All[i];
                    if (b == null || (b is UnityEngine.Object o && o == null)) { All.RemoveAt(i); continue; }
                    if (b.BarVisible) return b;
                }
                return null;
            }
        }
    }
}
