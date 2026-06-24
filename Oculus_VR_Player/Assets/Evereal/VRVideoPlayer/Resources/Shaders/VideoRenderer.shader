// Copyright (c) 2017-present Evereal. All rights reserved.

Shader "VRVideoPlayer/VideoRenderer"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "black" {}
        _StereoMode("Stereo Mode", Int) = 0
        _FlipX("Flip X", Int) = 0
        _FlipY("Flip Y", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Begin code to support _StereoMode parameter
            static const int _StereoModeNone = 0;
            static const int _StereoModeTopBottom = 1;
            static const int _StereoModeLeftRight = 2;
            int _StereoMode = _StereoModeNone;

            float2 ApplyVideoStereoModeTexCoord(float2 texcoord)
            {
                // The value of unity_StereoEyeIndex is:
                //  0 for left-eye rendering
                //  1 for right-eye rendering

                if (_StereoMode == _StereoModeLeftRight)
                {
                    texcoord.x /= 2.0f;
                    texcoord.x += 0.5f * unity_StereoEyeIndex;
                }
                else if (_StereoMode == _StereoModeTopBottom)
                {
                    texcoord.y /= 2.0f;
                    texcoord.y += (-0.5f * (unity_StereoEyeIndex - 1.0f));
                }
                return texcoord;
            }
            // End code to support _StereoMode parameter

            // Support video flip parameter
            int _FlipX;
            int _FlipY;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Apply tiling/offset first
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                // Set video flip x
                if (_FlipX == 1)
                {
                    o.texcoord.x = 1 - o.texcoord.x;
                }
                // Set video flip y
                if (_FlipY == 1)
                {
                    o.texcoord.y = 1 - o.texcoord.y;
                }
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                // Apply stereo mode after flips, so eyes aren't swapped
                float2 uv = ApplyVideoStereoModeTexCoord(i.texcoord);
                
                fixed4 col = tex2D(_MainTex, uv);
                UNITY_APPLY_FOG(i.fogCoord, col);
                UNITY_OPAQUE_ALPHA(col.a);
                return col;
            }
            ENDCG
        }
    }
}
