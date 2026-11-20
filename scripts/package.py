"""Package the validated skill, source sample, Windows runtime and provenance."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
import json,hashlib,re

ROOT=Path(__file__).resolve().parents[1]
VERSION='0.1.0'
EXCLUDED={'.git','Library','Temp','Obj','Logs','UserSettings','.vs','__pycache__','Builds','dist','evidence'}
def source_files():
 for p in ROOT.rglob('*'):
  if p.is_file() and not any(x in EXCLUDED for x in p.relative_to(ROOT).parts):yield p
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
 build=ROOT/'Builds/Windows';summary=json.loads((build/'build-summary.json').read_text(encoding='utf-8'))
 if summary['result']!='Succeeded' or summary['errors']:raise SystemExit('A successful build is required')
 assembly=build/'ImpactFeedbackGallery_Data/Managed/Assembly-CSharp.dll'
 reports={}
 for fps in (30,60):
  p=ROOT/f'evidence/qa{fps}/result.json';r=json.loads(p.read_text(encoding='utf-8'))
  if r['failed'] or not r['passed']:raise SystemExit(f'QA {fps} did not pass')
  if r.get('assemblySha256')!=sha(assembly):raise SystemExit(f'QA {fps} belongs to a different build')
  if re.search(r'Exception:|\bError\b',(p.parent/'player.log').read_text(encoding='utf-8')):raise SystemExit(f'Runtime errors in QA {fps} log')
  reports[str(fps)]=r
 out=ROOT/'dist';out.mkdir(exist_ok=True)
 delivery=dict(version=VERSION,build=summary,qa={k:dict(passed=v['passed'],failed=v['failed']) for k,v in reports.items()},assets_manifest_sha256=sha(ROOT/'references/asset-manifest.json'),executable_sha256=sha(build/'ImpactFeedbackGallery.exe'))
 manifest=json.dumps(delivery,ensure_ascii=False,indent=2)
 windows=out/f'ImpactFeedbackGallery-Windows-x64-{VERSION}.zip'
 skill=out/f'unity-2d-impact-feedback-{VERSION}.zip'
 for target,include_source in ((windows,False),(skill,True)):
  with ZipFile(target,'w',ZIP_DEFLATED,compresslevel=6) as z:
   prefix='unity-2d-impact-feedback/' if include_source else ''
   if include_source:
    for p in source_files():z.write(p,prefix+p.relative_to(ROOT).as_posix())
   for p in build.rglob('*'):
    if p.is_file() and not any(x.endswith('_BackUpThisFolder_ButDontShipItWithYourGame') for x in p.parts):z.write(p,prefix+'Windows/'+p.relative_to(build).as_posix())
   z.write(ROOT/'references/gallery.md',prefix+'Windows/操作与构建说明.md')
   z.writestr(prefix+'verification/delivery.json',manifest)
   for fps,r in reports.items():z.writestr(prefix+f'verification/qa{fps}.json',json.dumps(r,ensure_ascii=False,indent=2))
   for p in sorted((ROOT/'evidence/qa60').glob('*.png')):z.write(p,prefix+'verification/screenshots/'+p.name)
 delivery['archives']=[dict(name=p.name,bytes=p.stat().st_size,sha256=sha(p)) for p in (skill,windows)]
 (out/'delivery.json').write_text(json.dumps(delivery,ensure_ascii=False,indent=2),encoding='utf-8')
 print(json.dumps(delivery,ensure_ascii=False,indent=2))
if __name__=='__main__':main()
