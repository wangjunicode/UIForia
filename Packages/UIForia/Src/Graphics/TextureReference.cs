using UIForia.Graphics;
using UnityEngine;

namespace UIForia {

    public class TextureReference {

        // todo -- make readonly
        public readonly int textureId;
        public readonly Texture texture;
        public readonly string texturePath;
        public readonly AxisAlignedBounds2DUShort uvRect;
        public readonly string spriteName;

        internal TextureReference(in ResourceManager.SpriteAssetInfo asset, string spriteName, AxisAlignedBounds2DUShort uvRect) {
            this.texture = asset.texture;
            this.textureId = asset.textureId;
            this.texturePath = asset.texturePath;
            this.spriteName = spriteName;
            this.uvRect = uvRect;
        }

        internal TextureReference(Texture texture) {
            this.texture = texture;
            this.textureId = texture?.GetHashCode() ?? 0;
            this.texturePath = null;
            this.spriteName = null;
            if (!ReferenceEquals(texture, null)) {
                this.uvRect = uvRect = new AxisAlignedBounds2DUShort() {
                    xMin = 0, xMax = (ushort) texture.width,
                    yMin = 0, yMax = (ushort) texture.height
                };
            }
        }

        internal static readonly TextureReference s_Empty = new TextureReference(null);

        public static implicit operator TextureReference(Texture input) {
            return ReferenceEquals(input, null) ? s_Empty : new TextureReference(input);
        }

    }

}