using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

namespace Layout {

    public class FlexRowTests {

        [Template("Data/Layout/FlexHorizontal/FlexHorizontal_CrossAxis.xml")]
        public class FlexHorizontal_CrossAxis : UIElement { }

        [Test]
        public void AppliesCrossAxisStart() {
            MockApplication app = new MockApplication(typeof(FlexHorizontal_CrossAxis));
            FlexHorizontal_CrossAxis root = (FlexHorizontal_CrossAxis) app.RootElement.GetChild(0);
            
            app.Update();

            Assert.AreEqual(new Rect(000, 0, 100, 500), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 500), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 100, 500), root[2].layoutResult.AllocatedRect);
        }
        
        [Test]
        public void AppliesCrossAxisEnd() {
            MockApplication app = new MockApplication(typeof(FlexHorizontal_CrossAxis));
            FlexHorizontal_CrossAxis root = (FlexHorizontal_CrossAxis) app.RootElement.GetChild(0);
            
            root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.End, StyleState.Normal);
            
            app.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 500), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 500), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 100, 500), root[2].layoutResult.AllocatedRect);
        }

    }

}