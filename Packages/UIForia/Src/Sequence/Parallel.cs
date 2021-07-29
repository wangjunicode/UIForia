using UIForia.Util;

namespace UIForia {

    public class Parallel : IScriptNode {

        private LightList<IScriptNode> steps;

        public bool IsComplete { get; private set; }

        public Parallel(LightList<IScriptNode> steps) {
            this.steps = steps;
        }

        public void Reset() {
            IsComplete = false;
            
            for (int i = 0; i < steps.size; i++) {
                steps[i].Reset();
            }
            
        }

        public void Update(SequenceContext context, StructList<ElementId> targets) {
            int completeCount = 0;

            for (int i = 0; i < steps.size; i++) {
                IScriptNode node = steps[i];

                if (!node.IsComplete) {
                    node.Update(context, targets);
                }

                if (node.IsComplete) {
                    completeCount++;
                }
                
            }

            IsComplete = completeCount == steps.size;
        }

    }

}