namespace UIForia.Text {

    internal unsafe struct TextDataEntry {

        public bool explicitlyDirty;
        public byte nextSizeCacheIdx;
        public ElementId elementId;
        public TextLayoutInfo layoutInfo;

        public byte* buffer;
        public CheckedArray<TextCharacterRun> runList;

        public int dataLength;
        public int symbolLength;

        public int bufferCapacity; // also computable if going by bytes right?
        public float currentFlowWidth;
        public float cachedContentWidth;
        public float cachedContentHeight;
        public float sizeCache0Width;
        public float sizeCache0Height;
        public float sizeCache1Width;
        public float sizeCache1Height;
        
        public TextLayoutOutput layoutOutput;

        public void ClearCaches() {
            nextSizeCacheIdx = 0;
            cachedContentWidth = -1;
            cachedContentHeight = -1;
            sizeCache0Width = -1;
            sizeCache0Height = -1;
            sizeCache1Width = -1;
            sizeCache1Height = -1;
            currentFlowWidth = -1;
        }
        
        public CheckedArray<TextSymbol> GetSymbols() {
            TextSymbol* symbolsPtr = (TextSymbol*) (buffer + (dataLength * sizeof(char)));
            return new CheckedArray<TextSymbol>(symbolsPtr, symbolLength);
        }

        public CheckedArray<char> GetDataBuffer() {
            return new CheckedArray<char>((char*) buffer, dataLength);
        }

    }

}