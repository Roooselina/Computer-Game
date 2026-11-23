# -*- coding: utf-8 -*-
"""
7~11장·엔딩 대사·문서·말풍선을 Assets/Game/Data/Dialogue/dialogue_ko.json 에 넣는다 (같은 id 는 이 스크립트 내용으로 갱신, 다른 항목은 그대로).
src 표기: G = GDD 원문 그대로 / A = 이번에 보충한 연결 대사 (줄마다 added: true). 줄 단위 혼합은 4번째 값 'G' 또는 'A'.
공간 의존 대사(‘아래’, ‘위층’, ‘계단’ 등)는 world_layout.json 의 spaceLines 에 맵별로 적어 둔다.
실행: python Tools/ch7_11_dialogue.py
"""
import io, json, os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PATH = os.path.join(ROOT, 'Assets', 'Game', 'Data', 'Dialogue', 'dialogue_ko.json')
G = 'GDD 7~11장·마지막·7.4·7.5·7.7 원문'
A = '추가 (7~11장 연결, 2026-11-15)'

SPEAKERS = [('raviel', '라비엘', 'R'), ('meilin', '메이린', 'R'), ('lacrimosa', '라크리모사', 'R'), ('aurelius', '아우렐리우스', 'R'),
            ('edward', '에드워드', 'R'), ('member', '단원', 'R'), ('warden', '회수 경비', 'R')]
CHAPTERS = [('ch8', '8장 — 너였구나'), ('ch9', '9장 — 앙코르가 없는 노래'), ('ch10', '10장 — 모두라는 말'), ('ch11', '11장 — 조금만 더 같이 있자'), ('end', '마지막 — LAST LIGHT')]
E = []
B = []


def d(id, src, lines, choices=None):
    E.append((id, src, lines, choices))


def bub(id, cond, who, lines, map=None, need=None):
    B.append(dict(id=id, cond=cond, who=who, map=map, need=need, lines=[dict(s=s, t=t) for s, t in lines]))


# ================================================================ 7장 27 수용동
d('c7_gate_shut', A, [('narr', '등 뒤에서 연결문이 쾅 닫힌다. 이쪽에는 손잡이가 없다.'), ('lu', '...닫혔다. 그럼 앞으로!', 'determined')])
d('c7_holding', A, [('narr', '창살 너머 사람들이 웅크리고 있다. 목에 번호표.'), ('lu', '사람이다... 많아. 다 갇혀 있어.', 'sad')])
d('c7_holding_v', A, [('narr', '창살 너머 사람들이 웅크리고 있다. 목에 번호표.'), ('lu', '사람이다... 많아. 다 갇혀 있어.', 'sad'),
                      ('venelle', '여기... 누나도 있었던 데 같아~', 'blank'), ('lu', '그럼 길도 알겠네! 열어 주자.', 'determined')])
d('c7_desk_card', A, [('narr', '서랍에 직원 카드가 한 장 남아 있다.'), ('lu', '빌려 갈게요! 나중에... 돌려줄 수 있으면!', 'smile')])
d('c7_desk_again', A, [('narr', '빈 책상. 수용동 명부가 펼쳐져 있다.')])
d('c7_release_panel', A, [('narr', "제어반 화면: '수용칸 개방 — 관리자 확인'. 버튼 하나에 불이 들어와 있다."), ('lu', '확인! 아주 확인!', 'determined')])
d('c7_release_line', G, [('lu', '한 줄로! 밀지 말고! 나도 길 잘 몰라서 밀면 다 같이 길 잃어!', 'determined')])
d('c7_release_route', A, [('lu', '저 비상 계단으로 쭉 내려가면 물소리 나는 데가 있어. 폐기소야.', 'neutral'),
                          ('lu', 'F 할머니한테 루가 보냈다고 해! 거기 대피실 자리 남았대.', 'smile'),
                          ('survivor', '...밖으로 나가는 길이야?', 'scared'), ('lu', '아니. 아직 시설 안이야. 그래도 거긴 사람들이 버티고 있어.', 'sad')])
d('c7_release_route_v', A, [('lu', '저 비상 계단으로 쭉 내려가면 물소리 나는 데가 있어. 폐기소야.', 'neutral'),
                            ('lu', 'F 할머니한테 루가 보냈다고 해! 거기 대피실 자리 남았대.', 'smile'),
                            ('survivor', '...밖으로 나가는 길이야?', 'scared'), ('lu', '아니. 아직 시설 안이야. 그래도 거긴 사람들이 버티고 있어.', 'sad'),
                            ('venelle', '천천히 가~ 넘어지면 누나가... 아, 누나는 여기 있지.', 'gentle')])
d('c7_release_after', A, [('narr', '마지막 사람이 계단 아래로 사라진다.'), ('lu', '...다 갔다. 이제 접수 홀 쪽!', 'determined')])
d('c7_stairs', A, [('narr', "비상 계단. 벽에 '지하 배수로' 화살표."), ('lu', '여기로 가면 폐기소. 나는... 아직 할 일이 있어.', 'neutral')])
d('c7_stairs_after', A, [('narr', '아래쪽에서 발소리가 멀어진다.')])
d('c7_back_sealed', A, [('narr', '연결문은 이쪽에서 열리지 않는다.')])
d('c7_not_yet', A, [('lu', '사람들 두고는 못 가!', 'determined')])

# ================================================================ 7장 28 접수 홀 · 메이린
d('c7_meilin_v', G, [('narr', '루가 번호표 발급기를 두 번 누른다. 표가 두 장 나온다.'), ('meilin', '고객님, 번호표는 한 장씩입니다.', 'smile'), ('lu', '뒤에 누나 것도!', 'smile'),
                     ('meilin', '그 누나는 직원이잖아!', 'determined'), ('venelle', '나 직원이었어~?', 'blank'), ('meilin', '아 진짜! 둘 다 여기서 움직이지 마!', 'determined')])
d('c7_meilin_solo', G, [('narr', '루가 번호표 발급기를 두 번 누른다. 표가 두 장 나온다.'), ('meilin', '고객님, 번호표는 한 장씩입니다.', 'smile'),
                        ('lu', '한 장은 예비! 잃어버리면 어떡해!', 'smile'), ('meilin', '...아 진짜! 여기서 움직이지 마!', 'determined')])
d('c7_meilin_start', A, [('meilin', '47번 고객님— 호출입니다. 접수는 취소가 안 돼요.', 'smile'), ('narr', '천장의 포획팔이 레일을 따라 움직인다.'),
                         ('lu', '팔이 내 번호 부르는 거야?! 저쪽 단말... 유지보수용이면 번호 바꿀 수 있겠다!', 'surprised')])
d('c7_mt_screen', A, [('narr', "유지보수 단말 — 허용된 기능: '호출 번호 변경', '표적 위치 변경'. 대기 중 표적: 47 (대기석) / 00 운반 인형 (하역대).")])
d('c7_meilin_arm', G, [('meilin', '그 애 하나를 못 잡아왔단 말이야? ...아니, 너 말고 저 팔!', 'determined'), ('lu', '팔도 혼나네.', 'smile')])
d('c7_meilin_armoff', A, [('meilin', '고객님, 잠시만—아 진짜, 멈춰!', 'determined', 'G'), ('narr', '포획팔의 불이 꺼진다. 메이린이 가위를 꺼낸다.'),
                          ('meilin', '팔은 됐어. 내가 직접 할 거야.', 'determined')])
d('c7_meilin_lose', A, [('meilin', '아이.. 난 몰라 몰라...', 'scared', 'G'), ('meilin', '명부는... 다 적어야 하는데.', 'sad')])
d('c7_meilin_gone', A, [('narr', '메이린이 롤러를 굴려 안쪽 통로로 사라진다.'), ('lu', '...도망갔다. 다친 데는 없어 보였어.', 'neutral')])
d('c7_meilin_gone_v', A, [('narr', '메이린이 롤러를 굴려 안쪽 통로로 사라진다.'), ('lu', '...도망갔다. 다친 데는 없어 보였어.', 'neutral'), ('venelle', '저 애도 번호표 있었어~', 'quiet')])
d('c7_ticket_before', A, [('narr', '번호표 발급기. 47번이 깜빡인다.')])
d('c7_ticket_again', G, [('narr', '번호표가 하나 더 나온다.'), ('lu', '분실 사유요? ...아무도 안 물어보네.', 'neutral')])
d('c7_ticket_again2', A, [('narr', '발급기가 조용하다.')])
d('c7_hall_locked', A, [('narr', "문 옆 표시: '상담 중'.")])

# ================================================================ 7장 29 상담 부스
d('c7_booth_enter', A, [('lu', '상담 부스... 여기서 사람들 이름을 적었구나.', 'neutral')])
d('c7_chair', G, [('narr', '상담 의자. 앉으라는 안내문이 붙어 있다.'), ('lu', '앉으라고 한 적 없습니다— 라고 할 사람도 없네. 의자가 먼저 권했어.', 'smile')])
d('c7_chair_again', A, [('narr', '의자가 조금 기울어 있다.')])
d('c7_meilin_memory', G, [('narr', '부스 잠금장치. 안쪽에서는 열 수 없게 되어 있다.'), ('meilin', '(기억) 문만 나오면 되는 줄 알았는데.', 'sad'), ('lu', '...안에서 못 여는 문이구나.', 'sad')])
d('c7_booth_lock_again', A, [('narr', '잠금장치는 바깥쪽에만 손잡이가 있다.')])
d('c7_lostfound_card', A, [('narr', '분실물 상자 안에 직원 카드가 한 장 있다.'), ('lu', '여기 하나 있었다! 이걸로 영상실 문 열자.', 'smile')])
d('c7_lostfound_empty', A, [('narr', '짝 잃은 장갑, 번호표 몇 장. 쓸 만한 건 없다.')])
d('c7_need_card', A, [('narr', '카드 리더기에 빨간 불. 직원 카드가 필요하다.'), ('lu', '직원 카드... 경비 책상이나 분실물 상자에 있을지도.', 'neutral')])

# ================================================================ 7장 30 영상실
d('c7_monitors', A, [('narr', '감시 모니터 넉 대. 시설 카메라의 영상이 돌아간다.'), ('lu', '어디 보자... 아까 그 사람들!', 'surprised')])
d('c7_monitors_again', A, [('narr', "모니터 구석에 '봉쇄 중' 표시.")])
d('c7_monitors_first', A, [('lu', '저 모니터부터 보고 가자. 어디로 가야 하는지...', 'neutral')])
d('c7_feed_stairs', A, [('lu', '계단 내려가고 있어. 다들 한 줄로. ...잘했다!', 'smile')])
d('c7_feed_venelle', A, [('narr', '화물 승강장 카메라. 익숙한 옷차림이 승강기 쪽으로 걸어간다.'), ('lu', '...누나? 아래로 가는 거야?', 'surprised'),
                         ('lu', 'F 할머니네로 가면... 괜찮겠지. 거긴 사람도 있고, 물도 나오고.', 'sad')])
d('c7_lockdown', A, [('sys', "〔중앙 통제〕 수용·접수동 봉쇄를 시작합니다. 관계자는 지정 구역에서 대기하십시오."), ('narr', '경고등이 붉게 돈다. 관제탑 쪽 격벽이 내려오기 시작한다.')])
d('c7_lockdown_v', A, [('lu', '격벽 닫힌다! 누나, 저쪽으로!', 'scared'), ('venelle', '응~ 뛰는 건 싫은데에...', 'confused')])
d('c7_lockdown_solo', A, [('lu', '격벽 닫힌다! 저쪽으로!', 'scared')])
d('c7_wardens', A, [('warden', '대상 확인.'), ('narr', '뒤쪽 문에서 회수 경비 둘이 들어온다.')])
d('c7_wardens_solo', A, [('warden', '대상 확인.'), ('lu', '으갹! 지금 오면 어떡해!', 'scared')])
d('c7_bulkhead_talk', G, [('lu', '같이 가면 되잖아! 누나!', 'scared'), ('venelle', '가야지~ 같이. 그러니까 먼저 가서 문 좀 열어 줘.', 'gentle'),
                          ('lu', '금방 와야 돼! 나 기다린다!', 'determined'), ('venelle', '응. 이번엔...', 'quiet')])
d('c7_venelle_last', G, [('narr', '방어 장치의 불이 꺼진다. 약병을 놓친 손이 움직이지 않는다.', None, 'A'), ('venelle', '린든. 늦었지.', 'quiet')])
d('c7_bulkhead_solo', A, [('narr', '등 뒤에서 격벽이 닫힌다.'), ('lu', '...휴. 간발이었다.', 'scared')])
d('c7_bulkhead_shut', A, [('narr', '격벽이 내려와 있다. 이쪽에서는 열 수 없다.')])

# ================================================================ 7장 31 관제탑
d('c7_tower', A, [('lu', '관제탑... 저 단말로 위쪽 통로를 열 수 있을 것 같아.', 'neutral')])
d('c7_tower_v', A, [('lu', '여기서 문 열면 돼! 누나가 열어 달랬어!', 'determined')])
d('c7_control', A, [('narr', "중앙 통제 단말. '상층 회랑 통로 — 잠김 / 열기'.")])
d('c7_control_open', A, [('narr', '덜컹. 위쪽 어딘가에서 문이 열리는 소리.'), ('lu', '열었다!', 'smile')])
d('c7_knock', G, [('lu', '나 열었어! 누나, 이쪽은 열었어!', 'determined')])
d('c7_knock_after', A, [('narr', '격벽 너머에서는 아무 소리도 나지 않는다.'), ('lu', '...먼저 가 있으라고 했지. 위에서 기다릴게.', 'sad')])
d('c7_control_again', A, [('narr', "'상층 회랑 통로 — 열림'.")])
d('c7_control_first', A, [('lu', '위쪽 통로가 잠겨 있어. 관제 단말부터.', 'neutral')])

# ================================================================ 8장 32 상층 회랑 · 라비엘
d('c8_reunion_1', G, [('lu', '어? 너... 잠깐, 진짜 너야? 여기서 뭐 해!', 'surprised'), ('raviel', '넌 정말이지 변하지 않았구나.', 'smirk'),
                      ('lu', '너는 엄청 변했네! 그건... 아, 지금 그 말부터 하면 안 되나?', 'surprised'), ('raviel', '아무렇지도 않네. 돌아오지도 않았으면서.', 'sad'),
                      ('lu', '...돌아가려고 했어. 계속.', 'sad')])
d('c8_reunion_2', A, [('raviel', '...거짓말.', 'pained'), ('narr', '라비엘의 몸에서 빛이 일렁인다.')])
d('c8_transform', A, [('narr', '라비엘이 거대한 괴물 토끼의 모습으로 변한다.'), ('lu', '라비엘! 잠깐만..!?', 'scared')])
d('c8_route_ask', A, [('lu', '...다치게 하고 싶지 않아.', 'determined')], ['도망친다 — 거리를 벌린다', '멈춰 세운다'])
d('c8_flee_end', A, [('narr', '회랑 끝 문 앞. 쫓아오던 발소리가 멈춘다.'), ('lu', '라비엘! 나 여기 있어! 안 가!', 'determined')])
d('c8_fight_end', A, [('narr', '괴물 토끼가 무릎을 꿇는다.'), ('lu', '그만! 나 네 얼굴 때리기 싫어!', 'angry_cry', 'G')])
d('c8_after_duel', A, [('narr', '빛이 걷히고, 라비엘이 원래 모습으로 주저앉아 있다.'), ('raviel', '...왜 도망 안 가.', 'pained'), ('lu', '도망은 했지. 너한테서가 아니라 너 다치는 거에서.', 'sad'),
                       ('raviel', '......내 방. 이쪽이야.', 'sad')])
d('c8_not_yet', A, [('lu', '라비엘 두고는 안 가.', 'determined')])

# ================================================================ 8장 33 라비엘의 방
d('c8_room_enter', A, [('lu', '라비엘 방... 깔끔하다! 컵이 두 개네?', 'smile')])
d('c8_photo_alone', A, [('narr', '액자 두 개. 어린아이 둘이 나란히 웃고 있다.')])
d('c8_photo', G, [('lu', '너 이 사진 아직 갖고 있었어?', 'surprised'), ('raviel', '버리려고 꺼내 놨던 거야.', 'neutral'), ('lu', '액자에 넣어서?', 'smile'), ('raviel', '......말 많아.', 'smirk')])
d('c8_past', A, [('lu', '나 그때... 고민해결소 갔다가 잡혀 갔어. 친구도 도와주냐고 물어보러.', 'sad'),
                 ('raviel', '알아. 네가 안 돌아와서... 찾으러 갔어. 그리고 나도 같은 데로.', 'sad'), ('lu', '...너도.', 'sad')])
d('c8_confess', G, [('raviel', '널 부른 건 나야. 연구만 한다고 했어. 아니... 그 말에 기대고 싶었던 거지.', 'pained'), ('lu', '나 찾으면 화내려고 했어?', 'neutral'), ('raviel', '그래.', 'sad'),
                    ('lu', '그럼 찾았으니까 됐네. 화는 나가서 내.', 'smile'), ('raviel', '그렇게 간단하게...', 'surprised'),
                    ('lu', '간단하진 않지! 엄청 놀랐고 아팠고! 그래도 너 여기 두고는 안 가.', 'determined'), ('raviel', '왜.', 'sad'), ('lu', '내 친구니까. 아직도 몰라?', 'smile')])
d('c8_choice', A, [('narr', '라비엘이 침대 끝에 앉아 고개를 숙인다.')], ['손을 잡는다', '옆에 앉는다'])
d('c8_choice_hand', A, [('narr', '루가 라비엘의 손을 잡는다. 라비엘은 빼지 않는다.'), ('raviel', '......손 따뜻하네.', 'sad'), ('lu', '전기 만드는 몸이라서! 같이 가자.', 'smile')])
d('c8_choice_sit', A, [('narr', '루가 옆에 털썩 앉는다. 어깨가 닿는다.'), ('raviel', '......좁아.', 'smirk'), ('lu', '오랜만이잖아. 같이 가자.', 'smile')])
d('c8_raviel_mono', G, [('raviel', '(돌아오면 묻고 싶었다. 왜 나였냐고. 막상 돌아오니 먼저 이름을 부르고 싶었다.)', 'sad')])
d('c8_photo_after', G, [('lu', '그 사진 나 웃기게 나왔지?', 'smile'), ('raviel', '원래 그렇게 웃었어.', 'neutral'), ('lu', '지금도?', 'smile'), ('raviel', '응.', 'smile')])
d('c8_photo_after2', A, [('narr', '액자 두 개가 나란히 놓여 있다.')])
d('c8_room_wait', A, [('lu', '라비엘이랑 얘기부터.', 'neutral')])
d('c8_cups_alone', A, [('narr', '컵 두 개. 하나는 손잡이가 없다.')])
d('c8_cups', G, [('narr', '루가 손잡이 없는 컵을 집는다. 라비엘이 다른 컵을 내민다.', None, 'A'), ('raviel', '그거 깨진 거야. 이거 써.', 'neutral', 'A'),
                 ('lu', '손잡이만 없잖아. 양손으로 잡으면 돼.', 'smile'), ('lu', '버리지 마. 나중에 이걸로 같이 마시자.', 'smile', 'A'), ('raviel', '......그래.', 'smile', 'A')])
d('c8_cups_after', A, [('narr', '손잡이 없는 컵이 선반 앞쪽에 옮겨져 있다.')])

# ================================================================ 8장 34 통신실 · 35 교량
d('c8_console_solo', A, [('narr', '통신 단말. 라비엘의 계정으로 잠겨 있다.')])
d('c8_console', A, [('raviel', '여긴 바깥 통로 신호를 재는 데야. 경계 너머에서 돌아오는 반응을 기록해.', 'neutral'),
                    ('lu', '그러니까... 문 너머에 메아리 치는지 듣는 거구나!', 'smile'), ('raviel', '......대충 맞아.', 'smirk'),
                    ('raviel', '출력을 무리하게 올리면 냉각이랑 구조가 못 버텨. 안전 검토서에 써 뒀어.', 'sad'), ('lu', '그럼 무리 안 하면 되네!', 'smile'), ('raviel', '...그 말을 들을 사람이었으면.', 'sad')])
d('c8_console_again', A, [('raviel', '신호는 아직 불안정해.', 'neutral')])
d('c8_bench_solo', A, [('narr', '창밖으로 어두운 시설 동이 보인다.')])
d('c8_bench', A, [('lu', '잠깐 앉자! ...나가면 뭐 먹을래?', 'smile'), ('raviel', '갑자기?', 'surprised'), ('lu', '정해 둬야 빨리 나가지!', 'smile')], ['붕어빵', '떡볶이', '초코바'])
d('c8_bench_1', A, [('raviel', '붕어빵. ...팥으로.', 'smile'), ('lu', '좋아! 붕어빵! 꼬리부터 먹는 거 알지?', 'smile')])
d('c8_bench_2', A, [('raviel', '떡볶이. 안 매운 걸로.', 'neutral'), ('lu', '좋아! 떡볶이! 맵기는 내가 맡을게.', 'smile')])
d('c8_bench_3', A, [('raviel', '초코바. ...반씩.', 'smile'), ('lu', '좋아! 초코바! 큰 쪽은 너 줄게.', 'smile')])
d('c8_bench_again', A, [('narr', '창밖은 여전히 어둡다.'), ('lu', '먹을 거 정했으니까, 이제 나가기만 하면 돼!', 'smile')])

# ================================================================ 9장 36 대기실
d('c9_wait', A, [('narr', '대기실. 사람들이 번호표를 쥐고 조용히 기다린다.'), ('lu', '다들... 어디로 가는 걸 기다리는 거지.', 'neutral')])
d('c9_wait_r', A, [('narr', '대기실. 사람들이 번호표를 쥐고 조용히 기다린다.'), ('lu', '다들... 어디로 가는 걸 기다리는 거지.', 'neutral'), ('raviel', '전이. 단체로.', 'sad')])
d('c9_plans', G, [('narr', "벽에 붙은 종이들. '도착하면 식당 차릴 거임' '수리점 — 연장 챙길 것' '합창단 단원 모집' '호위대 지원'.", None, 'A'),
                  ('lu', '다들 진짜로 가서 할 일을 적어 놨네...', 'sad', 'A')])
d('c9_plans_again', A, [('narr', "'합창단 단원 모집 — 연습은 도착 후'.")])
d('c9_chart', A, [('narr', '배치도. 도착 구역마다 붉은 선이 그어져 있다. 옆에는 지휘 체계도 — 맨 위에 아우렐리우스.'),
                  ('lu', '...피난처라면서 왜 점령 구역이라고 써 있어?', 'surprised')])
d('c9_chart_again', A, [('narr', "'1차 거점 확보 — 호위대'.")])
d('c9_member1', A, [('member', '도착하면 식당을 할 거예요. 국밥. 여기선 다들 배가 고프니까.', 'neutral')])
d('c9_member1_again', A, [('member', '번호가 불리면 가요. 곧이래요.', 'neutral')])
d('c9_member2', A, [('member', '총재님이 자리를 비워 두셨대요. 내 몫이라고.', 'neutral'), ('lu', '...그 자리, 진짜 안전해?', 'neutral')])
d('c9_member2_again', A, [('member', '당신 몫도 있을 거예요.', 'neutral')])
d('c9_member3', A, [('member', '호위대에 지원했어요. 거기선 쓸모 있는 사람이 되고 싶어서.', 'neutral')])
d('c9_member3_again', A, [('member', '...쓸모없었던 게 아니라고, 그분이 그랬어요.', 'neutral')])

# ================================================================ 9장 37 공명 공연장 · 라크리모사
d('c9_lac_meet', G, [('lacrimosa', '노래는 참 아름답죠. 끝난 뒤에도 누군가 기억해 주니까요.', 'gentle'), ('lu', '좋아! 그럼 들으면서 지나가면 안 돼?', 'smile'),
                     ('lacrimosa', '오늘의 공연은 좌석 이동을 삼가 주십시오.', 'gentle'), ('lu', '나 좌석도 없는데!', 'surprised')])
d('c9_lac_meet_r', A, [('raviel', '저 장치, 마이크랑 스피커 사이에 소리가 돌면 스스로 멈춰. 보호 회로가 있어.', 'neutral'), ('lu', '소리가 빙글빙글! 알았어!', 'determined')])
d('c9_lac_stop', A, [('lacrimosa', '...소리가 되돌아오는군요. 오래된 극장처럼.', 'quiet'), ('lu', '내 목소리 왜 두 번 나와! 와!', 'surprised', 'G')])
d('c9_lac_listen', A, [('narr', '오래된 테이프에서 TV 노래 한 소절이 흘러나온다.'), ('lacrimosa', '......', 'quiet'), ('narr', '라크리모사가 손을 내린다. 공격을 멈추고 듣는다.')])
d('c9_lac_listen_again', A, [('narr', '같은 노래. 라크리모사가 다시 멈춰 선다.')])
d('c9_lac_talk_ask', A, [('narr', '라크리모사가 멈춰 서 있다.')], ['말을 건다', '그만둔다'])
d('c9_edward_1', G, [('narr', '어린 에드워드가 집안의 고성을 피해 TV 앞에 웅크려 있다. 노래가 흘러나온다.', None, 'A')])
d('c9_edward_2', G, [('narr', '음향실. 에드워드가 녹음 파일 이름을 하나씩 적는다.', None, 'A'),
                     ('edward', '내가 좋아한 소리는 남겨두고 싶었다. 집에 돌아가면 없어지는 것들이 너무 많아서.', 'quiet')])
d('c9_lac_lu', G, [('lu', '다음 노래 말고 지금 부르면 안 돼? 너 지금도 노래할 수 있잖아.', 'sad'), ('narr', '라크리모사는 답을 하지 못한다.', None, 'A')])
d('c9_lac_persuaded', A, [('lacrimosa', '......지나가십시오. 객석이 비어 있으니까요.', 'quiet'), ('narr', '무대 옆 문이 열린다.')])
d('c9_lac_subdued', A, [('narr', '라크리모사가 무대에 쓰러져 있다. 숨은 고르다. 옆문이 열린다.'), ('lu', '...미안. 노래, 나중에 들려줘.', 'sad')])
d('c9_hall_wait', A, [('lu', '공연장 문이 안 열려...', 'neutral')])
d('c9_hall_gone', A, [('narr', '공연장으로 가는 통로가 무너져 막혔다.')])

# ================================================================ 9장 38 녹음 보관실
d('c9_archive_enter', A, [('lu', '테이프가 잔뜩... 다 누가 녹음한 거지.', 'neutral')])
d('c9_tape', A, [('narr', "테이프 하나. 이름표에 'E.' 라고만. 옆에 녹음 목록."), ('lu', '...이거, 그 사람 거 같아. 들고 가자.', 'neutral')])
d('c9_tape_again', A, [('narr', '빈 자리에 먼지 자국만 남아 있다.')])

# ================================================================ 9장 39 서버실: 강제 가동 · 사망 확인
d('c9_force_broadcast', A, [('aurelius', '여러분. 봉쇄가 길어지면 우리는 흩어집니다. 지금 문을 엽니다. 모두를 위해.', 'gentle'),
                            ('sys', '〔전이 장치〕 예열 출력 상승 — 경계 안정도 기준 미달. 냉각 한계 초과. 강제 가동.')])
d('c9_force', A, [('narr', '바닥이 크게 흔들린다. 어딘가 깊은 곳에서 무언가 무너지는 소리.'), ('lu', '으아! 뭐야?!', 'scared')])
d('c9_force_r', A, [('narr', '바닥이 크게 흔들린다. 어딘가 깊은 곳에서 무언가 무너지는 소리.'), ('raviel', '...안 돼. 냉각이 못 버텨. 구조 하중도— 내가 쓴 그대로야.', 'pained'),
                    ('lu', '라비엘! 어디가 무너진 거야?', 'scared')])
d('c9_comm_terminal', A, [('narr', '통신 중계 단말에 시설 카메라 영상과 현장 방송이 연결된다.')])
d('c9_shelter_after', A, [('sys', '〔현장 방송〕 지하 B2 대피 구역 붕괴. 지지 구조·관로 손상. 응답 없음.'),
                          ('narr', '잔해 아래, 끊어진 번호표 하나. 움직이지 않는 손.')])
d('c9_hall_after', A, [('sys', '〔현장 방송〕 공명 공연장 조명 트러스 낙하.'), ('narr', '마지막 노래가 중간에서 끊겼다.')])
d('c9_lu_shock', G, [('lu', '...아래에 애가 있었어. 빨리 가자. 확인하러 가자.', 'despair'), ('narr', '대답할 사람이 없다. 길은 앞으로만 나 있다.', None, 'A')])
d('c9_lu_shock_r', G, [('lu', '...라비엘, 아래에 애가 있었어. 우리 빨리 가자. 확인하러 가자.', 'despair'), ('narr', '라비엘은 통로가 무너진 것을 알고 바로 답하지 못한다.', None, 'A'),
                       ('raviel', '......루.', 'pained', 'A')])
d('c9_server_wait', A, [('narr', "문 표시: '포획 단말 작업 중 — 출입 제한'.")])
d('c9_capture', A, [('sys', '〔원격 장치〕 E-D05-47 재고정 절차 시작.'), ('lu', '어?! 몸이— 빨려 들어가!', 'scared'), ('raviel', '루!', 'surprised')])

# ================================================================ 9장 40 포획 단말 · 41 휴지통
d('c9_terminal_in', A, [('lu', '여긴... 단말 안이다. 붉어. 기분 나빠.', 'scared')])
d('c9_incomplete', A, [('sys', "보관 항목 'E-D05-47' 판정: 불완전. 복구 대기 보관함으로 이동합니다."), ('lu', '불완전? 나 완전한데?!', 'surprised'),
                       ('lu', '...저기 파일 하나 떠 있다.', 'neutral')])
d('c9_moved', A, [('narr', '화면이 접히듯 뒤집힌다.')])
d('c9_bin_talk', G, [('lu', '...쓰레기통? 나 분리수거 잘했는데?', 'surprised'), ('raviel', '네 항목 찾아. 이름 말고 번호로.', 'neutral'), ('lu', '내 이름 루! 아주 짧은데!', 'smile'),
                     ('raviel', '저쪽이 그렇게 저장 안 했어.', 'neutral'), ('lu', '알았어, 긴 이름의 나 찾아볼게!', 'determined'),
                     ('raviel', '자동 정리는 내가 멈춰 뒀어. 서두르지 않아도 돼. 삭제한 날짜로 정렬하면 맨 위에 있을 거야.', 'neutral', 'A')])
d('c9_bin_status', A, [('narr', "'휴지통 비우기 예약 — 일시 중지됨 (운영 계정: 연구주임 R)'.")])
d('c9_bin_restored', A, [('raviel', '확인. 원래 위치로 돌아왔어. 이제 구속 해제할게.', 'neutral')])
d('c9_bin_released', A, [('raviel', '해제했어. 안에서 확인 누르면 나와.', 'neutral')])
d('c9_bin_exit', A, [('narr', '빛이 한쪽으로 쏠린다.')])
d('c9_catch', G, [('narr', '루가 단말 밖으로 나오며 휘청거린다. 라비엘이 잡는다.', None, 'A'), ('lu', '잡았다! 이번엔 네가 잡았다.', 'smile'),
                  ('raviel', '내가 잡았지. 넌 떨어졌고.', 'smirk'), ('lu', '잘 맞네, 우리.', 'smile')])

# ================================================================ 10장
d('c10_boarding', A, [('narr', '승선 대기실. 탑승권을 쥔 단원들이 줄을 서 있다.'), ('lu', '진짜로 다 데려가려는 거구나...', 'neutral')])
d('c10_board', A, [('narr', "탑승 현황판: '통과 완료 0 · 대기 64'."), ('lu', '0...', 'neutral')])
d('c10_board_again', A, [('narr', "'통과 완료 0'.")])
d('c10_member1', A, [('member', '드디어예요! 자리가 있대요. 내 자리.', 'neutral')])
d('c10_member1_again', A, [('member', '자리를 비워 두겠습니다, 당신의 몫입니다— 그분이 그러셨어요.', 'neutral')])
d('c10_member2', A, [('member', '방금 흔들림이... 괜찮은 거죠?', 'neutral')])
d('c10_member2_again', A, [('member', '번호가 불리면 가면 돼요.', 'neutral')])
d('c10_back_gone', A, [('narr', '뒤쪽 통로가 무너져 막혔다.')])
d('c10_capsule', A, [('narr', '캡슐 하나가 불꽃을 튀기며 꺼진다. 안의 단원이 쓰러진다. 숨은 있지만 깨어나지 않는다.'), ('lu', '저 사람— 다쳤어! 장치가 망가진 거야!', 'scared')])
d('c10_meilin', G, [('narr', '메이린이 접수 명부를 끌어안고 비상 해제 손잡이에 매달려 있다.', None, 'A'),
                    ('meilin', '이름 다 적었어! 빠진 사람 없이 다 적었다고! 멈추고 다시 하면 되잖아!', 'scared'),
                    ('aurelius', '메이린. 지금이 아니면 다시는 없습니다.', 'gentle'), ('meilin', '아이... 그 말 좀 그만해!', 'determined'),
                    ('narr', '메이린이 안쪽 캡슐을 향해 롤러를 굴린다.', None, 'A')])
d('c10_meilin_first', A, [('lu', '저쪽부터...!', 'neutral')])
d('c10_not_done', A, [('narr', '출구가 막혀 있다.')])
d('c10_aur_1', G, [('aurelius', '자리는 마련되어 있습니다. 저 사람들도, 당신의 친구도 함께 갑니다.', 'gentle'), ('lu', '그럼 아래 사람들은? 아직 못 올라왔잖아.', 'determined'),
                   ('aurelius', '지금 문을 닫으면 이곳의 모두가 기회를 잃습니다. 지금 멈출 수는 없습니다.', 'gentle'), ('lu', '길을 잃은 게 아니잖아. 네가...', 'angry_cry')])
d('c10_aur_reconnect', A, [('aurelius', '매개체를 다시 연결합니다. 함께 가면 됩니다.', 'determined'), ('narr', '장치에서 빛줄기가 루를 향해 뻗는다.')])
d('c10_raviel_hurt', A, [('raviel', '루한테 손대지 마!', 'pained'), ('narr', '라비엘이 빛줄기 앞을 막아선다. 충격에 쓰러진다.'), ('raviel', '...괜찮아. 연결은... 내가 붙잡고 있을게.', 'pained')])
d('c10_lu_cold', G, [('lu', '손 떼.', 'determined'), ('aurelius', '당신도 이 친구가 살아가길 바라지 않습니까.', 'gentle'), ('lu', '내 친구한테 손 떼라고.', 'determined'),
                     ('aurelius', '함께 갈 수 있습니다. 모두를 위해—', 'gentle'), ('lu', '그 입으로 모두라고 하지 마.', 'determined')])
d('c10_limit_1', A, [('narr', '루가 양손으로 전류를 붙잡는다. 장난스러운 자세가 사라진다.'), ('lu', '......하나.', 'determined')])
d('c10_limit_2', A, [('lu', '......둘.', 'pained')])
d('c10_limit_3', A, [('lu', '......셋.', 'pained')])
d('c10_limit_after_1', A, [('narr', '결합부 외피가 터져 나간다. 루의 손끝이 잠시 빛을 잃는다.'), ('aurelius', '그 몸으로는 오래 못 버팁니다. 함께 가면—', 'pained')])
d('c10_limit_after_2', A, [('narr', '축전부가 꺼진다. 루의 숨이 짧아진다.'), ('aurelius', '여러분, 조금만 더— 다음 생은 지금보다—', 'pained')])
d('c10_limit_after_3', A, [('narr', '과출력 장치의 결합부가 끊어진다.')])
d('c10_aur_last', G, [('aurelius', '다시... 다음에는...', 'pained')])
d('c10_aur_dead', A, [('narr', '외피와 몸체, 축전부의 불빛이 모두 꺼진다. 아우렐리우스는 더 이상 움직이지 않는다.')])
d('c10_meilin_last', A, [('meilin', '아직 하나 남았어... 다 적었는데... 다...', 'scared'), ('narr', '메이린이 마지막 캡슐의 수동 손잡이를 당긴다. 천장이 내려앉는다.')])
d('c10_capsules_dead', A, [('narr', '떨어진 접수 명부. 멈춘 롤러. 움직이지 않는 손.'), ('narr', "캡슐들의 불이 하나씩 꺼진다. 탑승 현황판: '통과 완료 0'."),
                           ('lu', '......라비엘. 가자.', 'sad')])
d('c10_floor_breaks', A, [('narr', '연결로 바닥이 통째로 꺼진다.'), ('lu', '라비엘—!', 'scared')])
d('c10_hold', A, [('narr', '라비엘이 괴물 토끼의 몸으로 루를 끌어안는다. 둘이 함께 떨어진다.')])

# ================================================================ 11장
d('c11_wake', G, [('lu', '...으아. 우리 착지 연습 좀 해야겠다.', 'pained'), ('lu', '라비엘, 나 잡았지? 봤어. 엄청 멋있었어.', 'smile'), ('raviel', '...괜찮아?', 'pained'),
                  ('lu', '응. 너 덕분에. 그러니까 너도 괜찮다고 해.', 'smile'), ('raviel', '미안해.', 'sad'), ('lu', '그건 아까 했고! 다음 대사!', 'smile')])
d('c11_breath', G, [('narr', '잠시 둘이 숨을 고른다. 루가 라비엘의 손을 찾아 잡는다.', None, 'A'), ('raviel', '네가... 날 버린 줄 알았어.', 'sad'),
                    ('lu', '난 너 좋아해. 예전에도, 지금도. 너 혼자 두고 어디 가겠어.', 'smile'), ('raviel', '...그러네. 넌 정말.', 'smile'), ('lu', '응. 나 정말이지 안 변하지?', 'smile'), ('raviel', '응.', 'smile')])
d('c11_route_flee', G, [('raviel', '이번엔 놓치지 않았네.', 'smile'), ('lu', '응. 꽉 잡았더라.', 'smile')])
d('c11_route_fight', G, [('raviel', '아까는... 아팠지.', 'sad'), ('lu', '그럼 다음엔 살살해. 약속.', 'smile')])
d('c11_food_1', A, [('lu', '나가면 붕어빵. 꼬리부터. 기억하지?', 'smile'), ('raviel', '...팥으로.', 'smile')])
d('c11_food_2', A, [('lu', '나가면 떡볶이. 안 매운 거. 기억하지?', 'smile'), ('raviel', '...맵기는 네가.', 'smile')])
d('c11_food_3', A, [('lu', '나가면 초코바. 반씩. 큰 쪽은 너.', 'smile'), ('raviel', '...응.', 'smile')])
d('c11_cup', A, [('lu', '그 컵, 손잡이 없는 거. 같이 쓰기로 했잖아.', 'smile')])
d('c11_hand_loose', G, [('narr', '라비엘의 손에서 힘이 빠진다.', None, 'A'), ('narr', '루가 잠깐 웃는 얼굴로 기다리다 다시 부른다.', None, 'A'), ('lu', '...다음 대사. 아직 남았는데.', 'sad')])
d('c11_no_answer', A, [('narr', '대답은 없다.')])
d('c11_hand', A, [('narr', '루가 라비엘의 손을 다시 잡는다. 따뜻함이 천천히 식어 간다.')])
d('c11_name', A, [('lu', '라비엘.', 'sad'), ('narr', '이름이 무너진 중정에 작게 울린다.')])
d('c11_eyes', A, [('narr', '루가 라비엘의 눈을 가만히 감겨 준다.'), ('lu', '...잘 자.', 'sad')])
d('c11_stand', A, [('narr', '루가 일어선다. 다리에 힘이 잘 들어가지 않는다.')])
d('c11_not_yet', A, [('narr', '...아직은.')])
d('c11_recovery', A, [('narr', '잔해 너머에서 무인 회수 장치가 예비 전원으로 다가온다. 안에 사람은 없다.'),
                      ('narr', '전선 경로는 끊겼다. 루의 몸은 형태 변환을 유지하지 못한다.')])
d('c11_last_lines', G, [('lu', '...또 가져가게는 안 할게.', 'determined'), ('lu', '나 여기 있어. 이번엔 어디 안 가.', 'sad')])
d('c11_release_ask', A, [('narr', '남은 전기가 손끝에 모인다.')], ['남은 전기를 모두 놓는다'])
d('c11_mem_venelle', A, [('narr', '베넬을 부축하던 바닥.')])
d('c11_mem_008', A, [('narr', '두 손으로 과자를 받던 008.')])
d('c11_mem_f', A, [('narr', 'F 가 툭 던진 빈 상자.')])
d('c11_mem_clothes', A, [('narr', '세탁실에 나란히 걸린 옷 두 벌.')])
d('c11_mem_raviel', A, [('narr', '라비엘과 나눈 쓸데없는 이야기.')])

# ================================================================ 말풍선: 루·라비엘 동행 (GDD 7.4)
bub('b_r_first', 'r_first', 'raviel', [('lu', '나란히 걸으니까 좋네!'), ('raviel', '복도 좁아.'), ('lu', '그럼 약간 나란히!')])
bub('b_r_lowdoor', 'r_lowdoor', 'raviel', [('lu', '그 몸으로 문 통과 돼?'), ('raviel', '변신 풀면 돼.'), ('lu', '아. 너무 정답이네.')])
bub('b_r_battery', 'r_battery', 'raviel', [('lu', '쟤만 안 꺼졌어!'), ('raviel', '예비 전원.'), ('lu', '도시락 챙긴 컴퓨터네.'), ('raviel', '...그렇게 이해해.')])
bub('b_r_safe', 'safe_room', 'raviel', [('lu', '나 먼저 자면 깨워줄 거지?'), ('raviel', '응.'), ('lu', '대답 빠르네. 좋다!')])
bub('b_r_cup', 'bridge1', 'raviel', [('lu', '네 방 컵 두 개더라.'), ('raviel', '하나는 씻기 귀찮아서.'), ('lu', '둘 다 깨끗하던데~'), ('raviel', '앞 봐.')])
bub('b_r_photo', 'bridge2', 'raviel', [('lu', '그 사진 나 웃기게 나왔지?'), ('raviel', '원래 그렇게 웃었어.'), ('lu', '지금도?'), ('raviel', '응.')])
bub('b_r_close', 'bridge3', 'raviel', [('raviel', '왜 자꾸 붙어.'), ('lu', '오랜만이잖아.'), ('raviel', '...발은 밟지 마.')])
bub('b_r_wire', 'walk', 'raviel', [('lu', '이 줄 타면 돼?'), ('raviel', '응. 끝에 불 들어온 데로 나와.'), ('lu', '좋아! 먼저 가서 멋진 척하고 있을게!')])
bub('b_r_share', 'walk', 'raviel', [('lu', '반씩 먹자.'), ('raviel', '너 다 먹어.'), ('lu', '그럼 네 반도 내가 맡아둘게. 나중에 받아가.')])
bub('b_r_lost', 'walk', 'raviel', [('raviel', '아까 온 길이야.'), ('lu', '복습했어!'), ('raviel', '참 성실하네.')])
bub('b_r_door', 'walk', 'raviel', [('lu', '한 번 터뜨려 볼까?'), ('raviel', '내가 열고 있잖아.'), ('lu', '미리 의견 냈어!')])
bub('b_r_win', 'walk', 'raviel', [('lu', '우리 좀 잘 맞는다!'), ('raviel', '내가 맞추는 거야.'), ('lu', '고마워! 앞으로도 잘 부탁해!')])
bub('b_r_charge', 'idle', 'raviel', [('raviel', '손 내려. 화면 떨려.'), ('lu', '나도 떨려. 서로 이해하자.')])

DOCS = [
    ('D10', '수용동 명부', "수용동 B 구역 — 접수 대기 인원.\n\n번호 / 접수일 / 행선지\n031  …  재개조 대기\n044  …  승선 명부 이관\n058  …  재개조 대기\n\n(손글씨) 행선지 칸이 두 가지뿐이다."),
    ('D11', '상담 기록 — 접수 담당 M', "상담자: M (접수·모집 창구)\n\n과거: 유곽에서 허드렛일. 도망 후 다시 붙잡힘. 개조 동의서는 본인 서명이 아님.\n현재: 수용동 통제·탑승 명부 담당. '빠진 사람 없이 다 적겠다'고 반복함.\n\n상담원 메모: 이 사람은 문 밖이 어디인지 묻지 않았다."),
    ('D12', '안전 검토서 — 연구주임 R', "경계 전이 장치 출력 검토.\n\n1. 예열 출력은 경계 안정도 기준을 넘기 전까지 올리지 말 것.\n2. 냉각 한계 초과 시 지하 지지 구조·관로에 하중 집중.\n3. 강제 가동은 B2 구역 붕괴 위험이 있음.\n\n결론: 현재 상태로는 단체 전이를 승인할 수 없음. — R"),
    ('D13', '에드워드의 녹음 목록', "001 극장 리허설 — 첫 소절\n002 비 오는 날 매표소 지붕\n003 TV 노래 (어릴 때 것, 다시 찾음)\n004 박수 — 객석 반쯤\n…\n(마지막 줄) 다음에 녹음할 것: "),
    ('D14', '엑소더스 배치도', "1차 도착 구역 — 거점 확보 (호위대)\n2차 — 생활 구역 (식당·수리점·합창단)\n3차 — 주변 지역 편입\n\n지휘: 총재 → 모집 담당(복수) → 구역 대장\n탑승 예정: 64명. 추가 증원 계획 첨부."),
    ('D15', '모집 승인 대장 (사본)', "모집 담당별 승인 기록.\n\nM — 수용동·접수 (승인 다수)\nL — 공연·방송 (승인 다수)\n기타 담당 4명\n\n최종 승인: 총재 아우렐리우스 (전원)\n'한 명만' 승인한 기록은 없다."),
    ('D16', '전이 장치 정비도', "전이 장치 구조: 외피 / 호위 장치 / 축전 모듈 둘 / 냉각관 / 전이 결합부.\n\n- 호위 장치는 강자성 부품. 정비용 전자석으로 고정 가능.\n- 축전 모듈 과부하 시 결합부 보호가 풀림.\n- 냉각 밸브를 닫으면 다음 충전이 과열로 중단.\n- 결합부는 대출력으로만 분리 가능."),
]


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
            if e and s not in ('narr', 'sys', 'warden', 'survivor'): o['e'] = e
            if e and s == 'survivor': o['e'] = e
            if (src == A and flag != 'G') or flag == 'A': o['added'] = True
            L.append(o)
        entry = {'id': id, 'src': src, 'lines': L}
        if choices: entry['choices'] = choices
        if id in ids: data['dialogues'][ids[id]] = entry
        else: data['dialogues'].append(entry); made += 1
    bids = {x['id']: k for k, x in enumerate(data['bubbles'])}
    for b in B:
        o = {k: v for k, v in b.items() if v is not None}
        if b['id'] in bids: data['bubbles'][bids[b['id']]] = o
        else: data['bubbles'].append(o)
    dids = {x['id']: k for k, x in enumerate(data['docs'])}
    for i, t, b in DOCS:
        e = {'id': i, 'title': t, 'added': True, 'body': b}
        if i in dids: data['docs'][dids[i]] = e
        else: data['docs'].append(e)
    with io.open(PATH, 'w', encoding='utf-8', newline='\r\n') as f:
        json.dump(data, f, ensure_ascii=False, indent=1)
    print('new dialogues', made, 'total', len(E), 'bubbles', len(B), 'docs', len(DOCS))


if __name__ == '__main__':
    main()
