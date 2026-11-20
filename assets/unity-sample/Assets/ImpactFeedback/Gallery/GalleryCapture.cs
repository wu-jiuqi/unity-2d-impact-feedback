using System.IO;
using UnityEngine;

namespace ImpactFeedback.Gallery {
 // Explicit GPU composition works for hidden QA windows. Native UI is reviewed separately.
 public static class GalleryCapture {
  public static void Save(ImpactGallery gallery,string path){
   int w=Screen.width,h=Screen.height;var full=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.ARGB32);var previous=RenderTexture.active;
   RenderTexture.active=full;GL.Clear(true,true,new Color32(252,245,228,255));
   RenderLane(gallery.Baseline.Camera,full);RenderLane(gallery.Active.Camera,full);
   var canvas=Object.FindFirstObjectByType<Canvas>();var captureObject=new GameObject("Temporary UI capture camera");var ui=captureObject.AddComponent<Camera>();ui.enabled=false;ui.orthographic=true;ui.cullingMask=1<<5;ui.clearFlags=CameraClearFlags.Depth;ui.targetTexture=full;
   var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;float plane=canvas.planeDistance;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=ui;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();ui.Render();
   canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=plane;Canvas.ForceUpdateCanvases();
   RenderTexture.active=full;var image=new Texture2D(w,h,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,w,h),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
   RenderTexture.active=previous;ui.targetTexture=null;Object.Destroy(captureObject);Object.Destroy(image);RenderTexture.ReleaseTemporary(full);
  }
  static void RenderLane(Camera camera,RenderTexture output){
   if(!camera.enabled)return;var rect=camera.rect;var pixel=camera.pixelRect;var old=camera.targetTexture;
   var lane=RenderTexture.GetTemporary(Mathf.Max(1,(int)pixel.width),Mathf.Max(1,(int)pixel.height),24,RenderTextureFormat.ARGB32);
   camera.targetTexture=lane;camera.rect=new Rect(0,0,1,1);camera.Render();camera.targetTexture=old;camera.rect=rect;
   RenderTexture.active=output;GL.PushMatrix();GL.LoadPixelMatrix(0,output.width,output.height,0);Graphics.DrawTexture(new Rect(pixel.x,output.height-pixel.y-pixel.height,pixel.width,pixel.height),lane);GL.PopMatrix();RenderTexture.ReleaseTemporary(lane);
  }
 }
}
