using System.Collections.Generic;
using Documentation.DocumentationElements;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace UnityEngine {
    
    [Template("Documentation/Features/RepeatALot.xml")]
    public class RepeatALot : UIElement {

        public List<PlayerData> players;

        public override void OnCreate() {
            players = new List<PlayerData>();
            
            for (int i = 0; i < 10; i++) {
                
                players.Add(new PlayerData() {
                        id = i,
                        name = "Player " + i,
                        friends = MakeFriends(4, i),
                        Active = false,
                        Visible = true
                });
            }
        }

        private List<PlayerData> MakeFriends(int friendCount, int parentId) {

            var friends = new List<PlayerData>();

            for (int i = 0; i < friendCount; i++) {
                int id = 1000 * (parentId + 1) + i;

                friends.Add(new PlayerData() {
                        id = id,
                        name = "Player " + i,
                        friends = MakeFriends(friendCount - 1, id),
                        Active = false,
                        Visible = false
                });
            }

            return friends;
        }

        public override void OnUpdate() {

        }
    }
}
