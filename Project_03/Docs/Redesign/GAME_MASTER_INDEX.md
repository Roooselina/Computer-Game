# GAME MASTER INDEX — ELECTRIC WORLD: LAST LIGHT

`GAME_MASTER_DESIGN.docx` (v0.1, 2026-11-16) 의 색인. 모든 ID 는 기존 개발 ID 를 그대로 썼다 (새 ID 를 만들지 않음). 문서 위치는 DOCX 의 PART·장·맵 제목.
자동 생성: 코드·씬·데이터 정적 추출 (`Docs/Generated/built_layout_report.json`, `world_layout.json`, `dialogue_ko.json`, `Story/*.cs`). 같은 폴더의 SOURCE_AUDIT.md 에 방법과 한계.

## 1. 문서 목차

| 위치 | 내용 |
|---|---|
| 문서 읽는 법 | 상태 표시 8종, ID 규칙, 이미지 원칙 |
| PART 01 | 개요 · 핵심 플레이 구조 · 진행 요약 · 핵심 문제 P1~P7 · 리뷰 항목 A1~F 전체 |
| PART 02 | 세계관 · 구역 · 캐릭터 10명/집단 · 관계도 · 등장/퇴장 |
| PART 03 | 조작 대조 · 플레이어 수치 · 기술 7종 · 성장 · 동료 · UI 연결 |
| PART 04 | 장별 경로 · 전체 흐름도 · 맵 연결도 5장 · 맵 목록 45 (가로 페이지) |
| PART 05 | 장 12개 · 맵 45개 상세 (A~H) · 장별 기획 검토 (I) |
| PART 06 | 보스 5명 상세·패턴 표 · 기타 조우 · 일반 적 수치 |
| PART 07 | 장별 화자 대사 줄 수 · 인물별 대화·선택 · 말풍선 원문 |
| PART 08 | 장치·상호작용 24종 (가로 페이지) |
| PART 09 | UI 14종 · 튜토리얼 순서 · 피드백 |
| PART 10 | 엔딩 구조 · 최종 상태 · 선택의 결과 · 이어하기 검토 |
| 부록 A~F | 사건 목록 · 미연결 사건 · 미참조 대사 · 문서 · 저장 필드 · 화면 기록 |

## 2. 장별 맵 목록

| 장 | 맵 ID | 이름 | 선택 | 연결(출구) | 문서 위치 |
|---|---|---|---|---|---|
| 프롤로그 | P01 | 불타는 마을 |  | (사건 이동) | PART 05 · 프롤로그 · P01 |
| 프롤로그 | P02 | 무너진 작은 집 |  | (사건 이동) | PART 05 · 프롤로그 · P02 |
| 프롤로그 | P03 | 컴퓨터 안 |  | (사건 이동) | PART 05 · 프롤로그 · P03 |
| 1장 | C01 | 실험실 |  | (사건 이동) | PART 05 · 1장 · C01 |
| 1장 | C02 | 격리 실험동 복도 |  | C03 | PART 05 · 1장 · C02 |
| 2장 | C03 | 설비실 · 자료실 |  | C02, C04, M09 | PART 05 · 2장 · C03 |
| 2장 | M09 | 직원 식당 | ★ | C03 | PART 05 · 2장 · M09 |
| 2장 | C04 | 화물 적재장 |  | C03, C05 | PART 05 · 2장 · C04 |
| 3장 | C05 | 배양실 |  | C04, C06, M14 | PART 05 · 3장 · C05 |
| 3장 | M14 | 약품 온실 | ★ | C05 | PART 05 · 3장 · M14 |
| 3장 | C06 | 연결 복도 |  | C05[막힘:c6_combat_locked], M13[필요:c3_boss_done] | PART 05 · 3장 · C06 |
| 3장 | M13 | 혼합실 |  | C06[막힘:venelle_carried], M15 | PART 05 · 3장 · M13 |
| 3장 | M15 | 관찰 휴게실 |  | M16[필요:c3_boss_done], M13 | PART 05 · 3장 · M15 |
| 4장 | M16 | 화물 승강장 |  | M15, M17 | PART 05 · 4장 · M16 |
| 4장 | M17 | 폐기소 거처 |  | M16, M20[필요:waste_resolved], M18[필요:waste_met], M19[필요:waste_met] | PART 05 · 4장 · M17 |
| 4장 | M18 | 물 펌프실 |  | M17 | PART 05 · 4장 · M18 |
| 4장 | M19 | 폐품 시장 | ★ | M17 | PART 05 · 4장 · M19 |
| 5장 | M20 | 유지보수 터널 |  | M17, M21 | PART 05 · 5장 · M20 |
| 5장 | M21 | 정비 작업장 |  | M20, M22 | PART 05 · 5장 · M21 |
| 5장 | M22 | 컨베이어 홀 |  | M21, M23 | PART 05 · 5장 · M22 |
| 5장 | M23 | 수직 배선통로 |  | M22, M24 | PART 05 · 5장 · M23 |
| 6장 | M24 | 공동 세탁실 |  | M23, M26, M25 | PART 05 · 6장 · M24 |
| 6장 | M25 | 숙소 3호 |  | M24 | PART 05 · 6장 · M25 |
| 6장 | M26 | 기숙사 옥상 | ★ | M24 | PART 05 · 6장 · M26 |
| 7장 | M27 | 수용동 |  | M24[막힘:ch7_entered], M28[필요:holding_released] | PART 05 · 7장 · M27 |
| 7장 | M28 | 접수 홀 |  | M27, M29[필요:c7_meilin_done] | PART 05 · 7장 · M28 |
| 7장 | M29 | 상담 부스 |  | M28, M30[필요:has_staff_card] | PART 05 · 7장 · M29 |
| 7장 | M30 | 영상실 |  | M31[필요:c7_feeds_seen], M29 | PART 05 · 7장 · M30 |
| 7장 | M31 | 관제탑 |  | M30[막힘:c7_bulkhead_closed], M32[필요:c7_control_done] | PART 05 · 7장 · M31 |
| 8장 | M32 | 상층 회랑 |  | M31, M33[필요:c8_duel_done] | PART 05 · 8장 · M32 |
| 8장 | M33 | 라비엘의 방 |  | M32, M34[필요:c8_room_done] | PART 05 · 8장 · M33 |
| 8장 | M34 | 통신실 |  | M33, M35 | PART 05 · 8장 · M34 |
| 8장 | M35 | 연결 교량 |  | M34, M36 | PART 05 · 8장 · M35 |
| 9장 | M36 | 대기실 |  | M35, M37, M38 | PART 05 · 9장 · M36 |
| 9장 | M38 | 녹음 보관실 | ★ | M36 | PART 05 · 9장 · M38 |
| 9장 | M37 | 공명 공연장 |  | M36, M39[필요:c9_lac_done] | PART 05 · 9장 · M37 |
| 9장 | M39 | 서버실 |  | M37[막힘:c9_hall_collapsed], M42[필요:c9_bin_done] | PART 05 · 9장 · M39 |
| 9장 | M40 | 포획 단말 |  | (사건 이동) | PART 05 · 9장 · M40 |
| 9장 | M41 | 휴지통 목록 |  | (사건 이동) | PART 05 · 9장 · M41 |
| 10장 | M42 | 승선 대기실 |  | M39[막힘:c10_boarding_seen], M43 | PART 05 · 10장 · M42 |
| 10장 | M43 | 캡슐 회랑 |  | M42, M44[필요:c10_meilin_seen] | PART 05 · 10장 · M43 |
| 10장 | M44 | 전이 장치실 |  | M45[필요:c10_boss_done] | PART 05 · 10장 · M44 |
| 10장 | M45 | 붕괴 연결로 |  | (사건 이동) | PART 05 · 10장 · M45 |
| 11장·마지막 | M46 | 무너진 중정 |  | M47[필요:c11_stood] | PART 05 · 11장·마지막 · M46 |
| 11장·마지막 | M47 | 잔해 더미 |  | (사건 이동) | PART 05 · 11장·마지막 · M47 |

사건(컷신)으로만 이어지는 이동: P01→P02 (op_village 연출), P02→P03 (op_house 연출), P03→C01 (pc_exit 사건), C01→C02 (실험실 문틈 사건), M24→M27 (ch7_gate 사건 (StoryEvents.Ch7Enter)), M30→M31 (c7_bulkhead 사건), M39→M40 (c9_capture 사건), M40→M41 (c9_d16 사건 (불완전 판정)), M41→M39 (c9_bin_list 사건 (복원·구속 해제)), M45→M46 (c10_fall 사건)

## 3. 이벤트 목록 (맵 씬에 연결된 사건 105개)

| 사건 ID | 맵 | 코드 파일 | 문서 위치 |
|---|---|---|---|
| `pc_enter` | P03 | `Story/StoryEvents.cs` | PART 05 · 프롤로그 · P03 · E |
| `pc_guide` | P03 | `Story/StoryEvents.cs` | PART 05 · 프롤로그 · P03 · E |
| `pc_internet` | P03 | `Story/StoryEvents.cs` | PART 05 · 프롤로그 · P03 · E |
| `lab_tank_label` | C01 | `Story/StoryEvents.cs` | PART 05 · 1장 · C01 · E |
| `op_gap` | C01 | `Story/StoryEvents.cs` | PART 05 · 1장 · C01 · E |
| `lab_pouch` | C01 | `Story/StoryEvents.cs` | PART 05 · 1장 · C01 · E |
| `lab_emergency` | C01 | `Story/StoryEvents.cs` | PART 05 · 1장 · C01 · E |
| `lab_jars` | C01 | `Story/StoryEvents.cs` | PART 05 · 1장 · C01 · E |
| `lab_bed` | C01 | `Story/StoryEvents.cs` | PART 05 · 1장 · C01 · E |
| `lab_tools` | C01 | `Story/StoryEvents.cs` | PART 05 · 1장 · C01 · E |
| `lab_board` | C01 | `Story/StoryEvents.cs` | PART 05 · 1장 · C01 · E |
| `lab_bin` | C01 | `Story/StoryEvents.cs` | PART 05 · 1장 · C01 · E |
| `lab_window` | C01 | `Story/StoryEvents.cs` | PART 05 · 1장 · C01 · E |
| `lab_lockers` | C01 | `Story/StoryEvents.cs` | PART 05 · 1장 · C01 · E |
| `lab_researcher` | C01 | `Story/StoryEvents.cs` | PART 05 · 1장 · C01 · E |
| `checkpoint` | M45 | `Story/StoryEvents.cs` | PART 05 · 10장 · M45 · E |
| `c2s_jam_tut` | C02 | `Story/StoryEvents.cs` | PART 05 · 1장 · C02 · E |
| `c2s_cover_tut` | C02 | `Story/StoryEvents.cs` | PART 05 · 1장 · C02 · E |
| `c2_ad_first` | C03 | `Story/StoryEvents.cs` | PART 05 · 2장 · C03 · E |
| `c2_vending` | M09 | `Story/StoryEvents.cs` | PART 05 · 2장 · M09 · E |
| `c2_lever` | M09 | `Story/StoryEvents.cs` | PART 05 · 2장 · M09 · E |
| `c2_line_tutorial` | C04 | `Story/StoryEvents.cs` | PART 05 · 2장 · C04 · E |
| `m15_gap_look` | M15 | `Story/StoryEvents.cs` | PART 05 · 3장 · M15 · E |
| `m16_lift` | M16 | `Story/StoryEventsUnderground.cs` | PART 05 · 4장 · M16 · E |
| `waste_meet` | M17 | `Story/StoryEventsUnderground.cs` | PART 05 · 4장 · M17 · E |
| `waste_jugs` | M17 | `Story/StoryEventsUnderground.cs` | PART 05 · 4장 · M17 · E |
| `waste_hatch` | M17 | `Story/StoryEventsUnderground.cs` | PART 05 · 4장 · M17 · E |
| `waste_f` | M17 | `Story/StoryEventsUnderground.cs` | PART 05 · 4장 · M17 · E |
| `waste_008` | M17 | `Story/StoryEventsUnderground.cs` | PART 05 · 4장 · M17 · E |
| `waste_survivors` | M17 | `Story/StoryEventsUnderground.cs` | PART 05 · 4장 · M17 · E |
| `waste_fight_end` | M17 | `Story/StoryEventsUnderground.cs` | PART 05 · 4장 · M17 · E |
| `m18_enter` | M18 | `Story/StoryEventsUnderground.cs` | PART 05 · 4장 · M18 · E |
| `m19_enter` | M19 | `Story/StoryEventsUnderground.cs` | PART 05 · 4장 · M19 · E |
| `market_lantern` | M19 | `Story/StoryEventsUnderground.cs` | PART 05 · 4장 · M19 · E |
| `market_scrap` | M19 | `Story/StoryEventsUnderground.cs` | PART 05 · 4장 · M19 · E |
| `market_lines` | M19 | `Story/StoryEventsUnderground.cs` | PART 05 · 4장 · M19 · E |
| `m20_crawl_hint` | M20 | `Story/StoryEventsUnderground.cs` | PART 05 · 5장 · M20 · E |
| `ws_manual` | M21 | `Story/StoryEventsUnderground.cs` | PART 05 · 5장 · M21 · E |
| `ws_after` | M21 | `Story/StoryEventsUnderground.cs` | PART 05 · 5장 · M21 · E |
| `m22_enter` | M22 | `Story/StoryEventsUnderground.cs` | PART 05 · 5장 · M22 · E |
| `hall_off_line` | M22 | `Story/StoryEventsUnderground.cs` | PART 05 · 5장 · M22 · E |
| `m23_chase` | M23 | `Story/StoryEventsUnderground.cs` | PART 05 · 5장 · M23 · E |
| `m23_l2` | M23 | `Story/StoryEventsUnderground.cs` | PART 05 · 5장 · M23 · E |
| `m23_l3` | M23 | `Story/StoryEventsUnderground.cs` | PART 05 · 5장 · M23 · E |
| `m23_top` | M23 | `Story/StoryEventsUnderground.cs` | PART 05 · 5장 · M23 · E |
| `m23_plate_zap` | M23 | `Story/StoryEventsUnderground.cs` | PART 05 · 5장 · M23 · E |
| `m23_shutter` | M23 | `Story/StoryEventsUnderground.cs` | PART 05 · 5장 · M23 · E |
| `m24_enter` | M24 | `Story/StoryEventsUnderground.cs` | PART 05 · 6장 · M24 · E |
| `laundry_twins` | M24 | `Story/StoryEventsUnderground.cs` | PART 05 · 6장 · M24 · E |
| `laundry_others` | M24 | `Story/StoryEventsUnderground.cs` | PART 05 · 6장 · M24 · E |
| `ch7_gate` | M24 | `Story/StoryEventsUnderground.cs` | PART 05 · 6장 · M24 · E |
| `m25_enter` | M25 | `Story/StoryEventsUnderground.cs` | PART 05 · 6장 · M25 · E |
| `room_photo` | M25 | `Story/StoryEventsUnderground.cs` | PART 05 · 6장 · M25 · E |
| `room_letter` | M25 | `Story/StoryEventsUnderground.cs` | PART 05 · 6장 · M25 · E |
| `room_wrappers` | M25 | `Story/StoryEventsUnderground.cs` | PART 05 · 6장 · M25 · E |
| `roof_sit` | M26 | `Story/StoryEventsUnderground.cs` | PART 05 · 6장 · M26 · E |
| `c7_enter` | M27 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M27 · E |
| `c7_stairs` | M27 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M27 · E |
| `c7_release` | M27 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M27 · E |
| `c7_desk` | M27 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M27 · E |
| `c7_hall_enter` | M28 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M28 · E |
| `c7_ticket` | M28 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M28 · E |
| `c7_booth_enter` | M29 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M29 · E |
| `c7_chair` | M29 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M29 · E |
| `c7_booth_lock` | M29 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M29 · E |
| `c7_lostfound` | M29 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M29 · E |
| `c7_bulkhead` | M30 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M30 · E |
| `c7_monitors` | M30 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M30 · E |
| `c7_tower_enter` | M31 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M31 · E |
| `c7_control` | M31 | `Story/StoryEventsLate.cs` | PART 05 · 7장 · M31 · E |
| `c8_meet` | M32 | `Story/StoryEventsLate.cs` | PART 05 · 8장 · M32 · E |
| `c8_room_enter` | M33 | `Story/StoryEventsLate.cs` | PART 05 · 8장 · M33 · E |
| `c8_photo` | M33 | `Story/StoryEventsLate.cs` | PART 05 · 8장 · M33 · E |
| `c8_cups` | M33 | `Story/StoryEventsLate.cs` | PART 05 · 8장 · M33 · E |
| `c8_comm_enter` | M34 | `Story/StoryEventsLate.cs` | PART 05 · 8장 · M34 · E |
| `c8_comm_console` | M34 | `Story/StoryEventsLate.cs` | PART 05 · 8장 · M34 · E |
| `c8_bridge1` | M35 | `Story/StoryEventsLate.cs` | PART 05 · 8장 · M35 · E |
| `c8_bridge2` | M35 | `Story/StoryEventsLate.cs` | PART 05 · 8장 · M35 · E |
| `c8_bridge3` | M35 | `Story/StoryEventsLate.cs` | PART 05 · 8장 · M35 · E |
| `c8_bridge_bench` | M35 | `Story/StoryEventsLate.cs` | PART 05 · 8장 · M35 · E |
| `c9_enter` | M36 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M36 · E |
| `c9_plans` | M36 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M36 · E |
| `c9_chart` | M36 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M36 · E |
| `c9_member1` | M36 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M36 · E |
| `c9_member2` | M36 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M36 · E |
| `c9_member3` | M36 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M36 · E |
| `c9_archive_enter` | M38 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M38 · E |
| `c9_tape` | M38 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M38 · E |
| `c9_stage_enter` | M37 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M37 · E |
| `c9_force` | M39 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M39 · E |
| `c9_capture` | M39 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M39 · E |
| `c9_terminal_in` | M40 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M40 · E |
| `c9_d16` | M40 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M40 · E |
| `c9_bin_enter` | M41 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M41 · E |
| `c9_bin_list` | M41 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M41 · E |
| `c9_bin_status` | M41 | `Story/StoryEventsLate.cs` | PART 05 · 9장 · M41 · E |
| `c10_enter` | M42 | `Story/StoryEventsFinal.cs` | PART 05 · 10장 · M42 · E |
| `c10_board` | M42 | `Story/StoryEventsFinal.cs` | PART 05 · 10장 · M42 · E |
| `c10_member1` | M42 | `Story/StoryEventsFinal.cs` | PART 05 · 10장 · M42 · E |
| `c10_member2` | M42 | `Story/StoryEventsFinal.cs` | PART 05 · 10장 · M42 · E |
| `c10_capsule` | M43 | `Story/StoryEventsFinal.cs` | PART 05 · 10장 · M43 · E |
| `c10_meilin` | M43 | `Story/StoryEventsFinal.cs` | PART 05 · 10장 · M43 · E |
| `c10_final_enter` | M44 | `Story/StoryEventsFinal.cs` | PART 05 · 10장 · M44 · E |
| `c10_fall` | M45 | `Story/StoryEventsFinal.cs` | PART 05 · 10장 · M45 · E |
| `c11_last` | M47 | `Story/StoryEventsFinal.cs` | PART 05 · 11장·마지막 · M47 · E |

맵 진입 사건(`OnMapEnter` → `OnEnterUnderground` → `OnEnterLate` → `OnEnterFinal`)은 맵마다 ‘(맵 진입)’ 블록으로 DOCX E 표에 있다.

## 4. 컷신 목록 (봇 측정 시간 포함)

| 컷신 ID | 맵 | 측정 (전체/대사/자동, 초) | 문서 위치 |
|---|---|---|---|
| `op_village` | P01 | 18.6 / 5.5 / 13.1 | PART 05 · 프롤로그 · P01 · D |
| `op_house` | P02 | 22.4 / 12.8 / 9.6 | PART 05 · 프롤로그 · P02 · D |
| `op_desktop_land` | P03 | 2.9 / 1.2 / 1.6 | PART 05 · 프롤로그 · P03 · D |
| `op_transfer` | P03 | 6.0 / 0.0 / 6.0 | PART 05 · 프롤로그 · P03 · D |
| `op_tank` | C01 | 33.2 / 23.2 / 10.0 | PART 05 · 1장 · C01 · D |
| `corridor_memory` | C02 | 6.7 / 6.0 / 0.7 | PART 05 · 1장 · C02 · D |
| `c3_boss_intro` | C06 | 24.2 / 18.6 / 5.6 | PART 05 · 3장 · C06 · D |
| `wire_duo_first` | M15 | 7.9 / 7.4 / 0.5 | PART 05 · 3장 · M15 · D |
| `wire_duo_arrive` | M15 | 4.1 / 4.1 / -0.1 | PART 05 · 3장 · M15 · D |
| `c4_waste_meet` | M17 | 29.7 / 25.1 / 4.6 | PART 05 · 4장 · M17 · D |
| `ivy_memory` | M25 | 29.4 / 23.8 / 5.6 | PART 05 · 6장 · M25 · D |
| `ivy_room_talk` | M25 | 22.8 / 17.7 / 5.1 | PART 05 · 6장 · M25 · D |
| `c7_release` | M27 | 41.7 / 36.7 / 5.0 | PART 05 · 7장 · M27 · D |
| `c7_meilin_meet` | M28 | 30.4 / 27.7 / 2.7 | PART 05 · 7장 · M28 · D |
| `c7_meilin_retreat` | M28 | 15.5 / 13.6 / 1.9 | PART 05 · 7장 · M28 · D |
| `c7_feeds` | M30 | 30.1 / 24.6 / 5.6 | PART 05 · 7장 · M30 · D |
| `c7_bulkhead` | M30 | 28.0 / 20.9 / 7.1 | PART 05 · 7장 · M30 · D |
| `c7_control` | M31 | 23.6 / 17.9 / 5.7 | PART 05 · 7장 · M31 · D |
| `c8_reunion` | M32 | 27.4 / 26.0 / 1.4 | PART 05 · 8장 · M32 · D |
| `c8_stop` | M32 | 18.2 / 18.1 / 0.1 | PART 05 · 8장 · M32 · D |
| `c8_confession` | M33 | 60.1 / 59.3 / 0.8 | PART 05 · 8장 · M33 · D |
| `c9_lac_meet` | M37 | 21.0 / 19.7 / 1.3 | PART 05 · 9장 · M37 · D |
| `c9_lac_end` | M37 | 30.7 / 27.1 / 3.6 | PART 05 · 9장 · M37 · D |
| `c9_collapse` | M39 | 63.2 / 51.5 / 11.7 | PART 05 · 9장 · M39 · D |
| `c9_capture` | M39 | 7.9 / 7.2 / 0.7 | PART 05 · 9장 · M39 · D |
| `c9_catch` | M39 | 11.2 / 10.6 / 0.6 | PART 05 · 9장 · M39 · D |
| `c10_capsule` | M43 | 9.4 / 8.8 / 0.6 | PART 05 · 10장 · M43 · D |
| `c10_meilin` | M43 | 17.9 / 18.1 / -0.2 | PART 05 · 10장 · M43 · D |
| `c10_confront` | M44 | 45.6 / 43.2 / 2.4 | PART 05 · 10장 · M44 · D |
| `c10_limit_1` | M44 | 13.8 / 13.4 / 0.4 | PART 05 · 10장 · M44 · D |
| `c10_limit_2` | M44 | 8.3 / 7.9 / 0.4 | PART 05 · 10장 · M44 · D |
| `c10_limit_3` | M44 | 4.9 / 4.4 / 0.5 | PART 05 · 10장 · M44 · D |
| `c10_end` | M44 | 28.9 / 26.1 / 2.9 | PART 05 · 10장 · M44 · D |
| `c10_fall` | M45 | 11.3 / 8.5 / 2.8 | PART 05 · 10장 · M45 · D |
| `c11_wake` | M46 | 60.4 / 54.4 / 6.0 | PART 05 · 11장·마지막 · M46 · D |
| `c11_last` | M47 | 32.5 / 25.7 / 6.7 | PART 05 · 11장·마지막 · M47 · D |

측정: 자동 진행 봇 (2026-11-16, `Docs/Media/ch7_11_2026-11-16/cutscene_timings.txt`). ‘대사’는 봇이 넘긴 시간이라 사람 읽기 시간이 아니다. 컷 단위 시간은 없음 [미확인].

## 5. 보스 목록

| 보스 | 맵 | HP | 결말 분기 | 문서 위치 |
|---|---|---|---|---|
| 베넬 | C06 | 30 | 협상 / 제압→처치·살림 | PART 06 · 베넬 |
| 메이린 | M28 | 36 | 퇴각 (생존) | PART 06 · 메이린 |
| 라비엘 | M32 | 34 또는 거리 % | 도망 / 멈춰 세우기 | PART 06 · 라비엘 |
| 라크리모사 | M37 | 40 | 설득 / 제압 | PART 06 · 라크리모사 |
| 아우렐리우스 | M44 | 30×3 단계 | 한계 방출 3회 | PART 06 · 아우렐리우스 |

## 6. 대사 목록 (맵별 대사 ID)

분기 대사가 함께 나열된다. 원문은 DOCX PART 05 각 맵의 E 표.

- **P01 불타는 마을** (3): `op_v_2` `op_v_4` `op_v_no`
- **P02 무너진 작은 집** (5): `op_h_a` `op_h_b` `op_h_c` `op_h_d` `op_h_return`
- **P03 컴퓨터 안** (6): `op_d_1` `pc_log_early` `pc_exit` `pc_log` `op_d_2` `pc_internet_denied`
- **C01 실험실** (28): `op_t_1` `op_t_2` `op_t_routine` `op_t_discovery` `op_t_r1` `op_t_r3` `op_t_calm1` `op_t_no` `op_t_stop` `c1_tank_broken` `esc_1` `esc_2` `lab_broadcast` `c1_console_broken` `lab_clue_tank` `lab_realize` `op_t_escape` `lab_clue_gap` `lab_pouch` `lab_m_emergency` `lab_m_jars` `lab_m_bed` `lab_m_tools` `lab_clue_board` `lab_m_bin` `lab_m_window` `lab_m_lockers` `lab_m_researcher`
- **C02 격리 실험동 복도** (5): `c2s_enter` `esc_3` `esc_4` `c2s_jam_tut` `c2s_cover_tut`
- **C03 설비실 · 자료실** (8): `c2_ad_lacrimosa` `c3_shutter_locked` `c2_ad_aurelius` `c2_ad_meilin` `c2_ad_old_again` `c2_ads_intro` `c2_memory` `c2_memory_after`
- **M09 직원 식당** (8): `c2_vend_off` `c2_vend_intro` `c2_vend_button` `c2_vend_empty` `c2_vend_weak` `c2_vend_overload` `c2_lever_on` `c2_lever`
- **C04 화물 적재장** (1): `sys_crate`
- **C05 배양실** (1): `c3_lab_note`
- **M14 약품 온실** (1): `c3_puddle_inspect`
- **C06 연결 복도** (7): `v_run_in` `v_where` `v_yawn` `v_intro_1` `v_intro_2` `v_intro_3` `sys_locked`
- **M13 혼합실** (3): `c3_chem_shelf` `c3_puddle_inspect` `sys_locked`
- **M15 관찰 휴게실** (17): `r_1` `r_2` `r_3` `r_4` `r_choc` `r_choc_give` `r_choc_keep` `r_nochoc` `r_6` `r_7` `r_8` `r_join_ok` `r_join_no` `r_apart` `c3_rest` `c3_lift_sign` `sys_locked`
- **M16 화물 승강장** (8): `c4_platform_enter` `c4_seal_look` `c4_platform_sign` `c4_lift_panel` `c4_lift_up_sealed` `c4_lift_up_again` `c4_lift_ride_v` `c4_lift_ride`
- **M17 폐기소 거처** (40): `c4_pump_thanks_repair` `c4_pump_thanks_fight` `c4_pump_thanks_gift` `c4_tally` `waste_door_wait` `waste_exit_wait` `c4_meet_1` `c4_meet_2` `c4_deal` `c4_deal_again` `c4_deal_short` `c4_deal_pump` `c4_deal_pump_again` `c4_deal_fail` `c4_gift` `c4_jugs_full` `c4_jugs_empty` `c4_hatch` `c4_f_fight` `c4_f_name` `c4_f_box_gift` `c4_f_box` `c4_f_pump_hint` `c4_f_again` `c4_008_hiding` `c4_008_ask` `c4_008_later` `c4_008_fed` `c4_008_venelle` `c4_008_split` `c4_008_split_none` `c4_008_solo` `c4_008_name` `c4_008_spark` `c4_008_after` `c4_sv_gift` `c4_sv_reconciled` `c4_sv_fight` `c4_sv_repair` `c4_fight_end`
- **M18 물 펌프실** (1): `c4_pump_enter`
- **M19 폐품 시장** (11): `market_board` `c4_market_enter` `c4_lantern_after` `c4_lantern_ask` `c4_lantern_no` `c4_lantern_lit` `c4_scrap` `c4_scrap_again` `c4_lines_again` `c4_lines` `c4_lines_sit`
- **M20 유지보수 터널** (1): `c3_puddle_inspect`
- **M21 정비 작업장** (6): `c5_manual_again` `c5_manual` `c5_hair_v` `c5_comb_v` `c5_hair_solo` `c5_lamp_note`
- **M22 컨베이어 홀** (3): `c5_hall_enter` `c5_hall_off_v` `c5_hall_off_solo`
- **M23 수직 배선통로** (7): `c5_chase` `c5_acid_v` `c5_shaft_solo` `c5_arm_ok` `c5_shaft_clear` `c5_plate_v` `c5_shutter`
- **M24 공동 세탁실** (13): `c6_enter_v` `c6_enter` `c6_twins_v` `c6_clothes_ask` `c6_clothes` `c6_twins_again_v` `c6_twins_dead` `c6_twins_apart` `c6_twins_again` `c6_others` `c6_others_again` `c7_gate_look` `c7_gate_ask`
- **M25 숙소 3호** (25): `c6_room_enter_v` `c6_room_enter_dead` `c6_room_enter_apart` `c6_photo_v` `c6_photo_again_v` `c6_photo_dead` `c6_photo_apart` `c6_photo_again` `iv_1` `iv_2` `iv_3` `iv_4` `iv_5` `iv_mono` `c6_v_confused` `c6_room_talk_1` `c6_room_talk_2` `c6_room_talk_3` `c6_letter_v` `c6_letter_dead` `c6_letter_apart` `c6_wrappers_after_v` `c6_wrappers_v` `c6_wrappers_solo` `c6_wrappers_again`
- **M26 기숙사 옥상** (3): `c6_roof_v` `c6_roof_dead` `c6_roof_apart`
- **M27 수용동** (14): `c7_not_yet` `c7_back_sealed` `c7_gate_shut` `c7_holding_v` `c7_holding` `c7_stairs` `c7_stairs_after` `c7_release_panel` `c7_release_line` `c7_release_route_v` `c7_release_route` `c7_release_after` `c7_desk_card` `c7_desk_again`
- **M28 접수 홀** (10): `c7_meilin_lose` `c7_meilin_gone_v` `c7_meilin_gone` `c7_hall_locked` `c7_meilin_v` `c7_meilin_solo` `c7_meilin_start` `c7_ticket_before` `c7_ticket_again` `c7_ticket_again2`
- **M29 상담 부스** (8): `c7_need_card` `c7_booth_enter` `c7_chair` `c7_chair_again` `c7_meilin_memory` `c7_booth_lock_again` `c7_lostfound_card` `c7_lostfound_empty`
- **M30 영상실** (13): `c7_monitors_first` `c7_wardens` `c7_bulkhead_talk` `c7_venelle_last` `c7_wardens_solo` `c7_bulkhead_solo` `c7_monitors_again` `c7_monitors` `c7_feed_stairs` `c7_feed_venelle` `c7_lockdown` `c7_lockdown_v` `c7_lockdown_solo`
- **M31 관제탑** (9): `c7_bulkhead_shut` `c7_control_first` `c7_tower_v` `c7_tower` `c7_control_again` `c7_control` `c7_control_open` `c7_knock` `c7_knock_after`
- **M32 상층 회랑** (8): `c8_flee_end` `c8_fight_end` `c8_after_duel` `c8_not_yet` `c8_route_ask` `c8_reunion_1` `c8_reunion_2` `c8_transform`
- **M33 라비엘의 방** (15): `c8_room_wait` `c8_room_enter` `c8_photo_after` `c8_photo_after2` `c8_photo_alone` `c8_photo` `c8_past` `c8_confess` `c8_choice` `c8_choice_hand` `c8_choice_sit` `c8_raviel_mono` `c8_cups_alone` `c8_cups_after` `c8_cups`
- **M34 통신실** (3): `c8_console_solo` `c8_console` `c8_console_again`
- **M35 연결 교량** (6): `c8_bench_solo` `c8_bench_again` `c8_bench` `c8_bench_1` `c8_bench_2` `c8_bench_3`
- **M36 대기실** (12): `c9_wait_r` `c9_wait` `c9_plans` `c9_plans_again` `c9_chart` `c9_chart_again` `c9_member1` `c9_member1_again` `c9_member2` `c9_member2_again` `c9_member3` `c9_member3_again`
- **M38 녹음 보관실** (3): `c9_archive_enter` `c9_tape` `c9_tape_again`
- **M37 공명 공연장** (8): `c9_edward_1` `c9_edward_2` `c9_lac_lu` `c9_lac_persuaded` `c9_lac_subdued` `c9_hall_wait` `c9_lac_meet` `c9_lac_meet_r`
- **M39 서버실** (12): `c9_catch` `c9_server_wait` `c9_hall_gone` `c9_force_broadcast` `c9_force_r` `c9_force` `c9_comm_terminal` `c9_shelter_after` `c9_hall_after` `c9_lu_shock_r` `c9_lu_shock` `c9_capture`
- **M40 포획 단말** (3): `c9_terminal_in` `c9_incomplete` `c9_moved`
- **M41 휴지통 목록** (5): `c9_bin_talk` `c9_bin_exit` `c9_bin_restored` `c9_bin_released` `c9_bin_status`
- **M42 승선 대기실** (8): `c10_back_gone` `c10_boarding` `c10_board` `c10_board_again` `c10_member1` `c10_member1_again` `c10_member2` `c10_member2_again`
- **M43 캡슐 회랑** (3): `c10_meilin_first` `c10_capsule` `c10_meilin`
- **M44 전이 장치실** (9): `c10_aur_last` `c10_aur_dead` `c10_meilin_last` `c10_capsules_dead` `c10_not_done` `c10_aur_1` `c10_aur_reconnect` `c10_raviel_hurt` `c10_lu_cold`
- **M45 붕괴 연결로** (2): `c10_floor_breaks` `c10_hold`
- **M46 무너진 중정** (12): `c11_wake` `c11_breath` `c11_route_flee` `c11_route_fight` `c11_cup` `c11_hand_loose` `c11_no_answer` `c11_hand` `c11_name` `c11_eyes` `c11_stand` `c11_not_yet`
- **M47 잔해 더미** (8): `c11_recovery` `c11_last_lines` `c11_release_ask` `c11_mem_venelle` `c11_mem_008` `c11_mem_f` `c11_mem_clothes` `c11_mem_raviel`

## 7. 미확인 및 누락 자료 목록

### 7.1 씬에 연결되지 않은 사건 (12)

- `c1_rest_room_enter` — `Story/StoryEvents.cs`
- `c1_shortcut` — `Story/StoryEvents.cs`
- `c2_glass` — `Story/StoryEvents.cs`
- `c2s_pull_tut` — `Story/StoryEvents.cs`
- `c2s_tray_tut` — `Story/StoryEvents.cs`
- `c3_rest` — `Story/StoryEvents.cs`
- `demo_end` — `Story/StoryEvents.cs`
- `pc_exit` — `Story/StoryEvents.cs`
- `pc_log` — `Story/StoryEvents.cs`
- `pc_photo` — `Story/StoryEvents.cs`
- `pc_status` — `Story/StoryEvents.cs`
- `pc_trash` — `Story/StoryEvents.cs`

### 7.2 어디서도 참조되지 않는 대사 (53)

`c1_debris_inspect` `c1_door_off` `c1_emergency_light` `c1_escape_look` `c1_researcher_down` `c1_shutter_locked` `c1_terminal_door` `c1_trench_inspect` `c1_wire_first` `c1_wire_inspect` `c2_cargo_sign` `c2_window_again` `c2s_quiet` `c3_carry_start` `c3_carry_wrongway` `c3_join` `c3_support` `ch7_wip` `lab_m_gap_locked` `lab_wake` `op_h_1` `op_h_2` `op_h_3` `op_t_3` `op_t_4` `op_t_5` `op_t_calm2` `op_t_r2` `op_v_1` `op_v_3` `pc_guide_after` `sys_barrel` `sys_battle_tutorial` `v_after_join` `v_kneel_reach` `v_meet_1` `v_meet_2` `v_meet_lu` `v_steps` `v_talk1` `v_talk1_good` `v_talk1_silent` `v_talk1_threat` `v_talk2` `v_talk2_command` `v_talk2_good` `v_talk2_mock` `v_talk3` `v_talk3_cmd` `v_talk3_good` `v_talk_wait1` `v_talk_wait2` `w_first_3`

### 7.3 코드에 이름은 있으나 맵 사건 추적으로 닿지 않은 대사 (82)

말풍선·UI·보스·연출 코드에서 직접 쓰이는 대사를 포함한다 (정적 추적의 한계, 오류라는 뜻이 아님).

`broadcast_repeat` `c10_limit_1` `c10_limit_2` `c10_limit_3` `c10_limit_after_1` `c10_limit_after_2` `c10_limit_after_3` `c11_food_1` `c11_food_2` `c11_food_3` `c1_rest_room` `c1_shortcut_line` `c1_terminal_inspect` `c2_glass` `c2_line_tutorial` `c2s_pull_tut` `c2s_tray_tut` `c3_collapse_common` `c3_collapse_nego` `c3_collapse_sub` `c3_subdued` `c3_success` `c7_meilin_arm` `c7_meilin_armoff` `c7_mt_screen` `c9_lac_listen` `c9_lac_listen_again` `c9_lac_stop` `c9_lac_talk_ask` `demo_end` `hall_door_note` `hall_motor` `hall_pc1` `hall_pc2` `hall_restored` `pc_photo_m` `pc_status` `pc_trash_m` `pump_coupling_live` `pump_coupling_wet` `pump_need_part` `pump_need_part_seen` `pump_part_done` `pump_part_ok` `pump_power_off` `pump_rattle` `pump_running` `pump_running_again` `pump_shelf` `pump_shelf_empty` `pump_trip` `pump_valve_close` `pump_valve_close_mid` `pump_valve_open` `pump_valve_open_empty` `sys_line_broken` `sys_line_fixed` `v_down_1` `v_down_ask` `v_grab_again` `v_grab_first` `v_kill` `v_kill_after` `v_r_hands_again` `v_r_hands_bad` `v_r_hands_ok` `v_r_leave_again` `v_r_leave_first` `v_r_provoke_again` `v_r_provoke_first` `v_r_release_again` `v_r_release_bad` `v_r_release_ok` `v_r_why_again` `v_r_why_first` `v_release` `v_spare` `v_stop` `v_talk_open` `w_first_1` `w_first_2` `w_first_4`

### 7.4 없는 자료

- 문서 D02, D04~D09 (GDD 16종 중)
- GDD 맵 3개 (48 → 45), 예: 2장 기록 창고★
- 현재 빌드 기준 화면 캡처가 없는 맵: C04, C05, M31, M34, M38, M43 (DOCX 에 [실제 화면 캡처 필요] 자리표시)
- 컷 단위 시간·사운드 큐 데이터 (스토리보드 시간·사운드 칸 [미확인])
- 사람 플레이 기록 (손맛·템포·난이도·전달력 전부 [미확인])
- 보스·캐릭터 외형 레퍼런스 (DOCX 에 [보스 패턴 레퍼런스 필요])

