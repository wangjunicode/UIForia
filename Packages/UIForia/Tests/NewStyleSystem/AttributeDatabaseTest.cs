using NUnit.Framework;
using UIForia;

namespace Tests {

    public class AttributeDatabaseTest {

        [Test]
        public void SetAnAttribute() {

            using (StringInternSystem system = new StringInternSystem())
            using (AttributeSystem attributeSystem = new AttributeSystem(system, default)) {

                ElementId el34 = new ElementId(34, 1);
                ElementId el31 = new ElementId(31, 1);
                
                attributeSystem.SetAttribute(el34, "attr1", "value1");
                attributeSystem.SetAttribute(el34, "attr2", "value2");
                
                Assert.AreEqual("value1", attributeSystem.GetAttribute(el34, "attr1"));
                Assert.AreEqual("value2", attributeSystem.GetAttribute(el34, "attr2"));
                
                Assert.AreEqual(2, attributeSystem.GetAttributeCount(el34));
                
                Assert.AreEqual(null, attributeSystem.GetAttribute(el34, "attr3"));
                Assert.AreEqual(null, attributeSystem.GetAttribute(el31, "attr3"));
                
                attributeSystem.SetAttribute(el34, "attr2", null);
                Assert.AreEqual(1, attributeSystem.GetAttributeCount(el34));
                Assert.AreEqual(null, attributeSystem.GetAttribute(el34, "attr2"));

            }

        }

        [Test]
        public void IndexSetAttributes() {

            using (StringInternSystem system = new StringInternSystem())
            using (AttributeSystem attributeSystem = new AttributeSystem(system, default)){
                
                ElementId el34 = new ElementId(34, 1);
                ElementId el31 = new ElementId(31, 1);
                ElementId el35 = new ElementId(35, 1);
                ElementId el36 = new ElementId(36, 1);
                ElementId el37 = new ElementId(37, 1);
                
                attributeSystem.SetAttribute(el34, "attr1", "value1");
                attributeSystem.SetAttribute(el34, "attr2", "value2");

                List_ElementId attr1List = attributeSystem.GetIndexForAttribute("attr1");
                Assert.AreEqual(1, attr1List.size);

                attributeSystem.SetAttribute(el35, "attr1", "value2");
                attributeSystem.SetAttribute(el36, "attr1", "value2");
                attributeSystem.SetAttribute(el37, "attr1", "value2");

                attr1List = attributeSystem.GetIndexForAttribute("attr1");
                Assert.AreEqual(4, attr1List.size);

                attributeSystem.RemoveAttribute(el35, "attr1");
                attr1List = attributeSystem.GetIndexForAttribute("attr1");
                
                Assert.AreEqual(3, attr1List.size);
                
                attributeSystem.SetAttribute(el34, "attr1", null);
                attributeSystem.SetAttribute(el36, "attr1", null);
                attributeSystem.SetAttribute(el37, "attr1", null);
                
                attr1List = attributeSystem.GetIndexForAttribute("attr1");
                
                Assert.AreEqual(0, attr1List.size);
            }

        }

    }

}