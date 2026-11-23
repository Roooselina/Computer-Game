from pathlib import Path
from PIL import Image
import json,re,hashlib,shutil
r=Path('.'); out=Path('Docs/Media/pixel_restore');snap=Path('Backups/2026-11-15_pixel_restore_doubletap')
changes=[]
for source,dest in [(snap/'Scripts',Path('Assets/Game/Scripts')),(snap/'Scenes',Path('Assets/Game/Scenes'))]:
 for p in dest.rglob('*'):
  if p.is_file() and p.suffix in ['.cs','.unity','.meta']:
   old=source/p.relative_to(dest)
   if not old.exists():status='added'
   elif old.read_bytes()!=p.read_bytes():status='modified'
   else:continue
   changes.append({'file':str(p).replace('\\','/'),'status':status})
metachanges=[]
for p in (snap/'Scenes').rglob('*.meta'):
 now=Path('Assets/Game/Scenes')/p.relative_to(snap/'Scenes')
 if not now.exists() or now.read_bytes()!=p.read_bytes():metachanges.append(str(now))
pg=re.search(r'guid: (\w+)',Path('Assets/Game/Scripts/Runtime/Visual/Parallax.cs.meta').read_text()).group(1)
sceneinfo={p.name:{'parallax_components':p.read_text(encoding='utf-8-sig').count(pg)} for p in Path('Assets/Game/Scenes').glob('*.unity')}
(out/'change_manifest.json').write_text(json.dumps({'comparison':'pre-task snapshot; no Git repository','code_and_scene_changes':changes,'scene_meta_changes':metachanges,'production_scene_parallax':sceneinfo},indent=2),encoding='utf-8')
# Actual captured frames: video/contact sheet are evidence, never source art.
frames=sorted((out/'dash_frames').glob('f_*.png'))
ims=[Image.open(f).convert('RGB').resize((1280,720),Image.Resampling.NEAREST) for f in frames]
ims[0].save(out/'dash_playback.gif',save_all=True,append_images=ims[1:],duration=120,loop=0,optimize=False)
shutil.copy2(frames[2],out/'sample_connection_game.png')
# Full camera frames at original capture pixels, with no art manipulation.
selected=[2,7,10,16,20,28]
contact=Image.new('RGB',(1280*3,720*2))
for i,k in enumerate(selected):contact.paste(ims[k],((i%3)*1280,(i//3)*720))
contact.save(out/'dash_contact_sheet.png')
print('Scene .meta changed:',metachanges)
print('Parallax counts:',sceneinfo)
print('Code/scene manifest entries',len(changes),'actual recording frames',len(frames))
