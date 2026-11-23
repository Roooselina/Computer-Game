from pathlib import Path
import re,json,difflib
art=Path('Assets/Game/Resources/EW/Illustrated');texts='\n'.join(p.read_text(encoding='utf-8-sig') for p in Path('Assets/Game/Scenes').glob('*.unity'))+'\n'+Path('Assets/Game/Resources/EW/GameConfig.asset').read_text(encoding='utf-8-sig')
linked=[];unlinked=[]
for p in sorted(art.glob('*.png')):
 m=re.search(r'guid: ([a-f0-9]+)',p.with_suffix('.png.meta').read_text());(linked if m and m.group(1) in texts else unlinked).append(p.name)
changed=[]
base=Path('Backups/2026-11-15_art_play_overhaul/Scripts');current=Path('Assets/Game/Scripts')
for p in current.rglob('*.cs'):
 old=base/p.relative_to(current)
 if not old.exists() or old.read_text(encoding='utf-8-sig').splitlines()!=p.read_text(encoding='utf-8-sig').splitlines():changed.append(str(p))
report={'illustrations_generated':len(linked)+len(unlinked),'illustrations_serialized_in_scenes_or_config':len(linked),'not_referenced':unlinked,'modified_or_new_code':changed}
Path('Docs/Media/art_play_overhaul/asset_code_audit.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8');print(report)
