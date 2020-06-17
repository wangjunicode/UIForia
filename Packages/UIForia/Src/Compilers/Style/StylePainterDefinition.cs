using System;
using UIForia.Elements;
using UIForia.Graphics;

namespace UIForia.Compilers.Style {

    public struct StylePainterDefinition {
        
        public string fileName;
        public PainterVariableDeclaration[] definedVariables;
        public Action<ShapeContext, UIElement> paintBackground;
        public Action<ShapeContext, UIElement> paintForeground;

    }

}