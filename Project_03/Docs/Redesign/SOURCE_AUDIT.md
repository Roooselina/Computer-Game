# SOURCE AUDIT — 자료 조사 기록 (2026-11-16)

`GAME_MASTER_DESIGN.docx` v0.1 의 근거 자료와 확인 방법이다.
이번 작업에서는 게임 코드, 씬, 대사, 세이브를 하나도 수정하지 않았다. 추출 스크립트와 중간 파일은 이 프로젝트 밖의 작업용 임시 폴더에서만 썼다.

**근거 표시**
- **[구현 확인]**: 코드나 데이터를 읽어 확인했다. 실제로 그렇게 동작하는지와는 다르다.
- **[봇]**: 자동 진행 봇이 지나갔다.
- **[테스트]**: 자동 테스트가 확인했다.
- **[미확인]**: 확인하지 못했다.

사람이 직접 플레이한 기록은 없다.

## 1. 확인한 파일

| 종류 | 파일 | 용도 |
|---|---|---|
| 기획 | `GDD.md` (1166줄) | 기존 설정, 확정 사항(0장), 개정 기록(12장) |
| 기획 | `Docs/GDD_original_2026-11-14.md` | 존재만 확인. 본문 비교는 하지 않음 |
| 스토리 리뷰 | `Docs/StoryReview/` 전체 (CURRENT_STORY, story/*.md 7개, CHARACTER_TIMELINE, GDD_DIFFERENCES, REVIEW_START_HERE, appendix 9개) | 이전 단계 조사 결과. 이번 작업에서 다시 대조 |
| 구현 상태 | `Docs/IMPLEMENTATION_STATUS.md` (+ v1~v6 백업은 목록만) | 구현 범위와 검증 결과 |
| 사양 | `Docs/STEALTH_COMBAT_SPEC.md`, `Docs/INPUT_GUIDE.md`, `Docs/CUTSCENE_FLOW.md`, `Docs/VENELLE_BRANCHES.md`, `Docs/BUILDING_LAYOUT_GUIDE.md` | 수치와 조작. 코드와 대조 |
| 코드 (런타임) | `Assets/Game/Scripts/Runtime/**/*.cs` 전체 | 사건 표, 함수 본문 추출 (정적) |
| 코드 (세부 확인) | `Core/GameInput.cs`, `Core/GameState.cs`, `Action/PlayerSkills.cs`, `Action/BossBase.cs`, `Action/VenelleBoss.cs`, `Action/MeilinBoss.cs`, `Action/RavielBeast.cs`, `Action/LacrimosaBoss.cs`, `Action/AureliusBoss.cs`, `Save/SaveData.cs`, `Data/DefaultContent.cs`, `Story/StoryEvents*.cs` | 키, 성장, 기술, 보스, 저장, 아이템 |
| 코드 (에디터) | `Assets/Game/Scripts/Editor/EWRooms.cs`, `EWRoomsUnderground.cs`, `EWRoomsLate.cs` | 맵별 배치 (정규식 집계) |
| 씬 | `Assets/Game/Scenes/*.unity` 45개 | 조사·잠김 대사 ID (dialogueId 등) 추출 |
| 맵 데이터 | `Docs/Generated/built_layout_report.json` | 실제 생성 씬의 스폰, 출구, 사건 |
| 맵 데이터 | `Assets/Game/Resources/EW/world_layout.json` | 맵 이름, 구역, 목표, storyLinks, chapterSafe, 검수 프리셋 |
| 튜닝 | `Assets/Game/Resources/EW/action_tuning.json` | 플레이어, 기술 7종, 적 3종, 베넬, 난이도, 경험치 |
| 대사 | `Assets/Game/Data/Dialogue/dialogue_ko.json` | 대사 513, 말풍선, 문서, 화자 |
| 캐릭터 그림 | `Assets/Game/Data/Art/*.asset` | 목록만 확인 (22종) |
| 테스트 | `Assets/Game/Tests/EditMode/*.cs` 4개, `Assets/Game/Tests/PlayMode/*.cs` 4개 | 테스트 범위 (결과 수치는 IMPLEMENTATION_STATUS 기록) |
| 봇 기록 | `Docs/Media/ch7_11_2026-11-16/run_newgame_to_end_log.txt`, `cutscene_timings.txt`, `exe_smoke_log.txt` | 구간 시간, 컷신 시간 |
| 화면 | `Docs/Media/ch7_11_2026-11-16/` (봇 프레임 24, 빌드 캡처 12), `ch4_6_2026-11-15/` 23, `art_play_overhaul/` (opening_final, tank_final, venelle_final, capture_final, after), `qwer_2026-11-15/` 16, `realtime/` 13 | DOCX 에 넣은 실제 화면 |

## 2. 확인하지 못한 것

| 항목 | 이유 |
|---|---|
| 사람 플레이 감각: 손맛, 템포, 난이도, 가독성 | 사람 플레이 자료가 없다 |
| 이번 단계에서 게임을 직접 실행한 확인 | 하지 않았다. 화면은 기존 기록만 썼다 |
| 엔딩 뒤 이어하기 동작 (D5) | 실행하지 않았다 |
| 검수 도구로 지난 맵에 갔을 때 사망 인물이 보이는지 (B4, E4) | 실행하지 않았다 |
| 동행 말풍선 `b_v_corridor` 의 정확한 출력 시점 (A6) | 봇 저장에 출력 기록만 있다 |
| 컷신의 컷 단위 시간, 사운드 큐 | 데이터가 없다. 전체 시간만 봇이 측정했다 |
| 적 드롭(`DefaultContent` dropItem)이 실시간에서 쓰이는지 | 추적하지 않았다 |
| 동료가 피해를 받는지 | 코드상 피격 처리가 없어 보이지만 실행 확인은 없다 |
| `Docs/GDD_original_2026-11-14.md` 와의 본문 차이 | 비교 범위 밖이다 |
| `Library/`, `Builds/`, `Backups/` 의 내용 | 무관하거나 생성물이라 열지 않았다 |
| 사용자 세이브와 인증 정보 | 열지 않았다 (원칙) |

## 3. 프로젝트 자료 구조 (요약)

```
GDD.md                         기획서 (수정 가능한 기존 설정)
Docs/                          구현 상태·사양·가이드·스토리 리뷰·미디어
  StoryReview/                 이전 단계 스토리 조사
  Generated/built_layout_report.json
  Media/                       실제 실행 캡처, 봇 기록
  Redesign/                    ← 이번 산출물
Assets/Game/
  Scripts/Runtime/  Core · Action · Devices · Field · Story · UI · Save · Data
  Scripts/Editor/   EWRooms*.cs (방 정의 → EWSceneBuilder 가 씬 생성)
  Scenes/           P01~P03, C01~C06, M09~M47 (45)
  Data/Dialogue/dialogue_ko.json · Data/Art/*.asset
  Resources/EW/     action_tuning.json · world_layout.json · GameConfig.asset
  Tests/            EditMode · PlayMode
```

## 4. 실제 코드에서 확인한 기능

| 기능 | 위치 | 확인 |
|---|---|---|
| 실시간 이동·대시(AA/DD)·사다리·웅크리기 | `Field/PlayerController`, `Action/DoubleTapDash.cs` | [구현 확인][테스트] |
| 기술 7종 (톡!, 간섭, 외부 전력 방전, 전원선, 스파크 샷, 펄스, 전하 모으기) | `Action/PlayerSkills.cs`, `action_tuning.json` | [구현 확인] |
| 경비, 드론, 운반 로봇, CCTV, 발각 게이지, 지원 드론 | `Action/Guard.cs`, `Cctv.cs`, `Reinforcements.cs` | [구현 확인][테스트] |
| 장치: 전원선, 배터리, 컨베이어, 제어함, 펌프, 자판기 | `Devices/*.cs` | [구현 확인][봇] |
| 보스 5종 + 공용 예고·빈틈 | `Action/*Boss*.cs`, `RavielBeast.cs`, `BossBase.cs` | [구현 확인][테스트][봇] |
| 사건과 컷신 | `Story/StoryEvents*.cs` (사건 117개 중 맵 연결 105개) | [구현 확인: 정적] |
| 저장: 분기 필드와 체크포인트 재시도 | `Save/SaveData.cs`, `Core/GameState.cs` | [구현 확인][테스트] |
| 성장: 경험치(단서, 구간, 조용히, 협상), 레벨에 따른 최대 안정도·전하 | `GameState.Reward/GainExp/MaxHp/MaxCharge` | [구현 확인] |
| 휴지통 복원 | `UI/RecycleBinUI.cs` | [구현 확인][테스트] |
| 시설 카메라 영상 | `UI/FeedView.cs` | [구현 확인][테스트] |
| 엔딩 크레딧과 가상 바탕화면 | `UI/EndingScreens.cs` | [구현 확인][봇][테스트] |
| Esc 검수 도구 (별도 저장 폴더) | `UI/ReviewMenu.cs`, `Core/ReviewMode.cs` | [구현 확인][테스트] |

## 5. GDD 와 구현의 불일치 (요약)

전체 표는 DOCX PART 01 의 ‘리뷰 항목 전체’에 있다 (A1~F, `Docs/StoryReview/GDD_DIFFERENCES.md` 와 같은 번호). 이번 단계에서 새로 확인한 것은 다음과 같다.

| 항목 | 기존 GDD | 실제 구현 | 차이 | 상태 |
|---|---|---|---|---|
| 장르 표기 (0장) | 일반 턴제 전투 | 실시간 잠입 액션 (11-14 사용자 지시) | 0장 표가 갱신되지 않음 | [구현 오류: 문서] |
| 조작 (4.1) | Space 점프 / J 조사 / Q·W·E·R / Shift 회피 | Space 점프·조사 / Q·E·R·F / AA·DD | 4.1 이 갱신되지 않음 (INPUT_GUIDE 는 코드와 일치) | [구현 오류: 문서] |
| 기술 (4.2·4.5) | 10종, 레벨 해금 | 실시간 7종, 레벨 해금 없음 (펄스는 사건으로 습득) | 아크 채찍, 번개 발차기, 연쇄 전류, 스파크 커튼, 과부하 없음 | [검토 필요] |
| 성장 (4.5) | 안정도, 출력, 방어 | 레벨은 최대 안정도와 전하 칸만 올림 | Atk·Def 성장값은 사용처 없음 | [구현 확인] |
| 동료 (4.6) | 베넬 확률 행동, 라비엘 명령 | 따라오기, 말풍선. 최종전 엄호막만 | 동료 행동 없음 | [검토 필요] |
| 일반 적 (5장) | 8종 | 3종 (경비, 드론, 운반 로봇) | 차폐 경비, 정비 거미, 약품 누출체, 공명 단원, 회수 경비(전투) 없음 | [검토 필요] |
| 과자 (4.7) | 고정 36개, 장별 배치 | 맵별 배치 수 [미확인] (9장은 1개, 정적) | 전체 집계 안 함 | [미확인] |
| 엄호막 주기 | 7초 (STEALTH_COMBAT_SPEC) | 코드 7초 (`AureliusBoss` supportT) | 클래스 주석에는 ‘9초’ | [구현 오류: 주석] |
| 조작 주석 | — | `PlayerSkills.cs` 주석은 Q/W/E/R, `VenelleBoss.cs` 주석은 Shift·J | 코드 주석이 낡음 | [구현 오류: 주석] |
| 적 정의 | — | `DefaultContent` 의 적 hp 26·14·28(턴제 값), 실시간은 `action_tuning` hp 3·2·4 | 두 데이터가 공존 | [검토 필요] |

## 6. 데이터 연결 오류 (후보)

- 씬에 연결되지 않은 사건 12개: `pc_log`, `pc_exit`, `pc_photo`, `pc_status`, `pc_trash`, `demo_end`, `c3_rest`, `c1_rest_room_enter`, `c1_shortcut`, `c2_glass`, `c2s_pull_tut`, `c2s_tray_tut`. 앞의 C 계열 일부는 옛 맵 통합 때 남은 것으로 보이지만 확인하지 않았다 [미확인]. 7장 임시 대사 `ch7_wip` 는 미참조 대사 목록에 있다.
- 어디서도 참조되지 않는 대사 53개. GDD 원문 `c3_rest`, `op_h_1~3`, `op_t_3~5` 등을 포함한다. 목록은 §7.2.
- `world_layout.json` 의 P03 설명(‘접속 기록 조사 후 연출’)과 storyLinks(‘pc_exit 사건’)가 현재 코드(안내서를 닫으면 전송)와 다르다.
- `ending_seen` 플래그가 저장되지 않는다 (D5). 실제 문제가 생기는지는 [미확인].
- 검수 프리셋 값이 실제 진행과 다르다 (E1~E3).

## 7. 검증 근거와 방법

| 무엇 | 방법 | 한계 |
|---|---|---|
| 맵·사건·대사 연결 | 씬 보고서의 사건 ID → 코드 사건 표 → 함수 호출을 따라가며 대사 ID 수집 (재귀 6단계, 흔한 이름 제외) | 분기 조건은 추출하지 않음. 한 사건의 분기 대사가 함께 나온다. 코드에서 문자열을 조합해 만든 ID 는 놓칠 수 있다 |
| 맵별 배치 수 | `EWRooms*.cs` 방 정의 본문 정규식 집계 | 근사값. 도우미 함수 이름이 다르면 빠질 수 있다 |
| 수치 | `action_tuning.json`, 코드 상수 직접 읽기 | 런타임 덮어쓰기는 추적하지 않음 |
| 컷신 시간 | 봇 측정 로그 | 대사 시간은 봇의 넘김 속도 기준 |
| 테스트 결과 | `Docs/IMPLEMENTATION_STATUS.md` §4 의 기록 (EditMode 47/47, PlayMode 54/54) | 이번 단계에서 다시 실행하지 않음 |
| 화면 | 기존 캡처 파일 (날짜를 그림 아래에 표시) | 2026-11-15 이전 캡처는 현재와 다를 수 있다 |

## 8. 산출물 검수 (이번 작업)

| 확인 | 결과 |
|---|---|
| DOCX 가 열리는지 | Microsoft Word (COM)로 열고 목차와 필드를 갱신한 뒤 저장. PDF 로 내보내 확인 |
| 목차와 제목 위계 | Word 자동 목차 (제목 1~2) |
| 표 잘림·한글 | PDF 렌더 페이지를 이미지로 표본 점검 (최종 보고 참고) |
| 페이지 번호 | 아래쪽 ‘현재 / 전체’ |
| 맵·사건 ID 일치 | DOCX 와 INDEX 는 같은 추출 데이터(`data.json`)에서 생성 |
