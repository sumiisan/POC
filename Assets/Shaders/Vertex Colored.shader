Shader "Custom/Vertex Colored" {
Properties {
//    _MainTex ("Base (RGB)", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1,1)
//    _Extrusion ("Extrusion Amount", Range(-1,1)) = 0
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 150

CGPROGRAM
#pragma surface surf Lambert vertex:vert noforwardadd
#pragma target 3.0

//sampler2D _MainTex;
fixed4 _Color;
//float _Extrusion;

struct Input {
//    float2 uv_MainTex;
    float3 vertexColor; // Vertex color stored here by vert() method
};

void vert (inout appdata_full v, out Input o) {
     UNITY_INITIALIZE_OUTPUT(Input,o);
     o.vertexColor = v.color; // Save the Vertex Color in the Input for the surf() method
//     v.vertex.xyz += v.normal * _Extrusion;
}

void surf (Input IN, inout SurfaceOutput o) {
    /*fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    o.Albedo = c.rgb * IN.vertexColor;
    o.Alpha = c.a;
    */
    o.Albedo = IN.vertexColor;
    o.Alpha = 1;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}