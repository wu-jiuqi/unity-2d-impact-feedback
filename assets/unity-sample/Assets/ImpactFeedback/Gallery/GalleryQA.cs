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
  [Serializable]class Report{public string unity,assemblySha256,version="0.1.0";public int configuredFps,passed,failed;public List<Check> checks=new List<Check>();}
  Report report=new Report();string folder;ImpactGallery g;
  void CheckThat(bool passed,string name){report.checks.Add(new Check{name=name,passed=passed});if(passed)report.passed++;else report.failed++;Debug.Log("GALLERY_QA "+(passed?"PASS ":"FAIL ")+name);}
  string Arg(string key,string fallback){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
  IEnumerator Start(){
   folder=Arg("--qa-out",Path.Combine(Application.persistentDataPath,"qa"));Directory.CreateDirectory(folder);report.unity=Application.unityVersion;report.configuredFps=int.Parse(Arg("--qa-fps","60"));Application.targetFrameRate=report.configuredFps;
   using(var hash=SHA256.Create())report.assemblySha256=BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(typeof(ImpactGallery).Assembly.Location))).Replace("-","").ToLowerInvariant();
   yield return null;g=ImpactGallery.Instance;g.SetMode(2);g.SetProfile(ImpactProfile.Teaching());g.ResetScene();
   int count=g.Active.Presenter.ImpactCount;yield return new WaitForSecondsRealtime(1.1f);
   CheckThat(g.Active.Presenter.ImpactCount>count,"real PhysicsScene2D collision triggers feedback");
   CheckThat(Time.timeScale==1,"gallery never takes process-wide Time.timeScale ownership");
   var clock=new FeedbackClock();var p=ImpactProfile.Teaching();clock.Request(10,p,true,true);clock.Tick(10.04f,.04f,false);
   CheckThat(clock.Delta==0&&clock.Stopped,"hitstop freezes simulation clock");clock.Request(10.05f,p,true,true);clock.Tick(10.091f,.016f,false);
   CheckThat(!clock.Stopped&&clock.Scale==p.slowScale,"coalesced stop ends and transitions into slow motion");
   CheckThat(clock.ActualStopMs>=90&&clock.ActualStopMs<100,"actual stop is measured from real-time elapsed");
   clock.Tick(11,.016f,false);CheckThat(clock.Scale==1,"slow motion returns to original speed");
   clock.Reset();CheckThat(!clock.Stopped&&clock.Scale==1,"clock reset releases all time control");
   p=new ImpactProfile();p.hitStopMs=200;p.hitStopBudgetMs=30;clock.Request(20,p,true,false);
   CheckThat(Mathf.Abs(clock.RequestedStopMs-30)<.01f,"per-second budget bounds requested stop duration");
   clock.Tick(20.04f,.04f,false);int stops=clock.Stops;clock.Request(20.1f,p,true,false);CheckThat(clock.Stops==stops,"cooldown prevents repeated stops");
   // Isolate both worlds for a physical A/B observation.
   g.SetProfile(ImpactProfile.Teaching());g.SetMode(1);g.Select(Effect.HitStop);g.Launch();
   g.Active.Presenter.Hit(Vector2.zero,Vector2.up,Color.cyan,ImpactKind.Special);
   Vector2 active=g.Active.Ball.position,baseline=g.Baseline.Ball.position;
   yield return new WaitForSecondsRealtime(.045f);
   CheckThat(Vector2.Distance(active,g.Active.Ball.position)<.0001f,"feedback world bodies remain fixed during hitstop");
   CheckThat(Vector2.Distance(baseline,g.Baseline.Ball.position)>.03f,"baseline world continues during feedback hitstop");
   yield return new WaitForSecondsRealtime(.2f);CheckThat(!g.Active.Presenter.Clock.Stopped,"player hitstop recovers at configured frame rate");
   var b=g.Active.Brick;Vector3 anchor=b.position;Vector3 scale=b.localScale;Vector2 collider=b.GetComponent<BoxCollider2D>().size;
   g.SetMode(2);g.SetProfile(ImpactProfile.Teaching());
   for(int i=0;i<20;i++)g.Active.Presenter.Hit(Vector2.zero,Vector2.right,Color.cyan,ImpactKind.Special);
   yield return null;CheckThat(g.Active.Presenter.ActiveParticles<=128&&g.Active.Presenter.PeakParticles<=128,"dense bursts respect 128-slot particle pool");
   CheckThat(b.position==anchor&&b.localScale==scale&&b.GetComponent<BoxCollider2D>().size==collider,"squash leaves body transform and collider unchanged");
   CheckThat(g.Active.Presenter.LastPitch<=g.Profile.pitchMax,"pitch progression is capped");
   g.SetPaused(true);Vector2 frozen=g.Active.Ball.position;yield return new WaitForSecondsRealtime(.12f);
   CheckThat(g.Active.Ball.position==frozen&&!g.Active.Presenter.Clock.Stopped,"pause freezes bodies and cancels hitstop");
   CheckThat(g.Active.Presenter.ActiveParticles==0,"pause resets transient pooled effects");g.SetPaused(false);
   g.Active.Presenter.ResetPresentation();g.Profile.pitchResetMs=200;g.Active.Presenter.Hit(Vector2.zero,Vector2.up,Color.cyan,ImpactKind.Hit);g.Active.Presenter.Hit(Vector2.zero,Vector2.up,Color.cyan,ImpactKind.Hit);
   CheckThat(g.Active.Presenter.LastPitch>1,"successive hit raises audio pitch");yield return new WaitForSecondsRealtime(.25f);
   g.Active.Presenter.Hit(Vector2.zero,Vector2.up,Color.cyan,ImpactKind.Hit);CheckThat(g.Active.Presenter.LastPitch==1,"pitch resets after configured gap");
   string original=File.Exists(g.PresetPath)?File.ReadAllText(g.PresetPath):null;
   g.Select(Effect.HitStop);yield return null;
   var durationField=GameObject.Find("Value 顿帧时长").GetComponent<InputField>();durationField.onEndEdit.Invoke("100");
   CheckThat(g.Profile.hitStopMs==100,"numeric duration input commits exact milliseconds");
   g.Profile.hitStopMs=137;g.Save();g.Profile.hitStopMs=1;g.Load();CheckThat(Mathf.Abs(g.Profile.hitStopMs-137)<.001f,"preset JSON round trip preserves edited hitstop duration");
   if(original==null)File.Delete(g.PresetPath);else File.WriteAllText(g.PresetPath,original);
   p=new ImpactProfile();p.hitStopMs=float.NaN;p.pitchMax=100;p.fragmentCount=1000;p.Clamp();CheckThat(p.hitStopMs==25&&p.pitchMax==2&&p.fragmentCount==64,"invalid preset numbers are normalized");
   for(int i=0;i<8;i++){
    g.SetProfile(ImpactProfile.Teaching());g.SetMode(1);g.Select((Effect)i);yield return null;
    CheckThat(g.Active.Presenter.Enabled((Effect)i),"effect independently available: "+ImpactGallery.Names[i]);
    CheckThat(!g.Active.Presenter.Enabled((Effect)((i+1)%8)),"single mode isolates selection: "+ImpactGallery.Names[i]);
    var sliders=FindObjectsByType<Slider>(FindObjectsSortMode.None);CheckThat(sliders.Length>=3,"parameter controls exist: "+ImpactGallery.Names[i]);
    foreach(var s in sliders)s.value=(s.minValue+s.maxValue)*.5f;
    g.Active.Presenter.Hit(new Vector2(0,1),Vector2.up,Color.cyan,ImpactKind.Special);yield return new WaitForSecondsRealtime(.06f);
    yield return new WaitForEndOfFrame();GalleryCapture.Save(g,Path.Combine(folder,"effect-"+i+".png"));yield return null;
   }
   g.SetProfile(ImpactProfile.Teaching());g.SetMode(2);g.Launch();yield return new WaitForSecondsRealtime(.7f);yield return new WaitForEndOfFrame();GalleryCapture.Save(g,Path.Combine(folder,"combination.png"));yield return null;
   for(int color=0;color<5;color++){
    g.SetColour(color);yield return null;
    CheckThat(g.Active.Face.sprite==FiveColourBricks.Frames(color)[0],"correct five-color rest sprite: "+FiveColourBricks.Keys[color]);
    GalleryCapture.Save(g,Path.Combine(folder,"brick-"+FiveColourBricks.Keys[color]+".png"));
    CheckThat(FiveColourBricks.Frames(color).Length==16,"all 16 authored frames available: "+FiveColourBricks.Keys[color]);
    g.Active.Sequence.Play(FiveColourBricks.Frames(color),25);g.Active.Sequence.Advance(.12f);
    CheckThat(g.Active.Sequence.FrameIndex==3,"authored animation follows 25 fps: "+FiveColourBricks.Keys[color]);
    int before=g.Active.Sequence.FrameIndex;g.Active.Sequence.Advance(0);CheckThat(g.Active.Sequence.FrameIndex==before,"zero presentation time freezes sequence: "+FiveColourBricks.Keys[color]);
    g.Active.Sequence.Advance(1);CheckThat(!g.Active.Sequence.Playing&&g.Active.Sequence.FrameIndex==15,"sequence ends at authored final frame: "+FiveColourBricks.Keys[color]);
   }
   g.SetColour(0);g.Select(Effect.HitStop);g.SetMode(1);yield return null;GalleryCapture.Save(g,Path.Combine(folder,"gallery-overview.png"));
   int hints=0;foreach(var text in FindObjectsByType<Text>(FindObjectsSortMode.None))if(text.name=="Effect explanation")hints++;
   CheckThat(hints==1,"rapid preset and effect switching retains one description label");
   g.SetPaused(true);g.Active.Presenter.ResetPresentation();CheckThat(g.Active.Presenter.Clock.Scale==1,"reset leaves presentation clock ready for a new session");
   File.WriteAllText(Path.Combine(folder,"result.json"),JsonUtility.ToJson(report,true));Debug.Log("GALLERY_QA_COMPLETE "+report.passed+" passed / "+report.failed+" failed");yield return new WaitForSecondsRealtime(.3f);Application.Quit(report.failed==0?0:1);
  }
 }
}
