# IMAGE GENERATION GUIDE — 이미지 생성 AI 제작 가이드

> 2026-11-17 · 현재 구현 기준. 이 문서는 **제작 지시서**다. 이번 작업에서 이미지를 생성하지 않았다. 외형 설명은 GDD 2장·Docs/ART_REQUESTS.md·현재 임시 그림 색에서 옮긴 것으로, **사용자의 최종 디자인이 오면 그것이 우선**한다.

## 1. 먼저 알아야 할 것

- 게임 화면은 **640×360 도트**를 정수 배율로 키운다 (PPU 32). 인물 한 프레임은 **48×64px** (괴물 토끼 80×96, 아우렐리우스 64×96). 그림 안 인물의 실제 키는 약 40~48px.
- 이미지 생성 AI 는 정확한 픽셀 격자·고정 크기·프레임 간 일관성을 잘 지키지 못한다. 2026-11-15 실제 시도에서도 격자와 치수가 맞지 않아 참고용으로만 보관했다 (Docs/PIXEL_RESTORE_REPORT.md).
- 그래서 권장 순서: **(1) 캐릭터 설정화 확정 → (2) 동작별 키 포즈를 한 장씩 큰 해상도로 생성 → (3) 최근접 축소 → (4) 팔레트 정리 → (5) 사람 손으로 도트 정리 → (6) 스크립트로 가로 한 줄 시트 조립**.
- 스탠딩(대화 일러스트)은 도트가 아니다. 고해상도 일러스트로 그대로 쓴다 (600×900, 부드러운 필터).

## 2. 공통 스타일 고정 (모든 도트 그림)

| 항목 | 지시 |
|---|---|
| 화풍 | 16비트풍 사이드뷰 도트. 큰 머리 SD 비율 (머리 ≈ 키의 40%), 디테일은 머리·상체에 모으고 다리는 짧게 (사용자 참고 이미지 `Docs/References/캐릭터 디자인 예시.png`) |
| 외곽선 | 1px 어두운 외곽선, 밝은 쪽 테두리 하이라이트 |
| 음영 | 평면 2~3단, 디더링 최소 |
| 팔레트 | 인물당 16~24색. 시설: 어두운 청회색·탁한 녹색. 진행 대상(엄폐 상자·출구·전원·조사 대상)은 대비 높게, 장식은 낮게. **붉은색은 경보 전용**. 산성 = 녹색, 전기 = 청백색, 출구 색과 겹치지 않게 |
| 방향 | 인물은 **오른쪽을 봄** (왼쪽은 코드가 반전). 좌우 비대칭 장식은 피하거나 왼쪽용 별도 시트 |
| 배경 | 인물·소품·이펙트는 **투명 배경** PNG. 생성 단계에서는 단색 크로마(#FF00FF) 배경으로 만들고 후처리에서 투명화 권장 |
| 금지 | 글자·워터마크·서명, 실제 운영체제 화면 흉내, 사실적 회화 질감과 도트 혼합, 신체 해부 묘사(도식은 선과 번호만, 유리 너머는 실루엣), 베스페르 로고를 임의로 만들기 (사용자 원안 필요, GDD 1.3) |
| 참고 | 실험실 소품 밀도: `Docs/References/에셋 픽셀 레퍼런스.png` (구도·밀도 참고만, 복제 금지) |

### 공통 프롬프트 접두어 (도트 인물)

```
16-bit pixel art sprite, side view facing right, chibi SD proportions with a big head (head about 40% of total height), short legs,
1px dark outline, flat 2-3 tone shading, limited palette (max 24 colors), clean readable silhouette, full body, single character, centered,
solid magenta #FF00FF background, no text, no watermark, no shadow on background
```

부정 프롬프트: `blurry, anti-aliasing, gradient background, realistic painting, 3d render, text, watermark, multiple characters, cropped feet, extra limbs`

## 3. 후처리 (모든 도트 그림 공통)

1. 생성 크기: 프레임의 **8배** (48×64 → 384×512, 80×96 → 640×768, 64×96 → 512×768, 소품은 실제 크기의 8배).
2. 크로마 제거 → 알파 0/255 로만 (반투명 금지, 경광등 빔 `S_AlarmBeam` 등 가산 그림만 예외).
3. **최근접(Nearest) 축소**로 실제 크기 → 팔레트 양자화 (인물 24색 이하) → 1px 외곽선·눈·손 정리 (사람 수정 필수).
4. 발바닥을 프레임 **아래 끝**에 맞추고 몸 중심을 프레임 **가로 가운데**에 (피벗 = 아래 가운데). 프레임마다 발 위치가 흔들리지 않게.
5. 동작 프레임을 **가로 한 줄**로 이어 붙인다 (여백 0, 행 1). 시트 폭 = 프레임 폭 × 프레임 수.
6. 파일 이름은 기존과 **똑같이** (예: `Lu_Walk.png`). 규격 검사 후 ArtDrop 으로 납품 (ASSET_IMPLEMENTATION_MAPPING.md).

### 개별 생성 vs 한 장 시트

| 대상 | 권장 | 이유 |
|---|---|---|
| 캐릭터 설정화 (정면·옆·뒤 3면) | 한 장 | 외형 고정용. 게임에 넣지 않음 |
| 동작 프레임 | **프레임마다 개별 생성** (같은 설정화를 참조 이미지로) | 한 장 시트 생성은 프레임 크기·발 위치·외형이 흔들림 |
| 반복 동작 (걷기 4~8) | 키 포즈 2장 생성 + 사이 프레임은 사람이 그림 | 일관성 |
| 스탠딩 표정 | 표정마다 개별, 같은 기본 포즈를 참조로 | 표정만 바뀌게 |
| 배경 레이어 | 레이어마다 개별 | 패럴랙스 분리 필수 (통짜 배경 금지) |
| 소품·타일 | 개별 | 크기가 모두 다름 |
| 이펙트 프레임 | 개별 생성 후 조립, 또는 사람이 직접 | 4~6프레임 작은 그림은 AI 보다 수작업이 빠를 수 있음 (추정) |

## 4. Unity 적용 (모든 종류 공통)

1. 같은 이름으로 대상 경로에 복사 (ArtDrop → 개발 쪽 검사·복사).
2. Unity 메뉴 `EW > 1. 전체 구성` 실행 → 가져오기 설정(Point·무압축·PPU 32)·시트 분할·클립 재생성·GameConfig 등록·씬 재생성이 자동.
3. 확인: Esc → 검수 도구로 해당 맵 이동해 화면 확인. 스탠딩은 해당 대사 장면에서 확인.

## 5. 캐릭터별 지시

### CH_PLAYER_001 루

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Lu_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 금발, 십대 후반 외형, 전기전자 개조. 밝고 명랑한 표정이 기본 (GDD 2장) |
| 의상·장비·무기 | 현재 임시 그림: 파란 상의·남색 바지. 확정된 의상 설정 없음 (추정: 실험체로 붙잡혔다 탈출하므로 소지품 없음) / 없음 (손끝 전기) |
| 신체 비율·크기 | 큰 머리 SD (머리 ≈ 키의 40%) · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | 좌우 비대칭 요소가 생기면 왼쪽용 컨트롤러 필요 (Art_lu.asset leftController) |

```
[공통 접두어] + a cheerful teenage boy-like character with short messy blond hair, big bright eyes, blue jacket, dark navy pants, small electric sparks at fingertips
```

동작별 포즈 (상태 이름 → 파일 `Lu_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) **[P0]** — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) **[P0]** — 걷기
- `Jump` (1프레임 · 8fps · 반복) **[P0]** — 점프 상승
- `Fall` (1프레임 · 8fps · 반복) **[P0]** — 낙하
- `Climb` (2프레임 · 6fps · 반복) **[P0]** — 사다리 오르기
- `Hurt` (1프레임 · 8fps · 반복) **[P0]** — 피격 경직
- `Down` (1프레임 · 8fps · 반복) **[P0]** — 쓰러져 누운 자세 (유지)
- `Sit` (1프레임 · 8fps · 반복) — 앉기 (벤치·바닥)
- `Flick` (3프레임 · 9fps · 1회) **[P0]** — 톡! — 검지를 튕겨 짧은 불꽃 (2번째 프레임이 판정 순간)
- `Point` (3프레임 · 9fps · 1회) **[P0]** — 겨누기 (스파크 샷·간섭)
- `Swing` (3프레임 · 9fps · 1회) — 휘두르기 (현재 근접 연출용)
- `Kick` (3프레임 · 9fps · 1회) — 발차기 (연출용)
- `Charge` (2프레임 · 6fps · 반복) **[P0]** — 외부 전력 끌어오기 — 손이 전원 쪽으로
- `Discharge` (2프레임 · 10fps · 반복) — 방전 (몸에서 전기 폭발)
- `Carry` (4프레임 · 5fps · 반복) — 부축하기 (베넬)
- `BackIdle` (2프레임 · 2fps · 반복) — 뒷모습 대기 (불타는 마을 첫 장면)
- `BackLook` (1프레임 · 8fps · 반복) — 뒷모습에서 고개 돌림
- `Limp` (6프레임 · 6fps · 반복) — 절뚝이는 이동
- `LimpIdle` (2프레임 · 2fps · 반복) — 절뚝이다 멈춰 숨 고르기
- `Sad` (2프레임 · 2fps · 반복) — 고개 숙임 (슬픔)
- `ReachPC` (2프레임 · 4fps · 1회) — 컴퓨터에 손 뻗기
- `Connect` (3프레임 · 6fps · 반복) — 접속 — 빛이 몸에 번짐, 머리카락 들림
- `Land` (1프레임 · 8fps · 반복) — 착지 충격
- `GetUp` (3프레임 · 6fps · 1회) — 몸 일으키기
- `LookAround` (4프레임 · 3fps · 반복) — 좌우 살피기
- `Knock` (3프레임 · 10fps · 반복) — 유리 두드리기
- `BackAway` (4프레임 · 6fps · 반복) — 겁먹고 뒷걸음
- `Fear` (2프레임 · 8fps · 반복) — 겁먹고 떨기
- `ChargeEyes` (3프레임 · 3fps · 1회) — 눈 질끈 감고 방전 준비
- `Recover` (2프레임 · 2fps · 반복) — 방전 후 숨 몰아쉬기
- `Squeeze` (4프레임 · 6fps · 반복) — 문틈 비집고 통과
- `Crouch` (2프레임 · 2fps · 반복) **[P0]** — 웅크려 숨기 — 보이는 키 30px(0.95유닛) 이하
- `CrouchWalk` (4프레임 · 6fps · 반복) **[P0]** — 웅크려 걷기 — 같은 높이 제한
- `Dodge` (1프레임 · 1fps · 반복) **[P0]** — 대시 (낮게 미끄러지듯)
- `WallLean` (2프레임 · 3fps · 반복) — 벽 짚고 숨 고르기
- `TankCrouch` (2프레임 · 2fps · 반복) — 실험 통 안 웅크림
- `TankStruggle` (2프레임 · 7fps · 반복) — 통 안에서 몸부림
- `TankCower` (2프레임 · 2fps · 반복) — 통 안에서 움츠림
- `EyesClosed` (1프레임 · 2fps · 반복) — 눈 질끈 감음
- `Electrify` (2프레임 · 12fps · 반복) — 전기화 직전 — 윤곽 발광
- `Disgust` (2프레임 · 3fps · 반복) — 입을 가리고 몸을 숙임
- `HeadHold` (2프레임 · 3fps · 반복) — 머리를 감싸 쥠
- `Lifted` (4프레임 · 7fps · 반복) — 목덜미를 잡혀 들림 — 목덜미 = 프레임 (24,33)px
- `Thrown` (1프레임 · 1fps · 반복) — 던져짐 (몸 웅크림, 회전은 코드)
- `HoldHand` (1프레임 · 1fps · 반복) — 동료 손 잡고 전원선으로
- `HairUp` (2프레임 · 3fps · 반복) — 펄스 뒤 머리가 곤두섬 + 작은 불꽃
- `Share` (1프레임 · 1fps · 반복) — 과자를 반 나눠 내밂
- `HandsUp` (2프레임 · 4fps · 반복) — 손 들기 (폐기소 첫 만남)
- `Wind` (2프레임 · 3fps · 반복) — 옥상 바람 맞기
- `Cold` (2프레임 · 4fps · 반복) — 최종전 낮고 차가운 자세 (추정)
- `Limit` (2프레임 · 4fps · 반복) — 한계 방출 — 양손으로 전류를 붙잡음
- `Lie` (1프레임 · 2fps · 반복) — 누워 있음 (중정)
- `Kneel` (1프레임 · 3fps · 1회) — 무릎 꿇기
- `HandHold` (1프레임 · 2fps · 반복) — 손 잡고 있기 (중정)
- `Release` (1프레임 · 4fps · 반복) — 남은 전기를 놓음 (마지막)

### CH_BOSS_001 베넬 (본명 아이비)

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Venelle_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 보라 머리 (현재 임시), 정신이 온전치 않은 다정·멍함·공격성 |
| 의상·장비·무기 | 동생이 좋아하던 캐릭터 옷 (코스프레처럼 보임). 팔에 장착하는 약품 혼합 장치: 약품 탱크→관→손목 약병 (GDD 2장, ART_REQUESTS) / 팔 장치 (산성 분사·약병 투척) |
| 신체 비율·크기 | 큰 머리 SD · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | 같은 옷 두 벌 소품 U_TwinOutfits 와 옷 색이 같아야 함 |

```
[공통 접두어] + a young woman with purple hair, gentle but unstable expression, maroon cosplay-like outfit, a mechanical chemical mixer device strapped to one arm (tank -> tube -> wrist vial) glowing acid green
```

동작별 포즈 (상태 이름 → 파일 `Venelle_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) **[P0]** — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) **[P0]** — 걷기
- `Jump` (1프레임 · 8fps · 반복) — 점프 상승
- `Fall` (1프레임 · 8fps · 반복) — 낙하
- `Climb` (2프레임 · 6fps · 반복) — 사다리 오르기
- `Hurt` (1프레임 · 8fps · 반복) **[P0]** — 피격 경직
- `Down` (1프레임 · 8fps · 반복) — 쓰러져 누운 자세 (유지)
- `Sit` (1프레임 · 8fps · 반복) — 앉기 (벤치·바닥)
- `Attack` (3프레임 · 9fps · 1회) **[P0]** — 휘두르기·약병 던지기
- `Collapsed` (1프레임 · 8fps · 반복) — 쓰러짐 (개그 장면)
- `Supported` (4프레임 · 5fps · 반복) — 부축받으며 걷기
- `Dizzy` (2프레임 · 3fps · 반복) — 어지러움
- `Stalk` (5프레임 · 5fps · 반복) **[P0]** — 불균형하고 느슨한 걸음, 고개 기울임
- `Windup` (2프레임 · 6fps · 반복) **[P0]** — 공격 준비 예고 자세 (한눈에 읽혀야 함)
- `Dash` (1프레임 · 1fps · 반복) **[P0]** — 돌진
- `Pant` (2프레임 · 3fps · 반복) **[P0]** — 숨 고르기 (말 걸기 기회)
- `Laugh` (2프레임 · 3fps · 반복) — 혼자 웃음
- `Grab` (2프레임 · 3fps · 반복) **[P0]** — 잡기 — 손 = 프레임 (37,18)px (VenelleBoss.GrabHand 와 일치)
- `Throw` (2프레임 · 7fps · 1회) — 집어 던짐
- `Spray` (2프레임 · 8fps · 반복) **[P0]** — 산성 분사 준비
- `Mix` (3프레임 · 6fps · 반복) **[P0]** — 재혼합 (거품, 빈틈임이 읽히게)
- `Toss` (2프레임 · 7fps · 1회) **[P0]** — 던지기 (약병·상자)
- `Downed` (1프레임 · 2fps · 반복) **[P0]** — 체력 0 — 무릎 꿇고 숨 몰아쉼, 관에서 약품이 샘
- `TalkWalk` (4프레임 · 6fps · 반복) — 투덜거리며 걸어옴 (등장)
- `Comb` (2프레임 · 3fps · 1회) — 빗을 꺼냈다 금속 빗을 보고 넣음
- `Wind` (2프레임 · 3fps · 반복) — 옥상 바람 맞기
- `SitQuiet` (2프레임 · 3fps · 반복) — 침대에 앉아 말을 잃음
- `Share` (1프레임 · 1fps · 반복) — 과자를 반 나눠 내밂

### CH_BOSS_001-P 아이비 (베넬의 과거)

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Ivy_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 베넬과 같은 얼굴, 팔 장치 없음 (ART_REQUESTS) |
| 의상·장비·무기 | 평상복 (추정) / 없음 |
| 신체 비율·크기 | 큰 머리 SD · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | 같은 인물의 과거 모습 |

```
[공통 접두어] + the same woman as Venelle before modification, purple long hair, plain clothes, NO arm device (rendered later in sepia)
```

동작별 포즈 (상태 이름 → 파일 `Ivy_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Hurt` (1프레임 · 8fps · 반복) — 피격 경직
- `Sit` (1프레임 · 8fps · 반복) — 앉기 (벤치·바닥)
- `Carry` (4프레임 · 5fps · 반복) — 부축하기 (베넬)
- `Kneel` (1프레임 · 3fps · 1회) — 무릎 꿇기
- `Give` (1프레임 · 3fps · 1회) — 건네기
- `Wave` (2프레임 · 4fps · 반복) — 손 흔들기

### CH_ALLY_001 라비엘

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Raviel_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 귀 없는 토끼 악마. 연구주임 가운 (ART_REQUESTS). 차분하고 약간 비꼬는 얼굴 |
| 의상·장비·무기 | 연구주임 가운 / 없음 (변신) |
| 신체 비율·크기 | 큰 머리 SD · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | 변신형은 별도 시트 (ravielBeast) |

```
[공통 접두어] + a calm, slightly sarcastic young man, rabbit demon WITHOUT ears, white hair, long white researcher lab coat
```

동작별 포즈 (상태 이름 → 파일 `Raviel_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Hurt` (1프레임 · 8fps · 반복) — 피격 경직
- `Sit` (1프레임 · 8fps · 반복) — 앉기 (벤치·바닥)
- `Down` (1프레임 · 8fps · 반복) — 쓰러져 누운 자세 (유지)
- `Talk` (2프레임 · 4fps · 반복) — 말하기 손짓
- `Point` (1프레임 · 9fps · 1회) — 겨누기 (스파크 샷·간섭)
- `Type` (2프레임 · 6fps · 반복) — 단말 입력
- `HoldHand` (1프레임 · 1fps · 반복) — 동료 손 잡고 전원선으로
- `Catch` (1프레임 · 6fps · 1회) — 붙잡기 (휴지통 탈출 뒤)
- `Injured` (2프레임 · 4fps · 반복) — 부상 상태로 서 있기·따라오기
- `Cup` (1프레임 · 2fps · 반복) — 컵을 양손으로 잡기
- `Sad` (1프레임 · 2fps · 반복) — 고개 숙임 (슬픔)

### CH_ALLY_001-B 라비엘 (괴물 토끼)

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/RavielBeast_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 온몸이 근육질인 약 2m 거대 토끼 (GDD 2장) |
| 의상·장비·무기 | 없음 / 앞발·도약 |
| 신체 비율·크기 | 거대 체형 (캔버스 80×96) · 프레임 80×96, 실제 키 약 59~72px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 640×768/프레임 → 최종 80×96/프레임, 가로 한 줄 시트 |
| 비고 | 같은 인물의 다른 형태 — 별도 캐릭터 아님 |

```
[공통 접두어] + a huge muscular monster rabbit about 2 meters tall, pale grey-white fur, no long ears or very short ones (keep consistent with human form), heavy forepaws
```

동작별 포즈 (상태 이름 → 파일 `RavielBeast_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) **[P0]** — 대기 (숨쉬기)
- `Walk` (4프레임 · 7fps · 반복) **[P0]** — 걷기
- `Windup` (2프레임 · 5fps · 반복) **[P0]** — 공격 준비 예고 자세 (한눈에 읽혀야 함)
- `Leap` (1프레임 · 3fps · 1회) **[P0]** — 도약
- `Slam` (1프레임 · 3fps · 1회) **[P0]** — 내려찍기
- `Swipe` (1프레임 · 3fps · 1회) **[P0]** — 앞발 휘두르기
- `Stun` (2프레임 · 5fps · 반복) **[P0]** — 빈틈 (경직)
- `Hurt` (1프레임 · 3fps · 1회) **[P0]** — 피격 경직
- `Hold` (1프레임 · 3fps · 반복) — 들고 있기
- `Down` (1프레임 · 3fps · 반복) — 쓰러져 누운 자세 (유지)

### CH_BOSS_002 메이린

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Meilin_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 중국식 도자기 인형, 얼굴 반쪽이 기워진 듯, 긴 땋은 머리, 바늘꽂이 머리장식 (GDD 2장) |
| 의상·장비·무기 | 치파오, 발바닥 파츠가 롤러스케이트 / 섬세한 공예용 가위 |
| 신체 비율·크기 | 큰 머리 SD (인형 관절 추정) · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | — |

```
[공통 접두어] + a porcelain doll-like girl, half of the face looks stitched, long black braided hair with a pincushion hair ornament with needles, red qipao dress, roller-skate feet, small craft scissors
```

동작별 포즈 (상태 이름 → 파일 `Meilin_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) **[P0]** — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Roll` (2프레임 · 6fps · 반복) **[P0]** — 롤러 이동
- `Windup` (1프레임 · 6fps · 반복) **[P0]** — 공격 준비 예고 자세 (한눈에 읽혀야 함)
- `Snip` (1프레임 · 6fps · 1회) **[P0]** — 가위 공격
- `Dash` (1프레임 · 1fps · 반복) **[P0]** — 돌진
- `Stun` (2프레임 · 4fps · 반복) **[P0]** — 빈틈 (경직)
- `Hurt` (1프레임 · 8fps · 반복) **[P0]** — 피격 경직
- `Talk` (2프레임 · 4fps · 반복) — 말하기 손짓
- `Angry` (2프레임 · 4fps · 반복) — 화냄
- `Pull` (2프레임 · 4fps · 반복) — 캡슐 손잡이 당기기
- `Down` (1프레임 · 8fps · 반복) — 쓰러져 누운 자세 (유지)

### CH_BOSS_003 라크리모사 (본명 에드워드)

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Lacrimosa_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 오페라 가수 같은 공연복, 목·흉곽 음향 증폭 장치 (GDD 2장) |
| 의상·장비·무기 | 공연복 / 목소리 (울림파) |
| 신체 비율·크기 | 큰 머리 SD · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | — |

```
[공통 접두어] + an elegant opera singer man in a dark navy performance suit, sound amplifier device built into the throat and chest
```

동작별 포즈 (상태 이름 → 파일 `Lacrimosa_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) **[P0]** — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Sing` (2프레임 · 4fps · 반복) **[P0]** — 노래 (울림파 발동)
- `Windup` (1프레임 · 6fps · 반복) **[P0]** — 공격 준비 예고 자세 (한눈에 읽혀야 함)
- `Stagger` (2프레임 · 4fps · 반복) **[P0]** — 보호 정지 휘청임
- `Listen` (1프레임 · 2fps · 반복) **[P0]** — 녹음을 듣고 멈춤
- `Hurt` (1프레임 · 8fps · 반복) **[P0]** — 피격 경직
- `Bow` (1프레임 · 2fps · 반복) — 인사
- `Down` (1프레임 · 8fps · 반복) — 쓰러져 누운 자세 (유지)

### CH_BOSS_003-P 에드워드 (라크리모사의 과거)

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Edward_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 극장 음향 일을 하던 성인 (GDD 2장) |
| 의상·장비·무기 | 작업복 (추정) / 없음 |
| 신체 비율·크기 | 큰 머리 SD · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | 같은 인물의 과거 모습 |

```
[공통 접두어] + a young man with brown hair in dark work clothes, working in a theater sound booth
```

동작별 포즈 (상태 이름 → 파일 `Edward_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) — 대기 (숨쉬기)
- `Write` (2프레임 · 2fps · 반복) — 녹음 파일 이름 적기

### CH_BOSS_003-K 어린 에드워드

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/EdwardKid_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 아이 |
| 의상·장비·무기 | — / 없음 |
| 신체 비율·크기 | 아이 비율 · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | — |

```
[공통 접두어] + a small poor child in grey clothes, quietly listening
```

동작별 포즈 (상태 이름 → 파일 `EdwardKid_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) — 대기 (숨쉬기)
- `Hide` (1프레임 · 3fps · 반복) — 숨기
- `Listen` (2프레임 · 2fps · 반복) — 녹음을 듣고 멈춤

### CH_BOSS_004 아우렐리우스

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Aurelius_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 기괴한 내부 기계를 덮는 아름다운 외피. 깨지면 내부 기계가 드러남 (GDD 2장, ART_REQUESTS) |
| 의상·장비·무기 | 외피 (긴 금발·긴 의복, 현재 임시) / 빛·연결 광선·호위 구 |
| 신체 비율·크기 | 큰 체형 (캔버스 64×96) · 프레임 64×96, 실제 키 약 59~72px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 512×768/프레임 → 최종 64×96/프레임, 가로 한 줄 시트 |
| 비고 | Broken·Ruined 상태로 외피 파손 단계를 표현 |

```
[공통 접두어] + a tall beautiful figure with long blond hair and a long navy robe with gold lines; the beautiful outer shell hides grotesque machinery inside
```

동작별 포즈 (상태 이름 → 파일 `Aurelius_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) **[P0]** — 대기 (숨쉬기)
- `Talk` (2프레임 · 3fps · 반복) — 말하기 손짓
- `Windup` (2프레임 · 6fps · 반복) **[P0]** — 공격 준비 예고 자세 (한눈에 읽혀야 함)
- `Cast` (1프레임 · 3fps · 1회) **[P0]** — 빛·광선 발사
- `Hurt` (1프레임 · 3fps · 1회) **[P0]** — 피격 경직
- `Broken` (2프레임 · 3fps · 반복) — 외피 일부 파손
- `Ruined` (2프레임 · 3fps · 반복) — 외피 크게 파손, 내부 기계 노출
- `Down` (1프레임 · 3fps · 1회) — 쓰러져 누운 자세 (유지)

### CH_ENEMY_001 순찰 경비

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD 5장 일반 적 표 · ART_REQUESTS §5 · 현재 대용 Foozle 그림은 **참고 금지** (다른 용도의 로봇) |
| 반드시 유지할 외형 | 사람보다 약간 작은 기계, 제압봉 (ART_REQUESTS). 현재 Foozle Droid03 대용 |
| 의상·장비·무기 | — / 제압봉 |
| 신체 비율·크기 | 기계 · 프레임 48×48, 실제 키 약 29~36px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×384/프레임 → 최종 48×48/프레임, 가로 한 줄 시트 |
| 비고 | 공격 준비 자세(제압봉 치켜듦) 프레임 필요 — 지금은 깜빡임으로만 예고 |

```
[공통 접두어] + a security robot slightly shorter than a human, holding a stun baton, sturdy lab-facility design
```

동작별 포즈 (상태 이름 → 파일 `Droid03_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (4프레임 · 8fps · 반복) **[P0]** — 대기 (숨쉬기)
- `Move` (8프레임 · 10fps · 반복) **[P0]** — 이동
- `Attack` (9프레임 · 14fps · 1회) **[P0]** — 휘두르기·약병 던지기
- `Hurt` (4프레임 · 10fps · 1회) **[P0]** — 피격 경직
- `Death` (11프레임 · 12fps · 1회) **[P0]** — 파괴

### CH_ENEMY_002 포획 드론

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD 5장 일반 적 표 · ART_REQUESTS §5 · 현재 대용 Foozle 그림은 **참고 금지** (다른 용도의 로봇) |
| 반드시 유지할 외형 | 아래를 비추는 렌즈가 보이는 비행 드론 (ART_REQUESTS). 현재 Foozle Droid01 대용 |
| 의상·장비·무기 | — / 그물 (GDD) |
| 신체 비율·크기 | 기계 (비행, 1유닛 떠 있음) · 프레임 48×48, 실제 키 약 29~36px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×384/프레임 → 최종 48×48/프레임, 가로 한 줄 시트 |
| 비고 | — |

```
[공통 접두어] + a small flying capture drone with a downward-facing lens and a net launcher
```

동작별 포즈 (상태 이름 → 파일 `Droid01_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (4프레임 · 8fps · 반복) **[P0]** — 대기 (숨쉬기)
- `Move` (4프레임 · 10fps · 반복) **[P0]** — 이동
- `Attack` (12프레임 · 14fps · 1회) **[P0]** — 휘두르기·약병 던지기
- `Hurt` (3프레임 · 10fps · 1회) **[P0]** — 피격 경직
- `Death` (9프레임 · 12fps · 1회) **[P0]** — 파괴

### CH_ENEMY_003 운반 로봇

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD 5장 일반 적 표 · ART_REQUESTS §5 · 현재 대용 Foozle 그림은 **참고 금지** (다른 용도의 로봇) |
| 반드시 유지할 외형 | 짐을 나르는 로봇. 현재 Foozle Droid02 대용 |
| 의상·장비·무기 | — / 짐 던지기 (GDD) |
| 신체 비율·크기 | 기계 (0.85유닛 떠 있음) · 프레임 48×48, 실제 키 약 29~36px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×384/프레임 → 최종 48×48/프레임, 가로 한 줄 시트 |
| 비고 | M47 회수 장치도 이 그림을 쓰고 있음 → 전용 그림 필요 |

```
[공통 접두어] + a hovering cargo carrier robot
```

동작별 포즈 (상태 이름 → 파일 `Droid02_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (4프레임 · 8fps · 반복) **[P0]** — 대기 (숨쉬기)
- `Move` (4프레임 · 10fps · 반복) **[P0]** — 이동
- `Attack` (7프레임 · 14fps · 1회) **[P0]** — 휘두르기·약병 던지기
- `Hurt` (4프레임 · 10fps · 1회) **[P0]** — 피격 경직
- `Death` (13프레임 · 12fps · 1회) **[P0]** — 파괴

### CH_NPC_001 연구원 1

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Researcher_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 베스페르 하급 연구원. 지친 사람 (GDD 2장 조연) |
| 의상·장비·무기 | 연구 가운 (추정) / 제압봉 |
| 신체 비율·크기 | 큰 머리 SD · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | 연구원 2와 머리 모양·체형·색으로 구분 (ART_REQUESTS) |

```
[공통 접두어] + a tired low-rank researcher in a white lab coat, brown hair, holding a stun baton
```

동작별 포즈 (상태 이름 → 파일 `Researcher_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Hurt` (1프레임 · 8fps · 반복) — 피격 경직
- `Down` (1프레임 · 8fps · 반복) — 쓰러져 누운 자세 (유지)
- `Scared` (1프레임 · 8fps · 반복) — 겁먹음
- `Talk` (2프레임 · 4fps · 반복) — 말하기 손짓
- `BatonOut` (3프레임 · 6fps · 1회) — 제압봉 꺼내기
- `BatonIdle` (2프레임 · 4fps · 반복) — 제압봉 든 대기 (끝이 지지직)
- `BatonWalk` (4프레임 · 8fps · 반복) — 제압봉 든 걷기

### CH_NPC_002 연구원 2

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Researcher2_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 연구원 1과 구분되는 체형·머리 |
| 의상·장비·무기 | 연구 가운 (추정) / 제압봉 |
| 신체 비율·크기 | 큰 머리 SD · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | — |

```
[공통 접두어] + a different tired researcher, black hair, white lab coat, green pants, stun baton
```

동작별 포즈 (상태 이름 → 파일 `Researcher2_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Hurt` (1프레임 · 8fps · 반복) — 피격 경직
- `Down` (1프레임 · 8fps · 반복) — 쓰러져 누운 자세 (유지)
- `Scared` (1프레임 · 8fps · 반복) — 겁먹음
- `Talk` (2프레임 · 4fps · 반복) — 말하기 손짓
- `BatonOut` (3프레임 · 6fps · 1회) — 제압봉 꺼내기
- `BatonIdle` (2프레임 · 4fps · 반복) — 제압봉 든 대기 (끝이 지지직)
- `BatonWalk` (4프레임 · 8fps · 반복) — 제압봉 든 걷기

### CH_NPC_003 F

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/F_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 짧고 투박한 할머니, 목에 번호표(F) (ART_REQUESTS) |
| 의상·장비·무기 | 낡은 옷 (추정) / 없음 |
| 신체 비율·크기 | 큰 머리 SD, 작고 투박 · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | — |

```
[공통 접두어] + a short, rough old woman with grey hair, worn brown clothes, a number tag with the letter F around her neck
```

동작별 포즈 (상태 이름 → 파일 `F_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Hurt` (1프레임 · 8fps · 반복) — 피격 경직
- `Sit` (1프레임 · 8fps · 반복) — 앉기 (벤치·바닥)
- `Talk` (2프레임 · 4fps · 반복) — 말하기 손짓
- `Point` (1프레임 · 9fps · 1회) — 겨누기 (스파크 샷·간섭)
- `Toss` (2프레임 · 7fps · 1회) — 던지기 (약병·상자)
- `Guard` (1프레임 · 3fps · 반복) — 막아서기·방어 자세
- `Tag` (1프레임 · 1fps · 반복) — 번호표 들여다보기

### CH_NPC_004 008

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Kid008_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 아이 (번호 8). 현재 임시는 어른 그림 0.78배 축소 → 아이 비율로 새로 (ART_REQUESTS) |
| 의상·장비·무기 | 낡은 옷, 번호표 (추정) / 없음 |
| 신체 비율·크기 | 아이 비율 · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | — |

```
[공통 접두어] + a small child with brown hair, worn olive clothes, a number tag 8 (child proportions)
```

동작별 포즈 (상태 이름 → 파일 `Kid008_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Hurt` (1프레임 · 8fps · 반복) — 피격 경직
- `Sit` (1프레임 · 8fps · 반복) — 앉기 (벤치·바닥)
- `Talk` (2프레임 · 4fps · 반복) — 말하기 손짓
- `Hide` (1프레임 · 3fps · 반복) — 숨기
- `Eat` (2프레임 · 3fps · 반복) — 먹기
- `Hold` (1프레임 · 1fps · 반복) — 들고 있기
- `Draw` (2프레임 · 3fps · 반복) — 쪼그려 바닥에 선 긋기

### CH_NPC_005 생존자 A

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/SurvivorA_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 경비·연구원이 아닌 일반인 (ART_REQUESTS) |
| 의상·장비·무기 | 낡은 옷 / 파이프·고철 |
| 신체 비율·크기 | 큰 머리 SD · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | 쓰러짐·사망 그림 쓰지 않음 (ART_REQUESTS) |

```
[공통 접두어] + an ordinary frightened civilian, brown hair, worn olive-green top, holding a metal pipe
```

동작별 포즈 (상태 이름 → 파일 `SurvivorA_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) **[P0]** — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Hurt` (1프레임 · 8fps · 반복) — 피격 경직
- `Sit` (1프레임 · 8fps · 반복) — 앉기 (벤치·바닥)
- `Scared` (1프레임 · 8fps · 반복) **[P0]** — 겁먹음
- `Guard` (2프레임 · 3fps · 반복) **[P0]** — 막아서기·방어 자세
- `Windup` (1프레임 · 6fps · 반복) **[P0]** — 공격 준비 예고 자세 (한눈에 읽혀야 함)
- `Strike` (1프레임 · 1fps · 1회) **[P0]** — 내려치기
- `Toss` (2프레임 · 7fps · 1회) **[P0]** — 던지기 (약병·상자)
- `Calm` (1프레임 · 3fps · 반복) **[P0]** — 파이프를 놓고 주저앉음 (진정)
- `Down` (1프레임 · 8fps · 반복) — 쓰러져 누운 자세 (유지)

### CH_NPC_006 생존자 B

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/SurvivorB_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 생존자 A와 구분되는 외형 |
| 의상·장비·무기 | 낡은 옷 / 파이프·고철 |
| 신체 비율·크기 | 큰 머리 SD · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | — |

```
[공통 접두어] + another ordinary frightened civilian, light brown hair, blue-grey top, holding scrap metal
```

동작별 포즈 (상태 이름 → 파일 `SurvivorB_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) **[P0]** — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Hurt` (1프레임 · 8fps · 반복) — 피격 경직
- `Sit` (1프레임 · 8fps · 반복) — 앉기 (벤치·바닥)
- `Scared` (1프레임 · 8fps · 반복) **[P0]** — 겁먹음
- `Guard` (2프레임 · 3fps · 반복) **[P0]** — 막아서기·방어 자세
- `Windup` (1프레임 · 6fps · 반복) **[P0]** — 공격 준비 예고 자세 (한눈에 읽혀야 함)
- `Strike` (1프레임 · 1fps · 1회) **[P0]** — 내려치기
- `Toss` (2프레임 · 7fps · 1회) **[P0]** — 던지기 (약병·상자)
- `Calm` (1프레임 · 3fps · 반복) **[P0]** — 파이프를 놓고 주저앉음 (진정)
- `Down` (1프레임 · 8fps · 반복) — 쓰러져 누운 자세 (유지)

### CH_NPC_007 베스페르 단원 A

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/MemberA_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 탑승 예정 단원, 번호표 (ART_REQUESTS) |
| 의상·장비·무기 | 단원복 (추정) / 없음 |
| 신체 비율·크기 | 큰 머리 SD · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | — |

```
[공통 접두어] + a cult organization member in a plain grey uniform with a boarding number tag
```

동작별 포즈 (상태 이름 → 파일 `MemberA_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Hurt` (1프레임 · 8fps · 반복) — 피격 경직
- `Sit` (1프레임 · 8fps · 반복) — 앉기 (벤치·바닥)
- `Down` (1프레임 · 8fps · 반복) — 쓰러져 누운 자세 (유지)
- `Wait` (2프레임 · 4fps · 반복) — 줄 서서 기다림
- `Scared` (1프레임 · 8fps · 반복) — 겁먹음
- `Cheer` (2프레임 · 4fps · 반복) — 환호

### CH_NPC_008 베스페르 단원 B

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/MemberB_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 단원 A와 구분 |
| 의상·장비·무기 | 단원복 (추정) / 없음 |
| 신체 비율·크기 | 큰 머리 SD · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | — |

```
[공통 접두어] + another member in a plain grey uniform with a number tag, light hair
```

동작별 포즈 (상태 이름 → 파일 `MemberB_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Hurt` (1프레임 · 8fps · 반복) — 피격 경직
- `Sit` (1프레임 · 8fps · 반복) — 앉기 (벤치·바닥)
- `Down` (1프레임 · 8fps · 반복) — 쓰러져 누운 자세 (유지)
- `Wait` (2프레임 · 4fps · 반복) — 줄 서서 기다림
- `Scared` (1프레임 · 8fps · 반복) — 겁먹음
- `Cheer` (2프레임 · 4fps · 반복) — 환호

### CH_NPC_009 회수 경비

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Warden_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 개조 경비병, 말이 거의 없음 (GDD 조연) |
| 의상·장비·무기 | 무장 (추정) / 총기 (컷신에서 총성 — 추정) |
| 신체 비율·크기 | 큰 머리 SD · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | 플레이 중 전투 대상 아님 |

```
[공통 접두어] + a silent modified security guard in dark armor-like clothes
```

동작별 포즈 (상태 이름 → 파일 `Warden_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Hurt` (1프레임 · 8fps · 반복) — 피격 경직
- `Sit` (1프레임 · 8fps · 반복) — 앉기 (벤치·바닥)
- `Down` (1프레임 · 8fps · 반복) — 쓰러져 누운 자세 (유지)
- `Guard` (2프레임 · 3fps · 반복) — 막아서기·방어 자세
- `Strike` (1프레임 · 1fps · 1회) — 내려치기

### CH_NPC_010 린든

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Linden_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 아이. 현재 임시는 0.72배 축소 → 아이 비율로 (ART_REQUESTS) |
| 의상·장비·무기 | 베넬 옷과 같은 캐릭터 옷 (추정) / 없음 |
| 신체 비율·크기 | 아이 비율 · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | — |

```
[공통 접두어] + a small child with purple hair and an orange top (Venelle's little brother/son), child proportions
```

동작별 포즈 (상태 이름 → 파일 `Linden_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (2프레임 · 3fps · 반복) — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Hurt` (1프레임 · 8fps · 반복) — 피격 경직
- `Sit` (1프레임 · 8fps · 반복) — 앉기 (벤치·바닥)
- `Hold` (1프레임 · 1fps · 반복) — 들고 있기
- `Wave` (2프레임 · 4fps · 반복) — 손 흔들기

### CH_NPC_011 어른 (회상)

| 항목 | 지시 |
|---|---|
| 참조할 디자인 | GDD.md 2장 해당 인물 · 현재 임시 그림 `Assets/Game/Art/Placeholder/Adult_Idle.png` (색 배치 참고) · 사용자 설정화가 있으면 그것 |
| 반드시 유지할 외형 | 얼굴 없는 어두운 실루엣이면 충분 (ART_REQUESTS) |
| 의상·장비·무기 | — / 목발 |
| 신체 비율·크기 | 어른 · 프레임 48×64, 실제 키 약 39~48px |
| 카메라 각도 | 정측면 (사이드뷰), 오른쪽을 봄, 눈높이 수평 |
| 배경·투명도 | 투명 (생성은 #FF00FF) |
| 출력 | PNG, 생성 384×512/프레임 → 최종 48×64/프레임, 가로 한 줄 시트 |
| 비고 | — |

```
[공통 접두어] + a faceless dark silhouette of an adult man walking with a crutch
```

동작별 포즈 (상태 이름 → 파일 `Adult_<상태>.png`, 프레임 수는 ANIMATION_CATALOG):

- `Idle` (1프레임 · 3fps · 반복) — 대기 (숨쉬기)
- `Walk` (4프레임 · 8fps · 반복) — 걷기
- `Crutch` (1프레임 · 1fps · 반복) — 목발 짚고 걷기

## 6. 스탠딩 (대화 일러스트)

| 항목 | 지시 |
|---|---|
| 크기 | 생성 1200×1800 → 최종 **600×900** (2:3). 투명 배경 |
| 구도 | 허리 위 또는 무릎 위까지, 얼굴 중심이 위에서 30~40% 지점 (아래쪽은 대화창에 가려도 됨) |
| 방향 | **루는 오른쪽을 봄** (왼쪽 자리). 나머지 인물은 **왼쪽을 봄** (오른쪽 자리). 좌우 반전하지 않는다 |
| 화풍 | 깔끔한 셀 채색 일러스트, 도트 스프라이트와 같은 의상·색 |
| 일관성 | 인물마다 기본(neutral) 1장을 먼저 확정하고, 표정만 바꾼 변형으로 생성 (몸·옷·포즈 고정) |
| 가져오기 | Bilinear, 고품질 압축, 아래 가운데 피벗 — 자동 |

```
clean anime-style cel-shaded character illustration, waist-up, [facing right | facing left], transparent background, consistent outfit with reference,
expression: <표정>, no text, no watermark, soft rim light, 2:3 portrait canvas, face center at about 35% from top
```

필요한 표정 목록과 사용 대사: CHARACTER_ASSET_CATALOG.md 각 인물 B 표. 표정 이름 → 영어: neutral, smile, sad, despair, surprised, scared, determined, pained, angry_cry(crying in anger), gentle, blank(vacant), excited(manic), confused, quiet(withdrawn), smirk.
루의 최종전 전용 표정(ST_LU_LOW, MISSING): 평소 밝은 루가 처음 보이는 낮고 차가운 분노 — 눈을 반쯤 내리깔고 입을 다문 얼굴 (GDD 0장·10장).

## 7. 배경 레이어

| 레이어 | 지시 | 크기 (현재) | 투명 |
|---|---|---|---|
| 하늘 (`V_Sky`) | 세로 그라데이션 | 4×360 (가로로 늘림) | 불투명 |
| 원경 (`V_Far`, `L_Far`) | **좌우가 이어지는 타일** (왼쪽 끝 = 오른쪽 끝). 멀수록 하늘색에 섞이게 | 640×220 / 640×260 | 불투명 또는 위쪽 투명 |
| 중경 (`L_MidA/B`, `V_HouseA/B/C`) | 개별 건물·장비 덩어리. 어둡게 | 320×200 / 64~96 × 76~112 | 투명 |
| 전경 (`L_ForePillar`, `L_ForePipe`, `V_ForeBeam`, `V_ForeRubble`) | 실루엣, 인물 앞을 지나감 | 40×240, 240×36, 80×200, 200×56 | 투명 |
| 벽·바닥 타일 (`L_WallPanel`, `L_Floor`, `L_FloorFill`, `V_Ground*`, `H_Wall/Floor`) | 32×32 (벽 64×64) **사방 반복 이음매 없게** | 32×32 | 불투명 |

```
16-bit pixel art background layer, side-scrolling game, <장소>, <원경|중경|전경> layer only, seamless horizontal tiling, muted dark blue-grey and murky green palette,
low contrast decoration, no characters, no text, no UI, [transparent background for mid/foreground layers]
```

7~11장 전용 레이어(MISSING): 수용동·접수 홀·공연장·전이 장치실·무너진 중정 — 지금은 연구소 레이어를 어둡게 재사용 (ART_REQUESTS 0.000). 크기는 `L_Far`/`L_MidA` 와 같게 만들면 방 정의 코드의 레이어 이름만 바꿔 연결할 수 있다 (코드 변경 필요 — 이번 범위 밖).

## 8. 소품·장치

- 크기·프레임·피벗은 SCENE_ASSET_CATALOG / ASSET_IMPLEMENTATION_MAPPING 의 현재 값을 **그대로** 쓴다 (예: `U_Pump` 64×48 ×4, `L_Door` 48×80 ×3).
- 상태 프레임이 있는 장치는 상태 차이가 **색 없이도 모양으로** 읽혀야 한다 (예: `S_IconPowerEmpty` 는 사선, 차단기는 손잡이 위치).
- 코드가 색칠하는 부품(`S_Lens`, `S_CtrlScreen`, `S_Lamp`)은 **흰색**으로 그린다.
- 위치가 코드와 맞아야 하는 것: `U_Pump` 이음쇠 (44,33)px, `Venelle_Grab` 손 (37,18)px, `Lu_Lifted` 목덜미 (24,33)px, 엄폐 상자 높이 정확히 32px, `Lu_Crouch` 보이는 키 ≤ 30px.

```
16-bit pixel art game prop, side view, <소품 설명>, exact canvas <W>x<H> pixels (generate at 8x), clean 1px outline, dark sci-fi lab facility palette, transparent background, no text
```

## 9. 이펙트

| 종류 | 이미지로 만들 것 | 코드·셰이더로 이미 처리하는 것 (그리지 않음) |
|---|---|---|
| 전기 | `FX_Spark` 32×32 ×6, `FX_Bolt` 16×8 ×2, `FX_Arc` 64×32 ×3, `FX_LightDot` 16×16 ×4 | 기술 대상 연결선·아크(LineRenderer), 전기화 윤곽·잔상·신호 왜곡(`Visual/PixelScreen`·셰이더 `SignalDistortion`) |
| 화학 (베넬) | `FX_AcidFan` 64×32 ×4 (왼쪽 가운데 피벗, 판정 높이 0.55유닛 이하로 낮게), `FX_PuddleAcid` 64×12 ×3, `FX_Flask` ×4, `FX_AcidSplash`·`FX_ReactBurst`·`FX_Bubbles`·`FX_ToxicMist` | 누출 입자(ReactionParticles) |
| 폭발·연기·불 | `FX_Explosion` ×6, `FX_Smoke` ×4, `FX_Fire` 16×24 ×6, `FX_FireBig` 32×48 ×6, `FX_Ember`·`FX_Ash`·`FX_Dust` (2×2 입자) | 화면 흔들림(`Cutscene.Shake`), 페이드, 흰 섬광 |
| 빛 | `FX_Glow`·`FX_GlowSoft` (가산), `S_AlarmBeam` (가산 반투명) | 경광등 회전 |
| 보스 예고 | — | 바닥 위험 띠·기둥 표식·깜빡임 (`BossBase` 코드 도형) |

## 10. UI

- 기술 아이콘 `UI_Skill_tok/jam/pull/wire/spark_shot.png`: **16×16**, 가운데 피벗, 1px 외곽선, 2배로 표시. 기술 이름이 없어도 모양으로 구분 (톡! = 손가락 불꽃, 간섭 = 물결 끊김, 방전 = 번개+플러그, 전선 = 콘센트+화살표, 샷 = 작은 탄).
- 안내서 미리보기 `UI_Prev_*` 10~20px.
- 엔딩 가상 바탕화면 아이콘 `U_IconFolder/Doc/Bin/Game` 과 배경(640×360, MISSING): **특정 운영체제와 닮지 않게** (ART_REQUESTS).
- 대화창·메뉴 패널은 현재 Kenney 9-slice(48×48, 테두리 4, PPU 100). 교체하려면 같은 9-slice 구조로.
