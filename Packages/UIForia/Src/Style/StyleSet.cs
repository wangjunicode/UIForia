namespace UIForia.Style {

    public struct StyleSet {

        internal ElementId elementId;
        internal StyleSystem2 styleSystem;

        public StyleSet(ElementId elementId, StyleSystem2 styleSystem) {
            this.elementId = elementId;
            this.styleSystem = styleSystem;
        }
        //
        // public void SetStyleList(IList<Style.Style> styleList) {
        //     styleSystem.SetStyleList(styleList);
        // }

    }

}