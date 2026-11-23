from pathlib import Path
from PIL import Image,ImageDraw
base=Path('Docs/Media/art_play_overhaul');out=base/'sequences'
for name in ['venelle_final','capture_final','tank_final','opening_final']:
 files=sorted((base/name).glob('f_*.png'));sheet=Image.new('RGB',(1280,420),(15,18,24));d=ImageDraw.Draw(sheet)
 frames=[]
 for j in range(8):
  f=files[round(j*(len(files)-1)/7)];im=Image.open(f).resize((320,180));x=j%4*320;y=j//4*210;sheet.paste(im,(x,y));d.text((x+6,y+184),name+' / '+f.stem,fill='white')
 sheet.save(out/(name+'_contact.jpg'),quality=92)
 for f in files:
  with Image.open(f) as im:frames.append(im.convert('RGB').resize((960,540)))
 frames[0].save(out/(name+'.webp'),save_all=True,append_images=frames[1:],duration=500,loop=0,quality=78,method=3)
 print('exported',name,len(files))
# Same camera and player location, before and after.
a=Image.open(base/'before/lab.png').convert('RGB').resize((960,540));b=Image.open(base/'after/lab.png').convert('RGB').resize((960,540))
sheet=Image.new('RGB',(1920,575),(15,18,24));sheet.paste(a,(0,35));sheet.paste(b,(960,35));d=ImageDraw.Draw(sheet);d.text((20,10),'BEFORE',fill='white');d.text((980,10),'AFTER / SAME CAMERA',fill='white');sheet.save(base/'comparison.jpg',quality=94)
