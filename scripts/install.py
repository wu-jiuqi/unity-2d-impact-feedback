"""Install this skill without copying Unity caches, VCS or Windows build outputs."""
from pathlib import Path
import os,argparse,shutil
ROOT=Path(__file__).resolve().parents[1]
SKIP={'.git','Library','Temp','Obj','Logs','UserSettings','.vs','__pycache__','Builds','dist','evidence'}
def main():
 p=argparse.ArgumentParser();p.add_argument('--replace',action='store_true');p.add_argument('--with-windows',action='store_true');p.add_argument('--destination',type=Path);a=p.parse_args()
 target=a.destination or Path(os.environ.get('CODEX_HOME',str(Path.home()/'.codex')))/'skills/unity-2d-impact-feedback'
 if target.resolve()==ROOT.resolve():raise SystemExit('Source and destination must differ')
 if target.exists() and not a.replace:raise SystemExit(f'Skill already exists: {target}. Use --replace to update.')
 count=0
 for s in ROOT.rglob('*'):
  relative=s.relative_to(ROOT)
  if not s.is_file() or any(x in SKIP for x in relative.parts):continue
  d=target/relative;d.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(s,d);count+=1
 if a.with_windows:
  runtime=ROOT/'Builds/Windows'
  if not (runtime/'ImpactFeedbackGallery.exe').exists():raise SystemExit('Build the Windows gallery first')
  shutil.copytree(runtime,target/'Windows',dirs_exist_ok=True,ignore=shutil.ignore_patterns('*_BackUpThisFolder_ButDontShipItWithYourGame'))
 print(f'Installed {count} skill files'+(' and Windows gallery' if a.with_windows else '')+f' to {target}')
if __name__=='__main__':main()
