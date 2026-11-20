using UnityEngine;

namespace ImpactFeedback {
 // No private Update: the host supplies the same presentation clock as the effects.
 public sealed class SpriteSequence {
  readonly SpriteRenderer target;Sprite[] frames;float age,fps;
  public int FrameIndex{get;private set;}public bool Playing{get;private set;}
  public SpriteSequence(SpriteRenderer image){target=image;}
  public void Play(Sprite[] sequence,float framesPerSecond=25){frames=sequence;fps=Mathf.Max(1,framesPerSecond);age=0;FrameIndex=0;Playing=frames!=null&&frames.Length>0;if(Playing){target.enabled=true;target.sprite=frames[0];}}
  public void Advance(float delta){if(!Playing||delta<=0)return;age+=delta;int next=Mathf.FloorToInt(age*fps);if(next>=frames.Length){FrameIndex=frames.Length-1;target.sprite=frames[FrameIndex];Playing=false;return;}FrameIndex=next;target.sprite=frames[next];}
  public void Reset(Sprite rest){Playing=false;FrameIndex=0;age=0;target.enabled=true;target.sprite=rest;}
 }
}
