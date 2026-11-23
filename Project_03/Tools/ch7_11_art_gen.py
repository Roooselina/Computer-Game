# -*- coding: utf-8 -*-
"""
ELECTRIC WORLD: LAST LIGHT - 7~11장·엔딩 임시 도트 생성기 (2026-11-15).

ch4_6_art_gen.py 와 같은 규칙: 기존 임시 그림 함수(placeholder_gen.py)의 그림체로 '새 이름의 파일만' 만든다.
기존 PNG·메타는 다시 쓰지 않는다 (이 스크립트의 매니페스트에 없는 기존 파일이면 중단).
- 사람 크기 인물 48x64 프레임, 하단 중앙 피벗, 32 PPU.
- 괴물 토끼 라비엘 80x96, 아우렐리우스 64x96 (EWArtSpecs 의 접두어 규칙과 일치).
- 소품은 Assets/Game/Art/Placeholder/Scene/U_*.png (개별 스프라이트, 통짜 배경 아님).
- 최종 아트가 아니다. 최종 그림 목록: Docs/ART_REQUESTS.md §0.000.

실행: python Tools/ch7_11_art_gen.py   (Pillow 필요)
"""
import os, sys, io, random
from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
OUT = os.path.join(ROOT, 'Assets', 'Game', 'Art', 'Placeholder')
SCENE = os.path.join(OUT, 'Scene')
REPLACE_OK = '--force' in sys.argv
MANIFEST = os.path.join(HERE, 'ch7_11_art_manifest.txt')
made = []
owned = set()
if os.path.exists(MANIFEST):
    owned = set(l.strip() for l in io.open(MANIFEST, encoding='utf-8') if l.strip())

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


def save_char(name, im):
    p = guard_path(os.path.join(OUT, name + '.png'))
    if p: im.save(p)


def save_scene(name, im):
    p = guard_path(os.path.join(SCENE, name + '.png'))
    if p: im.save(p)


def img(w, h): return Image.new('RGBA', (w, h), CLEAR)
def rect(d, b, c, o=OUTLINE): d.rectangle(b, fill=c, outline=o)


def shrink(im, k):
    w, h = im.size
    sm = im.resize((int(w * k), int(h * k)), Image.NEAREST)
    out = Image.new('RGBA', (w, h), CLEAR)
    out.paste(sm, ((w - sm.size[0]) // 2, h - sm.size[1]), sm)
    return out


def make(name, p, sets, scale=1.0):
    for state, poses in sets.items():
        frames = []
        for po in poses:
            po = dict(po)
            f_fx = po.pop('_fx', None)
            f = figure(p, po)
            if scale != 1.0: f = shrink(f, scale)
            if f_fx:
                for fx in (f_fx if isinstance(f_fx, (list, tuple)) else [f_fx]): fx(f)
            frames.append(f)
        save_char('%s_%s' % (name, state), strip(frames))


WALK = [dict(lx=3, rx=-3, ly=-1), dict(body=-1), dict(lx=-3, rx=3, ry=-1), dict(body=-1)]
BASE = {
    'Idle': [dict(), dict(body=1)],
    'Walk': [dict(w) for w in WALK],
    'Hurt': [dict(lean=-2, eyes='x', arm_f=(-3, 8), arm_b=(-7, 4))],
    'Sit': [dict(sit=True, arm_f=(4, 8))],
    'Down': [dict(down=True)],
}


# ------------------------------------------------------------------ 덧그림 효과
def needles_fx(f):
    """메이린: 머리의 바늘꽂이 장식 + 꽂힌 바늘, 얼굴 반쪽의 꿰맨 선."""
    d = ImageDraw.Draw(f)
    d.ellipse([18, 9, 26, 15], fill=(196, 52, 60, 255), outline=OUTLINE)
    for x, y in ((19, 4), (22, 3), (25, 5)): d.line([22, 12, x, y], fill=(220, 220, 230, 255))
    for y in range(22, 32, 2): d.point([(28, y)], fill=(120, 60, 70, 255))
    d.line([27, 22, 29, 31], fill=(150, 80, 90, 255))


def braid_fx(f):
    d = ImageDraw.Draw(f)
    for y in range(24, 46, 3): d.ellipse([11, y, 15, y + 3], fill=(30, 26, 34, 255), outline=OUTLINE)


def roller_fx(f):
    d = ImageDraw.Draw(f)
    for x in (19, 26): d.ellipse([x - 1, 60, x + 3, 63], fill=(70, 70, 80, 255), outline=OUTLINE)


def scissors_fx(open_=True):
    def fx(f):
        d = ImageDraw.Draw(f)
        x, y = 38, 34
        if open_:
            d.line([x, y, x + 7, y - 4], fill=(230, 230, 240, 255)); d.line([x, y, x + 7, y + 3], fill=(230, 230, 240, 255))
        else:
            d.line([x, y, x + 8, y], fill=(230, 230, 240, 255))
        d.ellipse([x - 3, y - 2, x, y + 1], outline=(200, 160, 60, 255))
    return fx


def throat_fx(on=True):
    def fx(f):
        d = ImageDraw.Draw(f)
        d.rectangle([24, 30, 30, 34], fill=(70, 80, 110, 255), outline=OUTLINE)
        if on: d.point([(26, 32), (28, 32)], fill=(150, 220, 255, 255))
    return fx


def waves_fx(k):
    def fx(f):
        d = ImageDraw.Draw(f)
        for r in range(3):
            rr = 4 + r * 4 + k * 2
            d.arc([30 - rr // 2, 30 - rr, 30 + rr, 30 + rr], -50, 50, fill=(150, 210, 255, 200 - r * 50))
    return fx


def tag_fx(f):
    d = ImageDraw.Draw(f)
    d.rectangle([20, 40, 26, 44], fill=(230, 220, 170, 255), outline=OUTLINE)


def headset_fx(f):
    d = ImageDraw.Draw(f)
    d.arc([14, 14, 34, 32], 180, 360, fill=(60, 60, 70, 255))
    d.rectangle([31, 22, 34, 27], fill=(60, 60, 70, 255))


def bandage_fx(f):
    d = ImageDraw.Draw(f)
    d.line([16, 40, 30, 46], fill=(235, 235, 235, 255), width=2)
    d.point([(24, 43), (25, 44)], fill=(170, 50, 50, 255))


def cup_fx(f):
    d = ImageDraw.Draw(f)
    d.rectangle([34, 32, 38, 37], fill=(230, 230, 236, 255), outline=OUTLINE)


def pen_fx(f):
    d = ImageDraw.Draw(f)
    d.line([34, 38, 40, 34], fill=(60, 60, 70, 255))
    d.rectangle([30, 39, 38, 43], fill=(240, 236, 220, 255), outline=OUTLINE)


def tv_fx(f):
    d = ImageDraw.Draw(f)
    d.rectangle([34, 40, 46, 50], fill=(60, 64, 80, 255), outline=OUTLINE)
    d.rectangle([36, 42, 44, 48], fill=(150, 200, 230, 255))


# ------------------------------------------------------------------ 괴물 토끼 라비엘 (80x96)
def beast(pose):
    W, H = 80, 96
    im = img(W, H); d = ImageDraw.Draw(im)
    FUR = (214, 206, 218, 255); FUR2 = (170, 160, 178, 255); BELLY = (236, 230, 236, 255)
    crouch = pose.get('crouch', 0); lean = pose.get('lean', 0); arm = pose.get('arm', 0); air = pose.get('air', 0)
    step = pose.get('step', 0); hurt = pose.get('hurt', False); hold = pose.get('hold', False)
    by = 30 + crouch - air
    cx = 38 + lean
    # 다리 (굵은 뒷다리)
    leg_y = H - 1 - air
    d.ellipse([cx - 22 + step, by + 34, cx - 4 + step, leg_y], fill=FUR2)
    d.ellipse([cx + 2 - step, by + 34, cx + 20 - step, leg_y], fill=FUR2)
    d.rectangle([cx - 24 + step, leg_y - 4, cx - 6 + step, leg_y], fill=FUR)
    d.rectangle([cx + 4 - step, leg_y - 4, cx + 22 - step, leg_y], fill=FUR)
    # 몸통 (근육질)
    d.ellipse([cx - 24, by + 6, cx + 22, by + 50], fill=FUR)
    d.ellipse([cx - 14, by + 16, cx + 12, by + 46], fill=BELLY)
    d.line([cx - 4, by + 22, cx - 4, by + 40], fill=FUR2)
    # 팔
    if hold:
        d.ellipse([cx - 4, by + 18, cx + 26, by + 34], fill=FUR2)
    else:
        ax = cx + 18 + arm; ay = by + 18 - arm
        d.line([cx + 14, by + 16, ax, ay + 18], fill=FUR2, width=8)
        d.ellipse([ax - 5, ay + 14, ax + 7, ay + 24], fill=FUR)
        d.line([cx - 16, by + 16, cx - 20, by + 34], fill=FUR2, width=7)
    # 머리 + 짧은 귀 (귀가 없던 라비엘이 개조 뒤 변신한 모습: 귀는 짧고 비대칭)
    hx, hy = cx + 6 + lean // 2, by - 14
    d.ellipse([hx - 14, hy, hx + 14, hy + 24], fill=FUR)
    d.polygon([(hx - 8, hy + 2), (hx - 12, hy - 10), (hx - 4, hy - 2)], fill=FUR2)
    d.polygon([(hx + 4, hy + 1), (hx + 6, hy - 6), (hx + 9, hy + 1)], fill=FUR2)
    ey = hy + 10
    if hurt: d.line([hx + 2, ey, hx + 6, ey + 2], fill=OUTLINE); d.line([hx + 2, ey + 2, hx + 6, ey], fill=OUTLINE)
    else: d.rectangle([hx + 3, ey - 1, hx + 6, ey + 2], fill=(200, 40, 70, 255))
    d.line([hx + 8, hy + 17, hx + 12, hy + 17], fill=OUTLINE)
    # 연구복 조각 (몸에 걸린 찢어진 흰 천)
    d.polygon([(cx - 20, by + 10), (cx - 6, by + 8), (cx - 12, by + 26)], fill=(236, 236, 244, 255))
    out = outline(im)
    if pose.get('glow'):
        g = Image.new('RGBA', out.size, CLEAR); gd = ImageDraw.Draw(g)
        gd.ellipse([cx - 30, by - 20, cx + 30, H - 2], outline=pose['glow'], width=2)
        g.alpha_composite(out); out = g
    return out


def beast_strip(name, poses):
    save_char('RavielBeast_' + name, strip([beast(p) for p in poses]))


# ------------------------------------------------------------------ 아우렐리우스 (64x96): 아름다운 겉껍데기 + 드러나는 내부 기계
def aurelius(pose):
    W, H = 64, 96
    im = img(W, H); d = ImageDraw.Draw(im)
    SHELL = (238, 228, 210, 255); GOLD = (214, 180, 96, 255); ROBE = (60, 54, 88, 255); MECH = (90, 96, 108, 255)
    broken = pose.get('broken', 0); arm = pose.get('arm', 0); lean = pose.get('lean', 0); glow = pose.get('glow')
    cx = 32 + lean
    # 긴 예복
    d.polygon([(cx - 14, 40), (cx + 14, 40), (cx + 20, H - 1), (cx - 20, H - 1)], fill=ROBE)
    d.line([cx, 42, cx, H - 2], fill=GOLD)
    # 가슴·어깨 외피
    d.ellipse([cx - 16, 26, cx + 16, 52], fill=SHELL)
    d.arc([cx - 12, 30, cx + 12, 50], 200, 340, fill=GOLD)
    # 팔 (설득하듯 펼침)
    d.line([cx + 12, 34, cx + 22 + arm, 48 - arm * 2], fill=SHELL, width=4)
    d.line([cx - 12, 34, cx - 22, 50], fill=SHELL, width=4)
    # 얼굴: 매끈한 가면 같은 얼굴, 금빛 둘레
    d.ellipse([cx - 10, 6, cx + 10, 30], fill=SHELL, outline=GOLD)
    d.line([cx - 4, 17, cx - 1, 17], fill=(90, 80, 70, 255)); d.line([cx + 3, 17, cx + 6, 17], fill=(90, 80, 70, 255))
    d.chord([cx - 10, 5, cx + 10, 25], 180, 360, fill=(232, 214, 160, 255))   # 매끈한 금빛 머리 (후광처럼 보이지 않게 머리에 붙임)
    d.rectangle([cx - 11, 12, cx - 8, 34], fill=(232, 214, 160, 255))
    if broken >= 1:   # 외피가 깨지고 내부 기계가 보임
        d.polygon([(cx - 2, 28), (cx + 12, 32), (cx + 6, 46), (cx - 4, 40)], fill=MECH)
        for y in range(32, 44, 3): d.line([cx, y, cx + 8, y + 1], fill=(160, 170, 180, 255))
        d.point([(cx + 4, 36)], fill=(255, 120, 80, 255))
    if broken >= 2:
        d.polygon([(cx - 10, 8), (cx + 2, 6), (cx - 2, 22), (cx - 10, 20)], fill=MECH)
        d.point([(cx - 5, 14)], fill=(255, 120, 80, 255))
        d.line([cx - 16, 50, cx + 16, 60], fill=(40, 36, 44, 255), width=2)
    out = outline(im)
    if glow:
        g = Image.new('RGBA', out.size, CLEAR); gd = ImageDraw.Draw(g)
        gd.ellipse([cx - 26, 4, cx + 26, 70], outline=glow, width=2)
        g.alpha_composite(out); out = g
    return out


def aur_strip(name, poses):
    save_char('Aurelius_' + name, strip([aurelius(p) for p in poses]))


def characters():
    # ---------------- 라비엘 (사람 모습): 귀 없는 토끼 악마, 연구주임 흰 가운, 표식 'R'
    rav = Pal((226, 222, 236, 255), (80, 88, 110, 255), 'R', coat=True)
    make('Raviel', rav, dict(BASE, **{
        'Talk': [dict(arm_f=(6, 2)), dict(arm_f=(7, -1), body=-1)],
        'Point': [dict(arm_f=(11, -2))],
        'Type': [dict(arm_f=(9, 4), lean=1), dict(arm_f=(10, 3), lean=1, body=1)],
        'HoldHand': [dict(arm_f=(8, 6))],
        'Catch': [dict(arm_f=(10, -6), arm_b=(8, -4), lean=2)],
        'Injured': [dict(sit=True, head_tilt=3, eyes='-', arm_f=(2, 10), _fx=bandage_fx), dict(sit=True, head_tilt=2, eyes='-', arm_f=(2, 10), body=1, _fx=bandage_fx)],
        'Cup': [dict(arm_f=(6, 0), _fx=cup_fx)],
        'Sad': [dict(head_tilt=3, eyes='-', arm_f=(2, 10))],
    }))
    # ---------------- 메이린: 도자기 인형 같은 접수원, 붉은 치파오, 땋은 머리, 바늘꽂이, 롤러 발
    mei = Pal((30, 26, 34, 255), (178, 40, 52, 255), 'M', long_hair=True)
    fx_base = [needles_fx, braid_fx, roller_fx]
    make('Meilin', mei, {
        'Idle': [dict(_fx=fx_base), dict(body=1, _fx=fx_base)],
        'Walk': [dict(w, _fx=fx_base) for w in WALK],
        'Roll': [dict(lean=3, arm_b=(-8, 2), _fx=fx_base), dict(lean=3, arm_b=(-9, 1), body=1, _fx=fx_base)],
        'Windup': [dict(lean=-2, arm_f=(-2, -12), _fx=fx_base + [scissors_fx(True)])],
        'Snip': [dict(lean=3, arm_f=(12, 0), _fx=fx_base + [scissors_fx(False)])],
        'Dash': [dict(lean=4, arm_f=(10, -2), arm_b=(-10, 2), _fx=fx_base + [scissors_fx(True)])],
        'Stun': [dict(lean=-3, eyes='x', arm_f=(-2, 10), _fx=fx_base), dict(lean=-2, eyes='x', arm_f=(-3, 9), body=1, _fx=fx_base)],
        'Hurt': [dict(lean=-2, eyes='x', arm_f=(-3, 8), _fx=fx_base)],
        'Talk': [dict(arm_f=(6, 2), _fx=fx_base), dict(arm_f=(7, -2), _fx=fx_base)],
        'Angry': [dict(arm_f=(4, -14), arm_b=(-4, -14), _fx=fx_base), dict(arm_f=(5, -13), arm_b=(-5, -13), body=1, _fx=fx_base)],
        'Pull': [dict(lean=-3, arm_f=(9, -10), arm_b=(7, -9), _fx=fx_base), dict(lean=-4, arm_f=(8, -8), arm_b=(6, -7), body=1, _fx=fx_base)],
        'Down': [dict(down=True)],
    })
    # ---------------- 라크리모사: 오페라 가수 같은 공연복, 목·흉곽 음향 장치
    lac = Pal((40, 40, 54, 255), (38, 48, 92, 255), 'L')
    make('Lacrimosa', lac, {
        'Idle': [dict(_fx=throat_fx()), dict(body=1, _fx=throat_fx())],
        'Walk': [dict(w, _fx=throat_fx()) for w in WALK],
        'Sing': [dict(arm_f=(10, -8), _fx=[throat_fx(), waves_fx(0)]), dict(arm_f=(11, -9), body=-1, _fx=[throat_fx(), waves_fx(1)])],
        'Windup': [dict(lean=-2, arm_f=(4, -14), arm_b=(-4, -14), _fx=throat_fx())],
        'Stagger': [dict(lean=-3, eyes='x', arm_f=(-4, 6), _fx=throat_fx(False)), dict(lean=-2, eyes='x', arm_f=(-3, 7), body=1, _fx=throat_fx(False))],
        'Listen': [dict(head_tilt=3, eyes='-', arm_f=(2, 10), _fx=throat_fx(False))],
        'Hurt': [dict(lean=-2, eyes='x', arm_f=(-3, 8), _fx=throat_fx(False))],
        'Bow': [dict(lean=4, head_tilt=3, arm_f=(3, 12), _fx=throat_fx(False))],
        'Down': [dict(down=True)],
    })
    # ---------------- 회상: 어린 에드워드 / 음향실의 에드워드 (개조 전, 장치 없음)
    edk = Pal((96, 72, 50, 255), (110, 120, 136, 255), 'E')
    make('EdwardKid', edk, {
        'Idle': [dict(), dict(body=1)],
        'Hide': [dict(crouch=8, lean=-2, arm_f=(-2, -8), arm_b=(-4, -8), eyes='-')],
        'Listen': [dict(sit=True, head_tilt=2, arm_f=(4, 8), _fx=tv_fx), dict(sit=True, head_tilt=3, arm_f=(4, 8), body=1, _fx=tv_fx)],
    }, scale=0.74)
    ed = Pal((96, 72, 50, 255), (70, 76, 90, 255), 'E')
    make('Edward', ed, {
        'Idle': [dict(_fx=headset_fx), dict(body=1, _fx=headset_fx)],
        'Write': [dict(sit=True, arm_f=(8, 6), _fx=[headset_fx, pen_fx]), dict(sit=True, arm_f=(9, 5), body=1, _fx=[headset_fx, pen_fx])],
    })
    # ---------------- 베스페르 단원 (일반 구성원 · 탑승 예정자): 회색 제복 + 번호표
    for nm, hair, cloth in (('MemberA', (70, 60, 52, 255), (108, 110, 118, 255)), ('MemberB', (170, 150, 110, 255), (96, 100, 112, 255))):
        p = Pal(hair, cloth, 'V')
        make(nm, p, dict(BASE, **{
            'Idle': [dict(_fx=tag_fx), dict(body=1, _fx=tag_fx)],
            'Wait': [dict(sit=True, arm_f=(4, 8), _fx=tag_fx), dict(sit=True, arm_f=(4, 8), body=1, _fx=tag_fx)],
            'Scared': [dict(lean=-2, arm_f=(4, -12), arm_b=(-4, -12))],
            'Cheer': [dict(arm_f=(4, -14)), dict(arm_f=(5, -12), body=-1)],
        }))
    # ---------------- 관제 경비 (회수 경비): 진한 제복, 제압봉
    gp = Pal((40, 40, 44, 255), (52, 58, 72, 255), 'G')
    make('Warden', gp, dict(BASE, **{
        'Guard': [dict(arm_f=(8, -2), baton='on'), dict(arm_f=(8, -1), baton='on', body=1)],
        'Strike': [dict(arm_f=(12, -4), lean=3, baton='on', baton_dir=(10, -2))],
    }))
    # ---------------- 루: 마지막 구간 동작
    lu = Pal((250, 214, 90, 255), (70, 92, 140, 255), 'LU')
    make('Lu', lu, {
        'Cold': [dict(head_tilt=1, eyes='-', arm_f=(4, 10), glow=(140, 220, 255, 170)), dict(head_tilt=1, eyes='-', arm_f=(4, 10), body=1, glow=(140, 220, 255, 120))],
        'Limit': [dict(arm_f=(6, -16), arm_b=(-6, -16), glow=(200, 240, 255, 230)), dict(arm_f=(7, -17), arm_b=(-7, -17), body=-1, glow=(255, 255, 255, 255))],
        'Lie': [dict(down=True)],
        'Kneel': [dict(crouch=9, lean=2, arm_f=(8, 6), eyes='-')],
        'HandHold': [dict(crouch=9, lean=3, arm_f=(10, 10))],
        'Release': [dict(arm_f=(4, -10), arm_b=(-4, -10), glow=(255, 255, 255, 255), eyes='-')],
    })
    # ---------------- 괴물 토끼 라비엘
    beast_strip('Idle', [dict(), dict(crouch=1)])
    beast_strip('Walk', [dict(step=3), dict(crouch=1), dict(step=-3), dict(crouch=1)])
    beast_strip('Windup', [dict(crouch=8, lean=-3, glow=(255, 120, 140, 200)), dict(crouch=9, lean=-4, glow=(255, 120, 140, 255))])
    beast_strip('Leap', [dict(air=6, lean=4)])
    beast_strip('Slam', [dict(crouch=6, arm=10, lean=4)])
    beast_strip('Swipe', [dict(arm=12, lean=3)])
    beast_strip('Stun', [dict(crouch=5, hurt=True), dict(crouch=6, hurt=True)])
    beast_strip('Hurt', [dict(lean=-4, hurt=True)])
    beast_strip('Hold', [dict(hold=True, crouch=4)])
    beast_strip('Down', [dict(crouch=12, hurt=True)])
    # ---------------- 아우렐리우스
    aur_strip('Idle', [dict(), dict(arm=1)])
    aur_strip('Talk', [dict(arm=2), dict(arm=4)])
    aur_strip('Windup', [dict(arm=6, glow=(255, 220, 140, 200)), dict(arm=7, glow=(255, 240, 180, 255))])
    aur_strip('Cast', [dict(arm=8, lean=2)])
    aur_strip('Hurt', [dict(lean=-3)])
    aur_strip('Broken', [dict(broken=1), dict(broken=1, arm=2)])
    aur_strip('Ruined', [dict(broken=2), dict(broken=2, arm=2)])
    aur_strip('Down', [dict(broken=2, lean=-6)])


def scene_props():
    METAL = (92, 100, 110, 255); DARK = (46, 50, 58, 255); RED = (196, 60, 52, 255); GOLD = (214, 180, 96, 255)
    GREEN = (90, 220, 130, 255); BLUE = (110, 180, 240, 255); WHITE = (240, 240, 244, 255)
    # 7장: 수용칸 창살 (2프레임: 닫힘 / 열림)
    fr = []
    for k in range(2):
        a = img(40, 72); d = ImageDraw.Draw(a)
        rect(d, [0, 0, 39, 4], METAL); rect(d, [0, 68, 39, 71], METAL)
        for x in range(3, 38, 6):
            if k == 0: d.line([x, 4, x, 68], fill=(150, 156, 166, 255), width=2)
            else: d.line([x, 4, x, 12], fill=(150, 156, 166, 255), width=2)
        fr.append(a)
    save_scene('U_CellBars', strip(fr))
    # 수용칸 제어반 (2프레임)
    fr = []
    for k in range(2):
        a = img(20, 36); d = ImageDraw.Draw(a)
        rect(d, [2, 2, 17, 33], (70, 74, 82, 255))
        for j in range(3): d.rectangle([5, 6 + j * 8, 14, 10 + j * 8], fill=(GREEN if k else RED))
        fr.append(a)
    save_scene('U_CellPanel', strip(fr))
    # 번호표 발급기 / 호출 번호판 (번호 3종: 47 = 루, 12 = 대기 중, 00 = 빈 운반 인형)
    a = img(24, 44); d = ImageDraw.Draw(a)
    rect(d, [2, 2, 21, 43], (200, 196, 186, 255)); rect(d, [5, 6, 18, 14], DARK); rect(d, [8, 30, 15, 34], (240, 236, 220, 255))
    text(a, 6, 8, 'NO', GREEN)
    save_scene('U_TicketMachine', a)
    fr = []
    for num in ('47', '12', '00', '--'):
        a = img(40, 20); d = ImageDraw.Draw(a)
        rect(d, [0, 0, 39, 19], (30, 30, 36, 255))
        text(a, 12, 7, num, (255, 120, 90, 255) if num != '--' else (110, 110, 110, 255))
        fr.append(a)
    save_scene('U_CallBoard', strip(fr))
    # 포획팔 (집게: 열림 / 닫힘 / 인형을 쥠 / 꺼짐)
    fr = []
    for k in range(4):
        a = img(32, 48); d = ImageDraw.Draw(a)
        rect(d, [13, 0, 18, 26], METAL)
        rect(d, [8, 24, 23, 30], (230, 190, 70, 255) if k != 3 else (100, 100, 100, 255))
        if k == 0:
            d.line([10, 30, 3, 44], fill=METAL, width=3); d.line([21, 30, 28, 44], fill=METAL, width=3)
        else:
            d.line([10, 30, 12, 46], fill=METAL, width=3); d.line([21, 30, 19, 46], fill=METAL, width=3)
        if k == 2: rect(d, [12, 34, 19, 46], (210, 200, 180, 255))
        if k != 3: d.point([(15, 27)], fill=(255, 80, 60, 255))
        fr.append(a)
    save_scene('U_Claw', strip(fr))
    a = img(32, 6); d = ImageDraw.Draw(a)
    rect(d, [0, 0, 31, 5], (80, 86, 96, 255)); d.line([0, 3, 31, 3], fill=(120, 126, 136, 255))
    save_scene('U_ClawRail', a)
    # 빈 운반 인형 (마네킹 같은 빈 몸, 번호표 '00')
    a = img(24, 44); d = ImageDraw.Draw(a)
    d.ellipse([7, 2, 17, 12], fill=(220, 210, 190, 255), outline=OUTLINE)
    rect(d, [6, 13, 17, 32], (210, 200, 180, 255)); rect(d, [8, 32, 10, 43], (190, 180, 160, 255)); rect(d, [13, 32, 15, 43], (190, 180, 160, 255))
    rect(d, [8, 18, 15, 22], WHITE); text(a, 8, 18, '00', DARK)
    save_scene('U_Doll', a)
    # 유지보수 단말 (2프레임: 대기 / 변경됨)
    fr = []
    for k in range(2):
        a = img(28, 36); d = ImageDraw.Draw(a)
        rect(d, [2, 12, 25, 35], (70, 74, 82, 255)); rect(d, [4, 2, 23, 14], DARK)
        d.rectangle([6, 4, 21, 12], fill=(60, 120, 200, 255) if k == 0 else (90, 200, 130, 255))
        text(a, 7, 6, 'MT', WHITE)
        fr.append(a)
    save_scene('U_MaintTerm', strip(fr))
    # 전도성 바닥 (반복 타일, 젖은 금속) / 제동 장치 (2프레임)
    a = img(32, 6); d = ImageDraw.Draw(a)
    rect(d, [0, 0, 31, 5], (110, 130, 150, 255), None)
    for x in range(2, 30, 6): d.line([x, 1, x + 3, 1], fill=(170, 210, 240, 255))
    save_scene('U_ConductFloor', a)
    fr = []
    for k in range(2):
        a = img(20, 30); d = ImageDraw.Draw(a)
        rect(d, [3, 6, 16, 29], (88, 70, 64, 255))
        d.line([10, 6, 10 - 6 + k * 12, 0], fill=(220, 200, 80, 255), width=2)
        fr.append(a)
    save_scene('U_Brake', strip(fr))
    # 상담 부스 잠금장치 / 감시 모니터 (영상실 벽)
    a = img(16, 20); d = ImageDraw.Draw(a)
    rect(d, [2, 2, 13, 17], (160, 150, 120, 255)); d.ellipse([5, 6, 10, 11], fill=DARK)
    save_scene('U_BoothLock', a)
    fr = []
    for k in range(3):
        a = img(48, 36); d = ImageDraw.Draw(a)
        rect(d, [0, 0, 47, 31], (40, 42, 50, 255)); rect(d, [3, 3, 44, 28], (30, 50, 60, 255) if k else (10, 12, 14, 255))
        if k:
            for y in range(4, 28, 3): d.line([4, y, 43, y], fill=(40, 70, 80, 255))
            text(a, 5, 5, 'CAM' + str(k), (150, 230, 200, 255))
        rect(d, [20, 31, 27, 35], (60, 60, 70, 255))
        fr.append(a)
    save_scene('U_Monitor', strip(fr))
    # 격벽 (2프레임: 열림 / 닫힘)
    fr = []
    for k in range(2):
        a = img(36, 96); d = ImageDraw.Draw(a)
        rect(d, [0, 0, 35, 95], (60, 64, 72, 255))
        if k == 1:
            rect(d, [4, 4, 31, 91], (120, 92, 70, 255))
            for y in range(8, 90, 10): d.polygon([(4, y), (31, y + 5), (31, y + 8), (4, y + 3)], fill=(220, 190, 60, 255))
        else:
            rect(d, [4, 4, 31, 12], (120, 92, 70, 255))
        fr.append(a)
    save_scene('U_Bulkhead', strip(fr))
    # 8장: 액자 두 개 (어린 시절 사진) / 컵 두 개 (하나는 손잡이 없음) / 안전 검토 서류
    a = img(28, 22); d = ImageDraw.Draw(a)
    rect(d, [0, 0, 27, 21], (150, 110, 70, 255)); rect(d, [3, 3, 24, 18], (210, 200, 170, 255))
    d.ellipse([6, 7, 11, 12], fill=(250, 214, 90, 255)); d.ellipse([15, 7, 20, 12], fill=(226, 222, 236, 255))
    d.rectangle([6, 12, 11, 17], fill=(70, 92, 140, 255)); d.rectangle([15, 12, 20, 17], fill=(120, 130, 150, 255))
    save_scene('U_ChildPhoto', a)
    fr = []
    for k in range(2):
        a = img(24, 12); d = ImageDraw.Draw(a)
        rect(d, [2, 3, 8, 11], WHITE); d.arc([7, 4, 11, 9], 270, 90, fill=WHITE)
        rect(d, [14, 3, 20, 11], WHITE)
        if k == 0: d.arc([19, 4, 23, 9], 270, 90, fill=(200, 200, 200, 255))
        else: d.point([(21, 5)], fill=(150, 150, 150, 255))
        fr.append(a)
    save_scene('U_Cups', strip(fr))
    # 9장: 대기실 계획 종이 (식당·수리점·합창단·호위대) / 점령 배치도·지휘 체계도
    a = img(64, 28); d = ImageDraw.Draw(a)
    for i, c in enumerate([(240, 220, 170, 255), (220, 236, 240, 255), (240, 230, 236, 255), (230, 236, 210, 255)]):
        rect(d, [i * 16 + 1, 2 + (i % 2) * 3, i * 16 + 14, 22 + (i % 2) * 3], c)
        for y in range(6 + (i % 2) * 3, 20 + (i % 2) * 3, 3): d.line([i * 16 + 3, y, i * 16 + 12, y], fill=(120, 120, 130, 255))
    save_scene('U_PlanNotes', a)
    a = img(56, 40); d = ImageDraw.Draw(a)
    rect(d, [0, 0, 55, 39], (210, 206, 196, 255))
    for (x, y) in ((27, 5), (12, 18), (27, 18), (42, 18), (8, 30), (18, 30), (36, 30), (46, 30)):
        rect(d, [x - 4, y - 2, x + 4, y + 2], (90, 70, 110, 255))
    d.line([27, 7, 27, 16], fill=OUTLINE); d.line([12, 16, 42, 16], fill=OUTLINE)
    d.polygon([(40, 4), (52, 4), (52, 12), (46, 14), (40, 12)], outline=RED)
    save_scene('U_CommandChart', a)
    # 공연장: 마이크 스탠드 (방향 2프레임: 관객 쪽 / 스피커 쪽), 스피커 (2프레임: 꺼짐/켜짐), 믹서 (게인 3단계), 조명 트러스
    fr = []
    for k in range(2):
        a = img(20, 40); d = ImageDraw.Draw(a)
        d.line([10, 12, 10, 38], fill=METAL, width=2); d.line([5, 39, 15, 39], fill=METAL, width=2)
        if k == 0: d.ellipse([6, 4, 14, 12], fill=(60, 60, 70, 255), outline=OUTLINE)
        else: d.ellipse([10, 6, 18, 14], fill=(60, 60, 70, 255), outline=OUTLINE); d.line([10, 12, 14, 10], fill=METAL)
        fr.append(a)
    save_scene('U_Mic', strip(fr))
    fr = []
    for k in range(2):
        a = img(32, 48); d = ImageDraw.Draw(a)
        rect(d, [2, 2, 29, 47], (40, 40, 48, 255))
        for cy, r in ((14, 8), (35, 10)):
            d.ellipse([16 - r, cy - r, 16 + r, cy + r], fill=(70, 70, 80, 255), outline=OUTLINE)
            d.ellipse([16 - r // 3, cy - r // 3, 16 + r // 3, cy + r // 3], fill=BLUE if k else (60, 60, 66, 255))
        fr.append(a)
    save_scene('U_Speaker', strip(fr))
    fr = []
    for k in range(3):
        a = img(32, 24); d = ImageDraw.Draw(a)
        rect(d, [1, 8, 30, 23], (60, 62, 72, 255))
        for j in range(3):
            x = 6 + j * 9; d.line([x, 11, x, 21], fill=(30, 30, 34, 255))
            ky = 20 - (k * 4 if j == 1 else 4)
            d.rectangle([x - 2, ky - 1, x + 2, ky + 1], fill=(230, 200, 80, 255) if j == 1 else WHITE)
        for j in range(5): d.rectangle([3 + j * 6, 2, 6 + j * 6, 6], fill=(GREEN if j < 2 + k else (60, 60, 60, 255)) if j < 4 else (RED if k == 2 else (60, 60, 60, 255)))
        fr.append(a)
    save_scene('U_Mixer', strip(fr))
    a = img(96, 16); d = ImageDraw.Draw(a)
    d.line([0, 2, 95, 2], fill=METAL, width=2); d.line([0, 12, 95, 12], fill=METAL, width=2)
    for x in range(0, 96, 8): d.line([x, 2, x + 8, 12], fill=METAL); d.line([x + 8, 2, x, 12], fill=METAL)
    for x in (12, 40, 68): d.rectangle([x, 12, x + 8, 15], fill=(80, 80, 90, 255)); d.point([(x + 4, 15)], fill=(255, 240, 180, 255))
    save_scene('U_Truss', a)
    a = img(20, 14); d = ImageDraw.Draw(a)
    rect(d, [1, 3, 18, 13], (80, 70, 60, 255)); d.ellipse([4, 5, 9, 10], fill=(40, 40, 40, 255)); d.ellipse([11, 5, 16, 10], fill=(40, 40, 40, 255))
    save_scene('U_Tape', a)
    # 서버실 랙 (불빛 2프레임) / 포획 단말 (3프레임: 대기 / 흡입 / 방출)
    fr = []
    for k in range(2):
        a = img(28, 64); d = ImageDraw.Draw(a)
        rect(d, [1, 1, 26, 63], (36, 40, 48, 255))
        for y in range(5, 60, 6):
            rect(d, [4, y, 23, y + 4], (54, 60, 70, 255))
            d.point([(6, y + 2)], fill=GREEN if (y // 6 + k) % 3 else (255, 180, 60, 255))
        fr.append(a)
    save_scene('U_ServerRack', strip(fr))
    fr = []
    for k in range(3):
        a = img(48, 72); d = ImageDraw.Draw(a)
        rect(d, [4, 10, 43, 71], (60, 64, 76, 255)); rect(d, [10, 16, 37, 56], (20, 30, 44, 255))
        c = (110, 180, 240, 255) if k == 0 else (240, 120, 90, 255) if k == 1 else (250, 250, 210, 255)
        d.ellipse([16, 24, 31, 46], outline=c, width=2)
        rect(d, [14, 0, 33, 10], (80, 84, 96, 255)); text(a, 17, 2, 'CAP', (200, 200, 210, 255))
        fr.append(a)
    save_scene('U_CaptureTerm', strip(fr))
    # 10장: 탑승 캡슐 (켜짐 / 꺼짐·손상) / 탑승 현황판 ('통과 완료 0') / 접수 명부
    fr = []
    for k in range(2):
        a = img(40, 76); d = ImageDraw.Draw(a)
        d.rounded_rectangle([2, 2, 37, 75], 10, fill=(150, 160, 176, 255), outline=OUTLINE)
        d.rounded_rectangle([8, 10, 31, 60], 8, fill=(120, 200, 230, 160) if k == 0 else (40, 46, 56, 255))
        if k == 1: d.line([10, 20, 28, 46], fill=(220, 220, 230, 255)); d.line([24, 14, 16, 40], fill=(220, 220, 230, 255))
        d.rectangle([14, 64, 25, 70], fill=(220, 190, 70, 255) if k == 0 else RED)
        fr.append(a)
    save_scene('U_Capsule', strip(fr))
    a = img(64, 28); d = ImageDraw.Draw(a)
    rect(d, [0, 0, 63, 27], (24, 26, 34, 255))
    text(a, 4, 4, 'EXODUS', (200, 200, 210, 255)); text(a, 4, 13, 'IN 0', GREEN); text(a, 32, 13, 'Q 64', (255, 200, 90, 255))
    save_scene('U_BoardStatus', a)
    a = img(14, 10); d = ImageDraw.Draw(a)
    rect(d, [0, 1, 13, 9], (150, 40, 50, 255)); d.line([7, 1, 7, 9], fill=(240, 220, 170, 255))
    save_scene('U_Register', a)
    # 최종전 기물: 호위 장치(구), 축전기(2프레임: 충전 / 과부하), 냉각관, 전이 결합부(3단계), 정비용 전자석
    fr = []
    for k in range(2):
        a = img(20, 20); d = ImageDraw.Draw(a)
        d.ellipse([2, 2, 17, 17], fill=(200, 190, 160, 255), outline=OUTLINE)
        d.ellipse([7, 7, 12, 12], fill=GOLD if k == 0 else RED)
        fr.append(a)
    save_scene('U_Escort', strip(fr))
    fr = []
    for k in range(2):
        a = img(28, 52); d = ImageDraw.Draw(a)
        rect(d, [3, 6, 24, 51], (70, 80, 96, 255)); rect(d, [7, 0, 20, 6], (120, 124, 136, 255))
        for y in range(12, 48, 6): d.line([6, y, 21, y], fill=(150, 220, 255, 255) if k == 0 else (255, 160, 90, 255))
        fr.append(a)
    save_scene('U_Capacitor', strip(fr))
    a = img(16, 64); d = ImageDraw.Draw(a)
    rect(d, [4, 0, 11, 63], (130, 170, 190, 255));
    for y in range(4, 60, 10): d.line([4, y, 11, y], fill=(90, 120, 140, 255))
    save_scene('U_CoolPipe', a)
    fr = []
    for k in range(3):
        a = img(72, 72); d = ImageDraw.Draw(a)
        d.ellipse([6, 6, 65, 65], fill=(60, 56, 80, 255), outline=OUTLINE)
        d.ellipse([18, 18, 53, 53], fill=(200, 170, 240, 255) if k < 2 else (60, 60, 70, 255))
        d.ellipse([28, 28, 43, 43], fill=(255, 255, 255, 255) if k == 0 else (255, 160, 120, 255) if k == 1 else (40, 40, 44, 255))
        if k >= 1:
            d.line([20, 14, 34, 34, 28, 58], fill=(30, 20, 30, 255), width=2)
        if k == 2:
            d.line([44, 10, 38, 36, 56, 52], fill=(30, 20, 30, 255), width=2)
        fr.append(a)
    save_scene('U_Coupling', strip(fr))
    a = img(32, 24); d = ImageDraw.Draw(a)
    rect(d, [2, 6, 29, 23], (140, 60, 52, 255)); d.arc([6, 0, 25, 18], 180, 360, fill=METAL, width=3)
    save_scene('U_Magnet', a)
    # 9~11장: 잔해 더미 / 무너진 트러스 / 회수 장치 (2프레임) / 끊어진 번호표 / 멈춘 롤러
    a = img(80, 32); d = ImageDraw.Draw(a)
    random.seed(711)
    for i in range(22):
        x = random.randint(0, 66); y = random.randint(6, 26); w = random.randint(6, 16); h = random.randint(4, 8)
        rect(d, [x, y, min(79, x + w), min(31, y + h)], random.choice([(84, 82, 88, 255), (110, 104, 96, 255), (70, 74, 80, 255), (120, 90, 70, 255)]))
    save_scene('U_Rubble', a)
    fr = []
    for k in range(2):
        a = img(40, 36); d = ImageDraw.Draw(a)
        rect(d, [4, 8, 35, 28], (110, 116, 126, 255)); d.ellipse([14, 12, 25, 23], fill=(30, 30, 34, 255))
        d.ellipse([17, 15, 22, 20], fill=(255, 80, 60, 255) if k else (120, 40, 40, 255))
        d.line([8, 28, 4, 35], fill=METAL, width=2); d.line([31, 28, 35, 35], fill=METAL, width=2)
        d.line([20, 8, 20, 1], fill=METAL)
        fr.append(a)
    save_scene('U_Recovery', strip(fr))
    a = img(12, 8); d = ImageDraw.Draw(a)
    d.polygon([(0, 1), (7, 0), (6, 7), (0, 6)], fill=(230, 220, 170, 255), outline=OUTLINE); text(a, 1, 1, 'F', DARK)
    d.line([8, 2, 11, 4], fill=(150, 140, 110, 255))
    save_scene('U_BrokenTag', a)
    a = img(16, 8); d = ImageDraw.Draw(a)
    d.rectangle([2, 2, 12, 6], fill=SKIN, outline=OUTLINE); d.line([12, 3, 15, 2], fill=SKIN)
    save_scene('U_StillHand', a)
    a = img(12, 8); d = ImageDraw.Draw(a)
    d.ellipse([1, 1, 6, 6], fill=(70, 70, 80, 255), outline=OUTLINE); d.ellipse([6, 1, 11, 6], fill=(70, 70, 80, 255), outline=OUTLINE)
    save_scene('U_Roller', a)
    # 엔딩: 가상 바탕화면 아이콘 (작품 내부 그림, 실제 OS 그림이 아님)
    a = img(16, 16); d = ImageDraw.Draw(a)
    rect(d, [1, 3, 14, 14], (240, 200, 90, 255)); rect(d, [1, 1, 7, 4], (240, 200, 90, 255))
    save_scene('U_IconFolder', a)
    a = img(16, 16); d = ImageDraw.Draw(a)
    rect(d, [2, 1, 13, 14], (236, 236, 240, 255)); [d.line([4, y, 11, y], fill=(150, 150, 160, 255)) for y in (4, 7, 10)]
    save_scene('U_IconDoc', a)
    a = img(16, 16); d = ImageDraw.Draw(a)
    rect(d, [3, 4, 12, 14], (190, 196, 206, 255)); rect(d, [2, 2, 13, 4], (150, 156, 166, 255))
    save_scene('U_IconBin', a)
    a = img(16, 16); d = ImageDraw.Draw(a)
    rect(d, [1, 1, 14, 14], (40, 46, 70, 255)); d.polygon([(4, 11), (8, 3), (12, 11)], fill=(255, 220, 100, 255))
    save_scene('U_IconGame', a)


def main():
    random.seed(7111)
    characters()
    scene_props()
    with io.open(MANIFEST, 'w', encoding='utf-8') as m:
        for r in sorted(set(made) | owned): m.write(r + '\n')
    print('made', len(made))


if __name__ == '__main__':
    main()
