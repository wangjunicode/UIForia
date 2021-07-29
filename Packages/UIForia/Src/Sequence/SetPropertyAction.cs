using UIForia.Util;

namespace UIForia {

    public class SetPropertyAction : IScriptNode {

        public NamedProperty property;

        public SetPropertyAction(NamedProperty namedProperty) {
            this.property = namedProperty;
        }

        public bool IsComplete { get; set; }

        public void Reset() {
            IsComplete = false;
        }

        public void Update(SequenceContext context, StructList<ElementId> targets) {
            if (IsComplete) return;
            context.SetProperty(property);
            IsComplete = true;
        }

    }

}