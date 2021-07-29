namespace UIForia.Text {

    internal struct TextSymbol {

        // todo -- compress to 4 bytes 
        public SymbolType symbolType;
        public int dataLength;
        public int metaInfo; // todo -- temp 
        
        public const int k_EmptyCharacterGroup = -1;

        public TextSymbol(SymbolType symbolType, int dataLength) {
            this.symbolType = symbolType;
            this.dataLength = dataLength;
            this.metaInfo = 0; 
        }

    }
}