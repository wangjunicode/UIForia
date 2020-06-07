using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

namespace Layout {

    public class FlexVerticalTests {

        [Template("Data/Layout/FlexVertical/FlexVertical_DistributeSpaceVertical.xml")]
        public class FlexVertical_DistributeSpaceVertical : UIElement { }

        [Test]
        public void DistributeSpaceVertical_Default() {
            using (MockApplication app = MockApplication.Setup<FlexVertical_DistributeSpaceVertical>()) {
                FlexVertical_DistributeSpaceVertical root = (FlexVertical_DistributeSpaceVertical) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 0, 500, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 100, 500, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 200, 500, 100), root[2].layoutResult.AllocatedRect);
            }
        }

        [Test]
        public void DistributeSpaceVertical_AfterContent() {
            using (MockApplication app = MockApplication.Setup<FlexVertical_DistributeSpaceVertical>()) {
                FlexVertical_DistributeSpaceVertical root = (FlexVertical_DistributeSpaceVertical) app.RootElement;
                root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.AfterContent, StyleState.Normal);
                app.Update();

                Assert.AreEqual(new Rect(0, 0, 500, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 100, 500, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 200, 500, 100), root[2].layoutResult.AllocatedRect);
            }
        }

        [Test]
        public void DistributeSpaceVertical_CenterContent() {
            using (MockApplication app = MockApplication.Setup<FlexVertical_DistributeSpaceVertical>()) {
                FlexVertical_DistributeSpaceVertical root = (FlexVertical_DistributeSpaceVertical) app.RootElement;

                root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.CenterContent, StyleState.Normal);

                app.Update();

                Assert.AreEqual(new Rect(0, 100, 500, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 200, 500, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 300, 500, 100), root[2].layoutResult.AllocatedRect);
            }
        }


        [Test]
        public void DistributeSpaceVertical_BeforeContent() {
            using (MockApplication app = MockApplication.Setup<FlexVertical_DistributeSpaceVertical>()) {
                FlexVertical_DistributeSpaceVertical root = (FlexVertical_DistributeSpaceVertical) app.RootElement;

                root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.BeforeContent, StyleState.Normal);
                app.Update();

                Assert.AreEqual(new Rect(0, 200, 500, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 300, 500, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 400, 500, 100), root[2].layoutResult.AllocatedRect);
            }
        }

        [Test]
        public void DistributeSpaceVertical_AroundContent() {
            using (MockApplication app = MockApplication.Setup<FlexVertical_DistributeSpaceVertical>()) {
                FlexVertical_DistributeSpaceVertical root = (FlexVertical_DistributeSpaceVertical) app.RootElement;

                // makes math cleaner
                root.style.SetPreferredHeight(600f, StyleState.Normal);
                root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.AroundContent, StyleState.Normal);

                app.Update();

                Assert.AreEqual(new Rect(0, 50, 500, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 250, 500, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 450, 500, 100), root[2].layoutResult.AllocatedRect);
            }
        }

        [Test]
        public void DistributeSpaceVertical_BetweenContent() {
            using (MockApplication app = MockApplication.Setup<FlexVertical_DistributeSpaceVertical>()) {
                FlexVertical_DistributeSpaceVertical root = (FlexVertical_DistributeSpaceVertical) app.RootElement;

                root.style.SetPreferredHeight(600f, StyleState.Normal);
                root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.BetweenContent, StyleState.Normal);

                app.Update();

                Assert.AreEqual(new Rect(0, 0, 500, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 250, 500, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 500, 500, 100), root[2].layoutResult.AllocatedRect);
            }
        }

        [Template("Data/Layout/FlexVertical/FlexVertical_GrowUnconstrained.xml")]
        public class FlexVertical_GrowUnconstrained : UIElement { }

        [Test]
        public void GrowUnconstrained() {
            using (MockApplication app = MockApplication.Setup<FlexVertical_GrowUnconstrained>()) {
                FlexVertical_GrowUnconstrained root = (FlexVertical_GrowUnconstrained) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 0, 100, 200), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 200, 100, 400), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 600, 100, 200), root[2].layoutResult.AllocatedRect);
            }
        }

        [Template("Data/Layout/FlexVertical/FlexVertical_GrowConstrained.xml")]
        public class FlexVertical_GrowConstrained : UIElement { }

        [Test]
        public void GrowConstrained() {
            using (MockApplication app = MockApplication.Setup<FlexVertical_GrowConstrained>()) {
                FlexVertical_GrowConstrained root = (FlexVertical_GrowConstrained) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 0, 100, 350), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 350, 100, 200), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 550, 100, 350), root[2].layoutResult.AllocatedRect);
            }
        }

        [Template("Data/Layout/FlexVertical/FlexVertical_GrowWithExtraSpace.xml")]
        public class FlexVertical_GrowWithExtraSpace : UIElement { }

        [Test]
        public void GrowWithExtraSpace() {
            using (MockApplication app = MockApplication.Setup<FlexVertical_GrowWithExtraSpace>()) {
                FlexVertical_GrowWithExtraSpace root = (FlexVertical_GrowWithExtraSpace) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 200, 100, 300), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 500, 100, 200), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 700, 100, 300), root[2].layoutResult.AllocatedRect);
            }
        }


        [Template("Data/Layout/FlexVertical/FlexVertical_RespectMarginVertical.xml")]
        public class FlexVertical_RespectMarginVertical : UIElement { }

        [Test]
        public void RespectMarginVertical() {
            using (MockApplication app = MockApplication.Setup<FlexVertical_RespectMarginVertical>()) {
                FlexVertical_RespectMarginVertical root = (FlexVertical_RespectMarginVertical) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 10, 100, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 310, 100, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 1220, 100, 100), root[2].layoutResult.AllocatedRect);
            }
        }

        [Template("Data/Layout/FlexVertical/FlexVertical_ShrinkUnconstrained.xml")]
        public class FlexVertical_ShrinkUnconstrained : UIElement { }

        [Test]
        public void ShrinkUnconstrained() {
            using (MockApplication app = MockApplication.Setup<FlexVertical_ShrinkUnconstrained>()) {
                FlexVertical_ShrinkUnconstrained root = (FlexVertical_ShrinkUnconstrained) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 0, 100, 200), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 200, 100, 200), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 400, 100, 200), root[2].layoutResult.AllocatedRect);
            }
        }

        [Template("Data/Layout/FlexVertical/FlexVertical_ShrinkConstrained.xml")]
        public class FlexVertical_ShrinkConstrained : UIElement { }

        [Test]
        public void ShrinkConstrained() {
            using (MockApplication app = MockApplication.Setup<FlexVertical_ShrinkConstrained>()) {
                FlexVertical_ShrinkConstrained root = (FlexVertical_ShrinkConstrained) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 0, 100, 175), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 175, 100, 250), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 425, 100, 175), root[2].layoutResult.AllocatedRect);
            }
        }
        
        [Template("Data/Layout/FlexVertical/FlexVertical_ShrinkWithOverflow.xml")]
        public class FlexVertical_ShrinkWithOverflow : UIElement { }

        [Test]
        public void ShrinkWithOverflow() {
            using (MockApplication app = MockApplication.Setup<FlexVertical_ShrinkWithOverflow>()) {
                FlexVertical_ShrinkWithOverflow root = (FlexVertical_ShrinkWithOverflow) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 0, 100, 250), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 250, 100, 250), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 500, 100, 250), root[2].layoutResult.AllocatedRect);
            }
        }

    }

}