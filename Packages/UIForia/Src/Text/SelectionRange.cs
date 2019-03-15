namespace UIForia.Text {

    public struct SelectionRange {

        public readonly int cursorIndex;
        public readonly int selectIndex;
        public readonly TextEdge cursorEdge;
        public readonly TextEdge selectEdge;

        public SelectionRange(int cursorIndex, TextEdge cursorEdge, int selectIndex = -1, TextEdge selectEdge = TextEdge.Left) {
            this.cursorIndex = cursorIndex;
            this.cursorEdge = cursorEdge;
            this.selectIndex = selectIndex;
            this.selectEdge = selectEdge;
        }

        public bool HasSelection => selectIndex != -1 && (selectIndex != cursorIndex || selectEdge != cursorEdge);

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SelectionRange && Equals((SelectionRange) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = cursorIndex;
                hashCode = (hashCode * 397) ^ selectIndex;
                hashCode = (hashCode * 397) ^ (int) cursorEdge;
                hashCode = (hashCode * 397) ^ (int) selectEdge;
                return hashCode;
            }
        }

        public bool Equals(SelectionRange previousSelectionRange) {
            return cursorIndex != previousSelectionRange.cursorIndex
                   || cursorEdge != previousSelectionRange.cursorEdge
                   || selectEdge != previousSelectionRange.selectEdge
                   || selectIndex != previousSelectionRange.selectIndex;
        }

        public static bool operator ==(SelectionRange a, SelectionRange b) {
            return a.cursorIndex == b.cursorIndex
                   && a.cursorEdge == b.cursorEdge
                   && a.selectEdge == b.selectEdge
                   && a.selectIndex == b.selectIndex;
        }

        public static bool operator !=(SelectionRange a, SelectionRange b) {
            return !(a == b);
        }

    }

}