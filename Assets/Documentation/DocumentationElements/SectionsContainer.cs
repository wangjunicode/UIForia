using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation.DocumentationElements {
    
    [Template("Documentation/DocumentationElements/SectionsContainer.xml")]
    public class SectionsContainer : UIElement {

        public List<SectionPanel> sectionPanels = new List<SectionPanel>();
        
        public override void OnCreate() {
            
            UIChildrenElement element = FindById<UIChildrenElement>("section-content");
            
            sectionPanels.Clear();

            UIElement ptr = element;

            while (ptr != null) {
                sectionPanels.Add(ptr as SectionPanel);
                ptr = ptr.GetNextSibling();
            }
        }
    }
}