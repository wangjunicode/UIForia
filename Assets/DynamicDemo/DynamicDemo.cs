using System;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;

public class DynamicDataTest<T> : IDynamicData where T : UIElement {

    public Type ElementType => typeof(T);

}

[Template("DynamicDemo/DynamicType0.xml")]
public class DynamicType0 : UIElement, IDynamicElement {

    public IDynamicData data;

    public void SetData(IDynamicData data) {
        this.data = data;
    } 

}

[Template("DynamicDemo/DynamicType1.xml")]
public class DynamicType1 : UIElement, IDynamicElement {

    public IDynamicData data;

    public void SetData(IDynamicData data) {
        this.data = data;
    }

}

[Template("DynamicDemo/DynamicDemo.xml")]
public class DynamicDemo : UIElement {

    public IDynamicData currentDynamicItem;

    public void SelectDynamic(int idx) {
        switch (idx) {
            case 0:
                currentDynamicItem = new DynamicDataTest<DynamicType0>();
                break;
            case 1:
                currentDynamicItem = new DynamicDataTest<DynamicType1>();
                break;
        }
    }

}