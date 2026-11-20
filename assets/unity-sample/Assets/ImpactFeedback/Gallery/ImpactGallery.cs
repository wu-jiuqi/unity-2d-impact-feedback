using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using A=ImpactFeedback.Gallery.GalleryArt;

namespace ImpactFeedback.Gallery {
 public sealed class ImpactGallery:MonoBehaviour {
  public static ImpactGallery Instance;public ImpactProfile Profile=ImpactProfile.GalleryDefault();
  public GalleryLane Baseline{get;private set;}public GalleryLane Active{get;private set;}
  public bool Paused{get;private set;}public int Mode{get;private set;}=1;public Effect Selected{get;private set;}
  public static readonly string[] Names={"闪白","拖尾","震屏","顿帧","慢镜","碎片","挤压","升调"};
  static readonly string[] Hints={"命中表面短促混白，保留透明轮廓。","双层毛线描绘球路，保留碰撞折返点。","首下清晰冲击并衰减回弹，HUD与瞄准稳定。","真实暂停演示侧物理；界面和恢复计时继续。","降低演示侧物理时钟倍率，结束后恢复。","原版液滴甩出后落为颜料色斑，固定128槽。","沿受力方向压缩，只改变视觉子层。","连续命中：do re mi fa so la xi 高音do；断连重置。"};
  RectTransform content,parameters,stage;Text status,message,title,description,modeLabel,leftLabel,rightLabel,pauseLabel,eventLabel,sequenceLabel,noteReadout;RectTransform noteControls;Text[] effectLabels=new Text[8],switchLabels=new Text[8],colourLabels=new Text[5];
  int colour;bool sequences=true;
  int screenW,screenH;bool focused=true;ImpactFeedback.ProjectLaunch.ProjectBattleInput projectInput;ImpactKind kind=ImpactKind.Special;
  readonly List<Parameter> rows=new List<Parameter>();
  public string PresetPath=>Path.Combine(Application.persistentDataPath,"impact-profile.json");
  sealed class Parameter{public string name,unit;public float min,max;public bool integer;public Func<ImpactProfile,float> get;public Action<ImpactProfile,float> set;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Instance==null)new GameObject("Impact Feedback Gallery").AddComponent<ImpactGallery>();}
  void Awake(){Instance=this;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;Application.runInBackground=true;
   var background=new GameObject("Background").AddComponent<Camera>();background.clearFlags=CameraClearFlags.SolidColor;background.backgroundColor=new Color32(252,245,228,255);background.cullingMask=0;background.depth=-10;
   Baseline=new GalleryLane("Baseline world",8,Profile,false);Active=new GalleryLane("Feedback world",9,Profile,true);BuildUI();Apply();
   var inputHost=new ImpactFeedback.ProjectLaunch.InputSession{LauncherPosition=Active.Production.LauncherPosition,Paused=()=>Paused,
    Shoot=TryLaunchDirection,Aim=d=>{if(d.HasValue){Baseline.SetAim(d.Value);Active.SetAim(d.Value);}else{Baseline.Production.SetAim(null);Active.Production.SetAim(null);}}};
   projectInput=new ImpactFeedback.ProjectLaunch.ProjectBattleInput(inputHost,ChooseCamera,ScreenToProjectWorld);
   if(Array.IndexOf(Environment.GetCommandLineArgs(),"--gallery-qa")>=0)gameObject.AddComponent<GalleryQA>();
  }
  void BuildUI(){
   var canvas=new GameObject("Gallery interface",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));canvas.layer=5;var c=canvas.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=100;
   if(FindFirstObjectByType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
   content=A.Rect(canvas.transform,"1600 x 900 content",0,0,1600,900);content.anchorMin=content.anchorMax=Vector2.one*.5f;content.pivot=Vector2.one*.5f;content.anchoredPosition=Vector2.zero;
   A.Image(content,"decor_cat_head",30,22,70,66);A.Label(content,"碰撞反馈实验室",112,20,540,55,36);A.Label(content,"IMPACT FEEDBACK  /  8 种手感，边玩边调",114,73,660,30,18,A.Muted);
   A.Label(content,"Unity 2D · Windows · v0.2.1",1190,42,370,35,18,A.Muted);
   for(int i=0;i<5;i++){int k=i;colourLabels[i]=A.Button(content,FiveColourBricks.Names[i],782+i*76,76,70,38,()=>SetColour(k)).GetComponentInChildren<Text>();}
   A.Image(content,"card_cream_help",24,126,248,720,true);A.Label(content,"效果目录",46,145,210,38,25);
   for(int i=0;i<8;i++){int k=i;var b=A.Button(content,Names[i],42,197+i*61,164,52,()=>Select((Effect)k));effectLabels[i]=b.GetComponentInChildren<Text>();
    var toggle=A.Button(content,"开",209,197+i*61,49,52,()=>{Profile.Enable((Effect)k,!Profile.Enabled((Effect)k));Apply();});switchLabels[i]=toggle.GetComponentInChildren<Text>();}
   A.Label(content,"选效果看参数；右侧开关启停。",42,705,213,60,17,A.Muted);
   A.Button(content,"项目参考预设",42,774,214,48,()=>SetProfile(new ImpactProfile()));
   A.Button(content,"单项体验",296,124,146,48,()=>SetMode(0));A.Button(content,"开关对照",450,124,146,48,()=>SetMode(1),true);A.Button(content,"组合体验",604,124,146,48,()=>SetMode(2));
   modeLabel=A.Label(content,"",776,132,380,34,19,A.Muted);
   stage=A.Rect(content,"Stage viewport",296,222,860,420);
   leftLabel=A.Label(content,"关闭反馈",314,184,385,33,22);rightLabel=A.Label(content,"开启反馈",750,184,385,33,22);
   A.Label(content,"手动5球 · 3行×5列 · 消失即补齐",310,649,630,30,18,A.Muted);
   sequenceLabel=A.Button(content,"序列帧：开",972,648,184,38,()=>{sequences=!sequences;Apply();}).GetComponentInChildren<Text>();
   A.Button(content,"发射5球 / 空格",296,697,175,56,Launch,true);
   A.Label(content,"每组5球\n间隔0.10秒",492,697,150,56,18,A.Muted);
   pauseLabel=A.Button(content,"暂停",651,697,132,56,()=>SetPaused(!Paused)).GetComponentInChildren<Text>();
   A.Button(content,"重置场景",793,697,170,56,ResetScene);
   eventLabel=A.Button(content,"特殊爆发",973,697,183,56,()=>{kind=(ImpactKind)(((int)kind+1)%3);Apply();}).GetComponentInChildren<Text>();
   status=A.Label(content,"",300,760,850,70,16);message=A.Label(content,"沿用游戏内拖动瞄准、松手发射；支持连发与一次输入缓存。",300,830,850,42,18,A.Muted);
   A.Image(content,"card_cream_help",1176,126,400,720,true);
   title=A.Label(content,"",1200,144,350,40,27);description=A.Label(content,"",1200,191,350,42,17,A.Muted);description.name="Effect explanation";parameters=A.Rect(content,"Parameter rows",1200,238,350,420);
   noteControls=A.Rect(content,"Scale controls",1195,632,360,38);noteReadout=A.Label(noteControls,"",0,0,175,38,18);A.Button(noteControls,"重置到do",185,0,170,38,()=>Active.Presenter.ResetNotes());
   A.Button(content,"恢复单项",1195,680,170,48,ResetSelected);A.Button(content,"教学强化",1377,680,170,48,()=>SetProfile(ImpactProfile.Teaching()));
   A.Button(content,"保存预设",1195,741,170,48,Save);A.Button(content,"载入预设",1377,741,170,48,Load);
   A.Label(content,"配置使用 JSON，可随 skill 带回项目。",1200,800,350,28,16,A.Muted);
   RefreshParameters();Resize();
  }
  public void Select(Effect e){Active.Presenter.ResetNotes();Selected=e;RefreshParameters();Apply();}
  public void SetMode(int mode){Mode=mode;Apply();Resize();ResetScene();}
  void Apply(){Profile.Clamp();Baseline.Presenter.Profile=Active.Presenter.Profile=Profile;
   Active.Presenter.Selected=Selected;Active.Presenter.ForceSelected=Mode!=2;Active.Kind=Baseline.Kind=kind;
   Baseline.Sequences=Active.Sequences=sequences;sequenceLabel.text="序列帧："+(sequences?"开":"关");for(int i=0;i<5;i++)colourLabels[i].text=(i==colour?"●":"")+FiveColourBricks.Names[i];
   // The baseline stays silent; fixed-pitch audio remains audible on the feedback side.
   Baseline.Presenter.AllowEffects=false;Baseline.Presenter.PlayAudio=false;
   for(int i=0;i<8;i++){effectLabels[i].text=((int)Selected==i?"● ":"")+Names[i];switchLabels[i].text=Profile.Enabled((Effect)i)?"开":"关";switchLabels[i].color=Profile.Enabled((Effect)i)?A.Mint:A.Muted;}
   eventLabel.text=kind==ImpactKind.Hit?"普通命中":kind==ImpactKind.Break?"破砖喷散":"特殊爆发";
   modeLabel.text=Mode==2?"八项自由组合":"当前单项："+Names[(int)Selected];
   leftLabel.text=Mode==1?"关闭反馈 · 相同初始条件":"";rightLabel.text=Mode==1?"开启反馈 · "+Names[(int)Selected]:Mode==2?"组合体验":"单项体验 · "+Names[(int)Selected];
  }
  void Add(string name,string unit,float min,float max,Func<ImpactProfile,float> get,Action<ImpactProfile,float> set,bool integer=false){rows.Add(new Parameter{name=name,unit=unit,min=min,max=max,get=get,set=set,integer=integer});}
  void RefreshParameters(){
   foreach(Transform child in parameters){child.gameObject.SetActive(false);Destroy(child.gameObject);}rows.Clear();title.text=Names[(int)Selected]+" · 实时调参";
   description.text=Hints[(int)Selected];noteControls.gameObject.SetActive(Selected==Effect.Pitch);
   switch(Selected){
    case Effect.Flash:Add("混白强度","",0,1,p=>p.flashStrength,(p,v)=>p.flashStrength=v);Add("持续时间","ms",5,400,p=>p.flashMs,(p,v)=>p.flashMs=v,true);break;
    case Effect.Trail:Add("最大长度","单位",.1f,5,p=>p.trailLength,(p,v)=>p.trailLength=v);Add("线芯宽度","单位",.02f,.6f,p=>p.trailWidth,(p,v)=>p.trailWidth=v);Add("保留寿命","s",.03f,1,p=>p.trailLife,(p,v)=>p.trailLife=v);Add("透明度","",0,1,p=>p.trailOpacity,(p,v)=>p.trailOpacity=v);break;
    case Effect.Shake:Add("视口高度比例","%",0,4,p=>p.shakeHeight*100,(p,v)=>p.shakeHeight=v/100);Add("持续时间","ms",40,800,p=>p.shakeMs,(p,v)=>p.shakeMs=v,true);break;
    case Effect.HitStop:Add("顿帧时长","ms",0,250,p=>p.hitStopMs,(p,v)=>p.hitStopMs=v,true);Add("恢复后的冷却","ms",0,1500,p=>p.hitStopCooldownMs,(p,v)=>p.hitStopCooldownMs=v,true);Add("每秒暂停预算","ms",0,500,p=>p.hitStopBudgetMs,(p,v)=>p.hitStopBudgetMs=v,true);break;
    case Effect.SlowMotion:Add("时间倍率","倍",.05f,1,p=>p.slowScale,(p,v)=>p.slowScale=v);Add("持续时间","ms",0,1500,p=>p.slowMs,(p,v)=>p.slowMs=v,true);break;
    case Effect.Fragments:Add("单次数量","个",0,64,p=>p.fragmentCount,(p,v)=>p.fragmentCount=Mathf.RoundToInt(v),true);Add("液滴飞行速度","",.1f,12,p=>p.fragmentSpeed,(p,v)=>p.fragmentSpeed=v);Add("色斑大小","比例",.03f,.5f,p=>p.fragmentSize,(p,v)=>p.fragmentSize=v);Add("存在寿命","s",.1f,3,p=>p.fragmentLife,(p,v)=>p.fragmentLife=v);break;
    case Effect.Squash:Add("挤压幅度","",0,.45f,p=>p.squashAmount,(p,v)=>p.squashAmount=v);Add("回弹时长","ms",50,1000,p=>p.squashMs,(p,v)=>p.squashMs=v,true);break;
    case Effect.Pitch:Add("最高音阶","级",1,8,p=>p.scaleNotes,(p,v)=>p.scaleNotes=Mathf.RoundToInt(v),true);Add("音符最小间隔","ms",30,250,p=>p.noteIntervalMs,(p,v)=>p.noteIntervalMs=v,true);Add("断连重置","ms",200,5000,p=>p.pitchResetMs,(p,v)=>p.pitchResetMs=v,true);Add("音阶音量","",0,1,p=>p.noteVolume,(p,v)=>p.noteVolume=v);break;
   }
   Add("演示音量","",0,1,p=>p.volume,(p,v)=>p.volume=v);
   for(int i=0;i<rows.Count;i++)ParameterRow(rows[i],i*74);
  }
  void ParameterRow(Parameter p,float y){
   A.Label(parameters,p.name,0,y,225,28,19);var fieldRoot=A.Rect(parameters,"Value "+p.name,238,y,110,30);var bg=fieldRoot.gameObject.AddComponent<Image>();bg.color=new Color(1,1,1,.8f);
   var valueText=A.Label(fieldRoot,"",5,0,100,30,18);valueText.alignment=TextAnchor.MiddleRight;
   var field=fieldRoot.gameObject.AddComponent<InputField>();field.textComponent=valueText;field.contentType=InputField.ContentType.DecimalNumber;
   string Format(float v)=>v.ToString(p.integer?"0":"0.###",CultureInfo.InvariantCulture);
   field.text=Format(p.get(Profile));
   var track=A.Rect(parameters,"Slider "+p.name,4,y+38,294,22);var back=track.gameObject.AddComponent<Image>();back.color=new Color(.79f,.88f,.82f,1);
   var slider=track.gameObject.AddComponent<Slider>();slider.minValue=p.min;slider.maxValue=p.max;slider.wholeNumbers=p.integer;slider.direction=Slider.Direction.LeftToRight;
   var handleArea=A.Rect(track,"Handle area",9,0,276,22);var handle=A.Image(handleArea,"toggle_knob",0,-3,28,28);handle.raycastTarget=true;handle.rectTransform.pivot=Vector2.one*.5f;
   slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.SetValueWithoutNotify(p.get(Profile));
   A.Label(parameters,p.unit,306,y+32,44,30,15,A.Muted);
   slider.onValueChanged.AddListener(v=>{p.set(Profile,v);Profile.Clamp();field.SetTextWithoutNotify(Format(p.get(Profile)));});
   field.onEndEdit.AddListener(s=>{if(float.TryParse(s,NumberStyles.Float,CultureInfo.InvariantCulture,out float v)&&!float.IsNaN(v)&&!float.IsInfinity(v)){v=Mathf.Clamp(v,p.min,p.max);p.set(Profile,p.integer?Mathf.Round(v):v);Profile.Clamp();slider.SetValueWithoutNotify(p.get(Profile));}field.SetTextWithoutNotify(Format(p.get(Profile)));});
  }
  public void SetProfile(ImpactProfile p){Profile=p;Profile.Clamp();Active.Presenter.ResetPresentation();Baseline.Presenter.ResetPresentation();RefreshParameters();Apply();}
  public void ResetSelected(){var d=new ImpactProfile();foreach(var r in rows)r.set(Profile,r.get(d));Profile.Enable(Selected,true);RefreshParameters();Apply();}
  public void SetColour(int value){colour=Mathf.Clamp(value,0,4);Baseline.SetColour(colour);Active.SetColour(colour);Apply();ResetScene();}
  public void SetPaused(bool paused){projectInput?.Cancel();Paused=paused;Baseline.Production.SetPaused(paused);Active.Production.SetPaused(paused);if(paused){Baseline.Presenter.ResetPresentation();Active.Presenter.ResetPresentation();}pauseLabel.text=paused?"继续":"暂停";}
  public void Launch(){TryLaunchDirection(Active.InitialVelocity);}
  bool TryLaunchDirection(Vector2 direction){if(Paused)return false;Baseline.SetAim(direction);Active.SetAim(direction);bool b=Baseline.Launch(),a=Active.Launch();message.text=a&&b?"项目原发射：每组5球 · 支持飞行中续发与一次松手缓存":"项目原发射状态暂不接受额外输入";return a&&b;}
  Camera ChooseCamera(Vector2 p){if(Active.Camera.pixelRect.Contains(p))return Active.Camera;if(Baseline.Camera.enabled&&Baseline.Camera.pixelRect.Contains(p))return Baseline.Camera;return null;}
  Vector2 ScreenToProjectWorld(Camera camera,Vector2 p){var lane=camera==Active.Camera?Active:Baseline;Vector2 world=camera.ScreenToWorldPoint(p);world-=(Vector2)lane.Presenter.CameraOffset;return lane.ToProject(world);}
  void OnGUI(){projectInput?.ProcessUnityGUI();}
  public void ResetScene(){projectInput?.Cancel();SetPaused(false);Baseline.Reset();Active.Reset();}
  public void Save(){try{Profile.Clamp();var json=JsonUtility.ToJson(Profile,true);File.WriteAllText(PresetPath+".tmp",json);if(File.Exists(PresetPath))File.Replace(PresetPath+".tmp",PresetPath,null);else File.Move(PresetPath+".tmp",PresetPath);message.text="已保存："+PresetPath;}catch(Exception e){message.text="保存失败："+e.Message;}}
  public void Load(){try{SetProfile(ImpactProfile.FromJson(File.ReadAllText(PresetPath)));message.text="已载入自定义预设";}catch(Exception e){message.text="载入失败："+e.Message;}}
  void Resize(){screenW=Screen.width;screenH=Screen.height;float scale=Mathf.Min(screenW/1600f,screenH/900f);content.localScale=Vector3.one*scale;
   var corners=new Vector3[4];stage.GetWorldCorners(corners);float x=corners[0].x/screenW,y=corners[0].y/screenH,w=(corners[2].x-corners[0].x)/screenW,h=(corners[2].y-corners[0].y)/screenH;
   Baseline.Camera.enabled=Mode==1;Baseline.Camera.rect=new Rect(x,y,w*.48f,h);Active.Camera.rect=Mode==1?new Rect(x+w*.52f,y,w*.48f,h):new Rect(x+w*.18f,y,w*.64f,h);
  }
  void Update(){if(content==null)return;if(Screen.width!=screenW||Screen.height!=screenH)Resize();float dt=Mathf.Min(.1f,Time.unscaledDeltaTime);bool paused=Paused||!focused;
   if(GetComponent<GalleryQA>()!=null)paused=Paused; // Headless QA is allowed to advance without desktop focus.
   Baseline.Tick(Time.unscaledTime,dt,paused);Active.Tick(Time.unscaledTime,dt,paused);
   bool editing=EventSystem.current!=null&&EventSystem.current.currentSelectedGameObject!=null&&EventSystem.current.currentSelectedGameObject.GetComponent<InputField>()!=null;
   if(!editing&&Input.GetKeyDown(KeyCode.Space))Launch();if(!editing&&Input.GetKeyDown(KeyCode.P))SetPaused(!Paused);
   noteReadout.text="当前："+Active.Presenter.LastNote+"  "+(Active.Presenter.LastNoteIndex+1)+"/8";var clock=Active.Presenter.Clock;status.text="本组已发射 "+Active.SpawnedThisVolley+"/5 · 飞行 "+Active.ActiveBalls+" · 缓存 "+(Active.Production.HasBufferedShot?1:0)+" · 补齐 "+Active.Refills+" · 命中 "+Active.Presenter.ImpactCount+" · 音符 "+Active.Presenter.LastNote+"\n顿帧 "+Profile.hitStopMs.ToString("0")+" / 实际 "+clock.ActualStopMs.ToString("0.0")+" ms · 时间倍率 "+clock.Scale.ToString("0.00")+"× · 震屏峰值 "+Active.Presenter.PeakShakePixels.ToString("0.0")+" px\n液滴/色斑 "+Active.Presenter.ActiveParticles+"/128 · 已落地 "+Active.Presenter.LandedParticles;

  }
  void OnApplicationFocus(bool f){projectInput?.Cancel();focused=f;if(!f){Baseline?.Presenter.ResetPresentation();Active?.Presenter.ResetPresentation();}}
  void OnApplicationPause(bool p){if(p){Baseline?.Presenter.ResetPresentation();Active?.Presenter.ResetPresentation();}}
  void OnDestroy(){if(Instance==this)Instance=null;}
 }
}
