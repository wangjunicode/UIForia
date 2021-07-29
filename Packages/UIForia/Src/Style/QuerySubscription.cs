using System;

namespace UIForia.Style {

    internal struct QuerySubscription : IComparable<QuerySubscription> {

        public StyleUsage styleUsage;
        public QueryId queryId; // could use 3 bytes & 1 byte here for blockIdx
        public int targetConditionIndex;

        public int CompareTo(QuerySubscription other) {
            return queryId.id - other.queryId.id;
        }

    }

}