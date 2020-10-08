namespace UIForia.Elements {

    public abstract class UIMixin {

        public string name;
        
        internal UIMixin next;
        internal UIMixin previous;
        
        public UIElement element { get; internal set; }

        public virtual void Initialize() { }
        
        public virtual void Destroy() {}

    }

}