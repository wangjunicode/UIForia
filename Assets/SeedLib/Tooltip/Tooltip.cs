using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {

    [Template("SeedLib/Tooltip/Tooltip.xml#simple")]
    public class SimpleTooltip : Tooltip {

    }

    [Template("SeedLib/Tooltip/Tooltip.xml")]
    public class Tooltip : UIElement {

        public string title;

    }

}