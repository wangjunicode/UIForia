﻿using NUnit.Framework;
 using Tests.Mocks;
 using UIForia.Attributes;
 using UIForia.Elements;
 using UIForia.Exceptions;
 using UIForia.Parsing.Expression;
 using UIForia.Templates;
 using UnityEngine;

 [TestFixture]
public class TemplateParsingTests {

    [Template("Tests/Templates/Parsing/Test1.xml")]
    private class Test1 : UIElement { }

    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
            <ThingWithSlot>
                <SlotContent name='ThingWithSlot.Slot0'>
                    <Text x-id='contents'>should be slot child</Text>
                </SlotContent>
            </ThingWithSlot>
        </Contents>
    </UITemplate>
    ")]
    private class TestThing2 : UIElement { }

    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
            <ThingWithSlot/>
        </Contents>
    </UITemplate>
    ")]
    private class TestThing3 : UIElement { }
    
    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
           <Slot name='ThingWithSlot.Slot0'>
                <Text>default content</Text>
           </Slot>
        </Contents>
    </UITemplate>
    ")]
    private class ThingWithSlot : UIElement { }

    [Test]
    public void Children_ParsesCorrectly() {
        ParsedTemplate parsedTemplate = new TemplateParser(null).ParseTemplateFromString<Test1>(@"
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
        var x = Assert.Throws<TemplateParseException>(() => {
            new TemplateParser(null).ParseTemplateFromString<Test1>(@"
                <UITemplate>
                    <Contents>
                        <Repeat list='{null}'>
                            <Children/>
                        </Repeat>
                    </Contents>
                </UITemplate>
            ");
        });
        Assert.IsTrue(x.Message.Contains("<Children> cannot be inside <Repeat>"), "Expected a different error message :(");
    }

   
    [Test]
    public void Text_Parses() {
        Assert.DoesNotThrow(() => {
            new TemplateParser(null).ParseTemplateFromString<Test1>(@"
                <UITemplate>
                    <Contents>
                            <Group>text</Group>
                    </Contents>
                </UITemplate>
            ");
        });
    }

    [Test]
    public void Text_AssignsRawString() {
        ParsedTemplate parsedTemplate = new TemplateParser(null).ParseTemplateFromString<Test1>(@"
                <UITemplate>
                    <Contents>
                            <Group>text {value} is here</Group>
                    </Contents>
                </UITemplate>
            ");
        UITextTemplate template = (UITextTemplate) parsedTemplate.childTemplates[0].childTemplates[0];
        Assert.IsNotNull(template);
        Assert.AreEqual("'text {value} is here'", template.RawText);
    }

    [Test]
    public void Repeat_CanNest() {
        Assert.DoesNotThrow(() => {
            new TemplateParser(null).ParseTemplateFromString<Test1>(@"
                <UITemplate>
                    <Contents>
                       <Repeat list='{something}'>
                            <Repeat list='{other}' as='thing'>
                                <Group>text</Group>                            
                            </Repeat>
                        </Repeat>
                    </Contents>
                </UITemplate>
            ");
        });
    }

    [Test]
    public void Slot_CanParse() {
        ParsedTemplate parsedTemplate = new TemplateParser(null).ParseTemplateFromString<Test1>(@"
                <UITemplate>
                    <Contents>
                        <Group>
                            <Slot name=""slot0""/>
                        </Group>
                    </Contents>
                </UITemplate>
            ");
        Assert.IsNotNull(parsedTemplate);
    }

    [Test]
    public void SlotContent_CanParse() {
        ParsedTemplate parsedTemplate = new TemplateParser(null).ParseTemplateFromString<Test1>(@"
                <UITemplate>
                    <Contents>
                        <Group>
                            <SlotContent name=""slot0""/>
                        </Group>
                    </Contents>
                </UITemplate>
            ");
        Assert.IsNotNull(parsedTemplate);
    }

    [Test]
    public void SlotContent_UsesAssignedSlotContentOnMatch() {
        MockApplication app = new MockApplication(typeof(TestThing2));
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        ThingWithSlot slotThing = app.RootElement.GetChild(0).FindFirstByType<ThingWithSlot>();
        UITextElement textElement = slotThing.FindFirstByType<UITextElement>();
        Assert.IsNotNull(textElement);
        Assert.AreEqual("should be slot child", textElement.text);
    }
    
    [Test]
    public void SlotContent_UsesDefaultSlotContentOnNoMatch() {
        MockApplication app = new MockApplication(typeof(TestThing3));
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        ThingWithSlot slotThing = app.RootElement.GetChild(0).FindFirstByType<ThingWithSlot>();
        UITextElement textElement = slotThing.FindFirstByType<UITextElement>();
        Assert.IsNotNull(textElement);
        Assert.AreEqual("default content", textElement.text);
    }

    [Test]
    public void Text_ParsesExpressionParts() {
        TextElementParser parser = new TextElementParser();
        string[] output1 = parser.Parse("'one expression'");
        Assert.AreEqual(1, output1.Length);
        Assert.AreEqual("'one expression'", output1[0]);

        string[] output2 = parser.Parse("'two {expressions}'");
        Assert.AreEqual(2, output2.Length);
        Assert.AreEqual("'two '", output2[0]);
        Assert.AreEqual("{expressions}", output2[1]);

        string[] output3 = parser.Parse("'three {expressions} here'");
        Assert.AreEqual(3, output3.Length);
        Assert.AreEqual("'three '", output3[0]);
        Assert.AreEqual("{expressions}", output3[1]);
        Assert.AreEqual("' here'", output3[2]);
    }

}