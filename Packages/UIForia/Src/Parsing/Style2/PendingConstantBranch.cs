using UIForia.Util;

namespace UIForia.Style2 {

    public struct PendingConstantBranch {

        public readonly int conditionId;
        public readonly CharSpan value;

        public PendingConstantBranch(int conditionId, in CharSpan charSpan) {
            this.conditionId = conditionId;
            this.value = charSpan;
        }

    }

}