using System;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;

namespace Tests {

    [ContainerElement]
    internal class MockElement : UIElement {

        public string name;
        public int depth;
        public Context context;

        public MockElement(Context context, MockElement parent) {
            
            this.depth = parent?.depth ?? 0;
            
            this.id = context.elementSystem.CreateElement(this.depth, 0, 0, UIElementFlags.EnabledFlagSet);
            context.styleSystem.CreateElement(id);
            this.context = context;
            this.parent = parent;
            this.children = new LightList<UIElement>();
            this.flags = UIElementFlags.EnabledFlagSet;
        }

        public bool IsDescendentOf(in MockElement info) {
            return ftbIndex > info.ftbIndex && btfIndex > info.btfIndex;
        }

        public bool IsAncestorOf(in MockElement info) {
            return ftbIndex < info.ftbIndex && btfIndex < info.btfIndex;
        }

        public bool IsParentOf(in MockElement info) {
            return depth == info.depth + 1 && ftbIndex < info.ftbIndex && btfIndex < info.btfIndex;
        }

        public MockElement AddChild(string name, Action<MockElement> action = null) {
            MockElement child = new MockElement(context, this);
            child.name = name;
            children.Add(child);
            action?.Invoke(child);
            context.elementSystem.AddChild(id, child.id);
            return child;
        }

        public MockElement AddChild(Action<MockElement> action = null) {
            return AddChild(null, action);
        }

        public override string ToString() {
            string str = "[" + id + "] depth = " + depth;
            if (name != null) {
                str += " (" + name + ")";
            }

            return str;
        }
        
        public class Context {

            public StyleSystem2 styleSystem;
            public ElementSystem elementSystem;

        }

        public static MockElement CreateTree(UIForiaSystems systems, Action<MockElement> action) {
            Context context = new Context();
            context.styleSystem = systems.styleSystem;
            context.elementSystem = systems.elementSystem;
            MockElement retn = new MockElement(context, null);
            action(retn);
            return retn;
        }
        

        public MockElement SetSharedStyles(params string[] styleNames) {

        

            return this;

        }

        public MockElement SetInstanceStyle(StyleProperty2 property2, StyleState2 state = StyleState2.Normal) {
            return this;
        }

        public MockElement GetDescendentByName(string s) {
            if (name == s) return this;
            if (children == null) return null;
            for (int i = 0; i < children.size; i++) {
                MockElement c = (MockElement) children[i];
                MockElement x = c.GetDescendentByName(s);
                if (x != null) {
                    return x;
                }
            }

            return null;
        }

    }

}