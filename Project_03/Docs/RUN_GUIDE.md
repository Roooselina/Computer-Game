# 실행 안내 — 아트·플레이 개선판

Unity 6000.6.0f1에서 `Assets/Game/Scenes/Title.unity`를 열고 Play한다. Windows 배포 실행 파일은 `Builds/Windows/ElectricWorld_LastLight.exe`다.

기본 조작: A/D 이동, Space 조사 우선/대상 없으면 점프, W/S 사다리, A/D 사다리 이탈, AA / DD 대시, Q 공격·E 간섭·R 강한 방전·F 전원선 이동, Esc 메뉴, Enter 컷신 스킵. 대화는 Space 문장 완성/다음, 방향키 선택 후 Space 확정이다. 단말은 WASD 무중력 자유 이동이며 점프가 없다.

단말에서는 인터넷과 안내서를 확인한 뒤 안내서를 닫으면 포획이 시작된다. 안내서는 Esc 메뉴에서 다시 볼 수 있다. HUD와 안내서는 설정에서 바꾼 실제 키를 사용한다.

아트는 `Assets/Game/Resources/EW/Illustrated/`에 있고 모든 완성 PNG가 실제 참조된다. 일반 `EW > 1. 전체 구성`에도 새 아트 재연결을 통합했다. 이 도구는 씬의 `__Generated`를 다시 만들므로 직접 작성할 오브젝트는 그 바깥에 둔다. 원본 그림은 유지한다.

개발용 자동 재생과 촬영은 `Temp/EWDevSave`를 사용한다. 사용자 저장 파일을 테스트 목적으로 삭제하거나 개발 데이터로 덮어쓰지 않는다.

실제 변경, 전후 비교, 움직이는 WebP/연속 프레임, 테스트·빌드 결과 및 남은 임시 자산: [실행 보고](ART_PLAY_OVERHAUL_REPORT.md). 상세 키: [입력 안내](INPUT_GUIDE.md).

## 본편 전체 (2026-11-15 추가)

- 새 게임은 실제 오프닝(P01 불타는 마을)에서 시작해 1~3장 → 승강장 → 4~6장 → 7~11장 → 크레딧 → 가상 바탕화면 → 타이틀까지 출구와 사건으로 이어진다. 개발용 중간 시작점은 기본 시작점이 아니다.
- 6장 끝 이전 저장(수용동 연결문 앞)은 ‘이어하기’ 후 연결문으로 들어가면 7장으로 이어진다 (예전처럼 타이틀로 돌아가지 않는다).
- **검수 도구**: 게임 중 Esc → ‘검수 도구’. 맵·주요 사건으로 이동할 수 있고, 처음 이동하는 순간부터 저장은 `%USERPROFILE%/AppData/LocalLow/DefaultCompany/ELECTRIC WORLD - LAST LIGHT (Demo)/review/` 에만 쓰인다 (일반 저장과 분리). 자세한 키: [입력 안내](INPUT_GUIDE.md) 마지막 절.
- 건물 도면 반영: [건물 배치 가이드](BUILDING_LAYOUT_GUIDE.md).
- 개발용 자동 경로 (에디터 브리지): `EW.DevHooks.StartGameToEndRecorded` (새 게임 → 엔딩), `EW.DevHooks.StartCh711Recorded` (배치 데이터 사건 프리셋 → 엔딩, 설정 `Temp/EWBridge/ch46.txt`).

