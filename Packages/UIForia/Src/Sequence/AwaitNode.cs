using UIForia.Util;

namespace UIForia {

    public class AwaitNode : IAwaitableNode {

        private IAwaitableNode awaitableNode;

        public AwaitNode(IAwaitableNode awaitableNode) {
            this.awaitableNode = awaitableNode;
        }

        public bool IsComplete { get; private set; }

        public void Reset() {
            IsComplete = false;
            awaitableNode?.Reset();
        }

        public void Update(SequenceContext context, StructList<ElementId> targets) {
            if (IsComplete) return;
            awaitableNode.Await(context, targets);
            IsComplete = awaitableNode.IsComplete;
        }

        public void Await(SequenceContext context, StructList<ElementId> targets) {
            if (IsComplete) return;
            awaitableNode.Await(context, targets);
            IsComplete = awaitableNode.IsComplete;
        }

    }

}