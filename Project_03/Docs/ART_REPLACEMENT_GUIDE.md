> 2026-11-15 ?????? ?? ????? ?? ArtDrop ?? ?? ??? ???? ???. ?? ???? ?? ??? ??? [?? ?? ??](ART_PLAY_OVERHAUL_REPORT.md)? ???? ??. ??? ?? ?? ??? ???? ????.

# 원화 교체 가이드

> 2026-11-14 개편 추가: 그려야 할 전체 목록·규격은 **`Docs/ART_REQUESTS.md`** (오프닝 동작·연구원·스탠딩·마을/집/바탕화면/실험실 배경).
> 이 문서는 '바꾸는 방법'을 다룬다.

현재 캐릭터 그림은 모두 **개발용 임시 그림**(몸통에 LU / VN / RS 표시)이며 최종 디자인이 아니다.
게임 로직은 그림을 직접 알지 못하고 **상태 이름**(Idle, Walk …)과 `CharacterArt` 에셋만 참조하므로,
아래 규격대로 그림을 바꾸면 코드 수정 없이 교체된다.

## 1. 공통 규격

| 항목 | 값 |
|---|---|
| 월드 단위 | 32 픽셀 = 1 유닛 (PPU 32), 지형 타일 32×32 |
| 기준 화면 | 640×360 (정수 배율 확대) |
| 캐릭터 캔버스 | **48×64**, 실제 키 약 40~48px (임시 그림은 약 45px) |
| 피벗 | **발바닥 가운데 (0.5, 0)** — 모든 프레임에서 발 위치가 같아야 한다 (걷기·공격 중 발이 미끄러지지 않게) |
| 기본 방향 | **오른쪽**을 본다. 왼쪽은 좌우 반전으로 처리 |
| 시트 형식 | 가로 한 줄 스트립 PNG, 투명 배경, 프레임 사이 여백 없음 |
| 가져오기 | Sprite (Multiple), Point 필터, 압축 없음, 밉맵 없음 — 구성 도구가 자동 설정 |

## 2. 캐릭터별 필요한 동작

파일 위치: `Assets/Game/Art/Placeholder/<캐릭터>_<상태>.png`
(클립: `Assets/Game/Art/Generated/Anim/<id>/<id>_<상태>.anim`, 컨트롤러: `<id>.controller`)

### 루 (`lu`, 파일 접두어 `Lu_`)
| 상태 | 프레임(현재) | fps | 반복 | 쓰임 |
|---|---|---|---|---|
| Idle | 2 | 3 | O | 서 있기 (숨쉬기: 몸만 1px, 발 고정) |
| Walk | 4 | 8 | O | 걷기 (달리기는 같은 클립 1.5배속) |
| Jump / Fall | 1 / 1 | — | O | 점프 상승 / 하강 |
| Climb | 2 | 6 | O | 사다리 (멈추면 애니메이션 정지) |
| Hurt | 1 | — | — | 피격 |
| Down | 1 | — | — | 전투 불능·주저앉음 |
| Sit | 1 | — | — | 앉기 (현재 미사용, 자리만) |
| Flick | 3 | 9 | X | **톡!** 검지 튕기기 |
| Point | 3 | 9 | X | **스파크 샷** 손가락 겨누기 |
| Swing | 3 | 9 | X | **아크 채찍** 팔 휘두르기 |
| Kick | 3 | 9 | X | **번개 발차기** |
| Charge | 2 | 6 | O | **전하 모으기** 볼에 힘주고 주먹 모으기 |
| Discharge | 2 | 10 | O | 큰 방전(1장 실험 통) |
| Carry | 4 | 5 | O | 베넬 부축하며 걷기 (앞팔을 옆으로) |

GDD 10.2 기준으로 추가하면 좋은 것: 표정 포트레이트(웃음·놀람·우쭐·투덜·집중·걱정), 따라잡기·뒤돌아보기, 최종전 전용 동작(이번 범위 밖).

### 베넬 (`venelle`, 접두어 `Venelle_`)
Idle, Walk, Jump, Fall, Climb, Hurt, Down, Sit(휴게실에서 앉기), **Attack**(팔 장치 산성 분사 3f), **Collapsed**(협상 후 주저앉음), **Supported**(부축받으며 걷기 4f), **Dizzy**(혼란 2f).
GDD 10.2 표정: 상냥·멍함·들뜸·혼란·기억의 정적.

### 연구원 (`researcher`, 접두어 `Researcher_`)
Idle, Walk, Hurt, Down, Scared(놀라서 손 듦). (Jump/Fall/Climb/Sit 파일도 있으나 미사용)

### 일반 적 (`droid01/02/03`)
Foozle 드로이드 그대로 사용 중. 상태: Idle, Move, Attack, Hurt, Death. 기본 방향이 **왼쪽**이라 `Art_droid0X.asset` 의 `Faces Right` 가 꺼져 있다.
고유 외형(순찰 경비·운반 로봇·정비 거미)으로 바꿀 때 GDD 13.6 권장: 대기 4, 이동 6, 공격 6, 피격 2, 파괴 6 프레임.

## 3. 교체 방법

### 방법 A — 같은 규격으로 덮어쓰기 (가장 간단)
1. `Assets/Game/Art/Placeholder/Lu_Walk.png` 를 같은 이름·같은 프레임 크기(48×64)의 새 그림으로 덮어쓴다. 프레임 수는 달라도 된다.
2. 메뉴 **EW > 2. 애니메이션 클립 다시 만들기** — 시트를 다시 자르고 클립을 새 프레임으로 채운다.
   (주의: 이 메뉴는 생성된 클립을 덮어쓴다. 클립을 직접 손본 경우 방법 B 를 쓴다.)

### 방법 B — 직접 만든 AnimationClip 연결
1. 새 클립을 만든다 (SpriteRenderer.Sprite 키프레임, 경로는 빈 문자열 = 같은 오브젝트).
2. `Assets/Game/Art/Generated/Anim/lu/lu.controller` 를 열고 해당 **상태(예: Walk)의 Motion** 에 새 클립을 넣는다.
   상태 이름은 바꾸지 않는다 (로직이 이름으로 재생한다).
3. 또는 `Assets/Game/Data/Art/Art_lu.asset` 의 `Controller` 를 새 Animator Controller / Override Controller 로 바꾼다 (같은 상태 이름 필요).

### 프레임 크기를 바꾸고 싶을 때
`Assets/Game/Scripts/Editor/EWArtSpecs.cs` 의 `ForPlaceholder` 에서 접두어별 `frameW/frameH` 를 바꾼 뒤 EW > 2 를 실행한다.
키가 달라지면 `Art_<id>.asset` 의 `Height`(말풍선·HP 표시 높이)도 맞춘다.

## 4. 좌우 반전 주의 (비대칭 캐릭터)

- 현재 왼쪽은 **스프라이트 좌우 반전**으로 그린다.
- **베넬의 팔 장치**처럼 좌우가 설정상 중요한 요소는 반전하면 반대 팔로 옮겨 간다 (임시 그림의 초록 상자로 확인 가능).
  해결: 왼쪽용 클립을 따로 그려 별도 Animator Controller 를 만들고 `Art_venelle.asset` 의 **Left Controller** 에 지정한다.
  지정하면 반전 대신 그 컨트롤러를 쓴다 (`CharacterView.Face`).
- 루의 앞머리·흉터·소지품, 메이린의 기워진 얼굴 반쪽(이후 장)도 같은 방식으로 확인한다.

## 5. 장치·소품·효과 교체

`Assets/Game/Art/Placeholder/Prop_*.png`, `FX_*.png` 를 같은 규격으로 덮어쓴 뒤 **EW > 1. 전체 구성** 을 실행하면 씬의 장치 그림이 갱신된다.

| 그림 | 프레임 | 상태 순서 |
|---|---|---|
| Prop_PowerSocket | 32×32 ×3 | 입구 / 빛남(출구 미리 표시) / 끊어짐 |
| Prop_Computer | 32×32 ×4 | 켜짐 / 꺼짐(검은 화면) / 부팅 / 메시지 |
| Prop_Door | 32×64 ×3 | 닫힘 / 열림 / 정전(닫힘·빨간불) — 현재 열림·정전 사용 |
| Prop_Switch | 16×32 ×2 | 꺼짐 / 켜짐 |
| Prop_RestoreButton | 16×32 ×2 | 대기 / 눌림 |
| Prop_Vending | 32×64 ×3 | 켜짐 / 꺼짐 / 고장 |
| Prop_Tank | 48×64 ×2 | 정상 / 깨짐 |
| Prop_CeilingLight | 32×8 ×2 | 켜짐 / 꺼짐 (피벗 위쪽) |
| Prop_EmergencyLight | 16×16 ×2 | 켜짐 / 꺼짐 (배터리 표식 포함) |
| Prop_Battery | 32×32 ×1 | 잔량 막대는 코드가 위에 그림 |
| FX_* | 시트별 (ASSET_INVENTORY 참고) | |

GDD 13.4: 전원선·배터리·스위치는 같은 그림체 세트로 만들고, 켜짐/꺼짐·잔량·입출구·끊어진 끝·복구 버튼을 구분할 수 있어야 한다.

## 5-1. 장면 배경 레이어 (2026-11-14 추가)

`Assets/Game/Art/Placeholder/Scene/*.png` (마을 V_*, 집 H_*, 바탕화면 D_*, 실험실 L_*, 효과 FX_*) 를 같은 이름·규격으로 덮어쓰고 **EW > 1** 을 실행한다.
- 바닥·벽 채움 타일(`V_Ground`, `V_GroundFill`, `H_Wall`, `H_Floor`, `D_Bar`, `L_Floor`, `L_FloorFill`, `L_WallPanel`)은 **중앙 피벗**, 나머지는 **아래 가운데 피벗**으로 자동 설정된다.
- 여러 상태 그림(문 닫힘/열림/비틀림 등)은 가로 스트립 순서가 곧 상태 번호다 (`FrameSet`). 순서표: `ART_REQUESTS.md` 4절.
- 시차(원경·중경·전경)·배치는 `Assets/Game/Scripts/Editor/EWRooms.cs` 의 각 맵 `layers` 에서 조정한다.

## 5-2. 스탠딩 일러스트 (2026-11-14 추가)

- `Assets/Game/Art/Portraits/<인물>_<표정>.png` 를 덮어쓰고 EW > 1 → 자동으로 `GameConfig.portraits` 에 연결된다. 새 표정은 새 파일을 추가하면 된다.
- 고해상도 그대로 표시된다 (Bilinear, 도트 렌더 텍스처를 거치지 않음). 권장 600×900, 아래 가운데 기준, 얼굴 중심이 위에서 30~40%.
- 어느 대사에 띄울지는 `dialogue_ko.json` 의 줄에 `"e": "표정"` (선택 `"p"`, `"pos": "L"/"R"`, `"hide": true`).

## 5-3. 루·연구원 새 동작 (2026-11-14 추가)

루: BackIdle, BackLook, Limp, LimpIdle, Sad, ReachPC, Connect, Land, GetUp, LookAround, Knock, BackAway, Fear, ChargeEyes, Recover, Squeeze.
연구원 1(`Researcher_`)·연구원 2(`Researcher2_`): Talk, BatonOut, BatonIdle, BatonWalk (+ Idle, Walk, Hurt, Down, Scared).
교체 방법은 위 3절과 같다 (새 시트를 같은 이름으로 덮어쓰고 EW > 2).

## 6. 로고

`Prop_PosterEmblem.png` (48×48) 는 **사용자 로고 원안 자리**다. 'LOGO ?' 표시만 있고 로고를 임의로 그리지 않았다.
원안을 도트화해 같은 이름으로 덮어쓰고 EW > 1 을 실행하면 자료실 광고 4장에 적용된다.
(웃는 얼굴·문 모양으로 바꾸지 않는다 — GDD 1.3)

## 7. UI

- 패널: `Assets/ThirdParty/Kenney_PixelUI/space.png` (9-slice). 다른 패널로 바꾸려면 `Resources/EW/GameConfig.asset` 의 `Panel / Panel Inlay` 를 교체.
- 글꼴: `GameConfig.Font` (Galmuri11). 한글이 들어간 다른 글꼴로 바꿀 수 있다.
