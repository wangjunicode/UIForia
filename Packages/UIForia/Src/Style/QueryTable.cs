using System.Runtime.InteropServices;
using UIForia.Parsing;

namespace UIForia.Style {

#pragma warning disable 660,661
    [AssertSize(16)]
    [StructLayout(LayoutKind.Explicit)]
    internal struct QueryData {

        [FieldOffset(0)] public QueryDataStateInfo stateInfo;
        [FieldOffset(0)] public QueryDataAttributeInfo attrInfo;
        [FieldOffset(0)] public QueryDataChildCountInfo childCountInfo;
        [FieldOffset(0)] public QueryDataNthChildInfo nthChild;
        [FieldOffset(0)] public QueryDataConditionInfo conditionInfo;

        [FieldOffset(0)] private long v0;
        [FieldOffset(8)] private long v1;

        public static bool operator ==(QueryData a, QueryData b) {
            return a.v0 == b.v0 && a.v1 == b.v1;
        }

        public static bool operator !=(QueryData a, QueryData b) {
            return !(a == b);
        }

    }
#pragma warning restore 660,661

    internal struct QueryDataAttributeInfo {

        public int tagId;

    }

    internal struct QueryDataStateInfo {

        public StyleState state;

        public QueryDataStateInfo(StyleState state) {
            this.state = state;
        }

    }

    internal struct QueryDataNthChildInfo {
        public short stepSize;
        public short offset;
    }


    internal struct QueryDataConditionInfo {
        public int tagId;
    }

    internal struct QueryDataChildCountInfo {

        public int targetCount;
        public ChildCountOperator op;

    }

    internal enum QueryType : byte {

        ChildCount,
        HasAttribute,
        HasAttributeWithValue,
        HasStyle,
        HasTag,
        HasSiblings,
        NthChild,
        State,
        Root,
        FirstChild,
        OnlyChild,
        LastChild,
        Condition,
    }

    internal struct QueryTable {

        public QueryId queryId;
        public QueryData queryData;
        public QueryType queryType;
        public bool invert;

        public static void RunStateQuery(CheckedArray<StyleState> stateByFlattenedElement, QueryDataStateInfo stateInfo, LongBoolMap resultSet) {
            StyleState state = stateInfo.state;

            for (int i = 0; i < stateByFlattenedElement.size; i++) {
                if ((stateByFlattenedElement[i] & state) != 0) {
                    resultSet.Set(i);
                }
            }

        }

        public static void RunChildCountQuery(CheckedArray<int> childCounts, QueryDataChildCountInfo childCountInfo, LongBoolMap resultSet) {

            ChildCountOperator op = childCountInfo.op;
            int targetCount = childCountInfo.targetCount;

            switch (op) {

                case ChildCountOperator.EqualTo: {
                    for (int i = 0; i < childCounts.size; i++) {
                        if (childCounts[i] == targetCount) {
                            resultSet.Set(i);
                        }
                    }

                    break;
                }

                case ChildCountOperator.NotEqualTo: {
                    for (int i = 0; i < childCounts.size; i++) {
                        if (childCounts[i] != targetCount) {
                            resultSet.Set(i);
                        }
                    }

                    break;
                }

                case ChildCountOperator.GreaterThan: {
                    for (int i = 0; i < childCounts.size; i++) {
                        if (childCounts[i] > targetCount) {
                            resultSet.Set(i);
                        }
                    }

                    break;
                }

                case ChildCountOperator.LessThan: {
                    for (int i = 0; i < childCounts.size; i++) {
                        if (childCounts[i] < targetCount) {
                            resultSet.Set(i);
                        }
                    }

                    break;
                }

                case ChildCountOperator.GreaterThanEqualTo: {
                    for (int i = 0; i < childCounts.size; i++) {
                        if (childCounts[i] >= targetCount) {
                            resultSet.Set(i);
                        }
                    }

                    break;
                }

                case ChildCountOperator.LessThanEqualTo: {
                    for (int i = 0; i < childCounts.size; i++) {
                        if (childCounts[i] <= targetCount) {
                            resultSet.Set(i);
                        }
                    }

                    break;
                }

            }
        }

    }

}