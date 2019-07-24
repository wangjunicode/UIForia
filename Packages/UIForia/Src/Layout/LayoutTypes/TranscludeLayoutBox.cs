using System;
using UIForia.Elements;

namespace UIForia.Layout {

    public class TranscludeLayoutBox : FastLayoutBox {
        
        protected override void PerformLayout() {
            throw new NotImplementedException("Should never call layout on a transcluded layout box");
        }

    }

}