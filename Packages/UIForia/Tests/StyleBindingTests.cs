using NUnit.Framework;
using Tests.Mocks;
using TMPro;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
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

    
    [Test]
    public void SetBackgroundImageConstant() {
        const string template = @"
        <UITemplate>
            <Contents>
                <Panel style.backgroundImage=""$url('path/to/tex1')""/>
            </Contents>
        </UITemplate>
        ";
        Texture2D tex1 = new Texture2D(1, 1);
        ResourceManager resourceManager = new ResourceManager();
        resourceManager.AddTexture("path/to/tex1", tex1);
        MockApplication view = null;
        StyleTestThing root = (StyleTestThing) view.RootElement.GetChild(0);
        view.Update();
        UIElement panel = root.FindFirstByType<UIElement>();
        Assert.AreEqual(tex1, panel.style.BackgroundImage);
    }
    
    [Test]
    public void SetBackgroundImageDynamic() {
        const string template = @"
        <UITemplate>
            <Contents>
                <Panel style.backgroundImage=""$url('path/to/' + textureName)""/>
            </Contents>
        </UITemplate>
        ";
        Texture2D tex1 = new Texture2D(1, 1);
        ResourceManager resourceManager = new ResourceManager();
        resourceManager.AddTexture("path/to/tex1", tex1);
        MockApplication view = null;
        StyleTestThing root = (StyleTestThing) view.RootElement.GetChild(0);
        root.textureName = "tex1";
        view.Update();
        UIElement panel = root.FindFirstByType<UIElement>();
        Assert.AreEqual(tex1, panel.style.BackgroundImage);
        root.textureName = "not-there";
        view.Update();
        Assert.AreEqual(DefaultStyleValues_Generated.BackgroundImage, panel.style.BackgroundImage);
        root.textureName = "tex1";
        view.Update();
        Assert.AreEqual(tex1, panel.style.BackgroundImage);

    }
    [Test]
    public void SetFontConstant() {
        const string template = @"
        <UITemplate>
            <Contents>
                <Panel style.textFontAsset=""$url('path/to/font1')""/>
            </Contents>
        </UITemplate>
        ";
        TMP_FontAsset font1 = Object.Instantiate(TMP_FontAsset.defaultFontAsset);
        font1.name = "new font";
        ResourceManager resourceManager = new ResourceManager();
        resourceManager.AddFont("path/to/font1", font1);
        MockApplication view = null;
        StyleTestThing root = (StyleTestThing) view.RootElement.GetChild(0);
        UIElement panel = root.FindFirstByType<UIElement>();
        view.Update();
        Assert.AreEqual(font1, panel.style.TextFontAsset.textMeshProFont);
        view.Update(); // should keep font after removing
        Assert.AreEqual(font1, panel.style.TextFontAsset.textMeshProFont);
        Object.DestroyImmediate(font1);
    }
    
    [Test]
    public void SetFontDynamic() {
        const string template = @"
        <UITemplate>
            <Contents>
                <Panel style.textFontAsset=""$url('path/to/' + fontName)""/>
            </Contents>
        </UITemplate>
        ";
        TMP_FontAsset font1 = Object.Instantiate(TMP_FontAsset.defaultFontAsset);
        font1.name = "new font";
        ResourceManager resourceManager = new ResourceManager();
        resourceManager.AddFont("path/to/font1", font1);
        MockApplication view = null;
        StyleTestThing root = (StyleTestThing) view.RootElement.GetChild(0);
        root.fontName = "font1";
        view.Update();
        UIElement panel = root.FindFirstByType<UIElement>();
        Assert.AreEqual(font1, panel.style.TextFontAsset.textMeshProFont);
        root.fontName = "not-there";
        view.Update();
        Assert.AreEqual(DefaultStyleValues_Generated.TextFontAsset.textMeshProFont, panel.style.TextFontAsset.textMeshProFont);
        root.fontName = "font1";
        view.Update();
        Assert.AreEqual(font1, panel.style.TextFontAsset.textMeshProFont);
        Object.DestroyImmediate(font1);

    }

}