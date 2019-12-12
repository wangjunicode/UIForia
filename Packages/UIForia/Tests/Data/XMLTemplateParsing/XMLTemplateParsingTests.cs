using System;
using System.IO;
using NUnit.Framework;
using UIForia;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using Application = UnityEngine.Application;

namespace TemplateParsing_XML {

    public class TemplateParsing_XMLTests {

        public TemplateSettings Setup(string appName) {
            TemplateSettings settings = new TemplateSettings();
            settings.applicationName = appName;
            settings.assemblyName = GetType().Assembly.GetName().Name;
            settings.outputPath = Path.Combine(Application.dataPath, "..", "Packages", "UIForia", "Tests", "UIForiaGenerated");
            settings.codeFileExtension = "cs";
            settings.preCompiledTemplatePath = "Assets/UIForia_Generated/" + appName;
            settings.templateResolutionBasePath = Path.Combine(Application.dataPath, "..", "Packages", "UIForia", "Tests");
            return settings;
        }

        [Template("Data/XMLTemplateParsing/XMLTemplateParsing_CollapseTextNode_Simple.xml")]
        public class XMLTemplateParsing_CollapseTextNode_Simple : UIElement { }

        [Test]
        public void CollapseTextNode_Simple() {
            TemplateCache cache = new TemplateCache(Setup("App"));
            RootTemplateNode templateRoot = cache.GetParsedTemplate(TypeProcessor.GetProcessedType(typeof(XMLTemplateParsing_CollapseTextNode_Simple)));

            Assert.AreEqual(1, templateRoot.ChildCount);

            ContainerNode child = AssertAndReturn<ContainerNode>(templateRoot[0]);
            TextNode text = AssertAndReturn<TextNode>(child[0]);

            Assert.AreEqual("Hello Templates", text.textExpressionList[0].text);
        }

        [Template("Data/XMLTemplateParsing/XMLTemplateParsing_CollapseTextNode_Complex.xml")]
        public class XMLTemplateParsing_CollapseTextNode_Complex : UIElement { }

        [Test]
        public void CollapseTextNode_Complex() {
            TemplateCache cache = new TemplateCache(Setup("App"));
            RootTemplateNode templateRoot = cache.GetParsedTemplate(TypeProcessor.GetProcessedType(typeof(XMLTemplateParsing_CollapseTextNode_Complex)));

            Assert.AreEqual(1, templateRoot.ChildCount);

            ContainerNode child = AssertAndReturn<ContainerNode>(templateRoot[0]);
            TextNode text = AssertAndReturn<TextNode>(child[0]);

            Assert.AreEqual(2, text.ChildCount);
            Assert.AreEqual("Hello", text.textExpressionList[0].text.Trim());

            TerminalNode terminalNode = AssertAndReturn<TerminalNode>(text[0]);
            TextNode subText = AssertAndReturn<TextNode>(text[1]);
            Assert.AreEqual("Templates", subText.textExpressionList[0].text.Trim());
        }

        [Template("Data/XMLTemplateParsing/XMLTemplateParsing_DefineSlot.xml")]
        public class XMLTemplateParsing_DefineSlot : UIElement { }

        [Test]
        public void DefineSlot() {
            TemplateCache cache = new TemplateCache(Setup("App"));
            RootTemplateNode templateRoot = cache.GetParsedTemplate(TypeProcessor.GetProcessedType(typeof(XMLTemplateParsing_DefineSlot)));

            Assert.AreEqual(1, templateRoot.ChildCount);

            ContainerNode child = AssertAndReturn<ContainerNode>("Div", templateRoot[0]);
            SlotDefinitionNode definitionNode = AssertAndReturn<SlotDefinitionNode>(child[0]);

            Assert.AreEqual("my-slot", definitionNode.slotName);
        }

        [Template("Data/XMLTemplateParsing/XMLTemplateParsing_DefineSlotNameTwice.xml")]
        public class XMLTemplateParsing_DefineSlotNameTwice : UIElement { }

        [Test]
        public void DefineSlotNameTwice() {
            ProcessedType processedType = TypeProcessor.GetProcessedType(typeof(XMLTemplateParsing_DefineSlotNameTwice));
            TemplateCache cache = new TemplateCache(Setup("App"));
            ParseException parseException = Assert.Throws<ParseException>(() => { cache.GetParsedTemplate(processedType); });
            Assert.AreEqual(ParseException.MultipleSlotsWithSameName(cache.GetTemplateFilePath(processedType), "my-slot").Message, parseException.Message);
        }

        [Template("Data/XMLTemplateParsing/XMLTemplateParsing_OverrideSlot.xml")]
        public class XMLTemplateParsing_OverrideSlot : UIElement { }
        
        [Test]
        public void OverrideSlot() {

            ProcessedType processedType = TypeProcessor.GetProcessedType(typeof(XMLTemplateParsing_OverrideSlot));
            TemplateCache cache = new TemplateCache(Setup("App"));
            RootTemplateNode templateRoot = cache.GetParsedTemplate(processedType);

            Assert.AreEqual(3, templateRoot.ChildCount);

            AssertTrimmedText("Hello Before", templateRoot[0]);
            AssertTrimmedText("Hello After", templateRoot[2]);

            ExpandedTemplateNode expandedTemplateNode = AssertAndReturn<ExpandedTemplateNode>(templateRoot[1]);
            SlotOverrideNode overrideNode = AssertAndReturn<SlotOverrideNode>(expandedTemplateNode[0]);
            Assert.AreEqual("my-slot", overrideNode.slotName);
            AssertTrimmedText("Hello Between", overrideNode[0]);
        }

        [Template("Data/XMLTemplateParsing/XMLTemplateParsing_ExpandTemplate.xml")]
        public class XMLTemplateParsing_ExpandTemplate : UIElement { }

        [Template("Data/XMLTemplateParsing/XMLTemplateParsing_ExpandedTemplateChild.xml")]
        public class XMLTemplateParsing_ExpandedTemplateChild : UIElement { }

        [Test]
        public void ExpandedTemplate() {
            ProcessedType processedType = TypeProcessor.GetProcessedType(typeof(XMLTemplateParsing_ExpandTemplate));
            TemplateCache cache = new TemplateCache(Setup("App"));
            RootTemplateNode templateRoot = cache.GetParsedTemplate(processedType);

            Assert.AreEqual(3, templateRoot.ChildCount);

            AssertAndReturn<TextNode>(templateRoot[0]);

            ExpandedTemplateNode expandedTemplate = AssertAndReturn<ExpandedTemplateNode>(templateRoot[1]);

            RootTemplateNode expandedRoot = expandedTemplate.expandedRoot;
            AssertTrimmedText("I am expanded!", expandedRoot[0]);
            
            AssertAndReturn<TextNode>(templateRoot[2]);
        }

        private static void AssertText(string expected, TemplateNode2 templateNode) {
            TextNode textNode = AssertAndReturn<TextNode>(templateNode);
            Assert.AreEqual(expected, textNode.rawTextContent);
        }
        
        private static void AssertTrimmedText(string expected, TemplateNode2 templateNode) {
            TextNode textNode = AssertAndReturn<TextNode>(templateNode);
            Assert.AreEqual(expected, textNode.rawTextContent.Trim());
        }
        
        private static T AssertAndReturn<T>(object b) where T : TemplateNode2 {
            Assert.IsInstanceOf<T>(b);
            return (T) b;
        }

        private static T AssertAndReturn<T>(string tagName, object b) where T : TemplateNode2 {
            Assert.IsInstanceOf<T>(b);
            T a = (T) b;
            if (!string.IsNullOrEmpty(a.namespaceName)) {
                Assert.AreEqual(tagName, a.namespaceName + ":" + a.tagName);
            }
            else {
                Assert.AreEqual(tagName, a.tagName);
            }

            return a;
        }

    }

}