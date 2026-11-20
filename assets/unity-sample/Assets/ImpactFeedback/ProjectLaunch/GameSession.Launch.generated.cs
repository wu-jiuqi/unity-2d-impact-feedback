// Generated from BounceBusters production code. See references/project-launch-source.json.
using System;using System.Collections.Generic;using UnityEngine;
namespace ImpactFeedback.ProjectLaunch { public sealed partial class GameSession {
bool TryShootCore(Vector2 direction)
        {
            if(RefillFreezeBalls)return false;
            if(IsCollection&&(CollectionVictoryPending||CollectionWon||EarnedChainGoal()))return false;
            // Accept one deliberate release during the previous volley/cooldown.
            // Charge it only when promoted; pause/navigation discard it.
            if(IsChainCollection&&IsPlaying&&!IsPaused&&Shots>0&&!CollectionWon&&!queuedShot.HasValue&&direction.sqrMagnitude>=.001f&&direction.y>0){
                if(bufferedChainShot.HasValue)return false;
                if(!ChainCanLaunch)return BufferChainShot(direction);
            }
            if(!IsPlaying||IsPaused||(IsCollection&&(!CollectionReady||CollectionTool!=0))||BossBlocksShot||(IsPixelDrop&&(ActiveBalls>0||DropSettling))||queuedShot.HasValue||!rules.CanShoot||(IsAbilityRoute&&ActiveBalls>0)||direction.sqrMagnitude<.001f||direction.y<=0)return false;
            direction=NormalizeAimDirection(direction);
            queuedShot=direction;SetAim(null);return true;
        }
public static Vector2 NormalizeAimDirection(Vector2 direction)
        {
            float angle=Mathf.Clamp(Mathf.Atan2(direction.y,direction.x),12*Mathf.Deg2Rad,168*Mathf.Deg2Rad);
            return new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
        }
void ReturnReserved()
        {
            while(reserved.Count>0){var ball=reserved.Dequeue();ball.Deactivate();pool.Push(ball);}
        }
bool BufferChainShot(Vector2 direction){if(bufferedChainShot.HasValue)return false;bufferedChainShot=NormalizeAimDirection(direction);changed=true;return true;}
void PromoteBufferedChainShot(){if(!bufferedChainShot.HasValue||!ChainCanLaunch)return;queuedShot=bufferedChainShot;strategyQueuedRequest=strategyBufferedRequest;bufferedChainShot=null;changed=true;}
public void FixedStep(float fixedDeltaTime){if(!IsPlaying||IsPaused||JuiceFrozen)return;PhysicsStep++;contactsThisStep.Clear();simulationClock+=fixedDeltaTime;
            if(IsChainCollection)PromoteBufferedChainShot();
            if(queuedShot.HasValue)
            {
                Vector2 direction=queuedShot.Value;queuedShot=null;
                if(rules.CanShoot)BeginBurst(direction);
            }
            if(PendingBurstBalls>0&&!RefillFreezeBalls&&!CollectionVictoryPending&&simulationClock+.00001f>=nextSpawnTime)
            {
                var ball=reserved.Dequeue();int id=nextBallId++;
                ball.Initialize(id,Generation,SpawnPosition(burstDirection),burstDirection*CurrentLevel.ballSpeed,CurrentLevel.gravity);
                if(!rules.SpawnPendingBall(id)){ball.Deactivate();pool.Push(ball);return;}
                active.Add(id,ball);SpawnCount++;nextSpawnTime+=CurrentLevel.ballInterval;OnBallSpawned(ball);
            }
        }
void BeginBurst(Vector2 direction){int count=Mathf.Max(1,CurrentLevel.volleyCount);
            for(int i=0;i<count;i++)reserved.Enqueue(pool.Count>0?pool.Pop():CreateBall());
            var first=reserved.Dequeue();int id=nextBallId++;
            first.Initialize(id,Generation,SpawnPosition(direction),direction*CurrentLevel.ballSpeed,CurrentLevel.gravity);
            if(!rules.BeginBurst(count,id))
            {
                first.Deactivate();pool.Push(first);ReturnReserved();return;
            }
            burstDirection=direction;active.Add(id,first);SpawnCount++;nextSpawnTime=simulationClock+CurrentLevel.ballInterval;
            if(IsCollection)collectionRoundPending=true;
            if(IsChainCollection)BeginChainVolley();OnBallSpawned(first);}
public void LoseBall(BallBody ball,Vector2 position)
        {
            if(ball==null||!ball.Active||ball.SessionGeneration!=Generation||!rules.LoseBall(ball.Id))return;
            active.Remove(ball.Id);ball.Deactivate(true);pool.Push(ball);LostCount++;changed=true;
            if(active.Count==0)noDamageTime=0;
            
        }
public bool ChainCanLaunch=>IsPlaying&&!IsPaused&&!RefillFreezeBalls&&!CollectionGoalEarned&&Shots>0&&PendingBurstBalls==0&&!queuedShot.HasValue&&simulationClock+.00001f>=chainNextLaunch;
void BeginChainVolley(){chainNextLaunch=simulationClock+.55f;}
public Vector2 SpawnPosition(Vector2 direction)=>LauncherPosition+NormalizeAimDirection(direction)*MuzzleOffset;
}}
