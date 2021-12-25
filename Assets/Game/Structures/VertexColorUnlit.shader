Shader "Custom/VertexColorUnlit"
{
    Properties{
    }

    Category{
        Tags { 
            "Queue" = "Geometry"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }
        Lighting Off
        BindChannels {
        Bind "Color", color
        Bind "Vertex", vertex
    }

    SubShader {
            Pass {
                
            }
        }
    }

}
