using System;
using System.Reflection;
using UIForia.Style;

namespace UIForia {

    public class ApplicationLoader {

        public static GameApplication LoadCompiledGameApplication(Assembly assembly, Compilation compilation) {
            if (compilation.compilationType != CompilationType.Compiled) {
                return null;
            }

            if (compilation.applicationType != ApplicationType.Game) {
                return null;
            }

            if (compilation.styleDatabase == null) {
                return null;
            }

            Type type = assembly.GetType("UIGenerated_" + compilation.guid + ".AppBootstrap");

            if (type == null) {
                return null;
            }

            object t = Activator.CreateInstance(type);

            if (!(t is ICompiledApplication compiledApplication)) {
                return null;
            }

            GameApplication application = new GameApplication();
            StyleDatabase styleDatabase = new StyleDatabase(compilation.styleDatabase);

            ApplicationInfo applicationInfo = new ApplicationInfo() {
                styleDatabase = styleDatabase,
                typeTemplates = compiledApplication.GetTypeTemplates(),
                entryPoints = compiledApplication.GetEntryPoints(),
                initialElementCapacity = 512
            };

            application.Initialize(applicationInfo);

            return application;

        }

        public static UIEditorApplication LoadCompiledEditorApplication(Assembly assembly, Compilation compilation) {
            if (compilation.compilationType != CompilationType.Compiled) {
                return null;
            }

            if (compilation.applicationType != ApplicationType.Editor) {
                return null;
            }

            if (compilation.styleDatabase == null) {
                return null;
            }

            Type type = assembly.GetType("UIGenerated_" + compilation.guid + ".AppBootstrap");

            if (type == null) {
                return null;
            }

            object t = Activator.CreateInstance(type);

            if (!(t is ICompiledApplication compiledApplication)) {
                return null;
            }

            UIEditorApplication application = new UIEditorApplication();
            StyleDatabase styleDatabase = new StyleDatabase(compilation.styleDatabase);

            ApplicationInfo applicationInfo = new ApplicationInfo() {
                styleDatabase = styleDatabase,
                typeTemplates = compiledApplication.GetTypeTemplates(),
                entryPoints = compiledApplication.GetEntryPoints(),
                initialElementCapacity = 512
            };

            application.Initialize(applicationInfo);

            return application;

        }

    }

}