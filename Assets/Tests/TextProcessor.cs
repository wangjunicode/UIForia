using NUnit.Framework;
using Src.Text;
using Src.Util;

[TestFixture]
public class TextProcessor {

  
    [Test]
    public void SplitsIntoWords() {
        string input = "this is my input";
        TextInfo textInfo = TextUtil.ProcessText(input, true, false);
        Assert.AreEqual(4, textInfo.wordCount);
        Assert.AreEqual(5, textInfo.wordInfos[0].charCount);
        Assert.AreEqual(4, textInfo.wordInfos[0].VisibleCharCount);
        Assert.AreEqual(3, textInfo.wordInfos[1].charCount);
        Assert.AreEqual(2, textInfo.wordInfos[1].VisibleCharCount);
        Assert.AreEqual(3, textInfo.wordInfos[2].charCount);
        Assert.AreEqual(2, textInfo.wordInfos[2].VisibleCharCount);
        Assert.AreEqual(5, textInfo.wordInfos[3].charCount);
        Assert.AreEqual(5, textInfo.wordInfos[3].VisibleCharCount);
    }




}