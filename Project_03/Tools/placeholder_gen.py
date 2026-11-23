# -*- coding: utf-8 -*-
"""
ELECTRIC WORLD: LAST LIGHT - 개발용 임시 스프라이트 생성기.

이 스크립트가 만드는 그림은 '최종 디자인이 아닌' 개발용 임시 그림이다.
캐릭터 몸통에 LU / VN / RS 같은 글자를 찍어 임시 그림임을 분명히 한다.
사용자 원화가 준비되면 Docs/ART_REPLACEMENT_GUIDE.md 의 규격대로 교체한다.

실행: python Tools/placeholder_gen.py   (Pillow 필요)
출력: Assets/Game/Art/Placeholder/*.png
모든 시트는 가로 한 줄 스트립. 프레임 크기는 Assets/Game/Scripts/Editor/EWArtSpecs.cs 와 일치해야 한다.
"""
import os, math, random
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'Assets', 'Game', 'Art', 'Placeholder')
os.makedirs(OUT, exist_ok=True)

OUTLINE = (26, 22, 38, 255)
SKIN = (240, 200, 170, 255)
WHITE = (255, 255, 255, 255)
CLEAR = (0, 0, 0, 0)

FONT = {  # 3x5 pixel glyphs
    'A': ['010', '101', '111', '101', '101'], 'D': ['110', '101', '101', '101', '110'],
    'E': ['111', '100', '110', '100', '111'], 'G': ['011', '100', '101', '101', '011'],
    'L': ['100', '100', '100', '100', '111'], 'M': ['101', '111', '111', '101', '101'],
    'N': ['101', '111', '111', '111', '101'], 'O': ['010', '101', '101', '101', '010'],
    'P': ['110', '101', '110', '100', '100'], 'R': ['110', '101', '110', '101', '101'],
    'S': ['011', '100', '010', '001', '110'], 'T': ['111', '010', '010', '010', '010'],
    'U': ['101', '101', '101', '101', '111'], 'V': ['101', '101', '101', '101', '010'],
    '?': ['110', '001', '010', '000', '010'], '+': ['000', '010', '111', '010', '000'],
    '-': ['000', '000', '111', '000', '000'], '!': ['010', '010', '010', '000', '010'],
}


def text(img, x, y, s, col):
    px = img.load()
    for ch in s:
        g = FONT.get(ch)
        if g:
            for gy, row in enumerate(g):
                for gx, c in enumerate(row):
                    if c == '1' and 0 <= x + gx < img.width and 0 <= y + gy < img.height:
                        px[x + gx, y + gy] = col
        x += 4


def outline(layer):
    """채워진 픽셀 주변 빈 칸에 외곽선을 그린다."""
    w, h = layer.size
    src = layer.load()
    out = layer.copy()
    dst = out.load()
    for y in range(h):
        for x in range(w):
            if src[x, y][3] == 0:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < w and 0 <= ny < h and src[nx, ny][3] > 0 and src[nx, ny] != OUTLINE:
                        dst[x, y] = OUTLINE
                        break
    return out


def strip(frames):
    w, h = frames[0].size
    s = Image.new('RGBA', (w * len(frames), h), CLEAR)
    for i, f in enumerate(frames):
        s.paste(f, (i * w, 0))
    return s


def save(img, name):
    img.save(os.path.join(OUT, name + '.png'))


def shade(c, k):
    return tuple(max(0, min(255, int(v * k))) for v in c[:3]) + (255,)


# ---------------------------------------------------------------- characters
class Pal:
    def __init__(self, hair, cloth, label, long_hair=False, coat=False, device=False):
        self.hair, self.cloth, self.label = hair, cloth, label
        self.long_hair, self.coat, self.device = long_hair, coat, device


def figure(p, pose):
    """48x64 캔버스, 오른쪽을 본다. 발바닥은 y=63 (피벗 = 하단 중앙)."""
    W, H = 48, 64
    img = Image.new('RGBA', (W, H), CLEAR)
    d = ImageDraw.Draw(img)
    g = dict(body=0, lx=0, rx=0, ly=0, ry=0, arm_f=(6, 10), arm_b=(-5, 10), lean=0,
             eyes='o', glow=None, down=False, sit=False, head_tilt=0,
             back=False, crouch=0, thin=False, baton=None, baton_dir=(3, -12), cover_mouth=False, hands_head=False)
    g.update(pose)
    if g['back']:
        return figure_back(p, g)
    if g['down']:
        # 쓰러진 자세: 몸을 가로로 눕힌다
        d.rectangle([8, 54, 36, 61], fill=p.cloth)
        d.ellipse([34, 49, 46, 62], fill=SKIN)
        d.rectangle([34, 49, 46, 54], fill=p.hair)
        d.rectangle([2, 56, 9, 60], fill=shade(p.cloth, 0.7))
        text(img, 15, 55, p.label, WHITE)
        img = outline(img)
        return img
    b = g['body'] + g['crouch']
    lean = g['lean']
    cx = 24 + lean
    # legs
    leg_col = shade(p.cloth, 0.6)
    if g['sit']:
        d.rectangle([cx - 6, 56, cx + 9, 60], fill=leg_col)
        d.rectangle([cx + 8, 56, cx + 11, 63], fill=leg_col)
    else:
        cr = g['crouch']
        d.rectangle([19 + g['lx'], min(62, 50 + g['ly'] + cr), 22 + g['lx'], 63 + min(0, g['ly'])], fill=leg_col)
        d.rectangle([25 + g['rx'], min(62, 50 + g['ry'] + cr), 28 + g['rx'], 63 + min(0, g['ry'])], fill=shade(leg_col, 0.85))
    # back arm
    sy = 35 + b + (4 if g['sit'] else 0)
    ab = g['arm_b']
    d.line([cx - 6, sy, cx - 6 + ab[0], sy + ab[1]], fill=shade(p.cloth, 0.75), width=3)
    # torso
    ty = 33 + b + (4 if g['sit'] else 0)
    torso_col = (225, 225, 235, 255) if p.coat else p.cloth
    if g['thin']:
        d.rectangle([cx - 4, ty, cx + 3, ty + 17], fill=torso_col)
    else:
        d.rectangle([cx - 7, ty, cx + 6, ty + 17], fill=torso_col)
    text(img, cx - 5, ty + 6, p.label, OUTLINE if p.coat else WHITE)
    # head
    hy = 15 + b + (4 if g['sit'] else 0) + abs(g['head_tilt'])
    hx = cx + g['head_tilt']
    d.ellipse([hx - 9, hy, hx + 9, hy + 18], fill=SKIN)
    d.chord([hx - 9, hy - 1, hx + 9, hy + 17], 180, 360, fill=p.hair)
    d.rectangle([hx - 9, hy + 6, hx - 5, hy + 12], fill=p.hair)   # 뒷머리
    if p.long_hair:
        d.rectangle([hx - 10, hy + 6, hx - 6, hy + 24], fill=p.hair)
    # eyes (facing right)
    ex, ey = hx + 3, hy + 10
    if g['eyes'] == 'x':
        for e in (ex - 3, ex + 2):
            d.point([(e - 1, ey - 1), (e + 1, ey + 1), (e + 1, ey - 1), (e - 1, ey + 1), (e, ey)], fill=OUTLINE)
    elif g['eyes'] == '-':
        d.line([ex - 4, ey, ex - 2, ey], fill=OUTLINE)
        d.line([ex + 1, ey, ex + 3, ey], fill=OUTLINE)
    else:
        d.rectangle([ex - 3, ey - 1, ex - 2, ey + 1], fill=OUTLINE)
        d.rectangle([ex + 2, ey - 1, ex + 3, ey + 1], fill=OUTLINE)
    # front arm
    af = g['arm_f']
    d.line([cx + 5, sy, cx + 5 + af[0], sy + af[1]], fill=shade(p.cloth, 1.1) if not p.coat else (210, 210, 220, 255), width=3)
    d.rectangle([cx + 4 + af[0], sy + af[1] - 1, cx + 6 + af[0], sy + af[1] + 1], fill=SKIN)
    if g['cover_mouth']:   # 입을 가림
        d.rectangle([hx + 2, hy + 12, hx + 7, hy + 15], fill=SKIN); d.line([cx + 5, sy, hx + 4, hy + 14], fill=shade(p.cloth, 1.1), width=3)
    if g['hands_head']:    # 머리를 감싸 쥠
        for side in (-1, 1):
            d.line([cx + side * 5, sy, hx + side * 8, hy + 6], fill=shade(p.cloth, 1.0), width=3)
            d.rectangle([hx + side * 8 - 1, hy + 5, hx + side * 8 + 1, hy + 8], fill=SKIN)
    if g['baton']:  # 제압봉: 손에서 뻗은 막대, 켜지면 끝이 빛남
        hx2, hy2 = cx + 5 + af[0], sy + af[1]
        bx, by = hx2 + g['baton_dir'][0], hy2 + g['baton_dir'][1]
        d.line([hx2, hy2, bx, by], fill=(60, 60, 70, 255), width=2)
        if g['baton'] == 'on':
            d.rectangle([bx - 1, by - 1, bx + 1, by + 1], fill=(255, 240, 120, 255))
            d.point([(bx - 2, by - 2), (bx + 2, by - 2), (bx, by - 3)], fill=(170, 240, 255, 255))
    if p.device:  # 화학 개조 팔 장치: 등의 약품 탱크 → 관 → 손목 약병 (액체가 보이게)
        tx0, ty0 = cx - 12, ty + 1
        d.rectangle([tx0, ty0, tx0 + 5, ty0 + 12], fill=(170, 190, 190, 255))
        lvl = g.get('liquid', 5)
        d.rectangle([tx0 + 1, ty0 + 12 - lvl, tx0 + 4, ty0 + 11], fill=(120, 240, 90, 255))
        wx, wy = cx + 5 + af[0] * 2 // 3, sy + af[1] * 2 // 3
        d.line([tx0 + 3, ty0, cx - 2, sy - 3, wx, wy], fill=(90, 210, 110, 255), width=1)
        d.rectangle([wx - 2, wy - 2, wx + 2, wy + 2], fill=(120, 240, 90, 255))
        d.point([(wx, wy - 3)], fill=(220, 255, 170, 255))
        if g.get('bubbles'):
            for k in range(3):
                d.point([(wx - 2 + k * 2, wy - 5 - (k + g['bubbles']) % 3 * 2)], fill=(220, 255, 170, 255))
    img = outline(img)
    if g['glow']:
        gl = Image.new('RGBA', img.size, CLEAR)
        gp = gl.load(); ip = img.load()
        for y in range(H):
            for x in range(W):
                if ip[x, y][3] == 0:
                    for dx, dy in ((2, 0), (-2, 0), (0, 2), (0, -2), (1, 1), (-1, -1), (1, -1), (-1, 1)):
                        nx, ny = x + dx, y + dy
                        if 0 <= nx < W and 0 <= ny < H and ip[nx, ny][3] > 0:
                            gp[x, y] = g['glow']
                            break
        gl.alpha_composite(img)
        img = gl
    return img


def figure_back(p, g):
    """뒷모습 (화면 안쪽을 본다). 얼굴은 보이지 않는다."""
    W, H = 48, 64
    img = Image.new('RGBA', (W, H), CLEAR)
    d = ImageDraw.Draw(img)
    b = g['body'] + g['crouch']
    cx = 24 + g['lean']
    leg_col = shade(p.cloth, 0.6)
    d.rectangle([18 + g['lx'], 50 + g['ly'], 21 + g['lx'], 63], fill=leg_col)
    d.rectangle([26 + g['rx'], 50 + g['ry'], 29 + g['rx'], 63], fill=shade(leg_col, 0.85))
    ty = 33 + b
    d.rectangle([cx - 7, ty, cx + 7, ty + 17], fill=p.cloth)
    text(img, cx - 4, ty + 6, p.label, WHITE)
    sy = 35 + b
    for side, ar in ((-1, g['arm_b']), (1, g['arm_f'])):
        x0 = cx + side * 8
        d.line([x0, sy, x0 + side * abs(ar[0]) // 3, sy + ar[1]], fill=shade(p.cloth, 0.8), width=3)
    hy = 15 + b + abs(g['head_tilt'])
    hx = cx + g['head_tilt']
    d.ellipse([hx - 9, hy, hx + 9, hy + 18], fill=p.hair)
    d.rectangle([hx - 8, hy + 12, hx + 8, hy + 19], fill=shade(p.hair, 0.85))
    d.point([(hx - 9, hy + 13), (hx + 9, hy + 13)], fill=SKIN)
    img = outline(img)
    return img


def spark_dot(img, x, y, r=2):
    d = ImageDraw.Draw(img)
    d.line([x - r, y, x + r, y], fill=(255, 250, 160, 255))
    d.line([x, y - r, x, y + r], fill=(255, 250, 160, 255))
    d.point([(x, y)], fill=WHITE)


def make_character(name, p, extra=None):
    walk = [dict(lx=3, rx=-3, ly=-1), dict(body=-1), dict(lx=-3, rx=3, ry=-1), dict(body=-1)]
    sets = {
        'Idle': [dict(), dict(body=1)],
        'Walk': [dict(w, arm_f=(4 - 4 * (i % 2 == 0) * (1 if i == 0 else -1), 10)) for i, w in enumerate(walk)],
        'Jump': [dict(ly=-4, ry=-2, arm_f=(5, -9), arm_b=(-6, -7))],
        'Fall': [dict(ly=-1, ry=-3, arm_f=(8, -4), arm_b=(-8, -4))],
        'Climb': [dict(arm_f=(1, -12), arm_b=(-1, 6), ly=-3), dict(arm_f=(1, 6), arm_b=(-1, -12), ry=-3)],
        'Hurt': [dict(lean=-2, eyes='x', arm_f=(-3, 8), arm_b=(-7, 4))],
        'Down': [dict(down=True)],
        'Sit': [dict(sit=True, arm_f=(4, 8))],
    }
    if extra:
        sets.update(extra)
    for state, poses in sets.items():
        frames = []
        for po in poses:
            fx = po.pop('_fx', None)
            f = figure(p, po)
            if fx:
                fx(f)
            frames.append(f)
        save(strip(frames), '%s_%s' % (name, state))


lu = Pal((250, 214, 90, 255), (70, 92, 140, 255), 'LU')
make_character('Lu', lu, {
    'Flick': [dict(arm_f=(4, 2)), dict(arm_f=(10, -2), _fx=lambda f: spark_dot(f, 42, 34)), dict(arm_f=(7, 4))],
    'Point': [dict(arm_f=(6, 0)), dict(arm_f=(12, -1), _fx=lambda f: spark_dot(f, 45, 34, 1)), dict(arm_f=(12, -1), _fx=lambda f: spark_dot(f, 45, 34))],
    'Swing': [dict(arm_f=(-4, -12), lean=-1), dict(arm_f=(10, -6)), dict(arm_f=(9, 8), lean=1)],
    'Kick': [dict(lean=-1, rx=2, ry=-3), dict(lean=-2, rx=9, ry=-8, arm_b=(-8, 2)), dict(lean=0, eyes='-')],
    'Charge': [dict(arm_f=(2, 4), arm_b=(2, 4), glow=(255, 240, 120, 160)), dict(arm_f=(2, 4), arm_b=(2, 4), body=1, glow=(255, 255, 200, 220))],
    'Discharge': [dict(arm_f=(9, -8), arm_b=(-9, -8), glow=(255, 250, 170, 255)), dict(arm_f=(10, -3), arm_b=(-10, -3), glow=(170, 240, 255, 255))],
    'Carry': [dict(w, arm_f=(10, -1)) for w in [dict(lx=2, rx=-2), dict(body=-1), dict(lx=-2, rx=2), dict(body=-1)]],
    # ---- 오프닝 연출용 (임시 그림, Docs/ART_REQUESTS.md 의 교체 대상)
    'BackIdle': [dict(back=True, arm_f=(2, 11), arm_b=(2, 11)), dict(back=True, body=1, arm_f=(2, 11), arm_b=(2, 11))],
    'BackLook': [dict(back=True, head_tilt=2, arm_f=(2, 11), arm_b=(2, 11))],
    'Limp': [dict(lx=3, rx=-2, lean=1, arm_b=(1, 6)), dict(body=-1, lean=1, arm_b=(1, 6)),
             dict(lx=-1, rx=3, ry=-1, lean=3, body=1, head_tilt=1, eyes='-', arm_b=(1, 6)), dict(lean=3, body=2, head_tilt=1, eyes='-', arm_b=(1, 6)),
             dict(lean=2, body=1, arm_b=(1, 6)), dict(lean=1, arm_b=(1, 6))],
    'LimpIdle': [dict(lean=2, body=1, head_tilt=1, arm_b=(1, 6)), dict(lean=2, body=2, head_tilt=1, arm_b=(1, 6))],
    'Sad': [dict(head_tilt=2, eyes='-', lean=1, arm_f=(1, 11), arm_b=(1, 11)), dict(head_tilt=2, eyes='-', lean=1, body=1, arm_f=(1, 11), arm_b=(1, 11))],
    'ReachPC': [dict(lean=2, arm_f=(8, 2), eyes='-'), dict(lean=2, arm_f=(10, 0))],
    'Connect': [dict(arm_f=(10, -1), glow=(170, 220, 255, 120)), dict(arm_f=(10, -1), glow=(200, 240, 255, 200)), dict(arm_f=(10, -2), body=-1, glow=(240, 255, 255, 255))],
    'Land': [dict(crouch=7, lx=-3, rx=3, arm_f=(8, 2), arm_b=(-8, 2), eyes='x')],
    'GetUp': [dict(crouch=6, arm_f=(5, 6), eyes='-'), dict(crouch=3, arm_f=(4, 8)), dict(crouch=1, arm_f=(5, 9))],
    'LookAround': [dict(), dict(head_tilt=-2), dict(), dict(head_tilt=2)],
    'Knock': [dict(lean=1, arm_f=(8, -6)), dict(lean=2, arm_f=(11, -6)), dict(lean=1, arm_f=(8, -5))],
    'BackAway': [dict(lean=-2, lx=-3, rx=2, arm_f=(5, -10), arm_b=(-3, -10)), dict(lean=-2, body=-1, arm_f=(5, -10), arm_b=(-3, -10)),
                 dict(lean=-2, lx=2, rx=-3, arm_f=(5, -9), arm_b=(-3, -10)), dict(lean=-2, body=-1, arm_f=(5, -10), arm_b=(-3, -10))],
    'Fear': [dict(lean=-2, crouch=2, arm_f=(4, -8), arm_b=(-2, -8)), dict(lean=-2, crouch=2, lx=1, arm_f=(4, -9), arm_b=(-2, -9))],
    'ChargeEyes': [dict(eyes='-', crouch=1, arm_f=(1, 8), arm_b=(1, 8), glow=(255, 240, 120, 90)),
                   dict(eyes='-', crouch=2, arm_f=(1, 7), arm_b=(1, 7), glow=(255, 250, 170, 170)),
                   dict(eyes='-', crouch=2, body=1, arm_f=(1, 7), arm_b=(1, 7), glow=(255, 255, 220, 255))],
    'Recover': [dict(crouch=3, lean=2, eyes='-', arm_f=(3, 6), arm_b=(3, 6)), dict(crouch=4, lean=2, eyes='-', arm_f=(3, 7), arm_b=(3, 7))],
    # ---- 실시간 잠입 (임시)
    'Crouch': [dict(crouch=18, lean=2, arm_f=(6, 1), arm_b=(-4, 2)), dict(crouch=18, lean=2, body=1, arm_f=(6, 1), arm_b=(-4, 2))],
    'CrouchWalk': [dict(crouch=18, lean=2, arm_f=(6, 1), arm_b=(-4, 2)), dict(crouch=19, lean=3, arm_f=(6, 1), arm_b=(-4, 2)),
                   dict(crouch=18, lean=1, arm_f=(6, 1), arm_b=(-4, 2)), dict(crouch=19, lean=2, arm_f=(6, 1), arm_b=(-4, 2))],
    'Dodge': [dict(lean=5, crouch=6, lx=-5, rx=6, ry=-2, arm_b=(-9, -2), arm_f=(7, 2), eyes='-')],
    'WallLean': [dict(lean=3, body=1, arm_f=(10, -5), eyes='-', head_tilt=1), dict(lean=3, body=2, arm_f=(10, -5), eyes='-', head_tilt=1)],
    # ---- 2026-11-15 개편 (임시): 통 안 수용 자세, 눈 감기, 전기화, 혐오·두통, 들어 올려짐, 던져짐, 손 잡기
    'TankCrouch': [dict(crouch=12, lean=1, arm_f=(5, 4), arm_b=(4, 4), eyes='-'), dict(crouch=13, lean=1, body=1, arm_f=(5, 4), arm_b=(4, 4), eyes='-')],
    'TankStruggle': [dict(crouch=8, lean=2, arm_f=(9, -9), arm_b=(-3, -10)), dict(crouch=9, lean=3, arm_f=(10, -5), arm_b=(-2, -11), eyes='x')],
    'TankCower': [dict(crouch=14, lean=-2, arm_f=(3, -9), arm_b=(1, -9), eyes='-'), dict(crouch=15, lean=-2, arm_f=(3, -10), arm_b=(1, -10), eyes='-')],
    'EyesClosed': [dict(head_tilt=2, eyes='-', lean=1, arm_f=(1, 11), arm_b=(1, 11), body=1)],
    'Electrify': [dict(arm_f=(10, -1), glow=(220, 245, 255, 255), thin=True), dict(arm_f=(10, -1), glow=(255, 255, 255, 255), thin=True, body=-1)],
    'Disgust': [dict(lean=2, crouch=3, cover_mouth=True, eyes='-', head_tilt=1), dict(lean=3, crouch=5, cover_mouth=True, eyes='-', head_tilt=2)],
    'HeadHold': [dict(lean=1, crouch=2, hands_head=True, eyes='x', _fx=lambda f: spark_dot(f, 34, 14)), dict(lean=2, crouch=3, hands_head=True, eyes='-', _fx=lambda f: spark_dot(f, 14, 12))],
    'Lifted': [dict(arm_f=(2, -12), arm_b=(-1, -12), lx=2, rx=-1, ly=2, eyes='x'), dict(arm_f=(3, -12), arm_b=(0, -13), lx=-2, rx=3, ry=1, eyes='-'),
               dict(arm_f=(2, -13), arm_b=(-1, -12), lx=3, rx=-2, ly=1, eyes='x'), dict(arm_f=(3, -12), arm_b=(0, -12), lx=-1, rx=2, ry=2, eyes='-')],
    'Thrown': [dict(lean=-5, eyes='x', arm_f=(-6, -8), arm_b=(-8, -6), lx=-4, rx=4, ly=-3)],
    'HoldHand': [dict(lean=1, arm_b=(-10, 2), arm_f=(6, 2))],
    'Squeeze': [dict(thin=True, lean=1, arm_f=(0, 8), arm_b=(0, 8), eyes='-'), dict(thin=True, lean=2, body=1, arm_f=(1, 8), arm_b=(0, 8), eyes='-'),
                dict(thin=True, lean=1, arm_f=(1, 7), arm_b=(0, 7), eyes='-'), dict(thin=True, lean=0, arm_f=(0, 8), arm_b=(0, 8))],
})

venelle = Pal((150, 110, 200, 255), (120, 62, 92, 255), 'VN', long_hair=True, device=True)
make_character('Venelle', venelle, {
    'Attack': [dict(arm_f=(6, -2)), dict(arm_f=(12, -2)), dict(arm_f=(9, 4))],
    'Collapsed': [dict(sit=True, head_tilt=3, eyes='-', arm_f=(2, 10))],
    'Supported': [dict(w, lean=-2, head_tilt=2, eyes='-', arm_b=(-2, -6)) for w in [dict(lx=1, rx=-1), dict(body=-1), dict(lx=-1, rx=1), dict(body=-1)]],
    'Dizzy': [dict(head_tilt=2, eyes='-'), dict(head_tilt=-2, eyes='-')],
    # ---- 복도 첫 만남 (임시): 불균형한 걸음, 몸 낮춤, 돌진, 숨 고르기, 웃음
    'Stalk': [dict(lean=2, lx=3, rx=-2, head_tilt=2), dict(lean=-1, body=1, head_tilt=-1), dict(lean=3, lx=-2, rx=3, ry=-1, head_tilt=2), dict(lean=0, body=2, head_tilt=1),
              dict(lean=2, lx=1, rx=-1, head_tilt=3)],
    'Windup': [dict(crouch=8, lean=4, arm_f=(8, -2), arm_b=(-6, 2), glow=(150, 255, 140, 120)), dict(crouch=9, lean=4, arm_f=(8, -2), arm_b=(-6, 2), glow=(190, 255, 170, 200))],
    'Dash': [dict(lean=6, crouch=4, lx=-5, rx=6, arm_f=(11, -1), arm_b=(-9, -3))],
    'Pant': [dict(lean=3, crouch=3, head_tilt=2, eyes='-', arm_f=(3, 9), arm_b=(3, 9)), dict(lean=3, crouch=4, head_tilt=2, eyes='-', arm_f=(3, 10), arm_b=(3, 10))],
    'Laugh': [dict(head_tilt=-2, arm_f=(4, 2), lean=-1), dict(head_tilt=-3, body=1, arm_f=(5, 0), lean=-2)],
    # ---- 2026-11-15 화학 공격·잡기 (임시). Grab 의 손 위치 = 발 기준 (+13px, -46px) → 코드 VenelleBoss.GrabHand 와 일치
    'Grab': [dict(arm_f=(8, -17), lean=1, liquid=4), dict(arm_f=(8, -17), lean=1, body=1, liquid=4)],
    'Throw': [dict(arm_f=(-6, -14), lean=-2), dict(arm_f=(12, -4), lean=3)],
    'Spray': [dict(arm_f=(12, 5), lean=2, crouch=3, liquid=3), dict(arm_f=(12, 6), lean=2, crouch=4, liquid=2)],
    'Mix': [dict(arm_f=(4, -6), arm_b=(4, -4), bubbles=1, liquid=2), dict(arm_f=(4, -7), arm_b=(4, -5), bubbles=2, liquid=4, body=1), dict(arm_f=(4, -6), arm_b=(4, -4), bubbles=3, liquid=6)],
    'Toss': [dict(arm_f=(-4, -15), lean=-1), dict(arm_f=(10, -10), lean=2)],
    'Downed': [dict(crouch=14, lean=3, head_tilt=3, eyes='-', arm_f=(4, 10), arm_b=(3, 10), liquid=1)],
    'TalkWalk': [dict(w, head_tilt=1, arm_f=(5, 4)) for w in [dict(lx=3, rx=-2), dict(body=-1), dict(lx=-2, rx=3, ry=-1), dict(body=-1)]],
})

BATON_STATES = {
    'Scared': [dict(lean=-2, arm_f=(4, -12), arm_b=(-4, -12))],
    'Talk': [dict(arm_f=(6, 2)), dict(arm_f=(7, -2), body=-1)],
    'BatonOut': [dict(arm_f=(-2, 10)), dict(arm_f=(4, 2), baton='off', baton_dir=(2, -10)), dict(arm_f=(7, -1), baton='on', baton_dir=(4, -11))],
    'BatonIdle': [dict(arm_f=(7, -1), baton='on', baton_dir=(4, -11)), dict(arm_f=(7, 0), body=1, baton='on', baton_dir=(4, -11))],
    'BatonWalk': [dict(w, arm_f=(7, -1), baton='on', baton_dir=(4, -11)) for w in [dict(lx=3, rx=-3, ly=-1), dict(body=-1), dict(lx=-3, rx=3, ry=-1), dict(body=-1)]],
}
researcher = Pal((110, 70, 40, 255), (90, 90, 100, 255), 'RS', coat=True)
make_character('Researcher', researcher, dict(BATON_STATES))
researcher2 = Pal((40, 40, 48, 255), (70, 110, 100, 255), 'R2', coat=True)
make_character('Researcher2', researcher2, dict(BATON_STATES))

# -------------------------------------------------------------- effects
random.seed(47)


def bolt(d, x0, y0, x1, y1, col, segs=6, jit=4, width=1):
    pts = [(x0, y0)]
    for i in range(1, segs):
        t = i / segs
        pts.append((x0 + (x1 - x0) * t + random.randint(-jit, jit), y0 + (y1 - y0) * t + random.randint(-jit, jit)))
    pts.append((x1, y1))
    d.line(pts, fill=col, width=width)


YEL = (255, 238, 120, 255); CYAN = (150, 230, 255, 255); PALE = (255, 255, 230, 255)

# spark 32x32 x6
fr = []
for i in range(6):
    im = Image.new('RGBA', (32, 32), CLEAR); d = ImageDraw.Draw(im)
    n = [2, 4, 5, 4, 2, 1][i]
    for k in range(n):
        a = random.uniform(0, math.pi * 2); r = random.randint(6, 14)
        bolt(d, 16, 16, 16 + math.cos(a) * r, 16 + math.sin(a) * r, YEL if k % 2 else CYAN, 3, 2)
    if i < 4:
        d.ellipse([14 - i // 2, 14 - i // 2, 18 + i // 2, 18 + i // 2], fill=PALE)
    fr.append(im)
save(strip(fr), 'FX_Spark')

# bolt projectile 16x8 x2
fr = []
for i in range(2):
    im = Image.new('RGBA', (16, 8), CLEAR); d = ImageDraw.Draw(im)
    bolt(d, 1, 4, 14, 4, YEL, 4, 2)
    d.rectangle([11, 3, 14, 5], fill=PALE)
    fr.append(im)
save(strip(fr), 'FX_Bolt')

# explosion 32x32 x6
fr = []
for i in range(6):
    im = Image.new('RGBA', (32, 32), CLEAR); d = ImageDraw.Draw(im)
    r = [4, 8, 12, 14, 15, 15][i]
    col = [PALE, PALE, YEL, (255, 180, 80, 255), (150, 150, 170, 200), (120, 120, 140, 120)][i]
    d.ellipse([16 - r, 16 - r, 16 + r, 16 + r], fill=col)
    if i >= 2:
        rr = r - 4
        d.ellipse([16 - rr, 16 - rr, 16 + rr, 16 + rr], fill=CLEAR)
    for k in range(4 if i < 4 else 0):
        a = random.uniform(0, 6.28)
        bolt(d, 16, 16, 16 + math.cos(a) * (r + 3), 16 + math.sin(a) * (r + 3), CYAN, 3, 2)
    fr.append(im)
save(strip(fr), 'FX_Explosion')

# arc whip 64x32 x3
fr = []
for i in range(3):
    im = Image.new('RGBA', (64, 32), CLEAR); d = ImageDraw.Draw(im)
    pts = [(4 + j * 7, 16 - int(math.sin(j / 8 * math.pi + i) * (10 - i * 2))) for j in range(9)]
    for k in range(2):
        jp = [(x, y + random.randint(-2, 2)) for x, y in pts]
        d.line(jp, fill=YEL if k == 0 else CYAN, width=2 if k == 0 else 1)
    fr.append(im)
save(strip(fr), 'FX_Arc')

# heal sparkle 32x32 x4
fr = []
for i in range(4):
    im = Image.new('RGBA', (32, 32), CLEAR)
    for k in range(4):
        x = 6 + (k * 7 + i * 3) % 22; y = 28 - ((i * 6 + k * 5) % 24)
        spark_dot(im, x, y, 1 + (k + i) % 2)
    px = im.load()
    for y in range(32):
        for x in range(32):
            if px[x, y][3]:
                px[x, y] = (150, 255, 170, 255)
    fr.append(im)
save(strip(fr), 'FX_Heal')

# acid splash 32x32 x4
fr = []
for i in range(4):
    im = Image.new('RGBA', (32, 32), CLEAR); d = ImageDraw.Draw(im)
    for k in range(6):
        a = k / 6 * 6.28 + i
        r = 3 + i * 3
        x = 16 + math.cos(a) * r; y = 18 + math.sin(a) * r * 0.6
        d.ellipse([x - 2, y - 2, x + 2, y + 2], fill=(140, 230, 90, 255 - i * 40))
    d.ellipse([12 - i, 14, 20 + i, 22], fill=(110, 200, 70, 230 - i * 50))
    fr.append(im)
save(strip(fr), 'FX_Acid')

# light dot (wire travel) 16x16 x4
fr = []
for i in range(4):
    im = Image.new('RGBA', (16, 16), CLEAR); d = ImageDraw.Draw(im)
    r = 4 + (i % 2)
    d.ellipse([8 - r - 2, 8 - r - 2, 8 + r + 2, 8 + r + 2], fill=(150, 230, 255, 90))
    d.ellipse([8 - r, 8 - r, 8 + r, 8 + r], fill=(255, 250, 180, 255))
    d.ellipse([6, 6, 10, 10], fill=WHITE)
    fr.append(im)
save(strip(fr), 'FX_LightDot')

# -------------------------------------------------------------- props / devices
STEEL = (70, 78, 98, 255); STEEL_L = (110, 120, 145, 255); DARK = (34, 34, 48, 255)


def boltsym(d, x, y, col):
    d.polygon([(x + 3, y), (x, y + 6), (x + 3, y + 6), (x + 1, y + 11), (x + 6, y + 4), (x + 3, y + 4), (x + 5, y)], fill=col)


# power socket: frames idle_on, glow, broken (32x32)
fr = []
for i, st in enumerate(['idle', 'glow', 'broken']):
    im = Image.new('RGBA', (32, 32), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([6, 8, 25, 31], fill=STEEL); d.rectangle([8, 10, 23, 29], fill=DARK)
    col = (255, 220, 60, 255) if st == 'idle' else (255, 255, 200, 255) if st == 'glow' else (110, 100, 80, 255)
    boltsym(d, 12, 13, col)
    d.rectangle([14, 0, 17, 8], fill=(40, 40, 40, 255))  # 굵은 전원선
    if st == 'glow':
        d.rectangle([4, 6, 27, 31], outline=(255, 250, 170, 255))
    if st == 'broken':
        d.line([9, 27, 22, 12], fill=(200, 80, 60, 255))
    fr.append(outline(im))
save(strip(fr), 'Prop_PowerSocket')

# replacement wire coil 32x32 x1
im = Image.new('RGBA', (32, 32), CLEAR); d = ImageDraw.Draw(im)
for k in range(3):
    d.ellipse([6 + k, 14 - k * 2, 26 - k, 30 - k * 2], outline=(40, 40, 40, 255), width=2)
d.rectangle([22, 6, 28, 12], fill=(255, 210, 60, 255))
save(outline(im), 'Prop_WireCoil')

# exposed broken wire 32x32 x4 (spark animation)
fr = []
for i in range(4):
    im = Image.new('RGBA', (32, 32), CLEAR); d = ImageDraw.Draw(im)
    d.line([(0, 30), (10, 29), (17, 27), (21, 24)], fill=(40, 40, 40, 255), width=3)
    d.rectangle([20, 22, 23, 25], fill=(200, 140, 70, 255))
    if i % 2 == 0:
        for k in range(2 + i // 2):
            bolt(d, 23, 23, 23 + random.randint(-6, 8), 23 - random.randint(4, 12), YEL, 3, 2)
    fr.append(outline(im))
save(strip(fr), 'Prop_ExposedWire')

# battery 32x32 frame (level bar drawn by script)
im = Image.new('RGBA', (32, 32), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([4, 6, 27, 31], fill=(60, 120, 80, 255)); d.rectangle([12, 2, 19, 6], fill=STEEL_L)
d.rectangle([7, 9, 24, 13], fill=DARK)  # 잔량 막대 칸
text(im, 7, 18, '+', WHITE); text(im, 19, 18, '-', WHITE)
save(outline(im), 'Prop_Battery')

# switch 16x32 x2 (off, on)
fr = []
for on in (0, 1):
    im = Image.new('RGBA', (16, 32), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([2, 6, 13, 27], fill=STEEL); d.rectangle([4, 8, 11, 25], fill=DARK)
    d.rectangle([5, 9 if on else 17, 10, 16 if on else 24], fill=(90, 220, 110, 255) if on else (200, 70, 70, 255))
    fr.append(outline(im))
save(strip(fr), 'Prop_Switch')

# restore button 16x32 x2 (idle, pressed)
fr = []
for pr in (0, 1):
    im = Image.new('RGBA', (16, 32), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([1, 8, 14, 27], fill=(200, 180, 60, 255))
    d.ellipse([3, 11 + pr, 12, 20 + pr], fill=(220, 60, 60, 255) if not pr else (150, 40, 40, 255))
    boltsym(d, 5, 21, DARK)
    fr.append(outline(im))
save(strip(fr), 'Prop_RestoreButton')

# lever 16x32 x2
fr = []
for up in (1, 0):
    im = Image.new('RGBA', (16, 32), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([3, 20, 12, 31], fill=STEEL)
    d.line([8, 24, 4 if up else 12, 8], fill=STEEL_L, width=2)
    d.ellipse([1 if up else 9, 5, 7 if up else 15, 11], fill=(220, 60, 60, 255))
    fr.append(outline(im))
save(strip(fr), 'Prop_Lever')

# computer 32x32 x4 (on, off, boot, message)
fr = []
for st in ('on', 'off', 'boot', 'msg'):
    im = Image.new('RGBA', (32, 32), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([3, 4, 28, 23], fill=STEEL_L); d.rectangle([5, 6, 26, 21], fill=DARK)
    d.rectangle([13, 23, 18, 27], fill=STEEL); d.rectangle([8, 27, 23, 30], fill=STEEL)
    if st == 'on':
        for k in range(4):
            d.line([7, 8 + k * 3, 7 + random.randint(6, 17), 8 + k * 3], fill=(120, 220, 200, 255))
    elif st == 'boot':
        d.rectangle([9, 13, 22, 15], outline=(120, 220, 200, 255)); d.rectangle([10, 14, 15, 14], fill=(120, 220, 200, 255))
    elif st == 'msg':
        d.rectangle([7, 8, 24, 19], fill=(60, 110, 170, 255)); d.rectangle([9, 10, 22, 12], fill=WHITE); d.rectangle([9, 14, 17, 16], fill=(255, 220, 90, 255))
    fr.append(outline(im))
save(strip(fr), 'Prop_Computer')

# shutter door 32x64 x3 (closed, open, unpowered)
fr = []
for st in ('closed', 'open', 'off'):
    im = Image.new('RGBA', (32, 64), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([0, 0, 31, 63], fill=STEEL)
    if st == 'open':
        d.rectangle([3, 3, 28, 63], fill=(20, 20, 30, 255)); d.rectangle([3, 3, 28, 9], fill=STEEL_L)
    else:
        d.rectangle([3, 3, 28, 63], fill=STEEL_L)
        for k in range(8):
            d.line([3, 8 + k * 7, 28, 8 + k * 7], fill=STEEL)
    d.rectangle([12, 0, 19, 3], fill=(90, 220, 110, 255) if st != 'off' else (120, 40, 40, 255))
    fr.append(outline(im))
save(strip(fr), 'Prop_Door')

# tank 48x64 x2 (intact, broken)
fr = []
for st in ('intact', 'broken'):
    im = Image.new('RGBA', (48, 64), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([4, 0, 43, 7], fill=STEEL); d.rectangle([4, 56, 43, 63], fill=STEEL)
    if st == 'intact':
        d.rectangle([7, 8, 40, 55], fill=(80, 200, 220, 110))
        d.rectangle([7, 8, 40, 14], fill=(160, 240, 255, 90))
        d.line([11, 12, 11, 50], fill=(220, 250, 255, 160))
    else:
        d.polygon([(7, 55), (7, 40), (12, 44), (16, 36), (20, 55)], fill=(180, 230, 250, 120))
        d.polygon([(40, 55), (40, 30), (35, 38), (31, 33), (28, 55)], fill=(180, 230, 250, 120))
        d.rectangle([5, 52, 42, 55], fill=(80, 200, 220, 120))
    fr.append(im)
save(strip(fr), 'Prop_Tank')

# lights: ceiling 32x8 x2 (on, off), emergency 16x16 x2 (on, off)
fr = []
for on in (1, 0):
    im = Image.new('RGBA', (32, 8), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([2, 0, 29, 3], fill=STEEL); d.rectangle([4, 4, 27, 6], fill=(255, 250, 200, 255) if on else (60, 60, 70, 255))
    fr.append(im)
save(strip(fr), 'Prop_CeilingLight')
fr = []
for on in (1, 0):
    im = Image.new('RGBA', (16, 16), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([2, 6, 13, 13], fill=STEEL); d.ellipse([4, 1, 11, 9], fill=(255, 90, 60, 255) if on else (90, 50, 50, 255))
    d.rectangle([10, 10, 13, 13], fill=(60, 120, 80, 255))  # 배터리 표식
    fr.append(outline(im))
save(strip(fr), 'Prop_EmergencyLight')

# vending machine 32x64 x3 (on, off, broken)
fr = []
for st in ('on', 'off', 'broken'):
    im = Image.new('RGBA', (32, 64), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([1, 2, 30, 63], fill=(170, 50, 60, 255))
    d.rectangle([4, 6, 21, 40], fill=(120, 200, 230, 255) if st == 'on' else (40, 50, 60, 255))
    for r in range(4):
        for c in range(3):
            d.rectangle([6 + c * 5, 9 + r * 8, 9 + c * 5, 13 + r * 8], fill=[(240, 200, 90, 255), (230, 120, 90, 255), (140, 200, 120, 255)][c])
    d.rectangle([23, 10, 28, 30], fill=STEEL_L); d.rectangle([5, 46, 26, 54], fill=DARK)
    if st == 'broken':
        d.line([4, 6, 21, 40], fill=(255, 240, 200, 255))
    fr.append(outline(im))
save(strip(fr), 'Prop_Vending')

# home & misc props (single frames)


def single(name, w, h, fn):
    im = Image.new('RGBA', (w, h), CLEAR); d = ImageDraw.Draw(im); fn(im, d); save(outline(im), name)


single('Prop_Mirror', 32, 48, lambda im, d: (d.rectangle([4, 2, 27, 45], fill=(140, 110, 80, 255)), d.rectangle([7, 5, 24, 42], fill=(170, 210, 230, 255)), d.line([(9, 8), (16, 22), (13, 30), (20, 40)], fill=WHITE)))
single('Prop_Cupboard', 32, 48, lambda im, d: (d.rectangle([2, 2, 29, 47], fill=(130, 90, 60, 255)), d.rectangle([5, 5, 14, 44], fill=(150, 105, 70, 255)), d.rectangle([17, 5, 26, 44], fill=(150, 105, 70, 255))))
single('Prop_Cups', 16, 16, lambda im, d: (d.rectangle([1, 6, 6, 14], fill=(220, 220, 210, 255)), d.rectangle([9, 6, 14, 14], fill=(200, 210, 230, 255)), d.point([(13, 7)], fill=CLEAR)))
single('Prop_Clothes', 32, 32, lambda im, d: (d.polygon([(6, 6), (26, 6), (29, 14), (24, 14), (24, 29), (8, 29), (8, 14), (3, 14)], fill=(90, 120, 170, 255)), d.rectangle([12, 16, 19, 22], fill=(170, 140, 100, 255))))
single('Prop_Desk', 48, 32, lambda im, d: (d.rectangle([0, 10, 47, 14], fill=(120, 85, 60, 255)), d.rectangle([3, 14, 6, 31], fill=(100, 70, 50, 255)), d.rectangle([41, 14, 44, 31], fill=(100, 70, 50, 255))))
single('Prop_Bed', 64, 32, lambda im, d: (d.rectangle([0, 16, 63, 26], fill=(110, 80, 60, 255)), d.rectangle([2, 12, 61, 18], fill=(200, 200, 215, 255)), d.rectangle([2, 8, 14, 16], fill=(235, 235, 240, 255)), d.rectangle([0, 26, 3, 31], fill=(90, 60, 40, 255)), d.rectangle([60, 26, 63, 31], fill=(90, 60, 40, 255))))
single('Prop_Bench', 48, 24, lambda im, d: (d.rectangle([0, 8, 47, 12], fill=STEEL_L), d.rectangle([3, 12, 6, 23], fill=STEEL), d.rectangle([41, 12, 44, 23], fill=STEEL)))
single('Prop_Stone', 16, 16, lambda im, d: d.ellipse([1, 5, 14, 15], fill=(130, 125, 120, 255)))
single('Prop_Snack', 16, 16, lambda im, d: (d.rectangle([2, 4, 13, 12], fill=(230, 160, 60, 255)), d.rectangle([5, 6, 10, 10], fill=(120, 70, 40, 255))))
single('Prop_Doc', 16, 16, lambda im, d: (d.rectangle([3, 1, 12, 14], fill=(235, 230, 210, 255)), d.line([5, 4, 10, 4], fill=STEEL), d.line([5, 7, 10, 7], fill=STEEL), d.line([5, 10, 9, 10], fill=STEEL)))
single('Prop_Water', 16, 16, lambda im, d: (d.rectangle([5, 3, 10, 14], fill=(150, 200, 240, 200)), d.rectangle([6, 1, 9, 3], fill=(60, 120, 200, 255))))


def emblem_placeholder(im, d):
    # 사용자 로고 원안 자리. 로고를 임의로 그리지 않고 'LOGO ?' 표시만 둔다.
    d.rectangle([0, 0, 47, 47], fill=(225, 220, 205, 255))
    d.rectangle([8, 8, 39, 39], outline=(60, 60, 70, 255))
    text(im, 16, 16, 'LOGO', (60, 60, 70, 255)); text(im, 22, 24, '?', (200, 60, 60, 255))


single('Prop_PosterEmblem', 48, 48, emblem_placeholder)


def window(im, d):
    d.rectangle([0, 0, 95, 63], fill=STEEL)
    d.rectangle([4, 4, 91, 59], fill=(40, 70, 90, 255))
    for x in (20, 46, 70):  # 유리 너머 사람의 흐린 실루엣 (임시)
        d.ellipse([x, 22, x + 9, 31], fill=(70, 100, 120, 255)); d.rectangle([x - 1, 31, x + 10, 55], fill=(70, 100, 120, 255))
    d.line([8, 8, 30, 30], fill=(140, 190, 210, 180))


single('Prop_GlassWindow', 96, 64, window)
single('Prop_LaundryLine', 64, 48, lambda im, d: (d.rectangle([0, 8, 2, 47], fill=STEEL), d.rectangle([61, 8, 63, 47], fill=STEEL), d.line([2, 10, 61, 10], fill=(200, 200, 200, 255)), d.rectangle([10, 10, 22, 26], fill=(90, 120, 170, 255)), d.rectangle([30, 10, 44, 22], fill=(220, 200, 150, 255))))
single('Prop_LaundryLineEmpty', 64, 48, lambda im, d: (d.rectangle([0, 8, 2, 47], fill=STEEL), d.rectangle([61, 8, 63, 47], fill=STEEL), d.line([2, 10, 61, 10], fill=(200, 200, 200, 255))))
single('Prop_Messenger', 64, 48, lambda im, d: (d.rectangle([0, 0, 63, 47], fill=(220, 225, 235, 255)), d.rectangle([0, 0, 63, 7], fill=(60, 110, 170, 255)), d.rectangle([4, 12, 40, 18], fill=(200, 205, 215, 255)), d.rectangle([4, 24, 30, 30], fill=(250, 210, 90, 255)), d.rectangle([4, 38, 59, 44], fill=(255, 255, 255, 255))))
single('Prop_AddressPanel', 48, 32, lambda im, d: (d.rectangle([0, 0, 47, 31], fill=(30, 50, 90, 255)), d.rectangle([3, 4, 44, 9], fill=(120, 200, 255, 255)), d.rectangle([3, 14, 30, 18], fill=(120, 255, 160, 255)), d.rectangle([3, 22, 24, 26], fill=(120, 200, 255, 255))))
single('Prop_Puddle', 64, 8, lambda im, d: d.ellipse([0, 1, 63, 7], fill=(120, 220, 90, 200)))
single('Prop_Rubble', 64, 32, lambda im, d: d.polygon([(0, 31), (8, 14), (20, 18), (30, 6), (44, 16), (56, 10), (63, 31)], fill=(110, 105, 100, 255)))
single('Prop_Ladder', 32, 32, lambda im, d: (d.rectangle([5, 0, 8, 31], fill=(150, 160, 180, 255)), d.rectangle([23, 0, 26, 31], fill=(150, 160, 180, 255)), d.rectangle([8, 6, 23, 8], fill=(120, 130, 150, 255)), d.rectangle([8, 22, 23, 24], fill=(120, 130, 150, 255))))

# -------------------------------------------------------------- terrain / backgrounds (tileable 32x32)


def tile(name, fn, w=32, h=32):
    im = Image.new('RGBA', (w, h), CLEAR); d = ImageDraw.Draw(im); fn(im, d); save(im, name)


tile('Tile_HomeWall', lambda im, d: (d.rectangle([0, 0, 31, 31], fill=(92, 72, 70, 255)), d.line([0, 15, 31, 15], fill=(82, 63, 62, 255)), d.line([16, 0, 16, 15], fill=(82, 63, 62, 255)), d.line([4, 16, 4, 31], fill=(82, 63, 62, 255)), d.point([(10, 6), (24, 22)], fill=(110, 90, 86, 255))))
tile('Tile_HomeFloor', lambda im, d: (d.rectangle([0, 0, 31, 31], fill=(120, 85, 60, 255)), d.rectangle([0, 0, 31, 3], fill=(160, 120, 85, 255)), d.line([0, 12, 31, 12], fill=(100, 70, 50, 255)), d.line([0, 22, 31, 22], fill=(100, 70, 50, 255)), d.line([12, 4, 12, 12], fill=(100, 70, 50, 255))))
tile('Tile_Digital', lambda im, d: (d.rectangle([0, 0, 31, 31], fill=(18, 26, 56, 255)), d.line([0, 0, 31, 0], fill=(40, 70, 130, 255)), d.line([0, 0, 0, 31], fill=(40, 70, 130, 255)), d.point([(16, 16)], fill=(90, 150, 230, 255))))
tile('Tile_DigitalBlock', lambda im, d: (d.rectangle([0, 0, 31, 31], fill=(30, 60, 120, 255)), d.rectangle([0, 0, 31, 2], fill=(120, 220, 255, 255)), d.rectangle([2, 6, 29, 29], outline=(50, 90, 160, 255))))
tile('Tile_Street', lambda im, d: (d.rectangle([0, 0, 31, 31], fill=(58, 56, 66, 255)), d.rectangle([0, 0, 31, 3], fill=(96, 92, 104, 255)), d.point([(6, 12), (20, 18), (27, 9), (12, 26)], fill=(76, 74, 86, 255))))
tile('Tile_Roof', lambda im, d: (d.rectangle([0, 0, 31, 31], fill=(70, 66, 80, 255)), d.rectangle([0, 0, 31, 4], fill=(130, 120, 110, 255)), d.line([0, 4, 31, 4], fill=(50, 46, 58, 255))))
tile('Tile_Platform', lambda im, d: (d.rectangle([0, 0, 31, 7], fill=STEEL), d.rectangle([0, 0, 31, 1], fill=STEEL_L), [d.rectangle([x, 3, x + 3, 5], fill=(230, 190, 60, 255)) for x in range(0, 32, 8)]), 32, 8)
tile('Tile_Pixel', lambda im, d: d.rectangle([0, 0, 31, 31], fill=WHITE), 4, 4)
print('placeholder sprites written to', OUT)
