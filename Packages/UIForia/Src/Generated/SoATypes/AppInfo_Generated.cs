using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    // Generated from SoA Generator, so not edit by hand!
    internal unsafe partial struct AppInfo {

        
        private int SoA_Capacity_PerElementId;
        private int SoA_Capacity_PerActiveElement;
        
        public void SoA_Initialize_PerElementId(int requiredElementCount) {
            requiredElementCount = requiredElementCount < 32 ? 32 : requiredElementCount;
            requiredElementCount += (int)(requiredElementCount * 0.5f);
            int sizeForOneElement = sizeof(UIForia.RuntimeTraversalInfo) + sizeof(UIForia.HierarchyInfo) + sizeof(UIForia.Style.StyleState) + sizeof(UIForia.TemplateInfo) + sizeof(int) + sizeof(UIForia.ElementId) + sizeof(UIForia.Style.StyleInfo) + sizeof(UIForia.Style.InstancePropertyInfo) + sizeof(ushort) + sizeof(ushort);
            long totalByteSize = sizeForOneElement * requiredElementCount;
            void * __buffer__ = UnsafeUtility.Malloc(totalByteSize, UnsafeUtility.AlignOf<long>(), Allocator.Persistent);
            UnsafeUtility.MemClear(__buffer__, totalByteSize);
            runtimeInfoTable = (UIForia.RuntimeTraversalInfo*)__buffer__;
            hierarchyTable = (UIForia.HierarchyInfo*)(runtimeInfoTable + requiredElementCount);
            states = (UIForia.Style.StyleState*)(hierarchyTable + requiredElementCount);
            templateInfoTable = (UIForia.TemplateInfo*)(states + requiredElementCount);
            elementIdToIndex = (int*)(templateInfoTable + requiredElementCount);
            elementIdToParentId = (UIForia.ElementId*)(elementIdToIndex + requiredElementCount);
            styleInfoTable = (UIForia.Style.StyleInfo*)(elementIdToParentId + requiredElementCount);
            instancePropertyInfoTable = (UIForia.Style.InstancePropertyInfo*)(styleInfoTable + requiredElementCount);
            depthTable = (ushort*)(instancePropertyInfoTable + requiredElementCount);
            elementIdToViewId = (ushort*)(depthTable + requiredElementCount);
            SoA_Capacity_PerElementId = requiredElementCount;
        }
        
        public void SoA_SetSize_PerElementId(int requiredElementCount, bool copyPrevContents) {
            if (requiredElementCount > SoA_Capacity_PerElementId) {
                SoA_EnsureCapacity_PerElementId(requiredElementCount, copyPrevContents);
            }
        }
        
        public void SoA_EnsureCapacity_PerElementId(int requiredElementCount, bool copyPrevContents) {
            if (requiredElementCount < SoA_Capacity_PerElementId) return;
            UIForia.RuntimeTraversalInfo* old_runtimeInfoTable = runtimeInfoTable;
            UIForia.HierarchyInfo* old_hierarchyTable = hierarchyTable;
            UIForia.Style.StyleState* old_states = states;
            UIForia.TemplateInfo* old_templateInfoTable = templateInfoTable;
            int* old_elementIdToIndex = elementIdToIndex;
            UIForia.ElementId* old_elementIdToParentId = elementIdToParentId;
            UIForia.Style.StyleInfo* old_styleInfoTable = styleInfoTable;
            UIForia.Style.InstancePropertyInfo* old_instancePropertyInfoTable = instancePropertyInfoTable;
            ushort* old_depthTable = depthTable;
            ushort* old_elementIdToViewId = elementIdToViewId;
            
            int prevCapacity = SoA_Capacity_PerElementId;
            SoA_Initialize_PerElementId(requiredElementCount);
            
            if (prevCapacity > 0 && copyPrevContents) {
                UnsafeUtility.MemCpy(runtimeInfoTable, old_runtimeInfoTable, sizeof(UIForia.RuntimeTraversalInfo) * prevCapacity);
                UnsafeUtility.MemCpy(hierarchyTable, old_hierarchyTable, sizeof(UIForia.HierarchyInfo) * prevCapacity);
                UnsafeUtility.MemCpy(states, old_states, sizeof(UIForia.Style.StyleState) * prevCapacity);
                UnsafeUtility.MemCpy(templateInfoTable, old_templateInfoTable, sizeof(UIForia.TemplateInfo) * prevCapacity);
                UnsafeUtility.MemCpy(elementIdToIndex, old_elementIdToIndex, sizeof(System.Int32) * prevCapacity);
                UnsafeUtility.MemCpy(elementIdToParentId, old_elementIdToParentId, sizeof(UIForia.ElementId) * prevCapacity);
                UnsafeUtility.MemCpy(styleInfoTable, old_styleInfoTable, sizeof(UIForia.Style.StyleInfo) * prevCapacity);
                UnsafeUtility.MemCpy(instancePropertyInfoTable, old_instancePropertyInfoTable, sizeof(UIForia.Style.InstancePropertyInfo) * prevCapacity);
                UnsafeUtility.MemCpy(depthTable, old_depthTable, sizeof(System.UInt16) * prevCapacity);
                UnsafeUtility.MemCpy(elementIdToViewId, old_elementIdToViewId, sizeof(System.UInt16) * prevCapacity);
            }

            UnsafeUtility.Free(old_runtimeInfoTable, Allocator.Persistent);
        }
        
        public void SoA_Clear_PerElementId(int requiredElementCount) {
            int sizeForOneElement = sizeof(UIForia.RuntimeTraversalInfo) + sizeof(UIForia.HierarchyInfo) + sizeof(UIForia.Style.StyleState) + sizeof(UIForia.TemplateInfo) + sizeof(int) + sizeof(UIForia.ElementId) + sizeof(UIForia.Style.StyleInfo) + sizeof(UIForia.Style.InstancePropertyInfo) + sizeof(ushort) + sizeof(ushort);
            UnsafeUtility.MemClear(runtimeInfoTable, requiredElementCount * sizeForOneElement);
        }
        
        public void SoA_Dispose_PerElementId() {
            if (runtimeInfoTable == null) return;
            UnsafeUtility.Free(runtimeInfoTable, Allocator.Persistent);
            runtimeInfoTable = default;
            hierarchyTable = default;
            states = default;
            templateInfoTable = default;
            elementIdToIndex = default;
            elementIdToParentId = default;
            styleInfoTable = default;
            instancePropertyInfoTable = default;
            depthTable = default;
            elementIdToViewId = default;
        }

        public void SoA_Initialize_PerActiveElement(int requiredElementCount) {
            requiredElementCount = requiredElementCount < 32 ? 32 : requiredElementCount;
            requiredElementCount += (int)(requiredElementCount * 0.5f);
            int sizeForOneElement = sizeof(UIForia.TraversalInfo) + sizeof(int) + sizeof(UIForia.ElementId) + sizeof(UIForia.Style.StyleState) + sizeof(int) + sizeof(int);
            long totalByteSize = sizeForOneElement * requiredElementCount;
            void * __buffer__ = UnsafeUtility.Malloc(totalByteSize, UnsafeUtility.AlignOf<long>(), Allocator.Persistent);
            UnsafeUtility.MemClear(__buffer__, totalByteSize);
            traversalTable = (UIForia.TraversalInfo*)__buffer__;
            parentIndexByActiveElementIndex = (int*)(traversalTable + requiredElementCount);
            indexToElementId = (UIForia.ElementId*)(parentIndexByActiveElementIndex + requiredElementCount);
            styleStateByActiveIndex = (UIForia.Style.StyleState*)(indexToElementId + requiredElementCount);
            childCountByActiveIndex = (int*)(styleStateByActiveIndex + requiredElementCount);
            siblingIndexByActiveIndex = (int*)(childCountByActiveIndex + requiredElementCount);
            SoA_Capacity_PerActiveElement = requiredElementCount;
        }
        
        public void SoA_SetSize_PerActiveElement(int requiredElementCount, bool copyPrevContents) {
            if (requiredElementCount > SoA_Capacity_PerActiveElement) {
                SoA_EnsureCapacity_PerActiveElement(requiredElementCount, copyPrevContents);
            }
        }
        
        public void SoA_EnsureCapacity_PerActiveElement(int requiredElementCount, bool copyPrevContents) {
            if (requiredElementCount < SoA_Capacity_PerActiveElement) return;
            UIForia.TraversalInfo* old_traversalTable = traversalTable;
            int* old_parentIndexByActiveElementIndex = parentIndexByActiveElementIndex;
            UIForia.ElementId* old_indexToElementId = indexToElementId;
            UIForia.Style.StyleState* old_styleStateByActiveIndex = styleStateByActiveIndex;
            int* old_childCountByActiveIndex = childCountByActiveIndex;
            int* old_siblingIndexByActiveIndex = siblingIndexByActiveIndex;
            
            int prevCapacity = SoA_Capacity_PerActiveElement;
            SoA_Initialize_PerActiveElement(requiredElementCount);
            
            if (prevCapacity > 0 && copyPrevContents) {
                UnsafeUtility.MemCpy(traversalTable, old_traversalTable, sizeof(UIForia.TraversalInfo) * prevCapacity);
                UnsafeUtility.MemCpy(parentIndexByActiveElementIndex, old_parentIndexByActiveElementIndex, sizeof(System.Int32) * prevCapacity);
                UnsafeUtility.MemCpy(indexToElementId, old_indexToElementId, sizeof(UIForia.ElementId) * prevCapacity);
                UnsafeUtility.MemCpy(styleStateByActiveIndex, old_styleStateByActiveIndex, sizeof(UIForia.Style.StyleState) * prevCapacity);
                UnsafeUtility.MemCpy(childCountByActiveIndex, old_childCountByActiveIndex, sizeof(System.Int32) * prevCapacity);
                UnsafeUtility.MemCpy(siblingIndexByActiveIndex, old_siblingIndexByActiveIndex, sizeof(System.Int32) * prevCapacity);
            }

            UnsafeUtility.Free(old_traversalTable, Allocator.Persistent);
        }
        
        public void SoA_Clear_PerActiveElement(int requiredElementCount) {
            int sizeForOneElement = sizeof(UIForia.TraversalInfo) + sizeof(int) + sizeof(UIForia.ElementId) + sizeof(UIForia.Style.StyleState) + sizeof(int) + sizeof(int);
            UnsafeUtility.MemClear(traversalTable, requiredElementCount * sizeForOneElement);
        }
        
        public void SoA_Dispose_PerActiveElement() {
            if (traversalTable == null) return;
            UnsafeUtility.Free(traversalTable, Allocator.Persistent);
            traversalTable = default;
            parentIndexByActiveElementIndex = default;
            indexToElementId = default;
            styleStateByActiveIndex = default;
            childCountByActiveIndex = default;
            siblingIndexByActiveIndex = default;
        }

        
        public void SoA_Dispose() {
            SoA_Dispose_PerElementId();
            SoA_Dispose_PerActiveElement();
        }
    }

}
