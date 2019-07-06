using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;

[TestFixture]
public class TestTemplateParser {

    [Test]
    public void ParseTemplate() {
        
        NameTable nameTable = new NameTable();
        
        XmlNamespaceManager nameSpaceManager = new XmlNamespaceManager(nameTable);
        
        nameSpaceManager.AddNamespace("attr", "attr");
        
        XmlParserContext parserContext = new XmlParserContext(null, nameSpaceManager, null, XmlSpace.None);
        
        XmlTextReader txtReader = new XmlTextReader(@"<Contents><Thing attr:thing=""someattr""/></Contents>", XmlNodeType.Element, parserContext);
        
        XElement elem = XElement.Load(txtReader);
        
        Assert.AreEqual("thing", (elem.FirstNode as XElement).FirstAttribute.Name.LocalName);
        Assert.AreEqual("attr", (elem.FirstNode as XElement).FirstAttribute.Name.NamespaceName);
    }

}