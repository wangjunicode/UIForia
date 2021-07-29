using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public class AnimateNode : IAwaitableNode {

        public AnimationCommandType commandType;

        public bool IsComplete { get; set; }

        public void Reset() {
            IsComplete = false;
        }

        public void Update(SequenceContext context, StructList<ElementId> targets) {
            if (IsComplete) return;
            Debug.Log("Animating " + context.rootElementId);
            IsComplete = true;
        }

        public void Await(SequenceContext context, StructList<ElementId> targets) { }

    }

}