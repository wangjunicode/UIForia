using NUnit.Framework;
using Rendering;
using Src;
using Src.Systems;
using Tests.Mocks;
using UnityEngine;

[TestFixture]
public class LayoutSystemTests {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.layoutType='Flex'>
                <Group x-id='child0' style='child1' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='100f' style.height='100f'/>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
    ")]
    public class LayoutTestThing : UIElement {

        public UIGroupElement child0;
        public UIGroupElement child1;
        public UIGroupElement child2;

        public override void OnCreate() {
            child0 = FindById<UIGroupElement>("child0");
            child1 = FindById<UIGroupElement>("child1");
            child2 = FindById<UIGroupElement>("child2");
        }

        public class Style {

            [ExportStyle("child1")]
            public static UIStyle Style1() {
                return new UIStyle() {
                    MinWidth = 300f
                };
            }

        }

    }

    [Test]
    public void Works() {
        MockView mockView = new MockView(typeof(LayoutTestThing));
        mockView.Initialize();
        mockView.layoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(Vector2.zero, root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
    }

}