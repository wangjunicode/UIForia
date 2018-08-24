using System.Collections.Generic;
using Rendering;

namespace Src.Systems {

    public interface IStyleSystem : IStyleChangeHandler {

        IReadOnlyList<UIStyleSet> GetAllStyles();
        IReadOnlyList<UIStyleSet> GetActiveStyles();

        event PaintChanged onPaintChanged;
        event LayoutChanged onLayoutChanged;
        event RectPropertyChanged onRectChanged;
        event ContentBoxChanged onMarginChanged;
        event ContentBoxChanged onBorderChanged;
        event ContentBoxChanged onPaddingChanged;
        event ConstraintChanged onConstraintChanged;
        event BorderRadiusChanged onBorderRadiusChanged;
        event FontPropertyChanged onFontPropertyChanged;

    }

}