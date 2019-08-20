using NUnit.Framework;
using Tests;
using Tests.Mocks;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

namespace Style {

    [TestFixture]
    public class StyleLifeCycle {

        [Test]
        public void SetStyleForOrphanedElements() {

            TestUtils.TestUIElementType element = new TestUtils.TestUIElementType();
            Assert.AreEqual(element.style.BorderTop.value, 0);
            element.style.SetBorder(new FixedLengthRect(10, 20, 30, 40), StyleState.Normal);
            Assert.AreEqual(element.style.BorderTop.value, 10);
        }
        
        [Test]
        public void DoNotPublishChangesIfElementIsNotEnabled() {
            string template = @"
            <UITemplate>
                <Style path='Templates/FlexLayoutColTests/FlexLayoutColTests.style'/>
                <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f' style.preferredHeight='500f'>
                    <Group x-id='child0' style='w100h100'/>
                </Contents>
            </UITemplate>
            ";
            MockApplication app = new MockApplication(typeof(FlexLayoutRowTests.FlexRowLayoutThing), template);
            int callCount = 0;
            app.StyleSystem.onStylePropertyChanged += (element, list) => { callCount++; }; 
            app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
            FlexLayoutRowTests.FlexRowLayoutThing root = (FlexLayoutRowTests.FlexRowLayoutThing) app.RootElement.GetChild(0);
            root.SetEnabled(false);
            app.Update();
            Assert.AreEqual(0, callCount);
            Assert.AreEqual(100f, app.RootElement.FindById("child0").style.PreferredWidth.value);
        }

    }

}