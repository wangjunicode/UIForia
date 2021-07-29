namespace UIForia {

    [AssertSize(16)]
    public struct HierarchyInfo {

        public ushort childCount;
        public ushort siblingIndex;
        public ElementId firstChildId;
        public ElementId nextSiblingId;
        public ElementId prevSiblingId; // todo -- if this really isn't used, store descendent count here instead (runtimeInfo.lastChildIndex - runtimeInfo.index)
        
    }

}