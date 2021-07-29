namespace UIForia {

    public interface ICompiledApplication {

        TemplateFor[] GetTypeTemplates();

        EntryPoint[] GetEntryPoints();

    }

}