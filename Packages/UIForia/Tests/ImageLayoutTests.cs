using NUnit.Framework;
using UIForia.Rendering;
using UIForia;
using Tests.Mocks;
using UnityEngine;

[TestFixture]
public class ImageLayoutTests {

//    [Template(TemplateType.String, @"
//    <UITemplate>
//        <Contents>
//            <Image x-id='image'/>
//        </Contents>
//    </UITemplate>
//    ")]
//    public class ImageTestThing : UIElement { }
//
//    [Test]
//    public void PreserveAspectRatio() {
//        MockView view = new MockView(typeof(ImageTestThing));
//        view.Initialize();
//        ImageTestThing root = (ImageTestThing) view.RootElement;
//        UIImageElement img = root.FindById<UIImageElement>("image");
//        Texture2D tex = new Texture2D(100, 100);
//        img.SetTexture(tex);
//        img.style.SetPreferredWidth(200f, StyleState.Normal);
//        view.Update();
//        Assert.AreEqual(200f, img.layoutResult.allocatedWidth);
//        img.SetPreserveAspectRatio(true);
//        view.Update();
//        Assert.AreEqual(100f, img.layoutResult.allocatedWidth);
//    }

}