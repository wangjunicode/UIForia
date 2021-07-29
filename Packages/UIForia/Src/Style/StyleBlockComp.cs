using System.Collections.Generic;

namespace UIForia.Style {

    [AssertSize.SizeOfDependentType(typeof(StyleState), sizeof(byte))]
    internal struct StyleBlockComp : IComparer<BlockUsage> {

        // sort by
        //    state
        //         user states -> hover -> focus -> active
        //    is animation
        //
        //    is selector
        //        depth diff -> style idx of source ->  selector idx to tie break
        //    is style
        //        idx of style -> depth of block -> idx of block

        // deeper is better | stronger
        // more state matches is stronger
        public int Compare(BlockUsage x, BlockUsage y) {
            return x.sortKey.value > y.sortKey.value ? -1 : 1;
        }

    }

}