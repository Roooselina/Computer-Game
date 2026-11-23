# 부록 — 씬에 연결되지 않은 사건·대사 (정적 분석)

> 자동 추출. ‘씬에 없음’ = 생성된 어떤 씬의 구역·조사 지점도 이 사건 ID 를 부르지 않는다. 다른 코드에서 직접 부르면 ‘코드에서 호출’로 표시한다. 도달 불가 확정은 본문에서 따로 판단한다.

## 씬에 없는 사건 ID

- `c1_rest_room_enter` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs · 직접 호출 흔적 없음
- `c1_shortcut` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs · 코드에서 직접 호출 흔적 있음
- `c2_glass` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs · 코드에서 직접 호출 흔적 있음
- `c2s_pull_tut` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs · 코드에서 직접 호출 흔적 있음
- `c2s_tray_tut` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs · 직접 호출 흔적 없음
- `c3_rest` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs · 코드에서 직접 호출 흔적 있음
- `demo_end` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs · 직접 호출 흔적 없음
- `pc_exit` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs · 코드에서 직접 호출 흔적 있음
- `pc_log` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs · 코드에서 직접 호출 흔적 있음
- `pc_photo` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs · 직접 호출 흔적 없음
- `pc_status` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs · 직접 호출 흔적 없음
- `pc_trash` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs · 직접 호출 흔적 없음

## 코드에서 문자열로 참조되지 않는 대사 ID (53개)

> 문자열 조합(예: 접두어 + 번호)이나 데이터(말풍선·대사 목록)로 쓰이는 경우가 있어 곧바로 ‘죽은 대사’는 아니다.

- `c1_debris_inspect` — 천장이 무너져 복도를 막았다. 위 통로나 아래 점검 통로로 돌아갈 수 있다.
- `c1_door_off` — 셔터가 멈춰 있다. 단말 전원을 켜자. 근처에 '전원 복구' 버튼이 있다.
- `c1_emergency_light` — 독립 비상등. 배터리가 붙어 있어서 정전에도 켜져 있다.
- `c1_escape_look` — 앞이 막혔어... 카메라도 있고.
- `c1_researcher_down` — 연구원이 쓰러져 꼼짝하지 않는다.
- `c1_shutter_locked` — 셔터가 닫혀 있다. 옆 제어판에 전기가 들어오지 않는다.
- `c1_terminal_door` — ...문도 같이 꺼졌네.
- `c1_trench_inspect` — 바닥 아래 점검 통로. 잔해 밑으로 지나갈 수 있다.
- `c1_wire_first` — 이쪽이야! 반짝반짝!
- `c1_wire_inspect` — 바닥에 끊어진 전선이 떨어져 있다. 끝에서 불꽃이 튄다. 싸울 때 쓸 수 있겠다.
- `c2_cargo_sign` — 화물 적재장. 위쪽 통로는 전원선으로 건너갈 수 있다.
- `c2_window_again` — ...금방 올게.
- `c2s_quiet` — ...아무한테도 안 들켰어.
- `c3_carry_start` — 베넬을 부축해 관찰 휴게실까지 가자. 부축하는 동안은 천천히 걷기만 할 수 있다.
- `c3_carry_wrongway` — 누나 먼저 쉬게 해야 해. 휴게실은 저쪽!
- `c3_join` — 베넬이 당분간 따라오기로 했다.
- `c3_support` — ...잡아. 저쪽에 쉴 데가 있을 거야.
- `ch7_wip` — 여기부터 7장 「접수는 취소가 안 된대」입니다. 아직 만들지 않은 구간입니다.
- `lab_m_gap_locked` — 틈이 있어... 저기로 나갈 수 있을까.
- `lab_wake` — 여기 어디야...?
- `op_h_1` — .....
- `op_h_2` — 나... 난....
- `op_h_3` — 난 이제 어떻게 살아야 해..?
- `op_t_3` — 뭐야!?
- `op_t_4` — 도와줘!
- `op_t_5` — 도와주세요!
- `op_t_calm2` — 괜찮아. 진정하렴?
- `op_t_r2` — 그러게. 계획했던 것보다 일러.
- `op_v_1` — .....
- `op_v_3` — ....
- `pc_guide_after` — ...이게 다, 내 전기로 할 수 있는 거라고?
- `sys_barrel` — 약품 통. 위험 표시가 붙어 있다.
- `sys_battle_tutorial` — 화면 아래 명령 띠에서 행동을 고릅니다. 대상을 고를 때 예상 피해가 표시됩니다.
- `v_after_join` — ...따라가는 거, 아니야. 누나가... 길 아니까.
- `v_kneel_reach` — ...괜찮아? 일어날 수 있어?
- `v_meet_1` — 어라라?
- `v_meet_2` — 후흐... 하학...
- `v_meet_lu` — 오, 오지 마...
- `v_steps` — 터벅...
- `v_talk1` — ...응? 할 말 있어~?
- `v_talk1_good` — 도망친... 애네?
- `v_talk1_silent` — 대답 안 해 주면... 누나 심심한데에~
- `v_talk1_threat` — 와하학학! 이자~식~ 정말 재미있네?
- `v_talk2` — 하아... 어디였더라아...
- `v_talk2_command` — 명령... 싫어. 싫어어!
- `v_talk2_good` — 누구... 였더라. 작은 애... 과자 좋아하는...
- `v_talk2_mock` — 이상한... 누나가, 이상해...?
- `v_talk3` — ...왜 안 때렸어? 아까... 때릴 수 있었는데.
- `v_talk3_cmd` — ...따라가는 거, 싫어. ...그래도, 조금만 더 생각할게.
- `v_talk3_good` — ...응. 잠깐만... 들어 줄게.
- `v_talk_wait1` — 응~? ...잠깐, 누나 생각 중이야...
- `v_talk_wait2` — 하아... 하아...
- `w_first_3` — 엥?!
