using System.IO;
using UIForia.Compilers;

namespace UIForia {

    public static class DefaultLocators {

        [TemplateLocator("UIForia.Default")]
        public static TemplateLocation LocateTemplate(TemplateLookup lookup) {

            if (string.IsNullOrEmpty(lookup.templatePath)) {
                string fileName = Path.GetFileNameWithoutExtension(lookup.elementLocation);
                return new TemplateLocation(Path.Combine(lookup.moduleLocation, fileName, fileName + ".xml"), lookup.templateId);
            }

            // todo -- GetFullPath && Combine are not fast, write something better later
            string location = Path.GetFullPath(Path.Combine(lookup.moduleLocation, lookup.templatePath));
            return new TemplateLocation(location, lookup.templateId);
        }
        
        [StyleLocator("UIForia.Default")]
        public static string LocateStyle(string elementPath, string stylePath) {
            return stylePath;
        }
    }

}