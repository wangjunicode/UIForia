using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;

namespace UIForia {

    public unsafe class AnimateResultsNode : IAwaitableNode {

        private int executionToken;
        private AnimationReference animationRef;
        
        public AnimateResultsNode(AnimationReference animationRef) {
            this.animationRef = animationRef;
            this.executionToken = -1;
        }

        public bool IsComplete { get; set; }

        public void Reset() {
            IsComplete = false;
            executionToken = -1;
        }

        public void Update(SequenceContext context, StructList<ElementId> targets) {
            if (IsComplete) return;

            if (targets.size > 0) {

                executionToken = context.appInfo->executionTokenGenerator.Get();
                context.appInfo->executionTokenGenerator.Set(executionToken + 1);

                for (int i = 0; i < targets.size; i++) {

                    context.appInfo->perFrameAnimationCommands.Add(new AnimationCommand() {
                        type = AnimationCommandType.Play,
                        animationReference = animationRef,
                        elementId = targets[i],
                        executionToken = executionToken
                    });

                }
            }

            IsComplete = true;
        }

        public void Await(SequenceContext context, StructList<ElementId> targets) {
            if (IsComplete) return;

            DataList<AnimationInstance> animationList = context.appInfo->activeAnimationList;

            if (executionToken == -1) {
                
                executionToken = context.appInfo->executionTokenGenerator.Get();
                context.appInfo->executionTokenGenerator.Set(executionToken + 1);

                for (int i = 0; i < targets.size; i++) {

                    context.appInfo->perFrameAnimationCommands.Add(new AnimationCommand() {
                        type = AnimationCommandType.Play,
                        animationReference = animationRef,
                        elementId = targets[i],
                        executionToken = executionToken
                    });
                    
                }

                return;
            }

            bool anyRunning = false;
            
            for (int j = 0; j < animationList.size; j++) {
                ref AnimationInstance anim = ref animationList.Get(j);
                if (anim.executionToken == executionToken) {
                    anyRunning = true;
                    break;
                }
            }


            IsComplete = !anyRunning;
            
            
        }

    }

}