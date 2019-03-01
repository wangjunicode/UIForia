using System;
using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;

[TestFixture]
public class DynamicElementTests {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                <Dynamic data=""dynamicData""/>  
            </Contents>
        </UITemplate>
    ")]
    private class DynamicThing : UIElement {

        public IDynamicData dynamicData;

    }
    
    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                Test Type 0  
            </Contents>
        </UITemplate>
    ")]
    private class DynamicTestType0 : UIElement, IDynamicElement {

        public IDynamicData data;

        public void SetData(IDynamicData data) {
            this.data = data;
        } 

    }
    
    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                Test Type 1  
            </Contents>
        </UITemplate>
    ")]
    private class DynamicTestType1 : UIElement, IDynamicElement {

        public IDynamicData data;

        public void SetData(IDynamicData data) {
            this.data = data;
        }

    }
    
    private class DynamicDataTest<T> : IDynamicData where T : UIElement {

        public Type ElementType => typeof(T);
        public string someValue;

    }
    
    [Test]
    public void SetsData() {
        MockApplication app = new MockApplication(typeof(DynamicThing));
        DynamicThing thing = app.RootElement as DynamicThing;
        app.Update();
        UIDynamicElement dynamicElement = thing.FindFirstByType<UIDynamicElement>();
        Assert.AreEqual(0, dynamicElement.ChildCount);
        thing.dynamicData = new DynamicDataTest<DynamicTestType0>();
        app.Update();
        Assert.AreEqual(1, dynamicElement.ChildCount);
        Assert.IsInstanceOf<DynamicTestType0>(dynamicElement.GetChild(0));
        thing.dynamicData = new DynamicDataTest<DynamicTestType1>();
        app.Update();
        Assert.AreEqual(1, dynamicElement.ChildCount);
        Assert.IsInstanceOf<DynamicTestType1>(dynamicElement.GetChild(0));
    }
    
    [Test]
    public void RemovesChildWhenDataIsNull() {
        MockApplication app = new MockApplication(typeof(DynamicThing));
        DynamicThing thing = app.RootElement as DynamicThing;
        app.Update();
        UIDynamicElement dynamicElement = thing.FindFirstByType<UIDynamicElement>();
        thing.dynamicData = new DynamicDataTest<DynamicTestType1>();
        app.Update();
        Assert.AreEqual(1, dynamicElement.ChildCount);
        thing.dynamicData = null;
        app.Update();
        Assert.AreEqual(0, dynamicElement.ChildCount);
    }
    
    [Test]
    public void DoesNotCreateNewWhenTypeDoesNotChange() {
        MockApplication app = new MockApplication(typeof(DynamicThing));
        DynamicThing thing = app.RootElement as DynamicThing;
        app.Update();
        UIDynamicElement dynamicElement = thing.FindFirstByType<UIDynamicElement>();
        
        DynamicDataTest<DynamicTestType1> d = new DynamicDataTest<DynamicTestType1>();
        d.someValue = "value0";
        thing.dynamicData = d;
        app.Update();
        
        Assert.AreEqual(1, dynamicElement.ChildCount);
        UIElement currentElement = dynamicElement.GetChild(0);
        
        DynamicDataTest<DynamicTestType1> d2 = new DynamicDataTest<DynamicTestType1>();
        d.someValue = "value1";
        thing.dynamicData = d2;
        app.Update();
        Assert.AreEqual(currentElement, dynamicElement.GetChild(0));
        Assert.AreEqual(1, dynamicElement.ChildCount);
    }


}