using System.Collections.Generic;

namespace UIForia.Animation {

    public class KeyFrameSorter : IComparer<ProcessedKeyFrame> {

        public int Compare(ProcessedKeyFrame x, ProcessedKeyFrame y) {
            if (x.time == y.time) return 0;
            return x.time > y.time ? 1 : -1;
        }

    }

}