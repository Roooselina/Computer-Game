namespace EW
{
    /// <summary>
    /// AA/DD 더블탭 판정. 같은 방향 눌림 → 뗌 → 눌림이 window 안이면 대시.
    /// - 기록(record)과 발동(fire)을 나눈다: 대시 중·재사용 대기 중에도 '첫 번째 누름'은 기록해 두고,
    ///   두 번째 누름 순간에 발동할 수 있을 때만 대시한다 (대기 중 누른 첫 탭이 버려져 한 번 더 기다리게 되던 문제 수정).
    /// - 두 번째 누름에 발동할 수 없으면 그 누름을 새 첫 탭으로 삼을 뿐, 입력을 저장해 두었다가 늦게 발동하지 않는다.
    /// - 길게 누르기·키 반복(눌림 상태 유지)은 뗌이 없으므로 대시가 아니다. 반대 방향·동시 누름은 기록을 지운다.
    /// - record 가 false(대화·메뉴·사다리·연출 등)이면 기록을 지운다.
    /// </summary>
    public sealed class DoubleTapDash
    {
        int direction;
        float firstTime;
        bool released;
        public void Reset() { direction = 0; released = false; }

        public int Poll(float now, bool leftDown, bool rightDown, bool leftUp, bool rightUp, bool record, bool fire, float window)
        {
            if (!record || (leftDown && rightDown)) { Reset(); return 0; }
            if (direction != 0 && now - firstTime > window) Reset();
            if ((direction == -1 && leftUp) || (direction == 1 && rightUp)) released = true;
            int pressed = leftDown ? -1 : rightDown ? 1 : 0;
            if (pressed == 0) return 0;
            if (direction == pressed && released && now - firstTime <= window && fire)
            { Reset(); return pressed; }
            direction = pressed; firstTime = now; released = false;
            return 0;
        }

        /// <summary>이전 형식 (기록·발동 조건이 같을 때).</summary>
        public int Poll(float now, bool leftDown, bool rightDown, bool leftUp, bool rightUp, bool eligible, float window)
            => Poll(now, leftDown, rightDown, leftUp, rightUp, eligible, eligible, window);
    }
}
