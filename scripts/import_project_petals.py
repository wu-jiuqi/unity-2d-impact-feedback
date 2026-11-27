"""Import only the 0.5.1 petals; do not require removed legacy paint assets."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser()
parser.add_argument('--source', type=Path, required=True)
args = parser.parse_args()
game = args.source / 'BounceGarden/BounceGarden/Assets'
art = game / 'Resources/Art/BattleGarden050/break-petals-atlas.png'
manifest = json.loads((args.source / 'ArtSource/BreakPetals-v1/manifest.json').read_text(encoding='utf-8-sig'))
digest = hashlib.sha256(art.read_bytes()).hexdigest()
if digest != manifest['sha256']:
    raise SystemExit('Petal atlas differs from its source manifest')
destination = 'assets/unity-sample/Assets/Resources/ImpactFeedback/break-petals-atlas.png'
out = root / destination
out.parent.mkdir(parents=True, exist_ok=True)
shutil.copyfile(art, out)
meta = Path(str(art) + '.meta').read_text(encoding='utf-8-sig')
Path(str(out) + '.meta').write_text('\n'.join(line.rstrip() for line in meta.splitlines()) + '\n', encoding='utf-8')
manifest['sourceGameVersion'] = '0.5.1'
manifest['sourceRuntimeSha256'] = hashlib.sha256((game / manifest['runtime'].removeprefix('Assets/')).read_bytes()).hexdigest()
manifest['adaptation'] = [
    'Reuse the original full-colour atlas, flutter, arc, randomized landing and fade.',
    'Plain world-space gallery has no BoardDisplay projection; host supplies optional PetalLandingBounds.',
    'Gallery ground order defaults to 20 (behind bricks); source game uses -22.',
    'Shared 128-slot pool and presentation delta; fragment controls adjust count, speed, size and ground hold.',
    'New profiles default to petals; JSON without petalFragments retains legacy paint.'
]
(root / 'references/petal-source.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
inventory_path = root / 'references/asset-manifest.json'
inventory = json.loads(inventory_path.read_text(encoding='utf-8-sig'))
inventory = [item for item in inventory if item['destination'] != destination]
inventory.append(dict(source='Resources/Art/BattleGarden050/break-petals-atlas.png', destination=destination, sha256=digest))
inventory_path.write_text(json.dumps(inventory, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print('Imported 0.5.1 petals: ' + digest)
