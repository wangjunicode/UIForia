using UIForia.Util;

namespace UIForia {

    public class Sequence : IScriptNode {

        public LightList<IScriptNode> steps;
        private int currentStepIndex;

        public bool IsComplete { get; private set; }

        public void Reset() {
            currentStepIndex = 0;
            for (int i = 0; i < steps.size; i++) {
                steps[i].Reset();
            }
        }

        public void Update(SequenceContext context, StructList<ElementId> targetList) {

            for (; currentStepIndex < steps.size; currentStepIndex++) {
                IScriptNode node = steps[currentStepIndex];

                node.Update(context, targetList); // does this take target list?

                if (!node.IsComplete) {
                    break;
                }
            }

            IsComplete = currentStepIndex == steps.size;
        }

    }

}