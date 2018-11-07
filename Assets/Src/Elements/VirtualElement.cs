namespace UIForia.Elements {

    // virtual element means element knows about parent but not vice versa

    public abstract class VirtualElement : UIElement {

        protected VirtualElement() {
            flags |= UIElementFlags.VirtualElement;
        }

    }

   

}