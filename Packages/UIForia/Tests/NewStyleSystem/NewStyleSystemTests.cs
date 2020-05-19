using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UIForia;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Src;
using UIForia.Style;
using UIForia.Systems;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

// ReSharper disable HeapView.BoxingAllocation

namespace Tests {

    [RecordFilePath]
    public class StyleSystemTestModule : Module { }

    public class UIForiaSystems : IDisposable {

        public StyleSystem2 styleSystem;
        public ElementSystem elementSystem;
        public TemplateSystem templateSystem;
        public AttributeSystem attributeSystem;
        public SelectorSystem selectorSystem;
        public StringInternSystem internSystem;
        public StyleDatabase styleDatabase;

        public UIForiaSystems(int initialElementCount = 32) {
            this.styleDatabase = new StyleDatabase(ModuleSystem.GetModule<StyleSystemTestModule>());
            this.elementSystem = new ElementSystem(initialElementCount);
            this.styleSystem = new StyleSystem2(initialElementCount, styleDatabase);
            this.templateSystem = new TemplateSystem(null);
            this.selectorSystem = new SelectorSystem();
            this.attributeSystem = new AttributeSystem(internSystem, elementSystem);
        }

        public void Dispose() {
            styleSystem?.Dispose();
            elementSystem?.Dispose();
            attributeSystem?.Dispose();
            selectorSystem?.Dispose();
        }

    }

    public class NewStyleSystemTests {

        [Test]
        public void BuildTraversalTree() {
            using (UIForiaSystems systems = new UIForiaSystems()) {
                MockElement tree = MockElement.CreateTree(systems, root => {
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
        }

        void BuildElementInfo(UIElement element, BufferList<ElementTraversalInfo> traversalInfo) { }

        [Test]
        public void MakeDummyElements() {
            
            using (UIForiaSystems systems = new UIForiaSystems()) {

                MockElement tree = MockElement.CreateTree(systems, root => {
                    root.name = "a";
                    root.AddChild("b", (b) => { b.AddChild("c"); })
                        .SetSharedStyles();
                    root.AddChild("d", (d) => {
                        d.AddChild("e", (e) => { e.AddChild("f"); });
                        d.AddChild("g", (g) => { g.AddChild("h"); });
                        d.AddChild("i", (i) => { i.AddChild("j"); });
                    });
                });

                // what do I want to test?
                // can I reduce my problem scope?
                // can I get to a point where I can run a real scene again? !!!!!!!
                //     start from loading a root module and end at something rendered on screen
                //     i have 6 weeks to do all this
                //     where does testing come in? where can I re-use existing tests?
                //    lets get the minimum up and running and then add back features on top of that
                //        -- parse & compile templates + style
                //        -- Run an application
                //        -- flex + text layout (nothing complicated)
                //        -- render (no clipping)
                //        -- focus on making editor work too (finally)
                // what needs testing exactly?
                // i have a lot in flight right now that needs verification in a real world way
                //    template compiler
                //    template parser
                //    style parser
                //    style compiler
                //    module system
                //    style system
                
                // how can i track memory usage and find leaks?
                // how can i get an overview of the memory used by my allocators
                // how many allocators do I have?
                // what is each one used for?
                
                new UpdateTraversalTable() {
                    rootId = tree.id,
                    hierarchyTable = systems.elementSystem.hierarchyTable,
                    traversalTable = systems.elementSystem.traversalTable,
                    metaTable = systems.elementSystem.metaTable,
                }.Run();

            }
        }

        public static StyleSetData CreateStyleSetData(StyleState2 state, StyleId[] styles) {
            unsafe {
                fixed (StyleId* s = styles) {
                    return new StyleSetData() {
                        styleChangeIndex = ushort.MaxValue,
                        state = state,
                        // styleIds = new ListHandle<StyleId>(s, styles.Length, styles.Length)
                    };
                }
            }

        }

        public static StyleSetData CreateStyleSetData(StyleState2 state = StyleState2.Normal) {
            return new StyleSetData() {
                styleChangeIndex = ushort.MaxValue,
                state = state
            };
        }

        public static StyleDatabase MakeStyleDatabase(Module rootModule = null) {

            StyleDatabase styleDB = new StyleDatabase(rootModule ?? ModuleSystem.GetModule<StyleSystemTestModule>());

            styleDB.AddStyleSheet(typeof(StyleSystemTestModule), "sheet1", (sheet) => {

                sheet.AddStyle("one", (style) => { style.Set(StyleProperty2.BorderBottom(4)); });

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

                sheet.AddStyle("selector-indexed", (style) => {

                    style.Selector("x", (selector) => {
                        // selector.QueryFrom()
                    });

                    style.Selector("select-1", (selector) => {

                        selector.QueryFrom(SelectionSource.Children)
                            .WithAttr("some-attr")
                            .WithTag("Div")
                            .WithStyle("two");

                        selector.Set(StyleProperty2.BackgroundColor(Color.red));

                    });
                });
            });

            styleDB.Initialize();
            return styleDB;
        }

        [Test]
        public unsafe void BuildSelectorQueries() {

            Module rootModule = ModuleSystem.GetModule<StyleSystemTestModule>();

            using (StyleDatabase db = MakeStyleDatabase(rootModule)) {
                Assert.AreEqual(1, db.selectorDatabase.table_SelectorQueries.size);
                SelectorQuery query = db.selectorDatabase.table_SelectorQueries[0];
                Assert.AreEqual(3, query.filterCount);
                Assert.AreEqual("some-attr", db.staticStringTable.Get(query.filters[0].key).ToString());
                Assert.AreEqual(TypeProcessor.GetProcessedType(typeof(UIDivElement)).id, query.filters[1].key);
                Assert.AreEqual(db.GetStyleSheet<StyleSystemTestModule>("sheet1").GetStyle("two").id, query.filters[2].key);
                Assert.AreEqual(StyleProperty2.BackgroundColor(Color.red), db.GetSelectorPropertyValue(0, PropertyId.BackgroundColor));
            }

        }

        [Test]
        public void DiffSharedStyleChanges() {

            AssertSize.AssertSizes();

            using (StyleDatabase db = MakeStyleDatabase())
            using (VertigoStyleSystem.TransientData transient = VertigoStyleSystem.TransientData.Create())
            using (ConvertedStyleList sharedStyleChangeList = new ConvertedStyleList(Allocator.TempJob)) {

                StyleSheetInterface styleSheet = db.GetStyleSheet<StyleSystemTestModule>("sheet1");

                DiffSharedStyleChanges diffJob = new DiffSharedStyleChanges() {
                    input_SharedStyleChangeList = sharedStyleChangeList,
                    output_AddedStyleStateList = transient.addedStyleStateList,
                    output_RemovedStyleStateList = transient.removedStyleStateList
                };

                ConvertedStyleList_Add(sharedStyleChangeList, 1, new[] {
                        new StyleStatePair(styleSheet.GetStyle("two"), StyleState2.Normal),
                        new StyleStatePair(styleSheet.GetStyle("four"), StyleState2.Normal),
                    },
                    new[] {
                        new StyleStatePair(styleSheet.GetStyle("three"), StyleState2.Hover),
                        new StyleStatePair(styleSheet.GetStyle("two"), StyleState2.Normal),
                        new StyleStatePair(styleSheet.GetStyle("three"), StyleState2.Normal),
                        new StyleStatePair(styleSheet.GetStyle("one"), StyleState2.Normal),
                    }
                );

                ConvertedStyleList_Add(sharedStyleChangeList, 2, new[] {
                        new StyleStatePair(styleSheet.GetStyle("two"), StyleState2.Hover),
                        new StyleStatePair(styleSheet.GetStyle("two"), StyleState2.Normal),
                    },
                    new[] {
                        new StyleStatePair(styleSheet.GetStyle("two"), StyleState2.Normal),
                    }
                );

                ConvertedStyleList_Add(sharedStyleChangeList, 3, new[] {
                        new StyleStatePair(styleSheet.GetStyle("two"), StyleState2.Normal),
                    },
                    new[] {
                        new StyleStatePair(styleSheet.GetStyle("two"), StyleState2.Normal),
                    }
                );

                ConvertedStyleList_Add(sharedStyleChangeList, 4, null,
                    new[] {
                        new StyleStatePair(styleSheet.GetStyle("five"), StyleState2.Normal),
                    }
                );

                diffJob.Run();

                Assert.AreEqual(4, transient.addedStyleStateList.size);
                Assert.AreEqual(2, transient.removedStyleStateList.size);

                Assert.AreEqual(new StyleStateElementId(styleSheet.GetStyle("three"), StyleState2.Hover, 1), transient.addedStyleStateList[0]);
                Assert.AreEqual(new StyleStateElementId(styleSheet.GetStyle("three"), StyleState2.Normal, 1), transient.addedStyleStateList[1]);
                Assert.AreEqual(new StyleStateElementId(styleSheet.GetStyle("one"), StyleState2.Normal, 1), transient.addedStyleStateList[2]);
                Assert.AreEqual(new StyleStateElementId(styleSheet.GetStyle("five"), StyleState2.Normal, 4), transient.addedStyleStateList[3]);

                Assert.AreEqual(new StyleStateElementId(styleSheet.GetStyle("four"), StyleState2.Normal, 1), transient.removedStyleStateList[0]);
                Assert.AreEqual(new StyleStateElementId(styleSheet.GetStyle("two"), StyleState2.Hover, 2), transient.removedStyleStateList[1]);

            }

            unsafe void ConvertedStyleList_Add(ConvertedStyleList sharedStyleChangeList, int elementId, StyleStatePair[] oldStyles, StyleStatePair[] newStyles) {
                int oldLength = oldStyles?.Length ?? 0;
                int newLength = newStyles?.Length ?? 0;

                StyleStatePair[] buffer = new StyleStatePair[oldLength + newLength];
                for (int i = 0; i < oldLength; i++) {
                    buffer[i] = oldStyles[i];
                }

                for (int i = 0; i < newLength; i++) {
                    buffer[oldLength + i] = newStyles[i];
                }

                fixed (StyleStatePair* ptr = buffer) {
                    sharedStyleChangeList.Add(elementId, ptr, oldLength, newLength);
                }
            }

        }

        [Test]
        public unsafe void RebuildSharedStyles() {

            AssertSize.AssertSizes();

            using (StyleDatabase styleDB = MakeStyleDatabase())
            using (StyleRebuildResultList rebuildResult = new StyleRebuildResultList(Allocator.TempJob))
            using (ConvertedStyleList convertedList = new ConvertedStyleList(Allocator.TempJob))
            using (DataList<ConvertedStyleId> convertedStyleList = new DataList<ConvertedStyleId>(32, Allocator.TempJob))
            using (DisposedDataList<FixedBlockAllocator>.Shared allocators = VertigoStyleSystem.CreateStyleAllocators(true))
            using (SharedStyleChangeSet data = new SharedStyleChangeSet(128, 128, Allocator.TempJob)) {
                //
                // StyleResultTable sharedTable = new StyleResultTable(Allocator.TempJob) {
                //     allocatorList = allocators
                // };
                //
                // sharedTable.EnsureElementCount(16);
                //
                // StyleSheetInterface styleSheet = styleDB.GetStyleSheet<StyleSystemTestModule>("sheet1");
                //
                // StyleRebuildResultList* pRebuildList = &rebuildResult;
                // ConvertedStyleList* pConvertedList = &convertedList;
                //
                // PerThread<ConvertedStyleList> x = PerThread<ConvertedStyleList>.Single(pConvertedList, Allocator.TempJob);
                //
                // ConvertStyleIdsToStatePairs convertJob = new ConvertStyleIdsToStatePairs() {
                //     perThreadOutput = x,
                //     sharedStyleChangeSet = data
                // };
                //
                // MergePerThreadData<ConvertedStyleList, ConvertedStyleId> gatherJob = new MergePerThreadData<ConvertedStyleList, ConvertedStyleId>() {
                //     gatheredOutput = convertedStyleList,
                //     perThread = x
                // };
                //
                // BuildSharedStyles buildJob = new BuildSharedStyles() {
                //     convertedStyleList = convertedStyleList,
                //     table_StyleInfo = styleDB.table_StyleInfo,
                //     staticStyleBuffer = styleDB.buffer_staticStyleProperties.data,
                //     perThread_RebuiltResult = PerThread<StyleRebuildResultList>.Single(pRebuildList, Allocator.TempJob)
                // };
                //
                // WriteToStyleResultTable writeJob = new WriteToStyleResultTable() {
                //     targetTable = sharedTable,
                //     writeList = pRebuildList
                // };
                //
                // data.InitializeSharedStyles(0, StyleState2.Normal, styleSheet.GetStyle("five"), styleSheet.GetStyle("two"));
                // data.InitializeSharedStyles(1, StyleState2.Hover, styleSheet.GetStyle("five"), styleSheet.GetStyle("two"));
                // data.InitializeSharedStyles(2, StyleState2.Normal, styleSheet.GetStyle("five"), styleSheet.GetStyle("two"));
                //
                // convertJob.Run();
                // gatherJob.Run();
                // buildJob.Run();
                // writeJob.Run();
                //
                // StyleRebuildResult r0 = rebuildResult.GetResultForId(0);
                // StyleRebuildResult r1 = rebuildResult.GetResultForId(1);
                // StyleRebuildResult r2 = rebuildResult.GetResultForId(2);
                //
                // Assert.AreEqual(0, r0.elementId);
                // Assert.AreEqual(1, r1.elementId);
                // Assert.AreEqual(2, r2.elementId);
                //
                // Assert.AreEqual(3, r0.propertyCount);
                // Assert.AreEqual(4, r1.propertyCount); // also has border top
                // Assert.AreEqual(3, r2.propertyCount);
                //
                // Assert.AreEqual(sharedTable[0].GetProperty(PropertyId.BackgroundColor), StyleProperty2.BackgroundColor(Color.red));
                // Assert.AreEqual(sharedTable[1].GetProperty(PropertyId.BackgroundColor), StyleProperty2.BackgroundColor(Color.blue)); // from hover
                // Assert.AreEqual(sharedTable[2].GetProperty(PropertyId.BackgroundColor), StyleProperty2.BackgroundColor(Color.red));
                //
                // Assert.AreEqual(sharedTable[0].GetProperty(PropertyId.BorderBottom), StyleProperty2.BorderBottom(4));
                // Assert.AreEqual(sharedTable[1].GetProperty(PropertyId.BorderBottom), StyleProperty2.BorderBottom(4));
                // Assert.AreEqual(sharedTable[2].GetProperty(PropertyId.BorderBottom), StyleProperty2.BorderBottom(4));
                //
                // Assert.AreEqual(sharedTable[0].GetProperty(PropertyId.Opacity), StyleProperty2.Opacity(0.5f));
                // Assert.AreEqual(sharedTable[1].GetProperty(PropertyId.Opacity), StyleProperty2.Opacity(1f)); // from hover
                // Assert.AreEqual(sharedTable[2].GetProperty(PropertyId.Opacity), StyleProperty2.Opacity(0.5f));
                //
                // Assert.IsFalse(sharedTable[0].HasProperty(PropertyId.BorderTop));
                // Assert.IsTrue(sharedTable[1].HasProperty(PropertyId.BorderTop));
                // Assert.IsFalse(sharedTable[2].HasProperty(PropertyId.BorderTop));
                //
                // Assert.AreEqual(sharedTable[1].GetProperty(PropertyId.BorderTop), StyleProperty2.BorderTop(4));
                //
                // sharedTable.Dispose();
            }

        }

        [Test]
        public unsafe void ConvertSharedStylesToStyleStatePairs() {

            using (StyleDatabase styleDB = MakeStyleDatabase())
            using (SharedStyleChangeSet data = new SharedStyleChangeSet(128, 128, Allocator.TempJob)) {

                StyleSheetInterface styleSheet = styleDB.GetStyleSheet<StyleSystemTestModule>("sheet1");

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

                Assert.AreEqual(new StyleStatePair(styleIds[0], StyleState2.Normal), convertedList[0].newStyles[0]);
                Assert.AreEqual(new StyleStatePair(styleIds[1], StyleState2.Normal), convertedList[0].newStyles[1]);
                Assert.AreEqual(new StyleStatePair(styleIds[2], StyleState2.Normal), convertedList[0].newStyles[2]);

                data.Clear();
                convertedList.Clear();

                styleSetData = CreateStyleSetData(StyleState2.Normal | StyleState2.Hover);

                data.SetSharedStyles(0, ref styleSetData, styleIds[0], styleIds[1], styleIds[2]);

                job.Run();

                Assert.AreEqual(1, convertedList.size);
                Assert.AreEqual(4, convertedList[0].newStyleCount);
                Assert.AreEqual(0, convertedList[0].oldStyleCount);

                Assert.AreEqual(new StyleStatePair(styleIds[1], StyleState2.Hover), convertedList[0].newStyles[0]);
                Assert.AreEqual(new StyleStatePair(styleIds[0], StyleState2.Normal), convertedList[0].newStyles[1]);
                Assert.AreEqual(new StyleStatePair(styleIds[1], StyleState2.Normal), convertedList[0].newStyles[2]);
                Assert.AreEqual(new StyleStatePair(styleIds[2], StyleState2.Normal), convertedList[0].newStyles[3]);

                data.Clear();
                convertedList.Clear();

                styleSetData = CreateStyleSetData(StyleState2.Normal | StyleState2.Hover, new[] {styleIds[0], styleIds[1], styleIds[2]});

                data.SetSharedStyles(0, ref styleSetData, styleIds[0], styleIds[1], styleIds[2]);

                job.Run();

                Assert.AreEqual(0, convertedList.size); // old and new styles expanded to the same data so no change was created

                convertedList.Dispose();

            }

        }

    }

}