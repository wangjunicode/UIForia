using Unity.Jobs;

namespace UIForia {

    public class DummyPainter : UIPainter {

        public override JobHandle SchedulePaintForeground() {
            return default;
        }

        public override JobHandle SchedulePaintBackground() {
            return default;
        }

        public override JobHandle ScheduleUpdate() {
            return default;
        }

        public override void OnSetupFrame() { }

        public override void OnTeardownFrame() { }

        public override void OnUpdate() { }

        public override void PaintForeground(ref UIRenderContext2D ctx) { }

        public override void PaintBackground(ref UIRenderContext2D ctx) { }

    }

}