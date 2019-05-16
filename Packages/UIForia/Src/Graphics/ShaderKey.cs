using UnityEngine;

namespace Vertigo {

    public static class ShaderKey {

        public static readonly int FontAtlas = Shader.PropertyToID("_FontTexture");
        public static readonly int StencilRef = Shader.PropertyToID("_Stencil");
        public static readonly int StencilReadMask = Shader.PropertyToID("_StencilReadMask");
        public static readonly int StencilWriteMask = Shader.PropertyToID("_StencilWriteMask");
        public static readonly int StencilComp = Shader.PropertyToID("_StencilComp");
        public static readonly int StencilPassOp = Shader.PropertyToID("_StencilOp");
        public static readonly int StencilFailOp = Shader.PropertyToID("_StencilOpFail");
        public static readonly int ColorMask = Shader.PropertyToID("_ColorMask");
        public static readonly int Culling = Shader.PropertyToID("_Culling");
        public static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
        public static readonly int BlendArgSrc = Shader.PropertyToID("_SrcBlend ");
        public static readonly int BlendArgDst = Shader.PropertyToID("_DstBlend ");
        public static readonly int MaskTexture = Shader.PropertyToID("_MaskTexture");
        public static readonly int MaskSoftness = Shader.PropertyToID("_MaskSoftness");
        public static readonly int MainTexture = Shader.PropertyToID("_MainTex");

    }

}