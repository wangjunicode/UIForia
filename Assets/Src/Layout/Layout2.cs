namespace Src.Layout {

    public abstract class Layout2 {

        public abstract void Run(LayoutBox box, LayoutUpdateType layoutUpdateType);
        
        public virtual float GetContentPreferredWidth(LayoutBox box) {
            return 0;
        }

        public virtual float GetContentMinWidth(LayoutBox box) {
            return 0;
        }

        public virtual float GetContentMaxWidth(LayoutBox box) {
            return 0;
        }
        
        public virtual float GetContentPreferredHeight(LayoutBox box, float width) {
            return 0;
        }

    }

}