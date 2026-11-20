using UnityEngine;

namespace ImpactFeedback {
 // Project FlightTrail algorithm: fixed-step samples preserve the actual reflection vertices.
 public sealed class YarnFlightTrail {
  readonly Vector3[] points=new Vector3[256];readonly float[] times=new float[256];int count;float clock;
  readonly LineRenderer line,glow;readonly Transform head;readonly Material material;
  public int Count=>count;public bool Emitting{get;private set;}
  public string ShaderName=>material.shader.name;
  public YarnFlightTrail(Transform root,Transform movingHead,Texture2D yarn,int layer){
   head=movingHead;material=new Material(Shader.Find("ImpactFeedback/YarnTrail")){mainTexture=yarn};
   line=Make(root,"Project yarn core",layer,44);glow=Make(root,"Project yarn outer glow",layer,43);
   var block=new MaterialPropertyBlock();block.SetFloat("_Glow",1);glow.SetPropertyBlock(block);
  }
  LineRenderer Make(Transform root,string name,int layer,int order){var go=new GameObject(name);go.layer=layer;go.transform.SetParent(root,false);var l=go.AddComponent<LineRenderer>();l.sharedMaterial=material;l.useWorldSpace=true;l.sortingOrder=order;l.numCornerVertices=0;l.numCapVertices=3;l.textureMode=LineTextureMode.Stretch;l.widthCurve=new AnimationCurve(new Keyframe(0,.02f),new Keyframe(.30f,.45f),new Keyframe(1,1));return l;}
  void DropFirst(){for(int i=1;i<count;i++){points[i-1]=points[i];times[i-1]=times[i];}count--;}
  public void Sample(Vector2 p){if(!Emitting)return;if(count>0&&Vector2.Distance(points[count-1],p)<.002f)return;if(count==points.Length)DropFirst();points[count]=p;times[count++]=clock;}
  public void Start(Vector2 p){Clear();Emitting=true;Sample(p);}
  public void Stop(){Emitting=false;}
  public void Clear(){count=0;clock=0;Emitting=false;if(line!=null)line.positionCount=0;if(glow!=null)glow.positionCount=0;}
  public void Step(float dt,ImpactProfile p,bool enabled){
   clock+=dt;if(!enabled){Clear();return;}
   while(count>0&&clock-times[0]>p.trailLife)DropFirst();
   float length=0;for(int i=count-1;i>0;i--){float segment=Vector3.Distance(points[i],points[i-1]);if(length+segment>p.trailLength){float keep=Mathf.Max(0,p.trailLength-length);points[i-1]=Vector3.Lerp(points[i],points[i-1],segment>0?keep/segment:0);while(i>1){DropFirst();i--;}break;}length+=segment;}
   line.widthMultiplier=p.trailWidth;glow.widthMultiplier=p.trailWidth*2.1f;
   float fade=count>0?Mathf.Clamp01(1-(clock-times[count-1])/p.trailLife):0;
   line.endColor=new Color(1,1,1,fade*p.trailOpacity);line.startColor=new Color(1,1,1,0);glow.endColor=new Color(1,1,1,fade*.22f);glow.startColor=new Color(1,1,1,0);
   line.positionCount=glow.positionCount=count;
   for(int i=0;i<count;i++){line.SetPosition(i,points[i]);glow.SetPosition(i,points[i]);}
   if(Emitting&&count>0){line.SetPosition(count-1,head.position);glow.SetPosition(count-1,head.position);}
  }
  public void Dispose(){Object.Destroy(material);}
 }
}
