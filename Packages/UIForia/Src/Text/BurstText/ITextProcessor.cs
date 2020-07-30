using UIForia.Util;

namespace UIForia.Text {

    public interface ITextProcessor {

        bool Process(CharStream stream, ref TextSymbolStream textSymbolStream);
        
        // maybe implement setup & teardown to help resolve assets / style / etc
        
    }

}