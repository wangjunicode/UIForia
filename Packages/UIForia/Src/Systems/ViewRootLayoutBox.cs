using UIForia.Layout;

namespace UIForia.Systems {

    public class ViewRootLayoutBox : FastLayoutBox {

        protected override void PerformLayout() {
            
            float width = element.View.Viewport.width;
            float height = element.View.Viewport.height;
            
            SizeConstraints targetSize = default;
            
            if (firstChild == null) return;
            
            firstChild.GetWidth(width, ref targetSize);
            firstChild.GetHeight(targetSize.prefWidth, width, height, ref targetSize);
            
            firstChild.ApplyHorizontalLayout(0, width, width, targetSize.prefWidth, default, Fit.Unset);
            firstChild.ApplyVerticalLayout(0, height, height, targetSize.prefHeight, default, Fit.Unset);
            
            // todo figure out how a view can take the size of it's contents
         
            firstChild.Layout();
            
        }

    }

}