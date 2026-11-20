using UnityEngine;
using UnityEngine.SceneManagement;

namespace ImpactFeedback.Gallery {
 public sealed class GalleryLane {
  public readonly Scene Scene;public readonly PhysicsScene2D Physics;public readonly Camera Camera;public readonly ImpactPresenter Presenter;
  public Rigidbody2D Ball{get;private set;}public Transform Brick{get;private set;}public Vector2 InitialVelocity=new Vector2(2.1f,5.5f);
  public ImpactKind Kind=ImpactKind.Special;public bool Auto=true,Sequences=true;public int Colour{get;private set;}
  public SpriteSequence Sequence{get;private set;}public SpriteRenderer Face=>face;
  float accumulator,respawn=-1;int layer;PhysicsMaterial2D material;SpriteRenderer face;BoxCollider2D brickCollider;
  public GalleryLane(string name,int renderLayer,ImpactProfile profile,bool effects){
   layer=renderLayer;Scene=SceneManager.CreateScene(name,new CreateSceneParameters(LocalPhysicsMode.Physics2D));Physics=Scene.GetPhysicsScene2D();
   var root=New(name);Presenter=root.AddComponent<ImpactPresenter>();Presenter.Profile=profile;Presenter.AllowEffects=effects;
   var camera=New("Gallery camera");Camera=camera.AddComponent<Camera>();Camera.orthographic=true;Camera.orthographicSize=3.7f;Camera.transform.position=new Vector3(0,0,-10);Camera.clearFlags=CameraClearFlags.SolidColor;Camera.backgroundColor=new Color32(240,249,243,255);Camera.cullingMask=1<<layer;Camera.depth=2;
   material=new PhysicsMaterial2D("Gallery zero friction"){bounciness=1,friction=0};
   Wall("Left",new Vector2(-2.5f,0),new Vector2(.2f,8));Wall("Right",new Vector2(2.5f,0),new Vector2(.2f,8));Wall("Top",new Vector2(0,3.45f),new Vector2(5,.2f));Wall("Bottom",new Vector2(0,-3.45f),new Vector2(5,.2f));
   Brick=New("Stable brick collider").transform;Brick.position=new Vector3(0,1.4f,0);brickCollider=Brick.gameObject.AddComponent<BoxCollider2D>();brickCollider.size=new Vector2(1.7f,1.7f);brickCollider.sharedMaterial=material;
   var artwork=New("Visual child");artwork.transform.SetParent(Brick,false);face=artwork.AddComponent<SpriteRenderer>();face.sprite=FiveColourBricks.Frames(0)[0];face.sortingOrder=5;artwork.transform.localScale=Vector3.one*FiveColourBricks.Scale(0,1.7f);Sequence=new SpriteSequence(face);
   var ball=New("Ball");Ball=ball.AddComponent<Rigidbody2D>();Ball.gravityScale=0;Ball.linearDamping=Ball.angularDamping=0;Ball.collisionDetectionMode=CollisionDetectionMode2D.Continuous;Ball.freezeRotation=true;
   var circle=ball.AddComponent<CircleCollider2D>();circle.radius=.15f;circle.sharedMaterial=material;var ballArt=New("Ball artwork");ballArt.transform.SetParent(ball.transform,false);var ballImage=ballArt.AddComponent<SpriteRenderer>();ballImage.sprite=GalleryArt.Sprite("ball_yarn_blue");ballImage.sortingOrder=20;ballArt.transform.localScale=Vector3.one*(.38f/ballImage.sprite.bounds.size.x);
   var relay=ball.AddComponent<GalleryCollision>();relay.Lane=this;
   Presenter.Initialize(face,ball.transform,Camera,GalleryArt.Particles(),Resources.Load<AudioClip>("Gallery/hit"),Resources.Load<Texture2D>("Gallery/thread_line_blue"));Presenter.SetBakedAlphaCutoff(.4f);Reset();
  }
  GameObject New(string name){var o=new GameObject(name);o.layer=layer;SceneManager.MoveGameObjectToScene(o,Scene);return o;}
  void Wall(string name,Vector2 p,Vector2 size){var o=New(name);o.transform.position=p;var c=o.AddComponent<BoxCollider2D>();c.size=size;c.sharedMaterial=material;}
  public void SetColour(int color){Colour=Mathf.Clamp(color,0,4);Presenter.ResetPresentation();Presenter.SetVisualRestScale(Vector3.one*FiveColourBricks.Scale(Colour,1.7f));Sequence.Reset(FiveColourBricks.Frames(Colour)[0]);brickCollider.enabled=true;respawn=-1;}
  public void Reset(){accumulator=0;respawn=-1;Sequence.Reset(FiveColourBricks.Frames(Colour)[0]);brickCollider.enabled=true;Presenter.ResetPresentation();Ball.position=new Vector2(-.8f,-2.4f);Ball.rotation=0;Ball.linearVelocity=Auto?InitialVelocity:Vector2.zero;Ball.angularVelocity=0;Ball.WakeUp();Physics2D.SyncTransforms();}
  public void Launch(){Reset();Ball.linearVelocity=InitialVelocity;}
  public void Tick(float now,float delta,bool paused){Presenter.Clock.Tick(now,delta,paused);float dt=Presenter.Clock.Delta;
   accumulator+=Mathf.Min(.1f,dt);int steps=0;
   while(accumulator>=1f/120&&steps++<16){Physics.Simulate(1f/120);accumulator-=1f/120;if(Presenter.Clock.Stopped){accumulator=0;break;}}
   if(Ball.linearVelocity.sqrMagnitude>.01f)Ball.linearVelocity=Ball.linearVelocity.normalized*InitialVelocity.magnitude;
   if(!Sequences&&respawn>=0){respawn=-1;Sequence.Reset(FiveColourBricks.Frames(Colour)[0]);brickCollider.enabled=true;}
   Sequence.Advance(dt);if(respawn>=0){respawn-=dt;if(!Sequence.Playing)face.enabled=false;if(respawn<=0){Sequence.Reset(FiveColourBricks.Frames(Colour)[0]);brickCollider.enabled=true;respawn=-1;}}
   Presenter.Step(dt,paused?0:delta);
  }
  public void Collided(Collision2D collision){if(collision.collider.transform!=Brick)return;var contact=collision.GetContact(0);Presenter.Hit(contact.point,-contact.normal,FiveColourBricks.Tints[Colour],Kind);if(Sequences&&Kind!=ImpactKind.Hit){Sequence.Play(FiveColourBricks.Frames(Colour),25);respawn=.90f;brickCollider.enabled=false;}}
 }
 public sealed class GalleryCollision:MonoBehaviour{public GalleryLane Lane;void OnCollisionEnter2D(Collision2D c){Lane.Collided(c);}}
}
