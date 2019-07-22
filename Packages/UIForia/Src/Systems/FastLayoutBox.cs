using UIForia.Elements;
using UIForia.Layout;

namespace UIForia.Systems {

    public abstract class FastLayoutBox {

        public LayoutRenderFlag flags;
        public FastLayoutBox relayoutBoundary;
    
        public FastLayoutBox parent;
        public FastLayoutBox firstChild;
        public FastLayoutBox nextSibling;
        public FastLayoutBox prevSibling;

        // optimized to use bits for units & holds resolved value 
        public OffsetBox paddingBox;
        public OffsetBox borderBox;
        public OffsetBox marginBox;

        // holds units in bit field
        public MeasurementSet widthMeasurement;
        public MeasurementSet heightMeasurement;

        public LayoutBehavior layoutBehavior;
        public BoxConstraint constraints;
        public FastLayoutSystem layoutSystem;
        public bool sizedByParent;
        internal bool isInPool;
        public int depth;

        public UIElement element;
        
        protected FastLayoutBox(UIElement element) {
            this.element = element;
        }

        public void Layout() { }

        public float GetIntrinsicMinWidth() {
            return 0;
        }

        public float GetIntrinsicMinHeight() {
            return 0;
        }

        public float GetIntrinsicMaxWidth() {
            return float.MaxValue;
        }

        public float GetIntrinsicMaxHeight() {
            return float.MaxValue;
        }

        public float GetMinWidth(in BoxConstraint constraint) {
            
            switch (widthMeasurement.minUnit) {
                
                case UIMeasurementUnit.Content:
                    return GetIntrinsicMinWidth();
                
                case UIMeasurementUnit.Pixel:
                    return widthMeasurement.min.value;
                
                case UIMeasurementUnit.Percentage:
                    return constraint.minWidth * widthMeasurement.min.value;
                
                case UIMeasurementUnit.Em:
                    return 0;
                
                case UIMeasurementUnit.ViewportWidth:
                    return element.View.Viewport.width * widthMeasurement.min.value;
                
                  
                case UIMeasurementUnit.ViewportHeight:
                    return element.View.Viewport.height * widthMeasurement.min.value;
                
            }
            
        }
        
        public Size GetSize(in BoxConstraint constraint) {

            // min width of content is the intrinsic content min
            float minWidth = GetMinWidth(constraint);
            
            // max width of content is the intrinsic content max
            float maxWidth = GetMaxWidth(constraint);

            
            float prefWidth = GetPrefWidth(constraint);
            
            return new Size();

        }
        
        public void Layout(BoxConstraint constraints, bool parentUsesSize = false) {
            FastLayoutBox relayoutBoundary;

            if (!parentUsesSize || sizedByParent || constraints.isTight) {
                relayoutBoundary = this;
            } 
            else {
                relayoutBoundary = parent.relayoutBoundary;
            }
    
            if ((flags & LayoutRenderFlag.NeedsLayout) != 0 && this.constraints.Equals(constraints) && this.relayoutBoundary == relayoutBoundary) {
                return;
            }
        
            this.constraints = constraints;
            this.relayoutBoundary = relayoutBoundary;
    
            if (sizedByParent) {
                PerformResize();
            }

            PerformLayout();
        
            flags &= ~(LayoutRenderFlag.NeedsLayout);

            MarkNeedsPaint();
        }

        public void PerformResize() { }
        
        public abstract void PerformLayout();

        public void MarkNeedsPaint() { }

        public void MarkNeedsLayout() {

            if ((flags & LayoutRenderFlag.NeedsLayout) != 0) {
                return;
            }

            if (relayoutBoundary != this) {
                parent.MarkNeedsLayout();
            } 
            else {
                flags |= LayoutRenderFlag.NeedsLayout;
                layoutSystem.nodesNeedingLayout.Add(this); // add to root list of nodes needing layout
                // renderSystem.RequireRender();
            }

        }

        public virtual void OnStyleChanged() {

        }

    }

}