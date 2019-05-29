using UIForia.Animation;
using UIForia.Elements;

namespace UIForia.Rendering {
    public class AnimationRunCommand : IRunCommand {

        public AnimationData animationData;

        public void Run(UIElement element) {
            element.Application.Animate(element, animationData);
        }
    }
}
