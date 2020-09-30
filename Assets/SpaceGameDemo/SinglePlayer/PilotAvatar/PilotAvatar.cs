using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;

namespace SpaceGameDemo.SinglePlayer {

    [Template("SinglePlayer/PilotAvatar/PilotAvatar.xml")]
    public class PilotAvatar : UIElement {

        public bool selected;
        public string imgSrc;
        public string name;

    }

}