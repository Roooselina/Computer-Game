from pathlib import Path
from PIL import Image
import hashlib,json
files=['Assets/Game/Art/Placeholder/Scene/L_Floor.png','Assets/Game/Art/Placeholder/Scene/L_TankBack.png','Assets/Game/Art/Placeholder/Scene/L_TankGlass.png','Assets/Game/Art/Placeholder/Lu_Idle.png','Assets/Game/Art/Placeholder/Tile_Platform.png']
for f in files:
 p=Path(f)
 if p.exists():
  im=Image.open(p);print(f,im.size,im.mode,len(im.getcolors(1000000) or []))
for name in ['EWSceneBuilder.cs','EWMenu.cs','EWRooms.cs']:
 import difflib
 a=Path('Backups/2026-11-15_art_play_overhaul/Scripts/Editor')/name;b=Path('Assets/Game/Scripts/Editor')/name
 if name!='EWRooms.cs':print(''.join(difflib.unified_diff(a.read_text(encoding='utf-8-sig').splitlines(True),b.read_text(encoding='utf-8-sig').splitlines(True),fromfile='before/'+name,tofile='current/'+name)))
print('Tile assets',len(list(Path('Assets').rglob('*.prefab'))))
