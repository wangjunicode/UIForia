using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
using Mono.Linq.Expressions;
using NUnit.Framework;
using Tests;
using Tests.Mocks;
using UIForia;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Parsing.Expression;
using UnityEngine;
using Application = UIForia.Application;

[TestFixture]
public class TestTemplateParser {

    [Test]
    public void ParseTemplate() {
        
        NameTable nameTable = new NameTable();
        
        XmlNamespaceManager nameSpaceManager = new XmlNamespaceManager(nameTable);
        
        nameSpaceManager.AddNamespace("attr", "attr");
        nameSpaceManager.AddNamespace("evt", "evt");
        
        XmlParserContext parserContext = new XmlParserContext(null, nameSpaceManager, null, XmlSpace.None);
        
        XmlTextReader txtReader = new XmlTextReader(@"<Contents><X/><Thing attr:thing=""someattr""/></Contents>", XmlNodeType.Element, parserContext);
        
        XElement elem = XElement.Load(txtReader);
        
        Assert.AreEqual("thing", (elem.FirstNode as XElement).FirstAttribute.Name.LocalName);
        Assert.AreEqual("attr", (elem.FirstNode as XElement).FirstAttribute.Name.NamespaceName);
    }

    [Test]
    public void CompileTemplate() {
        
        XMLTemplateParser parser = new XMLTemplateParser(new MockApplication(typeof(InputSystem_DragTests.DragTestThing)));
        parser.Parse(@"
            <UITemplate>
                <Content>
                    <Thing thing=""someattr""/>
                </Content>
            </UITemplate>
        ");
        
        TemplateCompiler compiler = new TemplateCompiler();
        
    }

    private class CompileTestElement : UIElement { }
    private class CompileTestChildElement : UIElement { }

    [Test]
    public void CompileTemplate_GenerateAttributes() {
        TemplateCompiler compiler = new TemplateCompiler();

        compiler.application = MockApplication.CreateWithoutView();

        CompiledTemplate result = compiler.Compile(new TemplateAST() {
            root = new TemplateNode() {
                typeLookup = new TypeLookup("CompileTestChildElement"),
                attributes = new [] {
                    new AttributeDefinition2(AttributeType.Attribute, "someAttr", "someAttrValue"),
                    new AttributeDefinition2(AttributeType.Attribute, "someAttr1", "someAttrValue1"),
                    new AttributeDefinition2(AttributeType.Attribute, "someAttr2", "someAttrValue2"),
                },
                children = new[] {
                    new TemplateNode() {
                        typeLookup = new TypeLookup("CompileTestChildElement"),
                        attributes = new [] {
                            new AttributeDefinition2(AttributeType.Attribute, "someAttr", "someAttrValue"),
                            new AttributeDefinition2(AttributeType.Attribute, "someAttr1", "someAttrValue1"),
                            new AttributeDefinition2(AttributeType.Attribute, "someAttr2", "someAttrValue2"),
                        },
                    }
                }
            }
        });
        
        LogCode(result.buildExpression);
        
    }
    
    private static string PrintCode(IList<Expression> expressions) {
        string retn = "";
        for (int i = 0; i < expressions.Count; i++) {
            retn += expressions[i].ToCSharpCode();
            if (i != expressions.Count - 1) {
                retn += "\n";
            }
        }

        return retn;
    }

    private static string PrintCode(Expression expression) {
        return expression.ToCSharpCode();
    }
    
    private static void LogCode(Expression expression) {
        Debug.Log(expression.ToCSharpCode());
    }

}