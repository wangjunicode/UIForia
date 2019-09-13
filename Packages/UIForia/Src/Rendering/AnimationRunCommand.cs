using System;
using UIForia.Animation;
using UIForia.Elements;

namespace UIForia.Rendering {
    public class AnimationRunCommand : IRunCommand {

        public AnimationData animationData;

        public AnimationRunCommand(bool isExit, RunAction runAction = RunAction.Run) {
            IsExit = isExit;
            RunAction = runAction;
        }

        public void Run(UIElement element) {
            switch (RunAction) {
                case RunAction.Run:
                    element.Application.Animate(element, animationData);
                    break;
                case RunAction.Pause:
                    element.Application.PauseAnimation(element, animationData);
                    break;
                case RunAction.Stop:
                    element.Application.StopAnimation(element, animationData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsExit { get; }
        
        public RunAction RunAction { get; }

    }
}
