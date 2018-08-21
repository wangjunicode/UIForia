namespace Src {

    public class UIGroupTemplate : UITemplate {

        public override UIElementCreationData CreateScoped(TemplateScope scope) {

            UIGroupElement instance = new UIGroupElement();

            UIElementCreationData instanceData = new UIElementCreationData(
                name, 
                instance,
                styleDefinition,
                null,
                scope.context
            );

            for (int i = 0; i < childTemplates.Count; i++) {
                scope.SetParent(childTemplates[i].CreateScoped(scope), instanceData);
            }
           
            return instanceData;

        }

    }

}