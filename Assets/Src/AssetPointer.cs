using System;
using System.Collections.Generic;
using Rendering;
using Src.Extensions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Src {

    public enum AssetType {

        Texture,
        Video,
        Sprite,
        Shape,
        Gradient,
        Font

    }

    public struct AssetPointer<T> where T : Object {

        public readonly T asset;
        public readonly int id;
        public readonly AssetType assetType;

        public AssetPointer(AssetType assetType, int id) {
            this.asset = null;
            this.assetType = assetType;
            this.id = id;
        }

    }

    public struct Texture2DAssetReference {

        public readonly int assetId;
        public readonly Texture2D asset;

        public Texture2DAssetReference(string referencePath) {
            if (referencePath == "default") {
                assetId = 0;
                asset = null;
                return;
            }

            // maybe dangerous to use hash code like this?
            assetId = referencePath.GetHashCode();
            asset = s_TextureMap.GetOrDefault(assetId);
            if (asset == null) {
                asset = (Texture2D) Resources.Load(referencePath);
                if (asset != null) {
                    s_TextureMap[assetId] = asset;
                }
                else {
                    throw new Exception("Unable to load texture at path: " + referencePath);
                }
            }
        }

        public Texture2DAssetReference(int id) {
            if (id == 0 || !IntUtil.IsDefined(id)) {
                assetId = 0;
                asset = null;
                return;
            }

            asset = s_TextureMap.GetOrDefault(id);
            assetId = id;
            if (asset == null) {
                throw new Exception("Unable to find texture with Id: " + id);
            }
        }

        public bool IsDefined() {
            return IntUtil.IsDefined(assetId);
        }

        public static implicit operator Texture2D(Texture2DAssetReference assetReference) {
            return assetReference.asset;
        }

        private static readonly Dictionary<int, Texture2D> s_TextureMap = new Dictionary<int, Texture2D>();

    }

    
    public struct FontAssetReference {

        public readonly int assetId;
        public readonly TMP_FontAsset asset;

        public FontAssetReference(string referencePath) {
            if (referencePath == "default") {
                assetId = 0;
                asset = TMP_FontAsset.defaultFontAsset;
                return;
            }

            // maybe dangerous to use hash code like this?
            assetId = referencePath.GetHashCode();
            asset = s_FontMap.GetOrDefault(assetId);
            if (asset == null) {
                asset = (TMP_FontAsset) Resources.Load(referencePath);
                if (asset != null) {
                    s_FontMap[assetId] = asset;
                }
                else {
                    throw new Exception("Unable to load font at path: " + referencePath);
                }
            }
        }

        public FontAssetReference(int id) {
            if (id == 0) {
                assetId = 0;
                asset = TMP_FontAsset.defaultFontAsset;
                return;
            }

            asset = s_FontMap.GetOrDefault(id);
            assetId = id;
            if (asset == null) {
                throw new Exception("Unable to find font with Id: " + id);
            }
        }

        public bool IsDefined() {
            return IntUtil.IsDefined(assetId);
        }

        public static implicit operator TMP_FontAsset(FontAssetReference assetReference) {
            return assetReference.asset;
        }

        private static readonly Dictionary<int, TMP_FontAsset> s_FontMap = new Dictionary<int, TMP_FontAsset>();

    }

}