using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine.Experimental.UIElements;

[TestFixture]
public class BindingTests {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                <Children/>
            </Contents>
        </UITemplate>
    ")]
    [TemplateTagName("BindingTestThingChild")]
    public class BindingTestThingChild : UIElement {

        public int intProperty;

    }

    public class Thing {
        public string name;

        public Thing(string name) {
            this.name = name;
        }
    }

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                <BindingTestThingChild x-id=""child"" intProperty='intProperty'/>
            </Contents>
        </UITemplate>
    ")]
    [TemplateTagName("BindingTestThing")]
    public class BindingTestThing : UIElement {

        public int intProperty;
        public RepeatableList<int> list;
        public RepeatableList<int> list2;

    }

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                <Repeat list='thelist'>
                    <Group style='[GetStyle($item)]'>
                        <Div />
                        <Div > {$item.name}</Div>
                    </Group>
                </Repeat>
            </Contents>
        </UITemplate>
    ")]
    public class RepeatBindingChecker : UIElement {

        public RepeatableList<Thing> thelist;

        public override void OnCreate() {
            thelist = new RepeatableList<Thing>() {
                new Thing("a"),
                new Thing("b"),
                new Thing("c"),
            };
        }

        public string GetStyle(Thing thing) {
            return thing.name;
        }
                
        [OnMouseUp]
        public void OnMouseUp() {
            
            thelist.RemoveAt(0);

            if (thelist.Count == 0) {
                int count = thelist.Count;
                for (int i = 0; i < count + 3; i++) {
                    thelist.Add(new Thing("index" + i));
                }
            }
        }
    }

    [Test]
    public void CreateAndDeleteRepeatableListItems() {
        MockApplication app = new MockApplication(typeof(RepeatBindingChecker));

        for (int i = 0; i < 1000; i++) {
            app.InputSystem.MouseUp();
            app.Update();
            app.InputSystem.MouseUp();
        }
    }
    
    [Test]
    public void RunAnActiveBinding() {
        MockApplication app = new MockApplication(typeof(BindingTestThing));
        BindingTestThing root = (BindingTestThing) app.RootElement;
        BindingTestThingChild child = (BindingTestThingChild) root.FindById("child");
        root.intProperty = 11;
        app.Update();
        Assert.AreEqual(11, child.intProperty);
    }

    [Test]
    public void RunConditionalBinding() {
        string template = @"
        <UITemplate>
            <Contents>
                <BindingTestThingChild x-id=""child"" if='{intProperty > 3}' intProperty='intProperty'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(BindingTestThing), template);
        BindingTestThing root = (BindingTestThing) app.RootElement;
        BindingTestThingChild child = (BindingTestThingChild) root.FindById("child");
        root.intProperty = 2;
        app.Update();
        Assert.AreEqual(2, child.intProperty);
        Assert.IsTrue(child.isDisabled);
        root.intProperty = 4;
        app.Update();
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
        MockApplication app = new MockApplication(typeof(BindingTestThing), template);
        BindingTestThing root = (BindingTestThing) app.RootElement;
        root.list = new RepeatableList<int>();
        root.list.Add(1);
        root.list.Add(2);
        root.list.Add(3);
        app.Update();
        List<BindingTestThingChild> children = root.FindByType<BindingTestThingChild>();
        Assert.AreEqual(3, children.Count);
        Assert.AreEqual(0, children[0].intProperty);
        Assert.AreEqual(1, children[1].intProperty);
        Assert.AreEqual(2, children[2].intProperty);
        root.list.RemoveAt(1);
        app.Update();
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
        MockApplication app = new MockApplication(typeof(BindingTestThing), template);
        BindingTestThing root = (BindingTestThing) app.RootElement;
        root.list = new RepeatableList<int>();
        root.list.Add(1);
        root.list.Add(2);
        root.list.Add(3);
        app.Update();
        List<BindingTestThingChild> children = root.FindByType<BindingTestThingChild>();
        var child = children[1];
        Assert.AreEqual(3, children.Count);
        Assert.IsTrue(children.Contains(child));
        root.list.RemoveAt(1);
        app.Update();
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
        MockApplication app = new MockApplication(typeof(BindingTestThing), template);
        BindingTestThing root = (BindingTestThing) app.RootElement;
        root.list = new RepeatableList<int>();
        root.list.Add(1);
        root.list.Add(2);
        root.list.Add(3);
        app.Update();
        List<BindingTestThingChild> children = root.FindByType<BindingTestThingChild>();
        Assert.AreEqual(3, children.Count);
        root.list = null;
        app.Update();
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
        MockApplication app = new MockApplication(typeof(BindingTestThing), template);
        BindingTestThing root = (BindingTestThing) app.RootElement;
        root.list = new RepeatableList<int>();
        root.list.Add(1);
        root.list.Add(2);
        root.list.Add(3);
        app.Update();
        List<BindingTestThingChild> children = root.FindByType<BindingTestThingChild>();
        Assert.AreEqual(3, children.Count);
        root.list.Add(4);
        app.Update();
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
        MockApplication app = new MockApplication(typeof(BindingTestThing), template);
        BindingTestThing root = (BindingTestThing) app.RootElement;
        root.list = new RepeatableList<int>();
        root.list.Add(1);
        root.list.Add(2);
        root.list.Add(3);
        app.Update();
        List<UITextElement> children = root.FindByType<UITextElement>();
        Assert.AreEqual(3, children.Count);
       
    }
    
    [Test]
    public void RunRepeat_ChildSetItem() {
        string template = @"
        <UITemplate>
            <Contents>
                <Repeat list='{list}' as='item'>
                    <Group>
                        <Text>{$item}</Text>
                    </Group>
                </Repeat>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(BindingTestThing), template);
        BindingTestThing root = (BindingTestThing) app.RootElement;
        root.list = new RepeatableList<int>();
        root.list.Add(1);
        root.list.Add(2);
        root.list.Add(3);
        app.Update();
        List<UITextElement> children = root.FindByType<UITextElement>();
        Assert.AreEqual(3, children.Count);
        Assert.AreEqual("1", children[0].text);
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
        MockApplication app = new MockApplication(typeof(BindingTestThing), template);
        BindingTestThing root = (BindingTestThing) app.RootElement;
        root.list = new RepeatableList<int>();
        root.list2 = new RepeatableList<int>();
        root.list.Add(1);
        root.list.Add(2);
        root.list.Add(3);
        root.list2.Add(4);
        root.list2.Add(5);
        root.list2.Add(6);
        app.Update();
        List<UITextElement> children = root.FindByType<UITextElement>();
        Assert.AreEqual(6, children.Count);
       
    }

}