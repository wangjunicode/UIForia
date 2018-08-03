namespace Src {
    public struct PropertyBindPair {
        public readonly string key;
        public readonly string value;

        public PropertyBindPair(string key, string value) {
            this.key = key;
            this.value = value;
        }
    }
}