using System;
using System.Collections.Generic;
using UIForia.Compilers.Style;
using UIForia.Exceptions;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace UIForia {

    public enum AssetLoadMethod {

        Resources,
        Addressable

    }

    public struct TextureDefinition {

        public string textureName;
        public string spriteName;

    }

    public enum AssetLoadState {

        NotLoaded,
        Loading,
        Loaded,
        Unloaded,

        NotFound

    }

    public sealed class ResourceManager : IDisposable {

        internal struct AssetEntry<T> where T : Object {

            public T asset;
            public int id;

        }

        internal readonly IntMap_Deprecated<AssetEntry<Texture2D>> textureMap;
        internal readonly IntMap_Deprecated<AssetEntry<SpriteAtlas>> spriteAtlasMap;
        internal readonly Dictionary<string, FontAsset> fontMap;
        internal readonly IntMap_Deprecated<AssetEntry<AudioClip>> audioMap;
        internal readonly Dictionary<string, StylePainterDefinition> stylePainters;
        internal readonly LightList<Texture> fontTextures;

        internal MaterialDatabase2 materialDatabase;
        internal DataList<FontAssetInfo>.Shared fontAssetMap;

        private static readonly PainterVariableDeclaration[] s_EmptyVariables = { };

        public ResourceManager() {
            stylePainters = new Dictionary<string, StylePainterDefinition>();
            textureMap = new IntMap_Deprecated<AssetEntry<Texture2D>>();
            spriteAtlasMap = new IntMap_Deprecated<AssetEntry<SpriteAtlas>>();
            fontMap = new Dictionary<string, FontAsset>();
            audioMap = new IntMap_Deprecated<AssetEntry<AudioClip>>();
            materialDatabase = new MaterialDatabase2();
            fontTextures = new LightList<Texture>();
        }

        public void Reset() {
            textureMap.Clear();
            fontTextures.Clear();
            spriteAtlasMap.Clear();
            fontMap.Clear();
            audioMap.Clear();
            stylePainters.Clear();
            Initialize();
        }

        public Texture2D AddTexture(string path, Texture2D texture) {
            return AddResource(path, texture, textureMap);
        }

        public Texture2D AddTexture(Texture2D texture) {
            return AddResource(texture, textureMap);
        }

        public AudioClip AddAudioClip(AudioClip clip) {
            return AddResource(clip, audioMap);
        }

        public Texture2D GetTexture(string path) {
            return GetResource(path, textureMap);
        }

        internal void Initialize() {
            fontAssetMap = new DataList<FontAssetInfo>.Shared(16, Allocator.Persistent);
            FontAsset font = FontAsset.defaultFontAsset;
            font.id = 0;
            font.OnEnable();
            fontAssetMap.Add(font.GetFontInfo());
            fontTextures.Add(font.atlas);
            fontMap.Add(font.name, font);
            
            StructList<MaterialPropertyDefinition> properties = StructList<MaterialPropertyDefinition>.Get();

            // todo -- having built in stuff in resources sucks, move them

            TryGetMaterialProperties(AssetLoadMethod.Resources, "UIForiaShape", properties);
            materialDatabase.TryRegisterMaterial(new MaterialDefinition() {
                properties = properties.ToArray(),
                loadMethod = AssetLoadMethod.Resources,
                assetPath = "UIForiaShape",
                materialName = "UIForiaShape"
            });

            properties.size = 0;

            TryGetMaterialProperties(AssetLoadMethod.Resources, "UIForiaText", properties);
            materialDatabase.TryRegisterMaterial(new MaterialDefinition() {
                properties = properties.ToArray(),
                loadMethod = AssetLoadMethod.Resources,
                assetPath = "UIForiaText",
                materialName = "UIForiaText"
            });

            properties.Release();

        }

        public FontAsset GetFont(string path, bool tryReloading = false) {
            // will be present & null if loaded but not resolved
            if (fontMap.TryGetValue(path, out FontAsset fontAsset)) {
                if (fontAsset != null) {
                    return fontAsset;
                }

                if (!tryReloading) {
                    return null;
                }
            }

            FontAsset retn = Resources.Load<FontAsset>(path);

            if (retn == null) {
                fontMap.Add(path, null);
                return null;
            }

            retn.id = fontAssetMap.size;
            fontAssetMap.Add(retn.GetFontInfo());
            fontTextures.Add(retn.atlas);
            fontMap.Add(path, retn);
            retn.id = fontMap.Count;
            return retn;
        }

        public AudioClip GetAudioClip(string path) {
            return GetResource(path, audioMap);
        }

        private T AddResource<T>(string path, T resource, IntMap_Deprecated<AssetEntry<T>> map) where T : Object {
            if (resource == null || path == null) {
                return null;
            }

            int pathId = path.GetHashCode();
            int id = resource.GetHashCode();

            AssetEntry<T> pathEntry;
            AssetEntry<T> idEntry;

            if (map.TryGetValue(pathId, out pathEntry)) {
                return resource;
            }

            idEntry.id = id;
            idEntry.asset = resource;
            pathEntry.id = id;
            pathEntry.asset = resource;
            map.Add(pathId, pathEntry);
            map.Add(id, idEntry);
            return resource;
        }

        private T AddResource<T>(T resource, IntMap_Deprecated<AssetEntry<T>> map) where T : Object {
            int id = resource.GetHashCode();
            AssetEntry<T> entry;
            if (map.TryGetValue(id, out entry)) {
                return resource;
            }

            entry.id = id;
            entry.asset = resource;
            map.Add(id, entry);
            return resource;
        }

        private T GetResource<T>(string path, IntMap_Deprecated<AssetEntry<T>> map) where T : Object {
            T resource;
            if (path == null) {
                return null;
            }

            AssetEntry<T> pathEntry;
            int pathId = path.GetHashCode();
            if (map.TryGetValue(pathId, out pathEntry)) {
                return pathEntry.asset;
            }
            else {
                // this might be null, but we want to mark the map to show that we tried to load it
                // during the lifecycle of an application we can expect Resources not to be updated
                resource = Resources.Load<T>(path);
                pathEntry.id = pathId;
                pathEntry.asset = resource;
                if (resource != null) {
                    // see if we already have it loaded by id and update linkedId accordingly
                    int resourceId = resource.GetHashCode();
                    AssetEntry<T> idEntry;
                    idEntry.id = resourceId;
                    idEntry.asset = resource;
                    map[idEntry.id] = idEntry;
                }

                map.Add(pathId, pathEntry);
            }

            return resource;
        }

        public void Dispose() {
            fontAssetMap.Dispose();
        }

        public void AddStylePainter(string painterName, StylePainterDefinition stylePainterDefinition) {
            if (Application.HasCustomPainter(painterName)) {
                throw new CompileException("Painter with name: " + painterName + " was already registered");
            }

            if (stylePainters.TryGetValue(painterName, out StylePainterDefinition existing)) {
                throw new CompileException("Painter with name " + painterName + " was already registered from " + existing.fileName);
            }

            stylePainterDefinition.definedVariables = stylePainterDefinition.definedVariables ?? s_EmptyVariables;
            stylePainters.Add(painterName, stylePainterDefinition);

        }

        public bool TryGetStylePainter(string customPainter, out StylePainterDefinition painterDefinition) {
            return stylePainters.TryGetValue(customPainter, out painterDefinition);
        }

        public MaterialId GetMaterialId(string materialName) {
            return materialDatabase.GetMaterialId(materialName);
        }

        internal bool TryGetMaterialProperties(AssetLoadMethod loadMethod, string assetPath, StructList<MaterialPropertyDefinition> output) {
            // todo -- when pre-compiled this data is already available
            output.size = 0;
#if UNITY_EDITOR

            if (loadMethod == AssetLoadMethod.Resources) {
                Material material = Resources.Load<Material>(assetPath);

                if (material == null) {
                    return false;
                }

                Shader shader = material.shader;
                int count = UnityEditor.ShaderUtil.GetPropertyCount(shader);

                for (int i = 0; i < count; i++) {
                    UnityEditor.ShaderUtil.ShaderPropertyType type = UnityEditor.ShaderUtil.GetPropertyType(shader, i);
                    string propertyName = UnityEditor.ShaderUtil.GetPropertyName(shader, i);

                    MaterialPropertyDefinition materialValue = new MaterialPropertyDefinition {
                        shaderPropertyId = Shader.PropertyToID(propertyName),
                        propertyType = ConvertPropertyType(type),
                        propertyName = propertyName
                    };

                    output.Add(materialValue);
                }

                return true;
            }

            MaterialPropertyType ConvertPropertyType(UnityEditor.ShaderUtil.ShaderPropertyType type) {
                switch (type) {

                    case UnityEditor.ShaderUtil.ShaderPropertyType.Color:
                        return MaterialPropertyType.Color;

                    case UnityEditor.ShaderUtil.ShaderPropertyType.Vector:
                        return MaterialPropertyType.Vector;

                    case UnityEditor.ShaderUtil.ShaderPropertyType.Float:
                        return MaterialPropertyType.Float;

                    case UnityEditor.ShaderUtil.ShaderPropertyType.Range:
                        return MaterialPropertyType.Range;

                    case UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv:
                        return MaterialPropertyType.Texture;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
#endif
            }

            throw new NotImplementedException("Todo -- read material property data from pre-compiled code");
        }

        internal void GetFontTextures(Dictionary<int, Texture> dictionary) {
            for (int i = 0; i < fontTextures.size; i++) {
                dictionary[fontTextures.array[i].GetHashCode()] = fontTextures.array[i];
            }
        }

        public Material GetMaterialInstance(MaterialId batchMaterialId) {
            if (batchMaterialId.index == 0) {
                return null;
            }

            if (!materialDatabase.TryGetMaterialInfo(batchMaterialId, out MaterialInfo materialInfo)) {
                return null;
            }

            if (materialInfo.loadState == AssetLoadState.Loaded) {
                return materialInfo.material;
            }

            if (materialInfo.loadState == AssetLoadState.NotLoaded) {
                Material material = Resources.Load<Material>(materialInfo.assetPath);
                if (ReferenceEquals(material, null)) {
                    materialInfo.loadState = AssetLoadState.NotFound;
                    materialDatabase.UpdateMaterialInfo(batchMaterialId, materialInfo);
                    return null;
                }

                materialInfo.loadState = AssetLoadState.Loaded;
                materialInfo.material = material;
                materialDatabase.UpdateMaterialInfo(batchMaterialId, materialInfo);
                return material;
            }

            return null;

        }

        public int GetFontTextureId(int fontAssetId) {
            if (fontAssetId < 0 || fontAssetId > fontAssetMap.size) {
                return 0;
            }

            return fontAssetMap[fontAssetId].atlasTextureId;
        }

    }

}