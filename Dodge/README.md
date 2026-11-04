# Dodge

교재의 기본 Dodge(탄알 피하기) 게임에서 출발해, 쉴드 · 일시정지 · 메인 메뉴 · 기록 랭킹 · 피격 연출을 추가한 컴퓨터 게임 텀프로젝트입니다.

- **Unity 버전**: 6000.6.0f1
- **렌더 파이프라인**: Built-in (3D)
- **주요 패키지**: uGUI

## 씬 구성

| 씬 | 내용 |
|---|---|
| `Assets/Scenes/SampleScene.unity` | **게임 씬** — 메인 메뉴, 게임플레이, 일시정지 메뉴를 모두 포함 (Build Settings 등록) |
| `Assets/Scenes/MainMenu.unity` | 별도 메인 메뉴 씬 (선택, Build Settings 미등록) |

## 게임 내용

- 플레이어(큐브)를 상하좌우로 움직여 사방의 Bullet Spawner가 쏘는 탄알을 피함 (`PlayerController`, `BulletSpawner`, `Bullet`)
- 탄알 한 발에 사망, 버틴 시간이 점수 (`GameManager`)
- **쉴드**: 3초 동안 피해 완전 차단, 종료 후 5초 쿨다운. 상태를 HUD에 표시 (`ShieldController`, `ShieldUI`)
- **메인 메뉴**: 실제 게임 화면을 배경으로 Game Start / Score Ranking / Exit Game (`MainMenuManager`)
- **기록**: 최고 점수와 Top 3 랭킹을 PlayerPrefs에 저장해 재실행해도 유지 (`RecordStore`)
- **일시정지**: Continue / Restart / Main Menu / Quit Game (`PauseManager`)
- **피격 연출**: 파편 + 흰 화면 Flash → 폭발 + 연기 + 충격파 + 붉은 Flash 2단계, Hit Flash · 카메라 흔들림 · Hit Stop · 사운드. 파티클을 재사용해 피격마다 Instantiate하지 않음 (`HitEffectManager`)

## 조작법

| 입력 | 동작 |
|---|---|
| 방향키 / WASD | 이동 |
| Space | 쉴드 |
| ESC | 일시정지 (Score Ranking 화면에서는 메인 메뉴로 돌아가기) |
| Enter | 메인 메뉴에서 Game Start |
| R | 게임오버 후 재시작 |

## 실행 방법

1. Unity Hub에서 이 `Dodge/` 폴더를 프로젝트로 추가하고 Unity **6000.6.0f1**로 엽니다.
2. `Assets/Scenes/SampleScene.unity`를 열고 ▶ Play를 누릅니다.

## 폴더 구조

```
Dodge/
├── Assets/
│   ├── Scenes/        SampleScene(게임), MainMenu
│   ├── Scripts/       게임 스크립트 (11개)
│   │   └── Editor/    씬 구성 도구 DodgeSetupTool (에디터 전용)
│   ├── Prefabs/       Bullet, Bullet Spawner
│   └── Materials/     플레이어 · 탄알 · 바닥 · 쉴드 · 피격 파티클 머티리얼
├── Packages/          manifest.json, packages-lock.json
├── ProjectSettings/
└── Documentation/     프로젝트 보고서 (Word)
```

## 에디터 메뉴

| 메뉴 | 기능 |
|---|---|
| `Tools > Dodge > Setup Shield, Pause Menu, High Score` | 쉴드 · 일시정지 · 최고 점수 오브젝트와 UI 생성 및 연결 |
| `Tools > Dodge > Setup Hit Effects` | 피격 연출 오브젝트 생성 |
| `Tools > Dodge > Setup Main Menu & Records` | 메인 메뉴 · 랭킹 UI 생성 및 버튼 OnClick 연결 |

모두 `DodgeSetupTool.cs`에 있으며, 이미 있는 오브젝트는 재사용하므로 여러 번 실행해도 안전합니다. 저장된 씬은 이 도구들을 실행하지 않아도 그대로 동작합니다.

## 문서

- [`Documentation/Dodge 보고서.docx`](Documentation/) — 프로젝트 보고서
