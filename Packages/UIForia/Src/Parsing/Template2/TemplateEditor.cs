using UIForia.Util;

namespace UIForia {

    public class TemplateEditor {

        private TemplateFileShell shell;
        private LightList<TemplateEditorRootNode> rootNodes;

        // todo -- after creating, we want to forget about the shell, essentially discard it
        public TemplateEditor(TemplateFileShell shell) {
            this.shell = shell;
            this.rootNodes = new LightList<TemplateEditorRootNode>(shell.rootNodes.Length);
            for (int i = 0; i < shell.rootNodes.Length; i++) {
                rootNodes[i] = Create(i);
            }

            rootNodes.size = shell.rootNodes.Length;
        }

        public TemplateEditorRootNode GetTemplateRoot(string templateName = null) {
            for (int i = 0; i < rootNodes.size; i++) {
                if (rootNodes[i].templateId == templateName) {
                    return rootNodes[i];
                }
            }

            return null;
        }

        private TemplateEditorNode CreateChildNode(int templateId) {
            ref TemplateASTNode node = ref shell.templateNodes[templateId];
            TemplateEditorNode editorNode = new TemplateEditorNode(shell.GetModuleName(node.index), shell.GetRawTagName(node.index));

            for (int i = node.attributeRangeStart; i < node.attributeRangeEnd; i++) {
                AttributeDefinition2 attr = shell.attributeList[i];
                editorNode.attributeNodes.Add(new TemplateEditorAttributeNode(attr));
            }

            int childPtr = node.firstChildIndex;

            for (int i = 0; i < node.childCount; i++) {
                ref TemplateASTNode childNode = ref shell.templateNodes[childPtr];
                editorNode.AddChild(CreateChildNode(childPtr));
                childPtr = childNode.nextSiblingIndex;
            }

            return editorNode;
        }

        private TemplateEditorRootNode Create(int rootId) {
            ref TemplateASTRoot root = ref shell.rootNodes[rootId];

            TemplateEditorRootNode rootNode = new TemplateEditorRootNode();

            rootNode.templateId = default; //shell.GetString(root.templateNameId);
            rootNode.shell = shell;
            TemplateASTNode node = shell.templateNodes[root.templateIndex];
            
            // rootNode.firstChild = CreateChildNode(shell.templateNodes[rootId].firstChildIndex);
            for (int i = node.attributeRangeStart; i < node.attributeRangeEnd; i++) {
                AttributeDefinition2 attr = shell.attributeList[i];
                rootNode.attributeNodes.Add(new TemplateEditorAttributeNode(attr));
            }

            int childPtr = node.firstChildIndex;

            for (int i = 0; i < node.childCount; i++) {
                ref TemplateASTNode childNode = ref shell.templateNodes[childPtr];
                rootNode.AddChild(CreateChildNode(childPtr));
                childPtr = childNode.nextSiblingIndex;
            }

            return rootNode;
        }

    }

}