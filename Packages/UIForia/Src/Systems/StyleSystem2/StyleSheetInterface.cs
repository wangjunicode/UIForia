namespace UIForia {

    public struct StyleSheetInterface {

        private readonly StyleDatabase db;
        private readonly int sheetIndex;

        internal StyleSheetInterface(StyleDatabase db, int sheetIndex) {
            this.db = db;
            this.sheetIndex = sheetIndex;
        }

        public StyleId GetStyle(string styleName) {
            return db.GetStyleId(StyleDatabase.MakeStyleKey(sheetIndex, styleName));
        }

        public bool TryGetStyle(string styleName, out StyleId styleId) {
            return db.TryGetStyleId(StyleDatabase.MakeStyleKey(sheetIndex, styleName), out styleId);
        }

    }

}