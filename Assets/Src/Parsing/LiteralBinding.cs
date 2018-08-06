namespace Src {

    public struct LiteralBinding {

        public readonly string propName;
        public readonly object value;

        public LiteralBinding(string propName, object value) {
            this.propName = propName;
            this.value = value;
        }

    }

}