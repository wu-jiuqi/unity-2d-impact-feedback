using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ImpactFeedback.Gallery {
 public static class GalleryArt {
  [Serializable]class Catalog{public Entry[] sprites;}
  [Serializable]class Entry{public string name,texture;public int[] rectUnityBottomLeft,borderUnityLTRB;}
  static Catalog catalog;static Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();static Font font;
  public static readonly Color Ink=new Color32(127,83,49,255),Muted=new Color32(164,128,89,255),Mint=new Color32(120,188,167,255);
  public static Font Font=>font!=null?font:font=Resources.Load<Font>("Fonts/MaokenZhuyuanTi");
  public static Sprite Get(string key){if(cache.TryGetValue(key,out var s))return s;
   if(catalog==null)catalog=JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("Art/SettingsC/atlas").text);
   foreach(var e in catalog.sprites)if(e.name==key){var t=Resources.Load<Texture2D>("Art/SettingsC/"+System.IO.Path.GetFileNameWithoutExtension(e.texture));var r=e.rectUnityBottomLeft;var b=e.borderUnityLTRB;s=UnityEngine.Sprite.Create(t,new Rect(r[0],r[1],r[2],r[3]),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(b[0],b[1],b[2],b[3]));cache[key]=s;return s;}
   throw new InvalidOperationException("Missing art: "+key);
  }
  public static Sprite Sprite(string key){if(cache.TryGetValue(key,out var s))return s;var t=Resources.Load<Texture2D>("Gallery/"+key);s=UnityEngine.Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,100);cache[key]=s;return s;}
  public static Sprite[] Particles(){var t=Resources.Load<Texture2D>("Gallery/particles");var result=new Sprite[4];for(int i=0;i<4;i++)result[i]=UnityEngine.Sprite.Create(t,new Rect(i%2*t.width/2,(1-i/2)*t.height/2,t.width/2,t.height/2),Vector2.one*.5f,100);return result;}
  public static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h){var o=new GameObject(name,typeof(RectTransform));o.layer=5;o.transform.SetParent(parent,false);var r=(RectTransform)o.transform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
  public static Text Label(Transform parent,string text,float x,float y,float w,float h,int size=20,Color? color=null){var r=Rect(parent,text,x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.font=Font;t.text=text;t.fontSize=size;t.color=color??Ink;t.alignment=TextAnchor.MiddleLeft;t.raycastTarget=false;t.supportRichText=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;}
  public static Image Image(Transform parent,string key,float x,float y,float w,float h,bool sliced=false){var r=Rect(parent,key,x,y,w,h);var im=r.gameObject.AddComponent<Image>();im.sprite=Get(key);im.raycastTarget=false;im.type=sliced?UnityEngine.UI.Image.Type.Sliced:UnityEngine.UI.Image.Type.Simple;im.preserveAspect=!sliced;if(sliced)im.pixelsPerUnitMultiplier=key.StartsWith("button_")?im.sprite.rect.height/h:im.sprite.rect.width/w;return im;}
  public static Button Button(Transform parent,string text,float x,float y,float w,float h,Action action,bool primary=false){var r=Rect(parent,text,x,y,w,h);var hit=r.gameObject.AddComponent<Image>();hit.color=Color.clear;
   var visual=Rect(r,"ButtonVisual",0,0,w,h);visual.pivot=Vector2.one*.5f;visual.anchoredPosition=new Vector2(w/2,-h/2);
   var face=Image(visual,primary?"button_peach_normal":"button_cream_normal",0,0,w,h,true);var label=Label(visual,text,7,0,w-14,h,19);label.alignment=TextAnchor.MiddleCenter;
   var b=r.gameObject.AddComponent<Button>();b.targetGraphic=face;b.transition=Selectable.Transition.SpriteSwap;b.spriteState=new SpriteState{pressedSprite=Get(primary?"button_peach_pressed":"button_cream_pressed")};b.onClick.AddListener(()=>action());r.gameObject.AddComponent<UIBounce>().Target=visual;return b;
  }
 }
}
