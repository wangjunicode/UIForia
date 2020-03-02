using UIForia.Util;

namespace UIForia.Style {

  public interface IStylePropertyParser {

        bool TryParse(CharStream stream, out PropertyData propertyData);

    }

}