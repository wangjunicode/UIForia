namespace UIForia {

    public struct StyleLocation {

        public readonly string filePath;

        public StyleLocation(string filePath) {
            this.filePath = filePath;
        }

        public override string ToString() {
            return filePath;
        }

    }

    public struct TemplateLocation {

        public readonly string filePath;
        public readonly string templateId;

        public TemplateLocation(string filePath, string templateId = null) {
            this.filePath = filePath;
            this.templateId = templateId;
        }

        public static implicit operator TemplateLocation(string value) {
            return new TemplateLocation(value);
        }
        public override string ToString() {
            if (string.IsNullOrEmpty(templateId)) {
                return filePath;
            }
            return filePath + "#" + templateId;
        }

    }

}