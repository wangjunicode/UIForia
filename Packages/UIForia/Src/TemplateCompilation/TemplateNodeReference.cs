namespace UIForia.Compilers {

    public struct TemplateNodeReference {

        public readonly TemplateFileShell fileShell;
        public readonly int templateNodeId;

        public TemplateNodeReference(TemplateFileShell fileShell, int templateNodeId) {
            this.fileShell = fileShell;
            this.templateNodeId = templateNodeId;
        }

    }

}