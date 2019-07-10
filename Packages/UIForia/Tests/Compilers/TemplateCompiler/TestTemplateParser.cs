using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Mono.Linq.Expressions;
using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

[TestFixture]
public class TestTemplateParser {

    [Template(TemplateType.String, @"
    <UITemplate>
        <Content attr:stuff='yep'>

            <Div attr:id='hello0'/>

            <CompileTestChildElement attr:id='hello1' floatValue='4f'>

                <Div> some content </Div>

            </CompileTestChildElement>

            <CompileTestChildElement attr:id='hello2' floatValue='14f'/>

        </Content>
    </UITemplate>
    ")]
    public class CompileTestElement : UIElement { }

    [Template(TemplateType.String, @"
        <UITemplate>
        <Content attr:isChild='yep'>

           <Text>{floatValue}</Text>

        </Content>
        </UITemplate>
    ")]
    
    public class CompileTestChildElement : UIElement {

        public float floatValue;

    }

    [Test]
    public void ParseTemplate2() {
        
        MockApplication application = MockApplication.CreateWithoutView();
        
        TemplateCompiler compiler = new TemplateCompiler(application);
        
        CompiledTemplate compiledTemplate = compiler.GetCompiledTemplate(typeof(CompileTestElement));

        Func<UIElement, TemplateScope2, CompiledTemplate, UIElement> create = compiledTemplate.Compile();

        Debug.Log(PrintCode(compiledTemplate.buildExpression, false));

        UIElement r = create(null, new TemplateScope2() {
            application = application,
            bindingNode = new LinqBindingNode()
        }, compiledTemplate);

        Assert.IsInstanceOf<CompileTestElement>(r);
        Assert.AreEqual(3, r.children.size);
        Assert.IsInstanceOf<UIDivElement>(r.children[0]);
        Assert.IsInstanceOf<CompileTestChildElement>(r.children[1]);
        Assert.IsInstanceOf<CompileTestChildElement>(r.children[2]);
        Assert.AreEqual(1, r.attributes.size);
        Assert.AreEqual("stuff", r.attributes[0].name);
        Assert.AreEqual("yep", r.attributes[0].value);
        Assert.AreEqual(1, r.children[0].attributes.size);
        Assert.AreEqual(2, r.children[1].attributes.size);
        Assert.AreEqual(2, r.children[2].attributes.size);
        
        Assert.AreEqual("id", r.children[0].attributes[0].name);
        Assert.AreEqual("hello0", r.children[0].attributes[0].value);
        
        Assert.AreEqual("id", r.children[1].attributes[0].name);
        Assert.AreEqual("hello1", r.children[1].attributes[0].value);
        Assert.AreEqual("isChild", r.children[1].attributes[1].name);
        Assert.AreEqual("yep", r.children[1].attributes[1].value);
        
        Assert.AreEqual("id", r.children[2].attributes[0].name);
        Assert.AreEqual("hello2", r.children[2].attributes[0].value);
        Assert.AreEqual("isChild", r.children[2].attributes[1].name);
        Assert.AreEqual("yep", r.children[2].attributes[1].value);

        UIElement element = application.CreateElementFromPool(typeof(CompileTestElement));
        create(element, new TemplateScope2() {
            application = application,
            bindingNode = new LinqBindingNode(),
        }, compiledTemplate);

        // application.CreateTemplate(templateId, element, bindingNode);
        // 
        
    }
    

    private static string PrintCode(IList<Expression> expressions, bool printNamespaces = true) {
        string retn = "";
        bool old = CSharpWriter.printNamespaces;
        CSharpWriter.printNamespaces = printNamespaces;
        for (int i = 0; i < expressions.Count; i++) {
            retn += expressions[i].ToCSharpCode();
            if (i != expressions.Count - 1) {
                retn += "\n";
            }
        }

        CSharpWriter.printNamespaces = old;
        return retn;
    }

    private static string PrintCode(Expression expression, bool printNamespaces = true) {
        bool old = CSharpWriter.printNamespaces;
        CSharpWriter.printNamespaces = printNamespaces;
        string retn =expression.ToCSharpCode();
        CSharpWriter.printNamespaces = old;
        return retn;
    }

    private static void LogCode(Expression expression, bool printNamespaces = true) {
        bool old = CSharpWriter.printNamespaces;
        CSharpWriter.printNamespaces = printNamespaces;
        string retn = expression.ToCSharpCode();
        CSharpWriter.printNamespaces = old;
        Debug.Log(retn);
    }

}