using System;

namespace UIForia.Parsing.Expressions {

    public struct ImportDeclaration {

        public readonly string path;
        public readonly string alias;

        public ImportDeclaration(string path, string alias) {
            this.path = path;
            this.alias = alias;
        }

    }

}