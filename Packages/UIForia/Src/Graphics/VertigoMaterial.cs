using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UIForia.Util;
using UnityEngine;

namespace Vertigo {

    public class VertigoMaterial {

        public string shaderName;
        public readonly Material material;
        internal readonly string[] keywords;
        internal readonly VertigoMaterial parent;
        private StructList<MaterialProperty> properties;
        private StructList<int> passes = new StructList<int>(1);

        internal readonly LightList<VertigoMaterial> instances;
        internal bool isActive;

        public bool isShared { get; set; }

        public VertigoMaterial(Material material, bool isShared) {
            this.properties = new StructList<MaterialProperty>();
            this.material = material;
            this.isShared = isShared;
            this.keywords = null;
            this.isActive = true;
            this.parent = null;
            this.instances = null;
        }

        internal VertigoMaterial(Material material, IList<string> keywords) {
            this.material = material;
            this.instances = new LightList<VertigoMaterial>(4);
            this.isActive = true;
            if (keywords == null) {
                this.keywords = null;
            }
            else {
                this.keywords = new string[keywords.Count];
                for (int i = 0; i < keywords.Count; i++) {
                    this.keywords[i] = keywords[i];
                }
            }
        }

        internal VertigoMaterial(VertigoMaterial parent) {
            this.material = new Material(parent.material);
            this.keywords = parent.keywords;
            this.parent = parent;
        }

        public Material GetMaterial() {
            if (material != null) {
                WriteToMaterial(this.material);
            }
            else {
                // material = MaterialPool.Get(shaderName, keywords);
            }

            WriteToMaterial(material);
            return material;
        }

        internal void WriteToMaterial(Material material) {
            material.SetInt(ShaderKey.StencilRef, renderSettings.stencilRefValue);
            material.SetInt(ShaderKey.StencilReadMask, renderSettings.stencilReadMask);
            material.SetInt(ShaderKey.StencilWriteMask, renderSettings.stencilWriteMask);
            material.SetInt(ShaderKey.StencilComp, (int) renderSettings.stencilComp);
            material.SetInt(ShaderKey.StencilPassOp, (int) renderSettings.stencilPassOp);
            material.SetInt(ShaderKey.StencilFailOp, (int) renderSettings.stencilFailOp);
            material.SetInt(ShaderKey.ColorMask, renderSettings.colorMask);
            material.SetInt(ShaderKey.Culling, renderSettings.cullMode);
            material.SetInt(ShaderKey.ZWrite, renderSettings.zWrite ? 1 : 0);
            material.SetInt(ShaderKey.BlendArgSrc, (int) renderSettings.blendArgSrc);
            material.SetInt(ShaderKey.BlendArgDst, (int) renderSettings.blendArgDst);

            int propertyCount = properties.size;
            MaterialProperty[] propertyList = properties.array;
            for (int i = 0; i < propertyCount; i++) {
                switch (propertyList[i].type) {
                    case PropertyType.Int:
                        material.SetInt(propertyList[i].key, propertyList[i].intVal);
                        break;
                    case PropertyType.Float:
                        material.SetFloat(propertyList[i].key, propertyList[i].floatVal);
                        break;
                    case PropertyType.Texture:
                        material.SetTexture(propertyList[i].key, propertyList[i].textureValue);
                        break;
                    case PropertyType.Color:
                        material.SetColor(propertyList[i].key, propertyList[i].colorValue);
                        break;
                    case PropertyType.Vector:
                        material.SetVector(propertyList[i].key, propertyList[i].vectorValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        internal void WriteToPropertyBlock(MaterialPropertyBlock block) {
            int propertyCount = properties.size;
            MaterialProperty[] propertyList = properties.array;
            for (int i = 0; i < propertyCount; i++) {
                switch (propertyList[i].type) {
                    case PropertyType.Int:
                        block.SetInt(propertyList[i].key, propertyList[i].intVal);
                        break;
                    case PropertyType.Float:
                        block.SetFloat(propertyList[i].key, propertyList[i].floatVal);
                        break;
                    case PropertyType.Texture:
                        block.SetTexture(propertyList[i].key, propertyList[i].textureValue);
                        break;
                    case PropertyType.Color:
                        block.SetColor(propertyList[i].key, propertyList[i].colorValue);
                        break;
                    case PropertyType.Vector:
                        block.SetVector(propertyList[i].key, propertyList[i].vectorValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool TexturesMatch(VertigoMaterial other) {
            return true;
        }


        private void SetProperty(in MaterialProperty property) {
            MaterialProperty[] values = properties.array;
            int count = properties.size;

            for (int i = 0; i < count; i++) {
                if (values[i].key == property.key) {
                    values[i] = property;
                    return;
                }
            }

            for (int i = 0; i < count; i++) {
                if (property.key < values[i].key) {
                    properties.Insert(i, property);
                    return;
                }
            }

            properties.Add(property);
        }

        public void SetTextureProperty(int key, Texture texture) {
            SetProperty(new MaterialProperty(key, texture));
        }

        public void SetIntProperty(string key, int value) {
            SetProperty(new MaterialProperty(Shader.PropertyToID(key), value));
        }

        public void SetIntProperty(int key, int value) {
            SetProperty(new MaterialProperty(key, value));
        }

        public void SetFloatProperty(int key, float value) {
            SetProperty(new MaterialProperty(key, value));
        }

        public void SetFloatProperty(string key, float value) {
            SetProperty(new MaterialProperty(Shader.PropertyToID(key), value));
        }

        public void SetVectorProperty(int key, in Vector4 value) {
            SetProperty(new MaterialProperty(key, value));
        }

        public void SetVectorProperty(string key, in Vector4 value) {
            SetProperty(new MaterialProperty(Shader.PropertyToID(key), value));
        }

        public void SetColorProperty(int key, in Color32 value) {
            SetProperty(new MaterialProperty(key, value));
        }

        public void SetColorProperty(string key, in Color32 value) {
            SetProperty(new MaterialProperty(Shader.PropertyToID(key), value));
        }

        public void Clear() {
            properties.QuickClear();
            renderSettings = new RenderSettings(); // todo -- find the defaults
        }

        public VertigoMaterial Clone(VertigoMaterial retn = null) {
            if (retn == null) {
                // retn = new VertigoMaterial(null);
            }
            else {
                retn.Clear();
            }

            retn.renderSettings = renderSettings;
//            retn.keywords = keywords;
            retn.properties.AddRange(properties);

            return retn;
        }

        private static IntMap<MaterialSettings> settingsMap;

        public struct MaterialSettings {

            public Material materialBase;
            public string[] keywords;

        }

        public static VertigoMaterial GetInstance(Material material) {
            if (settingsMap.TryGetValue(material.shader.GetInstanceID(), out MaterialSettings settings)) { }

            material.CopyPropertiesFromMaterial(material);
//            VertigoMaterial retn = null;
//            if (instances.Count > 0) {
//                retn = instances.RemoveLast();
//            }
//            else {
//                retn = new VertigoMaterial(this);
//            }
//
//            retn.isActive = true;
//            return retn;
            return null;
        }

        public void Release() {
            if (isActive) {
                parent?.instances.Add(this);
                isActive = false;
            }
        }

        public void SetMainTexture(Texture texture) {
            material.mainTexture = texture;
        }

        internal RenderSettings renderSettings;

        public void SetStencil(byte refVal, byte readMask, byte writeMask, CompareFunction compareFunction) {
            renderSettings.stencilRefValue = refVal;
            renderSettings.stencilReadMask = readMask;
            renderSettings.stencilWriteMask = writeMask;
            renderSettings.stencilComp = compareFunction;
        }

        public bool RenderStateMatches(VertigoMaterial other) {
            return renderSettings.IsEqualTo(other.renderSettings);
        }

        public bool KeywordsMatch(VertigoMaterial other) {
            if (keywords.Length != other.keywords.Length) {
                return false;
            }

            for (int i = 0; i < keywords.Length; i++) {
                if (keywords[i] != other.keywords[i]) {
                    return false;
                }
            }

            return true;
        }

        public bool PassesMatch(VertigoMaterial other) {
            if (passes.size != other.passes.size) {
                return false;
            }

            for (int i = 0; i < passes.size; i++) {
                if (passes[i] != other.passes[i]) {
                    return false;
                }
            }

            return true;
        }

        private enum PropertyType {

            Int,
            Float,
            Texture,
            Color,
            Vector

        }

        [StructLayout(LayoutKind.Explicit)]
        private struct MaterialProperty {

            [FieldOffset(0)] public readonly PropertyType type;
            [FieldOffset(4)] public readonly int key;
            [FieldOffset(8)] public readonly Texture textureValue;
            [FieldOffset(12)] public readonly Color32 colorValue;
            [FieldOffset(12)] public readonly float floatVal;
            [FieldOffset(12)] public readonly int intVal;
            [FieldOffset(12)] public readonly Vector4 vectorValue;

            public MaterialProperty(int key, Texture value) {
                this.type = PropertyType.Texture;
                this.key = key;
                this.colorValue = default;
                this.floatVal = default;
                this.intVal = default;
                this.vectorValue = default;
                this.textureValue = value;
            }

            public MaterialProperty(int key, float value) {
                this.type = PropertyType.Float;
                this.key = key;
                this.colorValue = default;
                this.intVal = default;
                this.vectorValue = default;
                this.textureValue = default;
                this.floatVal = value;
            }

            public MaterialProperty(int key, int value) {
                this.type = PropertyType.Int;
                this.key = key;
                this.colorValue = default;
                this.floatVal = default;
                this.vectorValue = default;
                this.textureValue = default;
                this.intVal = value;
            }

            public MaterialProperty(int key, Color32 value) {
                this.type = PropertyType.Color;
                this.key = key;
                this.floatVal = default;
                this.intVal = default;
                this.vectorValue = default;
                this.textureValue = default;
                this.colorValue = value;
            }

            public MaterialProperty(int key, Vector4 value) {
                this.type = PropertyType.Vector;
                this.key = key;
                this.colorValue = default;
                this.floatVal = default;
                this.intVal = default;
                this.textureValue = default;
                this.vectorValue = value;
            }

        }

    }

}