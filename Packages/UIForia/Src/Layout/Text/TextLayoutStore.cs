using System;
using UIForia.Prototype;
using UIForia.Text;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia.Layout {

    internal struct TextLayoutStore : IDisposable {

        public DataList<LineInset> insetStack;
        public DataList<TextAlignment> alignmentStack;
        public DataList<WhitespaceMode> whitespaceModeStack;
        public DataList<VerticalAlignment> verticalAlignStack;

        public static TextLayoutStore Create() {
            return new TextLayoutStore {
                insetStack = new DataList<LineInset>(8, Allocator.Temp),
                alignmentStack = new DataList<TextAlignment>(8, Allocator.Temp),
                whitespaceModeStack = new DataList<WhitespaceMode>(8, Allocator.Temp),
                verticalAlignStack = new DataList<VerticalAlignment>(8, Allocator.Temp)
            };
        }

        public void Dispose() {
            insetStack.Dispose();
            alignmentStack.Dispose();
            whitespaceModeStack.Dispose();
            verticalAlignStack.Dispose();
        }

        public void Reset() {
            insetStack.size = 0;
            alignmentStack.size = 0;
            whitespaceModeStack.size = 0;
            verticalAlignStack.size = 0;
        }

    }

}