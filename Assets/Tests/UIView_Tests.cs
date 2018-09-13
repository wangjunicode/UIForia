using System;
using NUnit.Framework;
using Src;

using static Tests.TestUtils;

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

           
    const TestEnableFlags enabled = TestEnableFlags.Enabled;
    const TestEnableFlags disabledAncestor = TestEnableFlags.DisabledAncestor;
    const TestEnableFlags disabledSelf = TestEnableFlags.DisabledSelf;
    
    public class TestView : UIView {

        public TestView(Type elementType) : base(elementType) { }

        public MetaData TestCreate() {
            MetaData data = TemplateParser.GetParsedTemplate(elementType, true).CreateWithoutScope(this);
            data.element.flags |= UIElementFlags.AncestorEnabled;
            InitHierarchy(data);
            return data;
        }

    }


    [Test]
    public void InitializesHierarchyInTreeOrder() {
        TestView testView = new TestView(typeof(ViewTestThing));

        MetaData data = testView.TestCreate();

        Assert.IsInstanceOf<ViewTestThing>(data.element);

        AssertHierarchy(data, new TypeAssert(typeof(ViewTestThing), 0, new[] {
            new TypeAssert(typeof(UIGroupElement), 0, new[] {
                new TypeAssert(typeof(TranscludedThing), 0, new[] {
                    new TypeAssert(typeof(UITextElement), 0),
                    new TypeAssert(typeof(UITextElement), 0),
                    new TypeAssert(typeof(UITextElement), 0)
                }),
            })
        }));
    }


    [Test]
    public void SetsFlagsForHierarchyChildren() {
        TestView testView = new TestView(typeof(ViewTestThing));

        MetaData data = testView.TestCreate();
        data.children[0].children[0].element.flags &= ~(UIElementFlags.Enabled);

        Assert.IsInstanceOf<TranscludedThing>(data.children[0].children[0].element);

        Assert.IsInstanceOf<ViewTestThing>(data.element);
        data.element.flags |= UIElementFlags.AncestorEnabled;     

        AssertHierarchyFlags(data, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), disabledSelf, new[] {
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UITextElement), disabledAncestor)
                }),
            })
        }));
    }

    [Test]
    public void DisableElement() {
        TestView testView = new TestView(typeof(ViewTestThing));
        MetaData data = testView.TestCreate();
        TranscludedThing thing = As<TranscludedThing>(data.children[0].children[0].element);
        
        testView.DisableElement(thing);

        AssertHierarchyFlags(data, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), disabledSelf, new[] {
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UITextElement), disabledAncestor)
                }),
            })
        }));
        
        TestView testView2 = new TestView(typeof(ViewTestThing));

        data = testView2.TestCreate();
        
        UITextElement text0 = As<UITextElement>(data.children[0].children[0].children[0].element);
        UITextElement text1 = As<UITextElement>(data.children[0].children[0].children[1].element);
        UITextElement text2 = As<UITextElement>(data.children[0].children[0].children[2].element);
        UIGroupElement group = As<UIGroupElement>(data.children[0].element);
        
        thing = As<TranscludedThing>(data.children[0].children[0].element);
        
        testView2.DisableElement(text1);
        AssertHierarchyFlags(data, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), enabled, new[] {
                    new TypeAssert(typeof(UITextElement), enabled),
                    new TypeAssert(typeof(UITextElement), disabledSelf),
                    new TypeAssert(typeof(UITextElement), enabled)
                }),
            })
        }));

        
        testView2.DisableElement(text0);
        AssertHierarchyFlags(data, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), enabled, new[] {
                    new TypeAssert(typeof(UITextElement), disabledSelf),
                    new TypeAssert(typeof(UITextElement), disabledSelf),
                    new TypeAssert(typeof(UITextElement), enabled)
                }),
            })
        }));
        
        testView2.DisableElement(group);
        AssertHierarchyFlags(data, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), disabledSelf, new[] {
                new TypeAssert(typeof(TranscludedThing), disabledAncestor, new[] {
                    new TypeAssert(typeof(UITextElement), disabledSelf),
                    new TypeAssert(typeof(UITextElement), disabledSelf),
                    new TypeAssert(typeof(UITextElement), disabledAncestor)
                }),
            })
        }));
        
    }

    [Test]
    public void Callback_OnDisable() {
         TestView testView = new TestView(typeof(ViewTestThing));
        MetaData data = testView.TestCreate();
        TranscludedThing thing = As<TranscludedThing>(data.children[0].children[0].element);

        int callCount = 0;
        
        thing.onDisableCallback = () => { callCount++; };
        testView.DisableElement(thing);
        Assert.AreEqual(1, callCount);
        
        TestView testView2 = new TestView(typeof(ViewTestThing));
        data = testView2.TestCreate();
        thing = As<TranscludedThing>(data.children[0].children[0].element);
        thing.onDisableCallback = () => { callCount++; };
        testView2.DisableElement(data.children[0].element);
        Assert.AreEqual(2, callCount);   
        testView2.DisableElement(data.children[0].element);
        Assert.AreEqual(2, callCount);   

    }


    [Test]
    public void EnableElement() {
        TestView testView = new TestView(typeof(ViewTestThing));
        MetaData data = testView.TestCreate();
        TranscludedThing thing = As<TranscludedThing>(data.children[0].children[0].element);
        
        testView.DisableElement(thing);

        AssertHierarchyFlags(data, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), disabledSelf, new[] {
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UITextElement), disabledAncestor)
                }),
            })
        }));
        
        testView.EnableElement(thing);
        AssertHierarchyFlags(data, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), enabled, new[] {
                    new TypeAssert(typeof(UITextElement), enabled),
                    new TypeAssert(typeof(UITextElement), enabled),
                    new TypeAssert(typeof(UITextElement), enabled)
                }),
            })
        }));
        UIGroupElement group = As<UIGroupElement>(data.children[0].element);
        UITextElement text2 = As<UITextElement>(data.children[0].children[0].children[2].element);

        testView.DisableElement(group);
        AssertHierarchyFlags(data, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), disabledSelf, new[] {
                new TypeAssert(typeof(TranscludedThing), disabledAncestor, new[] {
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UITextElement), disabledAncestor)
                }),
            })
        }));
        testView.DisableElement(text2);
        AssertHierarchyFlags(data, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), disabledSelf, new[] {
                new TypeAssert(typeof(TranscludedThing), disabledAncestor, new[] {
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UITextElement), disabledAncestor),
                    new TypeAssert(typeof(UITextElement), disabledSelf)
                }),
            })
        }));
        testView.EnableElement(group);
        AssertHierarchyFlags(data, new TypeAssert(typeof(ViewTestThing), enabled, new[] {
            new TypeAssert(typeof(UIGroupElement), enabled, new[] {
                new TypeAssert(typeof(TranscludedThing), enabled, new[] {
                    new TypeAssert(typeof(UITextElement), enabled),
                    new TypeAssert(typeof(UITextElement), enabled),
                    new TypeAssert(typeof(UITextElement), disabledSelf)
                }),
            })
        }));
    }

    [Test]
    public void Callback_OnEnable() {
        TestView testView = new TestView(typeof(ViewTestThing));
        MetaData data = testView.TestCreate();
        TranscludedThing thing = As<TranscludedThing>(data.children[0].children[0].element);
        UIGroupElement group = As<UIGroupElement>(data.children[0].element);
        int callCount = 0;
        thing.onEnableCallback = () => callCount++;
        testView.DisableElement(thing);
        testView.EnableElement(thing);
        Assert.AreEqual(1, callCount);
        testView.DisableElement(group);
        Assert.IsTrue(thing.isDisabled);
        testView.EnableElement(thing);
        Assert.AreEqual(1, callCount);
        testView.EnableElement(group);
        Assert.AreEqual(2, callCount);
    }

    
    
    [Test]
    public void LifeCycle_OnCreate() {
        TestView testView = new TestView(typeof(ViewTestThing));
        testView.Initialize(true);
        ViewTestThing thing = (ViewTestThing)testView.RootElement;
        Assert.IsTrue(thing.didCreate);
    }

    [Test]
    public void LifeCycle_OnUpdate() {
        TestView testView = new TestView(typeof(ViewTestThing));
        testView.Initialize(true);
        ViewTestThing thing = (ViewTestThing)testView.RootElement;
        Assert.AreEqual(0, thing.updateCount);
        testView.Update();
        Assert.AreEqual(1, thing.updateCount);
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

    private static void AssertHierarchy(MetaData data, TypeAssert assertRoot, int depth = 0) {
        Assert.AreEqual(data.element.GetType(), assertRoot.parentType);
        if (data.children.Count != assertRoot.childTypes.Length) {
            Assert.Fail("Child Count did not match at depth: " + depth);
        }

        for (int i = 0; i < data.children.Count; i++) {
            if (data.children[i].element.GetType() != assertRoot.childTypes[i].parentType) {
                Assert.Fail("Types did not match for child number " + i + " at depth " + depth);
            }

            AssertHierarchy(data.children[i], assertRoot.childTypes[i], depth + 1);
        }
    }

    private static void AssertHierarchyFlags(MetaData data, TypeAssert assertRoot, int depth = 0) {
        Assert.AreEqual(data.element.GetType(), assertRoot.parentType);

        switch (assertRoot.flags) {
            case TestEnableFlags.Disabled:
                Assert.IsTrue(data.element.isDisabled);
                Assert.IsFalse(data.element.isEnabled);
                break;
            case TestEnableFlags.DisabledSelf:
                Assert.IsTrue(data.element.isSelfDisabled);
                Assert.IsFalse(data.element.isSelfEnabled);
                break;
            case TestEnableFlags.DisabledAncestor:
                Assert.IsTrue(data.element.isDisabled);
                Assert.IsTrue(data.element.isSelfEnabled);
                Assert.IsFalse(data.element.isEnabled);
                break;
            case TestEnableFlags.Enabled:
                Assert.IsTrue(data.element.isEnabled);
                Assert.IsTrue(data.element.isSelfEnabled);
                Assert.IsFalse(data.element.isSelfDisabled);
                break;
            case TestEnableFlags.EnabledAncestor:
                break;
            case TestEnableFlags.EnabledSelf:
                Assert.IsTrue(data.element.isSelfEnabled);
                break;
        }

        if (data.children.Count != assertRoot.childTypes.Length) {
            Assert.Fail("Child Count did not match at depth: " + depth);
        }

        for (int i = 0; i < data.children.Count; i++) {
            if (data.children[i].element.GetType() != assertRoot.childTypes[i].parentType) {
                Assert.Fail("Types did not match for child number " + i + " at depth " + depth);
            }

            AssertHierarchy(data.children[i], assertRoot.childTypes[i], depth + 1);
        }
    }

}