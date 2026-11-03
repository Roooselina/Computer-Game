# UniRun

교재의 기본 UniRun 러너에서 출발해, 타일맵 기반의 확장 러너 게임을 만든 컴퓨터 게임 텀프로젝트입니다.

- **Unity 버전**: 6000.6.4f1
- **렌더 파이프라인**: Built-in (2D)
- **주요 패키지**: Input System, 2D Tilemap, 2D Sprite, uGUI

## 씬 구성

`File > Build Profiles`(빌드 설정)에 아래 두 씬이 등록되어 있습니다. 두 씬은 서로 전환되지 않으므로 **각각 열어서 실행**합니다.

| 씬 | 내용 |
|---|---|
| `Assets/Scenes/Main.unity` | **Classic** — 기본 UniRun |
| `Assets/Scenes/Adventure.unity` | **Adventure** — 확장 버전 |

### Classic (`Main`)

- 발판을 미리 생성해 두고 주기적으로 재배치 (`PlatformSpawner`, `Platform`)
- 왼쪽 끝으로 간 배경을 오른쪽으로 옮기는 무한 스크롤 (`BackgroundLoop`, `ScrollingObject`)
- 2단 점프, 발판을 밟을 때 점수 증가, 게임오버 후 점프 입력으로 재시작 (`PlayerController`, `GameManager`)

### Adventure (`Adventure`)

- 오른쪽으로 자동으로 달리는 플레이어. 진행할수록 속도가 8.5 → 11.5로 빨라짐
- 점프 / 이단 점프 / 짧게 누르면 낮은 점프 / 물속 수영 / 경사면 밀착 / 물에 뜬 통나무 탑승
- 4개 구역: **MEADOW**(물) → **SWAMP**(흙탕물) → **DUSK**(독물, 위험) → **VOLCANO**(용암, 위험)
- 목숨 3개로 시작(최대 5개), 점수, 체크포인트 깃발과 결승 깃발
- 아이템: 보석, 하트(추가 목숨), 실드, 자석(10초)
- 순찰하는 적, 붉게 경고하는 가시 함정
- 오프닝 → 3·2·1 카운트다운, 구역별 하늘 그라데이션과 시차 배경, 진행도에 따라 빨라지는 음악
- 효과음: jump / die는 wav, 나머지는 실행 시 코드로 합성 (`Sfx`)
- HUD(하트, 점수, 진행도 바, 구역 배너, 결과 화면)는 모두 코드로 생성 (`RunHud`)

## 조작법

| 입력 | 동작 |
|---|---|
| Space / 마우스 왼쪽 클릭 / 게임패드 South 버튼 / 터치 | 점프 (공중에서 한 번 더 누르면 이단 점프) |

## 실행 방법

1. Unity Hub에서 이 `UniRun/` 폴더를 프로젝트로 추가하고 Unity **6000.6.4f1**로 엽니다.
2. `Assets/Scenes/Main.unity` 또는 `Assets/Scenes/Adventure.unity`를 열고 ▶ Play를 누릅니다.

## 폴더 구조

```
UniRun/
├── Assets/
│   ├── Scenes/        Main, Adventure
│   ├── Scripts/
│   │   ├── Classic/   기본 UniRun 스크립트 (6개)
│   │   └── Adventure/ 확장 게임 스크립트 (17개)
│   ├── Editor/        씬 빌더, 에셋 정리, 검증/빌드 도구 (에디터 전용)
│   ├── Art/           Characters, Classic, Kenney, Generated, Fonts
│   ├── Audio/         jump, die, music
│   ├── Animations/    Player 애니메이터 (Run / Jump / Die)
│   ├── Prefabs/       Platform
│   ├── Data/          adventure_map.json (Adventure 맵 데이터)
│   └── Settings/      Input Actions, 물리 재질
├── Packages/          manifest.json, packages-lock.json
├── ProjectSettings/
├── Tools/MapGen/      맵 생성기 (gamemap.py)
└── Documentation/     프로젝트 보고서 (PPT)
```

## 에디터 메뉴

| 메뉴 | 스크립트 | 기능 |
|---|---|---|
| `UniRun > Build Adventure Scene` | `AdventureSceneBuilder.cs` | Adventure 씬을 처음부터 다시 생성 |
| `UniRun > Organize Assets` | `AssetOrganizer.cs` | 에셋 폴더 정리 (GUID 유지) |
| `W06 / W10 > ...` | `W06*.cs`, `W10Validation.cs` | Main 씬 검증, Windows 빌드 |

저장된 씬은 이 도구들을 실행하지 않아도 그대로 동작합니다.

## 맵 생성기 (선택)

Adventure 맵은 `Tools/MapGen/gamemap.py`가 생성합니다.

```bash
pip install pillow numpy
python Tools/MapGen/gamemap.py <UniRun 폴더 경로> [seed]
```

- 출력: `Assets/Data/adventure_map.json`, `Assets/Art/Generated/*.png`, `Tools/MapGen/preview.png`
- 원본 타일은 `Assets/Art/Kenney/Tilesheet/`, `Assets/Art/Kenney/PNG/Other/`를 사용합니다.
- 맵을 다시 생성해도 씬은 수정할 필요가 없습니다(실행 시 `MapLoader`가 JSON을 읽음).

## 사용 에셋 및 라이선스

- **Kenney** Pixel Platformer 에셋 — `Assets/Art/Kenney/`
- **Kenney Mini Square** 폰트 — `Assets/Art/Fonts/` (라이선스: `License.txt`)
- **Toko 캐릭터 및 Classic 스프라이트** — `Assets/Art/Characters/`, `Assets/Art/Classic/`
  - 함께 제공된 Unity-Chan License(UCL 2.0) 문서: `Assets/Art/Classic/License/`

## 문서

- [`Documentation/UniRun 보고서.pptx`](Documentation/) — 기획, 맵 생성 알고리즘, 구현, 결과 보고서
