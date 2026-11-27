using System;
using UnityEngine;

namespace ImpactFeedback {
 public enum Effect { Flash, Trail, Shake, HitStop, SlowMotion, Fragments, Squash, Pitch }
 public enum ImpactKind { Hit, Break, Special }
 [Serializable] public sealed class ImpactProfile {
  public int schemaVersion=1;
  public bool flash=true,trail=true,shake=true,hitStop=true,slowMotion=true,fragments=true,squash=true,pitch=true;
  public float flashStrength=.48f,flashMs=50;
  public float trailLength=2.1f,trailWidth=.20f,trailLife=.24f,trailOpacity=.95f;
  public float shakeHeight=.0032f,shakeMs=320;
  public float hitStopMs=25,hitStopCooldownMs=500,hitStopBudgetMs=30;
  public float slowScale=.55f,slowMs=160;
  public bool petalFragments=true;
  public int fragmentCount=12;
  public float fragmentSpeed=4,fragmentSize=.16f,fragmentLife=1.2f;
  public float paintMinRadius=.45f,paintMaxRadius=2.8f;
  public float squashAmount=.18f,squashMs=450;
  public float pitchStep=.025f,pitchMax=1.15f,pitchResetMs=1200,volume=.65f;
  // pitchStep/pitchMax remain for reading earlier presets. Scale mode uses exact notes.
  public int scaleNotes=8;public float noteIntervalMs=70,noteVolume=.8f;
  public bool Enabled(Effect e){switch(e){case Effect.Flash:return flash;case Effect.Trail:return trail;case Effect.Shake:return shake;case Effect.HitStop:return hitStop;case Effect.SlowMotion:return slowMotion;case Effect.Fragments:return fragments;case Effect.Squash:return squash;default:return pitch;}}
  public void Enable(Effect e,bool v){switch(e){case Effect.Flash:flash=v;break;case Effect.Trail:trail=v;break;case Effect.Shake:shake=v;break;case Effect.HitStop:hitStop=v;break;case Effect.SlowMotion:slowMotion=v;break;case Effect.Fragments:fragments=v;break;case Effect.Squash:squash=v;break;case Effect.Pitch:pitch=v;break;}}
  public ImpactProfile Copy()=>JsonUtility.FromJson<ImpactProfile>(JsonUtility.ToJson(this));
  public static ImpactProfile FromJson(string json){
   var p=JsonUtility.FromJson<ImpactProfile>(json);if(p==null||p.schemaVersion!=1)throw new ArgumentException("不支持的参数版本");
   if(json.IndexOf("\"scaleNotes\"",StringComparison.Ordinal)<0)p.scaleNotes=8;
   if(json.IndexOf("\"noteIntervalMs\"",StringComparison.Ordinal)<0)p.noteIntervalMs=70;
   if(json.IndexOf("\"noteVolume\"",StringComparison.Ordinal)<0)p.noteVolume=.8f;
   if(json.IndexOf("\"petalFragments\"",StringComparison.Ordinal)<0)p.petalFragments=false;
   p.Clamp();return p;
  }
  public void Clamp(){
   schemaVersion=1;flashStrength=C(flashStrength,0,1,.48f);flashMs=C(flashMs,5,400,50);
   trailLength=C(trailLength,.1f,5,2.1f);trailWidth=C(trailWidth,.02f,.6f,.2f);trailLife=C(trailLife,.03f,1,.24f);trailOpacity=C(trailOpacity,0,1,.95f);
   shakeHeight=C(shakeHeight,0,.04f,.0032f);shakeMs=C(shakeMs,40,800,320);
   hitStopMs=C(hitStopMs,0,250,25);hitStopCooldownMs=C(hitStopCooldownMs,0,1500,500);hitStopBudgetMs=C(hitStopBudgetMs,0,500,30);
   slowScale=C(slowScale,.05f,1,.55f);slowMs=C(slowMs,0,1500,160);
   fragmentCount=Mathf.Clamp(fragmentCount,0,64);fragmentSpeed=C(fragmentSpeed,.1f,12,4);fragmentSize=C(fragmentSize,.03f,.5f,.16f);fragmentLife=C(fragmentLife,.1f,3,1.2f);
   paintMinRadius=C(paintMinRadius,0,4,.45f);paintMaxRadius=C(paintMaxRadius,paintMinRadius,5,2.8f);
   squashAmount=C(squashAmount,0,.45f,.18f);squashMs=C(squashMs,50,1000,450);
   pitchStep=C(pitchStep,0,.15f,.025f);pitchMax=C(pitchMax,1,2,1.15f);pitchResetMs=C(pitchResetMs,200,5000,1200);volume=C(volume,0,1,.65f);
   scaleNotes=Mathf.Clamp(scaleNotes,1,8);noteIntervalMs=C(noteIntervalMs,30,250,70);noteVolume=C(noteVolume,0,1,.8f);
  }
  static float C(float v,float min,float max,float fallback)=>float.IsNaN(v)||float.IsInfinity(v)?fallback:Mathf.Clamp(v,min,max);
  public static ImpactProfile GalleryDefault(){var p=new ImpactProfile();p.shakeHeight=.014f;p.shakeMs=240;return p;}
  public static ImpactProfile Teaching(){var p=GalleryDefault();p.hitStopMs=90;p.hitStopBudgetMs=180;p.shakeHeight=.022f;p.shakeMs=300;p.slowScale=.25f;p.slowMs=450;p.flashMs=120;p.squashAmount=.28f;p.pitchStep=.07f;p.pitchMax=1.6f;return p;}
 }
}
