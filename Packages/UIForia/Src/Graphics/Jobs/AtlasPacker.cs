using System;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia.Graphics {

    public struct TextureRef : IComparable<TextureRef> {

        public int id;
        public int channel;
        public int textureX;
        public int textureY;
        public int width;
        public int height;

        public int CompareTo(TextureRef other) {
            return height.CompareTo(other.height);
        }

    }

    public struct AtlasPacker {

        public int width;
        public int height;

        private struct Bin {

            public int x;
            public int y;
            public int width;
            public int height;

        }

        public int Pack(DataList<TextureRef> textures, int channel) {

            DataList<Bin> bins = new DataList<Bin>(16, Allocator.Temp);

            int remainingHeight = height;

            float minWidth = textures[textures.size - 1].width;
            float minHeight = textures[textures.size - 1].height;

            // technically each pass should compute its own min width & height
            int unplaced = 0;

            for (int i = 0; i < textures.size; i++) {

                ref TextureRef texture = ref textures[i];

                if (texture.channel != -1) {
                    continue;
                }

                texture.channel = -1;

                for (int bidx = 0; bidx < bins.size; bidx++) {
                    ref Bin bin = ref bins[bidx];

                    if (texture.width > bin.width || texture.height > bin.height) {
                        continue;
                    }

                    texture.textureX = bin.x;
                    texture.textureY = bin.y;
                    texture.channel = channel;

                    if (bin.width - texture.width >= minWidth && bin.height - texture.height >= minHeight) {
                        bin.x += texture.width;
                        //bin.y += texture.height;
                        bin.width -= texture.width;
                        //  bin.height -= texture.height;
                    }
                    else {
                        // bin is too small to hold smallest element, remove it
                        bins[bidx] = bins[--bins.size];
                    }

                }

                if (texture.channel != -1) {
                    continue;
                }

                // -- improvement: if box doesnt fit, turn it on its side and flag it as rotated

                // no bins fit, add a level if we can
                if (remainingHeight > texture.height) {
                    texture.textureX = 0;
                    texture.textureY = height - remainingHeight;
                    texture.channel = channel;
                    remainingHeight -= texture.height;
                    int remainingWidth = width - texture.width;

                    if (remainingWidth >= minWidth) {
                        bins.Add(new Bin() {
                            x = texture.width,
                            y = texture.textureY,
                            width = remainingWidth - texture.width,
                            height = remainingHeight
                        });
                    }

                }
                else {
                    unplaced++;
                }

            }

            bins.Dispose();
            return unplaced;
        }

    }

}