using System;

namespace UIForia.Style {

    internal struct StyleBucketReference : IComparable<StyleBucketReference> {

        public StyleState state;
        public int bucketIndex; // set later probably;
        public int contributorIndex;
        public long id; // uniquely identify this somehow
        public int dataHandle;
        public int flags;

        public int CompareTo(StyleBucketReference other) {
            int stateComparison = state.CompareTo(other.state);
            if (stateComparison != 0) return stateComparison;
            int bucketIndexComparison = bucketIndex.CompareTo(other.bucketIndex);
            if (bucketIndexComparison != 0) return bucketIndexComparison;
            int contributorIndexComparison = contributorIndex.CompareTo(other.contributorIndex);
            if (contributorIndexComparison != 0) return contributorIndexComparison;
            int idComparison = id.CompareTo(other.id);
            if (idComparison != 0) return idComparison;
            int dataHandleComparison = dataHandle.CompareTo(other.dataHandle);
            if (dataHandleComparison != 0) return dataHandleComparison;
            return flags.CompareTo(other.flags);
        }

    }

}