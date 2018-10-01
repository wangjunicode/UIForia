using System;
using NUnit.Framework;
using Rendering;
using Src;
using Src.Systems;
using UnityEditor.VersionControl;
using UnityEngine;

[TestFixture]
public class StyleTests {

    
    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                <Group x-name='group1'>
                    <Group x-name='group2'/>
                </Group>                  
                <Group x-name='group3'/>                  
                <Group x-name='group4'/>                  
            </Contents>
        </UITemplate>
    ")]
    public class StyleSetTestThing : UIElement { }
    
    [Test]
    public void SetFontSize_UpdateComputedStyleWithValue() {
        
        UIView_Tests.TestView view = new UIView_Tests.TestView(typeof(StyleSetTestThing));
        StyleSystem s = new StyleSystem();
        MetaData data = view.TestCreate();
        s.OnElementCreatedFromTemplate(data);
        UIElement element = data.element;

        Assert.AreEqual(DefaultStyleValues.fontSize, element.style.computedStyle.FontSize);

        element.style.SetFontSize(DefaultStyleValues.fontSize + 1, StyleState.Normal);
        
        Assert.AreEqual(DefaultStyleValues.fontSize + 1, element.style.computedStyle.FontSize);

    }
    
    [Test]
    public void SetFontSize_UpdateComputedStyleWithUnsetValue() {
        
        UIView_Tests.TestView view = new UIView_Tests.TestView(typeof(StyleSetTestThing));
        StyleSystem s = new StyleSystem();
        MetaData data = view.TestCreate();
        s.OnElementCreatedFromTemplate(data);
        UIElement element = data.element;

        Assert.AreEqual(DefaultStyleValues.fontSize, element.style.computedStyle.FontSize);
        element.style.computedStyle.FontSize = DefaultStyleValues.fontSize + 1;
        Assert.AreEqual(DefaultStyleValues.fontSize + 1, element.style.computedStyle.FontSize);

        element.style.computedStyle.FontSize = IntUtil.UnsetValue;
        
        Assert.AreEqual(DefaultStyleValues.fontSize, element.style.computedStyle.FontSize);

    }

    [Test]
    public void SetFontSize_InheritedByChildrenWithoutOverride() {
        UIView_Tests.TestView view = new UIView_Tests.TestView(typeof(StyleSetTestThing));
        StyleSystem s = new StyleSystem();
        MetaData data = view.TestCreate();
        s.OnElementCreatedFromTemplate(data);
        UIElement element = data.element;
        UIElement child = data.children[0].element;

        
        Assert.AreEqual(DefaultStyleValues.fontSize, child.style.computedStyle.FontSize);
        
        element.style.computedStyle.FontSize = DefaultStyleValues.fontSize + 1;
        
        Assert.AreEqual(DefaultStyleValues.fontSize + 1, child.style.computedStyle.FontSize);

    }
    
    [Test]
    public void SetFontSize_ChildWithOverride() {
        UIView_Tests.TestView view = new UIView_Tests.TestView(typeof(StyleSetTestThing));
        StyleSystem s = new StyleSystem();
        MetaData data = view.TestCreate();
        s.OnElementCreatedFromTemplate(data);
        UIElement element = data.element;
        UIElement child = data.children[0].element;
        UIElement grandChild = data.children[0].children[0].element;

        int defaultSize = DefaultStyleValues.fontSize;

        child.style.computedStyle.FontSize = defaultSize + 1;
        element.style.computedStyle.FontSize = defaultSize - 1;
        
        Assert.AreEqual(defaultSize + 1, child.style.computedStyle.FontSize);
        Assert.AreEqual(defaultSize + 1, grandChild.style.computedStyle.FontSize);
        Assert.AreEqual(defaultSize - 1, element.style.computedStyle.FontSize);
        
    }

    [Test]
    public void SetFontSize_StateNotActive() {
        UIView_Tests.TestView view = new UIView_Tests.TestView(typeof(StyleSetTestThing));
        StyleSystem s = new StyleSystem();
        MetaData data = view.TestCreate();
        s.OnElementCreatedFromTemplate(data);
        UIElement element = data.element;
        int defaultSize = DefaultStyleValues.fontSize;

        element.style.SetFontSize(defaultSize + 1, StyleState.Hover);
        
        Assert.AreEqual(defaultSize, element.style.computedStyle.FontSize);
    }
    
    [Test]
    public void SetFontSize_StateActiveOverridden() {
        UIView_Tests.TestView view = new UIView_Tests.TestView(typeof(StyleSetTestThing));
        StyleSystem s = new StyleSystem();
        MetaData data = view.TestCreate();
        s.OnElementCreatedFromTemplate(data);
        UIElement element = data.element;
        UIElement child = data.children[0].element;
        int defaultSize = DefaultStyleValues.fontSize;
            
        element.style.EnterState(StyleState.Hover);
        element.style.SetFontSize(defaultSize + 1, StyleState.Hover);
        element.style.SetFontSize(defaultSize + 2, StyleState.Normal);
        
        Assert.AreEqual(defaultSize + 1, element.style.computedStyle.FontSize);
        Assert.AreEqual(defaultSize + 1, child.style.computedStyle.FontSize);

    }

    [Test]
    public void SetFontSize_EnterState() {
        UIView_Tests.TestView view = new UIView_Tests.TestView(typeof(StyleSetTestThing));
        StyleSystem s = new StyleSystem();
        MetaData data = view.TestCreate();
        s.OnElementCreatedFromTemplate(data);
        UIElement element = data.element;
        UIElement child = data.children[0].element;
        int defaultSize = DefaultStyleValues.fontSize;
            
        element.style.SetFontSize(defaultSize + 1, StyleState.Hover);
        element.style.SetFontSize(defaultSize + 2, StyleState.Normal);
        
        Assert.AreEqual(defaultSize + 2, element.style.computedStyle.FontSize);
        Assert.AreEqual(defaultSize + 2, child.style.computedStyle.FontSize);
        
        element.style.EnterState(StyleState.Hover);
        
        Assert.AreEqual(defaultSize + 1, element.style.computedStyle.FontSize);
        Assert.AreEqual(defaultSize + 1, child.style.computedStyle.FontSize);
        
    }
    
    [Test]
    public void SetFontSize_ExitState() {
        UIView_Tests.TestView view = new UIView_Tests.TestView(typeof(StyleSetTestThing));
        StyleSystem s = new StyleSystem();
        MetaData data = view.TestCreate();
        s.OnElementCreatedFromTemplate(data);
        UIElement element = data.element;
        UIElement child = data.children[0].element;
        int defaultSize = DefaultStyleValues.fontSize;
        
        element.style.EnterState(StyleState.Hover);
            
        element.style.SetFontSize(defaultSize + 1, StyleState.Hover);
        element.style.SetFontSize(defaultSize + 2, StyleState.Normal);
        
        Assert.AreEqual(defaultSize + 1, element.style.computedStyle.FontSize);
        Assert.AreEqual(defaultSize + 1, child.style.computedStyle.FontSize);
        
        element.style.ExitState(StyleState.Hover);
        
        Assert.AreEqual(defaultSize + 2, element.style.computedStyle.FontSize);
        Assert.AreEqual(defaultSize + 2, child.style.computedStyle.FontSize);
        
    }

    [Test]
    public void SetFontSize_UseBaseStyleFontSize() {
        UIView_Tests.TestView view = new UIView_Tests.TestView(typeof(StyleSetTestThing));
        StyleSystem s = new StyleSystem();
        MetaData data = view.TestCreate();
        s.OnElementCreatedFromTemplate(data);
        UIElement element = data.element;
        UIElement child = data.children[0].element;
        int defaultSize = DefaultStyleValues.fontSize;
        
        UIStyle baseStyle = new UIStyle();
        baseStyle.FontSize = defaultSize - 5;
        
        element.style.AddBaseStyle(baseStyle, StyleState.Normal);
        
        Assert.AreEqual(defaultSize - 5, element.style.computedStyle.FontSize);
        
        element.style.RemoveBaseStyle(baseStyle);
        
        Assert.AreEqual(defaultSize, element.style.computedStyle.FontSize);
        
    }

    [Test]
    public void SetFontSize_EmitChangeEvent_OnChange() {
        UIView_Tests.TestView view = new UIView_Tests.TestView(typeof(StyleSetTestThing));
        StyleSystem s = new StyleSystem();
        MetaData data = view.TestCreate();
        s.OnElementCreatedFromTemplate(data);
        UIElement element = data.element;
        int defaultSize = DefaultStyleValues.fontSize;
        int changeCount = 0;
        
        // 1 change call per child that changes
        s.onFontPropertyChanged += (id, prop) => {
            changeCount++;
            Assert.AreEqual(defaultSize + 1, prop.fontSize);
        };
        
        Assert.AreEqual(DefaultStyleValues.fontSize, element.style.computedStyle.FontSize);

        element.style.computedStyle.FontSize = defaultSize + 1;
        
        Assert.AreEqual(defaultSize + 1, element.style.computedStyle.FontSize);
        Assert.AreEqual(5, changeCount);
    }

    [Test]
    public void SetFontSize_EmitChangeEvent_ForChangedChildren() {
        UIView_Tests.TestView view = new UIView_Tests.TestView(typeof(StyleSetTestThing));
        StyleSystem s = new StyleSystem();
        MetaData data = view.TestCreate();
        s.OnElementCreatedFromTemplate(data);
        UIElement element = data.element;
        UIElement child = data.children[0].element;

        int defaultSize = DefaultStyleValues.fontSize;
        int changeCount = 0;
        
        s.onFontPropertyChanged += (cbChild, prop) => {
            if (child == cbChild) {
                changeCount++;
                Assert.AreEqual(defaultSize + 1, prop.fontSize);
            }
        };
        
        Assert.AreEqual(defaultSize, child.style.computedStyle.FontSize);
        
        element.style.computedStyle.FontSize = defaultSize + 1;
        
        Assert.AreEqual(defaultSize + 1, child.style.computedStyle.FontSize);
        Assert.AreEqual(1, changeCount);

    }
    
    [Test]
    public void SetFontSize_EmitChangeEvent_NotWhenUnchanged() {
        UIView_Tests.TestView view = new UIView_Tests.TestView(typeof(StyleSetTestThing));
        StyleSystem s = new StyleSystem();
        MetaData data = view.TestCreate();
        s.OnElementCreatedFromTemplate(data);
        UIElement element = data.element;
        int defaultSize = DefaultStyleValues.fontSize;
        int changeCount = 0;
        
        element.style.computedStyle.FontSize = defaultSize + 1;

        s.onFontPropertyChanged += (id, prop) => {
            changeCount++;
        };
        
        element.style.computedStyle.FontSize = defaultSize + 1;
        
        Assert.AreEqual(0, changeCount);
    }

}