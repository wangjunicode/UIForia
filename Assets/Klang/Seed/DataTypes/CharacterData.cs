using System.Collections.Generic;
using UIForia.Util;

namespace Klang.Seed.DataTypes {

    public class CharacterData {

        public string name;
        public string iconUrl;
        public string status;
        public float health;
        public float mood;
        public bool isInDirectControl;
        public string statusIconUrl;

        public RepeatableList<CharacterSkill> skills;

    }

}