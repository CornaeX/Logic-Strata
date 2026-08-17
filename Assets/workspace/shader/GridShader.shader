Shader "Unlit/GridShader"
{
    Properties
    {
        _CellSize ("Cell Size", Float) = 1.0
        _GridLineWidth ("Grid Line Width", Range(0.001, 0.1)) = 0.02
        _AxisLineWidth ("Axis Line Width", Range(0.001, 0.2)) = 0.05
        
        _GroundColor ("Ground Color", Color) = (0.15, 0.15, 0.15, 1.0)
        _GridColor ("Grid Line Color", Color) = (0.3, 0.3, 0.3, 1.0)
        _XAxisColor ("X-Axis Color (Red)", Color) = (0.9, 0.2, 0.2, 1.0)
        _YAxisColor ("Y-Axis Color (Green)", Color) = (0.2, 0.9, 0.2, 1.0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

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
            float _GridLineWidth;
            float _AxisLineWidth;
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
                // Calculate grid cell distance using world X and Z coordinates
                float2 coord = i.worldPos.xz / _CellSize;
                float2 gridFrac = abs(frac(coord - 0.5) - 0.5);
                
                // Screen-space anti-aliasing for clean lines at any camera distance
                float2 dcoord = fwidth(coord);
                float2 gridLines = smoothstep(dcoord * _GridLineWidth * 50.0, float2(0,0), gridFrac);
                float isGrid = max(gridLines.x, gridLines.y);

                // Check distance to World X-axis (Z = 0) and World Y/Z-axis (X = 0)
                float distToXAxis = abs(i.worldPos.z); // Red line running along X
                float distToYAxis = abs(i.worldPos.x); // Green line running along Z/Y

                float isXAxis = 1.0 - smoothstep(0.0, _AxisLineWidth, distToXAxis);
                float isYAxis = 1.0 - smoothstep(0.0, _AxisLineWidth, distToYAxis);

                // Composite Colors: Base Ground -> Grid Lines -> Red/Green Axes
                fixed4 finalColor = _GroundColor;
                finalColor = lerp(finalColor, _GridColor, isGrid);
                finalColor = lerp(finalColor, _XAxisColor, isXAxis);
                finalColor = lerp(finalColor, _YAxisColor, isYAxis);

                return finalColor;
            }
            ENDCG
        }
    }
}
