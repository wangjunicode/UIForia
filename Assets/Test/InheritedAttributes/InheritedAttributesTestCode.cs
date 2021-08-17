using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.UIInput;
using UnityEngine;

namespace Test {
    /// <summary>
    /// Tests whether inherited attributes are properly updated.
    /// In this test the property is changed by code (SetProperty method call).
    /// See also InheritedAttributesTest. 
    /// </summary>
    [Template("Test/InheritedAttributes/InheritedAttributesTestCode.xml")]
    public class InheritedAttributesTestCode : UIElement {
        public bool parentEnabled;
        public int clickCount;

        public new UIElement parent;
        public UIElement child;

        public override void OnCreate() {
            parent = FindById("parent");
            child = FindById("child");
            child.style.SetPointerEvents(PointerEvents.None, StyleState.Normal);
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
            child.style.SetProperty(StyleProperty.Unset(StylePropertyId.PointerEvents), StyleState.Normal);
        }
    }
}