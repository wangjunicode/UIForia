using System;
using UIForia.Style;

namespace UIForia {

    public class StyleBuilder {

        internal StyleGroupBuilder normalGroup;
        internal StyleGroupBuilder hoverGroup;
        internal StyleGroupBuilder focusGroup;
        internal StyleGroupBuilder activeGroup;

        public void Set(StyleProperty2 property) {
            normalGroup = normalGroup ?? new StyleGroupBuilder();
            normalGroup.Set(property);
        }

        public void Active(Action<StyleGroupBuilder> builder) {
            if (builder == null) return;
            activeGroup = activeGroup ?? new StyleGroupBuilder();
            builder.Invoke(activeGroup);
        }
        
        public void Focus(Action<StyleGroupBuilder> builder) {
            if (builder == null) return;
            focusGroup = focusGroup ?? new StyleGroupBuilder();
            builder.Invoke(focusGroup);
        }
        
        public void Hover(Action<StyleGroupBuilder> builder) {
            if (builder == null) return;
            hoverGroup = hoverGroup ?? new StyleGroupBuilder();
            builder.Invoke(hoverGroup);
        }

        public void Normal(Action<StyleGroupBuilder> builder) {
            if (builder == null) return;
            normalGroup = normalGroup ?? new StyleGroupBuilder();
            builder.Invoke(normalGroup);
        }

        public int GetSharedStylePropertyCount() {
            int count = activeGroup?.properties?.size ?? 0;
            count += focusGroup?.properties?.size ?? 0;
            count += hoverGroup?.properties?.size ?? 0;
            count += normalGroup?.properties?.size ?? 0;
            return count;
        }

    }

}