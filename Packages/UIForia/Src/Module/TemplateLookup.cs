using System;

namespace UIForia {

    public struct TemplateLookup {

        public Type elementType;
        public string modulePath;
        public string moduleLocation;
        public string elementLocation;
        public string templatePath;
        public string templateId;

    }

    public struct StyleLookup {

        public Type elementType;
        public string modulePath;
        public string moduleLocation;
        public string elementLocation;
        public string stylePath;
        public string styleAlias;

    }

}