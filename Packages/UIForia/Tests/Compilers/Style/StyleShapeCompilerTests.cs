using NUnit.Framework;
using UIForia.Graphics;

namespace Tests.Compilers.Style {

    public class StyleShapeCompilerTests {

        [Test]
        public void Works() {

            new ShapeCompiler().Compile(@"

                SetColor(StyleColor.magenta);
                SetPosition(20, 20);
                FillRect(100, 100);
                
            ");

        }

    }

}