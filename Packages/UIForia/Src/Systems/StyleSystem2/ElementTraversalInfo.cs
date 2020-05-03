namespace UIForia {

    public struct ElementTraversalInfo {

        public int depth;
        public ushort ftbIndex;
        public ushort btfIndex;

        public bool IsDescendentOf(in ElementTraversalInfo info) {
            return ftbIndex > info.ftbIndex && btfIndex > info.btfIndex;
        }

        public bool IsAncestorOf(in ElementTraversalInfo info) {
            return ftbIndex < info.ftbIndex && btfIndex < info.btfIndex;
        }

        public bool IsParentOf(in ElementTraversalInfo info) {
            return depth == info.depth + 1 && ftbIndex < info.ftbIndex && btfIndex < info.btfIndex;
        }

    }

}