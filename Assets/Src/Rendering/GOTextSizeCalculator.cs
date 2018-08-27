using System;

namespace Rendering {

    public class GOTextSizeCalculator : ITextSizeCalculator {

        public float CalcTextWidth(string text, UIStyleSet style) {
            return 100f;
        }

        public float CalcTextHeight(string text, UIStyleSet style, float width) {
            return 14f;
        }

    }

}