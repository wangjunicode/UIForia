using Src;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class ParsingTests {

    [Test]
    public void ParseThreeSelfClosingChildren() {
        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate(typeof(Spec.Test1));
        Assert.IsNotNull(parsedTemplate);
        Assert.AreEqual(3, parsedTemplate.children.Count);
    }

    [Test]
    public void ParseThreeNonSelfClosingChildren() {
        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate(typeof(Spec.Test2));
        Assert.IsNotNull(parsedTemplate);
        Assert.AreEqual(3, parsedTemplate.children.Count);
        Assert.AreEqual(1, parsedTemplate.children[0].children.Count);
        Assert.AreEqual(1, parsedTemplate.children[1].children.Count);
        Assert.AreEqual(1, parsedTemplate.children[2].children.Count);
    }

    [Test]
    public void ParseTextChildren() {
        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate(typeof(Spec.Test2));
        Assert.IsNotNull(parsedTemplate);
        Assert.AreEqual(1, parsedTemplate.children[0].children.Count);
        Assert.AreEqual(1, parsedTemplate.children[1].children.Count);
        Assert.AreEqual(1, parsedTemplate.children[2].children.Count);
        Assert.AreEqual("Text1", parsedTemplate.children[0].children[0].text);
        Assert.AreEqual("Text2", parsedTemplate.children[1].children[0].text);
        Assert.AreEqual("Text3", parsedTemplate.children[2].children[0].text);
    }

    [Test]
    public void GenerateUIElements() {
        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate(typeof(Spec.Test5));
        UIElement root = parsedTemplate.CreateElement();
        Assert.AreEqual(root.children.Length, 3);
        
        Assert.IsInstanceOf<UITextElement>(root.children[0]);
        Assert.IsInstanceOf<UIPanel>(root.children[1]);
        Assert.IsInstanceOf<UITextElement>(root.children[2]);
        
        Assert.AreEqual(root.children[1].children.Length, 1);
        Assert.IsInstanceOf<UITextElement>(root.children[1].children[0]);
    }
    
    [Test]
    public void GenerateUIElements_DoubleNested() {
        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Test6>();
        UIElement root = parsedTemplate.CreateElement();
        Assert.AreEqual(root.children.Length, 3);
        
        Assert.IsInstanceOf<UITextElement>(root.children[0]);
        Assert.IsInstanceOf<UIPanel>(root.children[1]);
        Assert.IsInstanceOf<UITextElement>(root.children[2]);

        UIPanel panel = root.children[1] as UIPanel;
        Assert.AreEqual(panel.children.Length, 3);
        Assert.IsInstanceOf<UITextElement>(panel.children[0]);
        Assert.IsInstanceOf<UIPanel>(panel.children[1]);
        Assert.IsInstanceOf<UITextElement>(panel.children[2]);

        UIPanel innerPanel = panel.children[1] as UIPanel;
        Assert.AreEqual(innerPanel.children.Length, 1);
        Assert.IsInstanceOf<UITextElement>(innerPanel.children[0]);

    }

    [Test]
    public void PassChildrenInToCreate() {
        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Test7>();
        UITextElement text = new UITextElement();
        UIElement[] children = { text };
        UIElement root = parsedTemplate.CreateElement(children);
        Assert.AreEqual(2, root.children.Length);
        Assert.IsInstanceOf<UITextElement>(root.children[0]);
        Assert.IsInstanceOf<UITextElement>(root.children[1]);
    }

    [Test] // todo -- maybe throw an error in this case
    public void PassChildrenToTemplateWithoutSlotDefined() {
        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Test2>();
        UITextElement text = new UITextElement();
        UIElement[] children = { text };
        UIElement root = parsedTemplate.CreateElement(children);
        Assert.AreEqual(3, root.children.Length);
        Assert.AreEqual(1, root.children[0].children.Length);
        Assert.AreEqual(1, root.children[1].children.Length);
        Assert.AreEqual(1, root.children[2].children.Length);
    }

    [Test]
    public void MultipleChildrenSlotsShouldError() {
        Assert.Throws<MultipleChildSlotException>(() => {
            TemplateParser.GetParsedTemplate<Spec.Test8>();
        });
    }
    
    [Test]
    public void ChildrenSlotWithChildrenShouldError() {
        Assert.Throws<ChildrenSlotWithChildrenException>(() => {
            TemplateParser.GetParsedTemplate<Spec.Test11>();
        });
    }

    [Test]
    public void TemplateMustHaveAContentsSection() {
        Assert.Throws<InvalidTemplateException>(() => {
            TemplateParser.GetParsedTemplate<Spec.Test12>();
        });
    }
    
    [Test]
    public void AllowChildrenSlotToBeOnlyElement() {
        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Test9>();
        UITextElement text1 = new UITextElement();
        UITextElement text2 = new UITextElement();
        UIElement[] children = { text1, text2};
        UIElement root = parsedTemplate.CreateElement(children);
        Assert.AreEqual(2, root.children.Length);
    }

    [Test]
    public void AllowChildrenSlotInNestedElement() {
        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Test10>();
        UITextElement text1 = new UITextElement();
        UITextElement text2 = new UITextElement();
        UIElement[] children = { text1, text2};
        UIElement root = parsedTemplate.CreateElement(children);
        Assert.AreEqual(3, root.children.Length);
        UIElement panel = root.children[1];
        Assert.AreEqual(2, panel.children.Length);
        Assert.AreEqual(text1, panel.children[0]);
        Assert.AreEqual(text2, panel.children[1]);
    }


    // template not found for type
    // type not found for tag name
   
}