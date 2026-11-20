using UnityEngine;

namespace ImpactFeedback {
 public sealed class ImpactPresenter:MonoBehaviour {
  public ImpactProfile Profile=new ImpactProfile();
  public readonly FeedbackClock Clock=new FeedbackClock();
  public bool AllowEffects=true,ForceSelected,PlayAudio=true;public Effect Selected;
  public int ImpactCount{get;private set;}public float LastPitch{get;private set;}=1;
  public int ActiveParticles{get;private set;}public int PeakParticles{get;private set;}
  SpriteRenderer face;Transform visual,head;Camera view;Vector3 visualRest,cameraRest;
  MaterialPropertyBlock block;Material flashMaterial;float flashAge=9,squashAge=9,shakeAge=9;
  Vector2 incoming;float pitchAt=-99,nextAudio;int pitchChain,voiceCursor;AudioSource[] voices;AudioClip clip;
  readonly Vector3[] path=new Vector3[256];readonly float[] pathTimes=new float[256];int pathCount;float localTime;
  LineRenderer line,glow;Material trailMaterial;System.Random random=new System.Random(79012);
  struct Drop{public SpriteRenderer view;public Vector2 origin,velocity;public float age,life,size,angle;}
  readonly Drop[] pool=new Drop[128];Sprite[] particleSprites;
  public bool Enabled(Effect e)=>AllowEffects&&Profile.Enabled(e)&&(!ForceSelected||Selected==e);
  public void SetVisualRestScale(Vector3 scale){visualRest=scale;visual.localScale=scale;}
  public void SetBakedAlphaCutoff(float cutoff){flashMaterial.SetFloat("_AlphaCutoff",Mathf.Clamp(cutoff,0,.9f));}
  public void Initialize(SpriteRenderer target,Transform movingHead,Camera camera,Sprite[] particles,AudioClip sound,Texture2D yarn){
   face=target;visual=face.transform;visualRest=visual.localScale;head=movingHead;view=camera;cameraRest=view.transform.localPosition;
   flashMaterial=new Material(Shader.Find("ImpactFeedback/SpriteFlash"));face.sharedMaterial=flashMaterial;block=new MaterialPropertyBlock();
   particleSprites=particles;clip=sound;
   voices=new AudioSource[12];for(int i=0;i<voices.Length;i++){voices[i]=gameObject.AddComponent<AudioSource>();voices[i].playOnAwake=false;voices[i].spatialBlend=0;}
   trailMaterial=new Material(Shader.Find("Sprites/Default"));trailMaterial.mainTexture=yarn;
   line=MakeLine("Yarn core",10);glow=MakeLine("Yarn glow",9);
   for(int i=0;i<pool.Length;i++){var o=new GameObject("Pooled fragment "+i);o.transform.SetParent(transform,false);o.layer=face.gameObject.layer;var sr=o.AddComponent<SpriteRenderer>();sr.sprite=particleSprites[i%particleSprites.Length];sr.sortingOrder=15;o.SetActive(false);pool[i].view=sr;}
  }
  LineRenderer MakeLine(string name,int order){var go=new GameObject(name);go.layer=face.gameObject.layer;go.transform.SetParent(transform,false);var l=go.AddComponent<LineRenderer>();l.sharedMaterial=trailMaterial;l.sortingOrder=order;l.useWorldSpace=true;l.numCapVertices=3;l.textureMode=LineTextureMode.Stretch;l.widthCurve=new AnimationCurve(new Keyframe(0,1),new Keyframe(1,0));return l;}
  public void Hit(Vector2 position,Vector2 direction,Color color,ImpactKind kind){
   ImpactCount++;float now=Time.unscaledTime;incoming=direction;
   if(Enabled(Effect.Flash))flashAge=0;
   if(Enabled(Effect.Squash))squashAge=0;
   bool special=kind==ImpactKind.Special;
   if(Enabled(Effect.Shake)&&(special||ForceSelected&&Selected==Effect.Shake))shakeAge=0;
   Clock.Request(now,Profile,Enabled(Effect.HitStop)&&(special||ForceSelected&&Selected==Effect.HitStop),Enabled(Effect.SlowMotion)&&(special||ForceSelected&&Selected==Effect.SlowMotion));
   if(Enabled(Effect.Fragments)&&(kind!=ImpactKind.Hit||ForceSelected&&Selected==Effect.Fragments))Emit(position,direction,color);
   if(now-pitchAt>Profile.pitchResetMs/1000)pitchChain=0;
   LastPitch=Enabled(Effect.Pitch)?Mathf.Min(Profile.pitchMax,1+pitchChain*Profile.pitchStep):1;pitchChain++;pitchAt=now;
   if(PlayAudio&&clip!=null&&now>=nextAudio){var a=voices[voiceCursor++%voices.Length];a.Stop();a.clip=clip;a.pitch=LastPitch;a.volume=Profile.volume;a.Play();nextAudio=now+.025f;}
  }
  public void Step(float dt,float realDelta){
   localTime+=dt;flashAge+=dt;squashAge+=dt;shakeAge+=realDelta;
   block.SetFloat("_Flash",Enabled(Effect.Flash)?Profile.flashStrength*Mathf.Clamp01(1-flashAge/(Profile.flashMs/1000)):0);face.SetPropertyBlock(block);
   float t=Mathf.Clamp01(squashAge/(Profile.squashMs/1000)),wave=t>=1||!Enabled(Effect.Squash)?0:Mathf.Sin(t*Mathf.PI*4)*Mathf.Exp(-5*t)*Profile.squashAmount;
   Vector3 stretch=Mathf.Abs(incoming.y)>=Mathf.Abs(incoming.x)?new Vector3(1+wave,1-wave,1):new Vector3(1-wave,1+wave,1);
   visual.localScale=Vector3.Scale(visualRest,stretch);
   float st=Mathf.Clamp01(shakeAge/(Profile.shakeMs/1000));float amp=Enabled(Effect.Shake)?Profile.shakeHeight*view.orthographicSize*2:0;
   view.transform.localPosition=cameraRest+new Vector3(Mathf.Sin(st*Mathf.PI*8),Mathf.Sin(st*Mathf.PI*11)*.7f,0)*amp*Mathf.Exp(-3*st)*(1-st);
   Trail(dt);Particles(dt);
  }
  void Trail(float dt){
   if(!Enabled(Effect.Trail)){pathCount=0;line.positionCount=glow.positionCount=0;return;}
   int remove=0;while(remove<pathCount&&localTime-pathTimes[remove]>Profile.trailLife)remove++;
   if(remove>0){for(int i=remove;i<pathCount;i++){path[i-remove]=path[i];pathTimes[i-remove]=pathTimes[i];}pathCount-=remove;}
   if(dt>0&&(pathCount==0||(path[pathCount-1]-head.position).sqrMagnitude>.0001f)){
    if(pathCount==path.Length){for(int i=1;i<pathCount;i++){path[i-1]=path[i];pathTimes[i-1]=pathTimes[i];}pathCount--;}
    path[pathCount]=head.position;pathTimes[pathCount++]=localTime;
   }
   if(pathCount==0){line.positionCount=glow.positionCount=0;return;}
   // Remove oldest nodes by geometric arc length; never bridge a reflection corner.
   float length=0;int first=pathCount-1;
   while(first>0){float segment=Vector3.Distance(path[first],path[first-1]);if(length+segment>Profile.trailLength)break;length+=segment;first--;}
   int n=Mathf.Max(0,pathCount-first);line.positionCount=glow.positionCount=n;
   for(int i=0;i<n;i++){var p=path[pathCount-1-i];line.SetPosition(i,p);glow.SetPosition(i,p);}
   line.widthMultiplier=Profile.trailWidth;glow.widthMultiplier=Profile.trailWidth*2.1f;
   line.startColor=new Color(.72f,.95f,1,Profile.trailOpacity);line.endColor=new Color(.72f,.95f,1,0);
   glow.startColor=new Color(.72f,.95f,1,Profile.trailOpacity*.22f);glow.endColor=new Color(.72f,.95f,1,0);
  }
  float R(float min,float max)=>min+(float)random.NextDouble()*(max-min);
  void Emit(Vector2 pos,Vector2 direction,Color color){
   int remaining=Profile.fragmentCount;
   for(int i=0;i<pool.Length&&remaining>0;i++){ref var d=ref pool[i];if(d.view.gameObject.activeSelf)continue;remaining--;
    float a=Mathf.Atan2(-direction.y,-direction.x)+R(-2.5f,2.5f);d.origin=pos;d.velocity=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*Profile.fragmentSpeed*R(.35f,1);
    d.age=0;d.life=Profile.fragmentLife*R(.85f,1.1f);d.size=Profile.fragmentSize*R(.7f,1.5f);d.angle=R(-180,180);d.view.color=color;d.view.transform.position=pos;d.view.transform.localScale=Vector3.one*d.size/d.view.sprite.bounds.size.x;d.view.gameObject.SetActive(true);
   }
  }
  void Particles(float dt){ActiveParticles=0;for(int i=0;i<pool.Length;i++){ref var d=ref pool[i];if(!d.view.gameObject.activeSelf)continue;
   if(!Enabled(Effect.Fragments)){d.view.gameObject.SetActive(false);continue;}d.age+=dt;if(d.age>=d.life){d.view.gameObject.SetActive(false);continue;}
   ActiveParticles++;float travel=Mathf.Min(d.age,.22f);d.view.transform.position=d.origin+d.velocity*(1-Mathf.Exp(-10*travel))/10;
   d.view.transform.rotation=Quaternion.Euler(0,0,d.angle+d.age*90);var c=d.view.color;c.a=.9f*Mathf.Clamp01((d.life-d.age)/.4f);d.view.color=c;
  }PeakParticles=Mathf.Max(PeakParticles,ActiveParticles);}
  public void ResetPresentation(){Clock.Reset();flashAge=squashAge=shakeAge=9;pathCount=0;pitchChain=0;pitchAt=-99;LastPitch=1;
   if(view!=null)view.transform.localPosition=cameraRest;if(visual!=null)visual.localScale=visualRest;
   if(face!=null){block.SetFloat("_Flash",0);face.SetPropertyBlock(block);}if(line!=null)line.positionCount=glow.positionCount=0;
   for(int i=0;i<pool.Length;i++)if(pool[i].view!=null)pool[i].view.gameObject.SetActive(false);
   if(voices!=null)foreach(var a in voices)a.Stop();ActiveParticles=0;
  }
  void OnApplicationFocus(bool focus){if(!focus)ResetPresentation();}
  void OnApplicationPause(bool pause){if(pause)ResetPresentation();}
  void OnDisable(){ResetPresentation();}
  void OnDestroy(){if(flashMaterial!=null)Destroy(flashMaterial);if(trailMaterial!=null)Destroy(trailMaterial);}
 }
}
