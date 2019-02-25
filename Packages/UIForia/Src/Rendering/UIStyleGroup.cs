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
        
        public bool IsApplicableTo(UIElement element) {

            if (rule != null) {
                return rule.IsApplicableTo(element);
            }

            return true;
        }

        protected bool Equals(UIStyleGroup other) {
            return string.Equals(name, other.name) && styleType == other.styleType && Equals(hover, other.hover) && Equals(normal, other.normal) && Equals(active, other.active) && Equals(focused, other.focused) && Equals(rule, other.rule) && isExported == other.isExported;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UIStyleGroup) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (name != null ? name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) styleType;
                hashCode = (hashCode * 397) ^ (hover != null ? hover.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (normal != null ? normal.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (active != null ? active.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (focused != null ? focused.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (rule != null ? rule.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ isExported.GetHashCode();
                return hashCode;
            }
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
