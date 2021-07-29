using UIForia.Elements;

namespace UIForia {

    internal static unsafe class ElementSystem {
        
        public static bool IsAlive(ElementId elementId, RuntimeTraversalInfo[] metaTable) {
            return metaTable[elementId.index].generation == elementId.generation;
        }
        
        public static bool IsAlive(ElementId elementId, RuntimeTraversalInfo metaTable) {
            return metaTable.generation == elementId.generation;
        }
        
        public static bool IsDeadOrDisabled(ElementId elementId, CheckedArray<RuntimeTraversalInfo> metaTable) {
            return metaTable[elementId.id & ElementId.k_IndexMask].generation != elementId.generation || (metaTable[elementId.id & ElementId.k_IndexMask].flags & UIElementFlags.Enabled) != UIElementFlags.Enabled;
        }
        
        public static bool IsDeadOrDisabled(ElementId elementId, RuntimeTraversalInfo[] metaTable) {
            return metaTable[elementId.id & ElementId.k_IndexMask].generation != elementId.generation || (metaTable[elementId.id & ElementId.k_IndexMask].flags & UIElementFlags.Enabled) != UIElementFlags.Enabled;
        }
        
        public static bool IsDeadOrDisabled(ElementId elementId, RuntimeTraversalInfo* metaTable) {
            return metaTable[elementId.id & ElementId.k_IndexMask].generation != elementId.generation || (metaTable[elementId.id & ElementId.k_IndexMask].flags & UIElementFlags.Enabled) != UIElementFlags.Enabled;
        }

    }


}