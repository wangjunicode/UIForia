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
        Assert.AreEqual(6, t.wordInfoList.Count);
        Assert.AreEqual(2, t.spanList.Count);
        Assert.AreEqual("this ", GetWord(t, 0));
        Assert.AreEqual("is ", GetWord(t, 1));
        Assert.AreEqual("a ", GetWord(t, 2));
        Assert.AreEqual("longer ", GetWord(t, 3));
        Assert.AreEqual("string ", GetWord(t, 4));
        Assert.AreEqual("world", GetWord(t, 5));
    }
    
    [Test]
    public void UpdateTextInfo_LastSpan() {
        const string newValue = "this is a longer string";
        
        TextInfo2 t = new TextInfo2(
            new TextSpan("hello "),
            new TextSpan("world")
        );
        
        t.UpdateSpan(1, new TextSpan(newValue));
        Assert.AreEqual(("hello " + newValue).Length, t.characterList.Count);
        Assert.AreEqual(("hello " + newValue).Length, t.charInfoList.Count);
        Assert.AreEqual(6, t.wordInfoList.Count);
        Assert.AreEqual(2, t.spanList.Count);
        Assert.AreEqual("hello ", GetWord(t, 0));
        Assert.AreEqual("this ", GetWord(t, 1));
        Assert.AreEqual("is ", GetWord(t, 2));
        Assert.AreEqual("a ", GetWord(t, 3));
        Assert.AreEqual("longer ", GetWord(t, 4));
        Assert.AreEqual("string", GetWord(t, 5));
    }

    public static string GetWord(TextInfo2 textInfo2, int word) {
        string retn = "";
        WordInfo wordInfo = textInfo2.wordInfoList[word];
        for (int i = wordInfo.startChar; i < wordInfo.startChar + wordInfo.charCount; i++) {
            retn += textInfo2.charInfoList[i].character;
        }

        return retn;
    }
}