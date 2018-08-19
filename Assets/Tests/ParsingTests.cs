using Src;
using NUnit.Framework;

[TestFixture]
public class ParsingTests {

    [Test]
    public void ParseTemplateParts() {
        TemplateParser.GetParsedTemplate(typeof(Spec.Temp));
    }

    [Test]
    public void ParseThreeSelfClosingChildren() {
        ParsedTemplate parsedTemplate = TemplateParser.GetParsedTemplate(typeof(Spec.Test1));
        Assert.IsNotNull(parsedTemplate);
        Assert.AreEqual(3, parsedTemplate.childTemplates.Count);
        Assert.AreEqual(typeof(UIPanel), parsedTemplate.childTemplates[0].ElementType);
        Assert.AreEqual(typeof(UIPanel), parsedTemplate.childTemplates[1].ElementType);
        Assert.AreEqual(typeof(UIPanel), parsedTemplate.childTemplates[2].ElementType);
    }

    [Test]
    public void ParseThreeNonSelfClosingChildren() {
        ParsedTemplate parsedTemplate = TemplateParser.GetParsedTemplate(typeof(Spec.Test2));
        Assert.AreEqual(3, parsedTemplate.childTemplates.Count);
        Assert.AreEqual(typeof(UIPanel), parsedTemplate.childTemplates[0].ElementType);
        Assert.AreEqual(typeof(UIPanel), parsedTemplate.childTemplates[1].ElementType);
        Assert.AreEqual(typeof(UIPanel), parsedTemplate.childTemplates[2].ElementType);
    }

    [Test]
    public void ParseTextAtRootLevel() {
        ParsedTemplate parsedTemplate = TemplateParser.GetParsedTemplate(typeof(Spec.Test3));
        Assert.IsNotNull(parsedTemplate);
        Assert.AreEqual(typeof(UITextElement), parsedTemplate.childTemplates[0].ElementType);
    }

    [Test]
    public void Children_ParsesCorrectly() {
        ParsedTemplate parsedTemplate = TemplateParser.ParseTemplateFromString<Spec.Test1>(@"
            <UITemplate>
                <Contents>
                    <Children/>
                </Contents>
            </UITemplate>
        ");
        Assert.IsInstanceOf<UIChildrenTemplate>(parsedTemplate.childTemplates[0]);
    }

    [Test]
    public void Children_CannotAppearInsideRepeat() {
        var x = Assert.Throws<InvalidTemplateException>(() => {
            TemplateParser.ParseTemplateFromString<Spec.Test1>(@"
                <UITemplate>
                    <Contents>
                        <Repeat list='{null}'>
                            <Children/>
                        </Repeat>
                    </Contents>
                </UITemplate>
            ");
        });
        Assert.AreEqual("<Children> cannot be inside <Repeat>", x.Message);
    }
    
    [Test]
    public void Children_MustBeEmpty() {
        var x = Assert.Throws<InvalidTemplateException>(() => {
            TemplateParser.ParseTemplateFromString<Spec.Test1>(@"
                <UITemplate>
                    <Contents>
                            <Children>text</Children>
                    </Contents>
                </UITemplate>
            ");
        });
        Assert.AreEqual("<Children> tags cannot have children", x.Message);
    }

    [Test]
    public void Switch_CanOnlyContainCaseAndDefault() {
        var x = Assert.Throws<InvalidTemplateException>(() => {
            TemplateParser.ParseTemplateFromString<Spec.Test1>(@"
                <UITemplate>
                    <Contents>
                        <Switch value='1'>
                            text                                        
                        </Switch>
                    </Contents>
                </UITemplate>
            ");
        });
        Assert.AreEqual("<Switch> can only contain <Case> and <Default> elements", x.Message);
    }

    [Test]
    public void Switch_CanOnlyContainOneDefault() {
        var x = Assert.Throws<InvalidTemplateException>(() => {
            TemplateParser.ParseTemplateFromString<Spec.Test1>(@"
                <UITemplate>
                    <Contents>
                        <Switch value='1'>
                            <Default>text1</Default>                                        
                            <Default>text2</Default>                                        
                        </Switch>
                    </Contents>
                </UITemplate>
            ");
        });
        Assert.AreEqual("<Switch> can only contain one <Default> element", x.Message);
    }

    [Test]
    public void Switch_CanContainOnlyCases() {
        Assert.DoesNotThrow(() => {
            TemplateParser.ParseTemplateFromString<Spec.Test1>(@"
                <UITemplate>
                    <Contents>
                        <Switch value='1'>
                            <Case when='1'>text1</Case>                                        
                            <Case when='2'>text2</Case>                                        
                        </Switch>
                    </Contents>
                </UITemplate>
            ");
        });
    }

    [Test]
    public void Switch_CanContainOnlyDefault() {
        Assert.DoesNotThrow(() => {
            TemplateParser.ParseTemplateFromString<Spec.Test1>(@"
                <UITemplate>
                    <Contents>
                        <Switch value='1'>
                            <Default>text1</Default>                                        
                        </Switch>
                    </Contents>
                </UITemplate>
            ");
        });
    }

    [Test]
    public void Switch_CannotBeEmpty() {
        var x = Assert.Throws<InvalidTemplateException>(() => {
            TemplateParser.ParseTemplateFromString<Spec.Test1>(@"
                <UITemplate>
                    <Contents>
                        <Switch value='1'>
                                                                  
                        </Switch>
                    </Contents>
                </UITemplate>
            ");
        });
        Assert.AreEqual("<Switch> cannot be empty", x.Message);
    }

    [Test]
    public void Case_MustHaveWhenAttribute() {
        var x = Assert.Throws<InvalidTemplateException>(() => {
            TemplateParser.ParseTemplateFromString<Spec.Test1>(@"
                <UITemplate>
                    <Contents>
                        <Switch value='1'>
                            <Case>text1</Case>                                        
                            <Case when='2'>text2</Case>                                        
                        </Switch>
                    </Contents>
                </UITemplate>
            ");
        });
        Assert.AreEqual("<Case> is missing required attribute 'when'", x.Message);
    }

    [Test]
    public void Prefab_MustBeEmpty() {
        var x = Assert.Throws<InvalidTemplateException>(() => {
            TemplateParser.ParseTemplateFromString<Spec.Test1>(@"
                <UITemplate>
                    <Contents>
                            <Prefab>text</Prefab>
                    </Contents>
                </UITemplate>
            ");
        });
        Assert.AreEqual("<Prefab> tags cannot have children", x.Message);
    }
    
}

//
//    [Test]
//    public void GenerateUIElements() {
//        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate(typeof(Spec.Test5));
//        UIElement root = parsedTemplate.CreateElement();
//        Assert.AreEqual(root.children.Length, 3);
//        
//        Assert.IsInstanceOf<UITextElement>(root.children[0]);
//        Assert.IsInstanceOf<UIPanel>(root.children[1]);
//        Assert.IsInstanceOf<UITextElement>(root.children[2]);
//        
//        Assert.AreEqual(root.children[1].children.Length, 1);
//        Assert.IsInstanceOf<UITextElement>(root.children[1].children[0]);
//    }
//    
//    [Test]
//    public void GenerateUIElements_DoubleNested() {
//        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Test6>();
//        UIElement root = parsedTemplate.CreateElement();
//        Assert.AreEqual(root.children.Length, 3);
//        
//        Assert.IsInstanceOf<UITextElement>(root.children[0]);
//        Assert.IsInstanceOf<UIPanel>(root.children[1]);
//        Assert.IsInstanceOf<UITextElement>(root.children[2]);
//
//        UIPanel panel = root.children[1] as UIPanel;
//        Assert.AreEqual(panel.children.Length, 3);
//        Assert.IsInstanceOf<UITextElement>(panel.children[0]);
//        Assert.IsInstanceOf<UIPanel>(panel.children[1]);
//        Assert.IsInstanceOf<UITextElement>(panel.children[2]);
//
//        UIPanel innerPanel = panel.children[1] as UIPanel;
//        Assert.AreEqual(innerPanel.children.Length, 1);
//        Assert.IsInstanceOf<UITextElement>(innerPanel.children[0]);
//
//    }
//
//    [Test]
//    public void PassChildrenInToCreate() {
//        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Test7>();
//        UITextElement text = new UITextElement();
//        UIElement[] children = { text };
//        UIElement root = parsedTemplate.CreateElement(children);
//        Assert.AreEqual(2, root.children.Length);
//        Assert.IsInstanceOf<UITextElement>(root.children[0]);
//        Assert.IsInstanceOf<UITextElement>(root.children[1]);
//    }
//
//    [Test] // todo -- maybe throw an error in this case
//    public void PassChildrenToTemplateWithoutSlotDefined() {
//        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Test2>();
//        UITextElement text = new UITextElement();
//        UIElement[] children = { text };
//        UIElement root = parsedTemplate.CreateElement(children);
//        Assert.AreEqual(3, root.children.Length);
//        Assert.AreEqual(1, root.children[0].children.Length);
//        Assert.AreEqual(1, root.children[1].children.Length);
//        Assert.AreEqual(1, root.children[2].children.Length);
//    }
//
//    [Test]
//    public void MultipleChildrenSlotsShouldError() {
//        Assert.Throws<MultipleChildSlotException>(() => {
//            TemplateParser.GetParsedTemplate<Spec.Test8>();
//        });
//    }
//    
//    [Test]
//    public void ChildrenSlotWithChildrenShouldError() {
//        Assert.Throws<ChildrenSlotWithChildrenException>(() => {
//            TemplateParser.GetParsedTemplate<Spec.Test11>();
//        });
//    }
//
//    [Test]
//    public void TemplateMustHaveAContentsSection() {
//        Assert.Throws<InvalidTemplateException>(() => {
//            TemplateParser.GetParsedTemplate<Spec.Test12>();
//        });
//    }
//    
//    [Test]
//    public void AllowChildrenSlotToBeOnlyElement() {
//        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Test9>();
//        UITextElement text1 = new UITextElement();
//        UITextElement text2 = new UITextElement();
//        UIElement[] children = { text1, text2};
//        UIElement root = parsedTemplate.CreateElement(children);
//        Assert.AreEqual(2, root.children.Length);
//    }
//
//    [Test]
//    public void AllowChildrenSlotInNestedElement() {
//        UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Test10>();
//        UITextElement text1 = new UITextElement();
//        UITextElement text2 = new UITextElement();
//        UIElement[] children = { text1, text2};
//        UIElement root = parsedTemplate.CreateElement(children);
//        Assert.AreEqual(3, root.children.Length);
//        UIElement panel = root.children[1];
//        Assert.AreEqual(2, panel.children.Length);
//        Assert.AreEqual(text1, panel.children[0]);
//        Assert.AreEqual(text2, panel.children[1]);
//    }
//
//
//    // template not found for type
//    // type not found for tag name
//   
//}