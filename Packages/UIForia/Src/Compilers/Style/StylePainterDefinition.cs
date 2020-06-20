using System;
using UIForia.Graphics;

namespace UIForia.Compilers.Style {

    public struct StylePainterDefinition {
        
        public string fileName;
        public string painterName;
        public PainterVariableDeclaration[] definedVariables;
        public Action<StylePainterContext> paintBackground;
        public Action<StylePainterContext> paintForeground;
        public string drawBgSrc;
        public string drawFgSrc;
        public int backgroundFnIndex;
        public int foregroundFnIndex;

    }

}