using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
namespace ImpactFeedback.Gallery {
 public sealed class GalleryQA:MonoBehaviour {
  [Serializable]class Check{public string name;public bool passed;}
  [Serializable]class Report{public string unity,assemblySha256,projectRuleTests,version="0.2.2";public int configuredFps,passed,failed;public float shakePeakPixels;public float[] scaleFrequencies;public List<Check> checks=new List<Check>();}
  Report report=new Report();string folder;ImpactGallery g;
  void C(bool passed,string name){report.checks.Add(new Check{name=name,passed=passed});if(passed)report.passed++;else report.failed++;Debug.Log("GALLERY_QA "+(passed?"PASS ":"FAIL ")+name);}
  string Arg(string key,string fallback){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
  void Capture(string name){g.RefreshReadouts();GalleryCapture.Save(g,Path.Combine(folder,name+".png"));}
  void VerifyScaleAudio(){
   var bank=new SolfeggioScale();float[] expected={261.625565f,293.664768f,329.627557f,349.228231f,391.995436f,440,493.883301f,523.251131f};report.scaleFrequencies=new float[8];
   int perNote=Mathf.RoundToInt(SolfeggioScale.SampleRate*.26f);var combined=new float[perNote*8];
   for(int n=0;n<8;n++){
    var clip=bank.Clip(n);var data=new float[clip.samples];clip.GetData(data,0);int first=-1,last=0,crossings=0;float peak=0;
    for(int i=1;i<data.Length;i++){peak=Mathf.Max(peak,Mathf.Abs(data[i]));if(i<SolfeggioScale.SampleRate*.015f||i>SolfeggioScale.SampleRate*.16f)continue;if(data[i-1]<=0&&data[i]>0){if(first<0)first=i;last=i;crossings++;}}
    float measured=(crossings-1)*SolfeggioScale.SampleRate/(float)(last-first);report.scaleFrequencies[n]=measured;
    C(Mathf.Abs(measured-expected[n])/expected[n]<.005f,"rendered audio frequency matches "+SolfeggioScale.Names[n]);C(peak<.5f&&Mathf.Abs(data[0])<.0001f&&Mathf.Abs(data[data.Length-1])<.001f,"note envelope is bounded with clean attack/release: "+SolfeggioScale.Names[n]);
    System.Array.Copy(data,0,combined,n*perNote,data.Length);
   }
   using(var writer=new BinaryWriter(File.Create(Path.Combine(folder,"solfeggio-scale.wav")))){
    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+combined.Length*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(SolfeggioScale.SampleRate);writer.Write(SolfeggioScale.SampleRate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(combined.Length*2);foreach(float sample in combined)writer.Write((short)(Mathf.Clamp(sample,-1,1)*32767));
   }bank.Dispose();
  }
  void VerifyPetals(){
   g.SetMode(0);g.Select(Effect.Fragments);g.SetProfile(new ImpactProfile());g.ResetScene();g.SetPaused(true);
   var p=g.Active.Presenter;var brick=g.Active.Bricks[12];Vector3 anchor=brick.Root.position;Vector2 collider=brick.Collider.size;
   C(g.Profile.petalFragments,"new profiles use 0.5.1 petal fragments");
   C(!ImpactProfile.FromJson("{\"schemaVersion\":1}").petalFragments,"old JSON presets preserve legacy paint");
   C(ImpactProfile.FromJson(JsonUtility.ToJson(g.Profile)).petalFragments,"petal preset round trip preserves mode");
   var frames=PetalAtlas.Frames;C(frames.Length==16&&frames[0].texture.width==1254&&frames[15].rect.y==0,"original 4x4 petal atlas slices top-left first");
   UnityEngine.Random.InitState(41051);var state=UnityEngine.Random.state;float expected=UnityEngine.Random.value;UnityEngine.Random.state=state;
   p.Hit(Vector2.zero,Vector2.up,Color.red,ImpactKind.Break,brick.Face,.9f);p.Step(.3f,0);
   C(UnityEngine.Random.value==expected,"petal emission does not consume the physics random source");UnityEngine.Random.state=state;
   var petals=new List<SpriteRenderer>();bool white=true,tilted=false;foreach(var sr in p.GetComponentsInChildren<SpriteRenderer>())if(sr.gameObject.activeSelf&&sr.sprite.texture.name=="break-petals-atlas"){petals.Add(sr);white&=sr.color==Color.white;tilted|=Mathf.Abs(sr.transform.eulerAngles.z)>1;}
   C(p.ActiveParticles==12&&petals.Count==12&&p.ActiveLandedPetals==0,"break emits twelve airborne petals from the shared pool");
   C(white&&tilted,"full-colour petal renders use white tint and randomized rotation");
   var positions=new Vector3[petals.Count];for(int i=0;i<petals.Count;i++)positions[i]=petals[i].transform.position;
   p.Step(0,0);bool frozen=true;for(int i=0;i<petals.Count;i++)frozen&=petals[i].transform.position==positions[i];C(frozen,"zero presentation delta freezes petal flight");Capture("petals-airborne");
   p.Step(1.25f,0);C(p.ActiveLandedPetals==12,"all petals land after the randomized flight interval");
   bool bounded=true,depth=true;var bounds=p.PetalLandingBounds.Value;var unique=new HashSet<Vector2>();foreach(var sr in petals){var v=(Vector2)sr.transform.position;bounded&=v.x>=bounds.xMin&&v.x<=bounds.xMax&&v.y>=bounds.yMin&&v.y<=bounds.yMax;depth&=sr.sortingOrder==p.PetalGroundSortingOrder;unique.Add(v);}
   C(bounded&&unique.Count>4&&depth,"petals settle at varied bounded points behind the bricks");Capture("petals-landed");
   p.Step(.2f,0);C(p.ActiveLandedPetals==12,"landed petals remain visible during ground hold");
   bool faded=false;for(int i=0;i<35;i++){p.Step(.05f,0);foreach(var sr in petals)if(sr.gameObject.activeSelf&&sr.color.a>0&&sr.color.a<.9f)faded=true;}
   C(faded,"landed petals fade gradually before recycling");p.Step(5,0);C(p.ActiveParticles==0&&p.ActiveLandedPetals==0,"petals disappear and release every pool slot");
   g.Profile.fragmentCount=64;for(int i=0;i<5;i++)p.Hit(Vector2.zero,Vector2.up,Color.red,ImpactKind.Break,brick.Face,.9f);p.Step(.1f,0);
   int poolCount=0;foreach(Transform child in p.transform)if(child.name.StartsWith("Project pooled particle "))poolCount++;
   C(p.ActiveParticles==128&&poolCount==128,"dense petal bursts reuse exactly 128 objects and drop excess");
   g.Profile.fragments=false;p.Step(.01f,0);C(p.ActiveParticles==0,"disabling fragments clears petals immediately");g.Profile.fragments=true;p.Hit(Vector2.zero,Vector2.up,Color.red,ImpactKind.Break,brick.Face,.9f);p.Step(.1f,0);p.ResetPresentation();C(p.ActiveParticles==0&&p.ActiveLandedPetals==0,"reset clears both airborne and landed petals");
   C(brick.Root.position==anchor&&brick.Collider.size==collider,"petals leave physics roots and colliders unchanged");g.SetPaused(false);g.ResetScene();
  }
  IEnumerator Start(){
   folder=Arg("--qa-out",Path.Combine(Application.persistentDataPath,"qa"));Directory.CreateDirectory(folder);report.unity=Application.unityVersion;report.configuredFps=int.Parse(Arg("--qa-fps","60"));Application.targetFrameRate=report.configuredFps;
   using(var hash=SHA256.Create())report.assemblySha256=BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(typeof(ImpactGallery).Assembly.Location))).Replace("-","").ToLowerInvariant();
   yield return null;g=ImpactGallery.Instance;g.ResetScene();VerifyScaleAudio();
   report.projectRuleTests=ImpactFeedback.ProjectLaunch.RuleSelfTests.RunAll();C(report.projectRuleTests.StartsWith("PASS"),"unchanged project RuleState self-tests pass");
   var shallow=ImpactFeedback.ProjectLaunch.GameSession.NormalizeAimDirection(new Vector2(1,.001f));C(Mathf.Abs(Mathf.Atan2(shallow.y,shallow.x)*Mathf.Rad2Deg-12)<.001f,"production minimum launch angle remains 12 degrees");
   C(!g.Active.Production.TryShoot(Vector2.down)&&!g.Active.Production.TryShoot(Vector2.zero),"production rejects downward and zero-length shots");
   C(g.Active.Bricks.Length==15&&g.Baseline.Bricks.Length==15,"both worlds contain 3 x 5 bricks");
   var positions=new HashSet<Vector2>();var colors=new HashSet<int>();foreach(var b in g.Active.Bricks){positions.Add(b.Root.position);colors.Add(b.Color);}
   C(positions.Count==15&&colors.Count==5,"fifteen distinct grid cells use all five authored colors");
   Capture("palette");var rendered=new Texture2D(2,2);rendered.LoadImage(File.ReadAllBytes(Path.Combine(folder,"palette.png")));var hues=new Color[5];
   for(int i=0;i<5;i++){var pixel=g.Active.Camera.WorldToScreenPoint(g.Active.Bricks[i].Root.position);hues[i]=rendered.GetPixel(Mathf.RoundToInt(pixel.x),Mathf.RoundToInt(pixel.y));}
   C(hues[0].r>hues[0].g&&hues[1].g>hues[1].b&&hues[2].g>hues[2].r&&hues[3].b>hues[3].r&&hues[4].b>hues[4].g,"rendered grid preserves pink yellow mint blue lilac across separate atlases");Destroy(rendered);
   C(g.Active.Balls.Count==5&&g.Active.Presenter.Trails.Count==5,"five pooled balls each own a separate yarn trail");
   bool loaded=true;var loadedPositions=new HashSet<Vector2>();for(int i=0;i<5;i++){loaded&=Vector2.Distance(g.Active.Balls[i].transform.position,g.Active.Balls[i].position)<.0001f;loadedPositions.Add(g.Active.Balls[i].transform.position);}C(loaded&&loadedPositions.Count==5,"all five loaded ball visuals match their distinct waiting positions");
   Vector2 resting=g.Active.Ball.position;yield return new WaitForSecondsRealtime(.25f);
   C(g.Active.SpawnedThisVolley==0&&g.Active.Ball.position==resting&&g.Active.Presenter.ImpactCount==0,"startup waits for player and never auto launches");
   g.SetMode(1);g.Select(Effect.Trail);float firstShotClock=g.Active.Production.SimulationClock;g.Launch();C(g.Active.Production.HasQueuedShot&&g.Active.SpawnedThisVolley==0,"manual release uses the original queued-shot fixed-step entry");
   yield return new WaitForSecondsRealtime(.05f);C(g.Active.SpawnedThisVolley==1,"second ball waits for the project 0.10 second interval");
   g.Launch();C(g.Active.Production.HasBufferedShot,"production launch accepts one release during a pending group");int beforeBuffered=g.Active.Production.SpawnCount;g.Launch();C(g.Active.Production.SpawnCount==beforeBuffered,"a second extra release cannot overwrite the original input buffer");
   float volleyDeadline=Time.realtimeSinceStartup+4;
   while(g.Active.Production.SimulationClock<firstShotClock+.44f&&Time.realtimeSinceStartup<volleyDeadline)yield return null;
   C(g.Active.SpawnedThisVolley==5&&g.Baseline.SpawnedThisVolley==5,"one command emits exactly five balls in both worlds");
   C(!g.Active.CanLaunch,"original 0.55 second live-group cooldown is retained");
   bool trail=false;foreach(var t in g.Active.Presenter.Trails)trail|=t.Count>1;C(trail,"real moving balls produce independent fixed-step yarn paths");
   C(g.Active.Presenter.Trails[0].ShaderName=="ImpactFeedback/YarnTrail","trail uses the project's calibrated fiber/glow shader");
   C(g.Active.Presenter.ImpactCount>0,"real PhysicsScene2D contacts hit the three-row board");Capture("effect-1");
   yield return new WaitForSecondsRealtime(.15f);C(g.Active.Production.SpawnCount>=6&&g.Active.ActiveBalls>0,"buffered release promotes while earlier balls are still flying");
   C(g.Active.Balls.Count>=10,"original reserve-whole-volley policy grows the pool instead of imposing a five-active-ball cap");
   float deadline=Time.realtimeSinceStartup+12;while((g.Active.VolleyInProgress||g.Baseline.VolleyInProgress)&&Time.realtimeSinceStartup<deadline)yield return null;
   C(g.Active.ActiveBalls==0&&g.Active.Production.PendingBurstBalls==0,"both original five-ball groups finish and return");
   int completed=g.Active.Production.SpawnCount;yield return new WaitForSecondsRealtime(.25f);C(g.Active.Production.SpawnCount==completed,"finished groups never auto replay without player input");
   g.Launch();yield return new WaitForSecondsRealtime(.05f);C(g.Active.Production.SpawnCount==completed+1,"another manual release enters the original fixed-step launcher");g.ResetScene();
   C(Time.timeScale==1,"per-world simulation never changes global Time.timeScale");
   var clock=new FeedbackClock();var p=ImpactProfile.Teaching();clock.Request(10,p,true,true);clock.Tick(10.04f,.04f,false);C(clock.Delta==0&&clock.Stopped,"hitstop freezes the simulation clock");
   clock.Request(10.05f,p,true,true);clock.Tick(10.091f,.016f,false);C(!clock.Stopped&&clock.Scale==p.slowScale,"coalesced stop transitions to slow motion");
   C(clock.ActualStopMs>=90&&clock.ActualStopMs<100,"actual stop duration is measured from real time");clock.Tick(11,.016f,false);C(clock.Scale==1,"slow motion restores original speed");
   clock.Reset();C(!clock.Stopped&&clock.Scale==1,"clock reset releases all time control");p=new ImpactProfile();p.hitStopMs=200;p.hitStopBudgetMs=30;clock.Request(20,p,true,false);C(Mathf.Abs(clock.RequestedStopMs-30)<.01f,"per-second budget bounds stop requests");
   clock.Tick(20.04f,.04f,false);int stops=clock.Stops;clock.Request(20.1f,p,true,false);C(clock.Stops==stops,"cooldown prevents repeated stop requests");
   g.SetProfile(ImpactProfile.Teaching());g.Select(Effect.HitStop);g.ResetScene();g.Launch();yield return new WaitForSecondsRealtime(.03f);g.Active.Presenter.Hit(Vector2.zero,Vector2.up,Color.cyan,ImpactKind.Special);
   Vector2 active=g.Active.Ball.position,baseline=g.Baseline.Ball.position;yield return new WaitForSecondsRealtime(.045f);
   C(Vector2.Distance(active,g.Active.Ball.position)<.0001f,"feedback bodies remain fixed during hitstop");C(Vector2.Distance(baseline,g.Baseline.Ball.position)>.03f,"baseline continues during the feedback-side stop");
   yield return new WaitForSecondsRealtime(.2f);C(!g.Active.Presenter.Clock.Stopped,"player hitstop recovers");
   g.SetProfile(ImpactProfile.GalleryDefault());g.Select(Effect.Shake);g.ResetScene();var brick=g.Active.Bricks[12];Vector3 anchor=brick.Root.position;Vector2 collider=brick.Collider.size;
   g.Active.Presenter.Hit(anchor,Vector2.up,FiveColourBricks.Tints[brick.Color],ImpactKind.Special,brick.Face,GalleryLane.BrickWidth);
   float pixels=g.Active.Presenter.CameraOffset.magnitude/(g.Active.Camera.orthographicSize*2)*g.Active.Camera.pixelHeight;report.shakePeakPixels=pixels;
   C(pixels>=3,"gallery shake delivers a clearly visible first impact of at least three pixels");
   Vector3 screen=new Vector3(g.Active.Camera.pixelRect.center.x,g.Active.Camera.pixelRect.center.y,0);Vector3 stable=g.Active.Camera.ScreenToWorldPoint(screen)-g.Active.Presenter.CameraOffset;
   g.Active.Presenter.Step(0,.07f);g.Active.Presenter.Step(0,.03f);Vector3 stable2=g.Active.Camera.ScreenToWorldPoint(screen)-g.Active.Presenter.CameraOffset;
   C((stable-stable2).sqrMagnitude<.000001f,"camera shake does not change the stable aiming coordinate");
   C(brick.Root.position==anchor&&brick.Collider.size==collider,"stronger shake preserves brick physics positions");
   yield return new WaitForSecondsRealtime(.4f);C(g.Active.Presenter.CameraOffset.sqrMagnitude<.000001f,"stronger shake returns exactly to rest");
   VerifyPetals();
   g.SetMode(2);g.SetProfile(ImpactProfile.GalleryDefault());g.Profile.petalFragments=false;g.ResetScene();int landed=g.Active.Presenter.LandedParticles;
   for(int i=0;i<20;i++)g.Active.Presenter.Hit(brick.Root.position,Vector2.up,FiveColourBricks.Tints[brick.Color],ImpactKind.Special,brick.Face,GalleryLane.BrickWidth);
   g.Active.Presenter.Step(.2f,0);C(g.Active.Presenter.ActiveParticles<=128&&g.Active.Presenter.PeakParticles<=128,"dense multi-brick paint bursts share a 128-slot cap");
   C(g.Active.Presenter.LandedParticles>landed&&g.Active.Presenter.ActivePaintMarks>0,"flying drops land as persistent paint marks");
   bool splat=false;foreach(var sr in g.Active.Presenter.GetComponentsInChildren<SpriteRenderer>())if(sr.gameObject.activeSelf&&sr.sprite.texture.name=="paint_splatters")splat=true;
   C(splat,"landed particles use the project's paint_splatters atlas");Capture("project-paint");
   g.SetPaused(true);C(g.Active.Presenter.ActiveParticles==0&&!g.Active.Presenter.Clock.Stopped,"pause cancels clock ownership and paint transients");g.SetPaused(false);g.ResetScene();
   g.SetMode(1);g.Select(Effect.Trail);g.Launch();yield return new WaitForSecondsRealtime(.03f);g.Active.Ball.position=(Vector2)brick.Root.position+Vector2.down*.8f;g.Active.Ball.linearVelocity=Vector2.up*7.12f;
   yield return new WaitForSecondsRealtime(.08f);C(brick.Breaking&&brick.Sequence.Playing,"real collision starts the authored brick break sequence");
   int beforeRefill=g.Active.Refills;brick.Sequence.Advance(1);g.Active.Tick(Time.unscaledTime,0,false);
   C(!brick.Breaking&&brick.Collider.enabled&&brick.Face.enabled&&brick.Face.sprite==FiveColourBricks.Frames(brick.Color)[0],"last break frame refills the original cell in the same tick with no extra delay");
   C(g.Active.Refills>beforeRefill&&g.Active.Bricks.Length==15,"refill reuses the original fifteen cells");
   g.SetPaused(true);var frozen=new Vector2[5];for(int i=0;i<5;i++)frozen[i]=g.Active.Balls[i].position;int frame=brick.Sequence.FrameIndex;yield return new WaitForSecondsRealtime(.12f);
   bool fixedAll=true;for(int i=0;i<5;i++)fixedAll&=g.Active.Balls[i].position==frozen[i];C(fixedAll&&frame==brick.Sequence.FrameIndex,"pause freezes all five bodies and sequence clocks");g.SetPaused(false);g.ResetScene();
   g.SetMode(1);g.Select(Effect.Pitch);g.Profile.pitchResetMs=200;g.Active.Presenter.ResetNotes();
   for(int n=0;n<8;n++){if(n>0)yield return new WaitForSecondsRealtime(.085f);g.Active.Presenter.Hit(Vector2.zero,Vector2.up,Color.cyan,ImpactKind.Hit);C(g.Active.Presenter.LastNoteIndex==n,"successive audible contacts advance through "+SolfeggioScale.Names[n]);}
   yield return new WaitForSecondsRealtime(.085f);g.Active.Presenter.Hit(Vector2.zero,Vector2.up,Color.cyan,ImpactKind.Hit);C(g.Active.Presenter.LastNoteIndex==7&&Mathf.Abs(g.Active.Presenter.LastPitch-2)<.001f,"ascending scale holds at the upper do octave");
   g.Active.Presenter.ResetNotes();g.Active.Presenter.Hit(Vector2.zero,Vector2.up,Color.cyan,ImpactKind.Hit);int played=g.Active.Presenter.NotesPlayed;g.Active.Presenter.Hit(Vector2.zero,Vector2.up,Color.cyan,ImpactKind.Hit);C(g.Active.Presenter.NotesPlayed==played&&g.Active.Presenter.LastNoteIndex==0,"coalesced dense contacts do not silently skip scale notes");
   yield return new WaitForSecondsRealtime(.085f);g.Active.Presenter.Hit(Vector2.zero,Vector2.up,Color.cyan,ImpactKind.Hit);C(g.Active.Presenter.LastNoteIndex==1,"the next audible contact continues at re after a merged hit");
   yield return new WaitForSecondsRealtime(.25f);g.Active.Presenter.Hit(Vector2.zero,Vector2.up,Color.cyan,ImpactKind.Hit);C(g.Active.Presenter.LastNoteIndex==0,"scale resets to do after its configured gap");
   g.Profile.pitch=false;g.Active.Presenter.ResetNotes();g.Active.Presenter.Hit(Vector2.zero,Vector2.up,Color.cyan,ImpactKind.Hit);yield return new WaitForSecondsRealtime(.085f);g.Active.Presenter.Hit(Vector2.zero,Vector2.up,Color.cyan,ImpactKind.Hit);C(g.Active.Presenter.LastNoteIndex==0,"switching off scale ascent keeps a fixed do for comparison");g.Profile.pitch=true;
   string original=File.Exists(g.PresetPath)?File.ReadAllText(g.PresetPath):null;g.Select(Effect.HitStop);yield return null;
   GameObject.Find("Value 顿帧时长").GetComponent<InputField>().onEndEdit.Invoke("100");C(g.Profile.hitStopMs==100,"numeric input commits exact milliseconds");
   g.Profile.hitStopMs=137;g.Save();g.Profile.hitStopMs=1;g.Load();C(Mathf.Abs(g.Profile.hitStopMs-137)<.001f,"preset JSON round trip preserves duration edits");if(original==null)File.Delete(g.PresetPath);else File.WriteAllText(g.PresetPath,original);
   p=new ImpactProfile();p.hitStopMs=float.NaN;p.pitchMax=100;p.fragmentCount=1000;p.Clamp();C(p.hitStopMs==25&&p.pitchMax==2&&p.fragmentCount==64,"external preset values are normalized");
   var legacy=ImpactProfile.FromJson("{\"schemaVersion\":1,\"pitch\":true,\"volume\":0.65}");C(legacy.scaleNotes==8&&legacy.noteIntervalMs==70&&legacy.noteVolume==.8f,"old presets gain audible eight-note defaults");
   var muted=ImpactProfile.FromJson("{\"schemaVersion\":1,\"noteVolume\":0}");C(muted.noteVolume==0,"explicitly muted scale presets remain muted");
   for(int i=0;i<8;i++){
    g.SetProfile(ImpactProfile.GalleryDefault());g.SetMode(1);g.Select((Effect)i);g.ResetScene();yield return null;
    C(g.Active.Presenter.Enabled((Effect)i),"effect independently available: "+ImpactGallery.Names[i]);C(!g.Active.Presenter.Enabled((Effect)((i+1)%8)),"single mode isolates selection: "+ImpactGallery.Names[i]);
    var sliders=FindObjectsByType<Slider>(FindObjectsSortMode.None);C(sliders.Length>=3,"live controls available: "+ImpactGallery.Names[i]);
    g.Launch();yield return new WaitForSecondsRealtime(.55f);Capture("effect-"+i);
   }
   for(int color=0;color<5;color++){
    g.SetColour(color);yield return null;C(g.Active.Face.sprite==FiveColourBricks.Frames(color)[0],"five-color rest sprite: "+FiveColourBricks.Keys[color]);C(FiveColourBricks.Frames(color).Length==16,"all authored frames: "+FiveColourBricks.Keys[color]);
    g.Active.Sequence.Play(FiveColourBricks.Frames(color),25);g.Active.Sequence.Advance(.12f);C(g.Active.Sequence.FrameIndex==3,"authored 25 fps cadence: "+FiveColourBricks.Keys[color]);int before=g.Active.Sequence.FrameIndex;g.Active.Sequence.Advance(0);C(g.Active.Sequence.FrameIndex==before,"zero delta freezes sequence: "+FiveColourBricks.Keys[color]);
    g.Active.Sequence.Advance(1);C(!g.Active.Sequence.Playing&&g.Active.Sequence.FrameIndex==15,"authored last frame: "+FiveColourBricks.Keys[color]);
   }
   g.SetProfile(ImpactProfile.GalleryDefault());g.SetMode(2);g.SetColour(0);g.Select(Effect.Shake);yield return null;Capture("gallery-overview");g.Launch();yield return new WaitForSecondsRealtime(.6f);Capture("combination");
   int hints=0;foreach(var t in FindObjectsByType<Text>(FindObjectsSortMode.None))if(t.name=="Effect explanation")hints++;C(hints==1,"rapid changes retain exactly one effect description");
   g.ResetScene();C(g.Active.CanLaunch&&g.Active.ActiveBalls==0&&g.Active.SpawnedThisVolley==0,"scene reset restores a loaded manual five-ball group");
   File.WriteAllText(Path.Combine(folder,"result.json"),JsonUtility.ToJson(report,true));Debug.Log("GALLERY_QA_COMPLETE "+report.passed+" passed / "+report.failed+" failed");yield return new WaitForSecondsRealtime(.3f);Application.Quit(report.failed==0?0:1);
  }
 }
}
