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
    
    //public struct TemplateLookup {
//
    //    public readonly Type elementType;
    //    public readonly string elementFilePath;
    //    public readonly string declaredTemplatePath;
    //    public readonly string templateId;
//
    //    public TemplateLookup(ProcessedType p) : this(p.rawType, p.elementPath, p.templatePath, p.templateId) { }
//
    //    public TemplateLookup(Type elementType, string elementFilePath, string declaredTemplatePath, string templateId) {
    //        this.elementType = elementType;
    //        this.elementFilePath = elementFilePath;
    //        this.declaredTemplatePath = declaredTemplatePath;
    //        this.templateId = templateId;
    //    }
//
    //}

}