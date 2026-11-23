# -*- coding: utf-8 -*-
"""
ELECTRIC WORLD: LAST LIGHT - 장면용 '임시 블록아웃' 도트 생성기.

판타지 마을·무너진 집·친구 컴퓨터 바탕화면·실험실의 깊이 레이어(원경/중경/근경/전경)와 소품.
보유 에셋에 판타지 마을 그림이 없고 유료 팩은 쓰지 않으므로, 구도와 연출을 잡기 위한 임시 그림이다.
연구소 타일을 색만 바꿔 마을로 쓰지 않는다. 최종 아트 목록과 규격은 Docs/ART_REQUESTS.md.

실행: python Tools/scene_art_gen.py   (Pillow 필요)
출력: Assets/Game/Art/Placeholder/Scene/*.png  (PPU 32, Point 필터)
"""
import os, math, random
from PIL import Image, ImageDraw, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'Assets', 'Game', 'Art', 'Placeholder', 'Scene')
os.makedirs(OUT, exist_ok=True)
CLEAR = (0, 0, 0, 0)
R = random.Random(2047)


def save(img, name):
    img.save(os.path.join(OUT, name + '.png'))


def strip(frames):
    w, h = frames[0].size
    s = Image.new('RGBA', (w * len(frames), h), CLEAR)
    for i, f in enumerate(frames):
        s.paste(f, (i * w, 0))
    return s


def lerp(a, b, k):
    return tuple(int(a[i] + (b[i] - a[i]) * k) for i in range(len(a)))


def col(c, a=255):
    return (c[0], c[1], c[2], a)


def dither_rect(d, box, c1, c2, vertical=True):
    """두 색을 4단 체크 디더로 섞은 그라데이션."""
    x0, y0, x1, y1 = box
    for y in range(y0, y1):
        k = (y - y0) / max(1, (y1 - y0 - 1))
        for x in range(x0, x1):
            kk = k if vertical else (x - x0) / max(1, (x1 - x0 - 1))
            q = kk * 4
            base = int(q)
            frac = q - base
            bayer = ((x & 1) * 2 + (y & 1)) / 4.0
            step = base + (1 if frac > bayer else 0)
            d.point((x, y), fill=col(lerp(c1, c2, min(1, step / 4))))


# ================================================================ 공통
def glow(size, color, falloff=2.0):
    """가산 혼합용 부드러운 원형 빛 (빛 표현에만 사용, 픽셀 아트 아님)."""
    im = Image.new('RGBA', (size, size), CLEAR)
    px = im.load()
    c = size / 2
    for y in range(size):
        for x in range(size):
            r = math.hypot(x + 0.5 - c, y + 0.5 - c) / c
            if r < 1:
                a = int(255 * (1 - r) ** falloff)
                px[x, y] = (color[0], color[1], color[2], a)
    return im


save(glow(64, (255, 255, 255)), 'FX_Glow')
save(glow(64, (255, 255, 255), 1.0), 'FX_GlowSoft')

# 2px 입자
for name, c in (('FX_Ember', (255, 170, 60)), ('FX_Ash', (150, 140, 140)), ('FX_Dust', (190, 190, 200))):
    im = Image.new('RGBA', (2, 2), col(c)); save(im, name)

# 연기 덩어리 32x32 x4 (부드럽지만 도트 경계를 유지하도록 양자화)
fr = []
for i in range(4):
    im = Image.new('RGBA', (32, 32), CLEAR); d = ImageDraw.Draw(im)
    for k in range(6):
        rx = R.randint(6, 12); cx = R.randint(10, 22); cy = R.randint(10, 22)
        d.ellipse([cx - rx, cy - rx, cx + rx, cy + rx], fill=(60, 52, 56, 120 + i * 10))
    for k in range(4):
        rx = R.randint(3, 7); cx = R.randint(10, 22); cy = R.randint(8, 18)
        d.ellipse([cx - rx, cy - rx, cx + rx, cy + rx], fill=(90, 80, 84, 140))
    fr.append(im)
save(strip(fr), 'FX_Smoke')

# 불 16x24 x6, 32x48 x6
def fire_frames(w, h, n=6, seed=1):
    rr = random.Random(seed)
    out = []
    for i in range(n):
        im = Image.new('RGBA', (w, h), CLEAR); px = im.load()
        for y in range(h):
            for x in range(w):
                cx = w / 2 + math.sin((y * 0.35) + i * 1.1) * w * 0.12
                width = (w * 0.45) * (y / h) ** 0.7
                dx = abs(x + 0.5 - cx)
                if dx < width:
                    k = 1 - dx / max(1, width)
                    heat = (y / h) * 0.6 + k * 0.6 + rr.uniform(-0.15, 0.15)
                    if heat > 0.95: c = (255, 245, 190)
                    elif heat > 0.75: c = (255, 200, 70)
                    elif heat > 0.5: c = (240, 110, 30)
                    elif heat > 0.32: c = (180, 50, 30)
                    else: continue
                    px[x, y] = col(c)
        # 위로 튀는 불티
        for k in range(3):
            x = rr.randint(2, w - 3); y = rr.randint(0, h // 3)
            px[x, y] = col((255, 200, 90))
        out.append(im)
    return out


save(strip(fire_frames(16, 24, 6, 3)), 'FX_Fire')
save(strip(fire_frames(32, 48, 6, 9)), 'FX_FireBig')

# ================================================================ 불타는 마을
SKY_TOP = (22, 13, 32); SKY_MID = (100, 36, 44); SKY_LOW = (238, 128, 60)

im = Image.new('RGBA', (4, 360), CLEAR); d = ImageDraw.Draw(im)
for y in range(360):
    # 가로로 늘려 쓰므로 디더 없이 단계(밴드)만 둔 세로 그라데이션
    k = round((y / 359) * 20) / 20
    c = lerp(SKY_TOP, SKY_MID, k / 0.55) if k < 0.55 else lerp(SKY_MID, SKY_LOW, (k - 0.55) / 0.45)
    d.line([0, y, 3, y], fill=col(c))
save(im, 'V_Sky')


def gable_house(d, x, base, w, h, roof, wall, dark, windows=True, broken=False, glowc=(255, 150, 60)):
    top = base - h
    d.rectangle([x, top, x + w, base], fill=col(wall))
    # 지붕
    rh = int(w * 0.55)
    if broken:
        d.polygon([(x - 3, top), (x + w * 0.35, top - rh * 0.8), (x + w * 0.5, top - rh * 0.3), (x + w * 0.62, top - rh * 0.7), (x + w + 3, top)], fill=col(roof))
    else:
        d.polygon([(x - 3, top), (x + w / 2, top - rh), (x + w + 3, top)], fill=col(roof))
    # 목조 골조
    for k in range(1, 3):
        xx = x + w * k // 3
        d.line([xx, top, xx, base], fill=col(dark))
    d.line([x, top + h // 2, x + w, top + h // 2], fill=col(dark))
    if windows:
        for k in range(2):
            wx = x + 4 + k * (w // 2)
            d.rectangle([wx, top + 5, wx + 5, top + 11], fill=col(glowc))


def far_town():
    W, H = 640, 220
    im = Image.new('RGBA', (W, H), CLEAR); d = ImageDraw.Draw(im)
    base = H - 6
    # 맨 뒤 성벽·탑 (가장 어두운 실루엣)
    sil = (70, 30, 44)
    d.rectangle([0, base - 40, W, base], fill=col(sil))
    for tx in (60, 250, 470):
        d.rectangle([tx, base - 110, tx + 22, base], fill=col(sil))
        d.polygon([(tx - 4, base - 110), (tx + 11, base - 140), (tx + 26, base - 110)], fill=col(sil))
        for k in range(3):
            d.rectangle([tx + 3 + k * 7, base - 116, tx + 6 + k * 7, base - 110], fill=CLEAR)
    # 이빨 모양 흉벽
    for x in range(0, W, 12):
        d.rectangle([x, base - 46, x + 6, base - 40], fill=col(sil))
    # 중간 지붕들
    x = 0
    rr = random.Random(5)
    while x < W:
        w = rr.randint(26, 44); h = rr.randint(18, 34)
        gable_house(d, x, base, w, h, (58, 26, 40), (52, 24, 36), (40, 18, 30), windows=rr.random() < 0.5, broken=rr.random() < 0.4, glowc=(250, 140, 60))
        x += w + rr.randint(2, 8)
    # 지평선 불빛
    for x in range(0, W):
        k = 0.5 + 0.5 * math.sin(x * 0.07) * math.sin(x * 0.013 + 1)
        hh = int(4 + 10 * k)
        for y in range(base - hh, base + 6):
            if (x + y) % 2 == 0 or y > base - hh + 2:
                d.point((x, y), fill=col(lerp((200, 70, 30), (255, 160, 60), k)))
    # 연기 기둥
    for cx in (130, 340, 560):
        for y in range(0, base - 30):
            ww = 8 + (base - 30 - y) * 0.06 + 6 * math.sin(y * 0.05 + cx)
            sx = cx + math.sin(y * 0.03 + cx) * 10
            for x in range(int(sx - ww), int(sx + ww)):
                if 0 <= x < W and (x * 3 + y) % 4 != 0:
                    a = int(170 * (y / base))
                    d.point((x, y), fill=(60, 40, 48, a))
    return im


save(far_town(), 'V_Far')


def mid_house(w, h, burning, seed):
    rr = random.Random(seed)
    im = Image.new('RGBA', (w, h + w // 2 + 4), CLEAR); d = ImageDraw.Draw(im)
    base = im.height - 1
    wall = (96, 64, 52); dark = (54, 34, 30); roof = (70, 38, 40)
    top = base - h
    d.rectangle([2, top, w - 3, base], fill=col(wall))
    # 기둥·대들보 (판타지 목조 주택)
    for k in range(0, w, 14):
        d.rectangle([2 + k, top, 4 + k, base], fill=col(dark))
    for y in (top, top + h // 2, base - 2):
        d.rectangle([2, y, w - 3, y + 2], fill=col(dark))
    d.line([4, top + 3, w // 2, top + h // 2 - 2], fill=col(dark), width=2)
    d.line([w - 5, top + 3, w // 2, top + h // 2 - 2], fill=col(dark), width=2)
    # 지붕 (일부 무너짐)
    rh = w // 2
    pts = [(0, top + 1), (w // 2, top - rh + 4), (w - 1, top + 1)]
    d.polygon(pts, fill=col(roof))
    for y in range(top - rh + 6, top, 3):
        d.line([w // 2 - (y - (top - rh + 4)), y, w // 2 + (y - (top - rh + 4)), y], fill=col((56, 28, 32)))
    if burning:
        # 구멍 난 지붕과 그을음
        hx = rr.randint(w // 4, w // 2)
        d.polygon([(hx, top - 2), (hx + 12, top - rh // 2), (hx + 22, top - 3)], fill=CLEAR)
        for k in range(40):
            x = rr.randint(2, w - 3); y = rr.randint(top, base)
            d.point((x, y), fill=col((30, 20, 20)))
    # 창 (안에서 비치는 불빛)
    for k in range(2):
        wx = 10 + k * (w - 30)
        c = (255, 150, 60) if burning else (60, 40, 40)
        d.rectangle([wx, top + 8, wx + 8, top + 18], fill=col(c))
        d.line([wx + 4, top + 8, wx + 4, top + 18], fill=col(dark))
    # 문
    d.rectangle([w // 2 - 6, base - 18, w // 2 + 6, base], fill=col((40, 24, 22)))
    return im


save(mid_house(80, 56, True, 1), 'V_HouseA')
save(mid_house(64, 44, False, 2), 'V_HouseB')
save(mid_house(96, 60, True, 3), 'V_HouseC')


def ruin_wall(w, h, seed):
    rr = random.Random(seed)
    im = Image.new('RGBA', (w, h), CLEAR); d = ImageDraw.Draw(im)
    stone = (110, 96, 92); dark = (70, 60, 60)
    prof = [h - rr.randint(int(h * 0.3), h) for _ in range(w // 6 + 2)]
    for x in range(w):
        top = prof[x // 6]
        for y in range(top, h):
            c = stone if ((x // 8 + y // 5) % 2 == 0) else (100, 88, 84)
            if y % 5 == 0 or (x + (y // 5) * 4) % 8 == 0: c = dark
            d.point((x, y), fill=col(c))
    return im


save(ruin_wall(64, 48, 4), 'V_RuinWall')

# 땅 타일
im = Image.new('RGBA', (32, 32), col((58, 44, 40))); d = ImageDraw.Draw(im)
for x in range(0, 32, 8):
    d.rectangle([x + 1, 0, x + 6, 3], fill=col((96, 80, 70)))
    d.rectangle([x + 5 if x < 24 else x - 3, 5, x + 9 if x < 24 else x + 1, 7], fill=col((80, 66, 60)))
d.line([0, 0, 31, 0], fill=col((130, 110, 92)))
for k in range(20):
    d.point((R.randint(0, 31), R.randint(9, 31)), fill=col((44, 34, 32)))
save(im, 'V_Ground')
im = Image.new('RGBA', (32, 32), col((38, 28, 28)))
d = ImageDraw.Draw(im)
for k in range(24):
    d.point((R.randint(0, 31), R.randint(0, 31)), fill=col((30, 22, 22)))
save(im, 'V_GroundFill')


def rubble(w, h, seed, dark=False):
    rr = random.Random(seed)
    im = Image.new('RGBA', (w, h), CLEAR); d = ImageDraw.Draw(im)
    for k in range(w // 3):
        x = rr.randint(0, w - 8); s = rr.randint(4, 10)
        y = h - rr.randint(s, min(h, s + int(h * 0.6 * (1 - abs(x + s / 2 - w / 2) / (w / 2)))))
        c = rr.choice([(100, 86, 80), (84, 72, 70), (120, 104, 94), (70, 50, 40)])
        if dark: c = tuple(int(v * 0.35) for v in c)
        d.rectangle([x, y, x + s, h - 1], fill=col(c))
        d.line([x, y, x + s, y], fill=col(tuple(min(255, v + 25) for v in c)))
    # 부러진 각재
    for k in range(2):
        x = rr.randint(0, w // 2); y = rr.randint(h // 3, h - 4)
        c = (90, 58, 40) if not dark else (30, 20, 16)
        d.line([x, y, x + rr.randint(w // 3, w // 2), y - rr.randint(-4, 8)], fill=col(c), width=3)
    return im


save(rubble(48, 24, 11), 'V_RubbleS')
save(rubble(80, 32, 12), 'V_RubbleM')
save(rubble(120, 40, 13), 'V_RubbleL')

# 쓰러진 수레
im = Image.new('RGBA', (64, 40), CLEAR); d = ImageDraw.Draw(im)
d.polygon([(4, 20), (54, 12), (58, 26), (8, 32)], fill=col((96, 62, 40)))
for x in range(10, 56, 8): d.line([x, 15 + (x - 10) // -8, x + 2, 30], fill=col((60, 38, 26)))
d.ellipse([10, 22, 28, 40], outline=col((60, 40, 28)), width=3)
d.line([19, 22, 19, 40], fill=col((60, 40, 28))); d.line([10, 31, 28, 31], fill=col((60, 40, 28)))
d.line([54, 14, 63, 4], fill=col((80, 52, 34)), width=2)
save(im, 'V_Cart')

# 부서진 울타리
im = Image.new('RGBA', (48, 24), CLEAR); d = ImageDraw.Draw(im)
for i, x in enumerate(range(2, 46, 7)):
    hh = [20, 16, 22, 8, 18, 12, 20][i % 7]
    d.rectangle([x, 24 - hh, x + 3, 23], fill=col((92, 64, 46)))
d.line([0, 10, 30, 12], fill=col((80, 54, 38)), width=2)
save(im, 'V_Fence')

# 전경 실루엣: 그을린 기둥·무너진 벽 모서리 (카메라 가까이, 아주 어둡게)
im = Image.new('RGBA', (80, 200), CLEAR); d = ImageDraw.Draw(im)
d.polygon([(20, 200), (26, 30), (40, 0), (52, 26), (56, 200)], fill=col((14, 8, 12)))
d.polygon([(0, 200), (0, 140), (30, 120), (60, 150), (80, 200)], fill=col((18, 10, 14)))
d.line([26, 60, 70, 40], fill=col((14, 8, 12)), width=6)
for y in range(40, 200, 9): d.point((27, y), fill=col((60, 24, 18)))   # 잔불 자국
save(im, 'V_ForeBeam')
im = Image.new('RGBA', (200, 56), CLEAR); d = ImageDraw.Draw(im)
pts = [(0, 56), (0, 30)] + [(x, 30 - int(14 * abs(math.sin(x * 0.05))) - R.randint(0, 6)) for x in range(10, 200, 10)] + [(200, 56)]
d.polygon(pts, fill=col((16, 9, 13)))
save(im, 'V_ForeRubble')

# 루의 작은 집 (마을 사이에 숨겨진, 무너진)
im = Image.new('RGBA', (112, 96), CLEAR); d = ImageDraw.Draw(im)
base = 95
d.rectangle([10, 46, 100, base], fill=col((86, 70, 60)))
for y in range(48, base, 6):
    d.line([10, y, 100, y], fill=col((70, 56, 50)))
d.polygon([(4, 48), (40, 18), (62, 30), (70, 22), (106, 48)], fill=col((70, 44, 44)))
d.polygon([(62, 30), (70, 22), (80, 34), (66, 40)], fill=CLEAR)     # 지붕 구멍
d.line([66, 26, 96, 60], fill=col((60, 40, 30)), width=3)          # 무너진 서까래
d.rectangle([40, 62, 58, base], fill=col((28, 18, 18)))           # 문틈 (어두운 입구)
d.line([40, 62, 58, 62], fill=col((60, 40, 30)), width=2)
d.rectangle([72, 58, 84, 68], fill=col((120, 170, 200)))           # 안쪽 모니터 빛이 새는 창
d.line([78, 58, 78, 68], fill=col((60, 40, 30)))
d.polygon([(100, base), (100, 60), (110, 70), (112, base)], fill=col((96, 84, 76)))
save(im, 'V_LuHouse')

# ================================================================ 무너진 작은 집 (실내)
im = Image.new('RGBA', (32, 32), col((70, 50, 40))); d = ImageDraw.Draw(im)
for x in (0, 10, 21):
    d.line([x, 0, x, 31], fill=col((50, 34, 28)))
for k in range(10): d.point((R.randint(0, 31), R.randint(0, 31)), fill=col((84, 62, 50)))
d.line([4, 14, 8, 18], fill=col((40, 26, 22)))
save(im, 'H_Wall')
im = Image.new('RGBA', (32, 32), col((92, 66, 48))); d = ImageDraw.Draw(im)
for y in (0, 8, 16, 24):
    d.line([0, y, 31, y], fill=col((64, 44, 32)))
d.line([0, 0, 31, 0], fill=col((130, 96, 70)))
save(im, 'H_Floor')
# 벽의 구멍 (밖의 불빛)
im = Image.new('RGBA', (96, 80), CLEAR); d = ImageDraw.Draw(im)
pts = [(10, 70), (4, 40), (18, 12), (44, 4), (70, 10), (90, 36), (84, 72), (50, 78)]
d.polygon(pts, fill=col((60, 20, 26)))
dither_rect(d, (20, 16, 80, 70), (90, 30, 30), (200, 90, 40))
im2 = Image.new('RGBA', im.size, CLEAR); m = Image.new('L', im.size, 0); ImageDraw.Draw(m).polygon(pts, fill=255)
im2.paste(im, (0, 0), m); im = im2; d = ImageDraw.Draw(im)
for x in range(6, 90, 9): d.rectangle([x, 60 - (x * 7 % 13), x + 6, 78], fill=col((26, 12, 16)))   # 밖의 실루엣
d.line(pts + [pts[0]], fill=col((40, 26, 22)), width=2)
save(im, 'H_WallHole')
# 침대 (부서짐)
im = Image.new('RGBA', (64, 32), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([2, 18, 62, 26], fill=col((96, 64, 44)))
d.rectangle([2, 8, 8, 31], fill=col((80, 52, 36))); d.rectangle([56, 14, 62, 31], fill=col((80, 52, 36)))
d.polygon([(8, 18), (56, 14), (60, 20), (8, 22)], fill=col((150, 140, 160)))   # 이불
d.rectangle([10, 13, 22, 18], fill=col((200, 196, 210)))                       # 베개
d.line([30, 26, 40, 31], fill=col((60, 40, 30)), width=2)                       # 부러진 다리
save(im, 'H_Bed')
# 책상
im = Image.new('RGBA', (56, 32), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 10, 55, 14], fill=col((110, 76, 52)))
d.rectangle([2, 14, 6, 31], fill=col((84, 56, 40))); d.rectangle([48, 14, 52, 31], fill=col((84, 56, 40)))
d.rectangle([34, 15, 50, 24], fill=col((96, 66, 46)))
save(im, 'H_Desk')
# 컴퓨터 (판타지 세계의 낡은 컴퓨터: 나무 덧댐·끈으로 묶은 케이블) 40x32 x4: 꺼짐/켜짐/밝음/최대 빛
fr = []
for i in range(4):
    im = Image.new('RGBA', (40, 32), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([6, 2, 34, 24], fill=col((70, 64, 62)))
    d.rectangle([4, 0, 36, 3], fill=col((96, 70, 50)))          # 덧댄 나무 틀
    d.rectangle([4, 0, 7, 26], fill=col((96, 70, 50)))
    scr = [(18, 26, 30), (60, 110, 140), (150, 210, 240), (230, 250, 255)][i]
    d.rectangle([9, 5, 31, 20], fill=col(scr))
    if i >= 1:
        for y in range(7, 19, 3): d.line([11, y, 11 + (y * 7) % 16, y], fill=col(lerp(scr, (255, 255, 255), 0.5)))
    d.rectangle([14, 24, 26, 27], fill=col((60, 56, 54)))
    d.rectangle([8, 28, 32, 31], fill=col((84, 78, 72)))          # 자판
    for x in range(10, 31, 3): d.point((x, 29), fill=col((50, 46, 44)))
    d.line([34, 18, 39, 31], fill=col((40, 40, 46)), width=1)    # 케이블
    d.point((37, 25), fill=col((200, 170, 90)))                  # 묶은 끈
    fr.append(im)
save(strip(fr), 'H_PC')
# 널브러진 자재
def materials(seed):
    rr = random.Random(seed)
    im = Image.new('RGBA', (64, 20), CLEAR); d = ImageDraw.Draw(im)
    for k in range(4):
        x = rr.randint(0, 40); y = rr.randint(8, 16)
        d.line([x, y, x + rr.randint(14, 24), y + rr.randint(-4, 3)], fill=col((120, 86, 56)), width=3)
    for k in range(3):
        x = rr.randint(0, 54); d.rectangle([x, 14, x + 8, 19], fill=col((140, 80, 66)))
    d.polygon([(30, 19), (36, 10), (46, 12), (50, 19)], fill=col((110, 100, 130)))   # 천
    d.line([52, 18, 60, 12], fill=col((150, 150, 160)), width=1)                      # 공구
    d.rectangle([58, 10, 61, 13], fill=col((110, 110, 120)))
    return im


save(materials(21), 'H_MaterialsA')
save(materials(22), 'H_MaterialsB')
# 무너진 지붕 대들보 (사선)
im = Image.new('RGBA', (128, 64), CLEAR); d = ImageDraw.Draw(im)
d.line([0, 4, 126, 60], fill=col((60, 40, 30)), width=7)
d.line([0, 2, 126, 58], fill=col((86, 60, 42)), width=2)
d.line([90, 44, 110, 20], fill=col((60, 40, 30)), width=4)
save(im, 'H_Beam')
# 쓰러진 선반
im = Image.new('RGBA', (40, 48), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([4, 2, 36, 47], outline=col((86, 58, 40)), width=3)
for y in (16, 30): d.line([4, y, 36, y], fill=col((86, 58, 40)), width=2)
d.rectangle([8, 22, 14, 29], fill=col((120, 60, 60))); d.rectangle([20, 8, 24, 15], fill=col((80, 90, 130)))
im = im.rotate(-12, expand=False)
save(im, 'H_Shelf')

# ================================================================ 친구 컴퓨터 바탕화면
im = Image.new('RGBA', (640, 360), CLEAR); d = ImageDraw.Draw(im)
dither_rect(d, (0, 0, 640, 360), (30, 58, 92), (70, 120, 150))
# 단순한 기본 무늬 (실제 운영체제를 흉내 내지 않는 둥근 물결)
for k in range(3):
    for x in range(640):
        y = int(200 + k * 40 + 26 * math.sin(x * 0.012 + k))
        for yy in range(y, min(360, y + 10)):
            if (x + yy) % 2 == 0:
                d.point((x, yy), fill=col((110 + k * 20, 170 + k * 10, 190)))
save(im, 'D_Wallpaper')
im = Image.new('RGBA', (32, 32), col((26, 34, 52))); d = ImageDraw.Draw(im)
d.line([0, 0, 31, 0], fill=col((90, 120, 160))); d.line([0, 1, 31, 1], fill=col((50, 66, 96)))
save(im, 'D_Bar')


def icon(kind):
    im = Image.new('RGBA', (24, 24), CLEAR); d = ImageDraw.Draw(im)
    if kind == 'folder':
        d.rectangle([2, 6, 21, 20], fill=col((240, 200, 90))); d.rectangle([2, 4, 10, 7], fill=col((220, 180, 70)))
        d.line([2, 9, 21, 9], fill=col((255, 225, 140)))
    elif kind == 'trash':
        d.rectangle([5, 7, 18, 21], fill=col((200, 210, 220))); d.rectangle([3, 4, 20, 7], fill=col((170, 180, 190)))
        for x in (8, 11, 14): d.line([x, 9, x, 19], fill=col((140, 150, 160)))
    elif kind == 'doc':
        d.rectangle([5, 2, 18, 21], fill=col((245, 245, 250))); d.polygon([(14, 2), (18, 6), (14, 6)], fill=col((190, 190, 200)))
        for y in range(8, 19, 3): d.line([7, y, 16, y], fill=col((150, 150, 170)))
    elif kind == 'msg':
        d.rounded_rectangle([2, 4, 21, 17], 3, fill=col((120, 200, 150))); d.polygon([(6, 17), (6, 22), (11, 17)], fill=col((120, 200, 150)))
    return im


for k in ('folder', 'trash', 'doc', 'msg'): save(icon(k), 'D_Icon_' + k)

# ================================================================ 실험실
def lab_far():
    W, H = 640, 260
    im = Image.new('RGBA', (W, H), CLEAR); d = ImageDraw.Draw(im)
    dk = (16, 22, 24); dk2 = (24, 32, 34)
    for cx in (40, 200, 360, 520):
        d.rectangle([cx, 0, cx + 26, H], fill=col(dk))
        for y in range(10, H, 22):
            d.rectangle([cx + 8, y, cx + 18, y + 3], fill=col((46, 70, 72)))
    for y in (60, 150):
        d.rectangle([0, y, W, y + 8], fill=col(dk2))
        for x in range(0, W, 10): d.line([x, y + 8, x + 5, y + 14], fill=col(dk2))
        for x in range(14, W, 48): d.rectangle([x, y + 2, x + 3, y + 4], fill=col((90, 150, 140)))
    # 멀리 있는 유리 층과 조명
    for y in (90, 180):
        for x in range(0, W, 64):
            d.rectangle([x + 6, y, x + 54, y + 40], fill=col((20, 32, 34)))
            d.rectangle([x + 8, y + 2, x + 52, y + 4], fill=col((52, 80, 80)))
    return im


save(lab_far(), 'L_Far')


def lab_mid(seed):
    rr = random.Random(seed)
    W, H = 320, 200
    im = Image.new('RGBA', (W, H), CLEAR); d = ImageDraw.Draw(im)
    body = (34, 44, 46); edge = (52, 66, 64); dark = (22, 30, 32)
    # 배관
    for y in (18, 30):
        d.rectangle([0, y, W, y + 6], fill=col(body)); d.line([0, y, W, y], fill=col(edge))
        for x in range(rr.randint(0, 30), W, 70): d.rectangle([x, y - 2, x + 6, y + 8], fill=col(dark))
    # 장비 랙
    x = 8
    while x < W - 40:
        w = rr.randint(28, 48); h = rr.randint(60, 120)
        d.rectangle([x, H - h, x + w, H], fill=col(body)); d.rectangle([x, H - h, x + w, H - h + 2], fill=col(edge))
        for yy in range(H - h + 8, H - 6, 10):
            d.rectangle([x + 4, yy, x + w - 4, yy + 5], fill=col(dark))
            if rr.random() < 0.5: d.point((x + 6, yy + 2), fill=col((120, 230, 150)))
            if rr.random() < 0.3: d.point((x + 9, yy + 2), fill=col((240, 120, 100)))
        x += w + rr.randint(10, 30)
    # 세로 관
    for k in range(3):
        px = rr.randint(0, W - 8)
        d.rectangle([px, 0, px + 5, H], fill=col(dark))
    return im


save(lab_mid(1), 'L_MidA')
save(lab_mid(2), 'L_MidB')

# 유리 뒤 방 (검체 복도에서 보이는 개조 현장의 실루엣 — 자세한 신체 묘사 없음)
im = Image.new('RGBA', (128, 96), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 127, 95], fill=col((36, 46, 62)))
d.rectangle([4, 4, 123, 91], fill=col((46, 80, 96)))
d.rectangle([20, 60, 108, 66], fill=col((70, 80, 96)))            # 침대형 장치
d.ellipse([52, 44, 64, 56], fill=col((26, 36, 46))); d.rectangle([60, 50, 92, 60], fill=col((26, 36, 46)))   # 누운 사람 실루엣
d.line([30, 10, 58, 46], fill=col((80, 96, 110)), width=2); d.line([100, 10, 80, 50], fill=col((80, 96, 110)), width=2)   # 기계 팔
d.rectangle([8, 6, 120, 9], fill=col((150, 210, 230)))
for k in range(18): d.line([R.randint(4, 120), 4, R.randint(4, 120) + 10, 30], fill=(200, 230, 255, 40))
save(im, 'L_Chamber')

# 실험 통: 뒤판(루 뒤)과 앞 유리(루 앞, 반투명)로 나눈다. 80x112 x2 (정상/파손)
def tank(front, broken):
    im = Image.new('RGBA', (80, 112), CLEAR); d = ImageDraw.Draw(im)
    metal = (92, 100, 118); dk = (52, 58, 72); hi = (150, 160, 180)
    if not front:
        d.rectangle([6, 96, 73, 111], fill=col(metal)); d.rectangle([6, 96, 73, 98], fill=col(hi))   # 받침
        d.rectangle([10, 0, 69, 14], fill=col(metal)); d.rectangle([10, 12, 69, 14], fill=col(dk))    # 뚜껑
        for x in (22, 40, 58):                                                                       # 전극
            d.rectangle([x - 2, 14, x + 2, 24], fill=col((180, 150, 70)))
        d.rectangle([10, 15, 69, 95], fill=(40, 90, 110, 200 if not broken else 120))                # 내부 액체/배경
        if not broken:
            for k in range(14):
                x = R.randint(14, 66); y = R.randint(20, 92)
                d.point((x, y), fill=(160, 230, 255, 200))
        else:
            d.rectangle([10, 80, 69, 95], fill=(30, 60, 70, 220))
            for k in range(10): d.point((R.randint(12, 66), R.randint(82, 94)), fill=(160, 210, 230, 200))
            d.line([22, 24, 30, 40], fill=col((40, 40, 50)), width=2)   # 그을린 전극
        d.line([0, 104, 6, 104], fill=col(dk)); d.line([73, 104, 79, 104], fill=col(dk))
    else:
        if not broken:
            d.rectangle([10, 15, 69, 95], fill=(170, 220, 240, 46))
            d.line([14, 18, 14, 92], fill=(230, 250, 255, 150)); d.line([16, 18, 16, 60], fill=(230, 250, 255, 90))
            d.line([62, 20, 62, 40], fill=(230, 250, 255, 110))
            d.rectangle([8, 15, 10, 95], fill=col((110, 118, 136))); d.rectangle([69, 15, 71, 95], fill=col((110, 118, 136)))
        else:
            # 깨진 유리 가장자리만 남음
            for side in (0, 1):
                x0 = 10 if side == 0 else 58
                pts = [(x0, 15), (x0 + 11, 15), (x0 + 6, 30), (x0 + 10, 44), (x0 + 3, 60), (x0 + 8, 80), (x0 + 4, 95), (x0, 95)] if side == 0 else \
                      [(x0, 15), (x0 + 11, 15), (x0 + 11, 95), (x0 + 6, 95), (x0 + 2, 74), (x0 + 7, 56), (x0 + 1, 40), (x0 + 5, 26)]
                d.polygon(pts, fill=(170, 220, 240, 70))
                d.line(pts, fill=(230, 250, 255, 160))
            d.rectangle([8, 15, 10, 95], fill=col((110, 118, 136))); d.rectangle([69, 15, 71, 95], fill=col((110, 118, 136)))
            d.line([10, 15, 69, 15], fill=col((60, 60, 70)), width=2)
    return im


save(strip([tank(False, False), tank(False, True)]), 'L_TankBack')
save(strip([tank(True, False), tank(True, True)]), 'L_TankGlass')

# 실험실 대형 문 48x80 x4: 닫힘 / 열림 / 비틀려 멈춤(틈) / 비틀림+경고등 꺼짐
def lab_door(state):
    im = Image.new('RGBA', (48, 80), CLEAR); d = ImageDraw.Draw(im)
    frame = (70, 78, 96); panel = (110, 120, 140); dk = (40, 44, 56)
    d.rectangle([0, 0, 47, 79], fill=col(frame)); d.rectangle([4, 6, 43, 79], fill=col((14, 16, 22)))
    if state == 'closed':
        d.rectangle([4, 6, 23, 79], fill=col(panel)); d.rectangle([24, 6, 43, 79], fill=col(panel))
        d.line([23, 6, 23, 79], fill=col(dk)); d.rectangle([18, 40, 21, 46], fill=col((80, 220, 140)))
    elif state == 'open':
        d.rectangle([4, 6, 7, 79], fill=col(panel)); d.rectangle([40, 6, 43, 79], fill=col(panel))
    else:
        # 과전류·기계 손상으로 비틀려 멈춘 문: 가운데 좁은 틈
        d.polygon([(4, 6), (21, 6), (19, 30), (22, 52), (18, 79), (4, 79)], fill=col((96, 100, 116)))
        d.polygon([(43, 6), (27, 6), (29, 26), (25, 50), (28, 79), (43, 79)], fill=col((96, 100, 116)))
        d.line([(21, 6), (19, 30), (22, 52), (18, 79)], fill=col((60, 40, 30)), width=1)
        d.line([(27, 6), (29, 26), (25, 50), (28, 79)], fill=col((60, 40, 30)), width=1)
        for k in range(12): d.point((R.randint(6, 40), R.randint(8, 76)), fill=col((40, 30, 30)))   # 그을음
        d.rectangle([12, 40, 15, 46], fill=col((60, 40, 40)))
    d.rectangle([0, 0, 47, 5], fill=col((90, 98, 118)))
    lamp = (90, 230, 140) if state in ('closed', 'open') else (110, 40, 40)
    d.rectangle([20, 1, 27, 3], fill=col(lamp))
    return im


save(strip([lab_door('closed'), lab_door('open'), lab_door('bent')]), 'L_Door')

# 콘솔 48x40 x3: 켜짐/꺼짐/파손
def console(state):
    im = Image.new('RGBA', (48, 40), CLEAR); d = ImageDraw.Draw(im)
    d.polygon([(2, 39), (4, 14), (44, 10), (46, 39)], fill=col((70, 78, 96)))
    d.polygon([(8, 16), (40, 13), (40, 26), (8, 28)], fill=col((20, 30, 40) if state != 'on' else (60, 160, 190)))
    if state == 'on':
        for y in (17, 20, 23): d.line([10, y, 30, y - 1], fill=col((170, 240, 255)))
    if state == 'broken':
        d.line([12, 16, 30, 27], fill=col((200, 210, 220))); d.line([22, 14, 16, 28], fill=col((200, 210, 220)))
        for k in range(8): d.point((R.randint(6, 42), R.randint(12, 38)), fill=col((30, 24, 24)))
    for x in range(8, 42, 6):
        c = (240, 200, 80) if state == 'on' and x % 12 == 2 else (60, 60, 70)
        d.rectangle([x, 31, x + 3, 33], fill=col(c))
    return im


save(strip([console('on'), console('off'), console('broken')]), 'L_Console')

# 통으로 이어지는 굵은 케이블 다발 96x24
im = Image.new('RGBA', (96, 24), CLEAR); d = ImageDraw.Draw(im)
for k, c in enumerate([(40, 44, 56), (60, 50, 40), (36, 60, 70)]):
    pts = [(x, 20 - k * 3 - int(6 * math.sin(x * 0.06 + k))) for x in range(0, 96, 4)]
    d.line(pts, fill=col(c), width=3)
save(im, 'L_Cables')

# 전경: 기둥·배관 실루엣
im = Image.new('RGBA', (40, 240), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([6, 0, 34, 239], fill=col((12, 14, 20)))
for y in range(20, 240, 46): d.rectangle([2, y, 38, y + 8], fill=col((16, 18, 26)))
save(im, 'L_ForePillar')
im = Image.new('RGBA', (240, 36), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 8, 239, 26], fill=col((12, 14, 20)))
for x in range(10, 240, 60): d.rectangle([x, 4, x + 10, 30], fill=col((16, 18, 26)))
save(im, 'L_ForePipe')

# 파편·쓰러진 천장 패널·그을음
save(rubble(48, 20, 31), 'L_DebrisS')
im = Image.new('RGBA', (72, 28), CLEAR); d = ImageDraw.Draw(im)
d.polygon([(2, 26), (10, 6), (70, 14), (64, 27)], fill=col((96, 104, 120)))
for x in range(14, 66, 10): d.line([x, 8 + (x - 10) // 8, x - 4, 26], fill=col((70, 76, 92)))
d.rectangle([30, 12, 44, 16], fill=col((60, 60, 70)))
save(im, 'L_FallenPanel')
im = Image.new('RGBA', (64, 16), CLEAR); d = ImageDraw.Draw(im)
for k in range(60):
    x = int(32 + R.gauss(0, 12)); y = int(8 + R.gauss(0, 3))
    if 0 <= x < 64 and 0 <= y < 16: d.point((x, y), fill=(20, 18, 20, 180))
save(im, 'L_Scorch')
# 금속 격자 바닥 타일
im = Image.new('RGBA', (32, 32), col((44, 52, 54))); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 31, 3], fill=col((92, 108, 106))); d.line([0, 0, 31, 0], fill=col((140, 160, 156)))
for x in range(2, 32, 6): d.line([x, 6, x, 31], fill=col((34, 42, 44)))
save(im, 'L_Floor')
im = Image.new('RGBA', (32, 32), col((28, 34, 36))); d = ImageDraw.Draw(im)
for y in range(0, 32, 8): d.line([0, y, 31, y], fill=col((22, 28, 30)))
save(im, 'L_FloorFill')
# 실내 벽 패널 (배경 반복)
im = Image.new('RGBA', (64, 64), col((30, 38, 40))); d = ImageDraw.Draw(im)
d.rectangle([2, 2, 61, 61], outline=col((40, 50, 50)))
d.line([0, 32, 63, 32], fill=col((24, 30, 32)))
d.rectangle([6, 8, 14, 10], fill=col((46, 58, 56)))
save(im, 'L_WallPanel')
# 깨진 관찰창 128x72 (뒤로 다음 장소가 보이게 가운데가 투명)
im = Image.new('RGBA', (128, 72), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 127, 71], outline=col((80, 90, 110)), width=4)
pts = [(4, 4), (40, 4), (30, 20), (52, 30), (24, 44), (4, 40)]
d.polygon(pts, fill=(170, 220, 240, 60)); d.line(pts, fill=(230, 250, 255, 150))
pts = [(123, 67), (90, 67), (100, 50), (80, 42), (110, 30), (123, 36)]
d.polygon(pts, fill=(170, 220, 240, 60)); d.line(pts, fill=(230, 250, 255, 150))
save(im, 'L_Window')
# 크레인 (화물 적재장 중앙의 큰 기계) 160x120
im = Image.new('RGBA', (160, 120), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([70, 0, 90, 119], fill=col((70, 64, 54)))
for y in range(0, 120, 12): d.line([70, y, 90, y + 12], fill=col((90, 84, 70)), width=2); d.line([90, y, 70, y + 12], fill=col((90, 84, 70)), width=2)
d.rectangle([0, 8, 159, 18], fill=col((96, 84, 60)))
d.line([24, 18, 24, 60], fill=col((40, 40, 46)))
d.rectangle([14, 60, 34, 72], fill=col((200, 160, 60)))
d.rectangle([40, 104, 120, 119], fill=col((60, 56, 50)))
save(im, 'L_Crane')

print('scene art done ->', OUT)

# ---- 연무용 부드러운 띠 (가로로 늘려 쓰는 세로 그라데이션 알파)
im = Image.new('RGBA', (4, 64), CLEAR)
for y in range(64):
    a = int(255 * math.sin(math.pi * y / 63) ** 1.5)
    for x in range(4): im.putpixel((x, y), (255, 255, 255, a))
save(im, 'FX_HazeBand')
im = Image.new('RGBA', (4, 64), CLEAR)
for y in range(64):
    a = int(255 * (1 - y / 63) ** 1.3)
    for x in range(4): im.putpixel((x, y), (255, 255, 255, a))
save(im, 'FX_FadeDown')
print('haze sprites done')
