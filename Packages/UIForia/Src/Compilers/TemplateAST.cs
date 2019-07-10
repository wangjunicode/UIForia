using UIForia.Templates;
using UIForia.Util;

namespace UIForia.Compilers {

    public struct SlotDefinition {

        public string tagName;
        public int slotId;

    }
    
    public class TemplateAST {

        public TemplateNode root;
        public StructList<UsingDeclaration> usings;
        public StructList<StyleDefinition> styles;
        public StructList<SlotDefinition> slotDefinitions;
        public string fileName;
        public bool extends;

        public int GetSlotId(string tagName) {
            slotDefinitions = slotDefinitions ?? StructList<SlotDefinition>.Get();
            for (int i = 0; i < slotDefinitions.Count; i++) {
                if (slotDefinitions[i].tagName == tagName) {
                    return i;
                }
            }
            
            slotDefinitions.Add(new SlotDefinition() {
                tagName = tagName,
                slotId = slotDefinitions.Count
            });
            
            return slotDefinitions.Count - 1;
        }

    }

}