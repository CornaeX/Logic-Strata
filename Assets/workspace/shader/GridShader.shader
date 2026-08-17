Shader "Unlit/GridShader"
{
    Properties
    {
        _CellSize ("Cell Size", Float) = 0.1
        _LineWidth ("Grid Line Width", Range(0.001, 0.2)) = 0.03
        _AxisWidth ("Axis Line Width", Range(0.001, 0.5)) = 0.08
        
        _GroundColor ("Ground Color", Color) = (0.2, 0.2, 0.2, 1.0)
        _GridColor ("Grid Line Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _XAxisColor ("X-Axis Color (Red)", Color) = (1.0, 0.1, 0.1, 1.0)
        _YAxisColor ("Y-Axis Color (Green)", Color) = (0.1, 1.0, 0.1, 1.0)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Cull Off 
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float _CellSize;
            float _LineWidth;
            float _AxisWidth;
            fixed4 _GroundColor;
            fixed4 _GridColor;
            fixed4 _XAxisColor;
            fixed4 _YAxisColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float safeSize = max(_CellSize, 0.001);
                
                // FIX: Scale world position coordinates by cell size frequency 
                // so grid lines repeat every _CellSize units in world space
                float2 coord = i.worldPos.xz / safeSize;
                float2 g = frac(coord);
                float2 derivative = fwidth(coord);
                
                // Screen-space anti-aliased grid lines (prevents shimmering)
                float2 lineDist = min(g, 1.0 - g);
                float2 gridWidth = max(float2(_LineWidth, _LineWidth), derivative);
                float gridFactor =-min(min(lineDist.x, lineDist.y), 1.0); // handled via step or smoothstep
                
                float isGrid = step(min(lineDist.x, lineDist.y), _LineWidth * 0.5);

                float isXAxis = step(abs(i.worldPos.z), _AxisWidth); 
                float isYAxis = step(abs(i.worldPos.x), _AxisWidth); 

                fixed4 finalColor = _GroundColor;
                finalColor = lerp(finalColor, _GridColor, isGrid);
                finalColor = lerp(finalColor, _XAxisColor, isXAxis);
                finalColor = lerp(finalColor, _YAxisColor, isYAxis);

                finalColor.a = 1.0; 

                return finalColor;
            }
            ENDCG
        }
    }
}