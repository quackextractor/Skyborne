

Shader "Custom/ComicalClouds"
{
    Properties
    {
        // Base Appearance
        _MainColor ("Cloud Color", Color) = (1,1,1,1)
        _EdgeColor ("Edge Color", Color) = (0.9,0.95,1,1)
        _Thickness ("Cloud Thickness", Range(0.1, 5.0)) = 1.0
        _Density ("Cloud Density", Range(0.0, 1.0)) = 0.8
        
        // Shape Parameters
        _Roughness ("Cloud Roughness", Range(0.0, 1.0)) = 0.5
        _Roundness ("Cloud Roundness", Range(0.0, 1.0)) = 0.7
        _DetailScale ("Detail Scale", Range(0.1, 5.0)) = 2.0
        
        // Movement
        _WindDirection ("Wind Direction", Vector) = (1,0.5,0,0)
        _WindSpeed ("Wind Speed", Range(0.0, 2.0)) = 0.5
        
        // Visual Effects
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.5)) = 0.1
        _DepthFade ("Depth Fade", Range(0.0, 5.0)) = 1.0
        
        
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "PreviewType"="Plane"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 3.0
            sampler2D _CameraDepthTexture;

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Properties
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _MainColor)
                UNITY_DEFINE_INSTANCED_PROP(float4, _EdgeColor)
                UNITY_DEFINE_INSTANCED_PROP(float, _Thickness)
                UNITY_DEFINE_INSTANCED_PROP(float, _Density)
                UNITY_DEFINE_INSTANCED_PROP(float, _Roughness)
                UNITY_DEFINE_INSTANCED_PROP(float, _Roundness)
                UNITY_DEFINE_INSTANCED_PROP(float, _DetailScale)
                UNITY_DEFINE_INSTANCED_PROP(float2, _WindDirection)
                UNITY_DEFINE_INSTANCED_PROP(float, _WindSpeed)
                UNITY_DEFINE_INSTANCED_PROP(float, _EdgeSoftness)
                UNITY_DEFINE_INSTANCED_PROP(float, _DepthFade)
            UNITY_INSTANCING_BUFFER_END(Props)

            // Noise functions
            float3 mod289(float3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float4 mod289(float4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float4 permute(float4 x) { return mod289(((x*34.0)+1.0)*x); }
            float4 taylorInvSqrt(float4 r) { return 1.79284291400159 - 0.85373472095314 * r; }

            float snoise(float3 v)
            {
                const float2 C = float2(1.0/6.0, 1.0/3.0);
                const float4 D = float4(0.0, 0.5, 1.0, 2.0);

                // First corner
                float3 i = floor(v + dot(v, C.yyy));
                float3 x0 = v - i + dot(i, C.xxx);

                // Other corners
                float3 g = step(x0.yzx, x0.xyz);
                float3 l = 1.0 - g;
                float3 i1 = min(g.xyz, l.zxy);
                float3 i2 = max(g.xyz, l.zxy);

                float3 x1 = x0 - i1 + C.xxx;
                float3 x2 = x0 - i2 + C.yyy;
                float3 x3 = x0 - D.yyy;

                // Permutations
                i = mod289(i);
                float4 p = permute(permute(permute(
                            i.z + float4(0.0, i1.z, i2.z, 1.0))
                        + i.y + float4(0.0, i1.y, i2.y, 1.0))
                        + i.x + float4(0.0, i1.x, i2.x, 1.0));

                // Gradients
                float n_ = 0.142857142857; // 1.0/7.0
                float3 ns = n_ * D.wyz - D.xzx;

                float4 j = p - 49.0 * floor(p * ns.z * ns.z);

                float4 x_ = floor(j * ns.z);
                float4 y_ = floor(j - 7.0 * x_);

                float4 x = x_ * ns.x + ns.yyyy;
                float4 y = y_ * ns.x + ns.yyyy;
                float4 h = 1.0 - abs(x) - abs(y);

                float4 b0 = float4(x.xy, y.xy);
                float4 b1 = float4(x.zw, y.zw);

                float4 s0 = floor(b0)*2.0 + 1.0;
                float4 s1 = floor(b1)*2.0 + 1.0;
                float4 sh = -step(h, 0.0);

                float4 a0 = b0.xzyw + s0.xzyw*sh.xxyy;
                float4 a1 = b1.xzyw + s1.xzyw*sh.zzww;

                float3 p0 = float3(a0.xy,h.x);
                float3 p1 = float3(a0.zw,h.y);
                float3 p2 = float3(a1.xy,h.z);
                float3 p3 = float3(a1.zw,h.w);

                // Normalise gradients
                float4 norm = taylorInvSqrt(float4(
                    dot(p0,p0), dot(p1,p1), dot(p2,p2), dot(p3,p3)));
                p0 *= norm.x;
                p1 *= norm.y;
                p2 *= norm.z;
                p3 *= norm.w;

                // Mix final noise value
                float4 m = max(0.6 - float4(
                    dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
                m = m * m;
                return 42.0 * dot(m*m, float4(
                    dot(p0,x0), dot(p1,x1), dot(p2,x2), dot(p3,x3)));
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                // Add subtle vertex movement
                float wave = sin(_Time.y * 2.0 + v.vertex.x * 5.0) * 0.02;
                v.vertex.y += wave * UNITY_ACCESS_INSTANCED_PROP(Props, _Roundness);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                // Get instance properties
                float thickness = UNITY_ACCESS_INSTANCED_PROP(Props, _Thickness);
                float roughness = UNITY_ACCESS_INSTANCED_PROP(Props, _Roughness);
                float roundness = UNITY_ACCESS_INSTANCED_PROP(Props, _Roundness);
                float detailScale = UNITY_ACCESS_INSTANCED_PROP(Props, _DetailScale);
                float2 windDir = UNITY_ACCESS_INSTANCED_PROP(Props, _WindDirection);
                float windSpeed = UNITY_ACCESS_INSTANCED_PROP(Props, _WindSpeed);
                float edgeSoftness = UNITY_ACCESS_INSTANCED_PROP(Props, _EdgeSoftness);

                // Animated position
                float3 pos = float3(
                    i.worldPos.x + _Time.y * windDir.x * windSpeed,
                    i.worldPos.y + _Time.y * windDir.y * windSpeed,
                    i.worldPos.z * thickness
                );

                // Base noise
                float baseNoise = snoise(pos * detailScale);
                
                // Detail noise
                float detailNoise = snoise(pos * (detailScale * (5.0 + roughness * 3.0)));
                
                // Combined shape
                float cloudShape = saturate(baseNoise * 0.8 + detailNoise * 0.4);
                cloudShape = smoothstep(
                    0.3 - roundness * 0.2,
                    0.7 + roundness * 0.3,
                    cloudShape
                );

                // Edge detection
                float edge = smoothstep(
                    cloudShape - edgeSoftness,
                    cloudShape + edgeSoftness,
                    cloudShape
                );

                // Color blending
                float4 col = lerp(
                    UNITY_ACCESS_INSTANCED_PROP(Props, _EdgeColor),
                    UNITY_ACCESS_INSTANCED_PROP(Props, _MainColor),
                    edge
                );

                // Depth fade
                float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
                float depthFade = saturate((depth - i.screenPos.w) / UNITY_ACCESS_INSTANCED_PROP(Props, _DepthFade));

                // Final alpha
                col.a = cloudShape * UNITY_ACCESS_INSTANCED_PROP(Props, _Density) * depthFade;

                return col;
            }
            ENDCG
        }
    }
    CustomEditor "CloudShaderGUI"
    FallBack "Diffuse"
}