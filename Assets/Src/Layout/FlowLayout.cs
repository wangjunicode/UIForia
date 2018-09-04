
using Rendering;
using UnityEngine;

namespace Src.Layout {

    public class FlowLayout : UILayout {

        public override void Run(Rect viewport, LayoutNode currentNode) { }

        public FlowLayout(ITextSizeCalculator textSizeCalculator) : base(textSizeCalculator) { }

    }

}