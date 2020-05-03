using System;

namespace UIForia {
    
    public struct ModuleBuilder {

        private readonly int moduleIndex;
        private readonly int moduleNameHash;
        private readonly StyleDatabase database;
        
        internal ModuleBuilder(StyleDatabase database, int moduleNameHash, int moduleIndex) {
            this.database = database;
            this.moduleIndex = moduleIndex;
            this.moduleNameHash = moduleNameHash;
        }

        public void AddStyleSheet(string styleSheetName, Action<StyleSheetBuilder> action) {
            if (action == null) return;
            database.AddStyleSheet(styleSheetName, moduleNameHash, moduleIndex, action);
        }

    }

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
            database.AddStyle(styleName, moduleIndex, styleSheetIndex, action);
        }

    }

}