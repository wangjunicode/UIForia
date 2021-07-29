namespace UIForia {

    public struct TraversalInfo {

        public ushort depth;
        public ushort zIndex;
        public int ftbIndex;
        public int btfIndex;

        public bool IsDescendentOf(in TraversalInfo info) {
            return ftbIndex > info.ftbIndex && btfIndex > info.btfIndex;
        }

        public bool IsSelfOrDescendentOf(in TraversalInfo info) {
            return ftbIndex == info.ftbIndex && btfIndex == info.btfIndex || ftbIndex > info.ftbIndex && btfIndex > info.btfIndex;
        }

        public bool IsAncestorOf(in TraversalInfo info) {
            return ftbIndex < info.ftbIndex && btfIndex < info.btfIndex;
        }

        public bool IsParentOf(in TraversalInfo info) {
            return depth == info.depth + 1 && ftbIndex < info.ftbIndex && btfIndex < info.btfIndex;
        }

        public bool IsChildOf(in TraversalInfo info) {
            return info.depth == depth - 1 && ftbIndex < info.ftbIndex && btfIndex < info.btfIndex;
        }

        public bool IsTemplateChildOf(in TraversalInfo info) {
            return info.depth == info.depth - 1 && ftbIndex < info.ftbIndex && btfIndex < info.btfIndex;
        }

        public bool IsLaterInHierarchy(in TraversalInfo info) {
            return ftbIndex > info.ftbIndex;
        }

    }

}