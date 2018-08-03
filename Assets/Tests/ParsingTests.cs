using UnityEngine;
using NUnit.Framework;
using System.IO;
using Src;

[TestFixture]
public class ParsingTests {
    [Test]
    public void ParseThreeSelfClosingChildren() {
        string template = File.ReadAllText(Application.dataPath + "/Tests/Test1.xml");
        ParsedTemplate parsedTemplate = TemplateParser.ParseTemplate(template);
        Assert.IsNotNull(parsedTemplate);
        Assert.AreEqual(3, parsedTemplate.rootElementTemplate.children.Count);
    }

    [Test]
    public void ParseThreeNonSelfClosingChildren() {
        string template = File.ReadAllText(Application.dataPath + "/Tests/Test2.xml");
        ParsedTemplate parsedTemplate = TemplateParser.ParseTemplate(template);
        Assert.IsNotNull(parsedTemplate);
        Assert.AreEqual(3, parsedTemplate.rootElementTemplate.children.Count);
        Assert.AreEqual(1, parsedTemplate.rootElementTemplate.children[0].children.Count);
        Assert.AreEqual(1, parsedTemplate.rootElementTemplate.children[1].children.Count);
        Assert.AreEqual(1, parsedTemplate.rootElementTemplate.children[2].children.Count);
    }

    [Test]
    public void ParseTextChildren() {
        string template = File.ReadAllText(Application.dataPath + "/Tests/Test2.xml");
        ParsedTemplate parsedTemplate = TemplateParser.ParseTemplate(template);
        Assert.IsNotNull(parsedTemplate);
        Assert.AreEqual(1, parsedTemplate.rootElementTemplate.children[0].children.Count);
        Assert.AreEqual(1, parsedTemplate.rootElementTemplate.children[1].children.Count);
        Assert.AreEqual(1, parsedTemplate.rootElementTemplate.children[2].children.Count);
        Assert.AreEqual("Text1", parsedTemplate.rootElementTemplate.children[0].children[0].text);
        Assert.AreEqual("Text2", parsedTemplate.rootElementTemplate.children[1].children[0].text);
        Assert.AreEqual("Text3", parsedTemplate.rootElementTemplate.children[2].children[0].text);
    }

    [Test]
    public void GenerateUIElements() {
        string template = File.ReadAllText(Application.dataPath + "/Tests/Test2.xml");
        ParsedTemplate parsedTemplate = TemplateParser.ParseTemplate(template);
        UIElement output = parsedTemplate.Instantiate();
        Assert.AreEqual(output.children.Length, 3);
        Assert.IsInstanceOf<UIPanel>(output.children[0]);
        Assert.IsInstanceOf<UIPanel>(output.children[1]);
        Assert.IsInstanceOf<UIPanel>(output.children[2]);
    }

    [Test]
    public void GenerateUIElementsWithObservedProperties() {
        string template = File.ReadAllText(Application.dataPath + "/Tests/Test3.xml");
        ParsedTemplate parsedTemplate = TemplateParser.ParseTemplate(template);
        UIElement output = parsedTemplate.Instantiate();
        Assert.IsNotNull(output);
        Assert.IsInstanceOf<Spec.Test3>(output);
        Spec.Test3 castElement = output as Spec.Test3;
        Assert.IsNotNull(castElement);
        Assert.IsNotNull(castElement.floatProperty);
    }

    [Test]
    public void CreateGameObjects() {
        string template = File.ReadAllText(Application.dataPath + "/Tests/Test2.xml");
        ParsedTemplate parsedTemplate = TemplateParser.ParseTemplate(template);
        UIElement output = parsedTemplate.Instantiate();
        Assert.IsNotNull(output.gameObject);
        Assert.AreEqual(output.children[0].gameObject.transform.parent, output.gameObject.transform);
        Assert.AreEqual(output.children[1].gameObject.transform.parent, output.gameObject.transform);
        Assert.AreEqual(output.children[2].gameObject.transform.parent, output.gameObject.transform);
    }
    
}