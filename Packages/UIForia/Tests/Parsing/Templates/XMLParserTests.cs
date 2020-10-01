using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using UIForia;
using UIForia.Elements;
using UIForia.NewStyleParsing;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using Application = UIForia.Application;
using Debug = UnityEngine.Debug;

namespace Tests.Parsing {

    public class XMLParserTests {

        [Test]
        public void ParseBasicTemplate() {

            UIForiaMLParser parser = new UIForiaMLParser();
            string fileName = Path.GetDirectoryName(GetFilePath()) + "\\TestXMLParser_BasicTemplate.xml";

            parser.TryParse(fileName, File.ReadAllText(fileName), out TemplateFileShell fileShell);

            // Diagnostics diagnostics = new Diagnostics();
            // parser.Setup(fileName, diagnostics);
            // parser.TryParse(fileName, out TemplateFileShell fileShell);
            //
            // if (diagnostics.HasErrors()) {
            //     diagnostics.Dump();
            //     Assert.IsTrue(false);
            // }
            //
            TemplateEditor editor = new TemplateEditor(fileShell);
            TemplateEditorRootNode templateRoot = editor.GetTemplateRoot();

            Assert.AreEqual(templateRoot.ChildCount, 3);
            Assert.AreEqual(templateRoot.GetChild(0).ChildCount, 3);
            Assert.AreEqual(templateRoot.GetChild(0).GetChild(0).tagName, "Group");
            Assert.AreEqual(templateRoot.GetChild(0).GetChild(1).tagName, "Group");
            Assert.AreEqual(templateRoot.GetChild(0).GetChild(2).tagName, "Group");

            TemplateEditorAttributeNode attribute0 = templateRoot.GetChild(0).GetAttribute("id");
            TemplateEditorAttributeNode attribute1 = templateRoot.GetChild(1).GetAttribute("id");
            TemplateEditorAttributeNode attribute2 = templateRoot.GetChild(2).GetAttribute("id");

            Assert.AreEqual(attribute0.key, "id");
            Assert.AreEqual(attribute0.value, "id-0");
            Assert.AreEqual(attribute1.key, "id");
            Assert.AreEqual(attribute1.value, "id-1");
            Assert.AreEqual(attribute2.key, "id");
            Assert.AreEqual(attribute2.value, "id-2");
        }

        [Test]
        public void SerializeAParsedTemplate() {
            UIForiaMLParser parser = new UIForiaMLParser();
            string fileName = Path.GetDirectoryName(GetFilePath()) + "\\TestXMLParser_BasicTemplate.xml";

            parser.TryParse(fileName, File.ReadAllText(fileName), out TemplateFileShell fileShell);

            ManagedByteBuffer buffer = new ManagedByteBuffer();
            fileShell.Serialize(ref buffer);

            byte[] bytes = new byte[buffer.array.Length];
            buffer.array.CopyTo(bytes, 0);

            ManagedByteBuffer hydrateBuffer = new ManagedByteBuffer(bytes);

            TemplateFileShell hydrated = new TemplateFileShell();

            hydrated.Deserialize(ref hydrateBuffer);

            Assert.AreEqual(fileShell.filePath, hydrated.filePath);
            Assert.IsTrue(MemCmp(fileShell.rootNodes, hydrated.rootNodes));
            Assert.IsTrue(MemCmp(fileShell.templateNodes, hydrated.templateNodes));
            Assert.IsTrue(MemCmp(fileShell.attributeList, hydrated.attributeList));
            Assert.IsTrue(MemCmp(fileShell.charBuffer, hydrated.charBuffer));
            Assert.IsTrue(MemCmp(fileShell.textContents, hydrated.textContents));
            Assert.IsTrue(MemCmp(fileShell.slots, hydrated.slots));
            Assert.IsTrue(MemCmp(fileShell.styles, hydrated.styles));

            unsafe bool MemCmp<T>(T[] a, T[] b) where T : unmanaged {
                if (a.Length != b.Length) return false;
                fixed (T* aptr = a)
                fixed (T* bptr = b) {
                    return UnsafeUtility.MemCmp(aptr, bptr, sizeof(T) * a.Length) == 0;
                }
            }

        }

        [Test]
        public void ParseUsingDeclaration() {

            UIForiaMLParser parser = new UIForiaMLParser();
            string fileName = Path.GetDirectoryName(GetFilePath()) + "\\TestXMLParser_UsingDeclarations.xml";

            parser.TryParse(fileName, File.ReadAllText(fileName), out TemplateFileShell fileShell);

            Assert.AreEqual(fileShell.usings.Length, 3);
            string namespace0 = fileShell.GetString(fileShell.usings[0].namespaceRange);
            string namespace1 = fileShell.GetString(fileShell.usings[1].namespaceRange);
            string namespace2 = fileShell.GetString(fileShell.usings[2].namespaceRange);

            Assert.AreEqual("superduper", namespace0);
            Assert.AreEqual("super.duper", namespace1);
            Assert.AreEqual("super_double.dip", namespace2);
        }

        [Test]
        public void ParseUsingDeclaration_MissingNamespace() {

            Diagnostics diagnostics = new Diagnostics();
            UIForiaMLParser parser = new UIForiaMLParser(diagnostics);
            string fileName = Path.GetDirectoryName(GetFilePath()) + "\\TestXMLParser_UsingDeclaration_Invalid.xml";

            bool result = parser.TryParse(fileName, File.ReadAllText(fileName), out TemplateFileShell fileShell);

            Assert.IsFalse(result);
            Assert.AreEqual(1, diagnostics.diagnosticList.size);

            Assert.IsTrue(diagnostics.diagnosticList.array[0].message.Contains("<Using/> tags require a `namespace`"));

        }

        [Test]
        public void ParseStyleBySrcReference() {

            UIForiaMLParser parser = new UIForiaMLParser();
            string fileName = Path.GetDirectoryName(GetFilePath()) + "\\TestXMLParser_StyleReference.xml";

            bool result = parser.TryParse(fileName, File.ReadAllText(fileName), out TemplateFileShell fileShell);

            Assert.IsTrue(result);

            Assert.AreEqual(fileShell.styles.Length, 3);
            Assert.AreEqual("path/to/style", fileShell.GetString(fileShell.styles[0].path));
            Assert.AreEqual(null, fileShell.GetString(fileShell.styles[0].alias));
            Assert.AreEqual(null, fileShell.GetString(fileShell.styles[0].sourceBody));

            Assert.AreEqual("path/to/style2", fileShell.GetString(fileShell.styles[1].path));
            Assert.AreEqual("my-style", fileShell.GetString(fileShell.styles[1].alias));
            Assert.AreEqual(null, fileShell.GetString(fileShell.styles[1].sourceBody));

            Assert.AreEqual(null, fileShell.GetString(fileShell.styles[2].path));
            Assert.AreEqual(null, fileShell.GetString(fileShell.styles[2].alias));
            Assert.AreEqual(Regex.Replace(@"style some-style {
            BackgroundColor = red;
            // comment
            PaddingLeft = 32px;
            }", @"\s+", ""),
                Regex.Replace(fileShell.GetString(fileShell.styles[2].sourceBody), @"\s+", "")
            );

        }

        [Test]
        public void ParseStyleReference_DuplicateAlias() {

            Diagnostics diagnostics = new Diagnostics();
            UIForiaMLParser parser = new UIForiaMLParser(diagnostics);
            string fileName = Path.GetDirectoryName(GetFilePath()) + "\\TestXMLParser_StyleReference_Invalid.xml";

            bool result = parser.TryParse(fileName, File.ReadAllText(fileName), out TemplateFileShell fileShell);

            Assert.IsFalse(result);
            Assert.AreEqual(1, diagnostics.diagnosticList.size);

            Assert.IsTrue(diagnostics.diagnosticList.array[0].message.Contains("Unable to add <Style> node because another style node already declared an alias `my-style`"));

        }

        [Test]
        public void ParseStyleReference_DuplicateStyleSource() {

            Diagnostics diagnostics = new Diagnostics();
            UIForiaMLParser parser = new UIForiaMLParser(diagnostics);
            string fileName = Path.GetDirectoryName(GetFilePath()) + "\\TestXMLParser_StyleReference_DupSource.xml";

            bool result = parser.TryParse(fileName, File.ReadAllText(fileName), out TemplateFileShell fileShell);

            Assert.IsFalse(result);
            Assert.AreEqual(1, diagnostics.diagnosticList.size);

            Assert.IsTrue(diagnostics.diagnosticList.array[0].message.Contains("Unable to add <Style> node because another style node referencing `path/to/style1` was already declared"));

        }

        [Test]
        public void ParseStyleReference_InvalidSetup() {
            
            UIForiaRuntime.CreateGameApplication("app", typeof(UIDivElement));
            
            Diagnostics diagnostics = new Diagnostics();
            UIForiaMLParser parser = new UIForiaMLParser(diagnostics);
            string fileName = Path.GetDirectoryName(GetFilePath()) + "\\TestXMLParser_StyleReference_InvalidSetup.xml";

            bool result = parser.TryParse(fileName, File.ReadAllText(fileName), out TemplateFileShell fileShell);

            Assert.IsFalse(result);
            Assert.AreEqual(1, diagnostics.diagnosticList.size);

            Assert.IsTrue(diagnostics.diagnosticList.array[0].message.Contains("A <Style> node that is not self-closing cannot take attributes."));
        }

        [Test]
        public void ParseABunchOfFiles() {
            string[] files = Directory.GetFiles(Path.Combine(GetFilePath(), "..", "..", "..", "Data", "TemplateBindings"), "*.xml");

            string[] sources = new string[files.Length];
            for (int i = 0; i < files.Length; i++) {
                sources[i] = File.ReadAllText(files[i]);
            }
            
            Assert.IsTrue(files.Length >= 15);
            
            Diagnostics diagnostics = new Diagnostics();
            UIForiaMLParser parser = new UIForiaMLParser(diagnostics);

            // Stopwatch w = Stopwatch.StartNew();
            for (int i = 0; i < files.Length; i++) {
                bool result = parser.TryParse(files[i], sources[i], out TemplateFileShell fileShell);
                Assert.IsTrue(result);
            }
            
            Assert.IsFalse(diagnostics.HasErrors());

            // bench mark code comparing .NET XML parser with new UIForia parser
            // JUST the parsing of the xml file takes 5-6x longer with .NET, not including the uiforia parse step
            
            // Debug.Log(w.Elapsed.TotalMilliseconds.ToString("F3"));
            // XmlParserContext parserContext = new XmlParserContext(null, new XMLTemplateParser.CustomNamespaceReader(new NameTable()), null, XmlSpace.None);
            //
            // w.Restart();
            // Stopwatch w2 = Stopwatch.StartNew();
            // for (int i = 0; i < files.Length; i++) {
            //     string source = sources[i];
            //     XElement root = XElement.Load(new XmlTextReader(source, XmlNodeType.Element, parserContext), LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
            //
            //     root.MergeTextNodes();
            // }
            // Debug.Log(w2.Elapsed.TotalMilliseconds.ToString("F3"));

        }

        private static string GetFilePath([CallerFilePath] string path = "") {
            return path;
        }

    }

}