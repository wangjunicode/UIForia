using Rendering;
using Src.Systems;

namespace Src.Layout.LayoutTypes {

    public class TextContainerLayoutBox : LayoutBox {

        protected ITextSizeCalculator textSizeCalculator;
        
        public TextContainerLayoutBox(ITextSizeCalculator textSizeCalculator, LayoutSystem2 layoutSystem, UIElement element)
            : base(layoutSystem, element) {
            this.textSizeCalculator = textSizeCalculator;
        }

        public override void RunLayout() {
            // no-op I think, maybe just apply padding / indent / whatever to text child
            // or maybe this evolves into handling inline things in text.
        }

        public void SetTextContent(string text) { }

        protected override float GetMinRequiredHeightForWidth(float width) {
            return textSizeCalculator.CalcTextHeight(element.style.textContent, element.style, width);
        }

    }

}