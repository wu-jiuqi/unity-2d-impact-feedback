"""Extract production launch code. No independently authored volley scheduler."""
from pathlib import Path
import argparse,json,hashlib,re
p=argparse.ArgumentParser();p.add_argument('--source',type=Path,required=True);a=p.parse_args()
root=Path(__file__).resolve().parents[1];src=a.source/'BounceGarden/BounceGarden/Assets/Scripts'
out=root/'assets/unity-sample/Assets/ImpactFeedback/ProjectLaunch';out.mkdir(parents=True,exist_ok=True)
game=(src/'Core/GameSession.cs').read_text(encoding='utf-8-sig');chain=(src/'Core/ChainLiveSession.cs').read_text(encoding='utf-8-sig');ui=(src/'UI/AppUI.cs').read_text(encoding='utf-8-sig');records=[]
def method(text,signature):
 start=text.index(signature);opening=text.index('{',start);depth=1;i=opening+1
 while depth:
  if text[i]=='{':depth+=1
  if text[i]=='}':depth-=1
  i+=1
 return text[start:i]
def keep(text,label,source):
 records.append(dict(symbol=label,source=source,sha256=hashlib.sha256(text.encode()).hexdigest(),body=text));return text
def gm(signature):return keep(method(game,signature),signature,'Core/GameSession.cs')
parts=[gm('bool TryShootCore(Vector2 direction)'),gm('public static Vector2 NormalizeAimDirection(Vector2 direction)'),gm('void ReturnReserved()')]
parts+=[keep(method(chain,s),s,'Core/ChainLiveSession.cs') for s in ['bool BufferChainShot(Vector2 direction)','void PromoteBufferedChainShot()']]
tick=gm('void FixedUpdate()');tick=tick[tick.index('PhysicsStep++;'):tick.rfind('}')]
tick=tick.replace('Time.fixedDeltaTime','fixedDeltaTime')
parts.append('public void FixedStep(float fixedDeltaTime){if(!IsPlaying||IsPaused||JuiceFrozen)return;'+tick+'}')
burst=gm('void BeginBurst(Vector2 direction)');start=burst.index('int count=Mathf.Max(1,CurrentLevel.volleyCount);');end=burst.index('var actualBooster=')
parts.append('void BeginBurst(Vector2 direction){'+burst[start:end]+'if(IsChainCollection)BeginChainVolley();OnBallSpawned(first);}')
lose=gm('public void LoseBall(BallBody ball,Vector2 position)');lose=lose.replace('if(IsChainCollection)ReturnChainBall(ball);','').replace('EmitFeedback(position,"splash",new Color(.58f,.94f,1));','')
parts.append(lose)
can=re.search(r'public bool ChainCanLaunch=>[^;]+;',chain).group();parts.append(keep(can,'ChainCanLaunch','Core/ChainLiveSession.cs'))
cooldown=re.search(r'chainNextLaunch=simulationClock\+\.55f;',chain).group();parts.append('void BeginChainVolley(){'+keep(cooldown,'live group cooldown','Core/ChainLiveSession.cs')+'}')
parts.append(keep('public Vector2 SpawnPosition(Vector2 direction)=>LauncherPosition+NormalizeAimDirection(direction)*MuzzleOffset;','SpawnPosition','Core/GameSession.cs'))
header='// Generated from BounceBusters production code. See references/project-launch-source.json.\nusing System;using System.Collections.Generic;using UnityEngine;\nnamespace ImpactFeedback.ProjectLaunch { public sealed partial class GameSession {\n'
(out/'GameSession.Launch.generated.cs').write_text(header+'\n'.join(parts)+'\n}}\n',encoding='utf-8')
for name in ['RuleState','Round4ReferenceData']:
 text=(src/f'Core/{name}.cs').read_text(encoding='utf-8-sig');keep(text,name,f'Core/{name}.cs')
 (out/(name+'.cs')).write_text(text.replace('namespace BounceGarden','namespace ImpactFeedback.ProjectLaunch'),encoding='utf-8')
# Original desktop OnGUI event path, movement threshold, and aim vector arithmetic.
begin=method(ui,'void BeginAim(Vector2 pos,int id)');update=method(ui,'void UpdateAim(Vector2 pos)');end=method(ui,'void EndAim(Vector2 pos)');cancel=method(ui,'void CancelAim()');gui=method(ui,'void OnGUI()')
for name,body in [('BeginAim',begin),('UpdateAim',update),('EndAim',end),('CancelAim',cancel),('OnGUI',gui)]:keep(body,name,'UI/AppUI.cs')
# Only host-specific decorations, tutorial and telemetry hooks are omitted.
begin=begin[:begin.index('if(Session.GardenDecor')]+begin[begin.index('aiming=true;'):]
begin=begin.replace('NotifyTutorialPointer(true);','').replace('Metrics?.Balance?.StrategyInput("begin",aimDirection);','')
update=update.replace('BoardDisplay.Inverse(CameraJuice.StableScreenToWorld(gameCamera,pos))','ScreenToProjectWorld(gameCamera,pos)').replace('Metrics?.Balance?.StrategyInput("adjust",aimDirection);','')
end=re.sub(r'if\(Array.IndexOf\(Environment.GetCommandLineArgs\(\),"--demo-record"\)>=0\)Debug.Log\([^;]+;','',end)
end=end.replace('Metrics?.Balance?.StrategyInput(valid?"release":"cancel",dir);','').replace('Metrics?.AcceptedRelease();','').replace('if(tutorialStep==1)ShowTutorial(2);','')
cancel=cancel.replace('if(aiming){Metrics?.Balance?.StrategyInput("cancel",aimDirection);NotifyTutorialPointer(false);}','')
gui=gui.replace('void OnGUI()','public void OnGUI()')
blocked=keep(method(ui,'bool UIBlocked(Vector2 pos)'),'UIBlocked','UI/AppUI.cs')
(out/'ProjectBattleInput.generated.cs').write_text('// Generated from AppUI.cs desktop input. No replacement drag/release algorithm.\nusing System;using UnityEngine;using UnityEngine.EventSystems;\nnamespace ImpactFeedback.ProjectLaunch {public sealed partial class ProjectBattleInput {\n'+'\n'.join([gui,begin,update,end,cancel,blocked])+'\n}}',encoding='utf-8')
catalog=(src/'Core/LevelCatalog.cs').read_text(encoding='utf-8-sig')
values={name:re.search(r'public (?:int|float) '+name+r' = ([^;]+);',catalog).group(1) for name in ['volleyCount','ballInterval','ballSpeed','gravity']}
multiplier=re.search(r'ballSpeed=source.ballSpeed\*([\d.]+f)',game).group(1)
(out/'LevelData.generated.cs').write_text('// Original LevelData defaults and GameSession speed fit.\nnamespace ImpactFeedback.ProjectLaunch {public sealed class LevelData {'+' '.join('public '+('int' if k=='volleyCount' else 'float')+' '+k+' = '+(v+'*'+multiplier if k=='ballSpeed' else v)+';' for k,v in values.items())+'}}',encoding='utf-8')
timing=(a.source/'BounceGarden/BounceGarden/ProjectSettings/TimeManager.asset').read_text(encoding='utf-8-sig')
(root/'assets/unity-sample/ProjectSettings/TimeManager.asset').write_text(timing,encoding='utf-8')
keep(timing,'TimeManager','ProjectSettings/TimeManager.asset')
(root/'references/project-launch-source.json').write_text(json.dumps(dict(source='BounceBusters',extractions=records,adaptations=['Generated class/namespace and explicit FixedStep binding replace MonoBehaviour.FixedUpdate scheduling only.','BeginBurst retains the original reservation, initialization, RuleState charging and pending queue. Unrelated shop/reward/metrics hooks are excluded.','Physical BallBody initialization/deactivation and camera/UI bindings are implemented by the gallery adapter.','Original RuleState is copied with only its namespace changed.','Original OnGUI events, 6px release threshold, minimum aim y and 12–168 degree clamp are retained.']),ensure_ascii=False,indent=2),encoding='utf-8')
print('Extracted production launch/input:',len(records),'source records')
