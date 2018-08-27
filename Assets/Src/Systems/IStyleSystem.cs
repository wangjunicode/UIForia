using Rendering;

namespace Src.Systems {

    public interface IStyleSystem : IStyleChangeHandler {

        event PaintChanged onPaintChanged;
        event LayoutChanged onLayoutChanged;
        event RectPropertyChanged onRectChanged;
        event ContentBoxChanged onMarginChanged;
        event ContentBoxChanged onBorderChanged;
        event ContentBoxChanged onPaddingChanged;
        event ConstraintChanged onConstraintChanged;
        event BorderRadiusChanged onBorderRadiusChanged;
        event FontPropertyChanged onFontPropertyChanged;
        event AvailableStatesChanged onAvailableStatesChanged;
        event TextContentChanged onTextContentChanged;
        
        UIStyleSet GetStyleForElement(int elementId);

        void EnterState(int elementId, StyleState state);
        void ExitState(int elementId, StyleState state);

    }

}