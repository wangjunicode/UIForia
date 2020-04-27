using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UIForia;
using UIForia.Elements;
using UIForia.Selectors;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Tests {

    public class NewStyleSystemTests {

        private static void CreateStandardStyleSheet(VertigoStyleSystem styleSystem) {
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

                styleSheet.AddStyle("style2", (style) => { style.Set(StyleProperty2.PreferredWidth(100)); });

            });
        }

        [Test]
        public void BuildTraversalTree() {
            VertigoStyleSystem styleSystem = new VertigoStyleSystem();
            MockElement tree = MockElement.CreateTree(styleSystem, root => {
                root.name = "a";
                root.AddChild("b", (b) => { b.AddChild("c"); });
                root.AddChild("d", (d) => {
                    d.AddChild("e", (e) => { e.AddChild("f"); });
                    d.AddChild("g", (g) => { g.AddChild("h"); });
                    d.AddChild("i", (i) => { i.AddChild("j"); });
                });
            });

            UnmanagedList<ElementTraversalInfo> traversalInfo = new UnmanagedList<ElementTraversalInfo>(32, Allocator.TempJob);

            BuildElementInfo(tree, traversalInfo);
            
            Assert.AreEqual(0, tree.GetDescendentByName("a").ftbIndex);
            Assert.AreEqual(1, tree.GetDescendentByName("b").ftbIndex);
            Assert.AreEqual(2, tree.GetDescendentByName("c").ftbIndex);
            Assert.AreEqual(3, tree.GetDescendentByName("d").ftbIndex);
            Assert.AreEqual(4, tree.GetDescendentByName("e").ftbIndex);
            Assert.AreEqual(5, tree.GetDescendentByName("f").ftbIndex);
            Assert.AreEqual(6, tree.GetDescendentByName("g").ftbIndex);
            Assert.AreEqual(7, tree.GetDescendentByName("h").ftbIndex);
            Assert.AreEqual(8, tree.GetDescendentByName("i").ftbIndex);
            Assert.AreEqual(9, tree.GetDescendentByName("j").ftbIndex);

            Assert.AreEqual(0, tree.GetDescendentByName("a").btfIndex);
            Assert.AreEqual(8, tree.GetDescendentByName("b").btfIndex);
            Assert.AreEqual(9, tree.GetDescendentByName("c").btfIndex);
            Assert.AreEqual(1, tree.GetDescendentByName("d").btfIndex);
            Assert.AreEqual(6, tree.GetDescendentByName("e").btfIndex);
            Assert.AreEqual(7, tree.GetDescendentByName("f").btfIndex);
            Assert.AreEqual(4, tree.GetDescendentByName("g").btfIndex);
            Assert.AreEqual(5, tree.GetDescendentByName("h").btfIndex);
            Assert.AreEqual(2, tree.GetDescendentByName("i").btfIndex);
            Assert.AreEqual(3, tree.GetDescendentByName("j").btfIndex);

            Assert.IsFalse(tree.GetDescendentByName("e").IsDescendentOf(tree.GetDescendentByName("b")));
            Assert.IsTrue(tree.GetDescendentByName("e").IsDescendentOf(tree.GetDescendentByName("d")));
            Assert.IsTrue(tree.GetDescendentByName("e").IsDescendentOf(tree.GetDescendentByName("a")));
            
            Assert.IsTrue(tree.GetDescendentByName("f").IsDescendentOf(tree.GetDescendentByName("a")));
            Assert.IsTrue(tree.GetDescendentByName("f").IsDescendentOf(tree.GetDescendentByName("d")));
            Assert.IsTrue(tree.GetDescendentByName("f").IsDescendentOf(tree.GetDescendentByName("e")));
            
            Assert.IsTrue(tree.GetDescendentByName("a").IsAncestorOf(tree.GetDescendentByName("f")));
            Assert.IsTrue(tree.GetDescendentByName("d").IsAncestorOf(tree.GetDescendentByName("f")));
            Assert.IsTrue(tree.GetDescendentByName("e").IsAncestorOf(tree.GetDescendentByName("f")));
            
            Assert.IsFalse(tree.GetDescendentByName("a").IsDescendentOf(tree.GetDescendentByName("f")));
            Assert.IsFalse(tree.GetDescendentByName("d").IsDescendentOf(tree.GetDescendentByName("f")));
            Assert.IsFalse(tree.GetDescendentByName("e").IsDescendentOf(tree.GetDescendentByName("f")));
            
            Assert.IsFalse(tree.GetDescendentByName("f").IsDescendentOf(tree.GetDescendentByName("b")));
            Assert.IsFalse(tree.GetDescendentByName("f").IsDescendentOf(tree.GetDescendentByName("c")));
            Assert.IsFalse(tree.GetDescendentByName("f").IsDescendentOf(tree.GetDescendentByName("g")));
            Assert.IsFalse(tree.GetDescendentByName("f").IsDescendentOf(tree.GetDescendentByName("h")));
            
            Assert.IsFalse(tree.GetDescendentByName("g").IsAncestorOf(tree.GetDescendentByName("j")));
            
            traversalInfo.Dispose();
        }

        void BuildElementInfo(UIElement element, UnmanagedList<ElementTraversalInfo> traversalInfo) {
            new TraversalIndexJob_Managed() {
                rootElementHandle = GCHandle.Alloc(element),
                traversalInfo = traversalInfo
            }.Run();
            
        }

        [Test]
        public unsafe void ProcessSharedStyleUpdatesJob() {

            VertigoStyleSystem styleSystem = new VertigoStyleSystem();

            NativeList<int> rebuildList = new NativeList<int>(16, Allocator.Persistent);
            NativeList<StyleStateGroup> addedList = new NativeList<StyleStateGroup>(64, Allocator.Persistent);
            NativeList<StyleStateGroup> removedList = new NativeList<StyleStateGroup>(64, Allocator.Persistent);

            CreateStandardStyleSheet(styleSystem);

            MockElement child0 = null;
            MockElement child1 = null;

            MockElement.CreateTree(styleSystem, (e) => {
                child0 = e.AddChild("1").SetSharedStyles("test-sheet/simple-style");
                child1 = e.AddChild("2").SetSharedStyles("test-sheet/style2", "test-sheet/simple-style");
            });

            RunJob();

            Assert.AreEqual(rebuildList.Length, 2);
            Assert.AreEqual(rebuildList[0], child0.id);
            Assert.AreEqual(rebuildList[1], child1.id);

            EndFrame();

            child0.SetSharedStyles("test-sheet/simple-style");

            RunJob();

            Assert.AreEqual(0, rebuildList.Length);
            Assert.AreEqual(0, addedList.Length);
            Assert.AreEqual(0, removedList.Length);

            EndFrame();

            rebuildList.Dispose();
            addedList.Dispose();
            removedList.Dispose();
            styleSystem.Destroy();

            void RunJob() {
                new ComputeStyleStateDiff() {
                    addedList = addedList,
                    removedList = removedList,
                    rebuildList = rebuildList,
                    changeSets = styleSystem.sharedStyleChangeSets.GetSpan(0, styleSystem.sharedStyleChangeSets.size)
                }.Run();
                new AssignUpdatedStyleGroupsJob() {
                    //dataMap = styleSystem.styleDataMap,
                    changeSets = styleSystem.sharedStyleChangeSets.GetSpan(0, styleSystem.sharedStyleChangeSets.size)
                }.Run();
            }

            void EndFrame() {
                rebuildList.Clear();
                addedList.Clear();
                removedList.Clear();
                styleSystem.EndFrame();
            }

        }

        [Test]
        public unsafe void StyleSystemWorks() {

            VertigoStyleSystem styleSystem = new VertigoStyleSystem();

            CreateStandardStyleSheet(styleSystem);

            MockElement rootElement = MockElement.CreateTree(styleSystem, (e) => {
                e.AddChild("1").SetSharedStyles("test-sheet/simple-style");
                e.AddChild("2").SetSharedStyles("test-sheet/style2", "test-sheet/simple-style");
                e.AddChild("3");
                e.AddChild("4");
            });

            styleSystem.TryResolveStyle("test-sheet", "simple-style", out StyleId simpleStyleId);
            styleSystem.TryResolveStyle("test-sheet", "style2", out StyleId style2Id);

            Assert.AreEqual(2, styleSystem.sharedStyleChangeSets.size);
            Assert.AreEqual(rootElement[0].styleSet2.styleDataId, styleSystem.sharedStyleChangeSets.array[0].styleDataId);
            Assert.AreEqual(rootElement[1].styleSet2.styleDataId, styleSystem.sharedStyleChangeSets.array[1].styleDataId);
            // Assert.AreEqual(3, styleSystem.changeSetStyleBuffer.size);
            //
            // Assert.AreEqual(simpleStyleId, styleSystem.changeSetStyleBuffer[0]);
            // Assert.AreEqual(style2Id, styleSystem.changeSetStyleBuffer[1]);
            // Assert.AreEqual(simpleStyleId, styleSystem.changeSetStyleBuffer[2]);

            Assert.AreEqual(styleSystem.sharedStyleChangeSets.array[0].styleDataId, rootElement[0].styleSet2.styleDataId);
            Assert.AreEqual(styleSystem.sharedStyleChangeSets.array[1].styleDataId, rootElement[1].styleSet2.styleDataId);

            Assert.AreEqual(styleSystem.sharedStyleChangeSets.array[0].styleDataId, rootElement[0].styleSet2.element.styleSet2.styleDataId);
            Assert.AreEqual(styleSystem.sharedStyleChangeSets.array[1].styleDataId, rootElement[1].styleSet2.element.styleSet2.styleDataId);

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
                public UnmanagedList<ElementInfo2> elementInfo;

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

            public MockElement SetSharedStyles(params string[] styleNames) {

                unsafe {

                    StackLongBuffer8 buffer = default;

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

                return this;

            }

            public MockElement SetInstanceStyle(StyleProperty2 property2, StyleState2 state = StyleState2.Normal) {
                return this;
            }

            public MockElement GetDescendentByName(string s) {
                if (name == s) return this;
                if (children == null) return null;
                for (int i = 0; i < children.size; i++) {
                    MockElement c = (MockElement)children[i];
                    MockElement x = c.GetDescendentByName(s);
                    if (x != null) {
                        return x;
                    }
                }

                return null;
            }

        }

    }

}