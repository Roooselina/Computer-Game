from pathlib import Path
from PIL import Image,ImageDraw
r=Path('Docs/Media/art_play_overhaul/run_051358');out=r.parent/'sequences';out.mkdir(exist_ok=True)
rows=[l.split('\t') for l in (r/'frames.tsv').read_text(encoding='utf-8').splitlines()[1:]]
for cut in ['op_village','op_house','op_transfer','op_tank','c3_boss_intro']:
 a=[int(x[0]) for x in rows if len(x)>3 and x[3]==cut]
 picked=[a[round(i*(len(a)-1)/7)] for i in range(8)]
 sheet=Image.new('RGB',(1280,420),(15,19,26));d=ImageDraw.Draw(sheet)
 for j,n in enumerate(picked):
  im=Image.open(r/f'f_{n:04}.jpg').resize((320,180));x=j%4*320;y=j//4*210;sheet.paste(im,(x,y));d.text((x+8,y+184),f'{cut} / frame {n}',fill='white')
  # duplicate lower row content dimensions laid out with breathing room
 sheet.save(out/(cut+'_contact.jpg'),quality=92)
 frames=[]
 for n in a:
  with Image.open(r/f'f_{n:04}.jpg') as im: frames.append(im.resize((768,432)).copy())
 try:frames[0].save(out/(cut+'.webp'),save_all=True,append_images=frames[1:],duration=360,loop=0,quality=74,method=3)
 except Exception as e:print('animation',cut,str(e))
print('sequence contacts and animated WebP exported')
