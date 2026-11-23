Shader "EW/SignalDistortion"
{
    Properties { _MainTex("Screen",2D)="white"{} _Amount("Interference",Range(0,1))=0 }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float _Amount;
            struct vIn {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct vOut {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            vOut vert(vIn v){vOut o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
            fixed4 frag(vOut i):SV_Target
            {
                float row=floor(i.uv.y*80);
                float noise=frac(sin(row*13.17+floor(_Time.y*15)*7.37)*43758.5453);
                float tear=step(0.94,noise)*_Amount;
                float2 uv=i.uv;uv.x=clamp(uv.x+(noise-0.5)*tear*0.035,0,1);
                float split=0.002*_Amount*step(0.65,noise);
                fixed4 c=tex2D(_MainTex,uv);
                c.r=tex2D(_MainTex,uv+float2(split,0)).r;
                c.b=tex2D(_MainTex,uv-float2(split,0)).b;
                c.rgb*=1-_Amount*0.05*(0.5+0.5*sin(i.uv.y*1080));
                return c*i.color;
            }
            ENDCG
        }
    }
}
