using System;

namespace UIForia.Style2 {

    public struct StyleCondition {

        public readonly int id;
        public readonly string name;
        public readonly Func<DisplayConfiguration, bool> fn;

        public StyleCondition(int id, string name, Func<DisplayConfiguration, bool> fn) {
            this.id = id;
            this.name = name;
            this.fn = fn;
        }

    }

}