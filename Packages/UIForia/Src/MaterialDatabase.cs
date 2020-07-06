using System;
using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public struct MaterialDefinition {

        public string assetPath;
        public string materialName;
        public string[] keywords; // -> map to feature flags
        public MaterialPropertyDefinition[] properties;
        public AssetLoadMethod loadMethod;

    }

    public struct MaterialInfo {

        public string assetPath;
        public Material material;
        public string materialName;
        public RangeInt floatProperties;
        public RangeInt textureProperties;
        public RangeInt vectorProperties;
        public RangeInt colorProperties;
        public MaterialPropertyDefinition[] properties;
        public MaterialId materialId;
        public AssetLoadState loadState;
        public bool supportsColorMask;

    }

    [Flags]
    public enum TextMaterialFeatures {

        Default = 0,
        Underlay = 1 << 0,
        Glow = 1 << 1,
        FaceTextures = 1 << 2,
        Effect = 1 << 3,
        Bevel = 1 << 4
        
    }
    
    public struct MaterialId {

        internal readonly int index;

        public static readonly MaterialId UIForiaShape = new MaterialId(1);
        public static readonly MaterialId UIForiaSDFText = new MaterialId(2);
        public static MaterialId UIForiaSDFTextEffect = new MaterialId(3);
        public static MaterialId UIForiaSoftGeometryMask = new MaterialId(4);

        internal MaterialId(int index) {
            this.index = index;
        }

    }

    internal struct TextMaterialInfo {

        public MaterialId materialId;
        public TextMaterialFeatures features;

    }
    
    public class MaterialDatabase2 {

        private MaterialInfo[] materialMap;
        private Dictionary<string, int> nameMap;
        private int materialCount;

        private static readonly PropertyComparer s_Comparer = new PropertyComparer();

        private static readonly MaterialPropertyDefinition[] s_EmptyPropertyArray = { };

        internal StructList<TextMaterialInfo> textMaterialList;
        public MaterialDatabase2() {
            this.materialMap = new MaterialInfo[16];
            this.textMaterialList = new StructList<TextMaterialInfo>();
            this.nameMap = new Dictionary<string, int>();
            this.materialCount = 1; // skip 0 index
        }
        
        internal MaterialId GetTextMaterial(TextMaterialFeatures features) {
            
            if (features == 0) {
                return MaterialId.UIForiaSDFText;
            }
            
            for (int i = 0; i < textMaterialList.size; i++) {
                if (textMaterialList.array[i].features == features) {
                    return textMaterialList.array[i].materialId;
                }
            }

            Material material = new Material(GetMaterial(MaterialId.UIForiaSDFText));
            
            if ((features & TextMaterialFeatures.Underlay) != 0) {
                material.EnableKeyword("USE_UNDERLAY");
            }
            
            if ((features & TextMaterialFeatures.Glow) != 0) {
                material.EnableKeyword("USE_GLOW");
            }

            if ((features & TextMaterialFeatures.FaceTextures) != 0) {
                material.EnableKeyword("USE_FACE_TEXTURES");
            }

            if ((features & TextMaterialFeatures.Bevel) != 0) {
                material.EnableKeyword("USE_BEVEL");
            }
            
            MaterialId materialId = new MaterialId(materialCount);
            if (materialCount + 1 >= materialMap.Length) {
                Array.Resize(ref materialMap, materialMap.Length * 2);
            }

            materialMap[materialCount++] = new MaterialInfo() {
                material = material,
                properties = s_EmptyPropertyArray,
                materialId = materialId,
                loadState = AssetLoadState.Loaded
            };

            textMaterialList.Add(new TextMaterialInfo() {
                features = features,
                materialId = materialId
            });
            
            return materialId;
        }

        public Material GetMaterial(MaterialId materialId) {
            if (materialId.index <= 0 || materialId.index > materialCount) {
                return null;
            }

            return materialMap[materialId.index].material;
        }

        internal bool TryGetMaterialInfo(MaterialId materialId, out MaterialInfo materialInfo) {

            if (materialId.index <= 0 || materialId.index > materialCount) {
                materialInfo = default;
                return false;
            }

            materialInfo = materialMap[materialId.index];
            return true;
        }

        public MaterialId TryRegisterMaterial(MaterialDefinition materialDefinition) {

            if (nameMap.TryGetValue(materialDefinition.materialName, out int _)) {
                return default;
            }

            MaterialInfo materialInfo = new MaterialInfo();

            MaterialId materialId = new MaterialId(materialCount);
            nameMap.Add(materialDefinition.materialName, materialCount);

            materialInfo.assetPath = materialDefinition.assetPath;
            materialInfo.materialName = materialDefinition.materialName;
            materialInfo.properties = materialDefinition.properties;
            materialInfo.materialId = materialId;

            Array.Sort(materialInfo.properties, s_Comparer);

            materialInfo.floatProperties = FindPropertyRange(materialInfo.properties, MaterialPropertyType.Float);
            materialInfo.textureProperties = FindPropertyRange(materialInfo.properties, MaterialPropertyType.Texture);
            materialInfo.colorProperties = FindPropertyRange(materialInfo.properties, MaterialPropertyType.Color);
            materialInfo.vectorProperties = FindPropertyRange(materialInfo.properties, MaterialPropertyType.Vector);

            if (materialCount + 1 >= materialMap.Length) {
                Array.Resize(ref materialMap, materialMap.Length * 2);
            }

            materialMap[materialCount] = materialInfo;

            materialCount++;
            return materialId;

        }

        private static RangeInt FindPropertyRange(MaterialPropertyDefinition[] properties, MaterialPropertyType propertyType) {
            int start = -1;
            for (int i = 0; i < properties.Length; i++) {
                if (properties[i].propertyType == propertyType) {
                    start = i;
                    break;
                }
            }

            if (start == -1) {
                return default;
            }

            int count = 1;
            for (int i = start + 1; i < properties.Length; i++) {
                if (properties[i].propertyType != propertyType) {
                    break;
                }

                count++;
            }

            return new RangeInt(start, count);

        }

        public bool HasTextureProperty(MaterialId materialId, string propertyName) {
            return HasTextureProperty(materialId, Shader.PropertyToID(propertyName));
        }

        public bool HasTextureProperty(MaterialId materialId, int propertyKey) {
            if (materialId.index <= 0 || materialId.index > materialCount) {
                return false;
            }

            ref MaterialInfo materialDef = ref materialMap[materialId.index];
            int start = materialDef.textureProperties.start;
            int end = materialDef.textureProperties.end;
            MaterialPropertyDefinition[] properties = materialDef.properties;
            for (int i = start; i < end; i++) {
                if (properties[i].shaderPropertyId == propertyKey) {
                    return true;
                }
            }

            return false;
        }

        public bool HasFloatProperty(MaterialId materialId, int propertyKey) {
            if (materialId.index <= 0 || materialId.index > materialCount) {
                return false;
            }

            ref MaterialInfo materialDef = ref materialMap[materialId.index];
            int start = materialDef.floatProperties.start;
            int end = materialDef.floatProperties.end;
            MaterialPropertyDefinition[] properties = materialDef.properties;
            for (int i = start; i < end; i++) {
                if (properties[i].shaderPropertyId == propertyKey) {
                    return true;
                }
            }

            return false;
        }

        private class PropertyComparer : IComparer<MaterialPropertyDefinition> {

            public int Compare(MaterialPropertyDefinition x, MaterialPropertyDefinition y) {
                if (x.propertyType != y.propertyType) {
                    return x.propertyType - y.propertyType;
                }

                return string.Compare(x.propertyName, y.propertyName, StringComparison.Ordinal);
            }

        }

        public MaterialId GetMaterialId(string materialName) {
            if (nameMap.TryGetValue(materialName, out int index)) {
                return new MaterialId(index);
            }

            return default;
        }

        public void UpdateMaterialInfo(MaterialId materialId, MaterialInfo materialInfo) {
            if (materialId.index <= 0 || materialId.index > materialCount) {
                return;
            }

            materialMap[materialId.index] = materialInfo;
        }

    }
    //
    // public class MaterialDatabase {
    //
    //     private MaterialInfo[] baseMaterialInfos;
    //     private MaterialPropertyInfo[] materialProperties;
    //     public Dictionary<long, MaterialInfo> materialMap;
    //     private int materialIdGenerator;
    //     private Dictionary<int, LightList<MaterialProperty>> instanceProperties;
    //
    //     private MaterialDefinition[] materialMap2;
    //
    //     public void AddMaterial(MaterialDefinition materialDefinition) { }
    //
    //     public MaterialDatabase(MaterialInfo[] baseMaterialInfos, MaterialPropertyInfo[] materialProperties) {
    //         materialIdGenerator = 1;
    //         this.baseMaterialInfos = baseMaterialInfos;
    //         this.materialProperties = materialProperties;
    //         this.materialMap = new Dictionary<long, MaterialInfo>();
    //         this.instanceProperties = new Dictionary<int, LightList<MaterialProperty>>();
    //         for (int i = 0; i < baseMaterialInfos.Length; i++) {
    //             materialMap.Add(new MaterialId(i + 1, 0).id, baseMaterialInfos[i]);
    //         }
    //
    //     }
    //
    //     public void Destroy() {
    //
    //         foreach (KeyValuePair<long, MaterialInfo> kvp in materialMap) {
    //             MaterialId materialId = (MaterialId) kvp.Key;
    //             if (materialId.instanceId != 0) {
    //                 try {
    //                     UnityEngine.Object.Destroy(kvp.Value.material);
    //                 }
    //                 catch (Exception e) { }
    //             }
    //         }
    //
    //     }
    //
    //     public void OnElementDestroyed(UIElement element) {
    //         if (instanceProperties.TryGetValue(element.id.id, out LightList<MaterialProperty> list)) {
    //             list.Release();
    //             instanceProperties.Remove(element.id.id);
    //         }
    //     }
    //
    //     public bool TryGetBaseMaterialId(string idSpan, out MaterialId materialId) {
    //         for (int i = 0; i < baseMaterialInfos.Length; i++) {
    //             if (baseMaterialInfos[i].materialName == idSpan) {
    //                 materialId = new MaterialId(i + 1, 0);
    //                 return true;
    //             }
    //         }
    //
    //         materialId = default;
    //         return false;
    //
    //     }
    //
    //     public bool TryGetBaseMaterialId(CharSpan idSpan, out MaterialId materialId) {
    //         for (int i = 0; i < baseMaterialInfos.Length; i++) {
    //             if (baseMaterialInfos[i].materialName == idSpan) {
    //                 materialId = new MaterialId(i + 1, 0);
    //                 return true;
    //             }
    //         }
    //
    //         materialId = default;
    //         return false;
    //
    //     }
    //
    //     public bool TryGetMaterialProperty(MaterialId materialId, string propertySpan, out MaterialPropertyInfo materialPropertyInfo) {
    //
    //         MaterialInfo materialInfo = baseMaterialInfos[materialId.baseId - 1];
    //         for (int i = materialInfo.propertyRange.start; i < materialInfo.propertyRange.end; i++) {
    //             if (materialProperties[i].propertyName == propertySpan) {
    //                 materialPropertyInfo = materialProperties[i];
    //                 return true;
    //             }
    //         }
    //
    //         materialPropertyInfo = default;
    //         return false;
    //
    //     }
    //
    //     public bool TryGetMaterialProperty(MaterialId materialId, CharSpan propertySpan, out MaterialPropertyInfo materialPropertyInfo) {
    //         MaterialInfo materialInfo = baseMaterialInfos[materialId.baseId - 1];
    //         for (int i = materialInfo.propertyRange.start; i < materialInfo.propertyRange.end; i++) {
    //             if (materialProperties[i].propertyName == propertySpan) {
    //                 materialPropertyInfo = materialProperties[i];
    //                 return true;
    //             }
    //         }
    //
    //         materialPropertyInfo = default;
    //         return false;
    //
    //     }
    //
    //     // base materials (the ones we imported)
    //     // static materials (the ones we overrode in styles, if any)
    //     // instance materials (the ones we override per element (also animations) )
    //
    //     internal bool HasFloatProperty(in MaterialInfo materialInfo, string key) {
    //         return false;
    //     }
    //
    //     internal bool HasTextureProperty(in MaterialInfo materialInfo, string key) {
    //         return false;
    //     }
    //
    //     internal bool HasTextureProperty(MaterialId materialId, string key) {
    //         return false;
    //     }
    //
    //     internal bool HasTextureProperty(MaterialId materialId, int key) {
    //         return false;
    //     }
    //
    //     internal bool HasVectorProperty(in MaterialInfo materialInfo, string key) {
    //         return false;
    //     }
    //
    //     internal bool HasColorProperty(in MaterialInfo materialInfo, string key) {
    //         return false;
    //     }
    //
    //     public MaterialId CreateStaticMaterialOverride(MaterialId baseId, IList<MaterialValueOverride> propertyOverrides) {
    //
    //         // get the base material (for now always a defined one)
    //
    //         // make sure we actually override properties, if not dont create it
    //
    //         // make new material clone of base
    //         // foreach property, set it
    //         // assign unique id to this material
    //
    //         Material baseMaterial = baseMaterialInfos[baseId.baseId - 1].material;
    //
    //         if (propertyOverrides.Count == 0) {
    //             return baseId;
    //         }
    //
    //         Material material = new Material(baseMaterial);
    //
    //         RangeInt range = new RangeInt(materialProperties.Length, 0);
    //
    //         for (int i = 0; i < propertyOverrides.Count; i++) {
    //             SetProperty(material, propertyOverrides[i]);
    //         }
    //
    //         MaterialId newId = new MaterialId(baseId.baseId, materialIdGenerator++);
    //
    //         MaterialInfo newInfo = new MaterialInfo() {
    //             keywords = default,
    //             material = material,
    //             materialName = material.name,
    //             propertyRange = range
    //         };
    //
    //         materialMap.Add(newId.id, newInfo);
    //
    //         return newId;
    //     }
    //
    //     private void SetProperty(Material material, MaterialValueOverride propertyOverride) {
    //
    //         switch (propertyOverride.propertyType) {
    //
    //             case MaterialPropertyType.Color:
    //                 material.SetColor(propertyOverride.propertyId, propertyOverride.value.colorValue);
    //                 break;
    //
    //             case MaterialPropertyType.Float:
    //                 material.SetFloat(propertyOverride.propertyId, propertyOverride.value.floatValue);
    //                 break;
    //
    //             case MaterialPropertyType.Vector:
    //                 material.SetVector(propertyOverride.propertyId, propertyOverride.value.vectorValue);
    //                 break;
    //
    //             case MaterialPropertyType.Range:
    //                 throw new NotImplementedException();
    //
    //             case MaterialPropertyType.Texture:
    //                 material.SetTexture(propertyOverride.propertyId, propertyOverride.value.texture);
    //                 break;
    //
    //             default:
    //                 throw new ArgumentOutOfRangeException();
    //         }
    //
    //     }
    //
    //     public bool TryGetMaterial(MaterialId materialId, out MaterialInfo materialInfo) {
    //
    //         if (materialMap.TryGetValue(materialId.id, out MaterialInfo info)) {
    //             materialInfo = info;
    //             return true;
    //         }
    //
    //         materialInfo = default;
    //         return false;
    //     }
    //
    //     public void SetInstanceProperty(ElementId elementId, string materialName, string propertyName, in MaterialPropertyValue2 value) {
    //         for (int i = 0; i < baseMaterialInfos.Length; i++) {
    //             if (baseMaterialInfos[i].materialName == materialName) {
    //                 SetInstanceProperty(elementId, new MaterialId(i + 1, 0), propertyName, value);
    //             }
    //         }
    //     }
    //
    //     public void SetInstanceProperty(ElementId elementId, MaterialId materialId, string propertyName, in MaterialPropertyValue2 value) {
    //
    //         // only care about base id here
    //         if (!materialMap.TryGetValue(materialId.baseId, out MaterialInfo info)) {
    //             return;
    //         }
    //
    //         MaterialPropertyType propertyType = default;
    //         int propertyId = default;
    //
    //         for (int i = info.propertyRange.start; i < info.propertyRange.end; i++) {
    //             if (materialProperties[i].propertyName == propertyName) {
    //                 propertyType = materialProperties[i].propertyType;
    //                 propertyId = materialProperties[i].propertyId;
    //             }
    //         }
    //
    //         // could be per-instance array but i dont think we'll enough of these to merit the memory overhead
    //         if (!instanceProperties.TryGetValue(elementId.id, out LightList<MaterialProperty> properties)) {
    //             properties = LightList<MaterialProperty>.Get();
    //             instanceProperties.Add(elementId.id, properties);
    //         }
    //
    //         for (int i = 0; i < properties.size; i++) {
    //
    //             if (properties[i].materialId.baseId == materialId.baseId && properties[i].stylePropertyId == propertyId) {
    //                 properties.array[i].value = value;
    //             }
    //
    //         }
    //
    //         properties.Add(new MaterialProperty() {
    //             value = value,
    //             materialId = materialId,
    //             stylePropertyId = propertyId,
    //             propertyType = propertyType
    //         });
    //
    //     }
    //
    //     public int GetInstanceProperties(ElementId elementId, MaterialId materialId, IList<MaterialProperty> output) {
    //         if (!instanceProperties.TryGetValue(elementId.id, out LightList<MaterialProperty> properties)) {
    //             return 0;
    //         }
    //
    //         int cnt = 0;
    //
    //         for (int i = 0; i < properties.size; i++) {
    //             ref MaterialProperty property = ref properties.array[i];
    //             if (property.materialId.baseId == materialId.baseId) {
    //                 cnt++;
    //                 output.Add(property);
    //             }
    //         }
    //
    //         return cnt;
    //     }
    //
    //     public int GetInstanceProperties(ElementId elementId, MaterialId materialId, Material material) {
    //         if (!instanceProperties.TryGetValue(elementId.id, out LightList<MaterialProperty> properties)) {
    //             return 0;
    //         }
    //
    //         int cnt = 0;
    //
    //         for (int i = 0; i < properties.size; i++) {
    //             ref MaterialProperty property = ref properties.array[i];
    //             if (property.materialId.baseId == materialId.baseId) {
    //                 cnt++;
    //                 switch (property.propertyType) {
    //
    //                     case MaterialPropertyType.Color:
    //                         material.SetColor(property.stylePropertyId, property.value.colorValue);
    //                         break;
    //
    //                     case MaterialPropertyType.Float:
    //                         material.SetFloat(property.stylePropertyId, property.value.floatValue);
    //                         break;
    //
    //                     case MaterialPropertyType.Vector:
    //                         material.SetVector(property.stylePropertyId, property.value.vectorValue);
    //                         break;
    //
    //                     case MaterialPropertyType.Range:
    //                         break;
    //
    //                     case MaterialPropertyType.Texture:
    //                         material.SetTexture(property.stylePropertyId, property.value.texture);
    //                         break;
    //
    //                     default:
    //                         throw new ArgumentOutOfRangeException();
    //                 }
    //             }
    //         }
    //
    //         return cnt;
    //     }
    //
    //     public int GetInstanceProperties(ElementId elementId, MaterialId materialId, MaterialPropertyBlock propertyBlock) {
    //         if (!instanceProperties.TryGetValue(elementId.id, out LightList<MaterialProperty> properties)) {
    //             return 0;
    //         }
    //
    //         int cnt = 0;
    //
    //         for (int i = 0; i < properties.size; i++) {
    //             ref MaterialProperty property = ref properties.array[i];
    //             if (property.materialId.baseId == materialId.baseId) {
    //                 cnt++;
    //                 switch (property.propertyType) {
    //
    //                     case MaterialPropertyType.Color:
    //                         propertyBlock.SetColor(property.stylePropertyId, property.value.colorValue);
    //                         break;
    //
    //                     case MaterialPropertyType.Float:
    //                         propertyBlock.SetFloat(property.stylePropertyId, property.value.floatValue);
    //                         break;
    //
    //                     case MaterialPropertyType.Vector:
    //                         propertyBlock.SetVector(property.stylePropertyId, property.value.vectorValue);
    //                         break;
    //
    //                     case MaterialPropertyType.Range:
    //                         break;
    //
    //                     case MaterialPropertyType.Texture:
    //                         propertyBlock.SetTexture(property.stylePropertyId, property.value.texture);
    //                         break;
    //
    //                     default:
    //                         throw new ArgumentOutOfRangeException();
    //                 }
    //             }
    //         }
    //
    //         return cnt;
    //     }
    //
    // }

}