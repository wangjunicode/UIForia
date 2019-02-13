namespace Klang.Seed.DataTypes {

    public class CharacterSkill {

        public string name;
        public float progress;
        public int level;

        public CharacterSkill(string name, float progress, int level) {
            this.name = name;
            this.progress = progress;
            this.level = level;
        }

    }

}