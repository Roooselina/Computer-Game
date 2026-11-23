# 건물 배치 가이드 (도면 교체용)

2026-11-15 작성. 지금의 맵 배치는 **GDD 기준 임시 배치**다. 최종 건물 도면이 아니다.
도면이 오면 아래 순서로 맵·동선·대사·연출을 **함께** 고친다. 도면 파일만 넣는다고 자동으로 다 바뀌지는 않는다.

## 1. 현재 임시 연결도

```
[0 집과 마을]  P01 불타는 마을 ─(연출)→ P02 무너진 집 ─(연출)→ P03 컴퓨터 안(단말)
[1 포획동]     P03 ─(사건)→ C01 실험실 → C02 격리 실험동 복도
[2 연구동]     C02 → C03 설비실·자료실 ⇄ M09 직원 식당★
               C03 → C04 화물 적재장
[3 약품동]     C04 → C05 배양실 ⇄ M14 약품 온실★
               C05 → C06 연결 복도(베넬) → M13 혼합실 → M15 관찰 휴게실
[4 지하]       M15 → M16 화물 승강장(B1→B2 승강기) → M17 폐기소 거처 ⇄ M18 물 펌프실 / M19 폐품 시장★
[5 정비동]     M17 → M20 유지보수 터널 → M21 정비 작업장 → M22 컨베이어 홀 → M23 수직 배선통로(B2→1F)
[6 숙소]       M23 → M24 공동 세탁실 ⇄ M25 숙소 3호 / M26 기숙사 옥상★
[7 수용·접수동] M24 ─(연결문 사건, 뒤에서 잠김)→ M27 수용동 → M28 접수 홀(메이린) → M29 상담 부스 → M30 영상실
               M30 ─(격벽 사건)→ M31 관제탑(2F)
[8 연구주임]   M31 → M32 상층 회랑(3F, 라비엘) → M33 라비엘의 방 → M34 통신실 → M35 연결 교량
[9 공연·서버]  M35 → M36 대기실 ⇄ M38 녹음 보관실★ → M37 공명 공연장(라크리모사) → M39 서버실
               M39 ─(포획 사건)→ M40 포획 단말 ─(사건)→ M41 휴지통 목록 ─(복원 사건)→ M39(방출 접점)
[10 전이동]    M39 → M42 승선 대기실(4F) → M43 캡슐 회랑 → M44 전이 장치실(최종전) → M45 붕괴 연결로
[11 잔해]      M45 ─(추락 사건)→ M46 무너진 중정 → M47 잔해 더미 → 크레딧 → 가상 바탕화면
```

맵 45개(★ 선택 5곳). GDD 의 48맵 목표 중 00~02·05+06·07+08 은 시연판 통합 상태 그대로다.
층(B1·B2·1F~4F)은 **임시 표기**이며 실제 높이·계단 위치는 도면을 따른다.

## 2. 수정할 데이터 위치

| 무엇 | 어디 |
|---|---|
| 맵 ID·표시 이름·장·구역·층·안내 문구(현재 목표)·안전 스폰·정상 진행의 다음 맵 후보·필수 조건 | `Assets/Game/Resources/EW/world_layout.json` → `maps[]` |
| 출입구 ID → 도착 맵·스폰, 잠금(require)·봉쇄(block)·잠김 대사(blocked) | 같은 파일 `maps[].exits[]`. **7~11장 방은 이 값을 그대로 읽어 출구를 만든다** (`EWRoomsLate.cs` 의 `LExit`/`LDoor`). 1~6장 방은 코드(`EWRooms.cs`, `EWRoomsUnderground.cs`)에 출구가 있고 JSON 은 거울이다 — 둘을 함께 고치면 검사기가 맞는지 확인한다 |
| 출구가 아닌 사건으로 넘어가는 연결 (연출·추락·포획 등) | `storyLinks[]` |
| 사라진 맵·바뀐 스폰의 옛 저장 이전 | `remap[]` (맵 단위), `chapterSafe[]` (장 단위 안전 지점), 맵별 `safeSpawn`. 런타임: `Game.ContinueGame`, `Game.OnMapReady` |
| 검수 도구의 주요 사건 프리셋 (선행 상태·플래그·베넬 분기) | `events[]` (`before` 로 사슬) |
| 방의 지형(격자)·소품·인물·카메라 기준점(마커)·순찰 구간·촬영 구역 | `Assets/Game/Scripts/Editor/EWRooms*.cs` 의 각 `Mxx()` |
| 사건 발생 조건·완료 플래그 | `Assets/Game/Scripts/Runtime/Story/StoryEvents*.cs` (사건 ID ↔ 방의 EventZone/Inspect) |
| 대사·문서·말풍선 | `Assets/Game/Data/Dialogue/dialogue_ko.json` (`Tools/ch7_11_dialogue.py` 등으로 생성) |

사건 ID·대사 ID·저장 플래그는 맵 위치와 무관하다. 맵이 옮겨져도 같은 ID 를 새 방에 다시 놓으면 저장 상태가 유지된다.

## 3. 공간 의존 대사·컷신 (도면이 바뀌면 다시 읽을 것)

`world_layout.json` 의 `maps[].spaceLines` 에 ID 로 적어 두었다. 주요 항목:

| 맵 | 대사·컷신 | 의존하는 공간 정보 |
|---|---|---|
| M16 | c4_platform_sign, c4_seal_look, c4_lift_up_sealed, c4_lift_ride(_v) | 승강기가 위층 봉쇄·지하로만 감 |
| M17 | c4_meet_2, c4_hatch, c4_deal_pump | 승강기 위가 막힘, 대피실 해치, 펌프실 문 |
| M24 | c7_gate_look, 컷신 ch7_gate | 숙소 복도 끝 수용동 연결문 |
| M27 | c7_release_route(_v), c7_stairs, c7_gate_shut | ‘비상 계단 → 지하 배수로 → 폐기소’ 경로 |
| M29·M30 | c7_booth_enter, c7_feed_* , c7_lockdown(_v), c7_bulkhead_talk, 컷신 c7_feeds·c7_bulkhead | 영상실 옆 관제탑 격벽, 카메라 위치(CAM 07/03/12) |
| M31 | c7_control_open, c7_knock | 관제탑이 격벽 바로 너머, 위층 통로 |
| M39 | c9_force_broadcast, c9_comm_terminal, c9_shelter_after, c9_lu_shock(_r) | 서버실 통신 중계 단말, 지하 B2 대피실 |
| M45 | c10_floor_breaks, 컷신 c10_fall | 연결로 아래가 중정 |

컷신 배치 기준점은 방 정의의 마커(`c.Mark`)다: meet, stairs, held_center, ticket, claw, doll, wet0/1, brake0/1, arena0/1, floor, bulkhead, confront, coupling, magnet, cap0/1, fall, feed_* 등. 컷신 코드는 좌표 숫자 대신 이 마커를 찾는다.

## 4. 도면을 바꿀 때 순서

1. 도면의 방 목록과 지금 맵 ID 를 맞춘다 (같은 역할이면 ID 유지 → 저장·사건이 그대로 이어진다). 합쳐지거나 없어진 맵은 `remap` 에 안전 지점을 적는다.
2. `world_layout.json` 의 층·구역·이름·출구(도착 맵/스폰)·다음 맵 후보·필수 조건을 고친다.
3. 각 방 정의(`EWRooms*.cs`)의 격자·출구 칸·스폰·마커·순찰 구간을 새 지형에 맞춘다. 7~11장 방은 출구 ID 만 맞으면 도착지는 데이터에서 읽는다.
4. 3절의 공간 의존 대사·컷신을 다시 읽고, 위치 표현(‘아래’, ‘계단’, ‘위층’)이 틀리면 대사 데이터를 고친다. 모든 대사를 자동 문구로 바꾸지 않는다.
5. Unity 메뉴 `EW > 1. 전체 구성` (씬 재생성 + 씬 보고서 `Docs/Generated/built_layout_report.json` 생성).
6. `EW > 7. 배치 데이터 검사` 또는 EditMode 테스트 `LateLogicTests.Layout_Check_*` 실행.
7. 검수 도구(Esc)로 바뀐 구간을 실제로 걸어 본다. 자동 경로 봇(`EW.DevHooks.StartGameToEndRecorded`)으로 새 게임→엔딩 연결을 다시 확인한다.

## 5. 검사기가 찾는 것 (`EWLayoutCheck.Check`)

- 끊긴 출구: 없는 맵으로 가는 출구, 도착 맵에 없는 스폰
- 데이터와 씬의 불일치: JSON 출구가 씬에 없음 / 씬 출구가 JSON 에 없음 / 잠금 조건이 다름 / 없는 스폰·안전 스폰
- 도달 불가: 새 게임(P01)에서 출구 + 장면 링크로 갈 수 없는 맵, M47 도달 여부
- 중복 ID: 맵·출구·스폰·사건 프리셋·대사(대사 데이터 파서 오류)
- 없는 사건 ID (방의 EventZone/Inspect 가 부르는 사건이 코드에 없음), 없는 잠김 대사·공간 의존 대사 ID
- 검수 프리셋의 없는 맵·스폰·선행 사건

검사기는 ‘연결이 끊기지 않았는지’만 본다. 동선이 자연스러운지, 대사가 새 공간과 맞는지는 사람이 확인해야 한다.
