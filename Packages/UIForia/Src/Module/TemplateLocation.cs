namespace UIForia {

    public struct StyleLocation {

        public readonly string filePath;

        public StyleLocation(string filePath) {
            this.filePath = filePath;
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

    }

}