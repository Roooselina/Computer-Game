from pathlib import Path
import hashlib,json
roots=[Path('Assets/Game/Art/Placeholder'),Path('Assets/ThirdParty')]
d={str(p):hashlib.sha256(p.read_bytes()).hexdigest() for root in roots for p in root.rglob('*') if p.is_file() and (p.suffix in ['.png','.meta','.mat','.prefab','.asset'])}
Path('Docs/Media/pixel_restore/original_asset_hashes.json').write_text(json.dumps(d,indent=2),encoding='utf-8')
print('original asset and GUID files hashed:',len(d))
