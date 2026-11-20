"""Verify that critical launch methods remain the production source, not a second scheduler."""
from pathlib import Path
import hashlib,json
r=Path(__file__).resolve().parents[1];folder=r/'assets/unity-sample/Assets/ImpactFeedback/ProjectLaunch'
records=json.loads((r/'references/project-launch-source.json').read_text(encoding='utf-8'))['extractions']
launch=(folder/'GameSession.Launch.generated.cs').read_text(encoding='utf-8');input_code=(folder/'ProjectBattleInput.generated.cs').read_text(encoding='utf-8')
intact={'bool TryShootCore(Vector2 direction)','public static Vector2 NormalizeAimDirection(Vector2 direction)','void ReturnReserved()','bool BufferChainShot(Vector2 direction)','void PromoteBufferedChainShot()','ChainCanLaunch','SpawnPosition','live group cooldown'}
count=0
for e in records:
 assert hashlib.sha256(e['body'].encode()).hexdigest()==e['sha256'],e['symbol']
 if e['symbol'] in intact:assert e['body'] in launch,e['symbol'];count+=1
 if e['symbol'] in ('RuleState','Round4ReferenceData'):
  actual=(folder/(e['symbol']+'.cs')).read_text(encoding='utf-8')
  assert actual==e['body'].replace('namespace BounceGarden','namespace ImpactFeedback.ProjectLaunch');count+=1
 if e['symbol']=='UIBlocked':assert e['body'] in input_code;count+=1
 if e['symbol']=='void FixedUpdate()':
  expected=e['body'][e['body'].index('PhysicsStep++;'):e['body'].rfind('}')].replace('Time.fixedDeltaTime','fixedDeltaTime')
  assert expected in launch,'original fixed-step spawn section';count+=1
 if e['symbol']=='void BeginBurst(Vector2 direction)':
  expected=e['body'][e['body'].index('int count=Mathf.Max(1,CurrentLevel.volleyCount);'):e['body'].index('var actualBooster=')]
  assert expected in launch,'original whole-volley reservation and first-ball section';count+=1
assert '(pos-pointerStart).sqrMagnitude>36&&gameCamera.pixelRect.Contains(pos)' in input_code
assert 'direction*CurrentLevel.ballSpeed' in launch and 'nextSpawnTime+=CurrentLevel.ballInterval' in launch
print(f'Production launch verified: {count} unchanged methods/resources; original queue, direction clamp, UI hit test and release threshold retained')
