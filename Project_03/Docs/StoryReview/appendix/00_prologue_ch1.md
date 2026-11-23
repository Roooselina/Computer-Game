# 부록 — 장면별 실제 대사 (정적 추출: 00_prologue_ch1)

> 자동 추출 (2026-11-16). 씬 보고서(`Docs/Generated/built_layout_report.json`, 실제 생성 씬의 사건 ID) → 사건 함수 → 호출하는 함수까지 따라가며 참조된 대사 ID → `dialogue_ko.json` 원문.
> **정적 분석이다.** 한 함수 안의 분기(동행/비동행 등) 대사가 모두 함께 나열된다. 어느 조건에서 어느 대사가 나오는지는 본문 CURRENT_STORY 의 장면 설명을 본다. *(보충)* 표시는 데이터의 `added: true` (GDD 원문이 아닌 연결 대사).

## P01 불타는 마을
씬: `Assets/Game/Scenes/P01_BurningVillage.unity` · 출구: 

### (맵 진입) `OnMapEnter` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs:83
호출: `Village2` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:57), `CamSnap` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:237), `Pan` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:231), `LuLimp` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:192), `FadeSprite` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:297), `FinVillage2` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:97)
- `op_v_2`
  - **루**: 다... 끝난 거야...?
- `op_v_4`
  - **루**: 로젤리나....
- `op_v_no`
  - **루**: 안돼...

## P02 무너진 작은 집
씬: `Assets/Game/Scenes/P02_RuinedHouse.unity` · 출구: 

### (맵 진입) `OnMapEnter` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs:83
호출: `House2` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:101), `CamSnap` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:237), `LuLimp` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:192), `FadeSprite` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:297), `Electrify` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:149), `Shake` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:245), `FinHouse2` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:258)
- `op_h_a`
  - **루**: 나... 로젤리나...
- `op_h_b`
  - **루**: 네가 없으면 난 어떡해..?
- `op_h_c`
  - **루**: ...
  - **루**: .......
- `op_h_d`
  - **루**: 이대로,... 이대로 그저 사라지고 싶어.
- `op_h_return`
  - **루**: 내 세계로 돌아갈래...

## P03 컴퓨터 안
씬: `Assets/Game/Scenes/P03_FriendDesktop.unity` · 출구: 

### (맵 진입) `OnMapEnter` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs:83
호출: `DesktopLand` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:267), `PcLog` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:320), `Transfer` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:331), `Shake` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:245), `FinTransfer` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:418), `FinDesktopLand` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:283)
- `op_d_1`
  - **루**: 아코!
- `pc_log_early`
  - **루**: 접속 기록...? 모르는 줄이 잔뜩이야. ...먼저 저 노란 안내서부터 볼까.
- `pc_exit`
  - **루**: 연결 끊기... 이걸 누르면 나갈 수 있나?
- `pc_log`
  - **서술**: 접속 기록 — 방금 전: 외부 접속 1건.  검색 대상: E-D05-47.
  - **루**: E-D05-47... 아까 안내서 첫 장에 있던 번호잖아.
- `op_d_2`
  - **루**: 여기... 분명 내 친구 IP가 맞는데?

### 사건 `pc_enter` — `g => { return Nothing(); }` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `Nothing` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs:55)

### 사건 `pc_guide` — `PcGuide` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `PcGuide` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:308), `Transfer` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:331), `Shake` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:245), `FinTransfer` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:418)

### 사건 `pc_internet` — `PcInternet` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `PcInternet` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:294)
- `pc_internet_denied`
  - **루**: 접속이 안 되네. 막혀 있어!

## C01 실험실
씬: `Assets/Game/Scenes/C01_TankLab.unity` · 출구: 

### (맵 진입) `OnMapEnter` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs:83
호출: `Tank2` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:427), `CamSnap` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:237), `Shake` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:245), `LabDischarge` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:576), `HitStop` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:145), `Arc` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:266), `DropDebris` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs:136), `FadeSprite` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:297), `EscapeStart` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:540), `Pan` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:231), `Discharge` (Assets/Game/Scripts/Runtime/Devices/PowerRules.cs:54), `AfterTankControl` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:621), `ClearHintLater` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:222), `FinTank2` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:596)
- `op_t_1`
  - **루**: 으얏!
- `op_t_2`
  - **루**: 머야, 머야?!
- `op_t_routine`
  - **연구원**: 그래서 말이야...
- `op_t_discovery`
  - **연구원**: 엥?! 그 꼬맹이가 벌써 왔어!
- `op_t_r1`
  - **연구원**: 벌써 깨어났어? 수치 좀 봐.
- `op_t_r3`
  - **연구원**: 쉿... 먼저 고정 장치부터.
- `op_t_calm1`
  - **연구원 1**: 꼬마야, 진정해~ 여긴 무서운 곳 아니야.
- `op_t_no`
  - **루**: 그러지 마!
- `op_t_stop`
  - **루**: 오지 말랬잖아!
- `c1_tank_broken`
  - **서술**: 깨진 실험 통. *(보충)*
- `esc_1`
  - **루**: 여긴... 실험실?
- `esc_2`
  - **루**: 일단... 밖으로 나가자.
- `lab_broadcast`
  - **서술**: 격리 구역 이탈 감지.
  - **서술**: 실험체 E-D05-47.
  - **서술**: 보안 요원은 해당 구역을 봉쇄하고 실험체를 확보하십시오.

### 씬에 직접 놓인 조사·잠김 대사 (씬 파일의 dialogueId 등)
- `c1_console_broken`
  - **서술**: 타 버린 제어 콘솔. 화면이 깨져 있다. *(보충)*

### 사건 `lab_tank_label` — `g => Clue(g, "tank", "lab_clue_tank")` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `Clue` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:120), `ClueBody` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:129), `Pan` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:231), `ClueReward` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:30), `Realize` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:145), `CamSnap` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:237), `Shake` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:245), `FinRealize` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:181)
- `lab_clue_tank`
  - **서술**: 깨진 통 옆의 표식. ‘E-D05-47’. 그 아래 작은 글씨로 ‘회수 대상’. *(보충)*
  - **루**: ...이 번호, 나야? *(보충)*
- `lab_realize`
  - **루**: 에...?
  - **루**: 연구실...?
  - **루**: 나, 연구실에 끌려온 거야...?
- `lab_broadcast`
  - **서술**: 격리 구역 이탈 감지.
  - **서술**: 실험체 E-D05-47.
  - **서술**: 보안 요원은 해당 구역을 봉쇄하고 실험체를 확보하십시오.
- `op_t_escape`
  - **루**: 얼른 도망쳐야 돼!

### 사건 `op_gap` — `DoorGap` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `DoorGap` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs:162), `Clue` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:120), `ClueBody` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:129), `Pan` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:231), `ClueReward` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:30), `Realize` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:145), `CamSnap` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:237), `Shake` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:245), `FinRealize` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:181), `LuMove` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:173)
- `lab_clue_gap`
  - **서술**: 비틀린 문틈 너머로 복도 표지판이 보인다. ‘격리 실험동’. *(보충)*
  - **루**: 격리... 실험동...? *(보충)*
- `lab_realize`
  - **루**: 에...?
  - **루**: 연구실...?
  - **루**: 나, 연구실에 끌려온 거야...?
- `lab_broadcast`
  - **서술**: 격리 구역 이탈 감지.
  - **서술**: 실험체 E-D05-47.
  - **서술**: 보안 요원은 해당 구역을 봉쇄하고 실험체를 확보하십시오.
- `op_t_escape`
  - **루**: 얼른 도망쳐야 돼!

### 사건 `lab_pouch` — `LabPouch` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `LabPouch` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:629)
- `lab_pouch`
  - **루**: 내 겉옷...! 주머니에—
  - **루**: ...초콜릿, 아직 있네.

### 사건 `lab_emergency` — `g => Mono(g, "lab_m_emergency")` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `Mono` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:18)
- `lab_m_emergency`
  - **루**: 이것만 켜져 있네... *(보충)*

### 사건 `lab_jars` — `LabJars` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `LabJars` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:96)
- `lab_m_jars`
  - **루**: 안이 안 보여... 다행인가. *(보충)*

### 사건 `lab_bed` — `LabBed` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `LabBed` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:38), `LuMove` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:173), `ClueReward` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:30)
- `lab_m_bed`
  - **루**: ...저기 가까이 가기 싫어. *(보충)*

### 사건 `lab_tools` — `LabTools` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `LabTools` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:58), `Jiggle` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:72), `ClueReward` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:30)
- `lab_m_tools`
  - **루**: 차가워... *(보충)*

### 사건 `lab_board` — `g => Clue(g, "board", "lab_clue_board")` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `Clue` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:120), `ClueBody` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:129), `Pan` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:231), `ClueReward` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:30), `Realize` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:145), `CamSnap` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:237), `Shake` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:245), `FinRealize` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:181)
- `lab_clue_board`
  - **서술**: 벽에 붙은 커다란 도식. 사람 몸 그림 위에 전선과 번호가 빼곡하다. *(보충)*
  - **루**: ...사람 몸에, 전선...? *(보충)*
- `lab_realize`
  - **루**: 에...?
  - **루**: 연구실...?
  - **루**: 나, 연구실에 끌려온 거야...?
- `lab_broadcast`
  - **서술**: 격리 구역 이탈 감지.
  - **서술**: 실험체 E-D05-47.
  - **서술**: 보안 요원은 해당 구역을 봉쇄하고 실험체를 확보하십시오.
- `op_t_escape`
  - **루**: 얼른 도망쳐야 돼!

### 사건 `lab_bin` — `LabBin` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `LabBin` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:81), `Jiggle` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:72), `ClueReward` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:30)
- `lab_m_bin`
  - **루**: ...방금, 움직였어? *(보충)*

### 사건 `lab_window` — `g => Mono(g, "lab_m_window")` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `Mono` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:18)
- `lab_m_window`
  - **루**: ...저 안에도, 누가 있어. *(보충)*

### 사건 `lab_lockers` — `g => Mono(g, "lab_m_lockers")` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `Mono` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:18)
- `lab_m_lockers`
  - **루**: 번호가... 엄청 많아. *(보충)*

### 사건 `lab_researcher` — `LabResearcher` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `LabResearcher` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:103), `LuMove` (Assets/Game/Scripts/Runtime/Story/Cutscene.cs:173)
- `lab_m_researcher`
  - **루**: ...숨은 쉬는 것 같아. 가까이 안 갈래. *(보충)*

## C02 격리 실험동 복도
씬: `Assets/Game/Scenes/C02_EscapeWing.unity` · 출구: edge→C03/start

### (맵 진입) `OnMapEnter` — Assets/Game/Scripts/Runtime/Story/StoryEvents.cs:83
호출: `CorridorMemory` (Assets/Game/Scripts/Runtime/Story/StoryEventsQwer.cs:564)
- `c2s_enter`
  - **루**: 경보가 울려도... 날 본 건 아니야. 조용히. *(보충)*
- `esc_3`
  - **루**: 내가 또 이런 곳에 잡혀오다니...
- `esc_4`
  - **루**: 젠장... 뭐가 됐든 여길 나가야겠어...

### 사건 `checkpoint` — `g => CheckpointHere(g)` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `CheckpointHere` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:20)

### 사건 `c2s_jam_tut` — `g => MonoOnce(g, "c2s_jam_tut")` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `MonoOnce` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:19)
- `c2s_jam_tut`
  - **루**: 저 제어함... 살짝만 건드리면 카메라가 꺼질지도. *(보충)*

### 사건 `c2s_cover_tut` — `g => MonoOnce(g, "c2s_cover_tut")` (Assets/Game/Scripts/Runtime/Story/StoryEvents.cs)
호출: `MonoOnce` (Assets/Game/Scripts/Runtime/Story/StoryEventsStealth.cs:19)
- `c2s_cover_tut`
  - **루**: 상자 뒤에 웅크리면... 안 보일 거야. *(보충)*

