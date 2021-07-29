using UIForia.Style;

namespace UIForia {

    public class QueryContext {

        public UIElement element { get; internal set; }
        
        internal int activeElementIndex;

        internal CheckedArray<int> childCounts;
        internal CheckedArray<int> siblingIndexByActiveIndex;
        internal CheckedArray<int> activeIndexByElementId;
        internal CheckedArray<int> parentIndexByActiveElementIndex;
        internal CheckedArray<StyleState> stateByFlattenedElement;
        internal CheckedArray<CheckedArray<bool>> styleConditionsByViewIndex;
        internal CheckedArray<RuntimeTraversalInfo> runtimeInfoTable;
        internal CheckedArray<ElementId> elementIdByActiveIndex;
        
        internal StyleDatabase styleDatabase;
        internal UIElement[] elementTable;

        internal void SetElement(UIElement element) {
            this.element = element;
            activeElementIndex = activeIndexByElementId[element.elementId.index];
        }
        
        public int GetChildCount() {
            return childCounts[activeElementIndex];
        }

        public bool IsFirstChild() {
            return siblingIndexByActiveIndex[activeElementIndex] == 0;
        }

        public bool IsLastChild() {
            int parentIndex = parentIndexByActiveElementIndex[activeElementIndex];
            int parentChildCount = childCounts[parentIndex];
            return siblingIndexByActiveIndex[activeElementIndex] == parentChildCount - 1;
        }

        public bool IsInState(StyleState state) {
            return (stateByFlattenedElement[activeElementIndex] & state) != 0;
        }

        public bool IsOnlyChild() {
            int parentIndex = parentIndexByActiveElementIndex[activeElementIndex];
            int parentChildCount = childCounts[parentIndex];
            return parentChildCount == 1;
        }

        public bool HasSiblings() {
            int parentIndex = parentIndexByActiveElementIndex[activeElementIndex];
            int parentChildCount = childCounts[parentIndex];
            return parentChildCount != 1;
        }

        public bool NthChild(int stepSize, int offset) {
            
            bool negativeStep = stepSize < 0;

            if (stepSize == 0) {
                return siblingIndexByActiveIndex[activeElementIndex] + 1 == offset;
            }

            int siblingIndex = siblingIndexByActiveIndex[activeElementIndex];
            bool matchesPattern = (siblingIndex + 1 + offset) % stepSize == 0;
            bool isInBounds = (negativeStep && siblingIndex + 1 <= offset && matchesPattern) ||
                              (!negativeStep && siblingIndex + 1 >= offset && matchesPattern);

            return matchesPattern && isInBounds;
        }

        public bool Condition(string conditionName) {
            int conditionId = styleDatabase.conditionTagger.GetTagId(conditionName);
            return conditionId >= 0 && styleConditionsByViewIndex[element.View.viewId][conditionId];
        }

        public bool HasAttribute(string attrName) {
            return element.HasAttribute(attrName);
        }
        
        public bool HasAttribute(string attrName, string attrValue) {
            return element.TryGetAttribute(attrName, out string value) && value == attrValue;
        }

        public bool HasChildCountMatching(ChildCountOperator op, int targetCount) {

            switch (op) {

                case ChildCountOperator.EqualTo:
                    return childCounts[activeElementIndex] == targetCount;

                case ChildCountOperator.NotEqualTo:
                    return childCounts[activeElementIndex] != targetCount;

                case ChildCountOperator.GreaterThan:
                    return childCounts[activeElementIndex] > targetCount;

                case ChildCountOperator.LessThan:
                    return childCounts[activeElementIndex] < targetCount;

                case ChildCountOperator.GreaterThanEqualTo:
                    return childCounts[activeElementIndex] >= targetCount;

                case ChildCountOperator.LessThanEqualTo:
                    return childCounts[activeElementIndex] <= targetCount;

                default:
                    return false;
            }
        }


    }

}