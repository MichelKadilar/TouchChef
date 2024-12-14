Shader "Custom/OutlineShader"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Float) = 1.0
        _OutlineEnabled ("Outline Enabled", Float) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+100" }
        
        Pass
        {
            Name "Outline"
            Cull Front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
            };
            
            float _OutlineWidth;
            float4 _OutlineColor;
            float _OutlineEnabled;
            
            v2f vert(appdata v)
            {
                v2f o;
                
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                worldPos += worldNormal * _OutlineWidth * 0.001 * _OutlineEnabled;
                
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor * _OutlineEnabled;
            }
            
            ENDCG
        }
    }
}