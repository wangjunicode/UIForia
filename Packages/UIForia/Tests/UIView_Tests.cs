using System;
using NUnit.Framework;
using UIForia;
using Tests.Mocks;
using static Tests.TestUtils;

[TemplateTagName("TranscludedThing")]
[Template(TemplateType.String, @"

        <UITemplate>
            <Contents>
                Text Before Children
                <Children/>
                Text After Children
            </Contents>
        </UITemplate>

    ")]
public class TranscludedThing : UIElement {

    public Action onDisableCallback;
    public Action onEnableCallback;

    public Action onCreate;

    public override void OnCreate() {
        onCreate?.Invoke();
    }

    public override void OnDisable() {
        onDisableCallback?.Invoke();
    }

    public override void OnEnable() {
        onEnableCallback?.Invoke();
    }

}

[TemplateTagName("ViewTestThing")]
[Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                <Group>
                    <TranscludedThing>
                        Text Inside Children
                    </TranscludedThing>
                </Group>
            </Contents>
        </UITemplate>
    ")]
public class ViewTestThing : UIElement {

    public bool didCreate;
    public int updateCount;

    public override void OnCreate() {
        didCreate = true;
    }

    public override void OnUpdate() {
        updateCount++;
    }

}


[TestFixture]
public class UIView_Tests {

    const string template = @"
        <UITemplate>
            <Contents>
                <Panel x-id='L0-0'>
                    <Panel x-id='L1-0'>
                        <Panel x-id='L2-0'/>
                        <Panel x-id='L2-1'>
                            <Panel x-id='L3-0'/>
                            <Panel x-id='L3-1'/>
                            <Panel x-id='L3-2'/>
                        </Panel>
                        <Panel x-id='L2-2'/>
                    </Panel>
                    <Panel x-id='L1-1'>
                        <Panel x-id='L2-3'/>
                        <Panel x-id='L2-4'>
                            <Panel x-id='L3-3'/>
                            <Panel x-id='L3-4'/>
                        </Panel>
                        <Panel x-id='L2-5'/>
                    </Panel>
                    <Panel x-id='L1-2'>
                        <Panel x-id='L2-6'/>
                        <Panel x-id='L2-7'>
                            <Panel x-id='L3-5'/>
                            <Panel x-id='L3-6'/>
                            <Panel x-id='L3-7'/>
                        </Panel>
                        <Panel x-id='L2-8'/>
                    </Panel>
                    <Panel x-id='L1-3'>
                        <Panel x-id='L2-9'/>
                        <Panel x-id='L2-10'>
                            <Panel x-id='L3-8'/>
                            <Panel x-id='L3-9'/>
                            <Panel x-id='L3-10'/>
                        </Panel>
                        <Panel x-id='L2-11'/>
                    </Panel>
                    <Panel x-id='L1-4'>
                        <Panel x-id='L2-12'>
                            <Panel x-id='L3-11'/>
                            <Panel x-id='L3-12'/>
                        </Panel>
                        <Panel x-id='L2-13'/>
                        <Panel x-id='L2-14'/>
                    </Panel>
                </Panel>
            </Contents>
        </UITemplate>
    ";


    const TestEnableFlags enabled = TestEnableFlags.Enabled;
    const TestEnableFlags disabledAncestor = TestEnableFlags.DisabledAncestor;
    const TestEnableFlags disabledSelf = TestEnableFlags.DisabledSelf;
//
//    public class TestView : UIView {
//
//        public TestView(Type elementType) : base(elementType) { }
//
//        public UIElement TestCreate() {
//            UIElement element = TemplateParser.GetParsedTemplate(elementType, true).Create(this);
//            element.flags |= UIElementFlags.AncestorEnabled;
//            return element;
//        }
//
//        
//
//    }


    [Test]
    public void InitializesHierarchyInTreeOrder() {
        MockApplication app = new MockApplication(typeof(ViewTestThing));

        UIElement element = app.RootElement;

        Assert.IsInstanceOf<ViewTestThing>(element);

        AssertHierarchy(element, new TypeAssert(typeof(ViewTestThing), 0, new[] {
            new TypeAssert(typeof(UIGroupElement), 0, new[] {
                new TypeAssert(typeof(TranscludedThing), 0, new[] {
                    new TypeAssert(typeof(UITextElement), 0),
                    new TypeAssert(typeof(UIChildrenElement), 0, new[] {
                        new TypeAssert(typeof(UITextElement), 0)
                    }),
                    new TypeAssert(typeof(UITextElement), 0)
                }),
            })
        }));
    }

    [Test]
    public void SetsFlagsForHierarchyChildren() {
        MockApplication application = new MockApplication(typeof(ViewTestThing));

        UIElement element = application.RootElement;
        element.children[0].children[0].flags &= ~(UIElementFlags.Enabled);

        Assert.IsInstanceOf<TranscludedThing>(element.children[0].children[0]);

        Assert.IsInstanceOf<ViewTestThing>(element);
        element.flags |= UIElementFlags.AncestorEnabled;

        AssertHierarchyFlags(element, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), disabledSelf, new[] {
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UIChildrenElement), disabledAncestor, new[] {
                        new TypeAssert(typeof(UITextElement), disabledAncestor)
                    }),
                    new TypeAssert(typeof(UITextElement), disabledAncestor)
                }),
            })
        }));
    }

    [Test]
    public void DisableElement() {
        MockApplication testView = new MockApplication(typeof(ViewTestThing));
        UIElement element = testView.RootElement;
        TranscludedThing thing = As<TranscludedThing>(element.children[0].children[0]);

        Application.DisableElement(thing);

        AssertHierarchyFlags(element, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), disabledSelf, new[] {
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UIChildrenElement), disabledAncestor, new[] {
                        new TypeAssert(typeof(UITextElement), disabledAncestor)
                    }),
                    new TypeAssert(typeof(UITextElement), disabledAncestor)
                }),
            })
        }));

        MockApplication testView2 = new MockApplication(typeof(ViewTestThing));

        element = testView2.RootElement;

        UITextElement text0 = As<UITextElement>(element.children[0].children[0].children[0]);
        UIChildrenElement children = As<UIChildrenElement>(element.children[0].children[0].children[1]);
        UITextElement text2 = As<UITextElement>(element.children[0].children[0].children[2]);
        UIGroupElement group = As<UIGroupElement>(element.children[0]);

        thing = As<TranscludedThing>(element.children[0].children[0]);

        Application.DisableElement(children);
        
        AssertHierarchyFlags(element, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), enabled, new[] {
                    new TypeAssert(typeof(UITextElement), enabled),
                    new TypeAssert(typeof(UIChildrenElement), disabledSelf, new[] {
                        new TypeAssert(typeof(UITextElement), disabledAncestor)
                    }),
                    new TypeAssert(typeof(UITextElement), enabled)
                }),
            })
        }));

        Application.DisableElement(text0);
        AssertHierarchyFlags(element, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), enabled, new[] {
                    new TypeAssert(typeof(UITextElement), disabledSelf),
                    new TypeAssert(typeof(UIChildrenElement), disabledSelf, new[] {
                        new TypeAssert(typeof(UITextElement), disabledAncestor)
                    }),
                    new TypeAssert(typeof(UITextElement), enabled)
                }),
            })
        }));

        Application.DisableElement(group);
        AssertHierarchyFlags(element, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), disabledSelf, new[] {
                new TypeAssert(typeof(TranscludedThing), disabledAncestor, new[] {
                    new TypeAssert(typeof(UITextElement), disabledSelf),
                    new TypeAssert(typeof(UIChildrenElement), disabledSelf, new[] {
                        new TypeAssert(typeof(UITextElement), disabledAncestor)
                    }),
                    new TypeAssert(typeof(UITextElement), disabledAncestor)
                }),
            })
        }));
    }

    [Test]
    public void Callback_OnDisable() {
        MockApplication application = new MockApplication(typeof(ViewTestThing));
        UIElement element = application.RootElement;
        TranscludedThing thing = As<TranscludedThing>(element.children[0].children[0]);

        int callCount = 0;

        thing.onDisableCallback = () => { callCount++; };
        Application.DisableElement(thing);
        Assert.AreEqual(1, callCount);

        MockApplication testView2 = new MockApplication(typeof(ViewTestThing));
        element = testView2.RootElement;
        thing = As<TranscludedThing>(element.children[0].children[0]);
        thing.onDisableCallback = () => { callCount++; };
        Application.DisableElement(element.children[0]);
        Assert.AreEqual(2, callCount);
        Application.DisableElement(element.children[0]);
        Assert.AreEqual(2, callCount);
    }


    [Test]
    public void EnableElement() {
        MockApplication testView = new MockApplication(typeof(ViewTestThing));
        UIElement element = testView.RootElement;
        TranscludedThing thing = As<TranscludedThing>(element.children[0].children[0]);

        Application.DisableElement(thing);

        AssertHierarchyFlags(element, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), disabledSelf, new[] {
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UIChildrenElement), disabledAncestor, new[] {
                        new TypeAssert(typeof(UITextElement), disabledAncestor)
                    }),
                    new TypeAssert(typeof(UITextElement), disabledAncestor)
                })
            })
        }));

        Application.EnableElement(thing);
        AssertHierarchyFlags(element, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), enabled, new[] {
                    new TypeAssert(typeof(UITextElement), enabled),
                    new TypeAssert(typeof(UIChildrenElement), enabled, new[] {
                        new TypeAssert(typeof(UITextElement), enabled)
                    }),
                    new TypeAssert(typeof(UITextElement), enabled)
                }),
            })
        }));
        UIGroupElement group = As<UIGroupElement>(element.children[0]);
        UITextElement text2 = As<UITextElement>(element.children[0].children[0].children[2]);

        Application.DisableElement(group);
        AssertHierarchyFlags(element, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), disabledSelf, new[] {
                new TypeAssert(typeof(TranscludedThing), disabledAncestor, new[] {
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UIChildrenElement), disabledAncestor, new[] {
                        new TypeAssert(typeof(UITextElement), disabledAncestor)
                    }),
                    new TypeAssert(typeof(UITextElement), disabledAncestor)
                }),
            })
        }));
        Application.DisableElement(text2);
        AssertHierarchyFlags(element, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), disabledSelf, new[] {
                new TypeAssert(typeof(TranscludedThing), disabledAncestor, new[] {
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UIChildrenElement), disabledAncestor, new[] {
                        new TypeAssert(typeof(UITextElement), disabledAncestor)
                    }),
                    new TypeAssert(typeof(UITextElement), disabledSelf)
                }),
            })
        }));
        Application.EnableElement(group);
        AssertHierarchyFlags(element, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), enabled, new[] {
                    new TypeAssert(typeof(UITextElement), enabled),
                    new TypeAssert(typeof(UIChildrenElement), enabled, new[] {
                        new TypeAssert(typeof(UITextElement), enabled)
                    }),
                    new TypeAssert(typeof(UITextElement), disabledSelf)
                }),
            })
        }));
    }

    [Test]
    public void Callback_OnEnable() {
        MockApplication testView = new MockApplication(typeof(ViewTestThing));
        UIElement element = testView.RootElement;
        TranscludedThing thing = As<TranscludedThing>(element.children[0].children[0]);
        UIGroupElement group = As<UIGroupElement>(element.children[0]);
        int callCount = 0;
        thing.onEnableCallback = () => callCount++;
        Application.DisableElement(thing);
        Application.EnableElement(thing);
        Assert.AreEqual(1, callCount);
        Application.DisableElement(group);
        Assert.IsTrue(thing.isDisabled);
        Application.EnableElement(thing);
        Assert.AreEqual(1, callCount);
        Application.EnableElement(group);
        Assert.AreEqual(2, callCount);
    }


    [Test]
    public void LifeCycle_OnCreate() {
        MockApplication testView = new MockApplication(typeof(ViewTestThing));
        ViewTestThing thing = (ViewTestThing) testView.RootElement;
        Assert.IsTrue(thing.didCreate);
    }

    [Test]
    public void LifeCycle_OnUpdate() {
        MockApplication testView = new MockApplication(typeof(ViewTestThing));
        ViewTestThing thing = (ViewTestThing) testView.RootElement;
        Assert.AreEqual(0, thing.updateCount);
        testView.Update();
        Assert.AreEqual(1, thing.updateCount);
    }

    [Test]
    public void SetDepthIndex() {
        MockApplication testView = new MockApplication(typeof(ViewTestThing), template);
        ViewTestThing root = (ViewTestThing) testView.RootElement;
        for (int i = 0; i < 5; i++) {
            Assert.AreEqual(root.FindById("L1-" + i).depthIndex, i);
        }

        for (int i = 0; i < 15; i++) {
            Assert.AreEqual(root.FindById("L2-" + i).depthIndex, i);
        }

        for (int i = 0; i < 13; i++) {
            Assert.AreEqual(root.FindById("L3-" + i).depthIndex, i);
        }
    }

    [Test]
    public void UpdateDepthIndexOnDestroy() {
        MockApplication testView = new MockApplication(typeof(ViewTestThing), template);
        
        ViewTestThing root = (ViewTestThing) testView.RootElement;
        Application.DestroyElement(root.FindById("L1-3"));

        Assert.AreEqual(0, root.FindById("L1-0").depthIndex);
        Assert.AreEqual(1, root.FindById("L1-1").depthIndex);
        Assert.AreEqual(2, root.FindById("L1-2").depthIndex);
        Assert.AreEqual(3, root.FindById("L1-4").depthIndex);

//        for (int i = 0; i < 5; i++) {
//            Assert.AreEqual(root.FindById("L1-" + i).depthIndex, i);
//        }
//        
//        for (int i = 0; i < 15; i++) {
//            Assert.AreEqual(root.FindById("L2-" + i).depthIndex, i);
//        }
//        
//        for (int i = 0; i < 13; i++) {
//            Assert.AreEqual(root.FindById("L3-" + i).depthIndex, i);
//        }
    }

    [Test]
    public void RemoveFromChildrenOnDestroy() {
        MockApplication testView = new MockApplication(typeof(ViewTestThing), template);
        
        ViewTestThing root = (ViewTestThing) testView.RootElement;
        UIElement parent = root.FindById("L1-3").parent;
        int childCount = parent.children.Count;
        Application.DestroyElement(root.FindById("L1-3"));
        Assert.IsNull(root.FindById("L1-3"));
        Assert.AreEqual(childCount - 1, parent.children.Count);
    }

    private struct TypeAssert {

        public readonly Type parentType;
        public readonly TypeAssert[] childTypes;
        public readonly TestEnableFlags flags;

        public TypeAssert(Type parentType, TestEnableFlags flags = 0, TypeAssert[] childTypes = null) {
            this.parentType = parentType;
            this.childTypes = childTypes ?? new TypeAssert[0];
            this.flags = flags;
        }

    }

    public enum TestEnableFlags {

        Enabled,
        Disabled,
        EnabledSelf,
        DisabledSelf,
        DisabledAncestor,
        EnabledAncestor

    }

    private static void AssertHierarchy(UIElement element, TypeAssert assertRoot, int depth = 0) {
        Assert.AreEqual(element.GetType(), assertRoot.parentType);
        if (element.children.Count != assertRoot.childTypes.Length) {
            Assert.Fail("Child Count did not match at depth: " + depth);
        }

        for (int i = 0; i < element.children.Count; i++) {
            if (element.children[i].GetType() != assertRoot.childTypes[i].parentType) {
                Assert.Fail($"Types did not match for child number {i} at depth {depth}. {element.children[i].GetType()} is not {assertRoot.childTypes[i].parentType}");
            }

            AssertHierarchy(element.children[i], assertRoot.childTypes[i], depth + 1);
        }
    }

    private static void AssertHierarchyFlags(UIElement element, TypeAssert assertRoot, int depth = 0) {
        Assert.AreEqual(element.GetType(), assertRoot.parentType);

        switch (assertRoot.flags) {
            case TestEnableFlags.Disabled:
                Assert.IsTrue(element.isDisabled);
                Assert.IsFalse(element.isEnabled);
                break;
            case TestEnableFlags.DisabledSelf:
                Assert.IsTrue(element.isSelfDisabled);
                Assert.IsFalse(element.isSelfEnabled);
                break;
            case TestEnableFlags.DisabledAncestor:
                Assert.IsTrue(element.isDisabled);
                Assert.IsTrue(element.isSelfEnabled);
                Assert.IsFalse(element.isEnabled);
                break;
            case TestEnableFlags.Enabled:
                Assert.IsTrue(element.isEnabled);
                Assert.IsTrue(element.isSelfEnabled);
                Assert.IsFalse(element.isSelfDisabled);
                break;
            case TestEnableFlags.EnabledAncestor:
                break;
            case TestEnableFlags.EnabledSelf:
                Assert.IsTrue(element.isSelfEnabled);
                break;
        }

        if (element.children.Count != assertRoot.childTypes.Length) {
            Assert.Fail("Child Count did not match at depth: " + depth);
        }

        for (int i = 0; i < element.children.Count; i++) {
            if (element.children[i].GetType() != assertRoot.childTypes[i].parentType) {
                Assert.Fail("Types did not match for child number " + i + " at depth " + depth);
            }

            AssertHierarchy(element.children[i], assertRoot.childTypes[i], depth + 1);
        }
    }

}