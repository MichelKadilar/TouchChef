Shader "Custom/HoldIndicator"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Progress ("Progress", Range(0,1)) = 0
        _Color ("Ring Color", Color) = (0.2, 0.6, 1, 0.8)
        _BackgroundColor ("Background Color", Color) = (0.1, 0.1, 0.1, 0.3)
        _Radius ("Radius", Range(0,1)) = 0.45
        _InnerRadius ("Inner Radius", Range(0,1)) = 0.35
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float _Progress;
            float4 _Color;
            float4 _BackgroundColor;
            float _Radius;
            float _InnerRadius;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 2 - 1; // Centre les UV de -1 à 1
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float dist = length(uv);
                
                float ring = step(dist, _Radius) - step(dist, _InnerRadius);
                
                float angle = atan2(uv.y, uv.x);
                angle = angle < 0 ? angle + 2 * UNITY_PI : angle;
                float progressAngle = _Progress * 2 * UNITY_PI;
                
                float progressMask = step(angle, progressAngle);

                float4 ringColor = ring * (progressMask ? _Color : _BackgroundColor);
                
                float smoothing = 0.01;
                float alpha = smoothstep(_Radius + smoothing, _Radius - smoothing, dist) - 
                             smoothstep(_InnerRadius + smoothing, _InnerRadius - smoothing, dist);
                
                ringColor.a *= alpha;
                
                return ringColor;
            }
            ENDCG
        }
    }
}