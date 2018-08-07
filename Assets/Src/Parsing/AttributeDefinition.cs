namespace Src {

    public class AttributeDefinition {

        public readonly string name;
        public readonly string[] modifiers;

        public readonly string key;
        public readonly string value;

        private static readonly string[] EmptyModifierList = new string[0];
        
        public AttributeDefinition(string key, string value) {
            this.key = key;
            this.value = value;

            if (key.IndexOf('.') == -1) {
                name = key.Trim();
                modifiers = EmptyModifierList;
                return;
            }
            
            string[] split = key.Split('.');
            string[] names = new string[split.Length - 1];
            
            for (int i = 0; i < names.Length; i++) {
                names[i] = split[i + 1].Trim();
            }
            
            modifiers = names;
            name = split[0].Trim();
        }

    }

}