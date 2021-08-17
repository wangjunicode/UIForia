using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace Test {
    /// <summary>
    /// Tests whether inherited attributes are properly updated.
    /// In this test, the inherited attributes are changed by changing style attributes.
    /// See also InheritedAttributeTestCode.
    /// </summary>
    [Template("Test/InheritedAttributes/InheritedAttributesTest.xml")]
    public class InheritedAttributesTest : UIElement {
        public bool parentEnabled;
        public bool childEnabled;
        public int clickCount;

        public new UIElement parent;
        public UIElement child;

        public override void OnCreate() {
            parent = FindById("parent");
            child = FindById("child");
        }

        public void OnChildClick() {
            Debug.Log("OnChildClick");
            clickCount++;
        }

        public void OnButtonClicked() {
            Debug.Log("Parent enabled");
            parentEnabled = true;
        }

        public void OnButton2Clicked() {
            Debug.Log("Child enabled");
            childEnabled = true;
        }
    }
}