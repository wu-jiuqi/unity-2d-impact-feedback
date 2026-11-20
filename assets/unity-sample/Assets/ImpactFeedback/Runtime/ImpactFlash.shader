Shader "ImpactFeedback/SpriteFlash" {
 Properties{[PerRendererData]_MainTex("Sprite",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _Flash("White mix",Range(0,1))=0 _AlphaCutoff("Baked alpha cleanup",Range(0,.9))=0}
 SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass{CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct A{float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
 struct V{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
 sampler2D _MainTex;fixed4 _Color;float _Flash,_AlphaCutoff;
 V vert(A a){V o;o.vertex=UnityObjectToClipPos(a.vertex);o.uv=a.uv;o.color=a.color*_Color;return o;}
 fixed4 frag(V i):SV_Target{fixed4 c=tex2D(_MainTex,i.uv);c.a=saturate((c.a-_AlphaCutoff)/(1-_AlphaCutoff));c*=i.color;c.rgb=lerp(c.rgb,1,_Flash);return c;}
 ENDCG}}
}
