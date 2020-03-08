using UIForia.Util;

namespace UIForia.Elements {

    [RecordFilePath]
    public class UIForiaElements : Module {

        public override void Initialize() {

            // can get assembly location to determine if in package mode or in project directly

            // SetDefaultRootType<T>();
            
            SetModuleName("Elements");
            
            SetFilePath(PathUtil.GetCallerFilePath());
            
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