using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Style {

    internal sealed class StyleListDebugView {

        private StyleList styleList;

        public StyleListDebugView(StyleList styleList) => this.styleList = styleList;

        public StyleId[] Items => styleList.ToArray();

    }

    [DebuggerTypeProxy(typeof(StyleListDebugView))]
    public struct StyleList {

        private readonly ElementId elementId;
        private readonly UIApplication application;

        internal StyleList(ElementId elementId, UIApplication application) {
            this.elementId = elementId;
            this.application = application;
        }

        public StyleId[] ToArray() {
            if (!application.IsElementAlive(elementId)) return ArrayPool<StyleId>.Get(0);
            StyleId[] styleIds = application.styleIdAllocator.memory;
            ref StyleInfo styleInfo = ref application.styleInfoTable[elementId.index];
            int start = styleInfo.listSlice.start;
            int end = start + styleInfo.listSlice.length;
            StyleId[] retn = new StyleId[end - start];
            int idx = 0;
            for (int i = start; i < end; i++) {
                retn[idx++] = styleIds[i];
            }

            return retn;
        }

        public void AddStyle(StyleId styleId) {
            if (!styleId.IsValid) return;
            if (!application.IsElementAlive(elementId)) return;
            StyleId[] styleIds = application.styleIdAllocator.memory;
            ref StyleInfo styleInfo = ref application.styleInfoTable[elementId.index];
            int start = styleInfo.listSlice.start;
            int end = start + styleInfo.listSlice.length;

            int idx = -1;

            for (int i = start; i < end; i++) {
                if (styleIds[i].id == styleId.id) {
                    idx = i;
                    break;
                }
            }

            application.runtimeInfoTable[elementId.index].flags |= UIElementFlags.StyleListChanged;

            if (idx == -1) {
                application.styleIdAllocator.Add(ref styleInfo.listSlice, styleId);
                return;
            }

            // if adding a duplicate style, remove the old one adn add this style last 
            styleInfo.listSlice.length--;
            end--;

            for (int i = idx; i < end; i++) {
                styleIds[i] = styleIds[i + 1];
            }

            application.styleIdAllocator.Add(ref styleInfo.listSlice, styleId);
        }

        public void RemoveStyle(StyleId styleId) {
            if (!styleId.IsValid) return;
            if (!application.IsElementAlive(elementId)) return;

            StyleId[] styleIds = application.styleIdAllocator.memory;
            ref StyleInfo styleInfo = ref application.styleInfoTable[elementId.index];
            int start = styleInfo.listSlice.start;
            int end = start + styleInfo.listSlice.length;

            int idx = -1;
            for (int i = start; i < end; i++) {
                if (styleIds[i].id == styleId.id) {
                    idx = i;
                    break;
                }
            }

            if (idx == -1) return;
            application.runtimeInfoTable[elementId.index].flags |= UIElementFlags.StyleListChanged;

            styleInfo.listSlice.length--;
            end--;

            for (int i = idx; i < end; i++) {
                styleIds[i] = styleIds[i + 1];
            }

        }

        public void ToggleStyle(StyleId styleId) {
            if (!styleId.IsValid) return;
            if (!application.IsElementAlive(elementId)) return;

            application.runtimeInfoTable[elementId.index].flags |= UIElementFlags.StyleListChanged;

            ref StyleInfo styleInfo = ref application.styleInfoTable[elementId.index];
            int start = styleInfo.listSlice.start;
            int end = styleInfo.listSlice.length + start;

            int idx = -1;
            StyleId[] styleIds = application.styleIdAllocator.memory;

            for (int i = start; i < end; i++) {
                if (styleIds[i].id == styleId.id) {
                    idx = i;
                    break;
                }
            }

            if (idx == -1) {
                application.styleIdAllocator.Add(ref styleInfo.listSlice, styleId);
                return;
            }

            styleInfo.listSlice.length--;

            end--;

            for (int i = idx; i < end; i++) {
                styleIds[i] = styleIds[i + 1];
            }

        }

        public void SetStyleList(IList<StyleId> styleList) {
            if (!application.IsElementAlive(elementId)) return;
        }

    }

}