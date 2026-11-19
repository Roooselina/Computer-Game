# Zombie Survivor

Unity 기반 싱글플레이 3D 좀비 서바이벌 슈터입니다. 카메라를 기준으로 이동하면서 좀비 웨이브를 상대하고, 수류탄과 회피 대시로 생존합니다.

## 개발 환경

- Unity **6000.6.4f1**
- C# / Universal Render Pipeline 17.6.0
- Cinemachine 6.6.0, AI Navigation 2.0.14, uGUI / TextMesh Pro
- Legacy Input Manager, Rigidbody 및 CapsuleCollider 이동

## 실행 방법

1. 저장소를 내려받고 Unity Hub에서 프로젝트 루트 폴더를 추가합니다.
2. Unity 6000.6.4f1로 열고 패키지 복원과 에셋 임포트가 끝날 때까지 기다립니다.
3. `Assets/Scenes/Main.unity`를 열고 Play를 누릅니다.
4. 실행 파일은 Unity의 **File → Build Profiles**에서 Main 씬을 포함해 빌드합니다. 별도 자동 설정이나 검증 메뉴는 필요하지 않습니다.

`Assets`, `Packages`, `ProjectSettings`와 모든 필요한 `.meta` 파일이 원본 프로젝트입니다. Library, Temp, Logs, 사용자 설정 및 실행 파일은 포함하지 않습니다.

## 기본 조작

| 입력 | 동작 |
|---|---|
| W / A / S / D | 카메라 기준 전후좌우 이동 |
| 마우스 | 시점 및 조준 |
| 마우스 왼쪽 버튼 | 사격 |
| R | 재장전 |
| G | 수류탄 투척 |
| Space | 회피 대시 |
| ESC | 일시정지 / 재개, 커서 해제 |

일시정지 화면에서 수평·수직 마우스 감도와 수직 반전을 설정할 수 있습니다. 게임 오버 후 화면의 재시작 버튼으로 다시 시작합니다.

## 주요 기능

- 화면 중앙 조준점과 Raycast 사격, 탄창 및 예비 탄약 관리
- 벽을 따라 이동하고 낮은 단차를 통과하는 Rigidbody 이동과 카메라 장애물 보정
- 일반·고속·중장갑 좀비, NavMesh 추적과 근접 공격, 웨이브 진행
- 중력 기반 수류탄 투척, 거리별 폭발 피해, 중복 Collider 피해 방지 및 보충 아이템
- 회피 대시, 벽 충돌 제한, 쿨다운과 짧은 무적 시간
- 체력팩·탄약팩·코인·수류탄팩 생성과 획득
- 체력·탄약·점수·웨이브·수류탄·대시 HUD
- 상체 조준 애니메이션과 양손 IK, 사격·폭발·피격 사운드 및 효과
- 일시정지, 게임 오버 입력·생성 제한, 재시작

## 폴더 구조

```text
Assets/
  Animations/          Animator, 클립, AvatarMask
  Audios/              음악과 효과음
  Fonts/               글꼴
  Materials/           머티리얼
  Models/              캐릭터, 총기, 맵 모델
  Prefabs/             플레이어, 좀비, 아이템, 효과, HUD
  Scenes/              Main 씬, 베이크 조명, NavMesh
  ScriptableData/      총기와 좀비 데이터
  Scripts/             실제 게임 시스템
  Settings/            렌더링 설정
  Sprites/             UI 이미지
  TextMesh Pro/        텍스트 리소스와 셰이더
  Textures/            텍스처
Packages/              패키지 목록과 잠금 파일
ProjectSettings/       Unity 프로젝트 설정
```

기존 에셋 경로와 GUID를 유지했습니다. TextMesh Pro의 글꼴·이미지 라이선스 및 출처 문서는 리소스와 함께 보존합니다.

## 저장소 관리

루트의 `.gitignore`는 게임 원본 폴더와 이 README 및 Git 설정만 업로드 대상으로 허용합니다. 로컬 백업·발표 자료·검증 기록은 업로드 대상이 아닙니다. 새로운 루트 문서를 공유하려면 해당 파일을 명시적으로 허용해야 합니다.

Burst와 일부 테스트 관련 패키지는 URP/Collections 등의 전이 의존성으로 남을 수 있습니다. Unity 패키지 메뉴이며 게임 전용 검증 코드가 아닙니다. 메뉴를 숨기기 위해 패키지 파일을 직접 수정하지 않습니다.
