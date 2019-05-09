using NUnit.Framework;
using SVGX;
using UIForia.Text;

[TestFixture]
public class TextInfoTests {

    [Test]
    public void CreateTextInfo_OneSpan() {
        TextInfo t = new TextInfo(new TextSpan("hello", new SVGX.SVGXTextStyle()));
        Assert.AreEqual("hello".Length, t.characterList.Count);
        Assert.AreEqual("hello".Length, t.charInfoList.Count);
        Assert.AreEqual(1, t.wordInfoList.Count);
        Assert.AreEqual(1, t.spanList.Count);
    }

    [Test]
    public void CreateTextInfo_TwoSpans() {
        TextInfo t = new TextInfo(
            new TextSpan("hello "),
            new TextSpan("world")
        );
        Assert.AreEqual("hello world".Length, t.characterList.Count);
        Assert.AreEqual("hello world".Length, t.charInfoList.Count);
        Assert.AreEqual(2, t.wordInfoList.Count);
        Assert.AreEqual(2, t.spanList.Count);
    }

    [Test]
    public void UpdateTextInfo_FirstSpanLarger() {
        const string newValue = "this is a longer string ";

        TextInfo t = new TextInfo(
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
    public void UpdateTextInfo_FirstSpanSmaller() {
        const string newValue = "this is less ";

        TextInfo t = new TextInfo(
            new TextSpan("this is a longer string "),
            new TextSpan("world")
        );

        t.UpdateSpan(0, new TextSpan(newValue));
        Assert.AreEqual((newValue + "world").Length, t.characterList.Count);
        Assert.AreEqual((newValue + "world").Length, t.charInfoList.Count);
        Assert.AreEqual(4, t.wordInfoList.Count);
        Assert.AreEqual(2, t.spanList.Count);
        Assert.AreEqual("this ", GetWord(t, 0));
        Assert.AreEqual("is ", GetWord(t, 1));
        Assert.AreEqual("less ", GetWord(t, 2));
        Assert.AreEqual("world", GetWord(t, 3));
    }

    [Test]
    public void UpdateTextInfo_LastSpanLarger() {
        const string newValue = "this is a longer string";

        TextInfo t = new TextInfo(
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

    [Test]
    public void UpdateTextInfo_LastSpanSmaller() {
        const string newValue = "this is less";

        TextInfo t = new TextInfo(
            new TextSpan("some words here "),
            new TextSpan("some other stuff "),
            new TextSpan("this is a longer string")
        );

        t.UpdateSpan(2, new TextSpan(newValue));
        Assert.AreEqual(("some words here some other stuff " + newValue).Length, t.characterList.Count);
        Assert.AreEqual(("some words here some other stuff " + newValue).Length, t.charInfoList.Count);
        Assert.AreEqual(9, t.wordInfoList.Count);
        Assert.AreEqual(3, t.spanList.Count);
        Assert.AreEqual("some ", GetWord(t, 0));
        Assert.AreEqual("words ", GetWord(t, 1));
        Assert.AreEqual("here ", GetWord(t, 2));
        Assert.AreEqual("some ", GetWord(t, 3));
        Assert.AreEqual("other ", GetWord(t, 4));
        Assert.AreEqual("stuff ", GetWord(t, 5));
        Assert.AreEqual("this ", GetWord(t, 6));
        Assert.AreEqual("is ", GetWord(t, 7));
        Assert.AreEqual("less", GetWord(t, 8));
    }

    [Test]
    public void UpdateTextInfo_MiddleSpanLarger() {
        const string newValue = "this is a longer string ";

        TextInfo t = new TextInfo(
            new TextSpan("hello "),
            new TextSpan("old stuff "),
            new TextSpan("world")
        );

        t.UpdateSpan(1, new TextSpan(newValue));
        Assert.AreEqual(("hello " + newValue + "world").Length, t.characterList.Count);
        Assert.AreEqual(("hello " + newValue + "world").Length, t.charInfoList.Count);
        Assert.AreEqual(7, t.wordInfoList.Count);
        Assert.AreEqual(3, t.spanList.Count);
        Assert.AreEqual("hello ", GetWord(t, 0));
        Assert.AreEqual("this ", GetWord(t, 1));
        Assert.AreEqual("is ", GetWord(t, 2));
        Assert.AreEqual("a ", GetWord(t, 3));
        Assert.AreEqual("longer ", GetWord(t, 4));
        Assert.AreEqual("string ", GetWord(t, 5));
        Assert.AreEqual("world", GetWord(t, 6));
    }

    [Test]
    public void UpdateTextInfo_MiddleSpanSmaller() {
        const string newValue = "this is less ";

        TextInfo t = new TextInfo(
            new TextSpan("some words here "),
            new TextSpan("this is a longer string"),
            new TextSpan("some other stuff")
        );

        t.UpdateSpan(1, new TextSpan(newValue));
        Assert.AreEqual(("some words here " + newValue + "some other stuff").Length, t.characterList.Count);
        Assert.AreEqual(("some words here " + newValue + "some other stuff").Length, t.charInfoList.Count);
        Assert.AreEqual(9, t.wordInfoList.Count);
        Assert.AreEqual(3, t.spanList.Count);
        Assert.AreEqual("some ", GetWord(t, 0));
        Assert.AreEqual("words ", GetWord(t, 1));
        Assert.AreEqual("here ", GetWord(t, 2));
        Assert.AreEqual("this ", GetWord(t, 3));
        Assert.AreEqual("is ", GetWord(t, 4));
        Assert.AreEqual("less ", GetWord(t, 5));
        Assert.AreEqual("some ", GetWord(t, 6));
        Assert.AreEqual("other ", GetWord(t, 7));
        Assert.AreEqual("stuff", GetWord(t, 8));
    }

    [Test]
    public void UpdateTextInfo_SameCharacterCount() {
        const string newValue = "this is the same ";

        TextInfo t = new TextInfo(
            new TextSpan("some words here "),
            new TextSpan("this is the word "),
            new TextSpan("some other stuff")
        );

        t.UpdateSpan(1, new TextSpan(newValue));
        Assert.AreEqual(("some words here " + newValue + "some other stuff").Length, t.characterList.Count);
        Assert.AreEqual(("some words here " + newValue + "some other stuff").Length, t.charInfoList.Count);
        Assert.AreEqual(10, t.wordInfoList.Count);
        Assert.AreEqual(3, t.spanList.Count);
        Assert.AreEqual("some ", GetWord(t, 0));
        Assert.AreEqual("words ", GetWord(t, 1));
        Assert.AreEqual("here ", GetWord(t, 2));
        Assert.AreEqual("this ", GetWord(t, 3));
        Assert.AreEqual("is ", GetWord(t, 4));
        Assert.AreEqual("the ", GetWord(t, 5));
        Assert.AreEqual("same ", GetWord(t, 6));
        Assert.AreEqual("some ", GetWord(t, 7));
        Assert.AreEqual("other ", GetWord(t, 8));
        Assert.AreEqual("stuff", GetWord(t, 9));
    }

    [Test]
    public void UpdateTextInfo_SameWordCount_FewerCharacters() {
        const string newValue = "this is the s ";

        TextInfo t = new TextInfo(
            new TextSpan("some words here "),
            new TextSpan("this is the word "),
            new TextSpan("some other stuff")
        );

        t.UpdateSpan(1, new TextSpan(newValue));
        Assert.AreEqual(("some words here " + newValue + "some other stuff").Length, t.characterList.Count);
        Assert.AreEqual(("some words here " + newValue + "some other stuff").Length, t.charInfoList.Count);
        Assert.AreEqual(10, t.wordInfoList.Count);
        Assert.AreEqual(3, t.spanList.Count);
        Assert.AreEqual("some ", GetWord(t, 0));
        Assert.AreEqual("words ", GetWord(t, 1));
        Assert.AreEqual("here ", GetWord(t, 2));
        Assert.AreEqual("this ", GetWord(t, 3));
        Assert.AreEqual("is ", GetWord(t, 4));
        Assert.AreEqual("the ", GetWord(t, 5));
        Assert.AreEqual("s ", GetWord(t, 6));
        Assert.AreEqual("some ", GetWord(t, 7));
        Assert.AreEqual("other ", GetWord(t, 8));
        Assert.AreEqual("stuff", GetWord(t, 9));
    }
    
    [Test]
    public void UpdateTextInfo_SameWordCount_MoveCharacters() {
        const string newValue = "this is the alotofcharactershere ";

        TextInfo t = new TextInfo(
            new TextSpan("some words here "),
            new TextSpan("this is the word "),
            new TextSpan("some other stuff")
        );

        t.UpdateSpan(1, new TextSpan(newValue));
        Assert.AreEqual(("some words here " + newValue + "some other stuff").Length, t.characterList.Count);
        Assert.AreEqual(("some words here " + newValue + "some other stuff").Length, t.charInfoList.Count);
        Assert.AreEqual(10, t.wordInfoList.Count);
        Assert.AreEqual(3, t.spanList.Count);
        Assert.AreEqual("some ", GetWord(t, 0));
        Assert.AreEqual("words ", GetWord(t, 1));
        Assert.AreEqual("here ", GetWord(t, 2));
        Assert.AreEqual("this ", GetWord(t, 3));
        Assert.AreEqual("is ", GetWord(t, 4));
        Assert.AreEqual("the ", GetWord(t, 5));
        Assert.AreEqual("alotofcharactershere ", GetWord(t, 6));
        Assert.AreEqual("some ", GetWord(t, 7));
        Assert.AreEqual("other ", GetWord(t, 8));
        Assert.AreEqual("stuff", GetWord(t, 9));
    }


    public static string GetWord(TextInfo textInfo, int word) {
        string retn = "";
        WordInfo wordInfo = textInfo.wordInfoList[word];
        for (int i = wordInfo.startChar; i < wordInfo.startChar + wordInfo.charCount; i++) {
            retn += textInfo.charInfoList[i].character;
        }

        return retn;
    }

}