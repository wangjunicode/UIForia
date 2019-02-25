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
        
        public bool HasAttributeRule => rule?.attributeName != null;

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
