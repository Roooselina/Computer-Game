from pathlib import Path
import json,re
out=Path('Docs/Media/pixel_restore')
log=Path('Temp/EWBridge/result.txt').read_text(encoding='utf-8-sig')
start=log.rfind('CMD test playmode\n')
# Include the final full run, final EditMode run, build and restored screenshot.
start=log.rfind('\n',0,start)+1 if start>=0 else 0
(out/'final_verification.log').write_text(log[start:],encoding='utf-8')
(out/'verification_history.log').write_text(log,encoding='utf-8')
Path('Docs/PIXEL_RESTORE_REPORT.md').write_text('''# 개별 픽셀 아트 복구 · AA/DD 대시 실행 보고

이번 요청 범위인 기존 아트 구성 복구, 더블탭 대시 구현, 개별 도트 샘플 연결·플레이 검증을 완료했다. 샘플은 본편에 일괄 적용하지 않았다.

## 복구 기준과 보존

Git 저장소가 없어 Git reset/revert를 사용하지 않았다. `Backups/2026-11-15_art_play_overhaul/`의 아트 작업 직전 코드·씬과 남아 있던 `Assets/Game/Art/Placeholder`, `Assets/ThirdParty` 원본을 비교해 복구했다. 이번 수정 직전 상태도 `Backups/2026-11-15_pixel_restore_doubletap/`에 별도 보관했다. 현재 프로젝트를 수정 중인 다른 활성 에이전트가 없는 것을 세션 목록으로 확인했다.

- `EWSceneBuilder.cs`: `IllustratedArtSetup.Apply(ctx)`만 제거. 최근 맵 배치·충돌·기믹 정의는 유지하고 생성 루트만 재구성.
- `IllustratedArtSetup.cs`: 통짜 배경 적용 기능을 비활성화. 기존 도구 호출이 잘못된 아트를 다시 적용하지 않도록 복구 도구로 연결.
- `EWMenu.cs`, 새 `PixelArtRestore.cs`: 원래 개별 그림을 쓰는 일반 전체 구성 경로로 변경. 사용자 별도 루트와 커스텀 그림은 보존.
- `GameConfig.asset`: 일러스트로 바뀐 FX 4세트와 스킬 아이콘 4세트만 원래 프레임으로 복구. 나머지 설정은 보존.
- 본편 13개 맵과 Title 씬 재구성. `.unity.meta` 변경 0건. 배경·소품·원경/중경/전경은 원래 개별 스프라이트 참조·배치·정렬·패럴랙스를 사용.
- `PixelScreen.cs`: 640×360 네이티브 렌더 타깃, Point, 가능한 경우 정수 배율 복구. 최근 신호 왜곡 효과 및 RenderTexture 안전 해제 수정은 보존.
- 새 일러스트 52개와 메타는 삭제하지 않고 `Assets/Game/Art/Archived/Illustrated_2026_11_15/`로 이동. 활성 본편 씬과 GameConfig의 해당 GUID 참조는 0건.

원본 PNG·메타·머티리얼 등 639개 파일의 SHA-256이 수정 전과 동일하다. 기존 원본이 아닌 그림을 만들어 복구했다고 하지 않았다. 원래 맵은 문자 격자에서 개별 SpriteRenderer/BoxCollider2D를 만드는 방식이었으며, 복구할 실제 Unity Tilemap/Tile Palette/프리팹은 없었다. 새 샘플에 실제 Tile/Tilemap과 연결 프리팹을 추가했다.

Space 상호작용, W/S 사다리, 단말 WASD, Q/E/R/F, 대사 속도·오프닝·포획·통 파괴, 추격·엄폐, 베넬 등장·보스 HP·화학 공격, 전투 후 통행 및 휴게실·초콜릿·협상·살리기/처치·동행·저장 개선은 되돌리지 않았다. `EWRooms`, 본문 스토리/전투 AI 코드는 이 작업에서 변경하지 않았다. 이야기 코드의 대시 안내 한 줄은 최신 키 표기로 갱신했다.

## AA/DD 대시

`DoubleTapDash.cs`를 추가하고 `GameInput.cs`, `PlayerController.cs`, `ActionTuning.cs`, `action_tuning.json`을 연결했다. 같은 방향 눌림→떼기→눌림만 0.25초 이내에 인정한다. 첫 입력은 즉시 이동, 둘째 입력은 해당 방향 대시다. 길게 누르기·같은 상태 반복·AD/DA·쿨다운 입력 예약은 인정하지 않는다.

대화/문서/메뉴/컷신·사다리·이탈 후 0.35초·컴퓨터 내부·경직·이동 차단 중에는 기록하지 않는다. 컨텍스트 세대 번호와 포커스/일시 중단/비활성화/텔레포트/재시도 초기화로 기록이 상태를 넘어가지 않는다. 기존 거리 2.4u, 시간 0.2초, 무적 0.28초, 쿨다운 0.6초, 충돌 처리와 전하 비용 없음은 유지했다. `player.doubleTapWindow`로 조정 가능하다.

기본 Shift 바인딩과 보조 RightShift를 제거했고, 저장된 옛 Dodge 키도 읽지 않는다. 키 변경 화면은 실제 최신 12개 동작을 다루도록 잘못된 옛 인덱스를 수정했다. 대시는 독립 키 대신 현재 이동 키 두 번으로 표시한다. HUD·안내서·타이틀·튜토리얼·설정·현재 조작 문서를 갱신했다. 선택적인 게임패드 트리거와 자동 검증용 Act.Dodge는 유지한다.

## 새 샘플과 실제 연결

실제 이미지 생성 도구를 호출했으나 결과는 요청한 정확한 격자/치수를 만족하지 않았다. `Media/pixel_restore/generated_design_reference/not_a_tileset.png`에 디자인 참고용으로만 보관했다. 최종 PNG는 고해상도 그림의 축소·모자이크가 아니라 `Tools/author_native_pixel_samples.py`의 네이티브 픽셀 좌표로 새로 구성했다.

- 발판 왼쪽/중앙/오른쪽: 각각 32×16, 상단 중앙 피벗.
- 배관 수평/수직/꺾임/T/오른쪽 T/끝마개: 각각 16×16, 중앙 피벗. 접합 포트 폭 8px.
- 독립 실험통: 80×112, 아래 중앙 피벗. 기존 실험통 프레임과 동일 크기.
- 10개 PNG, 총 25색, 알파 0/255, 모두 32 PPU·Point·무압축·밉맵 없음. 기존 캐릭터는 원본 48×64 프레임을 그대로 사용.
- 발판 좌/중/우와 배관 접합부의 픽셀 일치, 투명도·치수·팔레트를 자동 검사.
- `Assets/Game/Art/PixelSamples/Tests/PixelConnectionTest.unity`: 발판 Tilemap(1u), 배관 Tilemap(0.5u), 독립 실험통, 기존 플레이어와 사다리. 원경0.7/중경0.3/플레이0/전경−0.06을 분리. 독립 배경 조각을 반복 배치.
- 발판 상단만 별도 기능성 BoxCollider2D로 구성하고 장식 배관/실험통은 통행을 막지 않는다. 그림 외곽을 충돌체로 쓰지 않는다.
- 9개 Tile 에셋과 `PipeConnectionGrid.prefab` 조립 예시. 원래 맵의 배치 정보를 변환하거나 잃지 않았다. 테스트 씬은 본편 진행·빌드 목록에 포함하지 않는다.

상세 대응표: [PIXEL_ASSET_MAPPING.md](PIXEL_ASSET_MAPPING.md). Unity 메뉴 `EW > 아트 > 개별 도트 샘플 테스트 씬 만들기` 또는 위 씬을 열고 Play로 확인할 수 있다.

## 실행 검증과 화면

Unity 6000.6.0f1 / 실제 Built-in 렌더 파이프라인에서 실행했다. URP 패키지 설치 여부와 실제 사용 중인 파이프라인은 구분했다.

- EditMode **37/37**, PlayMode **20/20** 통과. 새 테스트는 실제 플레이어의 양방향 대시·첫 이동·쿨다운·AD·UI 전환, Input System KeyboardState의 Shift/눌림/뗌/유지·반복 상태, 포커스 콜백·텔레포트 초기화를 확인한다.
- 실제 샘플 Tilemap에서 캐릭터가 상단에 서고 걷는 것, 두 레이어의 서로 다른 이동량, W 사다리/A·D 이탈과 대시 오인 방지, 32 PPU/실험통 크기를 확인했다.
- 기존 Space 중복 방지·W/S·단말·추격/공격·엄폐·스킵 복구·베넬 분기·가까운 재시도·휴게실·저장 테스트도 모두 통과했다.
- 실제 Play 녹화 로그: DD `dodging=True vx=12`, AA `dodging=True vx=-12`, 길게 누르기 `dodging=False vx=4`. 5초/42프레임을 캡처했다.
- Windows 빌드 **Succeeded, 오류 0 / 경고 0**. `Builds/Windows/ElectricWorld_LastLight.exe` 실행 후 초기화·응답 확인, 로그 Exception/Error/Failed 없음. 타이틀 초기 실행 검사이며 빌드판을 사람이 처음부터 끝까지 플레이했다는 뜻은 아니다.
- 개발 재생은 사용자 저장 대신 `Temp/EWDevSave`, 자동 테스트는 임시 저장 경로를 사용했다.

[복구 전 실제 화면](Media/pixel_restore/before/illustrated_lab.png) · [같은 구도 복구 후 실제 화면](Media/pixel_restore/after/restored_lab.png)

[개별 도트 샘플 확대](Media/pixel_restore/native_samples_preview.png) · [캐릭터 포함 실제 연결 화면](Media/pixel_restore/sample_connection_game.png) · [AA/DD 실제 연속 프레임 GIF](Media/pixel_restore/dash_playback.gif) · [연속 프레임 모음](Media/pixel_restore/dash_contact_sheet.png)

[원본 보존/참조 검사](Media/pixel_restore/restoration_audit.json) · [픽셀/접합 검사](Media/pixel_restore/sample_pixel_audit.json) · [변경 파일/씬 메타/패럴랙스 목록](Media/pixel_restore/change_manifest.json) · [최종 테스트/빌드 로그](Media/pixel_restore/final_verification.log) · [전체 검증 이력](Media/pixel_restore/verification_history.log)

## 남은 범위와 검증 한계

실험통은 정적 독립 소품 샘플이다. 유리·액체·기포 개별 애니메이션과 파괴 상태, 전체 벽/천장 안팎 모서리, 밸브/파손 배관 및 전체 맵 교체는 이번에 완료했다고 하지 않는다. 원래 본편의 임시 아트는 복구 상태로 남아 있다. 이 샘플을 사용자 확인 전에 전체 맵에 적용하지 않았다.

플레이 검증은 Unity 실제 씬에 자동 입력/키 상태를 주입한 테스트와 화면 관찰이다. 사람이 실제 키보드로 완주하거나 실제 OS Alt+Tab/키 반복을 수동 검증하지 않았다. 포커스 테스트는 콜백을 주입했다. 첨부 스타일과 최종 선호의 주관적 일치는 샘플 화면으로 검토할 수 있도록 남겼다.
''',encoding='utf-8')
Path('Docs/IMPLEMENTATION_STATUS.md').write_text('''# 구현 상태 — 개별 픽셀 아트 복구 · AA/DD 대시

현재 기준은 [PIXEL_RESTORE_REPORT.md](PIXEL_RESTORE_REPORT.md)다. 이전 52개 통짜 일러스트 교체는 취소하고 별도 보관했다. 원본 639개 파일·원본 씬 메타는 변경하지 않았으며 활성 씬/설정의 일러스트 참조는 0건이다.

기존 게임플레이·연출 개선은 유지한다. 현재 조작은 Space 상호작용/점프, W/S 사다리, A/D 이탈, AA / DD 대시, Q/E/R/F 직접 기술, 단말 WASD 무중력 이동이다.

새 개별 도트 10개는 별도 Tilemap 테스트 씬에만 적용했다. 전체 맵을 일괄 교체하지 않았다. 픽셀 규격과 대응표는 PIXEL_ASSET_MAPPING.md, 실제 복구/연결 화면과 연속 프레임은 Media/pixel_restore에 있다.

최종 EditMode 37/37, PlayMode 20/20 통과. Windows 빌드 오류 0/경고 0, 실행 초기화·응답·로그 오류 없음. 자동 입력 검증이며 수동 완주와 동일하게 보고하지 않는다. 정적 실험통 샘플의 별도 액체·기포 애니메이션, 전체 모서리/벽 타일은 미제작이다.
''',encoding='utf-8')
print('Final report, status and verification logs saved')
