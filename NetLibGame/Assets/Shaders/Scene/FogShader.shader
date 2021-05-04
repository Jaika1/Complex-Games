Shader "Fog Shader Fixed"
{
    Properties
    {
        Vector1_D30978F2("Cloud Noise Scale", Float) = 10
        Vector1_C815250C("Cloud Speed", Float) = 1
        Vector1_C5F5659F("Height Modifier", Float) = 10
        Vector4_63521C71("Remap Values", Vector) = (0, 1, -1, 1)
        Color_55A7C162("Peak Colour", Color) = (1, 1, 1, 0)
        Color_FF6033CE("Valley Colour", Color) = (0, 0, 0, 0)
        Vector1_B1D97770("Noise Edge 1", Float) = 0
        Vector1_47EE67CD("Noise Edge 2", Float) = 1
        Vector1_4ACDCDD1("Fluffiness", Float) = 1
        Vector1_D2B62066("Base Scale", Float) = 5
        Vector1_9B58639F("Base Speed", Float) = 0.3
        Vector1_4C0FE8F1("Base Strength", Float) = 1
        Vector1_B5A66061("Emission Strength", Float) = 1
        Vector1_9656DE2A("Curvature Radius", Float) = 2000
        Vector1_A1C6EE3C("Fresnel Power", Float) = 2
        Vector1_2DEDB162("Fresnel Opacity", Float) = 1
        Vector1_AEA0383C("Cloud Density", Float) = 100
        Vector1_3573D945("Opacity", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent+0"
        }
        
        Pass
        {
            Name "Pass"
            Tags 
            { 
                // LightMode: <None>
            }
           
            // Render State
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            Cull Off
            ZTest LEqual
            ZWrite On
            // ColorMask: <None>
            
        
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            // Pragmas
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
        
            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma shader_feature _ _SAMPLE_GI
            // GraphKeywords: <None>
            
            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define VARYINGS_NEED_POSITION_WS 
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define FEATURES_GRAPH_VERTEX
            #pragma multi_compile_instancing
            #define SHADERPASS_UNLIT
            #define REQUIRE_DEPTH_TEXTURE
            
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float Vector1_D30978F2;
            float Vector1_C815250C;
            float Vector1_C5F5659F;
            float4 Vector4_63521C71;
            float4 Color_55A7C162;
            float4 Color_FF6033CE;
            float Vector1_B1D97770;
            float Vector1_47EE67CD;
            float Vector1_4ACDCDD1;
            float Vector1_D2B62066;
            float Vector1_9B58639F;
            float Vector1_4C0FE8F1;
            float Vector1_B5A66061;
            float Vector1_9656DE2A;
            float Vector1_A1C6EE3C;
            float Vector1_2DEDB162;
            float Vector1_AEA0383C;
            float Vector1_3573D945;
            CBUFFER_END
        
            // Graph Functions
            
            void Unity_Distance_float3(float3 A, float3 B, out float Out)
            {
                Out = distance(A, B);
            }
            
            void Unity_Divide_float(float A, float B, out float Out)
            {
                Out = A / B;
            }
            
            void Unity_Power_float(float A, float B, out float Out)
            {
                Out = pow(A, B);
            }
            
            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }
            
            void Unity_Rotate_About_Axis_Degrees_float(float3 In, float3 Axis, float Rotation, out float3 Out)
            {
                Rotation = radians(Rotation);
            
                float s = sin(Rotation);
                float c = cos(Rotation);
                float one_minus_c = 1.0 - c;
                
                Axis = normalize(Axis);
            
                float3x3 rot_mat = { one_minus_c * Axis.x * Axis.x + c,            one_minus_c * Axis.x * Axis.y - Axis.z * s,     one_minus_c * Axis.z * Axis.x + Axis.y * s,
                                          one_minus_c * Axis.x * Axis.y + Axis.z * s,   one_minus_c * Axis.y * Axis.y + c,              one_minus_c * Axis.y * Axis.z - Axis.x * s,
                                          one_minus_c * Axis.z * Axis.x - Axis.y * s,   one_minus_c * Axis.y * Axis.z + Axis.x * s,     one_minus_c * Axis.z * Axis.z + c
                                        };
            
                Out = mul(rot_mat,  In);
            }
            
            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }
            
            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }
            
            
            float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }
            
            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            { 
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }
            
            void Unity_Add_float(float A, float B, out float Out)
            {
                Out = A + B;
            }
            
            void Unity_Saturate_float(float In, out float Out)
            {
                Out = saturate(In);
            }
            
            void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
            {
                RGBA = float4(R, G, B, A);
                RGB = float3(R, G, B);
                RG = float2(R, G);
            }
            
            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }
            
            void Unity_Absolute_float(float In, out float Out)
            {
                Out = abs(In);
            }
            
            void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
            {
                Out = smoothstep(Edge1, Edge2, In);
            }
            
            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }
            
            void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
            {
                Out = lerp(A, B, T);
            }
            
            void Unity_Smoothstep_float4(float4 Edge1, float4 Edge2, float4 In, out float4 Out)
            {
                Out = smoothstep(Edge1, Edge2, In);
            }
            
            void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
            {
                Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
            }
            
            void Unity_Add_float4(float4 A, float4 B, out float4 Out)
            {
                Out = A + B;
            }
            
            void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
            
            void Unity_Subtract_float(float A, float B, out float Out)
            {
                Out = A - B;
            }
        
            // Graph Vertex
            struct VertexDescriptionInputs
            {
                float3 ObjectSpaceNormal;
                float3 WorldSpaceNormal;
                float3 ObjectSpaceTangent;
                float3 ObjectSpacePosition;
                float3 WorldSpacePosition;
                float3 TimeParameters;
            };
            
            struct VertexDescription
            {
                float3 VertexPosition;
                float3 VertexNormal;
                float3 VertexTangent;
            };
            
            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                float _Distance_C3096CE2_Out_2;
                Unity_Distance_float3(SHADERGRAPH_OBJECT_POSITION, IN.WorldSpacePosition, _Distance_C3096CE2_Out_2);
                float _Property_5238734F_Out_0 = Vector1_9656DE2A;
                float _Divide_B3B86D7D_Out_2;
                Unity_Divide_float(_Distance_C3096CE2_Out_2, _Property_5238734F_Out_0, _Divide_B3B86D7D_Out_2);
                float _Power_B2F0308E_Out_2;
                Unity_Power_float(_Divide_B3B86D7D_Out_2, 3, _Power_B2F0308E_Out_2);
                float3 _Multiply_20699AFE_Out_2;
                Unity_Multiply_float(IN.WorldSpaceNormal, (_Power_B2F0308E_Out_2.xxx), _Multiply_20699AFE_Out_2);
                float _Property_A400740_Out_0 = Vector1_B1D97770;
                float _Property_21C64270_Out_0 = Vector1_47EE67CD;
                float3 _RotateAboutAxis_2909AAC4_Out_3;
                Unity_Rotate_About_Axis_Degrees_float(IN.WorldSpacePosition, float3 (1, 0, 0), 90, _RotateAboutAxis_2909AAC4_Out_3);
                float _Property_CD4F56B4_Out_0 = Vector1_C815250C;
                float _Multiply_EED0F979_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_CD4F56B4_Out_0, _Multiply_EED0F979_Out_2);
                float2 _TilingAndOffset_F096EC6A_Out_3;
                Unity_TilingAndOffset_float((_RotateAboutAxis_2909AAC4_Out_3.xy), float2 (1, 1), (_Multiply_EED0F979_Out_2.xx), _TilingAndOffset_F096EC6A_Out_3);
                float _Property_4C9FD3BF_Out_0 = Vector1_D30978F2;
                float _GradientNoise_3D6B045C_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_F096EC6A_Out_3, _Property_4C9FD3BF_Out_0, _GradientNoise_3D6B045C_Out_2);
                float2 _TilingAndOffset_7F106F56_Out_3;
                Unity_TilingAndOffset_float((_RotateAboutAxis_2909AAC4_Out_3.xy), float2 (1, 1), float2 (0, 0), _TilingAndOffset_7F106F56_Out_3);
                float _GradientNoise_DD58818E_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_7F106F56_Out_3, _Property_4C9FD3BF_Out_0, _GradientNoise_DD58818E_Out_2);
                float _Add_9782A73D_Out_2;
                Unity_Add_float(_GradientNoise_3D6B045C_Out_2, _GradientNoise_DD58818E_Out_2, _Add_9782A73D_Out_2);
                float _Divide_5EBAE61E_Out_2;
                Unity_Divide_float(_Add_9782A73D_Out_2, 2, _Divide_5EBAE61E_Out_2);
                float _Saturate_FAF6D122_Out_1;
                Unity_Saturate_float(_Divide_5EBAE61E_Out_2, _Saturate_FAF6D122_Out_1);
                float _Property_158AFEB7_Out_0 = Vector1_4ACDCDD1;
                float _Power_E2D4DAC1_Out_2;
                Unity_Power_float(_Saturate_FAF6D122_Out_1, _Property_158AFEB7_Out_0, _Power_E2D4DAC1_Out_2);
                float4 _Property_D258128B_Out_0 = Vector4_63521C71;
                float _Split_F1ED60DD_R_1 = _Property_D258128B_Out_0[0];
                float _Split_F1ED60DD_G_2 = _Property_D258128B_Out_0[1];
                float _Split_F1ED60DD_B_3 = _Property_D258128B_Out_0[2];
                float _Split_F1ED60DD_A_4 = _Property_D258128B_Out_0[3];
                float4 _Combine_C7B329CD_RGBA_4;
                float3 _Combine_C7B329CD_RGB_5;
                float2 _Combine_C7B329CD_RG_6;
                Unity_Combine_float(_Split_F1ED60DD_R_1, _Split_F1ED60DD_G_2, 0, 0, _Combine_C7B329CD_RGBA_4, _Combine_C7B329CD_RGB_5, _Combine_C7B329CD_RG_6);
                float4 _Combine_7CF173C7_RGBA_4;
                float3 _Combine_7CF173C7_RGB_5;
                float2 _Combine_7CF173C7_RG_6;
                Unity_Combine_float(_Split_F1ED60DD_B_3, _Split_F1ED60DD_A_4, 0, 0, _Combine_7CF173C7_RGBA_4, _Combine_7CF173C7_RGB_5, _Combine_7CF173C7_RG_6);
                float _Remap_449CA72D_Out_3;
                Unity_Remap_float(_Power_E2D4DAC1_Out_2, _Combine_C7B329CD_RG_6, _Combine_7CF173C7_RG_6, _Remap_449CA72D_Out_3);
                float _Absolute_F490C4D_Out_1;
                Unity_Absolute_float(_Remap_449CA72D_Out_3, _Absolute_F490C4D_Out_1);
                float _Smoothstep_B39D056A_Out_3;
                Unity_Smoothstep_float(_Property_A400740_Out_0, _Property_21C64270_Out_0, _Absolute_F490C4D_Out_1, _Smoothstep_B39D056A_Out_3);
                float _Property_8300113F_Out_0 = Vector1_9B58639F;
                float _Multiply_3291FAD1_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_8300113F_Out_0, _Multiply_3291FAD1_Out_2);
                float2 _TilingAndOffset_B5767666_Out_3;
                Unity_TilingAndOffset_float((_RotateAboutAxis_2909AAC4_Out_3.xy), float2 (1, 1), (_Multiply_3291FAD1_Out_2.xx), _TilingAndOffset_B5767666_Out_3);
                float _Property_EC8A8B29_Out_0 = Vector1_D2B62066;
                float _GradientNoise_8870EA33_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_B5767666_Out_3, _Property_EC8A8B29_Out_0, _GradientNoise_8870EA33_Out_2);
                float _Property_F16589D3_Out_0 = Vector1_4C0FE8F1;
                float _Multiply_3195E5E2_Out_2;
                Unity_Multiply_float(_GradientNoise_8870EA33_Out_2, _Property_F16589D3_Out_0, _Multiply_3195E5E2_Out_2);
                float _Add_524CA4AD_Out_2;
                Unity_Add_float(_Smoothstep_B39D056A_Out_3, _Multiply_3195E5E2_Out_2, _Add_524CA4AD_Out_2);
                float _Add_77813DD5_Out_2;
                Unity_Add_float(1, _Property_F16589D3_Out_0, _Add_77813DD5_Out_2);
                float _Divide_E8314B3F_Out_2;
                Unity_Divide_float(_Add_524CA4AD_Out_2, _Add_77813DD5_Out_2, _Divide_E8314B3F_Out_2);
                float3 _Multiply_AFF36AB3_Out_2;
                Unity_Multiply_float(IN.ObjectSpaceNormal, (_Divide_E8314B3F_Out_2.xxx), _Multiply_AFF36AB3_Out_2);
                float _Property_7B73D722_Out_0 = Vector1_C5F5659F;
                float3 _Multiply_67A088E9_Out_2;
                Unity_Multiply_float(_Multiply_AFF36AB3_Out_2, (_Property_7B73D722_Out_0.xxx), _Multiply_67A088E9_Out_2);
                float3 _Add_B71DD03A_Out_2;
                Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_67A088E9_Out_2, _Add_B71DD03A_Out_2);
                float3 _Add_776BADD9_Out_2;
                Unity_Add_float3(_Multiply_20699AFE_Out_2, _Add_B71DD03A_Out_2, _Add_776BADD9_Out_2);
                description.VertexPosition = _Add_776BADD9_Out_2;
                description.VertexNormal = IN.ObjectSpaceNormal;
                description.VertexTangent = IN.ObjectSpaceTangent;
                return description;
            }
            
            // Graph Pixel
            struct SurfaceDescriptionInputs
            {
                float3 WorldSpaceNormal;
                float3 WorldSpaceViewDirection;
                float3 WorldSpacePosition;
                float4 ScreenPosition;
                float3 TimeParameters;
            };
            
            struct SurfaceDescription
            {
                float3 Color;
                float Alpha;
                float AlphaClipThreshold;
            };
            
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float4 _Property_3BDB0F73_Out_0 = Color_FF6033CE;
                float4 _Property_B099332A_Out_0 = Color_55A7C162;
                float _Property_A400740_Out_0 = Vector1_B1D97770;
                float _Property_21C64270_Out_0 = Vector1_47EE67CD;
                float3 _RotateAboutAxis_2909AAC4_Out_3;
                Unity_Rotate_About_Axis_Degrees_float(IN.WorldSpacePosition, float3 (1, 0, 0), 90, _RotateAboutAxis_2909AAC4_Out_3);
                float _Property_CD4F56B4_Out_0 = Vector1_C815250C;
                float _Multiply_EED0F979_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_CD4F56B4_Out_0, _Multiply_EED0F979_Out_2);
                float2 _TilingAndOffset_F096EC6A_Out_3;
                Unity_TilingAndOffset_float((_RotateAboutAxis_2909AAC4_Out_3.xy), float2 (1, 1), (_Multiply_EED0F979_Out_2.xx), _TilingAndOffset_F096EC6A_Out_3);
                float _Property_4C9FD3BF_Out_0 = Vector1_D30978F2;
                float _GradientNoise_3D6B045C_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_F096EC6A_Out_3, _Property_4C9FD3BF_Out_0, _GradientNoise_3D6B045C_Out_2);
                float2 _TilingAndOffset_7F106F56_Out_3;
                Unity_TilingAndOffset_float((_RotateAboutAxis_2909AAC4_Out_3.xy), float2 (1, 1), float2 (0, 0), _TilingAndOffset_7F106F56_Out_3);
                float _GradientNoise_DD58818E_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_7F106F56_Out_3, _Property_4C9FD3BF_Out_0, _GradientNoise_DD58818E_Out_2);
                float _Add_9782A73D_Out_2;
                Unity_Add_float(_GradientNoise_3D6B045C_Out_2, _GradientNoise_DD58818E_Out_2, _Add_9782A73D_Out_2);
                float _Divide_5EBAE61E_Out_2;
                Unity_Divide_float(_Add_9782A73D_Out_2, 2, _Divide_5EBAE61E_Out_2);
                float _Saturate_FAF6D122_Out_1;
                Unity_Saturate_float(_Divide_5EBAE61E_Out_2, _Saturate_FAF6D122_Out_1);
                float _Property_158AFEB7_Out_0 = Vector1_4ACDCDD1;
                float _Power_E2D4DAC1_Out_2;
                Unity_Power_float(_Saturate_FAF6D122_Out_1, _Property_158AFEB7_Out_0, _Power_E2D4DAC1_Out_2);
                float4 _Property_D258128B_Out_0 = Vector4_63521C71;
                float _Split_F1ED60DD_R_1 = _Property_D258128B_Out_0[0];
                float _Split_F1ED60DD_G_2 = _Property_D258128B_Out_0[1];
                float _Split_F1ED60DD_B_3 = _Property_D258128B_Out_0[2];
                float _Split_F1ED60DD_A_4 = _Property_D258128B_Out_0[3];
                float4 _Combine_C7B329CD_RGBA_4;
                float3 _Combine_C7B329CD_RGB_5;
                float2 _Combine_C7B329CD_RG_6;
                Unity_Combine_float(_Split_F1ED60DD_R_1, _Split_F1ED60DD_G_2, 0, 0, _Combine_C7B329CD_RGBA_4, _Combine_C7B329CD_RGB_5, _Combine_C7B329CD_RG_6);
                float4 _Combine_7CF173C7_RGBA_4;
                float3 _Combine_7CF173C7_RGB_5;
                float2 _Combine_7CF173C7_RG_6;
                Unity_Combine_float(_Split_F1ED60DD_B_3, _Split_F1ED60DD_A_4, 0, 0, _Combine_7CF173C7_RGBA_4, _Combine_7CF173C7_RGB_5, _Combine_7CF173C7_RG_6);
                float _Remap_449CA72D_Out_3;
                Unity_Remap_float(_Power_E2D4DAC1_Out_2, _Combine_C7B329CD_RG_6, _Combine_7CF173C7_RG_6, _Remap_449CA72D_Out_3);
                float _Absolute_F490C4D_Out_1;
                Unity_Absolute_float(_Remap_449CA72D_Out_3, _Absolute_F490C4D_Out_1);
                float _Smoothstep_B39D056A_Out_3;
                Unity_Smoothstep_float(_Property_A400740_Out_0, _Property_21C64270_Out_0, _Absolute_F490C4D_Out_1, _Smoothstep_B39D056A_Out_3);
                float _Property_8300113F_Out_0 = Vector1_9B58639F;
                float _Multiply_3291FAD1_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_8300113F_Out_0, _Multiply_3291FAD1_Out_2);
                float2 _TilingAndOffset_B5767666_Out_3;
                Unity_TilingAndOffset_float((_RotateAboutAxis_2909AAC4_Out_3.xy), float2 (1, 1), (_Multiply_3291FAD1_Out_2.xx), _TilingAndOffset_B5767666_Out_3);
                float _Property_EC8A8B29_Out_0 = Vector1_D2B62066;
                float _GradientNoise_8870EA33_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_B5767666_Out_3, _Property_EC8A8B29_Out_0, _GradientNoise_8870EA33_Out_2);
                float _Property_F16589D3_Out_0 = Vector1_4C0FE8F1;
                float _Multiply_3195E5E2_Out_2;
                Unity_Multiply_float(_GradientNoise_8870EA33_Out_2, _Property_F16589D3_Out_0, _Multiply_3195E5E2_Out_2);
                float _Add_524CA4AD_Out_2;
                Unity_Add_float(_Smoothstep_B39D056A_Out_3, _Multiply_3195E5E2_Out_2, _Add_524CA4AD_Out_2);
                float _Add_77813DD5_Out_2;
                Unity_Add_float(1, _Property_F16589D3_Out_0, _Add_77813DD5_Out_2);
                float _Divide_E8314B3F_Out_2;
                Unity_Divide_float(_Add_524CA4AD_Out_2, _Add_77813DD5_Out_2, _Divide_E8314B3F_Out_2);
                float4 _Lerp_46DAFE40_Out_3;
                Unity_Lerp_float4(_Property_3BDB0F73_Out_0, _Property_B099332A_Out_0, (_Divide_E8314B3F_Out_2.xxxx), _Lerp_46DAFE40_Out_3);
                float4 _Smoothstep_E52D90A1_Out_3;
                Unity_Smoothstep_float4(float4(0, 0, 0, 0), float4(1, 1, 1, 1), _Lerp_46DAFE40_Out_3, _Smoothstep_E52D90A1_Out_3);
                float _Property_E38C4D2A_Out_0 = Vector1_A1C6EE3C;
                float _FresnelEffect_F176B181_Out_3;
                Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_E38C4D2A_Out_0, _FresnelEffect_F176B181_Out_3);
                float _Multiply_E5A93072_Out_2;
                Unity_Multiply_float(_Divide_E8314B3F_Out_2, _FresnelEffect_F176B181_Out_3, _Multiply_E5A93072_Out_2);
                float _Property_C1992649_Out_0 = Vector1_2DEDB162;
                float _Multiply_AA5D1F35_Out_2;
                Unity_Multiply_float(_Multiply_E5A93072_Out_2, _Property_C1992649_Out_0, _Multiply_AA5D1F35_Out_2);
                float4 _Add_5A80EFD6_Out_2;
                Unity_Add_float4(_Smoothstep_E52D90A1_Out_3, (_Multiply_AA5D1F35_Out_2.xxxx), _Add_5A80EFD6_Out_2);
                float _SceneDepth_EC17DC7_Out_1;
                Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_EC17DC7_Out_1);
                float4 _ScreenPosition_F96EC78_Out_0 = IN.ScreenPosition;
                float _Split_EE0A8ED0_R_1 = _ScreenPosition_F96EC78_Out_0[0];
                float _Split_EE0A8ED0_G_2 = _ScreenPosition_F96EC78_Out_0[1];
                float _Split_EE0A8ED0_B_3 = _ScreenPosition_F96EC78_Out_0[2];
                float _Split_EE0A8ED0_A_4 = _ScreenPosition_F96EC78_Out_0[3];
                float _Subtract_27535CEB_Out_2;
                Unity_Subtract_float(_Split_EE0A8ED0_A_4, 1, _Subtract_27535CEB_Out_2);
                float _Subtract_E6D9BA43_Out_2;
                Unity_Subtract_float(_SceneDepth_EC17DC7_Out_1, _Subtract_27535CEB_Out_2, _Subtract_E6D9BA43_Out_2);
                float _Property_733ABBF7_Out_0 = Vector1_AEA0383C;
                float _Divide_89651C50_Out_2;
                Unity_Divide_float(_Subtract_E6D9BA43_Out_2, _Property_733ABBF7_Out_0, _Divide_89651C50_Out_2);
                float _Saturate_C2839B81_Out_1;
                Unity_Saturate_float(_Divide_89651C50_Out_2, _Saturate_C2839B81_Out_1);
                float _Smoothstep_98CD0A08_Out_3;
                Unity_Smoothstep_float(0, 1, _Saturate_C2839B81_Out_1, _Smoothstep_98CD0A08_Out_3);
                float _Property_76F8A273_Out_0 = Vector1_3573D945;
                float _Multiply_DFBCE244_Out_2;
                Unity_Multiply_float(_Smoothstep_98CD0A08_Out_3, _Property_76F8A273_Out_0, _Multiply_DFBCE244_Out_2);
                surface.Color = (_Add_5A80EFD6_Out_2.xyz);
                surface.Alpha = _Multiply_DFBCE244_Out_2;
                surface.AlphaClipThreshold = 0;
                return surface;
            }
        
            // --------------------------------------------------
            // Structs and Packing
        
            // Generated Type: Attributes
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
        
            // Generated Type: Varyings
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS;
                float3 normalWS;
                float3 viewDirectionWS;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Generated Type: PackedVaryings
            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                float3 interp00 : TEXCOORD0;
                float3 interp01 : TEXCOORD1;
                float3 interp02 : TEXCOORD2;
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Packed Type: Varyings
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output = (PackedVaryings)0;
                output.positionCS = input.positionCS;
                output.interp00.xyz = input.positionWS;
                output.interp01.xyz = input.normalWS;
                output.interp02.xyz = input.viewDirectionWS;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            
            // Unpacked Type: Varyings
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = input.positionCS;
                output.positionWS = input.interp00.xyz;
                output.normalWS = input.interp01.xyz;
                output.viewDirectionWS = input.interp02.xyz;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);
            
                output.ObjectSpaceNormal =           input.normalOS;
                output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
                output.ObjectSpaceTangent =          input.tangentOS;
                output.ObjectSpacePosition =         input.positionOS;
                output.WorldSpacePosition =          TransformObjectToWorld(input.positionOS);
                output.TimeParameters =              _TimeParameters.xyz;
            
                return output;
            }
            
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
            
            	// must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            	float3 unnormalizedNormalWS = input.normalWS;
                const float renormFactor = 1.0 / length(unnormalizedNormalWS);
            
            
                output.WorldSpaceNormal =            renormFactor*input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph
            
            
                output.WorldSpaceViewDirection =     input.viewDirectionWS; //TODO: by default normalized in HD, but not in universal
                output.WorldSpacePosition =          input.positionWS;
                output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
                output.TimeParameters =              _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
            #else
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            #endif
            #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            
                return output;
            }
            
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"
        
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags 
            { 
                "LightMode" = "ShadowCaster"
            }
           
            // Render State
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            Cull Off
            ZTest LEqual
            ZWrite On
            // ColorMask: <None>
            
        
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            // Pragmas
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing
        
            // Keywords
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // GraphKeywords: <None>
            
            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define VARYINGS_NEED_POSITION_WS 
            #define FEATURES_GRAPH_VERTEX
            #pragma multi_compile_instancing
            #define SHADERPASS_SHADOWCASTER
            #define REQUIRE_DEPTH_TEXTURE
            
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float Vector1_D30978F2;
            float Vector1_C815250C;
            float Vector1_C5F5659F;
            float4 Vector4_63521C71;
            float4 Color_55A7C162;
            float4 Color_FF6033CE;
            float Vector1_B1D97770;
            float Vector1_47EE67CD;
            float Vector1_4ACDCDD1;
            float Vector1_D2B62066;
            float Vector1_9B58639F;
            float Vector1_4C0FE8F1;
            float Vector1_B5A66061;
            float Vector1_9656DE2A;
            float Vector1_A1C6EE3C;
            float Vector1_2DEDB162;
            float Vector1_AEA0383C;
            float Vector1_3573D945;
            CBUFFER_END
        
            // Graph Functions
            
            void Unity_Distance_float3(float3 A, float3 B, out float Out)
            {
                Out = distance(A, B);
            }
            
            void Unity_Divide_float(float A, float B, out float Out)
            {
                Out = A / B;
            }
            
            void Unity_Power_float(float A, float B, out float Out)
            {
                Out = pow(A, B);
            }
            
            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }
            
            void Unity_Rotate_About_Axis_Degrees_float(float3 In, float3 Axis, float Rotation, out float3 Out)
            {
                Rotation = radians(Rotation);
            
                float s = sin(Rotation);
                float c = cos(Rotation);
                float one_minus_c = 1.0 - c;
                
                Axis = normalize(Axis);
            
                float3x3 rot_mat = { one_minus_c * Axis.x * Axis.x + c,            one_minus_c * Axis.x * Axis.y - Axis.z * s,     one_minus_c * Axis.z * Axis.x + Axis.y * s,
                                          one_minus_c * Axis.x * Axis.y + Axis.z * s,   one_minus_c * Axis.y * Axis.y + c,              one_minus_c * Axis.y * Axis.z - Axis.x * s,
                                          one_minus_c * Axis.z * Axis.x - Axis.y * s,   one_minus_c * Axis.y * Axis.z + Axis.x * s,     one_minus_c * Axis.z * Axis.z + c
                                        };
            
                Out = mul(rot_mat,  In);
            }
            
            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }
            
            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }
            
            
            float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }
            
            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            { 
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }
            
            void Unity_Add_float(float A, float B, out float Out)
            {
                Out = A + B;
            }
            
            void Unity_Saturate_float(float In, out float Out)
            {
                Out = saturate(In);
            }
            
            void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
            {
                RGBA = float4(R, G, B, A);
                RGB = float3(R, G, B);
                RG = float2(R, G);
            }
            
            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }
            
            void Unity_Absolute_float(float In, out float Out)
            {
                Out = abs(In);
            }
            
            void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
            {
                Out = smoothstep(Edge1, Edge2, In);
            }
            
            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }
            
            void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
            
            void Unity_Subtract_float(float A, float B, out float Out)
            {
                Out = A - B;
            }
        
            // Graph Vertex
            struct VertexDescriptionInputs
            {
                float3 ObjectSpaceNormal;
                float3 WorldSpaceNormal;
                float3 ObjectSpaceTangent;
                float3 ObjectSpacePosition;
                float3 WorldSpacePosition;
                float3 TimeParameters;
            };
            
            struct VertexDescription
            {
                float3 VertexPosition;
                float3 VertexNormal;
                float3 VertexTangent;
            };
            
            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                float _Distance_C3096CE2_Out_2;
                Unity_Distance_float3(SHADERGRAPH_OBJECT_POSITION, IN.WorldSpacePosition, _Distance_C3096CE2_Out_2);
                float _Property_5238734F_Out_0 = Vector1_9656DE2A;
                float _Divide_B3B86D7D_Out_2;
                Unity_Divide_float(_Distance_C3096CE2_Out_2, _Property_5238734F_Out_0, _Divide_B3B86D7D_Out_2);
                float _Power_B2F0308E_Out_2;
                Unity_Power_float(_Divide_B3B86D7D_Out_2, 3, _Power_B2F0308E_Out_2);
                float3 _Multiply_20699AFE_Out_2;
                Unity_Multiply_float(IN.WorldSpaceNormal, (_Power_B2F0308E_Out_2.xxx), _Multiply_20699AFE_Out_2);
                float _Property_A400740_Out_0 = Vector1_B1D97770;
                float _Property_21C64270_Out_0 = Vector1_47EE67CD;
                float3 _RotateAboutAxis_2909AAC4_Out_3;
                Unity_Rotate_About_Axis_Degrees_float(IN.WorldSpacePosition, float3 (1, 0, 0), 90, _RotateAboutAxis_2909AAC4_Out_3);
                float _Property_CD4F56B4_Out_0 = Vector1_C815250C;
                float _Multiply_EED0F979_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_CD4F56B4_Out_0, _Multiply_EED0F979_Out_2);
                float2 _TilingAndOffset_F096EC6A_Out_3;
                Unity_TilingAndOffset_float((_RotateAboutAxis_2909AAC4_Out_3.xy), float2 (1, 1), (_Multiply_EED0F979_Out_2.xx), _TilingAndOffset_F096EC6A_Out_3);
                float _Property_4C9FD3BF_Out_0 = Vector1_D30978F2;
                float _GradientNoise_3D6B045C_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_F096EC6A_Out_3, _Property_4C9FD3BF_Out_0, _GradientNoise_3D6B045C_Out_2);
                float2 _TilingAndOffset_7F106F56_Out_3;
                Unity_TilingAndOffset_float((_RotateAboutAxis_2909AAC4_Out_3.xy), float2 (1, 1), float2 (0, 0), _TilingAndOffset_7F106F56_Out_3);
                float _GradientNoise_DD58818E_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_7F106F56_Out_3, _Property_4C9FD3BF_Out_0, _GradientNoise_DD58818E_Out_2);
                float _Add_9782A73D_Out_2;
                Unity_Add_float(_GradientNoise_3D6B045C_Out_2, _GradientNoise_DD58818E_Out_2, _Add_9782A73D_Out_2);
                float _Divide_5EBAE61E_Out_2;
                Unity_Divide_float(_Add_9782A73D_Out_2, 2, _Divide_5EBAE61E_Out_2);
                float _Saturate_FAF6D122_Out_1;
                Unity_Saturate_float(_Divide_5EBAE61E_Out_2, _Saturate_FAF6D122_Out_1);
                float _Property_158AFEB7_Out_0 = Vector1_4ACDCDD1;
                float _Power_E2D4DAC1_Out_2;
                Unity_Power_float(_Saturate_FAF6D122_Out_1, _Property_158AFEB7_Out_0, _Power_E2D4DAC1_Out_2);
                float4 _Property_D258128B_Out_0 = Vector4_63521C71;
                float _Split_F1ED60DD_R_1 = _Property_D258128B_Out_0[0];
                float _Split_F1ED60DD_G_2 = _Property_D258128B_Out_0[1];
                float _Split_F1ED60DD_B_3 = _Property_D258128B_Out_0[2];
                float _Split_F1ED60DD_A_4 = _Property_D258128B_Out_0[3];
                float4 _Combine_C7B329CD_RGBA_4;
                float3 _Combine_C7B329CD_RGB_5;
                float2 _Combine_C7B329CD_RG_6;
                Unity_Combine_float(_Split_F1ED60DD_R_1, _Split_F1ED60DD_G_2, 0, 0, _Combine_C7B329CD_RGBA_4, _Combine_C7B329CD_RGB_5, _Combine_C7B329CD_RG_6);
                float4 _Combine_7CF173C7_RGBA_4;
                float3 _Combine_7CF173C7_RGB_5;
                float2 _Combine_7CF173C7_RG_6;
                Unity_Combine_float(_Split_F1ED60DD_B_3, _Split_F1ED60DD_A_4, 0, 0, _Combine_7CF173C7_RGBA_4, _Combine_7CF173C7_RGB_5, _Combine_7CF173C7_RG_6);
                float _Remap_449CA72D_Out_3;
                Unity_Remap_float(_Power_E2D4DAC1_Out_2, _Combine_C7B329CD_RG_6, _Combine_7CF173C7_RG_6, _Remap_449CA72D_Out_3);
                float _Absolute_F490C4D_Out_1;
                Unity_Absolute_float(_Remap_449CA72D_Out_3, _Absolute_F490C4D_Out_1);
                float _Smoothstep_B39D056A_Out_3;
                Unity_Smoothstep_float(_Property_A400740_Out_0, _Property_21C64270_Out_0, _Absolute_F490C4D_Out_1, _Smoothstep_B39D056A_Out_3);
                float _Property_8300113F_Out_0 = Vector1_9B58639F;
                float _Multiply_3291FAD1_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_8300113F_Out_0, _Multiply_3291FAD1_Out_2);
                float2 _TilingAndOffset_B5767666_Out_3;
                Unity_TilingAndOffset_float((_RotateAboutAxis_2909AAC4_Out_3.xy), float2 (1, 1), (_Multiply_3291FAD1_Out_2.xx), _TilingAndOffset_B5767666_Out_3);
                float _Property_EC8A8B29_Out_0 = Vector1_D2B62066;
                float _GradientNoise_8870EA33_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_B5767666_Out_3, _Property_EC8A8B29_Out_0, _GradientNoise_8870EA33_Out_2);
                float _Property_F16589D3_Out_0 = Vector1_4C0FE8F1;
                float _Multiply_3195E5E2_Out_2;
                Unity_Multiply_float(_GradientNoise_8870EA33_Out_2, _Property_F16589D3_Out_0, _Multiply_3195E5E2_Out_2);
                float _Add_524CA4AD_Out_2;
                Unity_Add_float(_Smoothstep_B39D056A_Out_3, _Multiply_3195E5E2_Out_2, _Add_524CA4AD_Out_2);
                float _Add_77813DD5_Out_2;
                Unity_Add_float(1, _Property_F16589D3_Out_0, _Add_77813DD5_Out_2);
                float _Divide_E8314B3F_Out_2;
                Unity_Divide_float(_Add_524CA4AD_Out_2, _Add_77813DD5_Out_2, _Divide_E8314B3F_Out_2);
                float3 _Multiply_AFF36AB3_Out_2;
                Unity_Multiply_float(IN.ObjectSpaceNormal, (_Divide_E8314B3F_Out_2.xxx), _Multiply_AFF36AB3_Out_2);
                float _Property_7B73D722_Out_0 = Vector1_C5F5659F;
                float3 _Multiply_67A088E9_Out_2;
                Unity_Multiply_float(_Multiply_AFF36AB3_Out_2, (_Property_7B73D722_Out_0.xxx), _Multiply_67A088E9_Out_2);
                float3 _Add_B71DD03A_Out_2;
                Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_67A088E9_Out_2, _Add_B71DD03A_Out_2);
                float3 _Add_776BADD9_Out_2;
                Unity_Add_float3(_Multiply_20699AFE_Out_2, _Add_B71DD03A_Out_2, _Add_776BADD9_Out_2);
                description.VertexPosition = _Add_776BADD9_Out_2;
                description.VertexNormal = IN.ObjectSpaceNormal;
                description.VertexTangent = IN.ObjectSpaceTangent;
                return description;
            }
            
            // Graph Pixel
            struct SurfaceDescriptionInputs
            {
                float3 WorldSpacePosition;
                float4 ScreenPosition;
            };
            
            struct SurfaceDescription
            {
                float Alpha;
                float AlphaClipThreshold;
            };
            
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float _SceneDepth_EC17DC7_Out_1;
                Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_EC17DC7_Out_1);
                float4 _ScreenPosition_F96EC78_Out_0 = IN.ScreenPosition;
                float _Split_EE0A8ED0_R_1 = _ScreenPosition_F96EC78_Out_0[0];
                float _Split_EE0A8ED0_G_2 = _ScreenPosition_F96EC78_Out_0[1];
                float _Split_EE0A8ED0_B_3 = _ScreenPosition_F96EC78_Out_0[2];
                float _Split_EE0A8ED0_A_4 = _ScreenPosition_F96EC78_Out_0[3];
                float _Subtract_27535CEB_Out_2;
                Unity_Subtract_float(_Split_EE0A8ED0_A_4, 1, _Subtract_27535CEB_Out_2);
                float _Subtract_E6D9BA43_Out_2;
                Unity_Subtract_float(_SceneDepth_EC17DC7_Out_1, _Subtract_27535CEB_Out_2, _Subtract_E6D9BA43_Out_2);
                float _Property_733ABBF7_Out_0 = Vector1_AEA0383C;
                float _Divide_89651C50_Out_2;
                Unity_Divide_float(_Subtract_E6D9BA43_Out_2, _Property_733ABBF7_Out_0, _Divide_89651C50_Out_2);
                float _Saturate_C2839B81_Out_1;
                Unity_Saturate_float(_Divide_89651C50_Out_2, _Saturate_C2839B81_Out_1);
                float _Smoothstep_98CD0A08_Out_3;
                Unity_Smoothstep_float(0, 1, _Saturate_C2839B81_Out_1, _Smoothstep_98CD0A08_Out_3);
                float _Property_76F8A273_Out_0 = Vector1_3573D945;
                float _Multiply_DFBCE244_Out_2;
                Unity_Multiply_float(_Smoothstep_98CD0A08_Out_3, _Property_76F8A273_Out_0, _Multiply_DFBCE244_Out_2);
                surface.Alpha = _Multiply_DFBCE244_Out_2;
                surface.AlphaClipThreshold = 0;
                return surface;
            }
        
            // --------------------------------------------------
            // Structs and Packing
        
            // Generated Type: Attributes
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
        
            // Generated Type: Varyings
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Generated Type: PackedVaryings
            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                float3 interp00 : TEXCOORD0;
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Packed Type: Varyings
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output = (PackedVaryings)0;
                output.positionCS = input.positionCS;
                output.interp00.xyz = input.positionWS;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            
            // Unpacked Type: Varyings
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = input.positionCS;
                output.positionWS = input.interp00.xyz;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);
            
                output.ObjectSpaceNormal =           input.normalOS;
                output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
                output.ObjectSpaceTangent =          input.tangentOS;
                output.ObjectSpacePosition =         input.positionOS;
                output.WorldSpacePosition =          TransformObjectToWorld(input.positionOS);
                output.TimeParameters =              _TimeParameters.xyz;
            
                return output;
            }
            
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
            
            
            
            
            
                output.WorldSpacePosition =          input.positionWS;
                output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
            #else
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            #endif
            #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            
                return output;
            }
            
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
        
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags 
            { 
                "LightMode" = "DepthOnly"
            }
           
            // Render State
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            Cull Off
            ZTest LEqual
            ZWrite On
            ColorMask 0
            
        
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            // Pragmas
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing
        
            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>
            
            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define VARYINGS_NEED_POSITION_WS 
            #define FEATURES_GRAPH_VERTEX
            #pragma multi_compile_instancing
            #define SHADERPASS_DEPTHONLY
            #define REQUIRE_DEPTH_TEXTURE
            
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float Vector1_D30978F2;
            float Vector1_C815250C;
            float Vector1_C5F5659F;
            float4 Vector4_63521C71;
            float4 Color_55A7C162;
            float4 Color_FF6033CE;
            float Vector1_B1D97770;
            float Vector1_47EE67CD;
            float Vector1_4ACDCDD1;
            float Vector1_D2B62066;
            float Vector1_9B58639F;
            float Vector1_4C0FE8F1;
            float Vector1_B5A66061;
            float Vector1_9656DE2A;
            float Vector1_A1C6EE3C;
            float Vector1_2DEDB162;
            float Vector1_AEA0383C;
            float Vector1_3573D945;
            CBUFFER_END
        
            // Graph Functions
            
            void Unity_Distance_float3(float3 A, float3 B, out float Out)
            {
                Out = distance(A, B);
            }
            
            void Unity_Divide_float(float A, float B, out float Out)
            {
                Out = A / B;
            }
            
            void Unity_Power_float(float A, float B, out float Out)
            {
                Out = pow(A, B);
            }
            
            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }
            
            void Unity_Rotate_About_Axis_Degrees_float(float3 In, float3 Axis, float Rotation, out float3 Out)
            {
                Rotation = radians(Rotation);
            
                float s = sin(Rotation);
                float c = cos(Rotation);
                float one_minus_c = 1.0 - c;
                
                Axis = normalize(Axis);
            
                float3x3 rot_mat = { one_minus_c * Axis.x * Axis.x + c,            one_minus_c * Axis.x * Axis.y - Axis.z * s,     one_minus_c * Axis.z * Axis.x + Axis.y * s,
                                          one_minus_c * Axis.x * Axis.y + Axis.z * s,   one_minus_c * Axis.y * Axis.y + c,              one_minus_c * Axis.y * Axis.z - Axis.x * s,
                                          one_minus_c * Axis.z * Axis.x - Axis.y * s,   one_minus_c * Axis.y * Axis.z + Axis.x * s,     one_minus_c * Axis.z * Axis.z + c
                                        };
            
                Out = mul(rot_mat,  In);
            }
            
            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }
            
            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }
            
            
            float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }
            
            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            { 
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }
            
            void Unity_Add_float(float A, float B, out float Out)
            {
                Out = A + B;
            }
            
            void Unity_Saturate_float(float In, out float Out)
            {
                Out = saturate(In);
            }
            
            void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
            {
                RGBA = float4(R, G, B, A);
                RGB = float3(R, G, B);
                RG = float2(R, G);
            }
            
            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }
            
            void Unity_Absolute_float(float In, out float Out)
            {
                Out = abs(In);
            }
            
            void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
            {
                Out = smoothstep(Edge1, Edge2, In);
            }
            
            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }
            
            void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
            
            void Unity_Subtract_float(float A, float B, out float Out)
            {
                Out = A - B;
            }
        
            // Graph Vertex
            struct VertexDescriptionInputs
            {
                float3 ObjectSpaceNormal;
                float3 WorldSpaceNormal;
                float3 ObjectSpaceTangent;
                float3 ObjectSpacePosition;
                float3 WorldSpacePosition;
                float3 TimeParameters;
            };
            
            struct VertexDescription
            {
                float3 VertexPosition;
                float3 VertexNormal;
                float3 VertexTangent;
            };
            
            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                float _Distance_C3096CE2_Out_2;
                Unity_Distance_float3(SHADERGRAPH_OBJECT_POSITION, IN.WorldSpacePosition, _Distance_C3096CE2_Out_2);
                float _Property_5238734F_Out_0 = Vector1_9656DE2A;
                float _Divide_B3B86D7D_Out_2;
                Unity_Divide_float(_Distance_C3096CE2_Out_2, _Property_5238734F_Out_0, _Divide_B3B86D7D_Out_2);
                float _Power_B2F0308E_Out_2;
                Unity_Power_float(_Divide_B3B86D7D_Out_2, 3, _Power_B2F0308E_Out_2);
                float3 _Multiply_20699AFE_Out_2;
                Unity_Multiply_float(IN.WorldSpaceNormal, (_Power_B2F0308E_Out_2.xxx), _Multiply_20699AFE_Out_2);
                float _Property_A400740_Out_0 = Vector1_B1D97770;
                float _Property_21C64270_Out_0 = Vector1_47EE67CD;
                float3 _RotateAboutAxis_2909AAC4_Out_3;
                Unity_Rotate_About_Axis_Degrees_float(IN.WorldSpacePosition, float3 (1, 0, 0), 90, _RotateAboutAxis_2909AAC4_Out_3);
                float _Property_CD4F56B4_Out_0 = Vector1_C815250C;
                float _Multiply_EED0F979_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_CD4F56B4_Out_0, _Multiply_EED0F979_Out_2);
                float2 _TilingAndOffset_F096EC6A_Out_3;
                Unity_TilingAndOffset_float((_RotateAboutAxis_2909AAC4_Out_3.xy), float2 (1, 1), (_Multiply_EED0F979_Out_2.xx), _TilingAndOffset_F096EC6A_Out_3);
                float _Property_4C9FD3BF_Out_0 = Vector1_D30978F2;
                float _GradientNoise_3D6B045C_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_F096EC6A_Out_3, _Property_4C9FD3BF_Out_0, _GradientNoise_3D6B045C_Out_2);
                float2 _TilingAndOffset_7F106F56_Out_3;
                Unity_TilingAndOffset_float((_RotateAboutAxis_2909AAC4_Out_3.xy), float2 (1, 1), float2 (0, 0), _TilingAndOffset_7F106F56_Out_3);
                float _GradientNoise_DD58818E_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_7F106F56_Out_3, _Property_4C9FD3BF_Out_0, _GradientNoise_DD58818E_Out_2);
                float _Add_9782A73D_Out_2;
                Unity_Add_float(_GradientNoise_3D6B045C_Out_2, _GradientNoise_DD58818E_Out_2, _Add_9782A73D_Out_2);
                float _Divide_5EBAE61E_Out_2;
                Unity_Divide_float(_Add_9782A73D_Out_2, 2, _Divide_5EBAE61E_Out_2);
                float _Saturate_FAF6D122_Out_1;
                Unity_Saturate_float(_Divide_5EBAE61E_Out_2, _Saturate_FAF6D122_Out_1);
                float _Property_158AFEB7_Out_0 = Vector1_4ACDCDD1;
                float _Power_E2D4DAC1_Out_2;
                Unity_Power_float(_Saturate_FAF6D122_Out_1, _Property_158AFEB7_Out_0, _Power_E2D4DAC1_Out_2);
                float4 _Property_D258128B_Out_0 = Vector4_63521C71;
                float _Split_F1ED60DD_R_1 = _Property_D258128B_Out_0[0];
                float _Split_F1ED60DD_G_2 = _Property_D258128B_Out_0[1];
                float _Split_F1ED60DD_B_3 = _Property_D258128B_Out_0[2];
                float _Split_F1ED60DD_A_4 = _Property_D258128B_Out_0[3];
                float4 _Combine_C7B329CD_RGBA_4;
                float3 _Combine_C7B329CD_RGB_5;
                float2 _Combine_C7B329CD_RG_6;
                Unity_Combine_float(_Split_F1ED60DD_R_1, _Split_F1ED60DD_G_2, 0, 0, _Combine_C7B329CD_RGBA_4, _Combine_C7B329CD_RGB_5, _Combine_C7B329CD_RG_6);
                float4 _Combine_7CF173C7_RGBA_4;
                float3 _Combine_7CF173C7_RGB_5;
                float2 _Combine_7CF173C7_RG_6;
                Unity_Combine_float(_Split_F1ED60DD_B_3, _Split_F1ED60DD_A_4, 0, 0, _Combine_7CF173C7_RGBA_4, _Combine_7CF173C7_RGB_5, _Combine_7CF173C7_RG_6);
                float _Remap_449CA72D_Out_3;
                Unity_Remap_float(_Power_E2D4DAC1_Out_2, _Combine_C7B329CD_RG_6, _Combine_7CF173C7_RG_6, _Remap_449CA72D_Out_3);
                float _Absolute_F490C4D_Out_1;
                Unity_Absolute_float(_Remap_449CA72D_Out_3, _Absolute_F490C4D_Out_1);
                float _Smoothstep_B39D056A_Out_3;
                Unity_Smoothstep_float(_Property_A400740_Out_0, _Property_21C64270_Out_0, _Absolute_F490C4D_Out_1, _Smoothstep_B39D056A_Out_3);
                float _Property_8300113F_Out_0 = Vector1_9B58639F;
                float _Multiply_3291FAD1_Out_2;
                Unity_Multiply_float(IN.TimeParameters.x, _Property_8300113F_Out_0, _Multiply_3291FAD1_Out_2);
                float2 _TilingAndOffset_B5767666_Out_3;
                Unity_TilingAndOffset_float((_RotateAboutAxis_2909AAC4_Out_3.xy), float2 (1, 1), (_Multiply_3291FAD1_Out_2.xx), _TilingAndOffset_B5767666_Out_3);
                float _Property_EC8A8B29_Out_0 = Vector1_D2B62066;
                float _GradientNoise_8870EA33_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_B5767666_Out_3, _Property_EC8A8B29_Out_0, _GradientNoise_8870EA33_Out_2);
                float _Property_F16589D3_Out_0 = Vector1_4C0FE8F1;
                float _Multiply_3195E5E2_Out_2;
                Unity_Multiply_float(_GradientNoise_8870EA33_Out_2, _Property_F16589D3_Out_0, _Multiply_3195E5E2_Out_2);
                float _Add_524CA4AD_Out_2;
                Unity_Add_float(_Smoothstep_B39D056A_Out_3, _Multiply_3195E5E2_Out_2, _Add_524CA4AD_Out_2);
                float _Add_77813DD5_Out_2;
                Unity_Add_float(1, _Property_F16589D3_Out_0, _Add_77813DD5_Out_2);
                float _Divide_E8314B3F_Out_2;
                Unity_Divide_float(_Add_524CA4AD_Out_2, _Add_77813DD5_Out_2, _Divide_E8314B3F_Out_2);
                float3 _Multiply_AFF36AB3_Out_2;
                Unity_Multiply_float(IN.ObjectSpaceNormal, (_Divide_E8314B3F_Out_2.xxx), _Multiply_AFF36AB3_Out_2);
                float _Property_7B73D722_Out_0 = Vector1_C5F5659F;
                float3 _Multiply_67A088E9_Out_2;
                Unity_Multiply_float(_Multiply_AFF36AB3_Out_2, (_Property_7B73D722_Out_0.xxx), _Multiply_67A088E9_Out_2);
                float3 _Add_B71DD03A_Out_2;
                Unity_Add_float3(IN.ObjectSpacePosition, _Multiply_67A088E9_Out_2, _Add_B71DD03A_Out_2);
                float3 _Add_776BADD9_Out_2;
                Unity_Add_float3(_Multiply_20699AFE_Out_2, _Add_B71DD03A_Out_2, _Add_776BADD9_Out_2);
                description.VertexPosition = _Add_776BADD9_Out_2;
                description.VertexNormal = IN.ObjectSpaceNormal;
                description.VertexTangent = IN.ObjectSpaceTangent;
                return description;
            }
            
            // Graph Pixel
            struct SurfaceDescriptionInputs
            {
                float3 WorldSpacePosition;
                float4 ScreenPosition;
            };
            
            struct SurfaceDescription
            {
                float Alpha;
                float AlphaClipThreshold;
            };
            
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float _SceneDepth_EC17DC7_Out_1;
                Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_EC17DC7_Out_1);
                float4 _ScreenPosition_F96EC78_Out_0 = IN.ScreenPosition;
                float _Split_EE0A8ED0_R_1 = _ScreenPosition_F96EC78_Out_0[0];
                float _Split_EE0A8ED0_G_2 = _ScreenPosition_F96EC78_Out_0[1];
                float _Split_EE0A8ED0_B_3 = _ScreenPosition_F96EC78_Out_0[2];
                float _Split_EE0A8ED0_A_4 = _ScreenPosition_F96EC78_Out_0[3];
                float _Subtract_27535CEB_Out_2;
                Unity_Subtract_float(_Split_EE0A8ED0_A_4, 1, _Subtract_27535CEB_Out_2);
                float _Subtract_E6D9BA43_Out_2;
                Unity_Subtract_float(_SceneDepth_EC17DC7_Out_1, _Subtract_27535CEB_Out_2, _Subtract_E6D9BA43_Out_2);
                float _Property_733ABBF7_Out_0 = Vector1_AEA0383C;
                float _Divide_89651C50_Out_2;
                Unity_Divide_float(_Subtract_E6D9BA43_Out_2, _Property_733ABBF7_Out_0, _Divide_89651C50_Out_2);
                float _Saturate_C2839B81_Out_1;
                Unity_Saturate_float(_Divide_89651C50_Out_2, _Saturate_C2839B81_Out_1);
                float _Smoothstep_98CD0A08_Out_3;
                Unity_Smoothstep_float(0, 1, _Saturate_C2839B81_Out_1, _Smoothstep_98CD0A08_Out_3);
                float _Property_76F8A273_Out_0 = Vector1_3573D945;
                float _Multiply_DFBCE244_Out_2;
                Unity_Multiply_float(_Smoothstep_98CD0A08_Out_3, _Property_76F8A273_Out_0, _Multiply_DFBCE244_Out_2);
                surface.Alpha = _Multiply_DFBCE244_Out_2;
                surface.AlphaClipThreshold = 0;
                return surface;
            }
        
            // --------------------------------------------------
            // Structs and Packing
        
            // Generated Type: Attributes
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
        
            // Generated Type: Varyings
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Generated Type: PackedVaryings
            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                float3 interp00 : TEXCOORD0;
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Packed Type: Varyings
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output = (PackedVaryings)0;
                output.positionCS = input.positionCS;
                output.interp00.xyz = input.positionWS;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            
            // Unpacked Type: Varyings
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = input.positionCS;
                output.positionWS = input.interp00.xyz;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);
            
                output.ObjectSpaceNormal =           input.normalOS;
                output.WorldSpaceNormal =            TransformObjectToWorldNormal(input.normalOS);
                output.ObjectSpaceTangent =          input.tangentOS;
                output.ObjectSpacePosition =         input.positionOS;
                output.WorldSpacePosition =          TransformObjectToWorld(input.positionOS);
                output.TimeParameters =              _TimeParameters.xyz;
            
                return output;
            }
            
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
            
            
            
            
            
                output.WorldSpacePosition =          input.positionWS;
                output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
            #else
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            #endif
            #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            
                return output;
            }
            
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
        
            ENDHLSL
        }
        
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}
