Shader "ImpactFeedback/YarnTrail" {
 Properties { _MainTex("Yarn",2D)="white"{} _Glow("Outer glow",Float)=0 _YRange("Calibrated yarn strip",Vector)=(.35,.65,0,0) }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   sampler2D _MainTex;float _Glow;float4 _YRange;
   v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
   fixed4 frag(v2f i):SV_Target {
    // The original strand occupies only the middle quarter of its transparent
    // texture. Enlarge that footprint while preserving its actual yarn fibers.
    fixed4 yarn=tex2D(_MainTex,float2(i.uv.x,lerp(_YRange.x,_YRange.y,i.uv.y)));
    float edge=saturate(1-abs(i.uv.y*2-1));
    float alpha=lerp(saturate(yarn.a*1.5),pow(edge,1.7),_Glow);
    fixed3 rgb=lerp(yarn.rgb,fixed3(1,1,.97),lerp(.3,.55,_Glow));
    return fixed4(rgb*i.color.rgb,alpha*i.color.a);
   }
   ENDCG }
 }
}
