using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class GalleryBuild {
 public static void Windows(){
  string root=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../"));
  Directory.CreateDirectory("Assets/Scenes");Directory.CreateDirectory("Assets/Resources/Materials");
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.SaveScene(scene,"Assets/Scenes/Gallery.unity");
  foreach(string shader in new[]{"ImpactFeedback/SpriteFlash","Sprites/Default"}){string file="Assets/Resources/Materials/"+shader.Replace('/','-')+".mat";if(AssetDatabase.LoadAssetAtPath<Material>(file)==null)AssetDatabase.CreateAsset(new Material(Shader.Find(shader)),file);}
  foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources"})){
   string path=AssetDatabase.GUIDToAssetPath(guid);var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)continue;
   if(importer.mipmapEnabled||!importer.alphaIsTransparency||importer.maxTextureSize!=2048){importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();}
  }
  PlayerSettings.companyName="BounceBusters";PlayerSettings.productName="Impact Feedback Gallery";PlayerSettings.bundleVersion="0.1.0";
  PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
  PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
  PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{GraphicsDeviceType.Direct3D11});
  var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);var input=settings.FindProperty("activeInputHandler");if(input!=null){input.intValue=0;settings.ApplyModifiedPropertiesWithoutUndo();}
  AssetDatabase.SaveAssets();
  string output=Environment.GetEnvironmentVariable("IMPACT_BUILD_OUTPUT")??Path.Combine(root,"Builds/Windows/ImpactFeedbackGallery.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Gallery.unity"},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  File.WriteAllText(Path.Combine(Path.GetDirectoryName(output),"build-summary.json"),JsonUtility.ToJson(new Summary{result=report.summary.result.ToString(),errors=(int)report.summary.totalErrors,warnings=(int)report.summary.totalWarnings,unity=Application.unityVersion,version="0.1.0"},true));
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Gallery build failed");Debug.Log("IMPACT_GALLERY_BUILD_OK "+output);
 }
 [Serializable]class Summary{public string result,unity,version;public int errors,warnings;}
}
