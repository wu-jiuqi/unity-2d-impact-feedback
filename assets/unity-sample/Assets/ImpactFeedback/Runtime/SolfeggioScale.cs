using UnityEngine;

namespace ImpactFeedback {
 // C major, equal temperament. A melodic voice makes the intervals audible even
 // when the project's original impact sample has no stable fundamental pitch.
 public sealed class SolfeggioScale {
  static readonly int[] semitones={0,2,4,5,7,9,11,12};
  public static readonly string[] Names={"do","re","mi","fa","so","la","xi","高音do"};
  public const float RootFrequency=261.625565f,Duration=.18f;public const int SampleRate=44100;
  readonly AudioClip[] clips=new AudioClip[8];
  public static float Ratio(int step)=>Mathf.Pow(2,semitones[Mathf.Clamp(step,0,7)]/12f);
  public static float Frequency(int step)=>RootFrequency*Ratio(step);
  public SolfeggioScale(){for(int step=0;step<8;step++){
   int count=Mathf.RoundToInt(SampleRate*Duration);var data=new float[count];float frequency=Frequency(step);
   for(int i=0;i<count;i++){float t=i/(float)SampleRate;float attack=Mathf.Clamp01(t/.004f),release=Mathf.Clamp01((Duration-t)/.035f),envelope=attack*release*Mathf.Exp(-t*12);
    float phase=2*Mathf.PI*frequency*t;data[i]=.45f*envelope*(Mathf.Sin(phase)+.22f*Mathf.Sin(phase*2)+.08f*Mathf.Sin(phase*3))/1.3f;}
   clips[step]=AudioClip.Create("Solfeggio "+Names[step],count,1,SampleRate,false);clips[step].SetData(data,0);
  }}
  public AudioClip Clip(int step)=>clips[Mathf.Clamp(step,0,7)];
  public void Dispose(){foreach(var clip in clips)if(clip!=null)Object.Destroy(clip);}
 }
}
