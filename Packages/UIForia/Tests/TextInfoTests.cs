using NUnit.Framework;
using SVGX;
using UIForia.Text;

[TestFixture]
public class TextInfoTests {

    [Test]
    public void CreateTextInfo_OneSpan() {
        TextInfo2 t = new TextInfo2(new TextSpan("hello", new SVGX.SVGXTextStyle()));
        Assert.AreEqual("hello".Length, t.characterList.Count);
        Assert.AreEqual("hello".Length, t.charInfoList.Count);
        Assert.AreEqual(1, t.wordInfoList.Count);
        Assert.AreEqual(1, t.spanList.Count);
    }

    [Test]
    public void CreateTextInfo_TwoSpans() {
        TextInfo2 t = new TextInfo2(
            new TextSpan("hello "),
            new TextSpan("world")
        );
        Assert.AreEqual("hello world".Length, t.characterList.Count);
        Assert.AreEqual("hello world".Length, t.charInfoList.Count);
        Assert.AreEqual(2, t.wordInfoList.Count);
        Assert.AreEqual(2, t.spanList.Count);
    }

    [Test]
    public void UpdateTextInfo_FirstSpan() {
        const string newValue = "this is a longer string ";
        
        TextInfo2 t = new TextInfo2(
            new TextSpan("hello "),
            new TextSpan("world")
        );
        t.UpdateSpan(0, new TextSpan(newValue));
        Assert.AreEqual((newValue + "world").Length, t.characterList.Count);
        Assert.AreEqual((newValue + "world").Length, t.charInfoList.Count);
        Assert.AreEqual(2, t.wordInfoList.Count);
        Assert.AreEqual(2, t.spanList.Count);
    }

}