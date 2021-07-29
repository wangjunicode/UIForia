#undef FAST_AND_DUMB

using System;
using UIForia.Text;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    internal struct AtlasGlyph {

        public ushort x;
        public ushort y;
        public ushort width;
        public ushort height;

    }
    
    internal struct FontAtlasPacker : IDisposable {

        public readonly ushort atlasWidth;
        public readonly ushort atlasHeight;

        private DataList<Shelf> shelves;

        public FontAtlasPacker(ushort atlasWidth, ushort atlasHeight) : this() {
            this.atlasWidth = atlasWidth;
            this.atlasHeight = atlasHeight;
            this.shelves = new DataList<Shelf>(16, Allocator.Persistent);
        }

        public void Dispose() {
            shelves.Dispose();
            this = default;
        }
#if FAST_AND_DUMB
        private ushort cursorX;
        private ushort cursorY;
        private ushort maxHeight;
#endif
        public bool TryPack(in GlyphResponse glyph, out AtlasGlyph atlasGlyph) {

            atlasGlyph = default;
            ushort rectWidth = (ushort) glyph.rectWidth;
            ushort rectHeight = (ushort) glyph.rectHeight;
#if FAST_AND_DUMB
            // if (rectWidth + cursorX >= atlasWidth) {
            //     cursorX = 0;
            //     cursorY += maxHeight;
            //     maxHeight = 0;
            // }
            //
            // atlasGlyph.x = cursorX;
            // atlasGlyph.y = cursorY;
            // atlasGlyph.width = rectWidth;
            // atlasGlyph.height = rectHeight;
            //
            // cursorX += atlasGlyph.width;
            // if (atlasGlyph.height > maxHeight) {
            //     maxHeight = atlasGlyph.height;
            // }
            //
            // return true;
#else

            if (rectWidth > atlasWidth || rectHeight > atlasHeight) {
                return false;
            }

            ushort y = 0;
            int bestWaste = int.MaxValue;
            int bestShelf = -1;

            // todo -- if I add a bin recycle feature we can search a free bin list before searching for shelf space

            for (int i = 0; i < shelves.size; i++) {
                ref Shelf shelf = ref shelves.Get(i);
                y += shelf.height;

                // not enough width on this shelf, skip it..
                // not enough height, skip it..
                if (rectWidth > shelf.free || rectHeight > shelf.height) {
                    continue;
                }

                // exactly the right height, pack it..
                if (rectHeight == shelf.height) {
                    atlasGlyph = shelf.Alloc(rectWidth, rectHeight);
                    return true;
                }

                // extra height, minimize wasted area..
                if (rectHeight < shelf.height) {
                    int waste = (shelf.height - rectHeight) * rectWidth;
                    if (waste < bestWaste) {
                        bestWaste = waste;
                        bestShelf = i;
                    }
                }
            }

            if (bestShelf != -1) {
                atlasGlyph = shelves.Get(bestShelf).Alloc(rectWidth, rectHeight);
                return true;
            }

            if (rectHeight <= (atlasHeight - y) && rectWidth <= atlasWidth) {
                Shelf shelf = new Shelf(y, atlasWidth, rectHeight);
                atlasGlyph = shelf.Alloc(rectWidth, rectHeight);
                shelves.Add(shelf);
                return true;
            }

            // no room left 
            return false;
#endif
        }

        private struct Shelf {

            public ushort x;
            public ushort y;
            public ushort height;
            public ushort free;

            public Shelf(ushort y, ushort width, ushort height) {
                this.x = 0;
                this.y = y;
                this.height = height;
                this.free = width;
            }

            public AtlasGlyph Alloc(ushort glyphWidth, ushort glyphHeight) {

                AtlasGlyph retn = new AtlasGlyph {
                    x = x,
                    y = y,
                    width = glyphWidth,
                    height = glyphHeight
                };

                x += glyphWidth;
                free -= glyphWidth;
                return retn;
            }

        }

    }
    
}