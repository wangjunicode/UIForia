using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UIForia;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

// ReSharper disable HeapView.BoxingAllocation

namespace Tests {

    // todo -- move this

    public unsafe class FixedBlockAllocatorTest {

        public struct SixteenBytes {

            private fixed byte bytes[16];

        }

        [Test]
        public void Allocates() {
            int blockSize = sizeof(SixteenBytes);
            int blocksPerPage = 4;

            FixedBlockAllocator alloc = new FixedBlockAllocator(blockSize, blocksPerPage, 1);

            SixteenBytes* a = alloc.Allocate<SixteenBytes>();
            SixteenBytes* b = alloc.Allocate<SixteenBytes>();
            SixteenBytes* c = alloc.Allocate<SixteenBytes>();
            SixteenBytes* d = alloc.Allocate<SixteenBytes>();

            Assert.IsTrue(a + 1 == b);
            Assert.IsTrue(b + 1 == c);
            Assert.IsTrue(c + 1 == d);

            Assert.IsTrue(alloc.freeList == null);

            alloc.Free(b);
            alloc.Free(c);
            alloc.Free(d);

            Assert.IsTrue(alloc.freeList != null);

            SixteenBytes* b2 = alloc.Allocate<SixteenBytes>();
            SixteenBytes* c2 = alloc.Allocate<SixteenBytes>();
            SixteenBytes* d2 = alloc.Allocate<SixteenBytes>();

            Assert.IsTrue(a + 1 == d2);
            Assert.IsTrue(d2 + 1 == c2);
            Assert.IsTrue(c2 + 1 == b2);

            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();
            
            Assert.AreEqual(alloc.pageListSize, 3);
            
            alloc.Dispose();

        }

    }

    public class NewStyleSystemTests {

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

            BufferList<ElementTraversalInfo> traversalInfo = new BufferList<ElementTraversalInfo>(32, Allocator.TempJob);

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

        void BuildElementInfo(UIElement element, BufferList<ElementTraversalInfo> traversalInfo) {
            new TraversalIndexJob_Managed() {
                rootElementHandle = GCHandle.Alloc(element),
                traversalInfo = traversalInfo
            }.Run();

        }

        public static StyleSetData CreateStyleSetData(StyleState2 state, StyleId[] styles) {
            unsafe {
                fixed (StyleId* s = styles) {
                    return new StyleSetData() {
                        changeSetId = ushort.MaxValue,
                        state = (StyleState2Byte) state,
                        styleIds = new ListHandle<StyleId>(s, styles.Length, styles.Length)
                    };
                }
            }

        }

        public static StyleSetData CreateStyleSetData(StyleState2 state = StyleState2.Normal) {
            return new StyleSetData() {
                changeSetId = ushort.MaxValue,
                state = (StyleState2Byte) state
            };
        }

        public static StyleDatabase MakeStyleDatabase() {
            StyleDatabase styleDB = new StyleDatabase();
            styleDB.AddModule("mod1", (module) => {
                module.AddStyleSheet("sheet1", (sheet) => {

                    sheet.AddStyle("one", (style) => { style.Normal((normal) => normal.Set(StyleProperty2.BorderBottom(4))); });
                    sheet.AddStyle("two", (style) => {
                        style.Normal((normal) => normal.Set(StyleProperty2.BorderBottom(4)));
                        style.Hover((hover) => hover.Set(StyleProperty2.BorderTop(4)));
                    });
                    sheet.AddStyle("three", (style) => { style.Normal((normal) => normal.Set(StyleProperty2.BorderBottom(4))); });
                    sheet.AddStyle("four", (style) => { });

                    sheet.AddStyle("five", (style) => {

                        style.Set(StyleProperty2.BackgroundColor(Color.red));
                        style.Set(StyleProperty2.Opacity(0.5f));

                        style.Hover((hover) => {
                            hover.Set(StyleProperty2.BackgroundColor(Color.blue));
                            hover.Set(StyleProperty2.Opacity(1f));
                        });

                    });
                });
            });
            return styleDB;
        }

        [Test]
        public unsafe void RebuildSharedStyles() {

            using (StyleDatabase styleDB = MakeStyleDatabase())
            using (StyleRebuildResultList rebuildResult = new StyleRebuildResultList(Allocator.TempJob))
            using (ConvertedStyleList convertedList = new ConvertedStyleList(Allocator.TempJob))
            using (UnmanagedList<ConvertedStyleId> convertedStyleList = new UnmanagedList<ConvertedStyleId>(32, Allocator.TempJob))
            using (SharedStyleChangeSet data = new SharedStyleChangeSet(128, 128, Allocator.TempJob)) {

                StyleSheetInterface styleSheet = styleDB.GetStyleSheet("mod1", "sheet1");

                StyleRebuildResultList* pRebuildList = &rebuildResult;
                ConvertedStyleList* pConvertedList = &convertedList;

                PerThread<ConvertedStyleList> x = PerThread<ConvertedStyleList>.Single(pConvertedList, Allocator.TempJob);

                ConvertStyleIdsToStatePairs convertJob = new ConvertStyleIdsToStatePairs() {
                    perThreadOutput = x,
                    sharedStyleChangeSet = data
                };

                MergePerThreadData<ConvertedStyleList, ConvertedStyleId> gatherJob = new MergePerThreadData<ConvertedStyleList, ConvertedStyleId>() {
                    gatheredOutput = convertedStyleList,
                    perThread = x
                };

                BuildSharedStyles buildJob = new BuildSharedStyles() {
                    convertedStyleList = convertedStyleList,
                    table_StyleInfo = styleDB.table_StyleInfo,
                    staticStyleBuffer = styleDB.buffer_staticStyleProperties.data,
                    perThread_RebuiltResult = PerThread<StyleRebuildResultList>.Single(pRebuildList, Allocator.TempJob)
                };

                data.InitializeSharedStyles(0, StyleState2.Normal, styleSheet.GetStyle("five"), styleSheet.GetStyle("two"));
                data.InitializeSharedStyles(1, StyleState2.Hover, styleSheet.GetStyle("five"), styleSheet.GetStyle("two"));
                data.InitializeSharedStyles(2, StyleState2.Normal, styleSheet.GetStyle("five"), styleSheet.GetStyle("two"));

                convertJob.Run();
                gatherJob.Run();
                buildJob.Run();

                StyleRebuildResult r0 = rebuildResult.GetResultForId(0);
                StyleRebuildResult r1 = rebuildResult.GetResultForId(1);
                StyleRebuildResult r2 = rebuildResult.GetResultForId(2);

                Assert.AreEqual(0, r0.styleSetId);
                Assert.AreEqual(1, r1.styleSetId);
                Assert.AreEqual(2, r2.styleSetId);

                Assert.AreEqual(3, r0.propertyCount);
                Assert.AreEqual(3, r1.propertyCount);
                Assert.AreEqual(3, r2.propertyCount);

                Assert.AreEqual(r0.GetProperty(PropertyId.BackgroundColor), styleSheet.GetStyle("five").GetProperty(PropertyId.BackgroundColor));

            }

        }

        [Test]
        public unsafe void ConvertSharedStylesToStyleStatePairs() {

            using (StyleDatabase styleDB = MakeStyleDatabase())
            using (SharedStyleChangeSet data = new SharedStyleChangeSet(128, 128, Allocator.TempJob)) {

                StyleSheetInterface styleSheet = styleDB.GetStyleSheet("mod1", "sheet1");

                ConvertedStyleList convertedList = new ConvertedStyleList(Allocator.TempJob);

                ConvertStyleIdsToStatePairs job = new ConvertStyleIdsToStatePairs() {
                    perThreadOutput = PerThread<ConvertedStyleList>.Single(ref convertedList, Allocator.TempJob),
                    sharedStyleChangeSet = data
                };

                StyleSetData styleSetData = CreateStyleSetData();

                StyleId[] styleIds = {
                    styleSheet.GetStyle("one"),
                    styleSheet.GetStyle("two"),
                    styleSheet.GetStyle("three")
                };

                data.SetSharedStyles(0, ref styleSetData, styleIds[0], styleIds[1], styleIds[2]);

                Assert.AreEqual(3, data.entries[0].newStyleCount);
                Assert.AreEqual(0, data.entries[0].oldStyleCount);

                job.Run();
                Assert.AreEqual(1, convertedList.size);

                Assert.AreEqual(3, convertedList[0].newStyleCount);
                Assert.AreEqual(0, convertedList[0].oldStyleCount);

                Assert.AreEqual(new StyleStatePair(styleIds[0], StyleState2.Normal), convertedList[0].pNewStyles[0]);
                Assert.AreEqual(new StyleStatePair(styleIds[1], StyleState2.Normal), convertedList[0].pNewStyles[1]);
                Assert.AreEqual(new StyleStatePair(styleIds[2], StyleState2.Normal), convertedList[0].pNewStyles[2]);

                data.Clear();
                convertedList.Clear();

                styleSetData = CreateStyleSetData(StyleState2.Normal | StyleState2.Hover);

                data.SetSharedStyles(0, ref styleSetData, styleIds[0], styleIds[1], styleIds[2]);

                job.Run();

                Assert.AreEqual(1, convertedList.size);
                Assert.AreEqual(4, convertedList[0].newStyleCount);
                Assert.AreEqual(0, convertedList[0].oldStyleCount);

                Assert.AreEqual(new StyleStatePair(styleIds[1], StyleState2.Hover), convertedList[0].pNewStyles[0]);
                Assert.AreEqual(new StyleStatePair(styleIds[0], StyleState2.Normal), convertedList[0].pNewStyles[1]);
                Assert.AreEqual(new StyleStatePair(styleIds[1], StyleState2.Normal), convertedList[0].pNewStyles[2]);
                Assert.AreEqual(new StyleStatePair(styleIds[2], StyleState2.Normal), convertedList[0].pNewStyles[3]);

                data.Clear();
                convertedList.Clear();

                styleSetData = CreateStyleSetData(StyleState2.Normal | StyleState2.Hover, new[] {styleIds[0], styleIds[1], styleIds[2]});

                data.SetSharedStyles(0, ref styleSetData, styleIds[0], styleIds[1], styleIds[2]);

                job.Run();

                Assert.AreEqual(0, convertedList.size); // old and new styles expanded to the same data so no change was created

                convertedList.Dispose();

            }

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
                public BufferList<ElementInfo2> elementInfo;

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

                    StackIntBuffer7 buffer = new StackIntBuffer7();

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

}