using UnityEngine;

namespace ImpactFeedback {
 // The host advances physics and presentation with Delta. UI and recovery use real time.
 // Per-world clocks allow A/B comparison without mutating process-wide Time.timeScale.
 public sealed class FeedbackClock {
  float stopUntil,stopStarted,nextStop,slowUntil,slowScale=1,windowStart,windowUsed;
  bool stopping;
  public float Delta{get;private set;}
  public float Scale{get;private set;}=1;
  public float ActualStopMs{get;private set;}
  public float RequestedStopMs{get;private set;}
  public int Stops{get;private set;}
  public bool Stopped=>stopping;
  public void Request(float now,ImpactProfile p,bool stop,bool slow){
   if(now-windowStart>=1){windowStart=now;windowUsed=0;}
   if(slow&&p.slowMs>0){slowUntil=Mathf.Max(slowUntil,now+p.slowMs/1000);slowScale=p.slowScale;}
   if(!stop||p.hitStopMs<=0||now<nextStop)return;
   if(stopping)return; // Same episode coalesces; repeated hits cannot extend it forever.
   float seconds=Mathf.Min(p.hitStopMs/1000,Mathf.Max(0,p.hitStopBudgetMs/1000-windowUsed));
   if(seconds<=0)return;
   stopping=true;stopStarted=now;stopUntil=now+seconds;RequestedStopMs=seconds*1000;Stops++;
   nextStop=stopUntil+p.hitStopCooldownMs/1000;
   // Slow motion starts after the stop, so a long stop does not consume its duration.
   if(slow)slowUntil=Mathf.Max(slowUntil,stopUntil+p.slowMs/1000);
  }
  public void Tick(float now,float realDelta,bool paused){
   if(stopping&&now>=stopUntil){ActualStopMs=(now-stopStarted)*1000;windowUsed+=now-stopStarted;stopping=false;}
   Scale=paused||stopping?0:now<slowUntil?slowScale:1;
   Delta=Mathf.Max(0,realDelta)*Scale;
  }
  public void Reset(){stopUntil=slowUntil=nextStop=windowUsed=0;stopping=false;Delta=0;Scale=1;}
 }
}
