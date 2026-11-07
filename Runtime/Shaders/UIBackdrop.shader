Shader "Custom/UIBackdrop" {
Properties { _MainTex("Texture",2D)="white"{} }
SubShader {
Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
Cull Off ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha
Pass {
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
struct A{float4 positionOS:POSITION;};
struct V{float4 positionCS:SV_POSITION;float4 screen:TEXCOORD0;};
V vert(A a){V o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.screen=ComputeScreenPos(o.positionCS);return o;}
half4 frag(V i):SV_Target {
float2 uv=i.screen.xy/i.screen.w;
// Use URP Opaque Downsampling = 4x Box for a softly blurred source.
half3 c=SampleSceneColor(uv);
return half4(c*.95, .96);
}
ENDHLSL
}
}}
