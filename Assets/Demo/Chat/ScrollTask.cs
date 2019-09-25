using UIForia.Elements;
using UIForia.Systems;

namespace Demo {

    public class ScrollTask : UITask {

        public ScrollView scrollView;

        public float percentage;

        public bool runThisFrame;

        public ScrollTask(ScrollView scrollView, float percentage) {
            this.scrollView = scrollView;
            this.percentage = percentage;
        }

        public override UITaskResult Run(float deltaTime) {
            if (!runThisFrame) {
                runThisFrame = true;
                return UITaskResult.Running;
            }
            scrollView.ScrollToVerticalPercent(percentage);
            return UITaskResult.Completed;
        }

    }

}