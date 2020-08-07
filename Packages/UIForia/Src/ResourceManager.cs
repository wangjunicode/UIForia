using System;
using System.Collections.Generic;
using TMPro;
using UIForia.Compilers.Style;
using UIForia.Exceptions;
using UIForia.Graphics;
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

    internal struct GPUGlyphInfo {

        public float atlasX;
        public float atlasY;
        public float atlasWidth;
        public float atlasHeight;

        public float xOffset;
        public float yOffset;
        public float width;
        public float height;

    }

    public sealed class ResourceManager : IDisposable {

        internal struct AssetEntry<T> where T : Object {

            public T asset;
            public int id;

        }

        public event Action<FontAsset> onFontAdded;

        internal readonly IntMap_Deprecated<AssetEntry<Texture2D>> textureMap;
        internal readonly IntMap_Deprecated<AssetEntry<SpriteAtlas>> spriteAtlasMap;
        internal readonly Dictionary<string, FontAsset> fontMap;
        internal readonly IntMap_Deprecated<AssetEntry<AudioClip>> audioMap;
        internal readonly Dictionary<string, StylePainterDefinition> stylePainters;
        internal readonly LightList<Texture> fontTextures;
        internal DataList<GPUGlyphInfo> renderedCharacterInfoList;
        internal DataList<GPUFontInfo> gpuFontInfoList;

        internal MaterialDatabase2 materialDatabase;
        internal DataList<FontAssetInfo>.Shared fontAssetMap;
        internal Dictionary<string, SpriteAssetInfo> spriteMap;

        private static readonly PainterVariableDeclaration[] s_EmptyVariables = { };

        public ResourceManager() {
            stylePainters = new Dictionary<string, StylePainterDefinition>();
            spriteMap = new Dictionary<string, SpriteAssetInfo>();
            textureMap = new IntMap_Deprecated<AssetEntry<Texture2D>>();
            spriteAtlasMap = new IntMap_Deprecated<AssetEntry<SpriteAtlas>>();
            fontMap = new Dictionary<string, FontAsset>();
            audioMap = new IntMap_Deprecated<AssetEntry<AudioClip>>();
            materialDatabase = new MaterialDatabase2();
            fontTextures = new LightList<Texture>();
            gpuFontInfoList = new DataList<GPUFontInfo>(8, Allocator.Persistent);
            renderedCharacterInfoList = new DataList<GPUGlyphInfo>(1024, Allocator.Persistent);
            renderedCharacterInfoList.size = 1; // 0 is invalid
        }

        public void Reset() {
            textureMap.Clear();
            fontTextures.Clear();
            spriteAtlasMap.Clear();
            fontMap.Clear();
            audioMap.Clear();
            stylePainters.Clear();
            spriteMap.Clear();
            renderedCharacterInfoList.size = 1; // 0 is invalid
            gpuFontInfoList.size = 0;
            onFontAdded = null;
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

        internal void GatherGPUFontInfo(FontAsset fontAsset) {

            GPUFontInfo fontInfo = new GPUFontInfo();
            fontInfo.normalStyle = fontAsset.normalStyle;
            fontInfo.gradientScale = fontAsset.gradientScale;
            fontInfo.glyphOffset = renderedCharacterInfoList.size;
            fontInfo.scale = fontAsset.faceInfo.scale;
            fontInfo.boldStyle = fontAsset.boldStyle;
            fontInfo.italicStyle = fontAsset.italicStyle;
            fontInfo.weightNormal = fontAsset.weightNormal;
            fontInfo.weightBold = fontAsset.weightBold;
            fontInfo.pointSize = fontAsset.faceInfo.pointSize;
            fontInfo.ascender = fontAsset.faceInfo.ascender;

            // todo -- I need to update this if I atlas fonts together
            fontInfo.atlasWidth = fontAsset.atlas.width;
            fontInfo.atlasHeight = fontAsset.atlas.height;

            gpuFontInfoList.Add(fontInfo);
            uint offset = (uint) renderedCharacterInfoList.size;

            for (int i = 0; i < fontAsset.glyphList.size; i++) {
                ref UIForiaGlyph glyph = ref fontAsset.glyphList[i];
                glyph.renderBufferIndex = offset++;
                // todo -- we have 2 glyph buffers right now, 1 per font and 1 global for gpu submission. try to not do this
                renderedCharacterInfoList.Add(new GPUGlyphInfo() {
                    width = glyph.width,
                    height = glyph.height,
                    xOffset = glyph.xOffset,
                    yOffset = glyph.yOffset,
                    atlasX = glyph.uvX,
                    atlasY = glyph.uvY,
                    atlasWidth = glyph.uvWidth,
                    atlasHeight = glyph.uvHeight
                });
            }
        }

        internal void Initialize() {
            fontAssetMap = new DataList<FontAssetInfo>.Shared(16, Allocator.Persistent);
            FontAsset font = FontAsset.defaultFontAsset;
            font.id = 0;
            font.Initialize(this);
            fontAssetMap.Add(font.GetFontInfo());
            fontTextures.Add(font.atlas);
            fontMap.Add(font.name, font);
            onFontAdded?.Invoke(font);
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

            return null;
            TMP_FontAsset tmp = Resources.Load<TMP_FontAsset>(path);

            if (tmp == null) {
                fontMap.Add(path, null);
                return null;
            }

            FontAsset retn = ScriptableObject.CreateInstance<FontAsset>();
            retn.convertFrom = tmp;

            retn.id = fontAssetMap.size;
            retn.Initialize(this);
            fontAssetMap.Add(retn.GetFontInfo());
            fontTextures.Add(retn.atlas);
            fontMap.Add(path, retn);
            retn.id = fontMap.Count;

            onFontAdded?.Invoke(retn);

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
            }
#endif

            return false;
            //throw new NotImplementedException("Todo -- read material property data from pre-compiled code");
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


        internal struct SpriteAssetInfo {

            public SpriteAtlas atlas;
            public Texture2D texture;
            public int textureId;
            public SpriteAssetRef[] spriteRefs; // todo -- would be better to use a single large array
            public string texturePath;

        }

        internal struct SpriteAssetRef {

            public AxisAlignedBounds2DUShort uvRect;
            public string spriteName;

        }

        // todo -- figure out how to unload these
        public TextureReference GetSpriteTexture(AssetInfo assetInfo) {

            if (spriteMap.TryGetValue(assetInfo.path, out SpriteAssetInfo asset)) {
                return GetSprite(asset, assetInfo.spriteName);
            }

            asset = new SpriteAssetInfo();
            SpriteAtlas atlas = Resources.Load<SpriteAtlas>(assetInfo.path);

            if (atlas == null) {
                asset.spriteRefs = new SpriteAssetRef[0];
                spriteMap.Add(assetInfo.path, asset);
                return TextureReference.s_Empty;
            }
            if (atlas.spriteCount == 0) {
                throw new Exception("Failed to load sprite atlas: " + assetInfo.path + ". There were no sprites in the atlas");
            }
            Sprite[] sprites = new Sprite[atlas.spriteCount];
            atlas.GetSprites(sprites);
            asset.atlas = atlas;
            asset.texture = sprites[0].texture;
            if (asset.texture == null) {
                throw new Exception("Failed to load sprite atlas: " + assetInfo.path + ". This might because it didnt pack properly");
            }
            asset.textureId = asset.texture.GetHashCode();
            asset.texturePath = assetInfo.path;
            asset.spriteRefs = new SpriteAssetRef[sprites.Length];
            for (int i = 0; i < sprites.Length; i++) {
                Rect spriteUVRect = sprites[i].textureRect;
                AxisAlignedBounds2DUShort uvRect = default;
                uvRect.xMin = (ushort) spriteUVRect.xMin;
                uvRect.yMin = (ushort) spriteUVRect.yMin;
                uvRect.xMax = (ushort) spriteUVRect.xMax;
                uvRect.yMax = (ushort) spriteUVRect.yMax;
                ref SpriteAssetRef spriteRef = ref asset.spriteRefs[i];
                spriteRef.uvRect = uvRect;
                spriteRef.spriteName = sprites[i].name.Substring(0, sprites[i].name.Length - "(Clone)".Length);
                Object.Destroy(sprites[i]);
            }

            spriteMap.Add(assetInfo.path, asset);

            return GetSprite(asset, assetInfo.spriteName);
        }

        private static TextureReference GetSprite(in SpriteAssetInfo asset, string spriteName) {
            for (int i = 0; i < asset.spriteRefs.Length; i++) {
                if (asset.spriteRefs[i].spriteName == spriteName) {
                    return new TextureReference(asset, spriteName, asset.spriteRefs[i].uvRect);
                }
            }

            return TextureReference.s_Empty;
        }

    }

}