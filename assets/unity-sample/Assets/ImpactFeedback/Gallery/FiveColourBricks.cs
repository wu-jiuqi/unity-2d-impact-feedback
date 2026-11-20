using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImpactFeedback.Gallery {
 public static class FiveColourBricks {
  public static readonly string[] Keys={"pink","yellow","mint","blue","lilac"};
  public static readonly string[] Names={"粉","黄","薄荷","蓝","丁香"};
  public static readonly Color[] Tints={new Color32(246,138,170,255),new Color32(245,211,108,255),new Color32(140,221,165,255),new Color32(125,206,235,255),new Color32(197,160,237,255)};
  [Serializable]class Footprints{public Footprint[] entries;}[Serializable]class Footprint{public string key;public float fraction;}
  static readonly Dictionary<int,Sprite[]> frames=new Dictionary<int,Sprite[]>();static readonly Dictionary<string,float> footprints=new Dictionary<string,float>();
  public static Sprite[] Frames(int color){color=Mathf.Clamp(color,0,4);if(frames.TryGetValue(color,out var result))return result;
   var t=Resources.Load<Texture2D>("Gallery/Bricks/ordinary-"+Keys[color]);result=new Sprite[16];float w=t.width/4f,h=t.height/4f;
   for(int i=0;i<16;i++){result[i]=Sprite.Create(t,new Rect(i%4*w,(3-i/4)*h,w,h),Vector2.one*.5f,256,0,SpriteMeshType.FullRect);result[i].name="ordinary-"+Keys[color]+"-"+i;}frames[color]=result;return result;
  }
  public static float Scale(int color,float visibleWidth){if(footprints.Count==0)foreach(var e in JsonUtility.FromJson<Footprints>(Resources.Load<TextAsset>("Gallery/Bricks/footprints").text).entries)footprints[e.key]=e.fraction;return visibleWidth/(Frames(color)[0].bounds.size.x*footprints["ordinary-"+Keys[color]]);}
 }
}
