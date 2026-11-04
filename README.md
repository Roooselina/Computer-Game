# 컴퓨터 게임

> 컴퓨터 게임 텀프로젝트 통합 저장소

| 항목 | 내용 |
|---|---|
| 과목 | 컴퓨터 게임 |
| 학기 | 4학년 2학기 |
| 엔진 | Unity (프로젝트별 버전은 아래 표 참고) |

이 저장소는 컴퓨터 게임 과목에서 진행하는 여러 텀프로젝트를 한곳에서 관리합니다.
각 프로젝트는 **자기 폴더 안에 `Assets/`, `Packages/`, `ProjectSettings/`를 따로 가지는 독립된 Unity 프로젝트**이므로 서로 영향을 주지 않습니다.

## 프로젝트 목록

| # | 프로젝트 | 장르 | Unity 버전 | 폴더 |
|---|---|---|---|---|
| 1 | **UniRun** | 2D 러너 (Classic + Adventure) | 6000.6.4f1 | [`UniRun/`](UniRun/) |
| 2 | **Dodge** | 3D 탄알 피하기 (쉴드 · 랭킹) | 6000.6.0f1 | [`Dodge/`](Dodge/) |
| 3 | *추후 추가* | | | `Project_03/` |

### 1. UniRun

교재의 기본 UniRun 러너(**Classic**, `Main` 씬)에서 출발해, Python으로 생성한 타일맵 위를 달리는 확장 게임(**Adventure**, `Adventure` 씬)을 만든 프로젝트입니다.

- **Classic**: 발판 생성 및 재배치, 무한 배경 스크롤, 2단 점프, 점수, 게임오버 후 재시작
- **Adventure**: 자동 달리기, 점프 / 이단 점프 / 수영, 4개 구역(MEADOW · SWAMP · DUSK · VOLCANO), 목숨(3개 시작, 최대 5개), 보석 · 하트 · 실드 · 자석 아이템, 순찰하는 적과 가시, 체크포인트, 진행도에 따라 빨라지는 음악
- 맵 데이터는 `UniRun/Tools/MapGen/gamemap.py`가 생성한 JSON을 실행 시 읽어서 배치합니다.

자세한 내용은 [UniRun/README.md](UniRun/README.md)를 참고하세요.

### 2. Dodge

교재의 기본 Dodge(사방에서 날아오는 탄알 피하기)에서 출발해 여러 기능을 추가한 프로젝트입니다.

- **기본**: 상하좌우 이동, 플레이어를 조준하는 탄알 생성기, 생존 시간 점수, 게임오버 후 R로 재시작
- **추가**: 쉴드(Space, 3초 무적 + 5초 쿨다운), 메인 메뉴, 최고 점수 및 Top 3 랭킹 저장, ESC 일시정지 메뉴, 2단계 피격 연출(파티클 · 화면 Flash · 카메라 흔들림 · Hit Stop · 사운드)

자세한 내용은 [Dodge/README.md](Dodge/README.md)를 참고하세요.

## 실행 방법

1. 저장소를 Clone합니다.
   ```bash
   git clone https://github.com/<계정>/Computer-Game.git
   ```
2. **Unity Hub** → **Add** → **Add project from disk** → 실행할 프로젝트 폴더(예: `Computer-Game/UniRun`)를 선택합니다.
   - 저장소 최상위 폴더가 아니라 **각 프로젝트 폴더**를 선택해야 합니다.
3. 프로젝트에 맞는 Unity 버전(위 표)을 설치해 엽니다.
   - 처음 열 때 `Library/`를 새로 만들고 패키지를 받으므로 몇 분 걸릴 수 있습니다.
4. 프로젝트별 README의 안내에 따라 씬을 열고 ▶ Play를 누릅니다.

## 저장소 구조

```
컴퓨터 게임/
├── README.md        ← 이 문서
├── .gitignore       ← 모든 프로젝트 공통 (Library, Temp, Build 등 제외)
├── .gitattributes
├── UniRun/          ← 1번 프로젝트 (독립 Unity 프로젝트)
│   ├── Assets/
│   ├── Packages/
│   ├── ProjectSettings/
│   ├── Tools/
│   ├── Documentation/
│   └── README.md
├── Dodge/           ← 2번 프로젝트 (독립 Unity 프로젝트)
│   ├── Assets/
│   ├── Packages/
│   ├── ProjectSettings/
│   ├── Documentation/
│   └── README.md
└── Project_03/ ...  ← 추후 추가
```

## 향후 프로젝트 추가 방법

1. 저장소 최상위에 새 폴더(예: `Project_02/`, 또는 게임 이름)를 만들고 Unity 프로젝트의 `Assets/`, `Packages/`, `ProjectSettings/`를 복사합니다.
   - `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Build/`는 최상위 `.gitignore`가 자동으로 제외합니다.
2. 프로젝트 폴더에 `README.md`를 작성합니다.
3. 이 문서의 **프로젝트 목록** 표에 한 줄을 추가합니다.
4. `git add <폴더>` → `git commit` → `git push`
