// Generated from AppUI.cs desktop input. No replacement drag/release algorithm.
using System;using UnityEngine;using UnityEngine.EventSystems;
namespace ImpactFeedback.ProjectLaunch {public sealed partial class ProjectBattleInput {
public void OnGUI(){
  if(nativeTouchReady||Input.touchCount>0)return;var e=Event.current;if(e==null||e.button!=0)return;
  if(!playing||busy||modal!=null||results!=null||!Session.IsPlaying||Session.IsPaused)return;
  Vector2 position=new Vector2(e.mousePosition.x,Screen.height-e.mousePosition.y);
  if(e.type==EventType.MouseDown)BeginAim(position,-1);
  else if(e.type==EventType.MouseDrag&&aiming&&pointerId==-1)UpdateAim(position);
  else if(e.type==EventType.MouseUp&&aiming&&pointerId==-1){UpdateAim(position);EndAim(position);}
 }
void BeginAim(Vector2 pos,int id){if(aiming)return;if(UIBlocked(pos)){BattleInputStatus="ui-hit";return;}if(gameCamera==null||!gameCamera.pixelRect.Contains(pos)){BattleInputStatus="outside";return;}aiming=true;BattleInputStatus="aiming";pointerId=id;pointerStart=pos;UpdateAim(pos);}
void UpdateAim(Vector2 pos){if(!aiming)return;var w=ScreenToProjectWorld(gameCamera,pos);var d=w-Session.LauncherPosition;if(d.y<.2f)d.y=.2f;if(d.sqrMagnitude<.01f)d=Vector2.up;aimDirection=d.normalized;Session.SetAim(aimDirection);}
void EndAim(Vector2 pos){if(!aiming)return;bool valid=(pos-pointerStart).sqrMagnitude>36&&gameCamera.pixelRect.Contains(pos);var dir=aimDirection;CancelAim();if(valid&&Session.TryShoot(dir)){BattleAcceptedShots++;BattleInputStatus="accepted";}else BattleInputStatus=valid?"shot-wait":"short/outside";}
void CancelAim(){aiming=false;pointerId=-999;if(Session!=null)Session.SetAim(null);}
bool UIBlocked(Vector2 pos){if(EventSystem.current==null)return false;uiHits.Clear();var data=new PointerEventData(EventSystem.current){position=pos};EventSystem.current.RaycastAll(data,uiHits);return uiHits.Count>0;}
}}