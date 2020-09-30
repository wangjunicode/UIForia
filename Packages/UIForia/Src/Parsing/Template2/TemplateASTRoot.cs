using System;
using UnityEngine;

namespace UIForia {

    [Serializable]
    public struct TemplateASTRoot {

        public int templateIndex;
        public RangeInt templateNameRange;
        public int slotDefinitionCount;
        public int firstSlotDefinitionIndex;

    }

}