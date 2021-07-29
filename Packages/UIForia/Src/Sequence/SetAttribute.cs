using UIForia.Util;

namespace UIForia {

    public class SetAttributeNode : IScriptNode {

        public string attr;
        public string value;
        public bool setFromRoot;

        public bool IsComplete { get; set; }

        public void Reset() {
            IsComplete = false;
        }

        public void Update(SequenceContext context, StructList<ElementId> targets) {

            if (setFromRoot) {
                context.SetAttribute(context.rootElementId, attr, value);

            }
            else {
                for (int i = 0; i < targets.size; i++) {
                    context.SetAttribute(targets[i], attr, value);
                }
            }

            IsComplete = true;
        }

    }

}