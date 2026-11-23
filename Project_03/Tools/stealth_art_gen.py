# -*- coding: utf-8 -*-
"""
ELECTRIC WORLD: LAST LIGHT - 실시간 잠입 개편용 '임시' 소품 도트 생성기 (2026-11-14).

어두운 청회색·탁한 녹색의 인체 개조 연구시설 소품, 잠입 기물(엄폐 상자·CCTV·제어함·전원·쟁반·셔터 제어판·경광등),
정전된 실험실 조사 소품(구속대 침대·수술등·신체 도식·부품 수거함·번호 보관함·불투명 용기·관찰창).
신체 묘사는 하지 않는다 (도식은 선과 번호만, 관찰창 너머는 실루엣만).
최종 아트 요청: Docs/ART_REQUESTS.md  (교체 시 같은 파일 이름·피벗 유지)

실행: python Tools/stealth_art_gen.py   (Pillow 필요)
출력: Assets/Game/Art/Placeholder/Scene/S_*.png  (PPU 32, Point 필터)
"""
import os, random
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'Assets', 'Game', 'Art', 'Placeholder', 'Scene')
FONT = os.path.join(ROOT, 'Assets', 'ThirdParty', 'quiple_Galmuri', 'Galmuri11.ttf')
os.makedirs(OUT, exist_ok=True)
CLEAR = (0, 0, 0, 0)
R = random.Random(4705)

# 팔레트: 청회색 시설 + 탁한 녹색 + 진행 대상만 밝게
STEEL = (58, 66, 78); STEEL_D = (38, 44, 54); STEEL_L = (84, 96, 110)
MURK = (52, 70, 62); MURK_D = (34, 46, 42); MURK_L = (78, 100, 88)
INK = (16, 18, 24)
CRATE = (112, 92, 58); CRATE_D = (78, 62, 40); CRATE_L = (150, 126, 82)
WARN = (232, 186, 64)


def c(rgb, a=255):
    return (rgb[0], rgb[1], rgb[2], a)


def save(img, name):
    img.save(os.path.join(OUT, name + '.png'))


def outline(img, col=INK):
    w, h = img.size
    src = img.load(); out = img.copy(); o = out.load()
    for y in range(h):
        for x in range(w):
            if src[x, y][3] == 0:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < w and 0 <= ny < h and src[nx, ny][3] > 200:
                        o[x, y] = c(col); break
    return out


def text(d, xy, s, col, size=11):
    try:
        f = ImageFont.truetype(FONT, size)
    except Exception:
        f = ImageFont.load_default()
    d.text(xy, s, font=f, fill=c(col))


# ---------------------------------------------------------------- 엄폐물 (막는 상자): 밝은 테두리 + 경고 띠 = '막힘·숨을 수 있음'
def crate(w, h, seed):
    rr = random.Random(seed)
    im = Image.new('RGBA', (w, h), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([0, 0, w - 1, h - 1], fill=c(CRATE))
    d.rectangle([0, 0, w - 1, 2], fill=c(CRATE_L))
    for x in range(4, w - 2, 8): d.line([x, 3, x, h - 2], fill=c(CRATE_D))
    d.line([1, 3, w - 2, h - 2], fill=c(CRATE_D)); d.line([1, h - 2, w - 2, 3], fill=c(CRATE_D))
    for x in range(0, w, 6): d.line([x, h - 4, x + 3, h - 4], fill=c(WARN)); d.line([x + 3, h - 3, x + 5, h - 3], fill=c(INK))
    d.rectangle([w // 2 - 6, 6, w // 2 + 6, 11], fill=c((40, 40, 46)))
    text(d, (w // 2 - 5, 4), 'B%d' % rr.randint(1, 9), (200, 200, 190), 8)
    return outline(im)


save(crate(48, 32, 1), 'S_Crate')        # 1.5 x 1.0 유닛 낮은 엄폐물
save(crate(32, 32, 2), 'S_CrateS')       # 1.0 x 1.0
save(crate(48, 48, 3), 'S_CrateTall')    # 1.5 x 1.5 (선 채로도 가려짐)

# 통과 가능한 장식 상자 (어둡고 테두리 없음, 배경 쪽) — 엄폐물과 외형 구분
im = Image.new('RGBA', (40, 24), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 39, 23], fill=c((44, 46, 50)))
for x in range(3, 38, 7): d.line([x, 2, x, 22], fill=c((36, 38, 42)))
save(im, 'S_BgBox')

# ---------------------------------------------------------------- CCTV
im = Image.new('RGBA', (12, 14), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([2, 0, 9, 4], fill=c(STEEL_L)); d.rectangle([5, 4, 7, 13], fill=c(STEEL))
save(outline(im), 'S_CctvMount')
im = Image.new('RGBA', (26, 12), CLEAR); d = ImageDraw.Draw(im)      # 피벗: 왼쪽(0.12, 0.5) 회전축
d.rectangle([3, 2, 22, 9], fill=c((176, 182, 190))); d.rectangle([3, 2, 22, 3], fill=c((210, 214, 220)))
d.rectangle([20, 1, 25, 10], fill=c((60, 64, 72)))
d.ellipse([0, 4, 5, 8], fill=c(STEEL))
save(outline(im), 'S_CctvHead')
im = Image.new('RGBA', (4, 4), CLEAR); d = ImageDraw.Draw(im); d.rectangle([0, 0, 3, 3], fill=c((255, 255, 255))); save(im, 'S_Lens')
im = Image.new('RGBA', (4, 4), CLEAR); d = ImageDraw.Draw(im); d.rectangle([0, 0, 3, 3], fill=c((40, 40, 46))); save(im, 'S_LensOff')

# CCTV 제어함 (벽걸이, 작은 화면)
im = Image.new('RGBA', (22, 30), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 21, 29], fill=c(MURK)); d.rectangle([0, 0, 21, 1], fill=c(MURK_L))
d.rectangle([3, 4, 18, 13], fill=c(INK))
for y in (17, 21, 25): d.rectangle([4, y, 17, y + 1], fill=c(MURK_D))
d.line([10, 29, 10, 31], fill=c(STEEL_D))
text(d, (3, 15), 'CAM', (190, 210, 190), 8)
save(outline(im), 'S_CtrlBox')
im = Image.new('RGBA', (14, 8), CLEAR); d = ImageDraw.Draw(im); d.rectangle([0, 0, 13, 7], fill=c((255, 255, 255)))
for x in range(1, 13, 3): d.point((x, 3), fill=c((180, 180, 180)))
save(im, 'S_CtrlScreen')

# ---------------------------------------------------------------- 외부 전원 (노출된 배전함) + 상태 아이콘
def powerbox(live):
    im = Image.new('RGBA', (26, 34), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([2, 0, 23, 26], fill=c(STEEL)); d.rectangle([2, 0, 23, 1], fill=c(STEEL_L))
    d.rectangle([5, 4, 20, 18], fill=c(INK))
    for x in (8, 12, 16): d.rectangle([x, 6, x + 1, 16], fill=c((150, 140, 110) if live else (70, 70, 70)))
    d.polygon([(2, 0), (0, 4), (0, 20), (2, 24)], fill=c(STEEL_D))   # 열린 문짝
    for i, x in enumerate((7, 13, 19)):
        d.line([x, 26, x + (i - 1) * 3, 33], fill=c((30, 30, 34)), width=2)
    if live:
        d.point([(9, 9), (13, 12), (17, 8)], fill=c((255, 250, 170)))
    return outline(im)


save(powerbox(True), 'S_PowerBox')
save(powerbox(False), 'S_PowerBoxDead')


def icon(kind):
    im = Image.new('RGBA', (11, 13), CLEAR); d = ImageDraw.Draw(im)
    pts = [(6, 0), (1, 7), (5, 7), (3, 12), (9, 5), (5, 5), (8, 0)]
    if kind == 'on':
        d.polygon(pts, fill=c((255, 228, 90))); d.line(pts + [pts[0]], fill=c((255, 255, 210)))
    elif kind == 'busy':
        d.polygon(pts, fill=c((255, 255, 240))); d.line(pts + [pts[0]], fill=c((140, 230, 255)))
    else:
        d.polygon(pts, fill=c((90, 92, 100))); d.line([1, 11, 10, 1], fill=c((200, 80, 70)), width=2)
    return outline(im)


save(icon('on'), 'S_IconPower')
save(icon('empty'), 'S_IconPowerEmpty')
save(icon('busy'), 'S_IconPowerBusy')

# ---------------------------------------------------------------- 소리 나는 물건: 매달린 공구 쟁반
im = Image.new('RGBA', (30, 30), CLEAR); d = ImageDraw.Draw(im)
d.line([8, 0, 8, 14], fill=c((90, 96, 104))); d.line([22, 0, 22, 14], fill=c((90, 96, 104)))
d.rectangle([2, 14, 28, 18], fill=c((150, 156, 166))); d.rectangle([2, 14, 28, 14], fill=c((200, 206, 214)))
for x, h in ((6, 6), (11, 4), (16, 7), (21, 5), (25, 3)): d.line([x, 14 - h, x + 1, 14], fill=c((190, 196, 205)))
d.point([(4, 20), (26, 21)], fill=c(WARN))
save(outline(im), 'S_Tray')

# 셔터 제어판 + 램프
im = Image.new('RGBA', (16, 24), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 15, 23], fill=c(STEEL)); d.rectangle([3, 8, 12, 19], fill=c(STEEL_D))
d.rectangle([5, 10, 10, 12], fill=c(WARN)); d.rectangle([5, 15, 10, 17], fill=c((90, 90, 96)))
save(outline(im), 'S_ShutterPanel')
im = Image.new('RGBA', (4, 4), CLEAR); d = ImageDraw.Draw(im); d.rectangle([0, 0, 3, 3], fill=c((255, 255, 255))); save(im, 'S_Lamp')

# 경광등 (평소 꺼진 회색, 경보 때 빛·회전 빔) + 빔
im = Image.new('RGBA', (14, 12), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([1, 8, 12, 11], fill=c(STEEL)); d.ellipse([2, 0, 11, 10], fill=c((150, 50, 46)))
d.ellipse([4, 2, 7, 5], fill=c((220, 120, 110)))
save(outline(im), 'S_AlarmLamp')
im = Image.new('RGBA', (96, 24), CLEAR); px = im.load()
for x in range(96):
    for y in range(24):
        k = 1 - x / 96.0; spread = 3 + x * 0.2
        dy = abs(y - 12)
        if dy <= spread: px[x, y] = (255, 70, 60, int(160 * k * (1 - dy / (spread + 1))))
save(im, 'S_AlarmBeam')

# 작업등 (천장, 정전 때 꺼짐 → 복구 때 차례로 켜짐)
im = Image.new('RGBA', (30, 12), CLEAR); d = ImageDraw.Draw(im)
d.line([15, 0, 15, 4], fill=c(STEEL_D))
d.polygon([(4, 11), (9, 4), (21, 4), (26, 11)], fill=c(STEEL_L)); d.rectangle([8, 10, 22, 11], fill=c((240, 240, 210)))
save(outline(im), 'S_WorkLamp')

# ---------------------------------------------------------------- 정전된 실험실 조사 소품
# 구속대가 달린 침대 (64x30)
im = Image.new('RGBA', (64, 30), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([2, 12, 61, 17], fill=c((150, 158, 160))); d.rectangle([2, 12, 61, 12], fill=c((190, 196, 198)))
for x in (6, 56): d.rectangle([x, 18, x + 2, 29], fill=c(STEEL))
for x in (14, 30, 46): d.rectangle([x, 10, x + 5, 19], fill=c((60, 44, 36))); d.rectangle([x + 1, 13, x + 4, 15], fill=c((150, 140, 110)))
d.rectangle([2, 6, 6, 12], fill=c(STEEL_L))
save(outline(im), 'S_RestraintBed')

# 수술등 (천장, 40x26) — 꺼진 상태
im = Image.new('RGBA', (40, 26), CLEAR); d = ImageDraw.Draw(im)
d.line([20, 0, 20, 10], fill=c(STEEL_D), width=2)
d.chord([2, 8, 37, 34], 180, 360, fill=c(STEEL_L)); d.line([4, 21, 35, 21], fill=c(INK))
for x in (10, 20, 30): d.ellipse([x - 3, 16, x + 3, 21], fill=c((70, 76, 80)))
save(outline(im), 'S_SurgLamp')

# 의료 도구 쟁반 (받침대 위)
im = Image.new('RGBA', (28, 30), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([12, 10, 14, 29], fill=c(STEEL)); d.rectangle([6, 28, 20, 29], fill=c(STEEL))
d.rectangle([1, 6, 26, 9], fill=c((170, 176, 184)))
for x, h in ((4, 3), (8, 5), (13, 2), (18, 4), (22, 3)): d.line([x, 6 - h, x + 2, 6], fill=c((210, 214, 220)))
save(outline(im), 'S_ToolTray')

# 신체 도식 판 (선·번호만, 48x40)
im = Image.new('RGBA', (48, 40), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 47, 39], fill=c((190, 196, 180))); d.rectangle([0, 0, 47, 39], outline=c(STEEL_D))
d.ellipse([20, 4, 27, 11], outline=c((70, 76, 80))); d.line([23, 11, 23, 26], fill=c((70, 76, 80)))
d.line([23, 14, 15, 22], fill=c((70, 76, 80))); d.line([23, 14, 31, 22], fill=c((70, 76, 80)))
d.line([23, 26, 18, 35], fill=c((70, 76, 80))); d.line([23, 26, 28, 35], fill=c((70, 76, 80)))
for (x, y) in ((23, 15), (16, 21), (30, 21), (23, 24)):
    d.point((x, y), fill=c((220, 60, 50))); d.line([x, y, 40 if x > 23 else 6, y - 4], fill=c((200, 150, 60)))
for y in (6, 14, 22, 30): d.rectangle([38, y, 45, y + 1], fill=c((90, 96, 100))); d.rectangle([2, y, 9, y + 1], fill=c((90, 96, 100)))
text(d, (2, 30), 'E-D', (150, 40, 40), 8)
save(outline(im), 'S_BodyBoard')

# 부품 수거함 (32x24) — 뚜껑 틈으로 기계 손가락 비슷한 부품 (사람 신체 아님)
im = Image.new('RGBA', (32, 26), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([1, 6, 30, 25], fill=c(MURK)); d.rectangle([0, 3, 31, 7], fill=c(MURK_L))
for x in range(4, 29, 6): d.line([x, 9, x, 24], fill=c(MURK_D))
d.line([8, 3, 11, 0], fill=c((150, 156, 160)), width=2); d.line([20, 3, 24, 1], fill=c((120, 124, 128)), width=2)
d.rectangle([10, 12, 21, 17], fill=c(WARN)); text(d, (11, 11), '폐기', (40, 30, 10), 8)
save(outline(im), 'S_PartsBin')

# 번호가 붙은 보관함 (64x48)
im = Image.new('RGBA', (64, 48), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 63, 47], fill=c(STEEL_D))
n = 31
for row in range(3):
    for col in range(4):
        x, y = 2 + col * 15, 2 + row * 15
        d.rectangle([x, y, x + 13, y + 13], fill=c(STEEL)); d.rectangle([x + 9, y + 6, x + 11, y + 7], fill=c(STEEL_L))
        text(d, (x + 1, y + 1), '%02d' % n, (190, 200, 205), 8); n += 1
save(outline(im), 'S_Lockers')

# 불투명 실험 용기 (탁한 녹색, 안이 보이지 않음)
for k, h in ((0, 34), (1, 42)):
    im = Image.new('RGBA', (18, h), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([2, 4, 15, h - 4], fill=c((60, 92, 74))); d.rectangle([2, 4, 4, h - 4], fill=c((96, 130, 106)))
    d.rectangle([1, 0, 16, 4], fill=c(STEEL)); d.rectangle([1, h - 4, 16, h - 1], fill=c(STEEL))
    for _ in range(4): x = R.randint(5, 13); y = R.randint(8, h - 8); d.point((x, y), fill=c((130, 170, 140)))
    save(outline(im), 'S_Jar%d' % k)

# 관찰창: 어두운 유리 너머 장비·사람 실루엣 일부만 (96x56)
im = Image.new('RGBA', (96, 56), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 95, 55], fill=c(STEEL)); d.rectangle([4, 4, 91, 51], fill=c((22, 34, 34)))
d.rectangle([12, 34, 54, 38], fill=c((30, 46, 44)))
d.ellipse([60, 18, 68, 26], fill=c((14, 20, 22))); d.rectangle([58, 26, 70, 51], fill=c((14, 20, 22)))  # 서 있는 실루엣
d.line([20, 4, 30, 30], fill=c((40, 56, 54)), width=2); d.line([84, 4, 76, 28], fill=c((40, 56, 54)), width=2)
for x in range(6, 90, 9): d.line([x, 5, x + 8, 50], fill=(140, 190, 180, 22))
d.rectangle([4, 4, 91, 6], fill=c((70, 100, 96)))
save(outline(im), 'S_ObsWindow')

# 통에 붙은 표식 (확대용 판)
im = Image.new('RGBA', (30, 14), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 29, 13], fill=c((210, 210, 196))); d.rectangle([0, 0, 29, 2], fill=c((200, 60, 50)))
text(d, (1, 2), 'E-D05', (30, 30, 30), 8)
save(outline(im), 'S_TankLabel')

# 복도 표지판 '격리 실험동'
im = Image.new('RGBA', (64, 16), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 63, 15], fill=c((200, 196, 170))); d.rectangle([0, 0, 63, 15], outline=c((90, 80, 50)))
text(d, (3, 2), '격리 실험동', (60, 40, 20), 11)
save(outline(im), 'S_SignIsolation')

# 과부하 분전함 (베넬 복도 3단계 위험 장치)
im = Image.new('RGBA', (22, 28), CLEAR); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 21, 27], fill=c(STEEL)); d.rectangle([3, 3, 18, 18], fill=c(INK))
for x in (6, 10, 14): d.rectangle([x, 5, x + 1, 16], fill=c((200, 170, 90)))
for x in range(0, 22, 4): d.line([x, 22, x + 2, 22], fill=c(WARN))
save(outline(im), 'S_Junction')

# 바닥의 끊긴 굵은 전선 (장식: 전력 없음)
im = Image.new('RGBA', (40, 10), CLEAR); d = ImageDraw.Draw(im)
d.line([0, 7, 30, 6], fill=c((30, 30, 34)), width=3); d.line([30, 6, 36, 2], fill=c((150, 110, 60)), width=1)
save(im, 'S_DeadCable')

print('stealth art done ->', OUT)
