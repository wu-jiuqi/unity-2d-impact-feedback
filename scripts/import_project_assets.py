"""Maintainer-only import; the resulting sample works without the source project."""
from pathlib import Path
import argparse, shutil, hashlib, json
p=argparse.ArgumentParser();p.add_argument('--source',type=Path,required=True);a=p.parse_args()
root=Path(__file__).resolve().parents[1];src=a.source/'BounceGarden/BounceGarden/Assets'
out=root/'assets/unity-sample/Assets';records=[]
def copy(relative,dest):
 s=src/relative;d=out/dest;d.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(s,d)
 records.append(dict(source=relative,destination=str(d.relative_to(root)).replace('\\','/'),sha256=hashlib.sha256(d.read_bytes()).hexdigest()))
for name in ['atlas.json','atlas-01.png','atlas-02.png']:
 copy('Resources/Art/SettingsC/'+name,'Resources/Art/SettingsC/'+name)
for name in ['ball_yarn_blue.png','thread_line_blue.png']:
 copy('Resources/Art/YarnV19/'+name,'Resources/Gallery/'+name)
for color in ['pink','yellow','mint','blue','lilac']:
 copy('Resources/Art/FiveBakedV80/ordinary-'+color+'.png','Resources/Gallery/Bricks/ordinary-'+color+'.png')
copy('Resources/Art/FiveBakedV80/footprints.json','Resources/Gallery/Bricks/footprints.json')
copy('Resources/Art/CombatFeedback79/jelly_particles.png','Resources/Gallery/particles.png')
for name in ['MaokenZhuyuanTi.ttf','MaokenZhuyuanTi-OFL.txt','MaokenZhuyuanTi-OFL_ZHS.txt','MaokenZhuyuanTi-字体授权说明.txt']:
 copy('Resources/Fonts/'+name,'Resources/Fonts/'+name)
copy('Resources/Audio/Jelly/AI/sfx/hit.wav','Resources/Gallery/hit.wav')
copy('Resources/Audio/Jelly/AI/SOURCE.md','Resources/Gallery/AUDIO-SOURCE.md')
copy('Plugins/Demigiant/DOTween/DOTween.dll','Plugins/DOTween/DOTween.dll')
copy('Plugins/Demigiant/DOTween/readme.txt','Plugins/DOTween/readme.txt')
# Preserve established elastic UI behavior, removing only host game dependencies.
s=(src/'Scripts/UI/UIBounce.cs').read_text(encoding='utf-8-sig')
s=s.replace('namespace BounceGarden','namespace ImpactFeedback.Gallery').replace('Button button; AppUI app;','Button button;')
s=s.replace('bool MotionAllowed => FeedbackProfile.Active.enabledFeedback;','bool MotionAllowed => true;')
s=s.replace('app=GetComponentInParent<AppUI>();','').replace('||(app!=null&&!app.HomeMotionAllowed)','')
d=out/'ImpactFeedback/Gallery/UIBounce.cs';d.write_text(s,encoding='utf-8')
records.append(dict(source='Scripts/UI/UIBounce.cs',destination=str(d.relative_to(root)).replace('\\','/'),adaptation='Namespace and host-game gates only; tween curves, timing and mesh deformation retained.'))
(root/'references/asset-manifest.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
print('Imported',len(records),'assets; source manifest written')
