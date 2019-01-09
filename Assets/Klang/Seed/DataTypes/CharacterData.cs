using System.Collections.Generic;

namespace Klang.Seed.DataTypes {

    public class CharacterData {

        public string name;
        public string iconUrl;
        public string status;
        public float health;
        public float mood;
        public bool isInDirectControl;
        public string statusIconUrl;

        public List<CharacterSkill> skills;

    }

}