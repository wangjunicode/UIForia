using NUnit.Framework;
using UIForia.Rendering;
using UIForia;
using Tests.Mocks;
using TMPro;
using UnityEngine;

[TestFixture]
public class StyleBindingTests {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
            </Contents>
        </UITemplate>
    ")]
    public class StyleTestThing : UIElement {

        public string textureName;
        public string fontName;

    }

    [SetUp]
    public void Setup() {
        UIForia.ResourceManager.Reset();
    }
    
    [Test]
    public void SetBackgroundImageConstant() {
        const string template = @"
        <UITemplate>
            <Contents>
                <Panel style.backgroundImage=""url('path/to/tex1')""/>
            </Contents>
        </UITemplate>
        ";
        Texture2D tex1 = new Texture2D(1, 1);
        UIForia.ResourceManager.AddTexture("path/to/tex1", tex1);
        MockApplication view = new MockApplication(typeof(StyleTestThing), template);
        StyleTestThing root = (StyleTestThing) view.RootElement;
        UIElement panel = root.FindFirstByType<UIElement>();
        Assert.AreEqual(tex1, panel.ComputedStyle.BackgroundImage);
    }
    
    [Test]
    public void SetBackgroundImageDynamic() {
        const string template = @"
        <UITemplate>
            <Contents>
                <Panel style.backgroundImage=""url('path/to/' + textureName)""/>
            </Contents>
        </UITemplate>
        ";
        Texture2D tex1 = new Texture2D(1, 1);
        UIForia.ResourceManager.AddTexture("path/to/tex1", tex1);
        MockApplication view = new MockApplication(typeof(StyleTestThing), template);
        StyleTestThing root = (StyleTestThing) view.RootElement;
        root.textureName = "tex1";
        view.Update();
        UIElement panel = root.FindFirstByType<UIElement>();
        Assert.AreEqual(tex1, panel.ComputedStyle.BackgroundImage);
        root.textureName = "not-there";
        view.Update();
        Assert.AreEqual(DefaultStyleValues.BackgroundImage, panel.ComputedStyle.BackgroundImage);
        root.textureName = "tex1";
        view.Update();
        Assert.AreEqual(tex1, panel.ComputedStyle.BackgroundImage);

    }
    
    [Test]
    public void SetFontConstant() {
        const string template = @"
        <UITemplate>
            <Contents>
                <Panel style.font=""url('path/to/font1')""/>
            </Contents>
        </UITemplate>
        ";
        TMP_FontAsset font1 = ScriptableObject.CreateInstance<TMP_FontAsset>();
        UIForia.ResourceManager.AddFont("path/to/font1", font1);
        MockApplication view = new MockApplication(typeof(StyleTestThing), template);
        StyleTestThing root = (StyleTestThing) view.RootElement;
        UIElement panel = root.FindFirstByType<UIElement>();
        Assert.AreEqual(font1, panel.ComputedStyle.TextFontAsset);
        UIForia.ResourceManager.RemoveFont(font1);
        view.Update();
        Assert.AreEqual(font1, panel.ComputedStyle.TextFontAsset);
        Object.DestroyImmediate(font1);
    }
    
    [Test]
    public void SetFontDynamic() {
        const string template = @"
        <UITemplate>
            <Contents>
                <Panel style.font=""url('path/to/' + fontName)""/>
            </Contents>
        </UITemplate>
        ";
        TMP_FontAsset font1 = ScriptableObject.CreateInstance<TMP_FontAsset>();
        UIForia.ResourceManager.AddFont("path/to/font1", font1);
        MockApplication view = new MockApplication(typeof(StyleTestThing), template);
        StyleTestThing root = (StyleTestThing) view.RootElement;
        root.fontName = "font1";
        view.Update();
        UIElement panel = root.FindFirstByType<UIElement>();
        Assert.AreEqual(font1, panel.ComputedStyle.TextFontAsset);
        root.fontName = "not-there";
        view.Update();
        Assert.AreEqual(DefaultStyleValues.TextFontAsset, panel.ComputedStyle.TextFontAsset);
        root.fontName = "font1";
        view.Update();
        Assert.AreEqual(font1, panel.ComputedStyle.TextFontAsset);
        Object.DestroyImmediate(font1);

    }

}