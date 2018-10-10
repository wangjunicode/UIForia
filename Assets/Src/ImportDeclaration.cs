using System;

namespace Src {

    public class ImportDeclaration {

        public readonly string path;
        public readonly string alias;
        public Type type;

        public ImportDeclaration(string path, string alias) {
            this.path = path;
            this.alias = alias;
        }

    }

}