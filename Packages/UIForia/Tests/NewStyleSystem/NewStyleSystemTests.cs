using System;
using NUnit.Framework;
using UIForia;
using UIForia.Elements;
using UIForia.Selectors;
using UIForia.Style;
using UIForia.Util;
using UnityEngine;

namespace Tests {

    public class NewStyleSystemTests {

        [Test]
        public unsafe void StyleSystemWorks() {

            VertigoStyleSystem styleSystem = new VertigoStyleSystem();

            styleSystem.AddStyleSheet("test-sheet", (styleSheet) => {
                
                styleSheet.AddStyle("simple-style", (style) => {

                    style.Set(StyleProperty2.BackgroundColor(Color.red));

                    style.AddSelector(FromTarget.Children, element => true, (selector) => {
                        selector.Set(StyleProperty2.BorderTop(32));
                        selector.Set(StyleProperty2.BorderRight(32));
                        selector.Set(StyleProperty2.BorderBottom(32));
                        selector.Set(StyleProperty2.BorderLeft(32));
                    });

                });
                
                styleSheet.AddStyle("style2", (style) => {
                    style.Set(StyleProperty2.PreferredWidth(100));
                });
                
            });

            MockElement rootElement = MockElement.CreateTree(styleSystem, (e) => {
                e.AddChild("1").SetSharedStyles("test-sheet/simple-style");
                e.AddChild("2").SetSharedStyles("test-sheet/style2", "test-sheet/simple-style");
                e.AddChild("3");
                e.AddChild("4");
            });

            styleSystem.TryResolveStyle("test-sheet", "simple-style", out StyleId simpleStyleId);
            styleSystem.TryResolveStyle("test-sheet", "style2", out StyleId style2Id);
            
            Assert.AreEqual(2, styleSystem.changedStyleSets.size);
            Assert.AreEqual(rootElement[0].id, styleSystem.sharedStyleChangeSets.array[0].elementId);
            Assert.AreEqual(rootElement[1].id, styleSystem.sharedStyleChangeSets.array[1].elementId);
            Assert.AreEqual(3, styleSystem.changeSetStyleBuffer.size);
            
            Assert.AreEqual(simpleStyleId, styleSystem.changeSetStyleBuffer[0]);
            Assert.AreEqual(style2Id, styleSystem.changeSetStyleBuffer[1]);
            Assert.AreEqual(simpleStyleId, styleSystem.changeSetStyleBuffer[2]);
            
            Assert.AreEqual(styleSystem.changedStyleSets.array[0], rootElement[0].styleSet2);
            Assert.AreEqual(styleSystem.changedStyleSets.array[1], rootElement[1].styleSet2);
            
            Assert.AreEqual(styleSystem.sharedStyleChangeSets.array[0].elementId, rootElement[0].styleSet2.element.id);
            Assert.AreEqual(styleSystem.sharedStyleChangeSets.array[1].elementId, rootElement[1].styleSet2.element.id);
            
            styleSystem.OnUpdate();
                        
            styleSystem.Destroy();

        }

        internal class MockElement : UIElement {

            public string name;
            public int depth;
            public Context context;

            public MockElement(Context context, MockElement parent) {
                this.id = context.idGenerator++;
                this.context = context;
                this.depth = parent?.depth ?? 0;
                this.parent = parent;
                this.children = new LightList<UIElement>();
                this.styleSet2 = new StyleSet(this, context.styleSystem);
                this.flags = UIElementFlags.EnabledFlagSet;
            }

            public MockElement AddChild(string name, Action<MockElement> action = null) {
                MockElement child = new MockElement(context, this);
                child.name = name;
                children.Add(child);
                action?.Invoke(child);
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

            private static VertigoStyleSystem system;

            public class Context {

                public int idGenerator;
                public VertigoStyleSystem styleSystem;
                public MockElement root;

            }

            public static MockElement CreateTree(VertigoStyleSystem styleSystem, Action<MockElement> action) {
                Context context = new Context();
                context.styleSystem = styleSystem;
                MockElement retn = new MockElement(context, null);
                action(retn);
                return retn;
            }

            private bool TryResolveStyle(string sheetName, string styleName, out StyleId styleId) {
                styleId = default;
                VertigoStyleSheet sheet = context.styleSystem.GetStyleSheet(sheetName);
                return sheet != null && sheet.TryGetStyle(styleName, out styleId);
            }

            public void SetSharedStyles(params string[] styleNames) {

   
                unsafe {

                    StackLongBuffer16 buffer = default;

                    for (int i = 0; i < styleNames.Length; i++) {
                        string[] split = styleNames[i].Split('/');
                        string sheetName = split[0];
                        string styleName = split[1];
                        if (TryResolveStyle(sheetName, styleName, out StyleId styleId)) {
                            // styles.Add(styleId);
                            buffer.array[buffer.size++] = styleId;
                        }
                    }

                    styleSet2.SetSharedStyles(ref buffer);

                }

            }

            public MockElement SetInstanceStyle(StyleProperty2 property2, StyleState2 state = StyleState2.Normal) {
                return this;
            }

        }

    }

}