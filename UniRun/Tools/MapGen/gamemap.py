"""UniRun Adventure map generator.

Writes (relative to the Unity project root):
  Assets/Data/adventure_map.json          map data read by MapLoader at runtime
  Assets/Art/Generated/tiles_atlas.png  padded terrain + fluid atlas (no seams under bilinear filtering)
  Assets/Art/Generated/*.png          generated sprites (log, heart, shield, magnet, fx)
  Tools/MapGen/preview.png                low-res preview of the generated map

usage: python gamemap.py <unity_project_root> [seed]
requires: pillow, numpy
"""
import sys, os, json, random, math
import numpy as np
from PIL import Image, ImageDraw

ROOT = sys.argv[1]
SEED = int(sys.argv[2]) if len(sys.argv) > 2 else 11
rnd = random.Random(SEED)
TD = os.path.join(ROOT, 'Assets/Art/Kenney')
OUT_SPR = os.path.join(ROOT, 'Assets/Art/Generated')
OUT_MAP = os.path.join(ROOT, 'Assets/Data')
os.makedirs(OUT_SPR, exist_ok=True); os.makedirs(OUT_MAP, exist_ok=True)

# tile index -> (col,row) inside a 9x3 colour block of tilesheet_complete.png
IDX = {1:(7,0),2:(8,0),3:(0,0),4:(1,0),5:(2,0),6:(3,0),7:(4,0),8:(0,1),9:(1,1),10:(2,1),
       11:(3,1),12:(4,1),13:(5,1),14:(6,1),15:(7,1),16:(8,1),17:(0,2),18:(1,2),19:(2,2),
       20:(3,2),21:(4,2),22:(5,2),23:(6,2),24:(7,2),25:(8,2),26:(5,0),27:(6,0)}
FLUIDS = ['Blue', 'Brown', 'Green', 'Red']

ZONES = [
 dict(name='MEADOW',  block=9, fluid=0, water=13, lethal=False, dive=True, logs=True,
      sky=((105,168,236),(214,238,255)), hill=(150,190,235), hills='set3_hills',
      plants=['plantGreen_1','plantGreen_2','plantGreen_3','plantGreen_4','plantGreen_5','plantGreen_6','plantTop_yellow','plantTop_leaves'],
      fence=True, gem='blueGem', gap=(3,6), up=4, enemy=.35, spike=.20, pillar=.08, flyer=.30),
 dict(name='SWAMP',   block=3, fluid=1, water=15, lethal=False, dive=True, logs=True,
      sky=((236,178,110),(255,234,192)), hill=(214,160,110), hills='set1_hills',
      plants=['plantGreen_4','plantGreen_5','plantRed_4','plantRed_6','plantTop_yellow','plantTop_red','plantLeaves_1'],
      fence=True, gem='yellowGem', gap=(4,8), up=4, enemy=.50, spike=.30, pillar=.14, flyer=.40),
 dict(name='DUSK',    block=6, fluid=2, water=11, lethal=True, dive=False, logs=False,
      sky=((58,46,104),(168,132,190)), hill=(98,78,140), hills='set2_hills',
      plants=['plantBlue_1','plantBlue_2','plantBlue_3','plantBlue_5','plantDark_1','plantDark_3','plantTop_blue'],
      fence=False, gem='greenGem', gap=(5,10), up=4, enemy=.62, spike=.38, pillar=.22, flyer=.50),
 dict(name='VOLCANO', block=0, fluid=3, water=10, lethal=True, dive=False, logs=False,
      sky=((40,22,30),(150,70,52)), hill=(90,42,40), hills='set4_hills',
      plants=['plantDark_1','plantDark_2','plantDark_5','plantRed_1','plantRed_3','plantRed_5'],
      fence=False, gem='redGem', gap=(6,11), up=3, enemy=.75, spike=.45, pillar=.30, flyer=.65),
]
ZONE_W = 240
EASE_END = 230   # 이 칸까지는 초반 구간: 발판을 넓게, 기둥/통나무 구간 없이
H = 84
HMAX = H - 12

# ---------------------------------------------------------------- terrain
cols, zcol = [], []
logs = []          # (x_center, width, zone)
h = 18; zi = 0
def Z(): return ZONES[zi]
def hmin(): return Z()['water'] + 2
def add(k, hh): cols.append((k, hh)); zcol.append(zi)
def flat(n):
    for _ in range(n): add('f', h)
def gap(n):
    for _ in range(n): add('g', 0)
def clamp(v): return max(hmin(), min(HMAX, v))

zone_marks = []
flat(34); last = 'f'; zstart = 0   # 시작 후 최소 3초는 장애물 없이 달린다
target, next_target = 24, 40
while True:
    z = Z()
    if len(cols) - zstart > ZONE_W and last == 'f':
        if zi == len(ZONES) - 1: break
        zone_marks.append((zstart, len(cols)))
        flat(3); gap(rnd.randint(*z['gap']))
        zi += 1; zstart = len(cols)
        h = max(h, max(ZONES[zi - 1]['water'], Z()['water']) + 4)
        flat(rnd.randint(12, 16)); last = 'f'
        continue
    ease = len(cols) < EASE_END
    if last != 'f':
        flat(rnd.randint(7, 12) if ease else rnd.randint(3, 9)); last = 'f'; continue
    if len(cols) >= next_target:
        target = rnd.randint(hmin() + 2, HMAX - 8); next_target = len(cols) + rnd.randint(35, 90)
    want_up, want_down = h < target - 3, h > target + 3
    r = rnd.random()
    if z['logs'] and r < 0.07 and not ease:
        # log run: hop across floating logs, exit onto low land
        gap(rnd.randint(4, 6))
        for i in range(rnd.randint(4, 8)):
            w = rnd.choice([2, 2, 3])
            logs.append((len(cols) + w / 2, w, zi))
            gap(w); gap(rnd.randint(3, 5))
        h = z['water'] + rnd.randint(2, 4)
        flat(rnd.randint(6, 10)); last = 'f'
        continue
    if z['dive'] and 0.07 <= r < 0.10 and h - z['water'] > 12:
        gap(rnd.randint(4, 7)); h = rnd.randint(3, z['water'] - 5); r = 0.0   # cliff dive
    if z['dive'] and r < 0.13 and h - (z['water'] - 5) <= 18:
        floor = rnd.randint(3, z['water'] - 5)
        while h > floor: add('d', h); h -= 1
        for _ in range(rnd.randint(2, 4)):
            flat(rnd.randint(4, 10))
            nh = max(2, min(z['water'] - 3, h + rnd.choice([-2, -1, 1, 2])))
            while h < nh: add('u', h); h += 1
            while h > nh: add('d', h); h -= 1
        flat(rnd.randint(3, 6))
        out = z['water'] + rnd.randint(2, 5)
        while h < out: add('u', h); h += 1
        last = 'u'
    elif r < 0.38:
        up = rnd.random() < (0.75 if want_up else 0.1 if want_down else 0.35)
        lo, hi = z['gap']
        if ease: hi = lo + 2
        if up: dh = rnd.randint(1, z['up']); n = rnd.randint(lo, min(hi, lo + 3))
        else:  dh = -rnd.choice([0, 1, 2, 3, 5, 7, 9, 12]); n = rnd.randint(lo, hi) + abs(dh) // 5
        gap(n); h = clamp(h + dh)
        flat(rnd.choice([8, 10, 13, 18]) if ease else rnd.choice([3, 4, 6, 9, 13, 18])); last = 'f'
    elif not ease and r < 0.38 + z['pillar']:
        for _ in range(rnd.randint(2, 5)):
            gap(rnd.randint(z['gap'][0], z['gap'][0] + 3))
            h = clamp(h + (rnd.randint(-1, 3) if want_up else rnd.randint(-4, 1) if want_down else rnd.randint(-3, 3)))
            flat(rnd.randint(2, 4))
        gap(rnd.randint(4, 8)); h = clamp(h + rnd.randint(-3, 3)); last = 'g'
    elif r < 0.80 and (want_up or (not want_down and r < 0.62)):
        n = min(rnd.randint(4, 15), HMAX - h)
        for _ in range(n): add('u', h); h += 1
        last = 'u'
    elif r < 0.80:
        n = min(rnd.randint(4, 17), h - hmin())
        for _ in range(n): add('d', h); h -= 1
        last = 'd'
    elif r < 0.90:
        h = clamp(h + (rnd.choice([2, 3, 3, -2]) if want_up else rnd.choice([-8, -6, -4, -3, -2, 2, 3])))
        flat(rnd.randint(7, 12) if ease else rnd.randint(4, 10)); last = 'f'
    else:
        flat(rnd.randint(6, 20)); last = 'f'
flat(18)
zone_marks.append((zstart, len(cols)))
W = len(cols)

fz, cur = [], 0
for x in range(W):
    if zcol[x] != cur and cols[x][0] != 'g': cur = zcol[x]
    fz.append(cur)

grid = [[0] * W for _ in range(H)]
def put(x, r, n):
    if 0 <= r < H: grid[r][x] = n
def top(x): return cols[x][1] if 0 <= x < W and cols[x][0] != 'g' else -1
for x, (k, hh) in enumerate(cols):
    if k == 'g': continue
    if k == 'f':
        lc = top(x - 1) < hh and (x == 0 or cols[x - 1][0] in 'gf')
        rc = top(x + 1) < hh and (x == W - 1 or cols[x + 1][0] in 'gf')
        put(x, hh - 1, 7 if lc and rc else 4 if lc else 6 if rc else 5); tf = hh - 2
    elif k == 'u': put(x, hh, 11); put(x, hh - 1, 20); tf = hh - 2
    else: put(x, hh - 1, 12); put(x, hh - 2, 21); tf = hh - 3
    for r in range(tf, -1, -1):
        put(x, r, 17 if rnd.random() < .003 else 8 if rnd.random() < .003 else 3)

# ---------------------------------------------------------------- objects
objs = []
def O(kind, x, y, sprite='', minX=0., maxX=0., scale=1.):
    objs.append(dict(kind=kind, sprite=sprite, x=round(x, 3), y=round(y, 3), minX=round(minX, 3), maxX=round(maxX, 3), scale=round(scale, 3)))

def runs():
    x = 0
    while x < W:
        if cols[x][0] == 'f':
            s = x
            while x < W and cols[x] == cols[s]: x += 1
            yield s, x - s, cols[s][1]
        else: x += 1
def gap_runs():
    x = 1
    while x < W:
        if cols[x][0] == 'g':
            s = x
            while x < W and cols[x][0] == 'g': x += 1
            yield s, x - s
        else: x += 1

checkpoints = {s for s, e in zone_marks}
SINK = 0.06
per_zone_power = {i: {'heart': 0, 'shield': 0, 'magnet': 0} for i in range(len(ZONES))}
for s, n, hh in runs():
    zi = zcol[s]; z = ZONES[zi]
    if s < 10: continue
    wet = hh <= z['water']
    # landing context: how far the player may still be travelling when touching down here
    prev = cols[s - 1]
    if prev[0] == 'g':
        px = s - 1
        while px > 0 and cols[px][0] == 'g': px -= 1
        land_pad = 5 + max(0, top(px) - hh) // 3     # falling from higher = landing further in
    else:
        land_pad = 3
    if wet:
        for x in range(s, s + n):
            if rnd.random() < .55:
                O('decor_front', x + .5, hh - SINK, rnd.choice(['plantBlue_1','plantBlue_2','plantBlue_5','plantDark_1','plantDark_3','plantDark_4','plantBottom_1','plantBottom_2']), scale=rnd.uniform(.8, 1.2))
        if n >= 5 and rnd.random() < .7:
            O('enemy_swim', s + n / 2, hh + rnd.uniform(1.8, max(2.0, min(4.0, z['water'] - hh - 1))), 'enemySwimming_1', s + .5, s + n - .5, 1.3)
        for i in range(rnd.randint(2, 5)):
            O('gem', s + 1 + i * 1.1, hh + 1.6, z['gem'])
        continue
    used = set()
    # fences / signs
    if z['fence'] and n >= 8 and rnd.random() < .40:
        fl = rnd.randint(2, min(5, n - 4)); fs = s + rnd.randint(1, n - fl - 2)
        for i in range(fl):
            nm = 'fenceLeft' if i == 0 else 'fenceRight' if i == fl - 1 else 'fenceMid'
            O('decor_back', fs + i + .5, hh - SINK, nm)
    elif not z['fence'] and n >= 8 and rnd.random() < .25:
        O('decor_back', s + rnd.randint(1, n - 2) + .5, hh - SINK, rnd.choice(['fenceBroken', 'fenceOpen']))
    # spikes: only where the player is already running, never inside a landing zone, never at run edges
    spiked = False
    if n >= 7 and rnd.random() < z['spike'] * 1.6:
        lo, hi = s + land_pad, s + n - 4
        if hi - lo >= 1:
            x0 = rnd.randint(lo, hi - 1)
            ln = rnd.choice([1, 1, 2])
            for x in range(x0, min(x0 + ln, hi)):
                O('spike', x + .5, hh, 'spikesLow' if zi < 2 else 'spikesHigh'); used.add(x)
            # gem arc that shows the jump over the spikes
            for i in range(5):
                t = i / 4
                O('gem', x0 - 1.5 + t * (ln + 3), hh + 1.0 + math.sin(t * math.pi) * 2.2, z['gem'])
            spiked = True
    # patrolling enemy on any run >= 2 tiles wide (keeps a landing window on narrow pillars)
    if not spiked and s not in checkpoints and n >= 2 and rnd.random() < (z['enemy'] if n >= 5 else z['enemy'] * .6):
        kind = 'enemy_walk' if (n < 5 or rnd.random() < .6) else 'enemy_spiky'
        spr = 'enemyWalking_1' if kind == 'enemy_walk' else 'enemySpikey_1'
        lo = s + (.45 if n < 5 else 1.5); hi = s + n - .45
        O(kind, rnd.uniform(lo, hi), hh, spr, lo, hi, 1.25)
    # gem rows
    if n >= 6 and rnd.random() < .35 and not spiked:
        gs = s + rnd.randint(1, n - 4)
        for i in range(rnd.randint(3, min(6, n - 2))):
            O('gem', gs + .5 + i, hh + 1.3, z['gem'])
    # plants
    for x in range(s, s + n):
        if x not in used and rnd.random() < .38:
            O('decor_front', x + rnd.uniform(.25, .75), hh - SINK, rnd.choice(z['plants']), scale=rnd.uniform(.7, 1.05))

# gaps: jump arcs, flyers, power-ups
log_cols = set()
for lx, lw, lz in logs:
    for x in range(int(lx - lw / 2), int(lx + lw / 2) + 1): log_cols.add(x)
for s, n in gap_runs():
    zi = zcol[s - 1]; z = ZONES[zi]
    h0, h1 = top(s - 1), top(s + n)
    if h0 < 0 or h1 < 0 or any(x in log_cols for x in range(s, s + n)): continue
    if rnd.random() < .6:
        apex = max(h0, h1) + 2.4; m = max(3, int(n / 1.1))
        for i in range(1, m):
            t = i / m
            y = (1 - t) ** 2 * (h0 + .9) + 2 * t * (1 - t) * apex + t * t * (h1 + .9)
            O('gem', s + t * n, y, z['gem'])
    if n >= 6 and rnd.random() < z['flyer']:
        k = rnd.choice(['enemy_fly', 'enemy_fly', 'enemy_float'])
        O(k, s + n * .5, max(h0, h1) + rnd.uniform(2.5, 4.5), 'enemyFlying_1' if k == 'enemy_fly' else 'enemyFloating_1', s + 1, s + n - 1, 1.3)
    # risky power-ups high above wide gaps
    pz = per_zone_power[zi]
    if n >= 6 and rnd.random() < .30:
        kind = min(pz, key=lambda k: pz[k] + rnd.random() * .5)
        if pz[kind] < (1 if kind == 'magnet' else 2 if kind == 'heart' else 3):
            pz[kind] += 1
            O(kind, s + n * .5, max(h0, h1) + 5.2)

for lx, lw, lz in logs:
    z = ZONES[lz]
    O('log', lx, z['water'] + .15, 'log', scale=lw)
    O('gem', lx, z['water'] + 1.6, z['gem'])
    if rnd.random() < .25:
        O('enemy_swim', lx + 2.5, z['water'] - 2.2, 'enemySwimming_1', lx - 3, lx + 5, 1.3)
# end of a log run: reward
for i in range(1, len(logs)):
    if logs[i][0] - logs[i - 1][0] > 12 or i == len(logs) - 1:
        lx = logs[i - 1][0]
        if rnd.random() < .5: O('heart', lx, ZONES[logs[i - 1][2]]['water'] + 4.5)

O('sign', 3.5, top(3) - SINK, 'signArrow_right')
for i, (s, e) in enumerate(zone_marks):
    if i > 0: O('checkpoint', s + 3.5, top(s + 3) - SINK, 'flagGreen_down')
O('goal', W - 6.5, top(W - 7) - SINK, 'flagRed_up', scale=1.3)

# ---------------------------------------------------------------- export map
tiles = []
for r in range(H):
    for x in range(W):
        tiles.append(ZONES[zcol[x]]['block'] // 3 * 32 + grid[r][x] if grid[r][x] else 0)
BLOCK_TO_ATLAS = {0: 3, 1: 1, 2: 2, 3: 0}   # sheet block (Brown,Yellow,Blue,Green) -> atlas row (Meadow..Volcano)
tiles = [0 if t == 0 else BLOCK_TO_ATLAS[t // 32] * 32 + t % 32 for t in tiles]
data = dict(width=W, height=H, seed=SEED,
            zones=[dict(name=z['name'], fluid=z['fluid'], water=z['water'], lethal=z['lethal'],
                        startX=a, endX=b, skyTop=list(z['sky'][0]), skyBottom=list(z['sky'][1]),
                        hill=list(z['hill']), hills=z['hills'], gem=z['gem'])
                   for z, (a, b) in zip(ZONES, zone_marks)],
            tiles=tiles, columnZone=zcol, fluidZone=fz,
            columnTop=[top(x) for x in range(W)], columnKind=''.join(c[0] for c in cols),
            objects=objs)
json.dump(data, open(os.path.join(OUT_MAP, 'adventure_map.json'), 'w'), separators=(',', ':'))

# ---------------------------------------------------------------- atlas (row = zone, col = tile 1..27; row 4 = fluids)
SHEET = Image.open(os.path.join(TD, 'Tilesheet/tilesheet_complete.png')).convert('RGBA')
P = 2; C = 64 + 2 * P
atlas = Image.new('RGBA', (27 * C, 5 * C))
def paste_padded(im, cx, cy):
    a = np.array(im)
    a = np.pad(a, ((P, P), (P, P), (0, 0)), mode='edge')
    atlas.paste(Image.fromarray(a), (cx * C, cy * C))
for zr, z in enumerate(ZONES):
    b = z['block']
    for n in range(1, 28):
        c, r = IDX[n]
        paste_padded(SHEET.crop((c * 64, (b + r) * 64, c * 64 + 64, (b + r) * 64 + 64)), n - 1, zr)
for i, f in enumerate(FLUIDS):
    paste_padded(Image.open(os.path.join(TD, f'PNG/Other/fluid{f}.png')).convert('RGBA'), i * 2, 4)
    paste_padded(Image.open(os.path.join(TD, f'PNG/Other/fluid{f}_top.png')).convert('RGBA'), i * 2 + 1, 4)
atlas.save(os.path.join(OUT_SPR, 'tiles_atlas.png'))

# ---------------------------------------------------------------- generated sprites (flat Kenney-like style)
def ss(w, h): return Image.new('RGBA', (w * 4, h * 4)), (lambda im: im.resize((w, h), Image.LANCZOS))
# log: 128 x 40, drawn per tile width and stretched by sliced sprite in Unity
im, down = ss(128, 40); d = ImageDraw.Draw(im)
d.rounded_rectangle((8, 8, 504, 152), radius=70, fill=(122, 74, 44))
d.rounded_rectangle((8, 8, 504, 70), radius=60, fill=(150, 96, 58))
for x in (120, 250, 380): d.line((x, 30, x + 30, 130), fill=(100, 60, 36), width=10)
d.ellipse((440, 8, 504, 152), fill=(214, 168, 110)); d.ellipse((456, 36, 490, 124), outline=(160, 112, 66), width=8)
down(im).save(os.path.join(OUT_SPR, 'log.png'))
# heart
im, down = ss(64, 64); d = ImageDraw.Draw(im)
d.ellipse((24, 40, 136, 150), fill=(226, 64, 80)); d.ellipse((120, 40, 232, 150), fill=(226, 64, 80))
d.polygon([(30, 112), (226, 112), (128, 226)], fill=(226, 64, 80))
d.ellipse((60, 66, 100, 100), fill=(255, 150, 160))
down(im).save(os.path.join(OUT_SPR, 'heart.png'))
# shield bubble icon
im, down = ss(64, 64); d = ImageDraw.Draw(im)
d.ellipse((16, 16, 240, 240), fill=(110, 200, 255, 120), outline=(160, 230, 255), width=14)
d.ellipse((60, 52, 110, 92), fill=(255, 255, 255, 200))
down(im).save(os.path.join(OUT_SPR, 'shield.png'))
# magnet
im, down = ss(64, 64); d = ImageDraw.Draw(im)
d.arc((36, 30, 220, 214), 180, 360, fill=(226, 64, 80), width=56)
d.rectangle((36, 120, 92, 200), fill=(226, 64, 80)); d.rectangle((164, 120, 220, 200), fill=(226, 64, 80))
d.rectangle((36, 176, 92, 220), fill=(235, 235, 240)); d.rectangle((164, 176, 220, 220), fill=(235, 235, 240))
down(im).save(os.path.join(OUT_SPR, 'magnet.png'))
# fx: soft circle + 4-point spark
im, down = ss(32, 32); d = ImageDraw.Draw(im); d.ellipse((4, 4, 124, 124), fill=(255, 255, 255))
down(im).save(os.path.join(OUT_SPR, 'fx_circle.png'))
im, down = ss(32, 32); d = ImageDraw.Draw(im)
d.polygon([(64, 0), (76, 52), (128, 64), (76, 76), (64, 128), (52, 76), (0, 64), (52, 52)], fill=(255, 255, 255))
down(im).save(os.path.join(OUT_SPR, 'fx_spark.png'))
im = Image.new('RGBA', (4, 4), (255, 255, 255, 255)); im.save(os.path.join(OUT_SPR, 'fx_pixel.png'))

# ---------------------------------------------------------------- preview
pv = Image.new('RGB', (W * 4, H * 4), (150, 200, 240)); pd = ImageDraw.Draw(pv)
for x in range(W):
    z = ZONES[fz[x]]
    pd.rectangle((x * 4, (H - z['water']) * 4, x * 4 + 3, H * 4), fill=(60, 120, 200) if not z['lethal'] else (200, 70, 60))
    if top(x) > 0:
        pd.rectangle((x * 4, (H - top(x)) * 4, x * 4 + 3, H * 4), fill=[(90, 170, 60), (220, 190, 60), (60, 80, 140), (110, 50, 50)][zcol[x]])
for o in objs:
    c = {'spike': (255, 0, 0), 'log': (140, 90, 40), 'heart': (255, 80, 160), 'shield': (80, 220, 255), 'magnet': (255, 255, 0)}.get(o['kind'])
    if o['kind'].startswith('enemy'): c = (30, 30, 30)
    if c: pd.rectangle((o['x'] * 4 - 2, (H - o['y']) * 4 - 6, o['x'] * 4 + 2, (H - o['y']) * 4), fill=c)
pv.save(os.path.join(ROOT, 'Tools/MapGen/preview.png'))
kinds = {}
for o in objs: kinds[o['kind']] = kinds.get(o['kind'], 0) + 1
print('W', W, 'H', H, 'zones', zone_marks, 'logs', len(logs), kinds)
