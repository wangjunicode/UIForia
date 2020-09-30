using System.Collections.Generic;

namespace UIForia {

    public class TemplateEditorRootNode : TemplateEditorNode {

        internal TemplateFileShell shell;
        internal TemplateEditorNode firstChild;
        public string templateId;

        public TemplateEditorRootNode() : base(null, "__CONTENTS__") { }

    }

    public class TemplateEditorNode {

        internal string moduleName;
        internal string tagName;
        internal TemplateEditorNode parent;
        internal List<TemplateEditorNode> children;
        internal List<TemplateEditorAttributeNode> attributeNodes;

        public TemplateEditorNode(string moduleName, string tagName) {
            this.moduleName = moduleName;
            this.tagName = tagName;
            this.attributeNodes = new List<TemplateEditorAttributeNode>(4);
            this.children = new List<TemplateEditorNode>(4);
        }
        
        public int ChildCount => children.Count;

        public void AddChild(TemplateEditorNode child) {
            if (child.parent != null) {
                child.parent.RemoveChild(child);
            }
            child.parent = this;
            children.Add(child);
        }

        public void RemoveChild(TemplateEditorNode child) {
            children.Remove(child);
        }

        public TemplateEditorNode GetChild(int idx) {
            if (idx < 0 || idx >= ChildCount) {
                return null;
            }

            return children[idx];
        }

        public TemplateEditorAttributeNode GetAttribute(string id) {
            for (int i = 0; i < attributeNodes.Count; i++) {
                if (attributeNodes[i].key == id) {
                    return attributeNodes[i];
                }
            }

            return null;
        }

    }

}