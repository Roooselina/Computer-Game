> 2026-11-15 ?????? ?? ????? ?? ArtDrop ?? ?? ??? ???? ???. ?? ???? ?? ??? ??? [?? ?? ??](ART_PLAY_OVERHAUL_REPORT.md)? ???? ??. ??? ?? ?? ??? ???? ????.

# 그려야 할 아트 명세 (사용자·아트 담당 에이전트용)

현재 화면의 인물·마을·실험실·잠입 기물 그림은 모두 **임시 그림(블록아웃)** 이다. 아래 항목은 아직 최종 그림이 **없다**.
같은 파일 이름·규격으로 덮어쓰면 코드 수정 없이 교체된다 (방법: `Docs/ART_REPLACEMENT_GUIDE.md`). 임시 그림 생성기: `Tools/placeholder_gen.py`(인물), `Tools/scene_art_gen.py`(배경), `Tools/stealth_art_gen.py`(잠입 기물·실험실 소품).

공통 규격: PPU 32 (32px = 1유닛), 기준 화면 640×360, Point 필터·무압축(구성 도구가 자동 설정), **투명 배경 PNG**, 가로 한 줄 스트립(프레임 사이 여백 없음), 1px 어두운 외곽선.
팔레트 방향: 어두운 청회색·탁한 녹색 시설. 진행 대상(엄폐 상자·출구·전원·조사 대상)은 대비를 높이고, 장식은 대비를 낮춘다. 붉은색은 경보 전용 (경광등·발견 상태). 신체 묘사는 하지 않는다 — 도식은 선·번호, 유리 너머는 실루엣만.

---

## 0. 납품 방법 (ArtDrop)

외부 아트 작업 결과는 프로젝트 루트 `ArtDrop/` 에 넣는다. **개발 쪽은 ArtDrop 을 읽기만 하고 수정하지 않는다.**

```
ArtDrop/
  2026-11-20_lu_stealth/          ← 납품 묶음 하나 = 폴더 하나 (날짜_내용)
    READY.txt                     ← 완료 표시. 이 파일이 있어야 가져간다 (작업 중에는 만들지 말 것)
    manifest.txt                  ← 한 줄에 하나: <파일명> -> <대상 경로>   (대상은 아래 표의 '파일')
    Lu_Crouch.png
    Lu_CrouchWalk.png
    ...
```

- READY.txt 에는 작성자·날짜·메모를 적는다. 개발 쪽은 READY.txt 가 있는 묶음만 확인 → 규격(크기·프레임 수·투명)을 검사 → `Assets/` 의 같은 이름 파일에 복사 → `EW > 1. 전체 구성` 으로 시트 분할·애니메이션·씬 연결 → 화면 확인. 검사에 맞지 않으면 묶음 안의 파일은 그대로 두고 문제를 보고한다.
- 2026-11-14 기준 `ArtDrop/` 폴더는 **없다** (받은 납품 없음). 2026-11-15 다시 확인: 여전히 없음.

## 0.000 7~11장·엔딩 (2026-11-15) — 새로 필요한 아트

지금은 모두 **임시 그림**이다 (`Tools/ch7_11_art_gen.py`, `Tools/ch7_11_portrait_gen.py` 가 새 이름 파일만 생성 — 매니페스트 `Tools/ch7_11_art_manifest.txt`).
같은 파일 이름·프레임 크기로 교체하면 코드 수정 없이 바뀐다 (규격: `Assets/Game/Scripts/Editor/EWArtSpecs.cs` 의 접두어 규칙).

### 인물 (48×64/프레임, 아래 가운데 기준점, 오른쪽을 봄)
| 파일 접두어 | 인물 | 상태 |
|---|---|---|
| `Raviel_` | 라비엘 (귀 없는 토끼 악마, 연구주임 가운) | Idle Walk Hurt Sit Down Talk Point Type HoldHand Catch Injured Cup Sad |
| `RavielBeast_` (**80×96**) | 괴물 토끼 라비엘 (2m 근육질) | Idle Walk Windup Leap Slam Swipe Stun Hurt Hold Down |
| `Meilin_` | 메이린 (도자기 인형, 치파오, 땋은 머리, 바늘꽂이, 롤러 발, 공예 가위) | Idle Walk Roll Windup Snip Dash Stun Hurt Talk Angry Pull Down |
| `Lacrimosa_` | 라크리모사 (공연복, 목·흉곽 증폭 장치) | Idle Walk Sing Windup Stagger Listen Hurt Bow Down |
| `Aurelius_` (**64×96**) | 아우렐리우스 (아름다운 외피 + 깨지면 드러나는 내부 기계) | Idle Talk Windup Cast Hurt Broken Ruined Down |
| `Edward_`, `EdwardKid_` | 회상: 음향실의 에드워드 / 어린 에드워드와 TV | Idle Write / Idle Hide Listen |
| `MemberA_`, `MemberB_` | 베스페르 단원 (탑승 예정자, 번호표) | Idle Walk Hurt Sit Down Wait Scared Cheer |
| `Warden_` | 회수 경비 | Idle Walk Hurt Sit Down Guard Strike |
| `Lu_` 추가 | 루 최종전·마지막 | Cold Limit Lie Kneel HandHold Release |

### 스탠딩 (600×900, `Assets/Game/Art/Portraits/`)
raviel (neutral smirk sad surprised smile pained), meilin (smile determined surprised scared sad), lacrimosa (gentle quiet sad pained),
aurelius (gentle determined pained), edward (quiet), member (neutral). 루의 최종전 ‘낮고 짧은’ 표정은 지금 determined 로 대신한다 — **전용 표정 필요**.

### 소품 (`Placeholder/Scene/U_*.png`)
수용칸 창살 U_CellBars(40×72×2) · 제어반 U_CellPanel(20×36×2) · 번호표 발급기 U_TicketMachine · 호출 번호판 U_CallBoard(40×20×4) · 포획팔 U_Claw(32×48×4, 위 기준점) · 레일 U_ClawRail ·
빈 운반 인형 U_Doll · 유지보수 단말 U_MaintTerm(28×36×2) · 전도성 바닥 U_ConductFloor(반복) · 제동 장치 U_Brake(20×30×2) · 부스 잠금장치 U_BoothLock · 감시 모니터 U_Monitor(48×36×3) · 격벽 U_Bulkhead(36×96×2) ·
어린 시절 사진 U_ChildPhoto · 컵 두 개 U_Cups(24×12×2) · 계획 종이 U_PlanNotes · 배치도 U_CommandChart · 마이크 U_Mic(20×40×2) · 스피커 U_Speaker(32×48×2) · 믹서 U_Mixer(32×24×3) · 조명 트러스 U_Truss · 테이프 U_Tape ·
서버 랙 U_ServerRack(28×64×2) · 포획 단말 U_CaptureTerm(48×72×3) · 탑승 캡슐 U_Capsule(40×76×2) · 탑승 현황판 U_BoardStatus · 접수 명부 U_Register · 호위 구 U_Escort(20×20×2) · 축전 모듈 U_Capacitor(28×52×2) ·
냉각관 U_CoolPipe · 전이 결합부 U_Coupling(72×72×3) · 정비용 전자석 U_Magnet · 잔해 U_Rubble · 회수 장치 U_Recovery(40×36×2) · 끊어진 번호표 U_BrokenTag · 움직이지 않는 손 U_StillHand · 멈춘 롤러 U_Roller · 가상 바탕화면 아이콘 U_IconFolder/Doc/Bin/Game

### 배경·연출
- 7~11장 방은 기존 연구소 레이어(`LabLayers`/`UnderLayers`)를 어둡게 조립했다. 수용동·접수 홀·공연장·전이 장치실·무너진 중정의 **전용 원경·중경 레이어가 필요**하다 (통짜 배경 금지, 레이어별 개별 그림).
- 무인 회수 장치는 지금 `droid02`(운반 로봇 임시) 인물로 움직인다 → 전용 인물 그림 필요.
- 엔딩 가상 바탕화면은 UI 로 그린 단색 배경·임시 아이콘이다. 특정 운영체제와 닮지 않은 전용 그림 필요.
- 소리: 새 효과음·음악은 만들지 않았다. 기존 생성 효과음(sfx_*)·bgm_battle·bgm_tension 을 다시 쓴다 → 메이린·라크리모사(노래·피드백)·최종전·붕괴·크레딧 음악 필요. 소리를 꺼도 진행할 수 있게 만들었다.

## 0.00 4~6장 (승강장 이후, 2026-11-15) — 새로 필요한 아트

임시 그림: `Tools/ch4_6_art_gen.py` (인물·소품, 새 파일만 만들고 기존 PNG·메타는 덮어쓰지 않음 — 만든 목록 `Tools/ch4_6_art_manifest.txt` 105개), `Tools/ch4_6_portrait_gen.py` (스탠딩 9장). 모두 **최종 그림이 아니다**. 맵의 벽·바닥·패럴랙스는 기존 개별 도트(연구소 타일·L_* 레이어·Tile_Home*·Prop_*)로 조립했고 통짜 배경은 쓰지 않았다.

### 인물 (48×64/프레임, 아래 가운데 기준점, 오른쪽을 봄)
| 우선 | 파일 | 사용 | 프레임 | 요구 |
|---|---|---|---|---|
| A | `F_Idle/Walk/Talk/Point/Toss/Guard/Tag/Hurt/Sit.png` | 폐기소의 F (할머니) | 1~4 | 짧고 투박한 사람. 목에 번호표(F). Toss = 빈 상자를 툭 던짐 (상자는 코드가 날림), Tag = 번호표를 들여다봄 |
| A | `Kid008_Idle/Walk/Talk/Hide/Eat/Hold/Draw/Sit.png` | 008 (아이) | 1~4 | **임시는 어른 그림을 0.78배로 줄인 것** — 아이 비율로 새로 그려야 함. Draw = 쪼그려 앉아 바닥에 선을 긋는 동작 |
| A | `SurvivorA_*`, `SurvivorB_*` (Idle/Walk/Scared/Guard/Windup/Strike/Toss/Calm/Hurt/Sit) | 겁먹은 생존자 (비살상 충돌) | 1~2 | 경비·연구원이 아닌 **일반인**. 파이프·고철을 들고 떨림. Windup 은 크게 치켜든 예고(0.8초), Calm = 파이프를 놓고 주저앉음. **쓰러짐·사망 그림은 쓰지 않는다** |
| B | `Ivy_*` (Idle/Walk/Carry/Kneel/Give/Wave/Sit) | 6장 회상: 개조 전 아이비 (장치 없음) | 1~4 | 세피아로 칠해 쓴다 (코드). 베넬과 같은 얼굴, 팔 장치 없음 |
| B | `Linden_Idle/Hold/Wave/Sit.png` | 회상: 동생 린든 | 1~2 | **임시는 0.72배 축소** — 아이 비율로 |
| C | `Adult_Idle/Walk/Crutch.png` | 회상: 아빠(목발) 실루엣 | 1~4 | 얼굴 없는 어두운 실루엣이면 충분. 새 관찰자·설명자로 쓰지 않는다 |
| A | `Lu_HairUp.png` | 펄스 버스트 뒤 머리가 곤두섬 | 2 · 3fps | 머리카락이 위로 뻗음 + 작은 불꽃 |
| B | `Lu_HandsUp.png` / `Lu_Share.png` / `Lu_Wind.png` | 폐기소 첫 만남 손 들기 / 과자 반 나눠 내밂 / 옥상 바람 | 2 / 1 / 2 | |
| B | `Venelle_Comb.png` | 빗을 꺼내다 금속 빗을 보고 다시 넣음 | 2 · 1회 | 손에 든 빗이 쇠빛으로 보이게 |
| B | `Venelle_SitQuiet.png` / `Venelle_Share.png` / `Venelle_Wind.png` | 방 침대에 앉아 말을 잃음 / 초코바를 내밂 / 옥상 | 2 / 1 / 2 | |

### 스탠딩 (600×900, `Assets/Game/Art/Portraits/`)
| 우선 | 파일 | 비고 |
|---|---|---|
| A | `f_neutral`, `f_determined`, `f_gentle` | F — 무뚝뚝/누그러짐 |
| A | `kid008_neutral`, `kid008_surprised`, `kid008_smile`, `kid008_sad` | 008 — 지금 임시는 어른 얼굴 비율 |
| B | `survivor_scared`, `survivor_neutral` | 생존자 공용 |
| — | (아이비·등불 장수·고물 장수) | 스탠딩 없이 이름만 표시 중. 회상 장면은 스탠딩을 쓰지 않는 쪽을 권장 |

### 소품 (`Assets/Game/Art/Placeholder/Scene/U_*.png`, 아래 가운데 기준점 — 표시한 것은 프레임 크기)
| 우선 | 파일 | 크기 | 사용 |
|---|---|---|---|
| A | `U_Pump` | 64×48 ×4 (고장 정지/정상 정지/가동 2) | 물 펌프. **이음쇠(연결 부품) 위치 = 프레임 (44, 33)px** — 깨짐/새것이 뚜렷이 달라야 함 |
| A | `U_Valve` (24×32 ×3 회전), `U_Breaker` (24×36 ×2 내림/올림), `U_Part` (14×12) | | 배수 밸브·차단기 손잡이·새 이음쇠. 상태가 그림으로 읽혀야 함 |
| A | `U_WaterTop` (32×6 ×2, 가로 반복), `U_WaterFill` (8×8 채움) | | 고인 물. 높이는 코드가 바꾼다 |
| A | `U_Belt` (32×10 ×4, 가로 반복), `U_BeltCtrl` (24×32 ×3: 정방향/역방향/정지), `U_BeltCrate` | | 컨베이어. **벨트가 멈추면 그림도 멈춘다** (코드) |
| A | `U_LiftDeck` (96×14), `U_LiftShaft` (112×120), `U_SealShutter` (48×96), `U_LiftPanel` (16×32 ×2) | | 화물 승강기·위층 봉쇄 셔터 |
| A | `U_TwinOutfits` (64×40) | | **베넬의 같은 옷 두 벌** (베넬 옷 색과 같아야 함) |
| B | `U_Barricade`, `U_Tarp`, `U_Stove` (×2), `U_CrateBed`, `U_Jug` (×2 빈/찬), `U_Tally`, `U_LaundryLine` | | 폐기소 생활 공간 |
| B | `U_StallA/B`, `U_Lantern` (×2), `U_ChalkLines` (×2: 여덟 개/한 줄 길게) | | 폐품 시장 |
| B | `U_Washer` (×2), `U_OtherOutfits`, `U_DormBed`, `U_DormDesk`, `U_Photo`, `U_Letter`, `U_Wrappers` (40×8, 열 장), `U_ChocoBox` | | 숙소·베넬의 방 |
| C | `U_TestRig`, `U_PartShelf`, `U_PipeH`, `U_Rail`, `U_WaterTower`, `U_HomeDoor`, `U_TrialFlyer` | | 작업장·옥상·회상 |

### 배경 (지금은 기존 레이어를 어둡게 덧칠해 조립)
| 우선 | 장소 | 지금 | 필요 |
|---|---|---|---|
| B | 폐기소 거처·시장 (M17·M19) | 연구소 레이어 + 어두운 막 + 폐기 설비 실루엣 | 버려진 설비·배관이 쌓인 지하 원경 (개별 레이어) |
| B | 숙소 복도·세탁실 (M24) | 연구소 레이어 | 숙소 벽·문 줄·게시판 |
| B | 베넬의 방 (M25) | 기존 `Tile_HomeWall` + `Prop_Bed/Desk/Mirror/Cupboard/Clothes` | 시설 숙소 방 전용 벽·창 |
| B | 기숙사 옥상 (M26) | 어둡게 칠한 `ansimuz_SynthCities/back.png` 실루엣 + 안개 | 시설 바깥이 보이지 않는 야간 원경 (바깥으로 나갈 수 있다는 인상 금지) |

## 0.0 2026-11-15 추가 수정에 필요한 아트 (우선 확인)

임시 그림 생성기: 인물 `Tools/placeholder_gen.py`, 아이콘·화학 효과·엄폐물·컴퓨터 안·무너진 집 `Tools/qwer_art_gen.py`. 아래는 전부 **임시 그림이 들어가 있고 최종 그림이 없다.**

### 인물 (48×64/프레임, 아래 가운데 기준점, 오른쪽을 봄)
| 우선 | 파일 | 사용 장면 | 프레임·fps | 판독·정렬 요구 |
|---|---|---|---|---|
| A | `Lu_TankCrouch.png` | 실험 통 착지·대기 (통 안 받침 위) | 2 · 2fps 반복 | **보이는 몸이 프레임 위쪽 80px(2.5유닛) 안**: 통 내부(받침 위 0.5 ~ 3.03유닛)를 넘지 않게. 무릎을 굽힌 수용 자세 |
| A | `Lu_TankStruggle.png` / `Lu_TankCower.png` | 통 두드리며 몸부림 / 연구원 앞에서 움츠림 | 2 · 7fps / 2 · 2fps | 위와 같은 높이 제한. 유리(앞)·뒤판(뒤) 사이에 그려짐 |
| A | `Lu_Lifted.png` | 베넬에게 목덜미를 잡혀 들어 올려짐 (대화 중) | 4 · 7fps 반복 | **목덜미 = 프레임 (24, 33)px** (발 기준 0.97유닛). 발이 떨어져 버둥거림, 두 손은 베넬 손목 쪽 |
| A | `Venelle_Grab.png` | 잡기 (예고 0.75초 + 들어 올림) | 2 · 3fps | **손 = 프레임 (37, 18)px** (발 기준 +0.41, +1.44유닛) — 코드 `VenelleBoss.GrabHand` 와 같아야 함. 베넬이 루보다 앞에 그려진다 |
| A | `Lu_Thrown.png` | 던져짐 (회전은 코드) | 1~2 | 몸을 웅크린 모양 |
| A | `Venelle_Spray.png` / `Venelle_Toss.png` / `Venelle_Mix.png` | 산성 분사 준비 / 약병 투척 / 재혼합 빈틈 | 2·8fps / 2·7fps 1회 / 3·6fps | 팔 장치(약품 탱크→관→손목 약병)와 액체가 보여야 함. Mix 는 거품이 이는 '빈틈'임이 읽히게 |
| A | `Venelle_Downed.png` | 체력 0 — 무릎 꿇고 숨을 몰아쉼 (처치 선택 전) | 1~2 · 2fps | 팔 장치 관에서 약품이 새는 모습 |
| B | `Lu_EyesClosed.png` / `Lu_Disgust.png` / `Lu_HeadHold.png` | 컴퓨터 앞 눈을 질끈 감음 / 입을 가리고 몸을 숙임 / 잔류 스파크에 머리를 감싸 쥠 | 1 / 2 / 2 | 탈출 시작 장면 (사용자 지정 연기) |
| B | `Lu_Electrify.png` | 전기화 직전 (윤곽 발광, 몸이 신호로 갈라짐) | 2 · 12fps | 압축·잔상·빛줄기는 코드가 그림 |
| B | `Lu_HoldHand.png` | 동료 손을 잡고 전원선으로 | 1 | 뒤쪽 손을 뻗음 |
| B | `Venelle_TalkWalk.png` | 투덜거리며 걸어옴 (등장) | 4 · 6fps | |

### 효과 (`FX_*`, 32×32 프레임 가운데 기준점 — 부채는 64×32 왼쪽 기준점, 웅덩이 64×12 아래 기준점)
| 우선 | 파일 | 사용 | 요구 |
|---|---|---|---|
| A | `FX_AcidFan.png` (4) | 낮은 부채꼴 분사 | **판정 높이(발목~허리, 0.55유닛)와 같은 높이**로 낮게. 길이는 코드가 4.2유닛에 맞춰 늘림 |
| A | `FX_PuddleAcid.png` (3) | 독 웅덩이 (폭 1.6유닛) | 끓는 거품, 전기·출구 색과 구분되는 산성 녹색 |
| A | `FX_Flask.png` (4) | 회전하며 날아가는 약병 | |
| B | `FX_AcidSplash.png` (5), `FX_ReactBurst.png` (5), `FX_Bubbles.png` (4), `FX_ToxicMist.png` (4) | 튐·반응 폭발·기포·독 안개 | 화려해도 판정 범위보다 크게 퍼지지 않게 |

### UI·기물
| 우선 | 파일 | 크기 | 사용 |
|---|---|---|---|
| A | `UI_Skill_tok/jam/pull/wire/spark_shot.png` | 16×16 (가운데) | Q/W/E/R 슬롯 아이콘, 안내서 |
| B | `UI_Prev_Dummy/Cctv/CctvOff/Source/Panel/Socket.png` | 10~20px | 안내서 미리보기 (판정 없는 시연) |
| A | `Scene/S_HideRack.png` (56×44), `Scene/S_HideSacks.png` (64×36) | 아래 가운데, 레이어 20 (루 앞) | **뒤에 숨는 전경 엄폐물** — 숨은 루의 윤곽이 앞에 비쳐 보임. 아래 1.2유닛이 몸을 가려야 함 |
| A | `Scene/S_CrateSolid.png` | 48×32 | **올라서는 고체 상자** — 윗면 밝은 띠·노란 사선으로 엄폐물과 구분 |
| A | `Scene/D_Desktop.png` (640×352) + `Scene/D_Taskbar.png` (640×32) | 아래 가운데 | 컴퓨터 안. **작업표시줄은 D_Taskbar 하나뿐** — 배경에 작업표시줄을 그리지 말 것 |
| B | `Scene/D_Window.png` (224×128), `Scene/D_Icon_guide/status/log/photo/webcam/battery/socket/exit/shutter.png` (24×24) | | 제목줄·아이콘 윗면이 발판 (보이지 않는 발판과 위치가 맞아야 함) |
| A | `Scene/H_RuinBack.png` (448×288, 14×9유닛) | 아래 가운데, 레이어 −45 | **뒤쪽 벽이 날아간 작은 집**: 들쭉날쭉한 벽 가장자리·남은 기둥·잔해. 구멍 밖은 투명(카메라 배경 검정). 정상 집 위에 검은 사각형을 덮는 방식 금지 |

## 0.1 실시간 잠입 개편 — 우선순위표

우선순위: **A** = 플레이 판독에 바로 영향 (먼저), **B** = 연출 품질, **C** = 분위기.
레이어 = SpriteRenderer sortingOrder (낮을수록 뒤). 기준점 = 그림 안의 피벗 (아래 가운데가 기본).

### 인물 (캔버스 48×64/프레임, 아래 가운데 기준점, 오른쪽을 봄, 레이어 12=루 / 9=베넬·적)

| 우선 | 파일 (`Assets/Game/Art/Placeholder/`) | 사용 장면 | 프레임 · fps · 반복 | 연결 상태 | 메모 |
|---|---|---|---|---|---|
| A | `Lu_Crouch.png` | 엄폐물 뒤 숨기 | 2 · 2fps · 반복(숨쉬기) | `Crouch` | **보이는 키가 30px(0.95유닛) 이하**여야 감지 높이와 맞는다 (상자 높이 32px) |
| A | `Lu_CrouchWalk.png` | 웅크려 걷기 | 4 · 6fps · 반복 | `CrouchWalk` | 위와 같은 높이 |
| A | `Lu_Dodge.png` | 회피 (0.2초) | 1~3 · 12fps · 1회 | `Dodge` | 낮게 미끄러지듯, 잔상은 코드가 아님(그림에 포함 가능) |
| A | `Lu_Hurt.png` | 피격 경직 0.25초 | 1~2 · 8fps | `Hurt` | 기존 파일, 실시간에 맞게 짧게 |
| A | `Lu_Flick.png` | 톡! (준비 0.07 · 판정 0.1) | 3 · 9fps · 1회 | `Flick` | 2번째 프레임이 판정 순간 |
| A | `Lu_Point.png` | 스파크 샷·약한 간섭 겨누기 | 3 · 9fps · 1회 | `Point` | |
| A | `Lu_Charge.png` | 외부 전력 끌어오기 (준비 0.5초) | 2 · 6fps · 반복 | `Charge` | 손이 전원 쪽으로 |
| B | `Lu_WallLean.png` | 베넬 복도: 벽 짚고 숨 고르기 | 2 · 3fps · 반복 | `WallLean` | “헉... 헉...” |
| B | `Lu_Fear.png`, `Lu_BackAway.png` | 실험실 침대·베넬 앞 뒷걸음 | 기존 | `Fear`/`BackAway` | |
| A | `Venelle_Windup.png` | 돌진 전 몸을 낮춤 (1.1~1.9초 예고) | 2 · 6fps · 반복 | `Windup` | **예고가 한눈에 읽혀야 함** (자세+초록 빛) |
| A | `Venelle_Dash.png` | 돌진 | 1~2 · 12fps | `Dash` | |
| A | `Venelle_Pant.png` | 숨 고르기 (말 걸기 기회) / 무릎 꿇음 | 2 · 3fps · 반복 | `Pant` | 무릎 꿇은 자세는 별도 `Venelle_Kneel.png` 요청 가능 (현재 `Collapsed` 대용) |
| B | `Venelle_Stalk.png` | 불균형하고 느슨한 걸음 | 5~6 · 5fps · 반복 | `Stalk` | 고개 기울임, 박자가 어긋난 걸음 |
| B | `Venelle_Laugh.png` | 혼자 웃음 | 2~3 · 3fps | `Laugh` | |
| B | `Venelle_Attack.png` | 휘두르기·약병 던지기 | 3 · 9fps · 1회 | `Attack` | |
| A | 순찰 경비 (현재 Foozle Droid03 임시) | C02~C05 | Idle/Move/Attack/Hurt/Death | `droid03` | 사람보다 약간 작은 기계. **공격 준비 자세(제압봉을 치켜듦)** 프레임 추가 필요 (현재는 깜빡임으로만 예고) |
| B | 포획 드론·운반 로봇 (Foozle 임시) | C03~C05 | 같음 | `droid01/02` | 드론은 아래를 비추는 렌즈가 보이게 |
| C | 베넬 보스용 큰 그림 | — | — | — | 지금은 일반 크기 임시 그림. 작은 로봇을 키워 보스로 쓰지 않는다 |

### 잠입 기물 (`Assets/Game/Art/Placeholder/Scene/`, 투명 배경)

| 우선 | 파일 | 크기(px) | 기준점 | 레이어 | 프레임 | 사용 | 판독 요구 |
|---|---|---|---|---|---|---|---|
| A | `S_Crate.png` / `S_CrateS.png` / `S_CrateTall.png` | 48×32 / 32×32 / 48×48 | 아래 가운데 | 4 | 1 | 막는 엄폐 상자 (고체) | 밝은 테두리·경고 띠로 **배경 상자와 구분**. 높이 32px 정확히 |
| A | `S_BgBox.png` | 40×24 | 아래 가운데 | −8 | 1 | 통과 가능한 배경 상자 | 어둡고 테두리 없음 |
| A | `S_CctvMount.png` | 12×14 | 위 가운데 | 20 | 1 | CCTV 받침 | |
| A | `S_CctvHead.png` | 26×12 | (0.12, 0.5) 회전축 | 21 | 1 | CCTV 몸통 (코드가 회전) | 렌즈 쪽이 오른쪽 |
| A | `S_Lens.png` / `S_LensOff.png` | 4×4 | 가운데 | 22 | 1 | 렌즈 (코드가 색칠: 청록/노랑/빨강/꺼짐) | 흰색으로 그려 둘 것 |
| A | `S_CtrlBox.png` + `S_CtrlScreen.png` | 22×30 / 14×8 | 아래 가운데 / 가운데 | 3 / 4 | 1 | CCTV 제어함 (간섭 대상) | 화면은 흰색 (코드가 깜빡임) |
| A | `S_PowerBox.png` / `S_PowerBoxDead.png` | 26×34 | 아래 가운데 | 2 | 1 | 외부 전원 (출력 있음 / 없음) | 같은 모양, 출력 있는 쪽만 내부 단자가 밝게 |
| A | `S_IconPower.png` / `S_IconPowerEmpty.png` / `S_IconPowerBusy.png` | 11×13 | 가운데 | 23 | 1 | 전원 상태 표식 | **‘전력 있음/없음/사용 중’을 색 없이도 모양으로 구분** (빈 쪽은 사선) |
| A | `S_Tray.png` | 30×30 | 위 가운데 | 3 | 1 | 매달린 공구 쟁반 (소리 미끼) | |
| B | `S_ShutterPanel.png` + `S_Lamp.png` | 16×24 / 4×4 | 아래 가운데 / 가운데 | 3 / 4 | 1 | 셔터 제어판 | 램프는 흰색 |
| B | `S_AlarmLamp.png` + `S_AlarmBeam.png` | 14×12 / 96×24 | 가운데 / (0, 0.5) | 20 / 42 가산 | 1 | 경광등 | 빔은 가산 혼합용 반투명 그라데이션 |
| B | `S_WorkLamp.png` | 30×12 | 위 가운데 | 3 | 1 | 천장 작업등 (전력 복구 때 차례로 켜짐) | |
| B | `S_Junction.png` | 22×28 | 아래 가운데 | 2 | 1 | 베넬 복도 과부하 분전함 | |
| C | `S_DeadCable.png` | 40×10 | 아래 가운데 | −2 | 1 | 끊긴 굵은 전선 장식 | |

### 정전된 실험실 조사 소품 (C01, 레이어 −3~−1, 도트, 투명 배경, 아래 가운데)

| 우선 | 파일 | 크기 | 조사 반응 | 비고 |
|---|---|---|---|---|
| A | `S_BodyBoard.png` | 48×40 | **핵심 단서** (카메라 2배 확대) | 사람 몸은 선과 번호만, 해부 묘사 금지. 'E-D' 계열 표기 |
| A | `S_TankLabel.png` | 30×14 | **핵심 단서** (통 표식 E-D05-47) | 2배 확대에서 번호가 읽혀야 함 |
| A | `S_SignIsolation.png` | 64×16 | **핵심 단서** (문틈 너머 ‘격리 실험동’) | 한글 표지 |
| B | `S_RestraintBed.png` | 64×30 | 다가가기 꺼림 | 구속대 |
| B | `S_SurgLamp.png` | 40×26 | (장식, 천장) | 꺼진 수술등 |
| B | `S_ToolTray.png` | 28×30 | 만지는 동작 + 달그락 | |
| B | `S_PartsBin.png` | 32×26 | 소리만 (끼긱) | 기계 부품만 보이게 |
| B | `S_Lockers.png` | 64×48 | 짧은 말풍선 | 번호표 |
| B | `S_Jar0.png`, `S_Jar1.png` | 18×34, 18×42 | 소리만 (보글) | **불투명** — 안이 보이지 않게 |
| B | `S_ObsWindow.png` | 96×56 | 짧은 말풍선 | 유리 뒤 실루엣 일부만 |

### 배경 (원경·중경)
- C 우선: `L_Far.png`(640×260), `L_MidA/B.png`(320×200), `L_WallPanel.png`, `L_Floor.png` — 어두운 청회색·탁한 녹색으로 다시 칠한 임시본. 최종은 인체 개조 연구시설의 규모(높은 천장 배관, 격리 용기 줄, 관찰창 층)를 보여 주되 진행 대상보다 대비를 낮게.

---

## (이전) 오프닝·연출 아트 명세

### 인물 비율 (참고 이미지 3 `Docs/References/캐릭터 디자인 예시.png` 기준)
- 큰 머리·작은 몸, 머리가 전체 키의 약 40%. 옷·장신구 디테일은 머리와 상체에 몰고 다리는 짧게. 1px 어두운 외곽선, 밝은 쪽 테두리 하이라이트.
- **현재 캔버스 48×64 (실제 키 약 40~48px)** 에서는 참고 이미지 수준의 얼굴·옷 디테일을 넣기 어렵다. 디테일을 살리려면 **64×80 캔버스, 실제 키 56~64px(머리 24~28px)** 를 권장한다.
  키를 키우면 함께 바꿀 곳: `EWArtSpecs.ForPlaceholder` 의 frameW/H, `Art_<id>.asset` 의 Height, `PlayerController.Height`(충돌 높이 1.3유닛), 문 그림 높이(현재 80px)·사다리·낮은 천장. 바꾸기 전에 한 동작만 시험 교체해 화면에서 비율을 확인하길 권한다.
- 화면에서의 크기: 탐험 구도(1배)에서 인물은 화면 높이의 약 13~18%, 전투·가까운 구도(2배)에서 약 25~35%.

## 1. 루 — 도트 동작 (캔버스 48×64 / 프레임, 기준점 발바닥 가운데, 기본 오른쪽을 봄)

| # | 동작 | 파일 (`Assets/Game/Art/Placeholder/`) | 길이·반복 | 권장 프레임 | 연결 게임 상태 (Animator 상태 / 사용 장면) |
|---|---|---|---|---|---|
| 1 | 뒷모습 서 있기 (화면 안쪽을 봄, 얼굴 안 보임) | `Lu_BackIdle.png` | 1초 반복 | 2~4 (숨쉬기) | `BackIdle` / 불타는 마을 첫 장면 |
| 2 | 뒷모습 고개 돌림 | `Lu_BackLook.png` | 정지 | 1~2 | `BackLook` / ‘다... 끝난 거야...?’ |
| 3 | 절뚝이는 이동 | `Lu_Limp.png` | 1.1초 반복 (6fps) | 6~8: 성한 발 디딤 2 → 다친 발 끌기 2(몸 기울임) → 회복 2 | `Limp` / 마을·집. 이동 속도는 코드가 단계별로 바꿈(빠름→느림→보통) |
| 4 | 절뚝이다 멈춰 숨 고르기 | `Lu_LimpIdle.png` | 반복 2fps | 2 | `LimpIdle` |
| 5 | 고개 숙임(슬픔) | `Lu_Sad.png` | 반복 | 2 | `Sad` / 집 ‘나... 난....’ |
| 6 | 컴퓨터 앞 정지 → 손 뻗기 | `Lu_ReachPC.png` | 0.5초, 1회 | 2~3 | `ReachPC` |
| 7 | 접속 (빛이 몸에 번짐) | `Lu_Connect.png` | 반복 | 3~4 (빛 강도 증가, 머리카락 들림) | `Connect` (코드가 몸 색을 푸르게 바꾸고 축소·이동시켜 빨려 들어가게 함) |
| 8 | 떨어진 뒤 착지 충격 | `Lu_Land.png` | 정지 0.35초 | 1~2 | `Land` / 바탕화면 ‘아코!’, 실험 통 ‘으얏!’ |
| 9 | 몸 일으키기 | `Lu_GetUp.png` | 0.5초, 1회 | 3~4 | `GetUp` |
| 10 | 좌우 살피기 | `Lu_LookAround.png` | 1초 반복 | 4 (정면→뒤→정면→앞) | `LookAround` (방향 전환은 코드의 좌우 반전으로도 함) |
| 11 | 유리 두드리기 | `Lu_Knock.png` | 반복 10fps | 3 (주먹 뒤로→유리→반동) | `Knock` / 소리는 프레임과 무관하게 0.18~0.24초 간격 |
| 12 | 겁먹고 뒷걸음질 | `Lu_BackAway.png` | 반복 | 4 (팔 들어 막기, 뒤로 걸음) | `BackAway` |
| 13 | 겁먹고 떨기 | `Lu_Fear.png` | 반복 8fps | 2 | `Fear` |
| 14 | 눈 감고 방전 준비 | `Lu_ChargeEyes.png` | 1.5초, 1회(마지막 유지) | 3~4 (눈 질끈, 주먹, 몸이 밝아짐) | `ChargeEyes` |
| 15 | 방전 | `Lu_Discharge.png` (기존) | 반복 | 2~4 | `Discharge` |
| 16 | 방전 후 회복 (숨 몰아쉬기) | `Lu_Recover.png` | 반복 2fps | 2~3 | `Recover` |
| 17 | 문틈 비집고 통과 (옆으로 몸 좁히기) | `Lu_Squeeze.png` | 반복 6fps | 4 | `Squeeze` / 문짝 뒤로 들어가며 사라짐 |
| — | 기존 동작 | Idle·Walk·Jump·Fall·Climb·Hurt·Down·Sit·Flick·Point·Swing·Kick·Charge·Carry | | GDD 13.6 참고 | |

주의: 루의 앞머리·흉터·소지품처럼 좌우가 중요한 요소가 있으면 왼쪽용 컨트롤러가 필요하다 (`Art_lu.asset` 의 Left Controller).

## 2. 연구원 두 명 (캔버스 48×64, 기준점 발바닥, 기본 오른쪽)

`Researcher_*.png` (연구원 1), `Researcher2_*.png` (연구원 2). 두 사람은 머리 모양·체형·색으로 구분한다.

| 동작 | 프레임 | 상태 |
|---|---|---|
| 대기 / 걷기 | 2 / 4 | `Idle` / `Walk` |
| 말하기(손짓) | 2 | `Talk` |
| 제압봉 꺼내기 | 3 (허리에서 꺼냄 → 켜짐) | `BatonOut` (1회) |
| 제압봉 든 대기 | 2 (끝이 지지직) | `BatonIdle` |
| 제압봉 든 걷기 | 4 | `BatonWalk` |
| 밀려남 / 쓰러짐 | 1~2 / 1 | `Hurt` / `Down` (쓰러진 채 유지 — 생사를 단정하는 표현은 피함) |

## 3. 스탠딩 일러스트 (고해상도, 도트 아님)

| 항목 | 규격 |
|---|---|
| 파일 | `Assets/Game/Art/Portraits/<인물>_<표정>.png` — 예: `lu_despair.png` |
| 캔버스 | **600×900** 권장 (비율 2:3 유지하면 크기 자유). 투명 배경 |
| 기준점 | 아래 가운데 (구성 도구가 Bottom Center 로 설정). 허리 아래는 대화창에 가려도 됨 |
| 얼굴 위치 | 얼굴 중심이 그림 위에서 약 30~40% 지점 (화면 높이의 82% 로 표시되므로 대화창 위에 얼굴이 온다) |
| 방향 | 루는 왼쪽 자리(오른쪽을 봄), 그 밖의 인물은 오른쪽 자리(왼쪽을 봄). 좌우 반전하지 않는다 |
| 가져오기 | Bilinear 필터, 밉맵 없음, 고품질 압축 — 구성 도구가 자동 설정 |
| 연결 | 대사 줄의 `"e": "<표정>"` (필요하면 `"p": "<인물>"`, `"pos": "L"/"R"`, `"hide": true`) |

필요한 표정 (임시 그림 있음 → 교체 대상):

| 인물 | 표정 파일 | 쓰는 곳 |
|---|---|---|
| 루 | `neutral`, `sad`, `despair`, `surprised`, `scared`, `determined`, `smile`, `pained`, `angry_cry` | 집(슬픔·절망), 바탕화면(놀람), 실험 통(놀람·겁먹음·울며 화냄), 탈출(결심), 3장(웃음 등) |
| 연구원 1 (`researcher`) | `smile`, `neutral`, `smirk` | ‘꼬마야, 진정해~’ |
| 연구원 2 (`researcher2`) | `neutral`, `smirk`, `surprised` | ‘괜찮아. 진정하렴?’ |
| 베넬 (`venelle`) | `gentle`, `blank`, `excited`, `confused`, `quiet` | 3장 만남·협상·휴식 (GDD 10.2) |

## 4. 장면 배경 (현재 임시 블록아웃 — `Assets/Game/Art/Placeholder/Scene/`)

연구소 타일을 색만 바꿔 마을로 쓰지 않았다. 아래는 모두 **직접 만든 임시 블록아웃**이며 최종 그림이 필요하다.

### 4.1 불타는 판타지 마을 (P01)
| 레이어 | 파일 | 규격 | 시차(카메라 따라감) |
|---|---|---|---|
| 하늘 | `V_Sky.png` | 4×360 세로 그라데이션 (가로로 늘림) | 1.0 (고정) |
| 원경: 멀리 불타는 성벽·탑·지붕·연기 기둥 | `V_Far.png` | 640×220, 좌우 이어짐(타일) | 0.75 |
| 중경: 판타지 목조 집 3종 (일부 무너짐·창의 불빛) | `V_HouseA/B/C.png` | 64~96 × 76~108 | 0.35 (어둡게) |
| 가까운 폐가 (크게, 실루엣) | 위 그림 1.6배 | | 0.12 |
| 근경 소품: 무너진 벽·잔해 3크기·쓰러진 수레·부서진 울타리 | `V_RuinWall`, `V_Rubble*`, `V_Cart`, `V_Fence` | 48~120 × 20~48 | 0 |
| 루의 작은 집 (숨겨진, 무너진) | `V_LuHouse.png` | 112×96, 문 구멍이 가운데보다 왼쪽 | 0 |
| 전경: 그을린 기둥·잔해 실루엣 | `V_ForeBeam.png`, `V_ForeRubble.png` | 80×200 / 200×56 (좌우 이어짐) | -0.4 / -0.25 |
| 땅 | `V_Ground.png`(윗면), `V_GroundFill.png`(채움) | 32×32 | |
| 효과 | `FX_Fire`(16×24×6), `FX_FireBig`(32×48×6), `FX_Smoke`(32×32×4), `FX_Ember`·`FX_Ash`(2×2 입자) | | |
요청: 사실적 화풍(참고 이미지 1)을 도트와 섞지 말고 같은 도트 규칙으로. 가까울수록 어둡게, 멀수록 하늘색에 섞이게.

### 4.2 무너진 작은 집 실내 (P02)
`H_Wall`(32×32 벽), `H_Floor`(32×32 바닥), `H_WallHole`(96×80 벽 구멍 — 밖의 불빛), `H_Bed`(64×32), `H_Desk`(56×32),
`H_PC`(40×32 ×4: 꺼짐/켜짐/밝음/최대 빛 — **판타지 세계의 낡은 컴퓨터**), `H_MaterialsA/B`(64×20 널브러진 자재), `H_Beam`(128×64 무너진 대들보, 전경), `H_Shelf`(40×48).

### 4.3 친구 컴퓨터 바탕화면 (P03)
`D_Wallpaper`(640×360, 실제 운영체제를 흉내 내지 않는 기본 무늬), `D_Bar`(32×32 작업 막대 = 바닥), `D_Icon_folder/doc/trash/msg`(24×24, 2배로 표시).
강제 전송 창은 UI(코드)로 그린다 — 디자인 변경은 `UIRoot.cs` 의 `SystemWindow`.

### 4.4 실험실 (C01·C02 이후 공통)
| 파일 | 규격 | 상태 |
|---|---|---|
| `L_TankBack.png` / `L_TankGlass.png` | 80×112 ×2 | 0 정상 / 1 파손. 뒤판(루 뒤)과 앞 유리(루 앞, 반투명)를 나눔. 전극 위치: 뚜껑 아래 3곳 |
| `L_Door.png` | 48×80 ×3 | 0 닫힘 / 1 열림 / 2 과전류·기계 손상으로 비틀려 멈춤(가운데 좁은 틈) |
| `L_Console.png` | 48×40 ×3 | 켜짐 / 꺼짐 / 파손 |
| `Prop_CeilingLight.png` | 32×8 ×2 | 켜짐 / 꺼짐 (매단 줄은 코드) |
| `L_Far`, `L_MidA/B` | 640×260, 320×200 | 원경(먼 층·조명) / 중경(장비·배관) |
| `L_WallPanel`, `L_Floor`, `L_FloorFill` | 64×64, 32×32 | 안쪽 벽 / 바닥 윗면 / 바닥 아래 채움 |
| `L_ForePillar`, `L_ForePipe` | 40×240, 240×36 | 전경 실루엣 |
| `L_FallenPanel`, `L_DebrisS`, `L_Scorch` | | 방전 후 파편·그을음 |
| `L_Chamber` | 128×96 | 유리 너머 방(실루엣만, 자세한 신체 묘사 없음) |
| `L_Window`, `L_Cables`, `L_Crane` | | 깨진 관찰창 / 통 케이블 / 화물 크레인 |

## 5. 적·보스 (턴제 전투 화면은 없어졌다 — 같은 필드에서 실시간으로 쓰이는 그림)

| 대상 | 상태 |
|---|---|
| 순찰 경비 / 포획 드론 / 운반 로봇 | Foozle 드로이드로 **임시 대용** (프레임 전부 사용). 고유 외형 필요: 대기 4, 이동 6, 공격 6(예고 동작 포함), 피격 2, 파괴 6 프레임 (GDD 13.6). 순찰 경비는 **방어 자세 / 자세 무너짐** 구분 그림이 있으면 좋다 (현재는 코드가 기울이고 떨게 함) |
| 베넬 보스 | **원화 없음.** 일반 인물 크기 임시 그림. 작은 로봇을 확대해 보스로 꾸미지 않았다. 필요: 입장(높은 곳에서 뛰어내림), 예고(팔 장치에 약품 섞기), 강공격(크게 뿌림), 피격, 쓰러짐 |

## 6. 소리 (그림은 아니지만 함께 교체)

`Assets/Game/Audio/Generated/*.wav` 는 직접 합성한 **임시 음원**이다 (목록·용도: `Docs/ASSET_INVENTORY.md`). 같은 이름으로 덮어쓰고 EW > 1 을 실행하면 연결된다.
