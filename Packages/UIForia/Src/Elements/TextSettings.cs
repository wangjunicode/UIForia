using System;

namespace UIForia.Elements {
    public struct TextSettings : IEquatable<TextSettings> {
        
        
        public bool multiline;
        
        public bool Equals(TextSettings other) {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj) {
            return obj is TextSettings other && Equals(other);
        }

        public override int GetHashCode() {
            throw new NotImplementedException();
        }
    }
}