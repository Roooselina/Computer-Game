> 과거 작업 기록: 이 보고서의 52개 일러스트 연결은 이후 취소되어 보관 경로로 옮겨졌다. 현재 아트/조작 상태는 PIXEL_RESTORE_REPORT.md를 참고한다. 정상 게임플레이·연출 수정은 보존했다.

# 아트·플레이 개선 실행 보고

프로젝트: `C:/Users/kuroc/Unity/term_project` · 작업일: 2026-11-15.

제안서가 아니라 실제 코드·씬·그림·셰이더를 수정하고 Unity에서 실행했다. 새 PNG 52개는 모두 씬 또는 GameConfig에 연결돼 있다. 전체 진행은 가상 입력을 사용한 실제 Play Mode로 새 게임부터 휴게실 초콜릿·임시 동행까지 363.7초에 도달했고, 첫 동행 전선 이동까지 383.8초에 도달했다. 최종 시각 수정 뒤 관련 장면을 따로 다시 촬영했다. 사람의 손 조작이나 주관적인 재미 평가를 대신하는 기록은 아니다.

## 프로젝트 확인과 보존

- Unity **6000.6.0f1**. URP 17.6 패키지가 설치돼 있지만 런타임 `GraphicsSettings.currentRenderPipeline`은 null이다. 실제 실행은 Built-in이다. QualitySettings의 옛 URP GUID도 프로젝트 내 에셋에 대응하지 않는다. 렌더 파이프라인을 이번 작업에서 교체하지 않았다.
- 실제 시작: `Title` → `P01` 마을 → `P02` 집 → `P03` 단말 → `C01` 실험실 → `C02` 관찰 복도 → `C03` 설비 → `C04` 화물/단절 → `C05` 배양 → `C06` 베넬. 협상·살리기 뒤 `M15` 휴게실로 이어진다. 처치 분기는 `M13`을 거쳐 진행한다.
- 앱의 활성 작업 목록과 로컬 세션 기록에서 같은 프로젝트를 동시에 수정하는 다른 활성 작업은 확인되지 않았다. 기존 CLI 세션 기록은 작업 시작 전에 갱신이 멈춘 상태였다.
- 수정 전 Scripts·Data·Scenes와 개발 브리지를 `Backups/2026-11-15_art_play_overhaul/`에 보존했다. 원본 그림은 삭제하지 않았다. 개발 재생·촬영 저장은 `Temp/EWDevSave`를 사용하며 사용자 LocalLow 저장과 분리된다.

## 실제 변경

| 영역 | 구현 |
|---|---|
| 입력 | Space 대상 조사 우선 / 대상 없으면 점프. 같은 물리 키의 동작을 함께 소비한다. 기본 J 제거, Q/E/R/F 직접 기술, W/S 사다리·단말 이동. 키 저장 접두어는 `ew.key3`. |
| 상호작용 | 최소 가로 범위 1.45, 높이 제한과 Sight 시야선. 하나의 대상만 프롬프트 표시. 대화·문서 종료와 맵 전환의 키 래치 유지. |
| 사다리 | 선택창 없이 W/S로 진입·상하 이동. A/D 이탈, 꼭대기·바닥 연결과 0.35초 재부착 방지. |
| 컴퓨터 | 발판·연습 기물·추가 아이콘 제거. 인터넷과 안내서만 유지. WASD 가감속 자유 이동, 중력·점프·낙하 없음, 화면 경계 제한. 인터넷 거절 반응과 지정 대사. 두 대상을 확인한 뒤 안내서를 닫아야 포획 시작. |
| 안내·HUD | 실제 바인딩을 보여주는 안내서와 기술 슬롯. 안내서 예문도 현재 키로 치환한다. 메뉴에서 재열람 가능. 상단 베넬 이름·긴 HP·즉시 피해와 지연 피해 표시. 방송 위치는 HP와 분리. |
| 대사·컷신 | 기본 한글 23자/초. 첫 Space 문장 완성, 다음 Space 진행. Enter 스킵과 수동 선택 유지. 전체 timeScale을 낮춰 연출을 늘리지 않았다. |
| 마을·집 | 몇 걸음 이동 → 돌아봄 → “안돼...” → 절망 자세와 여운 → 짧은 길. 작은 집의 찢긴 구조와 가구를 분리하고 뒤로 마을이 보이게 했다. 양옆 공간은 검정. 마지막 “내 세계로 돌아갈래...” 추가. |
| 전기화·포획 | 접촉·충전·윤곽·방향성 잔상·신호 줄기. 캐릭터 한 축 압축 제거. 포획 프레임 수축, 저항 이동 감소, 동적 아크·행 단위 찢어짐·색 분리. 창은 루를 가리지 않는 오른쪽으로 옮겼다. 효과음 글자 출력은 비활성화/호출 제거. |
| 실험통 | 혼자 깨어남 → 서로 다른 3회 탈출 시도 → 정적 → 화면 밖 일상 대화·발견 → 접근·속닥임 → 루의 거부 → 충전·파괴. 앞 유리/뒤 설비/파괴 상태 분리. 주변까지 뻗는 아크와 장비 정전·파편·잔류 효과. |
| 장소 인지 | 통 옆에서는 장소 확인·출구 관찰. 과거를 떠올리는 대사와 몸짓은 C02의 안전한 진입 구간으로 이동. |
| 탐지·추격 | 경비 초기화 전 CCTV 신고가 유실되는 문제를 대기 신고로 수정. 발각 시 별도 HUD 반응과 주변 경비 전파. 실제 추격·공격·시야 차단 후 수색·복귀 및 엄폐를 테스트했다. |
| 베넬 등장 | 숨 고르기 → 주변 확인 → 앉음·정적 → 실제 등장용 문 열림 → 하품·불평·발견·위협. ActorSpawn의 매 프레임 표시 갱신 때문에 먼저 보이던 문제를 컷신 전용 가림 상태로 해결했다. |
| 보스전 | 기존 43u 대비 37.8u로 약 12% 축소. 분노 시 분사 재조준·확장과 분사→투척 조합, 누출 입자. 후반 압력 폭주: 2.8초 준비, 가까운 3.4u 청록 안전 구역, 강한 피해, 회복 틈. 최초 돌진·잡기 이후 보호·협상 분기 유지. |
| 진행 문 | BackDoorBlock 콜라이더와 닫힘 연결 제거. 등장용 문은 별개 배경 연출. 전투 구역 제한은 종료 후 해제되며 실제 왼쪽 문 자리를 걷는 테스트를 추가했다. 휴게실 이동의 설명 대사는 호출하지 않는다. |
| 종료 복구 | 스킵 시 섬광·신호 왜곡·View 크기·입력·카메라 복구. 렌더 타깃은 카메라에서 분리한 뒤 해제. 이전 게임 객체를 참조하던 전기 효과와 파괴된 UI를 참조하던 말풍선도 정리했다. |

주요 소스: `Core/GameInput.cs`, `Field/PlayerController.cs`, `Action/Guard.cs`, `Action/VenelleBoss.cs`, `Story/StoryEvents*.cs`, `Story/Cutscene.cs`, `UI/GuideBook.cs`, `UI/Hud.cs`, `Visual/PixelScreen.cs`, `Visual/ReactionParticles.cs`, `Editor/EWRooms.cs`, `Editor/IllustratedArtSetup.cs`. 모두 `Assets/Game/Scripts/` 아래에 있다. 씬 생성은 기존 `__Generated` 안을 갱신하며 다른 루트는 보존한다.

## 새 그림과 연결

내장 **image_gen.imagegen**을 실제 호출했다. 별도 유료 API/CLI나 API 키는 사용하지 않았다. 선택한 원본 13장과 제작 프롬프트 명세는 [ArtSources manifest](ArtSources/overhaul_2026-11-15/manifest.json)에 보존했다. 완성 PNG는 `Assets/Game/Resources/EW/Illustrated/`에 있다. 배경에 사람·UI 글자·충돌체를 굽지 않았다.

- 연구실·관찰 복도·설비실·휴게실·마을 배경 5장, 투명 집 구조 1장.
- 실험통 후면/유리/파괴, 콘솔·캐비닛·엄폐 선반, 가구·조명, CCTV·문·침대·트롤리·배전함, 지형 6종, UI 아이콘 6종.
- 불·연기·전기·화학 효과 각 4프레임. 정지 그림의 확대만으로 대체하지 않고 flipbook·동적 LineRenderer·개별 운동 입자·빛·소리를 조합했다. 새 반복 입자는 최대 16개 시스템으로 풀링하며 각 시스템 입자는 최대 90개다. 기존 단발 flipbook은 수명이 끝나면 제거한다.
- 최종 VFX의 잘못된 흰 바탕은 실제 이미지 편집 호출로 제거한 뒤 알파 0~255를 검사했다. 사용하지 않은 변형을 완성 자산으로 세지 않았다.
- 논리 UI 640×360을 유지하면서 월드 렌더 타깃은 1920×1080으로 분리했다. 새 그림은 Bilinear/무압축/밉맵 없음/최대 4096 설정이다. 기존 인물 외형과 스탠딩은 유지했다.

[52개 생성/52개 직렬화 참조 감사](Media/art_play_overhaul/asset_code_audit.json). 파일 생성 성공과 연결 확인을 따로 검사했다.

## 화면과 연출 기록

[같은 구도 전후 비교](Media/art_play_overhaul/comparison.jpg) · [수정 전 원본](Media/art_play_overhaul/before/lab.png) · [수정 후 원본](Media/art_play_overhaul/after/lab.png) · [보스 HP](Media/art_play_overhaul/after/boss_hp.png) · [식당](Media/art_play_overhaul/after/cafeteria.png) · [혼합실](Media/art_play_overhaul/after/mixing.png) · [화학 온실](Media/art_play_overhaul/after/greenhouse.png)

| 최종 재촬영 | 연속 프레임 표 | 움직이는 WebP |
|---|---|---|
| 마을 | [프레임 표](Media/art_play_overhaul/sequences/opening_final_contact.jpg) | [재생](Media/art_play_overhaul/sequences/opening_final.webp) |
| 포획 | [프레임 표](Media/art_play_overhaul/sequences/capture_final_contact.jpg) | [재생](Media/art_play_overhaul/sequences/capture_final.webp) |
| 통 파괴 | [프레임 표](Media/art_play_overhaul/sequences/tank_final_contact.jpg) | [재생](Media/art_play_overhaul/sequences/tank_final.webp) |
| 베넬 등장 | [프레임 표](Media/art_play_overhaul/sequences/venelle_final_contact.jpg) | [재생](Media/art_play_overhaul/sequences/venelle_final.webp) |

전체 연결 재생의 1009개 JPEG와 상태 기록: `Docs/Media/art_play_overhaul/run_051358/frames.tsv`. 이 연속 재생 뒤 발견한 시각 수정은 위 최종 재촬영에서 확인했다. 자동 읽기 지연 0.9~1.0초를 사용한 최종 측정: 마을 19.4초, 집 약 22초, 단말 도착 2.8초, 포획 6.1초, 통 35.1초, 베넬 등장 26.8초. 실제 사용자는 Space를 기다리므로 읽는 시간에 따라 달라진다.

## 검증과 한계

최종 Play Mode **17/17 통과** (마지막 전체 실행 05:34:44). 이후 키 공유 설정의 순수 규칙과 편집기 연결을 보완했고, Edit Mode **31/31 통과** 및 일반 `EW > 전체 구성` 재생성 뒤에도 그림 **52/52 참조**를 확인했다. 최종 Windows 빌드는 **Succeeded / 오류 0 / 경고 0** (05:39:11).

근거: [플레이 모드 로그](Media/art_play_overhaul/final_tests_build.log), [최종 구성·편집 모드·빌드 로그](Media/art_play_overhaul/release_verification.log), [추가 공간 촬영 로그](Media/art_play_overhaul/optional_scene_verification.log). 중간 실행에서 발견한 종료 오류를 수정해 재실행했다. 대사 정적 검사가 일반 그림 이름을 대사 ID로 오인하던 정규식도 실제 Say/Ask 호출 대상으로 좁혔다.

생성된 Windows 실행 파일을 별도 프로세스로 실행해 엔진 초기화·프로세스 응답과 로그 오류 0을 확인한 뒤 종료했다. 숨김 창으로 실행했으므로 Windows 타이틀 화면 시각 캡처나 배포판 전체 플레이까지 확인했다고 보고하지 않는다. [실행 확인 기록](Media/art_play_overhaul/windows_smoke.json). 전체 플레이·시각 검증은 위 Unity Play Mode 기록을 기준으로 한다.

검증 항목: Space/점프 충돌, J 없는 기본 진행, W/S 사다리·A/D 이탈, 단말 무중력·발판 제거·안내서, 엄폐와 CCTV·경비의 추격·실제 피해·추적 해제, 컷신 스킵·선택 보호·재시도, 통 안 수용, 보스 첫 공격 보호, 잡기/던지기, 압력 공격 안전 구역, 전투 후 왼쪽 문 통과, 협상·처치·생존·초콜릿·동행·따로 가기·저장 유지.

인물 스탠딩·표정 일러스트는 요청 범위에서 제외되어 기존 임시 그림 그대로다. 없는 표정/포즈 그림을 새로 완성했다고 세지 않았다. 일부 작은 표지·자료판·바구니·캐릭터/경비 스프라이트와 기존 단발 스파크는 기존 자산을 사용한다. 따라서 프로젝트의 모든 임시 그림을 한 점도 남김없이 교체했다고 주장하지 않는다.

새 맵을 별도로 추가한 것이 아니라 기존 구간의 역할과 진행을 재사용하면서 아트·조작·기물·잠입 반응·보스 동선을 수정했다. 장기 플레이 난이도·사람의 조작감, 물리 키보드 재설정 모든 조합, 게임패드의 모든 조합은 검증 범위 밖이다. 실제 소리를 듣는 청음 도구가 없어 청음 평가는 하지 못했다. 오디오 재생 코드·연출 호출은 유지/연결했다. 섬광·흔들림 감소의 모든 장면을 별도로 반복 재생한 결과로 보고하지 않는다.

실행: Unity에서 `Assets/Game/Scenes/Title.unity`를 열고 Play. Windows 실행 파일은 `Builds/Windows/ElectricWorld_LastLight.exe`다. 개발 전용 도구와 테스트 저장은 배포 게임 저장과 분리된다.
