
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
        
        public bool HasSelection {
            get {
                if (selectIndex == -1) {
                    return false;
                }

                int diff = selectIndex - cursorIndex;

                switch (diff) {
                    case 0:
                        return selectEdge != cursorEdge;
                    case 1:
                        return cursorEdge != TextEdge.Right || selectEdge == TextEdge.Right;
                    case -1:
                        return cursorEdge != TextEdge.Left || selectEdge != TextEdge.Right;
                    default:
                        return selectIndex != cursorIndex;
                }
            }
        }

        // convert cursor to always be to the left of the target character
        // cursor 6 right == cursor 7 left
        // makes working with selection more consistent
        // only case that would use 'right' is the last character on a line
        
        public SelectionRange NormalizeLeft() {
            if (cursorEdge == TextEdge.Right) {
                return new SelectionRange(cursorIndex + 1, TextEdge.Left);    
            }

            return this;
        }
        
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SelectionRange a && Equals(a);
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