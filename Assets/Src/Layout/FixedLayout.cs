using Rendering;
using UnityEngine;

namespace Src.Layout {

    public class FixedLayout : UILayout {

        public FixedLayout(ITextSizeCalculator textSizeCalculator) : base(textSizeCalculator) { }

        public override void Run(Rect viewport, LayoutNode layoutNode, Rect[] results) {
            throw new System.NotImplementedException();
        }

    }

}