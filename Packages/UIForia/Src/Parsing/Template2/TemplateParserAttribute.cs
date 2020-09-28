using System;

namespace UIForia.Parsing {

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TemplateParserAttribute : Attribute {

        public readonly string extension;

        public TemplateParserAttribute(string extension) {
            if (extension[0] == '.') {
                extension = extension.Substring(1);
            }

            this.extension = extension;
        }

    }

}