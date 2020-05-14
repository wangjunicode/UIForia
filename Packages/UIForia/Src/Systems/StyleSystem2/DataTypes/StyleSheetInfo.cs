namespace UIForia {

    public struct StyleSheetInfo {

        public readonly int moduleIndex;
        public readonly NameKey nameKey;

        public StyleSheetInfo(int moduleIndex, NameKey nameKey) {
            this.nameKey = nameKey;
            this.moduleIndex = moduleIndex;
        }

    }

}