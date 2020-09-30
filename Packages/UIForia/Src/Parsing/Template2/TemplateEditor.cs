using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia {

    public struct TemplateStyleNode {

        public readonly string path;
        public readonly string alias;
        public readonly string source;

        public TemplateStyleNode(string path, string alias, string source) {
            this.path = path;
            this.alias = alias;
            this.source = source;
        }

    }
    
    public struct TemplateUsingNode {

        public readonly string namespaceName;

        public TemplateUsingNode(string namespaceName) {
            this.namespaceName = namespaceName;
        }

    }
    
    public class TemplateEditor {

        private TemplateFileShell shell;
        private LightList<TemplateEditorRootNode> rootNodes;
        private LightList<TemplateStyleNode> styleNodes;
        private LightList<TemplateUsingNode> usingNodes;

        // todo -- after creating, we want to forget about the shell, essentially discard it
        public TemplateEditor(TemplateFileShell shell) {
            this.shell = shell;
            this.rootNodes = new LightList<TemplateEditorRootNode>(shell.rootNodes.Length);
            for (int i = 0; i < shell.rootNodes.Length; i++) {
                rootNodes[i] = Create(i);
            }

            rootNodes.size = shell.rootNodes.Length;
            
            this.styleNodes = new LightList<TemplateStyleNode>(shell.styles.Length);
            for (int i = 0; i < shell.styles.Length; i++) {
                StyleDeclaration style = shell.styles[i];
                string stylePath = shell.GetString(style.path); 
                string styleAlias = shell.GetString(style.alias); 
                string styleSource = shell.GetString(style.sourceBody); 
                styleNodes.array[i] = new TemplateStyleNode(stylePath, styleAlias, styleSource);
            }

            styleNodes.size = shell.styles.Length;
            
            this.usingNodes = new LightList<TemplateUsingNode>(shell.usings.Length);
            for (int i = 0; i < shell.usings.Length; i++) {
                UsingDeclaration usingNode = shell.usings[i];
                usingNodes.array[i] = new TemplateUsingNode(shell.GetString(usingNode.namespaceRange));
            }

            usingNodes.size = shell.usings.Length;
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

            TemplateEditorNode editorNode = new TemplateEditorNode(shell.GetString(node.moduleNameRange), shell.GetString(node.tagNameRange));

            for (int i = node.attributeRangeStart; i < node.attributeRangeEnd; i++) {
                AttributeDefinition3 attr = shell.attributeList[i];
                string attrKey = shell.GetString(attr.key);
                string attrValue = shell.GetString(attr.value);
                editorNode.attributeNodes.Add(new TemplateEditorAttributeNode(attrKey, attrValue, attr.type, attr.flags));
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

            rootNode.templateId = shell.GetString(root.templateNameRange);
            rootNode.shell = shell;
            TemplateASTNode node = shell.templateNodes[root.templateIndex];
            
            // rootNode.firstChild = CreateChildNode(shell.templateNodes[rootId].firstChildIndex);
            for (int i = node.attributeRangeStart; i < node.attributeRangeEnd; i++) {
                AttributeDefinition3 attr = shell.attributeList[i];
                string attrKey = shell.GetString(attr.key);
                string attrValue = shell.GetString(attr.value);
                rootNode.attributeNodes.Add(new TemplateEditorAttributeNode(attrKey, attrValue, attr.type, attr.flags));
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