using UnityEngine;

namespace UIForia.Text {

    public struct SelectionRange2 {

        public readonly int cursorIndex;
        public readonly TextEdge cursorEdge;
        public readonly int selectionCount;
        
        public SelectionRange2(int cursorIndex, TextEdge cursorEdge, int selectionCount = 0) {
            this.cursorIndex = cursorIndex;
            this.cursorEdge = cursorEdge;
            this.selectionCount = selectionCount;
        }

        public bool IsSelectionForward => selectionCount > 0;
        public bool IsSelectionBackward => selectionCount < 0;
        
        public bool HasSelection => selectionCount != 0;

    }

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

        // cursorIndex
        // cursor is always before
        // if cursor index == length
        // cursor position = last char left
        // if cursor index is last on line, take right edge
        
        public bool HasSelection {
            get {
                if (selectIndex == -1) return false;

                if (selectIndex == cursorIndex + 1) {
                    // selected is larger, should be sure not to select if edges are not the same
                    return selectEdge == TextEdge.Right && cursorEdge == TextEdge.Left;
                }

                if (cursorIndex == selectIndex + 1) {
                    return selectEdge == TextEdge.Left && cursorEdge == TextEdge.Right;
                }
                
                if (selectIndex == cursorIndex) {
                    return selectEdge != cursorEdge;
                }

                return true;
            }
        }

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