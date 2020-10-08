using UIForia.Attributes;
using UIForia.Elements;

namespace CompilerTest {

    [Template]
    public class GenericThing<T> : UIElement {

        public T value;

    }

}