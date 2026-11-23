# CHARACTER ASSET CATALOG — 캐릭터 에셋 목록

> 분석일 2026-11-17 · 기준: 현재 프로젝트 파일 (코드·씬·데이터·이미지 메타). **현재 구현 기준**이다. 재기획(Docs/Redesign)으로 바뀔 수 있는 요구는 따로 표시했다.
> 상태: EXISTING 정상 사용 · PLACEHOLDER 임시 · MISSING 필요하지만 없음 · REPLACE 있으나 교체 필요 · OPTIONAL 선택 / 우선순위 P0~P3. **추정**은 따로 표시. 이미지를 만들거나 프로젝트 파일을 바꾸지 않았다.

## 공통 규격 (요약)

| 항목 | 값 | 근거 |
|---|---|---|
| 스탠딩 가져오기 설정 | Sprite Single, Bilinear, CompressedHQ, 밉맵 없음, 최대 1024 이상, 아래 가운데 피벗 | `EWAssetSetup.ConfigurePortraitsAndAudio` |
| 스탠딩 표시 크기 | 화면 높이의 82% (heightRatio 0.82) — 루는 왼쪽, 나머지 오른쪽 | `UI/UIRoot.cs` PortraitLayer |
| 인물 캔버스 | 48×64 / 프레임 (괴물 토끼 80×96, 아우렐리우스 64×96), 아래 가운데 피벗, **오른쪽을 봄** (왼쪽은 좌우 반전) | `EWArtSpecs.ForPlaceholder`, `Art_*.asset` flipForLeft |
| 인물 화면 크기 | 64px = 2유닛 = 화면 높이 17.8% (확대 1배) · 35.6% (2배). 1080p 에서 48×64 프레임은 144×192 로 보임 | 계산 |
| 애니메이션 | 상태마다 AnimationClip (SpriteRenderer.m_Sprite 키프레임) + 캐릭터별 AnimatorController (Any State 전이 없음, 코드가 상태 이름으로 Play) | `EWAssetSetup.BuildCharacters`/`FillClip` |

동작별 프레임·FPS·루프·시트 크기는 [ANIMATION_CATALOG.md](ANIMATION_CATALOG.md), 그림 생성 지시는 [IMAGE_GENERATION_GUIDE.md](IMAGE_GENERATION_GUIDE.md), 연결 위치는 [ASSET_IMPLEMENTATION_MAPPING.md](ASSET_IMPLEMENTATION_MAPPING.md).

## 캐릭터 ID 목록

| ID | 이름 | 구분 | 애니메이션 상태 수 | 스탠딩 (사용/보유) | 현재 그림 |
|---|---|---|---|---|---|
| CH_PLAYER_001 | 루 | 플레이어 | 55 | 9 / 9 | 임시 도트 (PLACEHOLDER) |
| CH_BOSS_001 | 베넬 (본명 아이비) | 적→아군 (분기) | 28 | 5 / 5 | 임시 도트 (PLACEHOLDER) |
| CH_BOSS_001-P | 아이비 (베넬의 과거) | 회상 | 8 | 0 / 0 | 임시 도트 (PLACEHOLDER) |
| CH_ALLY_001 | 라비엘 | 아군 (8장 대결) | 13 | 6 / 6 | 임시 도트 (PLACEHOLDER) |
| CH_ALLY_001-B | 라비엘 (괴물 토끼) | 적 (비살상 대결) | 10 | 0 / 0 | 임시 도트 (PLACEHOLDER) |
| CH_BOSS_002 | 메이린 | 적 | 12 | 4 / 5 | 임시 도트 (PLACEHOLDER) |
| CH_BOSS_003 | 라크리모사 (본명 에드워드) | 적 | 9 | 2 / 4 | 임시 도트 (PLACEHOLDER) |
| CH_BOSS_003-P | 에드워드 (라크리모사의 과거) | 회상 | 2 | 1 / 1 | 임시 도트 (PLACEHOLDER) |
| CH_BOSS_003-K | 어린 에드워드 | 회상 | 3 | 0 / 0 | 임시 도트 (PLACEHOLDER) |
| CH_BOSS_004 | 아우렐리우스 | 적 | 8 | 3 / 3 | 임시 도트 (PLACEHOLDER) |
| CH_ENEMY_001 | 순찰 경비 | 적 | 5 | 0 / 0 | Foozle 드로이드 대용 (REPLACE) |
| CH_ENEMY_002 | 포획 드론 | 적 | 5 | 0 / 0 | Foozle 드로이드 대용 (REPLACE) |
| CH_ENEMY_003 | 운반 로봇 | 적 | 5 | 0 / 0 | Foozle 드로이드 대용 (REPLACE) |
| CH_NPC_001 | 연구원 1 | 적대 NPC | 9 | 1 / 3 | 임시 도트 (PLACEHOLDER) |
| CH_NPC_002 | 연구원 2 | 적대 NPC | 9 | 1 / 3 | 임시 도트 (PLACEHOLDER) |
| CH_NPC_003 | F | 중립→아군 | 9 | 3 / 3 | 임시 도트 (PLACEHOLDER) |
| CH_NPC_004 | 008 | 중립 | 9 | 4 / 4 | 임시 도트 (PLACEHOLDER) |
| CH_NPC_005 | 생존자 A | 중립 (교섭 실패 시 비살상 충돌) | 11 | 2 / 2 | 임시 도트 (PLACEHOLDER) |
| CH_NPC_006 | 생존자 B | 중립 | 11 | 2 / 2 | 임시 도트 (PLACEHOLDER) |
| CH_NPC_007 | 베스페르 단원 A | 중립 | 8 | 1 / 1 | 임시 도트 (PLACEHOLDER) |
| CH_NPC_008 | 베스페르 단원 B | 중립 | 8 | 1 / 1 | 임시 도트 (PLACEHOLDER) |
| CH_NPC_009 | 회수 경비 | 적 (컷신 전용) | 7 | 0 / 0 | 임시 도트 (PLACEHOLDER) |
| CH_NPC_010 | 린든 | 회상 | 6 | 0 / 0 | 임시 도트 (PLACEHOLDER) |
| CH_NPC_011 | 어른 (회상) | 회상 | 3 | 0 / 0 | 임시 도트 (PLACEHOLDER) |

같은 인물의 다른 모습(괴물 토끼, 과거의 아이비·에드워드)은 별도 캐릭터가 아니라 같은 ID 의 형태(-B, -P, -K)로 묶었다. 애니메이션 시트는 코드 구조상 따로 있다.

---

## CH_PLAYER_001 — 루

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_PLAYER_001 (코드 artId `lu`) |
| 등장 씬 | 씬 배치 없음 (코드가 생성 — 다음 줄) |
| 등장 (코드·연출) | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| 스토리상 역할 | 주인공 (E-D05-47) |
| 구분 | 플레이어 |
| 기본 외형 | 금발, 십대 후반 외형, 전기전자 개조. 밝고 명랑한 표정이 기본 (GDD 2장) |
| 의상·장비 | 현재 임시 그림: 파란 상의·남색 바지. 확정된 의상 설정 없음 (추정: 실험체로 붙잡혔다 탈출하므로 소지품 없음) |
| 무기 | 없음 (손끝 전기) |
| 주요 색 | 금발 노랑 / 파랑 상의 / 전기 청백색 |
| 신체 비율 | 큰 머리 SD (머리 ≈ 키의 40%) · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | 좌우 비대칭 요소가 생기면 왼쪽용 컨트롤러 필요 (Art_lu.asset leftController) |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_LU_ANGRY_CRY | 울며 화냄 (angry_cry) | 5 | `op_t_stop`, `c5_hair_v`, `c5_hall_off_v`, `c8_fight_end` 외 1 | PLACEHOLDER | P1 |
| ST_LU_DESPAIR | 절망 (despair) | 6 | `op_h_3`, `lab_realize`, `op_h_b`, `op_h_d` 외 2 | PLACEHOLDER | P1 |
| ST_LU_DETERMINED | 결심·단호 (determined) | 39 | `op_t_escape`, `v_talk2_good`, `v_talk_wait2`, `esc_4` 외 33 | PLACEHOLDER | P0 |
| ST_LU_NEUTRAL | 기본 (neutral) | 58 | `v_talk_wait1`, `pc_log_early`, `pc_exit`, `lab_pouch` 외 52 | PLACEHOLDER | P0 |
| ST_LU_PAINED | 고통 (pained) | 3 | `c10_limit_2`, `c10_limit_3`, `c11_wake` | PLACEHOLDER | P1 |
| ST_LU_SAD | 슬픔 (sad) | 40 | `op_h_2`, `v_kneel_reach`, `op_h_a`, `v_kill_after` 외 31 | PLACEHOLDER | P0 |
| ST_LU_SCARED | 공포 (scared) | 29 | `op_t_3`, `op_t_4`, `op_t_5`, `c1_escape_look` 외 23 | PLACEHOLDER | P0 |
| ST_LU_SMILE | 미소·밝음 (smile) | 73 | `c3_rest`, `v_after_join`, `v_release`, `w_first_2` 외 55 | PLACEHOLDER | P0 |
| ST_LU_SURPRISED | 놀람 (surprised) | 32 | `op_d_2`, `op_t_2`, `c3_success`, `lab_clue_tank` 외 27 | PLACEHOLDER | P0 |
| ST_LU_LOW | 최종전 ‘낮고 짧은 말’ 전용 표정 | — | `c10_confront` (현재 determined 로 대신) | MISSING | P1 |

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P0 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P0 |
| Jump | 점프 상승 | 1 | 8 | O | PLACEHOLDER | P0 |
| Fall | 낙하 | 1 | 8 | O | PLACEHOLDER | P0 |
| Climb | 사다리 오르기 | 2 | 6 | O | PLACEHOLDER | P0 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P0 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 8 | O | PLACEHOLDER | P0 |
| Sit | 앉기 (벤치·바닥) | 1 | 8 | O | PLACEHOLDER | P1 |
| Flick | 톡! — 검지를 튕겨 짧은 불꽃 (2번째 프레임이 판정 순간) | 3 | 9 | 1회 | PLACEHOLDER | P0 |
| Point | 겨누기 (스파크 샷·간섭) | 3 | 9 | 1회 | PLACEHOLDER | P0 |
| Swing | 휘두르기 (현재 근접 연출용) | 3 | 9 | 1회 | PLACEHOLDER | P1 |
| Kick | 발차기 (연출용) | 3 | 9 | 1회 | PLACEHOLDER | P1 |
| Charge | 외부 전력 끌어오기 — 손이 전원 쪽으로 | 2 | 6 | O | PLACEHOLDER | P0 |
| Discharge | 방전 (몸에서 전기 폭발) | 2 | 10 | O | PLACEHOLDER | P2 |
| Carry | 부축하기 (베넬) | 4 | 5 | O | PLACEHOLDER | P2 |
| BackIdle | 뒷모습 대기 (불타는 마을 첫 장면) | 2 | 2 | O | PLACEHOLDER | P2 |
| BackLook | 뒷모습에서 고개 돌림 | 1 | 8 | O | PLACEHOLDER | P2 |
| Limp | 절뚝이는 이동 | 6 | 6 | O | PLACEHOLDER | P2 |
| LimpIdle | 절뚝이다 멈춰 숨 고르기 | 2 | 2 | O | PLACEHOLDER | P2 |
| Sad | 고개 숙임 (슬픔) | 2 | 2 | O | PLACEHOLDER | P2 |
| ReachPC | 컴퓨터에 손 뻗기 | 2 | 4 | 1회 | PLACEHOLDER | P2 |
| Connect | 접속 — 빛이 몸에 번짐, 머리카락 들림 | 3 | 6 | O | PLACEHOLDER | P2 |
| Land | 착지 충격 | 1 | 8 | O | PLACEHOLDER | P2 |
| GetUp | 몸 일으키기 | 3 | 6 | 1회 | PLACEHOLDER | P2 |
| LookAround | 좌우 살피기 | 4 | 3 | O | PLACEHOLDER | P2 |
| Knock | 유리 두드리기 | 3 | 10 | O | PLACEHOLDER | P2 |
| BackAway | 겁먹고 뒷걸음 | 4 | 6 | O | PLACEHOLDER | P2 |
| Fear | 겁먹고 떨기 | 2 | 8 | O | PLACEHOLDER | P2 |
| ChargeEyes | 눈 질끈 감고 방전 준비 | 3 | 3 | 1회 | PLACEHOLDER | P2 |
| Recover | 방전 후 숨 몰아쉬기 | 2 | 2 | O | PLACEHOLDER | P2 |
| Squeeze | 문틈 비집고 통과 | 4 | 6 | O | PLACEHOLDER | P2 |
| Crouch | 웅크려 숨기 — 보이는 키 30px(0.95유닛) 이하 | 2 | 2 | O | PLACEHOLDER | P0 |
| CrouchWalk | 웅크려 걷기 — 같은 높이 제한 | 4 | 6 | O | PLACEHOLDER | P0 |
| Dodge | 대시 (낮게 미끄러지듯) | 1 | 1 | O | PLACEHOLDER | P0 |
| WallLean | 벽 짚고 숨 고르기 | 2 | 3 | O | PLACEHOLDER | P1 |
| TankCrouch | 실험 통 안 웅크림 | 2 | 2 | O | PLACEHOLDER | P2 |
| TankStruggle | 통 안에서 몸부림 | 2 | 7 | O | PLACEHOLDER | P2 |
| TankCower | 통 안에서 움츠림 | 2 | 2 | O | PLACEHOLDER | P2 |
| EyesClosed | 눈 질끈 감음 | 1 | 2 | O | PLACEHOLDER | P2 |
| Electrify | 전기화 직전 — 윤곽 발광 | 2 | 12 | O | PLACEHOLDER | P2 |
| Disgust | 입을 가리고 몸을 숙임 | 2 | 3 | O | PLACEHOLDER | P2 |
| HeadHold | 머리를 감싸 쥠 | 2 | 3 | O | PLACEHOLDER | P2 |
| Lifted | 목덜미를 잡혀 들림 — 목덜미 = 프레임 (24,33)px | 4 | 7 | O | PLACEHOLDER | P2 |
| Thrown | 던져짐 (몸 웅크림, 회전은 코드) | 1 | 1 | O | PLACEHOLDER | P2 |
| HoldHand | 동료 손 잡고 전원선으로 | 1 | 1 | O | PLACEHOLDER | P2 |
| HairUp | 펄스 뒤 머리가 곤두섬 + 작은 불꽃 | 2 | 3 | O | PLACEHOLDER | P2 |
| Share | 과자를 반 나눠 내밂 | 1 | 1 | O | PLACEHOLDER | P2 |
| HandsUp | 손 들기 (폐기소 첫 만남) | 2 | 4 | O | PLACEHOLDER | P2 |
| Wind | 옥상 바람 맞기 | 2 | 3 | O | PLACEHOLDER | P2 |
| Cold | 최종전 낮고 차가운 자세 (추정) | 2 | 4 | O | PLACEHOLDER | P2 |
| Limit | 한계 방출 — 양손으로 전류를 붙잡음 | 2 | 4 | O | PLACEHOLDER | P2 |
| Lie | 누워 있음 (중정) | 1 | 2 | O | PLACEHOLDER | P2 |
| Kneel | 무릎 꿇기 | 1 | 3 | 1회 | PLACEHOLDER | P2 |
| HandHold | 손 잡고 있기 (중정) | 1 | 2 | O | PLACEHOLDER | P2 |
| Release | 남은 전기를 놓음 (마지막) | 1 | 4 | O | PLACEHOLDER | P2 |

---

## CH_BOSS_001 — 베넬 (본명 아이비)

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_BOSS_001 (코드 artId `venelle`) |
| 등장 씬 | C06 연결 복도, M15 관찰 휴게실, M30 영상실, M39 서버실 |
| 등장 (코드·연출) | C06 대결 / M15 벤치 / (동행) M16~M30 동료 / M30 영상(비동행) / M39 붕괴 영상 |
| 스토리상 역할 | 3장 보스 · 동료 후보 |
| 구분 | 적→아군 (분기) |
| 기본 외형 | 보라 머리 (현재 임시), 정신이 온전치 않은 다정·멍함·공격성 |
| 의상·장비 | 동생이 좋아하던 캐릭터 옷 (코스프레처럼 보임). 팔에 장착하는 약품 혼합 장치: 약품 탱크→관→손목 약병 (GDD 2장, ART_REQUESTS) |
| 무기 | 팔 장치 (산성 분사·약병 투척) |
| 주요 색 | 보라 머리 / 자주색 옷 / 산성 녹색 장치 |
| 신체 비율 | 큰 머리 SD · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | 같은 옷 두 벌 소품 U_TwinOutfits 와 옷 색이 같아야 함 |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_VENELLE_BLANK | 멍함 (blank) | 22 | `c3_rest`, `v_talk1`, `v_talk1_silent`, `v_talk2_mock` 외 18 | PLACEHOLDER | P0 |
| ST_VENELLE_CONFUSED | 혼란 (confused) | 15 | `c3_success`, `v_talk1_good`, `v_talk2`, `v_talk2_good` 외 11 | PLACEHOLDER | P0 |
| ST_VENELLE_EXCITED | 흥분 (excited) | 12 | `v_talk1_good`, `v_talk1_threat`, `v_talk2_command`, `v_intro_3` 외 8 | PLACEHOLDER | P0 |
| ST_VENELLE_GENTLE | 다정 (gentle) | 19 | `c3_success`, `c3_rest`, `v_talk3_good`, `r_2` 외 13 | PLACEHOLDER | P0 |
| ST_VENELLE_QUIET | 조용함 (quiet) | 24 | `c3_rest`, `v_talk1_good`, `v_talk2_good`, `v_talk_wait2` 외 18 | PLACEHOLDER | P0 |

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P0 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P0 |
| Jump | 점프 상승 | 1 | 8 | O | PLACEHOLDER | P1 |
| Fall | 낙하 | 1 | 8 | O | PLACEHOLDER | P1 |
| Climb | 사다리 오르기 | 2 | 6 | O | PLACEHOLDER | P1 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P0 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 8 | O | PLACEHOLDER | P1 |
| Sit | 앉기 (벤치·바닥) | 1 | 8 | O | PLACEHOLDER | P1 |
| Attack | 휘두르기·약병 던지기 | 3 | 9 | 1회 | PLACEHOLDER | P0 |
| Collapsed | 쓰러짐 (개그 장면) | 1 | 8 | O | PLACEHOLDER | P2 |
| Supported | 부축받으며 걷기 | 4 | 5 | O | PLACEHOLDER | P2 |
| Dizzy | 어지러움 | 2 | 3 | O | PLACEHOLDER | P2 |
| Stalk | 불균형하고 느슨한 걸음, 고개 기울임 | 5 | 5 | O | PLACEHOLDER | P0 |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 2 | 6 | O | PLACEHOLDER | P0 |
| Dash | 돌진 | 1 | 1 | O | PLACEHOLDER | P0 |
| Pant | 숨 고르기 (말 걸기 기회) | 2 | 3 | O | PLACEHOLDER | P0 |
| Laugh | 혼자 웃음 | 2 | 3 | O | PLACEHOLDER | P2 |
| Grab | 잡기 — 손 = 프레임 (37,18)px (VenelleBoss.GrabHand 와 일치) | 2 | 3 | O | PLACEHOLDER | P0 |
| Throw | 집어 던짐 | 2 | 7 | 1회 | PLACEHOLDER | P1 |
| Spray | 산성 분사 준비 | 2 | 8 | O | PLACEHOLDER | P0 |
| Mix | 재혼합 (거품, 빈틈임이 읽히게) | 3 | 6 | O | PLACEHOLDER | P0 |
| Toss | 던지기 (약병·상자) | 2 | 7 | 1회 | PLACEHOLDER | P0 |
| Downed | 체력 0 — 무릎 꿇고 숨 몰아쉼, 관에서 약품이 샘 | 1 | 2 | O | PLACEHOLDER | P0 |
| TalkWalk | 투덜거리며 걸어옴 (등장) | 4 | 6 | O | PLACEHOLDER | P2 |
| Comb | 빗을 꺼냈다 금속 빗을 보고 넣음 | 2 | 3 | 1회 | PLACEHOLDER | P2 |
| Wind | 옥상 바람 맞기 | 2 | 3 | O | PLACEHOLDER | P2 |
| SitQuiet | 침대에 앉아 말을 잃음 | 2 | 3 | O | PLACEHOLDER | P2 |
| Share | 과자를 반 나눠 내밂 | 1 | 1 | O | PLACEHOLDER | P2 |

### D. 적·보스 행동과 필요한 그림

- 패턴 6종: 돌진→휘두르기(Windup·Dash·Attack), 산성 분사(Spray + FX_AcidFan), 약병 투척(Toss + FX_Flask), 웅덩이(FX_PuddleAcid), 재혼합 빈틈(Mix), 잡기→던지기(Grab + 루 Lifted/Thrown)
- HP 0 → Downed. 협상 성공 → Collapsed·Supported (부축)
- 보스용 큰 그림 없음 — 일반 크기로 싸운다 (ART_REQUESTS ‘C 우선’)

---

## CH_BOSS_001-P — 아이비 (베넬의 과거)

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_BOSS_001-P (코드 artId `ivy`) |
| 등장 씬 | 씬 배치 없음 (코드가 생성 — 다음 줄) |
| 등장 (코드·연출) | M25 회상 컷신 (StoryEventsUnderground.Still) |
| 스토리상 역할 | 6장 회상 |
| 구분 | 회상 |
| 기본 외형 | 베넬과 같은 얼굴, 팔 장치 없음 (ART_REQUESTS) |
| 의상·장비 | 평상복 (추정) |
| 무기 | 없음 |
| 주요 색 | 보라 긴 머리 / 보라 상의 (세피아로 표시) |
| 신체 비율 | 큰 머리 SD · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | 같은 인물의 과거 모습 |

### B. 전신 스탠딩 (대화 일러스트)

스탠딩 없음 — 이 인물은 대사가 없거나 이름표만 쓴다 (대사 2줄). 회상 인물은 스탠딩을 쓰지 않는 쪽이 기존 권장 (ART_REQUESTS). 추가는 OPTIONAL.

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P2 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P2 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P2 |
| Sit | 앉기 (벤치·바닥) | 1 | 8 | O | PLACEHOLDER | P2 |
| Carry | 부축하기 (베넬) | 4 | 5 | O | PLACEHOLDER | P2 |
| Kneel | 무릎 꿇기 | 1 | 3 | 1회 | PLACEHOLDER | P2 |
| Give | 건네기 | 1 | 3 | 1회 | PLACEHOLDER | P2 |
| Wave | 손 흔들기 | 2 | 4 | O | OPTIONAL | P3 |

---

## CH_ALLY_001 — 라비엘

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_ALLY_001 (코드 artId `raviel`) |
| 등장 씬 | M32 상층 회랑, M33 라비엘의 방, M44 전이 장치실, M46 무너진 중정 |
| 등장 (코드·연출) | M32 재회 / M33 방 / (동료) M34~M43 / M44 부상 / M45 부상 동행 / M46 마지막 |
| 스토리상 역할 | 친구 · 8장 보스 · 동료 |
| 구분 | 아군 (8장 대결) |
| 기본 외형 | 귀 없는 토끼 악마. 연구주임 가운 (ART_REQUESTS). 차분하고 약간 비꼬는 얼굴 |
| 의상·장비 | 연구주임 가운 |
| 무기 | 없음 (변신) |
| 주요 색 | 흰 머리 / 흰 가운 (현재 임시) |
| 신체 비율 | 큰 머리 SD · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | 변신형은 별도 시트 (ravielBeast) |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_RAVIEL_NEUTRAL | 기본 (neutral) | 12 | `c8_photo`, `c8_photo_after`, `c8_cups`, `c8_console` 외 6 | PLACEHOLDER | P0 |
| ST_RAVIEL_PAINED | 고통 (pained) | 8 | `c8_reunion_2`, `c8_after_duel`, `c8_confess`, `c9_force_r` 외 3 | PLACEHOLDER | P1 |
| ST_RAVIEL_SAD | 슬픔 (sad) | 13 | `c8_reunion_1`, `c8_after_duel`, `c8_past`, `c8_confess` 외 7 | PLACEHOLDER | P0 |
| ST_RAVIEL_SMILE | 미소·밝음 (smile) | 10 | `c8_photo_after`, `c8_cups`, `c8_bench_1`, `c8_bench_3` 외 5 | PLACEHOLDER | P0 |
| ST_RAVIEL_SMIRK | 비웃음·씩 웃음 (smirk) | 5 | `c8_reunion_1`, `c8_photo`, `c8_choice_sit`, `c8_console` 외 1 | PLACEHOLDER | P1 |
| ST_RAVIEL_SURPRISED | 놀람 (surprised) | 3 | `c8_confess`, `c8_bench`, `c9_capture` | PLACEHOLDER | P1 |

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P1 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P1 |
| Sit | 앉기 (벤치·바닥) | 1 | 8 | O | PLACEHOLDER | P1 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 8 | O | PLACEHOLDER | P1 |
| Talk | 말하기 손짓 | 2 | 4 | O | PLACEHOLDER | P1 |
| Point | 겨누기 (스파크 샷·간섭) | 1 | 9 | 1회 | PLACEHOLDER | P1 |
| Type | 단말 입력 | 2 | 6 | O | OPTIONAL | P3 |
| HoldHand | 동료 손 잡고 전원선으로 | 1 | 1 | O | PLACEHOLDER | P2 |
| Catch | 붙잡기 (휴지통 탈출 뒤) | 1 | 6 | 1회 | PLACEHOLDER | P2 |
| Injured | 부상 상태로 서 있기·따라오기 | 2 | 4 | O | PLACEHOLDER | P2 |
| Cup | 컵을 양손으로 잡기 | 1 | 2 | O | OPTIONAL | P3 |
| Sad | 고개 숙임 (슬픔) | 1 | 2 | O | PLACEHOLDER | P2 |

---

## CH_ALLY_001-B — 라비엘 (괴물 토끼)

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_ALLY_001-B (코드 artId `ravielBeast`) |
| 등장 씬 | M32 상층 회랑, M45 붕괴 연결로 |
| 등장 (코드·연출) | M32 대결 / M45 추락 컷신 |
| 스토리상 역할 | 8장 대결 형태 · 10장 추락 |
| 구분 | 적 (비살상 대결) |
| 기본 외형 | 온몸이 근육질인 약 2m 거대 토끼 (GDD 2장) |
| 의상·장비 | 없음 |
| 무기 | 앞발·도약 |
| 주요 색 | 회백색 (현재 임시) |
| 신체 비율 | 거대 체형 (캔버스 80×96) · 캔버스 80×96 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | 같은 인물의 다른 형태 — 별도 캐릭터 아님 |

### B. 전신 스탠딩 (대화 일러스트)

스탠딩 없음 — 이 인물은 대사가 없거나 이름표만 쓴다 (대사 0줄). 회상 인물은 스탠딩을 쓰지 않는 쪽이 기존 권장 (ART_REQUESTS). 추가는 OPTIONAL.

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P0 |
| Walk | 걷기 | 4 | 7 | O | PLACEHOLDER | P0 |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 2 | 5 | O | PLACEHOLDER | P0 |
| Leap | 도약 | 1 | 3 | 1회 | PLACEHOLDER | P0 |
| Slam | 내려찍기 | 1 | 3 | 1회 | PLACEHOLDER | P0 |
| Swipe | 앞발 휘두르기 | 1 | 3 | 1회 | PLACEHOLDER | P0 |
| Stun | 빈틈 (경직) | 2 | 5 | O | PLACEHOLDER | P0 |
| Hurt | 피격 경직 | 1 | 3 | 1회 | PLACEHOLDER | P0 |
| Hold | 들고 있기 | 1 | 3 | O | PLACEHOLDER | P2 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 3 | O | PLACEHOLDER | P1 |

### D. 적·보스 행동과 필요한 그림

- 도약 내려찍기(Leap·Slam), 앞발(Swipe), 돌진(Walk 재사용 추정), 빈틈(Stun)
- 도망 경로에서는 추격(Walk·Leap)만
- HP 0 → Down → 사람 라비엘(Idle)로 교체

---

## CH_BOSS_002 — 메이린

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_BOSS_002 (코드 artId `meilin`) |
| 등장 씬 | M28 접수 홀, M43 캡슐 회랑, M44 전이 장치실 |
| 등장 (코드·연출) | — |
| 스토리상 역할 | 7장 보스 · 10장 재등장 |
| 구분 | 적 |
| 기본 외형 | 중국식 도자기 인형, 얼굴 반쪽이 기워진 듯, 긴 땋은 머리, 바늘꽂이 머리장식 (GDD 2장) |
| 의상·장비 | 치파오, 발바닥 파츠가 롤러스케이트 |
| 무기 | 섬세한 공예용 가위 |
| 주요 색 | 검은 땋은 머리 / 붉은 치파오 (현재 임시) |
| 신체 비율 | 큰 머리 SD (인형 관절 추정) · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | — |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_MEILIN_DETERMINED | 결심·단호 (determined) | 7 | `c7_meilin_v`, `c7_meilin_solo`, `c7_meilin_arm`, `c7_meilin_armoff` 외 1 | PLACEHOLDER | P1 |
| ST_MEILIN_SAD | 슬픔 (sad) | 2 | `c7_meilin_lose`, `c7_meilin_memory` | PLACEHOLDER | P1 |
| ST_MEILIN_SCARED | 공포 (scared) | 3 | `c7_meilin_lose`, `c10_meilin`, `c10_meilin_last` | PLACEHOLDER | P1 |
| ST_MEILIN_SMILE | 미소·밝음 (smile) | 3 | `c7_meilin_v`, `c7_meilin_solo`, `c7_meilin_start` | PLACEHOLDER | P1 |
| ST_MEILIN_SURPRISED | 놀람 (surprised) | 0 | — | OPTIONAL | P3 |

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P0 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |
| Roll | 롤러 이동 | 2 | 6 | O | PLACEHOLDER | P0 |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 1 | 6 | O | PLACEHOLDER | P0 |
| Snip | 가위 공격 | 1 | 6 | 1회 | PLACEHOLDER | P0 |
| Dash | 돌진 | 1 | 1 | O | PLACEHOLDER | P0 |
| Stun | 빈틈 (경직) | 2 | 4 | O | PLACEHOLDER | P0 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P0 |
| Talk | 말하기 손짓 | 2 | 4 | O | PLACEHOLDER | P1 |
| Angry | 화냄 | 2 | 4 | O | OPTIONAL | P3 |
| Pull | 캡슐 손잡이 당기기 | 2 | 4 | O | PLACEHOLDER | P2 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 8 | O | PLACEHOLDER | P1 |

### D. 적·보스 행동과 필요한 그림

- 포획팔(U_Claw) + 롤러 돌진(Roll·Dash) / 가위(Snip) / 빈틈(Stun)
- 페이즈: 같은 속임수 2회 뒤 포획팔 정지 → 가위·롤러 위주 (외형 변화 없음)
- 10장 Pull(캡슐 손잡이) → 사망 후 U_Roller·U_Register·U_StillHand 소품으로 대체 표시

---

## CH_BOSS_003 — 라크리모사 (본명 에드워드)

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_BOSS_003 (코드 artId `lacrimosa`) |
| 등장 씬 | M37 공명 공연장, M39 서버실 |
| 등장 (코드·연출) | — |
| 스토리상 역할 | 9장 보스 |
| 구분 | 적 |
| 기본 외형 | 오페라 가수 같은 공연복, 목·흉곽 음향 증폭 장치 (GDD 2장) |
| 의상·장비 | 공연복 |
| 무기 | 목소리 (울림파) |
| 주요 색 | 검은 머리 / 남색 공연복 + 목 장치 (현재 임시) |
| 신체 비율 | 큰 머리 SD · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | — |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_LACRIMOSA_GENTLE | 다정 (gentle) | 2 | `c9_lac_meet` | PLACEHOLDER | P1 |
| ST_LACRIMOSA_PAINED | 고통 (pained) | 0 | — | OPTIONAL | P3 |
| ST_LACRIMOSA_QUIET | 조용함 (quiet) | 3 | `c9_lac_stop`, `c9_lac_listen`, `c9_lac_persuaded` | PLACEHOLDER | P1 |
| ST_LACRIMOSA_SAD | 슬픔 (sad) | 0 | — | OPTIONAL | P3 |

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P0 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |
| Sing | 노래 (울림파 발동) | 2 | 4 | O | PLACEHOLDER | P0 |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 1 | 6 | O | PLACEHOLDER | P0 |
| Stagger | 보호 정지 휘청임 | 2 | 4 | O | PLACEHOLDER | P0 |
| Listen | 녹음을 듣고 멈춤 | 1 | 2 | O | PLACEHOLDER | P0 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P0 |
| Bow | 인사 | 1 | 2 | O | OPTIONAL | P3 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 8 | O | PLACEHOLDER | P1 |

### D. 적·보스 행동과 필요한 그림

- 울림파(Sing + 코드 파동), 공명 기둥(스피커 자리 표식), 보호 정지(Stagger), 녹음 듣기(Listen)
- 설득/제압 뒤 Down

---

## CH_BOSS_003-P — 에드워드 (라크리모사의 과거)

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_BOSS_003-P (코드 artId `edward`) |
| 등장 씬 | M37 공명 공연장 |
| 등장 (코드·연출) | M37 회상 |
| 스토리상 역할 | 9장 회상: 음향실 |
| 구분 | 회상 |
| 기본 외형 | 극장 음향 일을 하던 성인 (GDD 2장) |
| 의상·장비 | 작업복 (추정) |
| 무기 | 없음 |
| 주요 색 | 갈색 머리 / 어두운 작업복 |
| 신체 비율 | 큰 머리 SD · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | 같은 인물의 과거 모습 |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_EDWARD_QUIET | 조용함 (quiet) | 1 | `c9_edward_2` | PLACEHOLDER | P1 |

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P2 |
| Write | 녹음 파일 이름 적기 | 2 | 2 | O | PLACEHOLDER | P2 |

---

## CH_BOSS_003-K — 어린 에드워드

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_BOSS_003-K (코드 artId `edwardKid`) |
| 등장 씬 | M37 공명 공연장 |
| 등장 (코드·연출) | M37 회상·영상 (EWRoomsLate.cs:505) |
| 스토리상 역할 | 9장 회상: TV 노래를 듣는 아이 |
| 구분 | 회상 |
| 기본 외형 | 아이 |
| 의상·장비 | — |
| 무기 | 없음 |
| 주요 색 | 회색 옷 |
| 신체 비율 | 아이 비율 · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | — |

### B. 전신 스탠딩 (대화 일러스트)

스탠딩 없음 — 이 인물은 대사가 없거나 이름표만 쓴다 (대사 0줄). 회상 인물은 스탠딩을 쓰지 않는 쪽이 기존 권장 (ART_REQUESTS). 추가는 OPTIONAL.

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P2 |
| Hide | 숨기 | 1 | 3 | O | PLACEHOLDER | P2 |
| Listen | 녹음을 듣고 멈춤 | 2 | 2 | O | PLACEHOLDER | P2 |

---

## CH_BOSS_004 — 아우렐리우스

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_BOSS_004 (코드 artId `aurelius`) |
| 등장 씬 | M44 전이 장치실 |
| 등장 (코드·연출) | — |
| 스토리상 역할 | 최종 보스 (총재) |
| 구분 | 적 |
| 기본 외형 | 기괴한 내부 기계를 덮는 아름다운 외피. 깨지면 내부 기계가 드러남 (GDD 2장, ART_REQUESTS) |
| 의상·장비 | 외피 (긴 금발·긴 의복, 현재 임시) |
| 무기 | 빛·연결 광선·호위 구 |
| 주요 색 | 금발 / 남색 의복 / 금선 (현재 임시) |
| 신체 비율 | 큰 체형 (캔버스 64×96) · 캔버스 64×96 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | Broken·Ruined 상태로 외피 파손 단계를 표현 |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_AURELIUS_DETERMINED | 결심·단호 (determined) | 1 | `c10_aur_reconnect` | PLACEHOLDER | P1 |
| ST_AURELIUS_GENTLE | 다정 (gentle) | 6 | `c9_force_broadcast`, `c10_meilin`, `c10_aur_1`, `c10_lu_cold` | PLACEHOLDER | P1 |
| ST_AURELIUS_PAINED | 고통 (pained) | 3 | `c10_limit_after_1`, `c10_limit_after_2`, `c10_aur_last` | PLACEHOLDER | P1 |

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P0 |
| Talk | 말하기 손짓 | 2 | 3 | O | PLACEHOLDER | P1 |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 2 | 6 | O | PLACEHOLDER | P0 |
| Cast | 빛·광선 발사 | 1 | 3 | 1회 | PLACEHOLDER | P0 |
| Hurt | 피격 경직 | 1 | 3 | 1회 | PLACEHOLDER | P0 |
| Broken | 외피 일부 파손 | 2 | 3 | O | PLACEHOLDER | P2 |
| Ruined | 외피 크게 파손, 내부 기계 노출 | 2 | 3 | O | PLACEHOLDER | P2 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 3 | 1회 | PLACEHOLDER | P1 |

### D. 적·보스 행동과 필요한 그림

- 3단계: 1단계 외피+호위 구(U_Escort) / 2단계 축전(U_Capacitor)·냉각(U_CoolPipe) / 3단계 결합부(U_Coupling)
- 단계별 외형 변화 상태가 정의돼 있다: Broken·Ruined — **코드 문자열 참조는 있으나 단계 전환에서 실제로 쓰는지 정적 확인 필요 (추정)**
- 사망: 폭발·정지 후 숨김 (Down)

---

## CH_ENEMY_001 — 순찰 경비

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_ENEMY_001 (코드 artId `droid03`) |
| 등장 씬 | 씬 배치 없음 (코드가 생성 — 다음 줄) |
| 등장 (코드·연출) | C02~M35 감시 구역 (Guard 컴포넌트, artId 고정) |
| 스토리상 역할 | 감시 구역 (C02~M35 일부) |
| 구분 | 적 |
| 기본 외형 | 사람보다 약간 작은 기계, 제압봉 (ART_REQUESTS). 현재 Foozle Droid03 대용 |
| 의상·장비 | — |
| 무기 | 제압봉 |
| 주요 색 | Foozle 원본 색 |
| 신체 비율 | 기계 · 캔버스 Foozle 원본 (정사각 프레임 = 시트 높이) |
| 현재 이미지 | Foozle Sci-fi Lab Droids (CC0) — 다른 용도의 로봇을 대신 씀 |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | 공격 준비 자세(제압봉 치켜듦) 프레임 필요 — 지금은 깜빡임으로만 예고 |

### B. 전신 스탠딩 (대화 일러스트)

스탠딩 없음 — 이 인물은 대사가 없거나 이름표만 쓴다 (대사 0줄). 회상 인물은 스탠딩을 쓰지 않는 쪽이 기존 권장 (ART_REQUESTS). 추가는 OPTIONAL.

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 4 | 8 | O | REPLACE | P0 |
| Move | 이동 | 8 | 10 | O | REPLACE | P0 |
| Attack | 제압봉 공격 | 9 | 14 | 1회 | REPLACE | P0 |
| Hurt | 피격 경직 | 4 | 10 | 1회 | REPLACE | P0 |
| Death | 파괴 | 11 | 12 | 1회 | REPLACE | P0 |

### D. 적·보스 행동과 필요한 그림

- 순찰·의심·확인·추적·수색·복귀는 같은 Move/Idle 로 표현 (상태 표시는 코드의 아이콘·색) — **추정: 상태별 전용 프레임 없음**
- 공격 준비 0.75초 (`action_tuning.json` attackWindup) — 지금은 깜빡임으로만 예고 → **Windup 전용 프레임 필요 (MISSING)**
- 피격 경직·일시 무력화(재가동 28초) — Hurt / Death 프레임 재사용 (추정)
- 방어 자세 / 자세 무너짐 구분 그림 권장 (ART_REQUESTS)

---

## CH_ENEMY_002 — 포획 드론

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_ENEMY_002 (코드 artId `droid01`) |
| 등장 씬 | 씬 배치 없음 (코드가 생성 — 다음 줄) |
| 등장 (코드·연출) | 감시 구역 드론·지원 드론 (Reinforcements) |
| 스토리상 역할 | 감시 구역 |
| 구분 | 적 |
| 기본 외형 | 아래를 비추는 렌즈가 보이는 비행 드론 (ART_REQUESTS). 현재 Foozle Droid01 대용 |
| 의상·장비 | — |
| 무기 | 그물 (GDD) |
| 주요 색 | Foozle 원본 색 |
| 신체 비율 | 기계 (비행, 1유닛 떠 있음) · 캔버스 Foozle 원본 (정사각 프레임 = 시트 높이) |
| 현재 이미지 | Foozle Sci-fi Lab Droids (CC0) — 다른 용도의 로봇을 대신 씀 |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | — |

### B. 전신 스탠딩 (대화 일러스트)

스탠딩 없음 — 이 인물은 대사가 없거나 이름표만 쓴다 (대사 0줄). 회상 인물은 스탠딩을 쓰지 않는 쪽이 기존 권장 (ART_REQUESTS). 추가는 OPTIONAL.

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 4 | 8 | O | REPLACE | P0 |
| Move | 이동 | 4 | 10 | O | REPLACE | P0 |
| Attack | 그물·사격 | 12 | 14 | 1회 | REPLACE | P0 |
| Hurt | 피격 경직 | 3 | 10 | 1회 | REPLACE | P0 |
| Death | 파괴 | 9 | 12 | 1회 | REPLACE | P0 |

### D. 적·보스 행동과 필요한 그림

- 순찰·추적: Move / 공격(그물): Attack / 일시 무력화: Hurt·Death (추정)
- 지원 드론(CCTV 발각 시 화면 밖에서 등장)도 같은 그림

---

## CH_ENEMY_003 — 운반 로봇

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_ENEMY_003 (코드 artId `droid02`) |
| 등장 씬 | M47 잔해 더미 |
| 등장 (코드·연출) | 감시 구역 운반 로봇 · M47 무인 회수 장치 |
| 스토리상 역할 | 감시 구역 · M47 무인 회수 장치 대용 |
| 구분 | 적 |
| 기본 외형 | 짐을 나르는 로봇. 현재 Foozle Droid02 대용 |
| 의상·장비 | — |
| 무기 | 짐 던지기 (GDD) |
| 주요 색 | Foozle 원본 색 |
| 신체 비율 | 기계 (0.85유닛 떠 있음) · 캔버스 Foozle 원본 (정사각 프레임 = 시트 높이) |
| 현재 이미지 | Foozle Sci-fi Lab Droids (CC0) — 다른 용도의 로봇을 대신 씀 |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | M47 회수 장치도 이 그림을 쓰고 있음 → 전용 그림 필요 |

### B. 전신 스탠딩 (대화 일러스트)

스탠딩 없음 — 이 인물은 대사가 없거나 이름표만 쓴다 (대사 0줄). 회상 인물은 스탠딩을 쓰지 않는 쪽이 기존 권장 (ART_REQUESTS). 추가는 OPTIONAL.

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 4 | 8 | O | REPLACE | P0 |
| Move | 이동 | 4 | 10 | O | REPLACE | P0 |
| Attack | 짐 던지기·밀치기 (추정) | 7 | 14 | 1회 | REPLACE | P0 |
| Hurt | 피격 경직 | 4 | 10 | 1회 | REPLACE | P0 |
| Death | 파괴 | 13 | 12 | 1회 | REPLACE | P0 |

### D. 적·보스 행동과 필요한 그림

- 감시 구역 운반 로봇: Idle/Move/Attack/Hurt/Death
- **M47 무인 회수 장치가 이 그림을 대신 씀 → 전용 그림 MISSING** (ART_REQUESTS)

---

## CH_NPC_001 — 연구원 1

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_NPC_001 (코드 artId `researcher`) |
| 등장 씬 | C01 실험실 |
| 등장 (코드·연출) | Editor/EWRooms.cs (방 정의) |
| 스토리상 역할 | 1장 실험 통 장면 |
| 구분 | 적대 NPC |
| 기본 외형 | 베스페르 하급 연구원. 지친 사람 (GDD 2장 조연) |
| 의상·장비 | 연구 가운 (추정) |
| 무기 | 제압봉 |
| 주요 색 | 갈색 머리 / 흰 가운 |
| 신체 비율 | 큰 머리 SD · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | 연구원 2와 머리 모양·체형·색으로 구분 (ART_REQUESTS) |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_RESEARCHER_NEUTRAL | 기본 (neutral) | 0 | — | OPTIONAL | P3 |
| ST_RESEARCHER_SMILE | 미소·밝음 (smile) | 1 | `op_t_calm1` | PLACEHOLDER | P1 |
| ST_RESEARCHER_SMIRK | 비웃음·씩 웃음 (smirk) | 0 | — | OPTIONAL | P3 |

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P1 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P1 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 8 | O | PLACEHOLDER | P1 |
| Scared | 겁먹음 | 1 | 8 | O | PLACEHOLDER | P1 |
| Talk | 말하기 손짓 | 2 | 4 | O | PLACEHOLDER | P1 |
| BatonOut | 제압봉 꺼내기 | 3 | 6 | 1회 | PLACEHOLDER | P1 |
| BatonIdle | 제압봉 든 대기 (끝이 지지직) | 2 | 4 | O | PLACEHOLDER | P1 |
| BatonWalk | 제압봉 든 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |

---

## CH_NPC_002 — 연구원 2

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_NPC_002 (코드 artId `researcher2`) |
| 등장 씬 | C01 실험실 |
| 등장 (코드·연출) | Editor/EWRooms.cs (방 정의) |
| 스토리상 역할 | 1장 실험 통 장면 |
| 구분 | 적대 NPC |
| 기본 외형 | 연구원 1과 구분되는 체형·머리 |
| 의상·장비 | 연구 가운 (추정) |
| 무기 | 제압봉 |
| 주요 색 | 검은 머리 / 흰 가운 / 녹색 바지 |
| 신체 비율 | 큰 머리 SD · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | — |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_RESEARCHER2_NEUTRAL | 기본 (neutral) | 0 | — | OPTIONAL | P3 |
| ST_RESEARCHER2_SMIRK | 비웃음·씩 웃음 (smirk) | 1 | `op_t_calm2` | PLACEHOLDER | P1 |
| ST_RESEARCHER2_SURPRISED | 놀람 (surprised) | 0 | — | OPTIONAL | P3 |

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P1 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P1 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 8 | O | PLACEHOLDER | P1 |
| Scared | 겁먹음 | 1 | 8 | O | PLACEHOLDER | P1 |
| Talk | 말하기 손짓 | 2 | 4 | O | PLACEHOLDER | P1 |
| BatonOut | 제압봉 꺼내기 | 3 | 6 | 1회 | PLACEHOLDER | P1 |
| BatonIdle | 제압봉 든 대기 (끝이 지지직) | 2 | 4 | O | PLACEHOLDER | P1 |
| BatonWalk | 제압봉 든 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |

---

## CH_NPC_003 — F

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_NPC_003 (코드 artId `f`) |
| 등장 씬 | M17 폐기소 거처, M39 서버실 |
| 등장 (코드·연출) | Editor/EWRoomsUnderground.cs (방 정의) |
| 스토리상 역할 | 4장 폐기소 대표 할머니 |
| 구분 | 중립→아군 |
| 기본 외형 | 짧고 투박한 할머니, 목에 번호표(F) (ART_REQUESTS) |
| 의상·장비 | 낡은 옷 (추정) |
| 무기 | 없음 |
| 주요 색 | 회색 머리 / 갈색 옷 (현재 임시) |
| 신체 비율 | 큰 머리 SD, 작고 투박 · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | — |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_F_DETERMINED | 결심·단호 (determined) | 5 | `c4_meet_1`, `c4_meet_2`, `c4_deal_fail`, `c4_fight_end` 외 1 | PLACEHOLDER | P1 |
| ST_F_GENTLE | 다정 (gentle) | 8 | `c4_gift`, `c4_f_box_gift`, `c4_f_box`, `c4_f_again` 외 3 | PLACEHOLDER | P1 |
| ST_F_NEUTRAL | 기본 (neutral) | 17 | `c4_meet_2`, `c4_deal_again`, `c4_deal_pump`, `c4_deal_pump_again` 외 9 | PLACEHOLDER | P0 |

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P1 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P1 |
| Sit | 앉기 (벤치·바닥) | 1 | 8 | O | PLACEHOLDER | P1 |
| Talk | 말하기 손짓 | 2 | 4 | O | PLACEHOLDER | P1 |
| Point | 겨누기 (스파크 샷·간섭) | 1 | 9 | 1회 | PLACEHOLDER | P1 |
| Toss | 던지기 (약병·상자) | 2 | 7 | 1회 | PLACEHOLDER | P1 |
| Guard | 막아서기·방어 자세 | 1 | 3 | O | PLACEHOLDER | P1 |
| Tag | 번호표 들여다보기 | 1 | 1 | O | PLACEHOLDER | P2 |

---

## CH_NPC_004 — 008

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_NPC_004 (코드 artId `kid008`) |
| 등장 씬 | M17 폐기소 거처, M19 폐품 시장, M39 서버실 |
| 등장 (코드·연출) | Editor/EWRoomsUnderground.cs (방 정의) |
| 스토리상 역할 | 4장 폐기소 아이 |
| 구분 | 중립 |
| 기본 외형 | 아이 (번호 8). 현재 임시는 어른 그림 0.78배 축소 → 아이 비율로 새로 (ART_REQUESTS) |
| 의상·장비 | 낡은 옷, 번호표 (추정) |
| 무기 | 없음 |
| 주요 색 | 갈색 머리 / 올리브색 옷 |
| 신체 비율 | 아이 비율 · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | — |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_KID008_NEUTRAL | 기본 (neutral) | 6 | `c4_008_later`, `c4_008_fed`, `c4_008_solo`, `c4_008_name` 외 1 | PLACEHOLDER | P1 |
| ST_KID008_SAD | 슬픔 (sad) | 1 | `c4_008_hiding` | PLACEHOLDER | P1 |
| ST_KID008_SMILE | 미소·밝음 (smile) | 4 | `c4_008_spark`, `c4_008_after`, `c4_lines`, `c4_lines_again` | PLACEHOLDER | P1 |
| ST_KID008_SURPRISED | 놀람 (surprised) | 3 | `c4_meet_1`, `c4_008_ask`, `c4_008_split` | PLACEHOLDER | P1 |

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P1 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P1 |
| Sit | 앉기 (벤치·바닥) | 1 | 8 | O | PLACEHOLDER | P1 |
| Talk | 말하기 손짓 | 2 | 4 | O | PLACEHOLDER | P1 |
| Hide | 숨기 | 1 | 3 | O | PLACEHOLDER | P2 |
| Eat | 먹기 | 2 | 3 | O | PLACEHOLDER | P2 |
| Hold | 들고 있기 | 1 | 1 | O | PLACEHOLDER | P2 |
| Draw | 쪼그려 바닥에 선 긋기 | 2 | 3 | O | PLACEHOLDER | P2 |

---

## CH_NPC_005 — 생존자 A

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_NPC_005 (코드 artId `survivorA`) |
| 등장 씬 | M19 폐품 시장, M27 수용동, M30 영상실, M39 서버실 |
| 등장 (코드·연출) | Editor/EWRoomsUnderground.cs (방 정의) |
| 스토리상 역할 | 4장 폐기소 주민 · 7장 수용동 · 9장 영상 |
| 구분 | 중립 (교섭 실패 시 비살상 충돌) |
| 기본 외형 | 경비·연구원이 아닌 일반인 (ART_REQUESTS) |
| 의상·장비 | 낡은 옷 |
| 무기 | 파이프·고철 |
| 주요 색 | 갈색 머리 / 올리브 녹색 상의 |
| 신체 비율 | 큰 머리 SD · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | 쓰러짐·사망 그림 쓰지 않음 (ART_REQUESTS) |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_SURVIVOR_NEUTRAL | 기본 (neutral) | 4 | `c4_gift`, `c4_sv_gift`, `c4_sv_reconciled`, `c4_sv_repair` | PLACEHOLDER | P1 |
| ST_SURVIVOR_SCARED | 공포 (scared) | 4 | `c4_deal_fail`, `c4_sv_fight`, `c7_release_route`, `c7_release_route_v` | PLACEHOLDER | P1 |

생존자·단원 스탠딩은 A/B 공용 한 벌 (`survivor_*`, `member_*`).

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P0 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P1 |
| Sit | 앉기 (벤치·바닥) | 1 | 8 | O | PLACEHOLDER | P1 |
| Scared | 겁먹음 | 1 | 8 | O | PLACEHOLDER | P0 |
| Guard | 막아서기·방어 자세 | 2 | 3 | O | PLACEHOLDER | P0 |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 1 | 6 | O | PLACEHOLDER | P0 |
| Strike | 내려치기 | 1 | 1 | 1회 | PLACEHOLDER | P0 |
| Toss | 던지기 (약병·상자) | 2 | 7 | 1회 | PLACEHOLDER | P0 |
| Calm | 파이프를 놓고 주저앉음 (진정) | 1 | 3 | O | PLACEHOLDER | P0 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 8 | O | PLACEHOLDER | P1 |

### D. 적·보스 행동과 필요한 그림

- 교섭 거절 시 비살상 충돌: 경계(Guard) → 예고(Windup 0.8초) → 휘두르기(Strike) / 고철 던지기(Toss) → 진정(Calm)
- 쓰러짐·사망 그림 금지 (ART_REQUESTS)

---

## CH_NPC_006 — 생존자 B

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_NPC_006 (코드 artId `survivorB`) |
| 등장 씬 | M19 폐품 시장, M27 수용동, M30 영상실, M39 서버실 |
| 등장 (코드·연출) | Editor/EWRoomsUnderground.cs (방 정의) |
| 스토리상 역할 | 생존자 A와 같음 |
| 구분 | 중립 |
| 기본 외형 | 생존자 A와 구분되는 외형 |
| 의상·장비 | 낡은 옷 |
| 무기 | 파이프·고철 |
| 주요 색 | 연갈색 머리 / 청회색 상의 |
| 신체 비율 | 큰 머리 SD · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | — |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_SURVIVOR_NEUTRAL | 기본 (neutral) | 4 | `c4_gift`, `c4_sv_gift`, `c4_sv_reconciled`, `c4_sv_repair` | PLACEHOLDER | P1 |
| ST_SURVIVOR_SCARED | 공포 (scared) | 4 | `c4_deal_fail`, `c4_sv_fight`, `c7_release_route`, `c7_release_route_v` | PLACEHOLDER | P1 |

생존자·단원 스탠딩은 A/B 공용 한 벌 (`survivor_*`, `member_*`).

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P0 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P1 |
| Sit | 앉기 (벤치·바닥) | 1 | 8 | O | PLACEHOLDER | P1 |
| Scared | 겁먹음 | 1 | 8 | O | PLACEHOLDER | P0 |
| Guard | 막아서기·방어 자세 | 2 | 3 | O | PLACEHOLDER | P0 |
| Windup | 공격 준비 예고 자세 (한눈에 읽혀야 함) | 1 | 6 | O | PLACEHOLDER | P0 |
| Strike | 내려치기 | 1 | 1 | 1회 | PLACEHOLDER | P0 |
| Toss | 던지기 (약병·상자) | 2 | 7 | 1회 | PLACEHOLDER | P0 |
| Calm | 파이프를 놓고 주저앉음 (진정) | 1 | 3 | O | PLACEHOLDER | P0 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 8 | O | PLACEHOLDER | P1 |

---

## CH_NPC_007 — 베스페르 단원 A

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_NPC_007 (코드 artId `memberA`) |
| 등장 씬 | M36 대기실, M39 서버실, M42 승선 대기실 |
| 등장 (코드·연출) | — |
| 스토리상 역할 | 9~10장 대기실·탑승 |
| 구분 | 중립 |
| 기본 외형 | 탑승 예정 단원, 번호표 (ART_REQUESTS) |
| 의상·장비 | 단원복 (추정) |
| 무기 | 없음 |
| 주요 색 | 갈색 머리 / 회색 단원복 + 번호표 |
| 신체 비율 | 큰 머리 SD · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | — |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_MEMBER_NEUTRAL | 기본 (neutral) | 10 | `c9_member1`, `c9_member1_again`, `c9_member2`, `c9_member2_again` 외 6 | PLACEHOLDER | P0 |

생존자·단원 스탠딩은 A/B 공용 한 벌 (`survivor_*`, `member_*`).

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P1 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P1 |
| Sit | 앉기 (벤치·바닥) | 1 | 8 | O | PLACEHOLDER | P1 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 8 | O | PLACEHOLDER | P1 |
| Wait | 줄 서서 기다림 | 2 | 4 | O | PLACEHOLDER | P2 |
| Scared | 겁먹음 | 1 | 8 | O | PLACEHOLDER | P1 |
| Cheer | 환호 | 2 | 4 | O | PLACEHOLDER | P2 |

---

## CH_NPC_008 — 베스페르 단원 B

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_NPC_008 (코드 artId `memberB`) |
| 등장 씬 | M36 대기실, M39 서버실, M42 승선 대기실, M43 캡슐 회랑 |
| 등장 (코드·연출) | — |
| 스토리상 역할 | 단원 A와 같음 |
| 구분 | 중립 |
| 기본 외형 | 단원 A와 구분 |
| 의상·장비 | 단원복 (추정) |
| 무기 | 없음 |
| 주요 색 | 금갈색 머리 / 회색 단원복 + 번호표 |
| 신체 비율 | 큰 머리 SD · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | — |

### B. 전신 스탠딩 (대화 일러스트)

대사 데이터의 표정 지정(`"e"`)으로 실제 쓰이는 것만 제작 대상이다. 파일 이름 = `<인물>_<표정>.png` (`GameConfig.GetPortrait`). 없는 파일은 화면에 아무것도 뜨지 않는다.

| ID | 표정·상태 | 사용 줄 수 | 사용 대사 (예) | 상태 | 우선 |
|---|---|---|---|---|---|
| ST_MEMBER_NEUTRAL | 기본 (neutral) | 10 | `c9_member1`, `c9_member1_again`, `c9_member2`, `c9_member2_again` 외 6 | PLACEHOLDER | P0 |

생존자·단원 스탠딩은 A/B 공용 한 벌 (`survivor_*`, `member_*`).

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P1 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P1 |
| Sit | 앉기 (벤치·바닥) | 1 | 8 | O | PLACEHOLDER | P1 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 8 | O | PLACEHOLDER | P1 |
| Wait | 줄 서서 기다림 | 2 | 4 | O | PLACEHOLDER | P2 |
| Scared | 겁먹음 | 1 | 8 | O | PLACEHOLDER | P1 |
| Cheer | 환호 | 2 | 4 | O | PLACEHOLDER | P2 |

---

## CH_NPC_009 — 회수 경비

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_NPC_009 (코드 artId `warden`) |
| 등장 씬 | M30 영상실 |
| 등장 (코드·연출) | M30 격벽 컷신·영상 (EWRoomsLate.cs:257,271) |
| 스토리상 역할 | 7장 격벽 컷신·영상 |
| 구분 | 적 (컷신 전용) |
| 기본 외형 | 개조 경비병, 말이 거의 없음 (GDD 조연) |
| 의상·장비 | 무장 (추정) |
| 무기 | 총기 (컷신에서 총성 — 추정) |
| 주요 색 | 검은 머리 / 어두운 복장 |
| 신체 비율 | 큰 머리 SD · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | 플레이 중 전투 대상 아님 |

### B. 전신 스탠딩 (대화 일러스트)

스탠딩 없음 — 이 인물은 대사가 없거나 이름표만 쓴다 (대사 2줄). 회상 인물은 스탠딩을 쓰지 않는 쪽이 기존 권장 (ART_REQUESTS). 추가는 OPTIONAL.

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P1 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P1 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P1 |
| Sit | 앉기 (벤치·바닥) | 1 | 8 | O | PLACEHOLDER | P1 |
| Down | 쓰러져 누운 자세 (유지) | 1 | 8 | O | PLACEHOLDER | P1 |
| Guard | 막아서기·방어 자세 | 2 | 3 | O | PLACEHOLDER | P1 |
| Strike | 내려치기 | 1 | 1 | 1회 | PLACEHOLDER | P1 |

---

## CH_NPC_010 — 린든

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_NPC_010 (코드 artId `linden`) |
| 등장 씬 | 씬 배치 없음 (코드가 생성 — 다음 줄) |
| 등장 (코드·연출) | M25 회상 컷신 |
| 스토리상 역할 | 6장 회상 (베넬의 동생) |
| 구분 | 회상 |
| 기본 외형 | 아이. 현재 임시는 0.72배 축소 → 아이 비율로 (ART_REQUESTS) |
| 의상·장비 | 베넬 옷과 같은 캐릭터 옷 (추정) |
| 무기 | 없음 |
| 주요 색 | 보라 머리 / 주황 상의 (세피아) |
| 신체 비율 | 아이 비율 · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | — |

### B. 전신 스탠딩 (대화 일러스트)

스탠딩 없음 — 이 인물은 대사가 없거나 이름표만 쓴다 (대사 0줄). 회상 인물은 스탠딩을 쓰지 않는 쪽이 기존 권장 (ART_REQUESTS). 추가는 OPTIONAL.

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 2 | 3 | O | PLACEHOLDER | P2 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P2 |
| Hurt | 피격 경직 | 1 | 8 | O | PLACEHOLDER | P2 |
| Sit | 앉기 (벤치·바닥) | 1 | 8 | O | PLACEHOLDER | P2 |
| Hold | 들고 있기 | 1 | 1 | O | PLACEHOLDER | P2 |
| Wave | 손 흔들기 | 2 | 4 | O | OPTIONAL | P3 |

---

## CH_NPC_011 — 어른 (회상)

### A. 기본 디자인

| 항목 | 내용 |
|---|---|
| 캐릭터 ID | CH_NPC_011 (코드 artId `adult`) |
| 등장 씬 | 씬 배치 없음 (코드가 생성 — 다음 줄) |
| 등장 (코드·연출) | M25 회상 컷신 (추정: 회상 장면 다섯 중) |
| 스토리상 역할 | 6장 회상: 목발 짚은 아빠 실루엣 |
| 구분 | 회상 |
| 기본 외형 | 얼굴 없는 어두운 실루엣이면 충분 (ART_REQUESTS) |
| 의상·장비 | — |
| 무기 | 목발 |
| 주요 색 | 검은 실루엣 |
| 신체 비율 | 어른 · 캔버스 48×64 |
| 현재 이미지 | `Tools/*_gen.py` 로 만든 임시 도트 (몸에 이름 글자) |
| 제작 필요 | 예 — 최종 그림 없음 |
| 비고 | — |

### B. 전신 스탠딩 (대화 일러스트)

스탠딩 없음 — 이 인물은 대사가 없거나 이름표만 쓴다 (대사 0줄). 회상 인물은 스탠딩을 쓰지 않는 쪽이 기존 권장 (ART_REQUESTS). 추가는 OPTIONAL.

### C. 게임 플레이 스프라이트 (요약 — 상세는 ANIMATION_CATALOG)

| 애니메이션 | 설명 | 현재 프레임 | FPS | 루프 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| Idle | 대기 (숨쉬기) | 1 | 3 | O | PLACEHOLDER | P2 |
| Walk | 걷기 | 4 | 8 | O | PLACEHOLDER | P2 |
| Crutch | 목발 짚고 걷기 | 1 | 1 | O | PLACEHOLDER | P2 |

