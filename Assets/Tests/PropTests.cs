using System.Collections;
using System.IO;
using NUnit.Framework;
using Src;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = System.Diagnostics.Debug;

namespace Tests {
    [TestFixture]
    public class PropTests {
        [Test]
        public void ParseProp_BuiltIn() {
            string template = File.ReadAllText(Application.dataPath + "/Tests/Test4.xml");
            ParsedTemplate parsedTemplate = TemplateParser.ParseTemplate(template);
            UIElement output = parsedTemplate.Instantiate();
            Assert.AreEqual(1, output.templateContext.GetBoundElements(nameof(Spec.Test4.isPanelVisible)).Count);
        }

        [UnityTest]
        public IEnumerator ChangeVisibleValue() {
            string template = File.ReadAllText(Application.dataPath + "/Tests/Test4.xml");
            ParsedTemplate parsedTemplate = TemplateParser.ParseTemplate(template);
            UIElement output = parsedTemplate.Instantiate();
            Spec.Test4 test = output as Spec.Test4;
            Assert.IsNotNull(test);
            test.isPanelVisible.Value = true;
            Assert.AreEqual((test.children[0] as UIPanel).visible, true);
            yield return new WaitForEndOfFrame();
            
        }
    }
}