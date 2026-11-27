using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LaunchSession=ImpactFeedback.ProjectLaunch.GameSession;
using LaunchBall=ImpactFeedback.ProjectLaunch.BallBody;
namespace ImpactFeedback.Gallery {
 public sealed class GalleryBrick {public Transform Root;public SpriteRenderer Face;public BoxCollider2D Collider;public SpriteSequence Sequence;public int Color;public bool Breaking;}
 public sealed class GalleryLane {
  public const int Rows=3,Columns=5,VolleySize=5;public const float BrickWidth=.90f,WorldScale=.6f;
  public static readonly Vector2 LaunchOrigin=new Vector2(0,-2.75f);
  public static Vector2 ViewLauncher=>LaunchOrigin-Vector2.up*(LaunchSession.MuzzleOffset*WorldScale);
  public readonly Scene Scene;public readonly PhysicsScene2D Physics;public readonly Camera Camera;public readonly ImpactPresenter Presenter;
  public readonly GalleryBrick[] Bricks=new GalleryBrick[Rows*Columns];public readonly List<Rigidbody2D> Balls=new List<Rigidbody2D>();
  public readonly List<LaunchBall> Bindings=new List<LaunchBall>();readonly List<SpriteRenderer> ballFaces=new List<SpriteRenderer>();readonly List<float> born=new List<float>();
  public LaunchSession Production{get;private set;}
  public Rigidbody2D Ball=>Balls[0];public Transform Brick=>Bricks[0].Root;public SpriteRenderer Face=>Bricks[0].Face;public SpriteSequence Sequence=>Bricks[0].Sequence;
  public Vector2 InitialVelocity=Vector2.up;public ImpactKind Kind=ImpactKind.Special;public bool Sequences=true;
  public int Colour{get;private set;}public int Refills{get;private set;}
  public int SpawnedThisVolley=>Production.SpawnCount>0?VolleySize-Production.PendingBurstBalls:0;
  public int ActiveBalls=>Production.ActiveBalls;public bool CanLaunch=>Production.ChainCanLaunch;public bool VolleyInProgress=>ActiveBalls>0||Production.PendingBurstBalls>0;
  float accumulator;int layer;PhysicsMaterial2D material;LineRenderer aim;Material aimMaterial;
  public GalleryLane(string name,int renderLayer,ImpactProfile profile,bool effects){
   layer=renderLayer;Scene=SceneManager.CreateScene(name,new CreateSceneParameters(LocalPhysicsMode.Physics2D));Physics=Scene.GetPhysicsScene2D();
   var root=New(name);Presenter=root.AddComponent<ImpactPresenter>();Presenter.Profile=profile;Presenter.AllowEffects=effects;
   Camera=New("Gallery camera").AddComponent<Camera>();Camera.orthographic=true;Camera.orthographicSize=3.7f;Camera.transform.position=new Vector3(0,0,-10);Camera.clearFlags=CameraClearFlags.SolidColor;Camera.backgroundColor=new Color32(240,249,243,255);Camera.cullingMask=1<<layer;Camera.depth=2;
   material=new PhysicsMaterial2D("Gallery zero friction"){bounciness=1,friction=0};Wall("Left",new Vector2(-3.05f,0),new Vector2(.12f,8));Wall("Right",new Vector2(3.05f,0),new Vector2(.12f,8));Wall("Top",new Vector2(0,3.4f),new Vector2(6.2f,.12f));
   for(int r=0;r<Rows;r++)for(int c=0;c<Columns;c++){
    int i=r*Columns+c;var b=new GalleryBrick();Bricks[i]=b;b.Color=c;b.Root=New("Brick "+r+","+c).transform;b.Root.position=new Vector2((c-2)*1.07f,2.60f-r*1.05f);
    b.Collider=b.Root.gameObject.AddComponent<BoxCollider2D>();b.Collider.size=Vector2.one*BrickWidth;b.Collider.sharedMaterial=material;
    var artwork=New("Five color face");artwork.transform.SetParent(b.Root,false);b.Face=artwork.AddComponent<SpriteRenderer>();b.Face.sprite=FiveColourBricks.Frames(b.Color)[0];b.Face.sortingOrder=30;artwork.transform.localScale=Vector3.one*FiveColourBricks.Scale(b.Color,BrickWidth);b.Sequence=new SpriteSequence(b.Face);
   }
   for(int i=0;i<VolleySize;i++)CreateBallBinding();var yarn=Resources.Load<Texture2D>("Gallery/thread_line_blue");Presenter.Initialize(Face,Ball.transform,Camera,GalleryArt.Particles(),Resources.Load<AudioClip>("Gallery/hit"),yarn);Presenter.SetBakedAlphaCutoff(.4f);Presenter.PetalLandingBounds=new Rect(-2.75f,-2.8f,5.5f,5.8f);
   for(int i=1;i<Bricks.Length;i++)Presenter.AddTarget(Bricks[i].Face);for(int i=1;i<VolleySize;i++)Presenter.AddTrail(Balls[i].transform,yarn);
   aim=New("Project aim view").AddComponent<LineRenderer>();aimMaterial=new Material(Shader.Find("Sprites/Default"));aim.sharedMaterial=aimMaterial;aim.useWorldSpace=true;aim.sortingOrder=10;aim.widthMultiplier=.025f;aim.startColor=new Color(.35f,.68f,.57f,.6f);aim.endColor=new Color(.35f,.68f,.57f,.1f);aim.positionCount=2;
   Production=new LaunchSession(Bindings,CreateBallBinding);Production.AimChanged=RenderAim;Reset();
  }
  GameObject New(string name){var o=new GameObject(name);o.layer=layer;SceneManager.MoveGameObjectToScene(o,Scene);return o;}
  void Wall(string name,Vector2 p,Vector2 size){var o=New(name);o.transform.position=p;var c=o.AddComponent<BoxCollider2D>();c.size=size;c.sharedMaterial=material;}
  LaunchBall CreateBallBinding(){
   int i=Balls.Count;var ball=New("Project pooled ball "+i);var body=ball.AddComponent<Rigidbody2D>();Balls.Add(body);body.gravityScale=0;body.linearDamping=body.angularDamping=0;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;body.freezeRotation=true;body.simulated=false;
   var circle=ball.AddComponent<CircleCollider2D>();circle.radius=.11f;circle.sharedMaterial=material;
   var art=New("Project yarn ball");art.transform.SetParent(ball.transform,false);var sr=art.AddComponent<SpriteRenderer>();ballFaces.Add(sr);born.Add(0);sr.sprite=GalleryArt.Sprite("ball_yarn_blue");sr.sortingOrder=85;art.transform.localScale=Vector3.one*(.30f/sr.sprite.bounds.size.x);
   var relay=ball.AddComponent<GalleryCollision>();relay.Lane=this;relay.Index=i;
   for(int j=0;j<i;j++)Physics2D.IgnoreCollision(circle,Balls[j].GetComponent<Collider2D>());
   var binding=new LaunchBall{Handle=i,InitializeView=InitializeBallView,DeactivateView=DeactivateBallView};Bindings.Add(binding);
   if(Presenter.Trails.Count>0)Presenter.AddTrail(ball.transform,Resources.Load<Texture2D>("Gallery/thread_line_blue"));return binding;
  }
  public Vector2 ToView(Vector2 projectPosition)=>(projectPosition-Production.LauncherPosition)*WorldScale+ViewLauncher;
  public Vector2 ToProject(Vector2 viewPosition)=>(viewPosition-ViewLauncher)/WorldScale+Production.LauncherPosition;
  void InitializeBallView(LaunchBall binding,Vector2 position,Vector2 velocity){
   int i=binding.Handle;for(int j=0;j<Bindings.Count;j++)if(!Bindings[j].Active)ballFaces[j].enabled=false;
   var b=Balls[i];var p=ToView(position);b.position=p;b.transform.position=p;b.rotation=0;b.simulated=true;b.linearVelocity=velocity*WorldScale;b.WakeUp();ballFaces[i].enabled=true;born[i]=Production.SimulationClock;Presenter.Trails[i].Start(p);
  }
  void DeactivateBallView(LaunchBall binding,bool fade){int i=binding.Handle;Balls[i].linearVelocity=Vector2.zero;Balls[i].simulated=false;ballFaces[i].enabled=false;if(i<Presenter.Trails.Count){if(fade)Presenter.Trails[i].Stop();else Presenter.Trails[i].Clear();}}
  public void SetColour(int color){Colour=Mathf.Clamp(color,0,4);for(int i=0;i<Bricks.Length;i++){var b=Bricks[i];b.Color=(i%Columns+Colour)%5;Presenter.SetVisualRestScale(b.Face,Vector3.one*FiveColourBricks.Scale(b.Color,BrickWidth));Restore(b,false);}}
  void Restore(GalleryBrick b,bool refill){b.Breaking=false;b.Sequence.Reset(FiveColourBricks.Frames(b.Color)[0]);b.Collider.enabled=true;Presenter.ResetTarget(b.Face);if(refill)Refills++;}
  public void Reset(){accumulator=0;Production.Reset();Presenter.ResetPresentation();for(int i=0;i<Bricks.Length;i++)Restore(Bricks[i],false);ArrangeLoaded();SetAim(InitialVelocity);Physics2D.SyncTransforms();}
  void ArrangeLoaded(){for(int i=0;i<VolleySize;i++){var p=LaunchOrigin+new Vector2((i-2)*.25f,-.24f);Balls[i].position=p;Balls[i].transform.position=p;ballFaces[i].enabled=true;}}
  public void SetAim(Vector2 direction){Production.SetAim(direction);}
  void RenderAim(Vector2? direction){if(!direction.HasValue){aim.enabled=false;return;}var d=LaunchSession.NormalizeAimDirection(direction.Value);InitialVelocity=d*Production.CurrentLevel.ballSpeed*WorldScale;var p=ToView(Production.SpawnPosition(d));aim.SetPosition(0,p);aim.SetPosition(1,p+d*4);aim.enabled=true;}
  public bool Launch()=>Production.TryShoot(InitialVelocity);
  public void Tick(float now,float delta,bool paused){if(Production.IsPaused!=paused)Production.SetPaused(paused);Presenter.Clock.Tick(now,delta,paused);float dt=Presenter.Clock.Delta;accumulator+=Mathf.Min(.1f,dt);int steps=0;
   while(accumulator>=Time.fixedDeltaTime&&steps++<16){Production.FixedStep(Time.fixedDeltaTime);Physics.Simulate(Time.fixedDeltaTime);accumulator-=Time.fixedDeltaTime;
    for(int i=0;i<Bindings.Count;i++)if(Bindings[i].Active){Presenter.SampleTrail(i,Balls[i].position);if(Balls[i].linearVelocity.sqrMagnitude>.001f)Balls[i].linearVelocity=Balls[i].linearVelocity.normalized*(Production.CurrentLevel.ballSpeed*WorldScale);
     if(Balls[i].position.y<-3.0f&&Balls[i].linearVelocity.y<0||Production.SimulationClock-born[i]>10)Production.LoseBall(Bindings[i],ToProject(Balls[i].position));}
    if(Presenter.Clock.Stopped){accumulator=0;break;}
   }
   for(int i=0;i<Bricks.Length;i++){var b=Bricks[i];if(!Sequences&&b.Breaking){Restore(b,true);continue;}b.Sequence.Advance(dt);if(b.Breaking&&!b.Sequence.Playing)Restore(b,true);}
   Presenter.Step(dt,paused?0:delta);
  }
  public void Collided(int ballIndex,Collision2D collision){Presenter.SampleTrail(ballIndex,Balls[ballIndex].position);
   for(int i=0;i<Bricks.Length;i++)if(collision.collider==Bricks[i].Collider){var b=Bricks[i];var contact=collision.GetContact(0);Presenter.Hit(contact.point,-contact.normal,FiveColourBricks.Tints[b.Color],Kind,b.Face,BrickWidth);
    if(Kind!=ImpactKind.Hit){if(Sequences){b.Sequence.Play(FiveColourBricks.Frames(b.Color),25);b.Breaking=true;b.Collider.enabled=false;}else Restore(b,true);}return;}
  }
 }
 public sealed class GalleryCollision:MonoBehaviour{public GalleryLane Lane;public int Index;void OnCollisionEnter2D(Collision2D c){Lane.Collided(Index,c);}}
}
