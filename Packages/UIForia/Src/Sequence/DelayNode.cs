using UIForia.Util;

namespace UIForia {

    public class DelayNode : IScriptNode {

        public int delayMS;
        public int elapsed;
        
        public DelayNode(int delayMS) {
            this.delayMS = delayMS;
        }

        public bool IsComplete { get; private set; }

        public void Reset() {
            elapsed = 0;
            IsComplete = false;
        }

        public void Update(SequenceContext context, StructList<ElementId> targets) {

            IsComplete = elapsed >= delayMS;
            elapsed += context.deltaTime;

        }

    }

}