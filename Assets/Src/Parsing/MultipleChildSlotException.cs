using System;

namespace Src {

    public class MultipleChildSlotException : Exception {

        public MultipleChildSlotException(string templateName) 
            : base("Templates can only define one <Children/> slot. " + templateName + " defined multiple.") { }

    }

}