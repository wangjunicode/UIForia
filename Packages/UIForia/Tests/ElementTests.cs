using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;

[TestFixture]
public class ElementTests {

    [TemplateTagName("DepthThing")]
    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
            <Group x-id='g1'>
                <Group x-id='g1_child1'/>
            </Group>
            <Group x-id='g2'>
                <Group x-id='g2_child1'/>
                <Group x-id='g2_child2'/>
            </Group>
            <DepthThingChild/>
        </Contents>
    </UITemplate>
    ")]
    public class DepthThing : UIElement {

        public UIElement g1;
        public UIElement g2;
        public UIElement g1_1;
        public UIElement g2_1;
        public UIElement g2_2;
        
        public override void OnCreate() {
            g1 = FindById("g1");
            g1_1 = FindById("g1_child1");
            g2 = FindById("g2");
            g2_1 = FindById("g2_child1");
            g2_2 = FindById("g2_child2");
        }

    }
    
    [TemplateTagName("DepthThingChild")]
    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
            <Group x-id='g1'>
                <Group x-id='g1_child1'/>
            </Group>
            <Group x-id='g2'>
                <Group x-id='g2_child1'/>
                <Group x-id='g2_child2'/>
            </Group>
        </Contents>
    </UITemplate>
    ")]
    public class DepthThingChild : UIElement {}

    [Test]
    public void ProperDepth() {
        MockApplication app = new MockApplication(typeof(DepthThing));
        DepthThing root = (DepthThing) app.RootElement.GetChild(0);
        Assert.AreEqual(1, root.depth);
        Assert.AreEqual(2, root.g1.depth);
        Assert.AreEqual(3, root.g1_1.depth);
        Assert.AreEqual(2, root.g2.depth);
        Assert.AreEqual(3, root.g2_1.depth);
        Assert.AreEqual(3, root.g2_2.depth);
    }
    
    [Test]
    public void ProperDepthNested() {
        MockApplication app = new MockApplication(typeof(DepthThing));
        DepthThing root = (DepthThing) app.RootElement.GetChild(0);
        DepthThingChild child = root.FindFirstByType<DepthThingChild>();
        Assert.AreEqual(2, child.depth);
        Assert.AreEqual(3, child.FindById("g1").depth);
        Assert.AreEqual(4, child.FindById("g1_child1").depth);
        Assert.AreEqual(3, child.FindById("g2").depth);
        Assert.AreEqual(4, child.FindById("g2_child1").depth);
        Assert.AreEqual(4, child.FindById("g2_child2").depth);
    }
    
    [Test]
    public void ProperSiblingIndex() {
        MockApplication app = new MockApplication(typeof(DepthThing));
        DepthThing root = (DepthThing) app.RootElement.GetChild(0);
        Assert.AreEqual(0, root.siblingIndex);
        Assert.AreEqual(0, root.g1.siblingIndex);
        Assert.AreEqual(0, root.g1_1.siblingIndex);
        Assert.AreEqual(1, root.g2.siblingIndex);
        Assert.AreEqual(0, root.g2_1.siblingIndex);
        Assert.AreEqual(1, root.g2_2.siblingIndex);
    }

    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
           <GenericThing--int/>
        </Contents>
    </UITemplate>
    ")]
    public class GenericWrapper : UIElement {}
    
    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
           
        </Contents>
    </UITemplate>
    ")]
    public class GenericThing<T> : UIElement {


    }
    
    [Test]
    public void HandleGenericElementTypes() {
        string template = @"
            <UITemplate>
                <Contents>
                    <GenericThing--int/>
                </Contents>
            </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(GenericWrapper), template);
        Assert.IsInstanceOf<GenericThing<int>>(app.RootElement.GetChild(0).GetChild(0));
    }
    
}