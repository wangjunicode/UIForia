using System;
using System.Diagnostics;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia {

    [DebuggerTypeProxy(typeof(VertigoStyle))]
    [DebuggerDisplay("{" + nameof(styleName) + "}")]
    public struct VertigoStyleDebugProxy {

        private string styleName;
        public StyleStateGroup normal;
        public StyleStateGroup hover;
        public StyleStateGroup focus;
        public StyleStateGroup active;

        public VertigoStyleDebugProxy(VertigoStyle style) {
            // VertigoStyleSheet sheet = VertigoStyleSystem.GetSheetForStyle(style.index);
            // this.styleName = sheet.styleNames[style.index];
            // this.normal = new StyleStateGroup(style, sheet, StyleState2.Normal);
            // this.hover = new StyleStateGroup(style, sheet, StyleState2.Hover);
            // this.focus = new StyleStateGroup(style, sheet, StyleState2.Focused);
            // this.active = new StyleStateGroup(style, sheet, StyleState2.Active);
            this = default;
        }

        [DebuggerDisplay("property count = {properties?.Length ?? 0}")]
        public struct VertigoSelectorDebugView {

            public StyleProperty2[] properties;

            public VertigoSelectorDebugView(VertigoStyleSheet styleSheet, in VertigoSelector selector) {
                properties = default;
                if (selector.propertyCount > 0) {
                    properties = new StyleProperty2[selector.propertyCount];
                    for (int i = 0; i < selector.propertyCount; i++) {
                        properties[i] = styleSheet.propertyList.array[selector.propertyOffset + i];
                    }
                }
            }

        }

        [DebuggerDisplay("property count = {properties?.Length ?? 0}")]
        public struct StyleStateGroup {

            public StyleProperty2[] properties;
            public VertigoSelectorDebugView[] selectors;

            public StyleStateGroup(VertigoStyle style, VertigoStyleSheet sheet, StyleState2 state) {

                properties = default;
                selectors = default;

                int count = style.GetPropertyCount(state);
                int offset = style.GetPropertyStart(state);
                if (count > 0) {
                    properties = new StyleProperty2[count];
                    for (int i = 0; i < count; i++) {
                        properties[i] = sheet.propertyList.array[offset + i];
                    }
                }

                int selectorCount = style.selectorCount;
                if (selectorCount > 0) {
                    selectors = new VertigoSelectorDebugView[selectorCount];
                    for (int i = 0; i < style.selectorCount; i++) {
                        selectors[i] = new VertigoSelectorDebugView(sheet, sheet.selectors[style.selectorOffset + i]);
                    }
                }
            }

        }

    }

    [DebuggerTypeProxy(typeof(VertigoStyleSheet))]
    internal class VertigoStyleSheetDebugView {

        private VertigoStyleSheet target;
        public VertigoStyleDebugProxy[] styles;

        public VertigoStyleSheetDebugView(VertigoStyleSheet target) {
            this.target = target;
            this.styles = new VertigoStyleDebugProxy[target.styles.size];
            for (int i = 0; i < target.styles.size; i++) {
                unsafe {
                    styles[i] = new VertigoStyleDebugProxy(target.styles.array[i]);
                }

            }
        }

    }

    [DebuggerTypeProxy(typeof(VertigoStyleSheetDebugView))]
    public class VertigoStyleSheet {

        public StyleSheetId id;
        public int moduleId;
        public string filPath;

        public readonly string name;

        internal LightList<string> styleNames;

        internal BufferList<VertigoStyle> styles;
        internal BufferList<VertigoSelector> selectors;

        internal SizedArray<EventHook> eventList;
        internal StructList<StyleProperty2> propertyList;
        internal SizedArray<VertigoSelectorQuery> selectorQueries;
        internal SizedArray<Func<UIElement, bool>> selectorFilters; // todo -- should accept a QueryWrapper not an element

        private StyleBuilder builder;
        public RangeInt styleRange;

        internal VertigoStyleSheet(string name, StyleSheetId id) {
            this.name = name;
            this.id = id;
            this.styleNames = new LightList<string>();
            this.styles = new BufferList<VertigoStyle>(Allocator.Persistent);
            this.selectors = new BufferList<VertigoSelector>(Allocator.Persistent);
            this.eventList = new SizedArray<EventHook>();
            this.propertyList = new StructList<StyleProperty2>(64);
        }

        internal void Destroy() {
            styles.Dispose();
            selectors.Dispose();
        }

        public void AddAnimation() { }

        public void AddSpriteSheet() { }

        public void AddSound() { }

        public void AddCursor() { }

        public void AddConstant() { }

        public void AddStyle(string styleName, Action<StyleBuilder> action) {

            // styleNames.Add(styleName);
            //
            // builder = builder ?? new StyleBuilder();
            //
            // action?.Invoke(builder);
            //
            // builder.Build(this);

        }

        public bool TryGetStyle(string targetStyleName, out StyleId styleId) {
            // for now this is a linear search but likely wants to be a binary one
            // would need a 2nd list of <name, styleId> to do this efficiently
            // since right now names are 1 - 1 with the styles table
            // or a dictionary
            unsafe {
                for (int i = 0; i < styleNames.size; i++) {
                    if (styleNames[i] == targetStyleName) {
                        styleId = styles.array[i].id;
                        return true;
                    }
                }
            }

            styleId = default;
            return false;
        }

    }

}