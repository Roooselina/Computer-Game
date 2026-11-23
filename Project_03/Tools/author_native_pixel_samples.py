"""Native-resolution pixel authoring. Never loads/resizes generated art.
Generated concept is archived separately; every final pixel below is authored on its target grid.
"""
from pathlib import Path
from PIL import Image,ImageDraw
import json, shutil, hashlib
root=Path('Assets/Game/Art/PixelSamples')
root.mkdir(parents=True,exist_ok=True)
raw=Path('Docs/Media/pixel_restore/generated_design_reference');raw.mkdir(parents=True,exist_ok=True)
shutil.copy2(r'C:\Users\kuroc\.codex\generated_images\01a1db9d-fb2e-7e81-8d78-9de7fd80845d\exec-8f12e4e4-cf8d-443a-88d8-e5815afa7e5e.png',raw/'not_a_tileset.png')
P=['#101620','#1b2230','#2b3446','#3d485b','#546276','#75869a','#a3b4c4','#d8e5e4',
'#283a39','#365047','#416c48','#57984b','#7dbb51','#b2d667','#e5efab',
'#53382b','#875530','#bc813b','#e2ad57','#f1d589','#204831','#286b37','#369444','#54bd56',
'#81d97d','#bbefa5','#162e2c','#214a3c','#3e7055','#5c9370','#7ba78e','#94b9a0']
rgba=[tuple(bytes.fromhex(c[1:]))+(255,) for c in P]
def canvas(w,h):return Image.new('RGBA',(w,h))
def save(im,n):im.save(root/(n+'.png'))
def rect(d,box,k):d.rectangle(box,fill=rgba[k])
def line(d,pts,k,w=1):d.line(pts,fill=rgba[k],width=w)
def poly(d,pts,k):d.polygon(pts,fill=rgba[k])
def rivet(d,x,y):
 rect(d,(x,y,x+2,y+2),0);rect(d,(x,y,x+1,y+1),5);rect(d,(x,y,x,y),7)
# 32x16 platform; edges are defined by the same one-pixel row profile.
def platform(kind):
 im=canvas(32,16);d=ImageDraw.Draw(im)
 profile=[7,5,3,2,0,17,18,15,0,4,2,0,2,1,0,0]
 for y,k in enumerate(profile):rect(d,(0,y,31,y),k)
 for x in [3,19]:
  rect(d,(x,1,x+9,2),4);line(d,[(x,1),(x+8,1)],6)
  for z in range(5):
   xx=x+z
   line(d,[(xx+2,5),(xx,7)],16)
 rect(d,(5,9,26,13),0);rect(d,(7,10,24,12),3);line(d,[(9,11),(22,11)],5)
 poly(d,[(9,13),(11,11),(20,11),(22,13)],1)
 for x in [2,28]:rivet(d,x,10)
 if kind!='middle':
  left=kind=='left';a=0 if left else 28
  rect(d,(a,0,a+3,15),0);rect(d,(a+1,2,a+2,12),3)
  line(d,[(a+1,3),(a+1,11)],6)
  # stepped outside contour, retain the inner edge exactly.
  outside=0 if left else 31
  for y in [0,1,14,15]:im.putpixel((outside,y),(0,0,0,0))
 return im
for kind in ['left','middle','right']:save(platform(kind),'platform_'+kind)
# Native 16px pipes; ports 8px wide. Top-left reflected metal band is common across all ports.
H=canvas(16,16);d=ImageDraw.Draw(H)
for y,k in enumerate([0,3,6,5,4,3,2,0],4):rect(d,(0,y,15,y),k)
rect(d,(5,3,10,12),0);rect(d,(6,3,9,12),3);line(d,[(6,3),(9,3)],6);line(d,[(6,5),(9,5)],7);line(d,[(6,11),(9,11)],1)
rivet(d,6,6)
save(H,'pipe_horizontal');save(H.transpose(Image.Transpose.TRANSPOSE),'pipe_vertical')
E=canvas(16,16)
for y in range(16):
 for x in range(16):
  rr=(x*x+(15-y)*(15-y))**.5
  if 4<=rr<12:
   band=min(7,max(0,int(12-rr)))
   E.putpixel((x,y),rgba[[0,6,5,4,3,2,1,0][band]])
# Explicit canonical joints override curved pixels at the two ports.
for x in range(3):
 for y,k in enumerate([0,3,6,5,4,3,2,0],4):E.putpixel((x,y),rgba[k])
for y in range(13,16):
 for x,k in enumerate([0,3,6,5,4,3,2,0],4):E.putpixel((x,y),rgba[k])
save(E,'pipe_elbow')
T=H.copy();D=H.transpose(Image.Transpose.TRANSPOSE)
for y in range(8,16):
 for x in range(4,12):T.putpixel((x,y),D.getpixel((x,y)))
d=ImageDraw.Draw(T);rect(d,(4,5,11,10),3);line(d,[(4,5),(10,5)],6);rect(d,(6,7,9,10),4)
save(T,'pipe_t');save(T.transpose(Image.Transpose.TRANSPOSE),'pipe_t_right')
C=H.copy();d=ImageDraw.Draw(C);rect(d,(10,2,12,13),0);rect(d,(10,3,11,12),5);line(d,[(10,4),(10,10)],7)
for x in range(13,16):
 for y in range(16):C.putpixel((x,y),(0,0,0,0))
save(C,'pipe_cap')
# 80x112 original specimen tank. Side view, no character/background baked in.
im=canvas(80,112);d=ImageDraw.Draw(im)
# Rear rigid mounting frame and stepped hose.
poly(d,[(8,22),(13,16),(66,16),(70,22),(70,93),(65,101),(13,101),(8,94)],0)
rect(d,(10,22,67,94),2);rect(d,(12,24,65,92),1)
# right pressure loop: hard elbow silhouettes with distinct bands
poly(d,[(63,29),(71,29),(76,34),(76,79),(71,84),(63,84),(63,78),(69,78),(70,76),(70,36),(68,35),(63,35)],0)
line(d,[(65,31),(71,31),(73,34),(73,78),(70,81),(64,81)],4,3)
line(d,[(65,30),(70,30),(72,33),(72,77)],6)
for y in [39,70]:rect(d,(69,y,77,y+4),0);rect(d,(70,y+1,76,y+3),3);line(d,[(70,y+1),(75,y+1)],6)
# Chamber outer steel stepped cylinder, bevel reflecting upper-left.
poly(d,[(14,23),(20,19),(53,19),(60,23),(64,32),(64,83),(58,92),(20,92),(12,85),(10,77),(10,34)],0)
poly(d,[(16,26),(21,22),(53,22),(59,26),(61,33),(61,82),(56,88),(21,88),(15,82),(13,76),(13,35)],3)
line(d,[(16,32),(16,79),(20,85),(53,85),(58,80)],5,2)
# Glass: bounded flat bands with inset rather than smooth gradient.
poly(d,[(22,28),(51,28),(57,32),(59,39),(59,77),(55,83),(50,85),(23,85),(18,81),(16,76),(16,39),(18,32)],26)
poly(d,[(23,31),(50,31),(55,34),(57,40),(57,75),(53,81),(48,83),(24,83),(20,79),(18,74),(18,41),(20,34)],27)
rect(d,(22,35,51,79),8)
# Liquid in a stepped vessel; coloured shadow bands stay intentional pixels.
poly(d,[(19,48),(24,46),(51,46),(57,48),(57,74),(53,81),(48,83),(24,83),(20,78),(19,74)],21)
poly(d,[(24,49),(51,49),(53,52),(53,77),(48,81),(26,81),(23,76)],22)
rect(d,(27,51,45,78),23);rect(d,(30,53,42,77),11)
poly(d,[(20,48),(25,46),(50,46),(56,48),(52,50),(25,50)],12)
line(d,[(22,48),(28,47),(47,47),(53,48)],25)
# Faceted glass reflection and curved dark edge strips.
rect(d,(20,36,21,75),28);rect(d,(22,34,23,77),31)
rect(d,(24,36,25,73),7);rect(d,(26,39,26,68),25)
line(d,[(27,72),(27,76),(29,78)],24)
rect(d,(52,36,53,75),28);rect(d,(54,41,54,70),30)
line(d,[(29,32),(46,32),(50,34)],5)
# Pixel bubbles: stepped rings, crescent highlight, darker liquid core.
for x,y,r in [(37,68,4),(45,57,3),(32,55,2),(40,42,2),(49,73,2),(31,75,1),(45,65,1),(35,36,1)]:
 if r>=2:
  poly(d,[(x-r+1,y-r),(x+r-1,y-r),(x+r,y-r+1),(x+r,y+r-1),(x+r-1,y+r),(x-r+1,y+r),(x-r,y+r-1),(x-r,y-r+1)],12)
  rect(d,(x-r+1,y-r+1,x+r-1,y+r-1),22)
  line(d,[(x-r+1,y),(x-r+1,y-r+1),(x,y-r+1)],25)
  im.putpixel((x+1,y+r-1),rgba[11])
 else:rect(d,(x,y,x+1,y+1),24);im.putpixel((x,y),rgba[25])
# Steel head cap and heavy base, polygonal bevel edges.
for yy,hh in [(15,17),(83,16)]:
 poly(d,[(11,yy),(18,yy-3),(53,yy-3),(63,yy),(66,yy+5),(63,yy+hh-2),(56,yy+hh),(17,yy+hh),(8,yy+hh-4),(8,yy+5)],0)
 poly(d,[(12,yy+1),(19,yy-1),(53,yy-1),(61,yy+2),(63,yy+6),(60,yy+hh-4),(53,yy+hh-2),(18,yy+hh-2),(11,yy+hh-5),(10,yy+6)],3)
 line(d,[(13,yy+2),(21,yy),(52,yy),(59,yy+2)],6,2)
 line(d,[(12,yy+6),(61,yy+6)],5)
 line(d,[(13,yy+hh-4),(59,yy+hh-4)],1,2)
 for x in [14,54]:rivet(d,x,yy+8)
 # discrete panel ribs
 for x in [22,46]:rect(d,(x,yy+7,x+2,yy+hh-4),1);line(d,[(x,yy+8),(x,yy+hh-5)],5)
# Head pressure canister and feed connector
rect(d,(30,1,45,12),0);rect(d,(32,3,43,10),3);line(d,[(33,3),(42,3)],6)
rect(d,(34,5,41,11),8);rect(d,(36,5,39,11),12);line(d,[(36,5),(36,10)],25)
poly(d,[(28,0),(47,0),(49,2),(49,5),(27,5),(27,2)],0);rect(d,(29,1,47,3),4);line(d,[(31,1),(44,1)],7)
rect(d,(26,11,49,15),0);rect(d,(28,12,47,14),4);line(d,[(30,12),(45,12)],6)
# Cap's inset monitoring unit / LEDs / seams
rect(d,(29,22,45,29),0);rect(d,(30,23,44,28),5);rect(d,(31,24,43,27),26)
for x in [32,36,40]:rect(d,(x,25,x+1,26),24)
# Left control box with relief, LED and three distinct tactile buttons
poly(d,[(1,43),(7,41),(13,44),(13,68),(1,68)],0);rect(d,(2,44,11,66),2);line(d,[(3,44),(10,44)],6)
rect(d,(4,47,9,54),0);rect(d,(5,48,8,52),12);line(d,[(5,48),(7,48)],25)
for y in [57,61]:rect(d,(4,y,9,y+2),0);line(d,[(5,y),(8,y)],5)
rivet(d,3,64)
# Side regulator with a small glass inset and central screw hub.
rect(d,(67,50,78,69),0);rect(d,(68,51,77,68),3);line(d,[(68,51),(76,51)],6)
rect(d,(70,53,75,59),26);rect(d,(71,54,74,58),11);line(d,[(71,54),(71,57)],24)
poly(d,[(70,62),(75,62),(77,64),(75,67),(70,67),(68,65)],1);line(d,[(71,63),(74,66)],6);line(d,[(74,63),(71,66)],4)
# Vented plinth, reinforced feet, no ground shadow baked outside silhouette.
poly(d,[(16,98),(58,98),(64,104),(64,111),(9,111),(9,104)],0);rect(d,(14,99,59,108),2)
line(d,[(18,99),(54,99)],5);rect(d,(25,99,48,107),0)
for x in range(27,48,4):rect(d,(x,101,x+1,106),4);im.putpixel((x,101),rgba[6])
for x in [10,51]:rect(d,(x,104,x+10,110),1);line(d,[(x+1,104),(x+9,104)],5);rivet(d,x+4,106)
rect(d,(30,89,44,95),0);rect(d,(31,90,43,94),26)
for x in [32,36,40]:rect(d,(x,91,x+1,93),24);im.putpixel((x,91),rgba[25])
save(im,'specimen_tank')
# Preview uses integer nearest-neighbour enlargement ONLY of already native authored art.
preview=Image.new('RGBA',(320,176),(19,25,35,255))
for x,n in [(8,'platform_left'),(40,'platform_middle'),(72,'platform_middle'),(104,'platform_right')]:preview.alpha_composite(Image.open(root/(n+'.png')),(x,124))
for x,n in [(8,'pipe_horizontal'),(24,'pipe_t'),(40,'pipe_horizontal'),(56,'pipe_elbow')]:preview.alpha_composite(Image.open(root/(n+'.png')),(x,48))
for y in [64,80]:preview.alpha_composite(Image.open(root/'pipe_vertical.png'),(56,y))
preview.alpha_composite(im,(176,28))
preview.alpha_composite(Image.open('Assets/Game/Art/Placeholder/Lu_Idle.png').crop((0,0,48,64)),(108,60))
preview.resize((1280,704),Image.Resampling.NEAREST).save('Docs/Media/pixel_restore/native_samples_preview.png')
# Exact seam, palette, alpha, dimensions checks.
files={p.stem:Image.open(p) for p in root.glob('*.png')}
assert files['platform_left'].crop((31,0,32,16)).tobytes()==files['platform_middle'].crop((0,0,1,16)).tobytes()
assert files['platform_middle'].crop((31,0,32,16)).tobytes()==files['platform_right'].crop((0,0,1,16)).tobytes()
for n in ['pipe_elbow','pipe_t']:
 assert files[n].crop((0,0,1,16)).tobytes()==H.crop((0,0,1,16)).tobytes(),n+' left port'
 assert files[n].crop((0,15,16,16)).tobytes()==D.crop((0,15,16,16)).tobytes(),n+' bottom port'
assert files['pipe_horizontal'].crop((0,0,1,16)).tobytes()==files['pipe_horizontal'].crop((15,0,16,16)).tobytes()
for y in [0,15]:
 assert files['pipe_t_right'].crop((0,y,16,y+1)).tobytes()==D.crop((0,y,16,y+1)).tobytes()
assert files['pipe_t_right'].crop((15,0,16,16)).tobytes()==H.crop((15,0,16,16)).tobytes()
assert files['pipe_cap'].crop((0,0,1,16)).tobytes()==H.crop((0,0,1,16)).tobytes()
colors=set()
for v in files.values():colors.update(c for c in v.getdata() if c[3])
assert len(colors)<=32
assert all(c[3] in [0,255] for v in files.values() for c in v.getdata())
audit={'construction':'native pixel coordinates, not downsampled/mosaiced generation','palette_colors':len(colors),'alpha':'binary 0/255','seams':'platform L/M/R, repeatable pipe, elbow/T left-bottom, right T top-bottom-right, cap left: byte-exact','ppu':32,'files':{n:{'size':v.size,'sha256':hashlib.sha256((root/(n+'.png')).read_bytes()).hexdigest()} for n,v in files.items()}}
Path('Docs/Media/pixel_restore/sample_pixel_audit.json').write_text(json.dumps(audit,indent=2),encoding='utf-8')
print(json.dumps(audit,indent=2))

