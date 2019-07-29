using System;
using UnityEngine;

namespace UIForia.Rendering {

    public class UIForiaPropertyBlock {

        public readonly int size;
        public readonly Material material;
        public readonly MaterialPropertyBlock matBlock;
        public readonly Matrix4x4[] transformData;
        public readonly Vector4[] colorData;
        public readonly Vector4[] objectData;

        public static readonly int s_TransformDataKey = Shader.PropertyToID("_TransformData");
        public static readonly int s_ColorDataKey = Shader.PropertyToID("_ColorData");
        public static readonly int s_ObjectDataKey = Shader.PropertyToID("_ObjectData");
        public static readonly int s_FontDataScales = Shader.PropertyToID("_FontScales");
        public static readonly int s_FontTextureSize = Shader.PropertyToID("_FontTextureSize");
        public static readonly int s_FontTexture = Shader.PropertyToID("_FontTexture");
        public static readonly int s_MainTextureKey = Shader.PropertyToID("_MainTexture");

        public UIForiaPropertyBlock(Material material, int size) {
            this.size = size;
            this.material = material;
            this.matBlock = new MaterialPropertyBlock();
            this.transformData = new Matrix4x4[size];
            this.colorData = new Vector4[size];
            this.objectData = new Vector4[size];
        }

        public void SetData(UIForiaData data) {
            Array.Copy(data.transformData.array, 0, transformData, 0, data.transformData.size);
            Array.Copy(data.colors.array, 0, colorData, 0, data.colors.size);
            Array.Copy(data.objectData0.array, 0, objectData, 0, data.objectData0.size);

            matBlock.SetMatrixArray(s_TransformDataKey, transformData);
            matBlock.SetVectorArray(s_ColorDataKey, colorData);
            matBlock.SetVectorArray(s_ObjectDataKey, objectData);

            if (data.mainTexture != null) {
                material.SetTexture(s_MainTextureKey, data.mainTexture);
            }

            if (data.fontData.fontAsset != null) {
                FontData fontData = data.fontData;
                matBlock.SetVector(s_FontDataScales, new Vector4(fontData.gradientScale, fontData.scaleRatioA, fontData.scaleRatioB, fontData.scaleRatioC));
                matBlock.SetVector(s_FontTextureSize, new Vector4(fontData.textureWidth, fontData.textureHeight, 0, 0));
                matBlock.SetTexture(s_FontTexture, fontData.fontAsset.atlas);
            }
        }

    }

}