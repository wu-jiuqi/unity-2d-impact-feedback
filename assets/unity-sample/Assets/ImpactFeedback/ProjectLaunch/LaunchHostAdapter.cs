using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ImpactFeedback.ProjectLaunch {
 // Binds original BallBody lifecycle calls to exhibit physics objects. No queue decisions here.
 public sealed class BallBody {
  public int Handle,Id,SessionGeneration;public bool Active;
  public Action<BallBody,Vector2,Vector2> InitializeView;public Action<BallBody,bool> DeactivateView;
  public void Initialize(int id,int generation,Vector2 position,Vector2 velocity,float gravity){Id=id;SessionGeneration=generation;Active=true;InitializeView(this,position,velocity);}
  public void Deactivate(bool fade=false){Active=false;DeactivateView(this,fade);}
 }
 public sealed partial class GameSession {
  readonly RuleState rules=new RuleState();readonly Stack<BallBody> pool=new Stack<BallBody>();readonly Queue<BallBody> reserved=new Queue<BallBody>();readonly Dictionary<int,BallBody> active=new Dictionary<int,BallBody>();readonly HashSet<long> contactsThisStep=new HashSet<long>();
  readonly Func<BallBody> factory;readonly IReadOnlyList<BallBody> all;
  Vector2? queuedShot,bufferedChainShot;Vector2 burstDirection=Vector2.up;float simulationClock,nextSpawnTime,chainNextLaunch;int nextBallId;bool changed;
  public int strategyQueuedRequest,strategyBufferedRequest;public float noDamageTime;public bool collectionRoundPending;
  public int Generation{get;private set;}public int SpawnCount{get;private set;}public int LostCount{get;private set;}public int PhysicsStep{get;private set;}
  public bool IsPlaying=>true;public bool IsPaused{get;private set;}public bool Changed=>changed;
  public int Shots=>rules.Shots;public int ActiveBalls=>rules.ActiveBalls;public int PendingBurstBalls=>rules.PendingBurstBalls;
  public bool HasBufferedShot=>bufferedChainShot.HasValue;public bool HasQueuedShot=>queuedShot.HasValue;public float SimulationClock=>simulationClock;
  public readonly LevelData CurrentLevel=new LevelData();
  public float LaunchLayoutOffset=>IsChainCollection?1.15f:0;
  public Vector2 LauncherPosition=>Round4ReferenceData.Point(296,1087)+Vector2.up*LaunchLayoutOffset;
  public const float MuzzleOffset=105*10.8f/592f;
  // The exhibition has no objectives, tools, shop gates or boss encounter.
  bool IsChainCollection=>true;bool IsCollection=>true;bool CollectionReady=>true;int CollectionTool=>0;
  bool RefillFreezeBalls=>false;bool CollectionVictoryPending=>false;bool CollectionWon=>false;bool CollectionGoalEarned=>false;bool BossBlocksShot=>false;bool IsPixelDrop=>false;bool DropSettling=>false;bool IsAbilityRoute=>false;bool JuiceFrozen=>false;
  bool EarnedChainGoal()=>false;
  public Action<Vector2?> AimChanged;public Action<BallBody> BallSpawned;
  public GameSession(IReadOnlyList<BallBody> objects,Func<BallBody> create){all=objects;factory=create;Reset();}
  public bool TryShoot(Vector2 direction)=>TryShootCore(direction);
  public void SetAim(Vector2? direction){AimChanged?.Invoke(direction);}
  BallBody CreateBall()=>factory();void OnBallSpawned(BallBody ball){BallSpawned?.Invoke(ball);}
  public void SetPaused(bool paused){IsPaused=paused;queuedShot=null;bufferedChainShot=null;SetAim(null);}
  public void Reset(){Generation++;queuedShot=bufferedChainShot=null;reserved.Clear();active.Clear();pool.Clear();rules.Reset(20);rules.UnlimitedShots=true;
   for(int i=all.Count-1;i>=0;i--){all[i].Deactivate();pool.Push(all[i]);}simulationClock=nextSpawnTime=chainNextLaunch=0;SpawnCount=LostCount=PhysicsStep=0;IsPaused=false;}
 }
 // The GUI adapter chooses the exhibit camera and maps its coordinates to project space.
 // Begin/drag/release, minimum drag distance and UI hit testing are generated verbatim above.
 public sealed class InputSession {
  public Func<Vector2,bool> Shoot;public Func<bool> Paused;public Action<Vector2?> Aim;
  public Vector2 LauncherPosition;public bool IsPlaying=>true;public bool IsPaused=>Paused();
  public bool TryShoot(Vector2 direction)=>Shoot(direction);public void SetAim(Vector2? direction)=>Aim(direction);
 }
 public sealed partial class ProjectBattleInput {
  readonly InputSession Session;readonly Func<Vector2,Camera> cameraAt;readonly Func<Camera,Vector2,Vector2> ScreenToProjectWorld;
  readonly List<RaycastResult> uiHits=new List<RaycastResult>();Camera gameCamera;bool aiming;int pointerId=-999;Vector2 pointerStart,aimDirection=Vector2.up;
  public bool nativeTouchReady,busy;public bool playing=true;public object modal,results;public string BattleInputStatus;public int BattleAcceptedShots;
  public ProjectBattleInput(InputSession session,Func<Vector2,Camera> choose,Func<Camera,Vector2,Vector2> convert){Session=session;cameraAt=choose;ScreenToProjectWorld=convert;}
  public void ProcessUnityGUI(){var e=Event.current;if(!aiming&&e!=null)gameCamera=cameraAt(new Vector2(e.mousePosition.x,Screen.height-e.mousePosition.y));OnGUI();}
  public void Cancel(){CancelAim();}
 }
}
