Shader "LXZ/GlassBlur" {
        Properties {
                _MainTex ("Base (RGB)", 2D) = "white" {}
                _fSampleDist("SampleDist", Float) = 1.06
				_Color("Color",Color) =  (0.6,0.6,0.6,1)
        }
        SubShader {
                Pass {                 
                        CGPROGRAM
                        #pragma vertex vert
                        #pragma fragment frag
                        //#pragma fragmentoption ARB_precision_hint_fastest
        
                        #include "UnityCG.cginc"
        
                        struct appdata_t {
                                float4 vertex : POSITION;
                                half2 texcoord : TEXCOORD0;
                        };
        
                        struct v2f {
                                float4 vertex : SV_POSITION;
                                half2 texcoord : TEXCOORD0;
                        };
                        
                        float4 _MainTex_ST;
                        float _X;
						float _Y;
						float4 _Color; 
						
                        v2f vert (appdata_t v)
                        {
                                v2f o;
                                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                                return o;
                        }
        
                        sampler2D _MainTex;
                        float _fSampleDist; 

                        // some sample positions   
                        static const half2 samples[8] =  
                        {   
                           half2(-0.005,0.005),
						   half2(0,0.005),
						   half2(0.005,0.005),
						   half2(0.005,0),

						   half2(0.005,-0.005),
						   half2(0,-0.005),
						   half2(-0.005,-0.005),
						   half2(-0.005,0),
                        }; 
                        
                        half4 frag (v2f i) : COLOR
                        { 
                           half2 texcoord = i.texcoord;  
 
                           half4 color = tex2D(_MainTex, texcoord);   
                    
                            for (int i = 0; i < 8; ++i)  
                            {   
                              color += tex2D(_MainTex, texcoord + samples[i]* _fSampleDist);
                           }   
                      
                           color *=0.111f;    
 
                           return  color *_Color ;
                        }
                        ENDCG 
                }
        } 
}
