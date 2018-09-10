using JetBrains.Annotations;
using NUnit.Framework;
using Src;

[TestFixture]
public class ElementQueryTests {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                <Group x-id='target'/>
                <Group x-some-attr='some-unused-value'>
                    <Group x-id='find-me'/>
                </Group>
                <FindTestThingScoped x-id='child'/>
            </Contents>
        </UITemplate>
    ")]
    public class FindTestThing : UIElement {

        public UIElement target;
        public UIElement shouldBeNull;
        public UIElement findMe;
        public FindTestThingScoped child;

        public override void OnCreate() {
            target = FindById("target");
            shouldBeNull = FindById("only-find-from-self");
            findMe = FindById("find-me");
            child = (FindTestThingScoped)FindById("child");
        }

    }

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
               <Group x-id='only-find-from-self'/>
            </Contents>
        </UITemplate>
    ")]
    [UsedImplicitly]
    public class FindTestThingScoped : UIElement {

        public UIElement childThing;
        
        public override void OnCreate() {
            childThing = FindById("only-find-from-self");
        }

    }

    [Test]
    public void Query_FindById() {
        UIView_Tests.TestView testView = new UIView_Tests.TestView(typeof(FindTestThing));
        testView.Initialize(true);
        FindTestThing root = (FindTestThing) testView.RootElement;
        Assert.IsInstanceOf<UIElement>(root.target);
    }

    [Test]
    public void Query_FindById_DoNotSearchChildTemplates() {
        UIView_Tests.TestView testView = new UIView_Tests.TestView(typeof(FindTestThing));
        testView.Initialize(true);
        FindTestThing root = (FindTestThing) testView.RootElement;
        Assert.IsNull(root.shouldBeNull);
    }

    [Test]
    public void Query_FindById_SearchChildren() {
        UIView_Tests.TestView testView = new UIView_Tests.TestView(typeof(FindTestThing));
        testView.Initialize(true);
        FindTestThing root = (FindTestThing) testView.RootElement;
        Assert.IsNotNull(root.findMe);
    }

    [Test]
    public void Query_FindById_FromChild() {
        UIView_Tests.TestView testView = new UIView_Tests.TestView(typeof(FindTestThing));
        testView.Initialize(true);
        FindTestThing root = (FindTestThing) testView.RootElement;
        Assert.IsNotNull(root.child.childThing);
    }
}