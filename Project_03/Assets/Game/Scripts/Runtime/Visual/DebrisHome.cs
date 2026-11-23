using UnityEngine;

namespace EW
{
    /// <summary>연출로 움직이는 파편의 최종 위치. 컷신을 건너뛰면 이 위치로 되돌린다.</summary>
    public class DebrisHome : MonoBehaviour
    {
        Vector3 home;
        bool init;
        void Awake() { if (!init) { init = true; home = transform.position; } }
        public void Home() { Awake(); transform.position = home; }
    }
}
