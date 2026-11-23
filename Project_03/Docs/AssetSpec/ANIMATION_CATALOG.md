# ANIMATION CATALOG — 캐릭터 애니메이션 목록

> 분석일 2026-11-17 · 기준: 현재 프로젝트 파일 (코드·씬·데이터·이미지 메타). **현재 구현 기준**이다. 재기획(Docs/Redesign)으로 바뀔 수 있는 요구는 따로 표시했다.
> 상태: EXISTING 정상 사용 · PLACEHOLDER 임시 · MISSING 필요하지만 없음 · REPLACE 있으나 교체 필요 · OPTIONAL 선택 / 우선순위 P0~P3. **추정**은 따로 표시. 이미지를 만들거나 프로젝트 파일을 바꾸지 않았다.

모든 상태는 이미 클립과 컨트롤러가 있다 (`Assets/Game/Art/Generated/Anim/<id>/`, 253개). 그림만 교체하면 `EW > 1. 전체 구성`이 프레임 수에 맞춰 클립을 다시 만든다 (`forceClips`). **프레임 수는 바꿔도 코드에 영향이 없다** (예외는 표의 메모).

공통: 시트는 **가로 한 줄**, 프레임 사이 여백 0, 행 1, 열 = 프레임 수, 투명 PNG, 피벗 = 아래 가운데 (드론은 가운데), 기본 오른쪽을 봄 → 왼쪽은 코드가 좌우 반전 (방향별 그림 불필요. 좌우 비대칭 장식을 넣으면 왼쪽용 시트 필요).

‘구현’ = 현재 클립 존재 여부. ‘사용’ = 코드·씬에서 상태 이름을 부르는 곳 (정적 검색). 사용 0 은 OPTIONAL.

## CH_PLAYER_001 루 (`lu`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P0 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P0 |  |
| Jump | 점프 상승 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:273 | PLACEHOLDER | P0 |  |
| Fall | 낙하 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:436 | PLACEHOLDER | P0 |  |
| Climb | 사다리 오르기 | 2 | 2 | 6 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Field/Companion.cs:105 | PLACEHOLDER | P0 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P0 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P0 |  |
| Sit | 앉기 (벤치·바닥) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsLate.cs:522 | PLACEHOLDER | P1 |  |
| Flick | 톡! — 검지를 튕겨 짧은 불꽃 (2번째 프레임이 판정 순간) | 3 | 3 | 9 | 1회 | 144×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 | 2번째 프레임 = 판정 순간 |
| Point | 겨누기 (스파크 샷·간섭) | 3 | 3 | 9 | 1회 | 144×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsStealth.cs:62 | PLACEHOLDER | P0 |  |
| Swing | 휘두르기 (현재 근접 연출용) | 3 | 3 | 9 | 1회 | 144×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P1 |  |
| Kick | 발차기 (연출용) | 3 | 3 | 9 | 1회 | 144×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P1 |  |
| Charge | 외부 전력 끌어오기 — 손이 전원 쪽으로 | 2 | 2 | 6 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/VenelleBoss.cs:121 | PLACEHOLDER | P0 |  |
| Discharge | 방전 (몸에서 전기 폭발) | 2 | 2 | 10 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:505 | PLACEHOLDER | P2 |  |
| Carry | 부축하기 (베넬) | 4 | 4 | 5 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:979 | PLACEHOLDER | P2 |  |
| BackIdle | 뒷모습 대기 (불타는 마을 첫 장면) | 2 | 2 | 2 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:63 | PLACEHOLDER | P2 |  |
| BackLook | 뒷모습에서 고개 돌림 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:70 | PLACEHOLDER | P2 |  |
| Limp | 절뚝이는 이동 | 6 | 6~8 | 6 | O | 288×64 | 아래 가운데 | 클립 있음 | Runtime/Story/Cutscene.cs:196 | PLACEHOLDER | P2 |  |
| LimpIdle | 절뚝이다 멈춰 숨 고르기 | 2 | 2 | 2 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/Cutscene.cs:217 | PLACEHOLDER | P2 |  |
| Sad | 고개 숙임 (슬픔) | 2 | 2 | 2 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:82 | PLACEHOLDER | P2 |  |
| ReachPC | 컴퓨터에 손 뻗기 | 2 | 2 | 4 | 1회 | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:136 | PLACEHOLDER | P2 |  |
| Connect | 접속 — 빛이 몸에 번짐, 머리카락 들림 | 3 | 3 | 6 | O | 144×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Land | 착지 충격 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| GetUp | 몸 일으키기 | 3 | 3 | 6 | 1회 | 144×64 | 아래 가운데 | 클립 있음 | Runtime/Action/VenelleBoss.cs:492 | PLACEHOLDER | P2 |  |
| LookAround | 좌우 살피기 | 4 | 4 | 3 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:280 | PLACEHOLDER | P2 |  |
| Knock | 유리 두드리기 | 3 | 3 | 10 | O | 144×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| BackAway | 겁먹고 뒷걸음 | 4 | 4 | 6 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:771 | PLACEHOLDER | P2 |  |
| Fear | 겁먹고 떨기 | 2 | 2 | 8 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:369 | PLACEHOLDER | P2 |  |
| ChargeEyes | 눈 질끈 감고 방전 준비 | 3 | 3 | 3 | 1회 | 144×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:489 | PLACEHOLDER | P2 |  |
| Recover | 방전 후 숨 몰아쉬기 | 2 | 2 | 2 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:530 | PLACEHOLDER | P2 |  |
| Squeeze | 문틈 비집고 통과 | 4 | 4 | 6 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEvents.cs:170 | PLACEHOLDER | P2 |  |
| Crouch | 웅크려 숨기 — 보이는 키 30px(0.95유닛) 이하 | 2 | 2 | 2 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 | 보이는 키 ≤ 30px (감지 높이) |
| CrouchWalk | 웅크려 걷기 — 같은 높이 제한 | 4 | 4 | 6 | O | 192×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 | 보이는 키 ≤ 30px |
| Dodge | 대시 (낮게 미끄러지듯) | 1 | 1~3 | 1 | O | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 | 대시 0.15초 — 1~3프레임 |
| WallLean | 벽 짚고 숨 고르기 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P1 |  |
| TankCrouch | 실험 통 안 웅크림 | 2 | 2 | 2 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 | 통 안 높이 제한 (실험 통 내부) |
| TankStruggle | 통 안에서 몸부림 | 2 | 2 | 7 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 | 통 안 높이 제한 |
| TankCower | 통 안에서 움츠림 | 2 | 2 | 2 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 | 통 안 높이 제한 |
| EyesClosed | 눈 질끈 감음 | 1 | 1 | 2 | O | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Electrify | 전기화 직전 — 윤곽 발광 | 2 | 2 | 12 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Disgust | 입을 가리고 몸을 숙임 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| HeadHold | 머리를 감싸 쥠 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Lifted | 목덜미를 잡혀 들림 — 목덜미 = 프레임 (24,33)px | 4 | 4 | 7 | O | 192×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 | 목덜미 = (24,33)px |
| Thrown | 던져짐 (몸 웅크림, 회전은 코드) | 1 | 1 | 1 | O | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| HoldHand | 동료 손 잡고 전원선으로 | 1 | 1 | 1 | O | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| HairUp | 펄스 뒤 머리가 곤두섬 + 작은 불꽃 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/PlayerSkills.cs:396 | PLACEHOLDER | P2 |  |
| Share | 과자를 반 나눠 내밂 | 1 | 1 | 1 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:313 | PLACEHOLDER | P2 |  |
| HandsUp | 손 들기 (폐기소 첫 만남) | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Wind | 옥상 바람 맞기 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:816 | PLACEHOLDER | P2 |  |
| Cold | 최종전 낮고 차가운 자세 (추정) | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Limit | 한계 방출 — 양손으로 전류를 붙잡음 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Lie | 누워 있음 (중정) | 1 | 1 | 2 | O | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Kneel | 무릎 꿇기 | 1 | 1 | 3 | 1회 | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| HandHold | 손 잡고 있기 (중정) | 1 | 1 | 2 | O | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Release | 남은 전기를 놓음 (마지막) | 1 | 1 | 4 | O | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |

파일: `Assets/Game/Art/Placeholder/Lu_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/lu/lu.controller`

## CH_BOSS_001 베넬 (본명 아이비) (`venelle`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P0 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P0 |  |
| Jump | 점프 상승 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:273 | PLACEHOLDER | P1 |  |
| Fall | 낙하 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:436 | PLACEHOLDER | P1 |  |
| Climb | 사다리 오르기 | 2 | 2 | 6 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Field/Companion.cs:105 | PLACEHOLDER | P1 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P0 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P1 |  |
| Sit | 앉기 (벤치·바닥) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsLate.cs:522 | PLACEHOLDER | P1 |  |
| Attack | 휘두르기·약병 던지기 | 3 | 3 | 9 | 1회 | 144×64 | 아래 가운데 | 클립 있음 | Runtime/Action/Guard.cs:298 | PLACEHOLDER | P0 |  |
| Collapsed | 쓰러짐 (개그 장면) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:971 | PLACEHOLDER | P2 |  |
| Supported | 부축받으며 걷기 | 4 | 4 | 5 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Field/CarriedFollow.cs:20 | PLACEHOLDER | P2 |  |
| Dizzy | 어지러움 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Stalk | 불균형하고 느슨한 걸음, 고개 기울임 | 5 | 5~6 | 5 | O | 240×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 |  |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 2 | 2 | 6 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:139 | PLACEHOLDER | P0 | 예고가 한눈에 (자세 + 초록 빛) |
| Dash | 돌진 | 1 | 1 | 1 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/MeilinBoss.cs:157 | PLACEHOLDER | P0 |  |
| Pant | 숨 고르기 (말 걸기 기회) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 |  |
| Laugh | 혼자 웃음 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Grab | 잡기 — 손 = 프레임 (37,18)px (VenelleBoss.GrabHand 와 일치) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 | 손 = (37,18)px |
| Throw | 집어 던짐 | 2 | 2 | 7 | 1회 | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P1 |  |
| Spray | 산성 분사 준비 | 2 | 2 | 8 | O | 96×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 |  |
| Mix | 재혼합 (거품, 빈틈임이 읽히게) | 3 | 3 | 6 | O | 144×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 |  |
| Toss | 던지기 (약병·상자) | 2 | 2 | 7 | 1회 | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:257 | PLACEHOLDER | P0 |  |
| Downed | 체력 0 — 무릎 꿇고 숨 몰아쉼, 관에서 약품이 샘 | 1 | 1 | 2 | O | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 |  |
| TalkWalk | 투덜거리며 걸어옴 (등장) | 4 | 4 | 6 | O | 192×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Comb | 빗을 꺼냈다 금속 빗을 보고 넣음 | 2 | 2 | 3 | 1회 | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:445 | PLACEHOLDER | P2 |  |
| Wind | 옥상 바람 맞기 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:816 | PLACEHOLDER | P2 |  |
| SitQuiet | 침대에 앉아 말을 잃음 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:774 | PLACEHOLDER | P2 |  |
| Share | 과자를 반 나눠 내밂 | 1 | 1 | 1 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:313 | PLACEHOLDER | P2 |  |

파일: `Assets/Game/Art/Placeholder/Venelle_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/venelle/venelle.controller`

## CH_BOSS_001-P 아이비 (베넬의 과거) (`ivy`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P2 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P2 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P2 |  |
| Sit | 앉기 (벤치·바닥) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsLate.cs:522 | PLACEHOLDER | P2 |  |
| Carry | 부축하기 (베넬) | 4 | 4 | 5 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:979 | PLACEHOLDER | P2 |  |
| Kneel | 무릎 꿇기 | 1 | 1 | 3 | 1회 | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Give | 건네기 | 1 | 1 | 3 | 1회 | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Wave | 손 흔들기 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | 0 | OPTIONAL | P3 |  |

파일: `Assets/Game/Art/Placeholder/Ivy_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/ivy/ivy.controller`

## CH_ALLY_001 라비엘 (`raviel`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P1 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P1 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P1 |  |
| Sit | 앉기 (벤치·바닥) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsLate.cs:522 | PLACEHOLDER | P1 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P1 |  |
| Talk | 말하기 손짓 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/MeilinBoss.cs:127 | PLACEHOLDER | P1 |  |
| Point | 겨누기 (스파크 샷·간섭) | 1 | 1 | 9 | 1회 | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsStealth.cs:62 | PLACEHOLDER | P1 |  |
| Type | 단말 입력 | 2 | 2 | 6 | O | 96×64 | 아래 가운데 | 클립 있음 | 0 | OPTIONAL | P3 |  |
| HoldHand | 동료 손 잡고 전원선으로 | 1 | 1 | 1 | O | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Catch | 붙잡기 (휴지통 탈출 뒤) | 1 | 1 | 6 | 1회 | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Injured | 부상 상태로 서 있기·따라오기 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | scene:M44_TransferChamber:raviel_inj | PLACEHOLDER | P2 |  |
| Cup | 컵을 양손으로 잡기 | 1 | 1 | 2 | O | 48×64 | 아래 가운데 | 클립 있음 | 0 | OPTIONAL | P3 |  |
| Sad | 고개 숙임 (슬픔) | 1 | 1 | 2 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:82 | PLACEHOLDER | P2 |  |

파일: `Assets/Game/Art/Placeholder/Raviel_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/raviel/raviel.controller`

## CH_ALLY_001-B 라비엘 (괴물 토끼) (`ravielBeast`) — 캔버스 80×96

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 160×96 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P0 |  |
| Walk | 걷기 | 4 | 4 | 7 | O | 320×96 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P0 |  |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 2 | 2 | 5 | O | 160×96 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:139 | PLACEHOLDER | P0 |  |
| Leap | 도약 | 1 | 1 | 3 | 1회 | 80×96 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:95 | PLACEHOLDER | P0 |  |
| Slam | 내려찍기 | 1 | 1 | 3 | 1회 | 80×96 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:107 | PLACEHOLDER | P0 |  |
| Swipe | 앞발 휘두르기 | 1 | 1 | 3 | 1회 | 80×96 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:118 | PLACEHOLDER | P0 |  |
| Stun | 빈틈 (경직) | 2 | 2 | 5 | O | 160×96 | 아래 가운데 | 클립 있음 | Runtime/Action/MeilinBoss.cs:86 | PLACEHOLDER | P0 |  |
| Hurt | 피격 경직 | 1 | 1 | 3 | 1회 | 80×96 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P0 |  |
| Hold | 들고 있기 | 1 | 1 | 3 | O | 80×96 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:299 | PLACEHOLDER | P2 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 3 | O | 80×96 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P1 |  |

파일: `Assets/Game/Art/Placeholder/RavielBeast_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/ravielBeast/ravielBeast.controller`

## CH_BOSS_002 메이린 (`meilin`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P0 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P1 |  |
| Roll | 롤러 이동 | 2 | 2 | 6 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/MeilinBoss.cs:114 | PLACEHOLDER | P0 |  |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 1 | 1 | 6 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:139 | PLACEHOLDER | P0 |  |
| Snip | 가위 공격 | 1 | 1 | 6 | 1회 | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/MeilinBoss.cs:194 | PLACEHOLDER | P0 |  |
| Dash | 돌진 | 1 | 1 | 1 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/MeilinBoss.cs:157 | PLACEHOLDER | P0 |  |
| Stun | 빈틈 (경직) | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/MeilinBoss.cs:86 | PLACEHOLDER | P0 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P0 |  |
| Talk | 말하기 손짓 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/MeilinBoss.cs:127 | PLACEHOLDER | P1 |  |
| Angry | 화냄 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | 0 | OPTIONAL | P3 |  |
| Pull | 캡슐 손잡이 당기기 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | scene:M43_CapsuleCorridor:meilin | PLACEHOLDER | P2 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P1 |  |

파일: `Assets/Game/Art/Placeholder/Meilin_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/meilin/meilin.controller`

## CH_BOSS_003 라크리모사 (본명 에드워드) (`lacrimosa`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P0 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P1 |  |
| Sing | 노래 (울림파 발동) | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:125 | PLACEHOLDER | P0 |  |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 1 | 1 | 6 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:139 | PLACEHOLDER | P0 |  |
| Stagger | 보호 정지 휘청임 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:153 | PLACEHOLDER | P0 |  |
| Listen | 녹음을 듣고 멈춤 | 1 | 1 | 2 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:157 | PLACEHOLDER | P0 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P0 |  |
| Bow | 인사 | 1 | 1 | 2 | O | 48×64 | 아래 가운데 | 클립 있음 | 0 | OPTIONAL | P3 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P1 |  |

파일: `Assets/Game/Art/Placeholder/Lacrimosa_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/lacrimosa/lacrimosa.controller`

## CH_BOSS_003-P 에드워드 (라크리모사의 과거) (`edward`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P2 |  |
| Write | 녹음 파일 이름 적기 | 2 | 2 | 2 | O | 96×64 | 아래 가운데 | 클립 있음 | scene:M37_ResonanceHall:edward | PLACEHOLDER | P2 |  |

파일: `Assets/Game/Art/Placeholder/Edward_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/edward/edward.controller`

## CH_BOSS_003-K 어린 에드워드 (`edwardKid`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P2 |  |
| Hide | 숨기 | 1 | 1 | 3 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:138 | PLACEHOLDER | P2 |  |
| Listen | 녹음을 듣고 멈춤 | 2 | 2 | 2 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:157 | PLACEHOLDER | P2 |  |

파일: `Assets/Game/Art/Placeholder/EdwardKid_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/edwardKid/edwardKid.controller`

## CH_BOSS_004 아우렐리우스 (`aurelius`) — 캔버스 64×96

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 128×96 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P0 |  |
| Talk | 말하기 손짓 | 2 | 2 | 3 | O | 128×96 | 아래 가운데 | 클립 있음 | Runtime/Action/MeilinBoss.cs:127 | PLACEHOLDER | P1 |  |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 2 | 2 | 6 | O | 128×96 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:139 | PLACEHOLDER | P0 |  |
| Cast | 빛·광선 발사 | 1 | 1 | 3 | 1회 | 64×96 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:142 | PLACEHOLDER | P0 |  |
| Hurt | 피격 경직 | 1 | 1 | 3 | 1회 | 64×96 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P0 |  |
| Broken | 외피 일부 파손 | 2 | 2 | 3 | O | 128×96 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Ruined | 외피 크게 파손, 내부 기계 노출 | 2 | 2 | 3 | O | 128×96 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 3 | 1회 | 64×96 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P1 |  |

파일: `Assets/Game/Art/Placeholder/Aurelius_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/aurelius/aurelius.controller`

## CH_ENEMY_001 순찰 경비 (`droid03`) — 캔버스 Foozle 원본 (정사각 프레임 = 시트 높이)

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 4 | 4 | 8 | O | 192×48 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | REPLACE | P0 |  |
| Move | 이동 | 8 | 6 | 10 | O | 384×48 | 아래 가운데 | 클립 있음 | Runtime/Action/Guard.cs:423 | REPLACE | P0 |  |
| Attack | 제압봉 공격 | 9 | 6 (예고 포함) | 14 | 1회 | 432×48 | 아래 가운데 | 클립 있음 | Runtime/Action/Guard.cs:298 | REPLACE | P0 |  |
| Hurt | 피격 경직 | 4 | 2 | 10 | 1회 | 192×48 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | REPLACE | P0 |  |
| Death | 파괴 | 11 | 6 | 12 | 1회 | 528×48 | 아래 가운데 | 클립 있음 | Runtime/Action/Guard.cs:394 | REPLACE | P0 |  |

파일: `Assets/ThirdParty/Foozle_SciFiLab_Droids/… (교체 시 새 이름 권장 — MAPPING 참고)` · 컨트롤러 `Assets/Game/Art/Generated/Anim/droid03/droid03.controller`

## CH_ENEMY_002 포획 드론 (`droid01`) — 캔버스 Foozle 원본 (정사각 프레임 = 시트 높이)

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 4 | 4 | 8 | O | 128×32 | 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | REPLACE | P0 |  |
| Move | 이동 | 4 | 4 | 10 | O | 128×32 | 가운데 | 클립 있음 | Runtime/Action/Guard.cs:423 | REPLACE | P0 |  |
| Attack | 그물·사격 | 12 | 12 | 14 | 1회 | 576×48 | 가운데 | 클립 있음 | Runtime/Action/Guard.cs:298 | REPLACE | P0 |  |
| Hurt | 피격 경직 | 3 | 3 | 10 | 1회 | 96×32 | 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | REPLACE | P0 |  |
| Death | 파괴 | 9 | 9 | 12 | 1회 | 288×32 | 가운데 | 클립 있음 | Runtime/Action/Guard.cs:394 | REPLACE | P0 |  |

파일: `Assets/ThirdParty/Foozle_SciFiLab_Droids/… (교체 시 새 이름 권장 — MAPPING 참고)` · 컨트롤러 `Assets/Game/Art/Generated/Anim/droid01/droid01.controller`

## CH_ENEMY_003 운반 로봇 (`droid02`) — 캔버스 Foozle 원본 (정사각 프레임 = 시트 높이)

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 4 | 4 | 8 | O | 192×48 | 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | REPLACE | P0 |  |
| Move | 이동 | 4 | 4 | 10 | O | 192×48 | 가운데 | 클립 있음 | Runtime/Action/Guard.cs:423 | REPLACE | P0 |  |
| Attack | 짐 던지기·밀치기 (추정) | 7 | 7 | 14 | 1회 | 336×48 | 가운데 | 클립 있음 | Runtime/Action/Guard.cs:298 | REPLACE | P0 |  |
| Hurt | 피격 경직 | 4 | 4 | 10 | 1회 | 192×48 | 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | REPLACE | P0 |  |
| Death | 파괴 | 13 | 13 | 12 | 1회 | 624×48 | 가운데 | 클립 있음 | Runtime/Action/Guard.cs:394 | REPLACE | P0 |  |

파일: `Assets/ThirdParty/Foozle_SciFiLab_Droids/… (교체 시 새 이름 권장 — MAPPING 참고)` · 컨트롤러 `Assets/Game/Art/Generated/Anim/droid02/droid02.controller`

## CH_NPC_001 연구원 1 (`researcher`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P1 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P1 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P1 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P1 |  |
| Scared | 겁먹음 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | scene:M39_ServerRoom:feed_sv2 | PLACEHOLDER | P1 |  |
| Talk | 말하기 손짓 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/MeilinBoss.cs:127 | PLACEHOLDER | P1 |  |
| BatonOut | 제압봉 꺼내기 | 3 | 3 | 6 | 1회 | 144×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:478 | PLACEHOLDER | P1 |  |
| BatonIdle | 제압봉 든 대기 (끝이 지지직) | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:483 | PLACEHOLDER | P1 |  |
| BatonWalk | 제압봉 든 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P1 |  |

파일: `Assets/Game/Art/Placeholder/Researcher_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/researcher/researcher.controller`

## CH_NPC_002 연구원 2 (`researcher2`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P1 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P1 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P1 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P1 |  |
| Scared | 겁먹음 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | scene:M39_ServerRoom:feed_sv2 | PLACEHOLDER | P1 |  |
| Talk | 말하기 손짓 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/MeilinBoss.cs:127 | PLACEHOLDER | P1 |  |
| BatonOut | 제압봉 꺼내기 | 3 | 3 | 6 | 1회 | 144×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:478 | PLACEHOLDER | P1 |  |
| BatonIdle | 제압봉 든 대기 (끝이 지지직) | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsQwer.cs:483 | PLACEHOLDER | P1 |  |
| BatonWalk | 제압봉 든 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P1 |  |

파일: `Assets/Game/Art/Placeholder/Researcher2_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/researcher2/researcher2.controller`

## CH_NPC_003 F (`f`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P1 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P1 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P1 |  |
| Sit | 앉기 (벤치·바닥) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsLate.cs:522 | PLACEHOLDER | P1 |  |
| Talk | 말하기 손짓 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/MeilinBoss.cs:127 | PLACEHOLDER | P1 |  |
| Point | 겨누기 (스파크 샷·간섭) | 1 | 1 | 9 | 1회 | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsStealth.cs:62 | PLACEHOLDER | P1 |  |
| Toss | 던지기 (약병·상자) | 2 | 2 | 7 | 1회 | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:257 | PLACEHOLDER | P1 |  |
| Guard | 막아서기·방어 자세 | 1 | 1 | 3 | O | 48×64 | 아래 가운데 | 클립 있음 | scene:M30_VideoRoom:warden1 | PLACEHOLDER | P1 |  |
| Tag | 번호표 들여다보기 | 1 | 1 | 1 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:254 | PLACEHOLDER | P2 |  |

파일: `Assets/Game/Art/Placeholder/F_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/f/f.controller`

## CH_NPC_004 008 (`kid008`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P1 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P1 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P1 |  |
| Sit | 앉기 (벤치·바닥) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsLate.cs:522 | PLACEHOLDER | P1 |  |
| Talk | 말하기 손짓 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/MeilinBoss.cs:127 | PLACEHOLDER | P1 |  |
| Hide | 숨기 | 1 | 1 | 3 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:138 | PLACEHOLDER | P2 |  |
| Eat | 먹기 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:321 | PLACEHOLDER | P2 |  |
| Hold | 들고 있기 | 1 | 1 | 1 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:299 | PLACEHOLDER | P2 |  |
| Draw | 쪼그려 바닥에 선 긋기 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:399 | PLACEHOLDER | P2 |  |

파일: `Assets/Game/Art/Placeholder/Kid008_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/kid008/kid008.controller`

## CH_NPC_005 생존자 A (`survivorA`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P0 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P1 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P1 |  |
| Sit | 앉기 (벤치·바닥) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsLate.cs:522 | PLACEHOLDER | P1 |  |
| Scared | 겁먹음 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | scene:M39_ServerRoom:feed_sv2 | PLACEHOLDER | P0 |  |
| Guard | 막아서기·방어 자세 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | scene:M30_VideoRoom:warden1 | PLACEHOLDER | P0 |  |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 1 | 1 | 6 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:139 | PLACEHOLDER | P0 |  |
| Strike | 내려치기 | 1 | 1 | 1 | 1회 | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 |  |
| Toss | 던지기 (약병·상자) | 2 | 2 | 7 | 1회 | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:257 | PLACEHOLDER | P0 |  |
| Calm | 파이프를 놓고 주저앉음 (진정) | 1 | 1 | 3 | O | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P1 |  |

파일: `Assets/Game/Art/Placeholder/SurvivorA_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/survivorA/survivorA.controller`

## CH_NPC_006 생존자 B (`survivorB`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P0 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P1 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P1 |  |
| Sit | 앉기 (벤치·바닥) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsLate.cs:522 | PLACEHOLDER | P1 |  |
| Scared | 겁먹음 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | scene:M39_ServerRoom:feed_sv2 | PLACEHOLDER | P0 |  |
| Guard | 막아서기·방어 자세 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | scene:M30_VideoRoom:warden1 | PLACEHOLDER | P0 |  |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 1 | 1 | 6 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:139 | PLACEHOLDER | P0 |  |
| Strike | 내려치기 | 1 | 1 | 1 | 1회 | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 |  |
| Toss | 던지기 (약병·상자) | 2 | 2 | 7 | 1회 | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:257 | PLACEHOLDER | P0 |  |
| Calm | 파이프를 놓고 주저앉음 (진정) | 1 | 1 | 3 | O | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P0 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P1 |  |

파일: `Assets/Game/Art/Placeholder/SurvivorB_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/survivorB/survivorB.controller`

## CH_NPC_007 베스페르 단원 A (`memberA`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P1 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P1 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P1 |  |
| Sit | 앉기 (벤치·바닥) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsLate.cs:522 | PLACEHOLDER | P1 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P1 |  |
| Wait | 줄 서서 기다림 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | scene:M36_WaitingRoom:c9_member1 | PLACEHOLDER | P2 |  |
| Scared | 겁먹음 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | scene:M39_ServerRoom:feed_sv2 | PLACEHOLDER | P1 |  |
| Cheer | 환호 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | scene:M42_BoardingLounge:c10_member1 | PLACEHOLDER | P2 |  |

파일: `Assets/Game/Art/Placeholder/MemberA_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/memberA/memberA.controller`

## CH_NPC_008 베스페르 단원 B (`memberB`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P1 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P1 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P1 |  |
| Sit | 앉기 (벤치·바닥) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsLate.cs:522 | PLACEHOLDER | P1 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P1 |  |
| Wait | 줄 서서 기다림 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | scene:M36_WaitingRoom:c9_member1 | PLACEHOLDER | P2 |  |
| Scared | 겁먹음 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | scene:M39_ServerRoom:feed_sv2 | PLACEHOLDER | P1 |  |
| Cheer | 환호 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | scene:M42_BoardingLounge:c10_member1 | PLACEHOLDER | P2 |  |

파일: `Assets/Game/Art/Placeholder/MemberB_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/memberB/memberB.controller`

## CH_NPC_009 회수 경비 (`warden`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P1 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P1 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P1 |  |
| Sit | 앉기 (벤치·바닥) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsLate.cs:522 | PLACEHOLDER | P1 |  |
| Down | 쓰러져 누운 자세 (유지) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/LacrimosaBoss.cs:242 | PLACEHOLDER | P1 |  |
| Guard | 막아서기·방어 자세 | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | scene:M30_VideoRoom:warden1 | PLACEHOLDER | P1 |  |
| Strike | 내려치기 | 1 | 1 | 1 | 1회 | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P1 |  |

파일: `Assets/Game/Art/Placeholder/Warden_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/warden/warden.controller`

## CH_NPC_010 린든 (`linden`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 2 | 3 | O | 96×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P2 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P2 |  |
| Hurt | 피격 경직 | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/AureliusBoss.cs:179 | PLACEHOLDER | P2 |  |
| Sit | 앉기 (벤치·바닥) | 1 | 1 | 8 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsLate.cs:522 | PLACEHOLDER | P2 |  |
| Hold | 들고 있기 | 1 | 1 | 1 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Story/StoryEventsUnderground.cs:299 | PLACEHOLDER | P2 |  |
| Wave | 손 흔들기 | 2 | 2 | 4 | O | 96×64 | 아래 가운데 | 클립 있음 | 0 | OPTIONAL | P3 |  |

파일: `Assets/Game/Art/Placeholder/Linden_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/linden/linden.controller`

## CH_NPC_011 어른 (회상) (`adult`) — 캔버스 48×64

| 상태 | 설명 | 현재 프레임 | 권장 프레임 | FPS | 루프 | 시트 크기 (현재) | 피벗 | 구현 | 상태 이름 호출 위치 (예, 인물 구분 없음) | 상태 | 우선 | 메모 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 1 | 1 | 3 | O | 48×64 | 아래 가운데 | 클립 있음 | Runtime/Action/BossBase.cs:107 | PLACEHOLDER | P2 |  |
| Walk | 걷기 | 4 | 4 | 8 | O | 192×64 | 아래 가운데 | 클립 있음 | Runtime/Action/RavielBeast.cs:81 | PLACEHOLDER | P2 |  |
| Crutch | 목발 짚고 걷기 | 1 | 1 | 1 | O | 48×64 | 아래 가운데 | 클립 있음 | 문자열만 | PLACEHOLDER | P2 |  |

파일: `Assets/Game/Art/Placeholder/Adult_<상태>.png` · 컨트롤러 `Assets/Game/Art/Generated/Anim/adult/adult.controller`

## 향후 추가가 필요한 애니메이션 (현재 구현에 없음)

| 대상 | 애니메이션 | 이유 | 구분 |
|---|---|---|---|
| 순찰 경비 (CH_ENEMY_001) | Windup (제압봉 치켜듦) | 공격 예고가 깜빡임뿐 (ART_REQUESTS) | 현재 구현 보완 (코드에 상태 추가 필요) |
| 순찰 경비 | Guard / GuardBreak | 방어 자세·무너짐 구분 (현재 코드가 기울이고 떨게 함) | 현재 구현 보완 |
| 베넬 | Kneel (무릎 꿇음 전용) | 지금 Collapsed·Pant 로 대신 | 선택 |
| 무인 회수 장치 (M47) | Idle / Move | 지금 운반 로봇 그림 대용 | 현재 구현 보완 (새 artId 필요 — 코드 변경) |
| 루 | 최종전 낮은 목소리 표정 (스탠딩) | 지금 determined 대용 | 스탠딩 |
| (재기획) 동료 고유 행동 | 베넬 약품 유인·바닥 적시기 / 라비엘 단말 조작 등 | Docs/Redesign DECISION_LOG D-001 [제안] — **미확정. 지금 제작하지 않는다** | 재기획 의존 |

총 정의 상태 253개 (클립 253 = 모두 존재).
