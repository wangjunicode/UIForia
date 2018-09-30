using UnityEngine;

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

        public AssetPointer(T asset) {
            this.asset = asset;
            this.id = 0;
            this.assetType = AssetType.Texture;
        }

    }

}