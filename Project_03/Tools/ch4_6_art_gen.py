# -*- coding: utf-8 -*-
"""
ELECTRIC WORLD: LAST LIGHT - 4~6장(승강장 이후) 임시 도트 생성기.

기존 임시 그림(placeholder_gen.py / scene_art_gen.py / stealth_art_gen.py)과 같은 규격·같은 그림체로
'새 이름의 파일만' 만든다. 기존 PNG·메타는 다시 쓰지 않는다 (덮어쓰기 검사 포함).
- 캐릭터 48x64 프레임, 하단 중앙 피벗, 32 PPU (EWArtSpecs 규칙과 일치).
- 소품은 Assets/Game/Art/Placeholder/Scene/U_*.png (개별 스프라이트, 통짜 배경 아님).
- 최종 아트가 아니다. 필요한 최종 그림 목록: Docs/ART_REQUESTS.md §0.1.

실행: python Tools/ch4_6_art_gen.py   (Pillow 필요)
"""
import os, sys, io, random
from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
OUT = os.path.join(ROOT, 'Assets', 'Game', 'Art', 'Placeholder')
SCENE = os.path.join(OUT, 'Scene')
REPLACE_OK = '--force' in sys.argv     # 이 스크립트가 만든 파일만 다시 만든다 (기존 원본은 절대 아님)
MANIFEST = os.path.join(HERE, 'ch4_6_art_manifest.txt')
made = []
owned = set()
if os.path.exists(MANIFEST):
    owned = set(l.strip() for l in io.open(MANIFEST, encoding='utf-8') if l.strip())

# ---- placeholder_gen.py 의 그림 함수만 가져온다 (모듈 끝의 생성 호출은 실행하지 않음)
src = io.open(os.path.join(HERE, 'placeholder_gen.py'), encoding='utf-8').read()
cut = src.index("lu = Pal(")
ns = {'__file__': os.path.join(HERE, 'placeholder_gen.py')}
exec(compile(src[:cut], 'placeholder_gen_defs', 'exec'), ns)
figure, Pal, outline, strip, shade, text = ns['figure'], ns['Pal'], ns['outline'], ns['strip'], ns['shade'], ns['text']
spark_dot = ns['spark_dot']
CLEAR = (0, 0, 0, 0)
OUTLINE = ns['OUTLINE']
SKIN = ns['SKIN']


def guard_path(path):
    rel = os.path.relpath(path, ROOT).replace('\\', '/')
    if os.path.exists(path) and rel not in owned:
        raise SystemExit('기존 파일을 덮어쓰지 않습니다: ' + rel)
    if os.path.exists(path) and not REPLACE_OK:
        return None
    made.append(rel)
    return path


def save_char(name, img):
    p = guard_path(os.path.join(OUT, name + '.png'))
    if p: img.save(p)


def save_scene(name, img):
    p = guard_path(os.path.join(SCENE, name + '.png'))
    if p: img.save(p)


def shrink(img, k):
    """어린이: 같은 그림체를 줄여 아래 가운데에 붙인다 (임시 — 최종 아동 도트는 따로 필요)."""
    w, h = img.size
    sm = img.resize((int(w * k), int(h * k)), Image.NEAREST)
    out = Image.new('RGBA', (w, h), CLEAR)
    out.paste(sm, ((w - sm.size[0]) // 2, h - sm.size[1]), sm)
    return out


def make(name, p, sets, scale=1.0, fx=None):
    for state, poses in sets.items():
        frames = []
        for po in poses:
            po = dict(po)
            f_fx = po.pop('_fx', None)
            f = figure(p, po)
            if scale != 1.0: f = shrink(f, scale)
            if f_fx: f_fx(f)
            frames.append(f)
        save_char('%s_%s' % (name, state), strip(frames))


WALK = [dict(lx=3, rx=-3, ly=-1), dict(body=-1), dict(lx=-3, rx=3, ry=-1), dict(body=-1)]
BASE = {
    'Idle': [dict(), dict(body=1)],
    'Walk': [dict(w) for w in WALK],
    'Hurt': [dict(lean=-2, eyes='x', arm_f=(-3, 8), arm_b=(-7, 4))],
    'Sit': [dict(sit=True, arm_f=(4, 8))],
}


def box_fx(f, x=36, y=38):
    d = ImageDraw.Draw(f)
    d.rectangle([x - 4, y - 4, x + 4, y + 3], fill=(176, 140, 92, 255), outline=OUTLINE)
    d.line([x - 4, y - 1, x + 4, y - 1], fill=(130, 100, 64, 255))


def pipe_fx(f, x=38, y=24):
    d = ImageDraw.Draw(f)
    d.line([x - 6, y + 14, x + 2, y - 2], fill=(120, 124, 132, 255), width=2)
    d.point([(x + 2, y - 2)], fill=(200, 200, 210, 255))


def snack_fx(f, x=33, y=40):
    d = ImageDraw.Draw(f)
    d.rectangle([x - 3, y - 2, x + 3, y + 2], fill=(236, 98, 70, 255), outline=OUTLINE)
    d.point([(x, y)], fill=(255, 230, 150, 255))


def hair_up_fx(f):
    """루: 전기를 쓴 뒤 머리가 위로 곤두섬 (머리 위 가시 + 작은 불꽃)."""
    d = ImageDraw.Draw(f)
    hair = (250, 214, 90, 255)
    for k, x in enumerate((17, 21, 25, 29, 33)):
        h = 9 + (k % 2) * 4
        d.polygon([(x - 2, 17), (x + 2, 17), (x, 17 - h)], fill=hair, outline=OUTLINE)
    spark_dot(f, 14, 6, 1); spark_dot(f, 36, 4, 1)


def comb_fx(metal):
    def fx(f):
        d = ImageDraw.Draw(f)
        x, y = 37, 30
        d.rectangle([x - 3, y - 1, x + 3, y + 1], fill=(190, 196, 210, 255) if metal else (220, 120, 160, 255), outline=OUTLINE)
        for k in range(4): d.point([(x - 3 + k * 2, y + 2)], fill=(160, 168, 180, 255))
        if metal: d.point([(x + 4, y - 3)], fill=(255, 255, 255, 255))
    return fx


def lines_fx(n):
    def fx(f):
        d = ImageDraw.Draw(f)
        for k in range(n): d.line([30 + k * 2, 62, 30 + k * 2, 59], fill=(240, 240, 230, 255))
    return fx


def main():
    random.seed(4708)
    # ---------------- F (폐기소의 할머니): 회색 묶은 머리, 덧댄 갈색 외투, 번호표 'F'
    f = Pal((190, 190, 196, 255), (112, 86, 66, 255), 'F', coat=False)
    make('F', f, dict(BASE, **{
        'Talk': [dict(arm_f=(6, 2)), dict(arm_f=(7, -1), body=-1)],
        'Point': [dict(arm_f=(11, -2))],
        'Toss': [dict(arm_f=(-3, -12), lean=-1, _fx=lambda im: box_fx(im, 22, 20)), dict(arm_f=(12, -6), lean=2)],
        'Guard': [dict(arm_f=(8, -6), lean=-1, _fx=lambda im: pipe_fx(im))],
        'Tag': [dict(arm_f=(4, -8), head_tilt=1)],
    }))
    # ---------------- 008: 어린이 (같은 그림체를 0.78배로 줄인 임시)
    kid = Pal((70, 60, 58, 255), (150, 160, 120, 255), '08')
    make('Kid008', kid, dict(BASE, **{
        'Talk': [dict(arm_f=(6, 0)), dict(arm_f=(5, -3), body=-1)],
        'Hide': [dict(crouch=8, lean=-2, arm_f=(-2, -8), arm_b=(-4, -8), eyes='-')],
        'Eat': [dict(arm_f=(2, -8), _fx=lambda im: snack_fx(im, 30, 30)), dict(arm_f=(2, -9), body=1, _fx=lambda im: snack_fx(im, 30, 29))],
        'Hold': [dict(arm_f=(5, -2), _fx=lambda im: snack_fx(im, 33, 36))],
        'Draw': [dict(crouch=10, lean=3, arm_f=(8, 10), _fx=lines_fx(4)), dict(crouch=10, lean=3, arm_f=(10, 10), _fx=lines_fx(8))],
    }), scale=0.78)
    # ---------------- 폐기소의 겁먹은 생존자 둘 (파이프·고철을 든 일반인 — 연구원·경비가 아님)
    for nm, hair, cloth, label in (('SurvivorA', (60, 46, 40, 255), (96, 104, 80, 255), 'SA'), ('SurvivorB', (150, 120, 70, 255), (86, 92, 120, 255), 'SB')):
        p = Pal(hair, cloth, label)
        make(nm, p, dict(BASE, **{
            'Scared': [dict(lean=-2, arm_f=(4, -12), arm_b=(-4, -12))],
            'Guard': [dict(arm_f=(8, -6), lean=-1, _fx=lambda im: pipe_fx(im)), dict(arm_f=(8, -5), body=1, lean=-1, _fx=lambda im: pipe_fx(im, 38, 25))],
            'Windup': [dict(arm_f=(-2, -14), lean=-3, _fx=lambda im: pipe_fx(im, 26, 6))],
            'Strike': [dict(arm_f=(12, 2), lean=3, _fx=lambda im: pipe_fx(im, 44, 34))],
            'Toss': [dict(arm_f=(-4, -14), lean=-2), dict(arm_f=(11, -6), lean=2)],
            'Calm': [dict(sit=True, head_tilt=2, eyes='-', arm_f=(2, 9))],
            'Down': [dict(down=True)],
        }))
    # ---------------- 회상: 아이비(개조 전, 장치 없음), 린든(어린 동생), 어른 실루엣
    ivy = Pal((150, 112, 200, 255), (96, 84, 120, 255), 'IV', long_hair=True)
    make('Ivy', ivy, dict(BASE, **{
        'Carry': [dict(w, arm_f=(4, -4), arm_b=(4, -4), _fx=lambda im: box_fx(im, 30, 30)) for w in WALK],
        'Kneel': [dict(crouch=9, lean=2, arm_f=(8, 2))],
        'Give': [dict(arm_f=(10, 0), crouch=4, _fx=lambda im: snack_fx(im, 40, 34))],
        'Wave': [dict(arm_f=(4, -14)), dict(arm_f=(6, -12))],
    }))
    linden = Pal((150, 112, 200, 255), (220, 170, 80, 255), 'LN')
    make('Linden', linden, dict(BASE, **{
        'Hold': [dict(arm_f=(5, -2), _fx=lambda im: snack_fx(im, 33, 36))],
        'Wave': [dict(arm_f=(4, -14)), dict(arm_f=(6, -12))],
    }), scale=0.72)
    adult = Pal((38, 34, 44, 255), (52, 48, 60, 255), '')
    make('Adult', adult, {'Idle': [dict()], 'Walk': [dict(w) for w in WALK], 'Crutch': [dict(lean=2, arm_f=(6, 12), ly=-3)]})
    # ---------------- 루·베넬 추가 동작
    lu = Pal((250, 214, 90, 255), (70, 92, 140, 255), 'LU')
    make('Lu', lu, {
        'HairUp': [dict(arm_f=(5, -2), eyes='-', _fx=hair_up_fx), dict(arm_f=(6, -4), body=1, _fx=hair_up_fx)],
        'Share': [dict(arm_f=(9, -1), _fx=lambda im: snack_fx(im, 38, 34))],
        'HandsUp': [dict(arm_f=(3, -16), arm_b=(-3, -16), lean=-1), dict(arm_f=(3, -17), arm_b=(-3, -16), lean=-1, body=1)],
        'Wind': [dict(head_tilt=-2, eyes='-', arm_f=(3, 9)), dict(head_tilt=-2, eyes='-', arm_f=(3, 9), body=1)],
    })
    venelle = Pal((150, 110, 200, 255), (120, 62, 92, 255), 'VN', long_hair=True, device=True)
    make('Venelle', venelle, {
        'Comb': [dict(arm_f=(6, -10), _fx=comb_fx(True)), dict(arm_f=(2, 6))],
        'Wind': [dict(head_tilt=-2, eyes='-', arm_f=(3, 9)), dict(head_tilt=-2, eyes='-', arm_f=(3, 9), body=1)],
        'SitQuiet': [dict(sit=True, head_tilt=3, eyes='-', arm_f=(3, 8)), dict(sit=True, head_tilt=2, eyes='-', arm_f=(3, 8), body=1)],
        'Share': [dict(sit=True, arm_f=(9, -1), _fx=lambda im: snack_fx(im, 38, 36))],
    })
    scene_props()
    with io.open(MANIFEST, 'w', encoding='utf-8') as m:
        for r in sorted(set(made) | owned): m.write(r + '\n')
    print('made', len(made))


# =============================================================== 소품
def img(w, h): return Image.new('RGBA', (w, h), CLEAR)


def rect(d, b, c, o=OUTLINE): d.rectangle(b, fill=c, outline=o)


def scene_props():
    METAL = (92, 100, 110, 255); DARK = (46, 50, 58, 255); RUST = (140, 84, 52, 255); RED = (196, 60, 52, 255)
    # 화물 승강기 발판(96x14)과 승강로 틀(112x120), 상층 봉쇄 셔터(48x96)
    a = img(96, 14); d = ImageDraw.Draw(a)
    rect(d, [0, 0, 95, 6], METAL); rect(d, [2, 7, 93, 13], DARK)
    for x in range(4, 92, 8): d.line([x, 1, x + 3, 5], fill=(200, 170, 60, 255))
    save_scene('U_LiftDeck', a)
    a = img(112, 120); d = ImageDraw.Draw(a)
    for x in (2, 104): rect(d, [x, 0, x + 5, 119], (70, 76, 84, 255))
    for y in range(6, 118, 14): d.line([8, y, 103, y + 10], fill=(60, 64, 72, 200)); d.line([8, y + 10, 103, y], fill=(60, 64, 72, 200))
    save_scene('U_LiftShaft', a)
    a = img(48, 96); d = ImageDraw.Draw(a)
    rect(d, [0, 0, 47, 95], (88, 70, 64, 255))
    for y in range(4, 92, 8): d.line([2, y, 45, y], fill=(64, 50, 46, 255))
    for k in range(6): d.polygon([(k * 8, 40), (k * 8 + 4, 40), (k * 8 + 8, 52), (k * 8 + 4, 52)], fill=(220, 190, 60, 255))
    rect(d, [16, 60, 31, 70], RED); text(a, 18, 63, '!', (255, 255, 255, 255))
    save_scene('U_SealShutter', a)
    # 승강기 조작반 2프레임: 위(봉쇄, 빨강) / 아래(가능, 초록)
    fr = []
    for k in range(2):
        a = img(16, 32); d = ImageDraw.Draw(a)
        rect(d, [2, 2, 13, 29], (70, 74, 82, 255))
        d.ellipse([5, 6, 10, 11], fill=RED if k == 0 else (90, 70, 70, 255))
        d.ellipse([5, 18, 10, 23], fill=(80, 220, 120, 255) if k == 1 else (60, 120, 80, 255))
        fr.append(a)
    save_scene('U_LiftPanel', strip(fr))
    # 폐기소 거처: 고철 바리케이드, 방수포 지붕, 드럼통 난로(불 2프레임), 상자 침대, 물통(빈/찬), 벽의 날짜 금, 빨랫줄
    a = img(72, 44); d = ImageDraw.Draw(a)
    for i in range(14):
        x = random.randint(0, 60); y = random.randint(8, 34); w = random.randint(8, 18); h = random.randint(6, 12)
        rect(d, [x, y, min(71, x + w), min(43, y + h)], random.choice([METAL, RUST, (104, 96, 80, 255), (70, 80, 72, 255)]))
    rect(d, [0, 38, 71, 43], DARK)
    save_scene('U_Barricade', a)
    a = img(96, 20); d = ImageDraw.Draw(a)
    d.polygon([(0, 18), (48, 2), (95, 18)], fill=(64, 92, 112, 255), outline=OUTLINE)
    for x in range(8, 90, 12): d.line([x, 16, 48, 4], fill=(54, 80, 98, 255))
    save_scene('U_Tarp', a)
    fr = []
    for k in range(2):
        a = img(24, 30); d = ImageDraw.Draw(a)
        rect(d, [3, 10, 20, 29], (84, 70, 60, 255)); d.line([3, 16, 20, 16], fill=(60, 50, 44, 255))
        d.polygon([(6, 10), (12, 0 + k * 2), (18, 10)], fill=(250, 160, 60, 255)); d.polygon([(9, 10), (12, 4 + k), (15, 10)], fill=(255, 230, 120, 255))
        fr.append(a)
    save_scene('U_Stove', strip(fr))
    a = img(48, 20); d = ImageDraw.Draw(a)
    rect(d, [0, 6, 47, 19], (130, 104, 72, 255)); rect(d, [2, 2, 22, 8], (170, 160, 150, 255)); d.line([24, 6, 24, 19], fill=(100, 80, 56, 255))
    save_scene('U_CrateBed', a)
    fr = []
    for k in range(2):
        a = img(16, 24); d = ImageDraw.Draw(a)
        rect(d, [2, 4, 13, 23], (170, 200, 210, 160)); rect(d, [5, 0, 10, 4], (80, 120, 160, 255))
        if k == 1: rect(d, [3, 9, 12, 22], (70, 140, 220, 220), None)
        fr.append(a)
    save_scene('U_Jug', strip(fr))
    a = img(40, 24); d = ImageDraw.Draw(a)
    for g in range(5):
        for k in range(4): d.line([2 + g * 8 + k * 1.5, 4, 2 + g * 8 + k * 1.5, 14], fill=(210, 210, 200, 255))
        d.line([1 + g * 8, 12, 8 + g * 8, 6], fill=(210, 210, 200, 255))
    save_scene('U_Tally', a)
    a = img(80, 34); d = ImageDraw.Draw(a)
    d.line([0, 4, 79, 4], fill=(150, 140, 120, 255))
    for x, c in ((6, (190, 120, 100, 255)), (22, (120, 150, 170, 255)), (40, (150, 160, 110, 255)), (58, (200, 190, 170, 255))):
        rect(d, [x, 5, x + 12, 22], c)
    save_scene('U_LaundryLine', a)
    # 물 펌프실: 펌프(64x48, 4프레임: 고장 정지 / 정상 정지 / 정상 가동 2)
    fr = []
    for k in range(4):
        a = img(64, 48); d = ImageDraw.Draw(a)
        rect(d, [6, 18, 44, 47], (70, 96, 120, 255)); rect(d, [44, 26, 62, 40], (80, 86, 96, 255))
        d.ellipse([12, 4, 38, 30], fill=(86, 112, 136, 255), outline=OUTLINE)
        # 연결부(이음쇠) 위치 x 44
        if k == 0:
            rect(d, [42, 28, 48, 38], RUST); d.line([43, 30, 47, 36], fill=(40, 30, 30, 255)); d.line([47, 29, 43, 37], fill=(40, 30, 30, 255))
        else:
            rect(d, [42, 28, 48, 38], (210, 190, 90, 255))
        if k >= 2:
            ang = (k - 2) * 2
            d.line([25, 17, 25 + (6 if ang == 0 else -6), 17 + (6 if ang else -6)], fill=(230, 230, 240, 255), width=2)
            d.ellipse([22, 14, 28, 20], fill=(120, 230, 160, 255))
        else:
            d.line([25, 17, 31, 17], fill=(150, 150, 160, 255), width=2)
        fr.append(a)
    save_scene('U_Pump', strip(fr))
    # 배수 밸브 핸들 3프레임(회전), 차단기 2프레임(내림/올림), 이음쇠 부품(새것), 부품 선반
    fr = []
    for k in range(3):
        a = img(24, 32); d = ImageDraw.Draw(a)
        rect(d, [10, 14, 13, 31], METAL)
        d.ellipse([2, 2, 21, 21], outline=RED, width=3)
        import math
        for s in range(3):
            an = (k * 40 + s * 120) * math.pi / 180
            d.line([12, 12, 12 + 8 * math.cos(an), 12 + 8 * math.sin(an)], fill=RED, width=2)
        fr.append(a)
    save_scene('U_Valve', strip(fr))
    fr = []
    for k in range(2):
        a = img(24, 36); d = ImageDraw.Draw(a)
        rect(d, [1, 1, 22, 34], (72, 78, 86, 255)); rect(d, [7, 8, 16, 26], DARK)
        rect(d, [8, 9 if k == 1 else 19, 15, 16 if k == 1 else 25], (220, 220, 230, 255))
        d.ellipse([9, 28, 14, 33], fill=(90, 230, 120, 255) if k == 1 else (230, 70, 60, 255))
        fr.append(a)
    save_scene('U_Breaker', strip(fr))
    a = img(14, 12); d = ImageDraw.Draw(a); rect(d, [1, 2, 12, 9], (210, 190, 90, 255)); d.line([4, 2, 4, 9], fill=(150, 130, 60, 255)); d.line([9, 2, 9, 9], fill=(150, 130, 60, 255))
    save_scene('U_Part', a)
    a = img(40, 36); d = ImageDraw.Draw(a)
    for y in (10, 22, 34): rect(d, [0, y, 39, y + 2], METAL)
    rect(d, [3, 0, 5, 35], DARK); rect(d, [34, 0, 36, 35], DARK)
    rect(d, [10, 4, 18, 9], (120, 110, 100, 255)); rect(d, [22, 16, 30, 21], (100, 120, 110, 255))
    save_scene('U_PartShelf', a)
    # 물 표면(가로 반복 32x6 2프레임) + 물 몸통(채움), 관 흐름
    fr = []
    for k in range(2):
        a = img(32, 6); d = ImageDraw.Draw(a)
        rect(d, [0, 2, 31, 5], (60, 120, 190, 200), None)
        for x in range(k * 4, 32, 8): d.line([x, 1, x + 3, 1], fill=(170, 220, 255, 230))
        fr.append(a)
    save_scene('U_WaterTop', strip(fr))
    a = img(8, 8); ImageDraw.Draw(a).rectangle([0, 0, 7, 7], fill=(48, 100, 170, 190)); save_scene('U_WaterFill', a)
    a = img(16, 16); d = ImageDraw.Draw(a); rect(d, [0, 5, 15, 10], (110, 118, 128, 255)); d.line([0, 7, 15, 7], fill=(150, 156, 166, 255)); save_scene('U_PipeH', a)
    # 폐품 시장: 노점(64x48), 등불(2프레임: 꺼짐/켜짐), 분필 선 여덟 개(+긴 선)
    for nm, roof in (('U_StallA', (150, 70, 60, 255)), ('U_StallB', (70, 110, 130, 255))):
        a = img(64, 48); d = ImageDraw.Draw(a)
        d.polygon([(0, 14), (8, 2), (56, 2), (63, 14)], fill=roof, outline=OUTLINE)
        for x in (4, 58): rect(d, [x, 14, x + 2, 47], DARK)
        rect(d, [2, 32, 61, 40], (120, 96, 70, 255))
        for i in range(6): rect(d, [6 + i * 9, 26, 12 + i * 9, 31], random.choice([METAL, RUST, (180, 170, 90, 255), (110, 150, 120, 255)]))
        save_scene(nm, a)
    fr = []
    for k in range(2):
        a = img(12, 20); d = ImageDraw.Draw(a)
        d.line([6, 0, 6, 3], fill=DARK); rect(d, [2, 3, 9, 16], (230, 220, 150, 255) if k else (90, 90, 80, 255)); rect(d, [1, 16, 10, 18], DARK)
        fr.append(a)
    save_scene('U_Lantern', strip(fr))
    fr = []
    for k in range(2):
        a = img(48, 10); d = ImageDraw.Draw(a)
        for i in range(8): d.line([3 + i * 5, 2, 3 + i * 5, 9], fill=(235, 235, 225, 255))
        if k: d.line([43, 2, 47, 9], fill=(235, 235, 225, 255)); d.line([38, 9, 47, 9], fill=(235, 235, 225, 255))
        fr.append(a)
    save_scene('U_ChalkLines', strip(fr))
    # 정비동: 컨베이어 벨트(32x10 가로 반복 4프레임), 벨트 위 상자, 제어반(2프레임), 시험대, 접지 단자
    fr = []
    for k in range(4):
        a = img(32, 10); d = ImageDraw.Draw(a)
        rect(d, [0, 0, 31, 5], (54, 56, 62, 255)); rect(d, [0, 6, 31, 9], (80, 84, 92, 255))
        for x in range(-8 + k * 2, 32, 8): d.line([x, 1, x + 3, 4], fill=(110, 112, 120, 255))
        for x in (4, 16, 28): d.ellipse([x - 2, 6, x + 2, 9], fill=(40, 42, 48, 255))
        fr.append(a)
    save_scene('U_Belt', strip(fr))
    a = img(20, 16); d = ImageDraw.Draw(a); rect(d, [0, 0, 19, 15], (150, 120, 80, 255)); d.line([0, 5, 19, 5], fill=(120, 96, 64, 255)); save_scene('U_BeltCrate', a)
    fr = []
    for k in range(3):
        a = img(24, 32); d = ImageDraw.Draw(a)
        rect(d, [1, 1, 22, 30], (78, 84, 94, 255)); rect(d, [5, 5, 18, 14], (30, 60, 50, 255) if k < 2 else (20, 20, 24, 255))
        if k < 2: d.polygon([(8 + (k * 6), 9), (12 - k * 2 + k * 6, 6), (12 - k * 2 + k * 6, 12)], fill=(120, 240, 150, 255))
        d.ellipse([9, 19, 14, 24], fill=(230, 200, 70, 255) if k < 2 else (80, 70, 60, 255))
        fr.append(a)
    save_scene('U_BeltCtrl', strip(fr))      # 0 정방향 / 1 역방향 / 2 정지
    a = img(56, 22); d = ImageDraw.Draw(a)
    rect(d, [0, 12, 55, 21], (90, 96, 104, 255)); rect(d, [4, 6, 51, 12], (70, 76, 84, 255))
    for x in (10, 28, 46): d.line([x, 0, x, 6], fill=(230, 200, 70, 255))
    text(a, 20, 14, 'TEST', (240, 240, 230, 255)) if False else None
    save_scene('U_TestRig', a)
    # 숙소: 세탁기(2프레임), 같은 옷 두 벌, 다른 단원 옷, 침대, 책상, 사진, 편지, 포장지 열 장, 초코바 상자, 옥상 난간·물탱크
    fr = []
    for k in range(2):
        a = img(28, 32); d = ImageDraw.Draw(a)
        rect(d, [1, 1, 26, 31], (210, 214, 220, 255)); d.ellipse([6, 9, 21, 24], fill=(70, 110, 160, 255) if k else (90, 96, 110, 255), outline=OUTLINE)
        d.ellipse([20, 3, 23, 6], fill=(90, 220, 120, 255) if k else (90, 90, 90, 255))
        fr.append(a)
    save_scene('U_Washer', strip(fr))
    def outfit(d, x, c):
        d.polygon([(x, 6), (x + 14, 6), (x + 18, 12), (x + 15, 14), (x + 14, 34), (x, 34), (x - 1, 14), (x - 4, 12)], fill=c, outline=OUTLINE)
        d.line([x + 7, 6, x + 7, 34], fill=shade(c, 0.8))
        d.line([x + 7, 0, x + 7, 6], fill=(150, 150, 150, 255))
    a = img(64, 40); d = ImageDraw.Draw(a); d.line([0, 1, 63, 1], fill=(150, 140, 120, 255))
    outfit(d, 12, (120, 62, 92, 255)); outfit(d, 38, (120, 62, 92, 255))
    save_scene('U_TwinOutfits', a)
    a = img(64, 40); d = ImageDraw.Draw(a); d.line([0, 1, 63, 1], fill=(150, 140, 120, 255))
    outfit(d, 8, (90, 110, 90, 255)); outfit(d, 30, (170, 150, 110, 255)); outfit(d, 50, (100, 100, 130, 255))
    save_scene('U_OtherOutfits', a)
    a = img(56, 22); d = ImageDraw.Draw(a); rect(d, [0, 8, 55, 21], (110, 80, 70, 255)); rect(d, [2, 4, 18, 10], (220, 220, 210, 255)); rect(d, [18, 8, 54, 14], (150, 90, 120, 255)); save_scene('U_DormBed', a)
    a = img(40, 30); d = ImageDraw.Draw(a); rect(d, [0, 10, 39, 14], (120, 90, 64, 255)); rect(d, [2, 14, 5, 29], (90, 66, 48, 255)); rect(d, [34, 14, 37, 29], (90, 66, 48, 255)); rect(d, [24, 16, 36, 26], (110, 80, 58, 255)); save_scene('U_DormDesk', a)
    a = img(12, 14); d = ImageDraw.Draw(a); rect(d, [0, 0, 11, 13], (200, 170, 110, 255)); rect(d, [2, 2, 9, 10], (150, 170, 190, 255)); d.ellipse([3, 4, 6, 7], fill=SKIN); d.ellipse([6, 5, 8, 7], fill=SKIN); save_scene('U_Photo', a)
    a = img(14, 10); d = ImageDraw.Draw(a); rect(d, [0, 0, 13, 9], (236, 230, 210, 255)); [d.line([2, y, 11, y], fill=(150, 150, 170, 255)) for y in (3, 5, 7)]; save_scene('U_Letter', a)
    a = img(40, 8); d = ImageDraw.Draw(a)
    for i in range(10): rect(d, [i * 4, 3 + (i % 2), i * 4 + 3, 7], (236, 98, 70, 255) if i % 3 else (200, 80, 60, 255), None)
    save_scene('U_Wrappers', a)
    a = img(20, 10); d = ImageDraw.Draw(a); rect(d, [0, 2, 19, 9], (120, 70, 40, 255)); rect(d, [2, 0, 6, 2], (236, 98, 70, 255)); save_scene('U_ChocoBox', a)
    a = img(32, 14); d = ImageDraw.Draw(a); rect(d, [0, 2, 31, 4], METAL); [rect(d, [x, 4, x + 1, 13], METAL) for x in (1, 10, 20, 30)]; save_scene('U_Rail', a)
    a = img(48, 64); d = ImageDraw.Draw(a); rect(d, [6, 4, 41, 40], (110, 116, 124, 255)); d.ellipse([6, 0, 41, 10], fill=(130, 136, 144, 255), outline=OUTLINE); [rect(d, [x, 40, x + 2, 63], DARK) for x in (9, 36)]; save_scene('U_WaterTower', a)
    # 회상용 장면 소품 (세피아로 칠해 쓴다): 빈 침대·문·공장 상자·시약 체험 전단
    a = img(30, 40); d = ImageDraw.Draw(a); rect(d, [0, 0, 29, 39], (120, 96, 70, 255)); rect(d, [4, 4, 25, 39], (90, 70, 50, 255)); d.ellipse([20, 20, 23, 23], fill=(220, 200, 120, 255)); save_scene('U_HomeDoor', a)
    a = img(20, 26); d = ImageDraw.Draw(a); rect(d, [0, 0, 19, 25], (236, 230, 210, 255)); rect(d, [3, 3, 16, 9], (120, 200, 140, 255)); [d.line([3, y, 16, y], fill=(150, 150, 170, 255)) for y in (13, 16, 19, 22)]; save_scene('U_TrialFlyer', a)


if __name__ == '__main__':
    main()
