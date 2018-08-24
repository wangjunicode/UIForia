namespace Rendering {

    public interface ITextSizeCalculator {

        float CalcTextWidth(string text, UIStyleSet style);
        float CalcTextHeight(string text, UIStyleSet style, float width);

    }

}