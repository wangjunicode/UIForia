using System;

namespace UIForia.Util {

    [AttributeUsage(AttributeTargets.Field)]
    public class SoAGeneratorGroup : System.Attribute {

        public string groupName;

        public SoAGeneratorGroup(string groupName) {
            this.groupName = groupName;
        }

    }

}