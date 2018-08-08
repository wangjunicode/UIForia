using Rendering;

namespace Src {

    public abstract class TextPrimitive : UIRenderPrimitive {

        public abstract string Text { get; set; }

        public abstract void ApplyFontSettings(TextStyle fontSettings);

    }

}