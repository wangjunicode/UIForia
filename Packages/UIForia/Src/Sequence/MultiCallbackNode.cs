using UIForia.Util;

namespace UIForia {

    public class MultiCallbackNode : IScriptNode {

        public bool IsComplete { get; set; }

        public void Reset() {
            IsComplete = false;
        }

        public void Update(SequenceContext context, StructList<ElementId> targets) {

            for (int i = 0; i < targets.size; i++) {
                // fn.Invoke(target);
            }

            // complete

        }

    }

}