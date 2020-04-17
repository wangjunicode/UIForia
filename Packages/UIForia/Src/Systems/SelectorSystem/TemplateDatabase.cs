using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Selectors {

    public class TemplateDatabase {

        public UIElement root;
        public LightList<UIElement> elementsByDepth;
        public SizedArray<ElementStyleId> styleTable;
        public SizedArray<ElementAttributeId> attributeTable;
        // state table?
        // selector matches stored here?

        public TemplateDatabase(UIElement root) {
            this.root = root;
        }

        public void GetEnabledDescendents(UIElement element, LightList<UIElement> targets) {
            throw new System.NotImplementedException();
        }

    }

}