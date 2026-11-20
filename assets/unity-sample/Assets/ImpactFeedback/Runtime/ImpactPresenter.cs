using System.Collections.Generic;
using UnityEngine;
namespace ImpactFeedback {
 public sealed class ImpactPresenter:MonoBehaviour {
  public ImpactProfile Profile=new ImpactProfile();public readonly FeedbackClock Clock=new FeedbackClock();
  public bool AllowEffects=true,ForceSelected,PlayAudio=true;public Effect Selected;
  public int ImpactCount{get;private set;}public float LastPitch{get;private set;}=1;
  public int ActiveParticles{get;private set;}public int PeakParticles{get;private set;}public int ActivePaintMarks{get;private set;}public int LandedParticles{get;private set;}
  public float PeakShakePixels{get;private set;}public Vector3 CameraOffset=>view==null?Vector3.zero:view.transform.localPosition-cameraRest;
  public readonly List<YarnFlightTrail> Trails=new List<YarnFlightTrail>();readonly List<Transform> heads=new List<Transform>();
  sealed class Target{public SpriteRenderer face;public Vector3 rest;public Vector2 incoming;public float flashAge=9,squashAge=9;public MaterialPropertyBlock block=new MaterialPropertyBlock();}
  readonly Dictionary<SpriteRenderer,Target> targets=new Dictionary<SpriteRenderer,Target>();SpriteRenderer primary;
  Camera view;Vector3 cameraRest;Material flashMaterial;readonly Dictionary<Texture,Material> faceMaterials=new Dictionary<Texture,Material>();float alphaCutoff;float shakeAge=9,shakeDuration=.24f,shakeAmplitude;Vector2 shakeAxis=Vector2.right;
  float pitchAt=-99,nextAudio;int pitchChain,voiceCursor;AudioSource[] voices;AudioClip clip;
  System.Random random=new System.Random(79012);Sprite[] particleSprites;
  struct Drop{public SpriteRenderer view;public Vector2 origin,target;public Color color;public float age,life,size,travel,rotation;public int sprite;public bool landed;}
  readonly Drop[] pool=new Drop[128];
  public bool Enabled(Effect e)=>AllowEffects&&Profile.Enabled(e)&&(!ForceSelected||Selected==e);
  public void Initialize(SpriteRenderer target,Transform movingHead,Camera camera,Sprite[] particles,AudioClip sound,Texture2D yarn){
   primary=target;view=camera;cameraRest=view.transform.localPosition;particleSprites=particles;clip=sound;
   flashMaterial=new Material(Shader.Find("ImpactFeedback/SpriteFlash"));AddTarget(target);AddTrail(movingHead,yarn);
   voices=new AudioSource[12];for(int i=0;i<voices.Length;i++){voices[i]=gameObject.AddComponent<AudioSource>();voices[i].playOnAwake=false;voices[i].spatialBlend=0;}
   for(int i=0;i<pool.Length;i++){var o=new GameObject("Project pooled paint "+i);o.transform.SetParent(transform,false);o.layer=target.gameObject.layer;var sr=o.AddComponent<SpriteRenderer>();sr.sprite=particles[i%4];sr.sortingOrder=78;o.SetActive(false);pool[i].view=sr;}
  }
  void BindFace(SpriteRenderer target){var texture=target.sprite.texture;if(!faceMaterials.TryGetValue(texture,out var material)){material=new Material(flashMaterial){mainTexture=texture};material.SetFloat("_AlphaCutoff",alphaCutoff);faceMaterials.Add(texture,material);}target.sharedMaterial=material;}
  public void AddTarget(SpriteRenderer target){if(targets.ContainsKey(target))return;BindFace(target);targets.Add(target,new Target{face=target,rest=target.transform.localScale});}
  public void AddTrail(Transform movingHead,Texture2D yarn){heads.Add(movingHead);Trails.Add(new YarnFlightTrail(transform,movingHead,yarn,primary.gameObject.layer));}
  public void SetVisualRestScale(Vector3 scale){SetVisualRestScale(primary,scale);}
  public void SetVisualRestScale(SpriteRenderer target,Vector3 scale){var t=targets[target];t.rest=scale;target.transform.localScale=scale;}
  public void ResetTarget(SpriteRenderer target){if(target==null)return;BindFace(target);var t=targets[target];t.flashAge=t.squashAge=9;target.transform.localScale=t.rest;t.block.SetFloat("_Flash",0);target.SetPropertyBlock(t.block);}
  public void SetBakedAlphaCutoff(float cutoff){alphaCutoff=Mathf.Clamp(cutoff,0,.9f);flashMaterial.SetFloat("_AlphaCutoff",alphaCutoff);foreach(var m in faceMaterials.Values)m.SetFloat("_AlphaCutoff",alphaCutoff);}
  public void SampleTrail(int index,Vector2 position){Trails[index].Sample(position);}
  public void Hit(Vector2 position,Vector2 direction,Color color,ImpactKind kind,SpriteRenderer hitTarget=null,float brickWidth=1){
   ImpactCount++;float now=Time.unscaledTime;var t=targets[hitTarget!=null?hitTarget:primary];t.incoming=direction;
   if(Enabled(Effect.Flash))t.flashAge=0;if(Enabled(Effect.Squash))t.squashAge=0;bool special=kind==ImpactKind.Special;
   if(Enabled(Effect.Shake)&&(special||ForceSelected&&Selected==Effect.Shake)){
    if(shakeAge>=shakeDuration){shakeAge=0;shakeDuration=Profile.shakeMs/1000;shakeAmplitude=Profile.shakeHeight;shakeAxis=direction.sqrMagnitude>.001f?direction.normalized:Vector2.right;}
    else shakeAmplitude=Mathf.Max(shakeAmplitude,Profile.shakeHeight);ApplyShake();
   }
   Clock.Request(now,Profile,Enabled(Effect.HitStop)&&(special||ForceSelected&&Selected==Effect.HitStop),Enabled(Effect.SlowMotion)&&(special||ForceSelected&&Selected==Effect.SlowMotion));
   if(Enabled(Effect.Fragments)&&(kind!=ImpactKind.Hit||ForceSelected&&Selected==Effect.Fragments))Emit(position,direction,color,brickWidth);
   if(now-pitchAt>Profile.pitchResetMs/1000)pitchChain=0;
   LastPitch=Enabled(Effect.Pitch)?Mathf.Min(Profile.pitchMax,1+pitchChain*Profile.pitchStep):1;pitchChain++;pitchAt=now;
   if(PlayAudio&&clip!=null&&now>=nextAudio){var a=voices[voiceCursor++%voices.Length];a.Stop();a.clip=clip;a.pitch=LastPitch;a.volume=Profile.volume;a.Play();nextAudio=now+.025f;}
  }
  void ApplyShake(){float t=Mathf.Clamp01(shakeAge/shakeDuration);float amp=Enabled(Effect.Shake)?shakeAmplitude*view.orthographicSize*2:0;
   Vector2 tangent=new Vector2(-shakeAxis.y,shakeAxis.x);Vector2 offset=(shakeAxis*Mathf.Cos(t*Mathf.PI*6)+tangent*Mathf.Sin(t*Mathf.PI*8)*.3f)*amp*(1-t)*(1-t);
   view.transform.localPosition=cameraRest+(Vector3)offset;PeakShakePixels=Mathf.Max(PeakShakePixels,offset.magnitude/(view.orthographicSize*2)*view.pixelHeight);
  }
  public void Step(float dt,float realDelta){
   foreach(var t in targets.Values){t.flashAge+=dt;t.squashAge+=dt;t.block.SetFloat("_Flash",Enabled(Effect.Flash)?Profile.flashStrength*Mathf.Clamp01(1-t.flashAge/(Profile.flashMs/1000)):0);t.face.SetPropertyBlock(t.block);
    float u=Mathf.Clamp01(t.squashAge/(Profile.squashMs/1000)),wave=u>=1||!Enabled(Effect.Squash)?0:Mathf.Sin(u*Mathf.PI*4)*Mathf.Exp(-5*u)*Profile.squashAmount;
    Vector3 stretch=Mathf.Abs(t.incoming.y)>=Mathf.Abs(t.incoming.x)?new Vector3(1+wave,1-wave,1):new Vector3(1-wave,1+wave,1);t.face.transform.localScale=Vector3.Scale(t.rest,stretch);
   }
   ApplyShake();shakeAge+=realDelta;
   for(int i=0;i<Trails.Count;i++){bool running=heads[i].gameObject.activeSelf&&heads[i].GetComponent<Rigidbody2D>().simulated;
    if(running&&Enabled(Effect.Trail)&&!Trails[i].Emitting)Trails[i].Start(heads[i].position);
    if(!running&&Trails[i].Emitting)Trails[i].Stop();Trails[i].Step(dt,Profile,Enabled(Effect.Trail));}
   Particles(dt);
  }
  float R(float min,float max)=>min+(float)random.NextDouble()*(max-min);
  void Emit(Vector2 pos,Vector2 incoming,Color color,float cell){
   int remaining=Profile.fragmentCount,emitted=0;float lobe=Mathf.Atan2(incoming.y,incoming.x)+R(-1.2f,1.2f);
   for(int i=0;i<pool.Length&&remaining>0;i++){ref var d=ref pool[i];if(d.view.gameObject.activeSelf)continue;remaining--;
    float angle=emitted<8?lobe+R(-1.05f,1.05f):R(0,Mathf.PI*2),radius=Mathf.Sqrt(R(Profile.paintMinRadius*Profile.paintMinRadius,Profile.paintMaxRadius*Profile.paintMaxRadius));
    d.origin=pos;d.target=pos+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*cell*radius;d.color=color;d.age=0;d.life=Mathf.Max(.2f,Profile.fragmentLife+R(-.10f,.12f));
    d.travel=R(.08f,.16f)*4/Profile.fragmentSpeed;d.size=cell*(emitted<4?R(.75f,1.15f):R(.25f,.55f))*(Profile.fragmentSize/.16f);d.rotation=R(-180,180);d.sprite=4+random.Next(4);d.landed=false;
    d.view.sprite=particleSprites[1];d.view.sortingOrder=78;d.view.transform.position=pos;d.view.transform.rotation=Quaternion.Euler(0,0,angle*Mathf.Rad2Deg+135);d.view.color=color;Size(ref d,.55f,1.8f);d.view.gameObject.SetActive(true);emitted++;
   }
  }
  static void Size(ref Drop d,float factor,float stretch=1){float scale=d.size*factor/(d.view.sprite.bounds.size.x*.72f);d.view.transform.localScale=new Vector3(scale/stretch,scale*stretch,1);}
  void Particles(float dt){ActiveParticles=ActivePaintMarks=0;for(int i=0;i<pool.Length;i++){ref var d=ref pool[i];if(!d.view.gameObject.activeSelf)continue;
   if(!Enabled(Effect.Fragments)){d.view.gameObject.SetActive(false);continue;}d.age+=dt;if(d.age>=d.life){d.view.gameObject.SetActive(false);continue;}ActiveParticles++;
   if(d.age<d.travel){float flight=1-Mathf.Pow(1-d.age/d.travel,2);d.view.transform.position=Vector2.Lerp(d.origin,d.target,flight);d.view.color=new Color(d.color.r,d.color.g,d.color.b,.9f);Size(ref d,Mathf.Lerp(.45f,.75f,flight),1.8f);}
   else{if(!d.landed){d.landed=true;d.view.sprite=particleSprites[d.sprite];d.view.sortingOrder=38;d.view.transform.position=d.target;d.view.transform.rotation=Quaternion.Euler(0,0,d.rotation);LandedParticles++;}
    ActivePaintMarks++;float settle=Mathf.Clamp01((d.age-d.travel)/.10f);Size(ref d,1+.12f*Mathf.Sin(settle*Mathf.PI));float fade=Mathf.Clamp01((d.life-d.age)/.40f);d.view.color=new Color(d.color.r,d.color.g,d.color.b,.90f*fade);}
  }PeakParticles=Mathf.Max(PeakParticles,ActiveParticles);}
  public void ResetPresentation(){Clock.Reset();shakeAge=9;pitchChain=0;pitchAt=-99;LastPitch=1;
   if(view!=null)view.transform.localPosition=cameraRest;foreach(var t in targets.Values)ResetTarget(t.face);foreach(var trail in Trails)trail.Clear();
   for(int i=0;i<pool.Length;i++)if(pool[i].view!=null)pool[i].view.gameObject.SetActive(false);if(voices!=null)foreach(var a in voices)if(a!=null)a.Stop();ActiveParticles=ActivePaintMarks=0;
  }
  void OnApplicationFocus(bool focus){if(!focus)ResetPresentation();}void OnApplicationPause(bool pause){if(pause)ResetPresentation();}void OnDisable(){ResetPresentation();}
  void OnDestroy(){if(flashMaterial!=null)Destroy(flashMaterial);foreach(var material in faceMaterials.Values)Destroy(material);foreach(var trail in Trails)trail.Dispose();}
 }
}
