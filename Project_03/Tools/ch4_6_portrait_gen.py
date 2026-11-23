# -*- coding: utf-8 -*-
"""4~6장 새 인물 임시 스탠딩 (F, 008, 생존자). portrait_gen.py 의 그리기 함수를 그대로 쓰고 새 파일만 만든다."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import portrait_gen as pg
pg.CHARS.update({
    'f': ((192, 192, 198), (112, 86, 66), (232, 200, 176), 'F', 'bun'),
    'kid008': ((70, 60, 58), (150, 160, 120), (240, 210, 186), '008', 'short'),
    'survivor': ((60, 46, 40), (96, 104, 80), (226, 196, 172), '생존자', 'neat'),
})
pg.EXPR_KO.update({'stern': '무뚝뚝', 'soft': '누그러짐', 'curious': '궁금', 'worried': '걱정', 'tense': '긴장'})
NEW = {'f': ['neutral', 'determined', 'gentle'], 'kid008': ['neutral', 'surprised', 'smile', 'sad'], 'survivor': ['scared', 'neutral']}
n = 0
for cid, ex in NEW.items():
    for e in ex:
        path = os.path.join(pg.OUT, '%s_%s.png' % (cid, e))
        if os.path.exists(path) and '--force' not in sys.argv: continue
        pg.portrait(cid, e); n += 1
print('portraits', n)
