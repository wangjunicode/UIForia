using System;

namespace UIForia {

    public struct StyleSheetBuilder {

        private readonly int moduleIndex;
        private readonly int styleSheetIndex;
        private readonly StyleDatabase database;

        internal StyleSheetBuilder(StyleDatabase database, int moduleIndex, int styleSheetIndex) {
            this.database = database;
            this.moduleIndex = moduleIndex;
            this.styleSheetIndex = styleSheetIndex;
        }

        public void AddAnimation() {
            throw new NotImplementedException();
        }

        public void AddSpriteSheet() {
            throw new NotImplementedException();
        }

        public void AddSound() {
            throw new NotImplementedException();
        }

        public void AddCursor() {
            throw new NotImplementedException();
        }

        public void AddConstant() {
            throw new NotImplementedException();
        }

        public void AddStyle(string styleName, Action<StyleBuilder> action) {
            if (action == null) return;
            database.AddStyleFromBuilder(styleName, moduleIndex, styleSheetIndex, action);
        }

    }

}