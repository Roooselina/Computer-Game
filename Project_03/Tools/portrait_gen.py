# -*- coding: utf-8 -*-
"""
ELECTRIC WORLD: LAST LIGHT - 임시 스탠딩 일러스트 생성기.

사용자가 그릴 최종 스탠딩 일러스트를 대신하는 '임시 그림'이다. 얼굴 위치·비율·표정 교체·
좌우 배치를 시험하기 위한 것이며, 그림마다 '임시 스탠딩' 문구를 넣는다.
도트 월드와 달리 고해상도(부드러운 필터)로 표시된다.

실행: python Tools/portrait_gen.py   (Pillow 필요, Windows 맑은 고딕 글꼴 사용)
출력: Assets/Game/Art/Portraits/<인물>_<표정>.png  (600×900, 투명 배경, 발 대신 허리 아래가 잘린 반신)
규격: Docs/ART_REQUESTS.md 의 '스탠딩' 항목.
"""
import os
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'Assets', 'Game', 'Art', 'Portraits')
os.makedirs(OUT, exist_ok=True)
W, H = 600, 900
SS = 2  # 슈퍼샘플링

FONT_PATH = r'C:\Windows\Fonts\malgunbd.ttf'
if not os.path.exists(FONT_PATH):
    FONT_PATH = r'C:\Windows\Fonts\malgun.ttf'


def font(sz):
    try:
        return ImageFont.truetype(FONT_PATH, sz * SS)
    except Exception:
        return ImageFont.load_default()


def S(*v):
    return [x * SS for x in v]


CHARS = {
    # id: (머리색, 옷색, 피부, 표시 이름, 머리 모양)
    'lu': ((246, 208, 96), (68, 90, 140), (245, 214, 190), '루', 'short'),
    'venelle': ((150, 112, 200), (124, 64, 96), (240, 214, 200), '베넬', 'long'),
    'researcher': ((110, 72, 44), (226, 228, 236), (236, 206, 182), '연구원 1', 'neat'),
    'researcher2': ((44, 44, 54), (226, 232, 230), (226, 196, 172), '연구원 2', 'bun'),
}

EXPR = {
    'lu': ['neutral', 'sad', 'despair', 'surprised', 'scared', 'determined', 'smile', 'pained', 'angry_cry'],
    'venelle': ['gentle', 'blank', 'excited', 'confused', 'quiet'],
    'researcher': ['smile', 'neutral', 'smirk'],
    'researcher2': ['neutral', 'smirk', 'surprised'],
}

EXPR_KO = {
    'neutral': '보통', 'sad': '슬픔', 'despair': '절망', 'surprised': '놀람', 'scared': '겁먹음', 'determined': '결심',
    'smile': '웃음', 'pained': '아픔', 'angry_cry': '울며 화냄', 'gentle': '상냥', 'blank': '멍함', 'excited': '들뜸',
    'confused': '혼란', 'quiet': '기억의 정적', 'smirk': '능청',
}


def face(d, cx, cy, r, expr, ink=(40, 30, 50)):
    """얼굴 중심 (cx, cy), 반지름 r. 표정만 그린다."""
    ex, ey = r * 0.38, cy + r * 0.05
    lw = max(3, int(r * 0.05))
    def eye_open(x, big=1.0):
        d.ellipse(S(x - r * 0.09 * big, ey - r * 0.14 * big, x + r * 0.09 * big, ey + r * 0.14 * big), fill=ink)
        d.ellipse(S(x - r * 0.03, ey - r * 0.1 * big, x + r * 0.02, ey - r * 0.04 * big), fill=(255, 255, 255))
    def eye_closed(x, curve_up=False):
        if curve_up:
            d.arc(S(x - r * 0.12, ey - r * 0.12, x + r * 0.12, ey + r * 0.06), 200, 340, fill=ink, width=lw * SS)
        else:
            d.arc(S(x - r * 0.12, ey - r * 0.06, x + r * 0.12, ey + r * 0.12), 20, 160, fill=ink, width=lw * SS)
    def brows(tilt):
        for sx in (-1, 1):
            x0 = cx + sx * ex
            d.line(S(x0 - r * 0.12, ey - r * 0.28 + tilt * sx * -1 * r * 0.06, x0 + r * 0.12, ey - r * 0.28 - tilt * sx * -1 * r * 0.06), fill=ink, width=lw * SS)
    mx, my = cx, cy + r * 0.48
    if expr in ('neutral', 'blank', 'quiet'):
        for sx in (-1, 1): eye_open(cx + sx * ex, 0.9 if expr == 'blank' else 1.0)
        d.line(S(mx - r * 0.1, my, mx + r * 0.1, my), fill=ink, width=lw * SS)
        if expr == 'blank':
            d.ellipse(S(cx - r * 0.5, cy - r * 0.6, cx - r * 0.42, cy - r * 0.52), fill=(255, 255, 255))
    elif expr in ('sad', 'despair', 'pained'):
        for sx in (-1, 1):
            if expr == 'pained': eye_closed(cx + sx * ex)
            else: eye_open(cx + sx * ex, 0.8)
        brows(1)
        d.arc(S(mx - r * 0.13, my - r * 0.02, mx + r * 0.13, my + r * 0.16), 200, 340, fill=ink, width=lw * SS)
        if expr == 'despair':
            d.line(S(cx + ex, ey + r * 0.16, cx + ex - r * 0.02, ey + r * 0.5), fill=(150, 200, 255), width=lw * SS)
    elif expr in ('surprised', 'scared'):
        for sx in (-1, 1): eye_open(cx + sx * ex, 1.2)
        brows(1)
        d.ellipse(S(mx - r * 0.07, my - r * 0.06, mx + r * 0.07, my + r * 0.1), fill=ink)
        if expr == 'scared':
            for k in range(3):
                d.line(S(cx + r * 0.75, cy - r * 0.5 + k * r * 0.12, cx + r * 0.85, cy - r * 0.45 + k * r * 0.12), fill=(120, 160, 220), width=lw * SS)
    elif expr in ('determined', 'angry_cry'):
        for sx in (-1, 1):
            if expr == 'angry_cry': eye_closed(cx + sx * ex)
            else: eye_open(cx + sx * ex)
        brows(-1.5)
        d.line(S(mx - r * 0.12, my, mx + r * 0.12, my), fill=ink, width=lw * SS)
        if expr == 'angry_cry':
            for sx in (-1, 1):
                d.line(S(cx + sx * ex, ey + r * 0.1, cx + sx * ex, ey + r * 0.45), fill=(150, 200, 255), width=lw * SS)
            d.ellipse(S(mx - r * 0.12, my - r * 0.05, mx + r * 0.12, my + r * 0.12), fill=ink)
    elif expr in ('smile', 'gentle', 'excited', 'smirk'):
        for sx in (-1, 1):
            if expr in ('smile', 'gentle'): eye_closed(cx + sx * ex, True)
            else: eye_open(cx + sx * ex)
        if expr == 'smirk':
            d.arc(S(mx - r * 0.12, my - r * 0.12, mx + r * 0.16, my + r * 0.06), 20, 120, fill=ink, width=lw * SS)
        else:
            d.chord(S(mx - r * 0.14, my - r * 0.1, mx + r * 0.14, my + r * 0.14), 0, 180, fill=ink)
    elif expr == 'confused':
        eye_open(cx - ex, 1.1); eye_closed(cx + ex)
        d.line(S(mx - r * 0.12, my, mx - r * 0.04, my + r * 0.05, mx + r * 0.04, my - r * 0.03, mx + r * 0.12, my + r * 0.03), fill=ink, width=lw * SS)
        d.text(S(cx + r * 0.7, cy - r * 1.0), '?', font=font(int(r * 0.5)), fill=ink)
    if expr in ('smile', 'gentle', 'excited', 'scared', 'surprised'):
        for sx in (-1, 1):
            d.ellipse(S(cx + sx * r * 0.6 - r * 0.1, cy + r * 0.28, cx + sx * r * 0.6 + r * 0.1, cy + r * 0.36), fill=(240, 150, 150))


def portrait(cid, expr):
    hair, cloth, skin, name, style = CHARS[cid]
    img = Image.new('RGBA', (W * SS, H * SS), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    outline = (36, 28, 44)
    cx, cy, r = 300, 330, 150    # 큰 머리 비율 (머리:몸 = 약 1:1.6)
    # 몸 (반신)
    d.rounded_rectangle(S(cx - 150, cy + 150, cx + 150, H + 40), 60 * SS, fill=cloth, outline=outline, width=6 * SS)
    d.polygon(S(cx - 40, cy + 150, cx, cy + 230, cx + 40, cy + 150), fill=skin, outline=outline)
    if cid.startswith('researcher'):
        d.line(S(cx, cy + 230, cx, H), fill=(180, 184, 196), width=6 * SS)
        d.rectangle(S(cx + 50, cy + 260, cx + 110, cy + 300), fill=(120, 170, 200), outline=outline, width=3 * SS)  # 출입증
    if cid == 'lu':
        d.rectangle(S(cx - 90, cy + 300, cx - 40, cy + 330), fill=(150, 130, 120))   # 덧댄 천
    if cid == 'venelle':
        d.rounded_rectangle(S(cx + 120, cy + 240, cx + 200, cy + 400), 18 * SS, fill=(90, 190, 130), outline=outline, width=4 * SS)  # 팔 장치 (오른쪽)
    # 뒷머리
    if style == 'long':
        d.rounded_rectangle(S(cx - r - 20, cy - r * 0.6, cx + r + 20, cy + r * 1.6), 80 * SS, fill=hair, outline=outline, width=6 * SS)
    if style == 'bun':
        d.ellipse(S(cx - 60, cy - r - 70, cx + 60, cy - r + 30), fill=hair, outline=outline, width=6 * SS)
    # 얼굴
    d.ellipse(S(cx - r, cy - r, cx + r, cy + r), fill=skin, outline=outline, width=6 * SS)
    # 앞머리
    if style in ('short', 'long'):
        d.chord(S(cx - r - 8, cy - r - 14, cx + r + 8, cy + r * 0.5), 180, 360, fill=hair, outline=outline, width=6 * SS)
        for k in range(5):
            x = cx - r + 30 + k * 60
            d.polygon(S(x - 30, cy - r * 0.3, x, cy - r * 0.05 + (k % 2) * 20, x + 30, cy - r * 0.3), fill=hair)
        if style == 'short':
            d.polygon(S(cx - r - 6, cy - 40, cx - r + 26, cy - 40, cx - r + 10, cy + 70), fill=hair, outline=outline)
            d.polygon(S(cx + r + 6, cy - 40, cx + r - 26, cy - 40, cx + r - 10, cy + 70), fill=hair, outline=outline)
    else:
        d.chord(S(cx - r - 4, cy - r - 8, cx + r + 4, cy + r * 0.2), 180, 360, fill=hair, outline=outline, width=6 * SS)
    face(d, cx, cy, r, expr)
    # 임시 표시
    tag = '임시 스탠딩 · ' + name + ' · ' + EXPR_KO.get(expr, expr)
    d.rounded_rectangle(S(20, H - 70, W - 20, H - 18), 12 * SS, fill=(0, 0, 0, 150))
    d.text(S(36, H - 64), tag, font=font(30), fill=(255, 240, 160))
    img = img.resize((W, H), Image.LANCZOS)
    img.save(os.path.join(OUT, '%s_%s.png' % (cid, expr)))


if __name__ == '__main__':
    n = 0
    for cid, ex in EXPR.items():
        for e in ex:
            portrait(cid, e); n += 1
    print('portraits', n, '->', OUT)
