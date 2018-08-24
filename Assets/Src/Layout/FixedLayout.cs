using Rendering;
using UnityEngine;

namespace Src.Layout {

    public class FixedLayout : UILayout {

        public override void Run(Rect viewport, LayoutDataSet size, Rect[] results) {
            throw new System.NotImplementedException();
        }

        public FixedLayout(ITextSizeCalculator textSizeCalculator) : base(textSizeCalculator) { }

    }

}