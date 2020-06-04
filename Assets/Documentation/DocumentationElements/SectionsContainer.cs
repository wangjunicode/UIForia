using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation.DocumentationElements {
    
    [Template("Documentation/DocumentationElements/SectionsContainer.xml")]
    public class SectionsContainer : UIElement {

        public List<SectionPanel> sectionPanels = new List<SectionPanel>();
        
        public override void OnCreate() {
            var list = FindById<UIChildrenElement>("section-content").children;
            
            sectionPanels.Clear();

            for (int i = 0; i < list.Count; i++) {
                SectionPanel sectionPanel = (SectionPanel) list[i];
                sectionPanels.Add(sectionPanel);
            }
        }
    }
}