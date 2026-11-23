from pathlib import Path
import json,hashlib,re
before=json.loads(Path('Docs/Media/pixel_restore/original_asset_hashes.json').read_text())
changed=[f for f,h in before.items() if not Path(f).exists() or hashlib.sha256(Path(f).read_bytes()).hexdigest()!=h]
print('original files changed',changed)
new=Path('Assets/Game/Art/Archived/Illustrated_2026_11_15');guids=[re.search(r'guid: (\w+)',p.read_text()).group(1) for p in new.glob('*.png.meta')]
refs=[]
for p in list(Path('Assets/Game/Scenes').glob('*.unity'))+[Path('Assets/Game/Resources/EW/GameConfig.asset')]:
 t=p.read_text(encoding='utf-8-sig')
 for g in guids:
  if g in t:refs.append({'file':str(p),'guid':g})
print('illustrated references',refs)
Path('Docs/Media/pixel_restore/restoration_audit.json').write_text(json.dumps({'baseline':'Backups/2026-11-15_art_play_overhaul (pre art); originals retained in Assets/Game/Art/Placeholder and Assets/ThirdParty','original_files_checked':len(before),'originals_modified':changed,'archived_illustrations':len(guids),'active_illustration_references':refs},indent=2),encoding='utf-8')
