
using Rendering;
using UnityEngine;

namespace Src.Layout {

    public class FlowLayout : UILayout {

        public override void Run(Rect viewport, LayoutNode currentNode, Rect[] results) { }

        public FlowLayout(ITextSizeCalculator textSizeCalculator) : base(textSizeCalculator) { }

    }

}