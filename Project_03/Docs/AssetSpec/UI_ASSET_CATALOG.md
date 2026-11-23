# UI ASSET CATALOG — UI 에셋 목록

> 분석일 2026-11-17 · 기준: 현재 프로젝트 파일 (코드·씬·데이터·이미지 메타). **현재 구현 기준**이다. 재기획(Docs/Redesign)으로 바뀔 수 있는 요구는 따로 표시했다.
> 상태: EXISTING 정상 사용 · PLACEHOLDER 임시 · MISSING 필요하지만 없음 · REPLACE 있으나 교체 필요 · OPTIONAL 선택 / 우선순위 P0~P3. **추정**은 따로 표시. 이미지를 만들거나 프로젝트 파일을 바꾸지 않았다.

## 기준

| 항목 | 값 | 근거 |
|---|---|---|
| 렌더 해상도 | 640×360 렌더 타깃 → 화면에 정수 배율 확대 (1080p = 3배) | `Runtime/Visual/PixelScreen.cs` W=640,H=360 |
| Pixels Per Unit | 32 (도트 그림 전부) / Kenney UI 100 / sci_fi_mockup 128 | `PixelScreen.PPU`, `Editor/EWArtSpecs.cs`, 이미지 메타 |
| UI Canvas | 논리 640×360 프레임 (`UIRoot` ConstantPixelSize + 640×360 Root, 일부 창은 CanvasScaler 640×360 ScaleWithScreenSize) | `UI/UIRoot.cs`, `UI/UIKit.cs` |
| 스탠딩 표시 크기 | 화면 높이의 82% (heightRatio 0.82) — 루는 왼쪽, 나머지 오른쪽 | `UI/UIRoot.cs` PortraitLayer |
| 렌더 파이프라인 | 실제 실행은 Built-in (URP 패키지는 설치만) | ART_PLAY_OVERHAUL_REPORT |

UI 대부분은 코드(`Assets/Game/Scripts/Runtime/UI/*.cs`)가 단색 이미지(`GameConfig.pixel`)·9-slice 패널·글꼴로 그린다. 그림 파일이 필요한 것은 아래 ‘이미지’ 표시 항목뿐이다.

## 화면별 UI

| UI | 위치 | 구성 | 1 이미지 제작 | 2 텍스트 | 3 Unity 기본 컴포넌트·코드 | 현재 그림 | 상태 | 우선 |
|---|---|---|---|---|---|---|---|---|
| 메인 메뉴 (타이틀) | Title.unity + `UI/TitleController.cs` | 배경 2레이어 + 메뉴 4항목 (새 게임/이어하기/설정/종료) + 키 안내 | 타이틀 배경·로고 | 메뉴 글자 | 선택 강조 | ansimuz back/middle (CC0, 도시 야경) · 로고 없음(글자) | EXISTING 배경 / MISSING 로고 | P1 |
| 시작·설정·종료 버튼 | 타이틀·Esc 메뉴 | 글자 + 선택 틀 | — | O | Kenney outline_yellow 9-slice | Kenney | EXISTING | P3 |
| 체력바 (안정도) | HUD 왼쪽 위 | 막대 | — (선택: 테두리 그림) | 숫자 | 단색 이미지 | 코드 | CODE | P2 |
| 스태미나·에너지 (전하) | HUD | 칸 (Lv 에 따라 4~6칸) | — (선택: 칸 아이콘) | — | 단색 이미지 | 코드 | CODE | P2 |
| 기술 슬롯 아이콘 | HUD 아래, 안내서 | Q/E/R/F 4칸 + 실패 이유 | **UI_Skill_* 16×16** | 키 이름 | 칸 틀 | 임시 16×16 | PLACEHOLDER | P0 |
| 대상 강조 | 월드 위 | 꺾쇠 틀 | — | — | LineRenderer (코드) | 코드 | CODE | — |
| 상호작용 안내 | 대상 위 | 작은 패널 + ‘Space · 조사’ | — | O | 단색 패널 | 코드 | CODE | — |
| 대화창 | 화면 아래 24% | 9-slice 패널 + 이름표 + 본문 + 넘김 표시 | — (선택: 전용 9-slice 틀) | O | Kenney space 9-slice | Kenney | EXISTING | P2 |
| 캐릭터 초상화 (스탠딩) | 대화창 위 좌/우 | 600×900 일러스트 | **ST_* (CHARACTER_ASSET_CATALOG B)** | — | Image | 임시 49장 | PLACEHOLDER | P0~P1 |
| 선택지 | 대화창 | 항목 + 선택 틀 | — | O | outline_yellow | Kenney | EXISTING | P3 |
| 말풍선 (동료·루) | 인물 머리 위 | 작은 패널 + 글자 | — | O | 단색 | 코드 | CODE | — |
| 퀘스트 안내 | — | **없음** (힌트 줄 `SetHint` 한 줄만) | — | O | — | — | 해당 없음 | — |
| 인벤토리·메뉴 | Esc | 상태·기술 장착·아이템·문서 목록 | (선택: 아이템 아이콘 — `iconSprite` 필드는 정의만 있고 사용처 확인 안 됨, **추정 미사용**) | O | 패널 | Kenney | EXISTING | P3 |
| 안내서 | P03·Esc | 기술 설명 + 미리보기 그림 | **UI_Prev_* (10~20px)** | O | 패널 | 임시 | PLACEHOLDER | P2 |
| 보스 HP 바 | 화면 위 가운데 | 막대 + 이름·단계 글자 | — | O | 단색 | 코드 | CODE | — |
| 시설 카메라 영상 | M30·M39 | 별도 카메라 렌더 (320×180 RT) + 테두리·글자 | — | O (CAM 번호) | RenderTexture | 코드 | CODE | — |
| 휴지통 창 | M41 | 목록 + 버튼 4 | (선택: 파일 아이콘) | O | 패널 | 코드 | CODE | P3 |
| 강제 전송 창 (P03) | P03 | 시스템 창 2종 | — | O | `SystemWindow` (UIRoot.cs) | 코드 | CODE | — |
| 게임 오버 | — | **없음** — 쓰러지면 즉시 구간 재시도 (전용 화면 없음) | — | — | 페이드 | — | 해당 없음 | — |
| 재시작 | 쓰러짐 직후 | 페이드 → 체크포인트 | — | — | 코드 | — | 해당 없음 | — |
| 엔딩 UI | 엔딩 | 크레딧 글자 → 가상 바탕화면(단색 배경 + 아이콘 4 + 응답 없는 창 + 안내 상자) → 게임 메뉴 | **가상 바탕화면 배경 1, U_Icon* 4** (특정 운영체제와 닮지 않게) | O | 패널 | 임시 아이콘 | PLACEHOLDER (아이콘) / MISSING (배경) | P2 |
| 검수 도구 | Esc → 검수 도구 | 목록 메뉴 | — | O | 패널 | 코드 | CODE (개발용) | — |
| 장 제목 카드 | — | 없음 (확정: 쓰지 않음) | — | — | — | — | 해당 없음 | — |

## 이미지 파일 목록

| 에셋 ID | 파일 | 크기 | 피벗 | 사용 | 상태 | 우선 |
|---|---|---|---|---|---|---|
| UI_PREV_CCTV | `Assets/Game/Art/Placeholder/UI_Prev_Cctv.png` | 20×16 (1프레임) | 가운데 | 안내서 미리보기 | PLACEHOLDER | P2 |
| UI_PREV_CCTVOFF | `Assets/Game/Art/Placeholder/UI_Prev_CctvOff.png` | 20×16 (1프레임) | 가운데 | 안내서 미리보기 | PLACEHOLDER | P2 |
| UI_PREV_DUMMY | `Assets/Game/Art/Placeholder/UI_Prev_Dummy.png` | 16×24 (1프레임) | 가운데 | 안내서 미리보기 | PLACEHOLDER | P2 |
| UI_PREV_PANEL | `Assets/Game/Art/Placeholder/UI_Prev_Panel.png` | 16×26 (1프레임) | 가운데 | 안내서 미리보기 | PLACEHOLDER | P2 |
| UI_PREV_SOCKET | `Assets/Game/Art/Placeholder/UI_Prev_Socket.png` | 10×12 (1프레임) | 가운데 | 안내서 미리보기 | PLACEHOLDER | P2 |
| UI_PREV_SOURCE | `Assets/Game/Art/Placeholder/UI_Prev_Source.png` | 16×20 (1프레임) | 가운데 | 안내서 미리보기 | PLACEHOLDER | P2 |
| UI_SKILL_JAM | `Assets/Game/Art/Placeholder/UI_Skill_jam.png` | 16×16 (1프레임) | 가운데 | 기술 슬롯·안내서 | PLACEHOLDER | P0 |
| UI_SKILL_PULL | `Assets/Game/Art/Placeholder/UI_Skill_pull.png` | 16×16 (1프레임) | 가운데 | 기술 슬롯·안내서 | PLACEHOLDER | P0 |
| UI_SKILL_SPARK_SHOT | `Assets/Game/Art/Placeholder/UI_Skill_spark_shot.png` | 16×16 (1프레임) | 가운데 | 기술 슬롯·안내서 | PLACEHOLDER | P0 |
| UI_SKILL_TOK | `Assets/Game/Art/Placeholder/UI_Skill_tok.png` | 16×16 (1프레임) | 가운데 | 기술 슬롯·안내서 | PLACEHOLDER | P0 |
| UI_SKILL_WIRE | `Assets/Game/Art/Placeholder/UI_Skill_wire.png` | 16×16 (1프레임) | 가운데 | 기술 슬롯·안내서 | PLACEHOLDER | P0 |
| UI_ICONBIN | `Assets/Game/Art/Placeholder/Scene/U_IconBin.png` | 16×16 (1프레임) | 가운데 | 엔딩 가상 바탕화면 | PLACEHOLDER | P2 |
| UI_ICONDOC | `Assets/Game/Art/Placeholder/Scene/U_IconDoc.png` | 16×16 (1프레임) | 가운데 | 엔딩 가상 바탕화면 | PLACEHOLDER | P2 |
| UI_ICONFOLDER | `Assets/Game/Art/Placeholder/Scene/U_IconFolder.png` | 16×16 (1프레임) | 가운데 | 엔딩 가상 바탕화면 | PLACEHOLDER | P2 |
| UI_ICONGAME | `Assets/Game/Art/Placeholder/Scene/U_IconGame.png` | 16×16 (1프레임) | 가운데 | 엔딩 가상 바탕화면 | PLACEHOLDER | P2 |
| UI_PANEL_OUTLINE_YELLOW | `Assets/ThirdParty/Kenney_PixelUI/outline_yellow.png` | 48×48 (9-slice) | 가운데 | 대화창·메뉴 패널 | EXISTING | P3 |
| UI_PANEL_SPACE | `Assets/ThirdParty/Kenney_PixelUI/space.png` | 48×48 (9-slice) | 가운데 | 대화창·메뉴 패널 | EXISTING | P3 |
| UI_PANEL_SPACE_INLAY | `Assets/ThirdParty/Kenney_PixelUI/space_inlay.png` | 44×44 (9-slice) | 가운데 | 대화창·메뉴 패널 | EXISTING | P3 |
| UI_LOGO_001 | (없음) | 미정 | — | 타이틀 로고 | MISSING | P1 |
| UI_LOGO_VESPER | `Placeholder/Prop_PosterEmblem.png` (‘LOGO ?’) | 48×48 | 아래 가운데 | 베스페르 표장 (포스터·소품) — **사용자 원안 필요, 임의로 그리지 않는다** (GDD 1.3) | MISSING | P1 |
| UI_DESKTOP_BG | (없음 — 코드 단색) | 640×360 | — | 엔딩 가상 바탕화면 배경 | MISSING | P2 |

글꼴: Galmuri11 (OFL), Pretendard (OFL) — 이미지 아님.
