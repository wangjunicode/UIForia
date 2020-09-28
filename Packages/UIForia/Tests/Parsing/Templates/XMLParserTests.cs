using System.IO;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using UIForia;
using UIForia.Parsing;

namespace Tests.Parsing {

    public class XMLParserTests {

        [Test]
        public void ParseBasicTemplate() {
            TemplateParserXML2 parser = new TemplateParserXML2();

            string fileName = Path.GetDirectoryName(GetFilePath()) + "\\TestXMLParser_BasicTemplate.xml";

            Diagnostics diagnostics = new Diagnostics();
            parser.Setup(fileName, diagnostics);
            parser.TryParse(fileName, out TemplateFileShell fileShell);
            
            if (diagnostics.HasErrors()) {
                diagnostics.Dump();
                Assert.IsTrue(false);
            }

            TemplateEditor editor = new TemplateEditor(fileShell);
            TemplateEditorRootNode templateRoot = editor.GetTemplateRoot();
            
            Assert.AreEqual(templateRoot.ChildCount, 1);
            Assert.AreEqual(templateRoot.GetChild(0).ChildCount, 3);
            Assert.AreEqual(templateRoot.GetChild(0).GetChild(0).tagName, "Group");
            Assert.AreEqual(templateRoot.GetChild(0).GetChild(1).tagName, "Group");
            Assert.AreEqual(templateRoot.GetChild(0).GetChild(2).tagName, "Group");
            
            TemplateEditorAttributeNode attribute = templateRoot.GetChild(0).GetAttribute("id");

            Assert.AreEqual(attribute.name, "id");
            Assert.AreEqual(attribute.value, "id-0");
            
        }

        private static string GetFilePath([CallerFilePath] string path = "") {
            return path;
        }

    }

}