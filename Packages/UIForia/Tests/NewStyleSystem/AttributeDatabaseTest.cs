using NUnit.Framework;
using UIForia;

namespace Tests {

    public class AttributeDatabaseTest {

        [Test]
        public void SetAnAttribute() {

            using (StringInternSystem system = new StringInternSystem())
            using (AttributeSystem attributeSystem = new AttributeSystem(system, default)){
                
                attributeSystem.SetAttribute(34, "attr1", "value1");
                attributeSystem.SetAttribute(34, "attr2", "value2");
                
                Assert.AreEqual("value1", attributeSystem.GetAttribute(34, "attr1"));
                Assert.AreEqual("value2", attributeSystem.GetAttribute(34, "attr2"));
                
                Assert.AreEqual(2, attributeSystem.GetAttributeCount(34));
                
                Assert.AreEqual(null, attributeSystem.GetAttribute(34, "attr3"));
                Assert.AreEqual(null, attributeSystem.GetAttribute(31, "attr3"));
                
                attributeSystem.SetAttribute(34, "attr2", null);
                Assert.AreEqual(1, attributeSystem.GetAttributeCount(34));
                Assert.AreEqual(null, attributeSystem.GetAttribute(34, "attr2"));

            }

        }

        [Test]
        public void IndexSetAttributes() {

            using (StringInternSystem system = new StringInternSystem())
            using (AttributeSystem attributeSystem = new AttributeSystem(system, default)){
                
                attributeSystem.SetAttribute(34, "attr1", "value1");
                attributeSystem.SetAttribute(34, "attr2", "value2");

                List_ElementId attr1List = attributeSystem.GetIndexForAttribute("attr1");
                Assert.AreEqual(1, attr1List.size);

                attributeSystem.SetAttribute(35, "attr1", "value2");
                attributeSystem.SetAttribute(36, "attr1", "value2");
                attributeSystem.SetAttribute(37, "attr1", "value2");

                attr1List = attributeSystem.GetIndexForAttribute("attr1");
                Assert.AreEqual(4, attr1List.size);

                attributeSystem.RemoveAttribute(35, "attr1");
                attr1List = attributeSystem.GetIndexForAttribute("attr1");
                
                Assert.AreEqual(3, attr1List.size);
                
                attributeSystem.SetAttribute(34, "attr1", null);
                attributeSystem.SetAttribute(36, "attr1", null);
                attributeSystem.SetAttribute(37, "attr1", null);
                
                attr1List = attributeSystem.GetIndexForAttribute("attr1");
                
                Assert.AreEqual(0, attr1List.size);
            }

        }

    }

}