using System;

namespace UIForia {

    [Serializable]
    public struct TemplateASTRoot {

        public int templateIndex;
        public int templateNameId;
        public int slotDefinitionCount;
        public int firstSlotDefinitionIndex;

    }

}