# -*- coding: utf-8 -*-
"""
ELECTRIC WORLD: LAST LIGHT - 개발용 임시 효과음·환경음 합성기.

보유한 에셋 팩(Assets/ASSET/*.zip)에는 효과음이 없고(타이틀 음악 1곡뿐),
유료 효과음 팩은 구매하지 않았으므로, 연출에 필요한 소리를 직접 합성한 '임시 음원'이다.
최종 음원이 준비되면 같은 파일 이름으로 덮어쓰면 된다 (Docs/ASSET_INVENTORY.md).

실행: python Tools/audio_gen.py   (numpy 필요)
출력: Assets/Game/Audio/Generated/{sfx,amb,bgm}_*.wav  (44.1kHz, 16bit, mono)
"""
import os, wave
import numpy as np

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'Assets', 'Game', 'Audio', 'Generated')
os.makedirs(OUT, exist_ok=True)
SR = 44100
rng = np.random.default_rng(47)


def t(sec):
    return np.arange(int(SR * sec)) / SR


def env(n, a=0.005, d=0.1, s=0.0, total=None):
    """간단한 AD(S) 엔벨로프."""
    x = np.arange(n) / SR
    e = np.minimum(1.0, x / max(a, 1e-4))
    dec = np.exp(-np.maximum(0, x - a) / max(d, 1e-4))
    return e * (s + (1 - s) * dec)


def noise(n):
    return rng.uniform(-1, 1, n)


def lowpass(x, cutoff):
    """1차 저역 통과 (cutoff 는 Hz, 스칼라 또는 배열)."""
    c = np.broadcast_to(np.asarray(cutoff, dtype=float), x.shape)
    a = 1 - np.exp(-2 * np.pi * c / SR)
    y = np.zeros_like(x)
    acc = 0.0
    for i in range(len(x)):
        acc += a[i] * (x[i] - acc)
        y[i] = acc
    return y


def highpass(x, cutoff):
    return x - lowpass(x, cutoff)


def sweep(f0, f1, sec, shape='sin'):
    tt = t(sec)
    f = np.geomspace(max(f0, 1), max(f1, 1), len(tt))
    ph = 2 * np.pi * np.cumsum(f) / SR
    if shape == 'sq':
        return np.sign(np.sin(ph))
    if shape == 'saw':
        return 2 * ((ph / (2 * np.pi)) % 1) - 1
    return np.sin(ph)


def crackle(sec, density=30, sharp=0.002):
    n = int(SR * sec)
    x = np.zeros(n)
    k = int(density * sec)
    for _ in range(k):
        p = int(rng.integers(0, max(1, n - 200)))
        ln = int(min(rng.integers(40, 400), n - p))
        x[p:p + ln] += noise(ln) * np.exp(-np.arange(ln) / (SR * sharp)) * rng.uniform(0.2, 1.0)
    return x


def norm(x, peak=0.85):
    m = np.max(np.abs(x)) + 1e-9
    return x / m * peak


def loopify(x, fade=0.4):
    """앞뒤를 겹쳐 끊김 없이 반복되게 만든다."""
    f = int(SR * fade)
    head = x[:f].copy()
    body = x[f:].copy()
    w = np.linspace(0, 1, f)
    body[-f:] = body[-f:] * (1 - w) + head * w
    return body


def write(name, x, peak=0.85):
    x = norm(x, peak)
    data = (np.clip(x, -1, 1) * 32767).astype('<i2')
    with wave.open(os.path.join(OUT, name + '.wav'), 'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes(data.tobytes())
    print('wrote', name, round(len(x) / SR, 2), 's')


# ============================================================ 환경음 (반복)
def amb_fire():
    sec = 8.4
    n = int(SR * sec)
    rumble = lowpass(noise(n), 90) * 3.0
    roar = lowpass(noise(n), 700) * (0.6 + 0.4 * np.sin(2 * np.pi * 0.23 * t(sec)))
    wind = lowpass(highpass(noise(n), 200), 1400 + 600 * np.sin(2 * np.pi * 0.11 * t(sec))) * 0.35
    cr = highpass(crackle(sec, 55, 0.0015), 900) * 0.9
    return loopify(rumble * 0.6 + roar * 0.5 + wind + cr)


def amb_house():
    sec = 8.4
    n = int(SR * sec)
    distant = lowpass(noise(n), 160) * 1.2
    wind = lowpass(highpass(noise(n), 300), 900 + 400 * np.sin(2 * np.pi * 0.07 * t(sec))) * 0.5
    cr = lowpass(crackle(sec, 9, 0.002), 1800) * 0.35
    creak = np.zeros(n)
    for p in (1.7, 5.9):
        s = int(p * SR); c = sweep(180, 140, 0.6, 'saw') * env(int(0.6 * SR), 0.05, 0.3)
        creak[s:s + len(c)] += lowpass(c, 600) * 0.25
    return loopify(distant + wind + cr + creak)


def amb_digital():
    sec = 6.0
    tt = t(sec)
    hum = np.sin(2 * np.pi * 110 * tt) * 0.25 + np.sin(2 * np.pi * 220.5 * tt) * 0.12
    shimmer = np.sin(2 * np.pi * 1760 * tt) * (0.03 + 0.03 * np.sin(2 * np.pi * 0.5 * tt))
    air = lowpass(noise(len(tt)), 2500) * 0.08
    return loopify(hum + shimmer + air)


def amb_lab():
    sec = 8.0
    tt = t(sec)
    hvac = lowpass(noise(len(tt)), 300) * 1.4
    hum = np.sin(2 * np.pi * 60 * tt) * 0.18 + np.sin(2 * np.pi * 120 * tt) * 0.1
    beep = np.zeros(len(tt))
    for p in (1.2, 5.2):
        s = int(p * SR); b = np.sin(2 * np.pi * 1320 * t(0.08)) * env(int(0.08 * SR), 0.002, 0.04)
        beep[s:s + len(b)] += b * 0.12
    return loopify(hvac + hum + beep)


def amb_lab_dark():
    """정전 이후: 환기 멈춤, 낮은 잔류 웅웅거림과 간헐적 잔류 스파크."""
    sec = 9.0
    tt = t(sec)
    low = lowpass(noise(len(tt)), 120) * 0.9
    buzz = np.sign(np.sin(2 * np.pi * 50 * tt)) * 0.04 * (0.5 + 0.5 * np.sin(2 * np.pi * 0.3 * tt))
    sp = np.zeros(len(tt))
    for p in (0.8, 2.9, 3.05, 6.4):
        s = int(p * SR); z = highpass(crackle(0.25, 60, 0.001), 1500)
        sp[s:s + len(z)] += z * 0.6
    drip = np.zeros(len(tt))
    for p in (4.4, 8.1):
        s = int(p * SR); d = sweep(1400, 700, 0.06) * env(int(0.06 * SR), 0.001, 0.02)
        drip[s:s + len(d)] += d * 0.15
    return loopify(low + buzz + sp + drip)


# ============================================================ 효과음
def sfx_step():
    n = int(0.12 * SR)
    return lowpass(noise(n), 900) * env(n, 0.002, 0.03) + np.sin(2 * np.pi * 90 * t(0.12)) * env(n, 0.002, 0.04) * 0.6


def sfx_step_drag():
    """끌리는 발: 짧은 쿵 + 바닥을 긁는 소리."""
    a = sfx_step()
    n = int(0.32 * SR)
    sc = lowpass(highpass(noise(n), 400), 2200) * env(n, 0.04, 0.12) * 0.5
    out = np.zeros(int(0.45 * SR)); out[:len(a)] += a * 0.7; out[int(0.08 * SR):int(0.08 * SR) + n] += sc
    return out


def sfx_debris():
    n = int(0.9 * SR)
    x = np.zeros(n)
    for i in range(9):
        p = int(rng.uniform(0, 0.6) * SR); ln = int(0.12 * SR)
        x[p:p + ln] += lowpass(noise(ln), rng.uniform(600, 2500)) * env(ln, 0.001, 0.03) * rng.uniform(0.3, 1)
    return x + lowpass(noise(n), 120) * env(n, 0.01, 0.3)


def sfx_fire_burst():
    n = int(1.2 * SR)
    return lowpass(noise(n), np.linspace(2500, 300, n)) * env(n, 0.03, 0.4) + crackle(1.2, 40) * 0.4


def sfx_pc_on():
    a = sweep(80, 60, 0.5) * env(int(0.5 * SR), 0.01, 0.3) * 0.6
    whine = np.sin(2 * np.pi * 15000 * t(0.5)) * 0.03
    click = np.zeros(int(0.5 * SR)); click[:300] = noise(300) * 0.8
    return a + whine + click


def sfx_pc_beep():
    x = np.concatenate([np.sin(2 * np.pi * 880 * t(0.09)), np.zeros(int(0.04 * SR)), np.sin(2 * np.pi * 1320 * t(0.12))])
    return x * 0.6 * env(len(x), 0.002, 0.3, 0.5)


def sfx_connect():
    """접속: 점점 높아지는 웅웅거림 + 반짝임."""
    sec = 2.2
    n = int(sec * SR)
    rise = sweep(110, 880, sec, 'saw') * np.linspace(0.1, 0.6, n)
    rise = lowpass(rise, np.linspace(400, 5000, n))
    sh = np.sin(2 * np.pi * np.linspace(1500, 4000, n) * t(sec)) * np.linspace(0, 0.25, n)
    return (rise + sh) * env(n, 0.3, 3.0)


def sfx_absorb():
    """빛이 컴퓨터로 빨려 들어감: 슈욱."""
    sec = 1.1
    n = int(sec * SR)
    w = lowpass(noise(n), np.geomspace(300, 6000, n)) * np.linspace(0.2, 1.0, n) ** 2
    tone = sweep(300, 2400, sec) * np.linspace(0, 0.5, n)
    x = w + tone
    x[-int(0.08 * SR):] *= np.linspace(1, 0, int(0.08 * SR))
    return x


def sfx_land():
    n = int(0.35 * SR)
    return np.sin(2 * np.pi * np.linspace(140, 50, n) * t(0.35)) * env(n, 0.002, 0.08) + lowpass(noise(n), 700) * env(n, 0.001, 0.04) * 0.6


def sfx_window_pop():
    x = np.concatenate([np.sin(2 * np.pi * 660 * t(0.06)), np.sin(2 * np.pi * 990 * t(0.08))])
    x = np.sign(x) * 0.3 + x * 0.3
    return x * env(len(x), 0.001, 0.12, 0.2)


def sfx_alarm():
    seg = []
    for i in range(4):
        f = 740 if i % 2 == 0 else 560
        s = np.sign(np.sin(2 * np.pi * f * t(0.18))) * 0.4
        seg.append(lowpass(s, 3000))
    x = np.concatenate(seg)
    return x * env(len(x), 0.005, 2, 0.9)


def sfx_transfer():
    """위이잉— 쑥!"""
    a_sec, b_sec = 1.5, 0.35
    na = int(a_sec * SR)
    wi = sweep(220, 1100, a_sec, 'saw') * (0.5 + 0.5 * np.sin(2 * np.pi * np.linspace(6, 18, na) * t(a_sec)))
    wi = lowpass(wi, 3500) * np.linspace(0.2, 0.8, na)
    nb = int(b_sec * SR)
    suk = lowpass(noise(nb), np.geomspace(6000, 200, nb)) * env(nb, 0.002, 0.12)
    pop = np.sin(2 * np.pi * np.geomspace(1200, 90, nb) * t(b_sec)) * env(nb, 0.001, 0.08)
    return np.concatenate([wi, suk * 0.9 + pop])


def sfx_fall_whoosh():
    n = int(0.5 * SR)
    return lowpass(noise(n), np.geomspace(500, 3000, n)) * np.linspace(0.2, 1, n)


def sfx_glass_knock():
    n = int(0.25 * SR)
    tone = (np.sin(2 * np.pi * 2300 * t(0.25)) + 0.6 * np.sin(2 * np.pi * 3700 * t(0.25)) + 0.3 * np.sin(2 * np.pi * 5100 * t(0.25)))
    thud = lowpass(noise(n), 1200)
    return tone * env(n, 0.001, 0.05) * 0.5 + thud * env(n, 0.001, 0.015)


def sfx_door_slide():
    sec = 0.9
    n = int(sec * SR)
    hiss = highpass(noise(n), 1500) * env(n, 0.01, 0.25) * 0.6
    motor = sweep(150, 260, sec, 'saw') * 0.3
    motor = lowpass(motor, 900) * env(n, 0.05, 0.6, 0.3)
    clunk = np.zeros(n)
    s = int(0.72 * SR); c = lowpass(noise(int(0.15 * SR)), 500) * env(int(0.15 * SR), 0.001, 0.04)
    clunk[s:s + len(c)] += c
    return hiss + motor + clunk


def sfx_door_bang():
    n = int(0.6 * SR)
    return lowpass(noise(n), 1200) * env(n, 0.001, 0.08) + np.sin(2 * np.pi * 70 * t(0.6)) * env(n, 0.001, 0.2) * 0.8


def sfx_baton_on():
    """제압봉 작동: 찰칵 + 전기 웅."""
    click = np.zeros(int(0.06 * SR)); click[:200] = noise(200)
    sec = 0.9
    tt = t(sec)
    buzz = np.sign(np.sin(2 * np.pi * 120 * tt)) * 0.3 + np.sin(2 * np.pi * 240 * tt) * 0.2
    buzz = lowpass(buzz + highpass(crackle(sec, 50, 0.0008), 2000) * 0.4, 4500)
    buzz *= env(len(tt), 0.04, 0.6, 0.6)
    return np.concatenate([click * 0.8, buzz])


def sfx_charge_up():
    sec = 1.6
    n = int(sec * SR)
    hum = sweep(60, 480, sec, 'saw') * np.linspace(0.05, 0.6, n)
    hum = lowpass(hum, np.linspace(200, 4000, n))
    cr = highpass(crackle(sec, 80, 0.0008), 2500) * np.linspace(0, 0.6, n)
    return hum + cr


def sfx_discharge_big():
    sec = 2.4
    n = int(sec * SR)
    boom = np.sin(2 * np.pi * np.geomspace(90, 30, n) * t(sec)) * env(n, 0.002, 0.6)
    crack = highpass(noise(n), 1200) * env(n, 0.001, 0.12)
    zap = highpass(crackle(sec, 140, 0.0015), 1500) * env(n, 0.001, 0.9)
    body = lowpass(noise(n), 800) * env(n, 0.001, 0.5)
    return boom * 1.2 + crack * 0.8 + zap * 0.7 + body * 0.6


def sfx_glass_break():
    sec = 1.4
    n = int(sec * SR)
    x = highpass(noise(n), 3000) * env(n, 0.001, 0.08)
    for i in range(30):
        p = int(rng.uniform(0.02, 1.1) * SR); ln = int(0.05 * SR)
        f = rng.uniform(2500, 7000)
        x[p:p + ln] += np.sin(2 * np.pi * f * t(0.05)) * env(ln, 0.001, 0.015) * rng.uniform(0.1, 0.5)
    return x


def sfx_power_down():
    sec = 1.4
    n = int(sec * SR)
    x = sweep(240, 30, sec, 'saw') * env(n, 0.005, 0.8)
    return lowpass(x, np.geomspace(3000, 100, n)) + np.sign(np.sin(2 * np.pi * np.geomspace(120, 20, n) * t(sec))) * 0.1 * env(n, 0.005, 0.5)


def sfx_spark_small():
    n = int(0.3 * SR)
    return highpass(crackle(0.3, 50, 0.0008), 1500) * env(n, 0.001, 0.12, 0.1)


def sfx_metal_creak():
    sec = 1.0
    n = int(sec * SR)
    f = 210 + 40 * np.sin(2 * np.pi * 3 * t(sec)) + np.linspace(0, -60, n)
    ph = 2 * np.pi * np.cumsum(f) / SR
    x = (2 * ((ph / (2 * np.pi)) % 1) - 1) * (0.5 + 0.5 * np.sin(2 * np.pi * 23 * t(sec)))
    return lowpass(x, 1400) * env(n, 0.08, 0.6, 0.4)


def sfx_squeeze():
    sec = 0.8
    n = int(sec * SR)
    rub = lowpass(highpass(noise(n), 300), 1800) * (0.4 + 0.6 * np.abs(np.sin(2 * np.pi * 4 * t(sec))))
    return rub * env(n, 0.05, 0.4, 0.2)


# UI
def sfx_ui_move():
    return np.sign(np.sin(2 * np.pi * 1320 * t(0.035))) * env(int(0.035 * SR), 0.001, 0.015) * 0.5


def sfx_ui_ok():
    x = np.concatenate([np.sin(2 * np.pi * 988 * t(0.05)), np.sin(2 * np.pi * 1480 * t(0.08))])
    return np.sign(x) * 0.25 * env(len(x), 0.001, 0.08, 0.2)


def sfx_ui_cancel():
    x = np.concatenate([np.sin(2 * np.pi * 740 * t(0.05)), np.sin(2 * np.pi * 494 * t(0.08))])
    return np.sign(x) * 0.25 * env(len(x), 0.001, 0.08, 0.2)


def sfx_ui_no():
    x = np.sign(np.sin(2 * np.pi * 160 * t(0.12)))
    return x * 0.3 * env(len(x), 0.001, 0.08)


def sfx_text():
    return np.sin(2 * np.pi * 1700 * t(0.02)) * env(int(0.02 * SR), 0.001, 0.008) * 0.4


def sfx_advance():
    return np.sin(2 * np.pi * 1180 * t(0.04)) * env(int(0.04 * SR), 0.001, 0.02) * 0.5


# 전투
def sfx_hit_phys():
    n = int(0.25 * SR)
    return lowpass(noise(n), 1500) * env(n, 0.001, 0.03) + np.sin(2 * np.pi * np.geomspace(180, 60, n) * t(0.25)) * env(n, 0.001, 0.06)


def sfx_hit_elec():
    n = int(0.35 * SR)
    return highpass(crackle(0.35, 120, 0.0008), 1200) * env(n, 0.001, 0.15) + np.sign(np.sin(2 * np.pi * 95 * t(0.35))) * env(n, 0.001, 0.05) * 0.3


def sfx_flick():
    n = int(0.18 * SR)
    snap = highpass(noise(n), 2500) * env(n, 0.0005, 0.01)
    zz = highpass(crackle(0.18, 60, 0.0006), 2000) * env(n, 0.001, 0.08)
    return snap + zz * 0.7


def sfx_zap_shot():
    sec = 0.25
    n = int(sec * SR)
    return sweep(2400, 600, sec, 'sq') * env(n, 0.001, 0.1) * 0.4 + highpass(crackle(sec, 40, 0.0006), 2000) * 0.5


def sfx_whip():
    sec = 0.45
    n = int(sec * SR)
    sw = lowpass(noise(n), np.geomspace(400, 5000, n)) * env(n, 0.12, 0.08)
    return sw + highpass(crackle(sec, 90, 0.001), 1500) * np.linspace(0, 1, n) * 0.6


def sfx_kick_dash():
    sec = 0.3
    n = int(sec * SR)
    return lowpass(noise(n), np.geomspace(300, 3000, n)) * env(n, 0.1, 0.05)


def sfx_enemy_attack():
    sec = 0.3
    n = int(sec * SR)
    return sweep(500, 1500, sec, 'sq') * env(n, 0.005, 0.1) * 0.3 + lowpass(noise(n), 800) * env(n, 0.002, 0.05) * 0.5


def sfx_baton_swing():
    sec = 0.5
    n = int(sec * SR)
    return lowpass(noise(n), np.geomspace(300, 2500, n)) * env(n, 0.15, 0.08) + sfx_baton_on()[:n] * 0.4


def sfx_shot_enemy():
    sec = 0.2
    n = int(sec * SR)
    return sweep(1800, 300, sec, 'sq') * env(n, 0.001, 0.06) * 0.4


def sfx_enemy_die():
    sec = 1.0
    n = int(sec * SR)
    return sweep(600, 40, sec, 'saw') * env(n, 0.01, 0.4) * 0.4 + crackle(sec, 40, 0.002) * 0.5 + lowpass(noise(n), 500) * env(n, 0.2, 0.3) * 0.6


def sfx_guard_break():
    sec = 0.6
    n = int(sec * SR)
    clang = (np.sin(2 * np.pi * 620 * t(sec)) + np.sin(2 * np.pi * 931 * t(sec)) * 0.7) * env(n, 0.001, 0.2)
    return clang * 0.6 + highpass(crackle(sec, 80, 0.001), 1500) * 0.5


def sfx_heal():
    x = np.concatenate([np.sin(2 * np.pi * f * t(0.09)) for f in (660, 880, 1100, 1320)])
    return x * 0.4 * env(len(x), 0.005, 0.4, 0.4)


def sfx_levelup():
    x = np.concatenate([np.sign(np.sin(2 * np.pi * f * t(0.1))) for f in (523, 659, 784, 1047)] + [np.sign(np.sin(2 * np.pi * 1047 * t(0.3)))])
    return x * 0.25 * env(len(x), 0.005, 1.0, 0.5)


def sfx_battle_start():
    sec = 0.6
    n = int(sec * SR)
    return sweep(200, 1600, sec, 'saw') * env(n, 0.01, 0.4) * 0.4 + highpass(noise(n), 3000) * env(n, 0.001, 0.05)


def sfx_wire_arc():
    sec = 0.8
    n = int(sec * SR)
    return highpass(crackle(sec, 260, 0.0008), 1000) * env(n, 0.01, 0.5, 0.3) + np.sign(np.sin(2 * np.pi * 100 * t(sec))) * 0.15 * env(n, 0.01, 0.4)


def sfx_acid():
    sec = 0.7
    n = int(sec * SR)
    return highpass(lowpass(noise(n), 5000), 1500) * env(n, 0.02, 0.4) + crackle(sec, 30, 0.003) * 0.3


def sfx_light_off():
    n = int(0.2 * SR)
    x = np.zeros(n); x[:150] = noise(150)
    return x + np.sin(2 * np.pi * 100 * t(0.2)) * env(n, 0.001, 0.05) * 0.3


def sfx_heartbeat():
    def beat(f):
        return np.sin(2 * np.pi * f * t(0.12)) * env(int(0.12 * SR), 0.005, 0.04)
    x = np.zeros(int(0.9 * SR)); b1 = beat(60); b2 = beat(55)
    x[:len(b1)] += b1; x[int(0.22 * SR):int(0.22 * SR) + len(b2)] += b2 * 0.7
    return x


# ============================================================ 임시 음악
def bgm_battle():
    """짧은 반복 전투 음악 (임시). 128 BPM, 4마디."""
    bpm = 128; beat = 60 / bpm; bars = 4
    sec = beat * 4 * bars
    n = int(sec * SR)
    out = np.zeros(n)
    bass_notes = [45, 45, 48, 43]  # A2 A2 C3 G2
    for bar in range(bars):
        root = bass_notes[bar]
        for i in range(8):
            st = int((bar * 4 + i * 0.5) * beat * SR); ln = int(0.45 * beat * SR)
            f = 440 * 2 ** ((root - 69 + (12 if i % 4 == 3 else 0)) / 12)
            s = (2 * ((f * t(ln / SR)) % 1) - 1) * env(ln, 0.003, 0.12, 0.3)
            out[st:st + ln] += lowpass(s, 900) * 0.5
        for i in range(4):
            st = int((bar * 4 + i) * beat * SR); ln = int(0.2 * SR)
            k = np.sin(2 * np.pi * np.geomspace(120, 45, ln) * t(ln / SR)) * env(ln, 0.001, 0.07)
            out[st:st + ln] += k * 0.9
            if i % 2 == 1:
                sn = highpass(noise(ln), 1500) * env(ln, 0.001, 0.05)
                out[st:st + ln] += sn * 0.35
        for i in range(8):
            st = int((bar * 4 + i * 0.5 + 0.25) * beat * SR); ln = int(0.05 * SR)
            out[st:st + ln] += highpass(noise(ln), 6000) * env(ln, 0.001, 0.01) * 0.2
        lead = [76, 74, 72, 74, 76, 79, 76, 72] if bar % 2 == 0 else [72, 71, 69, 71, 72, 74, 71, 67]
        for i, m in enumerate(lead):
            st = int((bar * 4 + i * 0.5) * beat * SR); ln = int(0.4 * beat * SR)
            f = 440 * 2 ** ((m - 69) / 12)
            s = np.sign(np.sin(2 * np.pi * f * t(ln / SR))) * env(ln, 0.005, 0.1, 0.2)
            out[st:st + ln] += s * 0.12
    return out


def bgm_tension():
    """보스 입장·긴장 (임시): 느린 저음 펄스와 불협 패드."""
    sec = 9.6
    tt = t(sec)
    n = len(tt)
    pad = (np.sin(2 * np.pi * 110 * tt) + np.sin(2 * np.pi * 116.5 * tt) * 0.7 + np.sin(2 * np.pi * 164.8 * tt) * 0.4)
    pad = lowpass(pad, 600) * (0.5 + 0.3 * np.sin(2 * np.pi * 0.1 * tt))
    pulse = np.zeros(n)
    for i in range(int(sec / 0.8)):
        st = int(i * 0.8 * SR); ln = int(0.3 * SR)
        pulse[st:st + ln] += np.sin(2 * np.pi * np.geomspace(80, 40, ln) * t(ln / SR)) * env(ln, 0.002, 0.1)
    return loopify(pad * 0.5 + pulse)



# ============================================================ 실시간 잠입 개편 (2026-11-14)
def pad(x, n):
    return np.concatenate([x, np.zeros(max(0, n - len(x)))])[:max(n, len(x))]


def mix(*xs):
    n = max(len(x) for x in xs)
    return sum(pad(x, n) for x in xs)


def sfx_zap():
    """톡!: 아주 짧은 딱 + 고음 지직."""
    n = int(0.14 * SR)
    return highpass(crackle(0.14, 140, 0.0008), 2000) * env(n, 0.001, 0.05) * 0.9 + sweep(2400, 900, 0.14) * env(n, 0.001, 0.04) * 0.3


def sfx_discharge_small():
    n = int(0.6 * SR)
    return lowpass(noise(n), 3000) * env(n, 0.002, 0.18) + highpass(crackle(0.6, 90, 0.001), 1200) * env(n, 0.002, 0.3) * 0.8 + np.sin(2 * np.pi * 70 * t(0.6)) * env(n, 0.002, 0.2) * 0.6


def sfx_dodge():
    n = int(0.22 * SR)
    return highpass(noise(n), 1500) * env(n, 0.03, 0.08) * 0.5 + lowpass(noise(n), 500) * env(n, 0.01, 0.06) * 0.4


def sfx_target_on():
    n = int(0.25 * SR)
    return sweep(500, 1400, 0.25) * env(n, 0.01, 0.12) * 0.4 + np.sin(2 * np.pi * 60 * t(0.25)) * env(n, 0.01, 0.2) * 0.2


def sfx_suspect():
    """의심: 짧은 두 음 '삐-삐↑' (부드럽게)."""
    a = np.sin(2 * np.pi * 660 * t(0.09)) * env(int(0.09 * SR), 0.004, 0.05)
    b = np.sin(2 * np.pi * 990 * t(0.12)) * env(int(0.12 * SR), 0.004, 0.06)
    return np.concatenate([a, np.zeros(int(0.04 * SR)), b]) * 0.5


def sfx_alert():
    """발견: 날카로운 세 번 경고음."""
    one = np.sign(np.sin(2 * np.pi * 1180 * t(0.07))) * env(int(0.07 * SR), 0.002, 0.05) * 0.4
    gap = np.zeros(int(0.03 * SR))
    return np.concatenate([one, gap, one, gap, one * 1.2])


def sfx_hurt():
    n = int(0.25 * SR)
    return lowpass(noise(n), 1200) * env(n, 0.002, 0.06) + np.sin(2 * np.pi * np.geomspace(300, 120, n) * t(0.25)) * env(n, 0.002, 0.1) * 0.6


def sfx_robot_hit():
    n = int(0.2 * SR)
    clank = sum(np.sin(2 * np.pi * f * t(0.2)) for f in (820, 1330, 2010)) / 3
    return clank * env(n, 0.001, 0.06) * 0.7 + highpass(crackle(0.2, 80, 0.001), 1500) * env(n, 0.001, 0.1) * 0.5


def sfx_baton_hit():
    n = int(0.3 * SR)
    swish = highpass(noise(int(0.12 * SR)), 2000) * env(int(0.12 * SR), 0.03, 0.05) * 0.5
    zap = np.concatenate([np.zeros(int(0.1 * SR)), highpass(crackle(0.2, 120, 0.001), 1500) * env(int(0.2 * SR), 0.001, 0.08)])
    return mix(swish, zap)


def sfx_dash():
    n = int(0.35 * SR)
    return lowpass(noise(n), 700) * env(n, 0.01, 0.15) * 0.8 + sweep(200, 90, 0.35) * env(n, 0.005, 0.2) * 0.4


def sfx_clatter():
    """공구 쟁반 덜컹 (소리 미끼)."""
    out = np.zeros(int(0.8 * SR))
    for k, p in enumerate((0.0, 0.07, 0.15, 0.26, 0.4, 0.55)):
        st = int(p * SR); ln = int(0.18 * SR)
        f = 1500 + 700 * ((k * 37) % 5) / 5
        hit = (np.sin(2 * np.pi * f * t(0.18)) + np.sin(2 * np.pi * f * 1.47 * t(0.18)) * 0.6) * env(ln, 0.001, 0.05)
        out[st:st + ln] += hit * (1 - k * 0.12)
    return out * 0.6


def sfx_broadcast():
    """시설 방송 앞 차임 + 스피커 지직."""
    ch = mix(np.sin(2 * np.pi * 784 * t(0.35)) * env(int(0.35 * SR), 0.005, 0.25),
             np.concatenate([np.zeros(int(0.25 * SR)), np.sin(2 * np.pi * 587 * t(0.5)) * env(int(0.5 * SR), 0.005, 0.3)]))
    hiss = highpass(noise(len(ch)), 2500) * 0.05
    return (ch + hiss) * 0.6


def sfx_light_on():
    n = int(0.5 * SR)
    clunk = lowpass(noise(int(0.06 * SR)), 900) * env(int(0.06 * SR), 0.001, 0.02)
    hum = np.sin(2 * np.pi * 120 * t(0.5)) * env(n, 0.05, 0.4) * 0.25 + np.sin(2 * np.pi * 240 * t(0.5)) * env(n, 0.05, 0.3) * 0.1
    return mix(clunk, hum)


def sfx_bin_twitch():
    n = int(0.45 * SR)
    servo = np.sin(2 * np.pi * np.linspace(300, 520, n) * t(0.45)) * env(n, 0.02, 0.2) * 0.25
    return mix(servo, highpass(crackle(0.3, 40, 0.002), 900) * 0.5)


def sfx_bubble():
    out = np.zeros(int(0.9 * SR))
    for p in (0.0, 0.2, 0.33, 0.6):
        st = int(p * SR); ln = int(0.07 * SR)
        out[st:st + ln] += sweep(300, 700, 0.07) * env(ln, 0.002, 0.03) * 0.5
    return lowpass(out, 1500)


def amb_lab_alarm():
    """경보 이후: 환기 + 멀리서 울리는 낮은 사이렌 (대사를 덮지 않게 작게)."""
    sec = 10.0
    tt = t(sec)
    base = amb_lab()[:len(tt)] if len(amb_lab()) >= len(tt) else pad(amb_lab(), len(tt))
    siren = np.sin(2 * np.pi * (440 + 120 * np.sin(2 * np.pi * 0.5 * tt)) * tt) * 0.08
    siren = lowpass(siren, 900) * (0.6 + 0.4 * np.sin(2 * np.pi * 0.1 * tt))
    return loopify(base * 0.8 + siren)

ITEMS = {
    'amb_fire': amb_fire, 'amb_house': amb_house, 'amb_digital': amb_digital, 'amb_lab': amb_lab, 'amb_lab_dark': amb_lab_dark,
    'sfx_step': sfx_step, 'sfx_step_drag': sfx_step_drag, 'sfx_debris': sfx_debris, 'sfx_fire_burst': sfx_fire_burst,
    'sfx_pc_on': sfx_pc_on, 'sfx_pc_beep': sfx_pc_beep, 'sfx_connect': sfx_connect, 'sfx_absorb': sfx_absorb,
    'sfx_land': sfx_land, 'sfx_window_pop': sfx_window_pop, 'sfx_alarm': sfx_alarm, 'sfx_transfer': sfx_transfer,
    'sfx_fall_whoosh': sfx_fall_whoosh, 'sfx_glass_knock': sfx_glass_knock, 'sfx_door_slide': sfx_door_slide,
    'sfx_door_bang': sfx_door_bang, 'sfx_baton_on': sfx_baton_on, 'sfx_charge_up': sfx_charge_up,
    'sfx_discharge_big': sfx_discharge_big, 'sfx_glass_break': sfx_glass_break, 'sfx_power_down': sfx_power_down,
    'sfx_spark_small': sfx_spark_small, 'sfx_metal_creak': sfx_metal_creak, 'sfx_squeeze': sfx_squeeze,
    'sfx_ui_move': sfx_ui_move, 'sfx_ui_ok': sfx_ui_ok, 'sfx_ui_cancel': sfx_ui_cancel, 'sfx_ui_no': sfx_ui_no,
    'sfx_text': sfx_text, 'sfx_advance': sfx_advance,
    'sfx_hit_phys': sfx_hit_phys, 'sfx_hit_elec': sfx_hit_elec, 'sfx_flick': sfx_flick, 'sfx_zap_shot': sfx_zap_shot,
    'sfx_whip': sfx_whip, 'sfx_kick_dash': sfx_kick_dash, 'sfx_enemy_attack': sfx_enemy_attack, 'sfx_baton_swing': sfx_baton_swing,
    'sfx_shot_enemy': sfx_shot_enemy, 'sfx_enemy_die': sfx_enemy_die, 'sfx_guard_break': sfx_guard_break,
    'sfx_heal': sfx_heal, 'sfx_levelup': sfx_levelup, 'sfx_battle_start': sfx_battle_start, 'sfx_wire_arc': sfx_wire_arc,
    'sfx_acid': sfx_acid, 'sfx_light_off': sfx_light_off, 'sfx_heartbeat': sfx_heartbeat,
    'bgm_battle': bgm_battle, 'bgm_tension': bgm_tension,
    'sfx_zap': sfx_zap, 'sfx_discharge_small': sfx_discharge_small, 'sfx_dodge': sfx_dodge, 'sfx_target_on': sfx_target_on,
    'sfx_suspect': sfx_suspect, 'sfx_alert': sfx_alert, 'sfx_hurt': sfx_hurt, 'sfx_robot_hit': sfx_robot_hit,
    'sfx_baton_hit': sfx_baton_hit, 'sfx_dash': sfx_dash, 'sfx_clatter': sfx_clatter, 'sfx_broadcast': sfx_broadcast,
    'sfx_light_on': sfx_light_on, 'sfx_bin_twitch': sfx_bin_twitch, 'sfx_bubble': sfx_bubble, 'amb_lab_alarm': amb_lab_alarm,
}

if __name__ == '__main__':
    import sys
    only = set(sys.argv[1:])
    for k, f in ITEMS.items():
        if only and k not in only:
            continue
        peak = 0.6 if k.startswith('amb_') else 0.7 if k.startswith('bgm_') else 0.85
        write(k, f(), peak)
