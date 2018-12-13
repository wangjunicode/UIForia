using System.Collections.Generic;
using UIForia;
using UnityEngine;

namespace UI.LoginFlow {

    public struct JoinColonyInfo {

        public string colonyName;
        public string leaderName;
        public int playerCount;
        public int seedlingCount;

        public JoinColonyInfo(string colonyName, string leaderName, int playerCount, int seedlingCount) {
            this.colonyName = colonyName;
            this.leaderName = leaderName;
            this.playerCount = playerCount;
            this.seedlingCount = seedlingCount;
        }

    }

    [Template("Klang/Seed/UIForia/LoginFlow/JoinColonyScreen/JoinColonyScreen.xml")]
    public class JoinColonyScreen : UIElement {

        public List<JoinColonyInfo> colonies;
        public int selectedIndex = -1;

        public JoinColonyInfo SelectedColony => selectedIndex == -1 ? default : colonies[selectedIndex];
        
        public override void OnCreate() {
            colonies = new List<JoinColonyInfo>();
//            Debug.Log(typeof(Klang.Seed.Routes));
            colonies.Add(new JoinColonyInfo("Matt's Colony 1", "Matt", 5, 20));
            colonies.Add(new JoinColonyInfo("Matt's Colony 2", "Matt", 5, 20));
            colonies.Add(new JoinColonyInfo("Matt's Colony 3", "Matt", 5, 20));
            colonies.Add(new JoinColonyInfo("Matt's Colony 4", "Matt", 5, 20));
        }

        public void SelectColony(int index) {
            selectedIndex = index;
        }

    }

}