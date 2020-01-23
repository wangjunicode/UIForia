using NUnit.Framework;
using Tests.Data.Layout;
using Tests.Mocks;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Systems;

[TestFixture]
public class BasicLayoutTests {

    [Test]
    public void GathersDirtyData() {
        
        MockApplication app = MockApplication.Setup<BasicLayoutTest_GathersDirtyData>();
        
        UIElement viewRoot = app.RootElement;
        BasicLayoutTest_GathersDirtyData root = app.RootElement.children[0] as BasicLayoutTest_GathersDirtyData;
         
        AwesomeLayoutRunner runner = new AwesomeLayoutRunner(null, app.GetView(0).dummyRoot);
        
        runner.GatherLayoutData();

        Assert.AreEqual(5, runner.hierarchyRebuildList.size);
        Assert.AreEqual(runner.hierarchyRebuildList[0], viewRoot);
        Assert.AreEqual(runner.hierarchyRebuildList[1], root);
        Assert.AreEqual(runner.hierarchyRebuildList[2], root.GetChild(0));
        Assert.AreEqual(runner.hierarchyRebuildList[3], root.GetChild(1));
        Assert.AreEqual(runner.hierarchyRebuildList[4], root.GetChild(2));
        
        app.Update();
        
        runner.GatherLayoutData();
        Assert.AreEqual(0, runner.hierarchyRebuildList.size);

        root.FindById("one").SetEnabled(false);
        
        runner.GatherLayoutData();
        Assert.AreEqual(1, runner.hierarchyRebuildList.size);
        Assert.AreEqual(runner.hierarchyRebuildList[0], root);
        
    }
    
    [Test]
    public void RunWidthLayout() {
        
        MockApplication app = MockApplication.Setup<BasicLayoutTest_GathersDirtyData>();
        
        BasicLayoutTest_GathersDirtyData root = app.RootElement.children[0] as BasicLayoutTest_GathersDirtyData;

        app.Update();

        UIElement one = root[0];
        UIElement two = root[1];
        UIElement three = root[2];
        
        Assert.AreEqual(3, root.layoutBox.childCount);
        Assert.AreEqual(100, one.layoutBox.finalWidth);
        Assert.AreEqual(100, two.layoutBox.finalWidth);
        Assert.AreEqual(100, three.layoutBox.finalWidth);
        
        Assert.AreEqual(0, one.layoutResult.localPosition.x);
        Assert.AreEqual(100, two.layoutResult.localPosition.x);
        Assert.AreEqual(200, three.layoutResult.localPosition.x);

        root.FindById("one").SetEnabled(false);
        
        app.Update();

        Assert.AreEqual(2, root.layoutBox.childCount);
        Assert.AreEqual(100, two.layoutBox.finalWidth);
        Assert.AreEqual(100, three.layoutBox.finalWidth);
        
        Assert.AreEqual(0, two.layoutResult.localPosition.x);
        Assert.AreEqual(100, three.layoutResult.localPosition.x);

        app.Update();
        
    }
    
    [Test]
    public void HandleEnableDisableElementWithContentAncestor() {
        
        MockApplication app = MockApplication.Setup<BasicLayoutTest_ContentAncestor>();
        BasicLayoutTest_ContentAncestor root = app.RootElement.children[0] as BasicLayoutTest_ContentAncestor;

        app.Update();

        UIElement toggle = root["disable-me"];
        UIElement wrapper = root[0];
      
        Assert.AreEqual(200, root.layoutBox.finalWidth);
        Assert.AreEqual(200, wrapper.layoutBox.finalWidth);
        Assert.AreEqual(100, toggle.layoutBox.finalWidth);
        
        toggle.SetEnabled(false);
        
        app.Update();

        Assert.AreEqual(100, root.layoutBox.finalWidth);
        Assert.AreEqual(100, wrapper.layoutBox.finalWidth);
        
        toggle.SetEnabled(true);
        
        app.Update();
        
        Assert.AreEqual(200, root.layoutBox.finalWidth);
        Assert.AreEqual(200, wrapper.layoutBox.finalWidth);
        Assert.AreEqual(100, toggle.layoutBox.finalWidth);
    }

    [Test]
    public void RespondToBlockSizeChange() {
        
        MockApplication app = MockApplication.Setup<BasicLayoutTest_BlockSizeChanges>();
        
        UIElement viewRoot = app.RootElement;

        BasicLayoutTest_BlockSizeChanges root = viewRoot[0] as BasicLayoutTest_BlockSizeChanges;

        UIElement blockProvider = root[0];
        UIElement one = blockProvider[0];
        UIElement two = blockProvider[1];
        UIElement contentSize = blockProvider[2];
        UIElement blockUser = contentSize[0];
        
        app.Update();

        Assert.AreEqual(3, blockProvider.layoutBox.childCount);
        Assert.AreEqual(300, blockUser.layoutBox.finalWidth);
        
        blockProvider.style.SetPreferredWidth(200f, StyleState.Normal);
        app.Update();

//        Assert.IsTrue(root.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 1));
//        Assert.IsTrue(blockProvider.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 1));
//        Assert.IsTrue(contentSize.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 1));
//        Assert.IsTrue(blockUser.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 1));
//        Assert.IsFalse(one.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 1));
//        Assert.IsFalse(two.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 1));
//        
//        Assert.AreEqual(200, blockProvider.layoutBox.finalWidth);
//        Assert.AreEqual(200, blockUser.layoutBox.finalWidth);
//        
//        app.Update();
//        
//        Assert.IsFalse(root.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 2));
//        Assert.IsFalse(blockProvider.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 2));
//        Assert.IsFalse(contentSize.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 2));
//        Assert.IsFalse(blockUser.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 2));
//        Assert.IsFalse(one.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 2));
//        Assert.IsFalse(two.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 2));

    }
    
    [Test]
    public void UseViewBlockSize() {
        
        MockApplication app = MockApplication.Setup<BasicLayoutTest_BlockSizeChanges>();
        
        UIElement viewRoot = app.RootElement;

        BasicLayoutTest_BlockSizeChanges root = viewRoot[0] as BasicLayoutTest_BlockSizeChanges;

        UIElement blockProvider = root[0];
        UIElement one = blockProvider[0];
        UIElement two = blockProvider[1];
        UIElement contentSize = blockProvider[2];
        UIElement blockUser = contentSize[0];
        
        app.GetView(0).SetSize(1000, 1000);
        
        blockProvider.style.SetPreferredWidth(new UIMeasurement(1f, UIMeasurementUnit.Content), StyleState.Normal);
        
        app.Update();

        Assert.AreEqual(3, blockProvider.layoutBox.childCount);
        Assert.AreEqual(1000, blockUser.layoutBox.finalWidth);
        
    }
    
}