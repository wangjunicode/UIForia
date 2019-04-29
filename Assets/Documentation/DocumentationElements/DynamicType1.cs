using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation.DocumentationElements {
    [Template("Documentation/DocumentationElements/DynamicType1.xml")]
    public class DynamicType1 : UIElement, IDynamicElement {

        public IDynamicData data;

        public void SetData(IDynamicData data) {
            this.data = data;
        }

    }
}