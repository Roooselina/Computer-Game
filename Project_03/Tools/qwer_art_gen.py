# -*- coding: utf-8 -*-
"""
ELECTRIC WORLD: LAST LIGHT - 2026-11-15 개편용 임시 그림 생성기 (개발용, 최종 디자인 아님).

- UI_Skill_*      : Q/W/E/R 슬롯 아이콘 16x16
- UI_Prev_*       : 안내서 미리보기용 작은 그림 (판정 없는 UI 시연)
- FX_Acid*, FX_Bubbles, FX_ReactBurst, FX_ToxicMist, FX_Flask : 베넬 화학 공격 효과 (32x32 프레임)
- FX_AcidFan      : 낮은 부채꼴 산성 분사 64x32 프레임
- FX_PuddleAcid   : 독성 웅덩이 64x12 프레임 (끓는 거품)
- Scene/S_HideRack, S_HideSacks : 뒤에 숨는 전경 엄폐물 (B형)
- Scene/S_CrateSolid : 올라서는 고체 상자 (A형, 윗면 줄무늬로 구분)
- Scene/D_Desktop, D_Taskbar, D_Window, D_Icon_guide/status/log/photo/webcam/battery/socket/exit : 컴퓨터 안 (작업표시줄은 한 개)
- Scene/H_RuinBack : 뒤쪽 벽이 날아간 작은 집 (바깥은 투명 → 카메라 배경 검정)

실행: python Tools/qwer_art_gen.py   (Pillow 필요)
"""
import os, math, random
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'Assets', 'Game', 'Art', 'Placeholder')
SC = os.path.join(OUT, 'Scene')
CLEAR = (0, 0, 0, 0)
R = random.Random(1115)
OUTLINE = (26, 22, 38, 255)


def col(c, a=255):
    return (c[0], c[1], c[2], a)


def save(img, name, scene=False):
    img.save(os.path.join(SC if scene else OUT, name + '.png'))


def strip(frames):
    w, h = frames[0].size
    s = Image.new('RGBA', (w * len(frames), h), CLEAR)
    for i, f in enumerate(frames):
        s.paste(f, (i * w, 0))
    return s


def outline(layer, oc=OUTLINE):
    w, h = layer.size
    src = layer.load(); out = layer.copy(); dst = out.load()
    for y in range(h):
        for x in range(w):
            if src[x, y][3] == 0:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < w and 0 <= ny < h and src[nx, ny][3] > 120:
                        dst[x, y] = oc
                        break
    return out


def bolt(d, x0, y0, x1, y1, c, segs=4, jit=2, width=1):
    pts = [(x0, y0)]
    for i in range(1, segs):
        t = i / segs
        pts.append((x0 + (x1 - x0) * t + R.randint(-jit, jit), y0 + (y1 - y0) * t + R.randint(-jit, jit)))
    pts.append((x1, y1))
    d.line(pts, fill=c, width=width)


# ======================================================== 스킬 아이콘 16x16
def icon(kind):
    im = Image.new('RGBA', (16, 16), CLEAR); d = ImageDraw.Draw(im)
    if kind == 'tok':        # 손끝 불꽃
        d.rectangle([2, 9, 7, 13], fill=col((240, 200, 170)))
        d.rectangle([7, 10, 9, 11], fill=col((240, 200, 170)))
        bolt(d, 9, 10, 14, 4, col((255, 245, 140)), 3, 1)
        d.point([(13, 3), (14, 6), (11, 3)], fill=col((180, 240, 255)))
    elif kind == 'jam':      # 꺼진 렌즈 + 물결
        d.rectangle([2, 5, 10, 11], fill=col((90, 100, 120))); d.ellipse([7, 6, 11, 10], fill=col((60, 70, 80)))
        d.line([12, 4, 14, 6, 12, 8, 14, 10, 12, 12], fill=col((140, 230, 255)))
        d.line([3, 13, 9, 13], fill=col((140, 230, 255)))
    elif kind == 'pull':     # 배전함 → 큰 번개
        d.rectangle([1, 6, 5, 14], fill=col((110, 120, 140))); d.rectangle([2, 8, 4, 9], fill=col((255, 220, 90)))
        bolt(d, 5, 9, 14, 3, col((255, 240, 120)), 4, 2, 2)
        d.point([(14, 2), (15, 4)], fill=col((255, 255, 255)))
    elif kind == 'wire':     # 콘센트 → 빛 점 → 선
        d.rectangle([1, 9, 5, 14], fill=col((150, 160, 180))); d.point([(2, 11), (4, 11)], fill=col((40, 40, 50)))
        d.line([5, 11, 9, 11, 9, 5, 14, 5], fill=col((70, 76, 90)), width=1)
        d.ellipse([10, 3, 14, 7], fill=col((180, 240, 255)))
        d.point([(12, 5)], fill=col((255, 255, 255)))
    elif kind == 'spark_shot':
        d.ellipse([6, 5, 12, 11], fill=col((180, 240, 255))); d.ellipse([8, 7, 10, 9], fill=col((255, 255, 255)))
        d.line([1, 8, 5, 8], fill=col((140, 200, 255))); d.line([2, 6, 5, 7], fill=col((140, 200, 255))); d.line([2, 10, 5, 9], fill=col((140, 200, 255)))
    return outline(im)


for k in ('tok', 'jam', 'pull', 'wire', 'spark_shot'):
    save(icon(k), 'UI_Skill_' + k)

# ======================================================== 안내서 미리보기 그림
im = Image.new('RGBA', (16, 24), CLEAR); d = ImageDraw.Draw(im)     # 연습용 더미
d.rectangle([5, 2, 10, 7], fill=col((200, 200, 210))); d.rectangle([3, 8, 12, 18], fill=col((170, 175, 190)))
d.rectangle([7, 18, 8, 23], fill=col((120, 120, 130))); d.rectangle([4, 22, 11, 23], fill=col((120, 120, 130)))
d.ellipse([6, 10, 9, 13], fill=col((230, 90, 80)))
save(outline(im), 'UI_Prev_Dummy')
for st in ('on', 'off'):
    im = Image.new('RGBA', (20, 16), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([2, 4, 15, 11], fill=col((150, 160, 175))); d.rectangle([15, 6, 18, 9], fill=col((70, 76, 90)))
    d.ellipse([11, 5, 15, 10], fill=col((120, 255, 220) if st == 'on' else (60, 66, 76)))
    d.rectangle([5, 0, 7, 4], fill=col((90, 96, 110)))
    save(outline(im), 'UI_Prev_Cctv' + ('' if st == 'on' else 'Off'))
im = Image.new('RGBA', (16, 20), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([2, 4, 13, 19], fill=col((110, 118, 136))); d.rectangle([4, 7, 11, 12], fill=col((40, 44, 56)))
bolt(d, 5, 8, 10, 11, col((255, 230, 90)), 2, 1)
d.polygon([(8, 0), (5, 3), (8, 3), (6, 6), (11, 2), (8, 2)], fill=col((255, 220, 80)))
save(outline(im), 'UI_Prev_Source')
im = Image.new('RGBA', (16, 26), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([2, 2, 13, 25], fill=col((90, 96, 112))); d.rectangle([4, 4, 11, 10], fill=col((200, 80, 70)))
d.rectangle([4, 13, 11, 23], fill=col((70, 76, 90)))
save(outline(im), 'UI_Prev_Panel')
im = Image.new('RGBA', (10, 12), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([1, 2, 8, 11], fill=col((170, 180, 200))); d.point([(3, 6), (6, 6)], fill=col((30, 30, 40)))
d.rectangle([3, 0, 6, 2], fill=col((255, 230, 90)))
save(outline(im), 'UI_Prev_Socket')

# ======================================================== 베넬 화학 효과 (32x32 프레임, 가운데 피벗)
ACID = (150, 255, 90); ACID_D = (70, 170, 60); ACID_L = (220, 255, 170); MIST = (120, 200, 90)


def frames(n, fn, w=32, h=32):
    out = []
    for i in range(n):
        im = Image.new('RGBA', (w, h), CLEAR); d = ImageDraw.Draw(im); fn(i, n, im, d); out.append(im)
    return strip(out)


def splash(i, n, im, d):
    k = (i + 1) / n
    for a in range(9):
        ang = math.pi * (0.1 + 0.8 * a / 8)
        r = 3 + 12 * k
        x = 16 + math.cos(ang) * r * (1 if a % 2 else 0.8); y = 22 - math.sin(ang) * r * 0.9 + k * 6
        s = max(1, int(3 - k * 2))
        d.ellipse([x - s, y - s, x + s, y + s], fill=col(ACID if a % 3 else ACID_L, int(255 * (1 - k * 0.6))))
    d.ellipse([16 - 9 * k, 26, 16 + 9 * k, 29], fill=col(ACID_D, int(220 * (1 - k * 0.5))))


save(frames(5, splash), 'FX_AcidSplash')


def bubbles(i, n, im, d):
    rr = random.Random(i * 7 + 3)
    for b in range(6):
        x = 6 + rr.randint(0, 20); y = 28 - ((i * 5 + b * 7) % 26); s = rr.randint(1, 3)
        d.ellipse([x - s, y - s, x + s, y + s], outline=col(ACID_L, 220))


save(frames(4, bubbles), 'FX_Bubbles')


def react(i, n, im, d):
    k = (i + 1) / n
    r = 4 + 12 * k
    d.ellipse([16 - r, 16 - r, 16 + r, 16 + r], fill=col((200, 255, 120), int(200 * (1 - k))))
    d.ellipse([16 - r * 0.6, 16 - r * 0.6, 16 + r * 0.6, 16 + r * 0.6], fill=col((255, 255, 220), int(230 * (1 - k))))
    for a in range(8):
        ang = a * math.pi / 4 + i
        d.line([16 + math.cos(ang) * r * 0.7, 16 + math.sin(ang) * r * 0.7, 16 + math.cos(ang) * (r + 3), 16 + math.sin(ang) * (r + 3)], fill=col(ACID, int(255 * (1 - k * 0.7))))


save(frames(5, react), 'FX_ReactBurst')


def mist(i, n, im, d):
    rr = random.Random(11 + i)
    for b in range(9):
        x = rr.randint(4, 28); y = rr.randint(10, 28); s = rr.randint(3, 6)
        d.ellipse([x - s, y - s, x + s, y + s], fill=col(MIST, 60 + (b % 3) * 20))


save(frames(4, mist), 'FX_ToxicMist')


def flask(i, n, im, d):
    # 약병 (회전): 목과 몸통, 안의 초록 액체
    ang = i * math.pi / 2
    cx, cy = 16, 16
    def rot(x, y):
        return (cx + x * math.cos(ang) - y * math.sin(ang), cy + x * math.sin(ang) + y * math.cos(ang))
    body = [rot(-4, -1), rot(4, -1), rot(4, 6), rot(-4, 6)]
    neck = [rot(-1.5, -6), rot(1.5, -6), rot(1.5, -1), rot(-1.5, -1)]
    d.polygon(body, fill=col((200, 230, 220)))
    d.polygon([rot(-3, 2), rot(3, 2), rot(3, 5), rot(-3, 5)], fill=col(ACID))
    d.polygon(neck, fill=col((180, 200, 200)))


save(frames(4, flask), 'FX_Flask')


def fan(i, n, im, d):
    # 낮은 부채꼴 분사 (왼쪽이 손, 오른쪽으로 퍼짐). 바닥 쪽으로 낮게 깔린다.
    k = (i + 1) / n
    for j in range(40):
        t = j / 39
        x = 2 + t * 60 * min(1, k * 1.5)
        spread = 2 + t * 9
        for q in range(2):
            y = 20 + R.uniform(-spread, spread * 0.6)
            s = 1 + int(t * 2)
            c = ACID_L if R.random() < 0.3 else ACID
            d.ellipse([x - s, y - s, x + s, y + s], fill=col(c, int(230 * (1 - t * 0.4))))


save(frames(4, fan, 64, 32), 'FX_AcidFan')


def puddle(i, n, im, d):
    d.ellipse([1, 4, 62, 11], fill=col(ACID_D, 210))
    d.ellipse([6, 5, 57, 9], fill=col(ACID, 200))
    rr = random.Random(30 + i)
    for b in range(4):
        x = rr.randint(8, 56); s = rr.randint(1, 2)
        d.ellipse([x - s, 4 - s, x + s, 4 + s], outline=col(ACID_L, 230))


save(frames(3, puddle, 64, 12), 'FX_PuddleAcid')

# ======================================================== 엄폐물 (Scene)
# B형: 뒤에 숨는 전경 엄폐물 — 어두운 실루엣, 천 덮개, 루보다 앞에 그린다
im = Image.new('RGBA', (56, 44), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([2, 4, 53, 43], fill=col((46, 52, 66)))
d.rectangle([2, 4, 53, 6], fill=col((90, 98, 116)))
for y in (16, 30):
    d.rectangle([2, y, 53, y + 2], fill=col((80, 88, 104)))
for x, w, h, c in ((6, 10, 9, (120, 110, 90)), (19, 8, 6, (90, 110, 130)), (30, 14, 9, (110, 120, 100)), (8, 12, 12, (100, 90, 120)), (26, 9, 10, (130, 120, 90)), (40, 10, 12, (90, 100, 110))):
    pass
d.rectangle([6, 7, 16, 15], fill=col((120, 110, 90))); d.rectangle([20, 10, 28, 15], fill=col((90, 110, 130))); d.rectangle([33, 7, 47, 15], fill=col((110, 120, 100)))
d.rectangle([6, 19, 18, 29], fill=col((100, 90, 120))); d.rectangle([24, 21, 33, 29], fill=col((130, 120, 90)))
# 앞에 늘어진 천 덮개 (아래쪽 절반)
d.polygon([(0, 26), (55, 24), (55, 43), (0, 43)], fill=col((62, 74, 88)))
for x in range(3, 54, 7): d.line([x, 27, x + 2, 43], fill=col((50, 60, 72)))
d.line([0, 26, 55, 24], fill=col((110, 126, 140)))
save(outline(im), 'S_HideRack', True)
im = Image.new('RGBA', (64, 36), CLEAR); d = ImageDraw.Draw(im)
for i, (x, y, w, h) in enumerate(((2, 18, 22, 17), (20, 16, 24, 19), (40, 19, 22, 16), (10, 6, 22, 14), (31, 4, 22, 14))):
    c = (96, 90, 74) if i % 2 == 0 else (84, 80, 66)
    d.rounded_rectangle([x, y, x + w, y + h], 5, fill=col(c))
    d.line([x + 4, y + 3, x + w - 4, y + 3], fill=col((120, 114, 96)))
    d.rectangle([x + w // 2 - 3, y + h // 2 - 2, x + w // 2 + 3, y + h // 2 + 1], fill=col((150, 60, 50)))
save(outline(im), 'S_HideSacks', True)
# A형: 올라서는 고체 상자 — 밝은 윗면 + 노란 줄무늬 (발판임이 보이게)
im = Image.new('RGBA', (48, 32), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([1, 3, 46, 31], fill=col((112, 118, 132)))
d.rectangle([1, 3, 46, 6], fill=col((190, 196, 206)))
for x in range(2, 46, 8): d.polygon([(x, 6), (x + 4, 6), (x + 7, 9), (x + 3, 9)], fill=col((230, 190, 60)))
d.rectangle([5, 12, 42, 27], outline=col((90, 96, 110)))
d.line([5, 12, 42, 27], fill=col((90, 96, 110))); d.line([42, 12, 5, 27], fill=col((90, 96, 110)))
save(outline(im), 'S_CrateSolid', True)

# ======================================================== 컴퓨터 안 (Scene)
W, H = 640, 352
im = Image.new('RGBA', (W, H), CLEAR); d = ImageDraw.Draw(im)
for y in range(H):
    k = y / H
    c = (int(24 + 30 * k), int(48 + 50 * k), int(86 + 40 * k))
    d.line([0, y, W, y], fill=col(c))
for k in range(3):
    for x in range(W):
        y = int(190 + k * 46 + 24 * math.sin(x * 0.011 + k * 1.7))
        for yy in range(y, min(H, y + 8)):
            if (x + yy) % 2 == 0: d.point((x, yy), fill=col((90 + k * 18, 150 + k * 12, 180)))
# 가운데 희미한 원형 무늬 (단말 제조사 로고가 아닌 추상 무늬)
for r in range(60, 64):
    d.ellipse([320 - r, 120 - r, 320 + r, 120 + r], outline=col((70, 110, 150)))
save(im, 'D_Desktop', True)
# 작업표시줄: 하나만, 화면 맨 아래 한 줄 (640x32)
im = Image.new('RGBA', (640, 32), col((22, 28, 44))); d = ImageDraw.Draw(im)
d.line([0, 0, 639, 0], fill=col((96, 130, 170))); d.line([0, 1, 639, 1], fill=col((44, 58, 86)))
d.rounded_rectangle([6, 6, 54, 26], 4, fill=col((60, 110, 170)))
d.ellipse([12, 11, 20, 19], fill=col((200, 230, 255)))
for i, c in enumerate(((240, 200, 90), (120, 200, 150), (200, 120, 120))):
    d.rounded_rectangle([66 + i * 30, 7, 88 + i * 30, 25], 3, fill=col((40, 52, 76)))
    d.rectangle([71 + i * 30, 11, 83 + i * 30, 21], fill=col(c))
d.rectangle([560, 9, 632, 23], fill=col((34, 44, 66)))
for x in (566, 572, 584, 590, 604, 610, 620): d.rectangle([x, 13, x + 3, 19], fill=col((170, 200, 230)))
save(im, 'D_Taskbar', True)
# 탐색기 창 (제목줄 = 올라설 수 있는 가장자리). 224x128
im = Image.new('RGBA', (224, 128), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 223, 127], fill=col((220, 226, 236)))
d.rectangle([0, 0, 223, 13], fill=col((60, 100, 160)))
d.rectangle([200, 3, 219, 10], fill=col((200, 90, 80)))
d.rectangle([0, 14, 50, 127], fill=col((200, 208, 222)))
for y in range(22, 120, 14): d.rectangle([6, y, 44, y + 6], fill=col((170, 180, 200)))
for i in range(6):
    x = 64 + (i % 3) * 50; y = 26 + (i // 3) * 46
    d.rectangle([x, y, x + 22, y + 18], fill=col((240, 200, 90))); d.rectangle([x - 2, y + 24, x + 26, y + 28], fill=col((150, 160, 180)))
d.rectangle([0, 0, 223, 127], outline=col((40, 60, 100)))
save(im, 'D_Window', True)


def dicon(kind):
    im = Image.new('RGBA', (24, 24), CLEAR); d = ImageDraw.Draw(im)
    if kind == 'guide':     # 노란 표지의 안내서 문서
        d.rectangle([5, 2, 19, 21], fill=col((250, 230, 140))); d.rectangle([5, 2, 7, 21], fill=col((210, 170, 70)))
        for y in range(7, 18, 3): d.line([9, y, 17, y], fill=col((120, 100, 60)))
        d.rectangle([9, 3, 16, 5], fill=col((200, 80, 60)))
    elif kind == 'status':  # 신호 막대
        d.rectangle([3, 3, 20, 20], fill=col((40, 60, 90)))
        for i, h in enumerate((4, 7, 10, 13)): d.rectangle([5 + i * 4, 18 - h, 7 + i * 4, 18], fill=col((120, 255, 160) if i < 3 else (90, 110, 120)))
    elif kind == 'log':     # 붉은 경고가 붙은 로그 파일
        d.rectangle([5, 2, 18, 21], fill=col((235, 240, 245)))
        for y in range(6, 19, 3): d.line([7, y, 16, y], fill=col((120, 130, 150)))
        d.polygon([(14, 12), (21, 22), (8, 22)], fill=col((230, 70, 60))); d.line([14, 15, 14, 19], fill=col((255, 255, 255)))
    elif kind == 'photo':
        d.rectangle([2, 5, 21, 19], fill=col((240, 240, 245))); d.rectangle([4, 7, 19, 17], fill=col((120, 170, 220)))
        d.polygon([(5, 17), (10, 11), (14, 15), (16, 13), (19, 17)], fill=col((90, 150, 90)))
    elif kind == 'webcam':
        d.ellipse([5, 3, 18, 16], fill=col((60, 66, 80))); d.ellipse([8, 6, 15, 13], fill=col((120, 255, 220)))
        d.rectangle([10, 16, 13, 20], fill=col((60, 66, 80))); d.rectangle([6, 20, 17, 22], fill=col((60, 66, 80)))
    elif kind == 'battery':
        d.rectangle([4, 6, 18, 18], fill=col((80, 90, 104))); d.rectangle([18, 9, 20, 15], fill=col((80, 90, 104)))
        d.rectangle([6, 8, 13, 16], fill=col((255, 220, 80)))
    elif kind == 'socket':
        d.rectangle([5, 5, 18, 20], fill=col((170, 180, 200))); d.point([(9, 12), (14, 12)], fill=col((30, 30, 40)))
        d.rectangle([9, 2, 14, 5], fill=col((255, 230, 90)))
    elif kind == 'exit':
        d.rectangle([4, 3, 15, 21], fill=col((90, 110, 150))); d.rectangle([6, 5, 13, 19], fill=col((30, 40, 60)))
        d.polygon([(14, 12), (21, 7), (21, 17)], fill=col((255, 200, 90)))
    elif kind == 'shutter':
        d.rectangle([3, 2, 20, 21], fill=col((110, 118, 136)))
        for y in range(4, 21, 3): d.line([4, y, 19, y], fill=col((80, 86, 100)))
        d.rectangle([9, 9, 14, 13], fill=col((220, 70, 60)))
    return outline(im)


for k in ('guide', 'status', 'log', 'photo', 'webcam', 'battery', 'socket', 'exit', 'shutter'):
    save(dicon(k), 'D_Icon_' + k, True)

# ======================================================== 무너진 작은 집: 뒤쪽 벽이 날아간 실내 (바깥 = 투명 → 검정 배경)
W, H = 448, 288          # 14 x 9 유닛
im = Image.new('RGBA', (W, H), CLEAR); d = ImageDraw.Draw(im)
wall = (98, 78, 74); wall_d = (70, 54, 52); edge = (150, 120, 108); char = (40, 30, 30)
# 남은 벽 (왼쪽 큰 덩어리, 오른쪽 작은 덩어리) + 구멍 가장자리 들쭉날쭉
def jag(x0, x1, top, seed):
    rr = random.Random(seed); pts = []
    for x in range(x0, x1 + 1, 6): pts.append((x, top + rr.randint(-10, 14)))
    return pts
left_top = [(0, 40)] + jag(0, 150, 46, 1) + [(150, 300)]
d.polygon([(0, H)] + left_top + [(150, H)], fill=col(wall))
right = [(330, H), (330, 120)] + jag(330, W - 1, 70, 2) + [(W - 1, H)]
d.polygon(right, fill=col(wall))
# 가운데: 아래쪽만 남은 낮은 벽 (허리 높이 아래) — 그 위는 날아가서 바깥(검정)이 보인다
low = [(150, H)] + [(x, 210 + random.Random(x).randint(-12, 10)) for x in range(150, 331, 8)] + [(330, H)]
d.polygon(low, fill=col(wall_d))
# 벽돌 줄눈
for y in range(60, H, 16):
    for x in range(0, W, 2):
        if im.getpixel((x, y))[3] > 0: d.point((x, y), fill=col(wall_d))
# 그을린 가장자리 (구멍 둘레 3px)
px = im.load()
edge_pts = []
for y in range(1, H - 1):
    for x in range(1, W - 1):
        if px[x, y][3] > 0 and (px[x, y - 1][3] == 0 or px[x - 1, y][3] == 0 or px[x + 1, y][3] == 0):
            edge_pts.append((x, y))
for x, y in edge_pts:
    px[x, y] = col(edge)
    if y + 1 < H and px[x, y + 1][3] > 0: px[x, y + 1] = col(char)
    if y + 2 < H and px[x, y + 2][3] > 0: px[x, y + 2] = col(char)
# 남은 기둥 2개 (부러진 끝)
for x, top in ((150, 30), (330, 70)):
    d.rectangle([x - 7, top, x + 7, H], fill=col((80, 62, 50)))
    d.polygon([(x - 7, top), (x - 2, top - 14), (x + 3, top - 4), (x + 7, top - 10), (x + 7, top)], fill=col((80, 62, 50)))
    d.line([x - 7, top, x - 7, H], fill=col((110, 88, 70)))
# 바닥에 흩어진 잔해 더미 (구멍 아래)
for i in range(16):
    x = R.randint(160, 320); y = H - R.randint(2, 14); s = R.randint(3, 8)
    d.polygon([(x - s, y), (x - s // 2, y - s), (x + s, y - s // 2), (x + s, y)], fill=col((110, 92, 84) if i % 2 else (84, 70, 64)))
# 매달린 부러진 들보
d.polygon([(120, 26), (250, 64), (246, 72), (118, 34)], fill=col((92, 70, 56)))
save(im, 'H_RuinBack', True)
print('qwer art written')
