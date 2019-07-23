using System;
using UIForia.Elements;

namespace UIForia.Systems {

    public class TranscludeLayoutBox : FastLayoutBox {

        public TranscludeLayoutBox(UIElement element) : base(element) { }

        public override void PerformLayout() {
            throw new NotImplementedException("Should never call layout on a transcluded layout box");
        }

    }

}