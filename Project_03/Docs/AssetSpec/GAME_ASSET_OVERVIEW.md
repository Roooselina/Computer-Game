# GAME ASSET OVERVIEW — 게임 전체 에셋 요약

> 분석일 2026-11-17 · 기준: 현재 프로젝트 파일 (코드·씬·데이터·이미지 메타). **현재 구현 기준**이다. 재기획(Docs/Redesign)으로 바뀔 수 있는 요구는 따로 표시했다.
> 상태: EXISTING 정상 사용 · PLACEHOLDER 임시 · MISSING 필요하지만 없음 · REPLACE 있으나 교체 필요 · OPTIONAL 선택 / 우선순위 P0~P3. **추정**은 따로 표시. 이미지를 만들거나 프로젝트 파일을 바꾸지 않았다.

## 결론 (먼저 읽기)

- **최종 그림은 사실상 없다.** 인물·소품·배경·이펙트·스탠딩은 모두 Python 도구로 만든 임시 도트(몸에 ‘LU’, ‘VN’ 글자, 스탠딩에 ‘임시 스탠딩’ 표시)다. 일반 적 3종은 무료 팩(Foozle 드로이드)을 다른 용도로 대신 쓴다.
- **코드가 부르는데 파일이 없는 그림은 없다.** 애니메이션 상태 253개 모두 시트와 클립이 있고, 대사가 쓰는 표정 42종 모두 스탠딩 파일이 있으며, 코드가 이름으로 꺼내는 스프라이트 세트도 모두 있다 (정적 검사). → 교체 작업은 **같은 파일 이름·같은 프레임 크기로 덮어쓰기**만 하면 코드 수정 없이 연결된다.
- 진짜 ‘없는’ 것은 기능은 돌아가지만 전용 그림 대신 다른 그림을 쓰는 자리다: 무인 회수 장치(운반 로봇 그림), 순찰 경비 공격 예고 프레임, 루의 최종전 전용 표정, 베스페르 로고(‘LOGO ?’), 7~11장 전용 배경 레이어(연구소 레이어를 어둡게 재사용), 엔딩 바탕화면 배경, 벽·바닥 일부가 단색 사각형(`Tile_Pixel`).
- 맵은 Tilemap·Prefab 이 아니라 코드가 개별 스프라이트를 배치한다. 배경은 레이어별 개별 그림, 지형은 반복 타일, 소품은 개별 PNG 로 만들어야 한다 (통짜 배경 금지 — 이전에 적용 후 되돌림).
- 이미지 생성 AI 로 정확한 도트 격자를 바로 얻기는 어렵다 (2026-11-15 실제 시도에서 치수·격자 불일치 — PIXEL_RESTORE_REPORT). 가이드는 ‘큰 해상도로 생성 → 최근접 축소 → 팔레트 정리 → 사람 수정’ 순서를 권장한다.

## 수량

| 항목 | 수 | 비고 |
|---|---|---|
| 캐릭터 | 24 | 플레이어 1, 보스·동료 5 (+형태 4), 적 3, NPC 11 — 같은 인물의 다른 형태 포함 시트 기준 24 |
| 씬 | 46 | 타이틀 1 + 플레이 맵 45 (테스트 씬 제외) |
| 애니메이션 상태 (시트) | 253 | 모두 임시, 클립 253 존재 |
| 스탠딩 | 50 | 사용 42 · 미사용 보유 7 · 신규 필요 1 (루 최종전) |
| 배경 레이어 에셋 | 16 | 씬에 연결된 파일 기준 (재사용 포함) |
| 타일·지형 에셋 | 11 | + 코드 단색 사각형 다수 |
| 오브젝트 에셋 | 155 | 씬 배치 소품·장치 |
| 이펙트 (스프라이트 세트) | 24 | 코드 생성 효과(선·입자·흔들림)는 별도 — 이미지 아님 |
| UI 이미지 | 21 | 대부분 UI 는 코드 도형·글자 |
| 기존 에셋 (EXISTING, 계속 사용 가능) | 4 | 무료 팩 타일·장식·UI 패널·도시 배경 |
| 임시 (PLACEHOLDER) | 489 | 교체 대상 |
| 교체 필요 (REPLACE) | 15 | Foozle 드로이드 대용 적 |
| 누락 (MISSING) | 6 | 위 결론의 전용 그림 |
| 선택 (OPTIONAL) | 13 | 정의만 있고 쓰이지 않는 상태·표정 |

## 우선순위별 작업량

| 우선 | 수 | 내용 |
|---|---|---|
| P0 | 169 | 플레이 판독에 직접 필요: 루 기본 동작, 보스 예고·공격·빈틈, 일반 적, 엄폐물·전원·CCTV·문·사다리, 보스 기물, 기술 아이콘, 자주 쓰는 스탠딩(10줄 이상) |
| P1 | 175 | 인물·맵 기본 완성도: NPC 기본 동작, 배경 레이어·지형, 이야기 핵심 소품, 나머지 스탠딩 |
| P2 | 111 | 연출: 한 장면용 동작, 회상 인물, 환경 이펙트, 엔딩 UI |
| P3 | 72 | 장식 소품, 쓰이지 않는 상태 |

## 기술 기준

| 항목 | 값 | 근거 |
|---|---|---|
| 렌더 해상도 | 640×360 렌더 타깃 → 화면에 정수 배율 확대 (1080p = 3배) | `Runtime/Visual/PixelScreen.cs` W=640,H=360 |
| Pixels Per Unit | 32 (도트 그림 전부) / Kenney UI 100 / sci_fi_mockup 128 | `PixelScreen.PPU`, `Editor/EWArtSpecs.cs`, 이미지 메타 |
| 카메라 | Orthographic Size 5.625 (= 360/2/32) → 화면 20×11.25 유닛. 확대 1~3배 (`CameraRig.SetZoom`) | `PixelScreen.OrthoSize`, `Field/CameraRig.cs` |
| UI Canvas | 논리 640×360 프레임 (`UIRoot` ConstantPixelSize + 640×360 Root, 일부 창은 CanvasScaler 640×360 ScaleWithScreenSize) | `UI/UIRoot.cs`, `UI/UIKit.cs` |
| 화면 비율 | 16:9 | 위 |
| 도트 가져오기 설정 | Sprite, Point 필터, 무압축, 밉맵 없음, 시트는 가로 한 줄 자동 분할 (프레임 크기는 파일 이름 접두어 규칙) | `Editor/EWAssetSetup.cs`, `EWArtSpecs.ForPlaceholder` |
| 스탠딩 가져오기 설정 | Sprite Single, Bilinear, CompressedHQ, 밉맵 없음, 최대 1024 이상, 아래 가운데 피벗 | `EWAssetSetup.ConfigurePortraitsAndAudio` |
| 스탠딩 표시 크기 | 화면 높이의 82% (heightRatio 0.82) — 루는 왼쪽, 나머지 오른쪽 | `UI/UIRoot.cs` PortraitLayer |
| 인물 캔버스 | 48×64 / 프레임 (괴물 토끼 80×96, 아우렐리우스 64×96), 아래 가운데 피벗, **오른쪽을 봄** (왼쪽은 좌우 반전) | `EWArtSpecs.ForPlaceholder`, `Art_*.asset` flipForLeft |
| 인물 화면 크기 | 64px = 2유닛 = 화면 높이 17.8% (확대 1배) · 35.6% (2배). 1080p 에서 48×64 프레임은 144×192 로 보임 | 계산 |
| 애니메이션 | 상태마다 AnimationClip (SpriteRenderer.m_Sprite 키프레임) + 캐릭터별 AnimatorController (Any State 전이 없음, 코드가 상태 이름으로 Play) | `EWAssetSetup.BuildCharacters`/`FillClip` |
| 맵 구성 방식 | Tilemap·Prefab 없음. 방 정의 코드(`Editor/EWRooms*.cs`)가 개별 SpriteRenderer·BoxCollider2D 를 씬에 생성 | `Editor/EWSceneBuilder.cs`, PIXEL_RESTORE_REPORT |
| 렌더 파이프라인 | 실제 실행은 Built-in (URP 패키지는 설치만) | ART_PLAY_OVERHAUL_REPORT |

## 문서

| 파일 | 내용 |
|---|---|
| CHARACTER_ASSET_CATALOG.md | 캐릭터 24 — 디자인·스탠딩·동작 요약·적/보스 행동 |
| SCENE_ASSET_CATALOG.md | 씬 46 — 실제 연결된 스프라이트 전부, 인물, 적 |
| ANIMATION_CATALOG.md | 상태별 프레임·FPS·루프·시트 크기·피벗·사용 위치 |
| UI_ASSET_CATALOG.md | UI 구성과 이미지/글자/코드 구분 |
| IMAGE_GENERATION_GUIDE.md | 생성 AI 프롬프트·유지할 외형·후처리·Unity 적용 |
| ASSET_IMPLEMENTATION_MAPPING.md | 파일 → 경로·씬·SpriteRenderer·Animator·클립 연결 |

## 현재 구현 vs 재기획

- 이 문서의 요구는 **현재 구현 기준**이다.
- Docs/Redesign/DECISION_LOG.md 의 결정에 따라 바뀔 수 있는 것: 동료 고유 행동 동작(D-001), 일반 적 종류 확대(GDD 8종 중 3종만 구현), 보스 공략 기물(D-070·D-091), 오프닝 톤(D-010), 사망 연출 방식(D-090), 빠진 맵 3개·문서(D-025). **확정 전이므로 제작 목록에 넣지 않았다.**
