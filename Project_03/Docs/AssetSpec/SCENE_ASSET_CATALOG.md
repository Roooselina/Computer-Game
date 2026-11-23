# SCENE ASSET CATALOG — 씬별 에셋 목록

> 분석일 2026-11-17 · 기준: 현재 프로젝트 파일 (코드·씬·데이터·이미지 메타). **현재 구현 기준**이다. 재기획(Docs/Redesign)으로 바뀔 수 있는 요구는 따로 표시했다.
> 상태: EXISTING 정상 사용 · PLACEHOLDER 임시 · MISSING 필요하지만 없음 · REPLACE 있으나 교체 필요 · OPTIONAL 선택 / 우선순위 P0~P3. **추정**은 따로 표시. 이미지를 만들거나 프로젝트 파일을 바꾸지 않았다.

씬 파일에 실제로 연결된 스프라이트(`m_Sprite` 참조)를 모두 옮겼다. 맵은 Tilemap 이 아니라 개별 스프라이트로 조립돼 있어, **배경은 레이어별 개별 그림, 지형은 반복 타일, 소품은 개별 오브젝트**로 만든다 (한 장짜리 통짜 배경은 이전에 적용했다가 되돌린 이력이 있음 — PIXEL_RESTORE_REPORT).

`Tile_Pixel` = 1×1 흰 픽셀을 코드가 색칠·늘린 사각형 (벽·바닥 채움·빛 띠). 이미지 제작이 아니라 **지형 그림으로 바꿀지 결정이 필요한 자리**다 (아래 ‘코드 도형’ 수).

적 수는 방 정의 코드 정규식 집계 (근사). 인물은 씬의 ActorSpawn. 이펙트는 씬에 놓인 것만 (전투 이펙트는 코드가 생성 — UI·이펙트 목록 참고).

## 씬 진행 순서

Title → P01 → P02 → P03 → C01 → C02 → C03 → M09 → C04 → C05 → M14 → C06 → M13 → M15 → M16 → M17 → M18 → M19 → M20 → M21 → M22 → M23 → M24 → M25 → M26 → M27 → M28 → M29 → M30 → M31 → M32 → M33 → M34 → M35 → M36 → M38 → M37 → M39 → M40 → M41 → M42 → M43 → M44 → M45 → M46 → M47

(장별 연결도: Docs/Redesign/GAME_MASTER_DESIGN.docx PART 04)

---

## 타이틀 — Title 타이틀 (`Title.unity`)

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| (없음) | | | | |
| 배경 레이어 | `back` (112×272) | 패럴랙스 배경 | EXISTING | P1 |
| 배경 레이어 | `middle` (256×272) | 패럴랙스 배경 | EXISTING | P1 |

---

## 프롤로그 — P01 불타는 마을 (`P01_BurningVillage.unity`)

목표: 연출 장면 · 스프라이트 렌더러 66개 · 코드 도형 0곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 배경 레이어 | `V_Far` (640×220) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `V_ForeBeam` (80×200, ×3 배치) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `V_ForeRubble` (200×56) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `V_HouseA` (80×100, ×5 배치) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `V_HouseB` (64×80, ×2 배치) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `V_HouseC` (96×112, ×5 배치) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `V_Sky` (4×360) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `V_Ground` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `V_GroundFill` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `V_Cart` (64×40) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `V_Fence` (48×24) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `V_LuHouse` (112×96) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `V_RubbleL` (120×40, ×2 배치) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `V_RubbleM` (80×32, ×2 배치) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `V_RubbleS` (48×24) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `V_RuinWall` (64×48, ×2 배치) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Ash` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_Ember` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_FadeDown` (4×64) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_Fire` (96×24, ×42 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_FireBig` (192×48, ×70 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_Glow` (64×64, ×12 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_HazeBand` (4×64, ×2 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_Smoke` (128×32, ×25 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |

---

## 프롤로그 — P02 무너진 작은 집 (`P02_RuinedHouse.unity`)

목표: 연출 장면 · 스프라이트 렌더러 8개 · 코드 도형 1곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 배경 레이어 | `H_RuinBack` (448×288) — 뒤쪽 벽이 날아간 작은 집 (448×288) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `H_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `H_Bed` (64×32) — 침대 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `H_Desk` (56×32) — 책상 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `H_MaterialsA` (64×20) — 널브러진 자재 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `H_PC` (160×32, ×5 배치) — 판타지 세계의 낡은 컴퓨터 (꺼짐/켜짐/밝음/최대 빛) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `H_Shelf` (40×48) — 선반 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×1 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 프롤로그 — P03 컴퓨터 안 (`P03_FriendDesktop.unity`)

목표: 안내서·연습·접속 기록을 살펴본다 · 스프라이트 렌더러 4개 · 코드 도형 0곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 배경 레이어 | `D_Desktop` (640×352) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `D_Taskbar` (640×32) — 컴퓨터 안 작업표시줄 (유일한 막대) | 바닥·벽·발판 | PLACEHOLDER | P0 |
| 오브젝트 | `D_Icon_guide` (24×24) — 안내서 아이콘 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `D_Icon_status` (24×24) — 상태 아이콘 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 이펙트 | `FX_LightDot` (64×16) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |

---

## 1장 — C01 실험실 (`C01_TankLab.unity`)

목표: 실험실을 빠져나간다 · 스프라이트 렌더러 85개 · 코드 도형 17곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 researcher2 | CH_NPC_002 연구원 2 (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 op_r_in · 숨김 없음 | PLACEHOLDER | P1 |
| 인물 researcher | CH_NPC_001 연구원 1 (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 op_r_in · 숨김 없음 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×5 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Cables` (96×24) — 통 케이블 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `L_Console` (144×40, ×8 배치) — 콘솔 (켜짐/꺼짐/파손) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `L_DebrisS` (48×20) — 파편 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `L_Door` (144×80, ×4 배치) — 실험실 문 (닫힘/열림/비틀려 멈춤) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `L_FallenPanel` (72×28) — 방전 후 떨어진 패널 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `L_Scorch` (64×16) — 그을음 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `L_TankBack` (160×112, ×3 배치) — 실험 통 뒤판 (정상/파손) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `L_TankGlass` (160×112, ×3 배치) — 실험 통 앞 유리 (반투명, 정상/파손) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_CeilingLight` (64×8, ×9 배치) — 천장등 (켜짐/꺼짐) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Clothes` (32×32) — 옷 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_EmergencyLight` (32×16, ×2 배치) — 비상등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_AlarmBeam` (96×24, ×3 배치) — 경광등 빔 (가산, 반투명) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_AlarmLamp` (14×12, ×3 배치) — 경광등 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_BodyBoard` (48×40, ×2 배치) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34, ×2 배치) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48, ×3 배치) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56, ×2 배치) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_PartsBin` (32×26) — 부품 수거함 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_RestraintBed` (64×30) — 구속대 침대 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_SignIsolation` (64×16) — 핵심 단서: ‘격리 실험동’ 한글 표지 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_SurgLamp` (40×26) — 꺼진 수술등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_TankLabel` (30×14) — 핵심 단서: 통 표식 E-D05-47 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_ToolTray` (28×30) — 도구 쟁반 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_WorkLamp` (30×12, ×4 배치) — 천장 작업등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×16 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_Smoke` (128×32) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×17 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 1장 — C02 격리 실험동 복도 (`C02_EscapeWing.unity`)

목표: 복도 끝으로 · 스프라이트 렌더러 55개 · 코드 도형 14곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 적 | CCTV(오브젝트) 2 | 감시 구역 | REPLACE | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `Tile_Platform` (32×8) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Door` (144×80) — 실험실 문 (닫힘/열림/비틀려 멈춤) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_AlarmBeam` (96×24, ×4 배치) — 경광등 빔 (가산, 반투명) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_AlarmLamp` (14×12, ×4 배치) — 경광등 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvHead` (26×12, ×2 배치) — CCTV 몸통 (코드 회전, 렌즈 오른쪽) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvMount` (12×14, ×2 배치) — CCTV 받침 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CrateS` (32×32) — 작은 엄폐 상자 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CrateSolid` (48×32) — 올라서는 고체 상자 (윗면 밝은 띠·노란 사선) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideRack` (56×44, ×2 배치) — 뒤에 숨는 전경 엄폐물 (선반) — 아래 1.2유닛이 몸을 가림 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideSacks` (64×36) — 뒤에 숨는 전경 엄폐물 (자루) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lens` (4×4, ×4 배치) — CCTV 렌즈 (흰색, 코드가 색칠) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_LensOff` (4×4, ×2 배치) — 꺼진 렌즈 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56, ×2 배치) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×4 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×14 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 2장 — C03 설비실 · 자료실 (`C03_SpecimenArchive.unity`)

목표: 자료실을 지나 화물 적재장으로 · 스프라이트 렌더러 60개 · 코드 도형 15곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 적 | 순찰 경비(CH_ENEMY_001) 4, CCTV(오브젝트) 1 | 감시 구역 | REPLACE | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `Tile_Platform` (32×8, ×3 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Chamber` (128×96) — 유리 너머 방 (실루엣) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `L_Door` (144×80, ×4 배치) — 실험실 문 (닫힘/열림/비틀려 멈춤) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Desk` (48×32) — 책상 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Doc` (16×16) — 문서 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Door` (96×64) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Ladder` (32×32) — 사다리 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_PosterEmblem` (48×48, ×4 배치) — 베스페르 표장 포스터 — 현재 ‘LOGO ?’ 빈 자리 (사용자 원안 필요) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Snack` (16×16) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvHead` (26×12) — CCTV 몸통 (코드 회전, 렌즈 오른쪽) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvMount` (12×14) — CCTV 받침 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CrateS` (32×32) — 작은 엄폐 상자 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CrateSolid` (48×32) — 올라서는 고체 상자 (윗면 밝은 띠·노란 사선) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideRack` (56×44) — 뒤에 숨는 전경 엄폐물 (선반) — 아래 1.2유닛이 몸을 가림 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideSacks` (64×36) — 뒤에 숨는 전경 엄폐물 (자루) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPower` (11×13, ×2 배치) — 전원 상태 표식: 전력 있음 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerBusy` (11×13) — 전원 상태 표식: 사용 중 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerEmpty` (11×13) — 전원 상태 표식: 없음 (사선) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lamp` (4×4) — 램프 (흰색) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lens` (4×4, ×2 배치) — CCTV 렌즈 (흰색, 코드가 색칠) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_LensOff` (4×4) — 꺼진 렌즈 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Lockers` (64×48, ×2 배치) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56, ×2 배치) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_PowerBox` (26×34) — 외부 전원 (출력 있음) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_ShutterPanel` (16×24) — 셔터 제어판 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Tray` (30×30) — 매달린 공구 쟁반 (소리 미끼) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×15 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 2장 — M09 직원 식당 (`M09_StaffCafeteria.unity`)

목표: 선택 탐색 · 스프라이트 렌더러 26개 · 코드 도형 8곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePillar` (40×240, ×2 배치) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Desk` (48×32, ×2 배치) — 책상 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Door` (96×64) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Lever` (32×32) — 레버 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Vending` (96×64, ×4 배치) — 자판기 (켜짐/꺼짐/고장) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×8 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 2장 — C04 화물 적재장 (`C04_CargoBay.unity`)

목표: 적재장을 지나 배양실로 · 스프라이트 렌더러 51개 · 코드 도형 12곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 적 | 순찰 경비(CH_ENEMY_001) 2, 포획 드론(CH_ENEMY_002) 1, 운반 로봇(CH_ENEMY_003) 1, CCTV(오브젝트) 1 | 감시 구역 | REPLACE | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32, ×3 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×6 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `Tile_Platform` (32×8) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Crane` (160×120) — 화물 크레인 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Ladder` (32×32) — 사다리 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_PowerSocket` (96×32, ×16 배치) — 전원선 입구 (입구/빛/끊김) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Snack` (16×16, ×2 배치) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvHead` (26×12) — CCTV 몸통 (코드 회전, 렌즈 오른쪽) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvMount` (12×14) — CCTV 받침 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CrateS` (32×32) — 작은 엄폐 상자 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CrateSolid` (48×32) — 올라서는 고체 상자 (윗면 밝은 띠·노란 사선) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideRack` (56×44) — 뒤에 숨는 전경 엄폐물 (선반) — 아래 1.2유닛이 몸을 가림 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lens` (4×4, ×2 배치) — CCTV 렌즈 (흰색, 코드가 색칠) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_LensOff` (4×4) — 꺼진 렌즈 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_Tray` (30×30) — 매달린 공구 쟁반 (소리 미끼) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `full_decor_tiles` (96×96, ×3 배치) | 배치 소품·장치 | EXISTING | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×12 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 3장 — C05 배양실 (`C05_CultureRoom.unity`)

목표: 연결 복도로 · 스프라이트 렌더러 51개 · 코드 도형 12곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 적 | 순찰 경비(CH_ENEMY_001) 3, 운반 로봇(CH_ENEMY_003) 1, CCTV(오브젝트) 1 | 감시 구역 | REPLACE | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `Tile_Platform` (32×8) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Chamber` (128×96, ×3 배치) — 유리 너머 방 (실루엣) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Bench` (48×24) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Door` (96×64) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_ExposedWire` (128×32, ×5 배치) — 노출 전선 (4프레임) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Ladder` (32×32) — 사다리 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Snack` (16×16, ×3 배치) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvHead` (26×12) — CCTV 몸통 (코드 회전, 렌즈 오른쪽) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvMount` (12×14) — CCTV 받침 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CrateSolid` (48×32, ×2 배치) — 올라서는 고체 상자 (윗면 밝은 띠·노란 사선) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CrateTall` (48×48) — 큰 엄폐 상자 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideRack` (56×44) — 뒤에 숨는 전경 엄폐물 (선반) — 아래 1.2유닛이 몸을 가림 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideSacks` (64×36) — 뒤에 숨는 전경 엄폐물 (자루) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPower` (11×13, ×2 배치) — 전원 상태 표식: 전력 있음 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerBusy` (11×13) — 전원 상태 표식: 사용 중 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerEmpty` (11×13) — 전원 상태 표식: 없음 (사선) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lens` (4×4, ×2 배치) — CCTV 렌즈 (흰색, 코드가 색칠) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_LensOff` (4×4) — 꺼진 렌즈 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Lockers` (64×48, ×2 배치) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_PowerBox` (26×34) — 외부 전원 (출력 있음) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `full_decor_tiles` (96×96, ×3 배치) | 배치 소품·장치 | EXISTING | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×12 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 3장 — M14 약품 온실 (`M14_ChemGreenhouse.unity`)

목표: 선택 탐색 · 스프라이트 렌더러 30개 · 코드 도형 9곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 적 | 순찰 경비(CH_ENEMY_001) 1, 포획 드론(CH_ENEMY_002) 1 | 감시 구역 | REPLACE | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePillar` (40×240, ×2 배치) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Door` (96×64) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Puddle` (64×8) — 웅덩이 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Snack` (16×16, ×2 배치) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `full_decor_tiles` (96×96, ×3 배치) | 배치 소품·장치 | EXISTING | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×9 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 3장 — C06 연결 복도 (`C06_LongCorridor.unity`)

목표: 베넬 · 스프라이트 렌더러 46개 · 코드 도형 12곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 venelle | CH_BOSS_001 베넬 (본명 아이비) (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 c3_boss_done | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Door` (144×80, ×8 배치) — 실험실 문 (닫힘/열림/비틀려 멈춤) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_AlarmBeam` (96×24, ×3 배치) — 경광등 빔 (가산, 반투명) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_AlarmLamp` (14×12, ×3 배치) — 경광등 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Junction` (22×28) — 과부하 분전함 (베넬 복도) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Lockers` (64×48, ×2 배치) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_WorkLamp` (30×12, ×2 배치) — 천장 작업등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×7 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×12 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 3장 — M13 혼합실 (`M13_MixingRoom.unity`)

목표: 관찰 휴게실로 · 스프라이트 렌더러 33개 · 코드 도형 9곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePillar` (40×240, ×2 배치) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `Tile_Platform` (32×8) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Chamber` (128×96, ×2 배치) — 유리 너머 방 (실루엣) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Puddle` (64×8) — 웅덩이 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Snack` (16×16) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `chemical_pot_item` (384×32, ×4 배치) | 배치 소품·장치 | EXISTING | P3 |
| 오브젝트 | `full_decor_tiles` (96×96, ×2 배치) | 배치 소품·장치 | EXISTING | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×9 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 3장 — M15 관찰 휴게실 (`M15_ObservationLounge.unity`)

목표: 오른쪽 화물 승강장으로 · 스프라이트 렌더러 40개 · 코드 도형 12곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 venelle | CH_BOSS_001 베넬 (본명 아이비) (대기 `Sit`, 쓰러짐 `Down`) | 등장 조건 venelle_bench · 숨김 c3_joined | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32, ×3 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×5 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `Tile_Platform` (32×8) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Bench` (48×24) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Door` (96×64) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Ladder` (32×32) — 사다리 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_PowerSocket` (96×32, ×8 배치) — 전원선 입구 (입구/빛/끊김) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Snack` (16×16, ×2 배치) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Water` (16×16) — 물 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48, ×2 배치) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×12 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 4장 — M16 화물 승강장 (`M16_CargoLift.unity`)

목표: 승강기로 지하로 내려간다 · 스프라이트 렌더러 55개 · 코드 도형 14곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 적 | 순찰 경비(CH_ENEMY_001) 1, 포획 드론(CH_ENEMY_002) 1 | 감시 구역 | REPLACE | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×5 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Crane` (160×120) — 화물 크레인 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Bench` (48×24) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_PosterEmblem` (48×48) — 베스페르 표장 포스터 — 현재 ‘LOGO ?’ 빈 자리 (사용자 원안 필요) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Snack` (16×16, ×2 배치) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_BgBox` (40×24, ×4 배치) — 통과 가능한 배경 상자 (어둡게, 테두리 없음) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CrateSolid` (48×32) — 올라서는 고체 상자 (윗면 밝은 띠·노란 사선) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideRack` (56×44) — 뒤에 숨는 전경 엄폐물 (선반) — 아래 1.2유닛이 몸을 가림 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56, ×2 배치) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_WorkLamp` (30×12, ×2 배치) — 천장 작업등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_LiftDeck` (96×14) — 승강기 바닥 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_LiftPanel` (32×32, ×2 배치) — 승강기 조작반 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_LiftShaft` (112×120) — 승강로 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_SealShutter` (48×96) — 위층 봉쇄 셔터 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_Glow` (64×64) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×4 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×14 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 4장 — M17 폐기소 거처 (`M17_WasteShelter.unity`)

목표: F 와 교섭한다 (과자 10개 / 펌프 수리 / 충돌) · 스프라이트 렌더러 48개 · 코드 도형 13곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 f | CH_NPC_003 F (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 없음 | PLACEHOLDER | P1 |
| 인물 kid008 | CH_NPC_004 008 (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 없음 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Door` (96×64, ×3 배치) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48, ×4 배치) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_PartsBin` (32×26) — 부품 수거함 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_Barricade` (72×44, ×2 배치) — 폐기소 바리케이드 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_CrateBed` (48×20, ×2 배치) — 상자 침대 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_Jug` (32×24, ×9 배치) — 물통 (빈/찬) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_LaundryLine` (80×34) — 빨랫줄 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_Part` (14×12, ×3 배치) — 새 이음쇠 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Stove` (48×30, ×3 배치) — 화로 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Tally` (40×24) — 눈금 기록 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_Tarp` (96×20) — 방수포 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×4 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×13 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 4장 — M18 물 펌프실 (`M18_PumpRoom.unity`)

목표: 배수 → 차단기 → 이음쇠 → 가동 · 스프라이트 렌더러 43개 · 코드 도형 15곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePillar` (40×240, ×2 배치) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×6 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Door` (96×64) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPower` (11×13) — 전원 상태 표식: 전력 있음 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Breaker` (48×36, ×3 배치) — 차단기 손잡이 (내림/올림) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Part` (14×12) — 새 이음쇠 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_PartShelf` (40×36) — 부품 선반 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_PipeH` (16×16) — 가로 배관 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_Pump` (256×48, ×5 배치) — 물 펌프 (고장 정지/정상 정지/가동 2) — 이음쇠 위치 프레임 (44,33)px | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Valve` (72×32, ×4 배치) — 배수 밸브 (회전 3) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_WaterFill` (8×8) — 물 채움 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_WaterTop` (64×6, ×4 배치) — 고인 물 윗면 (가로 반복) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×15 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 4장 — M19 폐품 시장 (`M19_JunkMarket.unity`)

목표: 선택 탐색 · 스프라이트 렌더러 30개 · 코드 도형 11곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 survivorB | CH_NPC_006 생존자 B (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 없음 | PLACEHOLDER | P0 |
| 인물 survivorA | CH_NPC_005 생존자 A (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 없음 | PLACEHOLDER | P0 |
| 인물 kid008 | CH_NPC_004 008 (대기 `Draw`, 쓰러짐 `Down`) | 등장 조건 waste_met · 숨김 없음 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Door` (96×64) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_PosterEmblem` (48×48) — 베스페르 표장 포스터 — 현재 ‘LOGO ?’ 빈 자리 (사용자 원안 필요) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_ChalkLines` (96×10, ×3 배치) — 008의 분필 선 (여덟 개/한 줄) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Lantern` (24×20, ×3 배치) — 등불 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_StallA` (64×48) — 시장 노점 A | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_StallB` (64×48) — 시장 노점 B | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×11 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 5장 — M20 유지보수 터널 (`M20_MaintTunnel.unity`)

목표: 낮은 천장은 웅크려서 · 스프라이트 렌더러 55개 · 코드 도형 15곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 적 | 순찰 경비(CH_ENEMY_001) 2, CCTV(오브젝트) 1 | 감시 구역 | REPLACE | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×5 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_ExposedWire` (128×32, ×5 배치) — 노출 전선 (4프레임) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Puddle` (64×8) — 웅덩이 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Snack` (16×16) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvHead` (26×12) — CCTV 몸통 (코드 회전, 렌즈 오른쪽) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvMount` (12×14) — CCTV 받침 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CrateS` (32×32) — 작은 엄폐 상자 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideRack` (56×44) — 뒤에 숨는 전경 엄폐물 (선반) — 아래 1.2유닛이 몸을 가림 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideSacks` (64×36) — 뒤에 숨는 전경 엄폐물 (자루) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPower` (11×13, ×2 배치) — 전원 상태 표식: 전력 있음 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerBusy` (11×13) — 전원 상태 표식: 사용 중 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerEmpty` (11×13) — 전원 상태 표식: 없음 (사선) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lens` (4×4, ×2 배치) — CCTV 렌즈 (흰색, 코드가 색칠) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_LensOff` (4×4) — 꺼진 렌즈 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56, ×2 배치) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_PowerBox` (26×34) — 외부 전원 (출력 있음) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Tray` (30×30) — 매달린 공구 쟁반 (소리 미끼) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_WorkLamp` (30×12, ×3 배치) — 천장 작업등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×7 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×15 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 5장 — M21 정비 작업장 (`M21_Workshop.unity`)

목표: 새 기술 시험 · 스프라이트 렌더러 39개 · 코드 도형 12곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Console` (144×40) — 콘솔 (켜짐/꺼짐/파손) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Bench` (48×24) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_CeilingLight` (64×8, ×6 배치) — 천장등 (켜짐/꺼짐) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Doc` (16×16) — 문서 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_RestoreButton` (32×32) — 전원 복구 버튼 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_BodyBoard` (48×40, ×4 배치) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_PartsBin` (32×26) — 부품 수거함 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_ToolTray` (28×30) — 도구 쟁반 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_TestRig` (56×22) — 출력 시험대 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×2 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×12 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 5장 — M22 컨베이어 홀 (`M22_ConveyorHall.unity`)

목표: 출구 문 — 전원 복구 또는 환기 통로 · 스프라이트 렌더러 65개 · 코드 도형 17곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 적 | 순찰 경비(CH_ENEMY_001) 4, CCTV(오브젝트) 1 | 감시 구역 | REPLACE | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `Tile_Platform` (32×8, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Crane` (160×120) — 화물 크레인 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Battery` (32×32) — 배터리 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Computer` (128×32, ×8 배치) — 컴퓨터 (켜짐/꺼짐/부팅/메시지) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Desk` (48×32, ×2 배치) — 책상 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Door` (96×64, ×3 배치) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_ExposedWire` (128×32, ×5 배치) — 노출 전선 (4프레임) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Ladder` (32×32, ×2 배치) — 사다리 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_RestoreButton` (32×32) — 전원 복구 버튼 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Snack` (16×16) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvHead` (26×12) — CCTV 몸통 (코드 회전, 렌즈 오른쪽) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvMount` (12×14) — CCTV 받침 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideRack` (56×44) — 뒤에 숨는 전경 엄폐물 (선반) — 아래 1.2유닛이 몸을 가림 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideSacks` (64×36) — 뒤에 숨는 전경 엄폐물 (자루) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPower` (11×13, ×2 배치) — 전원 상태 표식: 전력 있음 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerBusy` (11×13) — 전원 상태 표식: 사용 중 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerEmpty` (11×13) — 전원 상태 표식: 없음 (사선) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34, ×2 배치) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Junction` (22×28) — 과부하 분전함 (베넬 복도) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Lens` (4×4, ×2 배치) — CCTV 렌즈 (흰색, 코드가 색칠) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_LensOff` (4×4) — 꺼진 렌즈 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56, ×2 배치) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_PowerBox` (26×34, ×3 배치) — 외부 전원 (출력 있음) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_PowerBoxDead` (26×34) — 외부 전원 (출력 없음) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Belt` (128×10, ×5 배치) — 컨베이어 벨트 (가로 반복 4) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_BeltCrate` (20×16, ×4 배치) — 벨트 위 상자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_BeltCtrl` (72×32, ×4 배치) — 벨트 제어반 (정/역/정지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×17 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 5장 — M23 수직 배선통로 (`M23_VerticalShaft.unity`)

목표: 세 층을 올라 따돌린다 · 스프라이트 렌더러 61개 · 코드 도형 11곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 적 | 순찰 경비(CH_ENEMY_001) 5, 포획 드론(CH_ENEMY_002) 1 | 감시 구역 | REPLACE | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePillar` (40×240, ×2 배치) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×13 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Cables` (96×24) — 통 케이블 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `L_Door` (144×80, ×4 배치) — 실험실 문 (닫힘/열림/비틀려 멈춤) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Door` (96×64, ×2 배치) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Ladder` (32×32, ×2 배치) — 사다리 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_PowerSocket` (96×32, ×8 배치) — 전원선 입구 (입구/빛/끊김) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Puddle` (64×8) — 웅덩이 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Snack` (16×16) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_HideRack` (56×44) — 뒤에 숨는 전경 엄폐물 (선반) — 아래 1.2유닛이 몸을 가림 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Junction` (22×28) — 과부하 분전함 (베넬 복도) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ShutterPanel` (16×24) — 셔터 제어판 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_WorkLamp` (30×12, ×3 배치) — 천장 작업등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_Belt` (128×10, ×5 배치) — 컨베이어 벨트 (가로 반복 4) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×6 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×11 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 6장 — M24 공동 세탁실 (`M24_Laundry.unity`)

목표: 숙소 복도 끝 수용동 연결문 · 스프라이트 렌더러 42개 · 코드 도형 14곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Door` (144×80) — 실험실 문 (닫힘/열림/비틀려 멈춤) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Bench` (48×24) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Cupboard` (32×48) — 찬장 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Door` (96×64, ×2 배치) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48, ×2 배치) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_OtherOutfits` (64×40) — 다른 옷들 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_TwinOutfits` (64×40) — 베넬의 같은 옷 두 벌 (베넬 옷 색과 같게) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Washer` (56×32, ×12 배치) — 세탁기 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_Glow` (64×64) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×14 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 6장 — M25 숙소 3호 (`M25_VenelleRoom.unity`)

목표: 사진·편지·포장지 · 스프라이트 렌더러 18개 · 코드 도형 1곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 타일·지형 | `Tile_HomeFloor` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `Tile_HomeWall` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Bed` (64×32) — 침대 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Clothes` (32×32) — 옷 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Cupboard` (32×48) — 찬장 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Cups` (16×16) — 컵 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Desk` (48×32) — 책상 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Door` (96×64) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Mirror` (32×48) — 거울 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_ChocoBox` (20×10) — 초코바 상자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_Letter` (14×10) — 편지 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Photo` (12×14) — 사진 (여자와 아이) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Wrappers` (40×8) — 초코바 포장지 열 장 (40×8) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×1 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 6장 — M26 기숙사 옥상 (`M26_DormRoof.unity`)

목표: 선택: 바람 · 스프라이트 렌더러 33개 · 코드 도형 1곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 배경 레이어 | `back` (112×272) | 패럴랙스 배경 | EXISTING | P1 |
| 타일·지형 | `Tile_Roof` (32×32, ×3 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Bench` (48×24) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Door` (96×64) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Rail` (32×14, ×24 배치) — 난간 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_WaterTower` (48×64) — 물탱크 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_HazeBand` (4×64) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×1 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 7장 — M27 수용동 (`M27_HoldingBlock.unity`)

목표: 수용칸 제어반으로 사람들을 풀어 준다 · 스프라이트 렌더러 65개 · 코드 도형 16곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 survivorA | CH_NPC_005 생존자 A (대기 `Sit`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_held1 | PLACEHOLDER | P0 |
| 인물 survivorB | CH_NPC_006 생존자 B (대기 `Sit`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_held2 | PLACEHOLDER | P0 |
| 인물 survivorB | CH_NPC_006 생존자 B (대기 `Sit`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_held5 | PLACEHOLDER | P0 |
| 인물 survivorB | CH_NPC_006 생존자 B (대기 `Sit`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_held3 | PLACEHOLDER | P0 |
| 인물 survivorA | CH_NPC_005 생존자 A (대기 `Sit`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_held4 | PLACEHOLDER | P0 |
| 적 | 순찰 경비(CH_ENEMY_001) 3, 포획 드론(CH_ENEMY_002) 1, CCTV(오브젝트) 1 | 감시 구역 | REPLACE | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Computer` (128×32) — 컴퓨터 (켜짐/꺼짐/부팅/메시지) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Desk` (48×32) — 책상 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Door` (96×64) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_ExposedWire` (128×32, ×5 배치) — 노출 전선 (4프레임) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Snack` (16×16) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvHead` (26×12) — CCTV 몸통 (코드 회전, 렌즈 오른쪽) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_CctvMount` (12×14) — CCTV 받침 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideRack` (56×44, ×2 배치) — 뒤에 숨는 전경 엄폐물 (선반) — 아래 1.2유닛이 몸을 가림 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideSacks` (64×36) — 뒤에 숨는 전경 엄폐물 (자루) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPower` (11×13, ×2 배치) — 전원 상태 표식: 전력 있음 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerBusy` (11×13) — 전원 상태 표식: 사용 중 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerEmpty` (11×13) — 전원 상태 표식: 없음 (사선) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34, ×2 배치) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lens` (4×4, ×2 배치) — CCTV 렌즈 (흰색, 코드가 색칠) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_LensOff` (4×4) — 꺼진 렌즈 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56, ×2 배치) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_PowerBox` (26×34) — 외부 전원 (출력 있음) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_WorkLamp` (30×12, ×3 배치) — 천장 작업등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_CellBars` (80×72, ×9 배치) — 수용칸 창살 (닫힘/열림) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_CellPanel` (40×36, ×4 배치) — 수용칸 제어반 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_CrateBed` (48×20, ×3 배치) — 상자 침대 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×7 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×16 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 7장 — M28 접수 홀 (`M28_ReceptionHall.unity`)

목표: 메이린 — 포획팔·유지보수 단말·셔터·제동 장치·젖은 바닥 · 스프라이트 렌더러 53개 · 코드 도형 13곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 meilin | CH_BOSS_002 메이린 (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_meilin | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Door` (144×80) — 실험실 문 (닫힘/열림/비틀려 멈춤) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Bench` (48×24) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Door` (96×64) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_PosterEmblem` (48×48) — 베스페르 표장 포스터 — 현재 ‘LOGO ?’ 빈 자리 (사용자 원안 필요) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPower` (11×13, ×2 배치) — 전원 상태 표식: 전력 있음 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerBusy` (11×13) — 전원 상태 표식: 사용 중 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerEmpty` (11×13) — 전원 상태 표식: 없음 (사선) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Junction` (22×28) — 과부하 분전함 (베넬 복도) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Lockers` (64×48, ×2 배치) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_PowerBox` (26×34) — 외부 전원 (출력 있음) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_ShutterPanel` (16×24) — 셔터 제어판 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_WorkLamp` (30×12, ×2 배치) — 천장 작업등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_BeltCrate` (20×16) — 벨트 위 상자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_Brake` (40×30, ×3 배치) — 제동 장치 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_CallBoard` (160×20) — 호출 번호판 (4) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Claw` (128×48) — 천장 포획팔 (4, 위 기준점) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_ClawRail` (32×6) — 포획팔 레일 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_ConductFloor` (32×6) — 전도성 바닥 (반복) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Doll` (24×44) — 빈 운반 인형 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_MaintTerm` (56×36, ×3 배치) — 유지보수 단말 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_TicketMachine` (24×44) — 번호표 발급기 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×5 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×13 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 7장 — M29 상담 부스 (`M29_CounselBooth.unity`)

목표: 영상실은 직원 카드로 (책상 / 분실물 상자) · 스프라이트 렌더러 35개 · 코드 도형 10곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePillar` (40×240, ×2 배치) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Bench` (48×24, ×3 배치) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Desk` (48×32, ×2 배치) — 책상 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Doc` (16×16) — 문서 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Door` (96×64, ×2 배치) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Snack` (16×16) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_PartsBin` (32×26) — 부품 수거함 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_BoothLock` (16×20) — 부스 잠금장치 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×10 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 7장 — M30 영상실 (`M30_VideoRoom.unity`)

목표: 감시 모니터 → 관제탑 쪽 격벽 · 스프라이트 렌더러 46개 · 코드 도형 18곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 survivorA | CH_NPC_005 생존자 A (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_feed_held4 | PLACEHOLDER | P0 |
| 인물 survivorA | CH_NPC_005 생존자 A (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_feed_held2 | PLACEHOLDER | P0 |
| 인물 warden | CH_NPC_009 회수 경비 (대기 `Guard`, 쓰러짐 `Down`) | 등장 조건 show_warden1 · 숨김 hide_warden1 | PLACEHOLDER | P1 |
| 인물 survivorB | CH_NPC_006 생존자 B (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_feed_held1 | PLACEHOLDER | P0 |
| 인물 warden | CH_NPC_009 회수 경비 (대기 `Walk`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_feed_warden | PLACEHOLDER | P1 |
| 인물 survivorB | CH_NPC_006 생존자 B (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_feed_held3 | PLACEHOLDER | P0 |
| 인물 warden | CH_NPC_009 회수 경비 (대기 `Guard`, 쓰러짐 `Down`) | 등장 조건 show_warden2 · 숨김 hide_warden2 | PLACEHOLDER | P1 |
| 인물 venelle | CH_BOSS_001 베넬 (본명 아이비) (대기 `Walk`, 쓰러짐 `Down`) | 등장 조건 show_feed_venelle · 숨김 hide_feed_venelle | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Desk` (48×32) — 책상 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Door` (96×64, ×2 배치) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_AlarmBeam` (96×24) — 경광등 빔 (가산, 반투명) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_AlarmLamp` (14×12) — 경광등 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Bulkhead` (72×96, ×4 배치) — 격벽 (36×96 ×2) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_LiftDeck` (96×14) — 승강기 바닥 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Monitor` (144×36, ×16 배치) — 감시 모니터 (3) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_SealShutter` (48×96) — 위층 봉쇄 셔터 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×18 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 7장 — M31 관제탑 (`M31_ControlTower.unity`)

목표: 중앙 통제 단말로 위층 통로를 연다 · 스프라이트 렌더러 29개 · 코드 도형 11곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Console` (144×40) — 콘솔 (켜짐/꺼짐/파손) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Bench` (48×24) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Bulkhead` (72×96, ×3 배치) — 격벽 (36×96 ×2) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Monitor` (144×36, ×8 배치) — 감시 모니터 (3) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×11 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 8장 — M32 상층 회랑 (`M32_UpperCorridor.unity`)

목표: 라비엘 · 스프라이트 렌더러 64개 · 코드 도형 18곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 raviel | CH_ALLY_001 라비엘 (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_raviel | PLACEHOLDER | P1 |
| 인물 ravielBeast | CH_ALLY_001-B 라비엘 (괴물 토끼) (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 show_ravielBeast · 숨김 hide_ravielBeast | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Door` (144×80, ×3 배치) — 실험실 문 (닫힘/열림/비틀려 멈춤) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Door` (96×64, ×3 배치) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Snack` (16×16) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_BodyBoard` (48×40, ×2 배치) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34, ×2 배치) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×3 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48, ×2 배치) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56, ×2 배치) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ShutterPanel` (16×24, ×3 배치) — 셔터 제어판 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_WorkLamp` (30×12, ×5 배치) — 천장 작업등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×10 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×18 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 8장 — M33 라비엘의 방 (`M33_RavielRoom.unity`)

목표: 액자 두 개 · 스프라이트 렌더러 13개 · 코드 도형 0곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 raviel | CH_ALLY_001 라비엘 (대기 `Sit`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_raviel | PLACEHOLDER | P1 |
| 타일·지형 | `Tile_HomeFloor` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `Tile_HomeWall` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Bed` (64×32) — 침대 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Bench` (48×24) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Cupboard` (32×48) — 찬장 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Desk` (48×32) — 책상 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_ChildPhoto` (28×22, ×2 배치) — 어린 시절 사진 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Cups` (48×12, ×3 배치) — 컵 두 개 (깨진 컵) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 이펙트 | `FX_GlowSoft` (64×64) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |

---

## 8장 — M34 통신실 (`M34_CommRoom.unity`)

목표: 통신실을 지나 교량으로 · 스프라이트 렌더러 41개 · 코드 도형 12곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 적 | 순찰 경비(CH_ENEMY_001) 2 | 감시 구역 | REPLACE | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Console` (144×40) — 콘솔 (켜짐/꺼짐/파손) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Battery` (32×32) — 배터리 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Computer` (128×32) — 컴퓨터 (켜짐/꺼짐/부팅/메시지) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Desk` (48×32) — 책상 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Doc` (16×16) — 문서 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Snack` (16×16) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideRack` (56×44) — 뒤에 숨는 전경 엄폐물 (선반) — 아래 1.2유닛이 몸을 가림 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_WorkLamp` (30×12, ×2 배치) — 천장 작업등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_ServerRack` (56×64) — 서버 랙 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×4 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×12 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 8장 — M35 연결 교량 (`M35_SkyBridge.unity`)

목표: 공연·서버동으로 · 스프라이트 렌더러 47개 · 코드 도형 16곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 적 | 순찰 경비(CH_ENEMY_001) 1, 포획 드론(CH_ENEMY_002) 1 | 감시 구역 | REPLACE | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `back` (112×272) | 패럴랙스 배경 | EXISTING | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Bench` (48×24) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_BodyBoard` (48×40, ×2 배치) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_HideSacks` (64×36) — 뒤에 숨는 전경 엄폐물 (자루) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34, ×2 배치) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56, ×2 배치) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_WorkLamp` (30×12, ×3 배치) — 천장 작업등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×6 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×16 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 9장 — M36 대기실 (`M36_WaitingRoom.unity`)

목표: 공연장으로 · 스프라이트 렌더러 44개 · 코드 도형 13곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 memberA | CH_NPC_007 베스페르 단원 A (대기 `Wait`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_c9_member1 | PLACEHOLDER | P1 |
| 인물 memberA | CH_NPC_007 베스페르 단원 A (대기 `Wait`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_c9_member3 | PLACEHOLDER | P1 |
| 인물 memberB | CH_NPC_008 베스페르 단원 B (대기 `Wait`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_c9_member2 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Bench` (48×24, ×2 배치) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Door` (96×64) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `Prop_Snack` (16×16) — 과자 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48, ×2 배치) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_WorkLamp` (30×12, ×3 배치) — 천장 작업등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_CommandChart` (56×40) — 배치도·지휘 체계 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_PlanNotes` (64×28) — 계획 종이 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×6 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×13 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 9장 — M38 녹음 보관실 (`M38_RecordArchive.unity`)

목표: 선택: 녹음 목록과 테이프 · 스프라이트 렌더러 30개 · 코드 도형 9곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePillar` (40×240, ×2 배치) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Bench` (48×24) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Cupboard` (32×48, ×3 배치) — 찬장 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Desk` (48×32) — 책상 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `Prop_Door` (96×64) — 문 (닫힘/열림/정전) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Tape` (20×14) — 녹음 테이프 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×9 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 9장 — M37 공명 공연장 (`M37_ResonanceHall.unity`)

목표: 라크리모사 — 믹서(마이크 방향·스피커·게인) · 스프라이트 렌더러 39개 · 코드 도형 15곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 edward | CH_BOSS_003-P 에드워드 (라크리모사의 과거) (대기 `Write`, 쓰러짐 `Down`) | 등장 조건 show_edward · 숨김 hide_edward | PLACEHOLDER | P1 |
| 인물 edwardKid | CH_BOSS_003-K 어린 에드워드 (대기 `Listen`, 쓰러짐 `Down`) | 등장 조건 show_edwardKid · 숨김 hide_edwardKid | PLACEHOLDER | P1 |
| 인물 lacrimosa | CH_BOSS_003 라크리모사 (본명 에드워드) (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_lacrimosa | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Desk` (48×32) — 책상 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Mic` (40×40) — 마이크 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Mixer` (96×24, ×4 배치) — 믹서 (3) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Speaker` (64×48, ×2 배치) — 스피커 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Truss` (96×16, ×3 배치) — 조명 트러스 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×15 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 9장 — M39 서버실 (`M39_ServerRoom.unity`)

목표: 포획 단말 / 승선 대기실 · 스프라이트 렌더러 48개 · 코드 도형 18곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 memberB | CH_NPC_008 베스페르 단원 B (대기 `Walk`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_feed_mb2 | PLACEHOLDER | P1 |
| 인물 survivorB | CH_NPC_006 생존자 B (대기 `Scared`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_feed_sv2 | PLACEHOLDER | P0 |
| 인물 memberA | CH_NPC_007 베스페르 단원 A (대기 `Walk`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_feed_mb1 | PLACEHOLDER | P1 |
| 인물 lacrimosa | CH_BOSS_003 라크리모사 (본명 에드워드) (대기 `Sing`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_feed_lac | PLACEHOLDER | P0 |
| 인물 survivorA | CH_NPC_005 생존자 A (대기 `Scared`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_feed_sv1 | PLACEHOLDER | P0 |
| 인물 kid008 | CH_NPC_004 008 (대기 `Hold`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_feed_kid | PLACEHOLDER | P1 |
| 인물 f | CH_NPC_003 F (대기 `Guard`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_feed_f | PLACEHOLDER | P1 |
| 인물 venelle | CH_BOSS_001 베넬 (본명 아이비) (대기 `SitQuiet`, 쓰러짐 `Down`) | 등장 조건 show_feed_venelle_b2 · 숨김 hide_feed_venelle_b2 | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `L_Console` (144×40) — 콘솔 (켜짐/꺼짐/파손) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_BrokenTag` (12×8) — 끊어진 번호표 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_CaptureTerm` (144×72) — 포획 단말 (3) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Rubble` (80×32) — 잔해 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_ServerRack` (56×64, ×18 배치) — 서버 랙 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_StillHand` (16×8) — 움직이지 않는 손 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Stove` (48×30, ×3 배치) — 화로 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Tarp` (96×20) — 방수포 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_Truss` (96×16) — 조명 트러스 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×18 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 9장 — M40 포획 단말 (`M40_CaptureTerminal.unity`)

목표: 정비도 파일 · 스프라이트 렌더러 2개 · 코드 도형 1곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 오브젝트 | `U_IconDoc` (16×16) — 아이콘: 문서 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_LightDot` (64×16) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×1 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 9장 — M41 휴지통 목록 (`M41_RecycleList.unity`)

목표: 항목 목록 → 정렬 → 선택 → 복원 → 구속 해제 → 내부 확인 · 스프라이트 렌더러 9개 · 코드 도형 1곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 오브젝트 | `U_IconBin` (16×16) — 아이콘: 휴지통 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_IconDoc` (16×16, ×7 배치) — 아이콘: 문서 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×1 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 10장 — M42 승선 대기실 (`M42_BoardingLounge.unity`)

목표: 캡슐 회랑으로 · 스프라이트 렌더러 41개 · 코드 도형 13곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 memberB | CH_NPC_008 베스페르 단원 B (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_c10_m4 | PLACEHOLDER | P1 |
| 인물 memberA | CH_NPC_007 베스페르 단원 A (대기 `Cheer`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_c10_member1 | PLACEHOLDER | P1 |
| 인물 memberB | CH_NPC_008 베스페르 단원 B (대기 `Wait`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_c10_member2 | PLACEHOLDER | P1 |
| 인물 memberA | CH_NPC_007 베스페르 단원 A (대기 `Wait`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_c10_m3 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Bench` (48×24, ×2 배치) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48, ×2 배치) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_WorkLamp` (30×12, ×3 배치) — 천장 작업등 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_BoardStatus` (64×28) — 탑승 현황판 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×6 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×13 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 10장 — M43 캡슐 회랑 (`M43_CapsuleCorridor.unity`)

목표: 전이 장치실로 · 스프라이트 렌더러 39개 · 코드 도형 14곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 meilin | CH_BOSS_002 메이린 (대기 `Pull`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_meilin | PLACEHOLDER | P0 |
| 인물 memberB | CH_NPC_008 베스페르 단원 B (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_cap_member | PLACEHOLDER | P1 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_Floor` (32×32) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `Prop_Bench` (48×24) — 벤치 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56, ×2 배치) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Capsule` (80×76, ×8 배치) — 탑승 캡슐 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Register` (14×10) — 접수 명부 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×14 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 10장 — M44 전이 장치실 (`M44_TransferChamber.unity`)

목표: 아우렐리우스 — 결합부에서 한계 방출 · 스프라이트 렌더러 46개 · 코드 도형 13곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 raviel | CH_ALLY_001 라비엘 (대기 `Injured`, 쓰러짐 `Down`) | 등장 조건 show_raviel_inj · 숨김 hide_raviel_inj | PLACEHOLDER | P1 |
| 인물 aurelius | CH_BOSS_004 아우렐리우스 (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_aurelius | PLACEHOLDER | P0 |
| 인물 meilin | CH_BOSS_002 메이린 (대기 `Pull`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_meilin | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `S_AlarmBeam` (96×24) — 경광등 빔 (가산, 반투명) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_AlarmLamp` (14×12) — 경광등 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPower` (11×13, ×2 배치) — 전원 상태 표식: 전력 있음 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerBusy` (11×13) — 전원 상태 표식: 사용 중 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_IconPowerEmpty` (11×13) — 전원 상태 표식: 없음 (사선) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_PowerBox` (26×34) — 외부 전원 (출력 있음) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Capacitor` (56×52, ×2 배치) — 축전 모듈 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Capsule` (80×76) — 탑승 캡슐 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_CoolPipe` (16×64, ×3 배치) — 냉각관·밸브 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Coupling` (216×72) — 전이 결합부 (72×72 ×3) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Magnet` (32×24) — 정비용 전자석 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `U_Register` (14×10) — 접수 명부 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Roller` (12×8) — 멈춘 롤러 | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Rubble` (80×32) — 잔해 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `U_Valve` (72×32) — 배수 밸브 (회전 3) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64, ×2 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×13 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 10장 — M45 붕괴 연결로 (`M45_CollapseWay.unity`)

목표: 무너지는 길 끝까지 · 스프라이트 렌더러 44개 · 코드 도형 17곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 ravielBeast | CH_ALLY_001-B 라비엘 (괴물 토끼) (대기 `Hold`, 쓰러짐 `Down`) | 등장 조건 show_fall_beast · 숨김 hide_fall_beast | PLACEHOLDER | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `S_AlarmBeam` (96×24) — 경광등 빔 (가산, 반투명) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_AlarmLamp` (14×12) — 경광등 | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_BodyBoard` (48×40, ×2 배치) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34, ×2 배치) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42, ×2 배치) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56, ×2 배치) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Rubble` (80×32, ×6 배치) — 잔해 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×17 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 11장 — M46 무너진 중정 (`M46_CollapsedCourt.unity`)

목표:  · 스프라이트 렌더러 32개 · 코드 도형 12곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 raviel | CH_ALLY_001 라비엘 (대기 `Down`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_raviel_end | PLACEHOLDER | P1 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidA` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×5 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Lockers` (64×48) — 보관함 (번호표) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Rubble` (80×32, ×4 배치) — 잔해 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2, ×2 배치) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 이펙트 | `FX_GlowSoft` (64×64) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×12 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 11장 — M47 잔해 더미 (`M47_RubblePile.unity`)

목표:  · 스프라이트 렌더러 28개 · 코드 도형 11곳

| 등장 요소 | 필요한 이미지 | 사용 상황 | 상태 | 우선 |
|---|---|---|---|---|
| 플레이어 | 루 Idle·Walk·Jump·Crouch 등 | 이동·잠입 | PLACEHOLDER | P0 |
| 인물 droid02 | CH_ENEMY_003 운반 로봇 (대기 `Idle`, 쓰러짐 `Down`) | 등장 조건 없음 · 숨김 hide_recovery | REPLACE | P0 |
| 배경 레이어 | `L_Far` (640×260) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_ForePipe` (240×36) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 배경 레이어 | `L_MidB` (320×200) | 패럴랙스 배경 | PLACEHOLDER | P1 |
| 타일·지형 | `L_FloorFill` (32×32, ×4 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 타일·지형 | `L_WallPanel` (64×64, ×2 배치) | 바닥·벽·발판 | PLACEHOLDER | P1 |
| 오브젝트 | `S_BodyBoard` (48×40) — 핵심 단서: 벽 도식 (선과 번호만, 해부 묘사 금지) | 배치 소품·장치 | PLACEHOLDER | P0 |
| 오브젝트 | `S_Jar0` (18×34) — 불투명 용기 (안 보이게) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_Jar1` (18×42) — 불투명 용기 (큰) | 배치 소품·장치 | PLACEHOLDER | P3 |
| 오브젝트 | `S_ObsWindow` (96×56) — 관찰창 (유리 뒤 실루엣 일부) | 배치 소품·장치 | PLACEHOLDER | P1 |
| 오브젝트 | `U_Rubble` (80×32, ×4 배치) — 잔해 | 배치 소품·장치 | PLACEHOLDER | P3 |
| 이펙트 | `FX_Dust` (2×2) | 환경 효과 (씬 배치) | PLACEHOLDER | P2 |
| 코드 도형 | (이미지 아님) `Tile_Pixel` ×11 | 벽·바닥 채움·빛 띠 등 단색 사각형 | CODE | — |

---

## 씬에서 쓰지 않는 기존 이미지 (OPTIONAL — 현재 어느 씬·설정·코드도 부르지 않음)

`Prop_AddressPanel`, `Prop_GlassWindow`, `Prop_LaundryLine`, `Prop_LaundryLineEmpty`, `Prop_Messenger`, `Prop_Rubble`, `Prop_Stone`, `Researcher2_Climb`, `Researcher2_Fall`, `Researcher2_Jump`, `Researcher2_Sit`, `Researcher_Climb`, `Researcher_Fall`, `Researcher_Jump`, `Researcher_Sit`, `D_Icon_battery`, `D_Icon_doc`, `D_Icon_exit`, `D_Icon_folder`, `D_Icon_log`, `D_Icon_msg`, `D_Icon_photo`, `D_Icon_shutter`, `D_Icon_socket`, `D_Icon_trash`, `D_Icon_webcam`, `D_Window`, `H_Beam`, `H_MaterialsB`, `H_WallHole`, `L_Window`, `S_DeadCable`, `Tile_Street`

(이전 오프닝·컴퓨터 안 구성에서 쓰던 그림이 남은 것으로 보임 — **추정**. 삭제하지 않았다.)
