# -*- coding: utf-8 -*-
"""
4~6장 대사·문서를 Assets/Game/Data/Dialogue/dialogue_ko.json 에 넣는다 (같은 id 는 이 스크립트 내용으로 갱신, 다른 항목은 그대로).
src 표기: U = 사용자 지시 원문 그대로 / G = GDD 원문 그대로 / A = 이번에 보충한 연결 대사 (줄마다 added: true).
실행: python Tools/ch4_6_dialogue.py
"""
import io, json, os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PATH = os.path.join(ROOT, 'Assets', 'Game', 'Data', 'Dialogue', 'dialogue_ko.json')
U = '사용자 지시 2026-11-15 (승강장 이후 4~6장)'
G = 'GDD 4~6장·7.3·7.5·7.6'
A = '추가 (4~6장 연결, 2026-11-15)'

SPEAKERS = [('f', 'F', 'R'), ('kid008', '008', 'R'), ('survivor', '생존자', 'R'), ('ivy', '아이비', 'R'), ('trader', '등불 장수', 'R'), ('scrapper', '고물 장수', 'R')]
CHAPTERS = [('ch4', '4장 — 여덟 번째 꼬마와 열 개의 과자'), ('ch5', '5장 — 걸어도 되는 곳으로 가자'), ('ch6', '6장 — 옷 두 벌'), ('ch7', '7장 — 접수는 취소가 안 된대')]

E = []


def d(id, src, lines, choices=None):
    E.append((id, src, lines, choices))


# 줄: (화자, 대사, 표정) — 표정 생략 가능. 보충 줄은 src=A 인 항목 전체에 added 를 붙인다. 혼합 항목은 4번째 값 True 로 표시.
# ---------------------------------------------------------------- 16 화물 승강장
d('c4_platform_enter', A, [('lu', '여기가 승강장... 표지판 따라 잘 왔다!')])
d('c4_platform_sign', A, [('narr', "'화물 승강장 — 상층 운행 중지. 지하 폐기 구역 전용.'")])
d('c4_seal_look', A, [('narr', '승강로 위쪽이 두꺼운 셔터로 막혀 있다. 붉은 경고등이 천천히 돈다.'), ('lu', '위는 완전히 막혔네... 그럼 아래부터.', 'neutral')])
d('c4_lift_panel', A, [('narr', '승강기 조작반. 버튼 셋 중 하나에만 불이 들어와 있다.')])
d('c4_lift_up_sealed', A, [('narr', "'상층' 버튼을 눌러도 반응이 없다. 화면에 '봉쇄 — 해제 권한 없음'."), ('lu', '...위로는 못 가. 출구가 위에 있으면 어떡하지.', 'sad'), ('lu', '아니! 아래에도 길은 있겠지!', 'determined')])
d('c4_lift_up_again', A, [('narr', "'봉쇄 — 해제 권한 없음'.")])
d('c4_lift_ride', A, [('lu', '내려간다... 위가 막혔으니까, 아래로!', 'determined')])
d('c4_lift_ride_v', A, [('lu', '내려간다... 위가 막혔으니까, 아래로!', 'determined'), ('venelle', '아래는 냄새 나는데~', 'blank'), ('lu', '냄새가 나도 길이면 가는 거야!', 'smile')])

# ---------------------------------------------------------------- 17 폐기소 거처
d('c4_meet_1', U, [('lu', '잠깐! 먹을 거면 나 말고 이거! 나 맛없어! 찌릿해!', 'scared'), ('f', '누가 널 먹는대. 손에 든 거 내놓으란 거지.', 'determined'),
                   ('lu', '아. 그럼 얘기가 빠르네!', 'surprised'), ('kid008', '얼마나 찌릿해?', 'surprised'), ('lu', '그건 먹어보고 확인하는 질문 아니야!', 'scared')])
d('c4_meet_2', A, [('f', '위로 가는 승강기는 막혔다. 여긴 거기서 밀려 내려온 사람들이야.', 'neutral'), ('f', '물이 끊긴 지 사흘. 펌프가 멎었어.', 'neutral'),
                   ('f', '먹을 거든 고칠 손이든. 하나는 내놓고 지나가.', 'determined')])
d('c4_deal', A, [('lu', '음... 그럼—', 'neutral')])
d('c4_deal_again', A, [('f', '정했냐.', 'neutral')])
d('c4_deal_short', A, [('narr', '과자가 모자란다.')])
d('c4_deal_pump', A, [('lu', '펌프! 내가 고쳐 볼게. 전기는 내가 좀 알거든!', 'determined'), ('f', '말로 고치는 건 누구나 해. 고치고 와.', 'neutral'), ('f', '펌프실은 저 문. 물이 차 있으니까 조심하고.', 'neutral')])
d('c4_deal_pump_again', A, [('f', '아직이냐. 물 소리가 안 들린다.', 'neutral')])
d('c4_deal_fail', A, [('lu', '못 줘! 나도 가야 할 데가 있단 말이야. 그냥 지나갈게!', 'determined'), ('f', '...그럼 우리도 못 비켜.', 'determined'),
                      ('survivor', '할머니 뒤로 가! 저거 전기 쓰는 애야!', 'scared'), ('narr', '파이프를 쥔 손들이 떨린다. 008 이 상자 뒤로 숨는다.')])
d('c4_gift', A, [('narr', '루가 과자를 하나씩 꺼내 놓는다. 열 개.'), ('f', '...진짜로 내놓을 줄은 몰랐다.', 'neutral'),
                 ('f', '하나씩 받아. 나눠. 애들 먼저.', 'gentle'), ('survivor', '...고마워.', 'neutral'), ('lu', '다들 배고팠구나. 다행이다!', 'smile')])
d('c4_fight_end', A, [('f', '그만! 다들 그만.', 'determined'), ('narr', '파이프가 바닥에 떨어진다. 사람들이 주저앉아 숨을 고른다.'),
                      ('lu', '...미안. 세게 안 했어. 찌릿만.', 'sad'), ('f', '...힘은 세네. 그 힘, 펌프에나 써 봐.', 'neutral'), ('f', '지나가도 돼. 길은 저쪽이다.', 'neutral')])
d('c4_f_fight', A, [('f', '...다들 겁먹었어. 손 내려.', 'determined')])
d('c4_f_name', U, [('narr', 'F 가 목에 건 번호표를 들여다본다.'), ('f', '이거 말고 이름이 있었는데, 오래돼서.', 'neutral'), ('lu', '그럼 오늘은 F 할머니! 내일 생각나면 다시 알려줘!', 'smile'),
                   ('f', '내일도 올 생각이냐.', 'neutral'), ('lu', '과자 주면!', 'smile')])
d('c4_f_box_gift', A, [('lu', '아야! ...아까 그 과자 상자잖아. 비었는데!', 'surprised'), ('f', '그러니까 던졌지.', 'gentle')])
d('c4_f_box', A, [('lu', '아야! ...빈 상자? 이건 왜!', 'surprised'), ('f', '들고 다니라고. 다음엔 채워 와.', 'gentle')])
d('c4_f_pump_hint', A, [('f', '물은 아직이다. 펌프실 문은 열어 뒀어.', 'neutral')])
d('c4_f_again', G, [('f', '또 왔냐.', 'neutral'), ('lu', '반가운 얼굴이라는 뜻이지?', 'smile'), ('f', '손 씻고 와.', 'gentle')])
d('c4_hatch', A, [('narr', "녹슨 해치에 손글씨: '대피실 — 자리 남음'."), ('f', '위가 막혀서 여기 모았다. 자리는 아직 남아.', 'neutral')])
d('c4_tally', A, [('narr', '벽에 그어진 금. 다섯 개씩 묶은 묶음이 몇 줄이나 된다.'), ('lu', '...날짜 세는 거구나.', 'sad')])
d('c4_jugs_empty', A, [('narr', '빈 물통들. 바닥에 물 자국만 말라붙어 있다.')])
d('c4_jugs_full', A, [('narr', '물통에 물이 찰랑거린다.'), ('lu', '물 소리 좋다!', 'smile')])
d('c4_008_hiding', A, [('kid008', '......', 'sad')])
d('c4_008_ask', A, [('kid008', '...그거, 과자야?', 'surprised')], ['과자를 하나 준다', '지금은 안 준다'])
d('c4_008_later', A, [('lu', '이따가! 이따가 줄게.', 'neutral'), ('kid008', '...응.', 'neutral')])
d('c4_008_fed', U, [('narr', '008 이 과자를 두 손으로 받는다. 한참 들여다본다.'), ('kid008', '하루에 하나씩 먹으면 오래 있어?', 'neutral')])
d('c4_008_venelle', U, [('narr', '베넬이 잠깐 말을 잃는다.'), ('venelle', '...하루에 하나만 먹지 마. 배고프면 더 먹어.', 'quiet')])
d('c4_008_split', A, [('narr', '루는 아무것도 묻지 않고 자기 과자를 반으로 쪼개 내민다.', None, True), ('kid008', '...더 있어도 돼?', 'surprised', True), ('lu', '응. 반은 네 거.', 'smile', True)])
d('c4_008_split_none', A, [('narr', '루가 주머니를 뒤진다. 비었다.'), ('lu', '...어? 내 건 방금 그게 마지막이었네. 다음에 꼭 하나 더 가져올게!', 'sad')])
d('c4_008_solo', A, [('lu', '아껴 먹는구나. 똑똑하다!', 'smile'), ('kid008', '...응.', 'neutral')])
d('c4_008_name', G, [('kid008', '나는 008.', 'neutral'), ('lu', '그럼 나는 루! 숫자 없어도 불러줘.', 'smile')])
d('c4_008_spark', G, [('kid008', '또 해 줘.', 'smile'), ('lu', '마지막 한 번!', 'smile'), ('f', '아까도 마지막이었어.', 'neutral')])
d('c4_008_after', A, [('kid008', '...아직 남았어. 하나.', 'smile')])
d('c4_sv_gift', A, [('survivor', '과자 맛, 오랜만이야. ...미안했어, 아까.', 'neutral')])
d('c4_sv_fight', A, [('survivor', '...손 아직 찌릿해. 할머니가 그만하래서 그만하는 거야.', 'scared')])
d('c4_sv_reconciled', A, [('survivor', '물 나오더라. ...아까 일은, 없던 걸로 하자.', 'neutral')])
d('c4_sv_repair', A, [('survivor', '펌프 소리 들려! 진짜 고쳤구나.', 'neutral')])
d('c4_pump_thanks_repair', A, [('f', '...물 나온다.', 'gentle'), ('f', '말로만 고치는 애는 아니었네. 지나가.', 'gentle')])
d('c4_pump_thanks_fight', A, [('f', '...물 나온다.', 'gentle'), ('f', '찌릿한 건 그걸로 갚은 셈 치자.', 'neutral')])
d('c4_pump_thanks_gift', A, [('f', '물까지 고쳤냐. ...과자 값은 넘쳤다.', 'gentle')])
d('waste_exit_wait', A, [('f', '아직 얘기 안 끝났어.', 'neutral')])
d('waste_door_wait', A, [('narr', '누군가 문 앞을 막고 서 있다.')])

# ---------------------------------------------------------------- 18 물 펌프실
d('c4_pump_enter', A, [('lu', '물이 펌프까지 잠겼어... 차례대로 해 보자. 상태판부터!')])
d('pump_valve_open', A, [('narr', '밸브가 끼익 돌아간다. 고인 물이 배수구로 빠지기 시작한다.')])
d('pump_valve_open_empty', A, [('narr', '밸브가 열렸다. 빠질 물은 이미 없다.')])
d('pump_valve_close', A, [('narr', '배수 밸브를 잠갔다.')])
d('pump_valve_close_mid', A, [('narr', '배수 밸브를 잠갔다. 물이 덜 빠진 채로 멈췄다.')])
d('pump_power_off', A, [('narr', '차단기를 내렸다. 펌프 쪽 전기가 끊겼다.')])
d('pump_trip', A, [('narr', '물 위로 불꽃이 튀고— 차단기가 탁 하고 도로 내려간다.'), ('lu', '물 있는 데에 전기부터 넣으면 안 되지! 나도 알아, 그건!', 'scared')])
d('pump_rattle', A, [('narr', '펌프가 덜덜거리며 헛돈다. 깨진 이음쇠에서 불꽃이 튄다.'), ('lu', '헛돌잖아... 이음쇠가 깨져서 힘이 안 넘어가.', 'neutral')])
d('pump_part_ok', A, [('narr', '새 이음쇠가 단단히 물려 있다.')])
d('pump_coupling_wet', A, [('narr', '이음쇠가 물속에 잠겨 손이 닿지 않는다.'), ('lu', '물부터 빼야겠다.', 'neutral')])
d('pump_coupling_live', A, [('narr', '손을 대자마자 이음쇠에서 불꽃이 튄다. 루가 손을 뗀다.'), ('lu', '나는 찌릿해도 괜찮지만... 새 부품까지 타겠어. 전원부터 내리자.', 'neutral')])
d('pump_need_part', A, [('narr', '깨진 이음쇠. 갈아 끼울 새 부품이 필요하다.')])
d('pump_need_part_seen', A, [('narr', '깨진 이음쇠. 선반에서 본 새 부품이 딱 맞을 것 같다.')])
d('pump_part_done', A, [('narr', '깨진 이음쇠를 빼고 새 부품을 끼웠다. 볼트를 끝까지 조인다.'), ('lu', '됐다! 이제 전기만 넣으면—', 'determined')])
d('pump_shelf', A, [('narr', "부품 선반. 상자에 '펌프 이음쇠 (예비)'."), ('lu', '이거다! 이걸로 갈면 되겠어.', 'smile')])
d('pump_shelf_empty', A, [('narr', '선반에 남은 쓸 만한 부품은 없다.')])
d('pump_running', A, [('narr', '펌프가 웅— 하고 돈다. 관을 따라 물소리가 거처 쪽으로 흘러간다.'), ('lu', '돈다! 물 간다아!', 'smile')])
d('pump_running_again', A, [('narr', '펌프가 다시 돈다.')])

# ---------------------------------------------------------------- 19 폐품 시장 (선택)
d('c4_market_enter', A, [('lu', '여기 시장이야? 다 고물인데... 다 팔리는 고물!')])
d('c4_lantern_ask', A, [('trader', '등불이 죽었어. 배터리가 다 됐거든. ...너 전기 쓰는 애라며?')], ['전기를 넣어 준다 (전하 1)', '그냥 둔다'])
d('c4_lantern_no', A, [('trader', '그래. 어두운 건 익숙해.')])
d('c4_lantern_lit', A, [('narr', '루가 손끝을 대자 등불이 노랗게 살아난다.'), ('trader', '...따뜻한 빛이다. 이거 가져가. 받을 건 받아야지.'),
                        ('lu', '전기 한 모금 값으로 과자 두 개! 남는 장사다!', 'smile')])
d('c4_lantern_after', A, [('trader', '아직 켜져 있어. 아껴 쓰는 중.')])
d('c4_scrap', A, [('scrapper', '뭐든 있어. 빗도 있어. 쇠로 된 거라 정전기는 장담 못 해.'), ('lu', '...나한텐 위험한 빗이네.', 'neutral')])
d('c4_scrap_again', A, [('scrapper', '빗은 아직 있어.')])
d('market_board', A, [('narr', "쪽지들이 꽂혀 있다. '상층 승강기 — 언제 열리나?' '정비동 터널로 가면 숙소 쪽.' '물 나오면 알려 줘.'")])
d('c4_lines', G, [('narr', '008 이 바닥에 선을 긋고 있다. 여덟 개.'), ('kid008', '여기 내 자리.', 'neutral', True), ('narr', '루가 아홉 번째 선을 그으려 하자 008 이 손을 막는다.'),
                  ('kid008', '거긴 할머니 자리.', 'neutral'), ('lu', '그럼 난 어디 앉아?', 'surprised'), ('narr', '008 이 선 하나를 더 길게 늘인다.'), ('kid008', '같이 앉아.', 'smile')])
d('c4_lines_sit', A, [('lu', '...좁다! 근데 좋다.', 'smile')])
d('c4_lines_again', A, [('kid008', '여기 루 자리. 비워 놨어.', 'smile')])

# ---------------------------------------------------------------- 5장
d('c5_manual', A, [('narr', "'출력 시험대 — 몸 전체로 한 번에 방전. 같은 전원선의 장치는 함께 정지함. 배터리 장치 제외.'"),
                   ('lu', '몸 전체로 한 번에...? 이건 안 해 봤는데.', 'surprised'), ('lu', '해 보자. 여기 인형들이면 다칠 사람도 없고!', 'determined')])
d('c5_manual_again', A, [('narr', "'시험 후에는 전원 복구 버튼을 누를 것.'")])
d('c5_hair_v', U, [('lu', '봤어? 지금 좀 멋있었지?', 'smile'), ('venelle', '응~ 위로 길어졌네.', 'gentle'), ('lu', '머리 말고 공격!', 'angry_cry')])
d('c5_comb_v', A, [('narr', '베넬이 주머니에서 빗을 꺼내다— 쇠로 된 빗인 걸 보고 도로 넣는다.'), ('lu', '...방금 그거 뭐야.', 'neutral'), ('venelle', '아무것도~', 'gentle')])
d('c5_hair_solo', A, [('lu', '...봤어? 아무도 못 봤지. 지금 좀 멋있었는데.', 'smile'), ('lu', '근데 머리는 왜 같이 서는 거야!', 'surprised')])
d('c5_lamp_note', A, [('lu', '저쪽 등만 꺼졌네... 배터리 달린 건 멀쩡하고. 기억해 둬야지.', 'neutral')])
d('c5_hall_enter', A, [('lu', '벨트가 이쪽으로 밀려와... 걸어서 오라는 거지? 무빙워크 금지!')])
d('c5_hall_off_v', U, [('lu', '다 껐다! ...우리 문도 껐다!', 'surprised'), ('venelle', '같이 쉬면 되지~', 'gentle'), ('lu', '쫓기는 중에 휴식시간 넣지 마아!', 'angry_cry')])
d('c5_hall_off_solo', A, [('lu', '다 껐다! ...내가 나갈 문도 껐다!', 'surprised'), ('lu', '전원 복구 버튼... 아니면 저 위 통로로 넘어가든가.', 'neutral')])
d('hall_motor', A, [('narr', "벨트 모터. '홀 전원선' 표시.")])
d('hall_pc1', A, [('narr', "작업 기록 PC. '홀 전원선'에 물려 있다.")])
d('hall_pc2', A, [('narr', '옆에 배터리가 달린 PC. 화면에는 오늘 작업량 표만 떠 있다. 잠긴 파일은 잠긴 그대로다.')])
d('hall_door_note', A, [('narr', "출구 문. '홀 전원선 — 정전 시 닫힘'.")])
d('hall_restored', A, [('lu', '다시 켜졌다! 벨트도... 같이 켜졌네.', 'neutral')])
d('c5_chase', A, [('lu', '들켰다! 위로!')])
d('c5_acid_v', A, [('venelle', '누나가 길 막을게~', 'excited'), ('lu', '그럼 내가 불 켤게!', 'determined')])
d('c5_shaft_solo', A, [('lu', '저 벨트, 저 셔터... 쓸 수 있겠다!')])
d('c5_plate_v', A, [('narr', '젖은 금속판 위로 전류가 번진다. 경비가 그 자리에 주저앉는다.', None), ('venelle', '반짝반짝~', 'excited', True), ('lu', '좋아, 아직 팔 붙어 있다! 다음!', 'determined', 'U')])
d('c5_shutter', A, [('lu', '쾅! 셔터 내렸다. 저쪽은 이제 못 와!', 'determined')])
d('c5_arm_ok', U, [('lu', '좋아, 아직 팔 붙어 있다! 다음!', 'determined')])
d('c5_shaft_clear', A, [('lu', '따돌렸다... 휴. 다음!')])

# ---------------------------------------------------------------- 6장
d('c6_enter', A, [('lu', '...빨래 냄새. 여기 사람들, 여기서 살았구나.')])
d('c6_enter_v', A, [('lu', '빨래 냄새다! ...누나, 여기 알아?')])
d('c6_twins_v', U, [('lu', '똑같은 옷이 두 개네! 하루 건너 새 옷인 척하기 좋겠다.', 'smile'), ('venelle', '린든이 좋아해서~', 'gentle'), ('lu', '아... 그럼 두 개 다 좋아하겠다.', 'neutral')])
d('c6_clothes_ask', A, [('venelle', '...저거, 누나 옷 같은데~', 'blank')], ['내려 줄게!', '나중에'])
d('c6_clothes', G, [('narr', '베넬이 옷을 고쳐 입는다. 구겨진 소매를 몇 번이나 편다.', None, 'A'), ('venelle', '린든이 이거 입으면 알아볼까?', 'gentle'), ('lu', '응. 근데 나도 이제 알아봐. 누나 옷.', 'smile')])
d('c6_twins_again_v', A, [('venelle', '하나는 빨아 둔 거야~', 'gentle')])
d('c6_twins_apart', A, [('lu', '똑같은 옷이 두 벌... 그 누나가 입던 옷이랑 똑같다.', 'neutral'), ('lu', '...혼자 괜찮으려나.', 'sad')])
d('c6_twins_dead', A, [('narr', '같은 옷 두 벌이 나란히 걸려 있다.'), ('lu', '......', 'sad'), ('lu', '...이 옷.', 'sad')])
d('c6_twins_again', A, [('narr', '같은 옷 두 벌.')])
d('c6_others', A, [('narr', "이름표가 붙은 작업복들. '3층 B조', '야간 배식', '정비 2'."), ('lu', '다들 여기서 빨래하고, 밥 먹고... 그냥 사는 사람들이었네.', 'neutral')])
d('c6_others_again', A, [('narr', '마르다 만 작업복들.')])
d('c6_room_enter_v', A, [('venelle', '여기... 누나 방 같아~', 'blank'), ('lu', '같아? 누나 방이면 누나가 알지!', 'surprised'), ('venelle', '아라라~', 'confused')])
d('c6_room_enter_apart', A, [('lu', '누가 쓰던 방이다... 침대가 아직 따뜻할 것 같아.')])
d('c6_room_enter_dead', A, [('lu', '...누가 쓰던 방이다.')])
d('c6_photo_v', A, [('narr', '사진 속에 베넬과, 베넬의 허리께까지 오는 아이. 뒷면에 삐뚤빼뚤한 글씨: 린든.'), ('lu', '...린든.', 'neutral'), ('venelle', '......', 'quiet')])
d('c6_photo_again_v', A, [('narr', '사진 속 두 사람이 웃고 있다.')])
d('c6_photo_apart', A, [('narr', '사진 속에 여자와 작은 아이. 뒷면에 글씨: 린든.'), ('lu', '그 누나다... 옆에 애가, 린든?', 'surprised')])
d('c6_photo_dead', A, [('narr', '사진 속에 여자와 작은 아이. 뒷면에 글씨: 린든.'), ('lu', '...이 사람.', 'sad'), ('lu', '......', 'sad')])
d('c6_photo_again', A, [('narr', '사진 속 두 사람.')])
d('c6_letter_v', A, [('lu', '아이비...?', 'surprised'), ('venelle', '...그거, 누나 이름이었나~?', 'confused')])
d('c6_letter_apart', A, [('lu', '아이비... 이 방 사람 이름인가.', 'neutral')])
d('c6_letter_dead', A, [('lu', '아이비.', 'sad'), ('narr', '루는 편지를 접어 원래 자리에 둔다.')])
d('c6_wrappers_v', A, [('narr', '서랍 위에 빈 초코바 포장지. 반듯하게 접힌 것이 열 장.'), ('lu', '...열 장.', 'neutral')])
d('c6_wrappers_after_v', A, [('narr', '열 장의 포장지. 베넬이 그쪽을 오래 보지는 않는다.')])
d('c6_wrappers_solo', A, [('narr', '빈 초코바 포장지 열 장. 반듯하게 접혀 있다.'), ('lu', '......', 'sad')])
d('c6_wrappers_again', A, [('narr', '포장지 열 장.')])
# 과거 (아이비): 짧은 장면 다섯 — 새 관찰자·설명자 없음, 동생의 죽음을 보여 주지 않는다
d('iv_1', A, [('narr', '엄마가 떠난 날, 집이 조용해졌다.')])
d('iv_2', A, [('narr', '아빠가 공장에서 사고를 당한 뒤로, 일은 아이비의 몫이 됐다.')])
d('iv_3', A, [('narr', '아이비는 상자를 날랐다. 낮에도, 밤에도.')])
d('iv_4', A, [('narr', "'시약 체험 참가자 모집 — 열흘. 숙식 제공. 보수 즉시 지급.'"), ('ivy', '...열흘이면.')])
d('iv_5', A, [('narr', '서랍에 초코바 열 개.')])
d('iv_mono', U, [('ivy', '열흘은 안 걸린대. 하루에 하나. 열 개면 남겠네. 돌아오면 또 사 오면 되고.')])
d('c6_v_confused', U, [('venelle', '루가... 린든이... 아라라, 누가 삐졌더라.', 'confused')])
d('c6_room_talk_1', U, [('lu', '천천히 말해. 나 지금 안 가.', 'neutral'), ('venelle', '기다리는 거 싫어하잖아~', 'gentle'),
                        ('lu', '내가 기다리는 건 잘해! 약속 늦는 건... 조금 못하지만.', 'smile')])
d('c6_room_talk_2', U, [('venelle', '그럼 초코바 먹을래?', 'gentle')])
d('c6_room_talk_3', U, [('lu', '응! 반씩 먹자. 큰 쪽은... 누나 줄게!', 'smile')])
d('c6_roof_v', G, [('lu', '나가면 어디 갈래?', 'smile'), ('venelle', '과자 사러~', 'gentle'), ('lu', '좋아! 그다음에도 과자 사러?', 'smile'), ('venelle', '잘 아네~', 'gentle'),
                   ('lu', '약속! 나가면 같이 과자 사러 가자.', 'smile', 'A'), ('venelle', '응~ 약속.', 'gentle', 'A')])
d('c6_roof_apart', A, [('lu', '바람 좋다...', 'neutral'), ('lu', '그 누나도 어디서 바람 쐬고 있으려나.', 'neutral')])
d('c6_roof_dead', A, [('lu', '......', 'sad'), ('lu', '...바람, 차갑다.', 'sad')])

# ---------------------------------------------------------------- 7장 직전
d('c7_gate_look', A, [('narr', "두꺼운 연결문. '수용동 — 관계자 외 출입 금지'. 안쪽에서 웅성거리는 소리."), ('lu', '...사람 소리다. 많이.', 'neutral')])
d('c7_gate_ask', A, [('narr', '들어가면 한동안 이쪽으로 돌아오지 못할 것 같다.')], ['수용동으로 들어간다', '아직 둘러본다'])
d('ch7_wip', A, [('sys', '여기부터 7장 「접수는 취소가 안 된대」입니다. 아직 만들지 않은 구간입니다.'), ('sys', '수용동 연결문 앞 상태로 저장했습니다. 타이틀로 돌아갑니다.')])

DOCS = [('D_IVY', '구겨진 편지', '린든에게.\n\n일 끝나면 바로 올게. 서랍에 초코바 열 개 넣어 놨어.\n하루에 하나씩.\n\n— 아이비')]


def main():
    data = json.load(io.open(PATH, encoding='utf-8'))
    sp = {s['id'] for s in data['speakers']}
    for i, n, side in SPEAKERS:
        if i not in sp: data['speakers'].append({'id': i, 'name': n, 'side': side})
    ch = {c['id'] for c in data['chapters']}
    for i, t in CHAPTERS:
        if i not in ch: data['chapters'].append({'id': i, 'title': t})
    ids = {x['id']: k for k, x in enumerate(data['dialogues'])}
    made = 0
    for id, src, lines, choices in E:
        L = []
        for ln in lines:
            s, t = ln[0], ln[1]
            e = ln[2] if len(ln) > 2 else None
            flag = ln[3] if len(ln) > 3 else None
            o = {'s': s, 't': t}
            if e: o['e'] = e
            # src A 이거나 줄 단위 표시(True/'A')면 보충 대사
            if (src == A and flag != 'U') or flag is True or flag == 'A': o['added'] = True
            L.append(o)
        entry = {'id': id, 'src': src, 'lines': L}
        if choices: entry['choices'] = choices
        if id in ids: data['dialogues'][ids[id]] = entry
        else: data['dialogues'].append(entry); made += 1
    dids = {x['id']: k for k, x in enumerate(data['docs'])}
    for i, t, b in DOCS:
        e = {'id': i, 'title': t, 'added': True, 'body': b}
        if i in dids: data['docs'][dids[i]] = e
        else: data['docs'].append(e)
    # 원본과 같은 형식 (CRLF, 들여쓰기 1, 끝 줄바꿈 없음)
    with io.open(PATH, 'w', encoding='utf-8', newline='\r\n') as f:
        json.dump(data, f, ensure_ascii=False, indent=1)
    print('new dialogues', made, 'total', len(E))


if __name__ == '__main__':
    main()
