using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace Documentation.DocumentationElements {

    public class PlayerData {
        public int id;
        public string name;
        public List<PlayerData> friends;
        public bool Active;
        public bool Visible;
    }
    
    [Template("DocumentationElements/PlayerDetail.xml")]
    public class PlayerDetail : UIElement {

        public PlayerData player;
    }
}
