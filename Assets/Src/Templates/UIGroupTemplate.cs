namespace Src {

    public class UIGroupTemplate : UITemplate {

        private Binding[] bindings;

        public override UIElementCreationData CreateScoped(TemplateScope scope) {

            UIGroupElement instance = new UIGroupElement();
            instance.name = name;
            bindings = new Binding[dynamicStyleBindings.Length];
            for (int i = 0; i < bindings.Length; i++) {
                bindings[i] = dynamicStyleBindings[i];
            }

            UIElementCreationData instanceData = new UIElementCreationData(instance, bindings, scope.context);
            ApplyConstantStyles(instance, scope);

            for (int i = 0; i < childTemplates.Count; i++) {
                scope.SetParent(childTemplates[i].CreateScoped(scope), instanceData);
            }
           
            return instanceData;

        }

    }

}