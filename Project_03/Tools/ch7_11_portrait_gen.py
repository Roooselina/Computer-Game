# -*- coding: utf-8 -*-
"""7~11장 새 인물 임시 스탠딩. portrait_gen.py 의 그리기 함수를 그대로 쓰고 새 파일만 만든다 (기존 파일은 건드리지 않음)."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import portrait_gen as pg
pg.CHARS.update({
    'raviel': ((226, 222, 236), (232, 232, 240), (236, 214, 206), '라비엘', 'short'),
    'meilin': ((30, 26, 34), (178, 40, 52), (244, 230, 226), '메이린', 'long'),
    'lacrimosa': ((40, 40, 54), (38, 48, 92), (226, 204, 190), '라크리모사', 'neat'),
    'aurelius': ((232, 214, 160), (60, 54, 88), (240, 230, 214), '아우렐리우스', 'long'),
    'edward': ((96, 72, 50), (70, 76, 90), (232, 204, 182), '에드워드', 'neat'),
    'member': ((70, 60, 52), (108, 110, 118), (230, 200, 176), '단원', 'neat'),
})
NEW = {'raviel': ['neutral', 'smirk', 'sad', 'surprised', 'smile', 'pained'],
       'meilin': ['smile', 'determined', 'surprised', 'scared', 'sad'],
       'lacrimosa': ['gentle', 'quiet', 'sad', 'pained'],
       'aurelius': ['gentle', 'determined', 'pained'],
       'edward': ['quiet'], 'member': ['neutral']}
n = 0
for cid, ex in NEW.items():
    for e in ex:
        path = os.path.join(pg.OUT, '%s_%s.png' % (cid, e))
        if os.path.exists(path): continue
        pg.portrait(cid, e); n += 1
print('portraits', n)
