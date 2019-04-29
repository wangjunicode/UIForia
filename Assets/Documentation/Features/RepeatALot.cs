using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace UnityEngine {
    
    [Template("Documentation/Features/RepeatALot.xml")]
    public class RepeatALot : UIElement {

        public RepeatableList<PlayerData> players;

        public override void OnCreate() {
            players = new RepeatableList<PlayerData>();
        }

        public override void OnUpdate() {
            players.Clear();
            for (int i = 0; i < Random.Range(3, 8); i++) {
                
                players.Add(new PlayerData() {
                    id = i,
                    name = "Player " + i
                });
            }
        }
    }

    public class PlayerData {
        public int id;
        public string name;
    }
}
