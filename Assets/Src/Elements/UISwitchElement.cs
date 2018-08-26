namespace Src {

    public abstract class UISwitchElement : UIElement { }

    public class UISwitchElement<T> : UISwitchElement {

        public T currentValue;

    }

}