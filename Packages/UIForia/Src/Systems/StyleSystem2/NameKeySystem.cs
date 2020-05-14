namespace UIForia {

    public struct NameKey {

        public int key;

        public NameKey(int value) {
            this.key = value;
        }
        
        public static implicit operator NameKey(int key) {
            return new NameKey(key);
        }

        public static implicit operator int(NameKey nameKey) {
            return nameKey.key;
        }

    }
    

}