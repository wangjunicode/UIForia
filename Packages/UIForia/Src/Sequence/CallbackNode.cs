using UIForia.Util;

namespace UIForia {

    public class CallbackNode : IScriptNode {

        public bool IsComplete { get; set; }

        public void Reset() {
            IsComplete = false;
        }

        public void Update(SequenceContext context, StructList<ElementId> targets) { }

    }

}