using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation.DocumentationElements {
    [Template("Documentation/DocumentationElements/DynamicType0.xml")]
    public class DynamicType0 : UIElement, IDynamicElement {

        public IDynamicData data;

        public void SetData(IDynamicData data) {
            this.data = data;
        } 

    }
}