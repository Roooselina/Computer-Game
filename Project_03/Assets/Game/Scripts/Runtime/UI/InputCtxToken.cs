using UnityEngine;

namespace EW
{
    /// <summary>UI 오브젝트가 사라지면(씬 전환 포함) 그 UI 가 연 입력 상태를 함께 닫는다. 상태가 남아 입력이 막히는 일을 막는다.</summary>
    public class InputCtxToken : MonoBehaviour
    {
        public int handle = -1;
        public static InputCtxToken Attach(GameObject go, InputCtx ctx)
        {
            var t = go.AddComponent<InputCtxToken>();
            t.handle = GameInput.Push(ctx);
            return t;
        }
        public void Release() { if (handle >= 0) { GameInput.Pop(handle); handle = -1; } }
        void OnDestroy() { Release(); }
    }
}
