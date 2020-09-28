
namespace UIForia.Elements {

    [RecordFilePath]
    public class BuiltInElementsModule : UIModule {

        public override void Configure() {

            // can get assembly location to determine if in package mode or in project directly

            // SetDefaultRootType<T>();
            
            SetModuleName("Elements");
            
            // <Using element="Module#ElementName"/>
            
            // <Module:ElementType/>
            
            // <Variant:KlangButton.Black/>
            
            // SetTemplateResolver();
            // SetStyleResolver();
            // SetAssetResolver(AssetType.All, resolver);
            //
            // AutoIncludeNamespace();
            // AutoIncludeStyleSheet();
            //
            // DefineGlobalStyleConstant();
            //
            // AddDependency<Module>();
            // AddDependency<Module>();
            // AddDependency<Module>();
            //
            // AddDynamicTypeReference<T>();
            // AddDynamicTypeReference<T>();
            // AddDynamicTypeReference<T>();
            // AddDynamicTypeReference<T>();
            // AddDynamicTypeReference<T>();
            

            // application = Application.Create<Element>();
            // must have a template attribute
            // from template attribute can find module it belongs to
            // can load module that way
            
            // application.CreateFromModule<Module>();
        }

    }

}