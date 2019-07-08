// Based on https://www.shadertoy.com/view/Ms23DR
Shader "Custom/Img"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Header(Curvature)]
        [Toggle]_UseCurve("UseCurve", Float) = 1
        _CurveX("CurveX", Range(1,16)) = 5
        _CurveY("CurveY", Range(1,9)) = 4
        _Zoom("Zoom", Float) = 1

        [Header(Convergence Error)]
        [Toggle]_UseConvergence("UseConvergence", Float) = 1
        _AbberationStrength("AbberationStrength", Float) = 1
        _DoubleOffset("DoubleOffset", Float) = 8
        _DoubleStrength("DoubleStrength", Float) = 4

        [Header(Vignette)]
        [Toggle]_UseVignette("UseVignette", Float) = 1
        _VignettePower("VignettePower", Float) = 0.3

        [Header(Colour Adjustments)]
        _ColourOffset("ColourOffset", Vector) = (0,0,0,0)
        _Brightness("Brightness", Float) = 2.5

        [Header(Scan Lines)]
        [Toggle]_UseScanLines("UseScanLines", Float) = 1
        _ScanSpeed("ScanSpeed", Float) = 2
        _LineScale("LineScale", Float) = 1.5
        _LineWeightMin("LineWeightMin", Float) = 0.2
        _LineWeightMax("LineWeightMax", Float) = 0.7
        _LineDistributionPow("LineDistributionPow", Float) = 4
        _LineWeight("LineWeight", Range(0,1)) = 1

        [Header(Test)]
        _Params("Params", Vector) = (0,0,0,0)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            float _UseCurve;
            float _CurveX;
            float _CurveY;
            float _Zoom;

            float _UseConvergence;
            float _AbberationStrength;
            float _DoubleOffset;
            float _DoubleStrength;

            float _UseVignette;
            float _VignettePower;

            float4 _ColourOffset;
            float _Brightness;

            float _UseScanLines;
            float _ScanSpeed;
            float _LineScale;
            float _LineWeightMin;
            float _LineWeightMax;
            float _LineDistributionPow;
            float _LineWeight;


            float4 _Params;

            float2 curve(float2 uv) {
                if (_UseCurve == 1) {
                    uv = (uv - 0.5) * 2.0;
                    uv *= 1.1;	
                    uv.x *= 1.0 + pow((abs(uv.y) / _CurveX), 2);
                    uv.y *= 1.0 + pow((abs(uv.x) / _CurveY), 2);
                    uv *=_Zoom;
                    uv  = (uv / 2.0) + 0.5;
                    uv =  uv *0.92 + 0.04;
                }
                return uv;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = curve(i.uv);                

                float3 col = tex2D(_MainTex, uv);
                float3 startCol = col;

                if (_UseConvergence) {
                    float abberationA = _AbberationStrength/1000;
                    
                    // Primary abberation
                    col.r = tex2D(_MainTex,float2(uv.x+abberationA,uv.y+abberationA)).x;
                    col.g = tex2D(_MainTex,float2(uv.x,uv.y-abberationA*2)).y;
                    col.b = tex2D(_MainTex,float2(uv.x-abberationA*2,uv.y)).z;

                    // Second, fainter abberation
                    float d_offset = _DoubleOffset/10;
                    float d_str = _DoubleStrength/10;
                    col.r += d_str*tex2D(_MainTex,d_offset*float2(0.025, -0.027)+float2(uv.x+abberationA,uv.y+abberationA)).x;
                    col.g += d_str*tex2D(_MainTex,d_offset*float2(0.022, -0.02)+float2(uv.x,uv.y-abberationA*2)).y;
                    col.b += d_str*tex2D(_MainTex,d_offset*float2(0.02, -0.018)+float2(uv.x-abberationA*2,uv.y)).z;
                }
              
                if (_UseScanLines) {
                    // Scan lines
                    float t = _Time*10;
                    float scanlines = 0.35+0.35*sin(_ScanSpeed*t+uv.y*_ScreenParams.y*_LineScale);
                    scanlines = pow(clamp(scanlines,0,1),1.7);
                    
                    // Distribute visibility of scan lines over screen
                    float wx = abs(uv.x*2-1);
                    float wy = abs(uv.y*2-1);
                    float w = sqrt(wx*wx+wy*wy)/sqrt(2);
                    w = pow(w,_LineDistributionPow);
                    
                    float lineWeight = lerp(_LineWeightMin,_LineWeightMax, w);
                    float3 colBeforeLines = col;
                    col *= (1-lineWeight)+lineWeight*scanlines;
                    col = lerp(colBeforeLines, col, _LineWeight);
                }

                if (_UseVignette) {
                    // Vignette
                    float vignette = (16*uv.x*uv.y*(1-uv.x)*(1-uv.y));
                    vignette = pow(vignette,_VignettePower);
                    col *= vignette;
                }

                col *= 1 + _ColourOffset/10;
                col *= _Brightness;

                // Ensure edges outside screen are blacked out
                if (uv.x < 0 || uv.x > 1 || uv.y <0 || uv.y > 1){
                    col *= 0.0;
                }

              

                return fixed4(col,1);
            }
            ENDCG
        }
    }
}
