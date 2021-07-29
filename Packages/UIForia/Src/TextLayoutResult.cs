namespace UIForia.Text {

    public struct TextLayoutResult {

        public float x;
        public float y;
        public float width;
        public float height;
        
        public ushort lineIndex;
        public ushort utilValue; // combine vAlign here? 
        public VerticalAlignment verticalAlignment;
        public int shapeResultIndex; //3  bytes is fine 

        public ushort GlyphCount {
            get => utilValue;
            set => utilValue = value;
        }

        public ushort StretchParts {
            get => utilValue;
            set => utilValue = value;
        }
            

    }

}