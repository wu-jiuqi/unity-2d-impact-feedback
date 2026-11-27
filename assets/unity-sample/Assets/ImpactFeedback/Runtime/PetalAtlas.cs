using UnityEngine;

namespace ImpactFeedback {
 // Shared once across presenters. Full-colour petals must use a white renderer tint.
 public static class PetalAtlas {
  static Sprite[] frames;
  public static Sprite[] Frames {
   get {
    if(frames!=null)return frames;
    var texture=Resources.Load<Texture2D>("ImpactFeedback/break-petals-atlas");
    if(texture==null)throw new System.InvalidOperationException("Missing Resources/ImpactFeedback/break-petals-atlas.png");
    frames=new Sprite[16];float w=texture.width/4f,h=texture.height/4f;
    for(int i=0;i<frames.Length;i++){
     frames[i]=Sprite.Create(texture,new Rect(i%4*w,(3-i/4)*h,w,h),Vector2.one*.5f,256,0,SpriteMeshType.FullRect);
     frames[i].name="Break petal "+i;
    }
    return frames;
   }
  }
 }
}
