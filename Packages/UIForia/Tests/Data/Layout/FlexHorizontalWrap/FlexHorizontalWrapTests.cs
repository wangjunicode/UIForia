using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

namespace Layout {

    public class FlexHorizontalWrapTests {

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_NoWrap.xml")]
        public class FlexHorizontalWrap_NoWrap : UIElement { }

        [Test]
        public void NoWrap() {
            using (MockApplication app = MockApplication.Setup<FlexHorizontalWrap_NoWrap>()) {
                FlexHorizontalWrap_NoWrap root = (FlexHorizontalWrap_NoWrap) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            }
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_WrapWhenTrackFull.xml")]
        public class FlexHorizontalWrap_WrapWhenTrackFull : UIElement { }

        [Test]
        public void WrapWhenTrackFull() {
            using (MockApplication app = MockApplication.Setup<FlexHorizontalWrap_WrapWhenTrackFull>()) {
                FlexHorizontalWrap_WrapWhenTrackFull root = (FlexHorizontalWrap_WrapWhenTrackFull) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 0, 200, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 100, 200, 100), root[3].layoutResult.AllocatedRect);
            }
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_WrapWhenItemTooBig.xml")]
        public class FlexHorizontalWrap_WrapWhenItemTooBig : UIElement { }

        [Test]
        public void WrapWhenItemTooBig() {
            using (MockApplication app = MockApplication.Setup<FlexHorizontalWrap_WrapWhenItemTooBig>()) {
                FlexHorizontalWrap_WrapWhenItemTooBig root = (FlexHorizontalWrap_WrapWhenItemTooBig) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 100, 800, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 200, 200, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 200, 200, 100), root[3].layoutResult.AllocatedRect);
            }
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_WrapWhenItemOverflows.xml")]
        public class FlexHorizontalWrap_WrapWhenItemOverflows : UIElement { }

        [Test]
        public void WrapWhenItemOverflows() {
            using (MockApplication app = MockApplication.Setup<FlexHorizontalWrap_WrapWhenItemOverflows>()) {
                FlexHorizontalWrap_WrapWhenItemOverflows root = (FlexHorizontalWrap_WrapWhenItemOverflows) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 100, 300, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(300, 100, 200, 100), root[3].layoutResult.AllocatedRect);
            }
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_GrowInTrack.xml")]
        public class FlexHorizontalWrap_GrowInTrack : UIElement { }

        [Test]
        public void GrowInTrack() {
            using (MockApplication app = MockApplication.Setup<FlexHorizontalWrap_GrowInTrack>()) {
                FlexHorizontalWrap_GrowInTrack root = (FlexHorizontalWrap_GrowInTrack) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 200, 200, 100), root[4].layoutResult.AllocatedRect);
            }
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_ShrinkInTrack.xml")]
        public class FlexHorizontalWrap_ShrinkInTrack : UIElement { }

        [Test]
        public void ShrinkInTrack() {
            using (MockApplication app = MockApplication.Setup<FlexHorizontalWrap_ShrinkInTrack>()) {
                FlexHorizontalWrap_ShrinkInTrack root = (FlexHorizontalWrap_ShrinkInTrack) app.RootElement;

                app.Update();

                Assert.AreEqual(new Rect(0, 0, 300, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(300, 0, 300, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 100, 600, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 200, 300, 100), root[3].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(300, 200, 300, 100), root[4].layoutResult.AllocatedRect);
            }
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWap_DistributeSpaceInTrack.xml")]
        public class FlexHorizontalWrap_DistributeSpaceInTrack : UIElement { }

        [Test]
        public void DistributeSpaceInTrack() {
            using (MockApplication app = MockApplication.Setup<FlexHorizontalWrap_DistributeSpaceInTrack>()) {
                FlexHorizontalWrap_DistributeSpaceInTrack root = (FlexHorizontalWrap_DistributeSpaceInTrack) app.RootElement;

                root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.AfterContent, StyleState.Normal);
                app.Update();

                Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 200, 200, 100), root[4].layoutResult.AllocatedRect);

                root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.BeforeContent, StyleState.Normal);
                app.Update();

                Assert.AreEqual(new Rect(200, 0, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 0, 200, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 200, 200, 100), root[4].layoutResult.AllocatedRect);

                root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.BetweenContent, StyleState.Normal);
                app.Update();

                Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 0, 200, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 200, 200, 100), root[4].layoutResult.AllocatedRect);

                root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.AroundContent, StyleState.Normal);
                app.Update();

                Assert.AreEqual(new Rect(50, 0, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(350, 0, 200, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 200, 200, 100), root[4].layoutResult.AllocatedRect);

                root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.CenterContent, StyleState.Normal);
                app.Update();

                Assert.AreEqual(new Rect(100, 0, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(300, 0, 200, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 200, 200, 100), root[4].layoutResult.AllocatedRect);
            }
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWap_DistributeSpaceBetweenTracks.xml")]
        public class FlexHorizontalWap_DistributeSpaceBetweenTracks : UIElement { }

        [Test]
        public void DistributeSpaceBetweenTracks() {
            using (MockApplication app = MockApplication.Setup<FlexHorizontalWap_DistributeSpaceBetweenTracks>()) {
                FlexHorizontalWap_DistributeSpaceBetweenTracks root = (FlexHorizontalWap_DistributeSpaceBetweenTracks) app.RootElement;

                root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.AfterContent, StyleState.Normal);
                app.Update();

                Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 200, 200, 100), root[4].layoutResult.AllocatedRect);

                root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.BeforeContent, StyleState.Normal);
                app.Update();

                Assert.AreEqual(new Rect(0, 300, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 300, 200, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 400, 400, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 400, 200, 100), root[3].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 500, 200, 100), root[4].layoutResult.AllocatedRect);

                root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.BetweenContent, StyleState.Normal);
                app.Update();

                Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 250, 400, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 250, 200, 100), root[3].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 500, 200, 100), root[4].layoutResult.AllocatedRect);

                root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.AroundContent, StyleState.Normal);
                app.Update();

                Assert.AreEqual(new Rect(0, 50, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 50, 200, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 250, 400, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 250, 200, 100), root[3].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 450, 200, 100), root[4].layoutResult.AllocatedRect);

                root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.CenterContent, StyleState.Normal);
                app.Update();

                Assert.AreEqual(new Rect(0, 150, 200, 100), root[0].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(200, 150, 200, 100), root[1].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 250, 400, 100), root[2].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(400, 250, 200, 100), root[3].layoutResult.AllocatedRect);
                Assert.AreEqual(new Rect(0, 350, 200, 100), root[4].layoutResult.AllocatedRect);
            }
        }

    }

}