namespace Src {

    public class UIGroupTemplate : UITemplate {

        private Binding[] bindings;

        public override RegistrationData CreateScoped(TemplateScope scope) {

            UIGroupElement instance = new UIGroupElement();
            
            bindings = new Binding[dynamicStyleBindings.Length];
            for (int i = 0; i < bindings.Length; i++) {
                bindings[i] = dynamicStyleBindings[i];
            }

            RegistrationData instanceData = new RegistrationData(instance, bindings, scope.context);
            ApplyConstantStyles(instance, scope);

            for (int i = 0; i < childTemplates.Count; i++) {
                scope.SetParent(childTemplates[i].CreateScoped(scope), instanceData);
            }
           
            return instanceData;

        }

    }

}