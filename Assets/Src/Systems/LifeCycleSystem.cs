namespace Src.Systems {

    public class LifeCycleData : ISkipTreeTraversable {

        public UIElement element;

        public LifeCycleData(UIElement element) {
            this.element = element;
        }
        
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

        public void OnParentChanged(ISkipTreeTraversable newParent) {
            throw new System.NotImplementedException();
        }

        public void OnBeforeTraverse() {
            throw new System.NotImplementedException();
        }

        public void OnAfterTraverse() {
            throw new System.NotImplementedException();
        }

    }

    public class LifeCycleSystem {

        private SkipTree<LifeCycleData> lifeCycleTree;

        public LifeCycleSystem() {
            this.lifeCycleTree = new SkipTree<LifeCycleData>();
        }
        
        public void Register(UIElement element) {
                
        }

        public void Reset() {
            
        }

    }

}