
using System.Collections.Generic;
using Rendering;
using Src.Util;
using UnityEngine;

namespace Src.Layout {

    public class FlowLayout : UILayout {

        public override List<Rect> Run(Rect viewport, LayoutNode currentNode) {
            return ListPool<Rect>.Get();
        }

        public FlowLayout(ITextSizeCalculator textSizeCalculator) : base(textSizeCalculator) { }

    }

}