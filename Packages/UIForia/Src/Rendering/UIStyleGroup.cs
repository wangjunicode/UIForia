namespace UIForia.Rendering {

    public class UIStyleGroup {

        public string name { get; internal set; }
        public StyleType styleType { get; internal set; }
        public UIStyle hover { get; internal set; }
        public UIStyle normal { get; internal set; }
        public UIStyle active { get; internal set; }
        public UIStyle focused { get; internal set; }
        public UIStyleRule rule { get; internal set; }

        public bool isExported { get; internal set; }

        public static bool operator ==(UIStyleGroup x, UIStyleGroup y) {
            return string.Equals(x.name, y.name) 
                   && Equals(x.hover, y.hover) 
                   && Equals(x.normal, y.normal) 
                   && Equals(x.active, y.active) 
                   && x.styleType == y.styleType 
                   && Equals(x.focused, y.focused)
                   && x.rule == y.rule
                   && x.isExported == y.isExported;
        }

        public static bool operator !=(UIStyleGroup x, UIStyleGroup y) {
            return !(x == y);
        }

        public int CountRules() {
            if (rule == null) return 0;
            UIStyleRule r = rule;
            int count = 1;
            while (r.next != null) {
                count++;
                r = r.next;
            }

            return count;
        }
    }
}
