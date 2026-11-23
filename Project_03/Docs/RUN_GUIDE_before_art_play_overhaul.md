# 실행 가이드 — ELECTRIC WORLD: LAST LIGHT 시연판

## 1. 환경

| 항목 | 값 |
|---|---|
| Unity | **6000.6.0f1** (ProjectVersion.txt 기준, 업그레이드하지 않음) |
| 입력 | Input System 전용 (기존 설정 `activeInputHandler: 1` 유지) |
| 렌더링 | URP 패키지는 설치돼 있으나 Quality 설정이 가리키는 URP 에셋 파일이 프로젝트에 없어 **실제로는 Built-in 렌더러**로 동작한다. 게임은 640×360 렌더 텍스처(Point 필터)를 정수 배율로 확대하는 자체 픽셀 퍼펙트 방식이라 파이프라인과 무관하게 동작한다. 파이프라인 설정은 건드리지 않았다. |
| 추가 패키지 | `com.unity.2d.sprite 1.0.0` (에디터 내장 패키지. 스프라이트 시트 분할 API용) |
| 한글 글꼴 | Galmuri11 (SIL OFL 1.1) — `Assets/ThirdParty/quiple_Galmuri/` |

## 2. 처음 열 때

1. Unity Hub 에서 `C:\Users\kuroc\Unity\term_project` 를 연다 (6000.6.0f1).
2. 씬·데이터는 이미 생성되어 있다. **추가로 할 일은 없다.**
3. 만약 `Assets/Game/Scenes` 가 비어 있거나 `Resources/EW/GameConfig.asset` 이 없다는 경고가 나오면
   메뉴 **EW > 1. 전체 구성 (에셋·데이터·씬·빌드 목록)** 을 한 번 누른다.
   - 여러 번 눌러도 안전하다. 씬 안의 `__Generated` 아래만 다시 만들고, 그 밖에 사용자가 추가한 오브젝트는 보존한다.
   - `GameDatabase.asset`, `Art_*.asset`, 애니메이션 클립은 **없을 때만** 만든다 (Inspector 에서 고친 값 보존).

## 3. 실행

- 시작 씬: **`Assets/Game/Scenes/Title.unity`** (메뉴 **EW > 3. 타이틀 씬 열기**) → Play.
- 타이틀: 새 게임 / 이어하기(저장이 있을 때) / 설정 / 종료.
- 새 게임은 오프닝 연출(불타는 마을 → 무너진 집 → 친구 컴퓨터 → 실험 통)로 시작하고, 실험 통 방전 뒤 '문틈'에서 처음 조작한다.
- 맵 씬(`C02_EscapeWing` 등)을 직접 열어 Play 하면 그 맵에서 **시험용 새 상태**로 바로 시작한다 (개발 확인용, 저장 진행과 무관). 오프닝 씬(`P01`~`C01`)을 직접 열면 그 장면 연출부터 시작한다.

## 4. 조작 (GDD 4.1)

| 동작 | 키보드 | 게임패드 |
|---|---|---|
| 좌우 이동 | ←/→, A/D | 왼쪽 스틱·D패드 |
| 달리기 | Shift (누른 채) | X(서쪽) |
| 점프 | Space | A(남쪽) |
| 사다리 오르내리기 | ↑/↓, W/S | 스틱 위/아래 |
| 조사·결정 | Z, E (메뉴에서는 Enter 도) | Y(북쪽)·A |
| 취소 | C, X, Backspace | B(동쪽) |
| 메뉴 (아이템·기록·대화 로그·상태·설정·타이틀로) | Esc | Start |
| 대사 넘기기 | Z·Space (출력 중이면 현재 문장 완성 → 다음 입력부터 진행) | A |
| 이미 읽은 대사 빨리 넘기기 | Ctrl 누른 채 | |
| **컷신 건너뛰기** | **Esc 를 1초 누르고 있기** (오른쪽 위 게이지) | Start/Select 길게 |

- 조사할 수 있는 물건에 가까이 가면 `Z 조사` 표시가 뜬다.
- 마우스: 타이틀·메뉴 항목 클릭 가능.
- 키 변경 기능은 아직 없다 (IMPLEMENTATION_STATUS 참고).
- **설정**(타이틀·Esc 메뉴): 전체/음악/환경음/효과음 볼륨, 화면 흔들림·화면 번쩍임 강도(0 = 끔), 글자 속도, 전투 연출 빠르기. ←/→ 로 조절, PlayerPrefs 에 저장(저장 데이터와 별개).

## 5. 저장

- 저장 시점: 실험 통 장면 직후(탈출 시작), 2·3장 진입, 침대·의자 휴식, 보스 앞(혼합실 진입), 시연판 종료 지점, **맵 이동 때 자동 저장**(시연판 편의 기능, `GameConfig.autosaveOnMapEnter` 로 끌 수 있음).
- 저장 형식 v2. 개편 전(v1) 저장은 불러올 때 새 맵으로 옮겨진다 (옛 프롤로그 저장은 새 오프닝 처음부터).
- 위치: `%USERPROFILE%\AppData\LocalLow\DefaultCompany\ELECTRIC WORLD - LAST LIGHT (Demo)\ew_lastlight_save.json` (+ `.bak` 백업)
  - 메뉴 **EW > 5. 저장 데이터 폴더 열기 / 6. 저장 데이터 삭제**
- 저장 파일이 손상되면 직전 백업으로 복구하고, 둘 다 손상이면 이어하기를 막고 안내한다.

## 6. Windows 빌드

- 메뉴 **EW > 4. Windows 빌드** → `Builds/Windows/ElectricWorld_LastLight.exe`
- 또는 File > Build Profiles 에서 Windows 로 빌드 (씬 목록은 구성 도구가 Title + 12개 맵으로 설정해 둠).
- 창 모드 1280×720, 크기 조절 가능.
- `Builds/Windows/*_BackUpThisFolder_ButDontShipItWithYourGame` 폴더는 배포하지 않는다.

## 7. 자동 테스트

Window > General > Test Runner
- **EditMode** (49개): 전투·협상 규칙, 동료 행동, 보상 1회, 재시도 복원, 장치 규칙, 저장/손상/버전/v1→v2 이전, 대사 ID·원문, 스탠딩 그림 존재, 장 카드 없음, 노출 전선 방어 해제, 기본 공격만으로 일반전 승리.
- **PlayMode** (13개): 오프닝 건너뛰기·끝까지 재생, 대화창(아래·첫 입력 무시·문장 완성·페이지), 첫 탈출 구역(계단·사다리·전투·지름길), 저장·이어하기, 베넬 협상~시연 종료, 주 경로 이동 통과 등. 약 5분.
- 자동 테스트는 사람이 직접 하는 플레이 검증을 대신하지 않는다.

## 8. 에디터 구성 도구 메뉴 (EW)

| 메뉴 | 내용 |
|---|---|
| 1. 전체 구성 | 텍스처 설정(Point·무압축·PPU)·시트 분할, 스탠딩(Bilinear)·소리 가져오기 설정 → 애니메이션 클립/컨트롤러 → 데이터·GameConfig(소리·스탠딩 목록) → 12개 맵 씬 + 타이틀 생성/갱신(옛 씬은 `Scenes/_Obsolete` 로 이동) → 빌드 씬 목록 |
| 2. 애니메이션 클립 다시 만들기 | 임시 그림을 같은 규격의 새 그림으로 덮어쓴 뒤 클립을 다시 만든다 (ART_REPLACEMENT_GUIDE 참고) |
| 3. 타이틀 씬 열기 | |
| 4. Windows 빌드 | |
| 5·6. 저장 폴더 열기 / 삭제 | |

## 9. 개발용 파일 (지워도 되는 것)

- `Assets/Game/Dev/Editor/EWDevBridge.cs` — 작업 중 열린 에디터를 원격으로 조작(갱신·구성·테스트·캡처)하는 데 쓴 파일 명령 연결 도구. 게임에는 포함되지 않는다(에디터 전용). 필요 없으면 `Assets/Game/Dev` 폴더째 삭제해도 된다.
- `Assets/Game/Scripts/Runtime/Core/DevHooks.cs` — 에디터 전용 도우미: 오프닝 자동 재생(사람이 읽는 속도로 대사 진행)·녹화(`StartOpeningDemoRecorded` → `Temp/EWRec`)·점검.
- `Tools/placeholder_gen.py`, `scene_art_gen.py`, `portrait_gen.py`, `audio_gen.py` — 임시 그림·스탠딩·소리 생성기 (Python + Pillow/numpy). 다시 실행하면 해당 폴더를 덮어쓰므로 **최종 그림·소리로 교체한 뒤에는 실행하지 않는다.**
- 실제 재생 기록: `Docs/Media/opening_to_first_battle.webp`, 대표 화면 `Docs/Media/*.png`.

## 10. 프로젝트 설정 변경 내역

| 파일 | 변경 |
|---|---|
| `Packages/manifest.json` | `com.unity.2d.sprite: 1.0.0` 추가 (나머지 그대로) |
| `ProjectSettings/EditorBuildSettings.asset` | 빌드 씬: Title + M00~M15(11 제외). 기존 `Assets/Scenes/SampleScene.unity` 항목은 파일이 존재하지 않아 제외 |
| `ProjectSettings/ProjectSettings.asset` | productName `ELECTRIC WORLD - LAST LIGHT (Demo)`, 기본 해상도 1280×720, 창 모드, 크기 조절 허용, Run In Background |
| 기존 `Assets/Scripts/*` (이전 과제 스크립트 7개) | 변경 없음 |
| `Assets/ASSET/*.zip` | 원본 보존, 변경 없음 |
