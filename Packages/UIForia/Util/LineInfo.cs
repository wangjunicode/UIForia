namespace UIForia.Util {

    public struct LineInfo {

        public readonly int line;
        public readonly int column;

        public LineInfo(int line, int column) {
            this.line = line;
            this.column = column;
        }

        public override string ToString() {
            return line + ":" + column;
        }

    }

}