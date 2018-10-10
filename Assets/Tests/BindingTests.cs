using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Src;
using Tests.Mocks;

[TestFixture]
public class BindingTests {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                <Children/>
            </Contents>
        </UITemplate>
    ")]
    public class BindingTestThingChild : UIElement {

        public int intProperty;

    }

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                <BindingTestThingChild x-id=""child"" intProperty='{intProperty}'/>
            </Contents>
        </UITemplate>
    ")]
    public class BindingTestThing : UIElement {

        public int intProperty;
        public List<int> list;
        public List<int> list2;

    }

    [Test]
    public void RunAnActiveBinding() {
        MockView view = new MockView(typeof(BindingTestThing));
        view.Initialize();
        BindingTestThing root = (BindingTestThing) view.RootElement;
        BindingTestThingChild child = (BindingTestThingChild) root.FindById("child");
        root.intProperty = 11;
        view.Update();
        Assert.AreEqual(11, child.intProperty);
    }

    [Test]
    public void RunConditionalBinding() {
        string template = @"
        <UITemplate>
            <Contents>
                <BindingTestThingChild x-id=""child"" x-if='{intProperty > 3}' intProperty='{intProperty}'/>
            </Contents>
        </UITemplate>
        ";
        MockView view = new MockView(typeof(BindingTestThing), template);
        view.Initialize();
        BindingTestThing root = (BindingTestThing) view.RootElement;
        BindingTestThingChild child = (BindingTestThingChild) root.FindById("child");
        root.intProperty = 2;
        view.Update();
        Assert.AreEqual(2, child.intProperty);
        Assert.IsTrue(child.isDisabled);
        root.intProperty = 4;
        view.Update();
        Assert.AreEqual(4, child.intProperty);
        Assert.IsTrue(child.isEnabled);
    }

    [Test]
    public void RunRepeat_SetIndex() {
        string template = @"
        <UITemplate>
            <Contents>
                <Repeat list='{list}' as='item'>
                    <BindingTestThingChild intProperty='{$index}'/>
                </Repeat>
            </Contents>
        </UITemplate>
        ";
        MockView view = new MockView(typeof(BindingTestThing), template);
        view.Initialize();
        BindingTestThing root = (BindingTestThing) view.RootElement;
        root.list = new List<int>();
        root.list.Add(1);
        root.list.Add(2);
        root.list.Add(3);
        view.Update();
        List<BindingTestThingChild> children = root.FindByType<BindingTestThingChild>();
        Assert.AreEqual(3, children.Count);
        Assert.AreEqual(0, children[0].intProperty);
        Assert.AreEqual(1, children[1].intProperty);
        Assert.AreEqual(2, children[2].intProperty);
        root.list.RemoveAt(1);
        view.Update();
        children = root.FindByType<BindingTestThingChild>();
        Assert.AreEqual(2, children.Count);
        Assert.AreEqual(children[0].intProperty, 0);
        Assert.AreEqual(children[1].intProperty, 1);
    }

    [Test]
    public void RepeatDestroysItemsOnRemove() {
        string template = @"
        <UITemplate>
            <Contents>
                <Repeat list='{list}' as='item'>
                    <BindingTestThingChild intProperty='{$index}'/>
                </Repeat>
            </Contents>
        </UITemplate>
        ";
        MockView view = new MockView(typeof(BindingTestThing), template);
        view.Initialize();
        BindingTestThing root = (BindingTestThing) view.RootElement;
        root.list = new List<int>();
        root.list.Add(1);
        root.list.Add(2);
        root.list.Add(3);
        view.Update();
        List<BindingTestThingChild> children = root.FindByType<BindingTestThingChild>();
        var child = children[2];
        Assert.AreEqual(3, children.Count);
        Assert.IsTrue(children.Contains(child));
        root.list.RemoveAt(1);
        view.Update();
        children = root.FindByType<BindingTestThingChild>();
        Assert.IsFalse(children.Contains(child));
        Assert.IsTrue(child.isDestroyed);
    }
    
    [Test]
    public void RepeatDestroysItemsOnListNull() {
        string template = @"
        <UITemplate>
            <Contents>
                <Repeat list='{list}' as='item'>
                    <BindingTestThingChild intProperty='{$index}'/>
                </Repeat>
            </Contents>
        </UITemplate>
        ";
        MockView view = new MockView(typeof(BindingTestThing), template);
        view.Initialize();
        BindingTestThing root = (BindingTestThing) view.RootElement;
        root.list = new List<int>();
        root.list.Add(1);
        root.list.Add(2);
        root.list.Add(3);
        view.Update();
        List<BindingTestThingChild> children = root.FindByType<BindingTestThingChild>();
        Assert.AreEqual(3, children.Count);
        root.list = null;
        view.Update();
        children = root.FindByType<BindingTestThingChild>();
        Assert.IsTrue(children.Count == 0);
    }
    
    [Test]
    public void RepeatUpdateWhenListItemAdded() {
        string template = @"
        <UITemplate>
            <Contents>
                <Repeat list='{list}' as='item'>
                    <BindingTestThingChild intProperty='{$index}'/>
                </Repeat>
            </Contents>
        </UITemplate>
        ";
        MockView view = new MockView(typeof(BindingTestThing), template);
        view.Initialize();
        BindingTestThing root = (BindingTestThing) view.RootElement;
        root.list = new List<int>();
        root.list.Add(1);
        root.list.Add(2);
        root.list.Add(3);
        view.Update();
        List<BindingTestThingChild> children = root.FindByType<BindingTestThingChild>();
        Assert.AreEqual(3, children.Count);
        root.list.Add(4);
        view.Update();
        children = root.FindByType<BindingTestThingChild>();
        Assert.AreEqual(4, children.Count);
    }
    
    [Test]
    public void RunRepeat_SetItem() {
        string template = @"
        <UITemplate>
            <Contents>
                <Repeat list='{list}' as='item'>
                    <Text>{$item}</Text>
                </Repeat>
            </Contents>
        </UITemplate>
        ";
        MockView view = new MockView(typeof(BindingTestThing), template);
        view.Initialize();
        BindingTestThing root = (BindingTestThing) view.RootElement;
        root.list = new List<int>();
        root.list.Add(1);
        root.list.Add(2);
        root.list.Add(3);
        view.Update();
        List<UITextElement> children = root.FindByType<UITextElement>();
        Assert.AreEqual(3, children.Count);
       
    }
    
    [Test]
    public void HandleMultipleNonNestedRepeats() {
        string template = @"
        <UITemplate>
            <Contents>
                <Repeat list='{list}' as='item'>
                    <Text>{$item}</Text>
                </Repeat>
                <Repeat list='{list2}' as='item'>
                    <Text>{$item}</Text>
                </Repeat>
            </Contents>
        </UITemplate>
        ";
        MockView view = new MockView(typeof(BindingTestThing), template);
        view.Initialize();
        BindingTestThing root = (BindingTestThing) view.RootElement;
        root.list = new List<int>();
        root.list2 = new List<int>();
        root.list.Add(1);
        root.list.Add(2);
        root.list.Add(3);
        root.list2.Add(4);
        root.list2.Add(5);
        root.list2.Add(6);
        view.Update();
        List<UITextElement> children = root.FindByType<UITextElement>();
        Assert.AreEqual(6, children.Count);
       
    }

}