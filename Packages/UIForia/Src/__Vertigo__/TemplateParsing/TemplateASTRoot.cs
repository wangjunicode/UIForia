using System;

namespace UIForia {

    [Serializable]
    public struct TemplateASTRoot {

        public int templateIndex;
        public string templateName;
        public int slotDefinitionCount;
        public int firstSlotDefinitionIndex;

    }

}