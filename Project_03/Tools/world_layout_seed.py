# -*- coding: utf-8 -*-
"""
배치 데이터 첫 생성기 (2026-11-15). Assets/Game/Resources/EW/world_layout.json 을 만든다.
이후로는 JSON 이 원본이다 — 도면 반영 때는 JSON 을 직접 고치거나 이 파일을 고쳐 다시 만든다 (Docs/BUILDING_LAYOUT_GUIDE.md).
주의: 다시 실행하면 JSON 을 통째로 덮어쓴다.
"""
import json, io, os
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'Assets', 'Game', 'Resources', 'EW', 'world_layout.json')
M = []


def m(id, scene, name, ch, zone, floor, goal, exits=(), nxt=(), spawns=('start',), req=(), opt=False, space=(), safe='start'):
    M.append(dict(id=id, scene=scene, name=name, chapter=ch, zone=zone, floor=floor, goal=goal, optional=opt, safeSpawn=safe,
                  exits=[dict(id=e[0], to=e[1], spawn=e[2], kind=e[3] if len(e) > 3 else 'edge', require=e[4] if len(e) > 4 else '',
                              block=e[5] if len(e) > 5 else '', blocked=e[6] if len(e) > 6 else '', note='') for e in exits],
                  next=[dict(map=n[0], label=n[1], cond=n[2] if len(n) > 2 else '') for n in nxt], spawns=list(spawns), requires=list(req),
                  spaceLines=list(space), anchors=[]))


# ---- 기존 1~6장 (출구는 EWRooms.cs / EWRoomsUnderground.cs 코드에 있다. 여기는 검사·검수용 거울 — 함께 고친다)
m('P01', 'P01_BurningVillage', '불타는 마을', 'prologue', '0 집과 마을', '지상', '연출 장면', nxt=[('P02', '무너진 작은 집', '연출')])
m('P02', 'P02_RuinedHouse', '무너진 작은 집', 'prologue', '0 집과 마을', '지상', '연출 장면', nxt=[('P03', '컴퓨터 안', '연출')])
m('P03', 'P03_FriendDesktop', '컴퓨터 안', 'ch1', '1 포획동', '단말', '안내서·연습·접속 기록을 살펴본다', nxt=[('C01', '실험실', '접속 기록 조사 후 연출')])
m('C01', 'C01_TankLab', '실험실', 'ch1', '1 포획동', 'B1', '실험실을 빠져나간다', nxt=[('C02', '격리 실험동 복도')], spawns=('start', 'tank'))
m('C02', 'C02_EscapeWing', '격리 실험동 복도', 'ch1', '1 포획동', 'B1', '복도 끝으로', exits=[('C02.east', 'C03', 'start')], nxt=[('C03', '설비실·자료실')], spawns=('start', 'from_lab', 'from_c03'))
m('C03', 'C03_SpecimenArchive', '설비실 · 자료실', 'ch2', '2 연구동', 'B1', '자료실을 지나 화물 적재장으로',
  exits=[('C03.west', 'C02', 'from_c03'), ('C03.east', 'C04', 'start'), ('C03.cafe', 'M09', 'from_archive', 'door')], nxt=[('C04', '화물 적재장'), ('M09', '직원 식당 ★', '선택')], spawns=('start', 'from_cafe', 'from_cargo'))
m('M09', 'M09_StaffCafeteria', '직원 식당', 'ch2', '2 연구동', 'B1', '선택 탐색', exits=[('M09.door', 'C03', 'from_cafe', 'door')], opt=True, spawns=('from_archive',), safe='from_archive')
m('C04', 'C04_CargoBay', '화물 적재장', 'ch2', '2 연구동', 'B1', '적재장을 지나 배양실로', exits=[('C04.west', 'C03', 'from_cargo'), ('C04.east', 'C05', 'start')], nxt=[('C05', '배양실')], spawns=('start', 'from_lab'))
m('C05', 'C05_CultureRoom', '배양실', 'ch3', '3 약품동', 'B1', '연결 복도로',
  exits=[('C05.west', 'C04', 'from_lab'), ('C05.east', 'C06', 'start'), ('C05.door', 'M14', 'from_lab', 'door')], nxt=[('C06', '연결 복도'), ('M14', '약품 온실 ★', '선택')], spawns=('start', 'from_mix', 'from_greenhouse'))
m('M14', 'M14_ChemGreenhouse', '약품 온실', 'ch3', '3 약품동', 'B1', '선택 탐색', exits=[('M14.door', 'C05', 'from_greenhouse', 'door')], opt=True, spawns=('from_lab',), safe='from_lab')
m('C06', 'C06_LongCorridor', '연결 복도', 'ch3', '3 약품동', 'B1', '베넬',
  exits=[('C06.west', 'C05', 'from_mix', 'edge', '', 'c6_combat_locked', 'sys_locked'), ('C06.east', 'M13', 'start', 'edge', 'c3_boss_done', '', 'sys_locked')], nxt=[('M13', '혼합실', '베넬 만남 뒤')], spawns=('start', 'from_mix'))
m('M13', 'M13_MixingRoom', '혼합실', 'ch3', '3 약품동', 'B1', '관찰 휴게실로',
  exits=[('M13.west', 'C06', 'from_mix', 'edge', '', 'venelle_carried', 'sys_locked'), ('M13.east', 'M15', 'start')], nxt=[('M15', '관찰 휴게실')], req=('c3_boss_done',), spawns=('start', 'from_rest'))
m('M15', 'M15_ObservationLounge', '관찰 휴게실', 'ch3', '3 약품동', 'B1', '오른쪽 화물 승강장으로',
  exits=[('M15.west', 'M13', 'from_rest'), ('M15.east', 'M16', 'start', 'edge', 'c3_boss_done', '', 'sys_locked')], nxt=[('M16', '화물 승강장')], req=('c3_boss_done',), spawns=('start', 'from_lift'))
m('M16', 'M16_CargoLift', '화물 승강장', 'ch4', '4 지하', 'B1→B2', '승강기로 지하로 내려간다', exits=[('M16.west', 'M15', 'from_lift'), ('M16.east', 'M17', 'start')], nxt=[('M17', '폐기소 거처')],
  spawns=('start', 'lift_top', 'lift_bottom', 'from_shelter'), space=('c4_platform_sign', 'c4_seal_look', 'c4_lift_up_sealed', 'c4_lift_ride', 'c4_lift_ride_v'))
m('M17', 'M17_WasteShelter', '폐기소 거처', 'ch4', '4 지하', 'B2', 'F 와 교섭한다 (과자 10개 / 펌프 수리 / 충돌)',
  exits=[('M17.west', 'M16', 'from_shelter'), ('M17.east', 'M20', 'start', 'edge', 'waste_resolved', '', 'waste_exit_wait'),
         ('M17.pump', 'M18', 'start', 'door', 'waste_met', '', 'waste_door_wait'), ('M17.market', 'M19', 'start', 'door', 'waste_met', '', 'waste_door_wait')],
  nxt=[('M20', '유지보수 터널', '교섭 해결'), ('M18', '물 펌프실', '만난 뒤'), ('M19', '폐품 시장 ★', '선택')], spawns=('start', 'from_pump', 'from_market', 'from_tunnel', 'meet'), space=('c4_meet_2', 'c4_hatch', 'c4_deal_pump'))
m('M18', 'M18_PumpRoom', '물 펌프실', 'ch4', '4 지하', 'B2', '배수 → 차단기 → 이음쇠 → 가동', exits=[('M18.door', 'M17', 'from_pump', 'door')], req=('waste_met',))
m('M19', 'M19_JunkMarket', '폐품 시장', 'ch4', '4 지하', 'B2', '선택 탐색', exits=[('M19.door', 'M17', 'from_market', 'door')], req=('waste_met',), opt=True)
m('M20', 'M20_MaintTunnel', '유지보수 터널', 'ch5', '5 정비동', 'B2', '낮은 천장은 웅크려서', exits=[('M20.west', 'M17', 'from_tunnel'), ('M20.east', 'M21', 'start')], nxt=[('M21', '정비 작업장')], req=('waste_resolved',), spawns=('start', 'from_workshop'))
m('M21', 'M21_Workshop', '정비 작업장', 'ch5', '5 정비동', 'B2', '새 기술 시험', exits=[('M21.west', 'M20', 'from_workshop'), ('M21.east', 'M22', 'start')], nxt=[('M22', '컨베이어 홀')], spawns=('start', 'from_hall'))
m('M22', 'M22_ConveyorHall', '컨베이어 홀', 'ch5', '5 정비동', 'B2', '출구 문 — 전원 복구 또는 환기 통로', exits=[('M22.west', 'M21', 'from_hall'), ('M22.east', 'M23', 'start')], nxt=[('M23', '수직 배선통로')], spawns=('start', 'from_shaft'))
m('M23', 'M23_VerticalShaft', '수직 배선통로', 'ch5', '5 정비동', 'B2→1F', '세 층을 올라 따돌린다', exits=[('M23.west', 'M22', 'from_shaft'), ('M23.top', 'M24', 'start')], nxt=[('M24', '공동 세탁실')], spawns=('start', 'from_dorm'))
m('M24', 'M24_Laundry', '공동 세탁실', 'ch6', '6 숙소', '1F', '숙소 복도 끝 수용동 연결문',
  exits=[('M24.west', 'M23', 'from_dorm'), ('M24.roof', 'M26', 'start', 'door'), ('M24.room', 'M25', 'start', 'door')],
  nxt=[('M27', '수용동', '연결문 사건 — 들어가면 돌아오지 못함'), ('M25', '숙소 3호'), ('M26', '기숙사 옥상 ★', '선택')], spawns=('start', 'from_roof', 'from_room'), space=('c7_gate_look',))
m('M25', 'M25_VenelleRoom', '숙소 3호', 'ch6', '6 숙소', '1F', '사진·편지·포장지', exits=[('M25.door', 'M24', 'from_room', 'door')])
m('M26', 'M26_DormRoof', '기숙사 옥상', 'ch6', '6 숙소', '옥상', '선택: 바람', exits=[('M26.door', 'M24', 'from_roof', 'door')], opt=True)
# ---- 7~11장 (출구는 EWRoomsLate.cs 가 이 데이터에서 읽는다: LExit / LDoor)
m('M27', 'M27_HoldingBlock', '수용동', 'ch7', '7 수용·접수동', '1F', '수용칸 제어반으로 사람들을 풀어 준다',
  exits=[('M27.west', 'M24', 'from_room', 'edge', '', 'ch7_entered', 'c7_back_sealed'), ('M27.east', 'M28', 'start', 'edge', 'holding_released', '', 'c7_not_yet')],
  nxt=[('M28', '접수 홀', '수용동 해방 뒤')], spawns=('start', 'from_hall'), req=('ch7_entered',), space=('c7_release_route', 'c7_release_route_v', 'c7_stairs', 'c7_gate_shut'))
m('M28', 'M28_ReceptionHall', '접수 홀', 'ch7', '7 수용·접수동', '1F', '메이린 — 포획팔·유지보수 단말·셔터·제동 장치·젖은 바닥',
  exits=[('M28.west', 'M27', 'from_hall'), ('M28.booth', 'M29', 'start', 'door', 'c7_meilin_done', '', 'c7_hall_locked')],
  nxt=[('M29', '상담 부스', '메이린 뒤')], spawns=('start', 'from_booth'), req=('holding_released',))
m('M29', 'M29_CounselBooth', '상담 부스', 'ch7', '7 수용·접수동', '1F', '영상실은 직원 카드로 (책상 / 분실물 상자)',
  exits=[('M29.hall', 'M28', 'from_booth', 'door'), ('M29.video', 'M30', 'start', 'door', 'has_staff_card', '', 'c7_need_card')],
  nxt=[('M30', '영상실', '직원 카드')], spawns=('start', 'from_video'), req=('c7_meilin_done',), space=('c7_booth_enter',))
m('M30', 'M30_VideoRoom', '영상실', 'ch7', '7 수용·접수동', '1F', '감시 모니터 → 관제탑 쪽 격벽',
  exits=[('M30.booth', 'M29', 'from_video', 'door'), ('M30.east', 'M31', 'start', 'edge', 'c7_feeds_seen', '', 'c7_monitors_first')],
  nxt=[('M31', '관제탑', '모니터 확인 → 격벽 사건')], spawns=('start', 'from_tower'), req=('has_staff_card',), space=('c7_lockdown', 'c7_lockdown_v', 'c7_bulkhead_talk', 'c7_feed_venelle', 'c7_feed_stairs'))
m('M31', 'M31_ControlTower', '관제탑', 'ch7', '7 수용·접수동', '2F', '중앙 통제 단말로 위층 통로를 연다',
  exits=[('M31.west', 'M30', 'from_tower', 'edge', '', 'c7_bulkhead_closed', 'c7_bulkhead_shut'), ('M31.east', 'M32', 'start', 'edge', 'c7_control_done', '', 'c7_control_first')],
  nxt=[('M32', '상층 회랑', '통로 개방 뒤')], spawns=('start', 'from_corridor'), req=('c7_bulkhead_closed',), space=('c7_control_open', 'c7_knock'))
m('M32', 'M32_UpperCorridor', '상층 회랑', 'ch8', '8 연구주임 구역', '3F', '라비엘',
  exits=[('M32.west', 'M31', 'from_corridor'), ('M32.east', 'M33', 'start', 'edge', 'c8_duel_done', '', 'c8_not_yet')],
  nxt=[('M33', '라비엘의 방', '재회 뒤')], spawns=('start', 'from_room'), req=('c7_control_done',))
m('M33', 'M33_RavielRoom', '라비엘의 방', 'ch8', '8 연구주임 구역', '3F', '액자 두 개',
  exits=[('M33.west', 'M32', 'from_room'), ('M33.east', 'M34', 'start', 'edge', 'c8_room_done', '', 'c8_room_wait')],
  nxt=[('M34', '통신실', '대화 뒤')], spawns=('start', 'from_comm'), req=('c8_duel_done',))
m('M34', 'M34_CommRoom', '통신실', 'ch8', '8 연구주임 구역', '3F', '통신실을 지나 교량으로', exits=[('M34.west', 'M33', 'from_comm'), ('M34.east', 'M35', 'start')], nxt=[('M35', '연결 교량')], spawns=('start', 'from_bridge'), req=('c8_room_done',))
m('M35', 'M35_SkyBridge', '연결 교량', 'ch8', '8 연구주임 구역', '3F 교량', '공연·서버동으로', exits=[('M35.west', 'M34', 'from_bridge'), ('M35.east', 'M36', 'start')], nxt=[('M36', '대기실')], spawns=('start', 'from_wait'), req=('c8_room_done',))
m('M36', 'M36_WaitingRoom', '대기실', 'ch9', '9 공연·서버동', '3F', '공연장으로',
  exits=[('M36.west', 'M35', 'from_wait'), ('M36.east', 'M37', 'start'), ('M36.archive', 'M38', 'start', 'door')],
  nxt=[('M37', '공명 공연장'), ('M38', '녹음 보관실 ★', '선택')], spawns=('start', 'from_hall', 'from_archive'), req=('c8_room_done',))
m('M37', 'M37_ResonanceHall', '공명 공연장', 'ch9', '9 공연·서버동', '3F', '라크리모사 — 믹서(마이크 방향·스피커·게인)',
  exits=[('M37.west', 'M36', 'from_hall'), ('M37.east', 'M39', 'start', 'edge', 'c9_lac_done', '', 'c9_hall_wait')], nxt=[('M39', '서버실', '공연 뒤')], spawns=('start', 'from_server'))
m('M38', 'M38_RecordArchive', '녹음 보관실', 'ch9', '9 공연·서버동', '3F', '선택: 녹음 목록과 테이프', exits=[('M38.wait', 'M36', 'from_archive', 'door')], opt=True)
m('M39', 'M39_ServerRoom', '서버실', 'ch9', '9 공연·서버동', '3F', '포획 단말 / 승선 대기실',
  exits=[('M39.west', 'M37', 'from_server', 'edge', '', 'c9_hall_collapsed', 'c9_hall_gone'), ('M39.east', 'M42', 'start', 'edge', 'c9_bin_done', '', 'c9_server_wait')],
  nxt=[('M42', '승선 대기실', '휴지통 복원 뒤'), ('M40', '포획 단말', '강제 가동 뒤 사건')], spawns=('start', 'from_terminal', 'from_boarding'), req=('c9_lac_done',),
  space=('c9_force_broadcast', 'c9_lu_shock', 'c9_lu_shock_r', 'c9_comm_terminal', 'c9_shelter_after'))
m('M40', 'M40_CaptureTerminal', '포획 단말', 'ch9', '9 공연·서버동', '단말', '정비도 파일', nxt=[('M41', '휴지통 목록', '불완전 판정 뒤 사건')], req=('shelter_collapsed',))
m('M41', 'M41_RecycleList', '휴지통 목록', 'ch9', '9 공연·서버동', '단말', '항목 목록 → 정렬 → 선택 → 복원 → 구속 해제 → 내부 확인', nxt=[('M39', '서버실 (방출 접점)', '복원 뒤 사건')], req=('c9_judged',))
m('M42', 'M42_BoardingLounge', '승선 대기실', 'ch10', '10 전이동', '4F', '캡슐 회랑으로',
  exits=[('M42.west', 'M39', 'from_boarding', 'edge', '', 'c10_boarding_seen', 'c10_back_gone'), ('M42.east', 'M43', 'start')], nxt=[('M43', '캡슐 회랑')], spawns=('start', 'from_capsule'), req=('c9_bin_done',))
m('M43', 'M43_CapsuleCorridor', '캡슐 회랑', 'ch10', '10 전이동', '4F', '전이 장치실로',
  exits=[('M43.west', 'M42', 'from_capsule'), ('M43.east', 'M44', 'start', 'edge', 'c10_meilin_seen', '', 'c10_meilin_first')], nxt=[('M44', '전이 장치실', '메이린 장면 뒤')], spawns=('start', 'from_device'))
m('M44', 'M44_TransferChamber', '전이 장치실', 'ch10', '10 전이동', '4F', '아우렐리우스 — 결합부에서 한계 방출',
  exits=[('M44.east', 'M45', 'start', 'edge', 'c10_boss_done', '', 'c10_not_done')], nxt=[('M45', '붕괴 연결로', '최종전 뒤')], spawns=('start', 'from_corridor'), req=('c10_meilin_seen',))
m('M45', 'M45_CollapseWay', '붕괴 연결로', 'ch10', '10 전이동', '4F→중정', '무너지는 길 끝까지', nxt=[('M46', '무너진 중정', '추락 사건')], req=('c10_boss_done',), space=('c10_floor_breaks',))
m('M46', 'M46_CollapsedCourt', '무너진 중정', 'ch11', '11 잔해', '중정', '', exits=[('M46.east', 'M47', 'start', 'edge', 'c11_stood', '', 'c11_not_yet')], nxt=[('M47', '잔해 더미', '일어선 뒤')], req=('c10_fall_done',))
m('M47', 'M47_RubblePile', '잔해 더미', 'ch11', '11 잔해', '중정', '', req=('c11_stood',))

links = [{'from': a, 'to': b, 'spawn': s, 'by': y} for a, b, s, y in [
    ('P01', 'P02', 'start', 'op_village 연출'), ('P02', 'P03', 'start', 'op_house 연출'), ('P03', 'C01', 'tank', 'pc_exit 사건'), ('C01', 'C02', 'from_lab', '실험실 문틈 사건'),
    ('M24', 'M27', 'start', 'ch7_gate 사건 (StoryEvents.Ch7Enter)'), ('M30', 'M31', 'start', 'c7_bulkhead 사건'),
    ('M39', 'M40', 'start', 'c9_capture 사건'), ('M40', 'M41', 'start', 'c9_d16 사건 (불완전 판정)'), ('M41', 'M39', 'from_terminal', 'c9_bin_list 사건 (복원·구속 해제)'),
    ('M45', 'M46', 'start', 'c10_fall 사건')]]
remap = [dict(map='M00', to='P01', spawn='start'), dict(map='M04', to='C01', spawn='tank'), dict(map='M05', to='C02', spawn='from_lab'), dict(map='M12', to='C05', spawn='start')]
chsafe = [dict(chapter=c, map=mp, spawn=sp) for c, mp, sp in [('prologue', 'P01', 'start'), ('ch1', 'C01', 'tank'), ('ch2', 'C03', 'start'), ('ch3', 'C05', 'start'), ('ch4', 'M16', 'start'),
          ('ch5', 'M20', 'start'), ('ch6', 'M24', 'start'), ('ch7', 'M27', 'start'), ('ch8', 'M32', 'start'), ('ch9', 'M36', 'start'), ('ch10', 'M42', 'start'), ('ch11', 'M46', 'start'), ('end', 'M47', 'start')]]
BASE3 = ['op_village_done', 'op_house_done', 'op_desktop_done', 'op_desktop_land', 'pc_guide_read', 'op_r_in', 'c1_tank_broken', 'c1_researchers_down', 'op_tank_done',
         'lab_woke', 'lab_realized', 'c1_left_lab', 'c1_escape_view', 'c1_shortcut_open', 'ev_c2_start', 'ev_c3_start', 'corridor_memory']
EV = []


def ev(id, label, ch, map, spawn='start', on=(), off=(), before='', chap='', ven='keep', rav='keep', skills=(), items=(), sets=()):
    EV.append(dict(id=id, label=label, chapter=ch, map=map, spawn=spawn, flagsOn=list(on), flagsOff=list(off), before=before, chapterId=chap,
                   venelle=ven, raviel=rav, skills=list(skills), items=list(items), sets=list(sets), note=''))


V3 = 'choose:ally,apart,dead'
V8 = 'choose:ally_bulkhead,apart,dead'
ev('pre_venelle', '베넬 전투 전', 'ch3', 'C06', on=BASE3, off=('c3_boss_intro', 'c3_boss_done', 'c3_negotiated', 'c3_joined', 'c3_rest_done'), chap='ch3', ven='set:pending', skills=('pull',))
ev('post_venelle', '베넬 전투 후', 'ch3', 'M13', on=('c3_boss_intro', 'c3_boss_done', 'c3_negotiated', 'venelle_spared'), before='pre_venelle', chap='ch3', ven=V3)
ev('ch4_lift', '승강장 (4장 시작)', 'ch4', 'M16', on=('seen_m15_lift_sign', 'demo_finished', 'c3_rest_done'), before='post_venelle', chap='ch4', ven=V3, items=('snack:13',))
ev('ch4_meet', 'F·008 첫 만남', 'ch4', 'M17', on=('ev_c4_start', 'm16_lift_down', 'c4_lift_first'), off=('waste_met',), before='ch4_lift', chap='ch4', ven=V3)
ev('ch4_pump_before', '펌프 해결 전', 'ch4', 'M18', on=('waste_met', 'waste_pump_promised'), before='ch4_meet', chap='ch4', ven=V3)
ev('ch4_pump_after', '펌프 해결 후', 'ch4', 'M17', spawn='from_pump', on=('pump_valve_open', 'pump_power_on', 'pump_part_fixed', 'waste_pump_fixed', 'waste_resolved', 'waste_water_on'),
   before='ch4_pump_before', chap='ch4', ven=V3, sets=('wasteResult=repair',))
ev('ch6_gate', '수용동 연결문 앞 (6장 끝)', 'ch6', 'M24', on=('ev_c5_start', 'pulse_learned', 'used_burst', 'ws_tested', 'ev_c6_start', 'm23_top_said'), before='ch4_pump_after', chap='ch6', ven=V3, skills=('pulse',))
ev('ch7_release_before', '수용동 해방 전', 'ch7', 'M27', on=('ch7_ready', 'ch7_entered'), off=('holding_released', 'holding_open'), before='ch6_gate', chap='ch7', ven=V3)
ev('ch7_release_after', '수용동 해방 후', 'ch7', 'M27', on=('holding_open', 'holding_released', 'c7_enter_seen', 'has_staff_card', 'got_staff_card_desk'), before='ch7_release_before', chap='ch7', ven=V3,
   items=('staff_card:1',), sets=('shelter+holding_released',))
ev('ch7_meilin', '메이린 전투', 'ch7', 'M28', off=('c7_meilin_done', 'c7_meilin_met', 'hide_meilin'), before='ch7_release_after', chap='ch7', ven=V3)
ev('ch7_bulkhead', '베넬 격벽 (영상실)', 'ch7', 'M30', on=('c7_meilin_met', 'c7_meilin_done', 'hide_meilin'), off=('c7_feeds_seen', 'c7_bulkhead_closed'), before='ch7_meilin', chap='ch7', ven=V3)
ev('ch8_reunion', '라비엘 재회', 'ch8', 'M32', on=('c7_feeds_seen', 'c7_lockdown', 'c7_bulkhead_closed', 'c7_control_done', 'c7_tower_seen'), off=('c8_met', 'c8_route_chosen', 'c8_duel_done', 'hide_raviel', 'show_ravielBeast'),
   before='ch7_bulkhead', chap='ch8', ven=V8)
ev('ch8_room', '라비엘의 방 (고백)', 'ch8', 'M33', on=('c8_met', 'c8_route_chosen', 'c8_duel_done'), off=('c8_room_done', 'hide_raviel'), before='ch8_reunion', chap='ch8', ven=V8, sets=('friendRoute=flee',))
ev('ch9_lacrimosa', '라크리모사 전투', 'ch9', 'M37', on=('c8_room_done', 'ev_ch9_start'), off=('c9_lac_done', 'c9_lac_met', 'hide_lacrimosa'), before='ch8_room', chap='ch9', ven=V8, rav='ally', sets=('reunionChoice=hand',))
ev('ch9_collapse', '대피실 붕괴 (서버실)', 'ch9', 'M39', on=('c9_lac_met', 'c9_lac_done'), off=('shelter_collapsed',), before='ch9_lacrimosa', chap='ch9', ven=V8, rav='ally', sets=('lacrimosaResult=subdued',))
ev('ch9_bin', '휴지통 복원', 'ch9', 'M41', on=('shelter_collapsed', 'c9_hall_collapsed', 'lacrimosa_dead', 'c9_captured', 'c9_terminal_seen', 'c9_judged', 'c9_moved_to_bin', 'c9_in_bin'), off=('c9_bin_done',),
   before='ch9_collapse', chap='ch9', ven=V8, rav='ally')
ev('ch10_phase1', '최종전 1단계', 'ch10', 'M44', on=('c9_bin_done', 'c9_caught', 'c9_restored', 'c9_released', 'c10_capsule_failed', 'c10_meilin_seen', 'c10_boarding_seen', 'ev_ch10_start'),
   off=('c10_confront_done', 'c10_boss_done'), before='ch9_bin', chap='ch10', ven=V8, rav='ally')
ev('ch10_phase2', '최종전 2단계', 'ch10', 'M44', on=('c10_confront_done', 'aur_phase_1_done', 'limit_release_1', 'show_raviel_inj'), before='ch10_phase1', chap='ch10', ven=V8, rav='injured', sets=('limitReleases=1',))
ev('ch10_phase3', '최종전 3단계', 'ch10', 'M44', on=('aur_phase_2_done', 'limit_release_2'), before='ch10_phase2', chap='ch10', ven=V8, rav='injured', sets=('limitReleases=2',))
ev('ch11_talk', '마지막 대화 (무너진 중정)', 'ch11', 'M46', on=('aur_phase_3_done', 'limit_release_3', 'c10_boss_done', 'aurelius_dead', 'meilin_dead', 'capsules_dead', 'transfer_zero', 'c10_fall_done'),
   off=('c11_talk_done', 'c11_stood', 'raviel_dead'), before='ch10_phase3', chap='ch11', ven=V8, rav='injured', sets=('limitReleases=3',))
ev('ch11_ending', '마지막 방출과 엔딩', 'ch11', 'M47', on=('c11_talk_done', 'raviel_dead', 'c11_stood', 'c11_hand'), before='ch11_talk', chap='ch11', ven=V8, rav='dead')

data = dict(version=1, note='건물 배치 데이터 — Docs/BUILDING_LAYOUT_GUIDE.md. 현재는 GDD 기준 임시 배치.', maps=M, storyLinks=links, remap=remap, chapterSafe=chsafe, events=EV)
with io.open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    json.dump(data, f, ensure_ascii=False, indent=1)
print('maps', len(M), 'events', len(EV))
