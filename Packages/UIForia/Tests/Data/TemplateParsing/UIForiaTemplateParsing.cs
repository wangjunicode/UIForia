using System.Collections;
using NUnit.Framework;
using UIForia.Parsing;
using UIForia.Util;

namespace TemplateParsing {

    public class UIForiaTemplateParsing {

        [TestCaseSource(nameof(ElementTagTestCases))]
        public string ParseBasicTagData(string source) {
            CharStream stream = new CharStream(source);

            Assert.IsTrue(new TemplateParser_XML_Bad().TryParseTag(ref stream, out TemplateParser_XML_Bad.TagData tagData));

            return tagData.tagName.ToString();
        }

        public static IEnumerable ElementTagTestCases() {
            yield return new TestCaseData("<Element>").Returns("Element");
            yield return new TestCaseData("<Element\n>").Returns("Element");
            yield return new TestCaseData("<Element/>").Returns("Element");
            yield return new TestCaseData("<Element />").Returns("Element");
            yield return new TestCaseData("<Element \n/>").Returns("Element");
            yield return new TestCaseData(@"<Element someValue=""4""/>").Returns("Element");
        }

    }

}