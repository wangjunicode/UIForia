using System.Collections.Generic;
using NUnit.Framework;
using Tests.Mocks;
using UIForia;
using UnityEngine;

[TestFixture]
public class RenderLayerComparerAscendingTest {
    
    [Template(TemplateType.String, @"
        <UITemplate>
            <Style>
                style c1 {
                    RenderLayer = View;
                }
                style c11 {
                    zIndex = 13;
                }
                style c12 {
                    zIndex = 11;
                }
                style c13 {
                    zIndex = 12;
                }
                style c23 {
                    ZIndex = 20;
                }
                style c3 {
                    zIndex = 10;
                }
                style c33 {
                    RenderLayer = Modal;
                }
            </Style>
            <Contents>
                <Group x-id='c1' style=""c1"">
                    <Group x-id='c11' style=""c11""></Group>
                    <Group x-id='c12' style=""c12""></Group>
                    <Group x-id='c13' style=""c13""></Group>
                </Group>
                <Group x-id='c2' style=""c2"">
                    <Group x-id='c21' style=""c21""></Group>
                    <Group x-id='c22' style=""c22""></Group>
                    <Group x-id='c23' style=""c23""></Group>
                </Group>
                <Group x-id='c3' style=""c3"">
                    <Group x-id='c31' style=""c31""></Group>
                    <Group x-id='c32' style=""c32""></Group>
                    <Group x-id='c33' style=""c33"">
                        <Group x-id='c331' style=""c331""></Group>
                    </Group>
                </Group>
            </Contents>
        </UITemplate>
    ")]
    private class TestThing : UIElement {
    }

    [Test]
    public void AssertSortOrderOfUIElementHierarchy() {
      
        MockApplication mockView = new MockApplication(typeof(TestThing));
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        
        UIElement root = mockView.RootElement;
        mockView.Update();

        List<UIElement> ordered = root.FindByType<UIElement>();
        ordered.Sort(new UIElement.RenderLayerComparerAscending());
        
        Assert.AreEqual("c11", ordered[12].GetAttribute("id"));
        Assert.AreEqual("c13", ordered[11].GetAttribute("id"));
        Assert.AreEqual("c12", ordered[10].GetAttribute("id"));
        Assert.AreEqual("c1", ordered[9].GetAttribute("id"));
        Assert.AreEqual("c331", ordered[8].GetAttribute("id"));
        Assert.AreEqual("c33", ordered[7].GetAttribute("id"));
        Assert.AreEqual("c23", ordered[6].GetAttribute("id"));
        Assert.AreEqual("c32", ordered[5].GetAttribute("id"));
        Assert.AreEqual("c31", ordered[4].GetAttribute("id"));
        Assert.AreEqual("c3", ordered[3].GetAttribute("id"));
        Assert.AreEqual("c22", ordered[2].GetAttribute("id"));
        Assert.AreEqual("c21", ordered[1].GetAttribute("id"));
        Assert.AreEqual("c2", ordered[0].GetAttribute("id"));
        
        Debug.Log((float.MaxValue / 1024).ToString("######"));
    }

}
