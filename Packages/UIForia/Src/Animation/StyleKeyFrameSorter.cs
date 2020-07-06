using System.Collections.Generic;

namespace UIForia.Animation {

    public class StyleKeyFrameSorter : IComparer<ProcessedStyleKeyFrame> {

        public int Compare(ProcessedStyleKeyFrame x, ProcessedStyleKeyFrame y) {
            if (x.time == y.time) return 0;
            return x.time > y.time ? 1 : -1;
        }

    }
    

}